<template>
    <el-form class="gridfm" label-width="110px">
        <div class="fm-toolbar" >
            <el-button @click="save">保存</el-button>
        </div>    
        
        
        <fminput v-model="focusRow.bf_name" label="业务名" ></fminput>
        <fminput v-model="focusRow.bf_table" label="业务表" ></fminput>
        
        <fminput v-model="focusRow.bf_code" label="业务编码" type="dict" dict="HH_CCflowTask"></fminput>
        <fminput v-model="focusRow.bf_flowNo" label="流程编号" ></fminput>
        <fminput v-model="focusRow.bf_flowName" label="流程名称" ></fminput>

        
        <fminput v-model="focusRow.bf_pk" label="主键字段" ></fminput>
        
        <fminput v-model="focusRow.bf_appType" label="业务回写字段" ></fminput>
   
        <fminput v-model="focusRow.bf_FlowNoField" label="流程编号字段" ></fminput>
        <fminput v-model="focusRow.bf_WorkIDField" label="实例字段" ></fminput>


        <fminput v-model="focusRow.bf_FlowProgField" label="流程进度字段" ></fminput>
        <fminput v-model="focusRow.bf_StartFlagField" label="启动标记字段" ></fminput>

        <fminput v-model="focusRow.bf_rdt" label="流转日期字段" ></fminput>
        <fminput v-model="focusRow.bf_step" label="步骤回写字段" ></fminput>
        <fminput v-model="focusRow.bf_wfstate" label="小状态回写字段" ></fminput>
        <fminput v-model="focusRow.bf_wfstat" label="大状态回写字段" ></fminput>
        <fminput v-model="focusRow.bf_FlowNameField" label="流程名字段" ></fminput>    
        <fminput v-model="focusRow.bf_NodeIDField" label="节点字段" ></fminput>
        <fminput v-model="focusRow.bf_Idx" label="排序" type="number"></fminput>
        <fminput v-model="focusRow.bf_state" label="状态" ></fminput>
        <fminput v-model="focusRow.bf_nodeCheck" label="是否可选节点" type="boolean"></fminput>
        <fminput v-model="focusRow.bf_nodeIds" label="可选的节点" ></fminput>
        <fminput v-model="focusRow.bf_manCheck" label="是否可选人"  type="boolean"></fminput>
        <fminput v-model="focusRow.bf_draftable" label="是否可存为草稿"  type="boolean"></fminput>
        <fminput v-model="focusRow.bf_pretip" label="启动流程的提示" class="gc1-1"></fminput>
        <fminput v-model="focusRow.bf_dotip" label="流转说明" class="gc1-1"></fminput>
        <fminput v-model="focusRow.bf_note" label="说明" class="gc1-1"></fminput>
        <fminput v-model="focusRow.bf_nextact" label="启动后的动作" ></fminput>
        <fminput v-model="focusRow.bf_nextnote" label="启动后的内容" ></fminput>

        <fminput v-model="focusRow.bf_condi" label="业务条件1" ></fminput>
        <fminput v-model="focusRow.bf_lv" label="业务条件2" ></fminput>
        <fminput v-model="focusRow.bf_orgCode" label="单位code" ></fminput>



        <template v-if="!adding">

            <fminput v-model="focusRow.ZH_Org_FK" label="单位外键" noedit></fminput>
            <fminput v-model="focusRow.ZH_Subject_FK" label="业务分类" ></fminput>
            <fminput v-model="focusRow.bf_Dept_FK" label="部门外键" ></fminput>
            <fminput v-model="focusRow.bf_has" label="包含下级" ></fminput>   
            <fminput v-model="focusRow.SYS_Created" label="创建日期" disabled></fminput>
            <fminput v-model="focusRow.SYS_LAST_UPD" label="最后修改日期" disabled></fminput>
            <fminput v-model="focusRow.SYS_Deleted" label="记录删除标记" disabled></fminput> 
            <fminput v-model="focusRow.C_bizflowOID" label="主键" noedit></fminput> 
        </template>
    </el-form>
</template>

<script>
/**
 <fminput v-model="focusRow.C_bizflowOID" label="" ></fminput>
            <fminput v-model="focusRow.SYS_CreatedBy" label="创建用户" noedit></fminput>
            <fminput v-model="focusRow.SYS_REPLACEMENT" label="授权用户" noedit></fminput>
            <fminput v-model="focusRow.SYS_POSTN" label="所属岗位" noedit></fminput>
            <fminput v-model="focusRow.SYS_DIVISION" label="所属部门" noedit></fminput>
            <fminput v-model="focusRow.SYS_ORG" label="所属组织" noedit></fminput>
            <fminput v-model="focusRow.SYS_LAST_UPD_BY" label="最后修改用户" noedit></fminput> 
 */

import { getCCBizFlowById,addCCBizFlow,updateCCBizFlow } from "@s/api/ccflow/CCBizFlowapi.js"

import fminput from "@/ui/common/input/fminput.vue"

import { ElForm,ElFormItem,ElLink,ElInput,ElButton } from 'element-plus'

export default {
    name:"CCBizFlowform",
    components: {
         ElForm,ElFormItem,ElLink,ElInput,ElButton,
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
            if(this.focusRow.C_bizflowOID != this.row.C_bizflowOID){
                this.getRowById();
            }
        },
        getRowById(){
            getCCBizFlowById(this.row.C_bizflowOID).then((res)=>{
                this.focusRow=res.data;
            })
        },
        save(){
            var para=[this.focusRow];
            if(this.adding){
                
                addCCBizFlow(para).then((res)=>{
                    this.$message(res.data);
                    this.$emit("close");
                    this.$emit("refresh");
                })
            }
            else{
                updateCCBizFlow(para).then((res)=>{
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