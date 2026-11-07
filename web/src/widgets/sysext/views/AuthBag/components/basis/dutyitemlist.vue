<template>
    <flexpan flex>
        <template #header="p">
            <el-form inline ref="ruleForm" :model="queryParams" label-width="60px" @submit.native.prevent>
                <el-form-item label="名称" prop="Di_Name">
                    <el-input v-model="queryParams.Di_Name" placeholder="名称" clearable style="width: 200px;"
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item label="说明" prop="Di_Note">
                    <el-input v-model="queryParams.Di_Note" placeholder="说明" clearable style="width: 200px;"
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item>
                    <el-button type="primary" icon="el-icon-search"  @click="getlist">查询</el-button>
                    <el-button  @click="resetQuery">重置</el-button>
                    <el-button  @click="openAdd" type="warning">添加</el-button>
                    
                    <el-dropdown @command="cleanCache" style="margin-left: 10px;">
                        <el-button type="primary">
                            缓存清理<el-icon class="el-icon--right"><ArrowDown/></el-icon>
                        </el-button>
                        <template #dropdown>
                        <el-dropdown-menu>
                            <el-dropdown-item command="1">权限条目缓存</el-dropdown-item>
                            <el-dropdown-item command="2">用户所有缓存</el-dropdown-item>
                            <el-dropdown-item command="3">门户部门配置所有缓存</el-dropdown-item>
                        </el-dropdown-menu>
                        </template>
                    </el-dropdown>
                </el-form-item>
            </el-form>  
        </template>
        <template #default="p">
            <vclist :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="p.height" :maxLine="1">
                <el-table-column    fixed="left" label="操作" width="110px">
                    <template #default="p">
                        <el-link type="primary" @click="openRow(p.row)" >编辑</el-link>
                        <el-link type="danger" @click="removeRow(p.row)" style="margin-left:10px;">删除</el-link>
                        <slot name="opmore" v-bind="p"></slot>
                    </template>
                </el-table-column>                
                <slot></slot>

	            <vccol field="Di_Name" caption="名称" min-width="220px" align="left" show-overflow-tooltip></vccol>
	            
	            <vccol field="Di_Note" caption="说明" min-width="320px" show-overflow-tooltip align="left"></vccol>
	            
	            <vccol field="Di_Code" caption="编号" width="150px"></vccol>
                <vccol field="Di_Idx" caption="排序号" width="90px"></vccol>
                <vccol field="Di_Remark" caption="备注"></vccol>
	            <vccol field="SYS_Created" caption="创建日期" width="130px"></vccol>
	            <vccol field="SYS_LAST_UPD" caption="最后修改日期" width="130px"></vccol>

            </vclist>
        </template>
        <template #footer="p">
            <el-dialog title="编辑" class="normal" v-model="diaRow" width="70vw" append-to-body @close="diaRow=false">
                <dutyitemform :row="focusRow" :adding="adding" @close="onclosefm" @refresh="getlist">
                </dutyitemform>
            </el-dialog>
        </template>
    </flexpan>
</template>

<script>
/**
	            <vccol field="HH_DutyItemOID" caption=""></vccol>
	            <vccol field="SYS_CreatedBy" caption="创建用户"></vccol>
	            <vccol field="SYS_REPLACEMENT" caption="授权用户"></vccol>
	            <vccol field="SYS_POSTN" caption="所属岗位"></vccol>
	            <vccol field="SYS_DIVISION" caption="所属部门"></vccol>
	            <vccol field="SYS_ORG" caption="所属组织"></vccol>
	            <vccol field="SYS_LAST_UPD_BY" caption="最后修改用户"></vccol>
	            <vccol field="HH_VisitorBag_FK" caption="访客集合"></vccol>
	            <vccol field="HH_WordBag_FK" caption="词条集合"></vccol>
	            <vccol field="HH_GoodsBag_FK" caption="资源集合"></vccol>
                <vccol field="SYS_Deleted" caption="记录删除标记"></vccol>
 */


import { selectHHDutyItemlist ,removeHHDutyItem,cleanDutyCache} from "../../api/dutyitemapi.js"

import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"

import dutyitemform from "./dutyitemform.vue"

import { ArrowDown } from '@element-plus/icons-vue'

export default {
    name:"cnPagelist",
    components: {
        flexpan,vclist,vccol,dutyitemform,ArrowDown
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
           
            selectHHDutyItemlist(this.listpara).then((res)=>{
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
                removeHHDutyItem(row).then((res)=>{
                    this.$message({ type: 'success', message: res.msg});
                    this.getlist();
                })
                
            })
        },
        cleanCache(cmd){
            this.$confirm('此操作将清空所有权限条目下的缓存, 是否继续?', '提示', {confirmButtonText: '确定',cancelButtonText: '取消',type: 'warning'})
            .then(() => {
                cleanDutyCache({
                    Di_Name:cmd
                }).then((res)=>{
                    this.$message({ type: 'success', message: res.data});
                })
                
            })
        },
    }
}
</script>