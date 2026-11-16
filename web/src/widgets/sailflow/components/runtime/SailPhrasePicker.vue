<template>
  <div class="sf-phrase-picker">
    <el-button
      v-for="p in phrases"
      :key="p"
      link
      :type="tone"
      size="small"
      @click="emit('pick', p)"
    >
      {{ p }}
    </el-button>
    <el-button
      v-if="showManage"
      link
      type="info"
      size="small"
      @click="emit('manage')"
    >
      管理
    </el-button>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { listMyApprovalPhrases } from '@sail/api/approvalPhrase';
import { pickField, unpackSailResult } from '@sail/utils/sailResult';

defineOptions({ name: 'SailPhrasePicker' });

const props = withDefaults(
  defineProps<{
    /** approve | reject */
    kind?: 'approve' | 'reject';
    fallback?: string[];
    showManage?: boolean;
    tone?: 'primary' | 'danger';
  }>(),
  {
    kind: 'approve',
    fallback: () => [],
    showManage: true,
    tone: 'primary'
  }
);

const emit = defineEmits<{
  pick: [string];
  manage: [];
}>();

const mine = ref<string[]>([]);

const defaultApprove = ['同意', '拟同意', '情况属实', '请按制度办理'];
const defaultReject = ['不同意', '请补充材料', '退回修改', '不符合要求'];

const phrases = computed(() => {
  if (mine.value.length) return mine.value;
  if (props.fallback?.length) return props.fallback;
  return props.kind === 'reject' ? defaultReject : defaultApprove;
});

async function reload() {
  try {
    const data = await listMyApprovalPhrases(true);
    const r = unpackSailResult<Record<string, unknown>[]>(data);
    const list = Array.isArray(r.entity) ? r.entity : [];
    mine.value = list
      .map(x => String(pickField(x, 'Content', 'content') || '').trim())
      .filter(Boolean);
  } catch {
    mine.value = [];
  }
}

onMounted(() => {
  void reload();
});

watch(
  () => props.kind,
  () => {
    /* keep mine shared for both kinds */
  }
);

defineExpose({ reload });
</script>

<style scoped>
.sf-phrase-picker {
  display: flex;
  flex-wrap: wrap;
  gap: 4px 8px;
  margin-top: 6px;
}
</style>
