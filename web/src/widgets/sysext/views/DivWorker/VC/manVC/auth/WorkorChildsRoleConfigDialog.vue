<template>
	<mdialog
		:title="dialogTitle"
		extraClass="workor-childs-role-dialog"
		v-model="opened"
		width="90%"
		maxWidth="1200px"
		top="5vh"
		destroy-on-close
		@close="onClose"
	>
		<div class="workor-pick-dialog-body workor-childs-role-body">
			<div class="role-toolbar">
				<el-button type="warning" @click="openAddRole">
					<SvgIcon name="fa fa-plus-circle" /> 添加
				</el-button>
				<el-button type="success" @click="getlist">
					<SvgIcon name="fa fa-refresh" /> 刷新
				</el-button>
			</div>
			<div class="role-list-wrap">
				<vclist
					:list="mlist"
					:total="total"
					:para="queryParams"
					@query="getlist"
					height="100%"
					flex
					v-loading="loading"
					query
					v-model:queryConditions="queryRowCondtions"
					:orderCol="true"
					:stripe="true"
				>
					<vccol field="op" caption="操作" width="120px" fixed="left" align="center">
						<template #default="cellp">
							<el-link type="primary" title="配置" @click="openRoleConfig(cellp.row)">
								<SvgIcon name="uil:setting" :size="16" />
							</el-link>
							<el-link type="danger" title="删除" @click="removeRow(cellp.row)" style="margin-left:10px;">
								<SvgIcon name="fa fa-trash" />
							</el-link>
							<el-link type="primary" title="查看菜单权限" @click="openMenuView(cellp.row)" style="margin-left:10px;">
								<SvgIcon name="hugeicons:view" :size="16" />
							</el-link>
						</template>
					</vccol>
					<vccol field="Di_Name" caption="名称" min-width="210px" align="left" />
					<vccol field="Di_Idx" caption="序号" width="120px" type="number" align="right" />
					<vccol field="Di_Code" caption="编码" width="180px" align="right" />
				</vclist>
			</div>
		</div>
	</mdialog>

	<mdialog
		title="为管理员类别设置适用角色"
		extraClass="workor-role-pick-dialog"
		v-model="drawEditOpened"
		width="90%"
		maxWidth="1200px"
		top="5vh"
		destroy-on-close
		@close="closeRowEdit"
	>
		<div class="workor-pick-dialog-body">
			<livebpo
				v-if="drawEditOpened"
				name="BPO_HH_DutyItemCheck"
				@close="closeRowEdit"
				@check="checkingRole"
			/>
		</div>
	</mdialog>

	<WorkorRoleConfigDialog
		v-model="roleConfigOpened"
		:row="roleConfigRow"
		@close="closeRoleConfig"
	/>

	<WorkorRoleMenuViewDialog
		v-model="menuViewOpened"
		:row="menuViewRow"
		@close="closeMenuView"
	/>
</template>

<script setup>
import { computed, ref, watch } from "vue";
import mdialog from "../../../ui/modal/mdialog.vue";
import vclist from "@/ui/common/vclist/vclist.vue";
import vccol from "@/ui/common/vclist/vccol.vue";
import livebpo from "@/uiext/gene/live/livebpo.vue";
import useListEditor from "@/uiext/gene/hook/listeditor.js";
import WorkorRoleConfigDialog from "./WorkorRoleConfigDialog.vue";
import WorkorRoleMenuViewDialog from "./WorkorRoleMenuViewDialog.vue";

const props = defineProps({
	modelValue: { type: Boolean, default: false },
	/** 可分配类别行（含 HH_WorkorType_FK / WT_Title） */
	row: { type: Object, default: null },
	api: { type: Object, default: null },
});

const emit = defineEmits(["update:modelValue", "close"]);

const opened = computed({
	get: () => props.modelValue,
	set: (v) => emit("update:modelValue", v),
});

const typeId = computed(() => props.row?.HH_WorkorType_FK || null);

const dialogTitle = computed(() => {
	const name = props.row?.WT_Title;
	return name ? `类别角色配置 - ${name}` : "类别角色配置";
});

const listHolder = useListEditor();
const loading = ref(false);
const mlist = ref([]);
const total = ref(0);
const queryParams = ref({
	pageSize: 30,
	pageNum: 1,
});
const queryRowCondtions = ref([]);

const roleConfigOpened = ref(false);
const roleConfigRow = ref(null);
const menuViewOpened = ref(false);
const menuViewRow = ref(null);
const drawEditOpened = ref(false);

function getlist() {
	if (!typeId.value || !props.api?.queryBCHHWorkorRolelist) {
		mlist.value = [];
		total.value = 0;
		return;
	}
	loading.value = true;
	const conditions = Array.isArray(queryRowCondtions.value) ? [...queryRowCondtions.value] : [];
	props.api
		.queryBCHHWorkorRolelist({
			masterFK: typeId.value,
			conditions,
			pageSize: queryParams.value.pageSize,
			pageNum: queryParams.value.pageNum,
		})
		.then((res) => {
			mlist.value = res.Items ?? [];
			total.value = res.Total ?? 0;
			listHolder.init(mlist.value, "HH_WorkorRoleOID");
		})
		.catch((err) => {
			swal({ text: err.message || "加载角色失败", icon: "error", button: "关闭" });
		})
		.finally(() => {
			loading.value = false;
		});
}

function saveChange(istip) {
	if (!props.api?.saveBCHHWorkorRoleList) {
		swal({ text: "未提供保存接口", icon: "error", button: "关闭" });
		return false;
	}
	const tar = listHolder.compare(mlist.value);
	if (tar.length === 0) {
		if (istip != false) {
			swal({ text: "没有变更的信息", icon: "info", button: "关闭" });
		}
		return false;
	}
	return props.api.saveBCHHWorkorRoleList(tar).then((res) => {
		if (istip != false) {
			swal({ text: res.data, icon: "info", button: "关闭" });
		}
		getlist();
		return res;
	});
}

function openAddRole() {
	if (!typeId.value) {
		swal({ text: "未找到类别信息，无法添加角色", icon: "info", button: "关闭" });
		return;
	}
	drawEditOpened.value = true;
}

function closeRowEdit() {
	drawEditOpened.value = false;
}

function checkingRole(e) {
	if (!Array.isArray(e) || e.length === 0) {
		swal({ text: "请先选择角色", icon: "info", button: "关闭" });
		return;
	}
	const existSet = new Set(
		mlist.value.map((r) => r.HH_DutyItem_FK).filter(Boolean),
	);
	let added = 0;
	e.forEach((li) => {
		const dutyId = li.HH_DutyItemOID;
		if (!dutyId || existSet.has(dutyId)) return;
		existSet.add(dutyId);
		mlist.value.push({
			HH_DutyItem_FK: dutyId,
			HH_WorkorType_FK: typeId.value,
			Di_Name: li.Di_Name,
			Di_Code: li.Di_Code,
			Di_Idx: li.Di_Idx,
		});
		added += 1;
	});
	if (added === 0) {
		swal({ text: "所选角色已存在，未新增", icon: "info", button: "关闭" });
		return;
	}
	const ret = saveChange();
	if (ret && typeof ret.then === "function") {
		ret.then(() => {
			closeRowEdit();
		});
	}
}

function openRoleConfig(row) {
	if (!row?.HH_DutyItem_FK) {
		swal({ text: "当前行未关联职责角色，无法配置", icon: "info", button: "关闭" });
		return;
	}
	roleConfigRow.value = row;
	roleConfigOpened.value = true;
}

function openMenuView(row) {
	if (!row?.HH_DutyItem_FK) {
		swal({ text: "当前行未关联职责角色，无法查看", icon: "info", button: "关闭" });
		return;
	}
	menuViewRow.value = row;
	menuViewOpened.value = true;
}

function removeRow(row, cap) {
	if (!row?.HH_WorkorRoleOID) {
		swal({ text: "当前行尚未保存，无法删除", icon: "info", button: "关闭" });
		return;
	}
	if (!props.api?.removeBCHHWorkorRoleByIds) {
		swal({ text: "未提供删除接口", icon: "error", button: "关闭" });
		return;
	}
	if (!cap) {
		cap = "删除当前行数据";
	}
	alertIsOK(
		"确认要操作吗？将删除当前行数据！",
		function (e) {
			if (e == "yes") {
				props.api.removeBCHHWorkorRoleByIds([row.HH_WorkorRoleOID]).then((res) => {
					getlist();
					swal({ text: res.data, icon: "success", button: "关闭" });
				});
			}
		},
		cap,
	);
}

function closeRoleConfig() {
	roleConfigOpened.value = false;
	roleConfigRow.value = null;
}

function closeMenuView() {
	menuViewOpened.value = false;
	menuViewRow.value = null;
}

function onClose() {
	mlist.value = [];
	total.value = 0;
	queryRowCondtions.value = [];
	closeRowEdit();
	closeRoleConfig();
	closeMenuView();
	emit("close");
}

watch(
	() => props.modelValue,
	(v) => {
		if (v) {
			queryParams.value.pageNum = 1;
			queryRowCondtions.value = [];
			getlist();
		}
	},
);
</script>

<style lang="css">
.el-overlay-dialog:has(.workor-childs-role-dialog) {
	display: flex !important;
	justify-content: center !important;
	align-items: flex-start !important;
}
.workor-childs-role-dialog.el-dialog.card-dia {
	width: min(1200px, 90vw) !important;
	max-width: 90vw !important;
	margin: 5vh auto !important;
	left: auto !important;
	right: auto !important;
	transform: none !important;
	outline: none !important;
	box-shadow: 0 8px 28px rgba(15, 23, 42, 0.12) !important;
	border-radius: 8px !important;
	overflow: hidden !important;
}
.workor-childs-role-dialog.el-dialog.card-dia .cardbox-pan {
	width: 100% !important;
	max-width: 100% !important;
	min-width: 0 !important;
	box-sizing: border-box;
	border-radius: 8px;
	overflow: hidden !important;
	display: flex !important;
	flex-direction: column !important;
}
.workor-childs-role-dialog.el-dialog.card-dia .cd-box {
	width: 100% !important;
	max-width: 100% !important;
	min-width: 0 !important;
	min-height: 0 !important;
	flex: 1 1 auto !important;
	box-sizing: border-box;
	overflow: hidden !important;
	display: flex !important;
	flex-direction: column !important;
}
.workor-childs-role-body {
	height: 75vh;
	max-height: calc(100vh - 120px);
	min-height: 420px;
	width: 100%;
	display: flex;
	flex-direction: column;
	overflow: hidden;
	box-sizing: border-box;
}
.workor-childs-role-body .role-toolbar {
	flex: 0 0 auto;
	padding: 0 0 8px;
	display: flex;
	justify-content: flex-end;
	gap: 8px;
}
.workor-childs-role-body .role-list-wrap {
	flex: 1 1 0;
	min-height: 0;
	overflow: hidden;
}
.workor-childs-role-body .vclistwrap,
.workor-childs-role-body .vclist-inner,
.workor-childs-role-body .vclist-inner-flex {
	height: 100% !important;
	min-height: 0 !important;
}
/* 表头白底黑字：盖过 global.css 的 #58a3f7 */
.workor-childs-role-body .vclist.sum-entry-table .el-table__header-wrapper th.el-table__cell,
.workor-childs-role-body .vclist.sum-entry-table .el-table__fixed-header-wrapper th.el-table__cell {
	background-color: #fff !important;
	color: #303133 !important;
	border-color: var(--el-border-color-lighter, #ebeef5) !important;
}
.workor-childs-role-body .vclist.sum-entry-table .el-table__header-wrapper th.el-table__cell {
	border-bottom: 1px solid var(--el-border-color-lighter, #ebeef5) !important;
}
.workor-childs-role-body .vclist.sum-entry-table .el-table__header-wrapper .vcthead.nqy,
.workor-childs-role-body .vclist.sum-entry-table .el-table__fixed-header-wrapper .vcthead.nqy {
	background-color: #fff !important;
	color: #303133 !important;
}
.workor-childs-role-body div.vchead.noquery {
	background-color: #fff !important;
	color: #303133 !important;
	border-bottom: 1px solid var(--el-border-color-lighter, #ebeef5) !important;
}
.workor-childs-role-body div.vchead--hasquery .vccap {
	background-color: #fff !important;
	color: #303133 !important;
}
.workor-childs-role-body .vclist.sum-entry-table .el-table__header-wrapper .cell,
.workor-childs-role-body .vclist.sum-entry-table .el-table__header-wrapper .vccap {
	color: #303133 !important;
}

/* 选角色弹窗（与 VC_HH_WorkorRoleList 一致） */
.el-overlay-dialog:has(.workor-role-pick-dialog) {
	display: flex !important;
	justify-content: center !important;
	align-items: flex-start !important;
}
.workor-role-pick-dialog.el-dialog.card-dia {
	width: min(1200px, 90vw) !important;
	max-width: 90vw !important;
	margin: 5vh auto !important;
	left: auto !important;
	right: auto !important;
	transform: none !important;
	outline: none !important;
	box-shadow: 0 8px 28px rgba(15, 23, 42, 0.12) !important;
	border-radius: 8px !important;
	overflow: hidden !important;
}
.workor-role-pick-dialog.el-dialog.card-dia .cardbox-pan {
	width: 100% !important;
	max-width: 100% !important;
	min-width: 0 !important;
	box-sizing: border-box;
	border-radius: 8px;
	overflow: hidden !important;
	display: flex !important;
	flex-direction: column !important;
}
.workor-role-pick-dialog.el-dialog.card-dia .cd-box {
	width: 100% !important;
	max-width: 100% !important;
	min-width: 0 !important;
	min-height: 0 !important;
	flex: 1 1 auto !important;
	box-sizing: border-box;
	overflow: hidden !important;
	display: flex !important;
	flex-direction: column !important;
}
.workor-pick-dialog-body {
	height: 75vh;
	max-height: calc(100vh - 120px);
	min-height: 420px;
	width: 100%;
	max-width: 100%;
	min-width: 0;
	overflow: hidden;
	box-sizing: border-box;
	display: flex;
	flex-direction: column;
}
.workor-pick-dialog-body .BPO_HH_DutyItemCheck,
.workor-pick-dialog-body .cellpan,
.workor-pick-dialog-body .formcard-shell,
.workor-pick-dialog-body .formcard,
.workor-pick-dialog-body .fc-body,
.workor-pick-dialog-body .wrap-bd,
.workor-pick-dialog-body .flexpan,
.workor-pick-dialog-body .vclistwrap,
.workor-pick-dialog-body .vclist-inner {
	width: 100% !important;
	max-width: 100% !important;
	min-width: 0 !important;
	min-height: 0 !important;
	box-sizing: border-box;
}
.workor-pick-dialog-body .BPO_HH_DutyItemCheck,
.workor-pick-dialog-body .cellpan,
.workor-pick-dialog-body .formcard-shell,
.workor-pick-dialog-body .formcard,
.workor-pick-dialog-body .fc-body,
.workor-pick-dialog-body .wrap-bd,
.workor-pick-dialog-body .flexpan {
	flex: 1 1 0 !important;
	height: 100% !important;
	max-height: 100% !important;
	display: flex !important;
	flex-direction: column !important;
}
.workor-pick-dialog-body .flexpan.flexed > .f-body {
	flex: 1 1 0 !important;
	min-height: 0 !important;
	overflow: hidden !important;
}
.workor-pick-dialog-body .vclist-inner-flex {
	height: 100% !important;
	min-height: 0 !important;
}
.workor-pick-dialog-body .vclist-inner-flex > .el-pagination.vcpager {
	flex: 0 0 auto !important;
}
.workor-pick-dialog-body .formcard-shell--vc {
	padding: 8px 10px 10px !important;
}
</style>
