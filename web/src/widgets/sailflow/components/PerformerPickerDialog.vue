<template>
  <el-dialog
    v-model="visible"
    :title="mode === 'notification' ? '添加通知收件人' : '添加参与者'"
    width="640px"
    destroy-on-close
    @closed="onClosed"
  >
    <el-radio-group
      v-if="mode === 'performer'"
      v-model="outerType"
      class="sf-perf__type"
      @change="loadList"
    >
      <el-radio-button value="Role">角色</el-radio-button>
      <el-radio-button value="User">用户</el-radio-button>
    </el-radio-group>
    <el-input
      v-model="keyword"
      clearable
      placeholder="筛选名称 / 编码 / Id"
      class="sf-perf__filter"
    />
    <el-alert v-if="loadErr" :title="loadErr" type="warning" show-icon :closable="false" class="sf-perf__alert" />
    <el-table
      v-loading="loading"
      :data="filtered"
      highlight-current-row
      height="320"
      size="small"
      empty-text="无数据（可检查资源种子）"
      @current-change="onSelect"
      @row-dblclick="row => confirmRow(row)"
    >
      <el-table-column v-if="outerType === 'Role'" prop="Id" label="Id" width="90" show-overflow-tooltip />
      <el-table-column v-if="outerType === 'Role'" prop="RoleName" label="角色名" min-width="140" />
      <el-table-column v-if="outerType === 'Role'" prop="RoleCode" label="编码" min-width="120" />
      <el-table-column v-if="outerType === 'User'" prop="UserId" label="UserId" width="100" show-overflow-tooltip />
      <el-table-column v-if="outerType === 'User'" prop="UserName" label="用户名" min-width="140" />
      <el-table-column v-if="outerType === 'User'" prop="EMail" label="邮箱" min-width="140" show-overflow-tooltip />
    </el-table>
    <template #footer>
      <el-button @click="visible = false">取消</el-button>
      <el-button type="primary" :disabled="!selected" @click="confirm">确定</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { ElMessage } from 'element-plus';
import { designerEvents, type PerformerDraft } from '@sail/bpmn/ui/designerEvents';
import {
  getRoleAll,
  getUserAll,
  unwrapList,
  type RoleRow,
  type UserRow
} from '@sail/api/resource';

defineOptions({ name: 'PerformerPickerDialog' });

const visible = ref(false);
const loading = ref(false);
const loadErr = ref('');
const mode = ref<'performer' | 'notification'>('performer');
const outerType = ref<'Role' | 'User'>('Role');
const keyword = ref('');
const roles = ref<RoleRow[]>([]);
const users = ref<UserRow[]>([]);
const selected = ref<RoleRow | UserRow | null>(null);
let onConfirm: ((draft: PerformerDraft) => void) | null = null;

const filtered = computed(() => {
  const kw = keyword.value.trim().toLowerCase();
  const list = outerType.value === 'Role' ? roles.value : users.value;
  if (!kw) return list;
  return list.filter(row => JSON.stringify(row).toLowerCase().includes(kw));
});

function reset() {
  mode.value = 'performer';
  outerType.value = 'Role';
  keyword.value = '';
  selected.value = null;
  loadErr.value = '';
  onConfirm = null;
}

function onClosed() {
  reset();
}

function onSelect(row: RoleRow | UserRow | null) {
  selected.value = row;
}

async function loadList() {
  selected.value = null;
  loading.value = true;
  loadErr.value = '';
  try {
    if (outerType.value === 'Role') {
      const data = await getRoleAll();
      if (data?.Status === -1 || data?.Status === '-1') {
        throw new Error(data?.Message || '加载角色失败');
      }
      roles.value = unwrapList<RoleRow>(data);
    } else {
      const data = await getUserAll();
      if (data?.Status === -1 || data?.Status === '-1') {
        throw new Error(data?.Message || '加载用户失败');
      }
      users.value = unwrapList<UserRow>(data);
    }
  } catch (e: unknown) {
    loadErr.value = e instanceof Error ? e.message : String(e);
    if (outerType.value === 'Role') roles.value = [];
    else users.value = [];
  } finally {
    loading.value = false;
  }
}

function toDraft(row: RoleRow | UserRow): PerformerDraft | null {
  if (outerType.value === 'Role') {
    const r = row as RoleRow;
    if (!r.Id || !r.RoleName) return null;
    return {
      name: r.RoleName,
      outerId: String(r.Id),
      outerCode: r.RoleCode || '',
      outerType: 'Role'
    };
  }
  const u = row as UserRow;
  if (!u.UserId || !u.UserName) return null;
  return {
    name: u.UserName,
    outerId: String(u.UserId),
    outerCode: '',
    outerType: 'User'
  };
}

function confirmRow(row: RoleRow | UserRow) {
  selected.value = row;
  confirm();
}

function confirm() {
  if (!selected.value) {
    ElMessage.warning('请选择一条记录');
    return;
  }
  const draft = toDraft(selected.value);
  if (!draft) {
    ElMessage.warning('选中项缺少 Id / 名称');
    return;
  }
  onConfirm?.(draft);
  visible.value = false;
}

function openPerformer(payload: { onConfirm: (draft: PerformerDraft) => void }) {
  reset();
  mode.value = 'performer';
  onConfirm = payload.onConfirm;
  visible.value = true;
  loadList();
}

function openNotification(payload: { onConfirm: (draft: PerformerDraft) => void }) {
  reset();
  mode.value = 'notification';
  outerType.value = 'User';
  onConfirm = payload.onConfirm;
  visible.value = true;
  loadList();
}

onMounted(() => {
  designerEvents.on('performer:open', openPerformer);
  designerEvents.on('notification:open', openNotification);
});
onBeforeUnmount(() => {
  designerEvents.off('performer:open', openPerformer);
  designerEvents.off('notification:open', openNotification);
});
</script>

<style scoped>
.sf-perf__type {
  margin-bottom: 10px;
}
.sf-perf__filter {
  margin-bottom: 10px;
}
.sf-perf__alert {
  margin-bottom: 8px;
}
</style>
