#!/usr/bin/env node
// PreToolUse (Bash): blocks the obvious shell detours around the approval gate.
// This is a guard rail against mistakes, not a security boundary: a shell can write files in countless ways.
'use strict';

const sdd = require('./lib/sdd');

const input = sdd.readStdinJson();
const root = sdd.projectDir(input);
const cmd = String((input.tool_input || {}).command || '');

if (/\.claude[\\/]+state/.test(cmd)) {
  sdd.block('[SDD] Blocked: .claude/state/ is managed by hooks and must not be read or written from the shell.');
}

// Writing "Status: Approved" into a specs/ or requirements document from the shell (sed -i, echo >, tee, Set-Content...).
const writes = /(sed\s+(-[a-zA-Z]*i|--in-place)|perl\s+-[a-zA-Z]*i|\btee\b|Set-Content|Out-File|Add-Content|(^|[^0-9&>])>{1,2}\s*[^&\s])/;
if (/specs|requirements/.test(cmd) && /Status/.test(cmd) && /Approved/.test(cmd) && writes.test(cmd) && !sdd.humanApproved(root, input)) {
  sdd.block('[SDD] Blocked: do not set Status to Approved from the shell. Only the user approves, via /sdd-approve.');
}
process.exit(0);
