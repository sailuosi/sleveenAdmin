<template>
    <flexpan flex>
        <template #header="p">
            <el-form inline ref="ruleForm" :model="queryParams" label-width="60px" @submit.native.prevent>
                <el-form-item label="名称" prop="Wd_Name">
                    <el-input v-model="queryParams.Wd_Name" placeholder="名称" clearable style="width: 200px;"
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item label="编号" prop="Wd_Code">
                    <el-input v-model="queryParams.Wd_Code" placeholder="编号" clearable style="width: 200px;"
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
            <vclist orderCol :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="p.height"
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


	            <vccol field="Wd_Name" caption="名称" min-width="220px"></vccol>
	            <vccol field="Wd_Code" caption="编号" width="90px"></vccol>
                <vccol field="Wd_IsOn" caption="启用" type="Boolean" width="60px"></vccol>
	            <vccol field="Wd_Type" caption="类型"></vccol>
	            <vccol field="Wd_Para" caption="解析参数"></vccol>
	            <vccol field="Wd_Parser" caption="解析器"></vccol>
	            <vccol field="Wd_Idx" caption="序号"></vccol>
	            <vccol field="SYS_Created" caption="创建日期" width="130px"></vccol>
	            <vccol field="SYS_LAST_UPD" caption="最后修改日期" width="130px"></vccol>
	            <vccol field="SYS_Deleted" caption="记录删除标记"></vccol>
            </vclist>
        </template>
        <template #footer="p">
            <el-dialog title="编辑" class="normal" v-model="diaRow" width="70vw" append-to-body @close="diaRow=false">
                <HHWordDefineform :row="focusRow" :adding="adding" @close="onclosefm" @refresh="getlist">
                </HHWordDefineform>
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
	            <vccol field="HH_WordDefineOID" caption=""></vccol>
 */


import { selectHHWordDefinelist ,removeHHWordDefine} from "../../../api/HHWordDefineapi.js"

import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"

import HHWordDefineform from "./HHWordDefineform.vue"

export default {
    name:"cnPagelist",
    components: {
        flexpan,vclist,vccol,HHWordDefineform
    },
    props:{
        channel:null,
        opWidth:{ type:String,default:"110px"},
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
           
            selectHHWordDefinelist(this.listpara).then((res)=>{
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
                removeHHWordDefine(row).then((res)=>{
                    this.$message({ type: 'success', message: res.msg});
                    this.getlist();
                })
                
            })
        },
    }
}
</script>