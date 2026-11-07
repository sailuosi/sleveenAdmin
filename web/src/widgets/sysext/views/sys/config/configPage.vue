<template>
	<div class="sys-config-container">
		<el-card shadow="hover" :body-style="{ paddingBottom: '0' }">

		</el-card>
		<el-card class="full-table" shadow="hover" style="margin-top: 8px">
			<slimtable ref="tableRef" :list="configlist" :cols="cols" 
			>
				<template #command>
					<el-button type="primary" @click="openAddConfig" > 新增 </el-button>

					<el-button v-if="state.selectlist.length > 0" type="danger"  @click="bacthDelete" > 批量删除 </el-button>
				</template>
				<template #sysFlag="scope">
					<el-tag v-if="scope.row.sysFlag === 1"> 是 </el-tag>
					<el-tag type="danger" v-else> 否 </el-tag>
				</template>
				<template #action="scope">
					<el-button size="small" text type="primary" @click="openEditConfig(scope.row)" > 编辑 </el-button>
					<el-button size="small" text type="danger" @click="delConfig(scope.row)" > 删除 </el-button>
				</template>
			</slimtable>
		</el-card>

		<EditConfig ref="editConfigRef" :title="state.editConfigTitle" :groupList="state.groupList" @updateData="updateData" />
	</div>
</template>

<script lang="ts" setup name="sysConfig">
import { onMounted, onUnmounted, reactive, ref, defineAsyncComponent, nextTick } from 'vue';
import { ElMessageBox, ElMessage } from 'element-plus';

import EditConfig from './component/editConfig.vue';

import slimtable from "@/ui/common/list/slimtable.vue"
import { apiSysConfigPagePost,apiSysConfigGroupListGet,apiSysConfigDeletePost,apiSysConfigBatchDeletePost } from '@s/api/system/sysconfigapi.js';

// 引入组件

const editConfigRef = ref<InstanceType<typeof EditConfig>>();
const tableRef = ref<RefType>();

const state = reactive({
	editConfigTitle: '',
	selectlist: [] as EmptyObjectType[],
	groupList: [] as Array<String>,
});
const cols=		 [
		{ field: 'Name', width: 200, caption: '配置名称', headerAlign: 'center', sortable: 'custom', isCheck: true, hideCheck: true },
		{ field: 'Code', width: 200, caption: '配置编码', headerAlign: 'center', toolTip: true, sortable: 'custom', isCheck: true },
		{ field: 'Value', width: 150, caption: '属性值', headerAlign: 'center', isCheck: true },
		{ field: 'SysFlag', width: 100, caption: '内置参数', align: 'center', isCheck: true },
		{ field: 'GroupCode', width: 120, caption: '分组编码', align: 'center', sortable: 'custom', isCheck: true },
		{ field: 'OrderNo', width: 80, caption: '排序', align: 'center', sortable: 'custom', isCheck: true },
		{ field: 'Remark', caption: '备注', align: '', headerAlign: 'center', showOverflowTooltip: true, isCheck: true },
		{ field: 'Action', width: 140, caption: '操作', type: 'action', align: 'center', isCheck: true, fixed: 'right', hideCheck: true },
	];
const tb = reactive<TableDemoState>({
	tableData: {
		// 表头内容（必传，注意格式）
		columns: [
			{ field: 'name', width: 200, caption: '配置名称', headerAlign: 'center', sortable: 'custom', isCheck: true, hideCheck: true },
			{ field: 'code', width: 200, caption: '配置编码', headerAlign: 'center', toolTip: true, sortable: 'custom', isCheck: true },
			{ field: 'value', width: 150, caption: '属性值', headerAlign: 'center', isCheck: true },
			{ field: 'sysFlag', width: 100, caption: '内置参数', align: 'center', isCheck: true },
			{ field: 'groupCode', width: 120, caption: '分组编码', align: 'center', sortable: 'custom', isCheck: true },
			{ field: 'orderNo', width: 80, caption: '排序', align: 'center', sortable: 'custom', isCheck: true },
			{ field: 'remark', caption: '备注', align: '', headerAlign: 'center', showOverflowTooltip: true, isCheck: true },
			{ field: 'action', width: 140, caption: '操作', type: 'action', align: 'center', isCheck: true, fixed: 'right', hideCheck: true },
		],
		// 配置项（必传）
		config: {
			isStripe: true, // 是否显示表格斑马纹
			isBorder: false, // 是否显示表格边框
			isSerialNo: true, // 是否显示表格序号
			isSelection: true, // 是否勾选表格多选
			showSelection: true, //是否显示表格多选
			pageSize: 20, // 每页条数
			hideExport: false, //是否隐藏导出按钮
			exportFileName: '系统参数', //导出报表的文件名，不填写取应用名称
		},
		// 搜索表单，动态生成（传空数组时，将不显示搜索，type有3种类型：input,date,select）
		search: [
			{ label: '配置名称', prop: 'name', placeholder: '搜索配置名称', required: false, type: 'input' },
			{ label: '配置编码', prop: 'code', placeholder: '搜索配置编码', required: false, type: 'input' },
			// { label: '创建时间', prop: 'time', placeholder: '请选择', required: false, type: 'date' },
		],
		param: {},
		defaultSort: {
			prop: 'orderNo',
			order: 'ascending',
		},
	},
});


const configlist = ref<any[]>([]);
const getData = (param: any) => {
	return apiSysConfigPagePost(param)
		.then((res) => {
			configlist.value = res.Items ?? [];

		});
};
const exportChangeData = (data: Array<EmptyObjectType>) => {
	data.forEach((item) => {
		item.sysFlag = item.sysFlag == 1 ? '是' : '否';
	});
	return data;
};
// 拖动显示列排序回调
const onSortHeader = (data: object[]) => {
	tb.tableData.columns = data;
};
// 搜索点击时表单回调
const onSearch = (data: EmptyObjectType) => {
	tb.tableData.param = Object.assign({}, tb.tableData.param, { ...data });
	nextTick(() => {
		tableRef.value.pageReset();
	});
};

const getGroupList = async () => {
	const res = await apiSysConfigGroupListGet();
	const groupSearch = {
		label: '分组编码',
		prop: 'groupCode',
		placeholder: '请选择',
		required: false,
		type: 'select',
		options: [],
	} as TableSearchType;
	state.groupList = res ?? [];
	res?.forEach((item) => {
		groupSearch.options?.push({ label: item, value: item });
	});
	let group = tb.tableData.search.filter((item) => {
		return item.prop == 'groupCode';
	});
	if (group.length == 0) {
		tb.tableData.search.push(groupSearch);
	} else {
		group[0] = groupSearch;
	}
};

//表格多选事件
const tableSelection = (data: EmptyObjectType[]) => {
	// console.log('表格多选事件', data)
	state.selectlist = data;
};

onMounted(async () => {
	getGroupList();
	getData({});
});

// 更新数据
const updateData = () => {
	tableRef.value.handleList();
	getGroupList();
};

// 打开新增页面
const openAddConfig = () => {
	state.editConfigTitle = '添加配置';
	editConfigRef.value?.openDialog({ sysFlag: 2, orderNo: 100 });
};

// 打开编辑页面
const openEditConfig = (row: any) => {
	state.editConfigTitle = '编辑配置';
	editConfigRef.value?.openDialog(row);
};

// 删除
const delConfig = (row: any) => {
	ElMessageBox.confirm(`确定删除配置：【${row.name}】?`, '提示', {
		confirmButtonText: '确定',
		cancelButtonText: '取消',
		type: 'warning',
	})
		.then(async () => {
			await apiSysConfigDeletePost({ id: row.id });
			tableRef.value.handleList();
			ElMessage.success('删除成功');
		})
		.catch(() => {});
};

//批量删除
const bacthDelete = () => {
	if (state.selectlist.length == 0) return false;
	ElMessageBox.confirm(`确定批量删除【${state.selectlist[0].name}】等${state.selectlist.length}个配置?`, '提示', {
		confirmButtonText: '确定',
		cancelButtonText: '取消',
		type: 'warning',
	})
		.then(async () => {
			const ids = state.selectlist.map((item) => {
				return item.id;
			});
			var res = await apiSysConfigBatchDeletePost(ids);
			tableRef.value.pageReset();
			ElMessage.success('删除成功');
		})
		.catch(() => {});
};
</script>
