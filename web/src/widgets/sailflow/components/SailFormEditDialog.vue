<template>
  <el-dialog
    v-model="visible"
    fullscreen
    append-to-body
    destroy-on-close
    :show-close="false"
    :close-on-click-modal="false"
    class="sf-form-edit-dialog"
    @closed="onClosed"
  >
    <SailFormEditorPage
      v-if="ready"
      :key="pageKey"
      embedded
      :edit-form-id="editFormId"
      @close="visible = false"
      @saved="emit('saved')"
    />
  </el-dialog>
</template>

<script setup lang="ts">
import { nextTick, ref } from 'vue';
import SailFormEditorPage from '@sail/views/form/SailFormEditorPage.vue';

defineOptions({ name: 'SailFormEditDialog' });

const emit = defineEmits<{ saved: [] }>();

const visible = ref(false);
const ready = ref(false);
const editFormId = ref('');
const pageKey = ref('');

async function open(opts?: { formId?: string }) {
  const id = String(opts?.formId || '').trim();
  editFormId.value = id;
  pageKey.value = `${id || 'new'}@${Date.now()}`;
  visible.value = true;
  await nextTick();
  ready.value = true;
}

function onClosed() {
  ready.value = false;
  editFormId.value = '';
  pageKey.value = '';
}

defineExpose({ open });
</script>

<style>
.sf-form-edit-dialog.el-dialog {
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}
.sf-form-edit-dialog .el-dialog__header {
  display: none;
  padding: 0;
  margin: 0;
}
.sf-form-edit-dialog .el-dialog__body {
  flex: 1 1 auto;
  height: 100%;
  min-height: 0;
  padding: 0;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}
.sf-form-edit-dialog .el-dialog__body > * {
  flex: 1 1 auto;
  min-height: 0;
  height: 100%;
}
</style>
