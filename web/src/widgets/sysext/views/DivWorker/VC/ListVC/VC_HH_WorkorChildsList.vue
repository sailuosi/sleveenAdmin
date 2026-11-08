<template>
    <flexpan :flex="flex" :height="height">
        <template #header="p">
            <teleport  :to="position" :disabled="!position" v-if="!hideToolbar">        
                <div class="vc-top-toolbar" >
                    <!-- #region autoGenerate type="cover" name="toolbar" -->
                          <el-button @click="onBtn_49115Click"   type="warning" ><SvgIcon name="fa fa-plus-circle" /> 添加</el-button>
      
      <!-- <el-button @click="onBtn_49119Click"   type="primary" ><SvgIcon name="fa fa-save" /> 保存</el-button> -->
      


                    <!-- #endregion autoGenerate type="cover" name="toolbar" --> 
                    <el-button @click="getlist" type="success"><SvgIcon name="fa fa-refresh" /> 刷新</el-button>
                    <slot name="toolbar"></slot>
                </div>
            </teleport>
            
        </template>
        <template #default="p">
<!-- #region autoGenerate type="cover" name="columns" -->
            <vclist :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="p.height"
                ref="mlistvm" v-loading="loading" :editable="!disabled"  query 
                v-model:queryConditions="queryRowCondtions" @update:focusRow="setFocusRow"
                
                :orderCol="true"
                :stripe="true"
            >
                <slot></slot>
	            <vccol field="op" caption="操作"  width="120px" type="text" fixed="left" align="center" >
                    <template #default="cellp">
<!-- <el-link type="primary" title="编辑" @click="openRow(cellp.row)" >编辑</el-link> -->
<el-link type="primary" title="配置" @click="openConfig(cellp.row)" ><SvgIcon name="uil:setting" :size="16" /></el-link>
<el-link type="danger" title="删除" @click="removeRow(cellp.row)" style="margin-left:10px;"><SvgIcon name="fa fa-trash" /></el-link>
<el-link type="primary" title="查看菜单权限" @click="openMenuView(cellp.row)" style="margin-left:10px;"><SvgIcon name="hugeicons:view" :size="16" /></el-link>
    
                    </template>
                </vccol>
	            <vccol field="WT_IsOn" caption="启用"  width="84px" type="bool" align="left" ></vccol>
	            <vccol field="WT_Title" caption="标题"  min-width="210px" type="text" align="left" ></vccol>
	            <vccol field="WT_Note" caption="简介"  width="210px" type="text" align="left" ></vccol>
	            <vccol field="WT_Order" caption="排序"  width="105px" type="number" align="right" ></vccol>
                
                
            </vclist>
<!-- #endregion autoGenerate type="cover" name="columns" -->            
<!-- #region autoGenerate type="cover" name="splitpart" -->
        
	<mdialog
		title="为管理员类别设置可以二次分配的类别"
		extraClass="workor-childs-pick-dialog"
		v-model="addHCOpened"
		width="90%"
		maxWidth="1100px"
		top="5vh"
		destroy-on-close
		@close="closeaddHC"
	>
		<div class="workor-pick-dialog-body">
			<Biz_WorkTypersCheck
				v-if="addHCOpened"
				@close="closeaddHC"
				@refresh="getlist"
				@checking="docheckaddtc"
			/>
		</div>
	</mdialog>

	<WorkorChildsRoleConfigDialog
		v-model="configOpened"
		:row="configRow"
		:api="api"
		@close="closeConfig"
	/>

	<WorkorRoleMenuViewDialog
		v-model="menuViewOpened"
		mode="workorType"
		:row="menuViewRow"
		@close="closeMenuView"
	/>

        </template>
        <template #footer="p">
<!-- #endregion autoGenerate type="cover" name="splitpart" --> 
        </template>
    </flexpan>
</template>
<script setup>
///////////////////////////////////////////////////////////
//  VC_HH_WorkorChildsList.vue
//  可分配类别列表，包含基础列表功能
//  功能框架由平台生成，请勿修改标记为[cover]区域的代码，否则将有被生成器覆盖的风险，生成器的覆盖范围随配置逐渐提升，各级别按[cover][lv2][lv3]标记。
//  新增逻辑时，在标记[autoGenerate]区域外部添加,比如[lv3]扩展业务逻辑的下方。
//  [autoGenerate]区域含义：[cover]--被覆盖，请勿修改；[lv2]--在方法模式被生成器覆盖，基础模式可修改；[lv3]--在全量模式下会被生成器覆盖。
//  创建时间:      2026/2/2 16:00:50
///////////////////////////////////////////////////////////

import { ref, computed, watch, onMounted,useAttrs } from 'vue';
//基础UI
import flexpan from "@/ui/common/flexpan.vue";
import vclist from "@/ui/common/vclist/vclist.vue";
import vccol from "@/ui/common/vclist/vccol.vue";
import livevc from "@/uiext/gene/live/livevc.vue";
import mdialog from "../../ui/modal/mdialog.vue";
	
//编辑控制
import useListEditor from "@/uiext/gene/hook/listeditor.js"
import {fileSaveLocal,checkExportListPara} from "@/uiext/gene/utils/filedown.js"
//自定义引用
import Biz_WorkTypersCheck from "../../Page/Biz_WorkTypersCheck.vue"
import WorkorChildsRoleConfigDialog from "../manVC/auth/WorkorChildsRoleConfigDialog.vue"
import WorkorRoleMenuViewDialog from "../manVC/auth/WorkorRoleMenuViewDialog.vue"

//其它引用
// #region [autoGenerate] [init] 自动初始化
const props = defineProps({
    page: null,
    api: null,
    bus:null,
    nav: null,
    height: null,
    vc: null,
	disabled:{ type:Boolean,default:false },
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
const VCName="VC_HH_WorkorChildsList";
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
            mlist.value = [];
            total.value = 0;
            setFocusRow({});
            listHolder.init([], "HH_WorkorChildsOID");
            if(props.bus){
                props.bus.setList(VCName,mlist);
            }
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
    props.api.queryBCHHWorkorChildslist(para).then((res) => {
        mlist.value = res.Items;
        total.value = res.Total;
        listHolder.init(res.Items, "HH_WorkorChildsOID");
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

    
    return props.api.saveBCHHWorkorChildsList(tar).then((res) => {
        if(istip !=false){swal({ text: res.data, icon: "info", button: "关闭", });}
		emit("saved",tar);
        getlist();
        
        return res;
    });
    
}
// 删除行
const removeRow = (row,cap) => {
    const para = [row.HH_WorkorChildsOID];
    if(!cap){
        cap="删除当前行数据"
    }

    alertIsOK("确认要操作吗？将删除当前行数据！",function(e){

        if(e=="yes"){
            props.api.removeBCHHWorkorChildsByIds(para).then((res) => {
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
    props.api.exportBCHHWorkorChildsToExcel(para).then((res) => {
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

//JS方法addHC--start



const addHCOpened = ref(false);
const taraddHCRow= ref({});
function addHC(row){
    taraddHCRow.value=row;
    addHCOpened.value=true;
}
function closeaddHC(){
    taraddHCRow.value={};
    addHCOpened.value=false;
}
function docheckaddtc(ck){
    if(!Array.isArray(ck) || ck.length===0){
        swal({ text: "请先选择类别", icon: "info", button: "关闭", });
        return;
    }
    ck.forEach((c)=>{
        mlist.value.push({
            HH_Boss_FK:props.fk,
            HH_WorkorType_FK:c.HH_WorkorTypeOID
        })
    })
    const ret = saveChange();
    if(ret && typeof ret.then === "function"){
        ret.then(()=>{
            closeaddHC();
        });
    }
}


//JS方法addHC--end
  
/**-|方法|-end**/
// #endregion [autoGenerate] [lv2] 业务方法

// #region [autoGenerate] [cover] [勿改] 工具栏响应方法
/***工具栏响应方法--开始***/


/***按钮 添加 点击事件***/
function onBtn_49115Click(e,tar){
    addHC();
}   


/***按钮 保存 点击事件***/
function onBtn_49119Click(e,tar){
        BusinessSubmit();
}   


/***工具栏响应方法--结束***/
// #endregion [autoGenerate] [cover] [勿改] 工具栏响应方法

// #region [autoGenerate] [lv3] 扩展业务逻辑
const configOpened = ref(false);
const configRow = ref(null);
const menuViewOpened = ref(false);
const menuViewRow = ref(null);

function openConfig(row){
    if(!row?.HH_WorkorType_FK){
        swal({ text: "当前行未关联管理员类别，无法配置", icon: "info", button: "关闭" });
        return;
    }
    configRow.value = row;
    configOpened.value = true;
}
function closeConfig(){
    configOpened.value = false;
    configRow.value = null;
}

function openMenuView(row){
    if(!row?.HH_WorkorType_FK){
        swal({ text: "当前行未关联管理员类别，无法查看", icon: "info", button: "关闭" });
        return;
    }
    menuViewRow.value = row;
    menuViewOpened.value = true;
}
function closeMenuView(){
    menuViewOpened.value = false;
    menuViewRow.value = null;
}
// #endregion [autoGenerate] [lv3] 扩展业务逻辑
</script>


<style lang="css" scoped>
/* 自定义样式--start */

/* 自定义样式--end */
</style>

<style lang="css">
.el-overlay-dialog:has(.workor-childs-pick-dialog) {
    display: flex !important;
    justify-content: center !important;
    align-items: flex-start !important;
}
.workor-childs-pick-dialog.el-dialog.card-dia {
    width: min(1100px, 90vw) !important;
    max-width: 90vw !important;
    margin: 5vh auto !important;
    left: auto !important;
    right: auto !important;
    transform: none !important;
    outline: none !important;
    box-shadow: 0 8px 28px rgba(15, 23, 42, 0.12) !important;
    border-radius: 8px !important;
    overflow: hidden !important;
}
.workor-childs-pick-dialog.el-dialog.card-dia .cardbox-pan {
    width: 100% !important;
    max-width: 100% !important;
    min-width: 0 !important;
    box-sizing: border-box;
    border-radius: 8px;
    overflow: hidden !important;
    display: flex !important;
    flex-direction: column !important;
}
.workor-childs-pick-dialog.el-dialog.card-dia .cd-box {
    width: 100% !important;
    max-width: 100% !important;
    min-width: 0 !important;
    min-height: 0 !important;
    flex: 1 1 auto !important;
    box-sizing: border-box;
    overflow: hidden !important;
    display: flex !important;
    flex-direction: column !important;
}
.workor-pick-dialog-body {
    height: 75vh;
    max-height: calc(100vh - 120px);
    min-height: 420px;
    width: 100%;
    max-width: 100%;
    min-width: 0;
    overflow: hidden;
    box-sizing: border-box;
    display: flex;
    flex-direction: column;
}
.workor-pick-dialog-body .Biz_WorkTypersCheck,
.workor-pick-dialog-body .cellpan,
.workor-pick-dialog-body .formcard-shell,
.workor-pick-dialog-body .formcard,
.workor-pick-dialog-body .fc-body,
.workor-pick-dialog-body .wrap-bd,
.workor-pick-dialog-body .flexpan,
.workor-pick-dialog-body .vclistwrap,
.workor-pick-dialog-body .vclist-inner {
    width: 100% !important;
    max-width: 100% !important;
    min-width: 0 !important;
    min-height: 0 !important;
    box-sizing: border-box;
}
.workor-pick-dialog-body .Biz_WorkTypersCheck,
.workor-pick-dialog-body .cellpan,
.workor-pick-dialog-body .formcard-shell,
.workor-pick-dialog-body .formcard,
.workor-pick-dialog-body .fc-body,
.workor-pick-dialog-body .wrap-bd,
.workor-pick-dialog-body .flexpan {
    flex: 1 1 0 !important;
    height: 100% !important;
    max-height: 100% !important;
    display: flex !important;
    flex-direction: column !important;
}
/* 覆盖 BPO 内写死的 70vh，否则分页会被裁掉 */
.workor-pick-dialog-body .flexpan.flexed,
.workor-pick-dialog-body .vclistwrap {
    height: 100% !important;
    max-height: none !important;
}
.workor-pick-dialog-body .flexpan.flexed > .f-body {
    flex: 1 1 0 !important;
    min-height: 0 !important;
    overflow: hidden !important;
}
.workor-pick-dialog-body .vclist-inner-flex {
    height: 100% !important;
    min-height: 0 !important;
}
.workor-pick-dialog-body .vclist-inner-flex > .el-pagination.vcpager {
    flex: 0 0 auto !important;
}
.workor-pick-dialog-body .formcard-shell--vc {
    padding: 8px 10px 10px !important;
}
</style>