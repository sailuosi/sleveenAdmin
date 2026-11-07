<template>
	<div class="sys-org-container">
		<el-row :gutter="8" style="width: 100%">
			<el-col :span="4" :xs="24">
				<OrgTree ref="orgTreeRef" @node-click="nodeClick" />
			</el-col>

			<el-col :span="20" :xs="24">
				<el-card shadow="hover" :body-style="{ paddingBottom: '0' }">
					<el-form :model="state.queryParams" ref="queryForm" :inline="true">
						<el-form-item label="机构名称">
							<el-input v-model="state.queryParams.name" placeholder="机构名称" clearable />
						</el-form-item>
						<el-form-item label="机构编码">
							<el-input v-model="state.queryParams.code" placeholder="机构编码" clearable />
						</el-form-item>
						<el-form-item label="机构类型">
							<el-select v-model="state.queryParams.type" filterable clearable class="w100">
								<el-option v-for="item in state.orgTypeList" :key="item.value" :label="item.value" :value="item.code" />
							</el-select>
						</el-form-item>
						<el-form-item>
							<el-button-group>
								<el-button type="primary" @click="handleQuery"> 查询 </el-button>
								<el-button type="info" @click="resetQuery"> 重置 </el-button>
							</el-button-group>
						</el-form-item>
						<el-form-item>
							<el-button type="primary"  @click="openAddOrg" v-auth="'sysOrg:add'"> 新增 </el-button>
						</el-form-item>
					</el-form>
				</el-card>

				<el-card class="full-table" shadow="hover" style="margin-top: 8px">
					<el-table :data="state.orgData" style="width: 100%" v-loading="state.loading" row-key="id" default-expand-all :tree-props="{ children: 'children', hasChildren: 'hasChildren' }" border>
						<el-table-column prop="Name" label="机构名称" header-align="center" show-overflow-tooltip />
						<el-table-column prop="Code" label="机构编码" align="center" show-overflow-tooltip />
						<el-table-column prop="Level" label="级别" width="70" align="center" show-overflow-tooltip />
						<el-table-column prop="Type" label="机构类型" align="center" :formatter="dictFormatter" show-overflow-tooltip />
						<el-table-column prop="OrderNo" label="排序" width="70" align="center" show-overflow-tooltip />
						<el-table-column label="状态" width="70" align="center" show-overflow-tooltip>
							<template #default="scope">
								<el-tag type="success" v-if="scope.row.Status === 1">启用</el-tag>
								<el-tag type="danger" v-else>禁用</el-tag>
							</template>
						</el-table-column>
						<el-table-column prop="CreateTime" label="修改时间" align="center" show-overflow-tooltip />
						<el-table-column prop="Remark" label="备注" header-align="center" show-overflow-tooltip />
						<el-table-column label="操作" width="140" fixed="right" align="center" show-overflow-tooltip>
							<template #default="scope">
								<el-button icon="ele-Edit" size="small" text type="primary" @click="openEditOrg(scope.row)" v-auth="'sysOrg:update'"> 编辑 </el-button>
								<el-button icon="ele-Delete" size="small" text type="danger" @click="delOrg(scope.row)" v-auth="'sysOrg:delete'"> 删除 </el-button>
							</template>
						</el-table-column>
					</el-table>
				</el-card>
			</el-col>
		</el-row>

		<EditOrg ref="editOrgRef" :title="state.editOrgTitle" :orgData="state.orgTreeData" @handleQuery="handleQuery" />
	</div>
</template>

<script setup name="sysOrg">
import { onMounted, reactive, ref } from 'vue';
import { ElMessageBox, ElMessage } from 'element-plus';
import OrgTree from './component/orgTree.vue';
import EditOrg from './component/editOrg.vue';

// import { getAPI } from '/@/utils/axios-utils';
// import { SysOrgApi, SysDictDataApi } from '/@/api-services/api';
// import { SysOrg } from '/@/api-services/models';
import { apiSysOrgListGet } from "@s/api/system/sysorgapi.js";
import {apiSysDictDataDataListCodeGet} from "@s/api/system/sysdictdataapi.js";

const editOrgRef = ref();
const orgTreeRef = ref();
const state = reactive({
	loading: false,
	orgData: [], // 机构列表数据
	orgTreeData: [] , // 机构树所有数据
	queryParams: {
		id: 0,
		name: undefined,
		code: undefined,
		type: undefined,
	},
	editOrgTitle: '',
	orgTypeList: [],
});

onMounted(async () => {
	handleQuery();

	let resDicData = await apiSysDictDataDataListCodeGet('org_type');
	state.orgTypeList = resDicData;
});

// 查询操作
const handleQuery = async (updateTree = false) => {
	state.loading = true;
	var res = await apiSysOrgListGet(state.queryParams);
	state.orgData = res ?? [];
	state.loading = false;
	// 是否更新左侧机构列表树
	if (updateTree == true) {
		orgTreeRef.value?.initTreeData();
	}

	// 若无选择节点并且查询条件为空时，更新编辑页面机构列表树
	if (state.queryParams.id == 0 && state.queryParams.name == undefined && state.queryParams.code == undefined && state.queryParams.type == undefined) state.orgTreeData = state.orgData;
};

// 重置操作
const resetQuery = () => {
	state.queryParams.id = 0;
	state.queryParams.name = undefined;
	state.queryParams.code = undefined;
	state.queryParams.type = undefined;
	handleQuery();
};

// 打开新增页面
const openAddOrg = () => {
	state.editOrgTitle = '添加机构';
	editOrgRef.value?.openDialog({ status: 1, orderNo: 100 });
};

// 打开编辑页面
const openEditOrg = (row) => {
	state.editOrgTitle = '编辑机构';
	editOrgRef.value?.openDialog(row);
};

// 删除
const delOrg = (row) => {
	ElMessageBox.confirm(`确定删除机构：【${row.name}】?`, '提示', {
		confirmButtonText: '确定',
		cancelButtonText: '取消',
		type: 'warning',
	})
		.then(async () => {
			await getAPI(SysOrgApi).apiSysOrgDeletePost({ id: row.id });
			ElMessage.success('删除成功');
			handleQuery(true);
		})
		.catch(() => {});
};

// 树组件点击
const nodeClick = async (node) => {
	state.queryParams.id = node.id;
	state.queryParams.name = undefined;
	state.queryParams.code = undefined;
	state.queryParams.type = undefined;
	handleQuery();
};

// 字典转换
const dictFormatter = (row, column, cellValue) => {
	return state.orgTypeList.find((u) => u.code == cellValue)?.value;
};
</script>
