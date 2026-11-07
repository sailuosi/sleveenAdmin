<template>
    <flexpan flex>
        <template #header="p">
            <el-form inline ref="ruleForm" :model="queryParams" label-width="100px" @submit.native.prevent>
                <el-form-item label="标题" prop="title">
                    <el-input v-model="queryParams.title" placeholder="标题" clearable 
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item label="流程" prop="FlowName">
                    <el-input v-model="queryParams.FlowName" placeholder="流程名称" clearable 
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item>
                    <el-button type="primary" icon="el-icon-search"  @click="getlist">查询</el-button>
                    <el-button type="info" @click="resetQuery">重置</el-button>
                    <el-button  @click="openAdd">添加</el-button>
                </el-form-item>
            </el-form>  
        </template>
        <template #default="p">
            <vclist :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="p.height" row-key="sacId">
	            <vccol fixed="left" field="AC_Idx" caption="序号"></vccol>
                <el-table-column    fixed="left" label="操作" width="170px">
                    <template #default="p">
                        <el-link type="primary" @click="openRow(p.row)" >编辑</el-link>
                        <el-link type="danger" @click="removeRow(p.row)" style="margin-left:10px;">删除</el-link>
                        <el-link type="primary" @click="addchild(p.row)" style="margin-left:10px;">添加子分类</el-link>
                    </template>
                </el-table-column>                
                <slot></slot>


	            <vccol field="AC_IsOn" caption="启用"></vccol>
	            <vccol field="AC_Name" caption="名称"></vccol>
	            <vccol field="AC_Note" caption="描述"></vccol>
	            <vccol field="AC_Remark" caption="备注"></vccol>

	            <vccol field="SYS_Created" caption="创建日期"></vccol>
	            <vccol field="SYS_LAST_UPD" caption="最后修改日期"></vccol>
	            <vccol field="SYS_Deleted" caption="记录删除标记"></vccol>
            </vclist>
        </template>
        <template #footer="p">
            <el-dialog title="编辑" class="normal" v-model="diaRow" width="70vw" append-to-body @close="diaRow=false">
                <ptcataform :row="focusRow" :adding="adding" @close="diaRow=false" @refresh="getlist">
                </ptcataform>
            </el-dialog>
        </template>
    </flexpan>
</template>

<script>
/**
	            <vccol field="sacId" caption=""></vccol>
	            <vccol field="SYS_CreatedBy" caption="创建用户"></vccol>
	            <vccol field="SYS_REPLACEMENT" caption="授权用户"></vccol>
	            <vccol field="SYS_POSTN" caption="所属岗位"></vccol>
	            <vccol field="SYS_DIVISION" caption="所属部门"></vccol>
	            <vccol field="SYS_ORG" caption="所属组织"></vccol>
	            <vccol field="SYS_LAST_UPD_BY" caption="最后修改用户"></vccol>
	            <vccol field="saAppletCataId" caption="组件分类"></vccol>
 */
import { fetchSysAppletCataPage ,deleteSysAppletCata} from "@s/api/paotou/paotalapi.js"

import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"

import ptcataform from "./ptcataform.vue"

export default {
    name:"cnPagelist",
    components: {
        flexpan,vclist,vccol,ptcataform
    },
    props:{
        channel:null,
    },
    emits:[],
    data(){
        return {
            mlist:[],
            total:0,  
            queryParams:{
                pageSize:100,
                pageNum:1,
                asTree:true,
            },
            focusRow:{},
            diaRow:false,//行编辑表单
            adding:false,
        };
    },
    computed:{
    },
    mounted() {
        this.getlist();
    },
    methods:{
        getlist(){

            
            fetchSysAppletCataPage(this.queryParams).then((res)=>{
                this.mlist=res.data;
                this.total=res.total;
            })
        },
        resetQuery(){
            this.queryParams={
                pageSize:30,
                pageNum:1,
                asTree:true,
            };
        },
        openAdd(){
            this.adding=true;
            this.focusRow={
                AC_IsOn:true,
                AC_Name:"新分类"
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
                deleteSysAppletCata(row).then((res)=>{
                    this.$message({ type: 'success', message: res.msg});
                    this.getlist();
                })
                
            })
        },
        addchild(row){
            this.adding=true;
            this.focusRow={
                saAppletCataId:row.sacId,
                AC_IsOn:true,
                AC_Name:"新分类"
            };
            this.diaRow=true;
        },
    }
}
</script>