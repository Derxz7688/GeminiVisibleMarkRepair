import { createReadStream, createWriteStream, existsSync, mkdirSync, readFileSync, statSync } from 'node:fs';
import { rm } from 'node:fs/promises';
import { createServer } from 'node:http';
import { basename, extname, join, resolve } from 'node:path';
import { randomUUID } from 'node:crypto';
import { spawn } from 'node:child_process';

const host = '127.0.0.1';
const port = Number(process.env.PORT || 4317);
const root = resolve(import.meta.dirname);
const repairScript = join(root, 'repair-video.ps1');
const jobsRoot = join(root, '.jobs');
const html = readFileSync(join(root, 'public', 'index.html'), 'utf8');
const jobs = new Map();
mkdirSync(jobsRoot, { recursive: true });

function json(res, status, value) {
  res.writeHead(status, { 'content-type': 'application/json; charset=utf-8' });
  res.end(JSON.stringify(value));
}

function safeName(value) {
  return basename(decodeURIComponent(value || 'input.mp4')).replace(/[^\p{L}\p{N}._ -]/gu, '_');
}

function fileResponse(req, res, path, download = false) {
  if (!existsSync(path)) return json(res, 404, { error: '文件不存在' });
  const size = statSync(path).size;
  const range = req.headers.range;
  const headers = {
    'content-type': extname(path) === '.mp4' ? 'video/mp4' : 'application/octet-stream',
    'accept-ranges': 'bytes',
  };
  if (download) headers['content-disposition'] = `attachment; filename*=UTF-8''${encodeURIComponent(basename(path))}`;
  if (!range) {
    res.writeHead(200, { ...headers, 'content-length': size });
    return createReadStream(path).pipe(res);
  }
  const [startText, endText] = range.replace('bytes=', '').split('-');
  const start = Number(startText);
  const end = endText ? Number(endText) : size - 1;
  res.writeHead(206, {
    ...headers,
    'content-length': end - start + 1,
    'content-range': `bytes ${start}-${end}/${size}`,
  });
  createReadStream(path, { start, end }).pipe(res);
}

async function upload(req, res) {
  const id = randomUUID();
  const dir = join(jobsRoot, id);
  mkdirSync(dir, { recursive: true });
  const filename = safeName(req.headers['x-filename']);
  const input = join(dir, filename);
  const output = join(dir, `${basename(filename, extname(filename))}_visible_mark_repaired.mp4`);
  const stream = createWriteStream(input);
  req.pipe(stream);
  await new Promise((ok, fail) => {
    stream.on('finish', ok);
    stream.on('error', fail);
    req.on('error', fail);
  });
  const job = { id, dir, filename, input, output, state: 'ready', logs: [], process: null };
  jobs.set(id, job);
  json(res, 200, { id, filename, state: job.state });
}

function processJob(job, res) {
  if (job.state === 'processing') return json(res, 409, { error: '任务正在处理中' });
  job.state = 'processing';
  job.logs = [];
  const child = spawn('powershell.exe', [
    '-NoProfile', '-NonInteractive', '-ExecutionPolicy', 'Bypass', '-File', repairScript,
    '-InputPath', job.input, '-OutputPath', job.output,
  ], { cwd: root, windowsHide: true, stdio: ['ignore', 'pipe', 'pipe'] });
  job.process = child;
  const append = (chunk) => {
    const text = chunk.toString().replace(/\x1b\[[0-9;]*m/g, '');
    job.logs.push(text);
    if (job.logs.length > 400) job.logs.shift();
  };
  child.stdout.on('data', append);
  child.stderr.on('data', append);
  child.on('error', (error) => {
    append(error.message);
    job.state = 'failed';
  });
  child.on('exit', (code) => {
    job.process = null;
    job.state = code === 0 && existsSync(job.output) ? 'completed' : 'failed';
  });
  json(res, 202, { id: job.id, state: job.state });
}

createServer(async (req, res) => {
  try {
    if (req.method === 'POST' && req.url === '/api/upload') return await upload(req, res);
    const match = req.url?.match(/^\/api\/jobs\/([^/]+)(?:\/(process|video|download))?$/);
    if (match) {
      const job = jobs.get(match[1]);
      if (!job) return json(res, 404, { error: '任务不存在' });
      if (req.method === 'POST' && match[2] === 'process') return processJob(job, res);
      if (req.method === 'GET' && match[2] === 'video') return fileResponse(req, res, job.output);
      if (req.method === 'GET' && match[2] === 'download') return fileResponse(req, res, job.output, true);
      if (req.method === 'GET' && !match[2]) return json(res, 200, {
        id: job.id, filename: job.filename, state: job.state, logs: job.logs.join(''),
      });
      if (req.method === 'DELETE' && !match[2]) {
        if (job.process) job.process.kill();
        jobs.delete(job.id);
        await rm(job.dir, { recursive: true, force: true });
        return json(res, 200, { deleted: true });
      }
    }
    res.writeHead(200, { 'content-type': 'text/html; charset=utf-8' });
    res.end(html);
  } catch (error) {
    json(res, 500, { error: error.message });
  }
}).listen(port, host, () => console.log(`Gemini visible mark repair: http://${host}:${port}/`));

for (const signal of ['SIGINT', 'SIGTERM']) {
  process.on(signal, async () => {
    for (const job of jobs.values()) {
      if (job.process) job.process.kill();
      await rm(job.dir, { recursive: true, force: true });
    }
    process.exit(0);
  });
}
