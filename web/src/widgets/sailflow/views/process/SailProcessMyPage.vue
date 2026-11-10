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
          <vccol field="op" caption="操作" width="80px" type="text" disabled align="center" headerAlign="center">
            <template #default="{ row }">
              <el-link type="primary" @click="goDetail(row)">详情</el-link>
            </template>
          </vccol>
          <vccol field="processName" caption="流程" min-width="140px" type="text" />
          <vccol field="appName" caption="应用" min-width="120px" type="text" />
          <vccol field="appInstanceId" caption="业务单号" min-width="140px" type="text" />
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
import formcard from '@/ui/panels/formcard.vue';
import flexpan from '@/ui/common/flexpan.vue';
import vclist from '@/ui/common/vclist/vclist.vue';
import vccol from '@/ui/common/vclist/vccol.vue';
import { querySailProcessList } from '@sail/api/workbench';

defineOptions({ name: 'SailProcessMyPage' });

const router = useRouter();
const loading = ref(false);
const mlist = ref([]);
const total = ref(0);
const queryParams = ref({ pageSize: 20, pageNum: 1 });

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

function goDetail(row) {
  router.push({
    path: '/sail/process-detail',
    query: { processInstanceId: String(row?.processInstanceId ?? row?.ProcessInstanceId ?? row?.id ?? row?.Id ?? '') }
  });
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
