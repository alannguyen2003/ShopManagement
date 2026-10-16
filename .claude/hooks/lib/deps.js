// Dependency and layering rules read from docs/architecture.md. Uses Node built-ins only.
// The "Allowed dependencies" table is the only source of truth for which packages may be added:
//   | `Package` | nuget/npm | Allowed in | Purpose |
// "Allowed in": Clean Architecture layers for nuget (Domain, Application, Infrastructure, Api, Tests, tool),
// runtime/dev for npm, or * for anywhere. A trailing * in the package name matches a prefix (@types/*).
'use strict';

const fs = require('node:fs');
const path = require('node:path');
const sdd = require('./sdd');

const ARCH_FILE = 'docs/architecture.md';
const LAYERS = ['Domain', 'Application', 'Infrastructure', 'Api'];
// Which layers a project of each layer may reference (the Clean Architecture dependency rule).
const LAYER_REFS = {
  Domain: [],
  Application: ['Domain'],
  Infrastructure: ['Domain', 'Application'],
  Api: ['Domain', 'Application', 'Infrastructure'],
  Tests: LAYERS,
};
// Official generators /sdd-scaffold may run while its token is valid.
const SCAFFOLD_GENERATORS = ['create-next-app', 'create-vite'];

function readAllowlist(root) {
  const text = sdd.readText(path.join(root, ARCH_FILE));
  if (text == null) return null;
  const section = (text.split(/^## Allowed dependencies.*$/m)[1] || '').split(/^## /m)[0];
  const list = [];
  for (const row of section.match(/^\|.*\|[ \t]*$/gm) || []) {
    const cells = row.trim().replace(/^\||\|$/g, '').split('|').map((c) => c.trim());
    const m = /^`([^`]+)`$/.exec(cells[0] || '');
    if (!m) continue;
    list.push({
      name: m[1],
      eco: (cells[1] || '').replace(/`/g, '').toLowerCase(),
      scopes: (cells[2] || '*').replace(/`/g, '').split(/[,\s]+/).filter(Boolean).map((s) => s.toLowerCase()),
    });
  }
  return list;
}

function nameMatches(pattern, name) {
  const re = new RegExp('^' + pattern.toLowerCase().replace(/[.+?^${}()|[\]\\]/g, '\\$&').replace(/\*/g, '.*') + '$');
  return re.test(String(name).toLowerCase());
}

// Returns null when allowed, otherwise the reason. scope null = any scope.
function checkPackage(list, eco, name, scope) {
  if (!list) return `${ARCH_FILE} does not exist yet, so no dependency may be added. Run /sdd-scaffold first`;
  const entry = list.find((e) => e.eco === eco && nameMatches(e.name, name));
  if (!entry) return `"${name}" (${eco}) is not in the "Allowed dependencies" table of ${ARCH_FILE}`;
  if (scope && !entry.scopes.includes('*') && !entry.scopes.includes(String(scope).toLowerCase())) {
    return `"${name}" is allowed only in: ${entry.scopes.join(', ')} (not in ${scope})`;
  }
  return null;
}

// Layer of a .NET project from its path or name: <Name>.<Layer>.csproj, anything under tests/ or *.Tests is Tests.
function layerOf(p) {
  if (!p) return null;
  const norm = String(p).replace(/\\/g, '/').replace(/\/+$/, '');
  const base = path.posix.basename(norm).replace(/\.csproj$/i, '');
  if (/Tests?$/i.test(base) || /(^|\/)tests\//i.test(norm + '/')) return 'Tests';
  const last = base.split('.').pop();
  return LAYERS.find((l) => l.toLowerCase() === String(last).toLowerCase()) || null;
}

function checkProjectRef(fromLayer, toLayer) {
  if (!fromLayer || !toLayer || fromLayer === 'Tests') return null;
  if (LAYER_REFS[fromLayer].includes(toLayer)) return null;
  return `Clean Architecture: ${fromLayer} must not reference ${toLayer} (allowed: ${LAYER_REFS[fromLayer].join(', ') || 'nothing'})`;
}

// ---------- manifests edited with Edit/Write ----------

function csprojRefs(text) {
  const all = (re) => [...String(text || '').matchAll(re)].map((m) => m[1]);
  return {
    packages: all(/<PackageReference\b[^>]*?\b(?:Include|Update)\s*=\s*"([^"]+)"/gi),
    projects: all(/<ProjectReference\b[^>]*?\bInclude\s*=\s*"([^"]+)"/gi),
  };
}

function packageJsonDeps(text) {
  try {
    const j = JSON.parse(text || '{}');
    const keys = (o) => Object.keys(o || {});
    return { runtime: [...keys(j.dependencies), ...keys(j.peerDependencies), ...keys(j.optionalDependencies)], dev: keys(j.devDependencies) };
  } catch {
    return null;
  }
}

function isManifest(rel) {
  if (/(^|\/)node_modules\//.test(rel)) return false;
  return /\.csproj$/i.test(rel) || /(^|\/)Directory\.(Build|Packages)\.props$/i.test(rel) || /(^|\/)package\.json$/.test(rel);
}

// Problems introduced by changing a manifest from `before` to `after` (only additions are checked).
function manifestProblems(root, rel, before, after) {
  const list = readAllowlist(root);
  const problems = [];
  const added = (a, b) => a.filter((x) => !b.some((y) => y.toLowerCase() === x.toLowerCase()));
  if (/package\.json$/.test(rel)) {
    const b = packageJsonDeps(before) || { runtime: [], dev: [] };
    const a = packageJsonDeps(after);
    if (!a) return problems;
    for (const n of added(a.runtime, b.runtime)) problems.push(checkPackage(list, 'npm', n, 'runtime'));
    for (const n of added(a.dev, b.dev)) problems.push(checkPackage(list, 'npm', n, 'dev'));
    return problems.filter(Boolean);
  }
  const b = csprojRefs(before);
  const a = csprojRefs(after);
  if (/\.props$/i.test(rel)) {
    // Directory.Build.props applies to every project, so only packages allowed everywhere (*) fit there.
    const scope = /Build\.props$/i.test(rel) ? '*' : null;
    for (const n of added(a.packages, b.packages)) problems.push(checkPackage(list, 'nuget', n, scope));
    for (const n of [...String(after).matchAll(/<PackageVersion\b[^>]*?\bInclude\s*=\s*"([^"]+)"/gi)].map((m) => m[1])) {
      if (!new RegExp('Include\\s*=\\s*"' + n.replace(/[.*+?^${}()|[\]\\]/g, '\\$&') + '"', 'i').test(before || '')) problems.push(checkPackage(list, 'nuget', n, null));
    }
    return problems.filter(Boolean);
  }
  const layer = layerOf(rel);
  for (const n of added(a.packages, b.packages)) problems.push(checkPackage(list, 'nuget', n, layer || '*'));
  for (const p of added(a.projects, b.projects)) problems.push(checkProjectRef(layer, layerOf(p)));
  return problems.filter(Boolean);
}

// ---------- shell commands ----------

// Splits a command line on ; && || | and newlines that are outside quotes.
function splitSegments(cmd) {
  const s = String(cmd);
  const out = [];
  let cur = '';
  let quote = null;
  for (let i = 0; i < s.length; i++) {
    const c = s[i];
    if (quote) {
      if (c === quote) quote = null;
      cur += c;
    } else if (c === '"' || c === "'") {
      quote = c;
      cur += c;
    } else if (c === ';' || c === '\n' || c === '|' || (c === '&' && s[i + 1] === '&')) {
      if (s[i + 1] === c) i++;
      out.push(cur);
      cur = '';
    } else cur += c;
  }
  out.push(cur);
  return out.map((x) => x.trim()).filter(Boolean);
}

function tokenize(seg) {
  const out = [];
  const re = /"([^"]*)"|'([^']*)'|(\S+)/g;
  let m;
  while ((m = re.exec(seg))) out.push(m[1] ?? m[2] ?? m[3]);
  // Drop leading env assignments (FOO=bar cmd) and sudo.
  while (out.length && (/^[A-Za-z_][A-Za-z0-9_]*=/.test(out[0]) || out[0] === 'sudo')) out.shift();
  return out;
}

function exeName(tok) {
  return path.basename(String(tok || '').replace(/\\/g, '/')).toLowerCase().replace(/\.(cmd|exe|ps1|bat)$/, '');
}

// Registry package name from an npm spec; null for git URLs, tarballs, local paths and aliases.
function npmName(spec) {
  if (!spec || /^(\.|\/|~)|:|^[^@][^/]*\/|\.tgz$/.test(spec)) return null;
  if (spec.startsWith('@')) {
    const i = spec.indexOf('@', 1);
    return i > 0 ? spec.slice(0, i) : spec;
  }
  return spec.split('@')[0];
}

// Options whose next token is their value, so it is not mistaken for a package or subcommand.
// Short flags that are booleans in some tool (npm -o, -f, -s…) are deliberately left out.
const OPTS_WITH_VALUE = ['--prefix', '-C', '--dir', '--cwd', '-w', '--workspace', '--filter', '--registry', '--project', '--package', '--version', '--source', '--framework'];

function positionals(tokens) {
  const out = [];
  for (let i = 0; i < tokens.length; i++) {
    const t = tokens[i];
    if (t.startsWith('-')) {
      if (!t.includes('=') && OPTS_WITH_VALUE.includes(t)) i++;
      continue;
    }
    out.push(t);
  }
  return out;
}

function optionValue(tokens, names) {
  for (let i = 0; i < tokens.length; i++) {
    for (const n of names) {
      if (tokens[i] === n) return tokens[i + 1];
      if (tokens[i].startsWith(n + '=')) return tokens[i].slice(n.length + 1);
    }
  }
  return null;
}

function localBinExists(root, cwd, name) {
  const dirs = [root, path.join(root, 'src', 'frontend'), cwd].filter(Boolean);
  return dirs.some((d) => ['', '.cmd', '.ps1'].some((ext) => fs.existsSync(path.join(d, 'node_modules', '.bin', name + ext))));
}

function checkRunner(ctx, spec) {
  const name = npmName(spec);
  if (!name) return `running "${spec}" with a package runner is not allowed (only registry packages from ${ARCH_FILE})`;
  if (localBinExists(ctx.root, ctx.cwd, name)) return null;
  if (ctx.scaffold && SCAFFOLD_GENERATORS.includes(name)) return null;
  return checkPackage(ctx.list, 'npm', name, null) && `"${name}" is neither installed in the project nor in the "Allowed dependencies" table of ${ARCH_FILE}`;
}

function checkCreate(ctx, spec) {
  const name = npmName(spec);
  if (!name) return `"${spec}" is not a registry generator`;
  const pkg = name.startsWith('@') ? (name.includes('/') ? name.replace('/', '/create-') : name + '/create') : 'create-' + name;
  if (ctx.scaffold && SCAFFOLD_GENERATORS.includes(pkg)) return null;
  return checkPackage(ctx.list, 'npm', pkg, null) && `the generator "${pkg}" may only run inside /sdd-scaffold`;
}

function checkJsPackageManager(ctx, exe, rest) {
  const pos = positionals(rest);
  const sub = (pos[0] || '').toLowerCase();
  const args = rest.slice(rest.indexOf(pos[0]) + 1);
  const pkgs = positionals(args);
  const global = args.some((t) => ['-g', '--global'].includes(t)) || sub === 'global';
  const dev = args.some((t) => ['-D', '--save-dev', '--dev'].includes(t));
  const installSubs = { npm: ['install', 'i', 'add', 'in', 'isntall'], pnpm: ['add', 'install', 'i'], yarn: ['add', 'global'], bun: ['add', 'install', 'i'] }[exe];
  const runSubs = { npm: ['exec', 'x'], pnpm: ['dlx'], yarn: ['dlx'], bun: ['x'] }[exe];
  if (installSubs.includes(sub) && pkgs.length) {
    if (global) return 'global installs are not allowed; tools go in the project as devDependencies listed in ' + ARCH_FILE;
    for (const spec of sub === 'global' ? pkgs.slice(1) : pkgs) {
      const name = npmName(spec);
      if (!name) return `installing "${spec}" is not allowed (only registry packages from ${ARCH_FILE})`;
      const r = checkPackage(ctx.list, 'npm', name, dev ? 'dev' : 'runtime');
      if (r) return r;
    }
    return null;
  }
  if (runSubs.includes(sub)) return checkRunner(ctx, optionValue(args, ['--package', '-p']) || pkgs[0]);
  if ((sub === 'create' || sub === 'init') && pkgs[0]) return checkCreate(ctx, pkgs[0]);
  return null;
}

function checkDotnet(ctx, rest) {
  const pos = positionals(rest);
  const [a, b] = [(pos[0] || '').toLowerCase(), (pos[1] || '').toLowerCase()];
  const project = optionValue(rest, ['--project']);
  const cwdLayer = layerOf(ctx.cwd);
  // dotnet add [PROJECT] package NAME  |  dotnet package add NAME [--project P]
  if ((a === 'add' && pos.includes('package')) || (a === 'package' && b === 'add')) {
    const i = pos.indexOf(a === 'add' ? 'package' : 'add');
    const name = pos[i + 1];
    const proj = a === 'add' ? (i > 1 ? pos[1] : null) : project;
    return checkPackage(ctx.list, 'nuget', name, layerOf(proj) || cwdLayer || '*');
  }
  // dotnet add [PROJECT] reference P...  |  dotnet reference add P... [--project P]
  if ((a === 'add' && pos.includes('reference')) || (a === 'reference' && b === 'add')) {
    const i = pos.indexOf(a === 'add' ? 'reference' : 'add');
    const from = layerOf(a === 'add' ? (i > 1 ? pos[1] : null) : project) || cwdLayer;
    for (const p of pos.slice(i + 1)) {
      const r = checkProjectRef(from, layerOf(p));
      if (r) return r;
    }
    return null;
  }
  if (a === 'tool' && ['install', 'update', 'exec', 'run'].includes(b)) {
    if (rest.some((t) => ['-g', '--global', '--tool-path'].includes(t))) return 'global .NET tools are not allowed; use a local tool manifest';
    if (b === 'run') return null;
    return pos[2] ? checkPackage(ctx.list, 'nuget', pos[2], 'tool') : null;
  }
  if (a === 'new' && b === 'install') return ctx.scaffold ? null : 'installing .NET templates is only allowed inside /sdd-scaffold';
  if (a === 'workload' && ['install', 'update', 'restore'].includes(b)) return 'installing .NET workloads changes the machine; ask the user to run it';
  return null;
}

const OTHER_ECOSYSTEMS = {
  pip: ['install'], pip3: ['install'], pipx: ['install'], uv: ['add', 'pip'], poetry: ['add'], gem: ['install'],
  cargo: ['install', 'add'], go: ['install', 'get'], composer: ['require'], conda: ['install'],
};
const SYSTEM_INSTALLERS = {
  winget: ['install', 'upgrade'], choco: ['install', 'upgrade'], scoop: ['install'], brew: ['install'], apt: ['install'],
  'apt-get': ['install'], yum: ['install'], dnf: ['install'], snap: ['install'],
};
const PS_INSTALLERS = ['install-module', 'install-package', 'install-script', 'install-psresource'];
const SHELLS = ['bash', 'sh', 'zsh', 'pwsh', 'powershell', 'cmd'];

// Returns the reason when a shell command adds a dependency that architecture.md does not allow.
function bashProblem(ctx, cmd, depth = 0) {
  for (const seg of splitSegments(cmd)) {
    const t = tokenize(seg);
    if (!t.length) continue;
    const exe = exeName(t[0]);
    const rest = t.slice(1);
    const sub = (positionals(rest)[0] || '').toLowerCase();
    let r = null;
    if (SHELLS.includes(exe) && depth < 2) {
      const i = rest.findIndex((x) => /^(-c|-command|\/c)$/i.test(x));
      if (i >= 0) r = bashProblem(ctx, rest.slice(i + 1).join(' '), depth + 1);
    } else if (['npm', 'pnpm', 'yarn', 'bun'].includes(exe)) r = checkJsPackageManager(ctx, exe, rest);
    else if (['npx', 'bunx', 'pnpx'].includes(exe)) r = checkRunner(ctx, optionValue(rest, ['--package', '-p']) || positionals(rest)[0]);
    else if (exe === 'dotnet') r = checkDotnet(ctx, rest);
    else if (OTHER_ECOSYSTEMS[exe] && OTHER_ECOSYSTEMS[exe].includes(sub)) r = `${exe} is not part of the stack in ${ARCH_FILE}`;
    else if (SYSTEM_INSTALLERS[exe] && SYSTEM_INSTALLERS[exe].includes(sub)) r = 'installing software on the machine is not allowed; ask the user to install it';
    else if (PS_INSTALLERS.includes(exe)) r = 'installing PowerShell modules is not allowed; ask the user to install it';
    if (r) return r;
  }
  return null;
}

module.exports = {
  ARCH_FILE,
  LAYERS,
  LAYER_REFS,
  readAllowlist,
  checkPackage,
  layerOf,
  isManifest,
  manifestProblems,
  splitSegments,
  tokenize,
  exeName,
  positionals,
  bashProblem,
};
