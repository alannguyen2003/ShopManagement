#!/usr/bin/env node
// PreToolUse (Bash and tools that reach outside the machine): Claude acts outside the project only with permission.
//   deny  publishing, pushing, deploying and changing infrastructure: the user does these themselves.
//   ask   any other effect outside this machine (network calls to non-local hosts, remote shells, mail,
//         GitHub writes, cloud CLIs, MCP/connector writes, publishing an Artifact): the user confirms each call.
//   deny  adding a dependency that docs/architecture.md does not allow (npm/pnpm/yarn/npx, dotnet add, system installers).
// Switches in .claude/settings.local.json: env SDD_EXTERNAL=off (external rules), SDD_DEPS=off (dependency rules).
// A guard rail against mistakes, not a security boundary: a script can still reach the network in ways this cannot see.
'use strict';

const sdd = require('./lib/sdd');
const deps = require('./lib/deps');

const input = sdd.readStdinJson();
const root = sdd.projectDir(input);
const tool = String(input.tool_name || '');
const ti = input.tool_input || {};
const externalOn = (process.env.SDD_EXTERNAL || '').toLowerCase() !== 'off';
const depsOn = (process.env.SDD_DEPS || '').toLowerCase() !== 'off';

const DENY_HINT = 'tell the user the exact command so they can run it themselves.';
const deny = (why) => sdd.block(`[SDD] Blocked: ${why}. Claude does not do this; ${DENY_HINT}`);
const ask = (why) => sdd.ask(`[SDD] Action outside the project: ${why}. Allow only if you asked for it.`);

// ---------- non-shell tools ----------

const READ_VERBS = /^(get|list|read|search|query|find|fetch|view|describe|show|guide|lookup|resolve|status|check|count|preview|inspect|diff|open|quickstart)$/;
const firstWord = (s) => String(s || '').replace(/([a-z])([A-Z])/g, '$1_$2').toLowerCase().split(/[_\-.\s]/)[0];

if (tool !== 'Bash') {
  if (!externalOn) process.exit(0);
  if (tool.startsWith('mcp__')) {
    const name = tool.split('__').slice(2).join('__');
    if (!READ_VERBS.test(firstWord(name))) ask(`MCP tool "${tool}" may change data in an external service`);
    process.exit(0);
  }
  if (['Artifact', 'ArtifactData', 'ArtifactComments'].includes(tool)) {
    const action = firstWord(ti.action || (tool === 'Artifact' ? 'publish' : ''));
    if (!READ_VERBS.test(action)) ask(`${tool} "${action}" publishes or changes content on claude.ai`);
    process.exit(0);
  }
  if (tool === 'RemoteTrigger') ask('starting a remote agent run');
  process.exit(0);
}

// ---------- shell ----------

const cmd = String(ti.command || '');
const ctx = { root, cwd: input.cwd || root, list: deps.readAllowlist(root), scaffold: sdd.humanCommand(root, input, 'scaffold') };

if (depsOn) {
  const r = deps.bashProblem(ctx, cmd);
  if (r) {
    sdd.block(
      `[SDD] Blocked: ${r}. Do not work around it (other package managers, manual downloads, editing manifests). ` +
        'Stop and propose /sdd-architecture "<add the dependency, layer, reason>" for the user to run.'
    );
  }
}
if (!externalOn) process.exit(0);

const LOCAL_HOST = /^(localhost|127\.\d+\.\d+\.\d+|0\.0\.0\.0|\[?::1\]?|host\.docker\.internal|[\w-]+\.localhost|\(localdb\)\S*)$/i;
function remoteHosts(text) {
  const hosts = [...String(text).matchAll(/\b[a-z][a-z0-9+.-]*:\/\/([^/\s:'"?#]+)/gi)].map((m) => m[1]);
  return hosts.filter((h) => !LOCAL_HOST.test(h));
}
// Hosts written without a scheme (curl example.com/x, ssh user@server).
function bareHosts(tokens) {
  return tokens
    .filter((t) => !t.startsWith('-') && !/:\/\//.test(t))
    .map((t) => t.replace(/^[^@\s]+@/, '').split(/[/:]/)[0])
    .filter((h) => /^[\w-]+(\.[\w-]+)+$/.test(h) && !/\.(json|txt|md|js|ts|cs|csproj|sln|html|xml|ya?ml|log|sh|ps1|zip|tgz)$/i.test(h) && !LOCAL_HOST.test(h));
}

const HTTP_CLIENTS = ['curl', 'wget', 'invoke-webrequest', 'iwr', 'invoke-restmethod', 'irm', 'http', 'https', 'xh', 'httpie', 'aria2c', 'start-bitstransfer'];
const REMOTE_SHELLS = ['ssh', 'scp', 'sftp', 'ftp', 'telnet', 'nc', 'ncat', 'socat', 'mosh', 'plink', 'pscp'];
const MAILERS = ['send-mailmessage', 'sendmail', 'mail', 'mailx', 'mutt', 'msmtp'];
const DB_CLIENTS = ['psql', 'sqlcmd', 'mysql', 'mongosh', 'mongo', 'redis-cli'];
const CLOUD = ['az', 'aws', 'gcloud', 'gsutil', 'doctl', 'oci', 'azd'];
const PLATFORMS = ['vercel', 'netlify', 'firebase', 'flyctl', 'fly', 'heroku', 'railway', 'surge', 'wrangler', 'amplify', 'eb', 'sls', 'serverless', 'cdk'];
const MUTATING = /^(create|delete|deploy|update|set|put|rm|cp|mv|sync|start|stop|restart|invoke|apply|publish|push|release|promote|up|destroy|remove|scale|--prod)$/i;
const INTERPRETERS = { node: ['-e', '--eval', '-p', '--print'], python: ['-c'], python3: ['-c'], py: ['-c'], deno: ['eval'], ruby: ['-e'], perl: ['-e'] };

function gitSub(rest) {
  for (let i = 0; i < rest.length; i++) {
    if (['-C', '-c', '--git-dir', '--work-tree'].includes(rest[i])) i++;
    else if (!rest[i].startsWith('-')) return { sub: rest[i].toLowerCase(), args: rest.slice(i + 1) };
  }
  return { sub: '', args: [] };
}

function ghReadOnly(rest) {
  const pos = deps.positionals(rest).map((s) => s.toLowerCase());
  const [group, action] = pos;
  if (['status', 'browse', 'help', 'version', '--version'].includes(group)) return true;
  if (group === 'auth') return action === 'status';
  if (group === 'api') {
    const i = rest.findIndex((t) => t === '-X' || t === '--method');
    const method = (i >= 0 ? rest[i + 1] : 'GET').toUpperCase();
    const hasFields = rest.some((t) => /^(-f|-F|--field|--raw-field|--input)$/.test(t));
    return method === 'GET' && !hasFields;
  }
  return ['view', 'list', 'status', 'diff', 'checks', 'watch', 'download', 'search'].includes(action);
}

function checkSegment(seg, depth) {
  const t = deps.tokenize(seg);
  if (!t.length) return;
  const exe = deps.exeName(t[0]);
  const rest = t.slice(1);
  const pos = deps.positionals(rest).map((s) => s.toLowerCase());
  const sub = pos[0] || '';

  if (['bash', 'sh', 'zsh', 'pwsh', 'powershell', 'cmd'].includes(exe) && depth < 2) {
    const i = rest.findIndex((x) => /^(-c|-command|\/c)$/i.test(x));
    if (i >= 0) deps.splitSegments(rest.slice(i + 1).join(' ')).forEach((s) => checkSegment(s, depth + 1));
    return;
  }

  if (exe === 'git') {
    const g = gitSub(rest);
    if (g.sub === 'push') D('git push sends commits to a remote');
    if (g.sub === 'remote' && ['add', 'set-url', 'rename', 'remove', 'rm'].includes((g.args[0] || '').toLowerCase())) A(`git remote ${g.args[0]} changes where this repo talks to`);
    if (g.sub === 'clone' || (g.sub === 'submodule' && (g.args[0] || '') === 'add')) A('downloading a repository from outside the project');
    if (g.sub === 'config' && g.args.some((a) => ['--global', '--system'].includes(a))) A('changing git configuration outside this project');
    if (g.sub === 'send-email') D('git send-email sends mail');
    return;
  }

  if (['npm', 'pnpm', 'yarn', 'bun'].includes(exe)) {
    if (['publish', 'unpublish', 'deprecate', 'dist-tag', 'owner', 'access', 'adduser', 'login', 'token', 'team', 'org'].includes(sub)) D(`${exe} ${sub} changes the package registry`);
    return;
  }
  if (exe === 'dotnet') {
    if (sub === 'nuget' && ['push', 'delete'].includes(pos[1])) D(`dotnet nuget ${pos[1]} changes a package feed`);
    return;
  }
  if (exe === 'nuget' && ['push', 'delete'].includes(sub)) D(`nuget ${sub} changes a package feed`);
  if (exe === 'docker' || exe === 'podman') {
    if (sub === 'push') D(`${exe} push uploads an image to a registry`);
    if (sub === 'login') A(`${exe} login stores registry credentials`);
    return;
  }
  if (exe === 'twine' && sub === 'upload') D('twine upload publishes a package');
  if (exe === 'cargo' && sub === 'publish') D('cargo publish publishes a crate');
  if (['vsce', 'ovsx'].includes(exe) && sub === 'publish') D(`${exe} publish publishes an extension`);
  if (exe === 'terraform' && ['apply', 'destroy', 'import', 'state', 'taint'].includes(sub)) D(`terraform ${sub} changes real infrastructure`);
  if (exe === 'pulumi' && ['up', 'destroy', 'import', 'refresh'].includes(sub)) D(`pulumi ${sub} changes real infrastructure`);
  if (['terraform', 'pulumi'].includes(exe) && !['fmt', 'validate', 'version', '--version', 'help'].includes(sub)) A(`${exe} ${sub} reads real infrastructure`);
  if (exe === 'kubectl' && ['apply', 'create', 'delete', 'replace', 'patch', 'scale', 'rollout', 'edit', 'set', 'label', 'annotate', 'drain', 'cordon', 'exec', 'cp'].includes(sub)) D(`kubectl ${sub} changes a cluster`);
  if (exe === 'kubectl') A('kubectl talks to a cluster');
  if (exe === 'helm' && ['install', 'upgrade', 'uninstall', 'rollback', 'push'].includes(sub)) D(`helm ${sub} changes a cluster`);

  if (exe === 'gh') {
    if (sub === 'release' && ['create', 'upload', 'delete', 'edit'].includes(pos[1])) D(`gh release ${pos[1]} publishes or changes a release`);
    if (sub === 'repo' && ['create', 'delete', 'archive', 'rename', 'edit', 'fork'].includes(pos[1])) D(`gh repo ${pos[1]} changes a GitHub repository`);
    if (!ghReadOnly(rest)) A(`gh ${pos.slice(0, 2).join(' ')} writes to GitHub`);
    return;
  }
  if (PLATFORMS.includes(exe)) {
    if (pos.length === 0 && exe === 'vercel') D('running vercel without arguments deploys');
    if (t.some((x) => MUTATING.test(x))) D(`${exe} ${sub} deploys or changes a hosted environment`);
    A(`${exe} talks to a hosting platform`);
  }
  if (CLOUD.includes(exe)) {
    if (t.slice(1).some((x) => MUTATING.test(x))) D(`${exe} ${pos.slice(0, 3).join(' ')} changes cloud resources`);
    A(`${exe} talks to a cloud account`);
  }

  if (MAILERS.includes(exe)) A('sending mail');
  if (REMOTE_SHELLS.includes(exe)) A(`${exe} connects to another machine`);
  if (exe === 'rsync' && rest.some((x) => /^[^-][^/\\]*:/.test(x))) A('rsync to or from another machine');
  if (HTTP_CLIENTS.includes(exe)) {
    const hosts = [...remoteHosts(seg), ...bareHosts(rest)];
    if (hosts.length) A(`${exe} sends a request to ${[...new Set(hosts)].join(', ')}`);
    return;
  }
  if (DB_CLIENTS.includes(exe)) {
    if (!/localhost|127\.0\.0\.1|\(localdb\)|::1/i.test(seg)) A(`${exe} may connect to a database outside this machine`);
    return;
  }
  if (['start', 'start-process', 'open', 'xdg-open', 'explorer'].includes(exe) && remoteHosts(seg).length) A(`opening ${remoteHosts(seg)[0]} in a browser`);
  if (INTERPRETERS[exe] && rest.some((x) => INTERPRETERS[exe].includes(x)) && remoteHosts(seg).length) A(`inline ${exe} code contacts ${remoteHosts(seg).join(', ')}`);
}

// Collect every finding first, so a harmless-looking part cannot hide a denied one behind a confirmation.
const found = [];
const D = (why) => found.push(['deny', why]);
const A = (why) => found.push(['ask', why]);
for (const seg of deps.splitSegments(cmd)) checkSegment(seg, 0);
const denied = found.find((f) => f[0] === 'deny');
if (denied) deny(denied[1]);
const asked = [...new Set(found.map((f) => f[1]))];
if (asked.length) ask(asked.join('; '));
process.exit(0);
