<template>
    <el-form class="gridfm" label-width="110px">
        <div class="fm-toolbar" >
            <el-button @click="save">保存</el-button>
        </div>    


        <fminput v-model="focusRow.Df_Title" label="标题" class="spanrow"></fminput>
        <fminput v-model="focusRow.Df_IsOn" label="启用" type="boolean"></fminput>
        <el-form-item label="所属单位" label-width="110px" prop="OrgName" class="fminput">
            <hrselect max-height="50vh" v-model="focusRow.Df_DeptOID" @select="setDept">
                <el-input v-model="focusRow.Df_DeptName" placeholder="请选择单位"  :disabled="disabled"></el-input>
            </hrselect>
        </el-form-item>
        <fminput v-model="focusRow.Df_Flow" label="关联的流程" ></fminput>

        <fminput v-model="focusRow.Df_Task" label="关联的业务" type="dict" dict="KB_CCflowTask"></fminput>
        <fminput v-model="focusRow.Df_FlowName" label="流程名称" ></fminput>
        <fminput v-model="focusRow.Df_CustTitle" label="是否自定义标题" type="boolean"></fminput>      
        <fminput v-model="focusRow.Df_DeptOID" label="关联的部门" ></fminput>
        <fminput v-model="focusRow.Df_DeptName" label="关联的部门名称" ></fminput>
        <fminput v-model="focusRow.Df_Idx" label="排序号" type="number"></fminput>
        <fminput v-model="focusRow.Df_NodeSelect" label="可选节点" ></fminput>
        <fminput v-model="focusRow.Df_NodeIDs" label="可选的节点" ></fminput>
        <fminput v-model="focusRow.Df_ToMan" label="接受人控制" type="dict" dict="KBFlowToMan"></fminput>
        <fminput v-model="focusRow.Df_ToManFix" label="固定的接受人" ></fminput>
        <fminput v-model="focusRow.Df_ToManNames" label="固定的接受人名称" ></fminput>
        <fminput v-model="focusRow.Df_Note" label="流转说明" class="spanrow"></fminput>
        <fminput v-model="focusRow.Df_Para1" label="额外参数1" class="spanrow"></fminput>
        <fminput v-model="focusRow.Df_Para2" label="额外参数2" class="spanrow"></fminput>
        <fminput v-model="focusRow.Df_Type_FK" label="绑定的分类" ></fminput>
        <el-form-item label="分类" prop="TT_Name" class="fminput">
            <ttypeselect max-height="50vh" v-model="focusRow.Df_Type_FK" 
                @select="setScope" :deptOID="focusRow.Df_DeptOID">
                <el-input :model-value="focusRow.TT_Name" placeholder="分类选择..." style="width: calc(100% - 120px)" :disabled="disabled"
                    
                ></el-input>
            </ttypeselect>
        </el-form-item>
        <template v-if="!adding">
            <fminput v-model="focusRow.Df_Created" label="创建时间" noedit></fminput>
            <fminput v-model="focusRow.Df_Updated" label="更新时间" noedit></fminput>
            <fminput v-model="focusRow.Df_CreatedAcount" label="创建人" noedit></fminput>
            <fminput v-model="focusRow.Df_UpdatedAcount" label="更新人" noedit></fminput>
            <fminput v-model="focusRow.Df_CreateMan" label="创建人" noedit></fminput>
            <fminput v-model="focusRow.Df_UpdateMan" label="更新人" noedit></fminput>
        </template>
    </el-form>
</template>

<script>
/**
<fminput v-model="focusRow.Df_OID" label="主键" ></fminput>
 */

import { getKBDeptFlowConfigById,addKBDeptFlowConfig,updateKBDeptFlowConfig } from "@s/api/ccflow/KBDeptFlowConfigapi.js"

import fminput from "@/ui/common/input/fminput.vue"
import hrselect from "@s/views/org/hrselect.vue"
//import ttypeselect from "@s/views/HR/TaskType/TaskTypeSelect.vue"

export default {
    name:"cnPageForm",
    components: {
        fminput,hrselect//,ttypeselect
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
        setDept(tar) {
            this.focusRow.Df_DeptName = tar.OrgName;
        },
        setScope(tar) {
            //console.log(tar);
            this.focusRow.TT_Name = tar.TT_Name;
            this.focusRow.Df_Type_FK = tar.KB_TaskTypeOID;
            //this.focusRow.KB_Type = tar.TT_No;
        },
        load(){
            if(this.adding){
                this.focusRow=this.row;
                return ;
            }
            if(this.focusRow.Df_OID != this.row.Df_OID){
                this.getRowById();
            }
        },
        getRowById(){
            getKBDeptFlowConfigById(this.row.Df_OID).then((res)=>{
                this.focusRow=res.data;
            })
        },
        save(){
            var para=[this.focusRow];
            if(this.adding){
                
                addKBDeptFlowConfig(para).then((res)=>{
                    this.$message(res.data);
                    this.$emit("close");
                    this.$emit("refresh");
                })
            }
            else{
                updateKBDeptFlowConfig(para).then((res)=>{
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
.spanrow{
    grid-column:1/-1;
}
</style>