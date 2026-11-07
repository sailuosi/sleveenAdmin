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
                    <el-button  @click="openSelectPortal">添加</el-button>
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
            
	            <vccol field="PL_Name" caption="关联对象名称" min-width="150px"></vccol>
	            <vccol field="PL_IsOn" caption="启用" type="Boolean" width="60px"></vccol>
	            
	            <vccol field="PL_Title" caption="标题"  min-width="150px"></vccol>
                <vccol field="PL_Key" caption="识别码" width="170px"></vccol>
	            <vccol field="PL_Code" caption="关联对象编号"></vccol>
	            <vccol field="PL_Type" caption="关联类型"></vccol>
	            <vccol field="PL_Style" caption="样式"></vccol>
	            <vccol field="PL_Place" caption="位置"></vccol>
	            <vccol field="PL_Css" caption="Css"></vccol>
	            <vccol field="PL_Para" caption="参数"></vccol>
	            <vccol field="PL_Note" caption="说明"></vccol>
                <vccol field="PL_Idx" caption="排序" type="number" width="60px"></vccol>
	            
	            <vccol field="SYS_Created" caption="创建日期" width="170px"></vccol>
	            <vccol field="SYS_LAST_UPD" caption="最后修改日期" width="170px"></vccol>
            </vclist>
        </template>
        <template #footer="p">
            <el-dialog title="编辑" class="normal" v-model="diaRow" width="70vw" append-to-body @close="diaRow=false">
                <ptlinkform :row="focusRow" :adding="adding" @close="onclosefm" @refresh="getlist">
                </ptlinkform>
            </el-dialog>
            <el-dialog title="选择门户" class="normal" v-model="seePortal" width="70vw" append-to-body @close="seePortal=false">
                <portalselectlist style="height:70vh;"
                    @select="doAdd"
                >

                </portalselectlist>
            </el-dialog>
        </template>
    </flexpan>
</template>

<script>
/// 门户关联列表

/**
<vccol field="splId" caption=""></vccol>
<vccol field="PL_Target_FK" caption="关联业务主键"></vccol>
	            <vccol field="SYS_CreatedBy" caption="创建用户"></vccol>
	            <vccol field="SYS_REPLACEMENT" caption="授权用户"></vccol>
	            <vccol field="SYS_POSTN" caption="所属岗位"></vccol>
	            <vccol field="SYS_DIVISION" caption="所属部门"></vccol>
	            <vccol field="SYS_ORG" caption="所属组织"></vccol>
	            <vccol field="SYS_LAST_UPD_BY" caption="最后修改用户"></vccol>
	            <vccol field="spPortalId" caption="门户"></vccol>
                <vccol field="SYS_Deleted" caption="记录删除标记"></vccol>
 */

//import { fetchSysPortalLinkPage ,deleteSysPortalLink,addZHPaotalLink} from "@/api/system/paotou/portalinkapi.js"

import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"
//业务组件
import ptlinkform from "./ptlinkform.vue"
import portalselectlist from "../selector/portalselectlist.vue"

export default {
    name:"cnPagelist",
    components: {
        flexpan,vclist,vccol,ptlinkform,portalselectlist
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
        };
    },
    computed:{
        listpara(){
            var obj=Object.assign({
                PL_Target_FK:this.targetfk,
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
           
            fetchSysPortalLinkPage(this.listpara).then((res)=>{
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
        openSelectPortal(){
            this.seePortal=true;
        },
        doAdd(list){
            var para=[];
            list.forEach((li,l)=>{
                var obj={
                    //PL_Target_FK:
                    //PL_Name:
                    //PL_Code:
                    PL_IsOn:true,
                    PL_Idx:1,
                    PL_Key:li.P_Key,
                    spPortalId:li.spId,
                    PL_Title:li.P_Name,
                }
                if(typeof this.onAdd=="function"){
                    obj=this.onAdd(obj,li);
                }
                para.push(obj);
            });

            addZHPaotalLink(para).then((res)=>{
                this.$message(res.data);
                this.getlist();
            })
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
                deleteSysPortalLink(row).then((res)=>{
                    this.$message({ type: 'success', message: res.msg});
                    this.getlist();
                })
                
            })
        },
    }
}
</script>