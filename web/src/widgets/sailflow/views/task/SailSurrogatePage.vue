<template>
  <formcard title="我的委托" width="" flex stretch>
    <template #toolbar>
      <div class="sf-formcard-toolbar">
        <el-select
          v-model="filterEnabled"
          clearable
          placeholder="状态"
          style="width: 110px"
          @change="reload"
        >
          <el-option label="启用" :value="1" />
          <el-option label="停用" :value="0" />
        </el-select>
        <el-button type="primary" @click="openEdit()">新建委托</el-button>
        <el-button :loading="loading" @click="reload">刷新</el-button>
      </div>
    </template>
    <flexpan :flex="true" height="100%">
      <template #default="p">
        <vclist
          :list="mlist"
          :total="total"
          :para="queryParams"
          :height="p.height"
          :flex="true"
          query
          :orderCol="true"
          :stripe="true"
          empty-text="暂无委托配置；新建后，到您名下的新待办将自动并入代理人"
          v-loading="loading"
          @query="getlist"
        >
          <vccol field="op" caption="操作" width="160px" type="text" disabled align="center" headerAlign="center">
            <template #default="{ row }">
              <el-link type="primary" @click="openEdit(row)">编辑</el-link>
              <el-divider direction="vertical" />
              <el-link type="primary" @click="toggleEnabled(row)">
                {{ Number(row.enabled ?? row.Enabled) === 1 ? '停用' : '启用' }}
              </el-link>
              <el-divider direction="vertical" />
              <el-link type="danger" @click="onDelete(row)">删除</el-link>
            </template>
          </vccol>
          <vccol field="agentName" caption="代理人" min-width="120px" type="text">
            <template #default="{ row }">
              {{ row.agentName || row.AgentName || row.agentId || row.AgentId }}
            </template>
          </vccol>
          <vccol field="processId" caption="流程范围" min-width="160px" type="text">
            <template #default="{ row }">
              {{ processLabel(row.processId ?? row.ProcessId) }}
            </template>
          </vccol>
          <vccol field="enabled" caption="状态" width="80px" type="text" align="center">
            <template #default="{ row }">
              <el-tag :type="Number(row.enabled ?? row.Enabled) === 1 ? 'success' : 'info'" size="small">
                {{ Number(row.enabled ?? row.Enabled) === 1 ? '启用' : '停用' }}
              </el-tag>
            </template>
          </vccol>
          <vccol field="startAt" caption="开始" width="168px" type="datetime" />
          <vccol field="endAt" caption="结束" width="168px" type="datetime" />
          <vccol field="updatedAt" caption="更新" width="168px" type="datetime" />
        </vclist>
      </template>
    </flexpan>

    <el-dialog
      v-model="editVisible"
      :title="form.id ? '编辑委托' : '新建委托'"
      width="520px"
      destroy-on-close
    >
      <el-form label-width="96px">
        <el-form-item label="代理人" required>
          <el-select
            v-model="form.agentId"
            filterable
            remote
            clearable
            placeholder="搜索用户"
            style="width: 100%"
            :remote-method="searchUsers"
            :loading="userLoading"
            @change="onAgentPick"
          >
            <el-option
              v-for="u in userOptions"
              :key="u.UserId"
              :label="`${u.UserName}（${u.UserId}）`"
              :value="u.UserId"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="流程范围">
          <el-select
            v-model="form.processId"
            clearable
            filterable
            placeholder="空=全部流程"
            style="width: 100%"
          >
            <el-option
              v-for="p in processOptions"
              :key="p.id"
              :label="p.name"
              :value="p.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="时间窗">
          <el-date-picker
            v-model="form.range"
            type="datetimerange"
            range-separator="至"
            start-placeholder="开始"
            end-placeholder="结束"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="启用">
          <el-switch v-model="form.enabled" :active-value="1" :inactive-value="0" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="editVisible = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="onSave">保存</el-button>
      </template>
    </el-dialog>
  </formcard>
</template>

<script setup>
import { onMounted, reactive, ref } from 'vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import formcard from '@/ui/panels/formcard.vue';
import flexpan from '@/ui/common/flexpan.vue';
import vclist from '@/ui/common/vclist/vclist.vue';
import vccol from '@/ui/common/vclist/vccol.vue';
import { loadHostCandidateUsers } from '@sail/adapters/hostUsers';
import { getProcessListSimple } from '@sail/api/process';
import {
  deleteSurrogate,
  queryMySurrogatePage,
  saveSurrogate,
  setSurrogateEnabled
} from '@sail/api/surrogate';

defineOptions({ name: 'SailSurrogatePage' });

const loading = ref(false);
const saving = ref(false);
const userLoading = ref(false);
const mlist = ref([]);
const total = ref(0);
const filterEnabled = ref(null);
const queryParams = ref({ pageSize: 20, pageNum: 1 });
const editVisible = ref(false);
const userOptions = ref([]);
const processOptions = ref([]);
const processNameMap = ref({});

const form = reactive({
  id: '',
  agentId: '',
  agentName: '',
  processId: '',
  enabled: 1,
  range: null
});

function processLabel(pid) {
  if (!pid) return '全部流程';
  return processNameMap.value[pid] || pid;
}

const getlist = () => {
  loading.value = true;
  queryMySurrogatePage({
    pageIndex: queryParams.value.pageNum,
    pageSize: queryParams.value.pageSize,
    enabled: filterEnabled.value == null ? null : filterEnabled.value
  })
    .then(res => {
      mlist.value = res?.Entity || res?.entity || res?.records || res?.Records || res?.Items || [];
      total.value = res?.TotalRowsCount ?? res?.total ?? res?.Total ?? 0;
    })
    .catch(err => {
      mlist.value = [];
      total.value = 0;
      ElMessage.error(err?.message || String(err));
    })
    .finally(() => {
      loading.value = false;
    });
};

function reload() {
  queryParams.value.pageNum = 1;
  getlist();
}

async function loadProcesses() {
  try {
    const res = await getProcessListSimple();
    const rows = res?.Entity || res?.entity || res?.Items || res || [];
    const list = Array.isArray(rows) ? rows : [];
    const map = {};
    processOptions.value = list
      .map(r => {
        const id = String(r.processId ?? r.ProcessId ?? r.id ?? r.Id ?? '').trim();
        const name = String(r.processName ?? r.ProcessName ?? r.name ?? r.Name ?? id).trim() || id;
        if (!id) return null;
        map[id] = name;
        return { id, name };
      })
      .filter(Boolean);
    processNameMap.value = map;
  } catch {
    processOptions.value = [];
  }
}

async function searchUsers(keyword) {
  userLoading.value = true;
  try {
    userOptions.value = await loadHostCandidateUsers(keyword, 50);
  } finally {
    userLoading.value = false;
  }
}

function onAgentPick(uid) {
  const u = userOptions.value.find(x => x.UserId === uid);
  form.agentName = u?.UserName || uid || '';
}

function openEdit(row) {
  if (row) {
    form.id = row.id || row.Id || '';
    form.agentId = row.agentId || row.AgentId || '';
    form.agentName = row.agentName || row.AgentName || form.agentId;
    form.processId = row.processId || row.ProcessId || '';
    form.enabled = Number(row.enabled ?? row.Enabled ?? 1) === 1 ? 1 : 0;
    const s = row.startAt || row.StartAt;
    const e = row.endAt || row.EndAt;
    form.range = s || e ? [s ? new Date(s) : null, e ? new Date(e) : null] : null;
    if (form.agentId) {
      userOptions.value = [{ UserId: form.agentId, UserName: form.agentName, Account: form.agentId }];
    }
  } else {
    form.id = '';
    form.agentId = '';
    form.agentName = '';
    form.processId = '';
    form.enabled = 1;
    form.range = null;
  }
  editVisible.value = true;
  if (!userOptions.value.length) searchUsers('');
}

async function onSave() {
  if (!form.agentId) {
    ElMessage.warning('请选择代理人');
    return;
  }
  saving.value = true;
  try {
    const range = form.range || [];
    await saveSurrogate({
      Id: form.id || undefined,
      AgentId: form.agentId,
      AgentName: form.agentName || form.agentId,
      ProcessId: form.processId || null,
      Enabled: form.enabled,
      StartAt: range[0] || null,
      EndAt: range[1] || null,
      Weight: 1
    });
    ElMessage.success('已保存');
    editVisible.value = false;
    getlist();
  } catch (err) {
    ElMessage.error(err?.message || String(err));
  } finally {
    saving.value = false;
  }
}

async function toggleEnabled(row) {
  const id = row.id || row.Id;
  if (!id) return;
  const next = Number(row.enabled ?? row.Enabled) !== 1;
  try {
    await setSurrogateEnabled(id, next);
    getlist();
  } catch (err) {
    ElMessage.error(err?.message || String(err));
  }
}

async function onDelete(row) {
  const id = row.id || row.Id;
  if (!id) return;
  try {
    await ElMessageBox.confirm('确认删除该委托？', '提示', { type: 'warning' });
  } catch {
    return;
  }
  try {
    await deleteSurrogate(id);
    ElMessage.success('已删除');
    getlist();
  } catch (err) {
    ElMessage.error(err?.message || String(err));
  }
}

onMounted(async () => {
  await loadProcesses();
  getlist();
});
</script>

<style scoped>
.sf-formcard-toolbar {
  display: flex;
  gap: 8px;
  align-items: center;
}
</style>
