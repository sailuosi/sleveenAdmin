<script setup lang="ts">
import { ref } from 'vue';
import { ElMessage } from 'element-plus';
import type { UploadFile, UploadFiles } from 'element-plus';

interface Props {
  title?: string;
}

withDefaults(defineProps<Props>(), {
  title: '上传头像'
});

const emit = defineEmits<{
  uploadCropperImg: [{ img: File }];
}>();

const visible = ref(false);
const selectedFile = ref<File | null>(null);

function openDialog(_currentAvatar?: string) {
  selectedFile.value = null;
  visible.value = true;
}

function handleFileChange(file: UploadFile, _fileList: UploadFiles) {
  selectedFile.value = (file.raw as File) || null;
}

function confirmUpload() {
  if (!selectedFile.value) {
    ElMessage.warning('请先选择图片');
    return;
  }
  emit('uploadCropperImg', { img: selectedFile.value });
  visible.value = false;
}

defineExpose({ openDialog });
</script>

<template>
  <ElDialog v-model="visible" :title="title" width="520px" destroy-on-close>
    <div class="cropper-placeholder">
      <ElUpload drag action="" :auto-upload="false" :show-file-list="true" :limit="1" :on-change="handleFileChange">
        <ElIcon class="el-icon--upload"><EleUploadFilled /></ElIcon>
        <div class="el-upload__text">拖拽图片到此处，或点击选择</div>
        <template #tip>
          <div class="el-upload__tip">当前为简化版上传，后续可替换为裁剪组件</div>
        </template>
      </ElUpload>
    </div>
    <template #footer>
      <ElButton @click="visible = false">取消</ElButton>
      <ElButton type="primary" :disabled="!selectedFile" @click="confirmUpload">确定</ElButton>
    </template>
  </ElDialog>
</template>

<style scoped>
.cropper-placeholder {
  padding: 8px 0;
}
</style>
