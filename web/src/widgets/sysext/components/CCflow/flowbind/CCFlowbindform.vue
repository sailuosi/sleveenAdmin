<template>
    <el-form class="gridfm" label-width="110px">
        <div class="fm-toolbar" >
            <el-button @click="save">保存</el-button>
        </div>    
        <fminput v-model="focusRow.fb_Id" label="主键" ></fminput>
        <fminput v-model="focusRow.fb_BizFlow_FK" label="配置外键" ></fminput>
        <fminput v-model="focusRow.fb_Type" label="类别" ></fminput>
        <fminput v-model="focusRow.fb_PK" label="绑定的业务主键" ></fminput>
        <fminput v-model="focusRow.fb_Code" label="绑定编码" ></fminput>
        <fminput v-model="focusRow.fb_Name" label="绑定的分类名" ></fminput>
        <template v-if="!adding">
        
        </template>
    </el-form>
</template>

<script>

import { getCCFlowbindById,addCCFlowbind,updateCCFlowbind } from "@/api/busi/portChannel.js"

import fminput from "@/ui/common/input/fminput.vue"

export default {
    name:"CCFlowbindform",
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
            if(this.focusRow.fb_Id != this.row.fb_Id){
                this.getRowById();
            }
        },
        getRowById(){
            getCCFlowbindById(this.row.fb_Id).then((res)=>{
                this.focusRow=res.data;
            })
        },
        save(){
            var para=[this.focusRow];
            if(this.adding){
                
                addCCFlowbind(para).then((res)=>{
                    this.$message(res.data);
                    this.$emit("close");
                    this.$emit("refresh");
                })
            }
            else{
                updateCCFlowbind(para).then((res)=>{
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