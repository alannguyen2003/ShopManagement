#!/usr/bin/env node
// SDD status computed in code, so it does not depend on the model reading files correctly.
//   node .claude/hooks/sdd-status.js            table of all features (used by /sdd-status)
//   node .claude/hooks/sdd-status.js 003        feature 003 only
//   node .claude/hooks/sdd-status.js R001       requirements R001 only
//   node .claude/hooks/sdd-status.js --session  short summary for the SessionStart hook
'use strict';

const fs = require('node:fs');
const path = require('node:path');
const sdd = require('./lib/sdd');

const args = process.argv.slice(2);
const session = args.includes('--session');
const input = session ? sdd.readStdinJson() : {};
const root = sdd.projectDir(input);

function warnings() {
  const w = [];
  if (!sdd.isGitRepo(root)) w.push('The project is not a git repository → run /sdd-init.');
  const constitution = sdd.readText(path.join(root, 'docs', 'constitution.md')) || '';
  if (!constitution || /^\*\*Status:\*\*[ \t]*Placeholder/m.test(constitution)) w.push('The constitution has no content yet → run /sdd-constitution.');
  const architecture = sdd.readText(path.join(root, 'docs', 'architecture.md'));
  if (architecture == null) w.push('No docs/architecture.md yet → run /sdd-scaffold before the first /sdd-plan.');
  else if (/^\*\*Status:\*\*[ \t]*Scaffolding/m.test(architecture)) w.push('The scaffold is unfinished (architecture.md is "Scaffolding") → run /sdd-scaffold again to resume.');
  const claude = sdd.readText(path.join(root, 'CLAUDE.md')) || '';
  const cmds = claude.split(/^## Project commands/m)[1] || '';
  if (/`<\.\.\.>`/.test(cmds)) w.push('"Project commands" in CLAUDE.md is still empty → run /sdd-init or fill it in.');
  return w;
}

function docCell(d, extra) {
  if (!d) return '—';
  let s = d.status || '?';
  if (d.rev != null) s += ` r${d.rev}`;
  if (extra) s += ` ${extra}`;
  return s;
}

function row(f) {
  if (f.kind === 'fix') {
    return [f.name + ' (fix)', '—', '—', docCell(f.fix), `${f.fix.done}/${f.fix.total}`, f.next];
  }
  const tasksCell = docCell(f.tasks, f.tasksStale ? '⚠ stale' : '');
  const planCell = docCell(f.plan, f.planStale ? '⚠ stale' : '');
  const progress = f.tasks ? `${f.tasks.done}/${f.tasks.total}` : '—';
  return [f.name, docCell(f.spec), planCell, tasksCell, progress, f.next];
}

const branch = sdd.isGitRepo(root) ? sdd.currentBranch(root) : null;
const current = sdd.parseBranch(branch);
let features = sdd.listFeatures(root);
let requirements = sdd.listRequirements(root);
const only = args.find((a) => /^\d{3}$/.test(a));
const onlyReq = args.find((a) => /^R\d{3}$/.test(a));
if (only) features = features.filter((f) => f.num === only);
if (onlyReq) requirements = requirements.filter((r) => r.id === onlyReq);
const openReqs = requirements.filter((r) => r.status !== 'Approved' || r.specced < r.total);

if (session) {
  const lines = ['[SDD] Workflow status (injected at session start):'];
  const cur = current && features.find((f) => f.num === current.num);
  if (cur) {
    const r = row(cur);
    lines.push(`- Branch ${branch} → specs/${cur.name}: spec ${r[1]} · plan ${r[2]} · tasks/fix ${r[3]} · progress ${r[4]}`);
    lines.push(`- Next step: ${cur.next}`);
  } else {
    const open = features.filter((f) => !/push the branch/.test(f.next));
    lines.push(`- Current branch: ${branch || '(no git)'}; not tied to any feature.`);
    if (open.length) lines.push(`- Unfinished features: ${open.map((f) => f.name).join(', ')} (see /sdd-status).`);
  }
  for (const r of openReqs) lines.push(`- Requirements ${r.name}: ${r.status || '?'}, ${r.specced}/${r.total} backlog items specced → ${r.next}`);
  for (const w of warnings()) lines.push(`- ⚠ ${w}`);
  process.stdout.write(lines.join('\n') + '\n');
  process.exit(0);
}

const out = [];
if (requirements.length && !only) {
  out.push('| Requirements | Status | Backlog specced | Next step |');
  out.push('|---|---|---|---|');
  for (const r of requirements) out.push(`| ${r.name} | ${r.status || '?'}${r.rev != null ? ` r${r.rev}` : ''} | ${r.specced}/${r.total} | ${r.next} |`);
  if (!onlyReq) out.push('');
} else if (onlyReq) {
  out.push(`No requirements ${onlyReq} in docs/requirements/.`);
}
if (onlyReq) {
  // Requirements only: no feature table.
} else if (!features.length) {
  out.push(only ? `No feature ${only} in specs/.` : 'No features in specs/ yet. Start with /sdd-ba <requirements>, /sdd-spec <description> or /sdd-fix <bug description>.');
} else {
  out.push('| Feature | Spec | Plan | Tasks/Fix | Progress | Next step |');
  out.push('|---|---|---|---|---|---|');
  for (const f of features) {
    const r = row(f);
    if (current && f.num === current.num) r[0] = `**${r[0]}** ← current branch`;
    out.push('| ' + r.join(' | ') + ' |');
  }
}
const w = warnings();
if (w.length) out.push('', ...w.map((x) => '⚠ ' + x));
if (fs.existsSync(path.join(root, 'specs')) && branch) out.push('', `Current branch: ${branch}`);
process.stdout.write(out.join('\n') + '\n');
