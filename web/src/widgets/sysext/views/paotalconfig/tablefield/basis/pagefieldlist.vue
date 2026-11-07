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
                    <el-button  @click="resetQuery">重置</el-button>
                    <el-button  @click="openAdd">添加</el-button>
                </el-form-item>
            </el-form>  
        </template>
        <template #default="p">
            <vclist :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="(p.height-5)"
                :maxLine="1"
            >
                <el-table-column    fixed="left" label="操作" width="110px">
                    <template #default="p">
                        <el-link type="primary" @click="openRow(p.row)" >编辑</el-link>
                        <el-link type="danger" @click="removeRow(p.row)" style="margin-left:10px;">删除</el-link>
                    </template>
                </el-table-column>                
                <slot></slot>
	            
	            <vccol field="PF_Idx" caption="排序" width="50px"></vccol>
	            <vccol field="PF_IsOn" caption="启用" type="Boolean" width="60px"></vccol>                
	            <vccol field="PF_Field" caption="字段名" width="150px"></vccol>
	            <vccol field="PF_Caption" caption="中文名" width="150px"></vccol>
	            <vccol field="PF_Show" caption="显示" type="Boolean" width="60px"></vccol>
                <vccol field="PF_Type" caption="类型" width="60px"></vccol>
	            <vccol field="PF_CType" caption="内容类型" width="90px"></vccol>
                <vccol field="PF_ListIdx" caption="列表排序" width="80px"></vccol>
                <vccol field="PF_Sortable" caption="列表可排序" width="90px"></vccol>
                
                <vccol field="PF_Todo_ListIdx" caption="待办列表排序" width="100px"></vccol>
                <vccol field="PF_Done_ListIdx" caption="已办列表排序" width="100px"></vccol>
                <vccol field="PF_Node_ListIdx" caption="节点列表排序" width="100px"></vccol>
                
	            <vccol field="PF_CodeTable" caption="代码表"></vccol>
	            <vccol field="PF_Min" caption="最小" width="50px"></vccol>
	            <vccol field="PF_Max" caption="最大" width="50px"></vccol>                
	            <vccol field="PF_Disabled" caption="禁用" type="Boolean" width="60px"></vccol>
                <vccol field="PF_ReadOnly" caption="只读" type="dict" dict="KB_Wether" width="60px"></vccol>
	            <vccol field="PF_ValueStyle" caption="内容样式" width="160px"></vccol>
	            <vccol field="PF_Tip" caption="提示"></vccol>
	            <vccol field="PF_Width" caption="列宽"></vccol>
	            <vccol field="PF_Align" caption="对齐"></vccol>
	            <vccol field="PF_TitleAlign" caption="标题对齐"></vccol>

	            <vccol field="PF_TitleStyle" caption="标题样式"></vccol>

	            <vccol field="PF_DefaultVal" caption="默认值"></vccol>

	            <vccol field="PF_Remark" caption="备注"></vccol>

            </vclist>
        </template>
        <template #footer="p">
            <el-dialog title="编辑" class="normal" v-model="diaRow" width="70vw" append-to-body @close="diaRow=false">
                <pagefieldform :row="focusRow" :adding="adding" @close="onclosefm" @refresh="getlist">
                </pagefieldform>
            </el-dialog>
        </template>
    </flexpan>
</template>

<script>
/**
<vccol field="spfId" caption=""></vccol>
	            <vccol field="spsId" caption="页面查询"></vccol>
	            <vccol field="spPortPageId" caption="门户页面"></vccol>
 */

//import { fetchSysPageFieldPage ,removeZHPageField} from "@/api/system/paotou/pagefieldapi.js"

import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"

import pagefieldform from "./pagefieldform.vue"

export default {
    name:"cnPagelist",
    components: {
        flexpan,vclist,vccol,pagefieldform
    },
    props:{
        page:null,
        table:null,
    },
    emits:[],
    data(){
        return {
            mlist:[],
            total:0,  
            queryParams:{
                pageSize:30,
                pageNum:1,
            },
            focusRow:{},
            diaRow:false,//行编辑表单
            adding:false,
        };
    },
    computed:{
        listpara(){
            var obj=Object.assign({
                spsId:this.table.spsId,
                spPortPageId:this.page.spId
            },this.queryParams);
			return obj;
        },
    },
    watch:{
        "listpara":function(){
            this.getlist();
        }
    },
    mounted() {
        this.getlist();
    },
    methods:{
        getlist(){
           
            fetchSysPageFieldPage(this.listpara).then((res)=>{
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
        onclosefm(){
            this.diaRow=false;
        },
        openAdd(){
            this.adding=true;
            this.focusRow={
                spsId:this.table.spsId,
                spPortPageId:this.page.sppId
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
                removeZHPageField(row).then((res)=>{
                    this.$message({ type: 'success', message: res.msg});
                    this.getlist();
                })
                
            })
        },
    }
}
</script>