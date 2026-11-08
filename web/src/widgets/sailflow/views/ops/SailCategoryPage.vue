<template>
  <formcard title="流程分类" width="" flex stretch>
    <template #toolbar>
      <div class="sf-formcard-toolbar">
        <el-button :loading="loading" @click="refresh">刷新</el-button>
        <el-button type="primary" @click="openCreate">新建分类</el-button>
        <el-button @click="router.push('/sail/processes')">流程定义</el-button>
        <el-button @click="router.push('/sail/report')">报表</el-button>
      </div>
    </template>
    <main class="sf-cat-page__main">
      <el-alert
        title="分类目录存 sf_process_category；绑定写入流程定义 AppType（不改引擎表结构）。"
        type="info"
        show-icon
        :closable="false"
        class="sf-cat-page__alert"
      />
      <el-alert v-if="err" :title="err" type="error" show-icon :closable="false" class="sf-cat-page__alert" />
      <el-row :gutter="16">
        <el-col :md="10" :xs="24">
          <el-table v-loading="loading" :data="rows" stripe border empty-text="暂无分类" height="420">
            <el-table-column prop="Code" label="编码" width="120" />
            <el-table-column prop="Name" label="名称" min-width="120" />
            <el-table-column prop="SortOrder" label="排序" width="70" />
            <el-table-column label="流程数" width="80">
              <template #default="{ row }">{{ counts[row.Code || ''] ?? 0 }}</template>
            </el-table-column>
            <el-table-column label="操作" width="140" fixed="right">
              <template #default="{ row }">
                <el-button link type="primary" @click="openEdit(row)">编辑</el-button>
                <el-button link type="danger" @click="onDelete(row)">删除</el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-col>
        <el-col :md="14" :xs="24">
          <h3 class="sf-cat-page__h">绑定流程到分类</h3>
          <el-form label-width="90px" class="sf-cat-page__bind">
            <el-form-item label="流程">
              <el-select v-model="bind.ProcessId" filterable style="width: 100%" @change="onPickProcess">
                <el-option
                  v-for="p in processes"
                  :key="String(field(p, 'ProcessId', 'processId'))"
                  :label="`${field(p, 'ProcessName', 'processName')} · ${field(p, 'AppType', 'appType') || '未分类'}`"
                  :value="String(field(p, 'ProcessId', 'processId'))"
                />
              </el-select>
            </el-form-item>
            <el-form-item label="分类">
              <el-select v-model="bind.CategoryCode" clearable placeholder="清空=未分类" style="width: 100%">
                <el-option v-for="c in rows" :key="c.Code" :label="`${c.Name} (${c.Code})`" :value="c.Code!" />
              </el-select>
            </el-form-item>
            <el-form-item>
              <el-button type="primary" :loading="binding" :disabled="!bind.ProcessId" @click="onBind">
                保存绑定
              </el-button>
            </el-form-item>
          </el-form>
        </el-col>
      </el-row>
    </main>

    <el-dialog v-model="dlg" :title="draft.Id ? '编辑分类' : '新建分类'" width="420px" destroy-on-close>
      <el-form label-width="70px">
        <el-form-item label="编码" required>
          <el-input v-model="draft.Code" :disabled="!!draft.Id" />
        </el-form-item>
        <el-form-item label="名称" required>
          <el-input v-model="draft.Name" />
        </el-form-item>
        <el-form-item label="排序">
          <el-input-number v-model="draft.SortOrder" :min="0" />
        </el-form-item>
        <el-form-item label="备注">
          <el-input v-model="draft.Remark" />
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
import { onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage, ElMessageBox } from 'element-plus';
import formcard from '@/ui/panels/formcard.vue';
import {
  bindProcessCategory,
  countByCategory,
  deleteCategory,
  listCategories,
  saveCategory,
  type CategoryRow
} from '@sail/api/category';
import { getProcessListSimple } from '@sail/api/process';
import { pickField, unpackSailResult } from '@sail/utils/sailResult';

defineOptions({ name: 'SailCategoryPage' });

const router = useRouter();
const loading = ref(false);
const saving = ref(false);
const binding = ref(false);
const err = ref('');
const rows = ref<CategoryRow[]>([]);
const counts = ref<Record<string, number>>({});
const processes = ref<Record<string, unknown>[]>([]);
const dlg = ref(false);
const draft = reactive<CategoryRow>({ Code: '', Name: '', SortOrder: 0, Remark: '' });
const bind = reactive({ ProcessId: '', Version: '1', CategoryCode: '' as string | undefined });

function field(row: Record<string, unknown>, ...keys: string[]) {
  return pickField(row, ...keys) ?? '';
}

/** API 多为 camelCase；表格/表单统一成 PascalCase */
function normalizeCategory(raw: Record<string, unknown> | CategoryRow): CategoryRow {
  const r = raw as Record<string, unknown>;
  return {
    Id: String(pickField(r, 'Id', 'id') ?? ''),
    Code: String(pickField(r, 'Code', 'code') ?? ''),
    Name: String(pickField(r, 'Name', 'name') ?? ''),
    SortOrder: Number(pickField(r, 'SortOrder', 'sortOrder') ?? 0),
    Remark: String(pickField(r, 'Remark', 'remark') ?? ''),
    CreatedAt: pickField<string>(r, 'CreatedAt', 'createdAt')
  };
}

async function refresh() {
  loading.value = true;
  err.value = '';
  try {
    const [catData, cntData, procData] = await Promise.all([
      listCategories(),
      countByCategory(),
      getProcessListSimple()
    ]);
    if ((catData?.Status ?? catData?.status) === -1) {
      err.value = catData?.Message || '分类加载失败';
      rows.value = [];
    } else {
      const r = unpackSailResult<CategoryRow[]>(catData);
      const list = Array.isArray(r.entity) ? r.entity : [];
      rows.value = list.map(x => normalizeCategory(x as CategoryRow));
    }
    const cnt = unpackSailResult<Record<string, number>>(cntData);
    counts.value = cnt.entity || {};
    const list = procData?.Entity ?? procData?.entity ?? procData;
    processes.value = Array.isArray(list) ? list : [];
  } catch (e: unknown) {
    err.value = e instanceof Error ? e.message : String(e);
  } finally {
    loading.value = false;
  }
}

function openCreate() {
  Object.assign(draft, { Id: undefined, Code: '', Name: '', SortOrder: rows.value.length, Remark: '' });
  dlg.value = true;
}

function openEdit(row: CategoryRow) {
  Object.assign(draft, { ...row });
  dlg.value = true;
}

async function onSave() {
  if (!draft.Code?.trim() || !draft.Name?.trim()) {
    ElMessage.warning('请填写编码与名称');
    return;
  }
  saving.value = true;
  try {
    const data = await saveCategory({ ...draft });
    if ((data?.Status ?? data?.status) === -1) {
      ElMessage.error(data?.Message || '保存失败');
      return;
    }
    ElMessage.success('已保存');
    dlg.value = false;
    refresh();
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  } finally {
    saving.value = false;
  }
}

async function onDelete(row: CategoryRow) {
  try {
    await ElMessageBox.confirm(`删除分类「${row.Name}」？`, '删除', { type: 'warning' });
  } catch {
    return;
  }
  try {
    const data = await deleteCategory(row.Id || row.Code || '');
    if ((data?.Status ?? data?.status) === -1) {
      ElMessage.error(data?.Message || '删除失败');
      return;
    }
    ElMessage.success('已删除');
    refresh();
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  }
}

function onPickProcess(processId: string) {
  const p = processes.value.find(x => String(field(x, 'ProcessId', 'processId')) === processId);
  bind.Version = String(field(p || {}, 'Version', 'version') || '1');
  bind.CategoryCode = String(field(p || {}, 'AppType', 'appType') || '') || undefined;
}

async function onBind() {
  binding.value = true;
  try {
    const data = await bindProcessCategory({
      ProcessId: bind.ProcessId,
      Version: bind.Version,
      CategoryCode: bind.CategoryCode || ''
    });
    if ((data?.Status ?? data?.status) === -1) {
      ElMessage.error(data?.Message || '绑定失败');
      return;
    }
    ElMessage.success('已绑定');
    refresh();
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  } finally {
    binding.value = false;
  }
}

onMounted(refresh);
</script>

<style scoped>
.sf-formcard-toolbar {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
  align-items: center;
}
.sf-cat-page__main {
  height: 100%;
  padding: 16px;
  overflow: auto;
  box-sizing: border-box;
}
.sf-cat-page__alert {
  margin-bottom: 12px;
}
.sf-cat-page__h {
  margin: 0 0 12px;
  font-size: 14px;
}
.sf-cat-page__bind {
  background: var(--el-bg-color, #fff);
  border: 1px solid var(--el-border-color-lighter);
  border-radius: 6px;
  padding: 16px 16px 4px;
}
</style>
