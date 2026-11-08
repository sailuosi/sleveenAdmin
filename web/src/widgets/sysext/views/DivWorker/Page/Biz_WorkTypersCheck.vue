<template>
<!-- #region autoGenerate type="cover" name="cellpan" -->
<cellpan type="one" flex bodyFlex class="Biz_WorkTypersCheck">   
    <template #default="panp">
            <formcard title="管理员类别" width=""  flex stretch>
                <template #toolbar>
                    <div ref="VC_HH_WorkorTypeCheckHead"></div>
                </template>
                <VC_HH_WorkorTypeCheck :api="api" :bus="bus" :page="pageData"
                    :showTitle="false" 
                    v-model:focusRow="VC_HH_WorkorTypeCheckFocusRow"
                    title="管理员类别"
                    ref="VC_HH_WorkorTypeCheckBody"
                    
                    style="height:100%;"                    
                    disabled
                    :pageSize="50"
                    :flex="true"
                    height="100%"
                    @checking="$emit('checking',$event)"
                    @close="$emit('close')"
                    :position="VC_HH_WorkorTypeCheckHead"
                >
                </VC_HH_WorkorTypeCheck>
            </formcard>


    </template>
</cellpan>
<!-- #endregion autoGenerate type="cover" name="cellpan" -->


</template>
<script setup>
///////////////////////////////////////////////////////////
//  Biz_WorkTypersCheck.vue
//  Biz_WorkTypersCheck基础页面
//  功能框架由平台生成，请勿修改标记为[cover]区域的代码，否则将有被生成器覆盖的风险，生成器的覆盖范围随配置逐渐提升，各级别按[cover][lv2][lv3]标记。
//  新增逻辑时，在标记[autoGenerate]区域外部添加,比如[lv3]扩展业务逻辑的下方。
//  [autoGenerate]区域含义：[cover]--被覆盖，请勿修改；[lv2]--在方法模式被生成器覆盖，基础模式可修改；[lv3]--在全量模式下会被生成器覆盖。
//  创建时间:      2026/2/2 16:02:22
///////////////////////////////////////////////////////////
//基础组件
import { unref, ref, reactive, onMounted, onUnmounted, getCurrentInstance,computed } from 'vue';
import { useRouter } from 'vue-router';
import useDataBus from "@/uiext/gene/hook/pageDataBus.js"

// #region [autoGenerate] [cover] [勿改] VC引入
import cellpan from "@/uiext/gene/panel/cellpan.vue"
import formcard from "@/ui/panels/formcard.vue"

//引入VC
import * as pageAPI from "./Biz_WorkTypersCheckapi.js"

import VC_HH_WorkorTypeCheck from "../VC/ListVC/VC_HH_WorkorTypeCheck.vue"  
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
const BPOName = "Biz_WorkTypersCheck";
const VC_HH_WorkorTypeCheckHead =ref(null);
const VC_HH_WorkorTypeCheckBody =ref(null);  
const VC_HH_WorkorTypeCheckFocusRow = ref({});
const Paneldefault =ref(null);
const PaneldefaultHead =ref(null);
const focusTab_default= ref('VC_HH_WorkorTypeCheck');


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