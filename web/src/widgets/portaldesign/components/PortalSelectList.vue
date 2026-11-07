<template>
	<flexpan flex height="100%">
		<template #header>
			<el-form inline :model="queryParams" @submit.prevent>
				<el-form-item label="名称">
					<el-input v-model="filters.P_Name" clearable placeholder="名称" @keyup.enter="reload" />
				</el-form-item>
				<el-form-item label="编码">
					<el-input v-model="filters.P_Key" clearable placeholder="唯一编码" @keyup.enter="reload" />
				</el-form-item>
				<el-form-item>
					<el-button type="primary" @click="reload">查询</el-button>
				</el-form-item>
			</el-form>
		</template>
		<template #default="p">
			<vclist
				v-loading="loading"
				:list="list"
				:total="total"
				:para="queryParams"
				:height="p.height"
				@query="onQuery"
			>
				<el-table-column fixed="left" label="操作" width="110">
					<template #default="cell">
						<el-button type="warning" @click="onSelect(cell.row)">选择该门户</el-button>
					</template>
				</el-table-column>
				<vccol field="P_Name" caption="名称" min-width="180" align="left" />
				<vccol field="P_Key" caption="唯一编码" width="180" align="left" />
				<vccol field="P_IsOn" caption="启用" width="60" type="Boolean" />
				<vccol field="P_Note" caption="说明" min-width="200" align="left" />
			</vclist>
		</template>
	</flexpan>
</template>

<script setup>
import { reactive, ref, onMounted } from "vue";
import flexpan from "@/ui/common/flexpan.vue";
import vclist from "@/ui/common/vclist/vclist.vue";
import vccol from "@/ui/common/vclist/vccol.vue";
import { queryBCHHPortallist } from "@/widgets/bid/src/views/sln_portal/Page/comm/BPO_PortalListapi.js";

const emit = defineEmits(["select"]);

const loading = ref(false);
const list = ref([]);
const total = ref(0);
const filters = reactive({
	P_Name: "",
	P_Key: ""
});
const queryParams = reactive({
	pageNum: 1,
	pageSize: 20
});

function buildConditions() {
	const conds = [];
	if (filters.P_Name) {
		conds.push({ Name: "P_Name", QueryType: "Like", Value: filters.P_Name });
	}
	if (filters.P_Key) {
		conds.push({ Name: "P_Key", QueryType: "Like", Value: filters.P_Key });
	}
	return conds;
}

function loadList() {
	loading.value = true;
	const para = {
		pageNum: queryParams.pageNum,
		pageSize: queryParams.pageSize,
		conditions: buildConditions()
	};
	queryBCHHPortallist(para)
		.then((res) => {
			list.value = res?.Items || res?.items || [];
			total.value = res?.TotalCount ?? res?.Total ?? res?.total ?? list.value.length;
		})
		.finally(() => {
			loading.value = false;
		});
}

function reload() {
	queryParams.pageNum = 1;
	loadList();
}

function onQuery(para) {
	if (para?.pageNum != null) queryParams.pageNum = para.pageNum;
	if (para?.pageSize != null) queryParams.pageSize = para.pageSize;
	loadList();
}

function onSelect(row) {
	emit("select", row);
}

onMounted(loadList);
</script>
