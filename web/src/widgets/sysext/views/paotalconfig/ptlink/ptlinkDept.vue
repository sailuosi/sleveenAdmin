<template>
    <flexpan flex>
        <template #header="p">
            <el-form inline ref="ruleForm" :model="queryParams" label-width="100px" @submit.native.prevent>
                <el-form-item label="标题" prop="PL_Title">
                    <el-input v-model="queryParams.PL_Title" placeholder="标题" clearable 
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item label="识别码" prop="PL_Key">
                    <el-input v-model="queryParams.PL_Key" placeholder="识别码" clearable 
                            @keyup.enter.native="getlist"></el-input>
                </el-form-item>
                <el-form-item>
                    <el-button type="primary" icon="el-icon-search"  @click="getlist">查询</el-button>
                    <el-button  @click="resetQuery">重置</el-button>
                </el-form-item>
            </el-form>  
        </template>
        <template #default="p">
            <vclist v-loading="loading" :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="p.height"
                :maxLine="1"
            >
                <el-table-column   fixed="left" label="操作" width="170px">
                    <template #default="p">
                        <el-link type="primary" @click="designRow(p.row, false)">设计</el-link>
                        <el-link type="primary" @click="designRow(p.row, true)" style="margin-left:10px;">复制</el-link>
                        <el-link type="primary" @click="openRow(p.row)" style="margin-left:10px;">编辑</el-link>
                    </template>
                </el-table-column>
                <slot></slot>
                <vccol field="OrgName" caption="单位" width="150px" align="left"></vccol>
                <vccol field="P_IsOn" caption="启用" width="60px" type="Boolean"></vccol>
                <vccol field="P_Name" caption="名称" width="250px" align="left"></vccol>
                <vccol field="P_Key" caption="唯一编码" width="180px" align="left"></vccol>
                <vccol field="P_Task" caption="业务" type="dict" dict="SysPtTask" width="100px"></vccol>
                <vccol field="P_Pulic" caption="共享" width="60px" type="dict" dict="SysShareStatus"></vccol>                
                <vccol field="P_Note" caption="说明" align="left" min-width="300px"></vccol>
                <vccol field="P_Theme" caption="主题" width="60px" type="dict" dict="SysPtCellTheme"></vccol>
                <vccol field="P_Idx" caption="排序号" width="60px"></vccol>
                <vccol field="P_Group" caption="分组"></vccol>
                <vccol field="P_ShowTitle" caption="显示标题" type="dict" dict="SysAppletTitle"></vccol>

                <vccol field="P_Icon" caption="图标"></vccol>
                <vccol field="P_Css" caption="类"></vccol>
                <vccol field="P_Display" caption="布局方式" type="dict" dict="SysPtDisplay"></vccol>
                <vccol field="P_ColNum" caption="列数" width="60px"></vccol>
                <vccol field="P_Rows" caption="行高" width="60px"></vccol>
                <vccol field="P_Editable" caption="可拖动" type="Boolean" width="60px"></vccol>
                <vccol field="P_Remark" caption="备注"></vccol>
            </vclist>
        </template>
        <template #footer="p">
            <el-dialog title="编辑" class="normal" v-model="diaRow" width="70vw" append-to-body @close="diaRow=false">
                <paotalform :row="focusRow" :adding="adding" @close="onclosefm" @refresh="getlist">
                </paotalform>
            </el-dialog>

        </template>
    </flexpan>
</template>

<script>
/// 门户关联列表



//import { selectPortLinkDeptTreeList ,deleteSysPortalLink,addZHPaotalLink} from "@/api/system/paotou/portalinkapi.js"

import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"
//业务组件
import paotalform from "../paotalform.vue"


export default {
    name:"cnPagelist",
    components: {
        flexpan,vclist,vccol,paotalform
    },
    props:{
        targetfk:null,
        target:null,
        onAdd:null,
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
            seePortal:false,
            loading:false,
        };
    },
    computed:{
        listpara(){
            var obj=Object.assign({
                PP_OrgOID:this.targetfk,
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
           this.loading=true;
            selectPortLinkDeptTreeList(this.listpara).then((res)=>{
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
        designRow(row, isCopy) {
            var page= this.$router.resolve({
                path: "/top/ptdesigner/"+row.P_Key,
                query: {
                    isCopy: isCopy
                }
            });
            console.log(page)
            window.open(page.href);
        },
        openRow(row){
            this.adding=false;
            this.focusRow=row;
            this.diaRow=true;
        },
    }
}
</script>