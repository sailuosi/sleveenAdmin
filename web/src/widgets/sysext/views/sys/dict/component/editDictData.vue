<template>
	<div class="sys-dictData-container">
		<el-dialog v-model="state.isShowDialog" draggable :close-on-click-modal="false" width="700px">
			<template #header>
				<div style="color: #fff">
					<el-icon size="16" style="margin-right: 3px; display: inline; vertical-align: middle"> <ele-Edit /> </el-icon>
					<span> {{ props.title }} </span>
				</div>
			</template>
			<el-form :model="state.ruleForm" ref="ruleFormRef" label-width="auto">
				<el-row :gutter="35">
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="字典值" prop="Value" :rules="[{ required: true, message: '字典值不能为空', trigger: 'blur' }]">
							<el-input v-model="state.ruleForm.Value" placeholder="字典值" clearable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="编码" prop="Code" :rules="[{ required: true, message: '编码不能为空', trigger: 'blur' }]">
							<el-input v-model="state.ruleForm.Code" placeholder="编码" clearable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="标签类型">
							<el-radio-group v-model="state.ruleForm.TagType">
								<el-radio label="default" value="default"><el-tag type="default">主题色</el-tag></el-radio>
								<el-radio type="success" label="success" value="success"><el-tag type="success">success</el-tag></el-radio>
								<el-radio type="info" label="info" value="info"><el-tag type="info">info</el-tag></el-radio>
								<el-radio type="warning" label="warning" value="warning"><el-tag type="warning">warning</el-tag></el-radio>
								<el-radio type="danger" label="danger" value="danger"><el-tag type="danger" style="font:">danger</el-tag></el-radio>
							</el-radio-group>
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="样式(Style)" prop="styleSetting">
							<el-input v-model="state.ruleForm.StyleSetting" placeholder="样式(Style)" clearable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="样式(Class)" prop="classSetting">
							<el-input v-model="state.ruleForm.ClassSetting" placeholder="样式(Class)" clearable />
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
					<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
						<el-form-item label="排序">
							<el-input-number v-model="state.ruleForm.OrderNo" placeholder="排序" class="w100" />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="备注">
							<el-input v-model="state.ruleForm.Remark" placeholder="请输入备注内容" clearable type="textarea" rows="6" />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="拓展数据">
							<el-input v-model="state.ruleForm.ExtData" placeholder="请输入拓展数据" clearable type="textarea" rows="12" />
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

<script lang="ts" setup name="sysEditDictData">
import { reactive, ref } from 'vue';

import { apiSysDictDataAddPost,apiSysDictDataUpdatePost } from '@s/api/system/dictapi.js';

const props = defineProps({
	title: String,
	dictTypeId: Number,
});
const emits = defineEmits(['handleQuery']);
const ruleFormRef = ref();
const state = reactive({
	isShowDialog: false,
	ruleForm: {} as UpdateDictDataInput,
});

// 打开弹窗（兼容后端 DictTypeId/CodeTableId 与前端 dictTypeId/Code）
const openDialog = (row: any) => {
	const r = row || {};
	state.ruleForm = {
		Id: r.Id ?? r.id,
		DictTypeId: r.DictTypeId ?? r.dictTypeId ?? props.dictTypeId,
		Value: r.Value ?? r.value ?? '',
		Code: r.Code ?? r.code ?? '',
		CodeTableId: r.CodeTableId ?? r.codeTableId ?? r.Code ?? r.code ?? '',
		TagType: r.TagType ?? r.tagType ?? 'default',
		StyleSetting: r.StyleSetting ?? r.styleSetting ?? '',
		ClassSetting: r.ClassSetting ?? r.classSetting ?? '',
		Status: r.Status ?? r.status ?? 1,
		OrderNo: r.OrderNo ?? r.orderNo ?? 100,
		Remark: r.Remark ?? r.remark ?? '',
		ExtData: r.ExtData ?? r.extData ?? '',
	};
	state.isShowDialog = true;
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

// 提交（后端必填 CodeTableId，与编码 Code 保持一致）
const submit = () => {
	ruleFormRef.value.validate(async (valid: boolean) => {
		if (!valid) return;
		const payload = { ...state.ruleForm };
		if (payload.CodeTableId == null || payload.CodeTableId === '') {
			payload.CodeTableId = payload.Code ?? '';
		}
		if (payload.DictTypeId == null && props.dictTypeId != null) {
			payload.DictTypeId = props.dictTypeId;
		}
		if (payload.Id != undefined && payload.Id > 0) {
			await apiSysDictDataUpdatePost(payload);
		} else {
			await apiSysDictDataAddPost(payload);
		}
		closeDialog();
	});
};

// 导出对象
defineExpose({ openDialog });
</script>
<style scoped>
.mb20{
	margin-bottom: 0;
}
</style>