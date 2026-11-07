<template>
    <el-form class="gridfm" label-width="110px">
        <div class="fm-toolbar" >
            <el-button @click="save">保存</el-button>
        </div>    
        <fminput v-model="focusRow.sprId" label="" ></fminput>
        <fminput v-model="focusRow.PR_Level" label="关联层级" ></fminput>
        <fminput v-model="focusRow.PR_Created" label="创建时间" ></fminput>
        <fminput v-model="focusRow.PR_Man" label="创建人" ></fminput>
        <fminput v-model="focusRow.PR_ManId" label="创建人ID" ></fminput>
        <fminput v-model="focusRow.PR_RleID" label="角色ID" ></fminput>
        <fminput v-model="focusRow.spPortalId" label="门户" ></fminput>
        <fminput v-model="focusRow.spPortCellId" label="门户区块" ></fminput>
        <fminput v-model="focusRow.spclId" label="门户组件" ></fminput>
        <template v-if="!adding">
        
        </template>
    </el-form>
</template>

<script>

import { fetchSysPortalRolePage,createSysPortalRole,updateSysPortalRole } from "@s/api/paotou/paotalapi.js"

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
            if(this.focusRow.sprId != this.row.sprId){
                this.getRowById();
            }
        },
        getRowById(){
            fetchSysPortalRolePage(this.row.sprId).then((res)=>{
                this.focusRow=res.data;
            })
        },
        save(){
            var para=[this.focusRow];
            if(this.adding){
                
                createSysPortalRole(para).then((res)=>{
                    this.$message(res.data);
                    this.$emit("close");
                    this.$emit("refresh");
                })
            }
            else{
                updateSysPortalRole(para).then((res)=>{
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