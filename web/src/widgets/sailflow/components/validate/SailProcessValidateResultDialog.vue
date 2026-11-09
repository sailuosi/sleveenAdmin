<template>
  <el-dialog
    v-model="visible"
    title="流程校验结果"
    width="720px"
    append-to-body
    destroy-on-close
    class="sf-validate-dialog"
    @closed="onClosed"
  >
    <el-alert
      v-if="result"
      :title="result.summary"
      :type="result.passed ? 'success' : 'error'"
      show-icon
      :closable="false"
      class="sf-validate-dialog__summary"
    />

    <div v-if="displayMessages.length" class="sf-validate-dialog__list">
      <div
        v-for="(msg, i) in displayMessages"
        :key="i"
        class="sf-validate-dialog__item"
      >
        <div class="sf-validate-dialog__item-head">
          <el-tag size="small" :type="levelTagType(msg.level)" effect="plain">
            {{ msg.level || 'Info' }}
          </el-tag>
          <el-tag size="small" type="info" effect="plain">{{ msg.code || '—' }}</el-tag>
          <span v-if="msg.activityType" class="sf-validate-dialog__meta">{{ msg.activityType }}</span>
        </div>
        <div class="sf-validate-dialog__text">{{ msg.text }}</div>
        <div
          v-if="msg.code === 'NextHop' && (msg.assigneeResolveType || msg.leaveJudgeType)"
          class="sf-validate-dialog__assignee"
        >
          待办人：Resolve={{ msg.assigneeResolveType || '—' }}
          / Leave={{ msg.leaveJudgeType || '—' }}
        </div>
        <div
          v-if="msg.code === 'NextHop' && msg.nextActivityNames?.length"
          class="sf-validate-dialog__next"
        >
          后续节点：{{ msg.nextActivityNames.join('、') }}
          <template v-if="msg.nextAssigneeSummary">
            <br />
            后续待办人：{{ msg.nextAssigneeSummary }}
          </template>
        </div>
      </div>
    </div>
    <el-empty v-else description="无详细消息" :image-size="64" />

    <template #footer>
      <el-button v-if="result && !result.passed" @click="onRepair">去修复</el-button>
      <el-button type="primary" @click="visible = false">关闭</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { designerEvents } from '@sail/bpmn/ui/designerEvents';
import type { ParsedValidateResult, ProcessValidateMessage } from '@sail/api/processValidate';

defineOptions({ name: 'SailProcessValidateResultDialog' });

const visible = ref(false);
const result = ref<ParsedValidateResult | null>(null);
let onRepairCb: (() => void) | null = null;

/** Summary 置顶，其余保持原序 */
const displayMessages = computed(() => {
  const list = result.value?.messages ?? [];
  const summary = list.filter(m => m.code === 'Summary');
  const rest = list.filter(m => m.code !== 'Summary');
  return [...summary, ...rest] as ProcessValidateMessage[];
});

function levelTagType(level: string): 'success' | 'warning' | 'danger' | 'info' {
  const l = (level || '').toLowerCase();
  if (l === 'error') return 'danger';
  if (l === 'warning') return 'warning';
  if (l === 'info') return 'info';
  return 'info';
}

function open(payload: {
  result: ParsedValidateResult;
  onRepair?: () => void;
}) {
  result.value = payload.result;
  onRepairCb = payload.onRepair ?? null;
  visible.value = true;
}

function onClosed() {
  result.value = null;
  onRepairCb = null;
}

function onRepair() {
  visible.value = false;
  onRepairCb?.();
}

onMounted(() => {
  designerEvents.on('validate:open', open);
});
onBeforeUnmount(() => {
  designerEvents.off('validate:open', open);
});
</script>

<style scoped>
.sf-validate-dialog__summary {
  margin-bottom: 12px;
}
.sf-validate-dialog__list {
  max-height: 420px;
  overflow: auto;
  display: flex;
  flex-direction: column;
  gap: 10px;
}
.sf-validate-dialog__item {
  padding: 10px 12px;
  border: 1px solid var(--el-border-color-lighter);
  border-radius: 6px;
  background: var(--el-fill-color-blank);
}
.sf-validate-dialog__item-head {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-bottom: 6px;
}
.sf-validate-dialog__meta {
  font-size: 12px;
  color: var(--el-text-color-secondary);
}
.sf-validate-dialog__text {
  font-size: 13px;
  line-height: 1.5;
  word-break: break-word;
}
.sf-validate-dialog__assignee,
.sf-validate-dialog__next {
  margin-top: 4px;
  font-size: 12px;
  color: var(--el-text-color-secondary);
  line-height: 1.45;
}
</style>
