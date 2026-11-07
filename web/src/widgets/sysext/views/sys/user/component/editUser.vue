<template>
	<div class="sys-user-container">
		<el-dialog v-model="state.isShowDialog" draggable :close-on-click-modal="false" width="700px">
			<template #header>
				<div style="color: #fff;display: flex;align-items: center;">
					<el-icon size="16" style="margin-right: 3px; display: inline; vertical-align: middle"> <ele-Edit /> </el-icon>
					<span>{{ props.title }}</span>
				</div>
			</template>
			<el-tabs v-loading="state.loading" v-model="state.selectedTabName" >
				<el-tab-pane label="基础信息" lazy>
					<el-form :model="state.ruleForm" ref="ruleFormRef" label-width="auto">
						<el-row :gutter="35">
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="账号名称" prop="Account" :rules="[{ required: true, message: '账号名称不能为空', trigger: 'blur' }]">
									<el-input v-model="state.ruleForm.Account" placeholder="账号名称" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="真实姓名" prop="RealName" :rules="[{ required: true, message: '真实姓名不能为空', trigger: 'blur' }]">
									<el-input v-model="state.ruleForm.RealName" placeholder="真实姓名" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="手机号码" prop="Phone" :rules="[{ required: true, message: '手机号码不能为空', trigger: 'blur' }]">
									<el-input v-model="state.ruleForm.Phone" placeholder="手机号码" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="出生日期" prop="Birthday" :rules="[{ required: true, message: '出生日期不能为空', trigger: 'blur' }]">
									<el-date-picker v-model="state.ruleForm.Birthday" type="date" placeholder="出生日期" format="YYYY-MM-DD" value-format="YYYY-MM-DD" class="w100" />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="昵称">
									<el-input v-model="state.ruleForm.NickName" placeholder="昵称" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="性别">
									<el-radio-group v-model="state.ruleForm.Sex">
										<el-radio :label="1" :value="1">男</el-radio>
										<el-radio :label="2" :value="2">女</el-radio>
									</el-radio-group>
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="角色集合" prop="roleIdList" >
									<el-select v-model="state.ruleForm.roleIdList" multiple value-key="id" clearable placeholder="角色集合" collapse-tags collapse-tags-tooltip class="w100" filterable>
										<el-option v-for="item in state.roleData" :key="item.id" :label="item.name" :value="item.id" />
									</el-select>
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="账号类型" prop="AccountType" :rules="[{ required: true, message: '角色集合不能为空', trigger: 'blur' }]">
									<el-select v-model="state.ruleForm.accountType" placeholder="账号类型" collapse-tags collapse-tags-tooltip class="w100">
										<el-option label="系统管理员" :value="888" :disabled="userInfos.accountType != 888 && userInfos.accountType != 999" />
										<el-option label="普通账号" :value="777" />
										<el-option label="会员" :value="666" />
										<el-option label="银行经理" :value="501" />
									</el-select>
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb5">
								<el-form-item label="年龄">
									<el-input-number v-model="state.ruleForm.Age" placeholder="年龄" class="w100" />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb5">
								<el-form-item label="排序">
									<el-input-number v-model="state.ruleForm.OrderNo" placeholder="排序" class="w100" />
								</el-form-item>
							</el-col>
							<el-divider border-style="dashed" content-position="center">
								<div style="color: #b1b3b8">机构组织</div>
							</el-divider>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="所属机构" prop="OrgId" :rules="[{ required: true, message: '所属机构不能为空', trigger: 'blur' }]">
									<el-cascader
										:options="props.orgData"
										:props="{ checkStrictly: true, emitPath: false, value: 'id', label: 'name', expandTrigger: 'hover' }"
										placeholder="所属机构"
										clearable
										class="w100"
										v-model="state.ruleForm.OrgId"
									>
										<template #default="{ node, data }">
											<span>{{ data.Name }}</span>
											<span v-if="!node.isLeaf"> ({{ data.children.length }}) </span>
										</template>
									</el-cascader>
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="职位" prop="PosId" :rules="[{ required: true, message: '职位名称不能为空', trigger: 'blur' }]">
									<el-select v-model="state.ruleForm.PosId" placeholder="职位" class="w100">
										<el-option v-for="d in state.posData" :key="d.Id" :label="d.Name" :value="d.Id" />
									</el-select>
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="工号">
									<el-input v-model="state.ruleForm.JobNum" placeholder="工号" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="入职日期">
									<el-date-picker v-model="state.ruleForm.JoinDate" type="date" placeholder="入职日期" format="YYYY-MM-DD" value-format="YYYY-MM-DD" class="w100" />
								</el-form-item>
							</el-col>
							<el-divider border-style="dashed" content-position="center">
								<div style="color: #b1b3b8">附属机构</div>
							</el-divider>
							<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
								<el-button icon="ele-Plus" type="primary" plain @click="addExtOrgRow"> 增加附属机构 </el-button>
								<span style="font-size: 12px; color: gray; padding-left: 5px"> 具有相应组织机构的数据权限 </span>
							</el-col>
							<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
								<template v-if="state.ruleForm.ExtOrgIdList != undefined && state.ruleForm.ExtOrgIdList.length > 0">
									<el-row :gutter="35" v-for="(v, k) in state.ruleForm.ExtOrgIdList" :key="k">
										<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
											<el-form-item label="机构" :prop="`ExtOrgIdList[${k}].OrgId`" :rules="[{ required: true, message: `机构不能为空`, trigger: 'blur' }]">
												<template #label>
													<el-button icon="ele-Delete" type="danger" circle plain size="small" @click="deleteExtOrgRow(k)" />
													<span class="ml5">机构</span>
												</template>
												<el-cascader
													:options="props.orgData"
													:props="{ checkStrictly: true, emitPath: false, value: 'Id', label: 'Name', expandTrigger: 'hover' }"
													placeholder="机构组织"
													clearable
													class="w100"
													v-model="state.ruleForm.ExtOrgIdList[k].OrgId"
												>
													<template #default="{ node, data }">
														<span>{{ data.name }}</span>
														<span v-if="!node.isLeaf"> ({{ data.children.length }}) </span>
													</template>
												</el-cascader>
											</el-form-item>
										</el-col>
										<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
											<el-form-item label="职位" :prop="`ExtOrgIdList[${k}].PosId`" :rules="[{ required: true, message: `职位不能为空`, trigger: 'blur' }]">
												<el-select v-model="state.ruleForm.ExtOrgIdList[k].PosId" placeholder="职位名称" class="w100">
													<el-option v-for="d in state.posData" :key="d.Id" :label="d.Name" :value="d.Id" />
												</el-select>
											</el-form-item>
										</el-col>
									</el-row>
								</template>
								<el-empty :image-size="80" description="空数据" v-else></el-empty>
							</el-col>
						</el-row>
					</el-form>
				</el-tab-pane>
				<el-tab-pane label="档案信息" lazy v-if="!!state.ruleForm">
					<el-form :model="state.ruleForm" label-width="auto">
						<el-row :gutter="35">
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="证件类型" prop="CardType">
									<el-select v-model="state.ruleForm.CardType" placeholder="证件类型" class="w100">
										<el-option label="身份证" :value="0" />
										<el-option label="护照" :value="1" />
										<el-option label="出生证" :value="2" />
										<el-option label="港澳台通行证" :value="3" />
										<el-option label="外国人居留证" :value="4" />
									</el-select>
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="证件号码">
									<el-input v-model="state.ruleForm.IdCardNum" placeholder="证件号码" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="民族">
									<el-input v-model="state.ruleForm.Nation" placeholder="民族" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="邮箱">
									<el-input v-model="state.ruleForm.Email" placeholder="邮箱" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
								<el-form-item label="地址">
									<el-input v-model="state.ruleForm.Address" placeholder="地址" clearable type="textarea" />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="毕业学校">
									<el-input v-model="state.ruleForm.College" placeholder="毕业学校" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="文化程度">
									<el-select v-model="state.ruleForm.CultureLevel" placeholder="文化程度" class="w100">
										<el-option label="其他" :value="0" />
										<el-option label="小学" :value="1" />
										<el-option label="初中" :value="2" />
										<el-option label="普通高中" :value="3" />
										<el-option label="技工学校" :value="4" />
										<el-option label="职业教育" :value="5" />
										<el-option label="职业高中" :value="6" />
										<el-option label="中等专科" :value="7" />
										<el-option label="大学专科" :value="8" />
										<el-option label="大学本科" :value="9" />
										<el-option label="硕士研究生" :value="10" />
										<el-option label="博士研究生" :value="11" />
									</el-select>
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="政治面貌">
									<el-input v-model="state.ruleForm.PoliticalOutlook" placeholder="政治面貌" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="办公电话">
									<el-input v-model="state.ruleForm.OfficePhone" placeholder="办公电话" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="紧急联系人">
									<el-input v-model="state.ruleForm.EmergencyContact" placeholder="紧急联系人" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="12" :md="12" :lg="12" :xl="12" class="mb20">
								<el-form-item label="联系人电话">
									<el-input v-model="state.ruleForm.EmergencyPhone" placeholder="联系人电话" clearable />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
								<el-form-item label="联系人地址">
									<el-input v-model="state.ruleForm.EmergencyAddress" placeholder="紧急联系人" clearable type="textarea" />
								</el-form-item>
							</el-col>
							<el-col :xs="24" :sm="24" :md="24" :lg="24" :xl="24" class="mb20">
								<el-form-item label="备注">
									<el-input v-model="state.ruleForm.Remark" placeholder="备注" clearable type="textarea" />
								</el-form-item>
							</el-col>
						</el-row>
					</el-form>
				</el-tab-pane>
			</el-tabs>
			<template #footer>
				<span class="dialog-footer">
					<el-button @click="cancel">取 消</el-button>
					<el-button type="primary" @click="submit">确 定</el-button>
				</span>
			</template>
		</el-dialog>
	</div>
</template>

<script lang="ts" setup name="sysEditUser">
import { onMounted, reactive, ref } from 'vue';
import { storeToRefs } from 'pinia';
import { useAuthStore } from '@/store/modules/auth';

//import { getAPI } from '/@/utils/axios-utils';
//import { SysPosApi, SysRoleApi, SysUserApi } from '/@/api-services/api';
//import { RoleOutput, SysOrg, SysPos, UpdateUserInput } from '/@/api-services/models';
import { loadPostlist } from "@s/api/system/posapi.js"
import { apiSysRoleListGet } from "@s/api/system/sysrole.js"
import { apiSysUserOwnRoleListUserIdGet,apiSysUserOwnExtOrgListUserIdGet,apiSysUserUpdatePost,apiSysUserAddPost } from '@s/api/system/sysuserapi.js';

const props = defineProps({
	title: String,
	orgData: Array<SysOrg>,
});
const emits = defineEmits(['handleQuery']);
const ruleFormRef = ref();
const userStore = useAuthStore();

const userInfos= userStore.userInfo;
const state = reactive({
	loading: false,
	isShowDialog: false,
	selectedTabName: '0', // 选中的 tab 页
	ruleForm: {} as UpdateUserInput,
	posData: [] as Array<SysPos>, // 职位数据
	roleData: [] as Array<RoleOutput>, // 角色数据
});

onMounted(async () => {
	state.loading = true;
	var res = await loadPostlist();
	console.log(res)
	state.posData = res ?? [];
	var res1 = await apiSysRoleListGet();
	state.roleData = res1 ?? [];
	state.loading = false;
});

// 打开弹窗
const openDialog = async (row: any) => {
	ruleFormRef.value?.resetFields();
	console.log(row)
	state.selectedTabName = '0'; // 重置为第一个 tab 页
	state.ruleForm = JSON.parse(JSON.stringify(row));
	if (row.Id != undefined) {
		var resRole = await apiSysUserOwnRoleListUserIdGet(row.Id);
		state.ruleForm.roleIdList = resRole;
		var resExtOrg = await apiSysUserOwnExtOrgListUserIdGet(row.Id);
		state.ruleForm.ExtOrgIdList = resExtOrg;
	} else state.ruleForm.AccountType = 777; // 默认普通账号类型
	state.isShowDialog = true;
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
		if (state.ruleForm.Id != undefined && state.ruleForm.Id > 0) {
			await apiSysUserUpdatePost(state.ruleForm);
		} else {
			await apiSysUserAddPost(state.ruleForm);
		}
		closeDialog();
	});
};

// 增加附属机构行
const addExtOrgRow = () => {
	if (state.ruleForm.ExtOrgIdList == undefined) state.ruleForm.ExtOrgIdList = [];
	state.ruleForm.ExtOrgIdList?.push({});
};

// 删除附属机构行
const deleteExtOrgRow = (k: number) => {
	state.ruleForm.ExtOrgIdList?.splice(k, 1);
};

// 导出对象
defineExpose({ openDialog });
</script>
<style lang="css" scoped>
.mb20{
	margin-bottom: 0;
}

</style>