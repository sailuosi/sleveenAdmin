<template>
  <formcard title="我的常用语" width="" flex stretch>
    <template #toolbar>
      <div class="sf-formcard-toolbar">
        <el-button type="primary" @click="openEdit()">新建</el-button>
        <el-button :loading="loading" @click="reload">刷新</el-button>
      </div>
    </template>
    <el-alert
      title="用于办理通过/驳回意见快捷填入；仅本人可见。未配置时办理条使用内置默认短语。"
      type="info"
      show-icon
      :closable="false"
      class="sf-phrase-page__alert"
    />
    <el-table v-loading="loading" :data="rows" stripe border empty-text="暂无常用语" height="100%">
      <el-table-column type="index" label="#" width="50" />
      <el-table-column prop="Content" label="内容" min-width="240" />
      <el-table-column prop="SortOrder" label="排序" width="80" />
      <el-table-column label="状态" width="80">
        <template #default="{ row }">{{ row.Enabled === 0 ? '停用' : '启用' }}</template>
      </el-table-column>
      <el-table-column label="操作" width="200" fixed="right">
        <template #default="{ row, $index }">
          <el-button link type="primary" :disabled="$index === 0" @click="move($index, -1)">上移</el-button>
          <el-button
            link
            type="primary"
            :disabled="$index >= rows.length - 1"
            @click="move($index, 1)"
          >
            下移
          </el-button>
          <el-button link type="primary" @click="openEdit(row)">编辑</el-button>
          <el-button link type="danger" @click="onDelete(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="dlg" :title="draft.Id ? '编辑常用语' : '新建常用语'" width="480px" destroy-on-close>
      <el-form label-width="70px">
        <el-form-item label="内容" required>
          <el-input v-model="draft.Content" type="textarea" :rows="3" maxlength="500" show-word-limit />
        </el-form-item>
        <el-form-item label="启用">
          <el-switch v-model="draftEnabled" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dlg = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="onSave">保存</el-button>
      </template>
    </el-dialog>
  </formcard>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import formcard from '@/ui/panels/formcard.vue';
import {
  deleteApprovalPhrase,
  listMyApprovalPhrases,
  saveApprovalPhrase,
  updateApprovalPhraseSort,
  type ApprovalPhraseRow
} from '@sail/api/approvalPhrase';
import { pickField, unpackSailResult } from '@sail/utils/sailResult';

defineOptions({ name: 'SailApprovalPhrasePage' });

const loading = ref(false);
const saving = ref(false);
const rows = ref<ApprovalPhraseRow[]>([]);
const dlg = ref(false);
const draft = reactive<ApprovalPhraseRow>({ Content: '', Enabled: 1 });

const draftEnabled = computed({
  get: () => draft.Enabled !== 0,
  set: v => {
    draft.Enabled = v ? 1 : 0;
  }
});

function normalize(raw: Record<string, unknown>): ApprovalPhraseRow {
  return {
    Id: String(pickField(raw, 'Id', 'id') || ''),
    Content: String(pickField(raw, 'Content', 'content') || ''),
    SortOrder: Number(pickField(raw, 'SortOrder', 'sortOrder') || 0),
    Enabled: Number(pickField(raw, 'Enabled', 'enabled') ?? 1)
  };
}

async function reload() {
  loading.value = true;
  try {
    const data = await listMyApprovalPhrases(false);
    const r = unpackSailResult<Record<string, unknown>[]>(data);
    const list = Array.isArray(r.entity) ? r.entity : [];
    rows.value = list.map(x => normalize(x));
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
    rows.value = [];
  } finally {
    loading.value = false;
  }
}

function openEdit(row?: ApprovalPhraseRow) {
  draft.Id = row?.Id || '';
  draft.Content = row?.Content || '';
  draft.Enabled = row?.Enabled === 0 ? 0 : 1;
  dlg.value = true;
}

async function onSave() {
  if (!String(draft.Content || '').trim()) {
    ElMessage.warning('请填写内容');
    return;
  }
  saving.value = true;
  try {
    await saveApprovalPhrase({
      Id: draft.Id || undefined,
      Content: draft.Content,
      Enabled: draft.Enabled
    });
    ElMessage.success('已保存');
    dlg.value = false;
    await reload();
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  } finally {
    saving.value = false;
  }
}

async function onDelete(row: ApprovalPhraseRow) {
  const id = String(row.Id || '');
  if (!id) return;
  try {
    await ElMessageBox.confirm('确认删除该常用语？', '提示', { type: 'warning' });
    await deleteApprovalPhrase(id);
    ElMessage.success('已删除');
    await reload();
  } catch {
    /* cancel */
  }
}

async function move(index: number, delta: number) {
  const next = index + delta;
  if (next < 0 || next >= rows.value.length) return;
  const copy = [...rows.value];
  const [item] = copy.splice(index, 1);
  copy.splice(next, 0, item!);
  rows.value = copy;
  try {
    await updateApprovalPhraseSort(copy.map(x => String(x.Id || '')).filter(Boolean));
    await reload();
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
    await reload();
  }
}

onMounted(() => {
  void reload();
});
</script>

<style scoped>
.sf-phrase-page__alert {
  margin-bottom: 12px;
}
</style>
