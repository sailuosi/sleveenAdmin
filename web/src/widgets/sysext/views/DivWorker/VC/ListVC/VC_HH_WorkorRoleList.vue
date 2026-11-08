<template>
    <flexpan :flex="flex" :height="height">
        <template #header="p">
            <teleport  :to="position" :disabled="!position" v-if="!hideToolbar">        
                <div class="vc-top-toolbar" >
                    <!-- #region autoGenerate type="cover" name="toolbar" -->
                          <el-button @click="onBtn_49107Click"   type="warning" ><SvgIcon name="fa fa-plus-circle" /> 添加</el-button>
      
      <!-- <el-button @click="saveChange"   type="primary" ><SvgIcon name="fa fa-save" /> 保存</el-button> -->
      


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
	            <vccol field="op" caption="操作"  width="120px" fixed="left" align="center" >
                    <template #default="cellp">
<!-- <el-link type="primary" title="编辑" @click="openRow(cellp.row)" >编辑</el-link> -->
<el-link type="primary" title="配置" @click="openConfig(cellp.row)" ><SvgIcon name="uil:setting" :size="16" /></el-link>
<el-link type="danger" title="删除" @click="removeRow(cellp.row)" style="margin-left:10px;"><SvgIcon name="fa fa-trash" /></el-link>
<el-link type="primary" title="查看菜单权限" @click="openMenuView(cellp.row)" style="margin-left:10px;"><SvgIcon name="hugeicons:view" :size="16" /></el-link>
    
                    </template>
                </vccol>
	            <vccol field="Di_Name" caption="名称"  min-width="210px" editable  align="left" ></vccol>
	            <vccol field="Di_Idx" caption="序号"  width="210px" type="number" editable  align="left" ></vccol>
	            <vccol field="Di_Code" caption="编码"  width="210px" editable  align="left" ></vccol>
                
                
            </vclist>
<!-- #endregion autoGenerate type="cover" name="columns" -->            

    <section name="openAddRole" class="sp-ht">
	<mdialog
		title="为管理员类别设置适用角色"
		extraClass="workor-role-pick-dialog"
		v-model="drawEditOpened"
		width="90%"
		maxWidth="1200px"
		top="5vh"
		destroy-on-close
		@close="closeRowEdit"
	>
		<div class="workor-pick-dialog-body">
			<livebpo
				v-if="drawEditOpened"
				name="Biz_HH_DutyItemCheck"
				@close="closeRowEdit"
				@check="checkingRole"
			/>
		</div>
	</mdialog>
    </section>

	<section name="openConfig" class="sp-ht">
		<WorkorRoleConfigDialog
			v-model="configOpened"
			:row="configRow"
			@saved="onConfigSaved"
			@close="closeConfig"
		/>
	</section>

	<section name="openMenuView" class="sp-ht">
		<WorkorRoleMenuViewDialog
			v-model="menuViewOpened"
			:row="menuViewRow"
			@close="closeMenuView"
		/>
	</section>
        </template>
        <template #footer="p">

        </template>
    </flexpan>
</template>
<script setup>
///////////////////////////////////////////////////////////
//  VC_HH_WorkorRoleList.vue
//  管理员角色列表，包含基础列表功能
//  功能框架由平台生成，请勿修改标记为[cover]区域的代码，否则将有被生成器覆盖的风险，生成器的覆盖范围随配置逐渐提升，各级别按[cover][lv2][lv3]标记。
//  新增逻辑时，在标记[autoGenerate]区域外部添加,比如[lv3]扩展业务逻辑的下方。
//  [autoGenerate]区域含义：[cover]--被覆盖，请勿修改；[lv2]--在方法模式被生成器覆盖，基础模式可修改；[lv3]--在全量模式下会被生成器覆盖。
//  创建时间:      2026/3/12 14:46:19
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
import livebpo from "@/uiext/gene/live/livebpo.vue"
import WorkorRoleConfigDialog from "../manVC/auth/WorkorRoleConfigDialog.vue"
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
const VCName="VC_HH_WorkorRoleList";
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
            listHolder.init([], "HH_WorkorRoleOID");
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
    props.api.queryBCHHWorkorRolelist(para).then((res) => {
        mlist.value = res.Items;
        total.value = res.Total;
        listHolder.init(res.Items, "HH_WorkorRoleOID");
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

    
    return props.api.saveBCHHWorkorRoleList(tar).then((res) => {
        if(istip !=false){swal({ text: res.data, icon: "info", button: "关闭", });}
		emit("saved",tar);
        getlist();
        
        return res;
    });
    
}
// 删除行
const removeRow = (row,cap) => {
    const para = [row.HH_WorkorRoleOID];
    if(!cap){
        cap="删除当前行数据"
    }

    alertIsOK("确认要操作吗？将删除当前行数据！",function(e){

        if(e=="yes"){
            props.api.removeBCHHWorkorRoleByIds(para).then((res) => {
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
    props.api.exportBCHHWorkorRoleToExcel(para).then((res) => {
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

//JS方法openAddRole--start

const drawEditOpened = ref(false);
const editingRow= ref({});
function openAddRole(row){
    editingRow.value=row;
    drawEditOpened.value=true;
}
function closeRowEdit(){
    editingRow.value={};
    drawEditOpened.value=false;
    //getlist();
}
function checkingRole(e){
    if(!Array.isArray(e) || e.length===0){
        swal({ text: "请先选择角色", icon: "info", button: "关闭", });
        return;
    }
    e.forEach((li)=>{
        mlist.value.push({
            HH_DutyItem_FK:li.HH_DutyItemOID,
            HH_WorkorType_FK:props.fk,
            Di_Name:li.Di_Name,
            Di_Code:li.Di_Code,
        });
    });
    const ret = saveChange();
    if(ret && typeof ret.then === "function"){
        ret.then(()=>{
            closeRowEdit();
        });
    }
}

//JS方法openAddRole--end
  
/**-|方法|-end**/
// #endregion [autoGenerate] [lv2] 业务方法

// #region [autoGenerate] [cover] [勿改] 工具栏响应方法
/***工具栏响应方法--开始***/


/***按钮 添加 点击事件***/
function onBtn_49107Click(e,tar){
    openAddRole();
}   


/***工具栏响应方法--结束***/
// #endregion [autoGenerate] [cover] [勿改] 工具栏响应方法

// #region [autoGenerate] [lv3] 扩展业务逻辑
const configOpened = ref(false);
const configRow = ref(null);
const menuViewOpened = ref(false);
const menuViewRow = ref(null);

function openConfig(row){
    if(!row?.HH_DutyItem_FK){
        swal({ text: "当前行未关联职责角色，无法配置", icon: "info", button: "关闭" });
        return;
    }
    configRow.value = row;
    configOpened.value = true;
}
function closeConfig(){
    configOpened.value = false;
    configRow.value = null;
}
function onConfigSaved(){
    getlist();
}

function openMenuView(row){
    if(!row?.HH_DutyItem_FK){
        swal({ text: "当前行未关联职责角色，无法查看", icon: "info", button: "关闭" });
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
/* 强制挂到 overlay 后水平居中，避免宽内容把弹窗顶出右屏 */
.el-overlay-dialog:has(.workor-role-pick-dialog) {
    display: flex !important;
    justify-content: center !important;
    align-items: flex-start !important;
}
.workor-role-pick-dialog.el-dialog.card-dia {
    width: min(1200px, 90vw) !important;
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
.workor-role-pick-dialog.el-dialog.card-dia .cardbox-pan {
    width: 100% !important;
    max-width: 100% !important;
    min-width: 0 !important;
    box-sizing: border-box;
    border-radius: 8px;
    overflow: hidden !important;
    display: flex !important;
    flex-direction: column !important;
}
.workor-role-pick-dialog.el-dialog.card-dia .cd-box {
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
.workor-pick-dialog-body .Biz_HH_DutyItemCheck,
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
.workor-pick-dialog-body .Biz_HH_DutyItemCheck,
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