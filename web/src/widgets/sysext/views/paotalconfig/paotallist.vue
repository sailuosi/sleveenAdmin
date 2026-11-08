<template>
    <flexpan flex>
        <template #header="p">
            <el-form inline ref="ruleForm" :model="queryParams" label-width="100px" @submit.native.prevent>
                <el-form-item label="名称" prop="P_Name">
                    <el-input v-model="queryParams.P_Name" placeholder="标题" clearable 
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item label="唯一编码" prop="P_Key">
                    <el-input v-model="queryParams.P_Key" placeholder="唯一编码" clearable 
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item>
                    <el-button type="primary" icon="el-icon-search"  @click="getlist">查询</el-button>
                    <el-button type="info" @click="resetQuery">
                        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1024 1024" data-v-ea893728=""><path fill="currentColor" d="M784.512 230.272v-50.56a32 32 0 1 1 64 0v149.056a32 32 0 0 1-32 32H667.52a32 32 0 1 1 0-64h92.992A320 320 0 1 0 524.8 833.152a320 320 0 0 0 320-320h64a384 384 0 0 1-384 384 384 384 0 0 1-384-384 384 384 0 0 1 643.712-282.88z"></path></svg>
                        重置
                    </el-button>
                    <el-button  @click="openAdd">添加</el-button>
                </el-form-item>
            </el-form>  
        </template>
        <template #default="p">
            <div class="m-pan" :style="{height:(p.height-5)+'px'}">
                <div class="left" :style="{height:(p.height-5)+'px'}" v-if="!this.page">
                    <div class="tabbar">
                        包含下级<el-switch v-model="hasChild" @change="changeChild"></el-switch>
                        <el-link @click="saveAll">全部</el-link>
                    </div>
                    <hrtree
                        @nodeclick="treeclick"
                    >

                    </hrtree>
                </div>
                <vclist v-loading="loading" :list="mlist" :total="total" :para="queryParams" 
                    class="right" orderCol :maxLine="1"
                    @query="getlist" :height="(p.height-5)">
                    <el-table-column fixed="left" label="操作" width="170px" v-if="!selecting">
                        <template #default="p">
                            <el-link type="primary" @click="designRow(p.row, false)">设计</el-link>
                            <el-link type="primary" @click="designRow(p.row, true)" style="margin-left:10px;">复制</el-link>
                            <el-link type="primary" @click="openRow(p.row)" style="margin-left:10px;">编辑</el-link>
                            <el-link type="danger" @click="removeRow(p.row)" style="margin-left:10px;">删除</el-link>
                        </template>
                    </el-table-column>
                    <slot></slot>
                    <vccol field="OrgName" caption="单位" width="150px" align="left"></vccol>
                    <vccol field="P_IsOn" caption="启用" width="60px" type="Boolean"></vccol>
                    <vccol field="P_Name" caption="名称" width="250px" align="left"></vccol>
                    <vccol field="P_Key" caption="唯一编码" width="180px" align="left"></vccol>
                    <vccol field="P_Task" caption="业务" type="dict" dict="SysPtTask"></vccol>
                    <vccol field="P_Pulic" caption="共享" width="60px" type="dict" dict="SysShareStatus"></vccol>                
                    <vccol field="P_Note" caption="说明" align="left" min-width="300px"></vccol>
                    <vccol field="P_Theme" caption="主题" width="60px" type="dict" dict="SysPtCellTheme"></vccol>
                    <vccol field="P_Idx" caption="排序号" width="60px"></vccol>
                    <vccol field="P_Group" caption="分组"></vccol>
                    <vccol field="P_ShowTitle" caption="显示标题" type="dict" dict="SysAppletTitle"></vccol>

                    <vccol field="P_Icon" caption="图标"></vccol>
                    <vccol field="P_Css" caption="类"></vccol>
                    <vccol field="P_Display" caption="布局方式" type="dict" dict="SysPtDisplay"></vccol>
                    <vccol field="P_ColNum" caption="列数" width="60px"></vccol>
                    <vccol field="P_Rows" caption="行高" width="60px"></vccol>
                    <vccol field="P_Editable" caption="可拖动" type="Boolean" width="60px"></vccol>
                    <vccol field="P_Remark" caption="备注"></vccol>
                    

                    <vccol field="SYS_Created" caption="创建日期" type="datetime" width="130px"></vccol>
                    <vccol field="SYS_LAST_UPD" caption="最后修改日期" type="datetime" width="130px"></vccol>

                </vclist>            
            </div>

        </template>
        <template #footer="p">
            <el-dialog title="编辑" class="normal" v-model="diaRow" width="70vw" append-to-body @close="diaRow=false">
                <paotalform :row="focusRow" :adding="adding" @close="diaRow=false" @refresh="getlist">
                </paotalform>
            </el-dialog>
        </template>
    </flexpan>
</template>

<script>
/**
	            <vccol field="P_Style" caption="自定义样式"></vccol>
	            <vccol field="SYS_CreatedBy" caption="创建用户"></vccol>
	            <vccol field="SYS_REPLACEMENT" caption="授权用户"></vccol>
	            <vccol field="SYS_POSTN" caption="所属岗位"></vccol>
	            <vccol field="SYS_DIVISION" caption="所属部门"></vccol>
	            <vccol field="SYS_ORG" caption="所属组织"></vccol>
	            <vccol field="SYS_LAST_UPD_BY" caption="最后修改用户"></vccol>
	            <vccol field="spOrgId" caption="组织机构基本信息外键"></vccol>
	            <vccol field="spSubjectId" caption="业务分类"></vccol>
	            <vccol field="spUserId" caption="用户外键"></vccol>
 */
import { fetchSysPortalPage ,deleteSysPortal} from "@s/api/paotou/paotalapi.js"

import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"

import paotalform from "./paotalform.vue"
import hrtree from "@s/views/OrgCommon/org/hrtree.vue"


export default {
    name:"paotallist",
    components: {
        flexpan,vclist,vccol,paotalform,hrtree
    },
    props:{
        page:null,
        selecting: {
        	type: Boolean,
        	default: false
        }
    },
    emits:[],
    data(){
        return {
            mlist:[],
            total:0,  
            queryParams:{
                pageSize:100,
                pageNum:1,
            },
            focusRow:{},
            diaRow:false,//行编辑表单
            adding:false,
            focusHR:null,
            hasChild:true,
            loading:false,
        };
    },
    computed:{
    },
    mounted() {
        if(!!this.page){
            // this.queryParams.spPortPageId=this.page.sppId;
            this.queryParams.spOrgId=this.page.PP_OrgOID;
        }
        this.getlist();
    },
    watch:{
        "page":function(){
            if(!!this.page){
                // this.queryParams.spPortPageId=this.page.sppId;
                this.queryParams.spOrgId=this.page.PP_OrgOID;
            }
            this.getlist();
        }
    },
    methods:{
        saveAll(){
            this.focusHR=null;
        },
        treeclick(tar){
            this.focusHR=tar;
            this.changeChild();
        },
        changeChild(){
            if(this.focusHR != null){
                if(this.hasChild){
                    this.queryParams.spOrgId=this.focusHR.Varchar1;
                }
                else{
                    this.queryParams.spOrgId=this.focusHR.HR_OrganizeOID;
                }
                this.getlist();
            }
        },
        getlist(){
            // console.log(this.page, "paotallist--page");
            // if(!!this.page){
            //     // this.queryParams.spPortPageId=this.page.sppId;
            //     this.queryParams.spOrgId=this.page.PP_OrgOID;
            // }
            // console.log(this.queryParams, "paotallist--getlist--queryParams");
            this.loading=true;
            fetchSysPortalPage(this.queryParams).then((res)=>{
                this.mlist=res.data;
                this.total=res.total;
                this.loading=false;
            })
        },
        resetQuery(){
            this.queryParams={
                pageSize:30,
                pageNum:1,
            };
            if(!!this.page){
                // this.queryParams.spPortPageId=this.page.sppId;
                this.queryParams.spOrgId=this.page.PP_OrgOID;
            }
            this.getlist();
        },
        openAdd(){
            this.adding=true;
            this.focusRow={
                //FK_channel:this.channel.pc_id,
                P_Display:"1",
                P_IsOn:true,
                P_Name:"新建门户",
                P_Theme:"1",
                P_ShowTitle:"1",
                P_ColNum:24,
                P_Rows:1,
                P_Editable:false,
            };
            if(!!this.page){
                this.focusRow.spPortPageId=this.page.sppId;
            }
            this.diaRow=true;
        },
        designRow(row, isCopy) {
            // console.log(row, "paotallist--designRow");
            this.$router.push({
                path: "/portal/design",
                query: {
                    name: row.P_Key,
                    isCopy: isCopy
                }
            });
        },
        openRow(row){
            this.adding=false;
            // console.log(row, "paotallist--openRow");
            this.focusRow=row;
            this.diaRow=true;
        },
        removeRow(row){
            this.$confirm('此操作将永久删除该记录, 并删除所有子单元格和关联组件, 是否继续?', '提示', {confirmButtonText: '确定',cancelButtonText: '取消',type: 'warning'})
            .then(() => {
                // deleteSysPortalPage(row).then((res)=>{
                deleteSysPortal(row.spId || row.SpId).then((res)=>{
                    this.$message({ type: 'success', message: res || '删除成功'});
                    this.getlist();
                })
                
            })
        },
    }
}
</script>


<style scoped>
.m-pan{
    display: flex;
    flex-direction: row;
}
.m-pan>.left{
    overflow:auto;
    flex:0 0 220px;
}
.m-pan>.right{
    width:0;
    flex:1;
}
.tabbar{
    position:sticky;
    top:0px;
    z-index:10;
    background:#fff;
    padding:5px;
}
</style>
