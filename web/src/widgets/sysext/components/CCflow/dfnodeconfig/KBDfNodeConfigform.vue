<template>
    <el-form class="gridfm" label-width="110px">
        <div class="fm-toolbar" >
            <el-button @click="save">保存</el-button>
        </div>    

        <fminput v-model="focusRow.Nc_NodeID" label="流程节点" ></fminput>
        <fminput v-model="focusRow.Nc_NodeName" label="节点名称" ></fminput>
        <fminput v-model="focusRow.Nc_Roles" label="关联的人员角色" ></fminput>
        <fminput v-model="focusRow.Nc_Limit" label="可选个数" ></fminput>

        <fminput v-model="focusRow.Nc_Para" label="选择器参数" ></fminput>
        <fminput v-model="focusRow.Nc_Fallback" label="回退策略" ></fminput>
        <fminput v-model="focusRow.Nc_FixedMan" label="固定的人" ></fminput>
        <fminput v-model="focusRow.Nc_OpRules" label="操作规则" ></fminput>
        <fminput v-model="focusRow.Nc_FixedManName" label="人名" class="gc1-1"></fminput>
        
        <fminput v-model="focusRow.Nc_BindVal" label="绑定值" ></fminput>
        <fminput v-model="focusRow.Nc_Bind" label="绑定类型" ></fminput>
        <fminput v-model="focusRow.Nc_Note" label="说明" class="gc1-1" type="textarea"></fminput>
        <fminput v-model="focusRow.Nc_Remark" label="备注" class="gc1-1"></fminput>

        <template v-if="!adding">

            <fminput v-model="focusRow.Nc_Created" label="创建时间" noedit></fminput>
            <fminput v-model="focusRow.Nc_Updated" label="更新时间" noedit></fminput>
            <fminput v-model="focusRow.NC_CreatedAcount" label="创建人" noedit></fminput>
            <fminput v-model="focusRow.Nc_UpdatedAcount" label="更新人" noedit></fminput>
            <fminput v-model="focusRow.Nc_CreateMan" label="创建人" noedit></fminput>
            <fminput v-model="focusRow.Nc_UpdateMan" label="更新人" noedit></fminput>                 
        </template>
    </el-form>
</template>

<script>
/**
            <fminput v-model="focusRow.Nc_OID" label="主键" noedit></fminput>
            <fminput v-model="focusRow.Nc_DfOID" label="部门流程配置外键" noedit></fminput>   
 */


import { getKBDfNodeConfigById,addKBDfNodeConfig,updateKBDfNodeConfig } from "@s/api/ccflow/KBDfNodeConfigapi.js"

import fminput from "@/ui/common/input/fminput.vue"

export default {
    name:"KBDfNodeConfigform",
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
            if(this.focusRow.Nc_OID != this.row.Nc_OID){
                this.getRowById();
            }
        },
        getRowById(){
            getKBDfNodeConfigById(this.row.Nc_OID).then((res)=>{
                this.focusRow=res.data;
            })
        },
        save(){
            var para=[this.focusRow];
            if(this.adding){
                
                addKBDfNodeConfig(para).then((res)=>{
                    this.$message(res.data);
                    this.$emit("close");
                    this.$emit("refresh");
                })
            }
            else{
                updateKBDfNodeConfig(para).then((res)=>{
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