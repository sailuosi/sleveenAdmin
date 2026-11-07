<template>
<div :style="mstyle" class="paotal-top">
	<div class="widget-portal-table">
		<div v-if="!applets.length" class="canvas-empty" aria-live="polite">
			<strong>从一个组件开始</strong>
			<span>拖拽左侧组件到这里</span>
		</div>
		<VueDraggable filter=".grid-div" class="widget-portal-table__content" v-model="draggList" :group="{ name: 'form' }" ghost-class="ghost" :animation="200" :sort="false" @add="handleWidgetGridAdd" :autoSize="false">
			<div class="grid-div">
				<!-- 对齐 minehub：grid-layout-plus + GridItem 直接子节点，禁止外包 div -->
				<GridLayout
					ref="gridLayout"
					:layout="applets"
					:auto-size="true"
					:col-num="safeColNum"
					:row-height="safeRowHeight"
					:is-draggable="true"
					:is-resizable="true"
					:vertical-compact="true"
					:use-css-transforms="true"
				>
					<GridItem
						v-for="(applet, a) in applets"
						:key="'g_' + applet.i"
						:class="{ 'designer-block--selected': selectedApplet.i == applet.i }"
						:x="Number(applet.x) || 0"
						:y="Number(applet.y) || 0"
						:w="Math.max(1, Number(applet.w) || 1)"
						:h="Math.max(1, Number(applet.h) || 1)"
						:i="applet.i"
						@click="handleClick(applet)"
					>
						<portlet
							:applet="applet"
							:index="a"
							:para="rendPara"
							:root="me"
							:paotou="portal"
						/>
						<el-button
							class="widget-portal-delete-boutton"
							v-if="selectedApplet.i == applet.i"
							type="danger"
							circle
							size="default"
							@pointerdown.stop
							@click.stop="handleDeleteApplet(applet, a)"
						/>
					</GridItem>
				</GridLayout>
			</div>
		</VueDraggable>
	</div>
</div>
</template>

<script>
import { parUrlPara } from "@/ui/portal/util.js";
import { styleStrToObj, objToStyleStr } from "@/utils/portal/style.js";

import portlet from "@/ui/portal/ui/portlet.vue";
import { VueDraggable } from "vue-draggable-plus";
// vue-grid-layout@3 beta 命名导出损坏，对齐 minehub 使用 grid-layout-plus
import { GridLayout, GridItem } from "grid-layout-plus";

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
		safeColNum() {
			const n = Number(this.portal?.colnum);
			return Number.isFinite(n) && n > 0 ? n : 24;
		},
		safeRowHeight() {
			const n = Number(this.portal?.rowheight);
			return Number.isFinite(n) && n > 0 ? n : 30;
		},
		rendPara() {
			var para = {};
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
		this.$nextTick(() => {
			try { window.dispatchEvent(new Event("resize")); } catch (e) {}
		});
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
				this.$refs.gridLayout?.layoutUpdate?.();
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
			this.$refs.gridLayout?.layoutUpdate?.();
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
				w: Math.floor(this.safeColNum / 2),
				h: 20,
				x: 0,
				y: 0,
				i: maxIndex,
				PC_Idx: maxIndex,
				PC_IsOn: 1,
				PC_Margin: "",
				PC_Padding: "",
				PC_Remark: "",
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
			this.$refs.gridLayout?.layoutUpdate?.();
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
	min-height: 100%;
	box-sizing: border-box;
}

.applet-panel {
	height: 100%;
}

.widget-portal-delete-boutton {
	position: absolute;
	right: 10px;
	bottom: 5px;
	z-index: 10;
}

.widget-portal-table {
	position: relative;
	min-height: max(480px, calc(100vh - 260px));
}

.widget-portal-table__content {
	width: 100%;
	min-height: max(480px, calc(100vh - 260px));
	outline: 1px dashed #a5b9d3;
	outline-offset: -1px;
	border-radius: 12px;
	background-image: linear-gradient(rgb(47 137 245 / 4%) 1px, transparent 1px), linear-gradient(90deg, rgb(47 137 245 / 4%) 1px, transparent 1px);
	background-size: 24px 24px;
}

.canvas-empty {
	position: absolute;
	inset: 0;
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	gap: 12px;
	padding: 24px;
	box-sizing: border-box;
	pointer-events: none;
	text-align: center;
	color: #64758c;
	z-index: 1;
}

.canvas-empty strong {
	font-size: 18px;
	font-weight: 600;
	color: #435975;
}

.canvas-empty span {
	max-width: 290px;
	font-size: 13px;
	line-height: 24px;
}

:deep(.vgl-item),
:deep(.vue-grid-item) {
	border-radius: 12px;
	outline: 1px solid #dce7f5;
	outline-offset: -1px;
	background: #fff;
	box-shadow: 0 2px 6px rgb(35 50 67 / 4%);
	overflow: hidden;
}

:deep(.vgl-item.designer-block--selected),
:deep(.vue-grid-item.designer-block--selected) {
	outline: 2px solid #2f89f5;
	box-shadow: 0 0 0 3px rgb(47 137 245 / 10%);
}
</style>
