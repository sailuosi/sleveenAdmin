<template>
<div class="pagelist">
	<div class="pagelist-type" v-for="pose in showPageList" :key="pose.id">
		<div class="title-container">
			<div class="title-bar">
				<div class="title-icon"></div>
				<span>{{ pose.name }}</span>
			</div>
			<div class="divider"></div>
		</div>
		<div class="portalpages">
			<a class="page-button" v-for="page in pose.pages" @click="previewPage(page)" :key="page.sppId">
				<span :class="[page.PP_IsOn ? 'dot-enabled' : 'dot-disabled', 'dot']"></span>
				<span class="page-name">{{ page.PP_OrgName }}</span>
			</a>
		</div>
	</div>
</div>
</template>

<script>
import { PortPageAccessList } from "@s/api/paotou/paotalapi.js";
import { loadCode } from "@/utils/portal/codevalue.js";

export default {
	name: "ptPageview",
	data() {
		return {
			pagePoses: [],
			pageList: []
		};
	},
	computed: {
		showPageList() {
			if (!this.pagePoses || this.pagePoses.length == 0 || !this.pageList || this.pageList.length == 0) {
				return [];
			}
			
			let poseList = [];
			let poseMap = {};
			this.pagePoses.forEach(pose => {
				poseMap[pose.value] = {
					id: pose.value,
					name: pose.label,
					pages: []
				};
				poseList.push(poseMap[pose.value]);
			});
			
			poseMap["other"] = {
				id: "other",
				name: "其它",
				pages: []
			};
			poseList.push(poseMap["other"]);
			
			this.pageList.forEach(page => {
				if (page.PP_Pose && poseMap[page.PP_Pose] && poseMap[page.PP_Pose].id) {
					poseMap[page.PP_Pose].pages.push(page);
				} else {
					poseMap["other"].pages.push(page);
				}
			});
			
			let showPageList = [];
			poseList.forEach(pl => {
				if (pl.pages && pl.pages.length > 0) {
					showPageList.push(pl);
				}
			});
			
			return showPageList;
		}
	},
	mounted() {
		this.loadPagePose();
		this.getPageList();
	},
	methods: {
		loadPagePose() {
			loadCode("KB_PtPagePose").then(res => {
				this.pagePoses = res;
			});
		},
		getPageList() {
			PortPageAccessList({
				pageSize: 200,
				pageNum: 1,
			}).then((res) => {
				this.pageList = res.data;
			});
		},
		previewPage(row) {
			if (!row) {
				this.$message({
					type: "warning",
					message: "未找到页面配置！"
				});
				return;
			}
			
			if (!row.PP_IsOn) {
				this.$message({
					type: "warning",
					message: "页面未启用！"
				});
				return;
			}
			
			let routerUrl = "";
        	if (row.PP_Pose == "1") {//机关部门看板
        		if (!row.PP_Router) {
        			this.$message({
						type: "warning",
						message: "未配置路由地址，无法预览！"
					});
					return;
        		}
				routerUrl = row.PP_Router;
			} else {
				if (!row.PP_Code) {
					this.$message({
						type: "warning",
						message: "未配置页面编码，无法预览！"
					});
					return;
				}
				routerUrl = "/top/general/orglv1/watchpan/" + row.PP_Code;
			}
        	
        	var pageRouter = this.$router.resolve({
				path: routerUrl
			});
			window.open(pageRouter.href, '_blank');
		}
	}
}
</script>

<style lang="scss" scoped>
.pagelist {
	height: 100%;
	width: 100%;
	overflow-y: auto;
	padding: 10px 10px 0 10px;
}

.pagelist-type {
	margin-bottom: 20px;
}

.title-container {
	display: flex;
	flex-direction: column;
}

.title-bar {
	color: #3a7ff0;
	font-size: 24px;
	font-weight: 700;
	display: flex;
	align-items: center;
}

.title-icon {
	width: 5px;
	height: 30px;
	background-color: #3a7ff0;
	margin-right: 10px;
}

.divider {
	height: 2px;
	background-color: #bfdbfe;
	margin-top: 10px;
}

.portalpages {
	display: flex;
	justify-content: flex-start;
	flex-wrap: wrap;
	padding: 20px 0 0 40px;
}

.page-button {
	display: flex;
	justify-content: flex-start;
	align-items: center;
	width: 18%;
	min-width: 280px;
	margin: 10px;
	padding: 12px 20px;
	border-radius: 10px;
	background-color: transparent;
	border: 2px solid #d1d5db;
	color: #000000;
	box-shadow: 0 4px 6px rgba(0, 0, 0, .1);
	transition: all .3s ease;
	text-decoration: none;
	cursor: pointer;
}

.dot {
	flex-shrink: 0;
	height: 15px;
	width: 15px;
	border-radius: 50%;
	margin-right: 10px;
}

.dot-enabled {
	background-color: #1E99DF;
}

.dot-disabled {
	background-color: #AAAAAA;
}

.page-name {
	flex-grow: 1;
	text-align: left;
	font-size: 18px;
}
</style>