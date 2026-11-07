<template>
    <el-form class="gridfm" label-width="110px">
        <div class="fm-toolbar" >
            <el-button @click="save">保存</el-button>
        </div>    
        <fminput v-model="focusRow.C_geneflowOID" label="" ></fminput>
        <fminput v-model="focusRow.f_state" label="流程状态" ></fminput>
        <fminput v-model="focusRow.remark" label="备注" ></fminput>
        <fminput v-model="focusRow.f_msg" label="消息" ></fminput>
        <fminput v-model="focusRow.f_nodeId" label="节点id" ></fminput>
        <fminput v-model="focusRow.f_taskCode" label="业务编码" ></fminput>
        <fminput v-model="focusRow.f_assId" label="辅助业务id" ></fminput>
        <fminput v-model="focusRow.f_flowNo" label="流程编号" ></fminput>
        <fminput v-model="focusRow.f_curUsers" label="当前办理人" ></fminput>
        <fminput v-model="focusRow.SYS_Created" label="创建日期" ></fminput>
        <fminput v-model="focusRow.SYS_LAST_UPD" label="最后修改日期" ></fminput>
        <fminput v-model="focusRow.SYS_Deleted" label="记录删除标记" ></fminput>
        <fminput v-model="focusRow.f_workId" label="流程实例id" ></fminput>
        <fminput v-model="focusRow.f_bizPK" label="业务主键" ></fminput>
        <fminput v-model="focusRow.f_bizId" label="业务id" ></fminput>
        <fminput v-model="focusRow.f_FID" label="父流程ID" ></fminput>
        <fminput v-model="focusRow.f_SysType" label="系统类型" ></fminput>
        <fminput v-model="focusRow.f_FlowName" label="流程名称" ></fminput>
        <fminput v-model="focusRow.f_Title" label="标题" ></fminput>
        <fminput v-model="focusRow.f_WFSta" label="状态" ></fminput>
        <fminput v-model="focusRow.f_WFState" label="状态" ></fminput>
        <fminput v-model="focusRow.f_Starter" label="发起人" ></fminput>
        <fminput v-model="focusRow.f_StarterName" label="发起人" ></fminput>
        <fminput v-model="focusRow.f_Sender" label="发送人" ></fminput>
        <fminput v-model="focusRow.f_RDT" label="日期" ></fminput>
        <fminput v-model="focusRow.f_HungupTime" label="挂起时间" ></fminput>
        <fminput v-model="focusRow.f_SendDT" label="发送时间" ></fminput>
        <fminput v-model="focusRow.f_NodeName" label="节点名称" ></fminput>
        <fminput v-model="focusRow.f_FK_Dept" label="单位" ></fminput>
        <fminput v-model="focusRow.f_DeptName" label="单位名称" ></fminput>
        <fminput v-model="focusRow.f_PRI" label="优先级" ></fminput>
        <fminput v-model="focusRow.f_SDTOfNode" label="SD节点" ></fminput>
        <fminput v-model="focusRow.f_SDTOfFlow" label="SD流程" ></fminput>
        <fminput v-model="focusRow.f_SDTOfFlowWarning" label="SD提示" ></fminput>
        <fminput v-model="focusRow.f_PFlowNo" label="父流程" ></fminput>
        <fminput v-model="focusRow.f_PWorkID" label="父工作" ></fminput>
        <fminput v-model="focusRow.f_PNodeID" label="父节点" ></fminput>
        <fminput v-model="focusRow.f_PFID" label="父FID" ></fminput>
        <fminput v-model="focusRow.f_PEmp" label="父办理人" ></fminput>
        <fminput v-model="focusRow.f_GuestNo" label="客户编号" ></fminput>
        <fminput v-model="focusRow.f_GuestName" label="客户名称" ></fminput>
        <fminput v-model="focusRow.f_BillNo" label="单据编号" ></fminput>
        <fminput v-model="focusRow.f_FlowNote" label="流程说明" ></fminput>
        <fminput v-model="focusRow.f_TodoEmps" label="待办人" ></fminput>
        <fminput v-model="focusRow.f_TodoEmpsNum" label="待办人数" ></fminput>
        <fminput v-model="focusRow.f_TaskSta" label="任务状态" ></fminput>
        <fminput v-model="focusRow.f_AtPara" label="参数" ></fminput>
        <fminput v-model="focusRow.f_Emps" label="用户" ></fminput>
        <fminput v-model="focusRow.f_TodoSta" label="待办状态" ></fminput>
        <fminput v-model="focusRow.f_PrjNo" label="项目编号" ></fminput>
        <fminput v-model="focusRow.f_PrjName" label="项目名称" ></fminput>
        <fminput v-model="focusRow.f_ReadTime" label="月度时间" ></fminput>
        <fminput v-model="focusRow.f_FK_biz" label="关联业务" ></fminput>
        <fminput v-model="focusRow.f_nStep" label="当前步骤" ></fminput>
        <fminput v-model="focusRow.f_nAppType" label="当前业务类型" ></fminput>
        <fminput v-model="focusRow.f_nTip" label="当前说明" ></fminput>
        <fminput v-model="focusRow.f_nFocusField" label="焦点字段" ></fminput>
        <fminput v-model="focusRow.f_bizPKStr" label="字符主键" ></fminput>
        <fminput v-model="focusRow.c_bizflow_FK" label="c_bizflow" ></fminput>
        <fminput v-model="focusRow.SYS_CreatedBy" label="创建用户" ></fminput>
        <fminput v-model="focusRow.SYS_REPLACEMENT" label="授权用户" ></fminput>
        <fminput v-model="focusRow.SYS_POSTN" label="所属岗位" ></fminput>
        <fminput v-model="focusRow.SYS_DIVISION" label="所属部门" ></fminput>
        <fminput v-model="focusRow.SYS_ORG" label="所属组织" ></fminput>
        <fminput v-model="focusRow.SYS_LAST_UPD_BY" label="最后修改用户" ></fminput>
        <template v-if="!adding">
        
        </template>
    </el-form>
</template>

<script>

import { getCCGeneFlowById,addCCGeneFlow,updateCCGeneFlow } from "@s/api/ccflow/CCGeneFlowapi.js"

import fminput from "@/ui/common/input/fminput.vue"

export default {
    name:"CCGeneFlowform",
    components: {
        fminput
    },
    props:{
        row:null,
        disabled:{ type:Boolean,default:false },
        adding:{ type:Boolean,default:false },
    },
    emits:[],
    data(){
        return {
            focusRow:{},
            diaRow:false,//行编辑表单
            pageTypes:[
                { label:"url链接",value:"1"},
                { label:"静态路由",value:"2"},
                { label:"已注册vue组件",value:"3"},
            ],
            onoff:[
                { label:"启用",value:"1"},
                { label:"停用",value:"0"},
            ],
        };
    },
    computed:{
    },
    mounted() {
        //this.getlist();
         this.load();
    },
    watch:{
        "row":function(val){
            this.load();
        },
    },    
    methods:{
        load(){
            if(this.adding){
                this.focusRow=this.row;
                return ;
            }
            if(this.focusRow.C_geneflowOID != this.row.C_geneflowOID){
                this.getRowById();
            }
        },
        getRowById(){
            getCCGeneFlowById(this.row.C_geneflowOID).then((res)=>{
                this.focusRow=res.data;
            })
        },
        save(){
            var para=[this.focusRow];
            if(this.adding){
                
                addCCGeneFlow(para).then((res)=>{
                    this.$message(res.data);
                    this.$emit("close");
                    this.$emit("refresh");
                })
            }
            else{
                updateCCGeneFlow(para).then((res)=>{
                    this.$message(res.data);
                    this.$emit("close");
                    this.$emit("refresh");
                })
            }
        },
    }
}
</script>

<style scoped>
.gridfm{
    display:grid;
    grid-template-columns:1fr 1fr;
}
.fm-toolbar{
    grid-column:1/-1;
    margin-bottom:15px;
}
</style>