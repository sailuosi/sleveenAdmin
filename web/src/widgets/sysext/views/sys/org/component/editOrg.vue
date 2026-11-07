<template>
	<div class="sys-org-container">
		<el-dialog v-model="state.isShowDialog" draggable :close-on-click-modal="false" width="700px">
			<template #header>
				<div style="color: #fff">
					<el-icon size="16" style="margin-right: 3px; display: inline; vertical-align: middle"> <ele-Edit /> </el-icon>
					<span> {{ props.title }} </span>
				</div>
			</template>
			<el-form :model="state.ruleForm" ref="ruleFormRef" label-width="auto">
				<el-row :gutter="35">
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="上级机构">
							<el-cascader
								:options="props.orgData"
								:props="{ checkStrictly: true, emitPath: false, value: 'id', label: 'name' }"
								placeholder="请选择上级机构"
								clearable
								class="w100"
								v-model="state.ruleForm.pid"
							>
								<template #default="{ node, data }">
									<span>{{ data.name }}</span>
									<span v-if="!node.isLeaf"> ({{ data.children.length }}) </span>
								</template>
							</el-cascader>
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="机构名称" prop="Name" :rules="[{ required: true, message: '机构名称不能为空', trigger: 'blur' }]">
							<el-input v-model="state.ruleForm.Name" placeholder="机构名称" clearable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="机构编码" prop="Code" :rules="[{ required: true, message: '机构编码不能为空', trigger: 'blur' }]">
							<el-input v-model="state.ruleForm.Code" placeholder="机构编码" clearable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="级别">
							<el-input-number v-model="state.ruleForm.Level" placeholder="级别" class="w100" />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="机构类型">
							<el-select v-model="state.ruleForm.Type" filterable clearable class="w100">
								<el-option v-for="item in state.orgTypeList" :key="item.value" :label="item.value" :value="item.Code" />
							</el-select>
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="排序">
							<el-input-number v-model="state.ruleForm.OrderNo" placeholder="排序" class="w100" />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="状态">
							<el-radio-group v-model="state.ruleForm.Status">
								<el-radio :label="1" :value="1">启用</el-radio>
								<el-radio :label="2" :value="2">禁用</el-radio>
							</el-radio-group>
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="备注">
							<el-input v-model="state.ruleForm.Remark" placeholder="请输入备注内容" clearable type="textarea" />
						</el-form-item>
					</el-col>
				</el-row>
			</el-form>
			<template #footer>
				<span class="dialog-footer">
					<el-button @click="cancel">取 消</el-button>
					<el-button type="primary" @click="submit">确 定</el-button>
				</span>
			</template>
		</el-dialog>
	</div>
</template>

<script  setup name="sysEditOrg">
import { onMounted, reactive, ref } from 'vue';

// import { getAPI } from '/@/utils/axios-utils';
// import { SysOrgApi, SysDictDataApi } from '/@/api-services/api';
// import { SysOrg, UpdateOrgInput } from '/@/api-services/models';
import {apiSysDictDataDataListCodeGet} from "@s/api/system/sysdictdataapi.js";
import { apiSysOrgUpdatePost,apiSysOrgAddPost } from "@s/api/system/sysorgapi.js";

const props = defineProps({
	title: String,
	orgData: null,
});
const emits = defineEmits(['handleQuery']);
const ruleFormRef = ref();
const state = reactive({
	isShowDialog: false,
	ruleForm: {} ,
	orgTypeList: [] ,
});

onMounted(async () => {
	let resDicData = await apiSysDictDataDataListCodeGet('org_type');
	state.orgTypeList = resDicData;
});

// 打开弹窗
const openDialog = (row) => {
	state.ruleForm = JSON.parse(JSON.stringify(row));
	state.isShowDialog = true;
	ruleFormRef.value?.resetFields();
};

// 关闭弹窗
const closeDialog = () => {
	emits('handleQuery', true);
	state.isShowDialog = false;
};

// 取消
const cancel = () => {
	state.isShowDialog = false;
};

// 提交
const submit = () => {
	ruleFormRef.value.validate(async (valid) => {
		if (!valid) return;
		if (state.ruleForm.id != undefined && state.ruleForm.id > 0) {
			await apiSysOrgUpdatePost(state.ruleForm);
		} else {
			await apiSysOrgAddPost(state.ruleForm);
		}
		closeDialog();
	});
};

// 导出对象
defineExpose({ openDialog });
</script>
<style lang="css" scoped>
.mb20{
	margin-bottom: 0;
}
</style>