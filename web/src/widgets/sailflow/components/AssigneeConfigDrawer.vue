<template>
  <el-drawer v-model="visible" title="单位审批人配置" size="480px" destroy-on-close>
    <div v-loading="loading">
      <el-alert
        type="info"
        :closable="false"
        title="仅 UNIT 流程可配；可用策略：users / role / unitMap / jobRelative / deptBoss"
        style="margin-bottom: 12px"
      />
      <el-empty v-if="!nodes.length" description="未解析到用户任务节点" />
      <div v-for="n in nodes" :key="n.ActivityId" class="sf-assignee-node">
        <div class="sf-assignee-node__title">{{ n.ActivityName || n.ActivityId }}</div>
        <el-select v-model="n.Strategy" style="width: 100%; margin-bottom: 8px">
          <el-option v-for="s in strategies" :key="s" :label="s" :value="s" />
          <el-option label="（半残预留）script" value="script" disabled />
        </el-select>
        <el-input
          v-model="n.ConfigJson"
          type="textarea"
          :rows="2"
          placeholder="策略配置 JSON（可选）"
        />
      </div>
    </div>
    <template #footer>
      <el-button @click="visible = false">取消</el-button>
      <el-button type="primary" :loading="saving" @click="save">保存</el-button>
    </template>
  </el-drawer>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { ElMessage } from 'element-plus';
import { getAssigneeConfig, saveAssigneeConfig } from '@sail/api/unitScope';
import { unpackSailResult, pickField } from '@sail/utils/sailResult';

defineOptions({ name: 'AssigneeConfigDrawer' });

const visible = ref(false);
const loading = ref(false);
const saving = ref(false);
const processId = ref('');
const version = ref('1');
const strategies = ref<string[]>(['users', 'role', 'unitMap', 'jobRelative', 'deptBoss']);
const nodes = ref<Array<{ ActivityId: string; ActivityName: string; Strategy: string; ConfigJson?: string }>>([]);

async function open(opts: { processId: string; version?: string }) {
  processId.value = opts.processId;
  version.value = opts.version || '1';
  visible.value = true;
  loading.value = true;
  try {
    const raw = await getAssigneeConfig({ ProcessId: processId.value, Version: version.value });
    const data = unpackSailResult(raw);
    const entity = (pickField(data, 'Entity', 'entity') || data) as Record<string, unknown>;
    nodes.value = (pickField(entity, 'Nodes', 'nodes') as typeof nodes.value) || [];
    const ctxStrategies = pickField(entity, 'AvailableStrategies') as string[] | undefined;
    if (Array.isArray(ctxStrategies) && ctxStrategies.length) strategies.value = ctxStrategies;
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
    nodes.value = [];
  } finally {
    loading.value = false;
  }
}

async function save() {
  saving.value = true;
  try {
    const raw = await saveAssigneeConfig({
      ProcessId: processId.value,
      Version: version.value,
      Nodes: nodes.value.map((n) => ({
        ActivityId: n.ActivityId,
        Strategy: n.Strategy,
        ConfigJson: n.ConfigJson
      }))
    });
    const data = unpackSailResult(raw);
    if ((data as { Status?: number })?.Status === -1) {
      ElMessage.error((data as { Message?: string })?.Message || '保存失败');
      return;
    }
    ElMessage.success('已保存');
    visible.value = false;
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  } finally {
    saving.value = false;
  }
}

defineExpose({ open });
</script>

<style scoped>
.sf-assignee-node {
  margin-bottom: 16px;
  padding-bottom: 12px;
  border-bottom: 1px solid var(--el-border-color-lighter);
}
.sf-assignee-node__title {
  font-weight: 600;
  margin-bottom: 8px;
}
</style>
