<template>
    <el-form class="gridfm" label-width="110px">
        <div class="fm-toolbar" >
            <el-button @click="save">保存</el-button>
        </div>    
        <fminput v-model="focusRow.Vi_Name" label="名称" class="spanrow"></fminput>        
        <fminput v-model="focusRow.Vi_IsOn" label="启用" type="boolean"></fminput>
        <fminput v-model="focusRow.Vi_Type" label="类型" type="dict" dict="HH_VisitorType"></fminput>

        <fminput v-model="focusRow.Vi_Code" label="编号" ></fminput>
        <fminput v-model="focusRow.Vi_OID" label="OID" ></fminput>
        <fminput v-model="focusRow.Vi_Src" label="来源" ></fminput>
        <fminput v-model="focusRow.Vi_Mode" label="方式" ></fminput>
        <fminput v-model="focusRow.Vi_Idx" label="序号" ></fminput>
        
        <template v-if="!adding">
            <fminput v-model="focusRow.HH_VisitorItemOID" label="主键" noedit></fminput>
            <fminput v-model="focusRow.HH_VisitorBag_FK" label="访客集合" noedit></fminput>
        </template>
    </el-form>
</template>

<script>
/**

 */


import { getHHVisitorItemById,addHHVisitorItem,updateHHVisitorItem } from "../../../api/HHVisitorItemapi.js"

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
            if(this.focusRow.HH_VisitorItemOID != this.row.HH_VisitorItemOID){
                this.getRowById();
            }
        },
        getRowById(){
            getHHVisitorItemById(this.row.HH_VisitorItemOID).then((res)=>{
                this.focusRow=res.data;
            })
        },
        save(){
            var para=[this.focusRow];
            if(this.adding){
                
                addHHVisitorItem(para).then((res)=>{
                    this.$message(res.data);
                    this.$emit("close");
                    this.$emit("refresh");
                })
            }
            else{
                updateHHVisitorItem(para).then((res)=>{
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