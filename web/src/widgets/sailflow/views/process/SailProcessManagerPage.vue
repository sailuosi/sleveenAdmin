<template>
  <formcard title="流程实例管理" width="" flex stretch>
    <template #toolbar>
      <div class="sf-formcard-toolbar">
        <el-select v-model="processState" clearable placeholder="状态" style="width: 120px" @change="reload">
          <el-option label="就绪" :value="1" />
          <el-option label="运行中" :value="2" />
          <el-option label="完成" :value="4" />
          <el-option label="挂起" :value="5" />
          <el-option label="已取消" :value="6" />
          <el-option label="已终止" :value="8" />
        </el-select>
        <el-input
          v-model="keyword"
          clearable
          placeholder="应用名称"
          style="width: 180px"
          @keyup.enter="reload"
        />
        <el-button :loading="loading" @click="reload">刷新</el-button>
      </div>
    </template>
    <flexpan :flex="true" height="100%">
      <template #default="p">
        <vclist
          :list="mlist"
          :total="total"
          :para="queryParams"
          :height="p.height"
          :flex="true"
          query
          :orderCol="true"
          :stripe="true"
          empty-text="暂无实例"
          v-loading="loading"
          @query="getlist"
        >
          <vccol field="op" caption="操作" width="148px" type="text" disabled align="center" headerAlign="center">
            <template #default="{ row }">
              <el-link type="primary" @click="goDetail(row)">详情</el-link>
              <el-link
                v-if="canForceEnd(row)"
                type="danger"
                class="sf-op-link"
                :disabled="endingId === rowId(row)"
                @click="onForceEnd(row)"
              >
                强制结束
              </el-link>
            </template>
          </vccol>
          <vccol field="processName" caption="流程" min-width="140px" type="text" />
          <vccol field="appName" caption="应用" min-width="120px" type="text" />
          <vccol field="appInstanceId" caption="业务单号" min-width="140px" type="text" />
          <vccol field="createdUserName" caption="发起人" width="110px" type="text" />
          <vccol field="processState" caption="状态" width="88px" type="text" disabled>
            <template #default="{ row }">
              {{ processStateLabel(row) }}
            </template>
          </vccol>
          <vccol field="createdDateTime" caption="发起时间" width="168px" type="datetime" />
        </vclist>
      </template>
    </flexpan>
  </formcard>
</template>

<script setup>
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessageBox } from 'element-plus';
import formcard from '@/ui/panels/formcard.vue';
import flexpan from '@/ui/common/flexpan.vue';
import vclist from '@/ui/common/vclist/vclist.vue';
import vccol from '@/ui/common/vclist/vccol.vue';
import { querySailProcessList } from '@sail/api/workbench';
import { terminateProcess } from '@sail/api/runtime';

defineOptions({ name: 'SailProcessManagerPage' });

const PROCESS_STATE_LABEL = {
  0: '未启动',
  1: '就绪',
  2: '运行中',
  4: '已完成',
  5: '已挂起',
  6: '已取消',
  7: '已作废',
  8: '已终止'
};

/** Ready / Running / Suspended — 与引擎 Terminate 可终止态一致 */
const FORCE_END_STATES = new Set([1, 2, 5]);

const router = useRouter();
const loading = ref(false);
const endingId = ref('');
const mlist = ref([]);
const total = ref(0);
const keyword = ref('');
const processState = ref(undefined);
const queryParams = ref({ pageSize: 20, pageNum: 1 });

function pick(row, ...keys) {
  for (const k of keys) {
    const v = row?.[k];
    if (v != null && v !== '') return v;
  }
  return '';
}

function rowId(row) {
  return String(pick(row, 'id', 'Id') || '');
}

function rowState(row) {
  return Number(pick(row, 'processState', 'ProcessState') || 0);
}

function processStateLabel(row) {
  const st = rowState(row);
  return PROCESS_STATE_LABEL[st] || String(st || '-');
}

function canForceEnd(row) {
  return FORCE_END_STATES.has(rowState(row));
}

const getlist = () => {
  loading.value = true;
  querySailProcessList({
    pageNum: queryParams.value.pageNum,
    pageSize: queryParams.value.pageSize,
    keyword: keyword.value || undefined,
    processState: processState.value ?? null,
    mine: false
  })
    .then(res => {
      mlist.value = res?.records || res?.Records || res?.Items || [];
      total.value = res?.total ?? res?.Total ?? 0;
    })
    .catch(err => {
      mlist.value = [];
      total.value = 0;
      window.$message?.error?.(err?.message || String(err));
    })
    .finally(() => {
      loading.value = false;
    });
};

function reload() {
  queryParams.value.pageNum = 1;
  getlist();
}

function goDetail(row) {
  router.push({
    path: '/sail/process-detail',
    query: { processInstanceId: rowId(row) }
  });
}

async function onForceEnd(row) {
  const id = rowId(row);
  if (!id || !canForceEnd(row)) return;
  try {
    await ElMessageBox.confirm(
      '确认强制结束该流程实例？运行中/挂起实例将被终止（≠ 关闭 / 作废），相关待办一并收口。',
      '强制结束',
      { type: 'warning', confirmButtonText: '强制结束', cancelButtonText: '取消' }
    );
  } catch {
    return;
  }

  endingId.value = id;
  try {
    await terminateProcess({
      ProcessInstanceId: id,
      AppInstanceId: String(pick(row, 'appInstanceId', 'AppInstanceId') || ''),
      AppName: String(pick(row, 'appName', 'AppName') || ''),
      ProcessId: String(pick(row, 'processId', 'ProcessId') || ''),
      Version: String(pick(row, 'version', 'Version') || '')
    });
    window.$message?.success?.('已强制结束');
    getlist();
  } catch (err) {
    window.$message?.error?.(err?.message || String(err));
  } finally {
    endingId.value = '';
  }
}

onMounted(getlist);
</script>

<style scoped>
.sf-formcard-toolbar {
  display: flex;
  gap: 8px;
  align-items: center;
  flex-wrap: wrap;
}
.sf-op-link {
  margin-left: 10px;
}
</style>
