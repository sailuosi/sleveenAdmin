<template>
    <flexpan flex>
        <template #header="p">
            <el-form inline ref="ruleForm" :model="queryParams" label-width="100px" @submit.native.prevent>
                <el-form-item label="标题" prop="A_Title">
                    <el-input v-model="queryParams.A_Title" placeholder="标题" clearable 
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item label="内容key" prop="A_Key">
                    <el-input v-model="queryParams.A_Key" placeholder="流程名称" clearable 
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item>
                    <el-button type="primary" icon="el-icon-search"  @click="getlist">查询</el-button>
                    <el-button type="info"  @click="resetQuery">重置</el-button>
                    <el-button type="primary"  @click="openAdd">添加</el-button>
                </el-form-item>
            </el-form>  
        </template>
        <template #default="p">
            <vclist :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="(p.height-10)" orderCol :maxLine="1" :checkable="checkable"
                rowKey="saId" v-model:checked="cking"
            >
                <el-table-column v-if="!selecting"   fixed="left" label="操作" width="110px">
                    <template #default="p">
                        <el-link type="primary" @click="openRow(p.row)" >编辑</el-link>
                        <el-link type="danger" @click="removeRow(p.row)" style="margin-left:10px;">删除</el-link>
                    </template>
                </el-table-column>                
                <slot></slot>

	            <vccol field="A_IsOn" caption="启用" type="Boolean" width="60px"></vccol>
	            <vccol field="A_Title" caption="标题" align="left" min-width="200px"></vccol>
	            <vccol field="A_Type" caption="类型" align="left" min-width="110px" type="dict" dict="SysAppletType"></vccol>
	            <vccol field="A_Key" caption="内容key" min-width="300px" align="left"></vccol>
	            <vccol field="A_TitleAlign" caption="显示标题" type="dict" dict="SysAppletTitle" width="90px"></vccol>

	            <vccol field="A_Para" caption="内容参数"></vccol>

	            <vccol field="A_Remark" caption="备注"></vccol>

	            <vccol field="A_Idx" caption="排序号" width="60px"></vccol>
            </vclist>
        </template>
        <template #footer="p">
            <el-dialog title="编辑" class="normal" v-model="diaRow" width="70vw" append-to-body @close="diaRow=false">
                <ptappletform :row="focusRow" :adding="adding" @close="diaRow=false" @refresh="getlist">
                </ptappletform>
            </el-dialog>
        </template>
    </flexpan>
</template>

<script>
/**
                <!-- comment some unuseful column in 门户组件库 edit by yudehai-->
	            <!-- <vccol field="A_Icon" caption="图标"></vccol> -->
	            <!-- <vccol field="A_Tool" caption="标题工具栏"></vccol> -->

	            <vccol field="A_Slot" caption="缺省内容"></vccol>
	            <vccol field="A_ParaVal" caption="默认参数"></vccol>
	            <vccol field="A_Para2" caption="第2参数"></vccol>
	            <vccol field="A_BodyStyle" caption="内容样式"></vccol>
	            <vccol field="A_Color" caption="颜色"></vccol>
	            <vccol field="A_Theme" caption="主题"></vccol> 
 	            <vccol field="A_TitleStyle" caption="标题样式"></vccol>
	            <vccol field="saId" caption=""></vccol>
	            <vccol field="SYS_Created" caption="创建日期"></vccol>
	            <vccol field="SYS_LAST_UPD" caption="最后修改日期"></vccol>
	            <vccol field="SYS_Deleted" caption="记录删除标记"></vccol>
 	            <vccol field="SYS_CreatedBy" caption="创建用户"></vccol>
	            <vccol field="SYS_REPLACEMENT" caption="授权用户"></vccol>
	            <vccol field="SYS_POSTN" caption="所属岗位"></vccol>
	            <vccol field="SYS_DIVISION" caption="所属部门"></vccol>
	            <vccol field="SYS_ORG" caption="所属组织"></vccol>
	            <vccol field="SYS_LAST_UPD_BY" caption="最后修改用户"></vccol>
	            <vccol field="saAppletCataId" caption="组件分类"></vccol>
 */
import { fetchSysAppletPage ,deleteSysApplet} from "@s/api/paotou/paotalapi.js"

import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"

import ptappletform from "./ptappletform.vue"

export default {
    name:"cnPagelist",
    components: {
        flexpan,vclist,vccol,ptappletform
    },
    props:{
        cata:null,
        selecting:{ type:Boolean,default:false },
        checkable:{ type:Boolean,default:false },
    },
    emits:[],
    expose:["getChecked"],
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
            appletType:{
                "1":"vue组件",
                "2":"内嵌url",
                "3":"本地BPO",
                "4":"echart",
                "5":"原生iframe",
            },
            cking:[],
        };
    },
    computed:{
    },
    mounted() {
        this.getlist();
    },
    watch:{
        "cata":function(){
            this.getlist();
        },
    },
    methods:{
        getChecked(){
            return this.cking;
        },
        getlist(){
            this.queryParams.saAppletCataId=this.cata.sacId;
            
            fetchSysAppletPage(this.queryParams).then((res)=>{
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
        openAdd(){
            if(!this.cata.sacId){
                this.$message({ type: 'error', message: "请先选择组件分类~"});
                return ;
            }
            this.adding=true;
            this.focusRow={
                saAppletCataId:this.cata.sacId,
                A_IsOn:true,
                A_Title:"标题",
                A_TitleAlign:"1",
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
                deleteSysApplet(row).then((res)=>{
                    this.$message({ type: 'success', message: res.msg});
                    this.getlist();
                })
                
            })
        },
    }
}
</script>