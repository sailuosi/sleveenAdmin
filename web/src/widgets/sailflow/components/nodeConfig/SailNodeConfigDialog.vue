<template>
  <el-dialog
    v-model="visible"
    :title="dialogTitle"
    width="720px"
    top="4vh"
    destroy-on-close
    append-to-body
    class="sf-node-config-dialog"
    @closed="onClosed"
  >
    <SailNodeConfigPanel
      v-if="model"
      v-model="model"
      :users="userAccounts"
      @pick-performer="openPerformer"
      @pick-form="openForm"
      @pick-notify="openNotify"
      @edit-resolve-expression="openResolveExpression"
      @edit-variables="openVariables"
    />
    <template #footer>
      <el-button @click="visible = false">取消</el-button>
      <el-button type="primary" :loading="saving" @click="onSave">保存到节点</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { ElMessage } from 'element-plus';
import { designerEvents } from '@sail/bpmn/ui/designerEvents';
import { loadNodeConfig, saveNodeConfig } from '@sail/bpmn/nodeConfig/loadSave';
import type { SailNodeConfigModel } from '@sail/bpmn/nodeConfig/types';
import { getUserAll, unwrapList, type UserRow } from '@sail/api/resource';
import {
  getCachedVariables,
  loadVariablesForActivity,
  setCachedVariables,
} from '@sail/bpmn/variable/wfVariableCache';
import SailNodeConfigPanel from './SailNodeConfigPanel.vue';

defineOptions({ name: 'SailNodeConfigDialog' });

const visible = ref(false);
const saving = ref(false);
const model = ref<SailNodeConfigModel | null>(null);
const userAccounts = ref<string[]>([]);
let element: unknown = null;
let modeling: { updateProperties: (el: unknown, props: object) => void } | null = null;
let moddle: { create: (type: string, props?: object) => any } | null = null;

const dialogTitle = computed(() => {
  const n = model.value?.name || model.value?.id || '';
  return n ? `节点配置 · ${n}` : '节点配置';
});

function open(payload: {
  element: unknown;
  modeling: { updateProperties: (el: unknown, props: object) => void };
  moddle: { create: (type: string, props?: object) => any };
}) {
  element = payload.element;
  modeling = payload.modeling;
  moddle = payload.moddle;
  model.value = loadNodeConfig(payload.element);
  visible.value = true;
  void loadUsers();
}

async function loadUsers() {
  try {
    const data = await getUserAll();
    const list = unwrapList<UserRow>(data);
    userAccounts.value = list
      .map(u =>
        String(
          (u as any).Account ||
            (u as any).account ||
            (u as any).UserId ||
            (u as any).userId ||
            ''
        )
      )
      .filter(Boolean);
  } catch {
    userAccounts.value = [];
  }
}

function openPerformer() {
  if (!model.value) return;
  designerEvents.emit('performer:open', {
    element,
    onConfirm: draft => {
      if (!model.value) return;
      if (model.value.performers.some(p => p.outerId === draft.outerId)) return;
      model.value.performers.push({ ...draft });
    }
  });
}

function openForm() {
  if (!model.value) return;
  designerEvents.emit('form:open', {
    element,
    onConfirm: draft => {
      if (!model.value) return;
      if (model.value.forms.some(f => f.outerId === draft.outerId)) {
        ElMessage.warning('表单已绑定');
        return;
      }
      model.value.forms.push({ ...draft });
    }
  });
}

function openNotify() {
  if (!model.value) return;
  designerEvents.emit('notification:open', {
    element,
    onConfirm: draft => {
      if (!model.value) return;
      if (model.value.notifications.some(n => n.outerId === draft.outerId)) return;
      model.value.notifications.push({ ...draft });
    }
  });
}

async function openVariables() {
  if (!model.value) return;
  const activityId = model.value.id;
  let rows = getCachedVariables(activityId);
  if (!rows) {
    try {
      rows = await loadVariablesForActivity(activityId);
    } catch {
      rows = [];
    }
  }
  designerEvents.emit('variable:open', {
    activityId,
    rows: [...(rows || [])],
    onSaved: next => {
      setCachedVariables(activityId, next);
    },
  });
}

function openResolveExpression() {
  if (!model.value) return;
  designerEvents.emit('expression:open', {
    value: (() => {
      try {
        const p = JSON.parse(model.value!.assigneeResolveParams || '{}');
        return String(p.expression || '');
      } catch {
        return '';
      }
    })(),
    onConfirm: (expr: string) => {
      if (!model.value) return;
      let p: Record<string, unknown> = {};
      try {
        p = JSON.parse(model.value.assigneeResolveParams || '{}') || {};
      } catch {
        p = {};
      }
      p.expression = expr;
      model.value.assigneeResolveParams = JSON.stringify(p);
    }
  });
}

function onSave() {
  if (!model.value || !element || !modeling || !moddle) return;
  saving.value = true;
  try {
    saveNodeConfig(element, model.value, modeling, moddle);
    ElMessage.success('已写入节点扩展');
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
  designerEvents.on('nodeconfig:open', open);
});
onBeforeUnmount(() => {
  designerEvents.off('nodeconfig:open', open);
});
</script>
