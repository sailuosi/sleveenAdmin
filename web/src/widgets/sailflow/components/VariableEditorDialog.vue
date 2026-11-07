<template>
  <el-dialog
    v-model="visible"
    title="编辑节点变量"
    width="720px"
    destroy-on-close
    @closed="onClosed"
  >
    <div class="sf-var-editor__toolbar">
      <el-button type="primary" @click="addRow">添加</el-button>
      <span class="sf-var-editor__hint">变量存 wf_variable，不写入 BPMN XML</span>
    </div>
    <el-table :data="rows" border size="small" empty-text="暂无变量">
      <el-table-column label="方向" width="100">
        <template #default="{ row }">
          <el-select v-model="row.Direction" size="small">
            <el-option label="Input" value="Input" />
            <el-option label="Output" value="Output" />
          </el-select>
        </template>
      </el-table-column>
      <el-table-column label="名称" min-width="120">
        <template #default="{ row }">
          <el-input v-model="row.Name" size="small" />
        </template>
      </el-table-column>
      <el-table-column label="类型" width="110">
        <template #default="{ row }">
          <el-select v-model="row.Type" size="small" allow-create filterable>
            <el-option label="string" value="string" />
            <el-option label="int" value="int" />
            <el-option label="bool" value="bool" />
            <el-option label="decimal" value="decimal" />
          </el-select>
        </template>
      </el-table-column>
      <el-table-column label="默认值" min-width="100">
        <template #default="{ row }">
          <el-input v-model="row.DefaultValue" size="small" />
        </template>
      </el-table-column>
      <el-table-column label="必填" width="70" align="center">
        <template #default="{ row }">
          <el-checkbox v-model="row._required" />
        </template>
      </el-table-column>
      <el-table-column label="操作" width="70" fixed="right">
        <template #default="{ $index }">
          <el-button link type="danger" @click="rows.splice($index, 1)">删</el-button>
        </template>
      </el-table-column>
    </el-table>
    <template #footer>
      <el-button @click="visible = false">取消</el-button>
      <el-button type="primary" :loading="saving" @click="confirm">保存</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue';
import { ElMessage } from 'element-plus';
import { designerEvents } from '@sail/bpmn/ui/designerEvents';
import { getDesignerProcessMeta } from '@sail/bpmn/ui/designerContext';
import { saveVariableList, type VariableRow } from '@sail/api/variable';

defineOptions({ name: 'VariableEditorDialog' });

type EditRow = VariableRow & { _required?: boolean };

const visible = ref(false);
const saving = ref(false);
const rows = ref<EditRow[]>([]);
let activityId = '';
let onSaved: ((rows: VariableRow[]) => void) | null = null;

function toEditRows(list: VariableRow[]): EditRow[] {
  return list.map(r => ({
    ...r,
    Direction: r.Direction === 'Output' ? 'Output' : 'Input',
    Type: r.Type || 'string',
    _required: r.IsRequired === 1 || r.IsRequired === true
  }));
}

function addRow() {
  rows.value.push({
    Name: '',
    Type: 'string',
    Direction: 'Input',
    DefaultValue: '',
    _required: false
  });
}

function onClosed() {
  rows.value = [];
  activityId = '';
  onSaved = null;
}

async function confirm() {
  const { processId, version } = getDesignerProcessMeta();
  if (!processId || !activityId) {
    ElMessage.warning('缺少 ProcessId / ActivityId');
    return;
  }
  for (const r of rows.value) {
    if (!String(r.Name || '').trim()) {
      ElMessage.warning('变量名称不能为空');
      return;
    }
  }

  const payload: VariableRow[] = rows.value.map((r, i) => ({
    Id: r.Id,
    ProcessId: processId,
    Version: version || '1',
    ActivityId: activityId,
    Name: String(r.Name || '').trim(),
    Type: r.Type || 'string',
    Direction: r.Direction === 'Output' ? 'Output' : 'Input',
    DefaultValue: r.DefaultValue || '',
    IsRequired: r._required ? 1 : 0,
    IsReferenced: r.IsReferenced ?? 0,
    SourceRef: r.SourceRef || '',
    SourceVariableName: r.SourceVariableName || '',
    SortOrder: i,
    Description: r.Description || ''
  }));

  saving.value = true;
  try {
    const data = await saveVariableList(processId, version || '1', activityId, payload);
    if (data?.Status === -1 || data?.Status === '-1') {
      throw new Error(data?.Message || '保存变量失败');
    }
    onSaved?.(payload);
    ElMessage.success('变量已保存');
    visible.value = false;
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  } finally {
    saving.value = false;
  }
}

function openHandler(payload: {
  activityId: string;
  rows: VariableRow[];
  onSaved: (rows: VariableRow[]) => void;
}) {
  activityId = payload.activityId;
  onSaved = payload.onSaved;
  rows.value = toEditRows(payload.rows || []);
  visible.value = true;
}

onMounted(() => designerEvents.on('variable:open', openHandler));
onBeforeUnmount(() => designerEvents.off('variable:open', openHandler));
</script>

<style scoped>
.sf-var-editor__toolbar {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 12px;
}
.sf-var-editor__hint {
  font-size: 12px;
  color: var(--el-text-color-secondary, #909399);
}
</style>
