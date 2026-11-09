/**
 * Opaque sf: round-trip check (run from web/):
 *   node --experimental-strip-types src/widgets/sailflow/vue-bpmn/scripts/check-sf-roundtrip.mjs
 * or with tsx after wiring; this mjs inlines a minimal assert via dynamic vite-node alternative.
 *
 * Prefer: pnpm exec vite-node src/widgets/sailflow/vue-bpmn/scripts/check-sf-roundtrip.ts
 */
import { readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const root = join(__dirname, '..');
const fixture = readFileSync(join(root, 'fixtures/sail-with-sf.bpmn'), 'utf8');

const importMod = await import(pathToFileURL(join(root, 'core/xml/importXml.ts')).href);
const exportMod = await import(pathToFileURL(join(root, 'core/xml/exportXml.ts')).href);

const { result, elements } = importMod.importXml(fixture);
const { xml } = exportMod.exportXml({
  rootId: result.rootId,
  definitionsId: result.definitionsId,
  definitionsAttrs: result.definitionsAttrs,
  elements,
  selection: [],
  activeTool: null,
  viewbox: { x: 0, y: 0, width: 1000, height: 600, scale: 1 },
});

const checks = [
  ['xmlns:sf', xml.includes('xmlns:sf')],
  ['sf:code', xml.includes('sf:code')],
  ['sf:version', xml.includes('sf:version')],
  ['extensionElements', xml.includes('extensionElements')],
  ['AssigneeResolve', xml.includes('AssigneeResolve')],
  ['LeaveJudge', xml.includes('LeaveJudge')],
  ['ApproverPick', xml.includes('ApproverPick')],
];

let failed = 0;
for (const [name, ok] of checks) {
  console.log(ok ? `PASS ${name}` : `FAIL ${name}`);
  if (!ok) failed++;
}
if (failed) {
  console.error(xml.slice(0, 1200));
  process.exit(1);
}
console.log('sf round-trip OK');
