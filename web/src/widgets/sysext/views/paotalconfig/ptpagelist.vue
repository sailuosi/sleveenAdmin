<template>
    <flexpan flex>
        <template #header="p">
            <el-form inline ref="ruleForm" :model="queryParams" label-width="100px" @submit.native.prevent>
                <el-form-item label="标题" prop="PP_Title" style="width: 300px;">
                    <el-input v-model="queryParams.PP_Title" placeholder="标题" clearable 
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item label="单位名称" prop="PP_OrgName" style="width: 300px;">
                    <el-input v-model="queryParams.PP_OrgName" placeholder="单位名称" clearable 
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item>
                    <el-button type="primary" icon="el-icon-search" @click="getlist">查询</el-button>
                    <el-button @click="resetQuery">重置</el-button>
                    <el-button type="primary" @click="openAdd">添加</el-button>
                    <el-button type="primary" @click="setUserDefaultPage">用户默认页面设置</el-button>
                </el-form-item>
            </el-form>  
        </template>
        <template #default="p">
            <vclist v-loading="loading" :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="p.height" orderCol :maxLine="1">
                <el-table-column fixed="left" label="操作" width="170px">
                    <template #default="p">

                        <el-link type="primary" @click="openRow(p.row)" >编辑</el-link>
                        <el-link type="primary" @click="openPortal(p.row)" style="margin-left:10px;">门户</el-link>
                        <el-link type="danger" @click="removeRow(p.row)" style="margin-left:10px;">删除</el-link>
                        <el-link type="primary" @click="previewPage(p.row)" style="margin-left:10px;">预览</el-link>
                        
                    </template>
                </el-table-column>
                <slot></slot>
	            <vccol field="PP_IsOn" caption="启用" type="Boolean" width="60px"></vccol>
	            <vccol field="PP_Title" caption="标题" min-width="250px"></vccol>
	            <vccol field="PP_OrgName" caption="单位名称" width="200px"></vccol>
                <vccol field="PP_Dept" caption="单位编码" width="90px"></vccol>

                <vccol field="PP_Code" caption="编码" min-width="170px"></vccol>
	            <vccol field="PP_Pose" caption="位置" width="110px" dict="KB_PtPagePose" type="dict"></vccol>
	            <vccol field="PP_Theme" caption="主题" width="90px" type="dict" dict="SysPtCellTheme"></vccol>
	            <vccol field="PP_Disabled" caption="禁用" width="90px"></vccol>
	            <vccol field="PP_Style" caption="自定义样式" width="90px"></vccol>
	            <vccol field="PP_Css" caption="自定义" width="90px"></vccol>

	            <vccol field="PP_AuthCode" caption="权限码" width="90px"></vccol>
	            
	            <vccol field="SYS_Created" caption="创建日期" width="130px"></vccol>
	            <vccol field="SYS_LAST_UPD" caption="最后修改日期" width="130px"></vccol>
	            <vccol field="SYS_Deleted" caption="记录删除标记" width="60px"></vccol>
            </vclist>
        </template>
        <template #footer="p">
            <el-dialog title="编辑页面配置" class="normal" v-model="diaRow" width="70vw" append-to-body @close="diaRow=false">
                <el-tabs type="border-card" v-model="activeTab" >
                    <el-tab-pane label="表单" name="form">
                        <div style="height:70vh;">
                            <ptpageform :row="focusRow" :adding="adding" @close="onclosefm" @refresh="getlist">
                            </ptpageform>                           
                        </div>
                 
                    </el-tab-pane>
                    <el-tab-pane label="查询业务" name="searchTransaction" v-if="!adding" lazy>
                        <pageSelectlist :page="focusRow" style="height:70vh;">
                        </pageSelectlist>
                    </el-tab-pane>
                    <el-tab-pane label="关联信息" name="linkInfo" v-if="!adding" lazy>
                        <pgconnectlist :page="focusRow" style="height:70vh;">
                        </pgconnectlist>
                    </el-tab-pane>
                    <el-tab-pane label="直属门户" name="belongHome" v-if="!adding" lazy>
                        <paotallist :page="focusRow" style="height:70vh;">
                        </paotallist>
                    </el-tab-pane>  
                    <el-tab-pane label="已配门户" name="configuredHome" v-if="!adding" lazy>
                        <ptlinkDept :targetfk="focusRow.PP_OrgOID" style="height:70vh;">
                        </ptlinkDept>
                    </el-tab-pane>                     
                </el-tabs>

            </el-dialog>
            <cardialog v-model="seePaotou" size="big">
                <paotalform :row="actPortal"/>
            </cardialog>
            <cardialog v-model="seeDeptfm" label="部门信息编辑" size="big">
                <depttab style="height: 70vh;" :row="curDept" :dept="dept" :adding="addingDept"/>
            </cardialog>
            <cardialog v-model="showUserPageWin" label="用户默认页面设置" size="big">
                <userPageSet style="height: 80vh;"/>
            </cardialog>
        </template>
    </flexpan>
</template>

<script>
/**
<vccol field="sppId" caption=""></vccol>
	            <vccol field="SYS_CreatedBy" caption="创建用户"></vccol>
	            <vccol field="SYS_REPLACEMENT" caption="授权用户"></vccol>
	            <vccol field="SYS_POSTN" caption="所属岗位"></vccol>
	            <vccol field="SYS_DIVISION" caption="所属部门"></vccol>
	            <vccol field="SYS_ORG" caption="所属组织"></vccol>
	            <vccol field="SYS_LAST_UPD_BY" caption="最后修改用户"></vccol>
	            <vccol field="PP_Para" caption="参数"></vccol>
	            <vccol field="PP_Note" caption="说明"></vccol>
                <vccol field="PP_Content" caption="内容"></vccol>
 */
import { fetchSysPortPagePage ,deleteSysPortPage} from "@s/api/paotou/paotalapi.js";

import flexpan from "@/ui/common/flexpan.vue";
import vclist from "@/ui/common/vclist/vclist.vue";
import vccol from "@/ui/common/vclist/vccol.vue";
import cardialog from "@/ui/panels/cardtab/cardialog.vue";
//子表业务
import ptpageform from "./ptpageform.vue";
import paotallist from "./paotallist.vue";
import pageSelectlist from "./tablefield/basis/pageSelectlist.vue";
//门户关联
import pgconnectlist from "./pageconnect/basis/pgconnectlist.vue";
import paotalform from "./paotalform.vue";
import ptlinkDept from "./ptlink/ptlinkDept.vue";
import userPageSet from "./parts/userPageSet.vue";


export default {
    name: "ptPagelist",
    components: {
        flexpan,vclist,vccol,ptpageform,paotallist,pageSelectlist,
        pgconnectlist,
        cardialog,paotalform,ptlinkDept,
        userPageSet
    },
    props: {
        channel:null
    },
    emits: [],
    data(){
        return {
        	loading: false,
            mlist: [],
            total: 0,  
            queryParams: {
                pageSize: 100,
                pageNum: 1
            },
            focusRow: {},
            actPortal: {},
            seePaotou: false,
            diaRow: false,//行编辑表单
            adding: false,
            seeDeptfm: false,
            curDept: {},
            addingDept: false,
            activeTab: "form",
            showUserPageWin: false
        };
    },
    computed: {
        dept() {
            var t = Object.assign({
                deptOID: this.curDept?.KB_DeptInfoOID,
                para:{
                    deptLevel:"center"
                }
            }, this.curDept);
            return t;
        }
    },
    mounted() {
        this.getlist();
    },
    methods:{
        openAddDept(row) {
            this.curDept = row;
            this.addingDept = true;
            this.seeDeptfm = true;
        },
        openDeptfm(row) {
            this.curDept = row;
            this.addingDept = false;
            this.seeDeptfm = true;
        },
        getlist() {
        	this.loading = true;
            fetchSysPortPagePage(this.queryParams).then((res) => {
                this.mlist=res.data;
                this.total=res.total;
            }).finally(() => {
            	this.loading = false;
            });
        },
        resetQuery() {
            this.queryParams = {
                pageSize: 100,
                pageNum: 1
            };
            this.getlist();
        },
        onclosefm() {
            this.diaRow = false;
        },
        openAdd() {
            this.adding = true;
            this.focusRow = {
                PP_IsOn: true,
                PP_Pose: "1",
                PP_Disabled: false
            };
            this.diaRow = true;
            this.activeTab = 'form';
        },
        openRow(row) {
            this.adding = false;
            this.focusRow = row;
            this.diaRow = true;
            this.activeTab = 'form';
        },
        openPortal(row) {
            this.adding = false;
            this.focusRow = row;
            this.actPortal = {
                spId: row.sppId
            };
            this.seePaotou = true;
        },
        removeRow(row) {
            this.$confirm('此操作将永久删除该记录, 是否继续?', '提示', {
            	confirmButtonText: '确定',
            	cancelButtonText: '取消',
            	type: 'warning'
            }).then(() => {
                deleteSysPortPage(row).then((res)=>{
                    this.$message({ type: 'success', message: res.data});
                    this.getlist();
                });
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
        },
        setUserDefaultPage() {
        	this.showUserPageWin = true;
        }
    }
}
</script>
