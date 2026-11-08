<template>
  <formcard title="流程模板库" width="" flex stretch>
    <template #toolbar>
      <div class="sf-formcard-toolbar">
        <el-button @click="router.push('/sail/processes')">流程定义</el-button>
        <el-button @click="router.push('/sail/leave')">请假样例</el-button>
        <el-button type="primary" @click="router.push('/sail/designer')">打开设计器</el-button>
      </div>
    </template>
    <main class="sf-tpl-page__main">
      <el-tabs v-model="tab">
        <el-tab-pane label="标准模板" name="std">
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
        <el-tab-pane label="业务模板" name="biz">
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
      <p class="sf-tpl-page__hint">从模板创建后进入设计器；请假样例走「请假」页发起并回写单据状态。</p>
    </main>
  </formcard>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage } from 'element-plus';
import formcard from '@/ui/panels/formcard.vue';
import { createProcessByTemplate } from '@sail/api/process';

defineOptions({ name: 'SailTemplateGalleryPage' });

const router = useRouter();
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

async function pick(templateName: string) {
  creating.value = true;
  try {
    const data = await createProcessByTemplate(templateName);
    if (data?.Status === -1 || data?.Status === '-1') {
      throw new Error(data?.Message || '创建失败');
    }
    const entity = data?.Entity ?? data?.entity ?? data;
    ElMessage.success('已从模板创建');
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
</script>

<style scoped>
.sf-formcard-toolbar {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
  align-items: center;
}
.sf-tpl-page__main {
  height: 100%;
  padding: 16px;
  overflow: auto;
  box-sizing: border-box;
}
.sf-tpl-page__hint {
  margin-top: 16px;
  color: var(--el-text-color-secondary);
  font-size: 13px;
}
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
