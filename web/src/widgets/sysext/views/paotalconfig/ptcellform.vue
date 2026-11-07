<template>
 <el-tabs v-model="activeName" class="cell-config" >
    <el-tab-pane label="区块表单" name="cell">
        <div style="height:70vh;overflow:auto;">
            <el-form class="gridfm" label-width="110px">
                <div class="fm-toolbar" >
                    <el-button @click="save" type="primary">保存</el-button>
                </div>    
                <fminput v-model="focusRow.PC_Name" label="名称" ></fminput>
                <fminput v-model="focusRow.PC_IsOn" label="启用" type="boolean"></fminput>

                <fminput v-model="focusRow.PC_Key" label="区块key" ></fminput>
                <fminput v-model="focusRow.PC_Idx" label="序号" type="number"></fminput>

                <fminput v-model="focusRow.PC_Many" label="多播" type="dict" dict="SysAppletMany"></fminput>
                <fminput v-model="focusRow.PC_Theme" label="主题" type="dict" dict="SysPtCellTheme"></fminput>
                <fminput v-model="focusRow.PC_TitleAlign" label="位置" type="dict" dict="SysAppletTitle"></fminput>
                <fminput v-model="focusRow.PC_I" label="i" ></fminput>                
                <fminput v-model="focusRow.PC_X" label="x" ></fminput>
                <fminput v-model="focusRow.PC_Y" label="y" ></fminput>
                <fminput v-model="focusRow.PC_W" label="w" ></fminput>
                <fminput v-model="focusRow.PC_H" label="h" ></fminput>

                <fminput v-model="focusRow.PC_Margin" label="外边距" ></fminput>
                <fminput v-model="focusRow.PC_Padding" label="内边距" ></fminput>
                <fminput v-model="focusRow.PC_Style" label="自定义样式" style="grid-column:1/-1;" type="textarea"></fminput>       
                <fminput v-model="focusRow.PC_Remark" label="备注" style="grid-column:1/-1;"></fminput>


                <fminput v-model="focusRow.PC_Css" label="Css" ></fminput>
                <fminput v-model="focusRow.PC_CellPara" label="额外配置参数" style="grid-column:1/-1;" type="textarea" placeholder="请使用url格式参数进行配置"></fminput>
                <template v-if="!adding">

                    <fminput v-model="focusRow.spPortalId" label="门户" noedit></fminput>

                    <fminput v-model="focusRow.spcId" label="主键" noedit></fminput>
                    <fminput v-model="focusRow.SYS_Created" label="创建日期" noedit></fminput>
                    <fminput v-model="focusRow.SYS_LAST_UPD" label="最后修改日期" noedit></fminput>
                    <fminput v-model="focusRow.SYS_Deleted" label="记录删除标记" noedit></fminput>        
                </template>

            </el-form>        
        </div>

    </el-tab-pane>
    <el-tab-pane label="控件" name="link" lazy v-if="!adding">
        <ptcelllinklist :cell="focusRow" style="height:70vh;" ></ptcelllinklist>
    </el-tab-pane>

</el-tabs>
</template>

<script>
/**
                    <fminput v-model="focusRow.SYS_CreatedBy" label="创建用户" noedit></fminput>
                    <fminput v-model="focusRow.SYS_REPLACEMENT" label="授权用户" noedit></fminput>
                    <fminput v-model="focusRow.SYS_POSTN" label="所属岗位" noedit></fminput>
                    <fminput v-model="focusRow.SYS_DIVISION" label="所属部门" noedit></fminput>
                    <fminput v-model="focusRow.SYS_ORG" label="所属组织" noedit></fminput>
                    <fminput v-model="focusRow.SYS_LAST_UPD_BY" label="最后修改用户" noedit></fminput>
 */
import { fetchSysPortCell, saveSysPortCell } from "@s/api/paotou/paotalapi.js"

import fminput from "@/ui/common/input/fminput.vue"
import ptcelllinklist from "./ptcelllinklist.vue"

export default {
    name:"ptcellform",
    components: {
        fminput,ptcelllinklist
    },
    props:{
        row:null,
        disabled:{ type:Boolean,default:false },
        adding:{ type:Boolean,default:false },
    },
    emits:[],
    data(){
        return {
            activeName:"cell",
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
                this.activeName="cell";
                return ;
            }
            if(this.focusRow.spcId != this.row.spcId){
                this.getRowById();
            }
        },
        getRowById(){
            fetchSysPortCell(this.row.spcId).then((res)=>{
                this.focusRow=res.data;
            })
        },
        save(){
            var para=[this.focusRow];
            if(this.adding){
                
                saveSysPortCell(para).then((res)=>{
                    this.$message(res.data);
                    this.$emit("close");
                    this.$emit("refresh");
                })
            }
            else{
                saveSysPortCell(para).then((res)=>{
                    this.$message(res.data);
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