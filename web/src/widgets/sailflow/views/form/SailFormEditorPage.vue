<template>
  <div class="sf-form-editor">
    <header class="sf-form-editor__bar">
      <div class="sf-form-editor__title">{{ formId ? '编辑表单' : '新建表单' }}</div>
      <div class="sf-form-editor__actions">
        <el-button @click="router.push('/sail/forms')">返回列表</el-button>
        <el-button type="primary" :loading="saving" @click="openSaveDialog">保存</el-button>
      </div>
    </header>
    <el-alert v-if="err" :title="err" type="error" show-icon :closable="false" class="sf-form-editor__alert" />
    <div class="sf-form-editor__body">
      <fc-designer ref="designer" class="sf-form-editor__designer" :config="designerConfig" />
    </div>

    <el-dialog v-model="saveVisible" title="保存表单" width="520px" destroy-on-close>
      <el-form label-width="80px">
        <el-form-item label="表单名" required>
          <el-input v-model="meta.FormName" placeholder="请输入表单名" />
        </el-form-item>
        <el-form-item label="编码">
          <el-input v-model="meta.FormCode" placeholder="可选编码" />
        </el-form-item>
        <el-form-item label="备注">
          <el-input v-model="meta.Description" type="textarea" :rows="2" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="saveVisible = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="submitSave">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { nextTick, onMounted, reactive, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ElMessage } from 'element-plus';
import { getForm, saveForm } from '@sail/api/form';
import { encodeConf, encodeFields, setConfAndFields } from '@sail/utils/formCreate';
import { unpackSailResult, pickField } from '@sail/utils/sailResult';

defineOptions({ name: 'SailFormEditorPage' });

const route = useRoute();
const router = useRouter();
const designer = ref<any>(null);
const err = ref('');
const saving = ref(false);
const saveVisible = ref(false);
const formId = ref<string>(String(route.query.formId || ''));

const meta = reactive({
  FormName: '',
  FormCode: '',
  Description: '',
  Version: '1'
});

const designerConfig = {
  showSaveBtn: false,
  autoActive: true,
  useTemplate: false,
  formOptions: {
    form: { labelWidth: '100px' }
  }
};

async function load() {
  err.value = '';
  if (!formId.value) {
    await nextTick();
    try {
      designer.value?.setOption?.({});
      designer.value?.setRule?.([]);
    } catch {
      /* ignore */
    }
    return;
  }
  try {
    const data = await getForm(formId.value);
    const r = unpackSailResult<Record<string, unknown>>(data);
    if ((data?.Status ?? data?.status) === -1) {
      err.value = r.message || '加载失败';
      return;
    }
    const entity = r.entity || {};
    meta.FormName = String(pickField(entity, 'FormName', 'formName') || '');
    meta.FormCode = String(pickField(entity, 'FormCode', 'formCode') || '');
    meta.Description = String(pickField(entity, 'Description', 'description') || '');
    meta.Version = String(pickField(entity, 'Version', 'version') || '1');
    await nextTick();
    setConfAndFields(
      designer,
      String(pickField(entity, 'Conf', 'conf') || '{}'),
      String(pickField(entity, 'FieldsJson', 'fieldsJson') || '[]')
    );
  } catch (e: unknown) {
    err.value = e instanceof Error ? e.message : String(e);
  }
}

function openSaveDialog() {
  if (!meta.FormName) meta.FormName = '未命名表单';
  saveVisible.value = true;
}

async function submitSave() {
  if (!meta.FormName.trim()) {
    ElMessage.warning('请填写表单名');
    return;
  }
  saving.value = true;
  try {
    const conf = encodeConf(designer);
    const fields = encodeFields(designer);
    const data = await saveForm({
      FormId: formId.value || undefined,
      FormName: meta.FormName.trim(),
      FormCode: meta.FormCode.trim() || undefined,
      Description: meta.Description,
      Version: meta.Version || '1',
      Conf: conf,
      FieldsJson: JSON.stringify(fields)
    });
    const r = unpackSailResult(data);
    if ((data?.Status ?? data?.status) === -1) {
      ElMessage.error(r.message || '保存失败');
      return;
    }
    const newId = r.newId != null ? String(r.newId) : formId.value;
    if (newId) formId.value = newId;
    ElMessage.success('已保存');
    saveVisible.value = false;
    if (newId && !route.query.formId) {
      router.replace({ path: '/sail/form-editor', query: { formId: newId } });
    }
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  } finally {
    saving.value = false;
  }
}

onMounted(load);
</script>

<style scoped>
.sf-form-editor {
  height: 100vh;
  display: flex;
  flex-direction: column;
  background: var(--el-bg-color-page, #f5f7fa);
}
.sf-form-editor__bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 16px;
  background: var(--el-bg-color);
  border-bottom: 1px solid var(--el-border-color-lighter);
  flex-shrink: 0;
}
.sf-form-editor__title {
  font-size: 16px;
  font-weight: 600;
}
.sf-form-editor__actions {
  display: flex;
  gap: 8px;
}
.sf-form-editor__alert {
  margin: 8px 16px 0;
}
.sf-form-editor__body {
  flex: 1;
  min-height: 0;
  padding: 0;
}
.sf-form-editor__designer {
  height: 100%;
}
</style>
