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
                <el-table-column    fixed="left" label="操作" width="130px">
                    <template #default="p">
                        <el-link type="primary" @click="openRow(p.row)" >编辑</el-link>
                        <el-link type="warning" @click="copyRow(p.row)" style="margin-left:10px;">复制</el-link>
                        <el-link type="danger" @click="removeRow(p.row)" style="margin-left:10px;">删除</el-link>
                    </template>
                </el-table-column>                
                <slot></slot>
	            <vccol field="Df_Title" caption="标题" min-width="220px"></vccol>
	            <vccol field="Df_IsOn" caption="启用" type="Boolean" width="60px"></vccol>
	            <vccol field="Df_Flow" caption="关联的流程"></vccol>
	            <vccol field="Df_Task" caption="关联的业务" type="dict" dict="KB_CCflowTask"></vccol>


	            <vccol field="Df_DeptName" caption="关联的部门名称"></vccol>
	            <vccol field="Df_Idx" caption="排序号"></vccol>
	            <vccol field="Df_NodeSelect" caption="可选节点"></vccol>
	            <vccol field="Df_NodeIDs" caption="可选的节点"></vccol>
	            <vccol field="Df_ToMan" caption="接受人控制"></vccol>
	            <vccol field="Df_ToManFix" caption="固定的接受人"></vccol>
	            <vccol field="Df_ToManNames" caption="固定的接受人名称"></vccol>
	            <vccol field="Df_Note" caption="流转说明"></vccol>
	            <vccol field="Df_Para1" caption="额外参数1"></vccol>
	            <vccol field="Df_Para2" caption="额外参数2"></vccol>

	            <vccol field="Df_Created" caption="创建时间" width="130px"></vccol>
	            <vccol field="Df_Updated" caption="更新时间" width="130px"></vccol>

	            <vccol field="Df_CreateMan" caption="创建人" width="90px"></vccol>
	            <vccol field="Df_UpdateMan" caption="更新人" width="90px"></vccol>
            </vclist>
        </template>
        <template #footer="p">
            <el-dialog title="编辑" class="normal" v-model="diaRow" width="70vw" append-to-body @close="diaRow=false">
                <el-tabs type="border-card">
                    <el-tab-pane label="表单">
                        <div style="height:70vh;overflow:auto;">
                            <KBDeptFlowConfigform :row="focusRow" :adding="adding" @close="onclosefm" @refresh="getlist">
                            </KBDeptFlowConfigform>                            
                        </div>
                    </el-tab-pane>
                    <el-tab-pane label="节点配置" lazy>
                        <KBDfNodeConfiglist :dfconfig="focusRow" style="height:70vh;">
                        </KBDfNodeConfiglist>
                    </el-tab-pane>
                </el-tabs>

            </el-dialog>
        </template>
    </flexpan>
</template>

<script>
/**
	            <vccol field="Df_OID" caption="主键"></vccol>
	            <vccol field="Df_CreatedAcount" caption="创建人"></vccol>
	            <vccol field="Df_UpdatedAcount" caption="更新人"></vccol>
	            <vccol field="Df_DeptOID" caption="关联的部门"></vccol>
	            <vccol field="Df_Type_FK" caption="绑定的分类"></vccol>
 */


import { selectKBDeptFlowConfiglist ,removeKBDeptFlowConfig,copyKBDeptFlowConfigById} from "@s/api/ccflow/KBDeptFlowConfigapi.js"

import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"

import KBDeptFlowConfigform from "./KBDeptFlowConfigform.vue"
import KBDfNodeConfiglist from "@s/components/CCflow/dfnodeconfig/KBDfNodeConfiglist.vue"

export default {
    name:"cnPagelist",
    components: {
        flexpan,vclist,vccol,KBDeptFlowConfigform,KBDfNodeConfiglist
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
           
            selectKBDeptFlowConfiglist(this.listpara).then((res)=>{
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
                //FK_channel:this.channel.pc_id,
            };
            this.diaRow=true;
        },
        //
        openRow(row){
            this.adding=false;
            this.focusRow=row;
            this.diaRow=true;
        },
        copyRow(row){
            this.$confirm('确定要复制吗?', '提示', {confirmButtonText: '确定',cancelButtonText: '取消',type: 'warning'})
            .then(() => {
                copyKBDeptFlowConfigById(row.Df_OID).then((res)=>{
                    this.$message({ type: 'success', message: res.msg});
                    this.getlist();
                })
                
            })
        },
        removeRow(row){
            this.$confirm('此操作将永久删除该记录, 是否继续?', '提示', {confirmButtonText: '确定',cancelButtonText: '取消',type: 'warning'})
            .then(() => {
                removeKBDeptFlowConfig(row).then((res)=>{
                    this.$message({ type: 'success', message: res.msg});
                    this.getlist();
                })
                
            })
        },
    }
}
</script>