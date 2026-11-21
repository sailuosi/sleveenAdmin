<template>
  <el-dialog
    v-model="visible"
    title="手写签名"
    width="640px"
    destroy-on-close
    append-to-body
    @opened="onOpened"
  >
    <div class="sf-sign-pad__canvas">
      <VueSignaturePad ref="padRef" :options="signOptions" class="sf-sign-pad__pad" />
    </div>
    <div class="sf-sign-pad__tools">
      <span class="sf-sign-pad__tool">
        画笔粗细：
        <el-input-number v-model="penWidth" :min="0.5" :max="2.5" :step="0.1" size="small" />
      </span>
      <span class="sf-sign-pad__tool">
        画笔颜色：
        <el-color-picker v-model="penColor" color-format="hex" size="small" />
      </span>
    </div>
    <template #footer>
      <el-button :disabled="submitting" @click="undo">撤销</el-button>
      <el-button :disabled="submitting" @click="clear">清屏</el-button>
      <el-button @click="visible = false">取消</el-button>
      <el-button type="primary" :loading="submitting" @click="submit">确定</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { reactive, ref, watch } from 'vue';
import { ElMessage } from 'element-plus';
import { base64ToFile } from '@s/utils/base64Conver';
import { fetchUploadFile } from '@/service/api';

defineOptions({ name: 'SailSignPadDialog' });

const emit = defineEmits<{ success: [url: string] }>();

const visible = ref(false);
const submitting = ref(false);
const padRef = ref<{
  resizeCanvas: () => void;
  saveSignature: () => { isEmpty: boolean; data: string };
  undoSignature: () => void;
  clearSignature: () => void;
  signaturePad: { penColor: string; minWidth: number };
} | null>(null);

const penWidth = ref(1.0);
const penColor = ref('#000000');

const signOptions = reactive({
  penColor: '#000000',
  minWidth: 1.0,
  onBegin: () => {
    padRef.value?.resizeCanvas();
  }
});

watch([penWidth, penColor], () => {
  signOptions.minWidth = penWidth.value;
  signOptions.penColor = penColor.value;
  const pad = padRef.value?.signaturePad;
  if (pad) {
    pad.minWidth = penWidth.value;
    pad.penColor = penColor.value;
  }
});

function open() {
  visible.value = true;
}

function onOpened() {
  padRef.value?.resizeCanvas();
  padRef.value?.clearSignature();
}

function undo() {
  padRef.value?.undoSignature();
}

function clear() {
  padRef.value?.clearSignature();
}

function pickUploadUrl(row: unknown): string {
  const r = (row || {}) as Record<string, unknown>;
  // 优先公开 URL（/uploads/...）；勿用 path+name 误拼（path 已含文件名）
  const url = String(r.url ?? r.Url ?? '').trim();
  if (url) return url;
  const path = String(r.path ?? r.Path ?? '').trim().replace(/\\/g, '/');
  if (path) return path.startsWith('/') ? path : `/uploads/${path.replace(/^\/+/, '')}`;
  const filePath = String(r.filePath ?? r.FilePath ?? '').trim().replace(/\\/g, '/');
  const name = String(r.name ?? r.Name ?? r.storedName ?? r.StoredName ?? '').trim();
  if (filePath && name) {
    const joined = `${filePath.replace(/\/$/, '')}/${name}`;
    return joined.startsWith('/') ? joined : `/uploads/${joined}`;
  }
  return '';
}

async function submit() {
  const pad = padRef.value;
  if (!pad) return;
  const { isEmpty, data } = pad.saveSignature();
  if (isEmpty) {
    ElMessage.warning('请先手写签名');
    return;
  }
  submitting.value = true;
  try {
    const file = base64ToFile(data, `approve-sign-${Date.now()}.png`);
    const { data: uploaded, error } = await fetchUploadFile(file);
    if (error || !uploaded) {
      const msg =
        typeof error === 'string'
          ? error
          : (error as { message?: string } | null)?.message || '签名上传失败';
      ElMessage.error(msg);
      return;
    }
    const url = pickUploadUrl(uploaded);
    if (!url) {
      ElMessage.error('签名上传成功但未返回地址');
      return;
    }
    emit('success', url);
    visible.value = false;
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : '签名上传失败');
  } finally {
    submitting.value = false;
  }
}

defineExpose({ open });
</script>

<style scoped>
.sf-sign-pad__canvas {
  border: 1px dashed var(--el-border-color);
  width: 100%;
  height: 260px;
  background: #fff;
  overflow: hidden;
}
.sf-sign-pad__pad {
  width: 100%;
  height: 100%;
  background-color: #fff;
}
.sf-sign-pad__tools {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
  margin-top: 12px;
  align-items: center;
}
.sf-sign-pad__tool {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: var(--el-text-color-regular);
}
</style>
