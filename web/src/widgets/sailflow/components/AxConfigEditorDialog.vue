<template>
  <el-dialog
    v-model="visible"
    title="AI 活动配置"
    width="720px"
    destroy-on-close
    @closed="onClosed"
  >
    <el-alert
      v-if="!processId"
      title="请先打开或保存带 ProcessId 的流程"
      type="warning"
      show-icon
      :closable="false"
      class="sf-ax__alert"
    />
    <el-form v-loading="loading" label-width="120px">
      <el-form-item label="ActivityId">
        <el-input :model-value="activityId" disabled />
      </el-form-item>
      <el-form-item label="服务类型">
        <el-select v-model="form.ServiceType" style="width: 200px">
          <el-option label="LLM" value="LLM" />
          <el-option label="RAG" value="RAG" />
          <el-option label="Agent" value="Agent" />
        </el-select>
      </el-form-item>
      <el-form-item label="模型账号" required>
        <el-select
          v-model="form.ModelProviderId"
          filterable
          clearable
          style="width: 100%"
          @change="onModelChange"
        >
          <el-option
            v-for="m in models"
            :key="m.Id"
            :label="`${m.ModelName || m.ModelProvider} (#${m.Id})`"
            :value="m.Id!"
          />
        </el-select>
      </el-form-item>
      <el-form-item label="模型名" required>
        <el-input v-model="form.ModelName" placeholder="与账号关联的模型名" />
      </el-form-item>
      <el-form-item label="说明">
        <el-input v-model="form.Description" />
      </el-form-item>
      <el-form-item label="System Prompt" required>
        <el-input v-model="form.SystemPrompt" type="textarea" :rows="4" />
      </el-form-item>
      <el-form-item label="User Message">
        <el-input v-model="form.UserMessage" type="textarea" :rows="3" />
      </el-form-item>
      <el-form-item label="Temperature">
        <el-input-number v-model="form.Temperature" :min="0" :max="2" :step="0.1" />
      </el-form-item>
      <el-form-item label="Max Tokens">
        <el-input-number v-model="form.MaxTokens" :min="0" :step="100" />
      </el-form-item>
      <el-form-item label="Memory Turns">
        <el-input-number v-model="form.MemoryTurns" :min="0" :max="50" />
      </el-form-item>
      <el-form-item label="响应格式">
        <el-radio-group v-model="form.ResponseFormat">
          <el-radio value="text">text</el-radio>
          <el-radio value="json">json</el-radio>
        </el-radio-group>
      </el-form-item>

      <template v-if="form.ServiceType === 'RAG'">
        <el-divider content-position="left">RAG / 知识库</el-divider>
        <el-form-item label="检索策略">
          <el-select v-model="form.RagSearchStrategy" style="width: 200px">
            <el-option label="similarity" value="similarity" />
            <el-option label="mmr" value="mmr" />
          </el-select>
        </el-form-item>
        <el-form-item label="检索条数">
          <el-input-number v-model="form.RagSearchCount" :min="1" :max="50" />
        </el-form-item>
        <el-form-item label="相似度阈值">
          <el-input-number v-model="form.RagSimilarityThreshold" :min="0" :max="1" :step="0.05" />
        </el-form-item>
        <el-form-item label="Embedding 模型">
          <el-select v-model="form.RagEmbeddingModelId" filterable clearable style="width: 100%">
            <el-option
              v-for="m in embeddingModels"
              :key="m.Id"
              :label="`${m.ModelName || m.ModelProvider} (#${m.Id})`"
              :value="m.Id!"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="Embedding 维数">
          <el-input-number v-model="form.RagEmbeddingDimensions" :min="1" :step="64" />
        </el-form-item>
        <el-form-item label="Rag Function">
          <el-input v-model="form.RagFunction" placeholder="可选函数名" />
        </el-form-item>
      </template>
    </el-form>
    <template #footer>
      <el-button :disabled="!processId || !activityId" @click="onDelete">删除配置</el-button>
      <el-button @click="visible = false">取消</el-button>
      <el-button type="primary" :loading="saving" :disabled="!processId" @click="onSave">
        保存
      </el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { onBeforeUnmount, onMounted, reactive, ref } from 'vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import { designerEvents } from '@sail/bpmn/ui/designerEvents';
import { getDesignerProcessMeta } from '@sail/bpmn/ui/designerContext';
import {
  deleteAxConfig,
  getAxConfig,
  getModelList,
  getModelListByType,
  saveAxConfig,
  unwrapEntity,
  unwrapList,
  type AiModelRow,
  type AxConfigRow
} from '@sail/api/ai';

defineOptions({ name: 'AxConfigEditorDialog' });

const visible = ref(false);
const loading = ref(false);
const saving = ref(false);
const activityId = ref('');
const processId = ref('');
const version = ref('1');
const models = ref<AiModelRow[]>([]);
const embeddingModels = ref<AiModelRow[]>([]);

const form = reactive<AxConfigRow>({
  ServiceType: 'LLM',
  ModelProviderId: null,
  ModelName: '',
  Description: '',
  SystemPrompt: '',
  UserMessage: '',
  Temperature: 0.7,
  MaxTokens: 2048,
  MemoryTurns: 10,
  ResponseFormat: 'text',
  Timeout: 60,
  MaxRetries: 0,
  RagSearchStrategy: 'similarity',
  RagSearchCount: 5,
  RagSimilarityThreshold: 0.7,
  RagEmbeddingModelId: null,
  RagEmbeddingDimensions: 1536,
  RagFunction: ''
});

function resetForm(serviceType?: string) {
  Object.assign(form, {
    Id: undefined,
    ServiceType: serviceType || 'LLM',
    ModelProviderId: null,
    ModelName: '',
    Description: '',
    SystemPrompt: '',
    UserMessage: '',
    Temperature: 0.7,
    MaxTokens: 2048,
    MemoryTurns: 10,
    ResponseFormat: 'text',
    Timeout: 60,
    MaxRetries: 0,
    RagSearchStrategy: 'similarity',
    RagSearchCount: 5,
    RagSimilarityThreshold: 0.7,
    RagEmbeddingModelId: null,
    RagEmbeddingDimensions: 1536,
    RagFunction: ''
  });
}

function onClosed() {
  activityId.value = '';
  processId.value = '';
  resetForm();
}

function onModelChange(id: string | null) {
  const m = models.value.find(x => x.Id === id);
  if (m?.ModelName) form.ModelName = m.ModelName;
}

async function loadModels() {
  try {
    const [all, emb] = await Promise.all([getModelList(), getModelListByType('vector_model')]);
    models.value = unwrapList<AiModelRow>(all);
    embeddingModels.value = unwrapList<AiModelRow>(emb);
    if (!embeddingModels.value.length) embeddingModels.value = models.value;
  } catch {
    models.value = [];
    embeddingModels.value = [];
  }
}

async function openHandler(payload: { activityId: string; serviceType?: string }) {
  activityId.value = payload.activityId;
  const meta = getDesignerProcessMeta();
  processId.value = meta.processId;
  version.value = meta.version || '1';
  resetForm(payload.serviceType);
  visible.value = true;
  await loadModels();

  if (!processId.value || !activityId.value) return;
  loading.value = true;
  try {
    const data = await getAxConfig(processId.value, version.value, activityId.value);
    if (data?.Status === -1 || data?.Status === '-1') {
      // 无配置也正常
      return;
    }
    const entity = unwrapEntity<AxConfigRow>(data);
    if (!entity || (!entity.Id && !entity.SystemPrompt && !entity.ModelProviderId)) return;
    Object.assign(form, {
      Id: entity.Id,
      ServiceType: entity.ServiceType || payload.serviceType || 'LLM',
      ModelProviderId: entity.ModelProviderId ?? null,
      ModelName: entity.ModelName || '',
      Description: entity.Description || '',
      SystemPrompt: entity.SystemPrompt || '',
      UserMessage: entity.UserMessage || '',
      Temperature: Number(entity.Temperature ?? 0.7),
      MaxTokens: Number(entity.MaxTokens ?? 2048),
      MemoryTurns: Number(entity.MemoryTurns ?? 10),
      ResponseFormat: entity.ResponseFormat || 'text',
      Timeout: Number(entity.Timeout ?? 60),
      MaxRetries: Number(entity.MaxRetries ?? 0),
      RagSearchStrategy: entity.RagSearchStrategy || 'similarity',
      RagSearchCount: entity.RagSearchCount ?? 5,
      RagSimilarityThreshold: entity.RagSimilarityThreshold ?? 0.7,
      RagEmbeddingModelId: entity.RagEmbeddingModelId ?? null,
      RagEmbeddingDimensions: entity.RagEmbeddingDimensions ?? 1536,
      RagFunction: entity.RagFunction || '',
      RagSearchMode: entity.RagSearchMode || ''
    });
  } catch {
    /* 无配置 */
  } finally {
    loading.value = false;
  }
}

async function onSave() {
  if (!processId.value || !activityId.value) {
    ElMessage.warning('缺少 ProcessId / ActivityId');
    return;
  }
  if (!form.SystemPrompt?.trim()) {
    ElMessage.warning('System Prompt 必填');
    return;
  }
  if (!form.ModelProviderId) {
    ElMessage.warning('请选择模型账号');
    return;
  }
  if (!form.ModelName?.trim()) {
    ElMessage.warning('模型名必填');
    return;
  }
  saving.value = true;
  try {
    const payload: AxConfigRow = {
      ...form,
      ProcessId: processId.value,
      Version: version.value,
      ActivityId: activityId.value
    };
    const data = await saveAxConfig(payload);
    if (data?.Status === -1 || data?.Status === '-1') {
      throw new Error(data?.Message || '保存失败');
    }
    ElMessage.success('活动配置已保存');
    visible.value = false;
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  } finally {
    saving.value = false;
  }
}

async function onDelete() {
  if (!processId.value || !activityId.value) return;
  try {
    await ElMessageBox.confirm('删除该活动的 AI 配置？', '确认', { type: 'warning' });
    const data = await deleteAxConfig(processId.value, version.value, activityId.value);
    if (data?.Status === -1 || data?.Status === '-1') {
      throw new Error(data?.Message || '删除失败');
    }
    ElMessage.success('已删除');
    visible.value = false;
  } catch (e: unknown) {
    if (e === 'cancel') return;
    ElMessage.error(e instanceof Error ? e.message : String(e));
  }
}

onMounted(() => designerEvents.on('axconfig:open', openHandler));
onBeforeUnmount(() => designerEvents.off('axconfig:open', openHandler));
</script>

<style scoped>
.sf-ax__alert {
  margin-bottom: 12px;
}
</style>
