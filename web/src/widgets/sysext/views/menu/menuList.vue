<template>
	<div class="sys-menu-container">
		<el-card class="menu-query-bar" shadow="hover" :body-style="{ paddingBottom: '0' }">
			<el-form :model="state.queryParams" ref="queryForm" :inline="true">
				<el-form-item label="菜单名称">
					<el-input v-model="state.queryParams.title" placeholder="菜单名称" clearable />
				</el-form-item>
				<el-form-item label="类型">
					<el-select v-model="state.queryParams.type" placeholder="类型" clearable style="width: 150px;">
						<el-option label="目录" :value="1" />
						<el-option label="菜单" :value="2" />
						<el-option label="按钮" :value="3" />
					</el-select>
				</el-form-item>
				<el-form-item>
					<el-button-group>
						<el-button type="primary" @click="handleQuery"> <SvgIcon name="fa fa-search" /> 查询 </el-button>
						<el-button type="info" @click="resetQuery"> <SvgIcon name="fa fa-refresh" /> 重置 </el-button>
					</el-button-group>
				</el-form-item>
				<el-form-item>
					<el-button type="primary" @click="openAddMenu"> <SvgIcon name="fa fa-plus" /> 新增 </el-button>
				</el-form-item>
			</el-form>
		</el-card>

		<div class="menu-table-wrap">
			<el-card class="full-table" shadow="hover" v-if="!!state.menuData">
				<el-table
					:data="state.menuData"
					style="width: 100%"
					v-loading="state.loading"
					row-key="Id"
					:tree-props="{ children: 'Children', hasChildren: 'hasChildren' }"
					border
				>
					<el-table-column label="菜单名称" header-align="center" show-overflow-tooltip min-width="160">
						<template #default="scope">
							<span class="ml10">{{ $t(scope.row?.Title) }}</span>
						</template>
					</el-table-column>
					<el-table-column label="类型" width="70" align="center" show-overflow-tooltip>
						<template #default="scope">
							<el-tag type="warning" v-if="scope.row.Type === 1">目录</el-tag>
							<el-tag v-else-if="scope.row.Type === 2">菜单</el-tag>
							<el-tag type="info" v-else>按钮</el-tag>
						</template>
					</el-table-column>
					<el-table-column prop="Path" label="路由路径" header-align="center" show-overflow-tooltip min-width="110" />
					<el-table-column prop="Component" label="组件路径" align="center" show-overflow-tooltip min-width="180" />
					<el-table-column prop="Permission" label="权限标识" align="center" show-overflow-tooltip width="150" />
					<el-table-column prop="OrderNo" label="排序" width="70" align="center" show-overflow-tooltip />
					<el-table-column label="状态" width="80" align="center" show-overflow-tooltip>
						<template #default="scope">
							<el-tag type="success" v-if="scope.row.Status === 1">启用</el-tag>
							<el-tag type="danger" v-else>禁用</el-tag>
						</template>
					</el-table-column>
					<el-table-column prop="CreateTime" label="修改时间" align="center" show-overflow-tooltip width="180" />
					<el-table-column label="操作" width="280" align="center">
						<template #default="scope">
							<div class="menu-ops">
								<el-button size="small" text type="primary" @click="openEditMenu(scope.row)">编辑</el-button>
								<el-button
									v-if="scope.row.Type === 2"
									size="small"
									text
									type="warning"
									@click="initMenuButtons(scope.row)"
								>
									初始化按钮
								</el-button>
								<el-button size="small" text type="danger" @click="delMenu(scope.row)">删除</el-button>
								<el-button size="small" text type="success" @click="openAuthHolders(scope.row)">查看权限</el-button>
							</div>
						</template>
					</el-table-column>
				</el-table>
			</el-card>
		</div>

		<EditMenu ref="editMenuRef" :title="state.editMenuTitle" :menuData="state.menuData" @handleQuery="handleQuery" />
		<MenuAuthHoldersDialog
			v-model="authHoldersOpened"
			:menu-id="authHoldersMenuId"
			:menu-title="authHoldersMenuTitle"
			:menu-path="authHoldersMenuPath"
		/>
	</div>
</template>

<script lang="ts" setup name="sysMenu">
import { onMounted, reactive, ref } from 'vue';
import { ElMessageBox, ElMessage } from 'element-plus';
import EditMenu from './component/editMenu.vue';
import MenuAuthHoldersDialog from './component/menuAuthHoldersDialog.vue';

import { apiSysMenuListGet, apiSysMenuDeletePost, apiSysMenuInitButtonsPost } from './api/sysmenu.js';

const editMenuRef = ref<InstanceType<typeof EditMenu>>();
const authHoldersOpened = ref(false);
const authHoldersMenuId = ref<string | number | null>(null);
const authHoldersMenuTitle = ref('');
const authHoldersMenuPath = ref('');
const state = reactive({
	loading: false,
	menuData: [] as Array<SysMenu>,
	queryParams: {
		title: undefined,
		type: undefined,
	},
	editMenuTitle: '',
});

onMounted(async () => {
	handleQuery();
});

// 查询操作
const handleQuery = async () => {
	state.loading = true;
	var res = await apiSysMenuListGet(state.queryParams.title, state.queryParams.type);
	state.menuData = res ?? [];
	state.loading = false;
};

// 重置操作
const resetQuery = () => {
	state.queryParams.title = undefined;
	state.queryParams.type = undefined;
	handleQuery();
};

// 打开新增页面
const openAddMenu = () => {
	state.editMenuTitle = '添加菜单';
	editMenuRef.value?.openDialog({ Type: 2, IsHide: false, IsKeepAlive: true, IsAffix: false, IsIframe: false, IsApp: false, Status: 1, OrderNo: 100 });
};

// 打开编辑页面
const openEditMenu = (row: any) => {
	state.editMenuTitle = '编辑菜单';
	editMenuRef.value?.openDialog(row);
};

/** 从菜单树拼出根→当前节点的完整路径 */
function buildMenuPath(nodes: any[], targetId: string | number | null | undefined, trail: string[] = []): string | null {
	if (targetId == null || targetId === '') return null;
	for (const node of nodes || []) {
		const title = node?.Title ?? '';
		const next = title ? [...trail, title] : trail;
		if (String(node?.Id) === String(targetId)) return next.join(' / ');
		const found = buildMenuPath(node?.Children, targetId, next);
		if (found) return found;
	}
	return null;
}

// 查看拥有该菜单的管理员类别 / 角色
const openAuthHolders = (row: any) => {
	authHoldersMenuId.value = row?.Id ?? null;
	authHoldersMenuTitle.value = row?.Title ?? '';
	authHoldersMenuPath.value = buildMenuPath(state.menuData, row?.Id) || row?.Title || '';
	authHoldersOpened.value = true;
};

// 删除当前行
const delMenu = (row: any) => {
	ElMessageBox.confirm(`确定删除菜单：【${row.Title}】?`, '提示', {
		confirmButtonText: '确定',
		cancelButtonText: '取消',
		type: 'warning',
	})
		.then(async () => {
			await apiSysMenuDeletePost({ id: row.Id });
			handleQuery();
			ElMessage.success('删除成功');
		})
		.catch(() => {});
};

// 初始化菜单默认按钮权限
const initMenuButtons = (row: any) => {
	ElMessageBox.confirm(`为菜单【${row.Title}】初始化默认按钮权限（添加、修改、删除、导出）？`, '提示', {
		confirmButtonText: '确定',
		cancelButtonText: '取消',
		type: 'info',
	})
		.then(async () => {
			const added = await apiSysMenuInitButtonsPost({ id: row.Id });
			handleQuery();
			if (added > 0) {
				ElMessage.success(`成功初始化 ${added} 个按钮权限`);
			} else {
				ElMessage.info('默认按钮权限已存在，无需重复创建');
			}
		})
		.catch(() => {});
};
</script>

<style scoped>
/* 父级 mainpan-lv2 是 flex，子项默认 min-width:auto，树展开后列内容变宽会把整页撑出横滚 */
.sys-menu-container {
	width: 100%;
	min-width: 0;
	max-width: 100%;
	overflow-x: hidden;
	box-sizing: border-box;
}

.menu-query-bar {
	flex-shrink: 0;
}

/* 仅列表区域滚动：用视口扣掉头/页签/查询栏，避开 height:100% 链路断裂 */
.menu-table-wrap {
	margin-top: 8px;
	overflow: auto;
	max-height: calc(100vh - var(--soy-header-height, 56px) - var(--soy-tab-height, 48px) - 120px);
}

.full-table :deep(.el-card__body) {
	overflow-x: auto;
}

.full-table :deep(.el-table__inner-wrapper) {
	width: 100%;
}

.menu-ops {
	display: inline-flex;
	align-items: center;
	justify-content: center;
	white-space: nowrap;
}

.menu-ops :deep(.el-button) {
	padding-left: 5px;
	padding-right: 5px;
	margin-left: 0;
}
</style>
