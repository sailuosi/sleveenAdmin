<template>
	<div class="widget-config-container">
		<el-form v-if="applet.oid" label-position="left" label-width="80px">
			<el-divider>区块信息</el-divider>
			<fminput v-model="applet.PC_Idx" label="序号" type="number"></fminput>
			<fminput v-model="applet.PC_IsOn" label="启用" type="boolean"></fminput>
			<fminput v-model="applet.name" label="名称" ></fminput>
			<fminput v-model="applet.x" label="x" type="float"></fminput>
			<fminput v-model="applet.y" label="y" type="float"></fminput>
			<fminput v-model="applet.w" label="宽度" type="float"></fminput>
			<fminput v-model="applet.h" label="高度" type="float"></fminput>
			<fminput v-model="applet.i" label="i"></fminput>
			<!--<fminput v-model="applet.ckey" label="区块key"></fminput>-->
			<fminput v-model="applet.mode" label="多播" type="dict" dict="SysAppletMany"></fminput>
			<fminput v-model="applet.theme" label="主题" type="dict" dict="SysPtCellTheme"></fminput>
			<fminput v-model="applet.titleAlign" label="位置" type="dict" dict="SysAppletTitle"></fminput>
			<fminput v-model="applet.css" label="Css"></fminput>
			<fminput v-model="applet.PC_Margin" label="外边距" ></fminput>
			<fminput v-model="applet.PC_Padding" label="内边距" ></fminput>
			<fminput v-model="applet.style" label="自定义样式" type="textarea"></fminput>       
			<fminput v-model="applet.PC_Remark" label="备注"></fminput>
			
			<el-divider>控件信息</el-divider>
			<el-tabs class="vclist-tab" v-model="selectedVc" type="card" :closable="applet.vclist.length > 1"
				@tab-remove="handleDeleteApplet" @tab-click="handleClickApplet">
				<el-tab-pane v-for="(vc, index) in applet.vclist" :key="index" :label="vc.title" :name="index">
					<div class="config-form">
						<!--<fminput v-model="vc.link" label="区块key"></fminput>-->
						<fminput v-model="vc.title" label="标题" class="fm-row" must></fminput>
						<fminput v-model="vc.titleAlign" label="显示标题" type="dict" dict="SysAppletTitle" must></fminput>
						<fminput v-model="vc.tools" label="标题工具栏" type="dict" dict="SysApplteTool"></fminput>
						<!--<fminput v-model="vc.CL_Idx" label="序号" type="number"></fminput>-->
						<fminput v-model="vc.CL_IsOn" label="启用" type="boolean"></fminput>
						<fminput v-model="vc.theme" label="主题" type="dict" dict="SysPtCellTheme" must></fminput>
						<fminput v-model="vc.type" label="内容类型" type="dict" dict="SysAppletType" must></fminput>
						<fminput v-model="vc.appletKey" label="组件key" class="fm-row" type="textarea" must></fminput>
						
						<el-button class="select-button" @click="openApplet(vc)" type="warning">从组件库选择</el-button>
						<el-button class="select-button" @click="openPage(vc)" type="warning">从门户选择</el-button>
						
						<fminput v-model="vc.para" label="组件参数" class="fm-row" type="textarea"></fminput>
						<fminput v-model="vc.disabled" label="可用性" type="dict" dict="SysCellDisabled"></fminput>
						<fminput v-model="vc.titleStyle" label="标题样式" class="fm-row" type="textarea"></fminput>
						<fminput v-model="vc.style" label="内容样式" class="fm-row" type="textarea"></fminput>
						<fminput v-model="vc.tcolor" label="颜色"></fminput>
						<fminput v-model="vc.icon" label="图标"></fminput>
						<fminput v-model="vc.slot" label="缺省内容" class="fm-row" type="textarea"></fminput>
						<fminput v-model="vc.paraval" label="默认参数" class="fm-row" type="textarea"></fminput>
						<fminput v-model="vc.para2" label="第2参数" class="fm-row" type="textarea"></fminput>
						<fminput v-model="vc.CL_Remark" label="备注" class="fm-row" type="textarea"></fminput>
					</div>
				</el-tab-pane>
			</el-tabs>
		</el-form>
		
		<el-dialog title="组件库" class="normal" v-model="showPtApplet" width="70vw" append-to-body @close="showPtApplet=false">
            <ptapplet style="height:75vh;" :selecting="true">
                <template #applet>
                    <el-table-column fixed="left" label="操作" width="110px">
                        <template #default="p">
                            <el-button type="warning" @click="selectApplet(p.row)">选择该组件</el-button>
                        </template>
                    </el-table-column>
                </template>
            </ptapplet>
        </el-dialog>
        <el-dialog title="门户" class="normal" v-model="showPtPage" width="70vw" append-to-body @close="showPtPage=false">
            <paotallist style="height:75vh;" :selecting="true">
                <el-table-column fixed="left" label="操作" width="110px">
                    <template #default="p">
                        <el-button type="warning" @click="selectPage(p.row)">选择该门户</el-button>
                    </template>
                </el-table-column>
            </paotallist>
        </el-dialog>
	</div>
</template>

<script>
import fminput from "@/ui/common/input/fminput.vue";
import ptapplet from  "@/widgets/sysext/views/paotalconfig/ptapplet.vue";
import paotallist from "@/widgets/sysext/views/paotalconfig/paotallist.vue"
import Sortable from 'sortablejs';

export default {
	name: "WidgetConfig",
	components: {
		fminput,
		ptapplet,
		paotallist
	},
	props: {
		isCopy: {
			type: Boolean,
			default: false
		},
		applet: {
			type: Object,
			default: {}
		}
	},
	data() {
		return {
			selectedVc: 0,
			showPtApplet: false,
			showPtPage: false,
			currentVc: {}
		}
	},
	computed: {
		appletOid() {
			if (this.applet) {
				return this.applet.oid;
			}
			return 0;
		}
	},
	watch: {
		appletOid(newValue) {
			this.selectedVc = 0;
		}
	},
	methods: {
		initSortable() {
			const elTabs = document.querySelector(".vclist-tab");
			if (elTabs) {
				const elTabsNav = elTabs.querySelector(".el-tabs__nav");
				if (elTabsNav) {
					let that = this;
					Sortable.create(elTabsNav, {
						onEnd({ newIndex, oldIndex }) {
							let _vclist = [];
							that.applet.vclist.forEach((vc, index) => {
								_vclist.push(vc);
							});
							_vclist.sort((a, b) => {
								return a.CL_Idx - b.CL_Idx;
							});
							
							let _vcs = _vclist.splice(oldIndex, 1);
							_vclist.splice(newIndex, 0, _vcs[0]);
							_vclist.forEach((vc, index) => {
								vc.CL_Idx = index + 1;
							});
						}
					});
				}
			}
		},
		handleClickApplet(index) {
			this.initSortable();//通过点击tab触发sortable的初始化
		},
		handleDeleteApplet(index) {
			this.$confirm("确定要删除该控件？", "提示", {
				type: "warning",
			}).then(() => {
				this.applet.vclist.splice(index, 1);
				this.selectedVc = index > 0 ? index - 1 : 0
			});
		},
		openApplet(vc) {
			this.currentVc = vc;
			this.showPtApplet = true;
		},
		selectApplet(row) {
			this.currentVc.appletId = row.saId || row.saId;
			this.currentVc.title = row.A_Title;
			this.currentVc.type = row.A_Type;
			this.currentVc.appletKey = row.A_Key;
			
			this.currentVc.titleAlign = this.currentVc.titleAlign || row.A_TitleAlign;
			this.currentVc.theme = this.currentVc.theme || row.A_Theme;
			this.currentVc.tools = this.currentVc.tools || row.A_Tool;
			this.currentVc.tcolor = this.currentVc.tcolor || row.A_Color;
			this.currentVc.para = this.currentVc.para || row.A_Para;
			this.currentVc.titleStyle = this.currentVc.titleStyle || row.A_TitleStyle;
			this.currentVc.style = this.currentVc.style || row.A_BodyStyle;
			this.currentVc.icon = this.currentVc.icon || row.A_Icon;
			this.currentVc.slot = this.currentVc.slot || row.A_Slot;
			this.currentVc.paraval = this.currentVc.paraval || row.A_ParaVal;
			this.currentVc.para2 = this.currentVc.para2 || row.A_Para2;
			this.currentVc.CL_Remark = this.currentVc.CL_Remark || row.A_Remark;
			
			this.showPtApplet = false;
		},
		openPage(vc) {
			this.currentVc = vc;
			this.showPtPage = true;
		},
		selectPage(row) {
			this.currentVc.appletId = row.spId || row.spId;
			this.currentVc.title = row.P_Name;
			this.currentVc.type = "7";//子门户
			this.currentVc.appletKey = row.P_Key;
			
			this.showPtPage = false;
		}
	}
}
</script>

<style scoped lang="scss">
.widget-config-container {
	padding: 5px 15px 20px 20px;
}

.config-form {
	padding: 10px 10px 20px 10px;
	border: 1px solid #e4e7ed;
	border-top-width: 0;
}

:deep(.el-tabs--card>.el-tabs__header .el-tabs__item) {
	border-bottom-width: 2px;
}

.select-button {
	margin: 0 25px 20px 25px;
}
</style>
