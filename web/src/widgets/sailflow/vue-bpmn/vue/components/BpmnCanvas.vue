<script setup lang="ts">
import { computed, onUnmounted, ref } from 'vue';
import type { Bounds, DiagramElement, DiagramSnapshot, Point } from '@vue-bpmn/core';
import { eventMarker, isConnection, isNonInterrupting } from '@vue-bpmn/core';

const props = defineProps<{
  state: DiagramSnapshot;
  pendingCreate?: string | null;
  connectSourceId?: string | null;
  connectPreviewTo?: Point | null;
}>();

const emit = defineEmits<{
  select: [id: string, additive: boolean];
  clearSelection: [];
  dragStart: [ids: string[]];
  dragMove: [delta: Point];
  dragEnd: [delta: Point];
  canvasClick: [position: Point];
  lassoSelect: [rect: Bounds];
  keyAction: [action: 'delete' | 'selectAll' | 'escape' | 'edit' | 'copy' | 'cut' | 'paste' | 'duplicate' | 'undo' | 'redo' | 'replace' | 'find' | 'tool'];
  toolShortcut: [tool: string];
  pan: [delta: Point];
  spaceDrag: [from: Point, to: Point];
  doubleClickElement: [id: string];
  connectPick: [id: string];
  connectMove: [position: Point];
  bendpointMove: [connectionId: string, index: number, point: Point];
  bendpointCommit: [connectionId: string, index: number, point: Point];
  bendpointAdd: [connectionId: string, point: Point];
  resizeMove: [id: string, bounds: Bounds];
  resizeCommit: [id: string, bounds: Bounds];
}>();

const shapes = computed(() =>
  Object.values(props.state.elements).filter((el) => !isConnection(el) && el.di.width > 0),
);

const edges = computed(() =>
  Object.values(props.state.elements).filter(
    (el) => isConnection(el) && (el.waypoints?.length ?? 0) > 0,
  ),
);

const selectedConnection = computed(() => {
  if (props.state.selection.length !== 1) return null;
  const el = props.state.elements[props.state.selection[0]!];
  return el && isConnection(el) ? el : null;
});

const selectedResizable = computed(() => {
  if (props.state.selection.length !== 1) return null;
  const el = props.state.elements[props.state.selection[0]!];
  if (!el || isConnection(el)) return null;
  if (
    el.type.includes('SubProcess') ||
    el.type === 'bpmn:Participant' ||
    el.type.includes('Task') ||
    el.type === 'bpmn:Group'
  ) {
    return el;
  }
  return null;
});

const viewBox = computed(() => {
  const v = props.state.viewbox;
  return `${v.x} ${v.y} ${v.width / v.scale} ${v.height / v.scale}`;
});

const svgRef = ref<SVGSVGElement | null>(null);
const dragging = ref(false);
const dragOrigin = ref<Point | null>(null);
const lastDelta = ref<Point>({ x: 0, y: 0 });

const lassoing = ref(false);
const lassoFrom = ref<Point | null>(null);
const lassoTo = ref<Point | null>(null);
const panning = ref(false);
const panOrigin = ref<Point | null>(null);
const spacing = ref(false);
const spaceFrom = ref<Point | null>(null);
const spaceTo = ref<Point | null>(null);

const bendpointDrag = ref<{ id: string; index: number } | null>(null);
const resizeDrag = ref<{ id: string; origin: Point; start: Bounds } | null>(null);

const lassoRect = computed(() => {
  if (!lassoFrom.value || !lassoTo.value) return null;
  const a = lassoFrom.value;
  const b = lassoTo.value;
  return {
    x: Math.min(a.x, b.x),
    y: Math.min(a.y, b.y),
    width: Math.abs(b.x - a.x),
    height: Math.abs(b.y - a.y),
  };
});

const connectPreviewPath = computed(() => {
  if (!props.connectSourceId || !props.connectPreviewTo) return '';
  const src = props.state.elements[props.connectSourceId];
  if (!src) return '';
  const x1 = src.di.x + src.di.width / 2;
  const y1 = src.di.y + src.di.height / 2;
  return `M ${x1} ${y1} L ${props.connectPreviewTo.x} ${props.connectPreviewTo.y}`;
});

function isSelected(id: string) {
  return props.state.selection.includes(id);
}

function edgeClass(el: DiagramElement): string {
  if (el.type.includes('MessageFlow')) return 'is-message';
  if (el.type.includes('Association')) return 'is-association';
  return 'is-sequence';
}

/** sequence / message 用箭头；association 开口；选中换色 marker */
function edgeMarker(el: DiagramElement): string {
  const selected = isSelected(el.id);
  if (el.type.includes('MessageFlow')) {
    return selected ? 'url(#vb-arrow-message-selected)' : 'url(#vb-arrow-message)';
  }
  if (el.type.includes('Association')) {
    return 'url(#vb-arrow-association)';
  }
  return selected ? 'url(#vb-arrow-sequence-selected)' : 'url(#vb-arrow-sequence)';
}

function clientToSvg(event: MouseEvent): Point {
  const svg = svgRef.value;
  if (!svg) return { x: event.clientX, y: event.clientY };
  const pt = svg.createSVGPoint();
  pt.x = event.clientX;
  pt.y = event.clientY;
  const ctm = svg.getScreenCTM();
  if (!ctm) return { x: event.clientX, y: event.clientY };
  const sp = pt.matrixTransform(ctm.inverse());
  return { x: sp.x, y: sp.y };
}

function detachWindowDrag() {
  window.removeEventListener('pointermove', onWindowDragMove);
  window.removeEventListener('pointerup', onWindowDragUp);
  window.removeEventListener('pointercancel', onWindowDragUp);
}

function onWindowDragMove(event: PointerEvent) {
  if (!dragging.value || !dragOrigin.value) return;
  const pos = clientToSvg(event);
  const delta = { x: pos.x - dragOrigin.value.x, y: pos.y - dragOrigin.value.y };
  lastDelta.value = delta;
  emit('dragMove', delta);
}

function onWindowDragUp() {
  detachWindowDrag();
  if (!dragging.value) return;
  emit('dragEnd', lastDelta.value);
  dragging.value = false;
  dragOrigin.value = null;
  lastDelta.value = { x: 0, y: 0 };
}

function onShapePointerDown(el: DiagramElement, event: PointerEvent) {
  if (event.button !== 0) return;
  if (props.pendingCreate || props.state.activeTool === 'lasso') return;
  if (props.state.activeTool === 'hand' || props.state.activeTool === 'space') return;
  if (props.state.activeTool === 'global-connect' || props.connectSourceId) {
    emit('connectPick', el.id);
    return;
  }
  event.preventDefault();
  const additive = event.ctrlKey || event.metaKey;
  if (!isSelected(el.id) || additive) {
    emit('select', el.id, additive);
  }
  const ids = additive
    ? isSelected(el.id)
      ? props.state.selection.filter((id) => id !== el.id)
      : [...props.state.selection, el.id]
    : isSelected(el.id)
      ? [...props.state.selection]
      : [el.id];
  const dragIds = ids.includes(el.id) ? ids : [el.id];
  dragging.value = true;
  dragOrigin.value = clientToSvg(event);
  lastDelta.value = { x: 0, y: 0 };
  emit('dragStart', dragIds.length ? dragIds : [el.id]);
  // Listen on window so Vue re-renders / ContextPad under cursor cannot cancel the drag.
  detachWindowDrag();
  window.addEventListener('pointermove', onWindowDragMove);
  window.addEventListener('pointerup', onWindowDragUp);
  window.addEventListener('pointercancel', onWindowDragUp);
}

onUnmounted(() => detachWindowDrag());

function onSvgPointerDown(event: PointerEvent) {
  if (event.button !== 0) return;
  const pos = clientToSvg(event);
  if (props.pendingCreate) {
    emit('canvasClick', pos);
    return;
  }
  if (props.state.activeTool === 'lasso') {
    lassoing.value = true;
    lassoFrom.value = pos;
    lassoTo.value = pos;
    (event.currentTarget as Element).setPointerCapture?.(event.pointerId);
    return;
  }
  if (props.state.activeTool === 'hand') {
    panning.value = true;
    panOrigin.value = pos;
    (event.currentTarget as Element).setPointerCapture?.(event.pointerId);
    return;
  }
  if (props.state.activeTool === 'space') {
    spacing.value = true;
    spaceFrom.value = pos;
    spaceTo.value = pos;
    (event.currentTarget as Element).setPointerCapture?.(event.pointerId);
    return;
  }
  if (event.target === svgRef.value) {
    emit('clearSelection');
  }
}

function onPointerMove(event: PointerEvent) {
  const pos = clientToSvg(event);
  if (props.connectSourceId) {
    emit('connectMove', pos);
  }
  if (bendpointDrag.value) {
    emit('bendpointMove', bendpointDrag.value.id, bendpointDrag.value.index, pos);
    return;
  }
  if (resizeDrag.value) {
    const { id, origin, start } = resizeDrag.value;
    const bounds = {
      x: start.x,
      y: start.y,
      width: Math.max(20, start.width + (pos.x - origin.x)),
      height: Math.max(20, start.height + (pos.y - origin.y)),
    };
    emit('resizeMove', id, bounds);
    return;
  }
  if (lassoing.value && lassoFrom.value) {
    lassoTo.value = pos;
    return;
  }
  if (panning.value && panOrigin.value) {
    emit('pan', { x: pos.x - panOrigin.value.x, y: pos.y - panOrigin.value.y });
    panOrigin.value = pos;
    return;
  }
  if (spacing.value && spaceFrom.value) {
    spaceTo.value = pos;
    return;
  }
}

function onPointerUp() {
  if (bendpointDrag.value) {
    const el = props.state.elements[bendpointDrag.value.id];
    const wp = el?.waypoints?.[bendpointDrag.value.index];
    if (wp) emit('bendpointCommit', bendpointDrag.value.id, bendpointDrag.value.index, wp);
    bendpointDrag.value = null;
    return;
  }
  if (resizeDrag.value) {
    const el = props.state.elements[resizeDrag.value.id];
    if (el) {
      emit('resizeCommit', el.id, {
        x: el.di.x,
        y: el.di.y,
        width: el.di.width,
        height: el.di.height,
      });
    }
    resizeDrag.value = null;
    return;
  }
  if (lassoing.value && lassoRect.value) {
    emit('lassoSelect', lassoRect.value);
    lassoing.value = false;
    lassoFrom.value = null;
    lassoTo.value = null;
    return;
  }
  if (panning.value) {
    panning.value = false;
    panOrigin.value = null;
    return;
  }
  if (spacing.value && spaceFrom.value && spaceTo.value) {
    emit('spaceDrag', spaceFrom.value, spaceTo.value);
    spacing.value = false;
    spaceFrom.value = null;
    spaceTo.value = null;
    return;
  }
  // Element drag completes via window listeners (onWindowDragUp).
}

function onKeyDown(event: KeyboardEvent) {
  const target = event.target as HTMLElement | null;
  if (target && ['INPUT', 'TEXTAREA'].includes(target.tagName)) return;
  const mod = event.ctrlKey || event.metaKey;
  const key = event.key.toLowerCase();
  if (event.key === 'Delete' || event.key === 'Backspace') {
    event.preventDefault();
    emit('keyAction', 'delete');
  } else if (mod && key === 'a') {
    event.preventDefault();
    emit('keyAction', 'selectAll');
  } else if (mod && key === 'c') {
    event.preventDefault();
    emit('keyAction', 'copy');
  } else if (mod && key === 'x') {
    event.preventDefault();
    emit('keyAction', 'cut');
  } else if (mod && key === 'v') {
    event.preventDefault();
    emit('keyAction', 'paste');
  } else if (mod && key === 'd') {
    event.preventDefault();
    emit('keyAction', 'duplicate');
  } else if (mod && key === 'z') {
    event.preventDefault();
    emit('keyAction', event.shiftKey ? 'redo' : 'undo');
  } else if (mod && key === 'y') {
    event.preventDefault();
    emit('keyAction', 'redo');
  } else if (mod && key === 'f') {
    event.preventDefault();
    emit('keyAction', 'find');
  } else if (event.key === 'Escape') {
    emit('keyAction', 'escape');
  } else if (key === 'e' && !mod && !event.altKey) {
    event.preventDefault();
    emit('keyAction', 'edit');
  } else if (key === 'r' && !mod && !event.altKey) {
    event.preventDefault();
    emit('keyAction', 'replace');
  } else if (key === 's' && !mod && !event.altKey) {
    event.preventDefault();
    emit('toolShortcut', 'space');
  } else if (key === 'l' && !mod && !event.altKey) {
    event.preventDefault();
    emit('toolShortcut', 'lasso');
  } else if (key === 'h' && !mod && !event.altKey) {
    event.preventDefault();
    emit('toolShortcut', 'hand');
  } else if (key === 'c' && !mod && !event.altKey) {
    event.preventDefault();
    emit('toolShortcut', 'global-connect');
  }
}

function onEdgeDblClick(el: DiagramElement, event: MouseEvent) {
  emit('bendpointAdd', el.id, clientToSvg(event));
}

function onBendpointDown(connectionId: string, index: number, event: PointerEvent) {
  event.stopPropagation();
  bendpointDrag.value = { id: connectionId, index };
  (event.currentTarget as Element).setPointerCapture?.(event.pointerId);
}

function onResizeDown(el: DiagramElement, event: PointerEvent) {
  event.stopPropagation();
  resizeDrag.value = {
    id: el.id,
    origin: clientToSvg(event),
    start: { x: el.di.x, y: el.di.y, width: el.di.width, height: el.di.height },
  };
  (event.currentTarget as Element).setPointerCapture?.(event.pointerId);
}

function pathFor(el: DiagramElement): string {
  const pts = el.waypoints ?? [];
  if (!pts.length) return '';
  return pts.map((p, i) => `${i === 0 ? 'M' : 'L'} ${p.x} ${p.y}`).join(' ');
}

function shapeKind(el: DiagramElement): 'event' | 'gateway' | 'task' | 'participant' | 'other' {
  if (el.type.includes('Event')) return 'event';
  if (el.type.includes('Gateway')) return 'gateway';
  if (el.type === 'bpmn:Participant') return 'participant';
  if (
    el.type.includes('Task') ||
    el.type.includes('Activity') ||
    el.type.includes('SubProcess') ||
    el.type.includes('Transaction') ||
    el.type === 'bpmn:Group' ||
    el.type === 'bpmn:TextAnnotation'
  ) {
    return 'task';
  }
  return 'other';
}

/** bpmn-js style: start = single ring, intermediate = double ring, end = thick ring. */
function eventVariant(el: DiagramElement): 'start' | 'intermediate' | 'end' {
  if (el.type === 'bpmn:EndEvent') return 'end';
  if (
    el.type === 'bpmn:IntermediateThrowEvent' ||
    el.type === 'bpmn:IntermediateCatchEvent' ||
    el.type === 'bpmn:BoundaryEvent'
  ) {
    return 'intermediate';
  }
  return 'start';
}

function eventCenter(el: DiagramElement): Point {
  return {
    x: el.di.x + el.di.width / 2,
    y: el.di.y + el.di.height / 2,
  };
}

function eventRadius(el: DiagramElement): number {
  return Math.min(el.di.width, el.di.height) / 2;
}

function labelPos(el: DiagramElement): Point {
  if (el.label?.kind === 'external' && el.label.bounds) {
    return {
      x: el.label.bounds.x + el.label.bounds.width / 2,
      y: el.label.bounds.y + el.label.bounds.height / 2,
    };
  }
  return {
    x: el.di.x + (el.di.width || 100) / 2,
    y: el.di.y + (el.di.height || 80) / 2,
  };
}

function markerGlyph(el: DiagramElement): string {
  const m = eventMarker(el);
  const map: Record<string, string> = {
    message: '✉',
    timer: '⏱',
    signal: '△',
    error: '⚡',
    escalation: '⬆',
    link: '🔗',
    conditional: '▤',
    compensate: '⏪',
    terminate: '●',
    cancel: '✕',
  };
  return map[m] ?? '';
}

function gatewayGlyph(el: DiagramElement): string {
  if (el.type.includes('Parallel')) return '+';
  if (el.type.includes('Inclusive')) return 'O';
  if (el.type.includes('EventBased')) return '⚡';
  if (el.type.includes('Complex')) return '*';
  return '×';
}

/** Map diagram (SVG user) coords → canvas-host CSS pixels. */
function worldToHost(x: number, y: number): Point {
  const svg = svgRef.value;
  const host = svg?.parentElement;
  if (!svg || !host) return { x, y };
  const pt = svg.createSVGPoint();
  pt.x = x;
  pt.y = y;
  const ctm = svg.getScreenCTM();
  if (!ctm) return { x, y };
  const screen = pt.matrixTransform(ctm);
  const rect = host.getBoundingClientRect();
  return { x: screen.x - rect.left, y: screen.y - rect.top };
}

/** SVG 客户区像素，供 fitViewport 按真实画布算缩放 */
function getClientSize(): { width: number; height: number } {
  const svg = svgRef.value;
  if (!svg) return { width: 0, height: 0 };
  return {
    width: svg.clientWidth || svg.getBoundingClientRect().width || 0,
    height: svg.clientHeight || svg.getBoundingClientRect().height || 0,
  };
}

defineExpose({ worldToHost, getClientSize, svgRef });
</script>

<template>
  <div
    class="vb-canvas"
    :class="{
      'is-create': !!pendingCreate,
      'is-lasso': state.activeTool === 'lasso',
      'is-connect': state.activeTool === 'global-connect' || !!connectSourceId,
      'is-dragging': dragging,
    }"
    tabindex="0"
    @keydown="onKeyDown"
  >
    <svg
      ref="svgRef"
      class="vb-svg"
      :viewBox="viewBox"
      @pointerdown="onSvgPointerDown"
      @pointermove="onPointerMove"
      @pointerup="onPointerUp"
      @pointercancel="onPointerUp"
    >
      <defs>
        <!-- sequenceFlow 实心三角箭头 -->
        <marker
          id="vb-arrow-sequence"
          viewBox="0 0 12 12"
          refX="10"
          refY="6"
          markerWidth="10"
          markerHeight="10"
          orient="auto"
          markerUnits="userSpaceOnUse"
        >
          <path d="M 0 1.5 L 10 6 L 0 10.5 Z" fill="#334e68" />
        </marker>
        <marker
          id="vb-arrow-sequence-selected"
          viewBox="0 0 12 12"
          refX="10"
          refY="6"
          markerWidth="10"
          markerHeight="10"
          orient="auto"
          markerUnits="userSpaceOnUse"
        >
          <path d="M 0 1.5 L 10 6 L 0 10.5 Z" fill="#0b6bcb" />
        </marker>
        <!-- messageFlow 空心三角 -->
        <marker
          id="vb-arrow-message"
          viewBox="0 0 12 12"
          refX="10"
          refY="6"
          markerWidth="10"
          markerHeight="10"
          orient="auto"
          markerUnits="userSpaceOnUse"
        >
          <path
            d="M 0 1.5 L 10 6 L 0 10.5 Z"
            fill="#fff"
            stroke="#334e68"
            stroke-width="1.2"
          />
        </marker>
        <marker
          id="vb-arrow-message-selected"
          viewBox="0 0 12 12"
          refX="10"
          refY="6"
          markerWidth="10"
          markerHeight="10"
          orient="auto"
          markerUnits="userSpaceOnUse"
        >
          <path
            d="M 0 1.5 L 10 6 L 0 10.5 Z"
            fill="#fff"
            stroke="#0b6bcb"
            stroke-width="1.2"
          />
        </marker>
        <!-- association 开口箭头 -->
        <marker
          id="vb-arrow-association"
          viewBox="0 0 12 12"
          refX="10"
          refY="6"
          markerWidth="10"
          markerHeight="10"
          orient="auto"
          markerUnits="userSpaceOnUse"
        >
          <path
            d="M 1 2 L 10 6 L 1 10"
            fill="none"
            stroke="#627d98"
            stroke-width="1.5"
            stroke-linecap="round"
            stroke-linejoin="round"
          />
        </marker>
      </defs>
      <g class="vb-edges">
        <g
          v-for="el in edges"
          :key="el.id"
          class="vb-edge-group"
          @click.stop="emit('select', el.id, $event.ctrlKey || $event.metaKey)"
          @dblclick.stop="onEdgeDblClick(el, $event)"
        >
          <!-- 宽透明描边扩大可点范围；可见线保持细 -->
          <path class="vb-edge-hit" :d="pathFor(el)" />
          <path
            class="vb-edge"
            :class="[
              edgeClass(el),
              { 'is-selected': isSelected(el.id) },
            ]"
            :d="pathFor(el)"
            :marker-end="edgeMarker(el)"
          />
        </g>
      </g>
      <path
        v-if="connectPreviewPath"
        class="vb-connect-preview"
        :d="connectPreviewPath"
        marker-end="url(#vb-arrow-sequence)"
      />
      <g class="vb-shapes">
        <g
          v-for="el in shapes"
          :key="el.id"
          class="vb-shape"
          :class="[`is-${shapeKind(el)}`, { 'is-selected': isSelected(el.id) }]"
          @pointerdown.stop="onShapePointerDown(el, $event)"
          @dblclick.stop="emit('doubleClickElement', el.id)"
        >
          <template v-if="shapeKind(el) === 'event'">
            <circle
              class="vb-event"
              :class="[
                `is-${eventVariant(el)}`,
                { 'is-non-interrupting': isNonInterrupting(el) },
              ]"
              :cx="eventCenter(el).x"
              :cy="eventCenter(el).y"
              :r="eventRadius(el)"
              :data-marker="eventMarker(el)"
            />
            <circle
              v-if="eventVariant(el) === 'intermediate'"
              class="vb-event vb-event-inner"
              :class="{ 'is-non-interrupting': isNonInterrupting(el) }"
              :cx="eventCenter(el).x"
              :cy="eventCenter(el).y"
              :r="Math.max(4, eventRadius(el) - 3)"
            />
            <text
              v-if="markerGlyph(el)"
              class="vb-marker"
              :x="eventCenter(el).x"
              :y="eventCenter(el).y + 4"
            >
              {{ markerGlyph(el) }}
            </text>
          </template>
          <template v-else-if="shapeKind(el) === 'gateway'">
            <polygon
              :points="[
                `${el.di.x + el.di.width / 2},${el.di.y}`,
                `${el.di.x + el.di.width},${el.di.y + el.di.height / 2}`,
                `${el.di.x + el.di.width / 2},${el.di.y + el.di.height}`,
                `${el.di.x},${el.di.y + el.di.height / 2}`,
              ].join(' ')"
            />
            <text
              class="vb-marker"
              :x="el.di.x + el.di.width / 2"
              :y="el.di.y + el.di.height / 2 + 4"
            >
              {{ gatewayGlyph(el) }}
            </text>
          </template>
          <rect
            v-else
            :x="el.di.x"
            :y="el.di.y"
            :width="el.di.width || 100"
            :height="el.di.height || 80"
            :rx="shapeKind(el) === 'participant' ? 0 : 8"
          />
          <text
            v-if="el.name"
            class="vb-label"
            :x="labelPos(el).x"
            :y="labelPos(el).y"
          >
            {{ el.name }}
          </text>
        </g>
      </g>
      <g v-if="selectedConnection" class="vb-bendpoints">
        <circle
          v-for="(wp, index) in selectedConnection.waypoints"
          :key="`${selectedConnection.id}-${index}`"
          class="vb-bendpoint"
          :cx="wp.x"
          :cy="wp.y"
          r="5"
          @pointerdown="onBendpointDown(selectedConnection.id, index, $event)"
        />
      </g>
      <rect
        v-if="selectedResizable"
        class="vb-resize-handle"
        :x="selectedResizable.di.x + selectedResizable.di.width - 5"
        :y="selectedResizable.di.y + selectedResizable.di.height - 5"
        width="10"
        height="10"
        @pointerdown="onResizeDown(selectedResizable, $event)"
      />
      <rect
        v-if="lassoRect"
        class="vb-lasso"
        :x="lassoRect.x"
        :y="lassoRect.y"
        :width="lassoRect.width"
        :height="lassoRect.height"
      />
    </svg>
  </div>
</template>
