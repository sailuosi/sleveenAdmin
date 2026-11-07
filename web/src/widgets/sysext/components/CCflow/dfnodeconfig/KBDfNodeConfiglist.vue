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
            <vclist :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="p.height">
                <el-table-column    fixed="left" label="操作" width="110px">
                    <template #default="p">
                        <el-link type="primary" @click="openRow(p.row)" >编辑</el-link>
                        <el-link type="danger" @click="removeRow(p.row)" style="margin-left:10px;">删除</el-link>
                    </template>
                </el-table-column>                
                <slot></slot>

	            <vccol field="Nc_NodeID" caption="流程节点"></vccol>
	            <vccol field="Nc_NodeName" caption="节点名称"></vccol>
	            <vccol field="Nc_Roles" caption="关联的人员角色"></vccol>
	            <vccol field="Nc_Limit" caption="可选个数"></vccol>
	            <vccol field="Nc_FixedMan" caption="固定的人"></vccol>
	            <vccol field="Nc_Para" caption="选择器参数"></vccol>
	            <vccol field="Nc_Fallback" caption="回退策略"></vccol>
	            <vccol field="Nc_FixedManName" caption="人名"></vccol>
	            <vccol field="Nc_OpRules" caption="操作规则"></vccol>
	            <vccol field="Nc_BindVal" caption="绑定值"></vccol>
	            <vccol field="Nc_Bind" caption="绑定类型"></vccol>
	            <vccol field="Nc_Note" caption="说明"></vccol>
	            <vccol field="Nc_Remark" caption="备注"></vccol>
	            <vccol field="Nc_Created" caption="创建时间"></vccol>
	            <vccol field="Nc_Updated" caption="更新时间"></vccol>

	            <vccol field="Nc_CreateMan" caption="创建人"></vccol>
	            <vccol field="Nc_UpdateMan" caption="更新人"></vccol>
            </vclist>
        </template>
        <template #footer="p">
            <el-dialog title="编辑" class="normal" v-model="diaRow" width="70vw" append-to-body @close="diaRow=false">
                <KBDfNodeConfigform :row="focusRow" :adding="adding" @close="onclosefm" @refresh="getlist">
                </KBDfNodeConfigform>
            </el-dialog>
        </template>
    </flexpan>
</template>

<script>
/** 部门流程节点配置列表
	            <vccol field="Nc_OID" caption="主键"></vccol>
	            <vccol field="Nc_DfOID" caption="部门流程配置外键"></vccol>
	            <vccol field="NC_CreatedAcount" caption="创建人"></vccol>
	            <vccol field="Nc_UpdatedAcount" caption="更新人"></vccol>
 */


import { selectKBDfNodeConfiglist ,removeKBDfNodeConfig} from "@s/api/ccflow/KBDfNodeConfigapi.js"

import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"

import KBDfNodeConfigform from "./KBDfNodeConfigform.vue"

export default {
    name:"cnPagelist",
    components: {
        flexpan,vclist,vccol,KBDfNodeConfigform
    },
    props:{
        dfconfig:null,
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
                Nc_DfOID:this.dfconfig.Df_OID,
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
           
            selectKBDfNodeConfiglist(this.listpara).then((res)=>{
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
                Nc_DfOID:this.dfconfig.Df_OID,
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
                removeKBDfNodeConfig(row).then((res)=>{
                    this.$message({ type: 'success', message: res.msg});
                    this.getlist();
                })
                
            })
        },
    }
}
</script>