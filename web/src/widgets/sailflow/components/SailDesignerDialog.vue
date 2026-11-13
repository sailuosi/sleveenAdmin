<template>
  <el-dialog
    v-model="visible"
    fullscreen
    append-to-body
    destroy-on-close
    :show-close="false"
    :close-on-click-modal="false"
    class="sf-designer-dialog"
    @closed="onClosed"
  >
    <SailDesignerPage
      v-if="ready"
      :key="pageKey"
      embedded
      :edit-process-id="editProcessId"
      :edit-version="editVersion"
      @close="visible = false"
      @saved="emit('saved')"
    />
  </el-dialog>
</template>

<script setup lang="ts">
import { nextTick, ref } from 'vue';
import SailDesignerPage from '@sail/views/SailDesignerPage.vue';

defineOptions({ name: 'SailDesignerDialog' });

const emit = defineEmits<{ saved: [] }>();

const visible = ref(false);
const ready = ref(false);
const editProcessId = ref('');
const editVersion = ref('1');
const pageKey = ref('');

async function open(opts?: { processId?: string; version?: string }) {
  const pid = String(opts?.processId || '').trim();
  editProcessId.value = pid;
  editVersion.value = String(opts?.version || '1');
  pageKey.value = `${pid || 'new'}@${editVersion.value}@${Date.now()}`;
  visible.value = true;
  await nextTick();
  ready.value = true;
}

function onClosed() {
  ready.value = false;
  editProcessId.value = '';
  editVersion.value = '1';
  pageKey.value = '';
}

defineExpose({ open });
</script>

<style>
.sf-designer-dialog.el-dialog {
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}
.sf-designer-dialog .el-dialog__header {
  display: none;
  padding: 0;
  margin: 0;
}
.sf-designer-dialog .el-dialog__body {
  flex: 1 1 auto;
  height: 100%;
  min-height: 0;
  padding: 0;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}
.sf-designer-dialog .el-dialog__body > * {
  flex: 1 1 auto;
  min-height: 0;
  height: 100%;
}
</style>
