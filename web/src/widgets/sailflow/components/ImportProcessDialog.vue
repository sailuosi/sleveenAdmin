<template>
  <el-dialog
    v-model="visible"
    title="导入流程"
    width="640px"
    destroy-on-close
    @closed="onClosed"
  >
    <el-tabs v-model="mode">
      <el-tab-pane label="粘贴 XML" name="paste">
        <el-input
          v-model="xmlText"
          type="textarea"
          :rows="14"
          placeholder="粘贴 BPMN / XML 内容…"
          class="sf-import__ta"
        />
      </el-tab-pane>
      <el-tab-pane label="上传文件" name="file">
        <el-upload
          drag
          :auto-upload="false"
          :limit="1"
          accept=".xml,.bpmn,.bpmn2,.txt,.xpdl"
          :on-change="onFileChange"
          :on-remove="() => (picked = null)"
        >
          <div class="el-upload__text">拖拽文件到此处，或<em>点击选择</em></div>
          <template #tip>
            <div class="el-upload__tip">支持 xml / bpmn / txt，单文件</div>
          </template>
        </el-upload>
      </el-tab-pane>
    </el-tabs>
    <template #footer>
      <el-button @click="visible = false">取消</el-button>
      <el-button type="primary" :loading="saving" @click="confirm">导入</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { ElMessage, type UploadFile } from 'element-plus';
import {
  importFromFile,
  importFromText,
  importMessage,
  isImportOk
} from '@sail/api/upload';

defineOptions({ name: 'ImportProcessDialog' });

const emit = defineEmits<{ imported: [] }>();

const visible = ref(false);
const mode = ref<'paste' | 'file'>('paste');
const xmlText = ref('');
const picked = ref<File | null>(null);
const saving = ref(false);

function open() {
  mode.value = 'paste';
  xmlText.value = '';
  picked.value = null;
  visible.value = true;
}

function onClosed() {
  xmlText.value = '';
  picked.value = null;
}

function onFileChange(file: UploadFile) {
  picked.value = (file.raw as File) || null;
}

async function confirm() {
  saving.value = true;
  try {
    let data;
    if (mode.value === 'paste') {
      const xml = xmlText.value.trim();
      if (!xml) {
        ElMessage.warning('请粘贴 XML/BPMN 内容');
        return;
      }
      data = await importFromText(xml);
    } else {
      if (!picked.value) {
        ElMessage.warning('请选择文件');
        return;
      }
      data = await importFromFile(picked.value);
    }
    if (!isImportOk(data)) {
      throw new Error(importMessage(data) || '导入失败');
    }
    ElMessage.success(importMessage(data) || '导入成功');
    visible.value = false;
    emit('imported');
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  } finally {
    saving.value = false;
  }
}

defineExpose({ open });
</script>

<style scoped>
.sf-import__ta :deep(textarea) {
  font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace;
  font-size: 12px;
}
</style>
