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
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="组织OID" prop="KB_OrgOID" :rules="[{ required: true, message: '组织ID不能为空', trigger: 'blur' }]">
							<el-input v-model="state.ruleForm.KB_OrgOID" placeholder="组织OID" clearable />
						</el-form-item>
					</el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="组织名称" prop="KB_OrgName" :rules="[{ required: true, message: '组织名称不能为空', trigger: 'blur' }]">
							<el-input v-model="state.ruleForm.KB_OrgName" placeholder="组织名称" clearable />
						</el-form-item>
					</el-col>

          <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
            <el-form-item label="指定创建人" prop="KB_CreatorMan" :rules="[{ required: true, message: '创建人不能为空', trigger: 'blur' }]">
              <el-input v-model="state.ruleForm.KB_CreatorMan" placeholder="指定创建人" clearable />
            </el-form-item>
          </el-col>

          <el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
            <el-form-item label="用户编码" prop="KB_CreatorCode" :rules="[{ required: true, message: '创建人不能为空', trigger: 'blur' }]">
              <el-input v-model="state.ruleForm.KB_CreatorCode" placeholder="用户编码" clearable />
            </el-form-item>
          </el-col>
					<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
						<el-form-item label="备注">
							<el-input v-model="state.ruleForm.KB_Remark" placeholder="请输入备注内容" clearable type="textarea" />
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

<script lang="ts" setup name="editDialog">
import { reactive, ref } from 'vue';
import  {add,update}  from '@s/api/system/deptCreatorConfApi.js'
const props = defineProps({
	title: String,
  Conf_Type:String
});
const emits = defineEmits(['handleQuery']);
const ruleFormRef = ref();
const state = reactive({
	isShowDialog: false,
	ruleForm: {}
});

// 打开弹窗
const openDialog = (row: any) => {
	state.ruleForm = JSON.parse(JSON.stringify(row));
	state.isShowDialog = true;
	ruleFormRef.value?.resetFields();
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
    state.ruleForm.Conf_Type=props.Conf_Type;
    await add(state.ruleForm);
		closeDialog();
	});
};

// 导出对象
defineExpose({ openDialog });
</script>
