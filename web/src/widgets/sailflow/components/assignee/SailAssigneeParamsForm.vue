<template>
  <div class="sf-assignee-params">
    <template v-if="kind === 'resolve'">
      <template v-if="type === 'FixedUsers'">
        <el-form-item label="固定用户 (Account)">
          <el-select
            v-model="draft.userIds"
            multiple
            filterable
            clearable
            allow-create
            default-first-option
            style="width: 100%"
            placeholder="选择或输入账号"
          >
            <el-option v-for="u in userOptions" :key="u" :label="u" :value="u" />
          </el-select>
        </el-form-item>
      </template>
      <template v-else-if="type === 'RoleGroup' || type === 'deptBoss'">
        <el-form-item :label="type === 'deptBoss' ? '负责人角色袋 roleIds（可选）' : '角色 Id / Code'">
          <el-select
            v-model="draft.roleIds"
            multiple
            filterable
            clearable
            allow-create
            default-first-option
            style="width: 100%"
            :placeholder="type === 'deptBoss' ? '空则用 Sail:DeptLeaderRoleIds' : '选择角色'"
          >
            <el-option
              v-for="r in roleOptions"
              :key="r.value"
              :label="r.label"
              :value="r.value"
            />
          </el-select>
        </el-form-item>
        <el-alert
          v-if="type === 'deptBoss'"
          type="info"
          :closable="false"
          show-icon
          title="按发起人组织链找负责人；未命中可弱回落同组织其他账号"
        />
      </template>
      <template v-else-if="type === 'UnitMap' || type === 'JobRelative'">
        <el-form-item label="单位/岗位字段 unitField">
          <el-input v-model="draft.unitField" clearable placeholder="如 companyId / jobCode" />
        </el-form-item>
        <el-form-item label="映射 maps（键→账号）">
          <div class="sf-assignee-params__maps">
            <div v-for="(row, i) in draft.mapRows" :key="i" class="sf-assignee-params__map-row">
              <el-input v-model="row.key" placeholder="键" />
              <el-input v-model="row.value" placeholder="账号" />
              <el-button link type="danger" @click="draft.mapRows.splice(i, 1)">删</el-button>
            </div>
            <el-button size="small" @click="draft.mapRows.push({ key: '', value: '' })">添加映射</el-button>
          </div>
        </el-form-item>
      </template>
      <template v-else-if="type === 'Expression'">
        <el-form-item label="表达式 / 字段">
          <el-input v-model="draft.expression" clearable placeholder="如 ${form.approver} / userIdsField" />
        </el-form-item>
        <el-form-item label="userIdsField（可选）">
          <el-input v-model="draft.userIdsField" clearable placeholder="从表单字段取用户列表" />
        </el-form-item>
        <el-button type="primary" link @click="emit('edit-expression')">可视化编辑表达式</el-button>
      </template>
      <template v-else-if="type === 'BusinessCallback' || type === 'Business.ClassStudents'">
        <el-form-item label="callback 名">
          <el-input v-model="draft.callback" clearable placeholder="如 ClassStudents" />
        </el-form-item>
      </template>
      <template v-else-if="type === 'Fallback'">
        <el-alert
          type="warning"
          :closable="false"
          show-icon
          class="sf-assignee-hint"
          title="产品空人策略请用节点「审批人为空时」（NodePolicy）。此处 Fallback 仅管道二次算人，≠ 跳过/拒绝整单。"
        />
        <el-form-item label="主策略空时">
          <el-select v-model="draft.onEmpty" clearable style="width: 100%">
            <el-option label="拒绝推进（Error，对齐 R4）" value="Error" />
            <el-option label="继续 Resolve 下一 Type" value="Resolve" />
            <el-option label="Suspend（慎用·非产品默认）" value="Suspend" />
          </el-select>
        </el-form-item>
        <el-form-item label="下一 Type（onEmpty=Resolve）">
          <el-input v-model="draft.nextType" clearable placeholder="如 FixedUsers" />
        </el-form-item>
      </template>
      <template v-else-if="type === 'ProcessRelative'">
        <el-form-item label="相对类型 relative">
          <el-select v-model="draft.relative" clearable style="width: 100%">
            <el-option label="Starter" value="Starter" />
            <el-option label="Previous" value="Previous" />
            <el-option label="Superior" value="Superior" />
          </el-select>
        </el-form-item>
      </template>
      <el-alert
        v-else-if="type === 'StarterPick' || type === 'ApproverPick' || type === 'Specific'"
        type="info"
        :closable="false"
        show-icon
        :title="pickHint"
      />
      <el-alert
        v-else-if="type"
        type="info"
        :closable="false"
        show-icon
        title="该 Type 可用下方高级 JSON 补充未知键；表单字段算人请用 Expression / BusinessCallback"
      />
    </template>

    <template v-else>
      <template v-if="type === 'Collaborate' || type === 'GroupCollaborate'">
        <el-form-item label="completeRate（0~1）">
          <el-input-number v-model="draft.completeRate" :min="0" :max="1" :step="0.1" />
        </el-form-item>
        <el-form-item v-if="type === 'GroupCollaborate'" label="groupJoin">
          <el-select v-model="draft.groupJoin" clearable style="width: 100%">
            <el-option label="And" value="And" />
            <el-option label="Or" value="Or" />
          </el-select>
        </el-form-item>
      </template>
      <template v-else-if="type === 'Captain'">
        <el-form-item label="gateRoles（关卡身份）">
          <el-select
            v-model="draft.gateRoles"
            multiple
            filterable
            allow-create
            default-first-option
            style="width: 100%"
          />
        </el-form-item>
      </template>
      <el-alert
        v-else-if="type === 'Race' || type === 'GroupRace'"
        type="info"
        :closable="false"
        show-icon
        title="或签 / 组内或签：通常无需 Params"
      />
    </template>

    <el-form-item v-if="showAdvanced" label="高级 Params JSON">
      <el-input v-model="advancedJson" type="textarea" :rows="3" @change="onAdvancedBlur" />
    </el-form-item>
    <el-button text type="primary" @click="showAdvanced = !showAdvanced">
      {{ showAdvanced ? '收起高级 JSON' : '高级 JSON' }}
    </el-button>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue';
import { getRoleAll, getUserAll, unwrapList, type RoleRow, type UserRow } from '@sail/api/resource';
import { parseParamsJson, stringifyParams } from '@sail/bpmn/assignee/types';

defineOptions({ name: 'SailAssigneeParamsForm' });

const props = defineProps<{
  kind: 'resolve' | 'leave';
  type: string;
  modelValue: string;
}>();

const emit = defineEmits<{
  'update:modelValue': [v: string];
  'edit-expression': [];
}>();

const userOptions = ref<string[]>([]);
const roleOptions = ref<Array<{ label: string; value: string }>>([]);
const showAdvanced = ref(false);
const advancedJson = ref('');

type Draft = {
  userIds: string[];
  roleIds: string[];
  unitField: string;
  mapRows: Array<{ key: string; value: string }>;
  expression: string;
  userIdsField: string;
  callback: string;
  onEmpty: string;
  nextType: string;
  relative: string;
  completeRate: number | undefined;
  groupJoin: string;
  gateRoles: string[];
};

const draft = reactive<Draft>(emptyDraft());

function emptyDraft(): Draft {
  return {
    userIds: [],
    roleIds: [],
    unitField: '',
    mapRows: [],
    expression: '',
    userIdsField: '',
    callback: '',
    onEmpty: '',
    nextType: '',
    relative: '',
    completeRate: undefined,
    groupJoin: '',
    gateRoles: []
  };
}

const pickHint = computed(() => {
  if (props.type === 'StarterPick') return '发起人自选：运行时由办理人指定下一跳；通常无需 Params';
  if (props.type === 'ApproverPick') return '发送人自选（出厂默认）：通常无需 Params';
  return 'Specific：办理人由调用方传入；通常无需 Params';
});

function applyFromJson(raw: string) {
  const p = parseParamsJson(raw);
  Object.assign(draft, emptyDraft());
  draft.userIds = Array.isArray(p.userIds) ? p.userIds.map(String) : Array.isArray(p.users) ? (p.users as unknown[]).map(String) : [];
  draft.roleIds = Array.isArray(p.roleIds) ? p.roleIds.map(String) : [];
  draft.unitField = String(p.unitField ?? '');
  const maps = (p.maps && typeof p.maps === 'object' ? p.maps : {}) as Record<string, string>;
  draft.mapRows = Object.keys(maps).map(k => ({ key: k, value: String(maps[k] ?? '') }));
  draft.expression = String(p.expression ?? '');
  draft.userIdsField = String(p.userIdsField ?? '');
  draft.callback = String(p.callback ?? '');
  draft.onEmpty = String(p.onEmpty ?? '');
  draft.nextType = String(p.nextType ?? '');
  draft.relative = String(p.relative ?? '');
  draft.completeRate =
    typeof p.completeRate === 'number' ? p.completeRate : p.completeRate != null ? Number(p.completeRate) : undefined;
  draft.groupJoin = String(p.groupJoin ?? '');
  draft.gateRoles = Array.isArray(p.gateRoles) ? p.gateRoles.map(String) : [];
  advancedJson.value = raw || '';
}

function buildParams(): Record<string, unknown> {
  const p: Record<string, unknown> = {};
  if (props.kind === 'resolve') {
    if (props.type === 'FixedUsers') p.userIds = [...draft.userIds];
    else if (props.type === 'RoleGroup') p.roleIds = [...draft.roleIds];
    else if (props.type === 'UnitMap' || props.type === 'JobRelative') {
      if (draft.unitField) p.unitField = draft.unitField;
      const maps: Record<string, string> = {};
      for (const r of draft.mapRows) {
        if (r.key.trim()) maps[r.key.trim()] = r.value;
      }
      if (Object.keys(maps).length) p.maps = maps;
    } else if (props.type === 'Expression') {
      if (draft.expression) p.expression = draft.expression;
      if (draft.userIdsField) p.userIdsField = draft.userIdsField;
    } else if (props.type === 'BusinessCallback' || props.type === 'Business.ClassStudents') {
      if (draft.callback) p.callback = draft.callback;
    } else if (props.type === 'Fallback') {
      if (draft.onEmpty) p.onEmpty = draft.onEmpty;
      if (draft.nextType) p.nextType = draft.nextType;
    } else if (props.type === 'ProcessRelative') {
      if (draft.relative) p.relative = draft.relative;
    }
  } else {
    if (props.type === 'Collaborate' || props.type === 'GroupCollaborate') {
      if (draft.completeRate != null && !Number.isNaN(draft.completeRate)) p.completeRate = draft.completeRate;
      if (props.type === 'GroupCollaborate' && draft.groupJoin) p.groupJoin = draft.groupJoin;
    } else if (props.type === 'Captain') {
      if (draft.gateRoles.length) p.gateRoles = [...draft.gateRoles];
    }
  }
  return p;
}

let syncing = false;

watch(
  () => [props.modelValue, props.type, props.kind] as const,
  () => {
    if (syncing) return;
    applyFromJson(props.modelValue || '');
  },
  { immediate: true }
);

watch(
  draft,
  () => {
    syncing = true;
    const json = stringifyParams(buildParams());
    advancedJson.value = json;
    emit('update:modelValue', json);
    syncing = false;
  },
  { deep: true }
);

function onAdvancedBlur() {
  try {
    const parsed = parseParamsJson(advancedJson.value);
    syncing = true;
    emit('update:modelValue', stringifyParams(parsed));
    applyFromJson(stringifyParams(parsed));
    syncing = false;
  } catch {
    /* keep */
  }
}

onMounted(async () => {
  try {
    const [u, r] = await Promise.all([getUserAll(), getRoleAll()]);
    userOptions.value = unwrapList<UserRow>(u)
      .map(x => String((x as any).Account || (x as any).account || x.UserId || (x as any).userId || ''))
      .filter(Boolean);
    roleOptions.value = unwrapList<RoleRow>(r).map(x => {
      const id = String((x as any).Id || (x as any).id || (x as any).RoleCode || x.RoleCode || '');
      const name = String(x.RoleName || (x as any).roleName || id);
      return { label: name, value: id || name };
    });
  } catch {
    userOptions.value = [];
    roleOptions.value = [];
  }
});
</script>

<style scoped>
.sf-assignee-hint {
  margin-bottom: 10px;
}
.sf-assignee-params__maps {
  display: flex;
  flex-direction: column;
  gap: 6px;
  width: 100%;
}
.sf-assignee-params__map-row {
  display: grid;
  grid-template-columns: 1fr 1fr auto;
  gap: 6px;
  align-items: center;
}
</style>
