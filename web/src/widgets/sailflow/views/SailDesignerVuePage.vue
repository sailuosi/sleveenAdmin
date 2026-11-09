<template>
  <div class="sf-designer-page">
    <header class="sf-designer-page__bar">
      <div class="sf-designer-page__meta">
        <div class="sf-designer-page__title">Sailflow 设计器 · Vue</div>
        <div v-if="metaLabel" class="sf-designer-page__sub">{{ metaLabel }}</div>
      </div>
      <div class="sf-designer-page__actions">
        <el-button text @click="goBack">返回</el-button>
        <el-button :loading="loading" @click="createNew">新建</el-button>
        <el-button :loading="validating" :disabled="!ready" @click="validate">校验</el-button>
        <el-button :disabled="!ready" @click="openConfigForSelection">完整配置</el-button>
        <el-dropdown :disabled="!ready" trigger="click" @command="onExport">
          <el-button>导出</el-button>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="xml">BPMN XML</el-dropdown-item>
              <el-dropdown-item command="svg">SVG</el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
        <el-button type="primary" :loading="saving" :disabled="!ready || loadBlocked" @click="save">
          保存
        </el-button>
      </div>
    </header>
    <SailDesignerToolbar
      :engine="engineApi"
      :file-base-name="fileBaseName()"
      :prepare-export="prepareXmlExport"
      @import-xml="onToolbarImportXml"
    />
    <el-alert
      v-if="statusMsg"
      :title="statusMsg"
      :type="statusOk ? 'success' : 'error'"
      show-icon
      :closable="true"
      class="sf-designer-page__alert"
      @close="statusMsg = ''"
    />
    <main class="sf-designer-page__main sf-designer-page__main--vue">
      <BpmnModeler
        ref="modelerRef"
        class="sf-designer-page__vue-modeler"
        @command-stack.changed="onStackChanged"
        @selection.changed="onSelectionChanged"
        @import.done="ready = true"
      />
    </main>
    <PerformerPickerDialog />
    <FormPickerDialog />
    <VariableEditorDialog />
    <ExpressionEditorDialog />
    <AxConfigEditorDialog />
    <RuleSetPickerDialog />
    <SubProcessPickerDialog />
    <SailNodeConfigDialog />
    <SailTransitionConfigDialog />
    <SailAssigneeParamsDialog />
    <SailElementConfigDialog />
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, shallowRef } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ElMessage } from 'element-plus';
import { BpmnModeler } from '@vue-bpmn/vue';
import { EMPTY_BPMN_XML } from '@sail/bpmn/createModeler';
import { setDesignerProcessMeta } from '@sail/bpmn/ui/designerContext';
import { designerEvents } from '@sail/bpmn/ui/designerEvents';
import { downloadSvg, downloadText } from '@sail/bpmn/utils/download';
import { clearVariableCache } from '@sail/bpmn/variable/wfVariableCache';
import { sanitizeBpmnXml } from '@sail/bpmn/utils/sanitizeBpmnXml';
import { initNewBpmnFile, queryProcessFile, saveProcessFile, validateProcess } from '@sail/api/process';
import type { ProcessDesignerEngine, DesignerAlignDir } from '@sail/designer/engine';
import { alignDirToMode } from '@sail/designer/engine';
import {
  createVueModdleBridge,
  createVueModelingBridge,
  stripSfVariablesFromStore,
  wrapDiagramElement,
} from '@sail/designer/vueModdle';
import { installVueDesignDefaults } from '@sail/designer/vueDefaults';
import PerformerPickerDialog from '@sail/components/PerformerPickerDialog.vue';
import FormPickerDialog from '@sail/components/FormPickerDialog.vue';
import VariableEditorDialog from '@sail/components/VariableEditorDialog.vue';
import ExpressionEditorDialog from '@sail/components/ExpressionEditorDialog.vue';
import AxConfigEditorDialog from '@sail/components/AxConfigEditorDialog.vue';
import RuleSetPickerDialog from '@sail/components/RuleSetPickerDialog.vue';
import SubProcessPickerDialog from '@sail/components/SubProcessPickerDialog.vue';
import SailNodeConfigDialog from '@sail/components/nodeConfig/SailNodeConfigDialog.vue';
import SailTransitionConfigDialog from '@sail/components/transitionConfig/SailTransitionConfigDialog.vue';
import SailAssigneeParamsDialog from '@sail/components/assignee/SailAssigneeParamsDialog.vue';
import SailElementConfigDialog from '@sail/components/elementConfig/SailElementConfigDialog.vue';
import SailDesignerToolbar from '@sail/components/designer/SailDesignerToolbar.vue';

defineOptions({ name: 'SailDesignerVuePage' });

type ProcessFileMeta = {
  ProcessId?: string;
  ProcessName?: string;
  ProcessCode?: string;
  Version?: string;
  Status?: number;
  XmlContent?: string;
  [key: string]: unknown;
};

type ModelerExpose = {
  importXML: (xml: string) => Promise<{ warnings?: string[] }>;
  exportXML: () => Promise<{ xml: string }>;
  exportSVG: () => Promise<{ svg: string }>;
  undo: () => void;
  redo: () => void;
  zoom: (factor?: number) => void;
  zoomFit: () => void;
  alignElements: (mode: ReturnType<typeof alignDirToMode>) => void;
  getStore: () => import('@vue-bpmn/core').DiagramStore;
};

const router = useRouter();
const route = useRoute();

const modelerRef = ref<ModelerExpose | null>(null);
const engineApi = shallowRef<ProcessDesignerEngine | null>(null);
const fileMeta = ref<ProcessFileMeta | null>(null);
const loading = ref(false);
const saving = ref(false);
const validating = ref(false);
const statusMsg = ref('');
const statusOk = ref(false);
const loadBlocked = ref(false);
const ready = ref(false);
const selectionIds = ref<string[]>([]);
const isEmbed = computed(() => String(route.query.embed || '') === '1');

let detachDefaults: (() => void) | null = null;
const changeListeners = new Set<() => void>();

const VALIDATE_MSG: Record<number, string> = {
  0: '未得到校验结果',
  1: '校验通过：存在开始/结束事件，且有通路；用户任务 Resolve/Leave 已配置',
  2: '校验未通过：缺少开始事件',
  3: '校验未通过：缺少结束事件',
  4: '校验未通过：开始到结束无连通路径',
  5: '校验未通过：用户任务缺少 AssigneeResolve 或 LeaveJudge Type（发布前须配置）',
};

function fileBaseName() {
  const m = fileMeta.value;
  return String(m?.ProcessCode || m?.ProcessId || 'process').replace(/[^\w.-]+/g, '_');
}

function pickField(obj: Record<string, unknown>, ...keys: string[]): unknown {
  for (const k of keys) {
    const v = obj[k];
    if (v != null && String(v).trim() !== '') return v;
  }
  return undefined;
}

function normalizeProcessFile(data: unknown): ProcessFileMeta | null {
  if (!data || typeof data !== 'object') return null;
  const d = data as Record<string, unknown>;
  const raw = (d.Entity ?? d.entity ?? d) as Record<string, unknown>;
  if (!raw || typeof raw !== 'object' || Array.isArray(raw)) return null;
  const processId = pickField(raw, 'ProcessId', 'processId');
  const xmlContent = pickField(raw, 'XmlContent', 'xmlContent');
  if (processId == null && xmlContent == null) return null;
  return {
    ...raw,
    ProcessId: processId != null ? String(processId) : undefined,
    ProcessName: pickField(raw, 'ProcessName', 'processName') as string | undefined,
    ProcessCode: pickField(raw, 'ProcessCode', 'processCode') as string | undefined,
    Version: String(pickField(raw, 'Version', 'version') ?? '1'),
    Status: (pickField(raw, 'Status', 'status') as number | undefined) ?? undefined,
    XmlContent: xmlContent != null ? String(xmlContent) : undefined,
  };
}

function metaProcessId(m: ProcessFileMeta | null | undefined): string {
  if (!m) return '';
  return String(m.ProcessId || (m as Record<string, unknown>).processId || '');
}

function syncProcessMeta(m: ProcessFileMeta | null) {
  setDesignerProcessMeta({
    processId: metaProcessId(m),
    version: String(m?.Version || (m as Record<string, unknown> | null)?.version || '1'),
  });
}

const metaLabel = computed(() => {
  const m = fileMeta.value;
  const id = metaProcessId(m);
  if (!id) return '';
  const name = m?.ProcessName || m?.ProcessCode || id;
  return `${name} · v${m?.Version ?? '1'} · ${id}`;
});

function goBack() {
  if (window.history.length > 1) router.back();
  else router.push('/sail/processes');
}

function normalizeSfNamespace(xml: string): string {
  return xml.replaceAll('http://www.slickflow.com/schema/sf', 'http://www.saiflow.com/schema/sf');
}

function notifyChange() {
  changeListeners.forEach(fn => fn());
}

function onStackChanged() {
  notifyChange();
}

function onSelectionChanged(ids: string[]) {
  selectionIds.value = ids;
  notifyChange();
}

function buildEngine(): ProcessDesignerEngine {
  const m = () => modelerRef.value;
  const store = () => m()?.getStore();
  return {
    kind: 'vue-bpmn',
    supportsSimulation: false,
    raw: undefined,
    async importXML(xml: string) {
      const r = await m()!.importXML(normalizeSfNamespace(xml));
      return { warnings: r?.warnings };
    },
    async exportXML() {
      return m()!.exportXML();
    },
    async exportSVG() {
      return m()!.exportSVG();
    },
    undo() {
      m()?.undo();
    },
    redo() {
      m()?.redo();
    },
    canUndo() {
      return store()?.commandStack.canUndo() ?? false;
    },
    canRedo() {
      return store()?.commandStack.canRedo() ?? false;
    },
    zoom(level) {
      if (level === 'fit-viewport') m()?.zoomFit();
      else m()?.zoom(typeof level === 'number' ? level : 0.1);
    },
    getZoom() {
      return store()?.getState().viewbox.scale ?? 1;
    },
    align(dir: DesignerAlignDir) {
      m()?.alignElements(alignDirToMode(dir));
    },
    destroy() {
      /* vue component teardown */
    },
    getModelingBridge() {
      return createVueModelingBridge(store()!);
    },
    getModdleBridge() {
      return createVueModdleBridge();
    },
    openNodeConfig: () => openConfigForSelection(),
    openTransitionConfig: () => openConfigForSelection(),
    onChange(fn) {
      changeListeners.add(fn);
      return () => changeListeners.delete(fn);
    },
  };
}

function prepareXmlExport() {
  const s = modelerRef.value?.getStore();
  if (s) stripSfVariablesFromStore(s);
}

function openConfigForSelection() {
  const id = selectionIds.value[0];
  const s = modelerRef.value?.getStore();
  if (!id || !s) {
    ElMessage.warning('请先选中一个节点或连线');
    return;
  }
  const de = s.getElement(id);
  if (!de) return;
  const view = wrapDiagramElement(s, id, { withFlowEnds: true });
  if (!view) return;
  const modeling = createVueModelingBridge(s);
  const moddle = createVueModdleBridge();
  const t = String(de.type);
  if (t === 'bpmn:SequenceFlow') {
    designerEvents.emit('transitionconfig:open', { element: view, modeling, moddle });
    return;
  }
  if (
    t === 'bpmn:UserTask' ||
    t === 'bpmn:Task' ||
    t.endsWith('Task') ||
    t === 'bpmn:SubProcess' ||
    t.includes('Gateway') ||
    t.includes('Event')
  ) {
    if (t === 'bpmn:UserTask' || t === 'bpmn:Task') {
      designerEvents.emit('nodeconfig:open', { element: view, modeling, moddle });
      return;
    }
    if (t.includes('Gateway')) {
      designerEvents.emit('gatewayconfig:open', { element: view, modeling, moddle });
      return;
    }
    if (t.includes('Service') || t.includes('Script') || t.includes('BusinessRule')) {
      const ev =
        t.includes('Script')
          ? 'scripttaskconfig:open'
          : t.includes('BusinessRule') || t.includes('Rule')
            ? 'ruletaskconfig:open'
            : 'servicetaskconfig:open';
      designerEvents.emit(ev as any, { element: view, modeling, moddle });
      return;
    }
    if (t.includes('Event')) {
      designerEvents.emit('eventconfig:open', { element: view, modeling, moddle });
      return;
    }
    if (t === 'bpmn:SubProcess' || t === 'bpmn:CallActivity') {
      designerEvents.emit('subprocessconfig:open', { element: view, modeling, moddle });
      return;
    }
  }
  ElMessage.info('当前元素暂无完整配置面板，可使用右侧属性编辑');
}

async function importXml(xml: string) {
  if (!modelerRef.value) return;
  clearVariableCache();
  const result = await modelerRef.value.importXML(normalizeSfNamespace(xml));
  await nextTick();
  modelerRef.value.zoomFit();
  return result?.warnings || [];
}

async function onToolbarImportXml(xml: string) {
  try {
    await importXml(xml);
    statusOk.value = true;
    statusMsg.value = '已从本地/工具栏载入流程图';
  } catch (e: unknown) {
    statusOk.value = false;
    statusMsg.value = e instanceof Error ? e.message : String(e);
    ElMessage.error(statusMsg.value);
  }
}

async function loadFromQuery() {
  const processId = String(route.query.processId ?? '');
  const version = String(route.query.version ?? '1');
  if (!processId) {
    await createNew();
    return;
  }
  loading.value = true;
  statusMsg.value = '';
  loadBlocked.value = false;
  fileMeta.value = { ProcessId: processId, Version: version };
  syncProcessMeta(fileMeta.value);
  try {
    const data = await queryProcessFile({ ProcessId: processId, Version: version });
    const failStatus = data?.Status ?? data?.status;
    if (failStatus === -1 || failStatus === '-1') {
      throw new Error(data?.Message || data?.message || '打开流程失败');
    }
    const entity = normalizeProcessFile(data);
    if (!entity) throw new Error('打开流程失败：响应无流程实体');
    fileMeta.value = { ...entity, ProcessId: entity.ProcessId || processId, Version: entity.Version || version };
    syncProcessMeta(fileMeta.value);
    const xml = entity.XmlContent?.trimEnd()?.trim();
    if (!xml) throw new Error('流程文件无 XmlContent');
    try {
      const warnings = await importXml(xml);
      loadBlocked.value = false;
      statusOk.value = true;
      const warnN = warnings?.length || 0;
      statusMsg.value = warnN
        ? `已打开流程（${warnN} 条 import 警告）`
        : '已打开流程（Vue 设计器）';
    } catch (importErr: unknown) {
      loadBlocked.value = true;
      statusOk.value = false;
      statusMsg.value = `流程图无法解析，已阻止覆盖保存：${
        importErr instanceof Error ? importErr.message : String(importErr)
      }`;
      ElMessage.error(statusMsg.value);
    }
  } catch (e: unknown) {
    loadBlocked.value = true;
    statusOk.value = false;
    statusMsg.value = e instanceof Error ? e.message : String(e);
    ElMessage.error(statusMsg.value);
  } finally {
    loading.value = false;
  }
}

async function createNew() {
  loading.value = true;
  statusMsg.value = '';
  loadBlocked.value = false;
  try {
    const data = await initNewBpmnFile();
    const failStatus = data?.Status ?? data?.status;
    if (failStatus === -1 || failStatus === '-1') {
      throw new Error(data?.Message || data?.message || 'InitNewBPMNFile 失败');
    }
    const entity = normalizeProcessFile(data);
    if (entity?.XmlContent) {
      fileMeta.value = entity;
      syncProcessMeta(entity);
      await importXml(entity.XmlContent);
    } else {
      fileMeta.value = entity;
      syncProcessMeta(entity);
      await importXml(EMPTY_BPMN_XML);
    }
    statusOk.value = true;
    statusMsg.value = '已新建空白流程（Vue 设计器）';
  } catch (e: unknown) {
    fileMeta.value = null;
    await importXml(EMPTY_BPMN_XML);
    statusOk.value = false;
    statusMsg.value = `新建走本地兜底：${e instanceof Error ? e.message : String(e)}`;
  } finally {
    loading.value = false;
  }
}

async function save() {
  if (!modelerRef.value) return;
  if (loadBlocked.value) {
    ElMessage.error('当前流程图未能正确打开，已禁止保存，以免覆盖库中定义');
    return;
  }
  saving.value = true;
  statusMsg.value = '';
  try {
    prepareXmlExport();
    const { xml: rawXml } = await modelerRef.value.exportXML();
    const normalized = normalizeSfNamespace(rawXml || '');
    const { xml: cleaned, removedDi } = sanitizeBpmnXml(normalized);
    const xml = cleaned;
    const base = fileMeta.value ?? {};
    const processId = metaProcessId(base) || String(route.query.processId ?? '');
    if (!processId) {
      ElMessage.warning('无 ProcessId，请先「新建」或从列表打开');
      return;
    }
    const processName = String(
      base.ProcessName ?? (base as Record<string, unknown>).processName ?? '',
    ).trim();
    if (!processName) {
      ElMessage.warning('流程名称为空，无法保存');
      return;
    }
    const payload = {
      ProcessId: processId,
      ProcessName: processName,
      ProcessCode: base.ProcessCode ?? (base as Record<string, unknown>).processCode,
      Version: String(base.Version ?? (base as Record<string, unknown>).version ?? route.query.version ?? '1'),
      Status: (base.Status ?? (base as Record<string, unknown>).status ?? 1) as number,
      XmlContent: xml,
    };
    const data = await saveProcessFile(payload);
    const failStatus = data?.Status ?? data?.status;
    if (failStatus === -1 || failStatus === '-1') {
      throw new Error(data?.Message || data?.message || '保存失败');
    }
    fileMeta.value = {
      ...base,
      ...payload,
      ProcessId: processId,
      ProcessCode: payload.ProcessCode != null ? String(payload.ProcessCode) : base.ProcessCode,
      XmlContent: xml
    };
    loadBlocked.value = false;
    statusOk.value = true;
    statusMsg.value = removedDi.length
      ? `保存成功（已去掉 ${removedDi.length} 个无元素 DI）`
      : '保存成功';
    ElMessage.success(statusMsg.value);
    if (isEmbed.value && window.parent && window.parent !== window) {
      window.parent.postMessage(
        { type: 'sail-designer-saved', processId, version: payload.Version },
        '*',
      );
    }
  } catch (e: unknown) {
    statusOk.value = false;
    statusMsg.value = e instanceof Error ? e.message : String(e);
    ElMessage.error(statusMsg.value);
    if (isEmbed.value && window.parent && window.parent !== window) {
      window.parent.postMessage(
        { type: 'sail-designer-save-failed', message: statusMsg.value },
        '*',
      );
    }
  } finally {
    saving.value = false;
  }
}

async function validate() {
  if (!modelerRef.value) return;
  validating.value = true;
  statusMsg.value = '';
  try {
    prepareXmlExport();
    const { xml } = await modelerRef.value.exportXML();
    const base = fileMeta.value ?? {};
    const data = await validateProcess({
      ProcessId: metaProcessId(base) || String(route.query.processId ?? '') || undefined,
      ProcessName: base.ProcessName ?? (base as Record<string, unknown>).processName,
      ProcessCode: base.ProcessCode ?? (base as Record<string, unknown>).processCode,
      Version: String(base.Version ?? (base as Record<string, unknown>).version ?? '1'),
      XmlContent: xml,
    });
    const failStatus = data?.Status ?? data?.status;
    if (failStatus === -1 || failStatus === '-1') {
      throw new Error(data?.Message || data?.message || '校验失败');
    }
    const entity = (data?.Entity ?? data?.entity ?? data) as Record<string, unknown>;
    const rawCode = entity?.ProcessValidatedResultType ?? entity?.processValidatedResultType ?? 0;
    const code = Number(rawCode);
    const issues = (entity?.Issues ?? entity?.issues ?? []) as unknown;
    const issueText =
      Array.isArray(issues) && issues.length ? `：${issues.map(String).join('；')}` : '';
    const detail = String(entity?.Message ?? entity?.message ?? '').trim();
    const msg =
      (VALIDATE_MSG[code] || `校验结果码：${code}`) + (issueText || (detail ? `：${detail}` : ''));
    statusOk.value = code === 1;
    statusMsg.value = msg;
    if (code === 1) ElMessage.success(msg);
    else ElMessage.warning(msg);
  } catch (e: unknown) {
    statusOk.value = false;
    statusMsg.value = e instanceof Error ? e.message : String(e);
    ElMessage.error(statusMsg.value);
  } finally {
    validating.value = false;
  }
}

async function onExport(cmd: string) {
  if (!modelerRef.value) return;
  try {
    const base = fileBaseName();
    if (cmd === 'xml') {
      prepareXmlExport();
      const { xml } = await modelerRef.value.exportXML();
      downloadText(`${base}.bpmn`, xml || '', 'application/xml');
      ElMessage.success('已导出 XML');
      return;
    }
    if (cmd === 'svg') {
      const { svg } = await modelerRef.value.exportSVG();
      downloadSvg(`${base}.svg`, svg || '');
      ElMessage.success('已导出 SVG');
    }
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  }
}

function onParentMessage(ev: MessageEvent) {
  const data = ev?.data;
  if (!data || typeof data !== 'object') return;
  if (data.type === 'sail-designer-save') {
    void save();
  }
}

onMounted(async () => {
  await nextTick();
  engineApi.value = buildEngine();
  const store = modelerRef.value?.getStore();
  if (store) detachDefaults = installVueDesignDefaults(store);
  window.addEventListener('message', onParentMessage);
  await loadFromQuery();
});

onBeforeUnmount(() => {
  window.removeEventListener('message', onParentMessage);
  detachDefaults?.();
  detachDefaults = null;
  clearVariableCache();
  setDesignerProcessMeta({ processId: '', version: '1' });
  engineApi.value = null;
});
</script>

<style scoped>
.sf-designer-page {
  height: 100vh;
  display: flex;
  flex-direction: column;
  background: var(--el-bg-color-page, #f5f7fa);
}
.sf-designer-page__bar {
  flex: 0 0 auto;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 10px 16px;
  border-bottom: 1px solid var(--el-border-color-lighter, #ebeef5);
  background: var(--el-bg-color, #fff);
}
.sf-designer-page__meta {
  min-width: 0;
}
.sf-designer-page__title {
  font-weight: 600;
  font-size: 15px;
}
.sf-designer-page__sub {
  font-size: 12px;
  color: var(--el-text-color-secondary);
  margin-top: 2px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.sf-designer-page__actions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  align-items: center;
}
.sf-designer-page__alert {
  margin: 0 12px;
  flex: 0 0 auto;
}
.sf-designer-page__main--vue {
  flex: 1 1 auto;
  min-height: 0;
  display: flex;
}
.sf-designer-page__vue-modeler {
  flex: 1 1 auto;
  min-height: 0;
  width: 100%;
}
</style>
