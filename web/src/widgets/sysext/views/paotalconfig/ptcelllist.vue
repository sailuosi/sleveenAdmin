<template>
    <flexpan flex>
        <template #header="p">
            <el-form inline ref="ruleForm" :model="queryParams" label-width="100px" @submit.native.prevent>
                <el-form-item label="名称" prop="PC_Name">
                    <el-input v-model="queryParams.PC_Name" placeholder="标题" clearable 
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item label="区块key" prop="PC_Key">
                    <el-input v-model="queryParams.PC_Key" placeholder="区块key" clearable 
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item>
                    <el-button type="primary" icon="el-icon-search"  @click="getlist">查询</el-button>
                    <el-button type="info" @click="resetQuery">重置</el-button>
                    <el-button type="warning" @click="openAdd">添加</el-button>
                    <el-button  @click="resetCellName">根据控件更新区块名</el-button>
                    <el-button type="warning" @click="openAppet">选择组件</el-button>
                </el-form-item>
            </el-form>  
        </template>
        <template #default="p">
            <vclist :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="(p.height-5)"
                :maxLine="1" orderCol
            >
                <el-table-column    fixed="left" label="操作" width="110px">
                    <template #default="p">
                        <el-link type="primary" @click="openRow(p.row)" >编辑</el-link>
                        <el-link type="danger" @click="removeRow(p.row)" style="margin-left:10px;">删除</el-link>
                    </template>
                </el-table-column>                
                <slot></slot>

	            
	            <vccol field="PC_IsOn" caption="启用" width="60px" type="Boolean"></vccol>
	            <vccol field="PC_Name" caption="名称" min-width="180px" align="left"></vccol>
                <vccol field="PC_Key" caption="区块key"></vccol>
	            <vccol field="PC_X" caption="x" width="60px"></vccol>
	            <vccol field="PC_Y" caption="y" width="60px"></vccol>
	            <vccol field="PC_W" caption="w" width="60px"></vccol>
	            <vccol field="PC_H" caption="h" width="60px"></vccol>
	            <vccol field="PC_I" caption="i" width="60px"></vccol>
	            <vccol field="PC_Margin" caption="外边距"></vccol>
	            <vccol field="PC_Padding" caption="内边距"></vccol>
                <vccol field="PC_Idx" caption="排序号" width="60px"></vccol>
	            <vccol field="PC_Remark" caption="备注" min-width="380px"></vccol>
	            
	            <vccol field="PC_Many" caption="多播"></vccol>
	            <vccol field="PC_Theme" caption="主题"></vccol>
	            <vccol field="PC_Css" caption="Css"></vccol>

	            <vccol field="SYS_Created" caption="创建日期" width="130px" type="datetime"></vccol>
	            <vccol field="SYS_LAST_UPD" caption="最后修改日期" width="130px" type="datetime"></vccol>
	            <vccol field="SYS_Deleted" caption="记录删除标记" width="60px"></vccol>
            </vclist>
        </template>
        <template #footer="p">
            <el-dialog title="编辑" class="normal" v-model="diaRow" width="70vw" append-to-body @close="diaRow=false">
                <ptcellform :row="focusRow" :adding="adding" @close="diaRow=false" @refresh="getlist">
                </ptcellform>
            </el-dialog>
            <el-dialog title="组件库" class="normal" v-model="seeAppet" width="70vw" append-to-body @close="seeAppet=false">
                <ptapplet style="height:75vh;" selecting
                    ref="applet" checkable
                >
                </ptapplet>
                <template #footer>
                    <el-button type="warning" @click="doAppetAdd" size="big">确定添加</el-button>
                </template>
            </el-dialog>
        </template>
    </flexpan>
</template>

<script>
/**
	            <vccol field="PC_Style" caption="自定义样式"></vccol>
	            <vccol field="SYS_CreatedBy" caption="创建用户"></vccol>
	            <vccol field="SYS_REPLACEMENT" caption="授权用户"></vccol>
	            <vccol field="SYS_POSTN" caption="所属岗位"></vccol>
	            <vccol field="SYS_DIVISION" caption="所属部门"></vccol>
	            <vccol field="SYS_ORG" caption="所属组织"></vccol>
	            <vccol field="SYS_LAST_UPD_BY" caption="最后修改用户"></vccol>
	            <vccol field="spPortalId" caption="门户"></vccol>
 */
import { fetchSysPortCellPage ,deleteSysPortCell,setCellNameBycont,addCelllLinks} from "@s/api/paotou/paotalapi.js"

import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"

import ptcellform from "./ptcellform.vue"
import ptapplet from  "./ptapplet.vue"

export default {
    name:"cnPagelist",
    components: {
        flexpan,vclist,vccol,ptcellform,ptapplet
    },
    props:{
        portal:null,
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
            //组件库批量选
            seeAppet:false,
        };
    },
    computed:{
    },
    mounted() {
        this.getlist();
    },
    watch:{
        "portal":function(){
            this.getlist();
        },
    },
    methods:{
        getlist(){

            this.queryParams.spPortalId=this.portal.spId
            fetchSysPortCellPage(this.queryParams).then((res)=>{
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
        openAppet(){
            this.seeAppet=true;
        },
        doAppetAdd(){
            var v= this.$refs.applet.getChecked();
            if(v.length==0){
                this.$message("尚未选择组件！");
            }
            //console.log(v);
            var para={
                CellLinks:v,
                spId:this.portal.spId
            }
            addCelllLinks(para).then((res)=>{
                this.seeAppet=false;
                this.$message({ type: 'success', message: res.data});
                this.getlist();
            })
        },
        openAdd(){
            this.adding=true;
            var idx= this.total+1;

            this.focusRow={
                spPortalId:this.portal.spId,
                PC_IsOn:true,
                PC_Idx:idx,
                PC_Name:idx+"#单元格",
                PC_X:0,
                PC_Y:0,
                PC_W:2,
                PC_H:2,
                PC_I:idx,
                PC_Key:"panel_"+idx,
                PC_Many:'1',
                PC_Theme:'1',
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
                deleteSysPortCell(row).then((res)=>{
                    this.$message({ type: 'success', message: res.msg});
                    this.getlist();
                })
                
            })
        },
        resetCellName(){
            this.$confirm('确定要根据区块的控件名更新区块名称吗?', '提示', {confirmButtonText: '确定',cancelButtonText: '取消',type: 'warning'})
            .then(() => {
                setCellNameBycont(this.portal.spId).then((res)=>{
                    this.$message({ type: 'success', message: res.msg});
                    this.getlist();
                })
                
            })
        },
    }
}
</script>