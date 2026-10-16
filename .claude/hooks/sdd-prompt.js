#!/usr/bin/env node
// UserPromptSubmit: when the user types a guarded command themselves, write a token so the guards allow what it needs.
//   /sdd-approve       → approval token (15 minutes): Status may move to Approved.
//   /sdd-scaffold      → scaffold token: src/ may be edited on main, generators may run, architecture.md may be written.
//   /sdd-architecture  → architecture token: docs/architecture.md may be edited.
// The scaffold and architecture tokens last only until the user's next prompt (and at most their TTL).
'use strict';

const sdd = require('./lib/sdd');

const input = sdd.readStdinJson();
const prompt = String(input.prompt || '');
const root = sdd.projectDir(input);

try {
  for (const [kind, command] of Object.entries(sdd.TOKEN_COMMANDS)) {
    if (new RegExp('^\\s*\\/' + command + '\\b').test(prompt)) sdd.writeToken(root, kind, prompt);
    else if (kind !== 'approval') sdd.clearToken(root, kind);
  }
} catch (e) {
  process.stderr.write('[SDD] Could not write the command token: ' + e.message + '\n');
}
process.exit(0);
