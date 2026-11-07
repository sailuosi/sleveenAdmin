<template>
    <flexpan :flex="flex" :height="height">
        <template #header="p">
            <teleport  :to="position" :disabled="!position" v-if="!hideToolbar">        
                <div class="vc-top-toolbar" >
                    <!-- #region autoGenerate type="cover" name="toolbar" -->
                          <el-button @click="onBtn_49099Click"   type="warning" ><SvgIcon name="fa fa-plus-circle" /> 添加</el-button>
      
      <el-button @click="saveChange"   type="primary" ><SvgIcon name="fa fa-save" /> 保存</el-button>
      


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
	            <vccol field="WT_IsOn" caption="启用"  width="70px" type="bool" editable  align="left" ></vccol>
	            <vccol field="WT_Order" caption="排序"  width="80px" type="number" editable  align="right" ></vccol>
	            <vccol field="WT_Title" caption="标题"  width="210px" type="text" editable  align="left" ></vccol>
                <vccol field="WT_OSrc" caption="适用组织类型"  width="245px" type="dict" :dictlist="codeTables.HH_DOrgSrc" editable  align="left" multiple></vccol>
	            <vccol field="WT_Note" caption="简介"  width="350px" type="text" editable  align="left" ></vccol>
	            <vccol field="WT_Remark" caption="备注"  width="140px" type="text" editable  align="left" ></vccol>
	            <vccol field="WT_CreateMan" caption="创建人"  width="140px" type="text" align="center" ></vccol>
	            <vccol field="WT_Created" caption="创建时间"  width="180px" type="datetime" align="center" ></vccol>

	            <vccol field="op" caption="操作"  width="120px" fixed="left" align="center" >
                    <template #default="cellp">

<el-link type="primary" title="编辑" @click="openRow(cellp.row)" ><SvgIcon name="fa fa-edit" :size="13" /></el-link>
<el-link type="danger" title="删除" @click="removeRow(cellp.row)" style="margin-left:10px;"><SvgIcon name="fa fa-trash" /></el-link>
<el-link type="primary" title="查看菜单权限" @click="openMenuView(cellp.row)" style="margin-left:10px;"><SvgIcon name="hugeicons:view" :size="16" /></el-link>
    
                    </template>
                </vccol>

                
            </vclist>
<!-- #endregion autoGenerate type="cover" name="columns" -->
    <section name="openRow" class="sp-ht">
    <mdialog
        :title="adding ? '添加' : '编辑'"
        class="workor-type-edit-dialog"
        v-model="RowOpened"
        width="860px"
        minHeight="420px"
        top="8vh"
        destroy-on-close
        @close="closeRow"
    >
        <livevc
            v-if="RowOpened"
            name="VC_HH_WorkorTypeEdit"
            v-model:row="tarRowRow"
            v-model:adding="adding"
            @close="closeRow"
            @refresh="getlist"
            :api="api"
            :bus="bus"
            :page="page"
            style="height:100%;"
        />
    </mdialog>
    </section>

	<section name="openMenuView" class="sp-ht">
		<WorkorRoleMenuViewDialog
			v-model="menuViewOpened"
			mode="workorType"
			:row="menuViewRow"
			@close="closeMenuView"
		/>
	</section>
<!-- #region autoGenerate type="cover" name="splitpart" -->
        </template>
        <template #footer="p">
<!-- #endregion autoGenerate type="cover" name="splitpart" --> 
        </template>
    </flexpan>
</template>
<script setup>
///////////////////////////////////////////////////////////
//  VC_HH_WorkorTypeList.vue
//  管理员类别列表，包含基础列表功能
//  功能框架由平台生成，请勿修改标记为[cover]区域的代码，否则将有被生成器覆盖的风险，生成器的覆盖范围随配置逐渐提升，各级别按[cover][lv2][lv3]标记。
//  新增逻辑时，在标记[autoGenerate]区域外部添加,比如[lv3]扩展业务逻辑的下方。
//  [autoGenerate]区域含义：[cover]--被覆盖，请勿修改；[lv2]--在方法模式被生成器覆盖，基础模式可修改；[lv3]--在全量模式下会被生成器覆盖。
//  创建时间:      2026/2/2 15:27:45
///////////////////////////////////////////////////////////

import { ref, computed, watch, onMounted, nextTick, useAttrs } from 'vue';
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
const VCName="VC_HH_WorkorTypeList";
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
    props.api.queryBCHHWorkorTypelist(para).then((res) => {
        mlist.value = res.Items;
        total.value = res.Total;
        listHolder.init(res.Items, "HH_WorkorTypeOID");
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

    
    return props.api.saveBCHHWorkorTypeList(tar).then((res) => {
        if(istip !=false){swal({ text: res.data, icon: "info", button: "关闭", });}
		emit("saved",tar);
        getlist();
        
        return res;
    });
    
}
// 删除行
const removeRow = (row,cap) => {
    const para = [row.HH_WorkorTypeOID];
    if(!cap){
        cap="删除当前行数据"
    }

    alertIsOK("确认要操作吗？将删除当前行数据！",function(e){

        if(e=="yes"){
            props.api.removeBCHHWorkorTypeByIds(para).then((res) => {
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
    props.api.exportBCHHWorkorTypeToExcel(para).then((res) => {
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

const RowOpened = ref(false);
const tarRowRow= ref({});
function openRow(row){
    adding.value=false;
    tarRowRow.value=row;
    RowOpened.value=true;
}
function closeRow(){
    tarRowRow.value={};
    adding.value=false;
    RowOpened.value=false;
}

//JS方法openRow--end
/**-|方法|-end**/
// #endregion [autoGenerate] [lv2] 业务方法

// #region [autoGenerate] [cover] [勿改] 工具栏响应方法
/***工具栏响应方法--开始***/


/***按钮 添加 点击事件***/
function onBtn_49099Click(e,tar){
    // 原：列表内直接插行，由页面 @add="addType" 处理
    // emit("add")
    openAdd();
}   


/***工具栏响应方法--结束***/
// #endregion [autoGenerate] [cover] [勿改] 工具栏响应方法

// #region [autoGenerate] [lv3] 扩展业务逻辑
/** 列表加载后默认选中第一行（当前选中行仍在列表中则保留），驱动主从表右侧查询 */
watch(mlist, (list) => {
    if (!Array.isArray(list) || list.length === 0) {
        setFocusRow({});
        return;
    }
    const curOid = focusRow.value?.HH_WorkorTypeOID;
    const row = (curOid && list.find((r) => r.HH_WorkorTypeOID === curOid)) || list[0];
    setFocusRow(row);
    nextTick(() => {
        mlistvm.value?.$refs?.table?.setCurrentRow?.(row);
    });
});

const menuViewOpened = ref(false);
const menuViewRow = ref(null);

function openMenuView(row) {
    if (!row?.HH_WorkorTypeOID) {
        swal({ text: "当前行无管理员类别主键，无法查看", icon: "info", button: "关闭" });
        return;
    }
    menuViewRow.value = row;
    menuViewOpened.value = true;
}
function closeMenuView() {
    menuViewOpened.value = false;
    menuViewRow.value = null;
}

/** 弹窗添加：打开编辑表单，保存成功后由 EditVC emit close 关闭 */
function openAdd() {
    const user = props.page?.user || {};
    adding.value = true;
    tarRowRow.value = {
        WT_IsOn: true,
        WT_Order: 1,
        WT_Title: "***管理员",
        WT_Created: new Date(),
        WT_CreateMan: user.name,
        WT_CreateUserId: user.oid,
    };
    RowOpened.value = true;
}
// #endregion [autoGenerate] [lv3] 扩展业务逻辑
</script>


<style lang="css" scoped>
/* 自定义样式--start */

/* 自定义样式--end */
</style>

<style lang="css">
/* mdialog 外壳透明，需强制 dialog 宽度，否则易回落到 el-dialog 默认 50% */
.workor-type-edit-dialog.el-dialog.card-dia {
    width: min(860px, 94vw) !important;
    outline: none !important;
    box-shadow: 0 8px 28px rgba(15, 23, 42, 0.12) !important;
    border-radius: 8px !important;
    overflow: hidden;
}
.workor-type-edit-dialog.el-dialog.card-dia .cardbox-pan {
    width: 100%;
    box-sizing: border-box;
    border-radius: 8px;
    overflow: hidden;
}
.workor-type-edit-dialog.el-dialog.card-dia .cd-box {
    width: 100%;
    box-sizing: border-box;
}
</style>