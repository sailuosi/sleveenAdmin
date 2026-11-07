<template>
    <flexpan flex>
        <template #header="p">
            <el-form inline ref="ruleForm" :model="queryParams" label-width="60px" @submit.native.prevent>
                <el-form-item label="名称" prop="Vi_Name">
                    <el-input v-model="queryParams.Vi_Name" placeholder="名称" clearable style="width: 200px;"
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item label="编号" prop="Vi_Code">
                    <el-input v-model="queryParams.Vi_Code" placeholder="编号" clearable style="width: 200px;"
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

	            <vccol field="Vi_IsOn" caption="启用" type="Boolean" width="60px"></vccol>
	            <vccol field="Vi_Type" caption="类型" type="dict" :dictlist="vtType"></vccol>
	            <vccol field="Vi_Name" caption="名称" min-width="130px"></vccol>
	            <vccol field="Vi_Code" caption="编号"></vccol>
	            
	            <vccol field="Vi_Src" caption="来源"></vccol>
	            <vccol field="Vi_Mode" caption="方式"></vccol>
	            <vccol field="Vi_Idx" caption="序号"></vccol>
	            <vccol field="Vi_OID" caption="OID" min-width="220px"></vccol>
            </vclist>
        </template>
        <template #footer="p">
            <el-dialog title="访客编辑" class="normal" v-model="diaRow" width="70vw" append-to-body @close="diaRow=false">
                <HHVisitorItemform :row="focusRow" :adding="adding" @close="onclosefm" @refresh="getlist">
                </HHVisitorItemform>
            </el-dialog>
        </template>
    </flexpan>
</template>

<script>
/**
	            <vccol field="HH_VisitorItemOID" caption=""></vccol>
                <vccol field="HH_VisitorBag_FK" caption="访客集合"></vccol>
 */


import { selectHHVisitorItemlist ,removeHHVisitorItem} from "../../../api/HHVisitorItemapi.js"
import { loadCode } from "@/utils/codevalue.js"

import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"

import HHVisitorItemform from "./HHVisitorItemform.vue"

export default {
    name:"cnPagelist",
    components: {
        flexpan,vclist,vccol,HHVisitorItemform
    },
    props:{
        bag:null,
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
            vtType:[],
        };
    },
    computed:{
        listpara(){
            var obj=Object.assign({
                HH_VisitorBag_FK:this.bag.HH_VisitorBagOID,
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
        this.loadCodes();
    },
    methods:{
        loadCodes(){
            loadCode("HH_VisitorType").then((res)=>{
                this.vtType=res;
            })
        },
        getlist(){
           
            selectHHVisitorItemlist(this.listpara).then((res)=>{
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
                HH_VisitorBag_FK:this.bag.HH_VisitorBagOID,
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
                removeHHVisitorItem(row).then((res)=>{
                    this.$message({ type: 'success', message: res.msg});
                    this.getlist();
                })
                
            })
        },
    }
}
</script>