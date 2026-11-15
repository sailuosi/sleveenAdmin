<template>
  <div v-if="show" class="sf-preselect-hint">
    <el-alert
      type="success"
      :closable="false"
      show-icon
      :title="title"
      :description="desc"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { probePreselectNext, type PreselectNextProbeDto } from '@sail/api/preselect';
import { pickField, unpackSailResult } from '@sail/utils/sailResult';

defineOptions({ name: 'SailPreselectApproveHint' });

const props = withDefaults(
  defineProps<{
    processId?: string;
    version?: string;
    processInstanceId?: string;
    activityId?: string;
  }>(),
  {
    processId: '',
    version: '1',
    processInstanceId: '',
    activityId: ''
  }
);

const emit = defineEmits<{
  'update:skipManualPick': [boolean];
  'update:probe': [PreselectNextProbeDto | null];
}>();

const probe = ref<PreselectNextProbeDto | null>(null);
const loading = ref(false);

const show = computed(
  () => !!probe.value?.IsPreselectTarget && !!probe.value?.SkipManualPick
);
const title = computed(() => '下一跳已预选办理人');
const desc = computed(() => {
  const list = probe.value?.Performers || [];
  if (!list.length) return '';
  return list
    .map(p => p.UserName || p.UserId)
    .filter(Boolean)
    .join('、');
});

watch(
  () =>
    [props.processId, props.version, props.processInstanceId, props.activityId] as const,
  () => {
    void refresh();
  },
  { immediate: true }
);

async function refresh() {
  probe.value = null;
  emit('update:skipManualPick', false);
  emit('update:probe', null);
  if (!props.activityId?.trim()) return;

  loading.value = true;
  try {
    const data = await probePreselectNext({
      processId: props.processId || undefined,
      version: props.version || '1',
      processInstanceId: props.processInstanceId || undefined,
      activityId: props.activityId
    });
    const r = unpackSailResult<PreselectNextProbeDto>(data);
    if ((data?.Status ?? data?.status) === -1) return;
    const raw = (r.entity || {}) as Record<string, unknown>;
    const dto: PreselectNextProbeDto = {
      ActivityId: String(pickField(raw, 'ActivityId', 'activityId') || props.activityId),
      IsPreselectTarget: !!(pickField(raw, 'IsPreselectTarget', 'isPreselectTarget') ?? false),
      HasPerformers: !!(pickField(raw, 'HasPerformers', 'hasPerformers') ?? false),
      SkipManualPick: !!(pickField(raw, 'SkipManualPick', 'skipManualPick') ?? false),
      Performers: (pickField(raw, 'Performers', 'performers') as PreselectNextProbeDto['Performers']) || []
    };
    probe.value = dto;
    emit('update:skipManualPick', !!dto.SkipManualPick);
    emit('update:probe', dto);
  } catch {
    /* 忽略：办理条不因预选探测失败阻断 */
  } finally {
    loading.value = false;
  }
}

defineExpose({ refresh, probe, loading });
</script>

<style scoped>
.sf-preselect-hint {
  margin-bottom: 10px;
}
</style>
