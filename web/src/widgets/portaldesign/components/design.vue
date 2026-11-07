<template>
	<div class="form-designer">
		<el-container class="form-designer-container">
			<!-- 左侧字段 -->
			<el-aside :width="fullScreen ? '0' : '270px'">
				<div class="fields-list">
					<!--<el-divider>剪切板内组件</el-divider>
					<div @click="getContentInClipBoard" title="点击粘贴">
						<VueDraggable>
							<ul>
								<li class="field-label">
									<a style="cursor: pointer;">
										<i class="copy-document"></i>
										<span>粘贴组件</span>
									</a>
								</li>
							</ul>
						</VueDraggable>
					</div>-->
					<el-divider>组件列表</el-divider>
					<div v-for="(item, index) in portlets" :key="'portlet_' + index">
						<div class="field-title">{{ item.typeName }}</div>
						<ul>
							<VueDraggable tag="ul" v-model="item.portlets" :group="{ name: 'form', pull: 'clone', put: false }" ghost-class="ghost" :sort="false">
								<li class="field-label" v-for="portlet in item.portlets" :key="portlet.id">
									<a>
										<i :class="portlet.icon"></i>
										<span>{{ portlet.name }}</span>
									</a>
								</li>
							</VueDraggable>
						</ul>
					</div>
				</div>
			</el-aside>
			<!-- 中间主布局 -->
			<el-container class="widget-container" direction="vertical">
				<el-header class="widget-portal-container-header">
					<span v-if="isCopy" class="widget-container-warning">当前为复制模式，保存后将生成新的门户配置。</span>
					<el-button icon="ele-Back" size="default" text type="primary" @click="handleBack">返回</el-button>
					<el-button icon="ele-Check" size="default" text type="primary" @click="handleSubmit">保存</el-button>
					<el-button icon="ele-RefreshLeft" size="default" text type="danger" @click="handleRegain">还原</el-button>
					<el-button icon="ele-Delete" size="default" text type="danger" @click="handleClear">清空</el-button>
					<el-button icon="ele-FullScreen" size="default" text type="primary" @click="handleFullScreen">
						{{ fullScreen ? "退出全屏" : "全屏" }}
					</el-button>
				</el-header>
				<el-main>
					<widget-portal
						ref="paotal" 
						:name="name"
						:isCopy="isCopy"
						:portal="portal"
						:applets="applets"
						:selectedApplet="selectedApplet"
						:deleteAppletIds="deleteAppletIds"
						@updateSelectd="updateSelectd"
						@deleteApplet="deleteApplet">
					</widget-portal>
				</el-main>
			</el-container>
			<!-- 右侧配置 -->
			<el-aside :width="fullScreen ? '0' : '380px'">
				<el-tabs class="config-container-tabs" v-model="configTab" stretch>
					<el-tab-pane label="区块属性" name="widget">
						<widget-config :applet="selectedApplet" :isCopy="isCopy"></widget-config>
					</el-tab-pane>
					<el-tab-pane label="页面属性" name="form">
						<page-config :portal="portal" :isCopy="isCopy" :api="api"></page-config>
					</el-tab-pane>
				</el-tabs>
			</el-aside>
		</el-container>
	</div>
</template>

<script>
import PageConfig from "@p/components/PageConfig.vue";
import WidgetConfig from "@p/components/WidgetConfig.vue";
import WidgetPortal from "@p/components/WidgetPortal.vue";
import { VueDraggable } from "vue-draggable-plus";

export default {
	name: 'LionPortalDesign',
	components: {
		PageConfig,
		WidgetConfig,
		WidgetPortal,
		VueDraggable
	},
	data() {
		return {
			configTab: "widget",
			fullScreen: false
		}
	},
	props: {
		name: {
			type: String,
			default: ""
		},
		isCopy: {
			type: Boolean,
			default: false
		},
		api: {
			type: Object,
			default: null
		},
		portlets: {
			type: Array,
			default: []
		},
		portal: {
			type: Object,
			default: {}
		},
		applets: {
			type: Array,
			default: []
		},
		selectedApplet: {
			type: Object,
			default: {}
		},
		deleteAppletIds: {
			type: Array,
			default: []
		}
	},
	methods: {
		updateSelectd(applet) {
			this.$emit('updateSelectd', applet);
		},
		deleteApplet(applet, index) {
			this.$emit('deleteApplet', applet, index);
		},
		handleBack() {
			this.$router.back(-1);
		},
		handleSubmit() {
			if (!this.portal.P_Key) {
				this.$message.error("请在右侧的页面属性中填写唯一编码！");
				return;
			}
			if (this.isCopy) {
				if (!this.portal.spOrgId) {
					this.$message.error("复制模式下请选择所属单位！");
					return;
				}
				if (this.portal.P_Key == this.name) {
					this.$message.error("页面的唯一编码与被复制的页面相同！");
					return;
				}
			} else {
				if (this.portal.P_Key != this.name) {
					this.$message.error("页面的唯一编码被篡改，请刷新后重试！");
					return;
				}
			}
			this.$emit('submit');
		},
		handleRegain() {
			this.$confirm("确定要还原初始版本，还原之后不能恢复？", "警告", {
				confirmButtonClass: "is-plain",
				roundButton: true,
				type: "warning",
			}).then(() => {
				this.$emit('regain');
			});
		},
		handleClear() {
			this.$confirm("确定要清空吗？", "警告", {
				type: "warning",
			}).then(() => {
				this.$emit('clearApplet');
			});
		},
		handleFullScreen() {
			this.fullScreen = !this.fullScreen;
		},
		getContentInClipBoard() {
			let str = localStorage.getItem("portlet_copy_str");
			if(str) {
				if(str.indexOf("jcpt_portlet_copy") < 0) {
					this.$message({
						message: "格式错误,请重新复制!",
						type: "warning",
					});
				} else {
					let obj = JSON.parse(str);
					let portlet = obj.portlet;
					let layout = obj.layout;
//					let max = 0;
//					let maxY = 0;
//					let layoutData = widgetPortlet.portal[portalIndex].layout; //当前页面layout
//					let maxHeight = 0;
//					if(layoutData) {
//						for(let i = 0; i < layoutData.length; i++) {
//							// 遍历所选tab页已有的portlet,找到最底位置和最大索引
//							if(parseInt(layoutData[i].i) > max) {
//								max = parseInt(layoutData[i].i);
//							}
//							if(parseInt(layoutData[i].y) > maxY) {
//								maxY = parseInt(layoutData[i].y);
//							}
//						}
//						for(let j = 0; j < layoutData.length; j++) {
//							if(parseInt(layoutData[j].y) == maxY) {
//								if(parseInt(layoutData[j].h) > maxHeight) {
//									maxHeight = parseInt(layoutData[j].h);
//								}
//							}
//						}
//					}
//					const newIndex = max + 1;
//		
//					let cloneportlet = deepClone(portlet);
//					let clonelayout = deepClone(layout);
//					clonelayout.i = newIndex;
//					clonelayout.y = maxY + maxHeight;
//					clonelayout.x = 0;
//					cloneportlet.index = newIndex;
//					cloneportlet.id = Date.now() + "_" + Math.ceil(Math.random() * 99999);
//		
//					portal.layout.push(clonelayout);
//					portal.portlet.push(cloneportlet);
				}
			} else {
				this.$message({
					message: "没有内容,请复制后再粘贴!",
					type: "warning",
				});
			}
		}
	}
}
</script>

<style scoped lang="scss">
.portal-bar {
	background-color: #337ecc;
}

.form-designer {
	background: #fff;
	height: 100%;
}

.form-designer-container {
	height: 100%;
}

.el-tag {
	margin-left: 10px;
	cursor: pointer;
}

.button-new-tag {
	margin-left: 10px;
	height: 32px;
	line-height: 30px;
	padding-top: 0;
	padding-bottom: 0;
}

.input-new-tag {
	width: 90px;
	margin-left: 10px;
	vertical-align: bottom;
}

:deep(.el-drawer__header) {
	-webkit-box-align: center;
	align-items: center;
	color: #72767b;
	display: flex;
	font-weight: 700;
	margin-bottom: 0;
	padding: 10px;
}

.field-title {
	padding: 8px 12px;
	font-size: 14px;
}

.field-title:before {
	content: " ";
	width: 4px;
	height: 14px;
	background: #3385cc;
	display: inline-block;
	vertical-align: middle;
	border-radius: 5px;
	margin-right: 3px;
}

.field-label {
	font-size: 12px;
	display: block;
	width: 48%;
	line-height: 26px;
	float: left;
	overflow: hidden;
	text-overflow: ellipsis;
	white-space: nowrap;
	margin: 1%;
	color: #333;
	border: 1px solid #f4f6fc;
}

.fields-list ul {
	position: relative;
	overflow: hidden;
	padding: 0 10px 10px;
	margin: 0;
	font-size: 16px;
}

.field-label>a {
	display: block;
	cursor: move;
	background: #f4f6fc;
	border: 1px solid #f4f6fc;
}

.field-label>a i {
	margin-left: 4px;
	margin-right: 4px;
	font-size: 14px;
}

.widget-portal-container-header {
	height: 30px;
	line-height: 30px;
	text-align: right;
}

.widget-container-warning {
	color: red;
	font-weight: bold;
	float: left;
}

:deep(.el-button+.el-button) {
	margin-left: 0;
}

:deep(.el-button) {
	padding: 0 15px;
}

:deep(.el-button [class*=el-icon]+span) {
	margin-left: 0px;
}

.config-container-tabs {
	height: 100%;
}

.el-main {
	padding: 5px;
}

.el-main::-webkit-scrollbar {
	width: 0;
}

:deep(.el-tabs__header) {
	margin-bottom: 0;
}

:deep(.el-tabs__content) {
	height: calc(100% - 60px);
	overflow-y: auto;
}

:deep(.el-tabs__content::-webkit-scrollbar) {
	width: 0;
}
</style>
