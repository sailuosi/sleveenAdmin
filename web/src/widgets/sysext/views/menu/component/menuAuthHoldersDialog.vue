<template>
	<mdialog
		:title="dialogTitle"
		extraClass="menu-auth-holders-dialog"
		v-model="opened"
		width="90%"
		maxWidth="1200px"
		top="5vh"
		destroy-on-close
		@close="onClose"
	>
		<div v-loading="loading" class="workor-pick-dialog-body menu-auth-holders-body">
			<div class="auth-toolbar">
				<div class="auth-menu-sub sub-title sub-title--md">
					<div class="sub-title__text" :title="displayMenuPath">
						当前菜单：<span class="auth-menu-name">{{ displayMenuPath }}</span>
					</div>
				</div>
				<el-button type="success" @click="load">
					<SvgIcon name="fa fa-refresh" /> 刷新
				</el-button>
			</div>
			<el-tabs v-model="activeTab" type="border-card" class="menu-auth-holders-tabs">
				<el-tab-pane :label="`管理员类别（${workorTypes.length}）`" name="workorType">
					<div class="tab-pane-body tab-pane-fill">
						<div class="auth-list-wrap">
							<vclist
								:list="workorTypes"
								:total="workorTypes.length"
								height="100%"
								flex
								:stripe="true"
								:orderCol="true"
								empty-text="暂无拥有该菜单的管理员类别"
							>
								<vccol field="WT_Title" caption="标题" min-width="150px" align="left" />
								<vccol field="WT_Note" caption="简介" min-width="220px" align="left" />
								<vccol field="WT_IsOn" caption="启用" width="80px" type="bool" align="center" />
								<vccol field="WT_Order" caption="排序" width="80px" type="number" align="right" header-align="center" />
							</vclist>
						</div>
					</div>
				</el-tab-pane>
				<el-tab-pane :label="`角色（${roles.length}）`" name="role" lazy>
					<div class="tab-pane-body tab-pane-fill">
						<div class="auth-list-wrap">
							<vclist
								:list="roles"
								:total="roles.length"
								height="100%"
								flex
								:stripe="true"
								:orderCol="true"
								empty-text="暂无拥有该菜单的角色"
							>
								<vccol field="Di_Name" caption="名称" min-width="210px" align="left" />
								<vccol field="Di_Note" caption="备注" min-width="200px" align="left" />
								<vccol field="Di_Code" caption="编码" width="150px" align="left" header-align="center" />
								<vccol field="Di_Idx" caption="序号" width="80px" type="number" align="right" header-align="center" />
							</vclist>
						</div>
					</div>
				</el-tab-pane>
			</el-tabs>
		</div>
	</mdialog>
</template>

<script setup>
import { computed, ref, watch } from 'vue';
import mdialog from '@/ui/common/modal/mdialog.vue';
import vclist from '@/ui/common/vclist/vclist.vue';
import vccol from '@/ui/common/vclist/vccol.vue';
import { getMenuAuthHolders } from '@/api/common.js';

const props = defineProps({
	modelValue: { type: Boolean, default: false },
	menuId: { type: [String, Number], default: null },
	menuTitle: { type: String, default: '' },
	/** 根→当前节点完整路径，如「系统管理 / 菜单管理」 */
	menuPath: { type: String, default: '' },
});

const emit = defineEmits(['update:modelValue', 'close']);

const opened = computed({
	get: () => props.modelValue,
	set: (v) => emit('update:modelValue', v),
});

const displayMenuPath = computed(() => props.menuPath || props.menuTitle || '—');

const dialogTitle = computed(() =>
	props.menuTitle ? `查看权限 - ${props.menuTitle}` : '查看权限',
);

const loading = ref(false);
const activeTab = ref('workorType');
const workorTypes = ref([]);
const roles = ref([]);

function unwrapRows(raw) {
	if (Array.isArray(raw)) return raw;
	if (Array.isArray(raw?.Items)) return raw.Items;
	if (Array.isArray(raw?.rows)) return raw.rows;
	return [];
}

async function load() {
	workorTypes.value = [];
	roles.value = [];
	if (props.menuId == null || props.menuId === '') return;

	loading.value = true;
	try {
		const res = (await getMenuAuthHolders(props.menuId)) ?? {};
		const data = res.data ?? res.result ?? res;
		workorTypes.value = unwrapRows(data.workorTypes ?? data.WorkorTypes);
		roles.value = unwrapRows(data.roles ?? data.Roles);
	} catch (err) {
		swal({ text: err?.message || '加载菜单权限持有者失败', icon: 'error', button: '关闭' });
	} finally {
		loading.value = false;
	}
}

function onClose() {
	workorTypes.value = [];
	roles.value = [];
	activeTab.value = 'workorType';
	emit('close');
}

watch(
	() => props.modelValue,
	(v) => {
		if (v) {
			activeTab.value = 'workorType';
			load();
		}
	},
);
</script>

<style lang="css">
.el-overlay-dialog:has(.menu-auth-holders-dialog) {
	display: flex !important;
	justify-content: center !important;
	align-items: flex-start !important;
}
.menu-auth-holders-dialog.el-dialog.card-dia {
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
.menu-auth-holders-dialog.el-dialog.card-dia .cardbox-pan {
	width: 100% !important;
	max-width: 100% !important;
	min-width: 0 !important;
	box-sizing: border-box;
	border-radius: 8px;
	overflow: hidden !important;
	display: flex !important;
	flex-direction: column !important;
}
.menu-auth-holders-dialog.el-dialog.card-dia .cd-box {
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
.menu-auth-holders-body {
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
.menu-auth-holders-body .auth-toolbar {
	flex: 0 0 auto;
	padding: 0 0 10px;
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 12px;
}
.menu-auth-holders-body .auth-menu-sub {
	min-width: 0;
	flex: 1 1 auto;
}
.menu-auth-holders-body .auth-menu-sub .sub-title__text {
	border-left: 8px solid rgb(59, 130, 246);
	font-weight: 600;
	font-size: 18px;
	padding-left: 8px;
	line-height: 1.4;
	min-width: 0;
	overflow: hidden;
	text-overflow: ellipsis;
	white-space: nowrap;
}
.menu-auth-holders-body .auth-menu-name {
	color: var(--el-color-success, #67c23a);
	font-weight: 700;
}
.menu-auth-holders-tabs {
	flex: 1 1 0;
	min-height: 0;
	display: flex;
	flex-direction: column;
	overflow: hidden;
}
.menu-auth-holders-tabs > .el-tabs__content {
	flex: 1 1 0;
	min-height: 0;
	overflow: hidden;
	padding: 10px 12px;
}
.menu-auth-holders-tabs .el-tab-pane {
	height: 100%;
}
.menu-auth-holders-body .tab-pane-body {
	height: 100%;
	min-height: 0;
	overflow: auto;
	box-sizing: border-box;
}
.menu-auth-holders-body .tab-pane-fill {
	display: flex;
	flex-direction: column;
	overflow: hidden;
}
.menu-auth-holders-body .auth-list-wrap {
	flex: 1 1 0;
	min-height: 0;
	overflow: hidden;
}
.menu-auth-holders-body .vclistwrap,
.menu-auth-holders-body .vclist-inner,
.menu-auth-holders-body .vclist-inner-flex {
	height: 100% !important;
	min-height: 0 !important;
}
/* 表头白底黑字：盖过 global.css 的 #58a3f7 */
.menu-auth-holders-body .vclist.sum-entry-table .el-table__header-wrapper th.el-table__cell,
.menu-auth-holders-body .vclist.sum-entry-table .el-table__fixed-header-wrapper th.el-table__cell {
	background-color: #fff !important;
	color: #303133 !important;
	border-color: var(--el-border-color-lighter, #ebeef5) !important;
}
.menu-auth-holders-body .vclist.sum-entry-table .el-table__header-wrapper th.el-table__cell {
	border-bottom: 1px solid var(--el-border-color-lighter, #ebeef5) !important;
}
.menu-auth-holders-body .vclist.sum-entry-table .el-table__header-wrapper .vcthead.nqy,
.menu-auth-holders-body .vclist.sum-entry-table .el-table__fixed-header-wrapper .vcthead.nqy {
	background-color: #fff !important;
	color: #303133 !important;
}
.menu-auth-holders-body div.vchead.noquery {
	background-color: #fff !important;
	color: #303133 !important;
	border-bottom: 1px solid var(--el-border-color-lighter, #ebeef5) !important;
}
.menu-auth-holders-body div.vchead--hasquery .vccap {
	background-color: #fff !important;
	color: #303133 !important;
}
.menu-auth-holders-body .vclist.sum-entry-table .el-table__header-wrapper .cell,
.menu-auth-holders-body .vclist.sum-entry-table .el-table__header-wrapper .vccap {
	color: #303133 !important;
}
</style>
