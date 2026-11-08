<template>
  <formcard title="知识库" width="" flex stretch>
    <template #toolbar>
      <div class="sf-formcard-toolbar">
        <el-button text @click="router.push('/sail/processes')">流程列表</el-button>
        <el-button text @click="router.push('/sail/ai-models')">AI 模型</el-button>
        <el-button :loading="loading" @click="loadAll">刷新</el-button>
        <el-button @click="openCreate">新增文档</el-button>
        <el-button type="primary" :loading="searching" @click="onSearch">向量检索</el-button>
      </div>
    </template>
    <main class="sf-kb-page__main">
      <el-form inline class="sf-kb-page__search">
        <el-form-item label="关键词">
          <el-input v-model="query" clearable style="width: 280px" placeholder="检索关键词" @keyup.enter="onSearch" />
        </el-form-item>
        <el-form-item label="阈值">
          <el-input-number v-model="threshold" :min="0" :max="1" :step="0.05" />
        </el-form-item>
        <el-form-item label="条数">
          <el-input-number v-model="count" :min="1" :max="50" />
        </el-form-item>
      </el-form>
      <el-alert v-if="err" :title="err" type="error" show-icon :closable="false" class="sf-kb-page__alert" />
      <div class="sf-kb-page__table">
        <el-table v-loading="loading || searching" :data="rows" stripe border height="100%" empty-text="暂无文档">
          <el-table-column prop="Id" label="编号" width="80" />
          <el-table-column prop="Content" label="内容" min-width="280" show-overflow-tooltip />
          <el-table-column prop="Metadata" label="元数据" min-width="160" show-overflow-tooltip />
          <el-table-column prop="IndustryId" label="行业 ID" width="100" />
          <el-table-column label="操作" width="100" fixed="right">
            <template #default="{ row }">
              <el-button link type="danger" @click="onDelete(row)">删除</el-button>
            </template>
          </el-table-column>
        </el-table>
      </div>
    </main>

    <el-dialog v-model="createVisible" title="新增知识文档" width="560px" destroy-on-close>
      <el-form label-width="80px">
        <el-form-item label="内容" required>
          <el-input v-model="draft.Content" type="textarea" :rows="6" placeholder="文档正文" />
        </el-form-item>
        <el-form-item label="元数据">
          <el-input v-model="draft.Metadata" type="textarea" :rows="2" placeholder='可选 JSON，如 {"source":"manual"}' />
        </el-form-item>
        <el-form-item label="行业ID">
          <el-input-number v-model="draft.IndustryId" :min="0" controls-position="right" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="createVisible = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="onSave">保存</el-button>
      </template>
    </el-dialog>
  </formcard>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage, ElMessageBox } from 'element-plus';
import formcard from '@/ui/panels/formcard.vue';
import {
  deleteKnowledgeBaseDocument,
  getKnowledgeBaseDocuments,
  saveKnowledgeBaseDocument,
  searchKnowledgeBase,
  unwrapList,
  type KbDocumentRow
} from '@sail/api/ai';

defineOptions({ name: 'SailKnowledgeBasePage' });

const router = useRouter();
const loading = ref(false);
const searching = ref(false);
const saving = ref(false);
const err = ref('');
const rows = ref<KbDocumentRow[]>([]);
const query = ref('');
const threshold = ref(0.7);
const count = ref(5);
const createVisible = ref(false);
const draft = reactive({
  Content: '',
  Metadata: '',
  IndustryId: undefined as number | undefined
});

async function loadAll() {
  loading.value = true;
  err.value = '';
  try {
    const data = await getKnowledgeBaseDocuments();
    if (data?.Status === -1 || data?.Status === '-1') {
      err.value = data?.Message || '加载失败';
      rows.value = [];
      return;
    }
    rows.value = unwrapList<KbDocumentRow>(data);
  } catch (e: unknown) {
    err.value = e instanceof Error ? e.message : String(e);
    rows.value = [];
  } finally {
    loading.value = false;
  }
}

async function onSearch() {
  if (!query.value.trim()) {
    ElMessage.warning('请输入检索关键词');
    return;
  }
  searching.value = true;
  err.value = '';
  try {
    const data = await searchKnowledgeBase({
      Query: query.value.trim(),
      Threshold: threshold.value,
      Count: count.value
    });
    if (data?.Status === -1 || data?.Status === '-1') {
      throw new Error(data?.Message || '检索失败');
    }
    rows.value = unwrapList<KbDocumentRow>(data);
    ElMessage.success(`检索到 ${rows.value.length} 条`);
  } catch (e: unknown) {
    err.value = e instanceof Error ? e.message : String(e);
    ElMessage.error(err.value);
  } finally {
    searching.value = false;
  }
}

function openCreate() {
  draft.Content = '';
  draft.Metadata = '';
  draft.IndustryId = undefined;
  createVisible.value = true;
}

async function onSave() {
  if (!draft.Content.trim()) {
    ElMessage.warning('请填写内容');
    return;
  }
  saving.value = true;
  try {
    const data = await saveKnowledgeBaseDocument({
      Content: draft.Content.trim(),
      Metadata: draft.Metadata.trim() || undefined,
      IndustryId: draft.IndustryId || null
    });
    if (data?.Status === -1 || data?.Status === '-1') {
      ElMessage.error(data?.Message || '保存失败');
      return;
    }
    ElMessage.success('已保存');
    createVisible.value = false;
    loadAll();
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  } finally {
    saving.value = false;
  }
}

async function onDelete(row: KbDocumentRow) {
  const id = row.Id;
  if (!id) return;
  try {
    await ElMessageBox.confirm(`确认删除文档 #${id}？`, '删除', { type: 'warning' });
  } catch {
    return;
  }
  try {
    const data = await deleteKnowledgeBaseDocument(id);
    if (data?.Status === -1 || data?.Status === '-1') {
      ElMessage.error(data?.Message || '删除失败');
      return;
    }
    ElMessage.success('已删除');
    loadAll();
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  }
}

onMounted(loadAll);
</script>

<style scoped>
.sf-formcard-toolbar {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
  align-items: center;
}
.sf-kb-page__main {
  height: 100%;
  min-height: 0;
  padding: 12px 16px;
  display: flex;
  flex-direction: column;
  box-sizing: border-box;
}
.sf-kb-page__search {
  margin-bottom: 8px;
  flex: 0 0 auto;
}
.sf-kb-page__alert {
  margin-bottom: 12px;
  flex: 0 0 auto;
}
.sf-kb-page__table {
  flex: 1 1 auto;
  min-height: 0;
}
</style>
