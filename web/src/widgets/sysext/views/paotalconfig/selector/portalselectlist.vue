<template>
    <flexpan flex>
        <template #header="p">
            <el-form inline ref="ruleForm" :model="queryParams" label-width="100px" @submit.native.prevent>
                <el-form-item label="名称" prop="P_Name">
                    <el-input v-model="queryParams.P_Name" placeholder="标题" clearable 
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item label="唯一编码" prop="P_Key">
                    <el-input v-model="queryParams.P_Key" placeholder="唯一编码" clearable 
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item>
                    <el-button type="primary" icon="el-icon-search"  @click="getlist">查询</el-button>
                    <el-button type="info" @click="resetQuery">
                        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1024 1024" data-v-ea893728=""><path fill="currentColor" d="M784.512 230.272v-50.56a32 32 0 1 1 64 0v149.056a32 32 0 0 1-32 32H667.52a32 32 0 1 1 0-64h92.992A320 320 0 1 0 524.8 833.152a320 320 0 0 0 320-320h64a384 384 0 0 1-384 384 384 384 0 0 1-384-384 384 384 0 0 1 643.712-282.88z"></path></svg>
                        重置
                    </el-button>
                    <el-button  @click="doselect" type="warning">选择</el-button>
                </el-form-item>
            </el-form>  
        </template>
        <template #default="p">
            <vclist v-loading="loading" :list="mlist" :total="total" :para="queryParams" @query="getlist" 
                v-model:checked="checked" checkable
            :height="(p.height-5)">

                <slot></slot>
				
	            <vccol field="P_IsOn" caption="启用" width="60px" type="Boolean"></vccol>
	            <vccol field="P_Name" caption="名称" width="250px" align="left"></vccol>
	            <vccol field="P_Key" caption="唯一编码" width="150px" align="left"></vccol>
	            <vccol field="P_Pulic" caption="共享" width="60px"></vccol>                
	            <vccol field="P_Note" caption="说明" align="left" min-width="300px"></vccol>
	            <vccol field="P_Theme" caption="主题" width="60px"></vccol>
	            <vccol field="P_Idx" caption="序号" width="60px"></vccol>
	            <vccol field="P_Group" caption="分组"></vccol>
	            <vccol field="P_ShowTitle" caption="显示标题"></vccol>

	            <vccol field="P_Icon" caption="图标"></vccol>
	            <vccol field="P_Css" caption="类"></vccol>
	            <vccol field="P_Display" caption="布局方式"></vccol>
	            <vccol field="P_ColNum" caption="列数" width="60px"></vccol>
	            <vccol field="P_Rows" caption="行高" width="60px"></vccol>
	            <vccol field="P_Editable" caption="可拖动" type="Boolean" width="60px"></vccol>
	            <vccol field="P_Remark" caption="备注"></vccol>
	            <vccol field="P_Task" caption="业务"></vccol>

	            <vccol field="SYS_Created" caption="创建日期" type="datetime" width="130px"></vccol>
	            <vccol field="SYS_LAST_UPD" caption="最后修改日期" type="datetime" width="130px"></vccol>

            </vclist>
        </template>
        <template #footer="p">
            <el-dialog title="编辑" class="normal" v-model="diaRow" width="70vw" append-to-body @close="diaRow=false">
  
            </el-dialog>
        </template>
    </flexpan>
</template>

<script>

import { fetchSysPortalPage ,deleteSysPortal} from "@s/api/paotou/paotalapi.js"

import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"



export default {
    name:"paotallist",
    components: {
        flexpan,vclist,vccol
    },
    props:{
        page:null,
        para:null,
    },
    emits:["select"],
    data(){
        return {
            checked:[],
            mlist:[],
            total:0,  
            queryParams:{
                pageSize:30,
                pageNum:1,
            },
            focusRow:{},
            diaRow:false,//行编辑表单
            adding:false,
            loading:false,
        };
    },
    mounted() {
        this.getlist();
    },
    watch:{
        "page":function(){
            this.getlist();
        },
        "para":function(){
            this.getlist();
        }
    },
    methods:{
        doselect() {
        	if (!this.checked || this.checked.length == 0) {
        		this.$message({
        			type: 'warning',
        			message: "请先选择门户！"
        		});
        		return;
        	}
            this.$emit("select", this.checked);
        },
        getlist(){
            if(!!this.page){
                this.queryParams.spPortPageId=this.page.sppId;
            }
            var para= Object.assign({},this.para,this.queryParams);
            this.loading=true;
            fetchSysPortalPage(para).then((res)=>{
                this.mlist=res.data;
                this.total=res.total;
                this.loading=false;
            })
        },
        resetQuery(){
            this.queryParams={
                pageSize:30,
                pageNum:1,
            };
        },
        openAdd(){
            this.adding=true;
            this.focusRow={
                //FK_channel:this.channel.pc_id,
            };
            if(!!this.page){
                this.focusRow.spPortPageId=this.page.sppId;
            }
            this.diaRow=true;
        },
        designRow(row, isCopy) {
            this.$router.push({
                path: "portalDesign",
                query: {
                    name: row.P_Key,
                    isCopy: isCopy
                }
            });
        },
        openRow(row){
            this.adding=false;
            this.focusRow=row;
            this.diaRow=true;
        },
        removeRow(row){
            this.$confirm('此操作将永久删除该记录, 是否继续?', '提示', {confirmButtonText: '确定',cancelButtonText: '取消',type: 'warning'})
            .then(() => {
                deleteSysPortalPage(row).then((res)=>{
                    this.$message({ type: 'success', message: res.msg});
                    this.getlist();
                })
                
            })
        },
    }
}
</script>