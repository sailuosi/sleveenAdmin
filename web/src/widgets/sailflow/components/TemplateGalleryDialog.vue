<template>
  <el-dialog
    v-model="visible"
    title="从模板创建"
    width="720px"
    destroy-on-close
  >
    <el-tabs v-model="tab">
      <el-tab-pane label="标准" name="std">
        <div class="sf-tpl-grid">
          <button
            v-for="item in STD_TEMPLATES"
            :key="item.name"
            type="button"
            class="sf-tpl-card"
            :disabled="creating"
            @click="pick(item.name)"
          >
            <div class="sf-tpl-card__name">{{ item.label }}</div>
            <div class="sf-tpl-card__code">{{ item.name }}</div>
          </button>
        </div>
      </el-tab-pane>
      <el-tab-pane label="业务" name="biz">
        <div class="sf-tpl-grid">
          <button
            v-for="item in BIZ_TEMPLATES"
            :key="item.name"
            type="button"
            class="sf-tpl-card"
            :disabled="creating"
            @click="pick(item.name)"
          >
            <div class="sf-tpl-card__name">{{ item.label }}</div>
            <div class="sf-tpl-card__code">{{ item.name }}</div>
          </button>
        </div>
      </el-tab-pane>
    </el-tabs>
    <template #footer>
      <el-button @click="visible = false">关闭</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage } from 'element-plus';
import { createProcessByTemplate } from '@sail/api/process';

defineOptions({ name: 'TemplateGalleryDialog' });

const emit = defineEmits<{ created: [] }>();
const router = useRouter();

const visible = ref(false);
const tab = ref<'std' | 'biz'>('std');
const creating = ref(false);

const STD_TEMPLATES = [
  { name: 'Blank', label: '空白' },
  { name: 'Sequence', label: '顺序' },
  { name: 'Gateway', label: '网关' },
  { name: 'MultipleInstance', label: '会签' },
  { name: 'AndSplitMI', label: '并行会签' },
  { name: 'SubProcess', label: '子流程' },
  { name: 'Complex', label: '综合' }
];

const BIZ_TEMPLATES = [
  { name: 'AskforLeave', label: '请假' },
  { name: 'EOrder', label: '订单' },
  { name: 'Reimbursement', label: '报销' },
  { name: 'Warehousing', label: '入库' },
  { name: 'OfficeIn', label: '入职' },
  { name: 'Contract', label: '合同' }
];

function open() {
  tab.value = 'std';
  visible.value = true;
}

function unwrapEntity(data: any): any {
  return data?.Entity ?? data?.entity ?? data;
}

async function pick(templateName: string) {
  creating.value = true;
  try {
    const data = await createProcessByTemplate(templateName);
    if (data?.Status === -1 || data?.Status === '-1') {
      throw new Error(data?.Message || '创建失败');
    }
    const entity = unwrapEntity(data) as {
      ProcessId?: string;
      Version?: string;
    };
    ElMessage.success('已从模板创建');
    visible.value = false;
    emit('created');
    if (entity?.ProcessId) {
      router.push({
        path: '/sail/designer',
        query: {
          processId: String(entity.ProcessId),
          version: String(entity.Version || '1')
        }
      });
    }
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  } finally {
    creating.value = false;
  }
}

defineExpose({ open });
</script>

<style scoped>
.sf-tpl-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
  gap: 10px;
}
.sf-tpl-card {
  text-align: left;
  padding: 12px;
  border: 1px solid var(--el-border-color, #dcdfe6);
  border-radius: 6px;
  background: var(--el-bg-color, #fff);
  cursor: pointer;
}
.sf-tpl-card:hover:not(:disabled) {
  border-color: var(--el-color-primary);
}
.sf-tpl-card:disabled {
  opacity: 0.6;
  cursor: wait;
}
.sf-tpl-card__name {
  font-weight: 600;
  font-size: 14px;
}
.sf-tpl-card__code {
  margin-top: 4px;
  font-size: 12px;
  color: var(--el-text-color-secondary, #909399);
}
</style>
