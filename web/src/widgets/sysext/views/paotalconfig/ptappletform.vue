<template>
    <el-form class="gridfm" label-width="120px">
        <div class="fm-toolbar" >
            <el-button @click="save">保存</el-button>
        </div>    
        <fminput v-model="focusRow.A_Title" label="标题" must></fminput>
        <fminput v-model="focusRow.A_IsOn" label="启用" type="boolean" must></fminput>

        <fminput v-model="focusRow.A_Theme" label="主题" type="dict" dict="SysPtCellTheme"></fminput>   
        <fminput v-model="focusRow.A_TitleAlign" label="显示标题" type="dict" dict="SysAppletTitle"></fminput>
        <fminput v-model="focusRow.A_Type" label="内容类型" type="dict" dict="SysAppletType" must></fminput>
        <fminput v-model="focusRow.A_Idx" label="序号" ></fminput>

        <fminput v-model="focusRow.A_Key" label="内容key" class="spanrow" must></fminput>
        <fminput v-model="focusRow.A_Para" label="内容参数" class="spanrow"></fminput>
        <fminput v-model="focusRow.A_Icon" label="图标" ></fminput>
        <fminput v-model="focusRow.A_Color" label="颜色" ></fminput>
        <fminput v-model="focusRow.A_Tool" label="标题工具栏" ></fminput>

        <fminput v-model="focusRow.A_Slot" label="缺省内容" ></fminput>
        <fminput v-model="focusRow.A_ParaVal" label="默认参数" ></fminput>
        <fminput v-model="focusRow.A_Para2" label="第2参数" ></fminput>
     
        <fminput v-model="focusRow.cp_params" label="其它参数" class="spanrow" type="textarea"></fminput>
        <fminput v-model="focusRow.A_TitleStyle" label="标题样式" class="spanrow"  type="textarea"></fminput>
        <fminput v-model="focusRow.A_BodyStyle" label="内容样式" class="spanrow"  type="textarea"></fminput>
        <fminput v-model="focusRow.A_Remark" label="备注" class="spanrow"></fminput>
        <template v-if="!adding">
            
            <fminput v-model="focusRow.saAppletCataId" label="组件分类" noedit></fminput>
            <fminput v-model="focusRow.saId" label="主键" noedit></fminput>
            <fminput v-model="focusRow.SYS_Created" label="创建日期" noedit></fminput>
            <fminput v-model="focusRow.SYS_LAST_UPD" label="最后修改日期" noedit></fminput>
            <fminput v-model="focusRow.SYS_Deleted" label="记录删除标记" noedit></fminput> 
        </template>



    </el-form>
</template>

<script>
/**
            <fminput v-model="focusRow.SYS_REPLACEMENT" label="授权用户" noedit></fminput>
            <fminput v-model="focusRow.SYS_POSTN" label="所属岗位" noedit></fminput>
            <fminput v-model="focusRow.SYS_DIVISION" label="所属部门" noedit></fminput>
            <fminput v-model="focusRow.SYS_ORG" label="所属组织" noedit></fminput>
            <fminput v-model="focusRow.SYS_LAST_UPD_BY" label="最后修改用户" noedit></fminput>
            <fminput v-model="focusRow.SYS_CreatedBy" label="创建用户" noedit></fminput>
 */

import { fetchSysApplet, saveSysApplet } from "@s/api/paotou/paotalapi.js"

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
            if(this.focusRow.saId != this.row.saId){
                this.getRowById();
            }
        },
        getRowById(){
            fetchSysApplet(this.row.saId).then((res)=>{
                this.focusRow=res.data;
            })
        },
        save(){
            var para=[this.focusRow];
            if(this.adding){
                
                saveSysApplet(para).then((res)=>{
                    this.$message(res.data);
                    this.$emit("close");
                    this.$emit("refresh");
                })
            }
            else{
                saveSysApplet(para).then((res)=>{
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