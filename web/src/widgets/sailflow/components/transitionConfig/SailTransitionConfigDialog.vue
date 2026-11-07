<template>
  <el-dialog
    v-model="visible"
    :title="dialogTitle"
    width="640px"
    top="6vh"
    destroy-on-close
    append-to-body
    class="sf-transition-config-dialog"
    @closed="onClosed"
  >
    <SailTransitionConfigPanel
      v-if="model"
      v-model="model"
      @edit-expression="openExpression"
    />
    <template #footer>
      <el-button @click="visible = false">取消</el-button>
      <el-button type="primary" :loading="saving" @click="onSave">保存到连接线</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { ElMessage } from 'element-plus';
import { designerEvents } from '@sail/bpmn/ui/designerEvents';
import { loadTransitionConfig, saveTransitionConfig } from '@sail/bpmn/transitionConfig/loadSave';
import type { SailTransitionConfigModel } from '@sail/bpmn/transitionConfig/types';
import SailTransitionConfigPanel from './SailTransitionConfigPanel.vue';

defineOptions({ name: 'SailTransitionConfigDialog' });

const visible = ref(false);
const saving = ref(false);
const model = ref<SailTransitionConfigModel | null>(null);
let element: unknown = null;
let modeling: { updateProperties: (el: unknown, props: object) => void } | null = null;
let moddle: { create: (type: string, props?: object) => any } | null = null;

const dialogTitle = computed(() => {
  const n = model.value?.name || model.value?.id || '';
  return n ? `连接线配置 · ${n}` : '连接线配置';
});

function open(payload: {
  element: unknown;
  modeling: { updateProperties: (el: unknown, props: object) => void };
  moddle: { create: (type: string, props?: object) => any };
}) {
  element = payload.element;
  modeling = payload.modeling;
  moddle = payload.moddle;
  model.value = loadTransitionConfig(payload.element);
  visible.value = true;
}

function openExpression() {
  if (!model.value) return;
  designerEvents.emit('expression:open', {
    value: model.value.conditionExpression || '',
    onConfirm: (expr: string) => {
      if (!model.value) return;
      model.value.conditionExpression = expr;
      model.value.flowType = 'condition';
    }
  });
}

function onSave() {
  if (!model.value || !element || !modeling || !moddle) return;
  saving.value = true;
  try {
    saveTransitionConfig(element, model.value, modeling, moddle);
    ElMessage.success('已写入连接线配置');
    visible.value = false;
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  } finally {
    saving.value = false;
  }
}

function onClosed() {
  model.value = null;
  element = null;
  modeling = null;
  moddle = null;
}

onMounted(() => {
  designerEvents.on('transitionconfig:open', open);
});
onBeforeUnmount(() => {
  designerEvents.off('transitionconfig:open', open);
});
</script>
