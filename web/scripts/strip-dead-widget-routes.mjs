/**
 * 去掉 elegant 中已不存在目录的死引用：beedesign / nflow / widgets/ccflow
 * （保留 sysext/views/ccflow）
 */
import fs from 'node:fs';
import path from 'node:path';

const root = process.cwd();
const deadRe = /_widgets_(?:beedesign|nflow|ccflow)_/;

function stripImports() {
  const p = path.join(root, 'src/router/elegant/imports.ts');
  let text = fs.readFileSync(p, 'utf8');
  const before = text;
  text = text.replace(/\n  _widgets_(?:beedesign|nflow|ccflow)_[^:]+: \(\) => import\([^)]+\),?/g, '');
  fs.writeFileSync(p, text);
  return before !== text;
}

function stripShared() {
  const p = path.join(root, 'src/router/elegant/shared.ts');
  let text = fs.readFileSync(p, 'utf8');
  const before = text;
  text = text.replace(/\n  "_widgets_(?:beedesign|nflow|ccflow)_[^"]+": "[^"]+",?/g, '');
  fs.writeFileSync(p, text);
  return before !== text;
}

function stripRoutes() {
  const p = path.join(root, 'src/router/elegant/routes.ts');
  const text = fs.readFileSync(p, 'utf8');
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

  const kept = items.filter(it => !deadRe.test(it));
  const out = `${header}\n${kept.map((it, i) => (i < kept.length - 1 ? `${it},` : it)).join('\n')}\n${footer}`;
  fs.writeFileSync(p, out);
  return { before: items.length, after: kept.length, removed: items.length - kept.length };
}

const importsChanged = stripImports();
const sharedChanged = stripShared();
const routes = stripRoutes();
console.log(JSON.stringify({ importsChanged, sharedChanged, routes }, null, 2));
