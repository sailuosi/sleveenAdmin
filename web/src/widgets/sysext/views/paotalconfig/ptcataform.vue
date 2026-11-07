<template>
    <el-form class="gridfm" label-width="110px">
        <div class="fm-toolbar" >
            <el-button @click="save">保存</el-button>
        </div>    

        
        <fminput v-model="focusRow.AC_Name" label="名称" ></fminput>
        <fminput v-model="focusRow.AC_IsOn" label="启用" type="boolean"></fminput>

        <fminput v-model="focusRow.AC_Note" label="描述" style="grid-column:1/-1;" type="textarea"></fminput>
        
        <fminput v-model="focusRow.AC_Idx" label="序号" ></fminput>
        <fminput v-model="focusRow.cp_params" label="其它参数" style="grid-column:1/-1;" type="textarea"></fminput>  
        <fminput v-model="focusRow.AC_Remark" label="备注" style="grid-column:1/-1;"></fminput>      
        <template v-if="!adding">
            <fminput v-model="focusRow.sacId" label="z主键" noedit></fminput>
            <fminput v-model="focusRow.SYS_Created" label="创建日期" noedit></fminput>
            <fminput v-model="focusRow.SYS_LAST_UPD" label="最后修改日期" noedit></fminput>
            <fminput v-model="focusRow.SYS_Deleted" label="记录删除标记" noedit></fminput>
            <fminput v-model="focusRow.SYS_CreatedBy" label="创建用户" noedit></fminput>
            <fminput v-model="focusRow.SYS_REPLACEMENT" label="授权用户" noedit></fminput>
            <fminput v-model="focusRow.SYS_POSTN" label="所属岗位" noedit></fminput>
            <fminput v-model="focusRow.SYS_DIVISION" label="所属部门" noedit></fminput>
            <fminput v-model="focusRow.SYS_ORG" label="所属组织" noedit></fminput>
            <fminput v-model="focusRow.SYS_LAST_UPD_BY" label="最后修改用户" noedit></fminput>
            <fminput v-model="focusRow.saAppletCataId" label="组件分类" noedit></fminput>        
        </template>



    </el-form>
</template>

<script>

import { fetchSysAppletCata, saveSysAppletCata } from "@s/api/paotou/paotalapi.js"

import fminput from "@/ui/common/input/fminput.vue"

export default {
    name:"ptcataform",
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
            if(this.focusRow.sacId != this.row.sacId){
                this.getRowById();
            }
        },
        getRowById(){
            fetchSysAppletCata(this.row.sacId).then((res)=>{
                this.focusRow=res.data;
            })
        },
        save(){
            var para=[this.focusRow];
            if(this.adding){
                
                saveSysAppletCata(para).then((res)=>{
                    this.$message(res.data);
                    this.$emit("close");
                    this.$emit("refresh");
                })
            }
            else{
                saveSysAppletCata(para).then((res)=>{
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