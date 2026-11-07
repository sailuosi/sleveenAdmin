<template>
    <flexpan flex>
        <template #header="p">
            <el-form inline ref="ruleForm" :model="queryParams" label-width="60px" @submit.native.prevent>
                <el-form-item label="名称" prop="Vb_Name">
                    <el-input v-model="queryParams.Vb_Name" placeholder="名称" clearable style="width: 200px;"
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item label="描述" prop="Vb_Note">
                    <el-input v-model="queryParams.Vb_Note" placeholder="描述" clearable style="width: 200px;"
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item>
                    <el-button type="primary" icon="el-icon-search"  @click="getlist">查询</el-button>
                    <el-button  @click="resetQuery">重置</el-button>
                    <el-button  @click="openAdd" type="warning">添加</el-button>
                </el-form-item>
            </el-form>  
        </template>
        <template #default="p">
            <vclist :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="p.height"
                :maxLine="1"
            >
                <el-table-column    fixed="left" label="操作" width="110px">
                    <template #default="p">
                        <el-link type="primary" @click="openRow(p.row)" >编辑</el-link>
                        <el-link type="danger" @click="removeRow(p.row)" style="margin-left:10px;">删除</el-link>
                    </template>
                </el-table-column>                
                <slot></slot>


	            <vccol field="Vb_Name" caption="名称" min-width="180px"></vccol>
	            <vccol field="Vb_Note" caption="描述" min-width="230px"></vccol>
	            <vccol field="Vb_Code" caption="编号"></vccol>
	            <vccol field="Vb_Remark" caption="备注"></vccol>
	            <vccol field="Vb_Type" caption="类型"></vccol>
	            <vccol field="Vb_KeyWord" caption="关键成员"></vccol>
	            <vccol field="Vb_Idx" caption="序号"></vccol>
	            <vccol field="SYS_Created" caption="创建日期" width="130px"></vccol>
	            <vccol field="SYS_LAST_UPD" caption="最后修改日期" width="130px"></vccol>
	            <vccol field="SYS_Deleted" caption="记录删除标记"></vccol>
            </vclist>
        </template>
        <template #footer="p">
            <el-dialog title="编辑" class="normal" v-model="diaRow" width="70vw" append-to-body @close="diaRow=false">
                <el-tabs type="border-card">
                    <el-tab-pane label="访客集合">
                        <div class="exp-page">
                            <HHVisitorBagform :row="focusRow" :adding="adding" @close="onclosefm" @refresh="getlist">
                            </HHVisitorBagform>                          
                        </div>
               
                    </el-tab-pane>
                    <el-tab-pane label="访客明细" lazy>
                        <HHVisitorItemlist class="exp-page"
                            :bag="focusRow"
                        ></HHVisitorItemlist>
                    </el-tab-pane>
                </el-tabs>

            </el-dialog>
        </template>
    </flexpan>
</template>

<script>
/**
	            <vccol field="SYS_CreatedBy" caption="创建用户"></vccol>
	            <vccol field="SYS_REPLACEMENT" caption="授权用户"></vccol>
	            <vccol field="SYS_POSTN" caption="所属岗位"></vccol>
	            <vccol field="SYS_DIVISION" caption="所属部门"></vccol>
	            <vccol field="SYS_ORG" caption="所属组织"></vccol>
	            <vccol field="SYS_LAST_UPD_BY" caption="最后修改用户"></vccol>
	            <vccol field="HH_Parent_FK" caption="父级访客集合"></vccol>
	            <vccol field="HH_VisitorBagOID" caption=""></vccol>
 */


import { selectHHVisitorBaglist ,removeHHVisitorBag} from "../../../api/HHVisitorBagapi.js"

import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"

import HHVisitorBagform from "./HHVisitorBagform.vue"
import HHVisitorItemlist from "../visitoritem/HHVisitorItemlist.vue"


export default {
    name:"cnPagelist",
    components: {
        flexpan,vclist,vccol,HHVisitorBagform,
        HHVisitorItemlist,
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
           
            selectHHVisitorBaglist(this.listpara).then((res)=>{
                this.mlist=res.data;
                this.total=res.total;
            })
        },
        resetQuery(){
            this.queryParams={
                pageSize:100,
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
        openRow(row){
            this.adding=false;
            this.focusRow=row;
            this.diaRow=true;
        },
        removeRow(row){
            this.$confirm('此操作将永久删除该记录, 是否继续?', '提示', {confirmButtonText: '确定',cancelButtonText: '取消',type: 'warning'})
            .then(() => {
                removeHHVisitorBag(row).then((res)=>{
                    this.$message({ type: 'success', message: res.msg});
                    this.getlist();
                })
                
            })
        },
    }
}
</script>


<style scoped>
.exp-page{
    height:70vh;
}
</style>