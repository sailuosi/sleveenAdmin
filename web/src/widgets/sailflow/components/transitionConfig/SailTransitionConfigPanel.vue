<template>
  <div class="sf-transition-config">
    <el-form label-position="top" size="default">
      <el-card shadow="never" class="sf-transition-config__card">
        <template #header><span class="sf-transition-config__h">基本信息</span></template>
        <el-form-item label="连接线名称">
          <el-input v-model="model.name" clearable placeholder="可选显示名" />
        </el-form-item>
        <el-form-item label="连接线 Id">
          <el-input :model-value="model.id" disabled />
        </el-form-item>
        <el-row :gutter="12">
          <el-col :span="12">
            <el-form-item label="源节点">
              <el-input :model-value="model.context.sourceLabel || '—'" disabled />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="目标节点">
              <el-input :model-value="model.context.targetLabel || '—'" disabled />
            </el-form-item>
          </el-col>
        </el-row>
      </el-card>

      <el-card shadow="never" class="sf-transition-config__card">
        <template #header><span class="sf-transition-config__h">流转条件</span></template>
        <el-form-item label="流转类型">
          <el-radio-group v-model="model.flowType">
            <el-radio value="normal">普通路径</el-radio>
            <el-radio value="default">默认路径</el-radio>
            <el-radio value="condition">条件路径</el-radio>
          </el-radio-group>
        </el-form-item>
        <template v-if="model.flowType === 'condition'">
          <el-form-item label="条件表达式">
            <el-input
              v-model="model.conditionExpression"
              type="textarea"
              :rows="3"
              placeholder="例 ${approved == true}"
            />
          </el-form-item>
          <el-button type="primary" link @click="emit('edit-expression')">可视化编辑</el-button>
        </template>
        <el-alert
          v-else-if="model.flowType === 'default'"
          type="info"
          :closable="false"
          show-icon
          title="默认路径：作为源节点的 BPMN default 出边；条件表达式会被清除"
          class="mt"
        />
      </el-card>

      <el-card shadow="never" class="sf-transition-config__card">
        <template #header><span class="sf-transition-config__h">转移行为</span></template>
        <el-form-item label="接收者类型">
          <el-select v-model="model.recieverType" style="width: 100%">
            <el-option
              v-for="o in receiverOptions"
              :key="o.value"
              :label="o.label"
              :value="o.value"
            />
          </el-select>
        </el-form-item>

        <el-form-item v-if="model.context.showApproval" label="审批选项">
          <el-select v-model="model.approval" style="width: 100%">
            <el-option
              v-for="o in approvalOptions"
              :key="o.value"
              :label="o.label"
              :value="o.value"
            />
          </el-select>
        </el-form-item>

        <el-form-item v-if="model.context.showPriority" label="优先级">
          <el-input v-model="model.priority" clearable placeholder="数字越小优先级越高" />
        </el-form-item>

        <el-form-item v-if="model.context.showDefaultBranch" label="默认分支">
          <el-switch v-model="model.defaultBranch" />
          <div class="sf-transition-config__hint">包容网关拆分时的 sf 默认分支标记</div>
        </el-form-item>

        <el-form-item v-if="model.context.showForcedMerge" label="强制合并分支">
          <el-switch v-model="model.forcedMerge" />
          <div class="sf-transition-config__hint">或合并时强制通过该分支</div>
        </el-form-item>

        <el-alert
          v-if="!hasGatewayExtras"
          type="info"
          :closable="false"
          show-icon
          title="网关相关选项（审批/优先级/默认分支/强制合并）会在源或目标为对应网关时自动出现"
        />
      </el-card>
    </el-form>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import {
  APPROVAL_OPTIONS,
  RECEIVER_OPTIONS,
  type SailTransitionConfigModel
} from '@sail/bpmn/transitionConfig/types';

defineOptions({ name: 'SailTransitionConfigPanel' });

const model = defineModel<SailTransitionConfigModel>({ required: true });

defineEmits<{
  'edit-expression': [];
}>();

const receiverOptions = RECEIVER_OPTIONS;
const approvalOptions = APPROVAL_OPTIONS;

const hasGatewayExtras = computed(() => {
  const c = model.value.context;
  return c.showApproval || c.showPriority || c.showDefaultBranch || c.showForcedMerge;
});
</script>

<style scoped>
.sf-transition-config {
  display: flex;
  flex-direction: column;
  gap: 12px;
  max-height: min(72vh, 640px);
  overflow: auto;
  padding-right: 4px;
}
.sf-transition-config__card {
  border-radius: 8px;
}
.sf-transition-config__h {
  font-weight: 600;
  font-size: 14px;
}
.sf-transition-config__hint {
  margin-top: 4px;
  font-size: 12px;
  color: var(--el-text-color-secondary);
}
.sf-transition-config .mt {
  margin-top: 8px;
}
</style>
