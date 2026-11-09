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
        <el-button type="primary" :loading="saving" :disabled="!modeler" @click="save">保存</el-button>
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

async function importXml(xml: string) {
  if (!modeler.value) return;
  clearVariableCache();
  await modeler.value.importXML(normalizeSfNamespace(xml));
  // iframe/分步页初次挂载时容器尺寸可能为 0，延后 fit 避免视口错乱
  await nextTick();
  requestAnimationFrame(() => {
    requestAnimationFrame(() => {
      if (!modeler.value) return;
      const canvas = modeler.value.get('canvas') as { zoom: (level: string, center?: string) => void };
      try {
        canvas.zoom('fit-viewport', 'auto');
      } catch {
        canvas.zoom('fit-viewport');
      }
    });
  });
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

    const xml = entity.XmlContent?.trim();
    if (!xml) throw new Error('流程文件无 XmlContent');
    try {
      await importXml(xml);
      statusOk.value = true;
      statusMsg.value = '已打开流程';
    } catch (importErr: unknown) {
      // 种子/旧 XPDL 等非 bpmn-js 格式：落空白图但保留 ProcessId，可直接保存覆盖
      await importXml(EMPTY_BPMN_XML);
      statusOk.value = false;
      statusMsg.value = `流程图无法解析，已用空白图打开（可保存覆盖）：${
        importErr instanceof Error ? importErr.message : String(importErr)
      }`;
    }
  } catch (e: unknown) {
    statusOk.value = false;
    statusMsg.value = e instanceof Error ? e.message : String(e);
    await importXml(EMPTY_BPMN_XML);
  } finally {
    loading.value = false;
  }
}

async function createNew() {
  loading.value = true;
  statusMsg.value = '';
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
  saving.value = true;
  statusMsg.value = '';
  try {
    prepareXmlExport();
    const { xml: rawXml } = await modeler.value.saveXML({ format: true });
    const xml = normalizeSfNamespace(rawXml);
    const base = fileMeta.value ?? {};
    const processId =
      metaProcessId(base) || String(route.query.processId ?? '');
    if (!processId) {
      ElMessage.warning('无 ProcessId，请先「新建」或从列表打开');
      return;
    }
    // 保存不硬拦；完整校验在发布/启用（UpdateProcessUsingState）
    const payload = {
      ProcessId: processId,
      ProcessName: base.ProcessName ?? (base as Record<string, unknown>).processName,
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
    statusOk.value = true;
    statusMsg.value = '保存成功';
    ElMessage.success('保存成功');
  } catch (e: unknown) {
    statusOk.value = false;
    statusMsg.value = e instanceof Error ? e.message : String(e);
    ElMessage.error(statusMsg.value);
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

onMounted(async () => {
  await nextTick();
  if (!canvasEl.value || !panelEl.value) return;
  modeler.value = createModeler({
    container: canvasEl.value,
    propertiesPanel: panelEl.value
  });
  await loadFromQuery();
});

onBeforeUnmount(() => {
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
