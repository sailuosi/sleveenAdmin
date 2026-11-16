<template>
  <formcard title="我的流程" width="" flex stretch>
    <template #toolbar>
      <div class="sf-formcard-toolbar">
        <el-button :loading="loading" @click="reload">刷新</el-button>
        <el-button type="primary" @click="router.push('/sail/process-create')">发起流程</el-button>
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
          empty-text="暂无流程实例"
          v-loading="loading"
          @query="getlist"
        >
          <vccol field="op" caption="操作" width="120px" type="text" disabled align="center" headerAlign="center">
            <template #default="{ row }">
              <el-link type="primary" @click="goDetail(row)">详情</el-link>
              <el-link
                v-if="canCancelRow(row)"
                type="danger"
                style="margin-left: 8px"
                :disabled="cancelingId === rowId(row)"
                @click="onCancel(row)"
              >
                撤销
              </el-link>
            </template>
          </vccol>
          <vccol field="title" caption="标题" min-width="160px" type="text">
            <template #default="{ row }">
              {{ displayTitle(row) }}
            </template>
          </vccol>
          <vccol field="processName" caption="流程" min-width="120px" type="text" />
          <vccol field="bizCode" caption="业务编码" min-width="120px" type="text">
            <template #default="{ row }">
              {{
                row?.bizCode ||
                row?.BizCode ||
                row?.appInstanceCode ||
                row?.AppInstanceCode ||
                '-'
              }}
            </template>
          </vccol>
          <vccol field="processState" caption="状态" width="90px" type="text" />
          <vccol field="createdDateTime" caption="发起时间" width="168px" type="datetime" />
        </vclist>
      </template>
    </flexpan>
  </formcard>
</template>

<script setup>
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessage, ElMessageBox } from 'element-plus';
import formcard from '@/ui/panels/formcard.vue';
import flexpan from '@/ui/common/flexpan.vue';
import vclist from '@/ui/common/vclist/vclist.vue';
import vccol from '@/ui/common/vclist/vccol.vue';
import { querySailProcessList } from '@sail/api/workbench';
import { buttonsOperation } from '@sail/api/runtime';
import { useSailCurrentUser } from '@sail/adapters/currentUser';
import { unpackSailResult, pickField } from '@sail/utils/sailResult';

defineOptions({ name: 'SailProcessMyPage' });

const router = useRouter();
const { userId, userName } = useSailCurrentUser();
const loading = ref(false);
const mlist = ref([]);
const total = ref(0);
const queryParams = ref({ pageSize: 20, pageNum: 1 });
const cancelingId = ref('');
/** processId|version → allowCancel（懒加载后缓存；默认 true） */
const cancelGate = ref({});

const getlist = () => {
  loading.value = true;
  querySailProcessList({
    pageNum: queryParams.value.pageNum,
    pageSize: queryParams.value.pageSize,
    mine: true
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

function rowId(row) {
  return String(row?.processInstanceId ?? row?.ProcessInstanceId ?? row?.id ?? row?.Id ?? '');
}

function displayTitle(row) {
  return (
    String(row?.title ?? row?.Title ?? '').trim() ||
    String(row?.processName ?? row?.ProcessName ?? '').trim() ||
    '-'
  );
}

function canCancelRow(row) {
  const st = Number(row?.processState ?? row?.ProcessState ?? 0);
  if (st !== 2) return false;
  const starter = String(row?.createdUserId ?? row?.CreatedUserId ?? '');
  if (!starter || starter !== userId.value) return false;
  const pid = String(row?.processId ?? row?.ProcessId ?? '');
  const ver = String(row?.version ?? row?.Version ?? '1');
  const key = `${pid}|${ver}`;
  if (key in cancelGate.value) return !!cancelGate.value[key];
  // 默认允许；详情页 Extra 为准；列表侧保守展示，点击时服务端再闸
  return true;
}

function goDetail(row) {
  router.push({
    path: '/sail/process-detail',
    query: { processInstanceId: rowId(row) }
  });
}

async function onCancel(row) {
  try {
    await ElMessageBox.confirm('确认撤销该审批中的申请？', '撤销', { type: 'warning' });
  } catch {
    return;
  }
  const id = rowId(row);
  cancelingId.value = id;
  try {
    const data = await buttonsOperation({
      ButtonId: 'Close',
      Runner: {
        UserId: userId.value,
        UserName: userName.value,
        ProcessId: String(row?.processId ?? row?.ProcessId ?? ''),
        Version: String(row?.version ?? row?.Version ?? '1'),
        ProcessInstanceId: id,
        BizPK: String(row?.bizPK ?? row?.BizPK ?? row?.appInstanceId ?? row?.AppInstanceId ?? ''),
        BizChannel: String(
          row?.bizChannel ?? row?.BizChannel ?? row?.appName ?? row?.AppName ?? ''
        )
      }
    });
    const entity = unpackSailResult(data);
    const status = Number(
      pickField(entity, 'Status', 'status') ?? data?.Status ?? data?.status
    );
    if (status === -1) {
      const msg = String(
        pickField(entity, 'Message', 'message') || data?.Message || data?.message || '撤销失败'
      );
      // 缓存禁止撤销，避免反复点
      if (String(pickField(entity, 'ReasonCode', 'reasonCode') || '').includes('Cancel')) {
        const pid = String(row?.processId ?? row?.ProcessId ?? '');
        const ver = String(row?.version ?? row?.Version ?? '1');
        cancelGate.value = { ...cancelGate.value, [`${pid}|${ver}`]: false };
      }
      throw new Error(msg);
    }
    ElMessage.success('已撤销');
    getlist();
  } catch (e) {
    ElMessage.error(e?.message || String(e));
  } finally {
    cancelingId.value = '';
  }
}

onMounted(getlist);
</script>

<style scoped>
.sf-formcard-toolbar {
  display: flex;
  gap: 8px;
  align-items: center;
}
</style>
