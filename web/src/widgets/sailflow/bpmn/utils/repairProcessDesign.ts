/**
 * 流程设计修复：清理断链/无效图元，补全待办人规则与其它缺省配置，并扫描无法自动修的问题。
 */
import { defaultButtonsSetting } from '../buttonsCatalog';
import {
  DEFAULT_ASSIGNEE_RESOLVE_TYPE,
  DEFAULT_LEAVE_JUDGE_PARAMS,
  DEFAULT_LEAVE_JUDGE_TYPE
} from '../assigneeDefaults';
import { sanitizeBpmnXml } from './sanitizeBpmnXml';

const SF_NS = 'http://www.saiflow.com/schema/sf';
const BPMN_NS = 'http://www.omg.org/spec/BPMN/20100524/MODEL';

const START_ALIASES = new Set(['', 'start', 'startevent', '开始事件']);
const END_ALIASES = new Set(['', 'end', 'endevent', '结束事件']);

export type RepairProcessOptions = {
  cleanOrphans: boolean;
  fillAssignee: boolean;
  resolveType: string;
  leaveType: string;
  leaveParams: string;
  fillButtons: boolean;
  renameEvents: boolean;
};

export type RepairProcessApplied = {
  removedFlows: string[];
  removedDi: string[];
  filledAssignee: string[];
  filledButtons: string[];
  renamedEvents: string[];
};

export type RepairProcessResult = {
  xml: string;
  applied: RepairProcessApplied;
  remaining: string[];
};

export function defaultRepairOptions(): RepairProcessOptions {
  return {
    cleanOrphans: true,
    fillAssignee: true,
    resolveType: DEFAULT_ASSIGNEE_RESOLVE_TYPE,
    leaveType: DEFAULT_LEAVE_JUDGE_TYPE,
    leaveParams: DEFAULT_LEAVE_JUDGE_PARAMS,
    fillButtons: true,
    renameEvents: true
  };
}

function localName(el: Element): string {
  return el.localName || el.tagName.replace(/^.*:/, '');
}

function findSfChild(parent: Element, names: string[]): Element | null {
  const want = new Set(names.map(n => n.toLowerCase()));
  for (let i = 0; i < parent.children.length; i++) {
    const c = parent.children[i];
    if (want.has(localName(c).toLowerCase())) return c;
  }
  return null;
}

function ensureExtensionElements(task: Element, doc: Document): Element {
  let ee = findSfChild(task, ['extensionElements']);
  if (ee) return ee;
  // also check namespaced BPMN children
  for (let i = 0; i < task.children.length; i++) {
    const c = task.children[i];
    if (/^extensionElements$/i.test(localName(c))) {
      return c;
    }
  }
  ee = doc.createElementNS(BPMN_NS, 'bpmn:extensionElements');
  task.appendChild(ee);
  return ee;
}

function getAttr(el: Element, name: string): string {
  return (el.getAttribute(name) || el.getAttributeNS(null, name) || '').trim();
}

function ensureSfConfig(
  ee: Element,
  doc: Document,
  tagLocal: string,
  attrs: Record<string, string>
): { created: boolean; filledType: boolean } {
  let node = findSfChild(ee, [tagLocal]);
  let created = false;
  let filledType = false;
  if (!node) {
    node = doc.createElementNS(SF_NS, `sf:${tagLocal}`);
    ee.appendChild(node);
    created = true;
  }
  const curType = getAttr(node, 'type');
  if (!curType) {
    if (attrs.type != null) node.setAttribute('type', attrs.type);
    filledType = true;
  }
  if (attrs.params != null && (created || filledType || !node.hasAttribute('params'))) {
    if (filledType || created) {
      node.setAttribute('params', attrs.params);
    }
  }
  if (attrs.json != null && (created || !getAttr(node, 'json'))) {
    node.setAttribute('json', attrs.json);
  }
  return { created, filledType };
}

function ensureDefinitionsSfNs(doc: Document): void {
  const root = doc.documentElement;
  if (!root) return;
  if (!root.getAttribute('xmlns:sf') && !root.getAttributeNS('http://www.w3.org/2000/xmlns/', 'sf')) {
    root.setAttribute('xmlns:sf', SF_NS);
  }
}

function collectByLocal(doc: Document, re: RegExp): Element[] {
  const out: Element[] = [];
  const all = doc.getElementsByTagName('*');
  for (let i = 0; i < all.length; i++) {
    if (re.test(localName(all[i]))) out.push(all[i]);
  }
  return out;
}

function shouldRename(name: string | null | undefined, aliases: Set<string>): boolean {
  const n = (name || '').trim().toLowerCase();
  return !n || aliases.has(n);
}

function taskLabel(el: Element): string {
  const id = el.getAttribute('id') || '?';
  const name = (el.getAttribute('name') || '').trim();
  return name ? `${name}(${id})` : id;
}

function hasAssigneeType(task: Element, tag: string): boolean {
  const ee = findSfChild(task, ['extensionElements']);
  if (!ee) return false;
  const node = findSfChild(ee, [tag]);
  if (!node) return false;
  return !!getAttr(node, 'type');
}

function hasButtons(task: Element): boolean {
  const ee = findSfChild(task, ['extensionElements']);
  if (!ee) return false;
  const node = findSfChild(ee, ['ButtonsSetting', 'buttonsSetting']);
  if (!node) return false;
  return !!(getAttr(node, 'json') || (node.textContent || '').trim());
}

/** start→end BFS over sequenceFlow edges inside process */
function hasStartEndPath(doc: Document): {
  hasStart: boolean;
  hasEnd: boolean;
  reachable: boolean;
} {
  const processes = collectByLocal(doc, /^process$/i);
  if (!processes.length) {
    return { hasStart: false, hasEnd: false, reachable: false };
  }
  // use first top-level process (skip nested subprocess process tags if any — rare in our XML)
  const process = processes[0];
  const starts: string[] = [];
  const ends = new Set<string>();
  const adj = new Map<string, string[]>();

  for (let i = 0; i < process.children.length; i++) {
    const el = process.children[i];
    const ln = localName(el);
    const id = el.getAttribute('id');
    if (!id) continue;
    if (/^startEvent$/i.test(ln)) starts.push(id);
    if (/^endEvent$/i.test(ln)) ends.add(id);
    if (/^sequenceFlow$/i.test(ln)) {
      const src = el.getAttribute('sourceRef') || '';
      const tgt = el.getAttribute('targetRef') || '';
      if (!src || !tgt) continue;
      const list = adj.get(src) || [];
      list.push(tgt);
      adj.set(src, list);
    }
  }

  const hasStart = starts.length > 0;
  const hasEnd = ends.size > 0;
  if (!hasStart || !hasEnd) {
    return { hasStart, hasEnd, reachable: false };
  }

  const seen = new Set<string>();
  const q = [...starts];
  for (const s of starts) seen.add(s);
  while (q.length) {
    const cur = q.shift()!;
    if (ends.has(cur)) return { hasStart, hasEnd, reachable: true };
    for (const n of adj.get(cur) || []) {
      if (seen.has(n)) continue;
      seen.add(n);
      q.push(n);
    }
  }
  return { hasStart, hasEnd, reachable: false };
}

function parseDoc(xml: string): Document | null {
  if (!xml || typeof xml !== 'string') return null;
  try {
    const doc = new DOMParser().parseFromString(xml, 'application/xml');
    if (doc.getElementsByTagName('parsererror')[0]) return null;
    return doc;
  } catch {
    return null;
  }
}

/**
 * 对 BPMN XML 执行设计修复。dry-run 与正式执行共用此函数（调用方丢弃 xml 即 dry-run）。
 */
export function repairProcessDesign(
  xml: string,
  options: RepairProcessOptions
): RepairProcessResult {
  const applied: RepairProcessApplied = {
    removedFlows: [],
    removedDi: [],
    filledAssignee: [],
    filledButtons: [],
    renamedEvents: []
  };
  const remaining: string[] = [];

  let working = xml || '';
  if (!working) {
    remaining.push('流程图 XML 为空，无法修复');
    return { xml: working, applied, remaining };
  }

  if (options.cleanOrphans) {
    const cleaned = sanitizeBpmnXml(working);
    working = cleaned.xml;
    applied.removedFlows = cleaned.removedFlows;
    applied.removedDi = cleaned.removedDi;
  }

  const doc = parseDoc(working);
  if (!doc) {
    remaining.push('BPMN XML 无法解析');
    return { xml: working, applied, remaining };
  }

  ensureDefinitionsSfNs(doc);

  const tasks = collectByLocal(doc, /^(userTask|task)$/i).filter(el => {
    // plain task/userTask only — exclude serviceTask etc. already by regex
    const ln = localName(el).toLowerCase();
    return ln === 'usertask' || ln === 'task';
  });

  for (const task of tasks) {
    const id = task.getAttribute('id') || '?';
    const needResolve = !hasAssigneeType(task, 'assigneeResolve') && !hasAssigneeType(task, 'AssigneeResolve');
    const needLeave = !hasAssigneeType(task, 'leaveJudge') && !hasAssigneeType(task, 'LeaveJudge');
    const needButtons = !hasButtons(task);

    if (options.fillAssignee && (needResolve || needLeave)) {
      const ee = ensureExtensionElements(task, doc);
      if (needResolve) {
        ensureSfConfig(ee, doc, 'assigneeResolve', {
          type: options.resolveType || DEFAULT_ASSIGNEE_RESOLVE_TYPE,
          params: ''
        });
      }
      if (needLeave) {
        ensureSfConfig(ee, doc, 'leaveJudge', {
          type: options.leaveType || DEFAULT_LEAVE_JUDGE_TYPE,
          params: options.leaveParams ?? DEFAULT_LEAVE_JUDGE_PARAMS
        });
      }
      applied.filledAssignee.push(id);
    }

    if (options.fillButtons && needButtons) {
      const ee = ensureExtensionElements(task, doc);
      ensureSfConfig(ee, doc, 'buttonsSetting', {
        json: JSON.stringify(defaultButtonsSetting())
      });
      applied.filledButtons.push(id);
    }
  }

  if (options.renameEvents) {
    for (const el of collectByLocal(doc, /^startEvent$/i)) {
      const name = el.getAttribute('name');
      if (shouldRename(name, START_ALIASES)) {
        el.setAttribute('name', '开始');
        applied.renamedEvents.push(el.getAttribute('id') || 'start');
      }
    }
    for (const el of collectByLocal(doc, /^endEvent$/i)) {
      const name = el.getAttribute('name');
      if (shouldRename(name, END_ALIASES)) {
        el.setAttribute('name', '结束');
        applied.renamedEvents.push(el.getAttribute('id') || 'end');
      }
    }
  }

  working = new XMLSerializer().serializeToString(doc);

  // remaining issues after planned repairs
  const after = parseDoc(working) || doc;
  const path = hasStartEndPath(after);
  if (!path.hasStart) remaining.push('缺少开始事件（需手动添加）');
  if (!path.hasEnd) remaining.push('缺少结束事件（需手动添加）');
  if (path.hasStart && path.hasEnd && !path.reachable) {
    remaining.push('无连通路径：开始事件无法到达结束事件（需手动调整连线）');
  }

  const tasksAfter = collectByLocal(after, /^(userTask|task)$/i).filter(el => {
    const ln = localName(el).toLowerCase();
    return ln === 'usertask' || ln === 'task';
  });
  for (const task of tasksAfter) {
    const needResolve = !hasAssigneeType(task, 'assigneeResolve') && !hasAssigneeType(task, 'AssigneeResolve');
    const needLeave = !hasAssigneeType(task, 'leaveJudge') && !hasAssigneeType(task, 'LeaveJudge');
    if (needResolve || needLeave) {
      remaining.push(
        `UserTask '${taskLabel(task)}' 缺少待办人规则（AssigneeResolve/LeaveJudge Type）${
          options.fillAssignee ? '' : '（未勾选自动补全）'
        }`
      );
    }
  }

  return { xml: working, applied, remaining };
}

export function summarizeApplied(applied: RepairProcessApplied): string {
  const parts: string[] = [];
  if (applied.removedFlows.length) parts.push(`清理断链 ${applied.removedFlows.length} 条`);
  if (applied.removedDi.length) parts.push(`清理无效图元 ${applied.removedDi.length} 处`);
  if (applied.filledAssignee.length) parts.push(`补全待办人 ${applied.filledAssignee.length} 个节点`);
  if (applied.filledButtons.length) parts.push(`补全按钮设置 ${applied.filledButtons.length} 个节点`);
  if (applied.renamedEvents.length) parts.push(`规范化起止名称 ${applied.renamedEvents.length} 个`);
  return parts.length ? parts.join('；') : '无需变更';
}
