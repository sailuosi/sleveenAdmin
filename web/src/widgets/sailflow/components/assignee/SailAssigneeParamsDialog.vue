<template>
  <el-dialog
    v-model="visible"
    :title="title"
    width="560px"
    append-to-body
    destroy-on-close
    @closed="onClosed"
  >
    <el-form label-position="top">
      <el-form-item label="Type">
        <el-input :model-value="type" disabled />
      </el-form-item>
      <SailAssigneeParamsForm
        v-if="visible"
        v-model="params"
        :kind="kind"
        :type="type"
        @edit-expression="openExpr"
      />
    </el-form>
    <template #footer>
      <el-button @click="visible = false">取消</el-button>
      <el-button type="primary" @click="onOk">确定</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { designerEvents } from '@sail/bpmn/ui/designerEvents';
import SailAssigneeParamsForm from './SailAssigneeParamsForm.vue';

defineOptions({ name: 'SailAssigneeParamsDialog' });

const visible = ref(false);
const kind = ref<'resolve' | 'leave'>('resolve');
const type = ref('');
const params = ref('');
let onConfirm: ((json: string) => void) | null = null;

const title = computed(() =>
  kind.value === 'resolve' ? '定范围 Params' : '办理模式 Params'
);

function open(payload: {
  kind: 'resolve' | 'leave';
  type: string;
  params: string;
  onConfirm: (paramsJson: string) => void;
}) {
  kind.value = payload.kind;
  type.value = payload.type || '';
  params.value = payload.params || '';
  onConfirm = payload.onConfirm;
  visible.value = true;
}

function openExpr() {
  designerEvents.emit('expression:open', {
    value: (() => {
      try {
        return String(JSON.parse(params.value || '{}').expression || '');
      } catch {
        return '';
      }
    })(),
    onConfirm: (expr: string) => {
      let p: Record<string, unknown> = {};
      try {
        p = JSON.parse(params.value || '{}') || {};
      } catch {
        p = {};
      }
      p.expression = expr;
      params.value = JSON.stringify(p);
    }
  });
}

function onOk() {
  onConfirm?.(params.value || '');
  visible.value = false;
}

function onClosed() {
  onConfirm = null;
}

onMounted(() => designerEvents.on('assignee-params:open', open));
onBeforeUnmount(() => designerEvents.off('assignee-params:open', open));
</script>
