<template>
  <formcard title="流程模板中心" width="" flex stretch>
    <template #toolbar>
      <div class="sf-formcard-toolbar">
        <el-button @click="router.push('/sail/processes')">单位流程配置</el-button>
        <el-button :loading="loadingCatalog" @click="loadCatalog">刷新目录</el-button>
        <el-button type="primary" @click="router.push('/sail/designer')">打开设计器</el-button>
      </div>
    </template>
    <main class="sf-tpl-page__main">
      <el-tabs v-model="tab">
        <el-tab-pane label="模板/公共目录" name="catalog">
          <el-table v-loading="loadingCatalog" :data="catalog" stripe empty-text="暂无 TEMPLATE/PUBLIC 流程">
            <el-table-column prop="ProcessName" label="名称" min-width="160" />
            <el-table-column prop="ProcessCode" label="编码" width="140" />
            <el-table-column prop="AuthType" label="归属" width="110" />
            <el-table-column prop="Version" label="版本" width="80" />
            <el-table-column prop="Status" label="状态" width="90">
              <template #default="{ row }">
                <el-tag :type="Number(row.Status) === 1 ? 'success' : 'info'" size="small">
                  {{ Number(row.Status) === 1 ? '启用' : '停用' }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column label="操作" width="260" fixed="right">
              <template #default="{ row }">
                <el-button link type="primary" @click="goEdit(row)">编辑</el-button>
                <el-button
                  link
                  type="primary"
                  :disabled="Number(row.Status) !== 1"
                  @click="openCopy(row)"
                >
                  创建单位草稿
                </el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-tab-pane>
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
      <p class="sf-tpl-page__hint">
        TEMPLATE/PUBLIC 在此维护；「创建单位草稿」写入 AuthType=UNIT。图种模板创建后请到编辑页补归属与业务标识。
      </p>
    </main>

    <el-dialog v-model="copyVisible" title="创建单位流程草稿" width="420px">
      <el-form label-width="100px">
        <el-form-item label="目标单位Id" required>
          <el-input v-model="copyUnitId" placeholder="SysOrg / 人力单位 Id" />
        </el-form-item>
        <el-form-item label="流程名称">
          <el-input v-model="copyName" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="copyVisible = false">取消</el-button>
        <el-button type="primary" :loading="copying" @click="doCopy">确定</el-button>
      </template>
    </el-dialog>
  </formcard>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage } from 'element-plus';
import formcard from '@/ui/panels/formcard.vue';
import { createProcessByTemplate, getProcessListSimple } from '@sail/api/process';
import { copyProcessToUnit } from '@sail/api/unitScope';
import { unpackSailResult, pickField } from '@sail/utils/sailResult';

defineOptions({ name: 'SailTemplateGalleryPage' });

const router = useRouter();
const tab = ref<'catalog' | 'std' | 'biz'>('catalog');
const creating = ref(false);
const loadingCatalog = ref(false);
const catalog = ref<Array<Record<string, unknown>>>([]);
const copyVisible = ref(false);
const copying = ref(false);
const copyUnitId = ref('');
const copyName = ref('');
const copySource = ref<Record<string, unknown> | null>(null);

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

async function loadCatalog() {
  loadingCatalog.value = true;
  try {
    const raw = await getProcessListSimple('templates');
    const data = unpackSailResult(raw);
    const list = (pickField(data, 'Entity', 'entity') || data) as unknown;
    catalog.value = Array.isArray(list) ? (list as Array<Record<string, unknown>>) : [];
  } catch (e: unknown) {
    catalog.value = [];
    ElMessage.error(e instanceof Error ? e.message : String(e));
  } finally {
    loadingCatalog.value = false;
  }
}

function goEdit(row: Record<string, unknown>) {
  router.push({
    path: '/sail/process-edit',
    query: {
      type: 'update',
      processId: String(row.ProcessId || row.processId || ''),
      version: String(row.Version || row.version || '1')
    }
  });
}

function openCopy(row: Record<string, unknown>) {
  copySource.value = row;
  copyName.value = `${String(row.ProcessName || row.processName || '')}-单位草稿`;
  copyUnitId.value = '';
  copyVisible.value = true;
}

async function doCopy() {
  if (!copySource.value || !copyUnitId.value.trim()) {
    ElMessage.warning('请填写目标单位 Id');
    return;
  }
  copying.value = true;
  try {
    const raw = await copyProcessToUnit({
      SourceProcessId: String(copySource.value.ProcessId || copySource.value.processId),
      SourceVersion: String(copySource.value.Version || copySource.value.version || '1'),
      TargetUnitId: copyUnitId.value.trim(),
      ProcessName: copyName.value || undefined
    });
    const data = unpackSailResult(raw);
    if ((data as { Status?: number })?.Status === -1) {
      ElMessage.error((data as { Message?: string })?.Message || '复制失败');
      return;
    }
    const entity = (pickField(data, 'Entity', 'entity') || data) as Record<string, unknown>;
    const process = (pickField(entity, 'Process', 'process') || {}) as Record<string, unknown>;
    ElMessage.success('已创建单位草稿');
    copyVisible.value = false;
    router.push({
      path: '/sail/process-edit',
      query: {
        type: 'update',
        processId: String(process.ProcessId || process.processId || ''),
        version: String(process.Version || process.version || '1')
      }
    });
  } catch (e: unknown) {
    ElMessage.error(e instanceof Error ? e.message : String(e));
  } finally {
    copying.value = false;
  }
}

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
        path: '/sail/process-edit',
        query: {
          type: 'update',
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

onMounted(() => {
  loadCatalog();
});
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
