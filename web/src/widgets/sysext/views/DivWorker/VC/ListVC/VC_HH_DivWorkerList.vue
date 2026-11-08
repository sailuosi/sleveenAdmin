<template>
    <flexpan :flex="flex" :height="height">
        <template #header="p">
            <teleport  :to="position" :disabled="!position" v-if="!hideToolbar">        
                <div class="vc-top-toolbar" >
                    <!-- #region autoGenerate type="cover" name="toolbar" -->
                    

                    <!-- #endregion autoGenerate type="cover" name="toolbar" --> 
                    <slot name="toolbar"></slot>
                </div>
            </teleport>

        </template>
        <template #default="p">
<!-- #region autoGenerate type="cover" name="columns" -->
            <vclist :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="p.height"
                ref="mlistvm" v-loading="loading" :editable="!disabled"  query  :maxLine="1" 
                v-model:queryConditions="queryRowCondtions" v-model:focusRow="focusRow"
                
                :orderCol="true"
                :stripe="true"
            >
                <slot></slot>
	            <vccol field="op" caption="操作"  width="105px" fixed="left" align="center" >
                    <template #default="cellp">

<el-link type="primary" @click="authRow(cellp.row)" >编辑</el-link>
<el-link type="danger" @click="removeAuthRow(cellp.row)" style="margin-left:10px;">删除</el-link>
    
                    </template>
                </vccol>
	            <vccol field="DW_OrgName" caption="组织名称"  width="245px" type="text" editable  align="left" ></vccol>
	            <vccol field="DW_OrgSrc" caption="组织来源"  width="105px" type="text" :dictlist="codeTables.HH_DOrgSrc" editable  align="left" ></vccol>
	            <vccol field="DW_Name" caption="姓名"  width="140px" type="text" editable  align="left" ></vccol>
	            <vccol field="DW_Card" caption="证件号"  width="210px" type="text" editable  align="left" ></vccol>
	            <vccol field="DW_IsOn" caption="启用"  width="84px" type="bool" editable  align="left" ></vccol>
	            <vccol field="DW_Note" caption="简介"  width="210px" type="text" editable  align="left" ></vccol>
	            <vccol field="DW_Remark" caption="备注"  width="140px" type="text" editable  align="left" ></vccol>
	            <vccol field="DW_Order" caption="排序"  width="105px" type="number" editable  align="left" ></vccol>
	            <vccol field="DW_Status" caption="状态"  width="105px" type="text" editable  align="left" ></vccol>
	            <vccol field="DW_IsMain" caption="主要单位"  width="84px" type="bool" editable  align="left" ></vccol>
	            <vccol field="DW_IsDuty" caption="责任人"  width="84px" type="bool" editable  align="left" ></vccol>
	            <vccol field="DW_GiveMan" caption="授权人"  width="140px" type="text" editable  align="left" ></vccol>
	            <vccol field="DW_GiveMancode" caption="授权人编码"  width="140px" type="text" editable  align="left" ></vccol>
	            <vccol field="SYS_Created" caption="创建日期"  width="168px" type="datetime" align="left" ></vccol>
	            <vccol field="SYS_LAST_UPD" caption="最后修改日期"  width="168px" type="datetime" align="left" ></vccol>
	            <vccol field="SYS_Deleted" caption="记录删除标记"  width="84px" type="bool" align="left" ></vccol>
                
                
            </vclist>
<!-- #endregion autoGenerate type="cover" name="columns" -->
    <section name="openRow" class="sp-ht">
    <el-drawer title="编辑" class="normal" v-model="authRowOpened" size="85vw" append-to-body @close="closeauthRow">
        <template #header>
            <div style="display:flex; align-items:center; justify-content:space-between; width:100%; gap:12px;">
                <div>编辑</div>
                <div ref="authToolbarMount"></div>
            </div>
        </template>
        <Biz_HumanAuthEdit name="Biz_HumanAuthEdit" :oid="tarauthRowRow.HH_User_FK" :toolbarTo="authToolbarMount" @close="closeauthRow" @refresh="getlist" :api="api" :bus="bus" :page="page">
        </Biz_HumanAuthEdit>
    </el-drawer>
    </section>            

        </template>
        <template #footer="p">

        </template>
    </flexpan>
</template>
<script setup>
///////////////////////////////////////////////////////////
//  VC_HH_DivWorkerList.vue
//  单位管理员列表，包含基础列表功能
//  功能框架由平台生成，请勿修改标记为[cover]区域的代码，否则将有被生成器覆盖的风险，生成器的覆盖范围随配置逐渐提升，各级别按[cover][lv2][lv3]标记。
//  新增逻辑时，在标记[autoGenerate]区域外部添加,比如[lv3]扩展业务逻辑的下方。
//  [autoGenerate]区域含义：[cover]--被覆盖，请勿修改；[lv2]--在方法模式被生成器覆盖，基础模式可修改；[lv3]--在全量模式下会被生成器覆盖。
//  创建时间:      2026/3/10 9:25:22
///////////////////////////////////////////////////////////

import { ref, computed, watch, onMounted,useAttrs } from 'vue';
//基础UI
import flexpan from "@/ui/common/flexpan.vue";
import vclist from "@/ui/common/vclist/vclist.vue";
import vccol from "@/ui/common/vclist/vccol.vue";
import livevc from "@/uiext/gene/live/livevc.vue";
	
//编辑控制
import useListEditor from "@/uiext/gene/hook/listeditor.js"
import {fileSaveLocal,checkExportListPara} from "@/uiext/gene/utils/filedown.js"
import Biz_HumanAuthEdit from "../../Page/Biz_HumanAuthEdit.vue"
//自定义引用

//其它引用
// #region [autoGenerate] [init] 自动初始化
const props = defineProps({
    page: null,
    api: null,
    bus:null,
    nav: null,
    height: null,
    vc: null,
	disabled:{ type:Boolean,default:true },
    hideToolbar:{ type:Boolean,default:false },
    autoLoad:{ type:Boolean,default:true },
    //工具栏迁移的目标
    position:null,
    flex:{ type:Boolean,default:false },
    conditions:null,//查询参数
    params:null,//额外的自由参数，为对象
    pageSize:{ type:Number,default:30 },//每页条数
    fk:null,//作为子表时，传入外键值，即主表的关联主键
    childed:{ type:Boolean,default:false },//是否作为子表，作为子表时，必须有传入fk才能加载
    beforeLoad:null,//加载列表前的处理
    
})

const emit = defineEmits(['change', 'delete', ])
const listHolder = useListEditor();
const VCName="VC_HH_DivWorkerList";
//视图显示前

//定义响应式数据
const loading = ref(false);
const mlist = ref([]);
const total = ref(0);
const queryParams = ref({
    pageSize: props.pageSize,
    pageNum: 1,
});
const checked = ref([]);
const focusRow = ref({});
const diaRow = ref(false); // 行编辑表单
const adding = ref(false);
const mlistvm = ref(null);//列表组件引用
const authToolbarMount = ref(null);

//查询行的条件
const queryRowCondtions=ref([]);
// 计算属性

const codeTables = computed(() => {
    if(!props.page){
        return {};
    }
    var tar= props.page.codeTable;
    if(!tar){
        return {};
    }
    return tar;
});

const busData= computed(() => {
    if(!props.bus){
        return {
            conditions:[],
        };
    }
    var tar= props.bus.dataMap[VCName];
    if(!tar){
        return {
            conditions:[],
        };

    }
    return tar;
});

//查询参数
const listpara = computed(() => {
    var res= {
        masterFK:props.fk,
        conditions:[],
        pageSize: queryParams.value.pageSize,
        pageNum: queryParams.value.pageNum,
        dict:props.params,
    };
    if(!!busData.value){
        if(Array.isArray(busData.value.conditions)){
            res.conditions= res.conditions.concat(busData.value.conditions);
        }
    }

    if(!!props.conditions){
        res.conditions= res.conditions.concat(props.conditions);
    }

    
    return res;
});


function setFocusRow(row){
    focusRow.value=row;
    emit('update:focusRow', row);
}

// 查询列表数据
const getlist = () => {
    if(props.childed==true){
        if(!props.fk){
            //swal({ text: "作为子表时，必须有传入外键值", icon: "error", button: "关闭", });
            return;
        }
    }
    var para=Object.assign({},listpara.value);
    if(Array.isArray(queryRowCondtions.value)){
        para.conditions= para.conditions.concat(queryRowCondtions.value);
    }
    if(typeof props.beforeLoad=="function"){
        para=props.beforeLoad(para);
    }
    
    loading.value = true;
    props.api.queryBCHHDivWorkerlist(para).then((res) => {
        mlist.value = res.Items;
        total.value = res.Total;
        listHolder.init(res.Items, "HH_DivWorkerOID");
        if(props.bus){
            props.bus.setList(VCName,mlist);            
        }
		emit("loaded",res);
        loading.value = false;
    })
    .catch((err) => {
        swal({ text: err.message, icon: "error", button: "关闭", });
        loading.value = false;
    });
};
//重置查询条件
const resetQuery = () => {
    queryParams.value = {
        pageSize: 30,
        pageNum: 1,
    };
};


// 保存修改
function saveChange(istip) {
    var tar = listHolder.compare(mlist.value);
    if(tar.length==0 ){
        if(istip !=false){
            swal({ text: "没有变更的信息", icon: "info", button: "关闭", });            
        }
        return false;
    }

    
    return props.api.saveBCHHDivWorkerList(tar).then((res) => {
        if(istip !=false){swal({ text: res.data, icon: "info", button: "关闭", });}
		emit("saved",tar);
        getlist();
        
        return res;
    });
    
}
// 删除行
const removeRow = (row,cap) => {
    const para = [row.HH_DivWorkerOID];
    if(!cap){
        cap="删除当前行数据"
    }

    alertIsOK("确认要操作吗？将删除当前行数据！",function(e){

        if(e=="yes"){
            props.api.removeBCHHDivWorkerByIds(para).then((res) => {
                getlist();
                swal({ text: res.data, icon: "success", button: "关闭", });
            });
        }
    },cap);
};
//导出数据
const exportListToExcel = (para) => { 
    if(para==void 0){
        para= listpara.value;
    }
    para=Object.assign({},para);
    if(Array.isArray(queryRowCondtions.value)){
        para.conditions= para.conditions.concat(queryRowCondtions.value);
    }
    if(typeof props.beforeLoad=="function"){
        para=props.beforeLoad(para);
    }
    
    if(para.fileName==void 0){
        const dateStr4 = new Date().toLocaleString('sv-SE').replace(' ', 'T');
        para.fileName="导出违规记录列表"+dateStr4+".xlsx";
    }
    para=checkExportListPara(para,mlistvm.value);
    props.api.exportBCHHDivWorkerToExcel(para).then((res) => {
        fileSaveLocal(res);
    });
};


// 监听 listpara 变化
watch(()=>props.fk,()=>{
    getlist();
})
watch(()=>props.pageSize,()=>{
    getlist();
})
watch(()=>props.conditions, (e1,e2) => {
    getlist();
},{ deep: true });

// 挂载时调用 
onMounted(() => {
    if(props.bus!=void 0 && props.bus  !=null){
        props.bus.initVCList(VCName,mlist,total,focusRow);
        props.bus.registerAction(VCName,getlist,saveChange,removeRow);
        props.bus.hub.on("refresh",getlist);
    }
    if(props.autoLoad==true){
        getlist();
    }
//JS业务初始化时刻--start

//JS业务初始化时刻--end

});
// #endregion [autoGenerate] [init] 自动初始化

// #region [autoGenerate] [lv2] 业务方法
//JS方法openRow--start

const authRowOpened = ref(false);
const tarauthRowRow= ref({});
function authRow(row){
    tarauthRowRow.value=row;
    authRowOpened.value=true;
}
function closeauthRow(){
    tarauthRowRow.value={};
    authRowOpened.value=false;
    getlist();
}
//JS方法openRow--end
/**-|方法|-end**/
// #endregion [autoGenerate] [lv2] 业务方法

// #region [autoGenerate] [cover] [勿改] 工具栏响应方法
/***工具栏响应方法--开始***/


/***工具栏响应方法--结束***/
// #endregion [autoGenerate] [cover] [勿改] 工具栏响应方法

// #region [autoGenerate] [lv3] 扩展业务逻辑
/** 操作列删除：走后端 removeAuth（先删从表授权范围再删主表） */
function removeAuthRow(row, cap) {
    if (!cap) {
        cap = "删除当前行数据"
    }
    alertIsOK("确认要操作吗？将撤销授权并删除当前行数据！", function (e) {
        if (e == "yes") {
            props.api.removeAuth({ oid: row.HH_DivWorkerOID }).then((res) => {
                getlist();
                swal({ text: res.data, icon: "success", button: "关闭", });
            }).catch((err) => {
                swal({ text: err.message, icon: "error", button: "关闭", });
            });
        }
    }, cap);
}
// #endregion [autoGenerate] [lv3] 扩展业务逻辑
</script>


<style lang="css" scoped>
/* 自定义样式--start */

/* 自定义样式--end */
</style>

<style lang="css" >
</style>