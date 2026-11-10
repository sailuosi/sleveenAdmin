<template>
  <formcard title="规则集" width="" flex stretch>
    <template #toolbar>
      <div class="sf-formcard-toolbar">
        <el-button text @click="goProcesses">流程列表</el-button>
        <el-button :loading="loading" @click="refresh">刷新</el-button>
        <el-button @click="createNew">新建</el-button>
        <el-button type="danger" plain :disabled="!form.RuleSetCode" @click="onDelete">删除</el-button>
        <el-button type="primary" :loading="saving" @click="onSave">保存</el-button>
      </div>
    </template>
    <main class="sf-ruleset-page__main">
      <aside class="sf-ruleset-page__list">
        <el-alert v-if="listErr" :title="listErr" type="error" show-icon :closable="false" />
        <el-table
          v-loading="loading"
          :data="rows"
          highlight-current-row
          height="100%"
          size="small"
          empty-text="暂无规则集"
          @current-change="onSelect"
        >
          <el-table-column prop="RuleSetCode" label="编码" min-width="120" show-overflow-tooltip />
          <el-table-column prop="RuleSetName" label="名称" min-width="120" show-overflow-tooltip />
          <el-table-column prop="Mode" label="模式" width="110" />
        </el-table>
      </aside>
      <section class="sf-ruleset-page__editor">
        <el-form label-width="88px" size="default">
          <el-form-item label="编码" required>
            <el-input
              v-model="form.RuleSetCode"
              :disabled="!!(form.RuleSetId || form.Id)"
              placeholder="RuleSetCode"
            />
          </el-form-item>
          <el-form-item label="名称" required>
            <el-input v-model="form.RuleSetName" placeholder="RuleSetName" />
          </el-form-item>
          <el-form-item label="模式">
            <el-select v-model="form.Mode" style="width: 220px" @change="onModeChange">
              <el-option label="ruleTypes (NRules)" :value="RULE_MODE_RULE_TYPES" />
              <el-option label="bindingsJson" :value="RULE_MODE_BINDINGS_JSON" />
            </el-select>
            <el-button link type="primary" class="sf-ruleset-page__sample" @click="fillSample">
              填入示例
            </el-button>
          </el-form-item>
          <el-form-item label="启用">
            <el-switch v-model="enabled" />
          </el-form-item>
          <el-form-item label="说明">
            <el-input v-model="form.Description" type="textarea" :rows="2" />
          </el-form-item>
          <el-form-item label="内容">
            <el-input
              v-model="form.RuleContent"
              type="textarea"
              :rows="12"
              class="sf-ruleset-page__code"
              placeholder="JSON rule_content"
            />
          </el-form-item>
          <el-form-item label="试跑变量">
            <el-input
              v-model="execVarsText"
              type="textarea"
              :rows="3"
              class="sf-ruleset-page__code"
              placeholder='{"Amount": 1200}'
            />
          </el-form-item>
          <el-form-item>
            <el-button :loading="executing" :disabled="!form.RuleSetCode" @click="onExecute">
              试跑 Execute
            </el-button>
            <span v-if="execResult" class="sf-ruleset-page__exec">{{ execResult }}</span>
          </el-form-item>
        </el-form>
      </section>
    </main>
  </formcard>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ElMessage, ElMessageBox } from 'element-plus';
import formcard from '@/ui/panels/formcard.vue';
import {
  RULE_CONTENT_SAMPLES,
  RULE_MODE_BINDINGS_JSON,
  RULE_MODE_RULE_TYPES,
  deleteRuleSet,
  executeRuleSet,
  getRuleSet,
  getRuleSetList,
  saveRuleSet,
  type RuleSetRow
} from '@sail/api/ruleset';

defineOptions({ name: 'SailRuleSetPage' });

const route = useRoute();
const router = useRouter();
const loading = ref(false);
const saving = ref(false);
const executing = ref(false);
const listErr = ref('');
const rows = ref<RuleSetRow[]>([]);
const execVarsText = ref('{"Amount": 1200}');
const execResult = ref('');

const form = reactive<RuleSetRow>({
  RuleSetCode: '',
  RuleSetName: '',
  Description: '',
  RuleContent: RULE_CONTENT_SAMPLES[RULE_MODE_BINDINGS_JSON],
  Mode: RULE_MODE_BINDINGS_JSON,
  IsEnabled: 1
});

const enabled = computed({
  get: () => Number(form.IsEnabled) === 1,
  set: (v: boolean) => {
    form.IsEnabled = v ? 1 : 0;
  }
});

function goProcesses() {
  router.push('/sail/processes');
}

function createNew() {
  Object.assign(form, {
    RuleSetId: undefined,
    Id: undefined,
    RuleSetCode: '',
    RuleSetName: '',
    Description: '',
    RuleContent: RULE_CONTENT_SAMPLES[RULE_MODE_BINDINGS_JSON],
    Mode: RULE_MODE_BINDINGS_JSON,
    IsEnabled: 1
  });
  execResult.value = '';
}

function onModeChange(mode: string) {
  if (!String(form.RuleContent || '').trim()) {
    form.RuleContent = RULE_CONTENT_SAMPLES[mode] || '';
  }
}

function fillSample() {
  form.RuleContent = RULE_CONTENT_SAMPLES[form.Mode || RULE_MODE_BINDINGS_JSON] || '';
}

function unwrapEntity(data: any): any {
  return data?.Entity ?? data?.entity ?? data;
}

async function refresh() {
  loading.value = true;
  listErr.value = '';
  try {
    const data = await getRuleSetList();
    if (data?.Status === -1 || data?.Status === '-1') {
      listErr.value = data?.Message || '加载失败';
      rows.value = [];
      return;
    }
    const list = unwrapEntity(data);
    rows.value = Array.isArray(list) ? list : [];
    const code = String(route.query.code || '');
    if (code) {
      const hit = rows.value.find(r => r.RuleSetCode === code);
      if (hit) await onSelect(hit);
    }
  } catch (e: unknown) {
    listErr.value = e instanceof Error ? e.message : String(e);
    rows.value = [];
  } finally {
    loading.value = false;
  }
}

async function onSelect(row: RuleSetRow | null) {
  if (!row?.RuleSetCode) return;
  try {
    const data = await getRuleSet(row.RuleSetCode);
    if (data?.Status === -1 || data?.Status === '-1') {
      throw new Error(data?.Message || '加载规则集失败');
    }
    const entity = unwrapEntity(data) as RuleSetRow;
    Object.assign(form, {
      RuleSetId: entity.RuleSetId ?? entity.Id,
      Id: entity.RuleSetId ?? entity.Id,
      RuleSetCode: entity.RuleSetCode || '',
      RuleSetName: entity.RuleSetName || '',
      Description: entity.Description || '',
      RuleContent: entity.RuleContent || '',
      Mode: entity.Mode || RULE_MODE_RULE_TYPES,
      IsEnabled: entity.IsEnabled ?? 1
    });
    execResult.value = '';
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  }
}

async function onSave() {
  if (!form.RuleSetCode?.trim() || !form.RuleSetName?.trim()) {
    ElMessage.warning('编码与名称必填');
    return;
  }
  saving.value = true;
  try {
    const data = await saveRuleSet({
      RuleSetId: form.RuleSetId || form.Id,
      RuleSetCode: form.RuleSetCode.trim(),
      RuleSetName: form.RuleSetName.trim(),
      Description: form.Description || '',
      RuleContent: form.RuleContent || '',
      Mode: form.Mode || RULE_MODE_RULE_TYPES,
      IsEnabled: form.IsEnabled ?? 1
    });
    if (data?.Status === -1 || data?.Status === '-1') {
      throw new Error(data?.Message || '保存失败');
    }
    ElMessage.success('已保存');
    await refresh();
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  } finally {
    saving.value = false;
  }
}

async function onDelete() {
  const code = form.RuleSetCode?.trim();
  if (!code) return;
  try {
    await ElMessageBox.confirm(`删除规则集 ${code}？`, '确认', { type: 'warning' });
    const data = await deleteRuleSet(code);
    if (data?.Status === -1 || data?.Status === '-1') {
      throw new Error(data?.Message || '删除失败');
    }
    ElMessage.success('已删除');
    createNew();
    await refresh();
  } catch (e: unknown) {
    if (e === 'cancel') return;
    ElMessage.error(e instanceof Error ? e.message : String(e));
  }
}

async function onExecute() {
  const code = form.RuleSetCode?.trim();
  if (!code) return;
  let variables: Record<string, unknown> = {};
  try {
    variables = execVarsText.value.trim() ? JSON.parse(execVarsText.value) : {};
  } catch {
    ElMessage.warning('试跑变量须为合法 JSON');
    return;
  }
  executing.value = true;
  execResult.value = '';
  try {
    const data = await executeRuleSet({ RuleSetCode: code, Variables: variables });
    if (data?.Status === -1 || data?.Status === '-1') {
      throw new Error(data?.Message || '试跑失败');
    }
    const entity = unwrapEntity(data);
    execResult.value = typeof entity === 'string' ? entity : JSON.stringify(entity, null, 2);
    ElMessage.success('试跑完成');
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  } finally {
    executing.value = false;
  }
}

onMounted(async () => {
  await refresh();
  if (rows.value[0]) await onSelect(rows.value[0]);
  else createNew();
});
</script>

<style scoped>
.sf-formcard-toolbar {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  align-items: center;
}
.sf-ruleset-page__main {
  height: 100%;
  min-height: 0;
  display: flex;
}
.sf-ruleset-page__list {
  flex: 0 0 320px;
  border-right: 1px solid var(--el-border-color-lighter, #ebeef5);
  background: var(--el-bg-color, #fff);
  padding: 8px;
  overflow: hidden;
}
.sf-ruleset-page__editor {
  flex: 1 1 auto;
  min-width: 0;
  padding: 16px 20px;
  overflow: auto;
}
.sf-ruleset-page__sample {
  margin-left: 12px;
}
.sf-ruleset-page__code :deep(textarea) {
  font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace;
  font-size: 12px;
}
.sf-ruleset-page__exec {
  margin-left: 12px;
  font-size: 12px;
  color: var(--el-text-color-secondary);
  white-space: pre-wrap;
  word-break: break-all;
}
@media (max-width: 900px) {
  .sf-ruleset-page__main {
    flex-direction: column;
  }
  .sf-ruleset-page__list {
    flex: 0 0 40%;
    border-right: none;
    border-bottom: 1px solid var(--el-border-color-lighter, #ebeef5);
  }
}
</style>
