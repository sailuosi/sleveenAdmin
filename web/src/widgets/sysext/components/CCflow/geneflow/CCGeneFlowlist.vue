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
                </el-form-item>
            </el-form>  
        </template>
        <template #default="p">
            <vclist :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="p.height">
                <el-table-column    fixed="left" label="操作" width="110px">
                    <template #default="p">
                        <el-link type="primary" @click="openRow(p.row)" >编辑</el-link>
                        <el-link type="danger" @click="removeRow(p.row)" style="margin-left:10px;">删除</el-link>
                    </template>
                </el-table-column>                
                <slot></slot>
	            <vccol field="C_geneflowOID" caption=""></vccol>
	            <vccol field="f_state" caption="流程状态"></vccol>
	            <vccol field="remark" caption="备注"></vccol>
	            <vccol field="f_msg" caption="消息"></vccol>
	            <vccol field="f_nodeId" caption="节点id"></vccol>
	            <vccol field="f_taskCode" caption="业务编码"></vccol>
	            <vccol field="f_assId" caption="辅助业务id"></vccol>
	            <vccol field="f_flowNo" caption="流程编号"></vccol>
	            <vccol field="f_curUsers" caption="当前办理人"></vccol>
	            <vccol field="SYS_Created" caption="创建日期"></vccol>
	            <vccol field="SYS_LAST_UPD" caption="最后修改日期"></vccol>
	            <vccol field="SYS_Deleted" caption="记录删除标记"></vccol>
	            <vccol field="f_workId" caption="流程实例id"></vccol>
	            <vccol field="f_bizPK" caption="业务主键"></vccol>
	            <vccol field="f_bizId" caption="业务id"></vccol>
	            <vccol field="f_FID" caption="父流程ID"></vccol>
	            <vccol field="f_SysType" caption="系统类型"></vccol>
	            <vccol field="f_FlowName" caption="流程名称"></vccol>
	            <vccol field="f_Title" caption="标题"></vccol>
	            <vccol field="f_WFSta" caption="状态"></vccol>
	            <vccol field="f_WFState" caption="状态"></vccol>
	            <vccol field="f_Starter" caption="发起人"></vccol>
	            <vccol field="f_StarterName" caption="发起人"></vccol>
	            <vccol field="f_Sender" caption="发送人"></vccol>
	            <vccol field="f_RDT" caption="日期"></vccol>
	            <vccol field="f_HungupTime" caption="挂起时间"></vccol>
	            <vccol field="f_SendDT" caption="发送时间"></vccol>
	            <vccol field="f_NodeName" caption="节点名称"></vccol>
	            <vccol field="f_FK_Dept" caption="单位"></vccol>
	            <vccol field="f_DeptName" caption="单位名称"></vccol>
	            <vccol field="f_PRI" caption="优先级"></vccol>
	            <vccol field="f_SDTOfNode" caption="SD节点"></vccol>
	            <vccol field="f_SDTOfFlow" caption="SD流程"></vccol>
	            <vccol field="f_SDTOfFlowWarning" caption="SD提示"></vccol>
	            <vccol field="f_PFlowNo" caption="父流程"></vccol>
	            <vccol field="f_PWorkID" caption="父工作"></vccol>
	            <vccol field="f_PNodeID" caption="父节点"></vccol>
	            <vccol field="f_PFID" caption="父FID"></vccol>
	            <vccol field="f_PEmp" caption="父办理人"></vccol>
	            <vccol field="f_GuestNo" caption="客户编号"></vccol>
	            <vccol field="f_GuestName" caption="客户名称"></vccol>
	            <vccol field="f_BillNo" caption="单据编号"></vccol>
	            <vccol field="f_FlowNote" caption="流程说明"></vccol>
	            <vccol field="f_TodoEmps" caption="待办人"></vccol>
	            <vccol field="f_TodoEmpsNum" caption="待办人数"></vccol>
	            <vccol field="f_TaskSta" caption="任务状态"></vccol>
	            <vccol field="f_AtPara" caption="参数"></vccol>
	            <vccol field="f_Emps" caption="用户"></vccol>
	            <vccol field="f_TodoSta" caption="待办状态"></vccol>
	            <vccol field="f_PrjNo" caption="项目编号"></vccol>
	            <vccol field="f_PrjName" caption="项目名称"></vccol>
	            <vccol field="f_ReadTime" caption="月度时间"></vccol>
	            <vccol field="f_FK_biz" caption="关联业务"></vccol>
	            <vccol field="f_nStep" caption="当前步骤"></vccol>
	            <vccol field="f_nAppType" caption="当前业务类型"></vccol>
	            <vccol field="f_nTip" caption="当前说明"></vccol>
	            <vccol field="f_nFocusField" caption="焦点字段"></vccol>
	            <vccol field="f_bizPKStr" caption="字符主键"></vccol>
	            <vccol field="c_bizflow_FK" caption="c_bizflow"></vccol>
	            <vccol field="SYS_CreatedBy" caption="创建用户"></vccol>
	            <vccol field="SYS_REPLACEMENT" caption="授权用户"></vccol>
	            <vccol field="SYS_POSTN" caption="所属岗位"></vccol>
	            <vccol field="SYS_DIVISION" caption="所属部门"></vccol>
	            <vccol field="SYS_ORG" caption="所属组织"></vccol>
	            <vccol field="SYS_LAST_UPD_BY" caption="最后修改用户"></vccol>
            </vclist>
        </template>
        <template #footer="p">
            <el-dialog title="编辑" class="normal" v-model="diaRow" width="70vw" append-to-body @close="diaRow=false">
                <CCGeneFlowform :row="focusRow" :adding="adding" @close="onclosefm" @refresh="getlist">
                </CCGeneFlowform>
            </el-dialog>
        </template>
    </flexpan>
</template>

<script>

import { selectCCGeneFlowlist ,removeCCGeneFlow} from "@s/api/ccflow/CCGeneFlowapi.js"

import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"

import CCGeneFlowform from "./CCGeneFlowform.vue"

export default {
    name:"cnPagelist",
    components: {
        flexpan,vclist,vccol,CCGeneFlowform
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
           
            selectCCGeneFlowlist(this.listpara).then((res)=>{
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
        openAdd(){
            this.adding=true;
            this.focusRow={
                //FK_channel:this.channel.pc_id,
            };
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
                removeCCGeneFlow(row).then((res)=>{
                    this.$message({ type: 'success', message: res.msg});
                    this.getlist();
                })
                
            })
        },
    }
}
</script>