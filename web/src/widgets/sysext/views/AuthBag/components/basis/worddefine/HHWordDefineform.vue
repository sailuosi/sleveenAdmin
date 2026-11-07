<template>
    <el-form class="gridfm" label-width="110px">
        <div class="fm-toolbar" >
            <el-button @click="save">保存</el-button>
        </div>    
        <div class="tip">
            编号99以内预留 内置数据范围编码使用。<br/>
            所有依据业务表定义的范围以 字母t开头。至少2位编码，如t01<br/>
            所有依据动态定义的范围以   字母d开头。至少2位编码，如d01<br/>
        </div>
        <fminput v-model="focusRow.Wd_Name" label="名称" ></fminput>
        <fminput v-model="focusRow.Wd_Code" label="编号" ></fminput>
        <fminput v-model="focusRow.Wd_IsOn" label="启用" type="boolean"></fminput>
        <fminput v-model="focusRow.Wd_Type" label="类型" type="dict" dict="HH_WordType"></fminput>
        <fminput v-model="focusRow.Wd_Para" label="解析参数" class="spanrow"></fminput>
    
        <el-form-item class="spanrow"  v-if="focusRow.Wd_Type=='3'" label="语义定义">
            <ruleEdit style="width: 100%;"
                :rule="JSON.parse(focusRow.Wd_Parser)" ref="ruleEditer"
            >
            </ruleEdit>            
        </el-form-item>
        <fminput v-model="focusRow.Wd_Note" label="说明" class="spanrow" type="textarea"></fminput>
        <fminput v-model="focusRow.Wd_Parser" label="解析器" class="spanrow" type="textarea"></fminput>
        <fminput v-model="focusRow.Wd_Idx" label="序号" ></fminput>
        <template v-if="!adding">

            <fminput v-model="focusRow.SYS_Created" label="创建日期" noedit></fminput>
            <fminput v-model="focusRow.SYS_LAST_UPD" label="最后修改日期" noedit></fminput>
            <fminput v-model="focusRow.SYS_Deleted" label="记录删除标记" noedit></fminput>
        
        </template>

    </el-form>
</template>

<script>

import { getHHWordDefineById,addHHWordDefine,updateHHWordDefine } from "../../../api/HHWordDefineapi.js"

import fminput from "@/ui/common/input/fminput.vue"
import ruleEdit from "../../word/ruleEdit.vue"
/**
 *             <fminput v-model="focusRow.HH_WordDefineOID" label="主键" noedit></fminput>
            <fminput v-model="focusRow.SYS_CreatedBy" label="创建用户" noedit></fminput>
            <fminput v-model="focusRow.SYS_REPLACEMENT" label="授权用户" noedit></fminput>
            <fminput v-model="focusRow.SYS_POSTN" label="所属岗位" noedit></fminput>
            <fminput v-model="focusRow.SYS_DIVISION" label="所属部门" noedit></fminput>
            <fminput v-model="focusRow.SYS_ORG" label="所属组织" noedit></fminput>
            <fminput v-model="focusRow.SYS_LAST_UPD_BY" label="最后修改用户" noedit></fminput>    
 */

export default {
    name:"cnPageForm",
    components: {
        fminput,ruleEdit
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
            if(this.focusRow.HH_WordDefineOID != this.row.HH_WordDefineOID){
                this.getRowById();
            }
        },
        getRowById(){
            getHHWordDefineById(this.row.HH_WordDefineOID).then((res)=>{
                this.focusRow=res.data;
            })
        },
        save(){
            if(this.$refs.ruleEditer !=null){
                var val=this.$refs.ruleEditer.getValue();
                this.focusRow.Wd_Parser=JSON.stringify(val)
            }

            var para=[this.focusRow];
            if(this.adding){
                
                addHHWordDefine(para).then((res)=>{
                    this.$message(res.data);
                    this.$emit("close");
                    this.$emit("refresh");
                })
            }
            else{
                updateHHWordDefine(para).then((res)=>{
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
.tip{
    grid-column:1/-1;
    color:green;
}
</style>