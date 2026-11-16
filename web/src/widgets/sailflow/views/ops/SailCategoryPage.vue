<template>
  <formcard title="流程分类" width="" flex stretch>
    <template #toolbar>
      <div class="sf-formcard-toolbar">
        <el-button :loading="loading" @click="refresh">刷新</el-button>
        <el-button v-if="isPlatformAdmin" type="primary" @click="openCreate('PUBLIC')">新建公共分类</el-button>
        <el-button type="primary" plain @click="openCreate('UNIT')">新建单位分类</el-button>
        <el-button @click="router.push('/sail/processes')">流程定义</el-button>
      </div>
    </template>
    <main class="sf-cat-page__layout">
      <OrgTreeSidePane storage-key="category">
        <SailOrgFilterTree :orgs="orgFlat" @select="onUnitSelect" />
      </OrgTreeSidePane>
      <div class="sf-cat-page__main">
        <el-alert
          title="分类目录存 sf_process_category；绑定写入流程定义 AppType。删除为逻辑删除。"
          type="info"
          show-icon
          :closable="false"
          class="sf-cat-page__alert"
        />
        <el-alert v-if="err" :title="err" type="error" show-icon :closable="false" class="sf-cat-page__alert" />
        <el-table v-loading="loading" :data="rows" stripe border empty-text="暂无分类" height="100%" class="sf-cat-page__table">
          <el-table-column type="index" label="序号" width="60" />
          <el-table-column prop="Code" label="编码" width="160" />
          <el-table-column prop="Name" label="名称" min-width="120" />
          <el-table-column label="归属" width="100">
            <template #default="{ row }">{{ row.AuthType || '待分配' }}</template>
          </el-table-column>
          <el-table-column prop="SortOrder" label="排序" width="70" />
          <el-table-column label="创建人" width="110">
            <template #default="{ row }">{{ row.CreatorName || row.Creator || '—' }}</template>
          </el-table-column>
          <el-table-column label="流程数" width="80">
            <template #default="{ row }">{{ counts[row.Code || ''] ?? 0 }}</template>
          </el-table-column>
          <el-table-column label="操作" width="220" fixed="right">
            <template #default="{ row, $index }">
              <el-button link type="primary" :disabled="$index === 0 || sorting" @click="moveRow($index, -1)">
                上移
              </el-button>
              <el-button
                link
                type="primary"
                :disabled="$index >= rows.length - 1 || sorting"
                @click="moveRow($index, 1)"
              >
                下移
              </el-button>
              <el-button link type="primary" @click="openEdit(row)">编辑</el-button>
              <el-button link type="danger" @click="onDelete(row)">删除</el-button>
            </template>
          </el-table-column>
        </el-table>
      </div>
    </main>

    <el-dialog v-model="dlg" :title="draft.Id ? '编辑分类' : '新建分类'" width="480px" destroy-on-close>
      <el-form label-width="90px">
        <ResourceScopeFields
          v-model:auth-type="draft.AuthType"
          v-model:auth-id="draft.AuthId"
          v-model:auth-code="draft.AuthCode"
          :existing="!!draft.Id"
          :fixed-scope-type="fixedScope"
          :unit-context="unitContext"
          resource-label="分类"
        />
        <el-form-item label="编码">
          <el-input v-model="draft.Code" :disabled="!!draft.Id" placeholder="空则按 O_OrgNo 规范自动生成" />
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
import { computed, onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage, ElMessageBox } from 'element-plus';
import formcard from '@/ui/panels/formcard.vue';
import OrgTreeSidePane from '@sail/components/OrgTreeSidePane.vue';
import SailOrgFilterTree from '@sail/components/SailOrgFilterTree.vue';
import ResourceScopeFields from '@sail/components/ResourceScopeFields.vue';
import {
  deleteCategory,
  getNextSort,
  listCategories,
  countByCategory,
  saveCategory,
  updateCategorySortBatch,
  type CategoryRow
} from '@sail/api/category';
import { getUnitScopeContext } from '@sail/api/unitScope';
import { pickField, unpackSailResult } from '@sail/utils/sailResult';

defineOptions({ name: 'SailCategoryPage' });

const router = useRouter();
const loading = ref(false);
const saving = ref(false);
const sorting = ref(false);
const err = ref('');
const rows = ref<CategoryRow[]>([]);
const counts = ref<Record<string, number>>({});
const dlg = ref(false);
const filterUnitId = ref<string | null>(null);
const unitContext = ref<any>(null);
const fixedScope = ref<'PUBLIC' | 'UNIT' | undefined>(undefined);
const draft = reactive<CategoryRow>({
  Code: '',
  Name: '',
  SortOrder: 0,
  Remark: '',
  AuthType: 'PUBLIC',
  AuthId: '',
  AuthCode: ''
});

const isPlatformAdmin = computed(() => !!unitContext.value?.isPlatformAdmin);

const orgFlat = computed(() => {
  const orgs = unitContext.value?.orgs || unitContext.value?.units || [];
  return (orgs || []).map((o: any) => ({
    id: String(o.id ?? o.Id ?? ''),
    parentId: o.parentId ?? o.ParentId ?? null,
    name: o.name ?? o.Name ?? o.label,
    label: o.label ?? o.name ?? o.Name
  })).filter((o: any) => o.id);
});

function normalizeCategory(raw: Record<string, unknown> | CategoryRow): CategoryRow {
  const r = raw as Record<string, unknown>;
  return {
    Id: String(pickField(r, 'Id', 'id') ?? ''),
    Code: String(pickField(r, 'Code', 'code') ?? ''),
    Name: String(pickField(r, 'Name', 'name') ?? ''),
    SortOrder: Number(pickField(r, 'SortOrder', 'sortOrder') ?? 0),
    Remark: String(pickField(r, 'Remark', 'remark') ?? ''),
    AuthType: String(pickField(r, 'AuthType', 'authType') ?? ''),
    AuthCode: String(pickField(r, 'AuthCode', 'authCode') ?? ''),
    AuthId: String(pickField(r, 'AuthId', 'authId') ?? ''),
    Creator: String(pickField(r, 'Creator', 'creator') ?? ''),
    CreatorName: String(pickField(r, 'CreatorName', 'creatorName') ?? ''),
    CreatedAt: pickField<string>(r, 'CreatedAt', 'createdAt')
  };
}

async function loadContext() {
  try {
    const data = await getUnitScopeContext();
    const r = unpackSailResult(data);
    unitContext.value = r.entity || data?.Entity || data?.entity || null;
  } catch {
    unitContext.value = { isPlatformAdmin: false, orgs: [] };
  }
}

async function refresh() {
  loading.value = true;
  err.value = '';
  try {
    const [catData, cntData] = await Promise.all([
      listCategories(filterUnitId.value),
      countByCategory()
    ]);
    if ((catData?.Status ?? catData?.status) === -1) {
      err.value = catData?.Message || '分类加载失败';
      rows.value = [];
    } else {
      const r = unpackSailResult<CategoryRow[]>(catData);
      const list = Array.isArray(r.entity) ? r.entity : [];
      rows.value = list.map((x) => normalizeCategory(x as CategoryRow));
    }
    const cnt = unpackSailResult<Record<string, number>>(cntData);
    counts.value = cnt.entity || {};
  } catch (e: unknown) {
    err.value = e instanceof Error ? e.message : String(e);
  } finally {
    loading.value = false;
  }
}

function onUnitSelect(unitId: string | null) {
  filterUnitId.value = unitId;
  void refresh();
}

async function openCreate(kind: 'PUBLIC' | 'UNIT') {
  fixedScope.value = kind;
  const authType = kind;
  const authId = kind === 'UNIT' ? filterUnitId.value || '' : '';
  let sort = 10;
  try {
    const data = await getNextSort(authType, authId || undefined);
    const r = unpackSailResult<{ sortOrder?: number }>(data);
    sort = Number(r.entity?.sortOrder ?? 10);
  } catch {
    /* ignore */
  }
  Object.assign(draft, {
    Id: undefined,
    Code: '',
    Name: '',
    SortOrder: sort,
    Remark: '',
    AuthType: authType,
    AuthId: authId,
    AuthCode: ''
  });
  dlg.value = true;
}

function openEdit(row: CategoryRow) {
  fixedScope.value = undefined;
  Object.assign(draft, { ...row });
  dlg.value = true;
}

async function onSave() {
  if (!draft.Name?.trim()) {
    ElMessage.warning('请填写名称');
    return;
  }
  if (draft.AuthType === 'UNIT' && !draft.AuthId) {
    ElMessage.warning('请选择所属单位');
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

async function moveRow(index: number, delta: number) {
  const target = index + delta;
  if (target < 0 || target >= rows.value.length) return;
  const next = rows.value.slice();
  const [item] = next.splice(index, 1);
  next.splice(target, 0, item);
  rows.value = next;
  const ids = next.map(r => String(r.Id || '')).filter(Boolean);
  if (!ids.length) return;
  sorting.value = true;
  try {
    const data = await updateCategorySortBatch(ids);
    if ((data?.Status ?? data?.status) === -1) {
      ElMessage.error(data?.Message || '排序保存失败');
      await refresh();
      return;
    }
    next.forEach((r, i) => {
      r.SortOrder = (i + 1) * 10;
    });
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
    await refresh();
  } finally {
    sorting.value = false;
  }
}

onMounted(async () => {
  await loadContext();
  await refresh();
});
</script>

<style scoped>
.sf-formcard-toolbar {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
  align-items: center;
}
.sf-cat-page__layout {
  display: flex;
  flex-direction: row;
  height: 100%;
  min-height: 0;
  padding: 12px;
  gap: 8px;
  box-sizing: border-box;
}
.sf-cat-page__main {
  flex: 1 1 auto;
  min-width: 0;
  min-height: 0;
  display: flex;
  flex-direction: column;
}
.sf-cat-page__alert {
  margin-bottom: 8px;
  flex: 0 0 auto;
}
.sf-cat-page__table {
  flex: 1 1 auto;
  min-height: 0;
}
</style>
