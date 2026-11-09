<template>
  <div class="sf-process-design">
    <el-alert
      v-if="!model.processId"
      type="warning"
      show-icon
      :closable="false"
      title="请先保存基本信息，生成流程定义后再设计 BPMN"
      class="sf-process-design__alert"
    />
    <template v-else>
      <div class="sf-process-design__bar">
        <el-tag :type="model.hasBpmn ? 'success' : 'info'" size="small">
          {{ model.hasBpmn ? '已有流程图' : '尚未设计（有默认开始节点）' }}
        </el-tag>
        <el-button type="primary" @click="openDesigner">在设计器中打开</el-button>
        <el-button @click="openDesignerVue">用 Vue 设计器打开</el-button>
        <el-button @click="refreshFlag">刷新状态</el-button>
      </div>
      <iframe
        v-if="showFrame"
        ref="frameRef"
        class="sf-process-design__frame"
        :src="designerUrl"
        title="Sailflow 设计器"
      />
      <div v-else class="sf-process-design__placeholder">
        <p>第三步嵌入 Sail BPMN 设计器（对标参照项目「流程设计」）。</p>
        <el-button type="primary" @click="showFrame = true">加载设计器</el-button>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage } from 'element-plus';
import { queryProcessFile } from '@sail/api/process';
import { unpackSailResult, pickField } from '@sail/utils/sailResult';
import type { ProcessEditFormModel } from './types';

defineOptions({ name: 'SailProcessDesignStep' });

const model = defineModel<ProcessEditFormModel>({ required: true });
const router = useRouter();
const showFrame = ref(true);
const frameRef = ref<HTMLIFrameElement | null>(null);
let saveWait: {
  resolve: (ok: boolean) => void;
  timer: ReturnType<typeof setTimeout>;
} | null = null;

function buildDesignerUrl(path: string, embed = true) {
  const q = new URLSearchParams({
    processId: String(model.value.processId || ''),
    version: String(model.value.version || '1')
  });
  if (embed) q.set('embed', '1');
  return `${router.resolve({ path }).href}?${q.toString()}`;
}

const designerUrl = computed(() => buildDesignerUrl('/sail/designer', true));
const designerVueUrl = computed(() => buildDesignerUrl('/sail/designer-vue', false));

function openDesigner() {
  if (!model.value.processId) return;
  window.open(designerUrl.value, '_blank');
}

function openDesignerVue() {
  if (!model.value.processId) return;
  window.open(designerVueUrl.value, '_blank');
}

async function refreshFlag() {
  if (!model.value.processId) return;
  try {
    const data = await queryProcessFile({
      ProcessId: model.value.processId,
      Version: model.value.version || '1'
    });
    const r = unpackSailResult<Record<string, unknown>>(data);
    const xml = String(pickField(r.entity, 'XmlContent', 'xmlContent') || '').trimEnd();
    model.value.hasBpmn = xml.includes('<process') || xml.includes('<bpmn');
    ElMessage.success(model.value.hasBpmn ? '已检测到流程图' : '仍为初始模板');
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  }
}

/** 让嵌入设计器先 SaveProcessFile，再由外层保存元数据 */
function saveDesigner(): Promise<boolean> {
  if (!showFrame.value || !frameRef.value?.contentWindow) {
    return Promise.resolve(true);
  }
  return new Promise(resolve => {
    if (saveWait) {
      clearTimeout(saveWait.timer);
      saveWait.resolve(false);
    }
    const timer = setTimeout(() => {
      saveWait = null;
      ElMessage.warning('设计器保存超时，请在设计器内点「保存」后再试');
      resolve(false);
    }, 15000);
    saveWait = { resolve, timer };
    frameRef.value!.contentWindow!.postMessage({ type: 'sail-designer-save' }, '*');
  });
}

function onFrameMessage(ev: MessageEvent) {
  const data = ev?.data;
  if (!data || typeof data !== 'object') return;
  if (data.type === 'sail-designer-saved') {
    if (saveWait) {
      clearTimeout(saveWait.timer);
      saveWait.resolve(true);
      saveWait = null;
    }
    model.value.hasBpmn = true;
    return;
  }
  if (data.type === 'sail-designer-save-failed') {
    if (saveWait) {
      clearTimeout(saveWait.timer);
      saveWait.resolve(false);
      saveWait = null;
    }
    ElMessage.error(String(data.message || '设计器保存失败'));
  }
}

async function validate() {
  if (!model.value.processId) {
    throw new Error('请先保存流程基本信息');
  }
  if (showFrame.value) {
    const ok = await saveDesigner();
    if (!ok) throw new Error('请先在设计器中成功保存流程图');
  }
  await refreshFlag();
}

onMounted(() => window.addEventListener('message', onFrameMessage));
onBeforeUnmount(() => {
  window.removeEventListener('message', onFrameMessage);
  if (saveWait) {
    clearTimeout(saveWait.timer);
    saveWait.resolve(false);
    saveWait = null;
  }
});

defineExpose({ validate, refresh: refreshFlag, saveDesigner });
</script>

<style scoped>
.sf-process-design {
  min-height: calc(100vh - 140px);
  display: flex;
  flex-direction: column;
}
.sf-process-design__alert {
  margin: 12px auto;
  max-width: 560px;
}
.sf-process-design__bar {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px 12px;
}
.sf-process-design__frame {
  flex: 1;
  width: 100%;
  min-height: calc(100vh - 180px);
  border: 1px solid #ebeef5;
  border-radius: 4px;
  background: #fff;
}
.sf-process-design__placeholder {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  color: #606266;
  border: 1px dashed #dcdfe6;
  margin: 12px;
  border-radius: 8px;
  min-height: 320px;
}
</style>
