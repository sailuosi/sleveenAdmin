<template>
 <el-tabs v-model="activeName" class="paotao-config" >
    <el-tab-pane label="门户表单" name="pao">
        <div style="height:70vh;overflow:auto;">
            <el-form class="gridfm" label-width="100px">
                <div class="fm-toolbar" >
                    <el-button @click="save" type="primary">保存</el-button>
                    <el-button @click="breaklink">清除关联页面</el-button>
                </div>    

                <fminput v-model="focusRow.P_Name" label="名称" must></fminput>
                <fminput v-model="focusRow.P_IsOn" label="启用" type="boolean" must></fminput>
                <fminput v-model="focusRow.P_Key" label="唯一编码" must></fminput>
                <el-form-item label="所属单位" label-width="110px" prop="OrgName" class="fminput">
                    <hrselect max-height="50vh" v-model="focusRow.spOrgId" @select="setDept">
                        <el-input v-model="focusRow.OrgName" placeholder="请选择单位"  :disabled="disabled"></el-input>
                    </hrselect>
                </el-form-item>

                <fminput v-model="focusRow.P_Pulic" label="共享" type="dict" dict="SysShareStatus" must></fminput>       
                <fminput v-model="focusRow.P_Theme" label="主题"  type="dict" dict="SysPtCellTheme" must></fminput>   

                <fminput v-model="focusRow.P_ColNum" label="列数" must></fminput>
                <fminput v-model="focusRow.P_Rows" label="行高" must></fminput>  

                <fminput v-model="focusRow.P_Style" label="自定义样式" style="grid-column:1/-1;" type="textarea"></fminput>
                <fminput v-model="focusRow.P_Para" label="参数" style="grid-column:1/-1;" placeholder="请使用url参数格式定义"></fminput>

                <fminput v-model="focusRow.P_Note" label="说明" style="grid-column:1/-1;" type="textarea"></fminput>
                <fminput v-model="focusRow.P_Task" label="业务" type="dict" dict="SysPtTask"></fminput>
                <fminput v-model="focusRow.P_ShowTitle" label="显示标题" type="dict" dict="SysAppletTitle"></fminput>

                <fminput v-model="focusRow.P_Display" label="布局方式" type="dict" dict="SysPtDisplay"></fminput>
                <fminput v-model="focusRow.P_Editable" label="可拖动" type="boolean"></fminput>            


                <fminput v-model="focusRow.P_Idx" label="序号" ></fminput>
                <fminput v-model="focusRow.P_Group" label="分组" ></fminput>

                <fminput v-model="focusRow.P_Icon" label="图标" ></fminput>
                <fminput v-model="focusRow.P_Css" label="类" ></fminput>

                <fminput v-model="focusRow.P_Remark" label="备注" style="grid-column:1/-1;"></fminput>

                
                <el-popover
                    placement="bottom"
                    title="页面绑定"
                    :width="500"
                    trigger="click"
                    content="this is content, this is content, this is content"
                >
                    <template #reference>
                        
                    <el-tag class="m-2">绑定页面{{focusRow.PP_Title}}</el-tag>
                    </template>
                    <pageselect :max="1" @update:modelValue="setPage"></pageselect>
                </el-popover>
                <template v-if="!adding">
                    <el-form-item label="绑定角色" style="margin-bottom:0;">
                        <tagsrole :disabled="disabled || adding" :row="focusRow"
                            type="1"
                        ></tagsrole>
                    </el-form-item>
                    <fminput v-model="focusRow.spId" label="主键" noedit></fminput>
                    <fminput v-model="focusRow.SYS_Created" label="创建日期" noedit></fminput>
                    <fminput v-model="focusRow.SYS_LAST_UPD" label="最后修改日期" noedit></fminput>
                    <fminput v-model="focusRow.SYS_Deleted" label="记录删除标记" noedit></fminput>

                    <fminput v-model="focusRow.spOrgId" label="组织机构基本信息外键" noedit></fminput>
                    <fminput v-model="focusRow.spSubjectId" label="业务分类" noedit></fminput>
                    <fminput v-model="focusRow.spUserId" label="用户外键" noedit></fminput> 
        
                </template>
            </el-form>  
        </div>
    </el-tab-pane>
    <el-tab-pane label="区块" name="cell" lazy v-if="!adding">
        <ptcelllist :portal="focusRow" style="height:70vh;"></ptcelllist>
    </el-tab-pane>

</el-tabs>

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
import { fetchSysPortal,createSysPortal,updateSysPortal } from "@s/api/paotou/paotalapi.js"

import fminput from "@/ui/common/input/fminput.vue"
import ptcelllist from "./ptcelllist.vue"
import tagsrole from "./parts/tagsrole.vue"

import pageselect from "./selector/pageselect.vue"
import hrselect from "@s/views/OrgCommon/org/hrselect.vue"

export default {
    name:"paotalform",
    components: {
        fminput,ptcelllist,tagsrole,pageselect,hrselect
    },
    props:{
        row:null,
        disabled:{ type:Boolean,default:false },
        adding:{ type:Boolean,default:false },
    },
    emits:[],
    data(){
        return {
            activeName:"pao",
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
        setDept(tar) {
            this.focusRow.OrgName = tar.OrgName;
        },
        load(){
            if(this.adding){
                this.focusRow=this.row;
                this.activeName="pao";
                return ;
            }
            if(this.focusRow.spId != this.row.spId){
                this.getRowById();
            }
        },
        getRowById(){
            fetchSysPortal(this.row.spId).then((res)=>{
                // requestAPI6 已解包 data；兼容仍带 data 的旧响应
                this.focusRow = res?.data ?? res ?? {};
            })
        },
        save(){
            var para=[this.focusRow];
            if(this.adding){
                
                createSysPortal(para).then((res)=>{
                    this.$message(res?.data ?? res ?? '保存成功');
                    this.$emit("close");
                    this.$emit("refresh");
                })
            }
            else{
                updateSysPortal(para).then((res)=>{
                    this.$message(res?.data ?? res ?? '保存成功');
                    //this.$emit("close");
                    this.$emit("refresh");
                })
            }
        },
        setPage(e){
            if(e.length==1){
                var page=e[0];
                this.focusRow.spPortPageId=page.sppId;
                this.focusRow.PP_Title=page.PP_Title;
                return ;
            }
            this.focusRow.spPortPageId=void 0;
            this.focusRow.PP_Title="";
        },
        breaklink(){
            this.focusRow.spPortPageId="";
            this.focusRow.PP_Title="";
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