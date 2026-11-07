<template>
    <el-form class="gridfm" label-width="110px">
        <div class="fm-toolbar" >
            <el-button @click="save">保存</el-button>
        </div>    
        <fminput v-model="focusRow.spcnId" label="" ></fminput>
        <fminput v-model="focusRow.PC_IsOn" label="启用" ></fminput>
        <fminput v-model="focusRow.PC_Type" label="关联对象类型" ></fminput>
        <fminput v-model="focusRow.PC_TargetOID" label="关联对象OID" ></fminput>
        <fminput v-model="focusRow.PC_TargetNo" label="关联对象编号" ></fminput>
        <fminput v-model="focusRow.PC_TargetName" label="关联对象名称" ></fminput>
        <fminput v-model="focusRow.PC_Note" label="说明" ></fminput>
        <fminput v-model="focusRow.PC_InSrc" label="添加方式" ></fminput>
        <fminput v-model="focusRow.PC_Created" label="添加时间" ></fminput>
        <fminput v-model="focusRow.PC_PageKey" label="页面识别码" ></fminput>
        <fminput v-model="focusRow.spPortPageId" label="门户页面" ></fminput>
        <template v-if="!adding">
        
        </template>
    </el-form>
</template>

<script>

//import { fetchSysPageConnectPage,saveSysPageConnect,saveSysPageConnect } from "@/api/system/paotou/pageconnectapi.js"

import fminput from "@/ui/common/input/fminput.vue"

export default {
    name:"cnPageForm",
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
            if(this.focusRow.spcnId != this.row.spcnId){
                this.getRowById();
            }
        },
        getRowById(){
            fetchSysPageConnectPage(this.row.spcnId).then((res)=>{
                this.focusRow=res.data;
            })
        },
        save(){
            var para=[this.focusRow];
            if(this.adding){
                
                saveSysPageConnect(para).then((res)=>{
                    this.$message(res.data);
                    this.$emit("close");
                    this.$emit("refresh");
                })
            }
            else{
                saveSysPageConnect(para).then((res)=>{
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