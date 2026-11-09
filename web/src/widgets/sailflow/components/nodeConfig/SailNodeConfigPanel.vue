<template>
  <div class="sf-node-config">
    <el-form label-position="top" size="default">
      <!-- 1 基本信息 -->
      <el-card shadow="never" class="sf-node-config__card">
        <template #header><span class="sf-node-config__h">基本信息</span></template>
        <el-form-item label="节点名称">
          <el-input v-model="model.name" clearable />
        </el-form-item>
        <el-form-item label="节点 Id">
          <el-input :model-value="model.id" disabled />
        </el-form-item>
        <el-row :gutter="12">
          <el-col :span="12">
            <el-form-item label="编码 sf:code">
              <el-input v-model="model.code" clearable />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="URL sf:url">
              <el-input v-model="model.url" clearable />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="文档说明">
          <el-input v-model="model.documentation" type="textarea" :rows="2" clearable />
        </el-form-item>
      </el-card>

      <el-card shadow="never" class="sf-node-config__card">
        <template #header><span class="sf-node-config__h">多实例</span></template>
        <el-row :gutter="12">
          <el-col :span="12">
            <el-form-item label="循环">
              <el-select v-model="model.loopCharacteristics" clearable style="width: 100%">
                <el-option label="无" value="" />
                <el-option label="并行" value="parallel" />
                <el-option label="串行" value="sequential" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="基数">
              <el-input v-model="model.loopCardinality" clearable placeholder="loopCardinality" />
            </el-form-item>
          </el-col>
        </el-row>
      </el-card>

      <el-card shadow="never" class="sf-node-config__card">
        <template #header>
          <div class="sf-node-config__card-hd">
            <span class="sf-node-config__h">变量</span>
            <el-button size="small" type="primary" link @click="emit('edit-variables')">编辑变量</el-button>
          </div>
        </template>
        <el-alert type="info" :closable="false" show-icon title="变量存 wf_variable，不写入 BPMN XML" />
      </el-card>

      <!-- 2 审批类型（对照芋道） -->
      <el-card shadow="never" class="sf-node-config__card">
        <template #header><span class="sf-node-config__h">审批类型</span></template>
        <el-radio-group v-model="model.policy.approveType">
          <el-radio :value="1">人工审批</el-radio>
          <el-radio :value="2">自动通过</el-radio>
          <el-radio :value="3">自动拒绝</el-radio>
        </el-radio-group>
        <el-alert
          v-if="model.policy.approveType === 2"
          class="sf-node-config__tip"
          type="warning"
          :closable="false"
          show-icon
          title="自动通过：不建人工待办，由锚点 Job 代办。下一跳若为「发送人自选/发起人自选」、多出口或会签，将跳过自动并回落人工。"
        />
      </el-card>

      <template v-if="model.policy.approveType === 1">
        <!-- 3 待办人定范围 -->
        <el-card shadow="never" class="sf-node-config__card">
          <template #header><span class="sf-node-config__h">待办人 · 定范围</span></template>
          <el-form-item label="策略 Type">
            <el-select v-model="model.assigneeResolveType" filterable clearable style="width: 100%">
              <el-option
                v-for="o in resolveTypes"
                :key="o.value"
                :label="o.label"
                :value="o.value"
              />
            </el-select>
          </el-form-item>
          <SailAssigneeParamsForm
            v-model="model.assigneeResolveParams"
            kind="resolve"
            :type="model.assigneeResolveType"
            @edit-expression="emit('edit-resolve-expression')"
          />
          <el-alert
            type="info"
            :closable="false"
            show-icon
            title="Type 为 string 码（非芋道枚举）；RoleGroup≈角色，FixedUsers≈用户，UnitMap/JobRelative≈映射，Expression/BusinessCallback≈表单字段或回调"
          />
        </el-card>

        <!-- 4 办理模式 -->
        <el-card shadow="never" class="sf-node-config__card">
          <template #header><span class="sf-node-config__h">待办人 · 办理模式</span></template>
          <el-form-item label="LeaveJudge Type">
            <el-select v-model="model.leaveJudgeType" filterable clearable style="width: 100%">
              <el-option v-for="o in leaveTypes" :key="o.value" :label="o.label" :value="o.value" />
            </el-select>
          </el-form-item>
          <SailAssigneeParamsForm
            v-model="model.leaveJudgeParams"
            kind="leave"
            :type="model.leaveJudgeType"
          />
          <el-alert
            type="info"
            :closable="false"
            show-icon
            title="Race≈或签，Collaborate≈会签比例，Captain≈队长关卡"
          />
        </el-card>

        <!-- 5 会签 -->
        <el-card shadow="never" class="sf-node-config__card">
          <template #header><span class="sf-node-config__h">会签 / 加签扩展</span></template>
          <el-row :gutter="12">
            <el-col :span="12">
              <el-form-item label="会签类型">
                <el-select v-model="model.multiSign.complexType" style="width: 100%">
                  <el-option label="默认" value="0" />
                  <el-option label="会签" value="1" />
                  <el-option label="加签" value="2" />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="串并行">
                <el-select v-model="model.multiSign.mergeType" style="width: 100%">
                  <el-option label="默认" value="0" />
                  <el-option label="串行" value="1" />
                  <el-option label="并行" value="2" />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="通过率类型">
                <el-select v-model="model.multiSign.compareType" style="width: 100%">
                  <el-option label="默认" value="0" />
                  <el-option label="按人数" value="1" />
                  <el-option label="按百分比" value="2" />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="通过值">
                <el-input v-model="model.multiSign.completeOrder" clearable />
              </el-form-item>
            </el-col>
          </el-row>
        </el-card>

        <!-- 6 参与者 -->
        <el-card shadow="never" class="sf-node-config__card">
          <template #header>
            <div class="sf-node-config__card-hd">
              <span class="sf-node-config__h">参与者</span>
              <el-button size="small" type="primary" link @click="emit('pick-performer')">添加</el-button>
            </div>
          </template>
          <el-table :data="model.performers" size="small" empty-text="暂无参与者">
            <el-table-column prop="name" label="名称" min-width="100" />
            <el-table-column prop="outerType" label="类型" width="80" />
            <el-table-column prop="outerId" label="Id" min-width="100" show-overflow-tooltip />
            <el-table-column label="" width="64">
              <template #default="{ $index }">
                <el-button link type="danger" @click="model.performers.splice($index, 1)">删</el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-card>

        <!-- 7 表单 -->
        <el-card shadow="never" class="sf-node-config__card">
          <template #header>
            <div class="sf-node-config__card-hd">
              <span class="sf-node-config__h">表单</span>
              <el-button size="small" type="primary" link @click="emit('pick-form')">绑定表单</el-button>
            </div>
          </template>
          <el-table :data="model.forms" size="small" empty-text="未绑定表单">
            <el-table-column prop="name" label="名称" min-width="120" />
            <el-table-column prop="outerId" label="FormId" width="100" />
            <el-table-column prop="outerCode" label="编码" min-width="100" />
            <el-table-column label="" width="64">
              <template #default="{ $index }">
                <el-button link type="danger" @click="model.forms.splice($index, 1)">删</el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-card>

        <!-- 8 操作按钮：待办 / 其他 -->
        <el-card shadow="never" class="sf-node-config__card">
          <template #header><span class="sf-node-config__h">操作按钮设置</span></template>
          <div class="sf-btn-group-title">待办办理</div>
          <div class="sf-btn-list">
            <div class="sf-btn-list__head">
              <span>操作按钮</span>
              <span>显示名称</span>
              <span>启用</span>
            </div>
            <div
              v-for="item in handleButtons"
              :key="item.id"
              class="sf-btn-list__row"
            >
              <span>{{ catalogName(item.id) }}</span>
              <el-input v-model="item.displayName" size="small" />
              <el-switch v-model="item.enable" />
            </div>
          </div>
          <div class="sf-btn-group-title mt">其他（已办 / 管理）</div>
          <div class="sf-btn-list">
            <div class="sf-btn-list__head">
              <span>操作按钮</span>
              <span>显示名称</span>
              <span>启用</span>
            </div>
            <div
              v-for="item in otherButtons"
              :key="item.id"
              class="sf-btn-list__row"
            >
              <span>{{ catalogName(item.id) }}</span>
              <el-input v-model="item.displayName" size="small" />
              <el-switch v-model="item.enable" />
            </div>
          </div>
          <p class="sf-node-config__hint">
            终止 / 作废默认关闭；开启后在管理态办理条出现（≠ 关闭）。
          </p>
        </el-card>

        <!-- 9 拒绝 / 空人 / 同人 / 签名 / 意见 / 跳过 / 超时 -->
        <el-card shadow="never" class="sf-node-config__card">
          <template #header><span class="sf-node-config__h">审批人拒绝时</span></template>
          <el-radio-group v-model="model.policy.rejectHandlerType" class="sf-node-config__col">
            <el-radio :value="1">结束流程</el-radio>
            <el-radio :value="2">驳回到指定节点</el-radio>
          </el-radio-group>
          <el-form-item v-if="model.policy.rejectHandlerType === 2" label="驳回节点 Id" class="mt">
            <el-input v-model="model.policy.returnNodeId" clearable placeholder="ActivityId" />
          </el-form-item>
        </el-card>

        <el-card shadow="never" class="sf-node-config__card">
          <template #header><span class="sf-node-config__h">退回策略（两维）</span></template>
          <el-form-item label="可退回目标">
            <el-radio-group v-model="model.policy.sendBackTarget" class="sf-node-config__col">
              <el-radio value="Prev">上一节点</el-radio>
              <el-radio value="Starter">发起人节点（默认）</el-radio>
              <el-radio value="History">自选历史节点</el-radio>
            </el-radio-group>
          </el-form-item>
          <el-form-item label="被退回后再发送">
            <el-radio-group v-model="model.policy.resubmitMode" class="sf-node-config__col">
              <el-radio value="ReturnToSender">返送退回人（默认）</el-radio>
              <el-radio value="ForwardNormal">正常发送（沿下一跳）</el-radio>
            </el-radio-group>
          </el-form-item>
          <div class="sf-node-config__hint">
            两维正交：先定「退到哪」，再定「改完怎么走」。落盘
            <code>sf:NodePolicy.json</code>；办理台按再发送维互斥露出出口。
          </div>
        </el-card>

        <el-card shadow="never" class="sf-node-config__card">
          <template #header><span class="sf-node-config__h">审批人为空时</span></template>
          <el-radio-group v-model="model.policy.assignEmptyHandlerType" class="sf-node-config__col">
            <el-radio :value="2">自动拒绝（默认）</el-radio>
            <el-radio :value="1">自动通过</el-radio>
            <el-radio :value="3">指定人员</el-radio>
            <el-radio :value="4">转交流程管理员</el-radio>
          </el-radio-group>
          <el-form-item v-if="model.policy.assignEmptyHandlerType === 3" label="指定用户 Account" class="mt">
            <el-select
              v-model="model.policy.assignEmptyUserIds"
              multiple
              filterable
              allow-create
              default-first-option
              style="width: 100%"
              placeholder="输入或选择 Account"
            >
              <el-option v-for="u in users" :key="u" :label="u" :value="u" />
            </el-select>
          </el-form-item>
        </el-card>

        <el-card shadow="never" class="sf-node-config__card">
          <template #header><span class="sf-node-config__h">审批人与发起人为同一人时</span></template>
          <el-radio-group v-model="model.policy.assignStartUserHandlerType" class="sf-node-config__col">
            <el-radio :value="1">由发起人自己审批</el-radio>
            <el-radio :value="2">自动跳过</el-radio>
            <el-radio :value="3">按指定待办人策略重新算人</el-radio>
          </el-radio-group>
          <template v-if="model.policy.assignStartUserHandlerType === 3">
            <el-form-item label="换人策略 Type" class="mt">
              <el-select
                v-model="model.policy.assignStartResolveType"
                filterable
                allow-create
                default-first-option
                style="width: 100%"
                placeholder="如 deptBoss / RoleGroup"
              >
                <el-option
                  v-for="o in resolveTypes"
                  :key="o.value"
                  :label="o.label"
                  :value="o.value"
                />
              </el-select>
            </el-form-item>
            <SailAssigneeParamsForm
              v-model="model.policy.assignStartResolveParams"
              kind="resolve"
              :type="model.policy.assignStartResolveType"
            />
            <el-alert
              type="info"
              :closable="false"
              show-icon
              class="mt"
              title="与主定范围同一套 Type 码；运行时走管道 Registry（默认 deptBoss=部门领导）"
            />
          </template>
        </el-card>

        <el-card shadow="never" class="sf-node-config__card">
          <template #header><span class="sf-node-config__h">签名 / 意见 / 跳过</span></template>
          <el-form-item label="需要签名">
            <el-switch v-model="model.policy.signEnable" active-text="是" inactive-text="否" />
          </el-form-item>
          <el-form-item label="审批意见">
            <el-switch
              v-model="model.policy.reasonRequire"
              active-text="必填"
              inactive-text="非必填"
            />
          </el-form-item>
          <el-form-item label="跳过表达式">
            <el-input v-model="model.policy.skipExpression" type="textarea" :rows="2" />
          </el-form-item>
        </el-card>

        <el-card shadow="never" class="sf-node-config__card">
          <template #header><span class="sf-node-config__h">审批人超时未处理时</span></template>
          <p class="sf-node-config__hint">
            产品超时策略（提醒/自动同意/拒绝），由 Host 作业消费。属性面板「逾期时长」写的是
            <code>sf:Boundaries</code> ISO8601，只打 Overdue 时刻、不执行本策略；两套可并存，启用本项时以更早截止为准。
          </p>
          <el-form-item label="启用">
            <el-switch v-model="model.policy.timeoutHandlerEnable" />
          </el-form-item>
          <template v-if="model.policy.timeoutHandlerEnable">
            <el-form-item label="执行动作">
              <el-radio-group v-model="model.policy.timeoutHandlerType">
                <el-radio-button :value="1">自动提醒</el-radio-button>
                <el-radio-button :value="2">自动同意</el-radio-button>
                <el-radio-button :value="3">自动拒绝</el-radio-button>
              </el-radio-group>
            </el-form-item>
            <el-form-item label="超时时间">
              <div class="sf-node-config__inline">
                <span>超过</span>
                <el-input-number v-model="model.policy.timeDuration" :min="1" />
                <el-select v-model="model.policy.timeUnit" style="width: 100px">
                  <el-option label="分钟" :value="1" />
                  <el-option label="小时" :value="2" />
                  <el-option label="天" :value="3" />
                </el-select>
                <span>未处理</span>
              </div>
            </el-form-item>
            <el-form-item v-if="model.policy.timeoutHandlerType === 1" label="最大提醒次数">
              <el-input-number v-model="model.policy.maxRemindCount" :min="1" :max="10" />
            </el-form-item>
          </template>
        </el-card>

        <!-- 10 字段权限 -->
        <el-card shadow="never" class="sf-node-config__card">
          <template #header>
            <div class="sf-node-config__card-hd">
              <span class="sf-node-config__h">表单字段权限</span>
              <el-button size="small" link @click="addFieldPerm">添加字段</el-button>
            </div>
          </template>
          <el-table :data="model.policy.fieldsPermission" size="small" empty-text="无字段权限项">
            <el-table-column label="字段" min-width="100">
              <template #default="{ row }">
                <el-input v-model="row.field" size="small" />
              </template>
            </el-table-column>
            <el-table-column label="标题" min-width="100">
              <template #default="{ row }">
                <el-input v-model="row.title" size="small" />
              </template>
            </el-table-column>
            <el-table-column label="编辑性" width="130">
              <template #default="{ row }">
                <el-select v-model="row.permission" size="small" style="width: 110px">
                  <el-option
                    v-for="opt in fieldModeOptions"
                    :key="opt.value"
                    :label="opt.label"
                    :value="opt.value"
                  />
                </el-select>
              </template>
            </el-table-column>
            <el-table-column width="56">
              <template #default="{ $index }">
                <el-button
                  link
                  type="danger"
                  @click="model.policy.fieldsPermission.splice($index, 1)"
                >
                  删
                </el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-card>

        <!-- 11 通知 -->
        <el-card shadow="never" class="sf-node-config__card">
          <template #header>
            <div class="sf-node-config__card-hd">
              <span class="sf-node-config__h">通知收件人</span>
              <el-button size="small" type="primary" link @click="emit('pick-notify')">添加</el-button>
            </div>
          </template>
          <el-table :data="model.notifications" size="small" empty-text="无通知人">
            <el-table-column prop="name" label="名称" min-width="120" />
            <el-table-column prop="outerId" label="Id" min-width="100" />
            <el-table-column width="64">
              <template #default="{ $index }">
                <el-button link type="danger" @click="model.notifications.splice($index, 1)">
                  删
                </el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-card>

        <!-- 12 动作 -->
        <el-card shadow="never" class="sf-node-config__card">
          <template #header>
            <div class="sf-node-config__card-hd">
              <span class="sf-node-config__h">节点动作 (sf:Actions)</span>
              <el-button size="small" link @click="addAction">添加</el-button>
            </div>
          </template>
          <div v-for="(act, i) in model.actions" :key="i" class="sf-node-config__action">
            <el-row :gutter="8">
              <el-col :span="8">
                <el-select v-model="act.fireType" placeholder="时机" style="width: 100%">
                  <el-option label="默认" value="0" />
                  <el-option label="之前" value="1" />
                  <el-option label="之后" value="2" />
                </el-select>
              </el-col>
              <el-col :span="8">
                <el-select v-model="act.methodType" placeholder="方法" style="width: 100%">
                  <el-option label="默认" value="0" />
                  <el-option label="WebApi" value="1" />
                  <el-option label="LocalService" value="2" />
                  <el-option label="CSharpLibrary" value="3" />
                  <el-option label="SQL" value="4" />
                </el-select>
              </el-col>
              <el-col :span="6">
                <el-button link type="danger" @click="model.actions.splice(i, 1)">删除</el-button>
              </el-col>
            </el-row>
            <el-input
              v-model="act.expression"
              class="mt"
              type="textarea"
              :rows="2"
              placeholder="表达式 / URL"
            />
            <el-input v-model="act.argus" class="mt" placeholder="参数" />
          </div>
          <el-empty v-if="!model.actions.length" description="无动作" :image-size="48" />
        </el-card>
      </template>
    </el-form>
  </div>
</template>

<script setup lang="ts">
import {
  SAIL_BUTTON_CATALOG,
  buttonDesignerGroup
} from '@sail/bpmn/buttonsCatalog';
import {
  LEAVE_TYPE_OPTIONS,
  RESOLVE_TYPE_OPTIONS
} from '@sail/bpmn/assignee/types';
import type { SailNodeConfigModel } from '@sail/bpmn/nodeConfig/types';
import SailAssigneeParamsForm from '@sail/components/assignee/SailAssigneeParamsForm.vue';
import { SAIL_FIELD_MODE_OPTIONS } from '@sail/utils/fieldModes';
import { computed } from 'vue';

defineOptions({ name: 'SailNodeConfigPanel' });

const model = defineModel<SailNodeConfigModel>({ required: true });

defineProps<{ users?: string[] }>();

const emit = defineEmits<{
  'pick-performer': [];
  'pick-form': [];
  'pick-notify': [];
  'edit-resolve-expression': [];
  'edit-variables': [];
}>();

const resolveTypes = RESOLVE_TYPE_OPTIONS;
const leaveTypes = LEAVE_TYPE_OPTIONS;
const fieldModeOptions = SAIL_FIELD_MODE_OPTIONS;

const handleButtons = computed(() =>
  (model.value.buttons || []).filter(b => {
    const def = SAIL_BUTTON_CATALOG.find(d => d.id === b.id);
    return def ? buttonDesignerGroup(def) === 'handle' : true;
  })
);
const otherButtons = computed(() =>
  (model.value.buttons || []).filter(b => {
    const def = SAIL_BUTTON_CATALOG.find(d => d.id === b.id);
    return def ? buttonDesignerGroup(def) === 'other' : false;
  })
);

function catalogName(id: string) {
  return SAIL_BUTTON_CATALOG.find(b => b.id === id)?.displayName || id;
}

function addFieldPerm() {
  model.value.policy.fieldsPermission.push({
    field: '',
    title: '',
    permission: 'readonly'
  });
}

function addAction() {
  model.value.actions.push({
    fireType: '2',
    methodType: '1',
    subMethodType: '2',
    argus: '',
    expression: ''
  });
}
</script>

<style scoped>
.sf-node-config {
  display: flex;
  flex-direction: column;
  gap: 12px;
  max-height: min(72vh, 760px);
  overflow: auto;
  padding-right: 4px;
}
.sf-node-config__card {
  border-radius: 8px;
}
.sf-node-config__h {
  font-weight: 600;
  font-size: 14px;
}
.sf-node-config__card-hd {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
}
.sf-node-config__col {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 6px;
}
.sf-node-config__inline {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}
.sf-node-config__action {
  padding: 8px 0;
  border-bottom: 1px solid var(--el-border-color-lighter);
}
.sf-node-config .mt {
  margin-top: 10px;
}
.sf-node-config__hint {
  margin-top: 8px;
  font-size: 12px;
  color: var(--el-text-color-secondary);
  line-height: 1.5;
}
.sf-btn-group-title {
  font-size: 12px;
  font-weight: 600;
  color: var(--el-text-color-regular);
  margin: 4px 0 6px;
}
.sf-btn-list__head,
.sf-btn-list__row {
  display: grid;
  grid-template-columns: 1fr 1.2fr 64px;
  gap: 8px;
  align-items: center;
  padding: 6px 8px;
  border: 1px solid var(--el-border-color-lighter);
}
.sf-btn-list__head {
  font-weight: 600;
  background: var(--el-fill-color-light);
  border-radius: 4px 4px 0 0;
}
.sf-btn-list__row {
  border-top: 0;
}
.sf-btn-list__row:last-child {
  border-radius: 0 0 4px 4px;
}
</style>
