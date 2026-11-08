<template>
<!-- #region autoGenerate type="cover" name="cellpan" -->
<cellpan type="lr" left="1fr" right="1fr"  flex bodyFlex  class="Biz_WorkorTypeSet">   
    <template #left="panp">
            <formcard title="管理员类别" width=""  flex stretch>
                <template #toolbar>
                    <div ref="VC_HH_WorkorTypeListHead"></div>
                </template>
                <VC_HH_WorkorTypeList :api="api" :bus="bus" :page="pageData"
                    :showTitle="false" 
                    v-model:focusRow="VC_HH_WorkorTypeListFocusRow"
                    title="管理员类别"
                    ref="VC_HH_WorkorTypeListBody"
                    
                    style="height:100%;"                    
                    :disabled="false"
                    :pageSize="100"
                    :flex="true"
                    height="100%"
                    :position="VC_HH_WorkorTypeListHead"
                >
                <!-- 原：@add="addType" 列表内插行添加；现改为列表弹窗表单添加 -->
                </VC_HH_WorkorTypeList>
            </formcard>


    </template>
    <template #right="panp">
        <formcard title="类别配置" width="" flex stretch>
            <template #toolbar>
                <div ref="PanelrightHead" class="tab-toolbar"></div>
            </template>
            <el-tabs v-model="focusTab_right" class="flexed right workor-type-right-tabs" type="card"
                ref="Panelright"
            >
                <el-tab-pane name="VC_HH_WorkorRoleList" label="管理员角色" lazy>
                    <template #label>
                        <div ref="VC_HH_WorkorRoleListHead">管理员角色</div>
                    </template>
                    <VC_HH_WorkorRoleList :api="api" :bus="bus" :page="pageData"
                        :showTitle="false"  childed  :fk="VC_HH_WorkorTypeListFocusRow.HH_WorkorTypeOID"
                        v-model:focusRow="VC_HH_WorkorRoleListFocusRow"
                        title="管理员角色"
                        ref="VC_HH_WorkorRoleListBody"
                        :hideToolbar="focusTab_right!='VC_HH_WorkorRoleList'"
                        style="height:100%;"
                        disabled
                        :pageSize="50"
                        :flex="true"
                        height="100%"
                        :position="PanelrightHead"
                    >
                    </VC_HH_WorkorRoleList>
                </el-tab-pane>

                <el-tab-pane name="VC_HH_WorkorChildsList" label="可分配类别" lazy>
                    <template #label>
                        <div ref="VC_HH_WorkorChildsListHead">可分配类别</div>
                    </template>
                    <VC_HH_WorkorChildsList :api="api" :bus="bus" :page="pageData"
                        :showTitle="false"  childed  :fk="VC_HH_WorkorTypeListFocusRow.HH_WorkorTypeOID"
                        v-model:focusRow="VC_HH_WorkorChildsListFocusRow"
                        title="可分配类别"
                        ref="VC_HH_WorkorChildsListBody"
                        :hideToolbar="focusTab_right!='VC_HH_WorkorChildsList'"
                        style="height:100%;"
                        :disabled="false"
                        :pageSize="50"
                        :flex="true"
                        height="100%"
                        :position="PanelrightHead"
                    >
                    </VC_HH_WorkorChildsList>
                </el-tab-pane>

                <el-tab-pane name="VC_HH_WorkorScopeManList" label="类别下的人" lazy>
                    <template #label>
                        <div ref="VC_HH_WorkorScopeManListHead">类别下的人</div>
                    </template>
                    <VC_HH_WorkorScopeManList :api="api" :bus="bus" :page="pageData"
                        :showTitle="false"  childed  :fk="VC_HH_WorkorTypeListFocusRow.HH_WorkorTypeOID"
                        v-model:focusRow="VC_HH_WorkorScopeManListFocusRow"
                        title="类别下的人"
                        ref="VC_HH_WorkorScopeManListBody"
                        :hideToolbar="focusTab_right!='VC_HH_WorkorScopeManList'"
                        style="height:100%;"
                        :disabled="false"
                        :pageSize="100"
                        :flex="true"
                        height="100%"
                        :position="PanelrightHead"
                    >
                    </VC_HH_WorkorScopeManList>
                </el-tab-pane>
            </el-tabs>
        </formcard>
    </template>
</cellpan>
<!-- #endregion autoGenerate type="cover" name="cellpan" -->


</template>
<script setup>
///////////////////////////////////////////////////////////
//  Biz_WorkorTypeSet.vue
//  Biz_WorkorTypeSet基础页面
//  功能框架由平台生成，请勿修改标记为[cover]区域的代码，否则将有被生成器覆盖的风险，生成器的覆盖范围随配置逐渐提升，各级别按[cover][lv2][lv3]标记。
//  新增逻辑时，在标记[autoGenerate]区域外部添加,比如[lv3]扩展业务逻辑的下方。
//  [autoGenerate]区域含义：[cover]--被覆盖，请勿修改；[lv2]--在方法模式被生成器覆盖，基础模式可修改；[lv3]--在全量模式下会被生成器覆盖。
//  创建时间:      2026/2/2 16:00:48
///////////////////////////////////////////////////////////
//基础组件
import { unref, ref, reactive, onMounted, onUnmounted, getCurrentInstance,computed } from 'vue';
import { useRouter } from 'vue-router';
import useDataBus from "@/uiext/gene/hook/pageDataBus.js"

// #region [autoGenerate] [cover] [勿改] VC引入
import cellpan from "@/uiext/gene/panel/cellpan.vue"
import formcard from "@/ui/panels/formcard.vue"

//引入VC
import * as pageAPI from "./Biz_WorkorTypeSetapi.js"

import VC_HH_WorkorTypeList from "../VC/ListVC/VC_HH_WorkorTypeList.vue"
import VC_HH_WorkorRoleList from "../VC/ListVC/VC_HH_WorkorRoleList.vue"
import VC_HH_WorkorChildsList from "../VC/ListVC/VC_HH_WorkorChildsList.vue"
import VC_HH_WorkorScopeManList from "../VC/ListVC/VC_HH_WorkorScopeManList.vue"
// #endregion [autoGenerate] [cover] [勿改] VC引入
// #region [autoGenerate] [lv3] 自定义引入


// #endregion [autoGenerate] [lv3] 自定义引入
const api= function(){ 
	return Object.assign({}, pageAPI);
}();


const props = defineProps({
    param: {
        type: Object,
    },
    bpo: {
        type: Object,
    },
    
});
const emit = defineEmits([]);

// #region [autoGenerate] [cover] [勿改] 基础声明
const BPOName = "Biz_WorkorTypeSet";
const VC_HH_WorkorTypeListHead =ref(null);
const VC_HH_WorkorTypeListBody =ref(null);  
const VC_HH_WorkorTypeListFocusRow = ref({});
const VC_HH_WorkorRoleListHead =ref(null);
const VC_HH_WorkorRoleListBody =ref(null);  
const VC_HH_WorkorRoleListFocusRow = ref({});
const VC_HH_WorkorChildsListHead =ref(null);
const VC_HH_WorkorChildsListBody =ref(null);  
const VC_HH_WorkorChildsListFocusRow = ref({});
const VC_HH_WorkorScopeManListHead =ref(null);
const VC_HH_WorkorScopeManListBody =ref(null);  
const VC_HH_WorkorScopeManListFocusRow = ref({});
const Panelleft =ref(null);
const PanelleftHead =ref(null);
const Panelright =ref(null);
const PanelrightHead =ref(null);
const focusTab_left= ref('VC_HH_WorkorTypeList');
const focusTab_right= ref('VC_HH_WorkorRoleList');


// #endregion [autoGenerate] [cover] [勿改] 基础声明

// #region [autoGenerate] [lv1] 基础部分

//数据总线，也包含了事件总线
const bus= useDataBus(BPOName);
const router = useRouter();
const pageLoading= ref(true);
const pageData= ref({});



function loadPageInit() { 
    pageAPI.loadPageInit({}).then((res)=>{
        pageLoading.value=false;
        pageData.value=res;
    })
}


// #endregion [autoGenerate] [lv1] 基础部分

/********自定义函数--开始*******/
// #region [autoGenerate] [lv2] 业务方法


/***操作--开始***/

// 原：列表内直接插行添加（已改为弹窗表单，保留备查）
// function addType(){
//     var row={
//         WT_IsOn:true,
//         WT_Order:1,
//         WT_Title:"***管理员",
//         WT_Created:new Date(),
//         WT_CreateMan:pageData.value.user.name,
//         WT_CreateUserId:pageData.value.user.oid,
//     }
//     bus.dataMap.VC_HH_WorkorTypeList.mlist.push(row )
// }

//弹窗触发
function addRR(){
    var url = "sln_tools/BusinessUnit/BPO_RoleSelect.html?callback=addrrback";

    var w = new UCML.OpenShowWindow({ 
        //                                可最大化        //最大化        //可收起          //可关闭         //可拖动       //可改变大小
        frameMode: "frame",modal:true, maximizable: true,maximized:false,collapsible: true,closable:true,draggable: true,resizable: true,
        URL: url, scroll: "yes",  width: 700, ratioHeight: 90, title: "选择角色"});
    w.on("close",function(){
        //BC_PX_ClassBase.Refresh();
    })
    w.open();
}
//弹窗回调
function addrrback(vc){
   var bc=vc.dataTable;
   var ck= vc.getCheckOID();
   ck.forEach(function(li,i){
        rowIndex = bc.dataStore.findLocateOID("", li); 
        if(rowIndex != -1){
            bc.setIndexNoEvent(rowIndex );
            BC_HH_WorkorRoleBase.Insert();
            BC_HH_WorkorRoleBase.setFieldValue("UCML_RESPONSIBILITY_FK",bc.getOID());
            BC_HH_WorkorRoleBase.setFieldValue("RESP_NAME",bc.getFieldValue("RESP_NAME"));    
            BC_HH_WorkorRoleBase.setFieldValue("RESP_DESC_TEXT",bc.getFieldValue("RESP_DESC_TEXT"));    
        }
 
   });
   BusinessSubmit();
}

//弹窗触发
function addHC(){
    var url = "sln_tools/BusinessUnit/BPO_WorkTypersCheck.HTML?callback=addhcback";

    var w = new UCML.OpenShowWindow({ 
        //                                可最大化        //最大化        //可收起          //可关闭         //可拖动       //可改变大小
        frameMode: "frame",modal:true, maximizable: true,maximized:false,collapsible: true,closable:true,draggable: true,resizable: true,
        URL: url, scroll: "yes",  ratioWidth: 70, ratioHeight: 90, title: "选择管理员"});
    w.on("close",function(){
        //BC_PX_ClassBase.Refresh();
    })
    w.open();
}
//弹窗回调
function addhcback(vc){
   var bc=vc.dataTable;
   var ck= vc.getCheckOID();
   ck.forEach(function(li,i){
        rowIndex = bc.dataStore.findLocateOID("", li); 
        if(rowIndex != -1){
            bc.setIndexNoEvent(rowIndex );
			BC_HH_WorkorChildsBase.Insert();
			BC_HH_WorkorChildsBase.setFieldValue("HH_Boss_FK",BC_HH_WorkorTypeBase.getOID());
			BC_HH_WorkorChildsBase.setFieldValue("HH_WorkorType_FK",li);   
            BC_HH_WorkorChildsBase.setFieldValue("WT_IsOn",bc.getFieldValue("WT_IsOn")); 
			BC_HH_WorkorChildsBase.setFieldValue("WT_Title",bc.getFieldValue("WT_Title"));    
			BC_HH_WorkorChildsBase.setFieldValue("WT_Note",bc.getFieldValue("WT_Note"));    			
        }

 
   });
	promptSubmit=true;
	BusinessSubmit();  
}

function removeeHC(){
	BC_HH_WorkorChildsBase.Delete();
}

function genDivOrg(){

    var config ={ BPOName:"BPO_DivWorkorSet", methodName:"genOrgName", params:{}, onSuccess:function(e,t){
	    var res=JSON.parse(t);
	    if(res.status=="success"){
	         
	        swal({ text: res.data, icon: "info", button: "确认", timer: 2000, });
	        //BC_HH_DivWorkerBase.Refresh();            
	    }else{
	        swal({ text: res.data, icon: "error", button: "关闭", });
	    }
    }, onFailure:function(){}};
    return invokeBPO(config);
}

/***操作--结束***/

// #endregion [autoGenerate] [lv2] 业务方法

// #region [autoGenerate] [cover] [勿改] 分离式方法
/***分离式函数--开始***/
/***分离式函数--结束***/
// #endregion [autoGenerate] [cover] [勿改] 分离式方法
/********自定义函数--结束*******/


// #region [autoGenerate] [lv3] 扩展业务逻辑
onMounted(() => {
    loadPageInit();
    
});
onUnmounted(() => {
    
});

/********手写JS--开始*******/

/********手写JS--结束*******/
// #region [autoGenerate] [lv3] 扩展业务逻辑
</script>
<style lang="css" scoped>
/**自定义样式--开始**/

/**自定义样式--结束**/
</style>

<style lang="css">
.Biz_WorkorTypeSet > .lr-left .formcard-shell--vc {
    padding-right: 6px;
}
.Biz_WorkorTypeSet > .lr-right .formcard-shell--vc {
    padding-left: 6px;
}
.Biz_WorkorTypeSet .workor-type-right-tabs {
    height: 100%;
    display: flex;
    flex-direction: column;
}
.Biz_WorkorTypeSet .workor-type-right-tabs > .el-tabs__header {
    flex-shrink: 0;
}
.Biz_WorkorTypeSet .workor-type-right-tabs > .el-tabs__content {
    flex: 1;
    min-height: 0;
}
.Biz_WorkorTypeSet .workor-type-right-tabs .el-tab-pane {
    height: 100%;
}
</style>