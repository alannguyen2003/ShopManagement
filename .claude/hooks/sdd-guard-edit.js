#!/usr/bin/env node
// PreToolUse (Edit|Write|MultiEdit|NotebookEdit): turns the SDD rules into mechanisms.
//  1. Nobody but the user can move a document to Approved.
//  2. docs/architecture.md changes only through /sdd-scaffold or /sdd-architecture, typed by the user.
//  3. Manifests (*.csproj, Directory.*.props, package.json) may only gain dependencies listed in docs/architecture.md
//     and project references that respect the Clean Architecture layers.
//  4. Product code cannot be edited until the current feature's documents are approved
//     (or, for src/, while the user's /sdd-scaffold is running).
//  5. .claude/state/ (where the tokens live) cannot be edited.
// Switches in .claude/settings.local.json: env SDD_GATE=off (code gate), SDD_DEPS=off (dependency guard).
'use strict';

const path = require('node:path');
const sdd = require('./lib/sdd');
const deps = require('./lib/deps');

const input = sdd.readStdinJson();
const root = sdd.projectDir(input);
const ti = input.tool_input || {};
const target = ti.file_path || ti.notebook_path;
if (!target) process.exit(0);

const rel = sdd.toRel(root, target);
if (rel === null) process.exit(0); // outside the project

if (rel.startsWith('.claude/state/')) {
  sdd.block('[SDD] .claude/state/ is managed by hooks and must not be edited directly.');
}

const before = () => sdd.readText(path.join(root, rel)) || '';

// 1. Approval gate.
if (/^(specs\/[^/]+\/(spec|plan|tasks|fix)|docs\/requirements\/[^/]+\/requirements)\.md$/.test(rel)) {
  // Simulate the file after the edit so only real promotions to Approved are caught.
  const b = before();
  const promotes = !sdd.STATUS_RE.test(b) && sdd.STATUS_RE.test(sdd.afterEdit(input, b));
  if (promotes && !sdd.humanApproved(root, input)) {
    sdd.block(
      '[SDD] Blocked: only the user can approve documents, by typing /sdd-approve <spec|plan|tasks|fix|requirements> themselves. ' +
        'Do not set Status to Approved any other way.'
    );
  }
  process.exit(0);
}

// 2. Architecture gate.
if (rel === deps.ARCH_FILE) {
  if (!sdd.humanCommand(root, input, 'architecture') && !sdd.humanCommand(root, input, 'scaffold')) {
    sdd.block(
      `[SDD] Blocked editing ${rel}: the architecture and its allowed dependencies change only when the user types ` +
        '/sdd-architecture <change> (or /sdd-scaffold for the first version). Propose the command and stop.'
    );
  }
  process.exit(0);
}

// 3. Dependency guard.
if (deps.isManifest(rel) && (process.env.SDD_DEPS || '').toLowerCase() !== 'off') {
  const b = before();
  const problems = deps.manifestProblems(root, rel, b, sdd.afterEdit(input, b));
  if (problems.length) {
    sdd.block(
      `[SDD] Blocked editing ${rel}: ${problems.join('; ')}. ` +
        'Do not work around it. Stop and propose /sdd-architecture "<add the dependency, layer, reason>" for the user to run.'
    );
  }
}

// 4. Code gate: documentation and process configuration may always be edited.
const ALWAYS_ALLOWED = [/^specs\//, /^docs\//, /^\.claude\//, /^[^/]+\.md$/, /^\.gitignore$/];
if (ALWAYS_ALLOWED.some((r) => r.test(rel))) process.exit(0);
if ((process.env.SDD_GATE || '').toLowerCase() === 'off') process.exit(0);
if (rel.startsWith('src/') && sdd.humanCommand(root, input, 'scaffold')) process.exit(0);

if (!sdd.isGitRepo(root)) {
  sdd.block(`[SDD] Blocked editing ${rel}: the project is not a git repository yet. Run /sdd-init first.`);
}
const branch = sdd.currentBranch(root);
const b = sdd.parseBranch(branch);
if (!b) {
  sdd.block(
    `[SDD] Blocked editing ${rel}: current branch "${branch}" is not feat/NNN-slug or fix/NNN-slug. ` +
      'Product code may only change inside an approved feature (/sdd-spec) or fix (/sdd-fix), or during /sdd-scaffold (src/ only). ' +
      'If the user explicitly asked for an out-of-process change, stop and ask them to set SDD_GATE=off.'
  );
}
const perm = sdd.buildPermission(sdd.findFeature(root, b.num));
if (!perm.ok) {
  sdd.block(`[SDD] Blocked editing ${rel}: ${perm.reason}. Run /sdd-status to see the next step.`);
}
process.exit(0);
