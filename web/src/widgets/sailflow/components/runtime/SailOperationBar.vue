<template>
  <div class="sf-op-bar">
    <div class="sf-op-bar__actions">
      <template v-if="canHandle">
        <el-popover
          v-if="showApprove && btnOn('Approve')"
          :visible="approveVisible"
          placement="top-end"
          :width="440"
          trigger="click"
        >
          <template #reference>
            <el-button type="success" plain :loading="loading" @click="approveVisible = true">
              {{ approveActionLabel }}
            </el-button>
          </template>
          <div class="sf-op-pop">
            <el-form label-position="top" @submit.prevent>
              <el-form-item label="办理意见">
                <el-input
                  v-model="comment"
                  type="textarea"
                  :rows="3"
                  placeholder="请输入办理意见（可选）"
                />
              </el-form-item>
              <el-form-item v-if="nextNodes.length" label="下一节点" required>
                <div v-if="nextNodes.length === 1" class="sf-op-pop__plain">
                  {{ nodeName(nextNodes[0]) }}
                </div>
                <el-radio-group v-else v-model="selectedActivityId">
                  <el-radio
                    v-for="n in nextNodes"
                    :key="nodeId(n)"
                    :value="nodeId(n)"
                    :label="nodeId(n)"
                  >
                    {{ nodeName(n) }}
                  </el-radio>
                </el-radio-group>
              </el-form-item>
              <el-form-item
                v-if="selectedActivityId && selectedNeedsPerformer"
                :label="needsManualPick ? '下一办理人（必选）' : '下一办理人'"
                :required="needsManualPick"
              >
                <el-select
                  v-model="selectedUserIds"
                  multiple
                  filterable
                  clearable
                  :placeholder="needsManualPick ? '请选择下一办理人' : '可选'"
                  style="width: 100%"
                >
                  <el-option
                    v-for="u in candidateUsers"
                    :key="userKey(u)"
                    :label="userLabel(u)"
                    :value="userKey(u)"
                  />
                </el-select>
              </el-form-item>
              <el-collapse v-if="selectedNeedsPerformer && selectedUserIds.length" class="sf-op-pop__adv">
                <el-collapse-item title="高级：组别 / 单位" name="adv">
                  <div class="sf-op-bar__group-row">
                    <el-input v-model="nextGroupId" clearable placeholder="GroupId" />
                    <el-select
                      v-model="nextGroupRole"
                      clearable
                      placeholder="GroupRole"
                      style="width: 140px"
                    >
                      <el-option v-for="r in groupRoleOptions" :key="r" :label="r" :value="r" />
                    </el-select>
                  </div>
                  <div class="sf-op-bar__group-row" style="margin-top: 8px">
                    <el-input v-model="nextUnitId" clearable placeholder="UnitId" />
                    <el-input v-model="nextUnitCode" clearable placeholder="UnitCode" />
                    <el-input v-model="nextUnitName" clearable placeholder="UnitName" />
                  </div>
                </el-collapse-item>
              </el-collapse>
              <div class="sf-op-pop__footer">
                <el-button @click="approveVisible = false">取消</el-button>
                <el-button type="success" :loading="loading" @click="onApprove">
                  {{ approveActionLabel }}
                </el-button>
              </div>
            </el-form>
          </div>
        </el-popover>
        <el-popover
          v-if="btnOn('Reject')"
          :visible="rejectVisible"
          placement="top-end"
          :width="420"
          trigger="click"
        >
          <template #reference>
            <el-button type="danger" plain :loading="loading" @click="rejectVisible = true">
              {{ btnLabel('Reject') }}
            </el-button>
          </template>
          <div class="sf-op-pop">
            <el-form label-position="top" @submit.prevent>
              <el-form-item label="驳回意见" required>
                <el-input
                  v-model="rejectComment"
                  type="textarea"
                  :rows="3"
                  placeholder="请输入驳回意见"
                />
              </el-form-item>
              <div class="sf-op-pop__footer">
                <el-button @click="rejectVisible = false">取消</el-button>
                <el-button type="danger" :loading="loading" @click="onReject">
                  {{ btnLabel('Reject') }}
                </el-button>
              </div>
            </el-form>
          </div>
        </el-popover>
        <el-button
          v-if="btnOn('SendBack')"
          plain
          :loading="loading"
          :disabled="!previousNodes.length"
          @click="sendBackVisible = true"
        >
          {{ btnLabel('SendBack') }}
        </el-button>
        <el-button
          v-if="btnOn('SignForward')"
          plain
          :loading="loading"
          @click="signVisible = true"
        >
          {{ btnLabel('SignForward') }}
        </el-button>
        <el-button
          v-if="btnOn('Transfer')"
          plain
          :loading="loading"
          @click="transferVisible = true"
        >
          {{ btnLabel('Transfer') }}
        </el-button>
        <el-button v-if="btnOn('Copy')" plain :loading="loading" @click="copyVisible = true">
          {{ btnLabel('Copy') }}
        </el-button>
        <el-button v-if="btnOn('Revise')" plain :loading="loading" @click="onRevise">
          {{ btnLabel('Revise') }}
        </el-button>
      </template>
      <el-button v-if="showJump && btnOn('Jump')" plain :loading="loading" @click="openJump">
        {{ btnLabel('Jump') }}
      </el-button>
      <el-button
        v-if="showWithdraw && btnOn('Withdraw')"
        plain
        :loading="loading"
        @click="onWithdraw"
      >
        {{ btnLabel('Withdraw') }}
      </el-button>
      <el-button
        v-if="showResend && btnOn('Resend')"
        :type="hasBackSrc ? 'primary' : 'default'"
        plain
        :loading="loading"
        @click="onResend"
      >
        {{ resendActionLabel }}
      </el-button>
      <el-button
        v-if="showClose && btnOn('Close')"
        type="warning"
        plain
        :loading="loading"
        @click="onClose"
      >
        {{ btnLabel('Close') }}
      </el-button>
      <el-button
        v-if="showClose && btnOn('Terminate')"
        type="danger"
        plain
        :loading="loading"
        @click="onTerminate"
      >
        {{ btnLabel('Terminate') }}
      </el-button>
      <el-button
        v-if="showClose && btnOn('Discard')"
        type="danger"
        plain
        :loading="loading"
        @click="onDiscard"
      >
        {{ btnLabel('Discard') }}
      </el-button>
      <el-button
        v-if="showClose && btnOn('Reverse')"
        type="warning"
        plain
        :loading="loading"
        @click="onReverse"
      >
        {{ btnLabel('Reverse') }}
      </el-button>
    </div>

    <!-- 退回 -->
    <el-dialog v-model="sendBackVisible" title="退回" width="480px" destroy-on-close>
      <el-form label-width="90px">
        <el-form-item label="退回节点" required>
          <el-radio-group v-model="sendBackActivityId">
            <el-radio
              v-for="n in previousNodes"
              :key="nodeId(n)"
              :value="nodeId(n)"
              :label="nodeId(n)"
            >
              {{ nodeName(n) }}
            </el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item v-if="sendBackUsers.length" label="接收人">
          <el-select v-model="sendBackUserIds" multiple filterable style="width: 100%">
            <el-option
              v-for="u in sendBackUsers"
              :key="userKey(u)"
              :label="userLabel(u)"
              :value="userKey(u)"
            />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="sendBackVisible = false">取消</el-button>
        <el-button type="primary" :loading="loading" @click="onSendBack">确认退回</el-button>
      </template>
    </el-dialog>

    <!-- 加签 -->
    <el-dialog v-model="signVisible" title="加签" width="480px" destroy-on-close>
      <el-form label-width="90px">
        <el-form-item label="加签类型">
          <el-radio-group v-model="signForwardType">
            <el-radio value="Parallel">并行</el-radio>
            <el-radio value="Serial">串行</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="加签人" required>
          <el-select
            v-model="signUserIds"
            multiple
            filterable
            style="width: 100%"
            placeholder="选择用户"
          >
            <el-option
              v-for="u in allUsers"
              :key="userKey(u)"
              :label="userLabel(u)"
              :value="userKey(u)"
            />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="signVisible = false">取消</el-button>
        <el-button type="primary" :loading="loading" @click="onSignForward">确认加签</el-button>
      </template>
    </el-dialog>

    <!-- 转办：本节点交人，不流转 -->
    <el-dialog v-model="transferVisible" :title="btnLabel('Transfer')" width="480px" destroy-on-close>
      <el-form label-width="90px">
        <el-form-item label="转办给" required>
          <el-select
            v-model="transferUserIds"
            multiple
            filterable
            style="width: 100%"
            placeholder="选择接收人（本人待办将结束）"
          >
            <el-option
              v-for="u in transferCandidates"
              :key="userKey(u)"
              :label="userLabel(u)"
              :value="userKey(u)"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="意见">
          <el-input v-model="transferComment" type="textarea" :rows="2" placeholder="可选" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="transferVisible = false">取消</el-button>
        <el-button type="primary" :loading="loading" @click="onTransfer">确认转办</el-button>
      </template>
    </el-dialog>

    <!-- 跳转（管理） -->
    <el-dialog v-model="jumpVisible" title="跳转" width="480px" destroy-on-close>
      <el-form label-width="90px">
        <el-form-item label="目标节点" required>
          <el-select
            v-model="jumpActivityId"
            filterable
            clearable
            placeholder="选择人工任务节点"
            style="width: 100%"
          >
            <el-option
              v-for="a in jumpActivities"
              :key="String(a.ActivityId || a.activityId)"
              :label="String(a.ActivityName || a.activityName || a.ActivityId || a.activityId)"
              :value="String(a.ActivityId || a.activityId)"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="办理人" required>
          <el-select
            v-model="jumpUserIds"
            multiple
            filterable
            style="width: 100%"
            placeholder="选择办理人"
          >
            <el-option
              v-for="u in allUsers"
              :key="userKey(u)"
              :label="userLabel(u)"
              :value="userKey(u)"
            />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="jumpVisible = false">取消</el-button>
        <el-button type="primary" :loading="loading" @click="onJump">确认跳转</el-button>
      </template>
    </el-dialog>

    <!-- 抄送 -->
    <el-dialog v-model="copyVisible" title="抄送" width="480px" destroy-on-close>
      <el-form label-width="90px">
        <el-form-item label="抄送人" required>
          <el-select
            v-model="copyUserIds"
            multiple
            filterable
            style="width: 100%"
            placeholder="选择用户"
          >
            <el-option
              v-for="u in allUsers"
              :key="userKey(u)"
              :label="userLabel(u)"
              :value="userKey(u)"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="原因">
          <el-input v-model="copyReason" type="textarea" :rows="2" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="copyVisible = false">取消</el-button>
        <el-button type="primary" :loading="loading" @click="onCopy">确认抄送</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import {
  buttonsOperation,
  createProcessCopy,
  getTaskActivityList,
  type FlowCommandContextPayload
} from '@sail/api/runtime';
import { loadHostCandidateUsers, type HostCandidateUser } from '@sail/adapters/hostUsers';
import { useSailCurrentUser } from '@sail/adapters/currentUser';
import { unpackSailResult, pickField } from '@sail/utils/sailResult';
import { formValuesToConditionsForSubmit } from '@sail/utils/formCreate';
import {
  buttonDisplayName,
  isButtonEnabled,
  mergeButtonsSetting,
  type SailButtonSetting
} from '@sail/bpmn/buttonsCatalog';

const props = withDefaults(
  defineProps<{
    task: Record<string, unknown>;
    processInstance?: Record<string, unknown> | null;
    nextNodes?: Record<string, unknown>[];
    previousNodes?: Record<string, unknown>[];
    formValues?: Record<string, unknown>;
    /** §C：field → editable|readonly|hidden|masked；有配置时仅 editable 写入 Conditions */
    fieldModes?: Record<string, string> | null;
    formRef?: { validate?: () => Promise<boolean> } | null;
    canHandle?: boolean;
    /** §J：策略允许「正常发送/通过」时为 true */
    showApprove?: boolean;
    showWithdraw?: boolean;
    showResend?: boolean;
    showClose?: boolean;
    /** 管理跳转（JumpProcess） */
    showJump?: boolean;
    /** 当前活动存在退回源（可返送）· §G */
    hasBackSrc?: boolean;
    /** 节点 sf:ButtonsSetting（Workbench 下发） */
    buttonsSetting?: SailButtonSetting[] | null;
  }>(),
  {
    canHandle: true,
    showApprove: true,
    showWithdraw: false,
    showResend: false,
    showClose: false,
    showJump: false,
    hasBackSrc: false,
    buttonsSetting: null,
    fieldModes: null
  }
);

const emit = defineEmits<{ done: []; failed: [string] }>();

const { userId, userName } = useSailCurrentUser();
const loading = ref(false);
const comment = ref('');
const rejectComment = ref('');
const approveVisible = ref(false);
const rejectVisible = ref(false);
const selectedActivityId = ref('');
const selectedUserIds = ref<string[]>([]);
const nextGroupId = ref('');
const nextGroupRole = ref('');
const nextUnitId = ref('');
const nextUnitCode = ref('');
const nextUnitName = ref('');
const groupRoleOptions = ['Organizer', 'Leader', 'DeputyLeader', 'Member'] as const;
/** R3：宿主 SysUser（Account）；ApproverPick 时 NextNodes.Users 为空，用此回落 */
const allUsers = ref<HostCandidateUser[]>([]);

const sendBackVisible = ref(false);
const sendBackActivityId = ref('');
const sendBackUserIds = ref<string[]>([]);
const signVisible = ref(false);
const signForwardType = ref('Parallel');
const signUserIds = ref<string[]>([]);
const transferVisible = ref(false);
const transferUserIds = ref<string[]>([]);
const transferComment = ref('');
const copyVisible = ref(false);
const copyUserIds = ref<string[]>([]);
const copyReason = ref('');
const jumpVisible = ref(false);
const jumpActivityId = ref('');
const jumpUserIds = ref<string[]>([]);
const jumpActivities = ref<Record<string, unknown>[]>([]);

const buttons = computed(() => mergeButtonsSetting(props.buttonsSetting));
function btnOn(id: string) {
  return isButtonEnabled(buttons.value, id);
}
function btnLabel(id: string) {
  return buttonDisplayName(buttons.value, id);
}

/** §G：被退回后再发送 —— 正常发送 vs 返送退回人 */
const approveActionLabel = computed(() =>
  props.hasBackSrc && props.canHandle ? '正常发送' : btnLabel('Approve')
);
const resendActionLabel = computed(() =>
  props.hasBackSrc ? '返送退回人' : btnLabel('Resend')
);

const nextNodes = computed(() => props.nextNodes || []);
const previousNodes = computed(() => props.previousNodes || []);

watch(
  nextNodes,
  list => {
    if (list.length && !selectedActivityId.value) selectedActivityId.value = nodeId(list[0]);
  },
  { immediate: true }
);

watch(
  previousNodes,
  list => {
    if (list.length && !sendBackActivityId.value) sendBackActivityId.value = nodeId(list[0]);
  },
  { immediate: true }
);

const selectedNode = computed(() =>
  nextNodes.value.find(n => nodeId(n) === selectedActivityId.value)
);
/** ActivityTypeEnum：TaskNode=4 / MultiSignNode=6 / SubProcessNode=5 需办理人；EndNode=2 等不需要 */
function nodeActivityType(n: Record<string, unknown> | undefined | null) {
  return Number(pickField(n || {}, 'ActivityType', 'activityType') || 0);
}
function nodeNeedsPerformer(n: Record<string, unknown> | undefined | null) {
  const t = nodeActivityType(n);
  return t === 4 || t === 5 || t === 6;
}
const selectedNeedsPerformer = computed(() => nodeNeedsPerformer(selectedNode.value));
const roleTreeUsers = computed(() => {
  const n = selectedNode.value;
  if (!n) return [] as Record<string, unknown>[];
  const users = (pickField<unknown[]>(n, 'Users', 'users') || []) as Record<string, unknown>[];
  return Array.isArray(users) ? users : [];
});
const candidateUsers = computed(() => {
  if (roleTreeUsers.value.length) return roleTreeUsers.value;
  // R3/ApproverPick：节点无角色候选人时回落宿主用户（Account）
  return allUsers.value as unknown as Record<string, unknown>[];
});
// 结束/网关/服务等无候选人节点不得要求选人（原先空 Users → 误判必选）
const needsManualPick = computed(
  () =>
    !!selectedActivityId.value &&
    selectedNeedsPerformer.value &&
    roleTreeUsers.value.length === 0
);

const sendBackNode = computed(() =>
  previousNodes.value.find(n => nodeId(n) === sendBackActivityId.value)
);
const sendBackUsers = computed(() => {
  const n = sendBackNode.value;
  if (!n) return [] as Record<string, unknown>[];
  const users = (pickField<unknown[]>(n, 'Users', 'users') || []) as Record<string, unknown>[];
  return Array.isArray(users) ? users : [];
});

watch(selectedActivityId, () => {
  selectedUserIds.value = [];
  nextGroupId.value = '';
  nextGroupRole.value = '';
  nextUnitId.value = '';
  nextUnitCode.value = '';
  nextUnitName.value = '';
});
watch(sendBackActivityId, () => {
  sendBackUserIds.value = [];
});

function nodeId(n: Record<string, unknown>) {
  return String(pickField(n, 'ActivityId', 'activityId') || '');
}
function nodeName(n: Record<string, unknown>) {
  return String(pickField(n, 'ActivityName', 'activityName') || nodeId(n));
}
/** R3：选人键优先 Account */
function userKey(u: Record<string, unknown>) {
  return String(
    pickField(u, 'Account', 'account', 'UserCode', 'userCode', 'UserId', 'userId', 'Id', 'id') || ''
  );
}
function userLabel(u: Record<string, unknown>) {
  return String(
    pickField(u, 'UserName', 'userName', 'RealName', 'realName', 'Name', 'name') || userKey(u)
  );
}

function buildRunner(extra?: Partial<FlowCommandContextPayload>): FlowCommandContextPayload {
  const t = props.task;
  // 对齐 szkfpt：有字段权限时仅合并 editable；无配置则全量
  const conditions = {
    ...formValuesToConditionsForSubmit(props.formValues, props.fieldModes),
    ...(comment.value ? { comment: comment.value } : {})
  };
  return {
    UserId: userId.value,
    UserName: userName.value,
    AppName: String(pickField(t, 'AppName', 'appName') || ''),
    AppInstanceId: String(pickField(t, 'AppInstanceId', 'appInstanceId') || ''),
    ProcessId: String(pickField(t, 'ProcessId', 'processId') || ''),
    Version: String(pickField(t, 'Version', 'version') || '1'),
    TaskId: String(pickField(t, 'TaskId', 'taskId', 'Id', 'id') || '').trim() || null,
    ActivityInstanceId:
      String(pickField(t, 'ActivityInstanceId', 'activityInstanceId') || '').trim() || null,
    Conditions: conditions,
    ...extra
  };
}

type NextPerformerPayload = {
  UserId: string;
  UserName: string;
  GroupId?: string;
  GroupRole?: string;
  UnitId?: string;
  UnitCode?: string;
  UnitName?: string;
};

function withNextPerformers(
  activityId: string,
  userIds: string[],
  pool: Record<string, unknown>[],
  group?: {
    groupId?: string;
    groupRole?: string;
    unitId?: string;
    unitCode?: string;
    unitName?: string;
  }
): Record<string, NextPerformerPayload[]> {
  const gid = group?.groupId?.trim();
  const role = group?.groupRole?.trim();
  const unitId = group?.unitId?.trim();
  const unitCode = group?.unitCode?.trim();
  const unitName = group?.unitName?.trim();
  const performers = userIds.map(uid => {
    const u = pool.find(x => userKey(x) === uid);
    const p: NextPerformerPayload = {
      UserId: uid,
      UserName: u ? userLabel(u) : uid
    };
    if (gid) p.GroupId = gid;
    if (role) p.GroupRole = role;
    if (unitId) p.UnitId = unitId;
    if (unitCode) p.UnitCode = unitCode;
    if (unitName) p.UnitName = unitName;
    return p;
  });
  return { [activityId]: performers };
}

/** 统一办理入口：码表 ButtonId → Host ButtonsOperation → Handler */
async function execButton(buttonId: string, runner: FlowCommandContextPayload, okMsg: string) {
  loading.value = true;
  try {
    const data = await buttonsOperation({ ButtonId: buttonId, Runner: runner });
    const r = unpackSailResult(data);
    if ((data?.Status ?? data?.status) === -1) {
      const extra = (data?.ExtraData ?? data?.extraData ?? r.entity ?? {}) as Record<string, unknown>;
      const reason = String(extra?.reasonCode ?? extra?.ReasonCode ?? '').trim();
      const msg = reason
        ? `${r.message || '操作失败'}（${reason}）`
        : r.message || '操作失败';
      ElMessage.error(msg);
      emit('failed', msg);
      return false;
    }
    ElMessage.success(r.message || okMsg);
    emit('done');
    return true;
  } catch (e: unknown) {
    const msg = e instanceof Error ? e.message : String(e);
    ElMessage.error(msg);
    emit('failed', msg);
    return false;
  } finally {
    loading.value = false;
  }
}

async function onApprove() {
  if (props.formRef?.validate) {
    try {
      await props.formRef.validate();
    } catch {
      ElMessage.warning('请完善表单');
      return;
    }
  }
  if (nextNodes.value.length && !selectedActivityId.value) {
    ElMessage.warning('请选择下一节点');
    return;
  }
  if (needsManualPick.value && !selectedUserIds.value.length) {
    ElMessage.warning(candidateUsers.value.length ? '请选择下一办理人' : '暂无候选人，无法提交');
    return;
  }
  const extra: Partial<FlowCommandContextPayload> = {};
  if (selectedActivityId.value && selectedUserIds.value.length) {
    extra.NextActivityPerformers = withNextPerformers(
      selectedActivityId.value,
      selectedUserIds.value,
      candidateUsers.value,
      {
        groupId: nextGroupId.value,
        groupRole: nextGroupRole.value,
        unitId: nextUnitId.value,
        unitCode: nextUnitCode.value,
        unitName: nextUnitName.value
      }
    );
  } else if (selectedActivityId.value && !selectedNeedsPerformer.value) {
    // 结束等非人工节点：引擎用 NextActivityPerformers.ContainsKey 匹配后续，须占位键（可空列表）
    extra.NextActivityPerformers = { [selectedActivityId.value]: [] };
  }
  const ok = await execButton('Approve', buildRunner(extra), '已通过');
  if (ok) approveVisible.value = false;
}

async function onReject() {
  if (!rejectComment.value.trim()) {
    ElMessage.warning('请输入驳回意见');
    return;
  }
  const runner = buildRunner();
  runner.Conditions = {
    ...(runner.Conditions || {}),
    comment: rejectComment.value.trim()
  };
  const ok = await execButton('Reject', runner, '已驳回');
  if (ok) {
    rejectVisible.value = false;
    rejectComment.value = '';
  }
}

async function onSendBack() {
  if (!sendBackActivityId.value) {
    ElMessage.warning('请选择退回节点');
    return;
  }
  let userIds = sendBackUserIds.value;
  if (!userIds.length && sendBackUsers.value.length) {
    userIds = [userKey(sendBackUsers.value[0])];
  }
  if (!userIds.length) {
    // 无候选人列表时用当前用户占位，引擎也可能自解析
    userIds = [userId.value];
  }
  const ok = await execButton(
    'SendBack',
    buildRunner({
      NextActivityPerformers: withNextPerformers(
        sendBackActivityId.value,
        userIds,
        sendBackUsers.value.length ? sendBackUsers.value : allUsers.value
      )
    }),
    '已退回'
  );
  if (ok) sendBackVisible.value = false;
}

async function onSignForward() {
  if (!signUserIds.value.length) {
    ElMessage.warning('请选择加签人');
    return;
  }
  const activityId = String(
    pickField(props.task, 'ActivityId', 'activityId') || selectedActivityId.value || 'sign'
  );
  const ok = await execButton(
    'SignForward',
    buildRunner({
      ControlParameterSheet: {
        SignForwardType: signForwardType.value,
        SignForwardCompareType: 'count',
        SignForwardCompleteOrder: 1
      },
      NextActivityPerformers: withNextPerformers(activityId, signUserIds.value, allUsers.value)
    }),
    '已加签'
  );
  if (ok) signVisible.value = false;
}

const transferCandidates = computed(() =>
  allUsers.value.filter(u => userKey(u) !== userId.value)
);

async function onTransfer() {
  if (!transferUserIds.value.length) {
    ElMessage.warning('请选择转办接收人');
    return;
  }
  const activityId = String(
    pickField(props.task, 'ActivityId', 'activityId') || selectedActivityId.value || 'transfer'
  );
  const runner = buildRunner({
    NextActivityPerformers: withNextPerformers(
      activityId,
      transferUserIds.value,
      allUsers.value
    )
  });
  if (transferComment.value.trim()) {
    runner.Conditions = {
      ...(runner.Conditions || {}),
      comment: transferComment.value.trim()
    };
  }
  const ok = await execButton('Transfer', runner, '已转办');
  if (ok) {
    transferVisible.value = false;
    transferUserIds.value = [];
    transferComment.value = '';
  }
}

async function onCopy() {
  if (!copyUserIds.value.length) {
    ElMessage.warning('请选择抄送人');
    return;
  }
  loading.value = true;
  try {
    const t = props.task;
    const pi = props.processInstance;
    const data = await createProcessCopy({
      Targets: copyUserIds.value.map(uid => {
        const u = allUsers.value.find(x => userKey(x) === uid);
        return { UserId: uid, UserName: u ? userLabel(u) : uid };
      }),
      ProcessInstanceId: String(
        pickField(t, 'ProcessInstanceId', 'processInstanceId') ||
          pickField(pi || {}, 'ProcessInstanceId', 'processInstanceId', 'Id', 'id') ||
          ''
      ),
      ProcessId: String(pickField(t, 'ProcessId', 'processId') || ''),
      ProcessName: String(pickField(pi || {}, 'ProcessName', 'processName') || ''),
      AppInstanceId: String(pickField(t, 'AppInstanceId', 'appInstanceId') || ''),
      TaskId: String(pickField(t, 'TaskId', 'taskId', 'Id', 'id') || ''),
      TaskName: String(pickField(t, 'ActivityName', 'activityName') || ''),
      StartUserId: String(pickField(pi || {}, 'CreatedUserId', 'createdUserId') || ''),
      CreatorId: userId.value,
      CreatorName: userName.value,
      Reason: copyReason.value || comment.value
    });
    if ((data?.Status ?? data?.status) === -1) {
      ElMessage.error(data?.Message || data?.message || '抄送失败');
      return;
    }
    ElMessage.success('已抄送');
    copyVisible.value = false;
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  } finally {
    loading.value = false;
  }
}

async function openJump() {
  jumpActivityId.value = '';
  jumpUserIds.value = [];
  jumpActivities.value = [];
  jumpVisible.value = true;
  try {
    const data = await getTaskActivityList({
      ProcessId: String(pickField(props.task, 'ProcessId', 'processId') || ''),
      Version: String(pickField(props.task, 'Version', 'version') || '1')
    });
    const r = unpackSailResult<Record<string, unknown>[]>(data);
    if ((data?.Status ?? data?.status) === -1) {
      ElMessage.error(r.message || '加载跳转节点失败');
      return;
    }
    const list = r.entity;
    jumpActivities.value = Array.isArray(list) ? list : [];
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  }
}

async function onJump() {
  if (!jumpActivityId.value) {
    ElMessage.warning('请选择目标节点');
    return;
  }
  if (!jumpUserIds.value.length) {
    ElMessage.warning('请选择办理人');
    return;
  }
  const ok = await execButton(
    'Jump',
    buildRunner({
      NextActivityPerformers: withNextPerformers(
        jumpActivityId.value,
        jumpUserIds.value,
        allUsers.value
      )
    }),
    '已跳转'
  );
  if (ok) jumpVisible.value = false;
}

async function onRevise() {
  try {
    await ElMessageBox.confirm('确认修订当前任务？', btnLabel('Revise'), { type: 'warning' });
  } catch {
    return;
  }
  await execButton('Revise', buildRunner(), '已修订');
}

async function onReverse() {
  try {
    await ElMessageBox.confirm(
      '反转与退回语义不同，确认执行？',
      btnLabel('Reverse'),
      { type: 'warning' }
    );
  } catch {
    return;
  }
  await execButton('Reverse', buildRunner(), '已反转');
}

async function onWithdraw() {
  try {
    await ElMessageBox.confirm('确认撤回？', btnLabel('Withdraw'), { type: 'warning' });
  } catch {
    return;
  }
  await execButton('Withdraw', buildRunner(), '已撤回');
}

async function onResend() {
  await execButton(
    'Resend',
    buildRunner(),
    props.hasBackSrc ? '已返送退回人' : '已重发'
  );
}

async function onClose() {
  try {
    await ElMessageBox.confirm('确认关闭流程实例？', '关闭', { type: 'warning' });
  } catch {
    return;
  }
  await execButton('Close', buildRunner(), '已关闭');
}

async function onTerminate() {
  try {
    await ElMessageBox.confirm(
      '确认终止流程？运行中实例将被强制结束（≠ 关闭 / 作废）。',
      btnLabel('Terminate'),
      { type: 'warning' }
    );
  } catch {
    return;
  }
  await execButton('Terminate', buildRunner(), '已终止');
}

async function onDiscard() {
  try {
    await ElMessageBox.confirm(
      '确认作废流程实例？（≠ 终止 / 关闭）',
      btnLabel('Discard'),
      { type: 'warning' }
    );
  } catch {
    return;
  }
  await execButton('Discard', buildRunner(), '已作废');
}

onMounted(async () => {
  allUsers.value = await loadHostCandidateUsers();
});
</script>

<style scoped>
.sf-op-bar {
  padding: 10px 16px;
  background: transparent;
}
.sf-op-bar__actions {
  display: flex;
  gap: 10px;
  justify-content: flex-end;
  flex-wrap: wrap;
  align-items: center;
  min-height: 40px;
}
.sf-op-bar__group-row {
  display: flex;
  gap: 8px;
  align-items: center;
}
.sf-op-pop {
  padding: 4px 4px 0;
}
.sf-op-pop__plain {
  font-size: 14px;
  color: var(--el-text-color-primary);
}
.sf-op-pop__adv {
  margin-bottom: 8px;
  border: none;
}
.sf-op-pop__adv :deep(.el-collapse-item__header) {
  height: 36px;
  font-size: 13px;
  color: var(--el-text-color-secondary);
}
.sf-op-pop__footer {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  margin-top: 8px;
}
</style>
