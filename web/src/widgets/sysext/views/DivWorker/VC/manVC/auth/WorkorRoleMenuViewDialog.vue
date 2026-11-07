<template>
	<mdialog
		:title="dialogTitle"
		extraClass="workor-role-menu-view-dialog"
		v-model="opened"
		width="520px"
		maxWidth="560px"
		top="5vh"
		destroy-on-close
		@close="onClose"
	>
		<div v-loading="loading" class="workor-pick-dialog-body workor-role-menu-view-body">
			<template v-if="canLoad">
				<div class="menu-view-toolbar">
					<span class="menu-view-hint">共 {{ bindCount }} 项已授权菜单/按钮</span>
					<div class="menu-view-actions">
						<el-button type="primary" plain :disabled="!treeData.length" @click="toggleExpandAll">
							<SvgIcon :name="allExpanded ? 'fa fa-angle-double-up' : 'fa fa-angle-double-down'" />
							{{ allExpanded ? "折叠全部" : "展开全部" }}
						</el-button>
						<el-button type="success" @click="load">
							<SvgIcon name="fa fa-refresh" /> 刷新
						</el-button>
					</div>
				</div>
				<div class="menu-view-tree">
					<template v-if="isWorkorTypeMode || isHumanMode || goodsBagId">
						<el-tree
							v-if="treeData.length"
							ref="treeRef"
							:data="treeData"
							node-key="Id"
							:props="{ children: 'Children', label: 'Title' }"
							highlight-current
							:default-expanded-keys="rootExpandedKeys"
						/>
						<el-empty v-else :description="emptyTreeHint" />
					</template>
					<el-empty v-else description="请先在角色管理模块完成资源集合绑定后重试" />
				</div>
			</template>
			<el-empty v-else :description="emptyGateHint" />
		</div>
	</mdialog>
</template>

<script setup>
import { computed, ref, watch } from "vue";
import mdialog from "../../../ui/modal/mdialog.vue";
import { getHHDutyItemById } from "@s/views/AuthBag/api/dutyitemapi.js";
import { loadBindKeys } from "@s/views/AuthBag/api/goodsitemapi.js";
// import { apiSysMenuListGet } from "@s/views/menu/api/sysmenu.js"; // 原：走 sysMenu:list，非超管会按本人菜单过滤
import { getWorkorTypeMenuBindKeys, getHumanMenuBindKeys, getSysMenuFullTree } from "@/api/common.js";

const props = defineProps({
	modelValue: { type: Boolean, default: false },
	/** duty：单角色；workorType：管理员类别；human：人员 WorkorScope 并集 */
	mode: { type: String, default: "duty" },
	/** duty / workorType / human 对应行字段见下方 computed */
	row: { type: Object, default: null },
});

const emit = defineEmits(["update:modelValue", "close"]);

const opened = computed({
	get: () => props.modelValue,
	set: (v) => emit("update:modelValue", v),
});

const loading = ref(false);
const dutyDetail = ref(null);
const treeData = ref([]);
const bindCount = ref(0);
const treeRef = ref(null);
const allExpanded = ref(false);

const isWorkorTypeMode = computed(() => props.mode === "workorType");
const isHumanMode = computed(() => props.mode === "human");

const dutyId = computed(() => props.row?.HH_DutyItem_FK || props.row?.HH_DutyItemOID || null);

const workorTypeId = computed(
	() => props.row?.HH_WorkorTypeOID || props.row?.HH_WorkorType_FK || null,
);

const humanOid = computed(() => props.row?.HR_HumanOID || null);

const goodsBagId = computed(() => dutyDetail.value?.HH_GoodsBag_FK || null);

const canLoad = computed(() => {
	if (isHumanMode.value) return !!humanOid.value;
	if (isWorkorTypeMode.value) return !!workorTypeId.value;
	return !!dutyId.value;
});

const dialogTitle = computed(() => {
	if (isHumanMode.value) {
		const name = props.row?.H_Name;
		return name ? `人员菜单权限 - ${name}` : "人员菜单权限";
	}
	if (isWorkorTypeMode.value) {
		const name = props.row?.WT_Title;
		return name ? `类别菜单权限 - ${name}` : "类别菜单权限";
	}
	const name = props.row?.Di_Name || dutyDetail.value?.Di_Name;
	return name ? `菜单权限 - ${name}` : "菜单权限";
});

const emptyGateHint = computed(() => {
	if (isHumanMode.value) return "未选中人员，无法查看";
	if (isWorkorTypeMode.value) return "未关联管理员类别，无法查看";
	return "未关联职责角色，无法查看";
});

const emptyTreeHint = computed(() => {
	if (!bindCount.value) {
		if (isHumanMode.value) return "该人员尚未配置管理员类别菜单权限";
		if (isWorkorTypeMode.value) return "该类别下角色尚未配置菜单权限";
		return "该角色尚未配置菜单权限";
	}
	return "已授权项未能匹配到菜单树，请检查菜单数据";
});

/** 仅展开根节点 */
const rootExpandedKeys = computed(() =>
	(treeData.value || []).map((n) => nodeId(n)).filter((id) => id != null),
);

function nodeId(node) {
	return node?.Id ?? node?.id;
}

function nodeChildren(node) {
	return node?.Children ?? node?.children ?? [];
}

function nodeTitle(node) {
	return node?.Title ?? node?.title ?? node?.meta?.title ?? "";
}

/** Gi_OID 多为字符串，菜单 Id 多为 number，统一成字符串再比 */
function toIdKey(id) {
	return id == null || id === "" ? null : String(id);
}

/** 保留已授权节点及其祖先，形成只读查看树 */
function filterTreeByIds(nodes, idSet) {
	const result = [];
	for (const node of nodes || []) {
		const kids = filterTreeByIds(nodeChildren(node), idSet);
		const key = toIdKey(nodeId(node));
		if ((key != null && idSet.has(key)) || kids.length) {
			result.push({
				...node,
				Id: nodeId(node),
				Title: nodeTitle(node),
				Children: kids,
			});
		}
	}
	return result;
}

function unwrapList(res) {
	if (Array.isArray(res)) return res;
	if (Array.isArray(res?.data)) return res.data;
	if (Array.isArray(res?.result)) return res.result;
	if (Array.isArray(res?.data?.result)) return res.data.result;
	return [];
}

function setAllExpanded(expanded) {
	const store = treeRef.value?.store;
	if (!store) return;
	const nodes = typeof store._getAllNodes === "function"
		? store._getAllNodes()
		: Object.values(store.nodesMap || {});
	nodes.forEach((node) => {
		node.expanded = expanded;
	});
	allExpanded.value = expanded;
}

function toggleExpandAll() {
	setAllExpanded(!allExpanded.value);
}

function applyBinds(menus, binds) {
	const oids = binds
		.map((li) => toIdKey(li.Gi_OID ?? li.gi_OID ?? li.gi_Oid))
		.filter((id) => id != null);
	bindCount.value = oids.length;
	treeData.value = filterTreeByIds(menus, new Set(oids));
}

async function loadDutyMode() {
	if (!dutyId.value) return;
	const dutyRes = await getHHDutyItemById(dutyId.value);
	dutyDetail.value = dutyRes?.data ?? dutyRes ?? {};
	const bagId = dutyDetail.value?.HH_GoodsBag_FK;
	if (!bagId) return;

	const [menuRes, bindRes] = await Promise.all([
		// apiSysMenuListGet(),
		getSysMenuFullTree(),
		loadBindKeys({ HH_GoodsBag_FK: bagId, Gi_Type: "1" }),
	]);
	applyBinds(unwrapList(menuRes), unwrapList(bindRes));
}

async function loadWorkorTypeMode() {
	if (!workorTypeId.value) return;
	const [menuRes, bindRes] = await Promise.all([
		// apiSysMenuListGet(),
		getSysMenuFullTree(),
		getWorkorTypeMenuBindKeys(workorTypeId.value),
	]);
	applyBinds(unwrapList(menuRes), unwrapList(bindRes));
}

async function loadHumanMode() {
	if (!humanOid.value) return;
	const [menuRes, bindRes] = await Promise.all([
		// apiSysMenuListGet(),
		getSysMenuFullTree(),
		getHumanMenuBindKeys(humanOid.value),
	]);
	applyBinds(unwrapList(menuRes), unwrapList(bindRes));
}

async function load() {
	treeData.value = [];
	bindCount.value = 0;
	allExpanded.value = false;
	dutyDetail.value = null;
	if (!canLoad.value) return;

	loading.value = true;
	try {
		if (isHumanMode.value) {
			await loadHumanMode();
		} else if (isWorkorTypeMode.value) {
			await loadWorkorTypeMode();
		} else {
			await loadDutyMode();
		}
	} catch (err) {
		swal({ text: err?.message || "加载菜单权限失败", icon: "error", button: "关闭" });
	} finally {
		loading.value = false;
	}
}

function onClose() {
	dutyDetail.value = null;
	treeData.value = [];
	bindCount.value = 0;
	allExpanded.value = false;
	emit("close");
}

watch(
	() => props.modelValue,
	(v) => {
		if (v) load();
	},
);
</script>

<style lang="css">
.el-overlay-dialog:has(.workor-role-menu-view-dialog) {
	display: flex !important;
	justify-content: center !important;
	align-items: flex-start !important;
}
.workor-role-menu-view-dialog.el-dialog.card-dia {
	width: min(560px, 92vw) !important;
	max-width: 92vw !important;
	margin: 5vh auto !important;
	left: auto !important;
	right: auto !important;
	transform: none !important;
	outline: none !important;
	box-shadow: 0 8px 28px rgba(15, 23, 42, 0.12) !important;
	border-radius: 8px !important;
	overflow: hidden !important;
}
.workor-role-menu-view-dialog.el-dialog.card-dia .cardbox-pan {
	width: 100% !important;
	max-width: 100% !important;
	min-width: 0 !important;
	box-sizing: border-box;
	border-radius: 8px;
	overflow: hidden !important;
	display: flex !important;
	flex-direction: column !important;
}
.workor-role-menu-view-dialog.el-dialog.card-dia .cd-box {
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
.workor-role-menu-view-body {
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
.workor-role-menu-view-body .menu-view-toolbar {
	flex: 0 0 auto;
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 8px;
	padding: 0 2px 8px;
}
.workor-role-menu-view-body .menu-view-actions {
	display: flex;
	align-items: center;
	gap: 8px;
	flex-shrink: 0;
}
.workor-role-menu-view-body .menu-view-hint {
	color: var(--el-text-color-secondary);
	font-size: 13px;
}
.workor-role-menu-view-body .menu-view-tree {
	flex: 1 1 0;
	min-height: 0;
	overflow: auto;
	padding: 4px 2px;
}
</style>
