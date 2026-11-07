<template>
	<mdialog
		:title="dialogTitle"
		extraClass="workor-role-config-dialog"
		v-model="opened"
		width="90%"
		maxWidth="1200px"
		top="5vh"
		destroy-on-close
		@close="onClose"
	>
		<div class="workor-pick-dialog-body workor-role-config-body">
			<template v-if="dutyId">
				<el-tabs v-model="activeTab" type="border-card" class="workor-role-config-tabs">
					<el-tab-pane label="基础信息" name="base">
						<div class="tab-pane-body">
							<WorkorRoleDutyBaseForm
								:duty-id="dutyId"
								@loaded="onDutyLoaded"
								@saved="onDutySaved"
							/>
						</div>
					</el-tab-pane>
					<el-tab-pane label="菜单配置" name="menu" lazy>
						<div class="tab-pane-body tab-pane-fill">
							<template v-if="goodsBagRow">
								<menubind :row="goodsBagRow" :default-expand-all="false" />
							</template>
							<el-empty v-else description="请先在角色管理模块完成绑定后重试" />
						</div>
					</el-tab-pane>
					<el-tab-pane label="词条列表" name="word" lazy>
						<div class="tab-pane-body tab-pane-fill">
							<template v-if="wordBagRow">
								<HHWordItemlist class="word-item-pane" :bag="wordBagRow" />
							</template>
							<el-empty v-else description="请先在角色管理模块完成绑定后重试" />
						</div>
					</el-tab-pane>
					<el-tab-pane label="访客集合" name="visitor" lazy>
						<div class="tab-pane-body">
							<template v-if="visitorBagRow">
								<HHVisitorBagform :row="visitorBagRow" :adding="false" />
							</template>
							<el-empty v-else description="请先在角色管理模块完成绑定后重试" />
						</div>
					</el-tab-pane>
				</el-tabs>
			</template>
			<el-empty v-else description="未关联职责角色，无法配置" />
		</div>
	</mdialog>
</template>

<script setup>
import { computed, ref, watch } from "vue";
import mdialog from "../../../ui/modal/mdialog.vue";
import menubind from "@s/views/AuthBag/components/menu/menubind.vue";
import HHWordItemlist from "@s/views/AuthBag/components/basis/worditem/HHWordItemlist.vue";
import HHVisitorBagform from "@s/views/AuthBag/components/basis/visitorbag/HHVisitorBagform.vue";
import WorkorRoleDutyBaseForm from "./WorkorRoleDutyBaseForm.vue";

const props = defineProps({
	modelValue: { type: Boolean, default: false },
	/** 管理员角色行（含 HH_DutyItem_FK / Di_Name） */
	row: { type: Object, default: null },
});

const emit = defineEmits(["update:modelValue", "close", "saved"]);

const opened = computed({
	get: () => props.modelValue,
	set: (v) => emit("update:modelValue", v),
});

const activeTab = ref("base");
const dutyDetail = ref(null);

const dutyId = computed(() => props.row?.HH_DutyItem_FK || props.row?.HH_DutyItemOID || null);

const dialogTitle = computed(() => {
	const name = props.row?.Di_Name || dutyDetail.value?.Di_Name;
	return name ? `角色配置 - ${name}` : "角色配置";
});

const goodsBagRow = computed(() => {
	const oid = dutyDetail.value?.HH_GoodsBag_FK;
	if (!oid) return null;
	return { HH_GoodsBagOID: oid };
});

const wordBagRow = computed(() => {
	const oid = dutyDetail.value?.HH_WordBag_FK;
	if (!oid) return null;
	return { HH_WordBagOID: oid };
});

const visitorBagRow = computed(() => {
	const oid = dutyDetail.value?.HH_VisitorBag_FK;
	if (!oid) return null;
	return { HH_VisitorBagOID: oid };
});

function onDutyLoaded(detail) {
	dutyDetail.value = detail || null;
}

function onDutySaved(detail) {
	dutyDetail.value = detail || dutyDetail.value;
	emit("saved", detail);
}

function onClose() {
	activeTab.value = "base";
	dutyDetail.value = null;
	emit("close");
}

watch(
	() => props.modelValue,
	(v) => {
		if (v) {
			activeTab.value = "base";
			dutyDetail.value = null;
		}
	},
);
</script>

<style lang="css">
.el-overlay-dialog:has(.workor-role-config-dialog) {
	display: flex !important;
	justify-content: center !important;
	align-items: flex-start !important;
}
.workor-role-config-dialog.el-dialog.card-dia {
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
.workor-role-config-dialog.el-dialog.card-dia .cardbox-pan {
	width: 100% !important;
	max-width: 100% !important;
	min-width: 0 !important;
	box-sizing: border-box;
	border-radius: 8px;
	overflow: hidden !important;
	display: flex !important;
	flex-direction: column !important;
}
.workor-role-config-dialog.el-dialog.card-dia .cd-box {
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
.workor-role-config-body {
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
.workor-role-config-tabs {
	flex: 1 1 0;
	min-height: 0;
	display: flex;
	flex-direction: column;
	overflow: hidden;
}
.workor-role-config-tabs > .el-tabs__content {
	flex: 1 1 0;
	min-height: 0;
	overflow: hidden;
	padding: 10px 12px;
}
.workor-role-config-tabs .el-tab-pane {
	height: 100%;
}
.workor-role-config-body .tab-pane-body {
	height: 100%;
	min-height: 0;
	overflow: auto;
	box-sizing: border-box;
}
.workor-role-config-body .tab-pane-scroll {
	overflow: auto;
}
.workor-role-config-body .tab-pane-fill {
	display: flex;
	flex-direction: column;
	overflow: hidden;
}
.workor-role-config-body .word-item-pane {
	flex: 1 1 0;
	min-height: 0;
	height: 100%;
}
</style>
