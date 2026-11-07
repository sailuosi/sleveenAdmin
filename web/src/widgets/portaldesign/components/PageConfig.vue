<template>
	<div class="form-config-container">
		<el-form label-position="left" label-width="80px">
			<!--<el-divider>基本信息</el-divider>-->
			<fminput v-model="portal.name" label="名称"></fminput>
			<fminput v-model="portal.P_IsOn" label="启用" type="boolean"></fminput>
			<fminput v-model="portal.P_Note" label="说明" type="textarea"></fminput>
			<fminput v-model="portal.P_Task" label="业务" type="dict" dict="SysPtTask"></fminput>
			<fminput v-if="isCopy" v-model="portal.P_Key" label="唯一编码" must></fminput>
			<el-form-item v-if="isCopy" label="所属单位" required>
				<el-input v-model="selectedOrgName" placeholder="请选择所属单位" readonly>
					<template #append>
						<el-button :disabled="!api" @click="openOrgTree">选择</el-button>
					</template>
				</el-input>
			</el-form-item>
			<fminput v-model="portal.P_Pulic" label="共享" type="dict" dict="SysShareStatus"></fminput>
			<fminput v-model="portal.theme" label="主题"  type="dict" dict="SysPtCellTheme"></fminput>
			<fminput v-model="portal.P_Idx" label="序号" type="float"></fminput>
			<fminput v-model="portal.P_Group" label="分组"></fminput>
			<fminput v-model="portal.P_ShowTitle" label="显示标题" type="dict" dict="SysAppletTitle"></fminput>
			<fminput v-model="portal.P_Icon" label="图标"></fminput>
			<fminput v-model="portal.css" label="类"></fminput>
			<fminput v-model="portal.display" label="布局方式"></fminput>
			<fminput v-model="portal.colnum" label="列数" type="float"></fminput>
			<fminput v-model="portal.rowheight" label="行高" type="float"></fminput>
			<fminput v-model="portal.editable" label="可拖动" type="boolean"></fminput>
			<fminput v-model="portal.style" label="自定义样式" type="textarea"></fminput>
			<fminput v-model="portal.para" label="参数" placeholder="请使用url参数格式定义"></fminput>
			<fminput v-model="portal.P_Remark" label="备注" type="textarea"></fminput>
		</el-form>
		<el-dialog
			title="选择所属单位"
			v-model="showOrgTree"
			width="400px"
			append-to-body
			:close-on-click-modal="false"
			@close="tempSelectedOrg = null"
		>
			<VC_HR_OrganizeTree
				v-if="showOrgTree"
				:api="api"
				height="400px"
				@nodeclick="onOrgNodeClick"
			/>
			<template #footer>
				<el-button @click="cancelOrgSelect">取消</el-button>
				<el-button type="primary" @click="confirmOrgSelect">确定</el-button>
			</template>
		</el-dialog>
	</div>
</template>

<script>
import fminput from "@/ui/common/input/fminput.vue";
import VC_HR_OrganizeTree from "@s/views/OrgCommon/org/hrtree.vue";

export default {
	name: "PageConfig",
	components: {
		fminput,
		VC_HR_OrganizeTree
	},
	props: {
		isCopy: {
			type: Boolean,
			default: false
		},
		portal: {
			type: Object,
			default: {}
		},
		api: {
			type: Object,
			default: null
		}
	},
	data() {
		return {
			showOrgTree: false,
			tempSelectedOrg: null,
			selectedOrgName: ""
		};
	},
	watch: {
		"portal.spOrgId": {
			immediate: true,
			handler(val) {
				if (!val) {
					this.selectedOrgName = "";
					return;
				}
				this.loadOrgNameFromId(val);
			}
		}
	},
	methods: {
		loadOrgNameFromId(orgId) {
			if (!orgId || !this.api || !this.api.queryBCHROrganizeById) return;
			this.api.queryBCHROrganizeById(orgId).then((res) => {
				this.selectedOrgName = res?.O_OrgName || res?.O_Name || res?.OrgName || "";
			}).catch(() => {
				if (this.portal?.spOrgId === orgId) {
					this.selectedOrgName = "";
				}
			});
		},
		openOrgTree() {
			this.tempSelectedOrg = null;
			this.showOrgTree = true;
		},
		onOrgNodeClick(node) {
			this.tempSelectedOrg = node || null;
			this.applyOrg(node);
			this.showOrgTree = false;
		},
		confirmOrgSelect() {
			this.applyOrg(this.tempSelectedOrg);
			this.showOrgTree = false;
			this.tempSelectedOrg = null;
		},
		cancelOrgSelect() {
			this.showOrgTree = false;
			this.tempSelectedOrg = null;
		},
		applyOrg(node) {
			if (!node) return;
			this.portal.spOrgId = node.HR_OrganizeOID;
			this.selectedOrgName = node.O_OrgName || node.O_Name || node.OrgName || "";
		}
	}
}
</script>

<style scoped lang="scss">
.form-config-container {
	padding: 20px 15px 20px 20px;
}
</style>
