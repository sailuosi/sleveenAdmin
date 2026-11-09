<template>
  <div class="sf-designer-page">
    <header class="sf-designer-page__bar">
      <div class="sf-designer-page__meta">
        <div class="sf-designer-page__title">Sailflow 设计器</div>
        <div v-if="metaLabel" class="sf-designer-page__sub">{{ metaLabel }}</div>
      </div>
      <div class="sf-designer-page__actions">
        <el-button text @click="goBack">返回</el-button>
        <el-button :loading="loading" @click="createNew">新建</el-button>
        <el-button :loading="validating" :disabled="!modeler" @click="validate">校验</el-button>
        <el-dropdown :disabled="!modeler" trigger="click" @command="onExport">
          <el-button>导出</el-button>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="xml">BPMN XML</el-dropdown-item>
              <el-dropdown-item command="svg">SVG</el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
        <el-button type="primary" :loading="saving" :disabled="!modeler || loadBlocked" @click="save">保存</el-button>
      </div>
    </header>
    <SailDesignerToolbar
      :modeler="modeler"
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
    <main class="sf-designer-page__main">
      <div ref="canvasEl" v-loading="loading" class="sf-designer-page__canvas" />
      <aside ref="panelEl" class="sf-designer-page__panel" />
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
import { computed, nextTick, onBeforeUnmount, onMounted, ref, shallowRef, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ElMessage } from 'element-plus';
import { createModeler, EMPTY_BPMN_XML, type SailBpmnModeler } from '@sail/bpmn/createModeler';
import { setDesignerProcessMeta } from '@sail/bpmn/ui/designerContext';
import { downloadSvg, downloadText } from '@sail/bpmn/utils/download';
import {
  clearVariableCache,
  stripSfVariablesFromDiagram
} from '@sail/bpmn/variable/wfVariableCache';
import { sanitizeBpmnXml } from '@sail/bpmn/utils/sanitizeBpmnXml';
import { initNewBpmnFile, queryProcessFile, saveProcessFile, validateProcess } from '@sail/api/process';
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

defineOptions({ name: 'SailDesignerPage' });

type ProcessFileMeta = {
  ProcessId?: string;
  ProcessName?: string;
  ProcessCode?: string;
  Version?: string;
  Status?: number;
  XmlContent?: string;
  [key: string]: unknown;
};

const router = useRouter();
const route = useRoute();

const canvasEl = ref<HTMLElement | null>(null);
const panelEl = ref<HTMLElement | null>(null);
const modeler = shallowRef<SailBpmnModeler | null>(null);
const fileMeta = ref<ProcessFileMeta | null>(null);
const loading = ref(false);
const saving = ref(false);
const validating = ref(false);
const statusMsg = ref('');
const statusOk = ref(false);
/** 打开已有流程时 import 失败：禁止用空白图覆盖库里的定义 */
const loadBlocked = ref(false);
const isEmbed = computed(() => String(route.query.embed || '') === '1');

/** 与后端 ProcessValidateResultTypeEnum 一致；硬拦仅在发布/启用 */
const VALIDATE_MSG: Record<number, string> = {
  0: '未得到校验结果',
  1: '校验通过：存在开始/结束事件，且有通路；用户任务 Resolve/Leave 已配置',
  2: '校验未通过：缺少开始事件',
  3: '校验未通过：缺少结束事件',
  4: '校验未通过：开始到结束无连通路径',
  5: '校验未通过：用户任务缺少 AssigneeResolve 或 LeaveJudge Type（发布前须配置）'
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

/** API 多为 camelCase；统一成设计器内部 PascalCase */
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
    XmlContent: xmlContent != null ? String(xmlContent) : undefined
  };
}

function metaProcessId(m: ProcessFileMeta | null | undefined): string {
  if (!m) return '';
  return String(m.ProcessId || (m as Record<string, unknown>).processId || '');
}

function syncProcessMeta(m: ProcessFileMeta | null) {
  setDesignerProcessMeta({
    processId: metaProcessId(m),
    version: String(m?.Version || (m as Record<string, unknown> | null)?.version || '1')
  });
}

watch(fileMeta, syncProcessMeta, { immediate: true });

/** 导出/保存前去掉 sf:Variables，变量只走 wf_variable */
function prepareXmlExport() {
  if (!modeler.value) return;
  const modeling = modeler.value.get('modeling') as { updateModdleProperties: Function };
  const elementRegistry = modeler.value.get('elementRegistry') as { getAll: () => any[] };
  stripSfVariablesFromDiagram(modeling, elementRegistry);
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

/** 统一为引擎约定的 saiflow.com；历史 slickflow.com 导入/保存时改写，避免 sf/sf_1 双前缀 */
function normalizeSfNamespace(xml: string): string {
  return xml.replaceAll('http://www.slickflow.com/schema/sf', 'http://www.saiflow.com/schema/sf');
}

function fitCanvasViewport(retries = 8) {
  if (!modeler.value) return;
  const el = canvasEl.value;
  const w = el?.clientWidth ?? 0;
  const h = el?.clientHeight ?? 0;
  // 容器为 0 时 fit-viewport 会算出 Infinity，触发 SVGMatrix.scale 非有限值
  if (w < 8 || h < 8) {
    if (retries > 0) {
      requestAnimationFrame(() => fitCanvasViewport(retries - 1));
    }
    return;
  }
  const canvas = modeler.value.get('canvas') as {
    zoom: (level: string | number, center?: string) => void;
  };
  try {
    canvas.zoom('fit-viewport', 'auto');
  } catch {
    try {
      canvas.zoom('fit-viewport');
    } catch {
      /* ignore */
    }
  }
}

async function importXml(xml: string) {
  if (!modeler.value) return;
  clearVariableCache();
  const result = await modeler.value.importXML(normalizeSfNamespace(xml));
  const warnings = (result as { warnings?: Array<{ message?: string }> })?.warnings || [];
  await nextTick();
  requestAnimationFrame(() => fitCanvasViewport());
  return warnings;
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
  // 先占位：即使打开失败，保存仍能带上列表传来的 ProcessId
  fileMeta.value = { ProcessId: processId, Version: version };
  try {
    const data = await queryProcessFile({ ProcessId: processId, Version: version });
    const failStatus = data?.Status ?? data?.status;
    if (failStatus === -1 || failStatus === '-1') {
      throw new Error(data?.Message || data?.message || '打开流程失败');
    }
    const entity = normalizeProcessFile(data);
    if (!entity) throw new Error('打开流程失败：响应无流程实体');
    fileMeta.value = { ...entity, ProcessId: entity.ProcessId || processId, Version: entity.Version || version };

    const xml = entity.XmlContent?.trimEnd()?.trim();
    if (!xml) throw new Error('流程文件无 XmlContent');
    try {
      const warnings = await importXml(xml);
      loadBlocked.value = false;
      statusOk.value = true;
      const warnN = warnings?.length || 0;
      statusMsg.value = warnN
        ? `已打开流程（${warnN} 条 import 警告，请检查断链/残缺节点）`
        : '已打开流程';
    } catch (importErr: unknown) {
      // 已有流程禁止用空白图顶掉库内容，否则一点保存就会丢节点
      loadBlocked.value = true;
      statusOk.value = false;
      statusMsg.value = `流程图无法解析，已阻止覆盖保存。请用「导出」备份或修复 XML 后再开：${
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
      await importXml(entity.XmlContent);
    } else {
      fileMeta.value = entity;
      await importXml(EMPTY_BPMN_XML);
    }
    statusOk.value = true;
    statusMsg.value = '已新建空白流程';
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
  if (!modeler.value) return;
  if (loadBlocked.value) {
    ElMessage.error('当前流程图未能正确打开，已禁止保存，以免覆盖库中定义');
    return;
  }
  saving.value = true;
  statusMsg.value = '';
  try {
    prepareXmlExport();
    const { xml: rawXml } = await modeler.value.saveXML({ format: true });
    const normalized = normalizeSfNamespace(rawXml || '');
    const { xml: cleaned, removedDi } = sanitizeBpmnXml(normalized);
    const xml = cleaned;
    const base = fileMeta.value ?? {};
    const processId =
      metaProcessId(base) || String(route.query.processId ?? '');
    if (!processId) {
      ElMessage.warning('无 ProcessId，请先「新建」或从列表打开');
      return;
    }
    const processName = String(
      base.ProcessName ?? (base as Record<string, unknown>).processName ?? ''
    ).trim();
    if (!processName) {
      ElMessage.warning('流程名称为空，无法保存');
      return;
    }
    // 保存不硬拦结构；完整校验在发布/启用（UpdateProcessUsingState）
    const payload = {
      ProcessId: processId,
      ProcessName: processName,
      ProcessCode: base.ProcessCode ?? (base as Record<string, unknown>).processCode,
      Version: String(base.Version ?? (base as Record<string, unknown>).version ?? route.query.version ?? '1'),
      Status: (base.Status ?? (base as Record<string, unknown>).status ?? 1) as number,
      XmlContent: xml
    };
    const data = await saveProcessFile(payload);
    const failStatus = data?.Status ?? data?.status;
    if (failStatus === -1 || failStatus === '-1') {
      throw new Error(data?.Message || data?.message || '保存失败');
    }
    fileMeta.value = { ...base, ...payload, ProcessId: processId, XmlContent: xml };
    loadBlocked.value = false;
    statusOk.value = true;
    statusMsg.value = removedDi.length
      ? `保存成功（已去掉 ${removedDi.length} 个无元素 DI：${removedDi.slice(0, 3).join(',')}）`
      : '保存成功';
    ElMessage.success(statusMsg.value);
    if (isEmbed.value && window.parent && window.parent !== window) {
      window.parent.postMessage(
        { type: 'sail-designer-saved', processId, version: payload.Version },
        '*'
      );
    }
  } catch (e: unknown) {
    statusOk.value = false;
    statusMsg.value = e instanceof Error ? e.message : String(e);
    ElMessage.error(statusMsg.value);
    if (isEmbed.value && window.parent && window.parent !== window) {
      window.parent.postMessage(
        { type: 'sail-designer-save-failed', message: statusMsg.value },
        '*'
      );
    }
  } finally {
    saving.value = false;
  }
}

async function validate() {
  if (!modeler.value) return;
  validating.value = true;
  statusMsg.value = '';
  try {
    prepareXmlExport();
    const { xml } = await modeler.value.saveXML({ format: true });
    const base = fileMeta.value ?? {};
    const data = await validateProcess({
      ProcessId: metaProcessId(base) || String(route.query.processId ?? '') || undefined,
      ProcessName: base.ProcessName ?? (base as Record<string, unknown>).processName,
      ProcessCode: base.ProcessCode ?? (base as Record<string, unknown>).processCode,
      Version: String(base.Version ?? (base as Record<string, unknown>).version ?? '1'),
      XmlContent: xml
    });
    const failStatus = data?.Status ?? data?.status;
    if (failStatus === -1 || failStatus === '-1') {
      throw new Error(data?.Message || data?.message || '校验失败');
    }
    const entity = (data?.Entity ?? data?.entity ?? data) as Record<string, unknown>;
    const rawCode = entity?.ProcessValidatedResultType ?? entity?.processValidatedResultType ?? 0;
    const code = Number(rawCode);
    const issues = (entity?.Issues ?? entity?.issues ?? []) as unknown;
    const issueText = Array.isArray(issues) && issues.length
      ? `：${issues.map(String).join('；')}`
      : '';
    const detail = String(entity?.Message ?? entity?.message ?? '').trim();
    const msg = (VALIDATE_MSG[code] || `校验结果码：${code}`) + (issueText || (detail ? `：${detail}` : ''));
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
  if (!modeler.value) return;
  try {
    const base = fileBaseName();
    if (cmd === 'xml') {
      prepareXmlExport();
      const { xml } = await modeler.value.saveXML({ format: true });
      downloadText(`${base}.bpmn`, xml || '', 'application/xml');
      ElMessage.success('已导出 XML');
      return;
    }
    if (cmd === 'svg') {
      const { svg } = await modeler.value.saveSVG();
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
  if (!canvasEl.value || !panelEl.value) return;
  modeler.value = createModeler({
    container: canvasEl.value,
    propertiesPanel: panelEl.value
  });
  window.addEventListener('message', onParentMessage);
  await loadFromQuery();
});

onBeforeUnmount(() => {
  window.removeEventListener('message', onParentMessage);
  clearVariableCache();
  setDesignerProcessMeta({ processId: '', version: '1' });
  modeler.value?.destroy();
  modeler.value = null;
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
  font-size: 16px;
  font-weight: 600;
}
.sf-designer-page__sub {
  margin-top: 2px;
  font-size: 12px;
  color: var(--el-text-color-secondary, #909399);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.sf-designer-page__actions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  justify-content: flex-end;
}
.sf-designer-page__alert {
  flex: 0 0 auto;
  margin: 0;
  border-radius: 0;
}
.sf-designer-page__main {
  flex: 1 1 auto;
  min-height: 0;
  display: flex;
  position: relative;
}
.sf-designer-page__canvas {
  flex: 1 1 auto;
  min-width: 0;
  position: relative;
  background: #fafafa;
}
.sf-designer-page__canvas :deep(.bjs-container) {
  height: 100%;
}
.sf-designer-page__panel {
  flex: 0 0 320px;
  width: 320px;
  max-width: 40vw;
  border-left: 1px solid var(--el-border-color-lighter, #ebeef5);
  background: var(--el-bg-color, #fff);
  overflow: auto;
}
.sf-designer-page__panel :deep(.bio-properties-panel) {
  height: 100%;
}
.sf-designer-page__panel :deep(.sf-condition-entry) {
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.sf-designer-page__panel :deep(.sf-condition-entry__btn) {
  align-self: flex-start;
  margin: 0 0 8px 12px;
  padding: 4px 10px;
  border: 1px solid var(--el-border-color, #dcdfe6);
  border-radius: 4px;
  background: var(--el-bg-color, #fff);
  color: var(--el-color-primary, #409eff);
  cursor: pointer;
  font-size: 12px;
}
.sf-designer-page__panel :deep(.sf-condition-entry__btn:hover) {
  border-color: var(--el-color-primary, #409eff);
}
@media (max-width: 768px) {
  .sf-designer-page__main {
    flex-direction: column;
  }
  .sf-designer-page__panel {
    flex: 0 0 40%;
    width: 100%;
    max-width: none;
    border-left: none;
    border-top: 1px solid var(--el-border-color-lighter, #ebeef5);
  }
}
</style>
