<template>
    <flexpan :flex="flex" :height="height">
        <template #header="p">
            <teleport  :to="position" :disabled="!position" v-if="!hideToolbar">        
                <div class="vc-top-toolbar" >
                    <!-- #region autoGenerate type="cover" name="toolbar" -->
                          <el-button @click="getlist"   type="success" ><SvgIcon name="fa fa-refresh" /> 刷新</el-button>
      
      <el-button type="success" plain :disabled="!canBatchActivate" @click="batchActivate"><SvgIcon name="fa fa-check-circle" /> 批量激活</el-button>
      <el-button type="info" plain :disabled="!canBatchDisable" @click="batchDisable"><SvgIcon name="fa fa-ban" /> 批量禁用</el-button>

      <el-button @click="onBtn_90983Click"   type="primary" ><SvgIcon name="fa fa-floppy-o" /> 保存</el-button>
      
    <el-dropdown placement="bottom"   class="c5">
      <el-button @click="onBtn_90974Click(void 0,'root')" type="warning"><SvgIcon name="fa-plus-circle" />添加</el-button>
      <template #dropdown>
        <el-dropdown-menu>
        </el-dropdown-menu>
      </template>
    </el-dropdown>


                    <!-- #endregion autoGenerate type="cover" name="toolbar" --> 
                    <slot name="toolbar"></slot>
                </div>
            </teleport>

        </template>
        <template #default="p">
<!-- #region autoGenerate type="cover" name="columns" -->
            <vclist :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="p.height"
                ref="mlistvm" v-loading="loading" :editable="!disabled"  query  :maxLine="1" 
                v-model:queryConditions="queryRowCondtions" @update:focusRow="setFocusRow"
                checkable row-key="HR_HumanOID" v-model:checked="checked"
                showColumnQuery
                :orderCol="true"
                :stripe="true"
            >
                <slot></slot>
	            <vccol field="op" caption="操作"  width="124px" type="text" fixed="left" editable  align="left" >
                    <template #default="cellp">


                        <el-link type="danger" @click="authRow(cellp.row)" style="margin-left:10px;">权限</el-link>
                        <el-link
                        v-if="cellp.row?.H_Disabled === true || cellp.row?.H_Disabled === 'true'"
                        type="forbid"
                        @click="activateRow(cellp.row)"
                        style="margin-left:10px;"
                        >
                        禁用
                        </el-link>
                        <el-link
                        v-else
                        type="activate"
                        @click="activateRow(cellp.row)"
                        style="margin-left:10px;"
                        >
                        激活
                        </el-link>
                    </template>
                </vccol>

	            <vccol field="H_Name" caption="姓名"  width="140px" editable  align="left" ></vccol>
	            <vccol field="O_OrgName" caption="部门"  width="180px"  align="left" ></vccol>
	            <vccol field="O_Path" caption="单位"  width="540px" editable  align="left" ></vccol>
                
	            <vccol field="H_JobType" caption="岗位工种"  width="140px" editable  align="left" ></vccol>
                <vccol field="H_MainCode" caption="编码"  width="140px" editable  align="left" ></vccol>
	            <vccol field="H_PostName" caption="岗位名称"  width="140px" editable  align="left" ></vccol>
                <vccol field="H_Type" caption="人员类别"  width="140px" editable type="dict" dict="HR_HumanType" align="center" ></vccol>
	            <vccol field="H_FirstEdu" caption="第一学历"  width="105px" editable  align="left" ></vccol>
	            <vccol field="H_DataSrc" caption="数据来源"  width="105px" editable  align="left" ></vccol>
	            <vccol field="H_Disabled" caption="是否激活"  width="84px" type="bool" editable  align="left" ></vccol>
	            <vccol field="SYS_Created" caption="创建日期"  width="154px" type="datetime" align="left" ></vccol>
	            <vccol field="SYS_LAST_UPD" caption="最后修改日期"  width="154px" type="datetime" align="left" ></vccol>
                
                
            </vclist>
<!-- #endregion autoGenerate type="cover" name="columns" -->            

    <section name="add3V" class="sp-ht">
	<el-dialog title="添加数据" class="normal" v-model="showadd3VDia" width="70vw" append-to-body @close="showadd3VDia=false">
		<el-form :model="addingRow" label-width="auto" >
            <fminput v-model="addingRow.H_Name" label="姓名"  type="text"></fminput>
            <fminput v-model="addingRow.H_Sex" label="性别"  type="text"></fminput>
            <fminput v-model="addingRow.H_IdCard" label="身份证"  type="text"></fminput>
            <fminput v-model="addingRow.H_StartWork" label="参加工作时间"  type="datetime"></fminput>
            <el-form-item>
                <el-button type="primary" @click="add3VOK">确定添加</el-button>
                <el-button @click="showadd3VDia=false">取消</el-button>
            </el-form-item>
        </el-form>
	</el-dialog>
    </section>
    <section name="openEditRow" class="sp-ht">
	<el-dialog :title="editingRow.editingRow!='9'?'查看人员信息':'编辑人员信息'" class="normal" v-model="diaEditOpened" width="70vw" append-to-body @close="closeRowEdit">
		<Biz_HumanAuthEdit name="VC_HR_HumanEdit" :row="editingRow" :disabled="editingRow.editingRow!='9'" @close="closeRowEdit" @refresh="getlist" :api="api" :bus="bus" :page="page">
		</Biz_HumanAuthEdit>
	</el-dialog>
    </section>
    <section name="openRow" class="sp-ht">
	<el-drawer title="弹窗" class="normal" v-model="drawEditOpened" size="85%" append-to-body @close="closeRowEditl">
		<livebpo name="BPO_HumanListEdit" :oid="editingRowl.HR_HumanOID" style="height:90vh;" @close="closeRowEditl" @refresh="getlist" :api="api" :bus="bus" :page="page">
		</livebpo>
	</el-drawer>
    </section>
    <section name="authRow" class="sp-ht">
    <el-drawer title="编辑" class="normal" v-model="authRowOpened" size="85vw" append-to-body @close="closeauthRow">
        <template #header>
            <div style="display:flex; align-items:center; justify-content:space-between; width:100%; gap:12px;">
                <div>编辑</div>
                <div ref="authToolbarMount"></div>
            </div>
        </template>
        <livebpo name="Biz_HumanAuthEdit" :oid="tarauthRowRow.HR_HumanOID" :toolbarTo="authToolbarMount" @close="closeauthRow" @refresh="getlist" :api="api" :bus="bus" :page="page">
        </livebpo>
    </el-drawer>
    </section>
        </template>
        <template #footer="p">

        </template>
    </flexpan>
</template>
<script setup>
///////////////////////////////////////////////////////////
//  VC_HR_HumanList.vue
//  人力人员列表，包含基础列表功能
//  功能框架由平台生成，请勿修改标记为[cover]区域的代码，否则将有被生成器覆盖的风险，生成器的覆盖范围随配置逐渐提升，各级别按[cover][lv2][lv3]标记。
//  新增逻辑时，在标记[autoGenerate]区域外部添加,比如[lv3]扩展业务逻辑的下方。
//  [autoGenerate]区域含义：[cover]--被覆盖，请勿修改；[lv2]--在方法模式被生成器覆盖，基础模式可修改；[lv3]--在全量模式下会被生成器覆盖。
//  创建时间:      2026/3/10 9:55:12
///////////////////////////////////////////////////////////
/**
 *                         <el-link type="primary" @click="openRow(cellp.row)" >编辑</el-link>
 */


import { ref, computed, watch, onMounted,useAttrs } from 'vue';
//基础UI
import flexpan from "@/ui/common/flexpan.vue";
import vclist from "@/ui/common/vclist/vclist.vue";
import vccol from "@/ui/common/vclist/vccol.vue";
import livevc from "@/uiext/gene/live/livevc.vue";
	
//编辑控制
import useListEditor from "@/uiext/gene/hook/listeditor.js"
import {fileSaveLocal,checkExportListPara} from "@/uiext/gene/utils/filedown.js"
//自定义引用
import fminput from "@/ui/common/input/fminput.vue"
import livebpo from "@/uiext/gene/live/livebpo.vue"
import Biz_HumanAuthEdit from "../../../Page/Biz_HumanAuthEdit.vue"
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
const VCName="VC_HR_HumanList";
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
    props.api.queryBCHRHumanlist(para).then((res) => {
        mlist.value = res.Items;
        total.value = res.Total;
        listHolder.init(res.Items, "HR_HumanOID");
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

    
    return props.api.saveBCHRHumanList(tar).then((res) => {
        if(istip !=false){swal({ text: res.data, icon: "info", button: "关闭", });}
		emit("saved",tar);
        getlist();
        
        return res;
    });
    
}
// 删除行
const removeRow = (row,cap) => {
    const para = [row.HR_HumanOID];
    if(!cap){
        cap="删除当前行数据"
    }

    alertIsOK("确认要操作吗？将删除当前行数据！",function(e){

        if(e=="yes"){
            props.api.removeBCHRHumanByIds(para).then((res) => {
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
    props.api.exportBCHRHumanToExcel(para).then((res) => {
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

//JS方法downloadTextAsFile--start
function downloadTextAsFile(text, filename = "download.txt") {
    // 创建 Blob 对象
    const blob = new Blob([text], { type: "text/plain;charset=utf-8" });

    // 创建临时链接
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = filename; // 设置下载的文件名

    // 触发点击
    document.body.appendChild(a);
    a.click();

    // 清理
    document.body.removeChild(a);
    URL.revokeObjectURL(url); // 释放内存
}
//JS方法downloadTextAsFile--end
  

//JS方法add3V--start


const showadd3VDia = ref(false);
const addingRow= ref({});
function add3V(){
    showadd3VDia.value=true;
}
function add3VOK(vc){
    mlist.value.push(addingRow.value);
    addingRow.value={
        H_DataSrc:"9"
    };
    showadd3VDia.value=false;
}
//JS方法add3V--end
  

//JS方法openEditRow--start
const editingRow = ref({});
const diaEditOpened = ref(false);
function openEditRow(row){
    editingRow.value=row;
    diaEditOpened.value=true;
}
//JS方法openEditRow--end
  

//JS方法openRow--start

const drawEditOpened = ref(false);
const editingRowl= ref({});
function openRow(row){
    editingRowl.value=row;
    drawEditOpened.value=true;
}
function closeRowEditl(){
    editingRowl.value={};
    drawEditOpened.value=false;
    getlist();
}

function isRowActivated(row) {
    const v = row?.H_Disabled;
    return v === true || v === 1 || v === "1" || v === "true";
}

const canBatchActivate = computed(() => {
    return (checked.value || []).some((r) => r?.HR_HumanOID && !isRowActivated(r));
});

const canBatchDisable = computed(() => {
    return (checked.value || []).some((r) => r?.HR_HumanOID && isRowActivated(r));
});

function batchToggleRows(nextState, cap) {
    const list = Array.isArray(checked.value) ? checked.value : [];
    if (list.length === 0) {
        swal({ text: "请先勾选要操作的数据", icon: "info", button: "关闭", });
        return;
    }
    const validRows = list.filter((r) => {
        if (!r?.HR_HumanOID) return false;
        return nextState === 1 ? !isRowActivated(r) : isRowActivated(r);
    });
    if (validRows.length === 0) {
        swal({
            text: nextState === 1 ? "所选数据均已激活，无需重复操作" : "所选数据均未激活，无需重复操作",
            icon: "warning",
            button: "关闭",
        });
        return;
    }
    const skipped = list.length - validRows.length;
    const skipTip = skipped > 0 ? `（已跳过 ${skipped} 条无需操作的数据）` : "";
    const tip = nextState === 1
        ? `确认要批量激活选中的 ${validRows.length} 条人员吗？${skipTip}`
        : `确认要批量禁用选中的 ${validRows.length} 条人员吗？${skipTip}`;

    alertIsOK(tip, function (e) {
        if (e == "yes") {
            const tasks = validRows.map((row) => props.api.toggleUserState({
                contactoid: row.HR_HumanOID,
                newstate: nextState,
            }));
            Promise.all(tasks).then(() => {
                checked.value = [];
                getlist();
                swal({ text: `${cap}成功，共 ${validRows.length} 条`, icon: "success", button: "关闭", });
            }).catch((err) => {
                swal({ text: err?.message || `${cap}失败`, icon: "error", button: "关闭", });
            });
        }
    }, cap, nextState === 0, "warning");
}

function batchActivate() {
    batchToggleRows(1, "批量激活");
}

function batchDisable() {
    batchToggleRows(0, "批量禁用");
}

function activateRow(row) {
    if (!row || !row.HR_HumanOID) {
        swal({ text: "缺少人员主键，无法操作", icon: "error", button: "关闭", });
        return;
    }

    const activated = isRowActivated(row);
    const nextState = activated ? 0 : 1;
    const cap = activated ? "禁用" : "激活";
    const tip = activated ? "确认要禁用该人员吗？" : "确认要激活该人员吗？";

    alertIsOK(tip, function (e) {
        if (e == "yes") {
            props.api.toggleUserState({
                contactoid: row.HR_HumanOID,
                newstate: nextState
            }).then((res) => {
                const msg = res?.data || `${cap}操作成功`;
                swal({ text: msg, icon: "success", button: "关闭", });
                getlist();
            }).catch((err) => {
                swal({ text: err.message, icon: "error", button: "关闭", });
            });
        }
    }, cap, activated, "warning");
}

//JS方法openRow--end
  

//JS方法authRow--start

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

//JS方法authRow--end
  
/**-|方法|-end**/
// #endregion [autoGenerate] [lv2] 业务方法

// #region [autoGenerate] [cover] [勿改] 工具栏响应方法
/***工具栏响应方法--开始***/


/***按钮 保存 点击事件***/
function onBtn_90983Click(e,tar){
    saveChange();
}   


/***按钮 添加 点击事件***/
function onBtn_90974Click(e,tar){
    add3V();
}


/***工具栏响应方法--结束***/
// #endregion [autoGenerate] [cover] [勿改] 工具栏响应方法

// #region [autoGenerate] [lv3] 扩展业务逻辑
// #endregion [autoGenerate] [cover] [勿改] 工具栏响应方法

// #region [autoGenerate] [lv3] 扩展业务逻辑

// #endregion [autoGenerate] [lv3] 扩展业务逻辑

 
// #endregion [autoGenerate] [lv3] 扩展业务逻辑
</script>


<style lang="css" scoped>
/* 自定义样式--start */
:global(:root) {
  --el-color-activate: #67c23a;
  --el-color-forbid: #909399;
}

:deep(.el-link.el-link--activate) {
  --el-link-text-color: var(--el-color-activate);
  --el-link-hover-text-color: #85ce61;
}
:deep(.el-link.el-link--activate:active) {
  color: #529b2e;
}

:deep(.el-link.el-link--forbid) {
  --el-link-text-color: var(--el-color-forbid);
  --el-link-hover-text-color: #a6a9ad;
}
:deep(.el-link.el-link--forbid:active) {
  color: #80848a;
}
:deep(.el-link.el-link--forbid),
:deep(.el-link.el-link--forbid:hover) {
  cursor: pointer;
}
/* 自定义样式--end */
</style>

<style lang="css" >








</style>