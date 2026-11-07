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
                    <el-button  @click="openIndu">引入</el-button>
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

	            <vccol field="PS_IsOn" caption="启用" type="Boolean" width="90px"></vccol>
	            <vccol field="PS_Title" caption="标题"></vccol>
                <vccol field="PS_Nick" caption="简称"></vccol>
	            <vccol field="PS_Table" caption="表名"></vccol>
	            <vccol field="PS_Key" caption="唯一编码"></vccol>
                <vccol field="PS_SQL" caption="查询SQL"></vccol>
	            <vccol field="PS_Note" caption="说明"></vccol>
	            <vccol field="PS_Remark" caption="备注"></vccol>
	            <vccol field="PS_Task" caption="业务名称"></vccol>
	            <vccol field="PS_Para" caption="参数"></vccol>
	            <vccol field="PS_Content" caption="内容"></vccol>

	            <vccol field="spPortPageId" caption="门户页面"></vccol>
            </vclist>
        </template>
        <template #footer="p">
            <el-dialog title="编辑" class="normal" v-model="diaRow" width="70vw" append-to-body @close="diaRow=false">
                <el-tabs type="border-card">
                    <el-tab-pane label="查询配置">
                        <div style="height:70vh;overflow:auto;">
                            <pageSelectform :row="focusRow" :adding="adding" @close="onclosefm" @refresh="getlist" >
                            </pageSelectform>
                        </div>

                    </el-tab-pane>
                    <el-tab-pane label="字段配置" lazy>
                        <pagefieldlist :page="page" :table="focusRow" style="height:70vh;"></pagefieldlist>
                    </el-tab-pane>
                </el-tabs>

            </el-dialog>
            <el-dialog title="编辑" class="normal" v-model="seeSelecting" width="400" append-to-body @close="seeSelecting=false">
                <el-checkbox-group v-model="checked">
                    <el-checkbox v-for="(li,i) in selecting" :label="li.value">【{{li.title}}】{{li.label}}</el-checkbox>
                </el-checkbox-group>
                <el-button type="primary" size="default" @click="doaddS">执行引入</el-button>
                <el-button type="primary" size="default" @click="copySelectIdx">复制排序</el-button>
            </el-dialog>
        </template>
    </flexpan>
</template>

<script>
/**
<vccol field="spsId" caption=""></vccol>
 */


//import { fetchSysPageSelectPage ,removeZHPageSelect,loadPageTemplateSelects,addBySelected,copyIdx} from "@/api/system/paotou/pageselectapi.js"

import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"

import pageSelectform from "./pageSelectform.vue"
import pagefieldlist from "./pagefieldlist.vue"

export default {
    name:"pageselectlist",
    components: {
        flexpan,vclist,vccol,pageSelectform,pagefieldlist
    },
    props:{
        page:null,
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
            //复制选择
            seeSelecting:false,
            selecting:[],
            checked:[],
        };
    },
    computed:{
        listpara(){
            var obj=Object.assign({
                spPortPageId:this.page.sppId
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

            fetchSysPageSelectPage(this.listpara).then((res)=>{
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
                spPortPageId:this.page.sppId,
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
                removeZHPageSelect(row).then((res)=>{
                    this.$message({ type: 'success', message: res.data});
                    this.getlist();
                })

            })
        },
        openIndu(){
            loadPageTemplateSelects({}).then((res)=>{
                this.selecting=res.data;
                this.seeSelecting=true;
            })
        },
        doaddS(){
            var para={
                spPortPageId:this.page.sppId,
                spsId:this.checked.join(",")
            }
            addBySelected(para).then((res)=>{

                const {data,status}=res;
                const msg= status=='success'?data:"添加失败";
                this.$message({ type: 'success', message: msg});
                this.seeSelecting=false;
                this.getlist();
            })
        },
        copySelectIdx(){
            var para={
                spPortPageId:this.page.sppId,
                spsId:this.checked.join(",")
            }
            copyIdx(para).then((res)=>{
                this.$message({ type: 'success', message: res.data});
                this.getlist();
            })
        },
    }
}
</script>
