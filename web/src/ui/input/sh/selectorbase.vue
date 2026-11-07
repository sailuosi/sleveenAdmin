<template>
    <el-form-item :label="label" :prop="prop" :span="span" :labelWidth="labelWidth" :field="name" :required="required">
        <template #default="p">
            <searchput @search="loadList" @more="onMorePanel" @panel-opened="$emit('panel-opened')" ref="searchputRef" 
                :trigger="trigger" :hideTag="hideTag"
                :mtype="mtype" :mwidth="mwidth" :mheight="mheight" :mplacement="mplacement" :mtrigger="mtrigger" :mtitle="mtitle"
                :disabled="disabled" :useMorePanel="useMorePanel" :dropdownWidth="dropdownWidth"
                :openOnFocus="openOnFocus"
                :placeholder="placeholder"
                :tagWidth="tagWidth" :tagWrap="effectiveTagWrap"
                :mode="mode" :theme="theme" :moreButtonText="moreButtonText" :moreButtonType="moreButtonType"
                v-model="query"
                v-model:showDropdown="showDropdown"
                :multiple="multiple"
                v-model:list="mlist"
                @remove="onRmLinkItem"
            >

                <template #dropdown>
                    <searchbody :list="selectList" :rowKey="rowKey"
                        :onload="loadListForSearchBody"
                        @update:modelValue="onSelect($event)"
                        v-model:txt="query"
                        v-model:searching="searching"
                    >
                        <template #option="opt">
                            <slot name="option" v-bind="opt"></slot>
                        </template>
                        <template #default>
                            <el-button circle @click="loadCurrent" v-if="refreshable">
                                <svg class="icon" style="width: 1em;height: 1em;vertical-align: middle;fill: currentColor;overflow: hidden;" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="2648"><path d="M832 512a32 32 0 0 0-32 32c0 158.784-129.216 288-288 288s-288-129.216-288-288 129.216-288 288-288c66.208 0 129.536 22.752 180.608 64H608a32 32 0 0 0 0 64h160a32 32 0 0 0 32-32V192a32 32 0 0 0-64 0v80.96A350.464 350.464 0 0 0 512 192C317.92 192 160 349.92 160 544s157.92 352 352 352 352-157.92 352-352a32 32 0 0 0-32-32" fill="#3E3A39" p-id="2649"></path></svg>
                            </el-button>
                            <el-button circle @click="onCloseDropdown" >
                                <svg class="icon" style="width: 1em;height: 1em;vertical-align: middle;fill: currentColor;overflow: hidden;" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="3488"><path d="M563.8 512l262.5-312.9c4.4-5.2 0.7-13.1-6.1-13.1h-79.8c-4.7 0-9.2 2.1-12.3 5.7L511.6 449.8 295.1 191.7c-3-3.6-7.5-5.7-12.3-5.7H203c-6.8 0-10.5 7.9-6.1 13.1L459.4 512 196.9 824.9c-4.4 5.2-0.7 13.1 6.1 13.1h79.8c4.7 0 9.2-2.1 12.3-5.7l216.5-258.1 216.5 258.1c3 3.6 7.5 5.7 12.3 5.7h79.8c6.8 0 10.5-7.9 6.1-13.1L563.8 512z" p-id="3489"></path></svg>
                            </el-button>
                        </template>
                    </searchbody>
                </template>
                <template #tag="opt">
                    <slot name="tag" v-bind="opt"></slot>
                </template>
                <template #prefix="pfp">
                    <slot>
                        <el-tag v-if="empty && !multiple" @click="pfp.open" class="em0-tip">未选择</el-tag>
                        <template v-if="!empty && !multiple">
                            <el-tag
                                :style="prefixTagStyle"
                                @click="pfp.open"
                                :closable="clearable && !disabled"
                                @close="onClearSingle"
                            >
                                <slot name="tag">{{ loadName() }}</slot>
                            </el-tag>
                        </template>
                    </slot>
                </template>
                <template #mheader="hp">
                    <slot name="mheader" v-bind="hp"></slot>
                </template>
                <template #mbody="mbp">
                    <slot name="mbody" v-bind="mbp"></slot>
                </template>
                <template #mfooter="fp">
                    <slot name="mfooter" v-bind="fp"></slot>
                </template>
            </searchput>
        </template>
        <template #label="labelp">
            <slot name="label" v-bind="labelp">{{ label }}</slot>
        </template>
        <template #error="errorp">
            <slot name="error" v-bind="errorp">{{ error }}</slot>
        </template>
    </el-form-item>
</template>

<script setup>

import { reactive, computed, inject, ref, onMounted } from 'vue';
//组件

import searchput from './searchput.vue';

import searchbody from './searchbody.vue';


//接口


const searchputRef = ref(null)
const emit = defineEmits(['change', 'select', 'more-panel', 'clear', 'remove','delete', 'panel-opened'])
const props = defineProps({
    label: { type: String, default: "" },
    prop: [String, Number, Array],
    span: null,
    labelWidth: { type: String,default:null },
    placement: { type: String, default: "auto" },
    dropdownWidth: { type: String, default: null },
    trigger:null,
    type: { type: String, default: "popover" },
    height: null,
    disabled: { type: Boolean, default: false },
    closeOnSelect: { type: Boolean, default: true },
    hideTag: { type: Boolean, default: false },
    /** 为 true 时标签前显示必填星号（与 el-form-item required 一致） */
    required: { type: Boolean, default: false },
    refreshable: { type: Boolean, default: false },
    width: { type: String, default: "500px" },
    useMorePanel: { type: Boolean, default: false },
    currentRow: {
        type: Object,
        default: () => ({})
    },
    sysCode: null,//系统编码
    bpo: null,
    useAuth: { type: Boolean, default: false },
    onload: Function,
    multiple: { type: Boolean, default: false },//是否多选
    limit: { type: Number, default: null },//多选时，最多选择多少个
    limitMsg: { type: String, default: "最多选择{limit}个" },//多选时，最多选择多少个的提示

    /** 未选值时输入框占位；已选值后由模板传空，避免与标签并列仍显示「请输入...」 */
    placeholder: { type: String, default:null },

    /** 单选已选后是否显示标签关闭按钮，用于清空 fk/name/code 等绑定 */
    clearable: { type: Boolean, default: true },

    /** tag 最大宽度，透传 searchput；单选 prefix tag 同步生效 */
    tagWidth: { type: [String, Number], default: null },
    /** true/false 显式控制；null 时 mode=wrap 默认开启换行 */
    tagWrap: { type: Boolean, default: null },

    /** 布局：inline 单行；wrap 首行 tag，次行搜索+更多 */
    mode: { type: String, default: 'inline' },
    /** UI 风格：line 线框（默认）；card 色块 */
    theme: { type: String, default: 'line' },
    /** 更多按钮文案；null 时按 mode 取默认 */
    moreButtonText: { type: String, default: null },
    /** 更多按钮类型 text|link|button；null 时按 mode 取默认 */
    moreButtonType: { type: String, default: null },

    rowKey: { type: String, default: '' },

    mtype: { type: String, default: 'popover' },
    mwidth: { type: String, default: '480px' },
    mheight: { type: String, default: '' },
    mtitle: { type: String, default: '' },
    mplacement: { type: String, default: 'bottom-end' },
    mtrigger: { type: String, default: 'click' },
    openOnFocus: { type: Boolean, default: true },
    selectOnUnique: { type: Boolean, default: false },//是否唯一时自动选择
})

const query= defineModel('txt');
const fk = defineModel('fk');
const name = defineModel('name');
const code = defineModel('code');
const OrgNo = defineModel('OrgNo');
const mineFK = defineModel('mineFK');
const value = defineModel('modelValue');
//多选时，绑定值为列表
const mlist = defineModel('list');
const searching= defineModel('searching',{type: Boolean, default: false});//是否正在搜索
const showDropdown = defineModel('showDropdown', { type: Boolean, default: false });
const pk = ref("MD_MineOID")
const stoped = computed(() => {
    if (props.disabled) {
        return true;
    }


    return false;
});


const empty=computed(()=>{
    if(props.multiple){
        return mlist.value.length === 0;
    }
    if(!!value.value){
        return false;
    }
    if(fk.value){
        return false;
    }
    if(name.value){
        return false;
    }
    if(code.value){
        return false;
    }
    return true;
});

function toCssSize(v) {
    if (v == null || v === '') return null
    return typeof v === 'number' ? `${v}px` : String(v)
}

/** wrap 模式默认开启 tag 换行；显式传入 tagWrap 时优先 */
const effectiveTagWrap = computed(() => {
    if (props.tagWrap != null) return !!props.tagWrap
    return props.mode === 'wrap'
})

const tagWidthCss = computed(() => toCssSize(props.tagWidth))
const prefixTagStyle = computed(() => {
    const style = {}
    const wrap = effectiveTagWrap.value
    if (tagWidthCss.value) {
        style.maxWidth = tagWidthCss.value
    } else if (wrap) {
        style.maxWidth = '100%'
    }
    if (wrap) {
        style.height = 'auto'
        style.whiteSpace = 'normal'
        style.wordBreak = 'break-word'
        style.overflow = 'visible'
        style.textOverflow = 'unset'
    } else if (tagWidthCss.value) {
        style.overflow = 'hidden'
        style.textOverflow = 'ellipsis'
        style.whiteSpace = 'nowrap'
    }
    return style
})

const selectList=ref([]);
function loadList(seachTxt) {
    return props.onload({
        txt: seachTxt,
        bpo: props.bpo,
        useAuth: props.useAuth
    }).then(res => {
        selectList.value = res;
        if(props.selectOnUnique && res.length == 1){
            onSelect(res[0]);
        }
        return res;
    });
}

/** 下拉内「输入搜索关键字」与 searchbody 约定：onload(inputKey, page)，与外层 loadList 共用同一套远程查询 */
function loadListForSearchBody(inputKey) {
    return loadList(inputKey ?? '');
}

function onMorePanel() {
    emit('more-panel');
}

function loadName(p, row) {

    var showName = name.value + '';
    if (!showName) {
        if (!!code.value) {
            return code.value;
        }
        return props.placeholder;
    }
    if (!!code.value && code.value != showName) {
        showName = "【" + code.value + "】" + showName;
    }
    return showName;
}

function onSelect(row, p, modal) {
    emit('select', row);
    if(props.closeOnSelect){
        query.value = '';
        showDropdown.value = false;
        searchputRef.value?.closeDropdown?.();
    }
    
}
function loadCurrent() {
    inputsearchRef.value.load();
}

function onCloseDropdown() {
    query.value = '';
    showDropdown.value = false;
}

function openMorePanel() {
    searchputRef.value?.openMorePanel?.()
}

//多选逻辑部分

function onRmLinkItem(row) {
    emit('remove', row);
    emit('delete', row);//兼容老版本
}

/** 单选：关闭标签时清空与选择器相关的 v-model，并通知外层清理 path 等扩展字段 */
function onClearSingle(ev) {
    ev?.stopPropagation?.();
    fk.value = '';
    name.value = '';
    code.value = '';
    OrgNo.value = '';
    mineFK.value = '';
    value.value = undefined;
    emit('clear');
}

onMounted(() => {

});

defineExpose({
    openMorePanel,
})
</script>
