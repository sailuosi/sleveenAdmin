<template>
<!-- #region autoGenerate type="cover" name="cellpan" -->
<cellpan type="lr" left="1fr" right="1fr"  flex bodyFlex  class="Biz_Sys_PostTypeSet">   
    <template #left="panp">
            <formcard title="岗位类别" width=""  flex stretch>
                <template #toolbar>
                    <div ref="VC_Sys_PostTypeListHead"></div>
                </template>
                <VC_Sys_PostTypeList :api="api" :bus="bus" :page="pageData"
                    :showTitle="false" 
                    v-model:focusRow="VC_Sys_PostTypeListFocusRow"
                    title="岗位类别"
                    ref="VC_Sys_PostTypeListBody"
                    
                    style="height:100%;"                    
                    :disabled="false"
                    :pageSize="100"
                    :flex="true"
                    height="100%"
                    :position="VC_Sys_PostTypeListHead"
                    >
                </VC_Sys_PostTypeList>
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
                <el-tab-pane name="VC_Sys_PostTypeRoleList" label="岗位类别角色" lazy>
                    <template #label>
                        <div ref="VC_Sys_PostTypeRoleListHead">岗位类别角色</div>
                    </template>
                    <VC_Sys_PostTypeRoleList :api="api" :bus="bus" :page="pageData"
                        :showTitle="false"  childed  :fk="VC_Sys_PostTypeListFocusRow.Pt_Id"
                        v-model:focusRow="VC_Sys_PostTypeRoleListFocusRow"
                        title="岗位类别角色"
                        ref="VC_Sys_PostTypeRoleListBody"
                        :hideToolbar="focusTab_right!='VC_Sys_PostTypeRoleList'"
                        style="height:100%;"
                        disabled
                        :pageSize="50"
                        :flex="true"
                        height="100%"
                        :position="PanelrightHead"
                    >
                    </VC_Sys_PostTypeRoleList>
                </el-tab-pane>

                <el-tab-pane name="VC_Sys_PostTypeChildList" label="下级岗位类别" lazy>
                    <template #label>
                        <div ref="VC_Sys_PostTypeChildListHead">下级岗位类别</div>
                    </template>
                    <VC_Sys_PostTypeChildList :api="api" :bus="bus" :page="pageData"
                        :showTitle="false"  childed  :fk="VC_Sys_PostTypeListFocusRow.Pt_Id"
                        v-model:focusRow="VC_Sys_PostTypeChildListFocusRow"
                        title="下级岗位类别"
                        ref="VC_Sys_PostTypeChildListBody"
                        :hideToolbar="focusTab_right!='VC_Sys_PostTypeChildList'"
                        style="height:100%;"
                        :disabled="false"
                        :pageSize="50"
                        :flex="true"
                        height="100%"
                        :position="PanelrightHead"
                    >
                    </VC_Sys_PostTypeChildList>
                </el-tab-pane>

                <el-tab-pane name="VC_Sys_PostScopeList" label="类别下的人" lazy>
                    <template #label>
                        <div ref="VC_Sys_PostScopeListHead">类别下的人</div>
                    </template>
                    <VC_Sys_PostScopeList :api="api" :bus="bus" :page="pageData"
                        :showTitle="false"  childed  :fk="VC_Sys_PostTypeListFocusRow.Pt_Id"
                        v-model:focusRow="VC_Sys_PostScopeListFocusRow"
                        title="类别下的人"
                        ref="VC_Sys_PostScopeListBody"
                        :hideToolbar="focusTab_right!='VC_Sys_PostScopeList'"
                        style="height:100%;"
                        :disabled="false"
                        :pageSize="100"
                        :flex="true"
                        height="100%"
                        :position="PanelrightHead"
                    >
                    </VC_Sys_PostScopeList>
                </el-tab-pane>
            </el-tabs>
        </formcard>
    </template>
</cellpan>
<!-- #endregion autoGenerate type="cover" name="cellpan" -->


</template>
<script setup>
///////////////////////////////////////////////////////////
//  Biz_Sys_PostTypeSet.vue
//  Biz_Sys_PostTypeSet基础页面
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
import * as pageAPI from "./Biz_Sys_PostTypeSetapi.js"

import VC_Sys_PostTypeList from "../VC/ListVC/VC_Sys_PostTypeList.vue"
import VC_Sys_PostTypeRoleList from "../VC/ListVC/VC_Sys_PostTypeRoleList.vue"
import VC_Sys_PostTypeChildList from "../VC/ListVC/VC_Sys_PostTypeChildList.vue"
import VC_Sys_PostScopeList from "../VC/ListVC/VC_Sys_PostScopeList.vue"
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
const BPOName = "Biz_Sys_PostTypeSet";
const VC_Sys_PostTypeListHead =ref(null);
const VC_Sys_PostTypeListBody =ref(null);  
const VC_Sys_PostTypeListFocusRow = ref({});
const VC_Sys_PostTypeRoleListHead =ref(null);
const VC_Sys_PostTypeRoleListBody =ref(null);  
const VC_Sys_PostTypeRoleListFocusRow = ref({});
const VC_Sys_PostTypeChildListHead =ref(null);
const VC_Sys_PostTypeChildListBody =ref(null);  
const VC_Sys_PostTypeChildListFocusRow = ref({});
const VC_Sys_PostScopeListHead =ref(null);
const VC_Sys_PostScopeListBody =ref(null);  
const VC_Sys_PostScopeListFocusRow = ref({});
const Panelleft =ref(null);
const PanelleftHead =ref(null);
const Panelright =ref(null);
const PanelrightHead =ref(null);
const focusTab_left= ref('VC_Sys_PostTypeList');
const focusTab_right= ref('VC_Sys_PostTypeRoleList');


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
.Biz_Sys_PostTypeSet .workor-type-right-tabs {
    height: 100%;
    display: flex;
    flex-direction: column;
}
.Biz_Sys_PostTypeSet .workor-type-right-tabs > .el-tabs__header {
    flex-shrink: 0;
}
.Biz_Sys_PostTypeSet .workor-type-right-tabs > .el-tabs__content {
    flex: 1;
    min-height: 0;
}
.Biz_Sys_PostTypeSet .workor-type-right-tabs .el-tab-pane {
    height: 100%;
}
</style>