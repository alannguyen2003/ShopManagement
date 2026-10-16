// Shared library for the SDD hooks and scripts. Uses Node built-ins only.
'use strict';

const fs = require('node:fs');
const path = require('node:path');
const { execFileSync } = require('node:child_process');

// Tokens written by the UserPromptSubmit hook when the user types a guarded command themselves.
//   approval      /sdd-approve       15 minutes
//   scaffold      /sdd-scaffold      until the next human prompt, at most 60 minutes
//   architecture  /sdd-architecture  until the next human prompt, at most 15 minutes
const TOKEN_TTL_MS = { approval: 15 * 60 * 1000, scaffold: 60 * 60 * 1000, architecture: 15 * 60 * 1000 };
const TOKEN_COMMANDS = { approval: 'sdd-approve', scaffold: 'sdd-scaffold', architecture: 'sdd-architecture' };
const STATUS_RE = /^\*\*Status:\*\*[ \t]*Approved\b/m;
const MARKER = '[NEEDS CLARIFICATION';

function readStdinJson() {
  try {
    const raw = fs.readFileSync(0, 'utf8');
    return raw.trim() ? JSON.parse(raw) : {};
  } catch {
    return {};
  }
}

function projectDir(input) {
  return path.resolve(process.env.CLAUDE_PROJECT_DIR || (input && input.cwd) || process.cwd());
}

function git(args, cwd) {
  try {
    return execFileSync('git', args, { cwd, stdio: ['ignore', 'pipe', 'ignore'] }).toString().trim();
  } catch {
    return null;
  }
}

function isGitRepo(root) {
  return git(['rev-parse', '--is-inside-work-tree'], root) === 'true';
}

// symbolic-ref also works in a repo with no commits yet.
function currentBranch(root) {
  return git(['symbolic-ref', '--short', 'HEAD'], root) || git(['rev-parse', '--abbrev-ref', 'HEAD'], root);
}

function parseBranch(branch) {
  const m = /^(feat|fix)\/(\d{3})-[a-z0-9-]+$/.exec(branch || '');
  return m ? { kind: m[1], num: m[2] } : null;
}

function readText(file) {
  try {
    return fs.readFileSync(file, 'utf8');
  } catch {
    return null;
  }
}

// Reads the value of a `**<Field>:** ...` header line.
function field(text, name) {
  if (text == null) return null;
  const re = new RegExp('^\\*\\*' + name + ':\\*\\*[ \\t]*(.*)$', 'm');
  const m = re.exec(text);
  return m ? m[1].trim() : null;
}

function statusOf(text) {
  const v = field(text, 'Status');
  return v ? v.split(/\s+/)[0] : null;
}

function intField(text, name) {
  const v = parseInt(field(text, name), 10);
  return Number.isNaN(v) ? null : v;
}

function countTasks(text) {
  if (!text) return { done: 0, total: 0 };
  const all = text.match(/^[ \t]*- \[[ xX]\] T\d{3}\b/gm) || [];
  const done = all.filter((l) => /\[[xX]\]/.test(l)).length;
  return { done, total: all.length };
}

function listFeatures(root) {
  const specs = path.join(root, 'specs');
  let names = [];
  try {
    names = fs.readdirSync(specs, { withFileTypes: true })
      .filter((d) => d.isDirectory() && /^\d{3}-/.test(d.name))
      .map((d) => d.name)
      .sort();
  } catch {
    return [];
  }
  return names.map((name) => loadFeature(root, name));
}

function findFeature(root, num) {
  return listFeatures(root).find((f) => f.num === num) || null;
}

function loadFeature(root, name) {
  const dir = path.join(root, 'specs', name);
  const read = (f) => readText(path.join(dir, f));
  const spec = read('spec.md');
  const plan = read('plan.md');
  const tasks = read('tasks.md');
  const fix = read('fix.md');
  const verification = read('verification.md');
  const f = {
    name,
    num: name.slice(0, 3),
    dir,
    kind: fix != null ? 'fix' : 'feat',
    spec: spec == null ? null : { status: statusOf(spec), rev: intField(spec, 'Revision'), markers: spec.includes(MARKER) },
    plan: plan == null ? null : { status: statusOf(plan), rev: intField(plan, 'Revision'), specRev: intField(plan, 'Spec revision') },
    tasks: tasks == null ? null : { status: statusOf(tasks), rev: intField(tasks, 'Revision'), planRev: intField(tasks, 'Plan revision'), ...countTasks(tasks) },
    fix: fix == null ? null : { status: statusOf(fix), markers: fix.includes(MARKER), ...countTasks(fix) },
    verification: verification == null ? null : (/NOT READY/i.test(field(verification, 'Verdict') || '') ? 'fail' : 'pass'),
    pr: fs.existsSync(path.join(dir, 'pr.md')),
  };
  f.planStale = !!(f.spec && f.plan && f.spec.rev != null && f.plan.specRev != null && f.plan.specRev < f.spec.rev);
  f.tasksStale = !!(f.plan && f.tasks && f.plan.rev != null && f.tasks.planRev != null && f.tasks.planRev < f.plan.rev);
  f.next = nextStep(f);
  return f;
}

function nextStep(f) {
  if (f.kind === 'fix') {
    const x = f.fix;
    if (x.status === 'Draft') return x.markers ? 'answer the open questions in fix.md, then /sdd-approve fix' : 'review fix.md, then /sdd-approve fix';
    if (x.status === 'Approved') {
      if (x.done < x.total) return '/sdd-build';
      return f.verification === 'fail' ? 'see verification.md, fix with /sdd-build, then /sdd-verify' : '/sdd-verify';
    }
    if (x.status === 'Done') return f.pr ? 'push the branch and open a PR' : '/sdd-pr';
    return 'check the Status line in fix.md';
  }
  const { spec, plan, tasks } = f;
  if (!spec) return '/sdd-spec';
  if (spec.status !== 'Approved') return spec.markers ? '/sdd-clarify' : 'review the spec, then /sdd-approve spec';
  if (!plan) return '/sdd-plan';
  if (f.planStale) return `/sdd-plan (update to spec rev ${spec.rev})`;
  if (plan.status !== 'Approved') return 'review the plan, then /sdd-approve plan';
  if (!tasks) return '/sdd-tasks';
  if (f.tasksStale) return `/sdd-tasks (update to plan rev ${plan.rev})`;
  if (tasks.status === 'Draft') return 'review the tasks, then /sdd-approve tasks';
  if (tasks.status === 'Approved') {
    if (tasks.done < tasks.total) return '/sdd-build';
    return f.verification === 'fail' ? 'see verification.md: fix the Must-fix items or /sdd-amend tasks, then /sdd-verify' : '/sdd-verify';
  }
  if (tasks.status === 'Done') return f.pr ? 'push the branch and open a PR' : '/sdd-pr';
  return 'check the Status line in tasks.md';
}

// Requirements documents written by /sdd-ba: docs/requirements/RNNN-slug/requirements.md.
function listRequirements(root) {
  const base = path.join(root, 'docs', 'requirements');
  let names = [];
  try {
    names = fs.readdirSync(base, { withFileTypes: true })
      .filter((d) => d.isDirectory() && /^R\d{3}-/.test(d.name))
      .map((d) => d.name)
      .sort();
  } catch {
    return [];
  }
  return names.map((name) => loadRequirements(root, name)).filter(Boolean);
}

function loadRequirements(root, name) {
  const text = readText(path.join(root, 'docs', 'requirements', name, 'requirements.md'));
  if (text == null) return null;
  const items = backlogItems(text);
  const r = {
    name,
    id: name.slice(0, 4),
    status: statusOf(text),
    rev: intField(text, 'Revision'),
    markers: text.includes(MARKER),
    total: items.length,
    specced: items.filter((i) => i.spec).length,
  };
  r.next = requirementsNextStep(r);
  return r;
}

// Rows of the backlog summary table: | BL-01 | ... | <NNN-slug once the spec exists, otherwise —> |
function backlogItems(text) {
  const rows = text.match(/^\|[ \t]*BL-\d+[ \t]*\|.*$/gm) || [];
  const items = new Map();
  for (const row of rows) {
    const cells = row.trim().replace(/^\||\|$/g, '').split('|').map((c) => c.trim());
    const last = cells[cells.length - 1];
    if (!items.has(cells[0])) items.set(cells[0], { id: cells[0], spec: /^\d{3}-[a-z0-9-]+$/.test(last) ? last : null });
  }
  return [...items.values()];
}

function requirementsNextStep(r) {
  if (r.status === 'Draft') return r.markers ? `/sdd-clarify ${r.id}` : `review requirements.md, then /sdd-approve requirements ${r.id}`;
  if (r.status === 'Approved') return r.specced < r.total ? `/sdd-ba-spec ${r.id}` : 'every backlog item has a spec';
  return 'check the Status line in requirements.md';
}

// Code may be edited only when the whole document chain is approved and nothing is stale.
function buildPermission(f) {
  if (!f) return { ok: false, reason: 'no specs/NNN-* directory matches the current branch' };
  if (f.kind === 'fix') {
    return f.fix.status === 'Approved'
      ? { ok: true }
      : { ok: false, reason: `fix.md is "${f.fix.status}", it must be "Approved" (the user runs /sdd-approve fix)` };
  }
  if (!f.tasks) return { ok: false, reason: 'tasks.md does not exist yet' };
  if (f.tasks.status !== 'Approved') return { ok: false, reason: `tasks.md is "${f.tasks.status}", it must be "Approved"` };
  if (!f.spec || f.spec.status !== 'Approved') return { ok: false, reason: 'spec.md is no longer Approved (probably just amended)' };
  if (!f.plan || f.plan.status !== 'Approved') return { ok: false, reason: 'plan.md is no longer Approved (probably just amended)' };
  if (f.planStale) return { ok: false, reason: 'plan.md is based on an outdated spec revision' };
  if (f.tasksStale) return { ok: false, reason: 'tasks.md is based on an outdated plan revision' };
  return { ok: true };
}

function stateDir(root) {
  return path.join(root, '.claude', 'state');
}

function writeToken(root, kind, prompt) {
  fs.mkdirSync(stateDir(root), { recursive: true });
  fs.writeFileSync(path.join(stateDir(root), kind + '.json'), JSON.stringify({ ts: Date.now(), prompt: String(prompt).slice(0, 200) }));
}

function clearToken(root, kind) {
  try {
    fs.unlinkSync(path.join(stateDir(root), kind + '.json'));
  } catch {
    // no token to clear
  }
}

function tokenValid(root, kind) {
  const raw = readText(path.join(stateDir(root), kind + '.json'));
  if (!raw) return false;
  try {
    return Date.now() - JSON.parse(raw).ts < TOKEN_TTL_MS[kind];
  } catch {
    return false;
  }
}

function writeApprovalToken(root, prompt) {
  writeToken(root, 'approval', prompt);
}

// Fallback for when the UserPromptSubmit hook did not run: was the latest human prompt /sdd-approve?
function lastHumanPrompt(transcriptPath) {
  const raw = transcriptPath ? readText(transcriptPath) : null;
  if (!raw) return '';
  const lines = raw.trimEnd().split('\n');
  for (let i = lines.length - 1; i >= 0; i--) {
    let e;
    try {
      e = JSON.parse(lines[i]);
    } catch {
      continue;
    }
    if (e.type !== 'user' || e.isMeta || e.isSidechain) continue;
    if (e.origin && e.origin.kind && e.origin.kind !== 'human') continue;
    const c = e.message && e.message.content;
    if (typeof c === 'string') return c;
    if (Array.isArray(c)) {
      if (c.some((x) => x && x.type === 'tool_result')) continue;
      const t = c.filter((x) => x && x.type === 'text').map((x) => x.text).join('\n');
      if (t) return t;
    }
  }
  return '';
}

function isCommandPrompt(text, command) {
  return new RegExp('(^|\\s|<command-name>)\\/' + command + '\\b').test(text || '');
}

function isApprovePrompt(text) {
  return isCommandPrompt(text, 'sdd-approve');
}

// The user typed the command behind this token kind: a valid token, or the latest human prompt as fallback.
function humanCommand(root, input, kind) {
  return tokenValid(root, kind) || isCommandPrompt(lastHumanPrompt(input && input.transcript_path), TOKEN_COMMANDS[kind]);
}

function humanApproved(root, input) {
  return humanCommand(root, input, 'approval');
}

function toRel(root, file) {
  const abs = path.resolve(root, file);
  const rel = path.relative(root, abs);
  if (!rel || rel.startsWith('..') || path.isAbsolute(rel)) return null;
  return rel.split(path.sep).join('/');
}

function block(message) {
  process.stderr.write(message + '\n');
  process.exit(2);
}

// PreToolUse: make the user confirm the call in a permission prompt instead of blocking it.
function ask(reason) {
  process.stdout.write(JSON.stringify({
    hookSpecificOutput: { hookEventName: 'PreToolUse', permissionDecision: 'ask', permissionDecisionReason: reason },
  }));
  process.exit(0);
}

// Content of an Edit/Write/MultiEdit target after the tool call, simulated from the tool input.
function afterEdit(input, before) {
  const ti = input.tool_input || {};
  const apply = (text, e) => {
    const from = e.old_string || '';
    if (!from) return text;
    return e.replace_all ? text.split(from).join(e.new_string || '') : text.replace(from, () => e.new_string || '');
  };
  if (input.tool_name === 'Write') return ti.content || '';
  if (Array.isArray(ti.edits)) return ti.edits.reduce(apply, before);
  return apply(before, ti);
}

module.exports = {
  STATUS_RE,
  MARKER,
  readStdinJson,
  projectDir,
  git,
  isGitRepo,
  currentBranch,
  parseBranch,
  readText,
  field,
  listFeatures,
  findFeature,
  listRequirements,
  buildPermission,
  TOKEN_COMMANDS,
  writeToken,
  clearToken,
  tokenValid,
  writeApprovalToken,
  humanApproved,
  humanCommand,
  isApprovePrompt,
  isCommandPrompt,
  toRel,
  block,
  ask,
  afterEdit,
};
