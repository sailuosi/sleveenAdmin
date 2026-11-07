<template>
	<div class="sys-user-container">
		<el-row :gutter="8" style="width: 100%">
			<el-col :span="0" :xs="24">
				<OrgTree ref="orgTreeRef" @node-click="nodeClick" />
			</el-col>

			<el-col :span="24" :xs="24">
				<el-card shadow="hover" :body-style="{ paddingBottom: '0' }">
					<el-form :model="state.queryParams" ref="queryForm" :inline="true">
						<el-form-item label="账号">
							<el-input v-model="state.queryParams.account" placeholder="账号" clearable />
						</el-form-item>
						<el-form-item label="姓名">
							<el-input v-model="state.queryParams.realName" placeholder="姓名" clearable  />
						</el-form-item>
						<el-form-item label="手机号码">
							<el-input v-model="state.queryParams.phone" placeholder="手机号码" clearable />
						</el-form-item>
						<el-form-item>
							<el-button-group>
								<el-button type="primary"  @click="handleQuery" v-auth="'sysUser:page'"> 查询 </el-button>
								<el-button type="info"  @click="resetQuery"> 重置 </el-button>
							</el-button-group>
						</el-form-item>
						<el-form-item>
							<el-button type="primary"  @click="openAddUser" v-auth="'sysUser:add'"> 新增 </el-button>
						</el-form-item>
					</el-form>
				</el-card>

				<el-card class="full-table" shadow="hover" style="margin-top: 8px">
					<el-table :data="state.userData" style="width: 100%" v-loading="state.loading" border>
						<el-table-column label="操作" width="290" align="center" fixed="left" show-overflow-tooltip>
							<template #default="scope">
								<el-button size="small" text type="primary" @click="openEditUser(scope.row)" > 编辑 </el-button>

								<el-button size="small" text type="primary" @click="resetUserPwd(scope.row)" > 密码 </el-button>
								<el-button size="small" text type="primary" @click="delUser(scope.row)" > 删除 </el-button>
							</template>
						</el-table-column>						
                        <el-table-column type="index" label="序号" width="55" align="center" fixed />
						<el-table-column prop="Account" label="账号" width="120" align="center" fixed show-overflow-tooltip />
						<!-- <el-table-column prop="nickName" label="昵称" width="120" align="center" show-overflow-tooltip /> -->
						<el-table-column prop="RealName" label="姓名" width="120" align="center" show-overflow-tooltip />
						<el-table-column label="头像" width="80" align="center" show-overflow-tooltip>
							<template #default="scope">
								<el-avatar :src="scope.row.Avatar" size="small">{{ scope.row.NickName?.slice(0, 1) ?? scope.row.RealName?.slice(0, 1) }} </el-avatar>
							</template>
						</el-table-column>
						<el-table-column prop="Phone" label="手机号码" width="130" align="left" show-overflow-tooltip />
	
						<el-table-column label="账号类型" width="100" align="center" show-overflow-tooltip>
							<template #default="scope">
								<el-tag v-if="scope.row.AccountType === 888"> 系统管理员 </el-tag>
								<el-tag v-else-if="scope.row.AccountType === 777"> 普通账号 </el-tag>
								<el-tag v-else-if="scope.row.AccountType === 666"> 会员 </el-tag>
								<el-tag v-else-if="scope.row.AccountType === 501"> 银行经理 </el-tag>
								<el-tag v-else> 其他 </el-tag>
							</template>
						</el-table-column>
						<el-table-column label="状态" width="70" align="center" show-overflow-tooltip>
							<template #default="scope">
								<el-switch v-model="scope.row.status" :active-value="1" :inactive-value="2" size="small" @click="changeStatus(scope.row)" v-auth="'sysUser:setStatus'" />
							</template>
						</el-table-column>
						<el-table-column label="出生日期" width="100" align="center" show-overflow-tooltip>
							<template #default="scope">
								{{ formatDate(new Date(scope.row.birthday), 'YYYY-mm-dd') }}
							</template>
						</el-table-column>
						<el-table-column label="性别" width="70" align="center" show-overflow-tooltip>
							<template #default="scope">
								<el-tag type="success" v-if="scope.row.sex === 1"> 男 </el-tag>
								<el-tag type="danger" v-else> 女 </el-tag>
							</template>
						</el-table-column>
						<el-table-column prop="OrderNo" label="排序" width="70" align="center" show-overflow-tooltip />
						<el-table-column prop="CreateTime" label="修改时间" width="160" align="center" show-overflow-tooltip />
						<el-table-column prop="Remark" label="备注" header-align="center" show-overflow-tooltip />

					</el-table>
					<el-pagination
						v-model:currentPage="state.tableParams.page"
						v-model:page-size="state.tableParams.pageSize"
						:total="state.tableParams.total"
						:page-sizes="[10, 20, 50, 100]"
						small
						background
						@size-change="handleSizeChange"
						@current-change="handleCurrentChange"
						layout="total, sizes, prev, pager, next, jumper"
					/>
				</el-card>
			</el-col>
		</el-row>

		<EditUser ref="editUserRef" :title="state.editUserTitle" :orgData="state.orgTreeData" @handleQuery="handleQuery" />
        <el-dialog :title="curRow.realName+'权限浏览'" class="normal" v-model="seeAuthed" width="70vw" append-to-body @close="seeAuthed=false">
            <userAuths :account="curRow.account" @close="diaRow=false" @refresh="getlist">
            </userAuths>
        </el-dialog>
	</div>
</template>

<script  setup name="sysUser">
import { onMounted, reactive, ref } from 'vue';
import { ElMessageBox, ElMessage } from 'element-plus';
import { formatDate } from '@/utils/date.js';

import EditUser from './component/editUser.vue';
//                                <el-button size="small" text type="primary" @click="seeUserAuth(scope.row)" > 权限 </el-button>

import { apiSysUserPagePost, apiSysUserDeletePost,apiSysUserSetStatusPost,apiSysUserResetPwdPost } from '@s/api/system/sysuserapi.js';

import { apiSysOrgListGet } from "@s/api/system/sysorgapi.js";
import userAuths from "./component/userAuths.vue"

const orgTreeRef = ref();
const editUserRef = ref();
const state = reactive({
	loading: false,
	userData: [] ,
	orgTreeData: [] ,
	queryParams: {
		orgId: -1,
		account: undefined,
		realName: undefined,
		phone: undefined,
	},
	tableParams: {
		page: 1,
		pageSize: 20,
		total: 0 ,
	},
	editUserTitle: '',
});

onMounted(async () => {
	loadOrgData();
	handleQuery();
});

// 查询机构数据
const loadOrgData = async () => {
	state.loading = true;
	var res = await apiSysOrgListGet(0);
	state.orgTreeData = res?? [];
	state.loading = false;
};

// 查询操作
const handleQuery = async () => {
	state.loading = true;
	let params = Object.assign(state.queryParams, state.tableParams);
	var res = await apiSysUserPagePost(params);
	console.log(res);
	state.userData = res.Items ?? [];
	state.tableParams.total = res.Total;
	state.loading = false;
};

// 重置操作
const resetQuery = () => {
	state.queryParams.orgId = -1;
	state.queryParams.account = undefined;
	state.queryParams.realName = undefined;
	state.queryParams.phone = undefined;
	handleQuery();
};

// 打开新增页面
const openAddUser = () => {
	state.editUserTitle = '添加账号';
	editUserRef.value?.openDialog({ Id: undefined, Birthday: '2000-01-01', Sex: 1, OrderNo: 100, CardType: 0, CultureLevel: 5 });
};

// 打开编辑页面
const openEditUser = (row) => {
	state.editUserTitle = '编辑账号';
	editUserRef.value?.openDialog(row);
};

// 删除
const delUser = (row) => {
	ElMessageBox.confirm(`确定删除账号：【${row.Account}】?`, '提示', {
		confirmButtonText: '确定',
		cancelButtonText: '取消',
		type: 'warning',
	})
		.then(async () => {
			await apiSysUserDeletePost({ id: row.Id });
			handleQuery();
			ElMessage.success('删除成功');
		})
		.catch(() => {});
};

// 改变页面容量
const handleSizeChange = (val) => {
	state.tableParams.pageSize = val;
	handleQuery();
};

// 改变页码序号
const handleCurrentChange = (val) => {
	state.tableParams.page = val;
	handleQuery();
};

// 修改状态
const changeStatus = (row) => {
	apiSysUserSetStatusPost({ id: row.Id, status: row.Status })
	.then(() => {
		ElMessage.success('账号状态设置成功');
	})
	.catch(() => {
		row.Status = row.Status == 1 ? 2 : 1;
	});
};

// 重置密码
const resetUserPwd = async (row) => {
	ElMessageBox.confirm(`确定重置密码：【${row.Account}】?`, '提示', {
		confirmButtonText: '确定',
		cancelButtonText: '取消',
		type: 'warning',
	})
		.then(async () => {
			apiSysUserResetPwdPost({ id: row.Id })
			.then((res) => {
				ElMessage.success(`密码重置成功为：${res}`);
			});
		})
		.catch(() => {});
};

// 树组件点击
const nodeClick = async (node) => {
	state.queryParams.orgId = node.id;
	state.queryParams.account = undefined;
	state.queryParams.realName = undefined;
	state.queryParams.phone = undefined;
	handleQuery();
};

/// 权限查询
const seeAuthed= ref(false);
const curRow=ref({})

function seeUserAuth(row){
    seeAuthed.value=true;
    curRow.value=row;
}

</script>
