<template>
<div class="vcfm sroll" v-loading="loading">
    <div class="top-toolbar" v-if="!hideToolbar">
        <teleport  :to="position" :disabled="!position">
            <!-- #region autoGenerate type="cover" name="toolbar" -->
            

            <!-- #endregion autoGenerate type="cover" name="toolbar" -->
            <el-button type="primary" :disabled="disabled" :loading="saving" @click="onSave">
                <SvgIcon name="fa fa-save" /> 保存
            </el-button>
            <slot name="toolbar"></slot>
        </teleport>
    </div>

    <!-- #region autoGenerate type="cover" name="columns" -->
    <el-form class="gridfm fmvcbody" 
                label-width="110px"
                :disabled="disabled"
    >
        <fminput v-model="focusRow.WT_Title" label="标题"  type="text"  class="gc11" ></fminput>
        <fminput v-model="focusRow.WT_OSrc" label="适用组织类型"  type="dict" :dictlist="codeTables.HH_DOrgSrc" multi></fminput>
        <fminput v-model="focusRow.WT_IsOn" label="启用"  type="bool" ></fminput>
        <fminput v-model="focusRow.WT_Note" label="简介"  type="textarea"  class="gc11" ></fminput>
        <fminput v-model="focusRow.WT_Order" label="排序"  type="number" ></fminput>
        <fminput v-model="focusRow.WT_Have" label="包含下级"  type="bool" ></fminput>
        <fminput v-model="focusRow.WT_Cust" label="允许自定义"  type="text" ></fminput>
        <fminput v-model="focusRow.WT_Remark" label="备注"  type="text"  class="gc11" ></fminput>
        <fminput v-model="focusRow.WT_Created" label="创建时间"  type="datetime" disabled  ></fminput>
        <fminput v-model="focusRow.WT_CreateMan" label="创建人"  type="text" disabled  ></fminput>
        <template v-if="!adding">
        
        </template>
        <slot></slot>
    </el-form>
    <!-- #endregion autoGenerate type="cover" name="columns" -->
    <div class="wt-cust-hint">
        「允许自定义」说明：填写「单卡片」表示该类别的可分配子类别仅能在已授予该类别的同一张行政组织/矿区地点卡片下二次分配；留空则保持原有全局可分配逻辑。
    </div>
<!-- #region autoGenerate type="cover" name="splitpart" -->
<!-- #endregion autoGenerate type="cover" name="splitpart" --> 
</div>
</template>
<script setup>



import fminput from "@/ui/common/input/fminput.vue"

import { ref, computed, watch, onMounted,useAttrs } from 'vue';
//基础UI
import flexpan from "@/ui/common/flexpan.vue";

import livevc from "@/uiext/gene/live/livevc.vue";
//编辑控制
import useListEditor from "@/uiext/gene/hook/listeditor.js"
import useFormEditor from "@/uiext/gene/hook/useFormEditor.js"
//自定义引用

//其它引用

const listHolder = useListEditor();
const fmholder=useFormEditor();
const emit = defineEmits(['change', 'update:focusRow', 'saved','refresh', 'close', 'update:adding', 'update:row'])
//视图显示前

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
    para:null,//查询参数
    oid:null,//传入主键时，通过主键加载行数据
    row:null,//行数据，传入时直接使用
    adding:{ type:Boolean,default:false },//区分添加和修改
    
})


//定义响应式数据
const focusRow = ref({});
const diaRow = ref(false); // 行编辑表单
const adding = ref(false);

const VCName="VC_HH_WorkorTypeEdit";
const loading = ref(false);
const saving = ref(false);
// 计算属性
const rowpara = computed(() => {
    if(!!props.oid){
        return props.oid;
    }
    if(!!props.row){
        return props.row.HH_WorkorTypeOID;
    }
    return null;
});

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




// 方法
const loadRow = () => {

    if(props.row !=void 0 && props.row !=null){
        focusRow.value = props.row;
        if(!props.adding){
            fmholder.init(props.row,"HH_WorkorTypeOID");        
        }

		if(props.bus!=void 0 && props.bus  !=null){
		    props.bus.setRow(VCName,focusRow);
		}
        
        emit('update:focusRow', props.row);        
        return;
    }
    if(!rowpara.value){
        focusRow.value={};

        return;
    }
    loading.value=true;
    return props.api.queryBCHHWorkorTypeById(rowpara.value).then((res) => {
        focusRow.value = res;
        loading.value=false;
        fmholder.init(res,"HH_WorkorTypeOID");
        props.bus.setRow(VCName,focusRow);
        emit('update:focusRow', res);
    });
};

function saveChange(istip) {
    const para = fmholder.compare(focusRow.value);
    if(para==null){
        if(istip !=false){
            swal({ text: "没有数据变更", icon: "info", button: "关闭", });
        }
        return false;
    }
    
    var tar=[para];
    return props.api.saveBCHHWorkorTypeList(tar).then((res) => {
        if(istip !=false){
            if(res.status=="success"){
                swal({ text: res.data, icon: "success", button: "关闭", });
            }else{
                swal({ text: res.data, icon: "error", button: "关闭", });
            }
        }
        if(props.adding && Array.isArray(res.list) && res.list.length>0){
            //在添加模式下，需要保存后返回的主键等信息刷入到焦点输入行
            var savedRow = res.list[0];
            focusRow.value = savedRow;
            emit("update:focusRow",savedRow);
            emit("update:adding",false);
        }
        else{
            loadRow();
        }
        
        emit("saved",res);
        emit("refresh");
        
        return res;
    });
    
}

const removeRow = (row) => {
    const para = [row.HH_WorkorTypeOID];
    alertIsOK("确认要操作吗？将删除当前列表组件吗！",function(e){
        if(e=="yes"){
            props.api.removeBCHHWorkorTypeByIds(para).then((res) => {
                proxy.$message({ type: 'success', message: res.msg });
                getlist();
            });
        }
    },"删除数据行：");
};

// 监听 listpara 变化
watch(rowpara, () => {
    loadRow();
});
//绑定的规范化响应式
const watchRowEdit=watch(focusRow,()=>{ emit("update:row",focusRow.value) });

// 挂载时调用 getlist
onMounted(() => {
    if(props.bus!=void 0 && props.bus  !=null){
        props.bus.initVCEdit(VCName,focusRow);
        props.bus.registerAction(VCName,loadRow,saveChange,removeRow);
        props.bus.hub.on("refresh",loadRow);
    }
    if(props.autoLoad==true){
        loadRow();
    }

    if(props.row==null ||props.row==void 0){
        watchRowEdit();
    }
//JS业务初始化时刻--start

//JS业务初始化时刻--end
});

// #endregion [autoGenerate] [init] 自动初始化

// #region [autoGenerate] [lv2] 业务方法
/**-|方法|-start**/
/**-|方法|-end**/
// #endregion [autoGenerate] [lv2] 业务方法

// #region [autoGenerate] [cover] [勿改] 工具栏响应方法
/***工具栏响应方法--开始***/


/***工具栏响应方法--结束***/
// #endregion [autoGenerate] [cover] [勿改] 工具栏响应方法


// #region [autoGenerate] [lv3] 扩展业务逻辑
async function onSave() {
    if (props.disabled || saving.value) return;
    saving.value = true;
    try {
        const res = await saveChange();
        if (res && res.status === 'success') {
            emit('close');
        }
    } catch (err) {
        swal({ text: err?.message || '保存失败', icon: 'error', button: '关闭' });
    } finally {
        saving.value = false;
    }
}

defineExpose({
    saveChange,
    onSave,
});
// #endregion [autoGenerate] [lv3] 扩展业务逻辑

</script>

<style scoped>
/* 自定义样式--start */
.top-toolbar {
    display: flex;
    align-items: center;
    justify-content: flex-end;
    gap: 8px;
    margin-bottom: 12px;
}
.wt-cust-hint {
    margin: 5px 0;
    padding: 8px 12px;
    font-size: 14px;
    font-weight: bold;
    line-height: 1.5;
    color: #2e7d32;
    background: #e8f5e9;
    border: 1px solid #a5d6a7;
    border-radius: 4px;
}
/* 自定义样式--end */
</style>