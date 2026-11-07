<template>
    <el-form class="gridfm" label-width="110px">
        <div class="fm-toolbar" >
            <el-button @click="save">保存</el-button>
        </div>    

        <fminput v-model="focusRow.Di_Name" label="名称" ></fminput>
        <fminput v-model="focusRow.Di_Idx" label="序号" ></fminput>
        <fminput v-model="focusRow.Di_Code" label="编号" ></fminput>
        <fminput v-model="focusRow.Di_Note" label="说明" class="spanrow"></fminput>
        <fminput v-model="focusRow.Di_Remark" label="备注" class="spanrow"></fminput>

        <el-form-item label="资源集合">
            <goodstag v-model="focusRow.HH_GoodsBag_FK"></goodstag>
        </el-form-item>
        <el-form-item label="词条集合">
            <wordtag v-model="focusRow.HH_WordBag_FK"></wordtag>
        </el-form-item>
        <el-form-item label="访客集合">
            <visitortag v-model="focusRow.HH_VisitorBag_FK"></visitortag>
        </el-form-item>
        

        <template v-if="!adding">
            
            <fminput v-model="focusRow.SYS_Created" label="创建日期" noedit></fminput>
            <fminput v-model="focusRow.SYS_LAST_UPD" label="最后修改日期" noedit></fminput>
             

        </template>
    </el-form>
</template>

<script>
/**
        <fminput v-model="focusRow.HH_VisitorBag_FK" label="访客集合" ></fminput>
        <fminput v-model="focusRow.HH_WordBag_FK" label="词条集合" ></fminput>
        <fminput v-model="focusRow.HH_GoodsBag_FK" label="资源集合" ></fminput> * 
 * 
             <fminput v-model="focusRow.SYS_CreatedBy" label="创建用户" noedit></fminput>
            <fminput v-model="focusRow.SYS_REPLACEMENT" label="授权用户" noedit></fminput>
            <fminput v-model="focusRow.SYS_POSTN" label="所属岗位" noedit></fminput>
            <fminput v-model="focusRow.SYS_DIVISION" label="所属部门" noedit></fminput>
            <fminput v-model="focusRow.SYS_ORG" label="所属组织" noedit></fminput>
            <fminput v-model="focusRow.SYS_LAST_UPD_BY" label="最后修改用户" noedit></fminput>
            <fminput v-model="focusRow.HH_DutyItemOID" label="主键" noedit></fminput>
            <fminput v-model="focusRow.SYS_Deleted" label="记录删除标记" noedit></fminput>   
 */
import { getHHDutyItemById,addHHDutyItem,updateHHDutyItem } from "../../api/dutyitemapi.js"

import fminput from "@/ui/common/input/fminput.vue"

import goodstag from "../duty/goodstag.vue"
import visitortag from "../duty/visitortag.vue"
import wordtag from "../duty/wordtag.vue"


export default {
    name:"cnPageForm",
    components: {
        fminput,goodstag,visitortag,wordtag,
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
            if(this.focusRow.HH_DutyItemOID != this.row.HH_DutyItemOID){
                this.getRowById();
            }
        },
        getRowById(){
            getHHDutyItemById(this.row.HH_DutyItemOID).then((res)=>{
                this.focusRow=res.data;
            })
        },
        save(){
            var para=[this.focusRow];
            if(this.adding){
                
                addHHDutyItem(para).then((res)=>{
                    this.$message(res.data);
                    this.$emit("close");
                    this.$emit("refresh");
                })
            }
            else{
                updateHHDutyItem(para).then((res)=>{
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