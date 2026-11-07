<template>
    <el-form class="gridfm" label-width="110px">
        <div class="fm-toolbar" >
            <el-button @click="save" type="primary">保存</el-button>
            <el-button @click="openApplet" type="warning">从组件库选择</el-button>
            <div>
                <el-dialog title="组件库" class="normal" v-model="diaRow" width="70vw" append-to-body @close="diaRow=false">
                    <ptapplet style="height:75vh;" selecting>
                        <template #applet>
                            <el-table-column    fixed="left" label="操作" width="110px">
                                <template #default="p">
                                    <el-button type="warning" @click="selectApplet(p.row)" >选择该组件</el-button>
                                </template>
                            </el-table-column>  
                        </template>
                    </ptapplet>
                </el-dialog>
            </div>            
        </div>    

        <fminput v-model="focusRow.CL_Link" label="区块key" ></fminput>
        <fminput v-model="focusRow.CL_IsOn" label="启用" type="boolean"></fminput>

        
        <fminput v-model="focusRow.CL_Title" label="标题" class="fm-row"></fminput>

        <fminput v-model="focusRow.CL_TitleAlign" label="显示标题" type="dict" dict="SysAppletTitle" must></fminput>
        <fminput v-model="focusRow.CL_Theme" label="主题" type="dict" dict="SysPtCellTheme" must></fminput>


        <fminput v-model="focusRow.CL_Type" label="内容类型" type="dict" dict="SysAppletType" must></fminput>
        <fminput v-model="focusRow.CL_Tool" label="标题工具栏" type="dict" dict="SysApplteTool"></fminput>
        <fminput v-model="focusRow.CL_Key" label="组件key" class="fm-row" must></fminput>

        <fminput v-model="focusRow.CL_Disabled" label="可用性" type="dict" dict="SysCellDisabled"></fminput>
        <fminput v-model="focusRow.CL_Color" label="颜色" ></fminput>

        <fminput v-model="focusRow.CL_Para" label="组件参数" class="fm-row"></fminput>
        <fminput v-model="focusRow.CL_TitleStyle" label="标题样式" class="fm-row" type="textarea"></fminput>
        <fminput v-model="focusRow.CL_BodyStyle" label="内容样式" class="fm-row" type="textarea"></fminput>
        <fminput v-model="focusRow.CL_Icon" label="图标" ></fminput>
        
        <fminput v-model="focusRow.CL_Idx" label="序号" ></fminput>
        <fminput v-model="focusRow.CL_Slot" label="缺省内容" class="fm-row" type="textarea"></fminput>
        <fminput v-model="focusRow.CL_ParaVal" label="默认参数" class="fm-row" type="textarea"></fminput>
        <fminput v-model="focusRow.CL_Para2" label="第2参数" class="fm-row" type="textarea" noedit></fminput>
        <fminput v-model="focusRow.CL_Remark" label="备注" class="fm-row" ></fminput>

        <paraEdit class="fm-row" v-model="focusRow.CL_Para2">
        </paraEdit>
        

        <template v-if="!adding">
            <fminput v-model="focusRow.spclId" label="主键" noedit></fminput>
            <fminput v-model="focusRow.SYS_Created" label="创建日期" noedit></fminput>
            <fminput v-model="focusRow.SYS_LAST_UPD" label="最后修改日期" noedit></fminput>
            <fminput v-model="focusRow.SYS_Deleted" label="记录删除标记" noedit></fminput>        

            <fminput v-model="focusRow.spPortalId" label="门户" noedit></fminput>
            <fminput v-model="focusRow.spPortCellId" label="门户区块" noedit></fminput>
            <fminput v-model="focusRow.saAppletId" label="门户组件库" noedit></fminput>        
        </template>



    </el-form>
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
import { fetchSysPortCellLink, saveSysPortCellLink } from "@s/api/paotou/paotalapi.js"
import { loadselectTask } from "@ho/api/KbTask/taskTypeApi.js"
import fminput from "@/ui/common/input/fminput.vue"
import ptapplet from  "./ptapplet.vue"
import paraEdit from "./parts/paraEdit.vue"

export default {
    name:"ptcelllinkform",
    components: {
        fminput,ptapplet,paraEdit
    },
    props:{
        orgoid:null,
        row:null,
        disabled:{ type:Boolean,default:false },
        adding:{ type:Boolean,default:false }
    },
    emits:[],
    data(){
        return {
            focusRow: {},
            diaRow: false//行编辑表单
        };
    },
    computed:{
    },
    mounted() {
         this.load();
    },
    watch:{
        "row":function(val){
            this.load();
        }
    },    
    methods:{
        loadList(){
                var t= loadselectTask({
                    orgoid:this.orgoid,
                }).then((res)=>{
                    return res.data;
                })
                //console.log(t)
                return t;
            },
        load(){
            if(this.adding){
                this.focusRow=this.row;
                return ;
            }
            if(this.focusRow.spclId != this.row.spclId){
                this.getRowById();
            }
        },
        getRowById(){
            fetchSysPortCellLink(this.row.spclId).then((res)=>{
                this.focusRow=res.data;
            })
        },
        save(){
            var para=[this.focusRow];
            if(this.adding){
                
                saveSysPortCellLink(para).then((res)=>{
                    this.$message(res.data);
                    this.$emit("close");
                    this.$emit("refresh");
                })
            }
            else{
                saveSysPortCellLink(para).then((res)=>{
                    this.$message(res.data);
                    //this.$emit("close");
                    this.$emit("refresh");
                })
            }
        },
        openApplet(){
            this.diaRow=true;
        },
        selectApplet(row){
            var obj={
                CL_Key:row.A_Key,
                CL_Title:row.A_Title,
                CL_TitleAlign:row.A_TitleAlign,
                CL_TitleStyle:row.A_TitleStyle,
                CL_Type:row.A_Type,
                CL_Para:row.A_Para,
                CL_BodyStyle:row.A_BodyStyle,
                CL_Icon:row.A_Icon,
                CL_Tool:row.A_Tool,
                CL_Slot:row.A_Slot,
                CL_ParaVal:row.A_ParaVal,
                CL_Para2:row.A_Para2,
                saAppletId:row.saId, 
            };
            this.focusRow= Object.assign(obj,this.focusRow);
            this.diaRow=false;
        }
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
.fm-row{
    grid-column:1/-1;
}
</style>