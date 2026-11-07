<template>
<div :style="mstyle" class="paotal-top">
	<div class="widget-portal-table" style="height: 5000px;">
		<VueDraggable filter=".grid-div" class="widget-portal-table__content" v-model="draggList" :group="{ name: 'form' }" ghost-class="ghost" :animation="200" :sort="false" @add="handleWidgetGridAdd" :autoSize="false">
			<div class="grid-div">
				<grid-layout ref="gridLayout" :layout="applets" :auto-size="false" :col-num="portal.colnum*1" :row-height="portal.rowheight*1" :is-draggable="true" :is-resizable="true" :vertical-compact="true" :use-css-transforms="true">
					<div v-for="(applet, a) in applets" :key="a" @click="handleClick(applet)" >
						<grid-item v-bind="applet" :key="a">
							<template #default="p">
								<portlet
						:applet="applet"
						:index="a"
						:sz="p"
						:para="rendPara"
						:root="me"
						:paotou="portal"
					/>
								<el-button class="widget-portal-delete-boutton" 
									v-if="selectedApplet.i == applet.i" type="danger" icon="el-icon-delete" circle size="default"
									@click="handleDeleteApplet(applet, a)" />
							</template>
						</grid-item>
					</div>
				</grid-layout>
			</div>
		</VueDraggable>
	</div>
</div>
</template>

<script>
import { getPortConfig, saveApplets } from "@p/api/design";
import { parUrlPara } from "@/ui/portal/util.js";
import { styleStrToObj, objToStyleStr } from "@/utils/portal/style.js";

import portlet from "@/ui/portal/ui/portlet.vue";
import { VueDraggable } from "vue-draggable-plus";
import { GridLayout, GridItem } from "vue-grid-layout";

export default {
	name: "paotal",
	components: {
		portlet,
		VueDraggable,
		GridLayout,
		GridItem
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
	data() {
		return {
			draggList: [],
			me: this
		}
	},
	computed: {
		mstyle() {
			var tar = this.portal.style;
			var obj = styleStrToObj(tar);
			return obj;
		},
		rendPara() {
			var para = {};
			//解析参数
			if(!!this.portal.para) {
				var urlPara = parUrlPara(this.portal.para);
				para = Object.assign(para, urlPara);
			}
			return para;
		},
		maxIndex() {
			let mi = 0;
			this.applets.forEach(applet => {
				if (mi < applet.i) {
					mi = applet.i;
				}
			});
			return mi;
		}
	},
	mounted() {
		this.appendStyle();
	},
	methods: {
		appendStyle() {
			var myid = "css" + this.name;
			var t = document.getElementById(myid);
			if(t == null || t == void 0) {
				var ele = document.createElement("style");
				ele.innerHTML = ":root{" + objToStyleStr(this.mstyle) + "}";
				ele.id = myid;
				document.getElementsByTagName('head')[0].appendChild(ele);
			} else {
				t.innerHTML = ":root{" + objToStyleStr(this.mstyle) + "}";
			}
		},
		handleClick(applet) {
			if (applet.vclist && applet.vclist.length > 0) {
				applet.vclist.sort((a, b) => {
					a.CL_Idx = a.CL_Idx || 0;
					b.CL_Idx = b.CL_Idx || 0;
					return a.CL_Idx - b.CL_Idx;
				});
				applet.vclist.forEach((vc, index) => {
					vc.CL_Idx = index + 1;
				});
			}
			this.$emit('updateSelectd', applet);
		},
		handleDeleteApplet(applet, index) {
			this.$confirm("删除区块时会删除该区块下所有组件，是否继续？", "警告", {
				confirmButtonClass: "is-plain",
				roundButton: true,
				type: "warning",
			}).then(() => {
				this.$emit('updateSelectd', {});
				this.$emit('deleteApplet', applet, index);
				this.$refs.gridLayout.layoutUpdate();
			});
		},
		addToSelectdApplet(data) {
			if (!Array.isArray(this.selectedApplet.vclist)) {
				this.selectedApplet.vclist = [];
			}
			let maxIndex = 1;
			this.selectedApplet.vclist.forEach(vc => {
				if (vc.CL_Idx > maxIndex) {
					maxIndex = vc.CL_Idx;
				}
			});
			this.selectedApplet.vclist.push({
				oid: "new",
				link: this.selectedApplet.ckey,
				CL_IsOn: 1,
				CL_Idx: maxIndex + 1,
				disabled: "",
				appletId: data.id,
				title: data.name,
				titleAlign: data.A_TitleAlign,
				theme: data.A_Theme,
				type: data.A_Type,
				tools: data.A_Tool,
				appletKey: data.A_Key,
				tcolor: data.A_Color,
				para: data.A_Para,
				titleStyle: data.A_TitleStyle,
				style: data.A_BodyStyle,
				icon: data.A_Icon,
				slot: data.A_Slot,
				paraval: data.A_ParaVal,
				para2: data.A_Para2,
				CL_Remark: data.A_Remark
			});
			this.$refs.gridLayout.layoutUpdate();
		},
		addToApples(data, maxIndex) {
			let applet = {
				ckey: "panel_" + maxIndex,
				css: "",
				mode: "1",
				name: data.name,
				oid: "new",
				style: "",
				theme: "2",
				titleAlign: "2",
				w: Math.floor(this.portal.colnum / 2),
				h: 20,
				x: 0,
				y: 0,
				i: maxIndex,
				PC_Idx: maxIndex,
				PC_IsOn: 1,
				PC_Margin: "",
				PC_Padding: "",
				PC_Remark: "",
//				moved: true,
//				static: false,
				vclist: [{
					oid: "new",
					link: "panel_" + maxIndex,
					CL_IsOn: 1,
					CL_Idx: 1,
					disabled: "",
					appletId: data.id,
					title: data.name,
					titleAlign: data.A_TitleAlign,
					theme: data.A_Theme,
					type: data.A_Type,
					tools: data.A_Tool,
					appletKey: data.A_Key,
					tcolor: data.A_Color,
					para: data.A_Para,
					titleStyle: data.A_TitleStyle,
					style: data.A_BodyStyle,
					icon: data.A_Icon,
					slot: data.A_Slot,
					paraval: data.A_ParaVal,
					para2: data.A_Para2,
					CL_Remark: data.A_Remark
				}]
			};
			this.applets.push(applet);
			this.$emit('updateSelectd', applet);
			this.$refs.gridLayout.layoutUpdate();
		},
		handleWidgetGridAdd(evt) {
			const data = this.draggList[0];
			let maxIndex = this.maxIndex + 1;
			if (this.selectedApplet && this.selectedApplet.vclist && this.selectedApplet.vclist.length > 0) {
				this.$confirm("是否将组件添加至已选中的区块？放弃放入请关闭。", "提示", {
					distinguishCancelAndClose: true,
					confirmButtonClass: "is-plain",
					confirmButtonText: "放入选中区块",
					cancelButtonClass: "is-plain",
					cancelButtonText: "新建区块并放入",
					roundButton: true,
					type: "warning"
				}).then(() => {
					this.addToSelectdApplet(data);
				}).catch(action => {
					if (action === 'cancel') {
						this.addToApples(data, maxIndex);
					}
				});
			} else {
				this.addToApples(data, maxIndex);
			}
		}
	}
}
</script>
<style scoped>
.paotal-top {
	font-size: var(--paotal-cell-font-size, 14px);
}

.grid-pan {}

.grid-applet {
	margin: 5px;
}

.applet-panel {
	height: 100%;
}

.paotal-tool {
	position: fixed;
	right: 40px;
	bottom: 120px;
	background-color: #fff;
	z-index: 100;
}

.pt-save {
	border-radius: 20px;
	padding: 12px 12px;
	width: 40px;
	height: 40px;
	border-radius: 50%;
	display: flex;
	align-items: center;
	justify-content: center;
	font-size: 20px;
	box-shadow: 0 0 6px rgba(0, 0, 0, .12);
	cursor: pointer;
}

.vue-grid-layout {
	background: transparent;
}

.vue-grid-item .resizing {
	opacity: 0.9;
}

.vue-grid-item .static {
	background: #cce;
}

.vue-grid-item .text {
	font-size: 24px;
	text-align: center;
	position: absolute;
	top: 0;
	bottom: 0;
	left: 0;
	right: 0;
	margin: auto;
	height: 100%;
	width: 100%;
}

.vue-grid-item .no-drag {
	height: 100%;
	width: 100%;
}

.vue-grid-item .minMax {
	font-size: 12px;
}

.vue-grid-item .add {
	cursor: pointer;
}

.layoutJSON {
	background: #ddd;
	border: 1px solid black;
	margin-top: 10px;
	padding: 10px;
}

.eventsJSON {
	background: #ddd;
	border: 1px solid black;
	margin-top: 10px;
	padding: 10px;
	height: 100px;
	overflow-y: scroll;
}

.widget-portal-delete-boutton {
	position: absolute;
	right: 10px;
	bottom: 5px;
}

.widget-portal-table:hover {
	background: #ecf8ff;
}

.widget-portal-table__content {
	width: 100%;
	height: 100%;
	outline: 1px dashed #ccc !important;
	outline-offset: -1px;
	min-height: 100px;
	background: -webkit-linear-gradient(top,transparent 40px,#ccc 41px),-webkit-linear-gradient(left,transparent 40px,#ccc 41px);
	background-size: 41px 41px;
}
</style>