<template>
  <div class="sf-designer-toolbar">
    <el-button-group>
      <el-tooltip content="打开本地 BPMN/XML" placement="bottom">
        <el-button :disabled="!active" @click="fileInput?.click()">
          <el-icon><FolderOpened /></el-icon>
          <span class="sf-designer-toolbar__txt">打开文件</span>
        </el-button>
      </el-tooltip>
      <el-dropdown :disabled="!active" trigger="click" @command="onDownload">
        <el-button :disabled="!active">
          <el-icon><Download /></el-icon>
          <span class="sf-designer-toolbar__txt">下载文件</span>
        </el-button>
        <template #dropdown>
          <el-dropdown-menu>
            <el-dropdown-item command="xml">下载为 XML</el-dropdown-item>
            <el-dropdown-item command="bpmn">下载为 BPMN</el-dropdown-item>
            <el-dropdown-item command="svg">下载为 SVG</el-dropdown-item>
          </el-dropdown-menu>
        </template>
      </el-dropdown>
      <el-dropdown :disabled="!active" trigger="click" @command="onPreview">
        <el-button :disabled="!active">
          <el-icon><View /></el-icon>
          <span class="sf-designer-toolbar__txt">预览</span>
        </el-button>
        <template #dropdown>
          <el-dropdown-menu>
            <el-dropdown-item command="xml">预览 XML</el-dropdown-item>
            <el-dropdown-item command="json">预览 JSON</el-dropdown-item>
          </el-dropdown-menu>
        </template>
      </el-dropdown>
      <el-tooltip :content="simulationOn ? '退出模拟' : '开启模拟'" placement="bottom">
        <el-button
          :disabled="!active || !supportsSimulation"
          :type="simulationOn ? 'warning' : undefined"
          @click="toggleSimulation"
        >
          <el-icon><Cpu /></el-icon>
          <span class="sf-designer-toolbar__txt">模拟</span>
        </el-button>
      </el-tooltip>
    </el-button-group>

    <el-button-group>
      <el-tooltip
        v-for="a in alignActions"
        :key="a.align"
        :content="a.label"
        placement="bottom"
      >
        <el-button :disabled="!active" class="sf-align" :class="'sf-align--' + a.align" @click="align(a.align)">
          <span class="sf-align__icon" aria-hidden="true" />
        </el-button>
      </el-tooltip>
    </el-button-group>

    <el-button-group>
      <el-tooltip content="缩小视图" placement="bottom">
        <el-button :disabled="!active || zoom <= 0.2" @click="zoomOut">
          <el-icon><ZoomOut /></el-icon>
        </el-button>
      </el-tooltip>
      <el-button disabled class="sf-designer-toolbar__zoom">{{ zoomPct }}</el-button>
      <el-tooltip content="放大视图" placement="bottom">
        <el-button :disabled="!active || zoom >= 4" @click="zoomIn">
          <el-icon><ZoomIn /></el-icon>
        </el-button>
      </el-tooltip>
      <el-tooltip content="重置视图并居中" placement="bottom">
        <el-button :disabled="!active" @click="zoomFit">
          <el-icon><FullScreen /></el-icon>
        </el-button>
      </el-tooltip>
    </el-button-group>

    <el-button-group>
      <el-tooltip content="撤销" placement="bottom">
        <el-button :disabled="!active || !canUndo" @click="undo">
          <el-icon><RefreshLeft /></el-icon>
        </el-button>
      </el-tooltip>
      <el-tooltip content="恢复" placement="bottom">
        <el-button :disabled="!active || !canRedo" @click="redo">
          <el-icon><RefreshRight /></el-icon>
        </el-button>
      </el-tooltip>
      <el-tooltip content="重新绘制" placement="bottom">
        <el-button :disabled="!active" @click="restart">
          <el-icon><Refresh /></el-icon>
        </el-button>
      </el-tooltip>
    </el-button-group>

    <input
      ref="fileInput"
      type="file"
      accept=".xml,.bpmn,application/xml,text/xml"
      class="sf-designer-toolbar__file"
      @change="onLocalFile"
    />

    <el-dialog
      v-model="previewVisible"
      :title="previewType === 'json' ? '预览 JSON' : '预览 XML'"
      width="80%"
      top="5vh"
      append-to-body
      destroy-on-close
      class="sf-designer-preview"
    >
      <pre class="sf-designer-preview__code"><code v-html="previewHtml" /></pre>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from 'vue';
import {
  Cpu,
  Download,
  FolderOpened,
  FullScreen,
  Refresh,
  RefreshLeft,
  RefreshRight,
  View,
  ZoomIn,
  ZoomOut
} from '@element-plus/icons-vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import hljs from 'highlight.js/lib/core';
import xmlLang from 'highlight.js/lib/languages/xml';
import jsonLang from 'highlight.js/lib/languages/json';
import 'highlight.js/styles/github.css';
import { parseXmlString } from 'steady-xml';
import type { SailBpmnModeler } from '@sail/bpmn/createModeler';
import { EMPTY_BPMN_XML } from '@sail/bpmn/createModeler';
import { downloadSvg, downloadText } from '@sail/bpmn/utils/download';
import type { ProcessDesignerEngine } from '@sail/designer/engine';

hljs.registerLanguage('xml', xmlLang);
hljs.registerLanguage('json', jsonLang);

defineOptions({ name: 'SailDesignerToolbar' });

const props = defineProps<{
  /** bpmn-js modeler（旧设计器） */
  modeler?: SailBpmnModeler | null;
  /** 统一引擎（Vue 设计器）；优先于 modeler */
  engine?: ProcessDesignerEngine | null;
  fileBaseName?: string;
  /** 导出/预览前清理（如剥离 sf:Variables） */
  prepareExport?: () => void;
}>();

const emit = defineEmits<{
  'import-xml': [xml: string];
}>();

type AlignDir = 'left' | 'right' | 'top' | 'bottom' | 'center' | 'middle';

const alignActions: Array<{ align: AlignDir; label: string }> = [
  { align: 'left', label: '向左对齐' },
  { align: 'center', label: '水平居中' },
  { align: 'right', label: '向右对齐' },
  { align: 'top', label: '向上对齐' },
  { align: 'middle', label: '垂直居中' },
  { align: 'bottom', label: '向下对齐' }
];

const fileInput = ref<HTMLInputElement | null>(null);
const zoom = ref(1);
const canUndo = ref(false);
const canRedo = ref(false);
const simulationOn = ref(false);
const previewVisible = ref(false);
const previewType = ref<'xml' | 'json'>('xml');
const previewRaw = ref('');

const active = computed(() => !!(props.engine || props.modeler));
const supportsSimulation = computed(
  () => props.engine?.supportsSimulation !== false && !!props.modeler && !props.engine
);

const zoomPct = computed(() => `${Math.floor(zoom.value * 100)}%`);
const previewHtml = computed(() => {
  const lang = previewType.value;
  let code = previewRaw.value;
  if (lang === 'json') {
    try {
      code = JSON.stringify(JSON.parse(code), null, 2);
    } catch {
      /* keep */
    }
  }
  return hljs.highlight(code || ' ', { language: lang, ignoreIllegals: true }).value;
});

type Off = { off?: (e: string, fn: (...a: any[]) => void) => void };
let detach: (() => void) | null = null;

function syncStackModeler(m: SailBpmnModeler) {
  try {
    const stack = m.get('commandStack') as { canUndo: () => boolean; canRedo: () => boolean };
    canUndo.value = !!stack.canUndo();
    canRedo.value = !!stack.canRedo();
  } catch {
    canUndo.value = false;
    canRedo.value = false;
  }
}

function syncStackEngine(e: ProcessDesignerEngine) {
  canUndo.value = e.canUndo();
  canRedo.value = e.canRedo();
  zoom.value = e.getZoom();
}

function bindModeler(m: SailBpmnModeler | null) {
  detach?.();
  detach = null;
  simulationOn.value = false;
  zoom.value = 1;
  canUndo.value = false;
  canRedo.value = false;
  if (!m) return;

  const eventBus = m.get('eventBus') as {
    on: (e: string, fn: (...a: any[]) => void) => void;
    off: (e: string, fn: (...a: any[]) => void) => void;
  };

  const onStack = () => syncStackModeler(m);
  const onViewbox = (ev: { viewbox?: { scale?: number } }) => {
    const scale = ev?.viewbox?.scale;
    if (typeof scale === 'number') zoom.value = Math.floor(scale * 100) / 100;
  };

  eventBus.on('commandStack.changed', onStack);
  eventBus.on('canvas.viewbox.changed', onViewbox);
  syncStackModeler(m);

  detach = () => {
    (eventBus as Off).off?.('commandStack.changed', onStack);
    (eventBus as Off).off?.('canvas.viewbox.changed', onViewbox);
  };
}

function bindEngine(e: ProcessDesignerEngine | null) {
  detach?.();
  detach = null;
  simulationOn.value = false;
  zoom.value = 1;
  canUndo.value = false;
  canRedo.value = false;
  if (!e) return;
  syncStackEngine(e);
  detach = e.onChange(() => syncStackEngine(e));
}

watch(
  () => [props.engine, props.modeler] as const,
  ([eng, mod]) => {
    if (eng) bindEngine(eng);
    else bindModeler(mod ?? null);
  },
  { immediate: true }
);

onBeforeUnmount(() => {
  detach?.();
  detach = null;
});

function baseName() {
  return (props.fileBaseName || 'diagram').replace(/[^\w.-]+/g, '_') || 'diagram';
}

async function onLocalFile(ev: Event) {
  const input = ev.target as HTMLInputElement;
  const file = input.files?.[0];
  input.value = '';
  if (!file) return;
  try {
    const xml = await file.text();
    emit('import-xml', xml);
    ElMessage.success(`已导入 ${file.name}`);
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  }
}

async function exportXmlFromActive(): Promise<string> {
  if (props.engine) {
    const { xml } = await props.engine.exportXML();
    return xml || '';
  }
  if (!props.modeler) return '';
  const { xml } = await props.modeler.saveXML({ format: true });
  return xml || '';
}

async function exportSvgFromActive(): Promise<string> {
  if (props.engine) {
    const { svg } = await props.engine.exportSVG();
    return svg || '';
  }
  if (!props.modeler) return '';
  const { svg } = await props.modeler.saveSVG();
  return svg || '';
}

async function onDownload(cmd: string) {
  if (!active.value) return;
  try {
    props.prepareExport?.();
    const name = baseName();
    if (cmd === 'svg') {
      const svg = await exportSvgFromActive();
      downloadSvg(`${name}.svg`, svg);
      ElMessage.success('已下载 SVG');
      return;
    }
    const xml = await exportXmlFromActive();
    const ext = cmd === 'bpmn' ? 'bpmn' : 'xml';
    downloadText(`${name}.${ext}`, xml, 'application/xml');
    ElMessage.success(`已下载 ${ext.toUpperCase()}`);
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  }
}

async function onPreview(cmd: string) {
  if (!active.value) return;
  try {
    props.prepareExport?.();
    const xml = await exportXmlFromActive();
    if (cmd === 'json') {
      const root = parseXmlString(xml || '');
      previewRaw.value = JSON.stringify(root.toJsObject(), null, 2);
      previewType.value = 'json';
    } else {
      previewRaw.value = xml || '';
      previewType.value = 'xml';
    }
    previewVisible.value = true;
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  }
}

function toggleSimulation() {
  if (props.engine) {
    if (!props.engine.supportsSimulation) {
      ElMessage.warning('Vue 设计器暂不支持令牌模拟');
      return;
    }
    const on = props.engine.toggleSimulation?.();
    if (typeof on === 'boolean') simulationOn.value = on;
    return;
  }
  if (!props.modeler) return;
  try {
    const toggle = props.modeler.get('toggleMode', false) as { toggleMode: () => void } | false;
    if (!toggle) {
      ElMessage.warning('当前未加载模拟模块');
      return;
    }
    toggle.toggleMode();
    simulationOn.value = !simulationOn.value;
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  }
}

function align(dir: AlignDir) {
  if (props.engine) {
    ElMessageBox.confirm('自动对齐可能造成图形变形，是否继续？', '警告', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
      .then(() => props.engine!.align(dir))
      .catch(() => undefined);
    return;
  }
  if (!props.modeler) return;
  const Align = props.modeler.get('alignElements') as {
    trigger: (els: unknown[], align: string) => void;
  };
  const selection = props.modeler.get('selection') as { get: () => unknown[] };
  const selected = selection.get() || [];
  if (selected.length <= 1) {
    ElMessage.warning('请按住 Shift 键选择多个元素对齐');
    return;
  }
  ElMessageBox.confirm('自动对齐可能造成图形变形，是否继续？', '警告', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning'
  })
    .then(() => Align.trigger(selected, dir))
    .catch(() => undefined);
}

function zoomIn() {
  if (props.engine) {
    props.engine.zoom(0.1);
    zoom.value = props.engine.getZoom();
    return;
  }
  if (!props.modeler) return;
  const next = Math.min(4, Math.floor(zoom.value * 100 + 10) / 100);
  zoom.value = next;
  (props.modeler.get('canvas') as { zoom: (n: number) => void }).zoom(next);
}

function zoomOut() {
  if (props.engine) {
    props.engine.zoom(-0.1);
    zoom.value = props.engine.getZoom();
    return;
  }
  if (!props.modeler) return;
  const next = Math.max(0.2, Math.floor(zoom.value * 100 - 10) / 100);
  zoom.value = next;
  (props.modeler.get('canvas') as { zoom: (n: number) => void }).zoom(next);
}

function zoomFit() {
  if (props.engine) {
    props.engine.zoom('fit-viewport');
    zoom.value = props.engine.getZoom();
    return;
  }
  if (!props.modeler) return;
  zoom.value = 1;
  (props.modeler.get('canvas') as { zoom: (n: string, c?: string) => void }).zoom(
    'fit-viewport',
    'auto'
  );
}

function undo() {
  if (props.engine) {
    props.engine.undo();
    return;
  }
  props.modeler?.get('commandStack') &&
    (props.modeler.get('commandStack') as { undo: () => void }).undo();
}

function redo() {
  if (props.engine) {
    props.engine.redo();
    return;
  }
  props.modeler?.get('commandStack') &&
    (props.modeler.get('commandStack') as { redo: () => void }).redo();
}

async function restart() {
  if (!active.value) return;
  try {
    await ElMessageBox.confirm('将清空当前画布并加载空白图，是否继续？', '重新绘制', {
      type: 'warning',
      confirmButtonText: '确定',
      cancelButtonText: '取消'
    });
  } catch {
    return;
  }
  canUndo.value = false;
  canRedo.value = false;
  emit('import-xml', EMPTY_BPMN_XML);
}
</script>

<style scoped>
.sf-designer-toolbar {
  flex: 0 0 auto;
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
  padding: 6px 12px;
  border-bottom: 1px solid var(--el-border-color-lighter, #ebeef5);
  background: var(--el-bg-color, #fff);
}
.sf-designer-toolbar__txt {
  margin-left: 4px;
}
.sf-designer-toolbar__zoom {
  min-width: 56px;
  cursor: default;
}
.sf-designer-toolbar__file {
  display: none;
}
.sf-align__icon {
  display: inline-block;
  width: 14px;
  height: 14px;
  position: relative;
  background: linear-gradient(
    to bottom,
    transparent 2px,
    currentColor 2px,
    currentColor 4px,
    transparent 4px,
    transparent 6px,
    currentColor 6px,
    currentColor 8px,
    transparent 8px,
    transparent 10px,
    currentColor 10px,
    currentColor 12px,
    transparent 12px
  );
}
.sf-align__icon::after {
  content: '';
  position: absolute;
  background: currentColor;
}
.sf-align--left .sf-align__icon::after {
  left: 0;
  top: 0;
  width: 2px;
  height: 100%;
}
.sf-align--right .sf-align__icon::after {
  right: 0;
  top: 0;
  width: 2px;
  height: 100%;
}
.sf-align--center .sf-align__icon::after {
  left: 50%;
  top: 0;
  width: 2px;
  height: 100%;
  transform: translateX(-50%);
}
.sf-align--top .sf-align__icon {
  background: linear-gradient(
    to right,
    transparent 2px,
    currentColor 2px,
    currentColor 4px,
    transparent 4px,
    transparent 6px,
    currentColor 6px,
    currentColor 8px,
    transparent 8px,
    transparent 10px,
    currentColor 10px,
    currentColor 12px,
    transparent 12px
  );
}
.sf-align--top .sf-align__icon::after {
  left: 0;
  top: 0;
  width: 100%;
  height: 2px;
}
.sf-align--bottom .sf-align__icon {
  background: linear-gradient(
    to right,
    transparent 2px,
    currentColor 2px,
    currentColor 4px,
    transparent 4px,
    transparent 6px,
    currentColor 6px,
    currentColor 8px,
    transparent 8px,
    transparent 10px,
    currentColor 10px,
    currentColor 12px,
    transparent 12px
  );
}
.sf-align--bottom .sf-align__icon::after {
  left: 0;
  bottom: 0;
  width: 100%;
  height: 2px;
}
.sf-align--middle .sf-align__icon {
  background: linear-gradient(
    to right,
    transparent 2px,
    currentColor 2px,
    currentColor 4px,
    transparent 4px,
    transparent 6px,
    currentColor 6px,
    currentColor 8px,
    transparent 8px,
    transparent 10px,
    currentColor 10px,
    currentColor 12px,
    transparent 12px
  );
}
.sf-align--middle .sf-align__icon::after {
  left: 0;
  top: 50%;
  width: 100%;
  height: 2px;
  transform: translateY(-50%);
}
</style>

<style>
.sf-designer-preview__code {
  margin: 0;
  max-height: 70vh;
  overflow: auto;
  padding: 12px;
  background: #f6f8fa;
  border-radius: 6px;
  font-size: 12px;
  line-height: 1.5;
}
/* 隐藏 token-simulation 自带角标，改由工具栏切换 */
.sf-designer-page__canvas .bts-toggle-mode,
.sf-designer-page__canvas .toggle-mode {
  display: none !important;
}
</style>
