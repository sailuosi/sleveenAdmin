<template>
  <formcard title="发起流程" width="" flex stretch>
    <template #toolbar>
      <div class="sf-formcard-toolbar">
        <el-button @click="router.push('/sail/forms')">表单设计</el-button>
        <el-button @click="router.push('/sail/task-todo')">返回待办</el-button>
        <el-button :loading="loadingList" @click="refreshAll">刷新</el-button>
      </div>
    </template>
    <div class="sf-create-body">
      <el-alert v-if="err" :title="err" type="error" show-icon :closable="false" class="sf-create-alert" />
      <div class="sf-create-form">
        <!-- 元数据表单：与 SailFormRender 拆开，禁止套进 form-create 的 ElForm -->
        <el-form label-width="100px" @submit.prevent>
          <el-form-item label="流程定义" required>
            <el-select
              v-model="selectedKey"
              filterable
              placeholder="选择流程"
              style="width: 100%"
              @change="onSelectProcess"
            >
              <el-option
                v-for="p in processes"
                :key="procKey(p)"
                :label="procLabel(p)"
                :value="procKey(p)"
              />
            </el-select>
          </el-form-item>
          <el-form-item label="应用名称">
            <el-input v-model="appName" placeholder="默认用流程名称" />
          </el-form-item>
          <el-form-item label="业务单号">
            <el-input v-model="appInstanceId" placeholder="留空则自动生成" />
          </el-form-item>
          <el-form-item label="动态表单">
            <el-select
              v-model="selectedFormId"
              clearable
              filterable
              placeholder="可选：绑定 form-create 表单"
              style="width: 100%"
              @change="onSelectForm"
            >
              <el-option
                v-for="f in forms"
                :key="String(field(f, 'FormId', 'formId'))"
                :label="String(field(f, 'FormName', 'formName'))"
                :value="String(field(f, 'FormId', 'formId'))"
              />
            </el-select>
          </el-form-item>
        </el-form>

        <div v-if="selectedFormId" class="sf-create-form-slot">
          <div class="sf-create-form-slot__label">填写</div>
          <SailFormRender
            ref="formRef"
            v-model="formValues"
            :conf="formConf"
            :fields-json="formFieldsJson"
            :title="formTitle"
          />
        </div>

        <el-form label-width="100px" @submit.prevent>
          <template v-if="nextNodes.length">
            <el-form-item label="下一节点">
              <el-radio-group v-model="nextActivityId">
                <el-radio v-for="n in nextNodes" :key="nodeId(n)" :value="nodeId(n)">
                  {{ nodeName(n) }}
                </el-radio>
              </el-radio-group>
            </el-form-item>
            <el-form-item v-if="nextActivityId" label="下一办理人" :required="needsManualPick">
              <el-select
                v-model="nextUserIds"
                multiple
                filterable
                clearable
                collapse-tags
                collapse-tags-tooltip
                :placeholder="needsManualPick ? '必选：请指定下一办理人' : '可选：覆盖默认候选人'"
                style="width: 100%"
              >
                <el-option
                  v-for="u in candidateUsers"
                  :key="userKey(u)"
                  :label="userLabel(u)"
                  :value="userKey(u)"
                />
              </el-select>
              <div v-if="needsManualPick && !candidateUsers.length" class="sf-create-hint">
                暂无候选人，请刷新或检查人员主数据
              </div>
            </el-form-item>
            <el-form-item v-if="nextUserIds.length" label="组别/身份">
              <div class="sf-create-group-row">
                <el-input v-model="nextGroupId" clearable placeholder="GroupId" />
                <el-select v-model="nextGroupRole" clearable placeholder="GroupRole" style="width: 160px">
                  <el-option
                    v-for="r in groupRoleOptions"
                    :key="r"
                    :label="r"
                    :value="r"
                  />
                </el-select>
              </div>
            </el-form-item>
            <el-form-item v-if="nextUserIds.length" label="单位 Unit*">
              <div class="sf-create-group-row">
                <el-input v-model="nextUnitId" clearable placeholder="UnitId" />
                <el-input v-model="nextUnitCode" clearable placeholder="UnitCode" />
                <el-input v-model="nextUnitName" clearable placeholder="UnitName" />
              </div>
            </el-form-item>
            <el-form-item label="发起单位">
              <div class="sf-create-group-row">
                <el-input v-model="startUnitId" clearable placeholder="StartUnitId" />
                <el-input v-model="startUnitCode" clearable placeholder="StartUnitCode" />
                <el-input v-model="startUnitName" clearable placeholder="StartUnitName" />
              </div>
            </el-form-item>
          </template>
          <el-form-item>
            <el-button type="primary" :loading="submitting" :disabled="!selected" @click="submit">
              提交发起
            </el-button>
          </el-form-item>
        </el-form>
      </div>
    </div>
  </formcard>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage } from 'element-plus';
import formcard from '@/ui/panels/formcard.vue';
import { getProcessListSimple } from '@sail/api/process';
import { getProcessEdit } from '@sail/api/processEdit';
import { getForm, getFormList } from '@sail/api/form';
import { getNextActivityRoleUserTree, startProcess } from '@sail/api/runtime';
import { useSailCurrentUser } from '@sail/adapters/currentUser';
import { loadHostCandidateUsers, type HostCandidateUser } from '@sail/adapters/hostUsers';
import { unpackSailResult, pickField } from '@sail/utils/sailResult';
import { formValuesToConditions } from '@sail/utils/formCreate';
import SailFormRender from '@sail/components/runtime/SailFormRender.vue';

defineOptions({ name: 'SailProcessCreatePage' });

const router = useRouter();
const { userId, userName } = useSailCurrentUser();
const loadingList = ref(false);
const submitting = ref(false);
const err = ref('');
const processes = ref<Record<string, unknown>[]>([]);
const forms = ref<Record<string, unknown>[]>([]);
const selectedKey = ref('');
const selected = ref<Record<string, unknown> | null>(null);
const appName = ref('');
const appInstanceId = ref('');
const selectedFormId = ref('');
const formConf = ref('');
const formFieldsJson = ref('');
const formTitle = ref('');
const formValues = ref<Record<string, unknown>>({});
const formRef = ref<InstanceType<typeof SailFormRender> | null>(null);
const nextNodes = ref<Record<string, unknown>[]>([]);
const nextActivityId = ref('');
const nextUserIds = ref<string[]>([]);
const nextGroupId = ref('');
const nextGroupRole = ref('');
const nextUnitId = ref('');
const nextUnitCode = ref('');
const nextUnitName = ref('');
const startUnitId = ref('');
const startUnitCode = ref('');
const startUnitName = ref('');
const groupRoleOptions = ['Organizer', 'Leader', 'DeputyLeader', 'Member'] as const;
const hostUsers = ref<HostCandidateUser[]>([]);

const selectedNextNode = computed(() =>
  nextNodes.value.find(n => nodeId(n) === nextActivityId.value)
);
/** 下一节点角色树展开的候选人（无则多为 ApproverPick/StarterPick） */
const roleTreeUsers = computed(() => {
  const n = selectedNextNode.value;
  if (!n) return [] as Record<string, unknown>[];
  const users = (pickField<unknown[]>(n, 'Users', 'users') || []) as Record<string, unknown>[];
  return Array.isArray(users) ? users : [];
});
const candidateUsers = computed(() => {
  const raw = roleTreeUsers.value.length
    ? roleTreeUsers.value
    : // R3/ApproverPick：无角色树候选人时用宿主 SysUser.Account
      (hostUsers.value as unknown as Record<string, unknown>[]);
  // 空 value 会让 el-select multiple 选不中 / 触发异常更新
  return raw.filter(u => !!userKey(u));
});
/** 无角色候选人时必须显式选人，否则引擎 R4 拒绝且不再默认塞发起人 */
const needsManualPick = computed(() => !!nextActivityId.value && roleTreeUsers.value.length === 0);

watch(nextActivityId, () => {
  nextUserIds.value = [];
  nextGroupId.value = '';
  nextGroupRole.value = '';
  nextUnitId.value = '';
  nextUnitCode.value = '';
  nextUnitName.value = '';
});

function nodeId(n: Record<string, unknown>) {
  return String(pickField(n, 'ActivityId', 'activityId') || '');
}
function nodeName(n: Record<string, unknown>) {
  return String(pickField(n, 'ActivityName', 'activityName') || nodeId(n));
}
/** R3：选人键优先 Account / UserCode，再回落 UserId */
function userKey(u: Record<string, unknown>) {
  return String(
    pickField(u, 'Account', 'account', 'UserCode', 'userCode', 'UserId', 'userId', 'Id', 'id') || ''
  );
}
function userLabel(u: Record<string, unknown>) {
  return String(pickField(u, 'UserName', 'userName', 'RealName', 'realName', 'Name', 'name') || userKey(u));
}

function field(row: Record<string, unknown>, ...keys: string[]) {
  return pickField(row, ...keys) ?? '';
}
function procKey(p: Record<string, unknown>) {
  return `${pickField(p, 'ProcessId', 'processId')}@${pickField(p, 'Version', 'version') || '1'}`;
}
function procLabel(p: Record<string, unknown>) {
  const name = pickField(p, 'ProcessName', 'processName') || pickField(p, 'ProcessCode', 'processCode');
  return `${name} (v${pickField(p, 'Version', 'version') || '1'})`;
}

async function loadNextNodes(processId: string, version: string) {
  nextNodes.value = [];
  nextActivityId.value = '';
  nextUserIds.value = [];
  nextGroupId.value = '';
  nextGroupRole.value = '';
  try {
    // 发起前：只需 ProcessId/Version（后端从开始事件取首个办理节点）
    const data = await getNextActivityRoleUserTree({
      UserId: userId.value,
      UserName: userName.value,
      ProcessId: processId,
      Version: version,
      Conditions: formValuesToConditions(formValues.value)
    });
    const r = unpackSailResult<Record<string, unknown>[]>(data);
    if ((data?.Status ?? data?.status) === -1) {
      err.value = r.message || '加载下一节点失败';
      nextNodes.value = [];
      return;
    }
    const list = Array.isArray(r.entity) ? r.entity : [];
    // 发起选人优先人工任务（ActivityTypeEnum.TaskNode = 4）
    const tasks = list.filter(n => Number(pickField(n, 'ActivityType', 'activityType')) === 4);
    nextNodes.value = tasks.length ? tasks : list;
    if (nextNodes.value.length) nextActivityId.value = nodeId(nextNodes.value[0]);
  } catch (e: unknown) {
    nextNodes.value = [];
    err.value = e instanceof Error ? e.message : String(e);
  }
}

async function onSelectProcess() {
  selected.value = processes.value.find(p => procKey(p) === selectedKey.value) || null;
  selectedFormId.value = '';
  formValues.value = {};
  formConf.value = '';
  formFieldsJson.value = '';
  formTitle.value = '';
  nextNodes.value = [];
  nextActivityId.value = '';
  nextUserIds.value = [];
  if (!selected.value) return;
  appName.value = String(
    pickField(selected.value, 'ProcessName', 'processName') ||
      pickField(selected.value, 'ProcessCode', 'processCode') ||
      ''
  );
  const pid = String(pickField(selected.value, 'ProcessId', 'processId') || '');
  const ver = String(pickField(selected.value, 'Version', 'version') || '1');
  // 自动带出流程编辑「表单设计」绑定
  try {
    if (!pid) return;
    const data = await getProcessEdit({ ProcessId: pid, Version: ver });
    const r = unpackSailResult<Record<string, unknown>>(data);
    const meta = (pickField(r.entity, 'Meta', 'meta') || {}) as Record<string, unknown>;
    const visible = pickField(meta, 'Visible', 'visible');
    if (visible === 0 || visible === '0' || visible === false) {
      ElMessage.warning('该流程已设为不可见，仍可管理员发起');
    }
    const formType = Number(pickField(meta, 'FormType', 'formType') ?? 10);
    const formId = String(pickField(meta, 'FormId', 'formId') || '');
    const customPath = String(pickField(meta, 'FormCustomCreatePath', 'formCustomCreatePath') || '');
    if (formType === 20 && customPath) {
      ElMessage.info(`业务表单路由：${customPath}`);
      await loadNextNodes(pid, ver);
      return;
    }
    if (formId) {
      selectedFormId.value = formId;
      await onSelectForm();
    }
  } catch {
    /* 无元数据时保持手动选表单 */
  }
  if (pid) await loadNextNodes(pid, ver);
}

async function onSelectForm() {
  formValues.value = {};
  formConf.value = '';
  formFieldsJson.value = '';
  formTitle.value = '';
  if (!selectedFormId.value) return;
  try {
    const data = await getForm(selectedFormId.value);
    const r = unpackSailResult<Record<string, unknown>>(data);
    if ((data?.Status ?? data?.status) === -1) {
      ElMessage.error(r.message || '加载表单失败');
      return;
    }
    const entity = r.entity || {};
    formConf.value = String(pickField(entity, 'Conf', 'conf') || '{}');
    formFieldsJson.value = String(pickField(entity, 'FieldsJson', 'fieldsJson') || '[]');
    formTitle.value = String(pickField(entity, 'FormName', 'formName') || '');
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  }
}

async function loadProcesses() {
  const data = await getProcessListSimple();
  const r = unpackSailResult<Record<string, unknown>[]>(data);
  if ((data?.Status ?? data?.status) === -1) throw new Error(r.message || '加载流程失败');
  processes.value = Array.isArray(r.entity) ? r.entity : [];
}

async function loadForms() {
  const data = await getFormList();
  const r = unpackSailResult<Record<string, unknown>[]>(data);
  if ((data?.Status ?? data?.status) === -1) {
    forms.value = [];
    return;
  }
  forms.value = Array.isArray(r.entity) ? r.entity : [];
}

async function refreshAll() {
  loadingList.value = true;
  err.value = '';
  try {
    const [, , users] = await Promise.all([
      loadProcesses(),
      loadForms(),
      loadHostCandidateUsers()
    ]);
    hostUsers.value = users;
  } catch (e: unknown) {
    err.value = e instanceof Error ? e.message : String(e);
  } finally {
    loadingList.value = false;
  }
}

async function submit() {
  if (!selected.value) return;
  submitting.value = true;
  err.value = '';
  try {
    if (selectedFormId.value && formRef.value) {
      try {
        await formRef.value.validate();
      } catch {
        ElMessage.warning('请完善表单');
        return;
      }
    }
    if (!nextNodes.value.length) {
      ElMessage.warning('未能加载下一节点，请刷新后重试');
      return;
    }
    // R4/R6：ApproverPick 等无角色候选人时必须显式选人（引擎不再默认塞发起人）
    if (needsManualPick.value && !nextUserIds.value.length) {
      ElMessage.warning(candidateUsers.value.length ? '请选择下一办理人' : '暂无候选人，无法发起');
      return;
    }
    const processId = String(pickField(selected.value, 'ProcessId', 'processId') || '');
    const version = String(pickField(selected.value, 'Version', 'version') || '1');
    const instanceId = appInstanceId.value.trim() || `APP-${Date.now()}`;
    // 优先读 form-create 内部值，避免 v-model 未及时同步导致 Conditions 为空
    const rawForm =
      (formRef.value?.getValue?.() as Record<string, unknown> | undefined) || formValues.value;
    formValues.value = rawForm || {};
    const conditions = formValuesToConditions(rawForm);
    const payload: Parameters<typeof startProcess>[0] = {
      UserId: userId.value,
      UserName: userName.value,
      ProcessId: processId,
      Version: version,
      AppName: appName.value || 'Sailflow',
      AppInstanceId: instanceId,
      // 约定：AppInstanceCode 存 formId，办理页 Workbench 可回取表单
      AppInstanceCode: selectedFormId.value || undefined,
      Conditions: conditions,
      StartUnitId: startUnitId.value.trim() || undefined,
      StartUnitCode: startUnitCode.value.trim() || undefined,
      StartUnitName: startUnitName.value.trim() || undefined
    };
    if (nextActivityId.value && nextUserIds.value.length) {
      const gid = nextGroupId.value.trim();
      const role = nextGroupRole.value.trim();
      const unitId = nextUnitId.value.trim();
      const unitCode = nextUnitCode.value.trim();
      const unitName = nextUnitName.value.trim();
      payload.NextActivityPerformers = {
        [nextActivityId.value]: nextUserIds.value.map(uid => {
          const u = candidateUsers.value.find(x => userKey(x) === uid);
          const p: {
            UserId: string;
            UserName: string;
            GroupId?: string;
            GroupRole?: string;
            UnitId?: string;
            UnitCode?: string;
            UnitName?: string;
          } = {
            UserId: uid,
            UserName: u ? userLabel(u) : uid
          };
          if (gid) p.GroupId = gid;
          if (role) p.GroupRole = role;
          if (unitId) p.UnitId = unitId;
          if (unitCode) p.UnitCode = unitCode;
          if (unitName) p.UnitName = unitName;
          return p;
        })
      };
      payload.NextPerformerType = 0; // Specific
    }
    const data = await startProcess(payload);
    const r = unpackSailResult(data);
    if ((data?.Status ?? data?.status) === -1) {
      err.value = r.message || '发起失败';
      ElMessage.error(err.value);
      return;
    }
    ElMessage.success(r.message || '发起成功');
    router.push({ path: '/sail/task-todo' });
  } catch (e: unknown) {
    err.value = e instanceof Error ? e.message : String(e);
    ElMessage.error(err.value);
  } finally {
    submitting.value = false;
  }
}

onMounted(refreshAll);
</script>

<style scoped>
.sf-create-group-row {
  display: flex;
  gap: 8px;
  width: 100%;
}
.sf-create-hint {
  margin-top: 4px;
  font-size: 12px;
  color: var(--el-color-warning);
}
.sf-formcard-toolbar {
  display: flex;
  gap: 8px;
  align-items: center;
  flex-wrap: wrap;
}
.sf-create-body {
  padding: 16px;
  height: 100%;
  overflow: auto;
  box-sizing: border-box;
}
.sf-create-alert {
  margin-bottom: 12px;
}
.sf-create-form {
  max-width: 760px;
  padding: 20px;
  background: var(--el-bg-color);
  border-radius: 8px;
}
.sf-create-form-slot {
  display: flex;
  gap: 12px;
  margin-bottom: 18px;
}
.sf-create-form-slot__label {
  width: 100px;
  flex-shrink: 0;
  text-align: right;
  line-height: 32px;
  color: var(--el-text-color-regular);
}
.sf-create-form-slot__label::after {
  content: '';
}
.sf-create-form-slot :deep(.sf-form-render) {
  flex: 1;
  min-width: 0;
}
</style>
