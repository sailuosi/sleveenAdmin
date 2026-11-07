<template>
	<div class="worktype-select-list">
		<div class="worktype-select-list__toolbar">
			<span class="worktype-select-list__hint">已选 {{ selectedRows.length }} 项</span>
			<div class="worktype-select-list__actions">
				<el-input
					v-model="keyword"
					clearable
					placeholder="搜索管理员类别"
					style="width: 200px"
				>
					<template #prefix>
						<SvgIcon name="fa fa-search" />
					</template>
				</el-input>
				<el-button type="primary" :disabled="!selectedRows.length" @click="dook">
					<SvgIcon name="fa fa-check" /> 确 定
				</el-button>
				<el-button type="success" @click="loadData">
					<SvgIcon name="fa fa-refresh" /> 刷新
				</el-button>
			</div>
		</div>
		<div class="worktype-select-list__table">
			<el-table
				ref="tableRef"
				v-loading="loading"
				:data="pageRows"
				row-key="HH_WorkorTypeOID"
				height="100%"
				stripe
				border
				@selection-change="onSelectionChange"
			>
				<el-table-column type="selection" width="48" :selectable="rowSelectable" align="center" reserve-selection />
				<el-table-column type="index" label="序号" width="60" align="center" :index="indexMethod" />
				<el-table-column prop="WT_Title" label="管理员类别" min-width="160" show-overflow-tooltip />
				<el-table-column prop="WT_Note" label="简介" min-width="220" show-overflow-tooltip />
				<el-table-column label="操作" width="120" align="center" fixed="right">
					<template #default="{ row }">
						<el-link type="primary" @click.stop="openMenuView(row)">查看菜单权限</el-link>
					</template>
				</el-table-column>
			</el-table>
		</div>
		<div class="worktype-select-list__pager">
			<el-pagination
				v-model:current-page="page"
				v-model:page-size="pageSize"
				:total="filteredOpts.length"
				:page-sizes="[10, 20, 50]"
				layout="total, sizes, prev, pager, next"
				background
				small
			/>
		</div>

		<WorkorRoleMenuViewDialog
			v-model="menuViewOpened"
			mode="workorType"
			:row="menuViewRow"
			@close="closeMenuView"
		/>
	</div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from "vue";
import WorkorRoleMenuViewDialog from "../auth/WorkorRoleMenuViewDialog.vue";

defineOptions({
	name: "WorktypeSelectList",
	inheritAttrs: false,
});

const props = defineProps({
	added: {
		type: Array,
		default() {
			return [];
		},
	},
	api: null,
	src: null,
	/** 当前组织/地点卡片 HH_Organize_FK；不传则特殊（单卡片）Child 不出 */
	orgOid: { type: [String, Number], default: null },
});

const emit = defineEmits(["ok", "change"]);

const loading = ref(false);
const opts = ref([]);
const keyword = ref("");
const page = ref(1);
const pageSize = ref(50);
const selectedRows = ref([]);
const tableRef = ref(null);
const menuViewOpened = ref(false);
const menuViewRow = ref(null);

const filteredOpts = computed(() => {
	const kw = (keyword.value || "").trim().toLowerCase();
	if (!kw) return opts.value;
	return opts.value.filter((r) => String(r?.WT_Title || "").toLowerCase().includes(kw));
});

const pageRows = computed(() => {
	const start = (page.value - 1) * pageSize.value;
	return filteredOpts.value.slice(start, start + pageSize.value);
});

watch([keyword, pageSize], () => {
	page.value = 1;
});

watch(filteredOpts, (list) => {
	const maxPage = Math.max(1, Math.ceil(list.length / pageSize.value) || 1);
	if (page.value > maxPage) page.value = maxPage;
});

function toIdKey(id) {
	return id == null || id === "" ? null : String(id);
}

function isAdded(oid) {
	const key = toIdKey(oid);
	if (key == null) return false;
	return (props.added || []).some((x) => toIdKey(x) === key);
}

function rowSelectable(row) {
	return !isAdded(row?.HH_WorkorTypeOID);
}

function indexMethod(i) {
	return (page.value - 1) * pageSize.value + i + 1;
}

function unwrapList(res) {
	if (Array.isArray(res)) return res;
	if (Array.isArray(res?.data)) return res.data;
	if (Array.isArray(res?.result)) return res.result;
	return [];
}

function loadData() {
	if (!props.api?.loadWorkTypeSelect) return;
	loading.value = true;
	selectedRows.value = [];
	tableRef.value?.clearSelection?.();
	props.api
		.loadWorkTypeSelect({ src: props.src, orgOid: props.orgOid || undefined })
		.then((res) => {
			opts.value = unwrapList(res);
			page.value = 1;
		})
		.finally(() => {
			loading.value = false;
		});
}

function onSelectionChange(rows) {
	selectedRows.value = (rows || []).filter((r) => !isAdded(r?.HH_WorkorTypeOID));
	emit(
		"change",
		selectedRows.value.map((r) => r.HH_WorkorTypeOID),
	);
}

function dook() {
	const list = [];
	const val = [];
	selectedRows.value.forEach((li) => {
		if (!isAdded(li.HH_WorkorTypeOID)) {
			list.push(li);
			val.push(li.HH_WorkorTypeOID);
		}
	});
	if (!list.length) {
		swal({ text: "请先勾选要添加的管理员类别", icon: "info", button: "关闭" });
		return;
	}
	emit("ok", list, val);
	selectedRows.value = [];
	tableRef.value?.clearSelection?.();
}

function openMenuView(row) {
	if (!row?.HH_WorkorTypeOID) {
		swal({ text: "当前行无管理员类别主键，无法查看", icon: "info", button: "关闭" });
		return;
	}
	menuViewRow.value = row;
	menuViewOpened.value = true;
}

function closeMenuView() {
	menuViewOpened.value = false;
	menuViewRow.value = null;
}

onMounted(() => {
	loadData();
});
</script>

<style scoped>
.worktype-select-list {
	display: flex;
	flex-direction: column;
	height: min(62vh, 560px);
	min-height: 360px;
	box-sizing: border-box;
}
.worktype-select-list__toolbar {
	flex: 0 0 auto;
	display: flex;
	align-items: center;
	gap: 8px;
	margin-bottom: 10px;
}
.worktype-select-list__hint {
	color: var(--el-text-color-secondary);
	font-size: 13px;
	white-space: nowrap;
}
.worktype-select-list__actions {
	margin-left: auto;
	display: flex;
	align-items: center;
	gap: 8px;
}
.worktype-select-list__table {
	flex: 1 1 0;
	min-height: 0;
	overflow: hidden;
}
.worktype-select-list__pager {
	flex: 0 0 auto;
	display: flex;
	justify-content: flex-end;
	padding-top: 10px;
}
</style>
