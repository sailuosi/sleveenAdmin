<template>
  <div class="sf-ai-page">
    <header class="sf-ai-page__bar">
      <div class="sf-ai-page__title">AI 模型配置</div>
      <div class="sf-ai-page__actions">
        <el-button text @click="router.push('/sail/processes')">流程列表</el-button>
        <el-button text @click="router.push('/sail/knowledge')">知识库</el-button>
        <el-button :loading="loading" @click="refresh">刷新</el-button>
        <el-button @click="createNew">新建</el-button>
        <el-button type="danger" plain :disabled="!form.Id" @click="onDelete">删除</el-button>
        <el-button :loading="testing" @click="onTest">测试连接</el-button>
        <el-button type="primary" :loading="saving" @click="onSave">保存</el-button>
      </div>
    </header>
    <main class="sf-ai-page__main">
      <aside class="sf-ai-page__list">
        <el-alert v-if="listErr" :title="listErr" type="error" show-icon :closable="false" />
        <el-table
          v-loading="loading"
          :data="rows"
          highlight-current-row
          height="100%"
          size="small"
          empty-text="暂无模型（需 ai_model_provider 表）"
          @current-change="onSelect"
        >
          <el-table-column prop="ModelProvider" label="Provider" min-width="100" show-overflow-tooltip />
          <el-table-column prop="ModelName" label="模型" min-width="110" show-overflow-tooltip />
          <el-table-column prop="ModelType" label="类型" width="100" show-overflow-tooltip />
          <el-table-column label="启用" width="60" align="center">
            <template #default="{ row }">
              <span>{{ row.IsActive ? '是' : '否' }}</span>
            </template>
          </el-table-column>
        </el-table>
      </aside>
      <section class="sf-ai-page__editor">
        <el-form label-width="110px">
          <el-form-item label="Provider" required>
            <el-select v-model="form.ModelProvider" filterable allow-create style="width: 100%">
              <el-option v-for="p in PROVIDERS" :key="p" :label="p" :value="p" />
            </el-select>
          </el-form-item>
          <el-form-item label="模型名">
            <el-input v-model="form.ModelName" placeholder="如 gpt-4o-mini" />
          </el-form-item>
          <el-form-item label="类型">
            <el-select v-model="form.ModelType" style="width: 220px">
              <el-option label="chat_model" value="chat_model" />
              <el-option label="vector_model" value="vector_model" />
              <el-option label="other" value="other" />
            </el-select>
          </el-form-item>
          <el-form-item label="Base URL">
            <el-input v-model="form.BaseUrl" placeholder="https://api.openai.com/v1" />
          </el-form-item>
          <el-form-item label="Api Key" :required="!form.Id">
            <el-input
              v-model="form.ApiKey"
              type="password"
              show-password
              :placeholder="form.Id ? '留空则保留原密钥' : '必填'"
            />
          </el-form-item>
          <el-form-item label="Api UUID">
            <el-input v-model="form.ApiUUID" placeholder="可选" />
          </el-form-item>
          <el-form-item label="启用">
            <el-switch v-model="form.IsActive" />
          </el-form-item>
          <el-form-item label="说明">
            <el-input v-model="form.Description" type="textarea" :rows="3" />
          </el-form-item>
          <el-form-item v-if="testMsg">
            <el-alert :title="testMsg" :type="testOk ? 'success' : 'error'" show-icon :closable="true" @close="testMsg = ''" />
          </el-form-item>
        </el-form>
      </section>
    </main>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage, ElMessageBox } from 'element-plus';
import {
  deleteModel,
  getModel,
  getModelList,
  saveModel,
  testModelConnection,
  unwrapEntity,
  unwrapList,
  type AiModelRow
} from '@sail/api/ai';

defineOptions({ name: 'SailAiModelPage' });

const PROVIDERS = ['OpenAI', 'AzureOpenAI', 'DeepSeek', 'Ollama', 'Custom'];

const router = useRouter();
const loading = ref(false);
const saving = ref(false);
const testing = ref(false);
const listErr = ref('');
const rows = ref<AiModelRow[]>([]);
const testMsg = ref('');
const testOk = ref(false);

const form = reactive<AiModelRow>({
  Id: undefined,
  ModelProvider: 'OpenAI',
  ModelName: '',
  ModelType: 'chat_model',
  Description: '',
  BaseUrl: '',
  ApiUUID: '',
  ApiKey: '',
  IsActive: true
});

function createNew() {
  Object.assign(form, {
    Id: undefined,
    ModelProvider: 'OpenAI',
    ModelName: '',
    ModelType: 'chat_model',
    Description: '',
    BaseUrl: '',
    ApiUUID: '',
    ApiKey: '',
    IsActive: true
  });
  testMsg.value = '';
}

async function refresh() {
  loading.value = true;
  listErr.value = '';
  try {
    const data = await getModelList();
    if (data?.Status === -1 || data?.Status === '-1') {
      listErr.value = data?.Message || '加载失败';
      rows.value = [];
      return;
    }
    rows.value = unwrapList<AiModelRow>(data);
  } catch (e: unknown) {
    listErr.value = e instanceof Error ? e.message : String(e);
    rows.value = [];
  } finally {
    loading.value = false;
  }
}

async function onSelect(row: AiModelRow | null) {
  if (!row?.Id) return;
  try {
    const data = await getModel(row.Id);
    if (data?.Status === -1 || data?.Status === '-1') {
      throw new Error(data?.Message || '加载模型失败');
    }
    const entity = unwrapEntity<AiModelRow>(data);
    if (!entity) return;
    Object.assign(form, {
      Id: entity.Id,
      ModelProvider: entity.ModelProvider || '',
      ModelName: entity.ModelName || '',
      ModelType: entity.ModelType || 'chat_model',
      Description: entity.Description || '',
      BaseUrl: entity.BaseUrl || '',
      ApiUUID: entity.ApiUUID || '',
      // 编辑时不回填明文密钥；空表示保留库内
      ApiKey: '',
      IsActive: entity.IsActive !== false
    });
    testMsg.value = '';
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  }
}

async function onSave() {
  if (!form.ModelProvider?.trim()) {
    ElMessage.warning('Provider 必填');
    return;
  }
  if (!form.Id && !form.ApiKey?.trim()) {
    ElMessage.warning('新建须填写 Api Key');
    return;
  }
  saving.value = true;
  try {
    const data = await saveModel({
      Id: form.Id || 0,
      ModelProvider: form.ModelProvider.trim(),
      ModelName: form.ModelName || '',
      ModelType: form.ModelType || 'chat_model',
      Description: form.Description || '',
      BaseUrl: form.BaseUrl || '',
      ApiUUID: form.ApiUUID || '',
      ApiKey: form.ApiKey || '',
      IsActive: form.IsActive !== false
    });
    if (data?.Status === -1 || data?.Status === '-1') {
      throw new Error(data?.Message || '保存失败');
    }
    const saved = unwrapEntity<AiModelRow>(data);
    if (saved?.Id) form.Id = saved.Id;
    form.ApiKey = '';
    ElMessage.success('已保存');
    await refresh();
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  } finally {
    saving.value = false;
  }
}

async function onDelete() {
  if (!form.Id) return;
  try {
    await ElMessageBox.confirm(`删除模型 #${form.Id}？`, '确认', { type: 'warning' });
    const data = await deleteModel(form.Id);
    if (data?.Status === -1 || data?.Status === '-1') {
      throw new Error(data?.Message || '删除失败');
    }
    ElMessage.success('已删除');
    createNew();
    await refresh();
  } catch (e: unknown) {
    if (e === 'cancel') return;
    ElMessage.error(e instanceof Error ? e.message : String(e));
  }
}

async function onTest() {
  if (!form.BaseUrl?.trim() || !form.ModelProvider?.trim()) {
    ElMessage.warning('测试需要 Base URL 与 Provider');
    return;
  }
  if (!form.ApiKey?.trim() && !form.Id) {
    ElMessage.warning('请填写 Api Key 后再测');
    return;
  }
  // 已保存且未改密钥：用占位触发服务端 ResolvePlainApiKey（需带 ApiUUID）
  const apiKey = form.ApiKey?.trim() || '********';
  testing.value = true;
  testMsg.value = '';
  try {
    const data = await testModelConnection({
      BaseUrl: form.BaseUrl.trim(),
      ApiKey: apiKey,
      ModelProvider: form.ModelProvider.trim(),
      ApiUUID: form.ApiUUID || null
    });
    if (data?.Status === -1 || data?.Status === '-1') {
      throw new Error(data?.Message || '连接失败');
    }
    testOk.value = true;
    testMsg.value = data?.Message || '连接成功';
    ElMessage.success(testMsg.value);
  } catch (e: unknown) {
    testOk.value = false;
    testMsg.value = e instanceof Error ? e.message : String(e);
    ElMessage.error(testMsg.value);
  } finally {
    testing.value = false;
  }
}

onMounted(async () => {
  await refresh();
  if (rows.value[0]) await onSelect(rows.value[0]);
  else createNew();
});
</script>

<style scoped>
.sf-ai-page {
  height: 100vh;
  display: flex;
  flex-direction: column;
  background: var(--el-bg-color-page, #f5f7fa);
}
.sf-ai-page__bar {
  flex: 0 0 auto;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 10px 16px;
  border-bottom: 1px solid var(--el-border-color-lighter, #ebeef5);
  background: var(--el-bg-color, #fff);
}
.sf-ai-page__title {
  font-size: 16px;
  font-weight: 600;
}
.sf-ai-page__actions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}
.sf-ai-page__main {
  flex: 1 1 auto;
  min-height: 0;
  display: flex;
}
.sf-ai-page__list {
  flex: 0 0 340px;
  border-right: 1px solid var(--el-border-color-lighter, #ebeef5);
  background: var(--el-bg-color, #fff);
  padding: 8px;
  overflow: hidden;
}
.sf-ai-page__editor {
  flex: 1 1 auto;
  min-width: 0;
  padding: 16px 20px;
  overflow: auto;
}
@media (max-width: 900px) {
  .sf-ai-page__main {
    flex-direction: column;
  }
  .sf-ai-page__list {
    flex: 0 0 40%;
    border-right: none;
    border-bottom: 1px solid var(--el-border-color-lighter, #ebeef5);
  }
}
</style>
