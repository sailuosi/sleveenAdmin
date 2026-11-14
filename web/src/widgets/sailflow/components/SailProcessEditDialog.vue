<template>
  <el-dialog
    v-model="visible"
    fullscreen
    append-to-body
    destroy-on-close
    :show-close="false"
    :close-on-click-modal="false"
    class="sf-process-edit-dialog"
    @closed="onClosed"
  >
    <SailProcessEditPage
      v-if="ready"
      :key="pageKey"
      embedded
      :edit-type="editType"
      :edit-process-id="editProcessId"
      :edit-version="editVersion"
      @close="visible = false"
      @saved="emit('saved')"
    />
  </el-dialog>
</template>

<script setup lang="ts">
import { nextTick, ref } from 'vue';
import { ElMessage } from 'element-plus';
import SailProcessEditPage from '@sail/views/model/form/SailProcessEditPage.vue';

defineOptions({ name: 'SailProcessEditDialog' });

const emit = defineEmits<{ saved: [] }>();

type EditType = 'create' | 'update' | 'copy';

const visible = ref(false);
const ready = ref(false);
const editType = ref<EditType>('update');
const editProcessId = ref('');
const editVersion = ref('1');
const pageKey = ref('');

async function open(opts?: {
  type?: EditType;
  processId?: string;
  version?: string;
}) {
  const type: EditType = opts?.type || (opts?.processId ? 'update' : 'create');
  const pid = String(opts?.processId || '').trim();
  if (type !== 'create' && !pid) {
    ElMessage.error('缺少 processId，无法打开弹窗');
    return;
  }
  editType.value = type;
  editProcessId.value = pid;
  editVersion.value = String(opts?.version || '1');
  pageKey.value = `${type}@${pid || 'new'}@${editVersion.value}@${Date.now()}`;
  visible.value = true;
  await nextTick();
  ready.value = true;
}

function onClosed() {
  ready.value = false;
  editType.value = 'update';
  editProcessId.value = '';
  editVersion.value = '1';
  pageKey.value = '';
}

defineExpose({ open });
</script>

<style>
.sf-process-edit-dialog.el-dialog {
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}
.sf-process-edit-dialog .el-dialog__header {
  display: none;
  padding: 0;
  margin: 0;
}
.sf-process-edit-dialog .el-dialog__body {
  flex: 1 1 auto;
  height: 100%;
  min-height: 0;
  padding: 0;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}
.sf-process-edit-dialog .el-dialog__body > * {
  flex: 1 1 auto;
  min-height: 0;
  height: 100%;
}
</style>
