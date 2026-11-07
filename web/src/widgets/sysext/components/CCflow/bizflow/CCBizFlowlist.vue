<template>
    <flexpan flex>
        <template #header="p">
            <el-form inline ref="ruleForm" :model="queryParams" label-width="100px" @submit.native.prevent>
                <el-form-item label="标题" prop="title">
                    <el-input v-model="queryParams.title" placeholder="标题" clearable 
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item label="流程" prop="FlowName">
                    <el-input v-model="queryParams.FlowName" placeholder="流程名称" clearable 
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item>
                    <el-button type="primary" icon="el-icon-search"  @click="getlist">查询</el-button>
                    <el-button  @click="resetQuery">重置</el-button>
                    <el-button  @click="openAdd">添加</el-button>
                    <el-button  @click="openCopy">复制</el-button>
                </el-form-item>
            </el-form>  
        </template>
        <template #default="p">
            <vclist :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="p.height"
                @current-change="rowchanged"
            >
                <el-table-column    fixed="left" label="操作" width="110px">
                    <template #default="p">
                        <el-link type="primary" @click="openRow(p.row)" >编辑</el-link>
                        <el-link type="danger" @click="removeRow(p.row)" style="margin-left:10px;">删除</el-link>
                    </template>
                </el-table-column>                
                <slot></slot>

	            <vccol field="bf_table" caption="业务表" width="150px"></vccol>
	            <vccol field="bf_name" caption="业务名" width="150px"></vccol>
	            <vccol field="bf_code" caption="业务编码" type="dict" dict="KB_CCflowTask"></vccol>

	            <vccol field="bf_step" caption="步骤回写字段" width="150px"></vccol>
	            <vccol field="bf_appType" caption="业务回写字段" width="150px"></vccol>
	            <vccol field="bf_wfstate" caption="小状态回写字段" width="150px"></vccol>
	            <vccol field="bf_wfstat" caption="大状态回写字段" width="150px"></vccol>
                <vccol field="bf_FlowNoField" caption="流程编号字段" width="150px"></vccol>
                <vccol field="bf_WorkIDField" caption="实例字段" width="150px"></vccol>
                <vccol field="bf_NodeIDField" caption="节点字段" width="150px"></vccol>
                <vccol field="bf_FlowNameField" caption="流程名字段" width="150px"></vccol>
                <vccol field="bf_FlowProgField" caption="流程进度字段" width="150px"></vccol>   
                <vccol field="bf_StartFlagField" caption="启动标记字段" width="150px"></vccol>   

	            <vccol field="bf_flowNo" caption="流程编号"></vccol>
	            <vccol field="bf_flowName" caption="流程名称" width="250px"></vccol>
	            <vccol field="bf_Idx" caption="排序"></vccol>
	            <vccol field="bf_state" caption="状态"></vccol>



	            <vccol field="bf_condi" caption="业务条件1"></vccol>
	            <vccol field="bf_lv" caption="业务条件2"></vccol>
	            <vccol field="bf_orgCode" caption="单位code"></vccol>

	            <vccol field="bf_has" caption="包含下级"></vccol>
	            <vccol field="SYS_Created" caption="创建日期" width="170px"></vccol>
	            <vccol field="SYS_LAST_UPD" caption="最后修改日期" width="170px"></vccol>                
            </vclist>
        </template>
        <template #footer="p">
            <el-dialog title="编辑" class="normal" v-model="diaRow" width="70vw" append-to-body @close="diaRow=false">
                <CCBizFlowform :row="focusRow" :adding="adding" @close="onclosefm" @refresh="getlist">
                </CCBizFlowform>
            </el-dialog>
        </template>
    </flexpan>
</template>

<script>
/*
	            <vccol field="bf_nodeCheck" caption="是否可选节点"></vccol>
	            <vccol field="bf_nodeIds" caption="可选的节点" type="Boolean"></vccol>
	            <vccol field="bf_manCheck" caption="是否可选人"></vccol>
	            <vccol field="bf_draftable" caption="是否可存为草稿"></vccol>
	            <vccol field="bf_pretip" caption="启动流程的提示"></vccol>
	            <vccol field="bf_dotip" caption="流转说明"></vccol>
	            <vccol field="bf_note" caption="说明"></vccol>
	            <vccol field="bf_nextact" caption="启动后的动作"></vccol>
	            <vccol field="bf_nextnote" caption="启动后的内容"></vccol>
	            <vccol field="C_bizflowOID" caption=""></vccol>
	            <vccol field="SYS_CreatedBy" caption="创建用户"></vccol>
	            <vccol field="SYS_REPLACEMENT" caption="授权用户"></vccol>
	            <vccol field="SYS_POSTN" caption="所属岗位"></vccol>
	            <vccol field="SYS_DIVISION" caption="所属部门"></vccol>
	            <vccol field="SYS_ORG" caption="所属组织"></vccol>
	            <vccol field="SYS_LAST_UPD_BY" caption="最后修改用户"></vccol>
	            <vccol field="ZH_Org_FK" caption="单位外键"></vccol>
	            <vccol field="ZH_Subject_FK" caption="业务分类"></vccol>
	            <vccol field="bf_Dept_FK" caption="部门外键"></vccol>   
	            <vccol field="SYS_Deleted" caption="记录删除标记"></vccol>                             
*/

import { selectCCBizFlowlist ,removeCCBizFlow} from "@s/api/ccflow/CCBizFlowapi.js"

import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"

import CCBizFlowform from "./CCBizFlowform.vue"



import { ElForm,ElFormItem,ElLink,ElInput } from 'element-plus'

export default {
    name:"cnPagelist",
    components: {
        ElForm,ElFormItem,ElLink,ElInput,
        flexpan,vclist,vccol,CCBizFlowform
    },
    props:{
        channel:null,
    },
    emits:[],
    data(){
        return {
            mlist:[],
            total:0,  
            queryParams:{
                pageSize:30,
                pageNum:1,
            },
            focusRow:{},
            diaRow:false,//行编辑表单
            adding:false,
            listCur:null,
        };
    },
    computed:{
        listpara(){
            var obj=Object.assign({

            },this.queryParams);
			return obj;
        },
    },
    watch:{
        "listpara":function(){
            this.getlist();
        }
    },
    mounted() {

        this.getlist();
    },
    methods:{
        getlist(){
           
            selectCCBizFlowlist(this.listpara).then((res)=>{
                this.mlist=res.data;
                this.total=res.total;
            })
        },
        resetQuery(){
            this.queryParams={
                pageSize:30,
                pageNum:1,
            };
        },
        onclosefm(){
            this.diaRow=false;
        },
        rowchanged(e){
            this.listCur=e;
        },
        openAdd(){
            this.adding=true;
            this.focusRow={
                //FK_channel:this.channel.pc_id,
            };
            this.diaRow=true;
        },
        openCopy(){
            var newone= Object.assign({},this.listCur);
            newone.C_bizflowOID=null;
            this.focusRow=newone;
            this.adding=true;
            this.diaRow=true;
        },
        openRow(row){
            this.adding=false;
            this.focusRow=row;
            this.diaRow=true;
        },
        removeRow(row){
            this.$confirm('此操作将永久删除该记录, 是否继续?', '提示', {confirmButtonText: '确定',cancelButtonText: '取消',type: 'warning'})
            .then(() => {
                removeCCBizFlow(row).then((res)=>{
                    this.$message({ type: 'success', message: res.msg});
                    this.getlist();
                })
                
            })
        },
    }
}
</script>