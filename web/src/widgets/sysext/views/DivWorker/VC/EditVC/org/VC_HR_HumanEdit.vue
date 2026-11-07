<template>
<div class="vcfm sroll" :class="{ 'vcfm--profile': profile }" v-loading="loading">
    <div class="top-toolbar" v-if="!hideToolbar && !profile">
        <teleport  :to="position" :disabled="!position">
            <!-- #region autoGenerate type="cover" name="toolbar" -->
            

            <!-- #endregion autoGenerate type="cover" name="toolbar" --> 
            <slot name="toolbar"></slot>
        </teleport>
    </div>

    <article v-if="profile" class="human-profile">
        <header class="human-profile__head">
            <div class="human-profile__avatar">{{ avatarText }}</div>
            <div class="human-profile__identity">
                <h2 class="human-profile__name">
                    <span class="human-profile__name-text">{{ displayText(focusRow.H_Name) }}</span>
                    <span v-if="focusRow.H_MainCode" class="human-profile__account-inline">{{ focusRow.H_MainCode }}</span>
                </h2>
                <p v-if="focusRow.O_Path" class="human-profile__org-path">{{ focusRow.O_Path }}</p>
            </div>
            <span
                class="human-profile__status"
                :class="isActive ? 'human-profile__status--active' : 'human-profile__status--inactive'"
            >
                {{ isActive ? '已激活' : '已停用' }}
            </span>
        </header>

        <div class="human-profile__body">
            <div class="human-profile__grid">
                <div
                    v-for="item in profileItems"
                    :key="item.label"
                    class="human-profile__item"
                    :class="{ 'human-profile__item--wide': item.wide }"
                >
                    <span class="human-profile__label">{{ item.label }}</span>
                    <span class="human-profile__value" :title="item.value">{{ item.value }}</span>
                </div>
            </div>
        </div>
    </article>

    <!-- #region autoGenerate type="cover" name="columns" -->
    <el-form v-else class="gridfm fmvcbody" 
                label-width="110px"
                :disabled="disabled"
    >
        <fminput v-model="focusRow.H_Name" label="姓名"  ></fminput>
        <fminput v-model="focusRow.H_StartWork" label="参加工作时间"  type="datetime" ></fminput>
        <fminput v-model="focusRow.H_MainCode" label="主数据编码"  ></fminput>
        <fminput v-model="focusRow.H_TechPost" label="专业技术岗位"  ></fminput>
        <fminput v-model="focusRow.H_ZYGZ" label="现职业工种"  ></fminput>
        <fminput v-model="focusRow.H_PostRank" label="层级"  ></fminput>
        <fminput v-model="focusRow.H_JobType" label="岗位工种"  ></fminput>
        <fminput v-model="focusRow.H_Class" label="人员分类"  ></fminput>
        <fminput v-model="focusRow.H_PostName" label="岗位名称"  ></fminput>
        <fminput v-model="focusRow.H_DataSrc" label="数据来源"  ></fminput>
        <fminput v-model="focusRow.H_Disabled" label="停用"  type="bool" ></fminput>
        <fminput v-model="focusRow.H_HRModifyDate" label="修改时间"  type="datetime" ></fminput>
        <fminput v-model="focusRow.H_MultSrc" label="来源"  type="bool" ></fminput>
        <fminput v-model="focusRow.H_IndIDBef" label="前独立公司ID"  type="number" ></fminput>
        <template v-if="!adding">
        
        </template>
        <slot></slot>
    </el-form>
    <!-- #endregion autoGenerate type="cover" name="columns" -->     
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
const emit = defineEmits(['change', 'delete', 'update:focusRow', 'update:row'])
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
    profile:{ type:Boolean,default:false },
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

const VCName="VC_HR_HumanEdit";
const loading = ref(false);

function displayText(val) {
    if (val === null || val === undefined || val === '') return '-';
    return String(val);
}

const avatarText = computed(() => {
    const name = focusRow.value?.H_Name;
    if (!name) return '?';
    return String(name).trim().charAt(0);
});

const isActive = computed(() => {
    const v = focusRow.value?.H_Disabled;
    return v === true || v === 1 || v === '1' || v === 'true';
});

const profileItems = computed(() => {
    const row = focusRow.value || {};
    return [
        { label: '岗位', value: displayText(row.H_PostName) },
        { label: '学历', value: displayText(row.H_FirstEdu) },
        { label: '岗位类型', value: displayText(row.H_JobType) },
        { label: '技能等级', value: displayText(row.H_TechPost) },
    ].filter((item) => item.value !== '-');
});

// 计算属性
const rowpara = computed(() => {
    if(!!props.oid){
        return props.oid;
    }
    if(!!props.row){
        return props.row.HR_HumanOID;
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
        if(props.adding){
            fmholder.init({},"HR_HumanOID");
        }
        else{
            fmholder.init(props.row,"HR_HumanOID");        
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
    return props.api.queryBCHRHumanById(rowpara.value).then((res) => {
        focusRow.value = res;
        loading.value=false;
        fmholder.init(res,"HR_HumanOID");
        if (props.bus) {
            props.bus.setRow(VCName,focusRow);
        }
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
    return props.api.saveBCHRHumanList(tar).then((res) => {
        if(istip !=false){swal({ text: res.data, icon: "info", button: "关闭", });}
        loadRow();
        
        return res;
    });
    
}

const removeRow = (row) => {
    const para = [row.HR_HumanOID];
    alertIsOK("确认要操作吗？将删除当前列表组件吗！",function(e){
        if(e=="yes"){
            props.api.removeBCHRHumanByIds(para).then((res) => {
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
const watchRowEdit=watch(focusRow,()=>{ emit("update:row",focusRow.value); emit('update:focusRow', focusRow.value); });

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

// #region [autoGenerate] [lv3] 扩展业务逻辑



</script>

<style scoped>
.vcfm--profile {
    padding: 0;
}

.human-profile {
    background: #fff;
    border: 1px solid var(--el-border-color-lighter, #ebeef5);
    border-radius: 12px;
    overflow: hidden;
    box-shadow: 0 4px 16px rgba(15, 23, 42, 0.06);
}

.human-profile__head {
    display: flex;
    align-items: center;
    gap: 16px;
    padding: 20px 24px;
    background: linear-gradient(135deg, #f0f7ff 0%, #fafbff 48%, #fff 100%);
    border-bottom: 1px solid var(--el-border-color-lighter, #ebeef5);
}

.human-profile__avatar {
    flex-shrink: 0;
    width: 56px;
    height: 56px;
    border-radius: 14px;
    background: linear-gradient(145deg, #409eff 0%, #337ecc 100%);
    color: #fff;
    font-size: 24px;
    font-weight: 600;
    line-height: 56px;
    text-align: center;
    box-shadow: 0 6px 16px rgba(64, 158, 255, 0.28);
}

.human-profile__identity {
    flex: 1;
    min-width: 0;
}

.human-profile__name {
    display: flex;
    align-items: baseline;
    flex-wrap: wrap;
    gap: 8px 10px;
    margin: 0 0 4px;
    font-size: 20px;
    font-weight: 600;
    line-height: 1.3;
    color: var(--el-text-color-primary, #303133);
}

.human-profile__name-text {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    max-width: 100%;
}

.human-profile__account-inline {
    font-size: 14px;
    font-weight: 500;
    color: var(--el-text-color-secondary, #909399);
    font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
}

.human-profile__org-path {
    margin: 0;
    font-size: 13px;
    color: var(--el-text-color-regular, #606266);
    line-height: 1.5;
    word-break: break-all;
}

.human-profile__status {
    flex-shrink: 0;
    padding: 4px 12px;
    border-radius: 999px;
    font-size: 12px;
    font-weight: 500;
    line-height: 1.5;
}

.human-profile__status--active {
    color: #389e0d;
    background: #f6ffed;
    border: 1px solid #b7eb8f;
}

.human-profile__status--inactive {
    color: #cf1322;
    background: #fff1f0;
    border: 1px solid #ffa39e;
}

.human-profile__body {
    padding: 16px 24px 20px;
}

.human-profile__grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
    gap: 12px 24px;
}

.human-profile__item {
    display: flex;
    flex-direction: column;
    gap: 4px;
    min-width: 0;
}

.human-profile__label {
    font-size: 12px;
    color: var(--el-text-color-secondary, #909399);
    line-height: 1.4;
}

.human-profile__value {
    font-size: 14px;
    color: var(--el-text-color-primary, #303133);
    line-height: 1.5;
    word-break: break-all;
}

.human-profile__item--wide {
    grid-column: 1 / -1;
}

.human-profile__item--wide .human-profile__value {
    display: block;
}

@media (max-width: 640px) {
    .human-profile__head {
        flex-wrap: wrap;
        padding: 16px;
    }

    .human-profile__status {
        margin-left: 72px;
    }

    .human-profile__body {
        padding: 12px 16px 16px;
    }

    .human-profile__grid {
        grid-template-columns: 1fr;
    }
}
</style>
