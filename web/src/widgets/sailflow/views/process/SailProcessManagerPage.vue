<template>
  <flexpan :flex="true" height="100%">
    <template #header>
      <div class="sf-vc-toolbar">
        <div class="sf-vc-toolbar__title">流程实例管理</div>
        <div class="sf-vc-toolbar__actions">
          <el-select v-model="processState" clearable placeholder="状态" style="width: 120px" @change="reload">
            <el-option label="运行中" :value="2" />
            <el-option label="完成" :value="4" />
            <el-option label="取消/关闭" :value="5" />
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
      </div>
    </template>
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
        <vccol field="op" caption="操作" width="80px" type="text" disabled align="center" headerAlign="center">
          <template #default="{ row }">
            <el-link type="primary" @click="goDetail(row)">详情</el-link>
          </template>
        </vccol>
        <vccol field="processName" caption="流程" min-width="140px" type="text" />
        <vccol field="appName" caption="应用" min-width="120px" type="text" />
        <vccol field="appInstanceId" caption="业务单号" min-width="140px" type="text" />
        <vccol field="createdUserName" caption="发起人" width="110px" type="text" />
        <vccol field="processState" caption="状态" width="80px" type="text" />
        <vccol field="createdDateTime" caption="发起时间" width="168px" type="datetime" />
      </vclist>
    </template>
  </flexpan>
</template>

<script setup>
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import flexpan from '@/ui/common/flexpan.vue';
import vclist from '@/ui/common/vclist/vclist.vue';
import vccol from '@/ui/common/vclist/vccol.vue';
import { querySailProcessList } from '@sail/api/workbench';

defineOptions({ name: 'SailProcessManagerPage' });

const router = useRouter();
const loading = ref(false);
const mlist = ref([]);
const total = ref(0);
const keyword = ref('');
const processState = ref(undefined);
const queryParams = ref({ pageSize: 20, pageNum: 1 });

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
    query: { processInstanceId: String(row?.id ?? row?.Id ?? '') }
  });
}

onMounted(getlist);
</script>

<style scoped>
.sf-vc-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 8px 0 12px;
}
.sf-vc-toolbar__title {
  font-size: 16px;
  font-weight: 600;
}
.sf-vc-toolbar__actions {
  display: flex;
  gap: 8px;
  align-items: center;
  flex-wrap: wrap;
}
</style>
