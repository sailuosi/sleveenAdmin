<template>
  <div class="sf-op-bar">
    <div class="sf-op-bar__actions">
      <template v-if="canHandle && isParticipant">
        <el-popover
          v-if="btnOn('Acknowledge')"
          :visible="ackVisible"
          placement="top-end"
          :width="420"
          trigger="click"
        >
          <template #reference>
            <el-button type="primary" plain :loading="loading" @click="ackVisible = true">
              {{ btnLabel('Acknowledge') }}
            </el-button>
          </template>
          <div class="sf-op-pop">
            <el-form label-position="top" @submit.prevent>
              <el-form-item label="阅办意见" required>
                <el-input
                  v-model="ackComment"
                  type="textarea"
                  :rows="3"
                  placeholder="请输入反馈意见"
                />
              </el-form-item>
              <div class="sf-op-pop__footer">
                <el-button @click="ackVisible = false">取消</el-button>
                <el-button type="primary" :loading="loading" @click="onAcknowledge">
                  {{ btnLabel('Acknowledge') }}
                </el-button>
              </div>
            </el-form>
          </div>
        </el-popover>
      </template>
      <template v-else-if="canHandle">
        <el-alert
          v-if="consultPending"
          type="warning"
          :closable="false"
          show-icon
          class="sf-op-bar__consult-alert"
          :title="`待沟通反馈（${openParticipantCount} 人），审批已锁定`"
        />
        <el-popover
          v-if="showApprove && btnOn('Approve') && !consultPending"
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
              <el-form-item :label="reasonRequire ? '办理意见（必填）' : '办理意见'">
                <el-input
                  v-model="comment"
                  type="textarea"
                  :rows="3"
                  :placeholder="reasonRequire ? '请输入办理意见' : '请输入办理意见（可选）'"
                />
                <SailPhrasePicker kind="approve" tone="primary" @pick="v => (comment = v)" @manage="goPhrases" />
              </el-form-item>
              <el-form-item v-if="signEnable" label="签名" required>
                <div class="sf-op-pop__sign">
                  <el-image
                    v-if="signPicUrl"
                    :src="signPicDisplayUrl"
                    fit="contain"
                    class="sf-op-pop__sign-img"
                    :preview-src-list="[signPicDisplayUrl]"
                  />
                  <div class="sf-op-pop__sign-actions">
                    <el-button type="primary" size="small" @click="openSignPad">
                      手写签名
                    </el-button>
                    <el-button
                      link
                      type="primary"
                      size="small"
                      :loading="signLoading"
                      @click="loadMySignature"
                    >
                      使用个人中心电子签名
                    </el-button>
                    <el-button
                      v-if="signPicUrl"
                      link
                      type="info"
                      size="small"
                      @click="signPicUrl = ''"
                    >
                      清除
                    </el-button>
                  </div>
                </div>
                <div class="sf-op-pop__hint">节点要求签名；可通过手写或个人中心签名</div>
              </el-form-item>
              <el-form-item v-if="needsNextHopPick && nextNodes.length" label="下一节点" required>
                <div class="sf-op-pop__hint" style="margin-bottom: 6px">
                  请确认下一节点；需要人工选人时在下方选择办理人
                </div>
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
              <SailPreselectApproveHint
                v-if="needsNextHopPick && selectedActivityId"
                :process-id="String(pickField(task, 'ProcessId', 'processId') || '')"
                :version="String(pickField(task, 'Version', 'version') || '1')"
                :process-instance-id="approveProcessInstanceId"
                :activity-id="selectedActivityId"
                @update:skip-manual-pick="v => (preselectSkipManualPick = v)"
              />
              <el-form-item
                v-if="needsNextHopPick && selectedActivityId && selectedNeedsPerformer && !preselectSkipManualPick"
                :label="needsManualPick ? '下一办理人（必选）' : '下一办理人'"
                :required="needsManualPick"
              >
                <SailCandidatePicker
                  v-model="selectedUserIds"
                  :required="needsManualPick"
                  :selector-type="selectedSelectorType"
                  :selector-params="selectedSelectorParams"
                  :candidate-mode="selectedCandidateMode"
                  :inline-users="roleTreeUsers"
                  :candidate-filter="selectedCandidateFilter"
                  :query-context="candidateQueryContext"
                  @users-change="onApprovePickerUsers"
                />
              </el-form-item>
              <el-collapse
                v-if="needsNextHopPick && selectedNeedsPerformer && selectedUserIds.length"
                class="sf-op-pop__adv"
              >
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
          v-if="btnOn('Reject') && !consultPending"
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
                <SailPhrasePicker
                  kind="reject"
                  tone="danger"
                  @pick="v => (rejectComment = v)"
                  @manage="goPhrases"
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
          v-if="btnOn('SendBack') && !consultPending"
          plain
          :loading="loading"
          :disabled="!previousNodes.length"
          @click="sendBackVisible = true"
        >
          {{ btnLabel('SendBack') }}
        </el-button>
        <el-button
          v-if="btnOn('SignForward') && !consultPending"
          plain
          :loading="loading"
          @click="signVisible = true"
        >
          {{ btnLabel('SignForward') }}
        </el-button>
        <el-button
          v-if="btnOn('RemoveAssignee') && !consultPending && removableAssignees.length"
          plain
          :loading="loading"
          @click="removeVisible = true"
        >
          {{ btnLabel('RemoveAssignee') }}
        </el-button>
        <el-button
          v-if="btnOn('ChangeAssignee') && !consultPending"
          plain
          :loading="loading"
          @click="openChangeAssignee"
        >
          {{ btnLabel('ChangeAssignee') }}
        </el-button>
        <el-button
          v-if="btnOn('Urge') && !consultPending"
          plain
          :loading="loading"
          @click="openUrge"
        >
          {{ btnLabel('Urge') }}
        </el-button>
        <el-button
          v-if="btnOn('Transfer') && !consultPending"
          plain
          :loading="loading"
          @click="transferVisible = true"
        >
          {{ btnLabel('Transfer') }}
        </el-button>
        <el-button
          v-if="btnOn('Delegate') && !consultPending"
          plain
          :loading="loading"
          @click="delegateVisible = true"
        >
          {{ btnLabel('Delegate') }}
        </el-button>
        <el-button
          v-if="btnOn('Consult')"
          plain
          :loading="loading"
          @click="consultVisible = true"
        >
          {{ btnLabel('Consult') }}
        </el-button>
        <el-button
          v-if="btnOn('RecallConsult') && consultPending"
          plain
          type="warning"
          :loading="loading"
          @click="onRecallConsult"
        >
          {{ btnLabel('RecallConsult') }}
        </el-button>
        <el-button v-if="btnOn('Copyto')" plain :loading="loading" @click="copytoVisible = true">
          {{ btnLabel('Copyto') }}
        </el-button>
        <el-button
          v-if="btnOn('Revise') && !consultPending"
          plain
          :loading="loading"
          @click="onRevise"
        >
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

    <!-- 减签 -->
    <el-dialog
      v-model="removeVisible"
      :title="btnLabel('RemoveAssignee')"
      width="480px"
      destroy-on-close
    >
      <el-form label-width="90px">
        <el-form-item label="减签人员" required>
          <el-select
            v-model="removeUserIds"
            multiple
            filterable
            style="width: 100%"
            placeholder="选择要移除的未办结办理人"
          >
            <el-option
              v-for="u in removableAssignees"
              :key="u.UserId || u.userId"
              :label="`${u.UserName || u.userName || u.UserId || u.userId}`"
              :value="String(u.UserId || u.userId)"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="意见">
          <el-input v-model="removeComment" type="textarea" :rows="2" placeholder="可选" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="removeVisible = false">取消</el-button>
        <el-button type="primary" :loading="loading" @click="onRemoveAssignee">确认减签</el-button>
      </template>
    </el-dialog>

    <!-- 改派：新办理人全集 -->
    <el-dialog
      v-model="changeVisible"
      :title="btnLabel('ChangeAssignee')"
      width="520px"
      destroy-on-close
    >
      <el-form label-width="100px">
        <el-form-item label="办理人全集" required>
          <el-select
            v-model="changeUserIds"
            multiple
            filterable
            style="width: 100%"
            placeholder="编辑当前节点办理人（差量增删）"
          >
            <el-option
              v-for="u in changeCandidateOptions"
              :key="userKey(u)"
              :label="userLabel(u)"
              :value="userKey(u)"
            />
          </el-select>
          <div class="sf-op-pop__hint">提交后按全集差量关旧/建新，不推进下一节点</div>
        </el-form-item>
        <el-form-item label="意见">
          <el-input v-model="changeComment" type="textarea" :rows="2" placeholder="可选" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="changeVisible = false">取消</el-button>
        <el-button type="primary" :loading="loading" @click="onChangeAssignee">确认改派</el-button>
      </template>
    </el-dialog>

    <!-- 催办 -->
    <el-dialog v-model="urgeVisible" :title="btnLabel('Urge')" width="480px" destroy-on-close>
      <el-form label-width="90px">
        <el-form-item label="催办对象">
          <el-select
            v-model="urgeUserIds"
            multiple
            filterable
            clearable
            style="width: 100%"
            placeholder="空=催本节点全部打开待办"
          >
            <el-option
              v-for="u in currentAssignees"
              :key="u.UserId || u.userId"
              :label="`${u.UserName || u.userName || u.UserId || u.userId}`"
              :value="String(u.UserId || u.userId)"
            />
          </el-select>
          <div class="sf-op-pop__hint">不改待办状态，仅发催办通知</div>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="urgeVisible = false">取消</el-button>
        <el-button type="primary" :loading="loading" @click="onUrge">确认催办</el-button>
      </template>
    </el-dialog>

    <!-- 加签 -->
    <el-dialog v-model="signVisible" title="加签" width="520px" destroy-on-close>
      <el-form label-width="90px">
        <el-form-item label="加签类型">
          <el-radio-group v-model="signForwardType">
            <el-radio value="SignForwardParallel">并加签</el-radio>
            <el-radio value="SignForwardBefore">前加签</el-radio>
            <el-radio value="SignForwardBehind">后加签</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="加签人" required>
          <el-select
            v-model="signUserIds"
            multiple
            filterable
            style="width: 100%"
            placeholder="选择用户"
            @change="syncSignWeights"
          >
            <el-option
              v-for="u in allUsers"
              :key="userKey(u)"
              :label="userLabel(u)"
              :value="userKey(u)"
            />
          </el-select>
        </el-form-item>
        <el-form-item v-if="signUserIds.length" label="票权">
          <div class="sf-sign-weights">
            <div v-for="uid in signUserIds" :key="uid" class="sf-sign-weights__row">
              <span>{{ signUserLabel(uid) }}</span>
              <el-input-number
                v-model="signWeights[uid]"
                :min="0.0001"
                :step="1"
                :precision="2"
                controls-position="right"
                size="small"
              />
            </div>
          </div>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="signVisible = false">取消</el-button>
        <el-button type="primary" :loading="loading" @click="onSignForward">确认加签</el-button>
      </template>
    </el-dialog>

    <!-- 沟通：建参与者待办 -->
    <el-dialog v-model="consultVisible" :title="btnLabel('Consult')" width="480px" destroy-on-close>
      <el-form label-width="90px">
        <el-form-item label="征询对象" required>
          <el-select
            v-model="consultUserIds"
            multiple
            filterable
            style="width: 100%"
            placeholder="选择需要反馈的人（非审批）"
          >
            <el-option
              v-for="u in transferCandidates"
              :key="userKey(u)"
              :label="userLabel(u)"
              :value="userKey(u)"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="说明">
          <el-input v-model="consultComment" type="textarea" :rows="2" placeholder="可选" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="consultVisible = false">取消</el-button>
        <el-button type="primary" :loading="loading" @click="onConsult">发起沟通</el-button>
      </template>
    </el-dialog>

    <!-- 转办：本节点交人，不流转；办完不还回 -->
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
          <div class="sf-op-pop__hint">办完不还回原办理人；需要办完还回请用「委派」</div>
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

    <!-- 委派：受托人办完自动还回原人 -->
    <el-dialog v-model="delegateVisible" :title="btnLabel('Delegate')" width="480px" destroy-on-close>
      <el-form label-width="90px">
        <el-form-item label="受托人" required>
          <el-select
            v-model="delegateUserId"
            filterable
            clearable
            style="width: 100%"
            placeholder="选择代办人（仅一人）"
          >
            <el-option
              v-for="u in transferCandidates"
              :key="userKey(u)"
              :label="userLabel(u)"
              :value="userKey(u)"
            />
          </el-select>
          <div class="sf-op-pop__hint">办完自动还回原办理人；不还回请用「转办」</div>
        </el-form-item>
        <el-form-item label="意见">
          <el-input v-model="delegateComment" type="textarea" :rows="2" placeholder="可选" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="delegateVisible = false">取消</el-button>
        <el-button type="primary" :loading="loading" @click="onDelegate">确认委派</el-button>
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
    <el-dialog v-model="copytoVisible" title="抄送" width="480px" destroy-on-close>
      <el-form label-width="90px">
        <el-form-item label="抄送人" required>
          <el-select
            v-model="copytoUserIds"
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
          <el-input v-model="copytoReason" type="textarea" :rows="2" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="copytoVisible = false">取消</el-button>
        <el-button type="primary" :loading="loading" @click="onCopyto">确认抄送</el-button>
      </template>
    </el-dialog>

    <SailSignPadDialog ref="signPadRef" @success="onSignPadSuccess" />
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import {
  buttonsOperation,
  createProcessCopyto,
  getTaskActivityList,
  type FlowCommandContextPayload
} from '@sail/api/runtime';
import { loadHostCandidateUsers, type HostCandidateUser } from '@sail/adapters/hostUsers';
import { useSailCurrentUser } from '@sail/adapters/currentUser';
import { apiSysUserBaseInfoGet } from '@/widgets/sysext/api/system/sysuserapi.js';
import { unpackSailResult, pickField } from '@sail/utils/sailResult';
import { formValuesToConditionsForSubmit } from '@sail/utils/formCreate';
import {
  buttonDisplayName,
  isButtonEnabled,
  mergeButtonsSetting,
  type SailButtonSetting
} from '@sail/bpmn/buttonsCatalog';
import SailCandidatePicker from '@sail/components/runtime/SailCandidatePicker.vue';
import SailPhrasePicker from '@sail/components/runtime/SailPhrasePicker.vue';
import SailSignPadDialog from '@sail/components/runtime/SailSignPadDialog.vue';
import { useRouter } from 'vue-router';
import SailPreselectApproveHint from '@sail/components/preselect/SailPreselectApproveHint.vue';
import { resolvePublicFileUrl } from '@sail/utils/publicFileUrl';

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
    /**
     * 会签/加签是否最后一人（Workbench IsLastTask）。
     * 非末人完成本席即可，不需下一节点选人；缺省 true 兼容旧接口。
     */
    isLastTask?: boolean;
    /** 节点 sf:ButtonsSetting（Workbench 下发） */
    buttonsSetting?: SailButtonSetting[] | null;
    /** 办理人角色：0/null 常规；1 参与者 */
    assigneeRole?: number | null;
    /** 发起人侧沟通未反馈完 */
    consultPending?: boolean;
    openParticipantCount?: number;
    /** NodePolicy.signEnable */
    signEnable?: boolean;
    /** NodePolicy.reasonRequire */
    reasonRequire?: boolean;
    /** Workbench.RemovableAssignees */
    removableAssignees?: Array<{
      TaskId?: string;
      taskId?: string;
      UserId?: string;
      userId?: string;
      UserName?: string;
      userName?: string;
    }>;
    /** Workbench.CurrentAssignees（含本人） */
    currentAssignees?: Array<{
      TaskId?: string;
      taskId?: string;
      UserId?: string;
      userId?: string;
      UserName?: string;
      userName?: string;
    }>;
  }>(),
  {
    canHandle: true,
    showApprove: true,
    showWithdraw: false,
    showResend: false,
    showClose: false,
    showJump: false,
    hasBackSrc: false,
    isLastTask: true,
    buttonsSetting: null,
    fieldModes: null,
    assigneeRole: null,
    consultPending: false,
    openParticipantCount: 0,
    signEnable: false,
    reasonRequire: false,
    removableAssignees: () => [],
    currentAssignees: () => []
  }
);

const emit = defineEmits<{ done: []; failed: [string] }>();

const { userId, userName } = useSailCurrentUser();
const loading = ref(false);
const comment = ref('');
const rejectComment = ref('');
const signPicUrl = ref('');
const signPicDisplayUrl = computed(() => resolvePublicFileUrl(signPicUrl.value));
const signLoading = ref(false);
const signPadRef = ref<{ open: () => void } | null>(null);
const approveVisible = ref(false);
const rejectVisible = ref(false);
const removeVisible = ref(false);
const removeUserIds = ref<string[]>([]);
const removeComment = ref('');
const changeVisible = ref(false);
const changeUserIds = ref<string[]>([]);
const changeComment = ref('');
const urgeVisible = ref(false);
const urgeUserIds = ref<string[]>([]);
const router = useRouter();
const signEnable = computed(() => !!props.signEnable);

function goPhrases() {
  router.push('/sail/approval-phrases');
}
const reasonRequire = computed(() => !!props.reasonRequire);
const removableAssignees = computed(() => props.removableAssignees || []);
const currentAssignees = computed(() => props.currentAssignees || []);
const changeCandidateOptions = computed(() => {
  const map = new Map<string, Record<string, unknown>>();
  for (const u of allUsers.value) {
    const k = userKey(u as never);
    if (k) map.set(k, u as Record<string, unknown>);
  }
  for (const u of currentAssignees.value) {
    const k = String(u.UserId || u.userId || '');
    if (!k) continue;
    if (!map.has(k)) {
      map.set(k, {
        UserId: k,
        UserName: u.UserName || u.userName || k,
        Account: k
      });
    }
  }
  return Array.from(map.values());
});

async function loadMySignature() {
  signLoading.value = true;
  try {
    const res = await apiSysUserBaseInfoGet();
    const row = (res as any)?.data?.result ?? (res as any)?.result ?? res ?? {};
    const url = String(
      pickField(row, 'signature', 'Signature', 'signPicUrl', 'SignPicUrl') || ''
    ).trim();
    if (!url) {
      ElMessage.warning('个人中心尚未上传电子签名，请先到用户中心设置');
      return;
    }
    signPicUrl.value = url;
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : '加载签名失败');
  } finally {
    signLoading.value = false;
  }
}

function openSignPad() {
  signPadRef.value?.open();
}

function onSignPadSuccess(url: string) {
  signPicUrl.value = String(url || '').trim();
}

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
/** 对齐引擎 SignForwardTypeEnum：SignForwardParallel / Before / Behind */
const signForwardType = ref('SignForwardParallel');
const signUserIds = ref<string[]>([]);
/** 加签人票权；默认 1 */
const signWeights = reactive<Record<string, number>>({});
const transferVisible = ref(false);
const delegateVisible = ref(false);
const delegateUserId = ref('');
const delegateComment = ref('');
const transferUserIds = ref<string[]>([]);
const transferComment = ref('');
const consultVisible = ref(false);
const consultUserIds = ref<string[]>([]);
const consultComment = ref('');
const ackVisible = ref(false);
const ackComment = ref('');
const copytoVisible = ref(false);
const copytoUserIds = ref<string[]>([]);
const copytoReason = ref('');
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

const isParticipant = computed(
  () => Number(props.assigneeRole ?? 0) === 1
);
const consultPending = computed(() => !!props.consultPending);
const openParticipantCount = computed(() => Number(props.openParticipantCount || 0));

/** §G：被退回后再发送 —— 正常发送 vs 返送退回人 */
const approveActionLabel = computed(() =>
  props.hasBackSrc && props.canHandle ? '正常发送' : btnLabel('Approve')
);
const resendActionLabel = computed(() =>
  props.hasBackSrc ? '返送退回人' : btnLabel('Resend')
);

const nextNodes = computed(() => props.nextNodes || []);
const previousNodes = computed(() => props.previousNodes || []);
/** 非末人（会签/加签中间席）不走出节点，无需选下一办理人 */
const needsNextHopPick = computed(() => props.isLastTask !== false);

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
  return Array.isArray(users) ? users.filter(u => !!userKey(u)) : [];
});
const approvePickerUsers = ref<Record<string, unknown>[]>([]);
const selectedCandidateMode = computed(() =>
  String(pickField(selectedNode.value || {}, 'CandidateMode', 'candidateMode') || '')
);
const selectedSelectorType = computed(() =>
  String(
    pickField(selectedNode.value || {}, 'CandidateSelectorType', 'candidateSelectorType') || ''
  )
);
const selectedSelectorParams = computed(() =>
  String(
    pickField(
      selectedNode.value || {},
      'CandidateSelectorParams',
      'candidateSelectorParams'
    ) || ''
  )
);
const selectedCandidateFilter = computed(() => {
  const n = selectedNode.value;
  if (!n) return null;
  return (pickField(n, 'CandidateFilter', 'candidateFilter') || null) as {
    RoleIds?: string[];
    roleIds?: string[];
  } | null;
});
const candidateQueryContext = computed(() => ({
  ProcessId: String(pickField(props.task, 'ProcessId', 'processId') || ''),
  Version: String(pickField(props.task, 'Version', 'version') || '1'),
  ActivityId: selectedActivityId.value,
  TaskId: String(pickField(props.task, 'TaskId', 'taskId') || '')
}));
function onApprovePickerUsers(list: Record<string, unknown>[]) {
  approvePickerUsers.value = list || [];
}
const candidateUsers = computed(() => {
  if (approvePickerUsers.value.length) return approvePickerUsers.value;
  if (roleTreeUsers.value.length) return roleTreeUsers.value;
  return allUsers.value as unknown as Record<string, unknown>[];
});
/** 下一跳已有 Preselect 表数据时跳过强制选人（独立组件探测） */
const preselectSkipManualPick = ref(false);
const approveProcessInstanceId = computed(() =>
  String(
    pickField(props.task, 'ProcessInstanceId', 'processInstanceId') ||
      pickField(props.processInstance || {}, 'ProcessInstanceId', 'processInstanceId', 'Id', 'id') ||
      ''
  )
);

// 结束/网关等不需选人；非末人跳过；anyselect/search 必选；无 Mode 时兼容旧接口
const needsManualPick = computed(() => {
  if (!needsNextHopPick.value) return false;
  if (preselectSkipManualPick.value) return false;
  if (!selectedActivityId.value || !selectedNeedsPerformer.value) return false;
  const m = selectedCandidateMode.value.toLowerCase();
  if (m === 'anyselect' || m === 'search') return true;
  if (!m) return roleTreeUsers.value.length === 0;
  return false;
});

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
    BizChannel: String(pickField(t, 'BizChannel', 'bizChannel') || ''),
    BizPK: String(pickField(t, 'BizPK', 'bizPK') || ''),
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
  Weight?: number;
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
  },
  weightByUser?: Record<string, number>
): Record<string, NextPerformerPayload[]> {
  const gid = group?.groupId?.trim();
  const role = group?.groupRole?.trim();
  const unitId = group?.unitId?.trim();
  const unitCode = group?.unitCode?.trim();
  const unitName = group?.unitName?.trim();
  const performers = userIds.map(uid => {
    const u = pool.find(x => userKey(x) === uid);
    const w = Number(weightByUser?.[uid]);
    const p: NextPerformerPayload = {
      UserId: uid,
      UserName: u ? userLabel(u) : uid,
      Weight: w > 0 ? w : 1
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
  if (reasonRequire.value && !comment.value.trim()) {
    ElMessage.warning('请输入办理意见');
    return;
  }
  if (signEnable.value && !signPicUrl.value.trim()) {
    ElMessage.warning('本节点需要签名');
    return;
  }
  const extra: Partial<FlowCommandContextPayload> = {};
  if (needsNextHopPick.value) {
    if (nextNodes.value.length && !selectedActivityId.value) {
      ElMessage.warning('请选择下一节点');
      return;
    }
    if (needsManualPick.value && !selectedUserIds.value.length) {
      ElMessage.warning(candidateUsers.value.length ? '请选择下一办理人' : '暂无候选人，无法提交');
      return;
    }
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
  }
  const runner = buildRunner(extra);
  if (signPicUrl.value.trim()) {
    runner.Conditions = {
      ...(runner.Conditions || {}),
      signPicUrl: signPicUrl.value.trim()
    };
  }
  const ok = await execButton('Approve', runner, '已通过');
  if (ok) {
    approveVisible.value = false;
    signPicUrl.value = '';
  }
}

async function onRemoveAssignee() {
  if (!removeUserIds.value.length) {
    ElMessage.warning('请选择要减签的人员');
    return;
  }
  const activityId =
    pickField(props.task, 'ActivityId', 'activityId') || selectedActivityId.value || 'remove';
  const pool = removableAssignees.value.map(u => ({
    UserId: String(u.UserId || u.userId || ''),
    UserName: String(u.UserName || u.userName || u.UserId || u.userId || '')
  }));
  const runner = buildRunner({
    NextActivityPerformers: withNextPerformers(String(activityId), removeUserIds.value, pool)
  });
  if (removeComment.value.trim()) {
    runner.Conditions = {
      ...(runner.Conditions || {}),
      comment: removeComment.value.trim()
    };
  }
  const ok = await execButton('RemoveAssignee', runner, '已减签');
  if (ok) {
    removeVisible.value = false;
    removeUserIds.value = [];
    removeComment.value = '';
  }
}

function openChangeAssignee() {
  changeUserIds.value = currentAssignees.value
    .map(u => String(u.UserId || u.userId || ''))
    .filter(Boolean);
  changeComment.value = '';
  changeVisible.value = true;
}

async function onChangeAssignee() {
  if (!changeUserIds.value.length) {
    ElMessage.warning('请至少保留一名办理人');
    return;
  }
  const activityId =
    pickField(props.task, 'ActivityId', 'activityId') || selectedActivityId.value || 'change';
  const runner = buildRunner({
    NextActivityPerformers: withNextPerformers(
      String(activityId),
      changeUserIds.value,
      changeCandidateOptions.value
    )
  });
  if (changeComment.value.trim()) {
    runner.Conditions = {
      ...(runner.Conditions || {}),
      comment: changeComment.value.trim()
    };
  }
  const ok = await execButton('ChangeAssignee', runner, '已改派');
  if (ok) {
    changeVisible.value = false;
    changeUserIds.value = [];
    changeComment.value = '';
  }
}

function openUrge() {
  urgeUserIds.value = [];
  urgeVisible.value = true;
}

async function onUrge() {
  const activityId =
    pickField(props.task, 'ActivityId', 'activityId') || selectedActivityId.value || 'urge';
  const extra: Partial<FlowCommandContextPayload> = {};
  if (urgeUserIds.value.length) {
    const pool = currentAssignees.value.map(u => ({
      UserId: String(u.UserId || u.userId || ''),
      UserName: String(u.UserName || u.userName || u.UserId || u.userId || '')
    }));
    extra.NextActivityPerformers = withNextPerformers(String(activityId), urgeUserIds.value, pool);
  }
  const ok = await execButton('Urge', buildRunner(extra), '已催办');
  if (ok) {
    urgeVisible.value = false;
    urgeUserIds.value = [];
  }
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

function syncSignWeights() {
  const ids = signUserIds.value || [];
  for (const uid of ids) {
    if (signWeights[uid] == null || !(signWeights[uid] > 0)) signWeights[uid] = 1;
  }
  Object.keys(signWeights).forEach(k => {
    if (!ids.includes(k)) delete signWeights[k];
  });
}

function signUserLabel(uid: string) {
  const u = allUsers.value.find(x => userKey(x as never) === uid);
  return u ? userLabel(u as never) : uid;
}

async function onSignForward() {
  if (!signUserIds.value.length) {
    ElMessage.warning('请选择加签人');
    return;
  }
  syncSignWeights();
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
      NextActivityPerformers: withNextPerformers(
        activityId,
        signUserIds.value,
        allUsers.value as never[],
        undefined,
        signWeights
      )
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

async function onDelegate() {
  const uid = String(delegateUserId.value || '').trim();
  if (!uid) {
    ElMessage.warning('请选择受托人');
    return;
  }
  const activityId = String(
    pickField(props.task, 'ActivityId', 'activityId') || selectedActivityId.value || 'delegate'
  );
  const runner = buildRunner({
    NextActivityPerformers: withNextPerformers(activityId, [uid], allUsers.value)
  });
  if (delegateComment.value.trim()) {
    runner.Conditions = {
      ...(runner.Conditions || {}),
      comment: delegateComment.value.trim()
    };
  }
  const ok = await execButton('Delegate', runner, '已委派');
  if (ok) {
    delegateVisible.value = false;
    delegateUserId.value = '';
    delegateComment.value = '';
  }
}

async function onConsult() {
  if (!consultUserIds.value.length) {
    ElMessage.warning('请选择征询对象');
    return;
  }
  const activityId = String(
    pickField(props.task, 'ActivityId', 'activityId') || selectedActivityId.value || 'consult'
  );
  const runner = buildRunner({
    NextActivityPerformers: withNextPerformers(
      activityId,
      consultUserIds.value,
      allUsers.value
    )
  });
  if (consultComment.value.trim()) {
    runner.Conditions = {
      ...(runner.Conditions || {}),
      comment: consultComment.value.trim()
    };
  }
  const ok = await execButton('Consult', runner, '已发起沟通');
  if (ok) {
    consultVisible.value = false;
    consultUserIds.value = [];
    consultComment.value = '';
  }
}

async function onAcknowledge() {
  if (!ackComment.value.trim()) {
    ElMessage.warning('请填写阅办意见');
    return;
  }
  const runner = buildRunner({
    Conditions: { comment: ackComment.value.trim() }
  });
  const ok = await execButton('Acknowledge', runner, '已阅办');
  if (ok) {
    ackVisible.value = false;
    ackComment.value = '';
  }
}

async function onRecallConsult() {
  await execButton('RecallConsult', buildRunner(), '已收回沟通');
}

async function onCopyto() {
  if (!copytoUserIds.value.length) {
    ElMessage.warning('请选择抄送人');
    return;
  }
  loading.value = true;
  try {
    const t = props.task;
    const pi = props.processInstance;
    const data = await createProcessCopyto({
      Targets: copytoUserIds.value.map(uid => {
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
      BizPK: String(pickField(t, 'BizPK', 'bizPK') || ''),
      TaskId: String(pickField(t, 'TaskId', 'taskId', 'Id', 'id') || ''),
      TaskName: String(pickField(t, 'ActivityName', 'activityName') || ''),
      StartUserId: String(pickField(pi || {}, 'CreatedUserId', 'createdUserId') || ''),
      CreatorId: userId.value,
      CreatorName: userName.value,
      Reason: copytoReason.value || comment.value
    });
    if ((data?.Status ?? data?.status) === -1) {
      ElMessage.error(data?.Message || data?.message || '抄送失败');
      return;
    }
    ElMessage.success('已抄送');
    copytoVisible.value = false;
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
.sf-op-bar__consult-alert {
  width: 100%;
  margin-bottom: 4px;
}
.sf-op-bar__group-row {
  display: flex;
  gap: 8px;
  align-items: center;
}
.sf-sign-weights {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.sf-sign-weights__row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
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
.sf-op-pop__phrases {
  display: flex;
  flex-wrap: wrap;
  gap: 4px 8px;
  margin-top: 6px;
}
.sf-op-pop__hint {
  margin-top: 4px;
  font-size: 12px;
  color: var(--el-text-color-secondary);
  line-height: 1.4;
}
.sf-op-pop__sign {
  display: flex;
  flex-direction: column;
  gap: 6px;
  width: 100%;
}
.sf-op-pop__sign-img {
  width: 160px;
  height: 56px;
  border: 1px solid var(--el-border-color-lighter);
  border-radius: 4px;
  background: #fff;
}
.sf-op-pop__sign-actions {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 4px 8px;
}
</style>
