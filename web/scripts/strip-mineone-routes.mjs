import fs from 'node:fs';

const p = new URL('../src/router/elegant/routes.ts', import.meta.url);
let text = fs.readFileSync(p, 'utf8');
const start = text.indexOf('export const routes');
const arrStart = text.indexOf('[', start);
const arrEnd = text.lastIndexOf(']');
const header = text.slice(0, arrStart + 1);
const footer = text.slice(arrEnd);
const body = text.slice(arrStart + 1, arrEnd);

const items = [];
let depth = 0;
let cur = '';
let inStr = null;
let esc = false;

for (let i = 0; i < body.length; i++) {
  const ch = body[i];
  cur += ch;
  if (inStr) {
    if (esc) {
      esc = false;
      continue;
    }
    if (ch === '\\') {
      esc = true;
      continue;
    }
    if (ch === inStr) inStr = null;
    continue;
  }
  if (ch === '"' || ch === "'" || ch === '`') {
    inStr = ch;
    continue;
  }
  if (ch === '{') depth++;
  else if (ch === '}') {
    depth--;
    if (depth === 0) {
      const item = cur.trim().replace(/^,/, '').replace(/,$/, '').trim();
      if (item) items.push(item);
      cur = '';
    }
  }
}

const kept = items.filter(it => !/mineone/i.test(it));
const removed = items.length - kept.length;
const out = `${header}\n${kept.map((it, i) => (i < kept.length - 1 ? `${it},` : it)).join('\n')}\n${footer}`;
fs.writeFileSync(p, out);
console.log(JSON.stringify({ total: items.length, removed, kept: kept.length }));
