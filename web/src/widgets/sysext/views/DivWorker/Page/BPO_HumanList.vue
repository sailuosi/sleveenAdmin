<template>
<!-- #region autoGenerate type="cover" name="cellpan" -->
<cellpan type="lr" left="1fr" right="4fr"  flex bodyFlex  class="BPO_HumanList">
    <template #left="panp">
            <formcard title="人力组织" width=""  flex stretch>
                <template #toolbar>
                    <div ref="VC_HR_OrganizeHead"></div>
                </template>
                <VC_HR_Organize :api="api" :bus="bus" :page="pageData"
                    :showTitle="false" 
                    v-model:focusRow="VC_HR_OrganizeFocusRow"
                    title="人力组织"
                    ref="VC_HR_OrganizeBody"
                    @nodeclick="onOrgClick"
                    style="height:100%;"
                    disabled
                    :pageSize="10"
                    :flex="true"
                    height="100%"
                >
                </VC_HR_Organize>
            </formcard>


    </template>
    <template #right="panp">
            <formcard title="人力人员" width=""  flex stretch>
                <template #toolbar>
                    <div ref="VC_HR_HumanListHead"></div>
                </template>
                <VC_HR_HumanList :api="api" :bus="bus" :page="pageData" :showColumnQuery="true"
                    :showTitle="false" 
                    v-model:focusRow="VC_HR_HumanListFocusRow"
                    title="人力人员"
                    ref="VC_HR_HumanListBody"
                    :conditions="conditions"
                    style="height:100%;"
                    disabled
                    :pageSize="100"
                    :flex="true"
                    height="100%"
                    :position="VC_HR_HumanListHead"
                >
                </VC_HR_HumanList>
            </formcard>


    </template>
</cellpan>
<!-- #endregion autoGenerate type="cover" name="cellpan" -->


</template>
<script setup>
///////////////////////////////////////////////////////////
//  BPO_HumanList.vue
//  BPO_HumanList基础页面
//  功能框架由平台生成，请勿修改标记为[cover]区域的代码，否则将有被生成器覆盖的风险，生成器的覆盖范围随配置逐渐提升，各级别按[cover][lv2][lv3]标记。
//  新增逻辑时，在标记[autoGenerate]区域外部添加,比如[lv3]扩展业务逻辑的下方。
//  [autoGenerate]区域含义：[cover]--被覆盖，请勿修改；[lv2]--在方法模式被生成器覆盖，基础模式可修改；[lv3]--在全量模式下会被生成器覆盖。
//  创建时间:      2026/2/2 17:18:09
///////////////////////////////////////////////////////////
//基础组件
import { unref, ref, reactive, onMounted, onUnmounted, getCurrentInstance,computed } from 'vue';
import { useRouter } from 'vue-router';
import useDataBus from "@/uiext/gene/hook/pageDataBus.js"

// #region [autoGenerate] [cover] [勿改] VC引入
import cellpan from "@/uiext/gene/panel/cellpan.vue"
import formcard from "@/ui/panels/formcard.vue"

//引入VC
import * as pageAPI from "./BPO_HumanListapi.js"

import VC_HR_Organize from "../VC/ListVC/org/VC_HR_Organize.vue"
import VC_HR_HumanList from "../VC/ListVC/org/VC_HR_HumanList.vue"  
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
const BPOName = "BPO_HumanList";
const VC_HR_OrganizeHead =ref(null);
const VC_HR_OrganizeBody =ref(null);  
const VC_HR_OrganizeFocusRow = ref({});
const VC_HR_HumanListHead =ref(null);
const VC_HR_HumanListBody =ref(null);  
const VC_HR_HumanListFocusRow = ref({});
const Panelleft =ref(null);
const PanelleftHead =ref(null);
const Panelright =ref(null);
const PanelrightHead =ref(null);
const focusTab_left= ref('VC_HR_Organize');
const focusTab_right= ref('VC_HR_HumanList');


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


/***onOrgClick--开始***/

const focusOrg= ref(null);
const conditions= computed(()=>{
    var res=[{
        // 本页不展示 H_Type=3（在册减员），全量见 BPO_HumanListAll
        field:"H_Type",
        op:"<>",
        value:"3",
    }];
    if(!!focusOrg.value){
        res.push({
            field:"O_ClassCode",
            op:"likeLeft",
            value:focusOrg.value.O_ClassCode,
        })
    }

    return res;
})

function onOrgClick(e){
    focusOrg.value=e;

    console.log(e)
}

/***onOrgClick--结束***/

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

<style lang="css" >
</style>