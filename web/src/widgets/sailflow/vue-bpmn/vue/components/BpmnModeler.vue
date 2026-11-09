<script setup lang="ts">
import { computed, onMounted, ref, shallowRef } from 'vue';
import {
  addBendpoint,
  alignElements,
  canConnect,
  createAppendElementCommand,
  createConnectElementsCommand,
  createCopyCommand,
  createCreateElementsCommand,
  createCutCommand,
  createDeleteElementsCommand,
  createDuplicateCommand,
  createEmptyDiagram,
  createLaneCommand,
  createMoveElementsCommand,
  createPasteCommand,
  createReplaceElementCommand,
  createResizeElementCommand,
  createSetNameCommand,
  createSetPropertyCommand,
  createSpaceToolCommand,
  createUpdateWaypointsCommand,
  drilldown,
  exportSvg,
  exportXml,
  findElements,
  fitViewport,
  getContextPadEntries,
  getPaletteEntries,
  importXml,
  lassoSelectIds,
  paletteEntryByKey,
  panViewbox,
  selectAllIds,
  snapAlignAxis,
  spaceFromDrag,
  zoomViewbox,
  type AlignMode,
  type Bounds,
  type DiagramSnapshot,
  type DiagramStore,
  type Point,
} from '@vue-bpmn/core';
import { useDiagramStore } from '../useDiagramStore';
import BpmnCanvas from './BpmnCanvas.vue';
import BpmnContextPad from './BpmnContextPad.vue';
import BpmnDirectEditing from './BpmnDirectEditing.vue';
import BpmnPalette from './BpmnPalette.vue';
import BpmnPropertiesPanel from './BpmnPropertiesPanel.vue';

const props = withDefaults(
  defineProps<{
    store?: DiagramStore;
    xml?: string;
    readonly?: boolean;
  }>(),
  { readonly: false },
);

const emit = defineEmits<{
  'selection.changed': [ids: string[]];
  'commandStack.changed': [];
  'import.done': [];
  'import.error': [error: Error];
}>();

const { store, state } = useDiagramStore(props.store);

store.commandStack.onChange(() => emit('commandStack.changed'));
store.subscribe((s) => emit('selection.changed', s.selection));

const dragIds = ref<string[]>([]);
/** Must be shallowRef: deep ref() wraps snapshots in Proxies that structuredClone cannot clone (breaks drag). */
const dragBase = shallowRef<DiagramSnapshot | null>(null);
const pendingCreate = ref<string | null>(null);
const connectSourceId = ref<string | null>(null);
const connectPreviewTo = ref<Point | null>(null);
const resizeBase = shallowRef<DiagramSnapshot | null>(null);
const bendpointBase = shallowRef<DiagramSnapshot | null>(null);

function cloneSnapshot(snap: DiagramSnapshot): DiagramSnapshot {
  return structuredClone(snap);
}
const replaceOpen = ref(false);
const editingId = ref<string | null>(null);
const searchOpen = ref(false);
const searchQuery = ref('');
const overlays = ref<Record<string, { html: string; position: { top?: number; right?: number; bottom?: number; left?: number } }>>({});
const canvasRef = ref<{ worldToHost: (x: number, y: number) => Point } | null>(null);

function mapPoint(x: number, y: number): Point {
  return canvasRef.value?.worldToHost(x, y) ?? { x, y };
}

const editingElement = computed(() =>
  editingId.value ? (store.getState().elements[editingId.value] ?? null) : null,
);

/** Bottom status: why element drag is blocked (or ready). */
const canvasStatus = computed(() => {
  if (props.readonly) {
    return {
      level: 'warn' as const,
      text: '只读模式：无法拖动元素',
      hint: '',
    };
  }
  if (editingId.value) {
    return {
      level: 'info' as const,
      text: '正在编辑标签：Enter 确认 / Esc 取消',
      hint: '',
    };
  }
  if (pendingCreate.value) {
    const createLabel =
      getPaletteEntries().find((e) => e.type === pendingCreate.value)?.label ??
      pendingCreate.value.replace(/^bpmn:/, '');
    return {
      level: 'warn' as const,
      text: `创建模式（${createLabel}）：点击画布空白处放置，无法拖动已有元素`,
      hint: 'Esc 或再点一次工具退出',
    };
  }
  if (connectSourceId.value || state.value.activeTool === 'global-connect') {
    return {
      level: 'warn' as const,
      text: connectSourceId.value
        ? `连线中：已选 ${connectSourceId.value}，再点目标元素`
        : '连线工具：点击源元素，再点目标（无法拖动）',
      hint: 'Esc 取消',
    };
  }
  const tool = state.value.activeTool;
  if (tool === 'hand') {
    return {
      level: 'warn' as const,
      text: '抓手工具：拖动画布平移，无法拖动元素',
      hint: '再点「抓手」或 Esc 退出',
    };
  }
  if (tool === 'lasso') {
    return {
      level: 'warn' as const,
      text: '套索工具：拖动框选，无法拖动元素',
      hint: '再点「套索」或 Esc 退出',
    };
  }
  if (tool === 'space') {
    return {
      level: 'warn' as const,
      text: '间距工具：拖动插入间距，无法拖动元素',
      hint: '再点「间距」或 Esc 退出',
    };
  }
  const sel = state.value.selection.length;
  const toolNames: Record<string, string> = {
    hand: '抓手',
    lasso: '套索',
    space: '间距',
    'global-connect': '连线',
  };
  return {
    level: 'ok' as const,
    text:
      sel === 0
        ? '可拖动：在元素上按住拖移 · 点击选中 · Delete 删除'
        : sel === 1
          ? '已选 1 个：可直接拖动 · 右侧为上下文菜单'
          : `已选 ${sel} 个：可一起拖动`,
    hint: tool ? `当前工具：${toolNames[tool] ?? tool}` : '',
  };
});

function commandCtx() {
  return {
    store: {
      getState: () => store.getState(),
      replaceState: (n: DiagramSnapshot) => store.replaceState(n),
      patchState: (p: Partial<DiagramSnapshot>) => store.patchState(p),
    },
  };
}

async function importXML(xml: string) {
  try {
    const { result, elements } = importXml(xml);
    store.loadElements({
      rootId: result.rootId,
      definitionsId: result.definitionsId,
      definitionsAttrs: result.definitionsAttrs,
      elements,
    });
    pendingCreate.value = null;
    connectSourceId.value = null;
    replaceOpen.value = false;
    editingId.value = null;
    emit('import.done');
    return { warnings: result.warnings };
  } catch (err) {
    const error = err instanceof Error ? err : new Error(String(err));
    emit('import.error', error);
    throw error;
  }
}

async function exportXML() {
  return exportXml(store.getState());
}

async function exportSVG() {
  return exportSvg(store.getState());
}

function createDiagram() {
  store.commandStack.clear();
  store.replaceState(createEmptyDiagram());
  pendingCreate.value = null;
  connectSourceId.value = null;
  replaceOpen.value = false;
  editingId.value = null;
}

function select(id: string, additive: boolean) {
  if (props.readonly) return;
  replaceOpen.value = false;
  editingId.value = null;
  if (additive) {
    const cur = store.getState().selection;
    store.setSelection(cur.includes(id) ? cur.filter((x) => x !== id) : [...cur, id]);
  } else {
    store.setSelection([id]);
  }
}

function onTool(tool: string) {
  if (props.readonly) return;
  pendingCreate.value = null;
  connectSourceId.value = null;
  connectPreviewTo.value = null;
  replaceOpen.value = false;
  store.setActiveTool(store.getState().activeTool === tool ? null : tool);
}

function onCreate(type: string) {
  if (props.readonly) return;
  store.setActiveTool(null);
  connectSourceId.value = null;
  replaceOpen.value = false;
  pendingCreate.value = pendingCreate.value === type ? null : type;
}

function createAt(typeOrKey: string, position: Point) {
  if (props.readonly) return null;
  const entry = paletteEntryByKey(typeOrKey);
  const type = entry?.type ?? typeOrKey;
  store.commandStack.execute(createCreateElementsCommand({ type, position }), commandCtx());
  pendingCreate.value = null;
  return store.getState().selection[0] ?? null;
}

function connect(sourceId: string, targetId: string) {
  if (props.readonly) return { ok: false as const, reason: 'readonly' };
  const decision = canConnect(store.getState(), sourceId, targetId);
  if (!decision.allowed) return { ok: false as const, reason: decision.reason ?? 'rejected' };
  store.commandStack.execute(
    createConnectElementsCommand({ sourceId, targetId }),
    commandCtx(),
  );
  return { ok: true as const, type: decision.type };
}

function onConnectPick(id: string) {
  if (props.readonly) return;
  if (!connectSourceId.value) {
    connectSourceId.value = id;
    store.setSelection([id]);
    return;
  }
  if (connectSourceId.value === id) return;
  connect(connectSourceId.value, id);
  connectSourceId.value = null;
  connectPreviewTo.value = null;
  store.setActiveTool(null);
}

function onConnectMove(position: Point) {
  connectPreviewTo.value = position;
}

function onCanvasClick(position: Point) {
  if (props.readonly || !pendingCreate.value) return;
  createAt(pendingCreate.value, position);
}

function onLassoSelect(rect: Bounds) {
  if (props.readonly) return;
  store.setSelection(lassoSelectIds(store.getState(), rect));
}

function removeSelection() {
  if (props.readonly) return;
  const ids = store.getState().selection;
  if (!ids.length) return;
  store.commandStack.execute(createDeleteElementsCommand({ ids }), commandCtx());
  replaceOpen.value = false;
}

function selectAll() {
  if (props.readonly) return;
  store.setSelection(selectAllIds(store.getState()));
}

function startDirectEditing(id?: string) {
  if (props.readonly) return;
  const target = id ?? store.getState().selection[0];
  if (!target) return;
  store.setSelection([target]);
  editingId.value = target;
}

function commitDirectEditing(text: string) {
  const id = editingId.value;
  editingId.value = null;
  if (!id) return;
  store.commandStack.execute(createSetNameCommand({ id, name: text }), commandCtx());
}

function onKeyAction(
  action:
    | 'delete'
    | 'selectAll'
    | 'escape'
    | 'edit'
    | 'copy'
    | 'cut'
    | 'paste'
    | 'duplicate'
    | 'undo'
    | 'redo'
    | 'replace'
    | 'find'
    | 'tool',
) {
  if (props.readonly && action !== 'escape' && action !== 'selectAll') return;
  if (action === 'delete') removeSelection();
  if (action === 'selectAll') selectAll();
  if (action === 'edit') startDirectEditing();
  if (action === 'copy') store.commandStack.execute(createCopyCommand(), commandCtx());
  if (action === 'cut') store.commandStack.execute(createCutCommand(), commandCtx());
  if (action === 'paste') store.commandStack.execute(createPasteCommand(), commandCtx());
  if (action === 'duplicate') store.commandStack.execute(createDuplicateCommand(), commandCtx());
  if (action === 'undo') store.commandStack.undo();
  if (action === 'redo') store.commandStack.redo();
  if (action === 'replace') replaceOpen.value = true;
  if (action === 'find') searchOpen.value = true;
  if (action === 'escape') {
    pendingCreate.value = null;
    connectSourceId.value = null;
    connectPreviewTo.value = null;
    replaceOpen.value = false;
    searchOpen.value = false;
    editingId.value = null;
    store.setActiveTool(null);
    store.clearSelection();
  }
}

function onToolShortcut(tool: string) {
  if (props.readonly) return;
  onTool(tool);
}

function onPan(delta: Point) {
  store.replaceState(panViewbox(store.getState(), delta));
}

function onSpaceDrag(from: Point, to: Point) {
  if (props.readonly) return;
  const payload = spaceFromDrag(from, to);
  if (payload.amount === 0) return;
  store.commandStack.execute(createSpaceToolCommand(payload), commandCtx());
}

function zoom(delta: number) {
  store.replaceState(zoomViewbox(store.getState(), delta));
}

function zoomFit() {
  store.replaceState(fitViewport(store.getState()));
}

function alignSelection(mode: AlignMode) {
  if (props.readonly) return;
  const ids = store.getState().selection;
  store.replaceState(alignElements(store.getState(), ids, mode));
}

function searchAndSelect(query: string) {
  const hits = findElements(store.getState(), query);
  if (hits[0]) store.setSelection([hits[0]]);
  searchOpen.value = false;
  searchQuery.value = '';
  return hits;
}

function addOverlay(elementId: string, html: string, position: { top?: number; right?: number; bottom?: number; left?: number }) {
  overlays.value = { ...overlays.value, [elementId]: { html, position } };
}

function openDrilldown(id: string) {
  const next = drilldown(store.getState(), id);
  if (next) store.replaceState(next);
}

function overlayStyle(
  elementId: string,
  position: { top?: number; right?: number; bottom?: number; left?: number },
) {
  const el = store.getState().elements[elementId];
  if (!el) return { display: 'none' };
  const worldX =
    position.right !== undefined
      ? el.di.x + el.di.width - (position.right ?? 0)
      : el.di.x + (position.left ?? 0);
  const worldY = el.di.y + (position.top ?? 0);
  const host = mapPoint(worldX, worldY);
  return {
    position: 'absolute' as const,
    top: `${host.y}px`,
    left: `${host.x}px`,
    pointerEvents: 'none' as const,
  };
}

function onContextPadAction(key: string, type?: string) {
  if (props.readonly) return;
  const id = store.getState().selection[0];
  if (!id) return;
  if (key === 'delete') {
    removeSelection();
    return;
  }
  if (key === 'connect') {
    store.setActiveTool('global-connect');
    connectSourceId.value = id;
    return;
  }
  if (key.startsWith('append.') && type) {
    store.commandStack.execute(
      createAppendElementCommand({ sourceId: id, type }),
      commandCtx(),
    );
    replaceOpen.value = false;
    return;
  }
  if (key.startsWith('lane-')) {
    store.commandStack.execute(
      createLaneCommand({ laneId: id, action: key as 'lane-insert-above' | 'lane-insert-below' | 'lane-divide-two' | 'lane-divide-three' }),
      commandCtx(),
    );
  }
}

function onReplace(target: string) {
  if (props.readonly) return;
  const id = store.getState().selection[0];
  if (!id) return;
  store.commandStack.execute(createReplaceElementCommand({ id, target }), commandCtx());
  replaceOpen.value = false;
}

function updateName(id: string, name: string) {
  if (props.readonly) return;
  store.commandStack.execute(createSetNameCommand({ id, name }), commandCtx());
}

function updateProperty(id: string, key: string, value: unknown) {
  if (props.readonly) return;
  store.commandStack.execute(createSetPropertyCommand({ id, key, value }), commandCtx());
}

function updateId(id: string, nextIdValue: string) {
  if (props.readonly || !nextIdValue || nextIdValue === id) return;
  const stateNow = store.getState();
  const el = stateNow.elements[id];
  if (!el || stateNow.elements[nextIdValue]) return;
  const elements = { ...stateNow.elements };
  delete elements[id];
  elements[nextIdValue] = { ...el, id: nextIdValue };
  for (const other of Object.values(elements)) {
    other.incoming = other.incoming.map((x) => (x === id ? nextIdValue : x));
    other.outgoing = other.outgoing.map((x) => (x === id ? nextIdValue : x));
    if (other.parentId === id) other.parentId = nextIdValue;
    if (other.hostId === id) other.hostId = nextIdValue;
    if (other.businessObject.sourceRef === id) other.businessObject.sourceRef = nextIdValue;
    if (other.businessObject.targetRef === id) other.businessObject.targetRef = nextIdValue;
  }
  store.patchState({
    elements,
    selection: stateNow.selection.map((x) => (x === id ? nextIdValue : x)),
  });
}

function nudgeSelected(dx: number, dy: number) {
  if (props.readonly) return;
  const ids = store.getState().selection;
  if (!ids.length) return;
  store.commandStack.execute(createMoveElementsCommand({ ids, delta: { x: dx, y: dy } }), commandCtx());
}

function onDragStart(ids: string[]) {
  if (props.readonly) return;
  dragIds.value = ids;
  dragBase.value = store.snapshot();
  replaceOpen.value = false;
  const cur = store.getState().selection;
  const sameSet =
    cur.length === ids.length && ids.every((id) => cur.includes(id));
  if (!sameSet) {
    store.setSelection(ids);
  }
}

function onDragMove(delta: Point) {
  if (props.readonly || !dragBase.value || !dragIds.value.length) return;
  // Restore from a fresh clone of the baseline so preview never mutates dragBase.
  store.replaceState(cloneSnapshot(dragBase.value));
  createMoveElementsCommand({ ids: dragIds.value, delta }).execute(commandCtx());
}

function onDragEnd(delta: Point) {
  if (props.readonly || !dragBase.value || !dragIds.value.length) {
    dragIds.value = [];
    dragBase.value = null;
    return;
  }
  store.replaceState(cloneSnapshot(dragBase.value));
  if (Math.abs(delta.x) > 0.5 || Math.abs(delta.y) > 0.5) {
    store.commandStack.execute(
      createMoveElementsCommand({ ids: dragIds.value, delta }),
      commandCtx(),
    );
  }
  dragIds.value = [];
  dragBase.value = null;
}

function onBendpointMove(connectionId: string, index: number, point: Point) {
  if (!bendpointBase.value) bendpointBase.value = store.snapshot();
  const base = bendpointBase.value;
  const el = base.elements[connectionId];
  if (!el?.waypoints) return;
  const waypoints = el.waypoints.map((p, i) => (i === index ? { ...point } : { ...p }));
  store.replaceState(cloneSnapshot(base));
  createUpdateWaypointsCommand({ connectionId, waypoints }).execute(commandCtx());
}

function onBendpointCommit(connectionId: string, index: number, point: Point) {
  if (!bendpointBase.value) return;
  const base = bendpointBase.value;
  const el = base.elements[connectionId];
  if (!el?.waypoints) {
    bendpointBase.value = null;
    return;
  }
  const waypoints = el.waypoints.map((p, i) => (i === index ? { ...point } : { ...p }));
  store.replaceState(cloneSnapshot(base));
  store.commandStack.execute(
    createUpdateWaypointsCommand({ connectionId, waypoints }),
    commandCtx(),
  );
  bendpointBase.value = null;
}

function onBendpointAdd(connectionId: string, point: Point) {
  if (props.readonly) return;
  const el = store.getElement(connectionId);
  if (!el?.waypoints) return;
  const { waypoints } = addBendpoint(el.waypoints, point);
  store.commandStack.execute(
    createUpdateWaypointsCommand({ connectionId, waypoints }),
    commandCtx(),
  );
  store.setSelection([connectionId]);
}

function onResizeMove(id: string, bounds: Bounds) {
  if (!resizeBase.value) resizeBase.value = store.snapshot();
  store.replaceState(cloneSnapshot(resizeBase.value));
  createResizeElementCommand({ id, bounds }).execute(commandCtx());
}

function onResizeCommit(id: string, bounds: Bounds) {
  if (!resizeBase.value) return;
  store.replaceState(cloneSnapshot(resizeBase.value));
  store.commandStack.execute(createResizeElementCommand({ id, bounds }), commandCtx());
  resizeBase.value = null;
}

function appendTask(sourceId: string) {
  if (props.readonly) return;
  store.commandStack.execute(
    createAppendElementCommand({ sourceId, type: 'bpmn:Task' }),
    commandCtx(),
  );
}

function replaceElement(id: string, target: string) {
  if (props.readonly) return;
  store.commandStack.execute(createReplaceElementCommand({ id, target }), commandCtx());
}

onMounted(async () => {
  if (props.xml) {
    await importXML(props.xml);
  }
});

defineExpose({
  importXML,
  exportXML,
  exportSVG,
  createDiagram,
  createAt,
  connect,
  canConnect: (sourceId: string, targetId: string) =>
    canConnect(store.getState(), sourceId, targetId),
  appendTask,
  replaceElement,
  copy: () => store.commandStack.execute(createCopyCommand(), commandCtx()),
  cut: () => store.commandStack.execute(createCutCommand(), commandCtx()),
  paste: () => store.commandStack.execute(createPasteCommand(), commandCtx()),
  duplicate: () => store.commandStack.execute(createDuplicateCommand(), commandCtx()),
  getContextPadEntries: (id?: string) => getContextPadEntries(store.getState(), id),
  startDirectEditing,
  removeSelection,
  selectAll,
  getPaletteEntries,
  getStore: () => store,
  undo: () => store.commandStack.undo(),
  redo: () => store.commandStack.redo(),
  nudgeSelected,
  zoom,
  zoomFit,
  alignElements: alignSelection,
  find: searchAndSelect,
  addOverlay,
  drilldown: openDrilldown,
  snapAlign: (movingId: string, alignWithId: string, axis: 'x' | 'y') => {
    const delta = snapAlignAxis(store.getState(), movingId, alignWithId, axis);
    store.commandStack.execute(
      createMoveElementsCommand({ ids: [movingId], delta }),
      commandCtx(),
    );
  },
});
</script>

<template>
  <div class="vb-modeler" :class="{ 'is-readonly': readonly }">
    <BpmnPalette
      v-if="!readonly"
      :active-tool="state.activeTool"
      :pending-create="pendingCreate"
      @tool="onTool"
      @create="onCreate"
    />
    <div class="vb-canvas-host">
      <BpmnCanvas
        ref="canvasRef"
        :state="state"
        :pending-create="pendingCreate"
        :connect-source-id="connectSourceId"
        :connect-preview-to="connectPreviewTo"
        @select="select"
        @clear-selection="store.clearSelection(); replaceOpen = false"
        @drag-start="onDragStart"
        @drag-move="onDragMove"
        @drag-end="onDragEnd"
        @canvas-click="onCanvasClick"
        @lasso-select="onLassoSelect"
        @key-action="onKeyAction"
        @tool-shortcut="onToolShortcut"
        @pan="onPan"
        @space-drag="onSpaceDrag"
        @double-click-element="startDirectEditing"
        @connect-pick="onConnectPick"
        @connect-move="onConnectMove"
        @bendpoint-move="onBendpointMove"
        @bendpoint-commit="onBendpointCommit"
        @bendpoint-add="onBendpointAdd"
        @resize-move="onResizeMove"
        @resize-commit="onResizeCommit"
      />
      <BpmnContextPad
        v-if="!readonly && dragIds.length === 0"
        :state="state"
        :replace-open="replaceOpen"
        :map-point="mapPoint"
        @action="onContextPadAction"
        @replace="onReplace"
        @toggle-replace="replaceOpen = !replaceOpen"
      />
      <BpmnDirectEditing
        :element="editingElement"
        :active="!!editingId"
        :map-point="mapPoint"
        @commit="commitDirectEditing"
        @cancel="editingId = null"
      />
      <div v-if="searchOpen" class="vb-search-pad">
        <input
          v-model="searchQuery"
          placeholder="查找元素…"
          @keydown.enter="searchAndSelect(searchQuery)"
          @keydown.escape="searchOpen = false"
        />
      </div>
      <div
        v-for="(ov, eid) in overlays"
        :key="eid"
        class="vb-overlay"
        :style="overlayStyle(eid, ov.position)"
        v-html="ov.html"
      />
      <footer class="vb-status-bar" :data-level="canvasStatus.level" aria-live="polite">
        <span class="vb-status-main">{{ canvasStatus.text }}</span>
        <span v-if="canvasStatus.hint" class="vb-status-hint">{{ canvasStatus.hint }}</span>
      </footer>
    </div>
    <BpmnPropertiesPanel
      v-if="!readonly"
      :state="state"
      @update-name="updateName"
      @update-id="updateId"
      @update-property="updateProperty"
    />
  </div>
</template>
