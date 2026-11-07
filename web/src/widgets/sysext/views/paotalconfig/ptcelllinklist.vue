<template>
    <flexpan flex>
        <template #header="p">
            <el-form inline ref="ruleForm" :model="queryParams" label-width="100px" @submit.native.prevent>
                <el-form-item label="标题" prop="CL_Title">
                    <el-input v-model="queryParams.CL_Title" placeholder="标题" clearable 
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item label="组件key" prop="CL_Key">
                    <el-input v-model="queryParams.CL_Key" placeholder="组件key" clearable 
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item>
                    <el-button type="primary" icon="el-icon-search"  @click="getlist">查询</el-button>
                    <el-button type="info" @click="resetQuery">重置</el-button>
                    <el-button type="primary" @click="openAdd">添加</el-button>
                </el-form-item>
            </el-form>  
        </template>
        <template #default="p">
            <vclist :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="(p.height-10)"
                :maxLine="1" orderCol
            >
                <el-table-column    fixed="left" label="操作" width="110px">
                    <template #default="p">
                        <el-link type="primary" @click="openRow(p.row)" >编辑</el-link>
                        <el-link type="danger" @click="removeRow(p.row)" style="margin-left:10px;">删除</el-link>
                    </template>
                </el-table-column>                
                <slot></slot>
                <vccol field="CL_IsOn" caption="启用" type="Boolean" width="60px"></vccol>
	            <vccol field="CL_Link" caption="区块key"></vccol>
	            <vccol field="CL_Key" caption="组件key" min-width="300px"></vccol>
	            <vccol field="CL_Title" caption="标题" min-width="180px"></vccol>
	            <vccol field="CL_TitleAlign" caption="显示标题" type="dict" dict="SysAppletTitle"></vccol>
	            <vccol field="CL_Theme" caption="主题" type="dict" dict="SysPtCellTheme"></vccol>                
	            <vccol field="CL_TitleStyle" caption="标题样式"></vccol>
	            <vccol field="CL_Type" caption="内容类型" type="dict" dict="SysAppletType"></vccol>
	            <vccol field="CL_Idx" caption="排序号"></vccol>                
	            <vccol field="CL_Para" caption="组件参数"></vccol>
	            <vccol field="CL_Icon" caption="图标"></vccol>
	            <vccol field="CL_Tool" caption="标题工具栏"></vccol>

	            <vccol field="CL_Slot" caption="缺省内容"></vccol>
	            <vccol field="CL_ParaVal" caption="默认参数"></vccol>
	            <vccol field="CL_Para2" caption="第2参数"></vccol>
	            <vccol field="CL_Remark" caption="备注"></vccol>
	            
	            <vccol field="CL_Color" caption="颜色"></vccol>


            </vclist>
        </template>
        <template #footer="p">
            <el-dialog title="编辑" class="normal" v-model="diaRow" width="70vw" append-to-body @close="diaRow=false">
                <ptcelllinkform :row="focusRow" :adding="adding" @close="diaRow=false" @refresh="getlist">
                </ptcelllinkform>
            </el-dialog>
        </template>
    </flexpan>
</template>

<script>
/**
	            <vccol field="spclId" caption=""></vccol>
	            <vccol field="CL_BodyStyle" caption="内容样式"></vccol>
	            <vccol field="SYS_Created" caption="创建日期"></vccol>
	            <vccol field="SYS_LAST_UPD" caption="最后修改日期"></vccol>
	            <vccol field="SYS_Deleted" caption="记录删除标记"></vccol>
	            <vccol field="SYS_CreatedBy" caption="创建用户"></vccol>
	            <vccol field="SYS_REPLACEMENT" caption="授权用户"></vccol>
	            <vccol field="SYS_POSTN" caption="所属岗位"></vccol>
	            <vccol field="SYS_DIVISION" caption="所属部门"></vccol>
	            <vccol field="SYS_ORG" caption="所属组织"></vccol>
	            <vccol field="SYS_LAST_UPD_BY" caption="最后修改用户"></vccol>
	            <vccol field="spPortalId" caption="门户"></vccol>
	            <vccol field="spPortCellId" caption="门户区块"></vccol>
	            <vccol field="saAppletId" caption="门户组件库"></vccol>
 */
import { fetchSysPortCellLinkPage ,deleteSysPortCellLink} from "@s/api/paotou/paotalapi.js"

import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"

import ptcelllinkform from "./ptcelllinkform.vue"


export default {
    name:"cnPagelist",
    components: {
        flexpan,vclist,vccol,ptcelllinkform
    },
    props:{
        cell:null,
    },
    emits:[],
    data(){
        return {
            mlist:[],
            total:0,  
            queryParams:{
                pageSize:100,
                pageNum:1,
            },
            focusRow:{},
            diaRow:false,//行编辑表单
            adding:false,
        };
    },
    computed:{
        listpara(){
            var t= Object.assign({
                spPortCellId:this.cell.spcId,
                spPortalId:this.cell.spPortalId,

            },this.queryParams);
            return t;
        },
    },
    mounted() {
        this.getlist();
    },
    watch:{
        "listpara":function(){
            this.getlist();
        }
    },
    methods:{
        getlist(){

            
            fetchSysPortCellLinkPage(this.listpara).then((res)=>{
                this.mlist=res.data;
                this.total=res.total;
            })
        },
        resetQuery(){
            this.queryParams={
                pageSize:30,
                pageNum:1,
            };
        },
        openAdd(){
            this.adding=true;
            this.focusRow={
                spPortCellId:this.cell.spcId,
                spPortalId:this.cell.spPortalId,
                CL_Link:this.cell.PC_Key,
                CL_IsOn:true,
            };
            this.diaRow=true;
        },
        openRow(row){
            this.adding=false;
            this.focusRow=row;
            this.diaRow=true;
        },
        removeRow(row){
            this.$confirm('此操作将永久删除该记录, 是否继续?', '提示', {confirmButtonText: '确定',cancelButtonText: '取消',type: 'warning'})
            .then(() => {
                deleteSysPortCellLink(row).then((res)=>{
                    this.$message({ type: 'success', message: res.msg});
                    this.getlist();
                })
                
            })
        },
    }
}
</script>