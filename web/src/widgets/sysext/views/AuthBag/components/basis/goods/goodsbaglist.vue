<template>
    <flexpan flex>
        <template #header="p">
            <el-form inline ref="ruleForm" :model="queryParams" label-width="60px" @submit.native.prevent>
                <el-form-item label="名称" prop="Gb_Name">
                    <el-input v-model="queryParams.Gb_Name" placeholder="名称" clearable style="width: 200px;"
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item label="说明" prop="Gb_Note">
                    <el-input v-model="queryParams.Gb_Note" placeholder="说明" clearable style="width: 200px;"
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
                <el-table-column    fixed="left" label="操作" :width="opWidth">
                    <template #default="p">
                        <el-link type="primary" @click="openRow(p.row)" >编辑</el-link>
                        <el-link type="danger" @click="removeRow(p.row)" style="margin-left:10px;">删除</el-link>
                        <slot name="opmore" v-bind="p"></slot>
                    </template>
                </el-table-column>                
                <slot></slot>


	            <vccol field="Gb_Name" caption="名称" min-width="250px" align="left"></vccol>
	            <vccol field="Gb_Note" caption="说明" min-width="300px" show-overflow-tooltip align="left"></vccol>
	            <vccol field="Gb_Remark" caption="备注" width="150px" show-overflow-tooltip align="left"></vccol>
	            <vccol field="Gb_Type" caption="类型" width="90px"></vccol>
	            <vccol field="Gb_Code" caption="编号" width="100px"></vccol>
	            <vccol field="Gb_Idx" caption="序号" width="90px"></vccol>
	            <vccol field="SYS_Created" caption="创建日期" width="180px"></vccol>
	            <vccol field="SYS_LAST_UPD" caption="最后修改日期" width="180px"></vccol>
	                        

            </vclist>
        </template>
        <template #footer="p">
            <el-dialog title="编辑" class="normal" v-model="diaRow" width="70vw" append-to-body @close="diaRow=false">
                <el-tabs type="border-card">
                    <el-tab-pane label="资源集合">
                        <div class="exp-page">
                            <goodsbagform :row="focusRow" :adding="adding" @close="onclosefm" @refresh="getlist">
                            </goodsbagform>                             
                        </div>
               
                    </el-tab-pane>
                    <el-tab-pane label="明细列表">
                        <goodsitemlist class="exp-page"
                            :bag="focusRow"
                        ></goodsitemlist>
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
	            <vccol field="HH_GoodsBagOID" caption=""></vccol>
<vccol field="SYS_Deleted" caption="记录删除标记"></vccol>    
 */


import { selectHHGoodsBaglist ,removeHHGoodsBag} from "../../../api/goodsbagapi.js"

import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"

import goodsbagform from "./goodsbagform.vue"
import goodsitemlist from "./goodsitemlist.vue"


export default {
    name:"goodsbaglist",
    components: {
        flexpan,vclist,vccol,goodsbagform,
        goodsitemlist,
    },
    props:{
        channel:null,
        opWidth:{ type:String,default:"110px" },
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
           
            selectHHGoodsBaglist(this.listpara).then((res)=>{
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
                removeHHGoodsBag(row).then((res)=>{
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