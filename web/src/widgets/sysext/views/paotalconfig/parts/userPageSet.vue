<template>
	<flexpan flex>
		<template #header="p">
			<el-form inline ref="ruleForm" :model="queryParams" label-width="60px" @submit.native.prevent>
				<el-form-item label="用户账号" prop="Account" class="search-field">
					<el-input v-model="queryParams.Account" placeholder="用户账号" clearable @keyup.enter.native="getlist"></el-input>
				</el-form-item>
				<el-form-item label="用户姓名" prop="RealName" class="search-field">
					<el-input v-model="queryParams.RealName" placeholder="姓名" clearable @keyup.enter.native="getlist"></el-input>
				</el-form-item>
				<!--<el-form-item label="用户昵称" prop="NickName" class="search-field">
					<el-input v-model="queryParams.NickName" placeholder="昵称" clearable @keyup.enter.native="getlist"></el-input>
				</el-form-item>-->
				<el-form-item label="页面标题" prop="PP_Title" class="search-field">
					<el-input v-model="queryParams.PP_Title" placeholder="页面标题" clearable @keyup.enter.native="getlist"></el-input>
				</el-form-item>
				<el-form-item label="页面编码" prop="PP_Code" class="search-field">
					<el-input v-model="queryParams.PP_Code" placeholder="页面编码" clearable @keyup.enter.native="getlist"></el-input>
				</el-form-item>
				<el-form-item label="是否设置" prop="isSet" class="search-field">
					<el-select v-model="queryParams.isSet" placeholder="是否设置" clearable @change="getlist">
						<el-option label="未设置" :value="false"></el-option>
						<el-option label="已设置" :value="true"></el-option>
					</el-select>
				</el-form-item>
				<el-form-item>
					<el-button type="primary" icon="el-icon-search" @click="getlist">查询</el-button>
					<el-button @click="resetQuery">重置</el-button>
				</el-form-item>
			</el-form>
		</template>
		<template #default="p">
			<vclist :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="p.height - 10" v-loading="loading">
				<el-table-column type="index" fixed="left" width="50" label="序号" align="center" />
				<el-table-column fixed="left" align="center" label="操作" width="200px">
					<template #default="p">
						<el-link v-if="!p.row.sppId" type="primary" @click="selectPage(p.row)">设置默认页面</el-link>
						<template v-if="p.row.sppId">
							<el-link class="oper-button" type="primary" @click="selectPage(p.row)">修改默认页面</el-link>
							<el-link class="oper-button" type="danger" @click="deleteUserPage(p.row)">取消设置</el-link>
						</template>
					</template>
				</el-table-column>
				<vccol field="Account" caption="用户账号" min-width="100"></vccol>
				<vccol field="RealName" caption="用户姓名" min-width="150"></vccol>
				<!--<vccol field="NickName" caption="用户昵称" min-width="200"></vccol>-->
				<vccol field="PP_Title" caption="页面标题" min-width="200"></vccol>
				<vccol field="PP_IsOn" caption="页面启用" type="Boolean" min-width="100"></vccol>
				<vccol field="PP_Pose" caption="页面位置" type="dict" dict="KB_PtPagePose" min-width="160"></vccol>
				<vccol field="PP_OrgName" caption="单位名称" min-width="200"></vccol>
				<vccol field="PP_Code" caption="页面编码" min-width="200"></vccol>
			</vclist>
			
			<cardialog v-model="showPageSelectWin" label="请选择页面" size="middle">
				<el-form inline ref="ruleForm" :model="pQueryParams" label-width="60px" @submit.native.prevent>
					<el-form-item label="标题" prop="PP_Title" class="search-field">
						<el-input v-model="pQueryParams.PP_Title" placeholder="标题" clearable @keyup.enter.native="getPageList"></el-input>
					</el-form-item>
					<el-form-item label="单位名称" prop="PP_OrgName" class="search-field">
						<el-input v-model="pQueryParams.PP_OrgName" placeholder="单位名称" clearable @keyup.enter.native="getPageList"></el-input>
					</el-form-item>
					<el-form-item>
						<el-button type="primary" icon="el-icon-search" @click="getPageList">查询</el-button>
						<el-button @click="resetPageQuery">重置</el-button>
					</el-form-item>
				</el-form>
				<vclist v-loading="ploading" :list="plist" :total="ptotal" :para="pQueryParams" height="480" @query="getPageList" :maxLine="1">
					<el-table-column type="index" fixed="left" width="50" label="序号" align="center" />
					<el-table-column fixed="left" align="center" label="操作" width="80px">
						<template #default="p">
							<el-link type="primary" @click="submitSelectPage(p.row)">选择</el-link>
						</template>
					</el-table-column>
					<slot></slot>
					<vccol field="PP_IsOn" caption="启用" type="Boolean" width="60px"></vccol>
					<vccol field="PP_Title" caption="标题" min-width="200"></vccol>
					<vccol field="PP_OrgName" caption="单位名称" min-width="200"></vccol>
					<vccol field="PP_Code" caption="编码" min-width="150"></vccol>
				</vclist>
			</cardialog>
		</template>
	</flexpan>
</template>

<script>
import { selectUserWithDefaultPage, saveUseDefaultPage, removeUseDefaultPage, fetchSysPortPagePage } from "@s/api/paotou/paotalapi.js";

import flexpan from '@/ui/common/flexpan.vue';
import vclist from '@/ui/common/vclist/vclist.vue';
import vccol from '@/ui/common/vclist/vccol.vue';
import cardialog from "@/ui/panels/cardtab/cardialog.vue";

export default {
	name: 'userPageSet',
	components: {
		flexpan,
		vclist,
		vccol,
		cardialog
	},
	props: {
		para: null
	},
	data() {
		return {
			loading: false,
			mlist: [],
			total: 0,
			queryParams: {
				pageSize: 30,
				pageNum: 1
			},
			currentUser: "",
			ploading: false,
			plist: [],
			ptotal: 0,
			pQueryParams: {
				pageSize: 100,
				pageNum: 1
			},
			showPageSelectWin: false
		};
	},
	mounted() {
		this.getlist();
		this.getPageList();
	},
	methods: {
		getlist() {
			this.loading = true;
			
			selectUserWithDefaultPage(this.queryParams).then(res => {
				this.mlist = res.data;
				this.total = res.total;
			}).finally(() => {
				this.loading = false;
			});
		},
		resetQuery() {
			this.queryParams = {
				pageSize: 30,
				pageNum: 1
			};
			this.getlist();
		},
		getPageList() {
			this.ploading = true;
			fetchSysPortPagePage(this.pQueryParams).then(res => {
				this.plist = res.data;
				this.ptotal = res.total;
			}).finally(() => {
				this.ploading = false;
			});
		},
		resetPageQuery() {
			this.pQueryParams = {
				pageSize: 100,
				pageNum: 1
			};
			this.getPageList();
		},
		selectPage(row) {
			this.currentUser = row;
			this.showPageSelectWin = true;
		},
		submitSelectPage(prow) {
			saveUseDefaultPage({
				Account: this.currentUser.Account,
				sppId: prow.sppId
			}).then(res => {
				this.$message({
					type: res.status,
					message: res.data || res.info
				});
				
				this.getlist();
				
				this.showPageSelectWin = false;
			});
		},
		deleteUserPage(row) {
			this.$confirm("确定要取消设置用户【" + row.RealName + "】的默认页面吗？", "提示", {
				confirmButtonText: "确定",
				cancelButtonText: "取消",
				type: "warning"
			}).then(() => {
				removeUseDefaultPage({
					Account: row.Account,
					sppId: row.sppId
				}).then(res => {
					this.$message({
						type: res.status,
						message: res.data || res.info
					});
					
					this.getlist();
				});
			});
		}
	}
};
</script>
<style scoped>
.search-field {
	width: 200px;
}

.oper-button {
	margin: 0 5px;
}
</style>
