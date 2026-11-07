<template>
    <flexpan flex>
        <template #header="p">
            <el-form inline ref="ruleForm" :model="queryParams" label-width="60px" @submit.native.prevent>
                <el-form-item label="名称" prop="Wi_Name">
                    <el-input v-model="queryParams.Wi_Name" placeholder="名称" clearable style="width: 200px;"
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item>
                    <el-button type="primary" @click="getlist">查询</el-button>
                    <el-button style="margin-left: 10px;" @click="resetQuery">重置</el-button>
                    
                    <wordselect v-model:word="selectWord" style="width: 260px; margin: 0 10px;"></wordselect>
                    <el-button type="warning" @click="openAdd">添加</el-button>
                </el-form-item>
            </el-form>
        </template>
        <template #default="p">
            <vclist :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="p.height">
                <el-table-column    fixed="left" align="center" label="操作" width="110px">
                    <template #default="p">
                        <el-link type="primary" @click="openRow(p.row)" >编辑</el-link>
                        <el-link type="danger" @click="removeRow(p.row)" style="margin-left:10px;">删除</el-link>
                    </template>
                </el-table-column>                
                <slot></slot>

	            <vccol field="Wi_Name" caption="名称"></vccol>
	            <vccol field="Wi_Code" caption="编号"></vccol>
	            <vccol field="Wi_Type" caption="类型"></vccol>
	            <vccol field="Wi_Note" caption="说明"></vccol>

            </vclist>
        </template>
        <template #footer="p">
            <el-dialog title="编辑" class="normal" v-if="diaRow" v-model="diaRow" width="600px" append-to-body @close="diaRow=false">
                <HHWordItemform :row="focusRow" :adding="adding" @close="onclosefm" @refresh="getlist">
                </HHWordItemform>
            </el-dialog>
        </template>
    </flexpan>
</template>

<script>
/**
 	            <vccol field="HH_WordItemOID" caption=""></vccol>
	            <vccol field="HH_WordBag_FK" caption="词条集合"></vccol>
	            <vccol field="HH_WordDefine_FK" caption="词条定义"></vccol>
 */

import { selectHHWordItemlist ,removeHHWordItem} from "../../../api/HHWordItemapi.js"

import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"

import HHWordItemform from "./HHWordItemform.vue"

import wordselect from "../../word/wordselect.vue"


export default {
    name:"cnPagelist",
    components: {
        flexpan,vclist,vccol,HHWordItemform,
        wordselect,
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
            selectWord:null,
        };
    },
    computed:{
        listpara(){
            var obj=Object.assign({
                HH_WordBag_FK:this.bag.HH_WordBagOID
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
           
            selectHHWordItemlist(this.listpara).then((res)=>{
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
        openAdd() {
            if (!this.selectWord) {
                this.$message("请先选择词条")
                return ;
            }
            this.adding = true;
            
            this.focusRow = {
                HH_WordBag_FK: this.bag.HH_WordBagOID,
                Wi_Name: this.selectWord.name,
                Wi_Code: this.selectWord.Wd_Code,
                Wi_Type: this.selectWord.Wd_Type,
                HH_WordDefine_FK: this.selectWord.oid,
            };
            this.diaRow = true;
        },
        openRow(row){
            this.adding=false;
            this.focusRow=row;
            this.diaRow=true;
        },
        removeRow(row){
            this.$confirm('此操作将永久删除该记录, 是否继续?', '提示', {confirmButtonText: '确定',cancelButtonText: '取消',type: 'warning'})
            .then(() => {
                removeHHWordItem(row).then((res)=>{
                    this.$message({ type: 'success', message: res.msg});
                    this.getlist();
                })
                
            })
        },
    }
}
</script>