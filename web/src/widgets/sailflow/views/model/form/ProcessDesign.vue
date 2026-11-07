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
        <el-button @click="refreshFlag">刷新状态</el-button>
      </div>
      <iframe
        v-if="showFrame"
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
import { computed, ref } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage } from 'element-plus';
import { queryProcessFile } from '@sail/api/process';
import { unpackSailResult, pickField } from '@sail/utils/sailResult';
import type { ProcessEditFormModel } from './types';

defineOptions({ name: 'SailProcessDesignStep' });

const model = defineModel<ProcessEditFormModel>({ required: true });
const router = useRouter();
const showFrame = ref(true);

const designerUrl = computed(() => {
  const q = new URLSearchParams({
    processId: String(model.value.processId || ''),
    version: String(model.value.version || '1'),
    embed: '1'
  });
  return `${router.resolve({ path: '/sail/designer' }).href}?${q.toString()}`;
});

function openDesigner() {
  if (!model.value.processId) return;
  window.open(designerUrl.value, '_blank');
}

async function refreshFlag() {
  if (!model.value.processId) return;
  try {
    const data = await queryProcessFile({
      ProcessId: model.value.processId,
      Version: model.value.version || '1'
    });
    const r = unpackSailResult<Record<string, unknown>>(data);
    const xml = String(pickField(r.entity, 'XmlContent', 'xmlContent') || '');
    model.value.hasBpmn = xml.includes('<process') || xml.includes('<bpmn');
    ElMessage.success(model.value.hasBpmn ? '已检测到流程图' : '仍为初始模板');
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  }
}

async function validate() {
  if (!model.value.processId) {
    throw new Error('请先保存流程基本信息');
  }
  // 新建会带默认开始节点 XML；此处只要求定义已落库，复杂节点在设计器内完成
  await refreshFlag();
}

defineExpose({ validate, refresh: refreshFlag });
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
