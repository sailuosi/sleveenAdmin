<template>
  <formcard title="任务管理" width="" flex stretch>
    <template #toolbar>
      <div class="sf-formcard-toolbar">
        <el-input
          v-model="keyword"
          clearable
          placeholder="应用名称"
          style="width: 200px"
          @keyup.enter="reload"
        />
        <el-button :loading="loading" @click="reload">刷新</el-button>
      </div>
    </template>
    <flexpan :flex="true" height="100%">
      <template #header>
        <el-alert
          title="管理员视角：未按办理人过滤的待办任务（Ready）"
          type="info"
          show-icon
          :closable="false"
          class="sf-vc-alert"
        />
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
          empty-text="暂无任务"
          v-loading="loading"
          @query="getlist"
        >
          <vccol field="op" caption="操作" width="80px" type="text" disabled align="center" headerAlign="center">
            <template #default="{ row }">
              <el-link type="primary" @click="goDetail(row)">查看</el-link>
            </template>
          </vccol>
          <vccol field="appName" caption="应用" min-width="140px" type="text" />
          <vccol field="activityName" caption="节点" min-width="120px" type="text" />
          <vccol field="assignedUserName" caption="办理人" width="120px" type="text" />
          <vccol field="appInstanceId" caption="业务单号" min-width="140px" type="text" />
          <vccol field="createdDateTime" caption="到达时间" width="168px" type="datetime" />
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
import { querySailTodoList } from '@sail/api/workbench';

defineOptions({ name: 'SailTaskManagerPage' });

const router = useRouter();
const loading = ref(false);
const mlist = ref([]);
const total = ref(0);
const keyword = ref('');
const queryParams = ref({ pageSize: 20, pageNum: 1 });

const getlist = () => {
  loading.value = true;
  querySailTodoList({
    pageNum: queryParams.value.pageNum,
    pageSize: queryParams.value.pageSize,
    keyword: keyword.value || undefined,
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
    query: {
      taskId: String(row?.id ?? row?.Id ?? ''),
      processInstanceId: String(row?.processInstanceId || row?.ProcessInstanceId || '')
    }
  });
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
.sf-vc-alert {
  margin-bottom: 8px;
}
</style>
