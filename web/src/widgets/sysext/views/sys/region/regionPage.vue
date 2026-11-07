<template>
	<div class="sys-region-container">
		<el-row :gutter="8" style="width: 100%">
			<el-col :span="6" :xs="24">
				<RegionTree ref="regionTreeRef" @node-click="nodeClick" />
			</el-col>

			<el-col :span="18" :xs="24">
				<el-card shadow="hover" :body-style="{ paddingBottom: '0' }">
					<el-form :model="state.queryParams" ref="queryForm" :inline="true">
						<el-form-item label="行政名称">
							<el-input v-model="state.queryParams.name" placeholder="行政名称" clearable />
						</el-form-item>
						<el-form-item label="行政代码">
							<el-input v-model="state.queryParams.code" placeholder="行政代码" clearable />
						</el-form-item>
						<el-form-item>
							<el-button-group>
								<el-button type="primary" @click="handleQuery" > 查询 </el-button>
								<el-button type="info"  @click="resetQuery"> 重置 </el-button>
							</el-button-group>
						</el-form-item>
						<el-form-item>
							<el-button type="primary"  @click="openAddRegion" > 新增 </el-button>
							<el-button type="danger"  @click="handlSync" > 同步统计局 </el-button>
						</el-form-item>
					</el-form>
				</el-card>

				<el-card class="full-table" shadow="hover" style="margin-top: 8px">
					<el-table :data="state.regionData" style="width: 100%" v-loading="state.loading" row-key="id" default-expand-all :tree-props="{ children: 'children', hasChildren: 'hasChildren' }" border>
						<el-table-column prop="name" label="行政名称" align="center" show-overflow-tooltip />
						<el-table-column prop="code" label="行政代码" align="center" show-overflow-tooltip />
						<el-table-column prop="cityCode" label="区号" align="center" show-overflow-tooltip />
						<el-table-column prop="orderNo" label="排序" width="70" align="center" show-overflow-tooltip />
						<el-table-column prop="remark" label="备注" header-align="center" show-overflow-tooltip />
						<el-table-column label="操作" width="170" fixed="right" align="center" show-overflow-tooltip>
							<template #default="scope">
								<el-button icon="ele-Edit" size="small" text type="primary" @click="openEditRegion(scope.row)" > 编辑 </el-button>
								<el-button icon="ele-Delete" size="small" text type="danger" @click="delRegion(scope.row)" > 删除 </el-button>
							</template>
						</el-table-column>
					</el-table>
					<el-pagination
						v-model:currentPage="state.tableParams.page"
						v-model:page-size="state.tableParams.pageSize"
						:total="state.tableParams.total"
						:page-sizes="[10, 20, 50, 100]"
						small
						background
						@size-change="handleSizeChange"
						@current-change="handleCurrentChange"
						layout="total, sizes, prev, pager, next, jumper"
					/>
				</el-card>
			</el-col>
		</el-row>

		<EditRegion ref="editRegionRef" :title="state.editRegionTitle" @handleQuery="handleQuery" />
	</div>
</template>

<script  setup name="sysRegion">
import { onMounted, reactive, ref } from 'vue';
import { ElMessageBox, ElMessage, ElNotification } from 'element-plus';
import RegionTree from './component/regionTree.vue';
import EditRegion from './component/editRegion.vue';

import { loadRegionlist,rmRegion,syncRegion } from '@s/api/system/regionapi.js';


const editRegionRef = ref();
const regionTreeRef = ref();
const state = reactive({
	loading: false,
	regionData: [] , // 列表数据
	queryParams: {
		id: -1,
		pid: undefined,
		name: undefined,
		code: undefined,
	},
	tableParams: {
		page: 1,
		pageSize: 20,
		total: 0 ,
	},
	editRegionTitle: '',
});

onMounted(() => {
	handleQuery();
});

// 查询操作
const handleQuery = async () => {
	state.loading = true;
	let params = Object.assign(state.queryParams, state.tableParams);
	var res = await loadRegionlist(params);
	console.log(res);
	state.regionData = res.items ?? [];
	state.tableParams.total = res.total;
	state.loading = false;
};

// 重置操作
const resetQuery = () => {
	state.queryParams.id = -1;
	state.queryParams.pid = undefined;
	state.queryParams.name = undefined;
	state.queryParams.code = undefined;
	handleQuery();
};

// 打开新增页面
const openAddRegion = () => {
	state.editRegionTitle = '添加行政区域';
	editRegionRef.value?.openDialog({ orderNo: 100 });
};

// 打开编辑页面
const openEditRegion = (row) => {
	state.editRegionTitle = '编辑行政区域';
	editRegionRef.value?.openDialog(row);
};

// 删除
const delRegion = (row) => {
	ElMessageBox.confirm(`确定删除行政区域：【${row.name}】?`, '提示', {
		confirmButtonText: '确定',
		cancelButtonText: '取消',
		type: 'warning',
	})
		.then(async () => {
			await rmRegion({ id: row.id });
			handleQuery();
			// 编辑删除后更新机构数据
			regionTreeRef.value?.initTreeData();
			ElMessage.success('删除成功');
		})
		.catch(() => {});
};

// 树组件点击
const nodeClick = async (node) => {
	state.queryParams.pid = node.id;
	state.queryParams.name = undefined;
	state.queryParams.code = undefined;
	handleQuery();
};

// 同步国家统计局操作
const handlSync = async () => {
	ElMessageBox.confirm('确认同步国家统计局行政区域数据？', '提示', {
		confirmButtonText: '确定',
		cancelButtonText: '取消',
		type: 'warning',
	})
		.then(async () => {
			ElNotification({
				title: '提示',
				message: '后台努力同步中...',
				type: 'success',
				position: 'bottom-right',
			});
			await syncRegion({ timeout: 1000 * 60 * 30 });
		})
		.catch(() => {});
};

// 改变页面容量
const handleSizeChange = (val) => {
	state.tableParams.pageSize = val;
	handleQuery();
};

// 改变页码序号
const handleCurrentChange = (val) => {
	state.tableParams.page = val;
	handleQuery();
};
</script>
