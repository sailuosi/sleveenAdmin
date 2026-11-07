<template>
	<div v-loading="loading" class="workor-role-duty-base">
		<el-form class="gridfm" label-width="110px">
			<div class="fm-toolbar">
				<el-button type="primary" :loading="saving" @click="save"> <SvgIcon name="fa fa-save" /> 保存</el-button>
			</div>

			<fminput v-model="focusRow.Di_Name" label="名称" />
			<fminput v-model="focusRow.Di_Idx" label="序号" />
			<fminput v-model="focusRow.Di_Code" label="编号" />
			<fminput v-model="focusRow.Di_Note" label="说明" class="spanrow" />
			<fminput v-model="focusRow.Di_Remark" label="备注" class="spanrow" />

			<!-- 只读展示，暂不提供选取修改 -->
			<el-form-item label="资源集合" class="spanrow bag-row">
				<goodstag v-model="focusRow.HH_GoodsBag_FK" disabled placeholder="未绑定" />
			</el-form-item>
			<el-form-item label="词条集合" class="spanrow bag-row">
				<wordtag v-model="focusRow.HH_WordBag_FK" disabled placeholder="未绑定" />
			</el-form-item>
			<el-form-item label="访客集合" class="spanrow bag-row">
				<visitortag v-model="focusRow.HH_VisitorBag_FK" disabled placeholder="未绑定" />
			</el-form-item>

			<template v-if="focusRow.HH_DutyItemOID">
				<fminput v-model="focusRow.SYS_Created" label="创建日期" noedit />
				<fminput v-model="focusRow.SYS_LAST_UPD" label="最后修改日期" noedit />
			</template>
		</el-form>
	</div>
</template>

<script setup>
import { ref, watch } from "vue";
import { ElMessage } from "element-plus";
import fminput from "@/ui/common/input/fminput.vue";
import goodstag from "@s/views/AuthBag/components/duty/goodstag.vue";
import wordtag from "@s/views/AuthBag/components/duty/wordtag.vue";
import visitortag from "@s/views/AuthBag/components/duty/visitortag.vue";
import { getHHDutyItemById, updateHHDutyItem } from "@s/views/AuthBag/api/dutyitemapi.js";

const props = defineProps({
	/** HH_DutyItemOID */
	dutyId: { type: [String, Object], default: null },
});

const emit = defineEmits(["saved", "loaded"]);

const loading = ref(false);
const saving = ref(false);
const focusRow = ref({});

function load() {
	if (!props.dutyId) {
		focusRow.value = {};
		return;
	}
	loading.value = true;
	getHHDutyItemById(props.dutyId)
		.then((res) => {
			focusRow.value = res?.data ?? res ?? {};
			emit("loaded", focusRow.value);
		})
		.finally(() => {
			loading.value = false;
		});
}

function save() {
	if (!focusRow.value?.HH_DutyItemOID) {
		ElMessage.warning("角色基础信息未加载");
		return;
	}
	saving.value = true;
	updateHHDutyItem([focusRow.value])
		.then((res) => {
			ElMessage.success(res?.data ?? res ?? "保存成功");
			emit("saved", focusRow.value);
		})
		.finally(() => {
			saving.value = false;
		});
}

watch(
	() => props.dutyId,
	() => load(),
	{ immediate: true },
);

defineExpose({ load, save, focusRow });
</script>

<style scoped>
.gridfm {
	display: grid;
	grid-template-columns: 1fr 1fr;
	gap: 4px 12px;
}
.fm-toolbar {
	grid-column: 1 / -1;
	margin-bottom: 12px;
}
.spanrow {
	grid-column: 1 / -1;
}
.bag-row :deep(.el-form-item__content) {
	line-height: 32px;
}
</style>
