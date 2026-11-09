<template>
  <el-dialog
    v-model="visible"
    :title="title"
    width="520px"
    top="8vh"
    append-to-body
    destroy-on-close
    @closed="onClosed"
  >
    <el-form v-if="model" label-position="top">
      <el-form-item label="名称">
        <el-input v-model="model.name" clearable />
      </el-form-item>
      <el-form-item label="Id">
        <el-input :model-value="model.id" disabled />
      </el-form-item>
      <el-form-item v-if="model.type === 'bpmn:Process'" label="可执行">
        <el-switch v-model="model.isExecutable" />
      </el-form-item>
      <el-form-item v-if="model.type === 'bpmn:Participant'" label="流程标识">
        <el-input :model-value="model.processRef || ''" disabled />
      </el-form-item>
      <el-form-item label="文档说明">
        <el-input v-model="model.documentation" type="textarea" :rows="4" clearable />
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="visible = false">取消</el-button>
      <el-button type="primary" :loading="saving" @click="onSave">保存</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { ElMessage } from 'element-plus';
import { designerEvents } from '@sail/bpmn/ui/designerEvents';
import { loadBasicProps, saveBasicProps } from '@sail/bpmn/elementConfig/loadSave';
import type { SailBasicPropsModel } from '@sail/bpmn/elementConfig/types';

defineOptions({ name: 'SailBasicPropsDialog' });

const visible = ref(false);
const saving = ref(false);
const model = ref<SailBasicPropsModel | null>(null);
let element: unknown = null;
let modeling: { updateProperties: (el: unknown, props: object) => void } | null = null;

const title = computed(() => {
  const t = model.value?.type?.replace(/^bpmn:/, '') || '元素';
  const n = model.value?.name || model.value?.id || '';
  return n ? `属性 · ${t} · ${n}` : `属性 · ${t}`;
});

function open(payload: {
  element: unknown;
  modeling: { updateProperties: (el: unknown, props: object) => void };
  moddle: { create: (type: string, props?: object) => any };
}) {
  element = payload.element;
  modeling = payload.modeling;
  model.value = loadBasicProps(payload.element);
  visible.value = true;
}

function onSave() {
  if (!model.value || !element || !modeling) return;
  saving.value = true;
  try {
    saveBasicProps(element, model.value, modeling);
    ElMessage.success('已写入属性');
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
}

onMounted(() => {
  designerEvents.on('basicconfig:open', open);
});
onBeforeUnmount(() => {
  designerEvents.off('basicconfig:open', open);
});
</script>
