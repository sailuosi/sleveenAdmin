<template>
	<div class="sys-dictType-container">
		<el-dialog v-model="state.isShowDialog" draggable :close-on-click-modal="false" width="700px">
			<template #header>
				<div style="color: #fff">
					<el-icon size="16" style="margin-right: 3px; display: inline; vertical-align: middle"> <ele-Edit /> </el-icon>
					<span> {{ props.title }} </span>
				</div>
			</template>
			<el-form :model="state.ruleForm" ref="ruleFormRef" label-width="auto">
				<el-row :gutter="35">
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" >
						<el-form-item label="字典名称" prop="name" :rules="[{ required: true, message: '字典名称不能为空', trigger: 'blur' }]">
							<el-input v-model="state.ruleForm.name" placeholder="字典名称" clearable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" >
						<el-form-item label="字典编码" prop="code" :rules="[{ required: true, message: '字典编码不能为空', trigger: 'blur' }]">
							<el-input v-model="state.ruleForm.code" placeholder="字典编码" clearable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" >
						<el-form-item label="状态">
							<el-radio-group v-model="state.ruleForm.status">
								<el-radio :label="1" :value="1">启用</el-radio>
								<el-radio :label="2" :value="2">禁用</el-radio>
							</el-radio-group>
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" >
						<el-form-item label="排序">
							<el-input-number v-model="state.ruleForm.orderNo" placeholder="排序" class="w100" />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" >
						<el-form-item label="备注">
							<el-input v-model="state.ruleForm.remark" placeholder="请输入备注内容" clearable type="textarea" />
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

<script lang="ts" setup name="sysEditDictType">
import { reactive, ref } from 'vue';
import { apiSysDictTypeAddPost, apiSysDictTypeUpdatePost } from '@s/api/system/dictapi.js';

const props = defineProps({
	title: String,
});
const emits = defineEmits(['handleQuery']);
const ruleFormRef = ref();
const state = reactive({
	isShowDialog: false,
	ruleForm: {} as { id?: number; name?: string; code?: string; status?: number; orderNo?: number; remark?: string },
});

// 打开弹窗（兼容列表行 Name/Code 与表单 name/code）
const openDialog = (row: any) => {
	const r = row || {};
	state.ruleForm = {
		id: r.Id ?? r.id,
		name: r.Name ?? r.name,
		code: r.Code ?? r.code,
		status: r.Status ?? r.status ?? 1,
		orderNo: r.OrderNo ?? r.orderNo ?? 100,
		remark: r.Remark ?? r.remark ?? '',
	};
	state.isShowDialog = true;
	// 仅清除校验状态，不 resetFields()，否则会清空 id 导致编辑时后端收到 Id=0 误判为重复
	ruleFormRef.value?.clearValidate();
};

// 关闭弹窗
const closeDialog = () => {
	emits('handleQuery');
	state.isShowDialog = false;
};

// 取消
const cancel = () => {
	state.isShowDialog = false;
};

// 提交
const submit = () => {
	ruleFormRef.value.validate(async (valid: boolean) => {
		if (!valid) return;
		if (state.ruleForm.id != undefined && state.ruleForm.id > 0) {
			await apiSysDictTypeUpdatePost(state.ruleForm);
		} else {
			await apiSysDictTypeAddPost(state.ruleForm);
		}
		closeDialog();
	});
};

// 导出对象
defineExpose({ openDialog });
</script>
