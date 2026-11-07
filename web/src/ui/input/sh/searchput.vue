<template>
    <div class="searchput" :class="[attrs.class, theme === 'card' ? 'searchput--card' : 'searchput--line']" :style="attrs.style" ref="Vroot">
        <el-popover ref="mpopoverRef"
            :visible="queing"
            placement="bottom-start"
            :persistent="false"
            :teleported="teleported"
            :disabled="disabled"
            :width="resolvedDropdownWidth"
            :show-arrow="false"
            popper-class="searchput-dropdown-popper"
        >
            <template #default>
                <div ref="dropContent"
                class="searchput-dropdown"
                    :style="dropdownBodyStyle"
                    v-loading="loading"
                >
                    <slot name="dropdown" :keyword="query" :close="closeDropdown"></slot>
                </div>
            </template>
            <template #reference>
                <popinput
                    ref="inputRef"
                    v-model="query" :multiple="multiple"
                    v-model:list="list"
                    class="shput-pt"
                    :placeholder="placeholder"
                    :disabled="disabled"
                    :clearable="clearable"
                    :hideTag="hideTag"
                    :tagWidth="tagWidth"
                    :tagWrap="resolvedTagWrap"
                    :mode="mode"
                    :theme="theme"
                    v-bind="attrsPass"
                    @input="onInputChange"
                    @focus="onInputFocus"
                    @blur="onInputBlur"
                    @clear="onClear"
                    @keydown.enter.prevent="onEnterSearch"
                    @remove="$emit('remove',$event)"
                    @click="onInputClick"
                >
                    <template #tag="opt">
                        <slot name="tag" v-bind="opt"></slot>
                    </template>
                    <template #prefix>
                        <slot name="prefix" :open="openDropdown"></slot>
                    </template>
                    <template #append>
                        <div
                            v-if="useMorePanel && !disabled"
                            class="searchput-append-wrap"
                            :class="{ 'searchput-append-wrap--wrap': mode === 'wrap' }"
                            @mousedown.prevent.stop="onAppendMousedown"
                        >
                            <anyopen
                                ref="anyopenRef"
                                :type="mtype"
                                :width="mwidth"
                                :height="mheight"
                                :title="mtitle"
                                :placement="mplacement"
                                :trigger="mtrigger"
                                :disabled="disabled || mdisabled"
                                :close-on-click-modal="mcloseOnClickModal"
                            >
                                <template #default="dp">
                                    <slot name="more-button" :type="resolvedMoreButtonType" :text="resolvedMoreButtonText" :open="() => onMoreClick(dp)">
                                        <div
                                            v-if="resolvedMoreButtonType === 'text'"
                                            class="searchput-more-btn"
                                            title="更多"
                                            :disabled="disabled || moreDisabled"
                                            @click="onMoreClick(dp)"
                                        >
                                            {{ resolvedMoreButtonText }}
                                        </div>
                                        <el-button
                                            v-else-if="resolvedMoreButtonType === 'link'"
                                            link
                                            type="primary"
                                            :disabled="disabled || moreDisabled"
                                            @click="onMoreClick(dp)"
                                        >
                                            {{ resolvedMoreButtonText }}
                                        </el-button>
                                        <el-button
                                            v-else
                                            type="primary"
                                            size="small"
                                            :disabled="disabled || moreDisabled"
                                            @click="onMoreClick(dp)"
                                        >
                                            {{ resolvedMoreButtonText }}
                                        </el-button>
                                    </slot>
                                </template>
                                <template #header="hp">
                                    <slot name="mheader" :open="hp.open" :close="hp.close"></slot>
                                </template>
                                <template #body="bp">
                                    <slot name="mbody" :open="bp.open" :close="bp.close"></slot>
                                </template>
                                <template #footer="fp">
                                    <slot name="mfooter" :open="fp.open" :close="fp.close"></slot>
                                </template>
                            </anyopen>
                        </div>
                    </template>
                </popinput>
            </template>
        </el-popover>
    </div>
</template>

<script setup>
import {
    ref,
    computed,
    useAttrs,useSlots,
    watch,
    onMounted,
    onBeforeUnmount,onDeactivated,onUnmounted,
    nextTick,
} from 'vue'
import anyopen from '@/ui/panels/anyopen.vue'

import popinput from "./popinput.vue"

let searchputSeed = 0
const activeSearchputId = ref(null)

defineOptions({
    inheritAttrs: false,
})

const props = defineProps({
    /** 下拉区域最大高度（滚动） */
    dropdownMaxHeight: { type: String, default: '280px' },
    /** 下拉 popover 宽度；不设则贴合输入框宽度 */
    dropdownWidth: { type: [String, Number], default: null },
    placeholder: { type: String, default: null },
    disabled: { type: Boolean, default: false },
    clearable: { type: Boolean, default: true },
    hideTag: { type: Boolean, default: false },
    /** tag 最大宽度，透传 popinput / poptags */
    tagWidth: { type: [String, Number], default: null },
    /** true：tag 换行；null 时由上层 resolved，wrap 模式默认 true */
    tagWrap: { type: Boolean, default: null },
    /** 输入防抖 ms，触发 search 事件 */
    debounce: { type: Number, default: 300 },
    /** 下拉内 loading（如远程搜索中） */
    loading: { type: Boolean, default: false },
    teleported: { type: Boolean, default: true },
    /** 是否在聚焦时展开下拉（无关键字也可展示占位） */
    openOnFocus: { type: Boolean, default: true },
    /** blur 后延迟关闭下拉，便于在浮层内点击 */
    blurCloseDelay: { type: Number, default: 200 },
    multiple: { type: Boolean, default: false },
    /** 布局：inline 单行；wrap 首行 tag，次行搜索+更多 */
    mode: { type: String, default: 'inline' },
    /** UI 风格：line 线框（默认）；card 色块 */
    theme: { type: String, default: 'line' },
    /** —— 更多（anyopen）—— */
    useMorePanel: { type: Boolean, default: false },
    /** 更多按钮文案；null 时按 mode 取默认（inline: ... / wrap: 选择...） */
    moreButtonText: { type: String, default: null },
    /** 更多按钮类型 text|link|button；null 时按 mode 取默认（inline: text / wrap: link） */
    moreButtonType: { type: String, default: null },
    moreDisabled: { type: Boolean, default: false },
    mtype: { type: String, default: 'popover' },
    mwidth: { type: String, default: '480px' },
    mheight: { type: String, default: '' },
    mtitle: { type: String, default: '' },
    mplacement: { type: String, default: 'bottom-end' },
    mtrigger: { type: String, default: 'click' },
    moreCloseOnClickModal: { type: Boolean, default: true },
    /** 点击「更多」时是否先关闭关键字下拉 */
    closeDropdownOnMore: { type: Boolean, default: true },
    //触发下拉的类型,默认为输入触发，也可以是点击触发 click
    trigger: { type: String, default: 'input' },
})

const emit = defineEmits(['search', 'more', 'clear', 'remove', 'panel-opened'])

const resolvedMoreButtonText = computed(() => {
    if (props.moreButtonText != null && props.moreButtonText !== '') return props.moreButtonText
    return props.mode === 'wrap' ? '选择...' : '...'
})
const resolvedMoreButtonType = computed(() => {
    if (props.moreButtonType != null && props.moreButtonType !== '') return props.moreButtonType
    return props.mode === 'wrap' ? 'link' : 'text'
})
/** wrap 默认开启 tag 换行 */
const resolvedTagWrap = computed(() => {
    if (props.tagWrap != null) return !!props.tagWrap
    return props.mode === 'wrap'
})

/** 与输入框关键字双向绑定 */
const query = defineModel({ type: String, default: '' })
const list = defineModel('list', { type: Array, default: [] })

const showDropdown = defineModel('showDropdown', { type: Boolean, default: false });
const queing= computed(() => {
    if (activeSearchputId.value !== instanceId) {
        return false;
    }
    if(props.trigger == 'click' && showDropdown.value){
        return true;
    }
    return !!query.value;
})

const attrs = useAttrs();
const slots = useSlots();
const attrsPass = computed(() => {
    const { class: _c, style: _s, ...rest } = attrs
    return rest
})

const inputRef = ref(null)
const anyopenRef = ref(null)
const mpopoverRef = ref(null)
const instanceId = ++searchputSeed
const dropdownVisible = ref(false)
const autoPopWidth = ref(260)
let debounceTimer = null
let blurCloseTimer = null

const dropdownBodyStyle = computed(() => ({
    maxHeight: props.dropdownMaxHeight,
    overflow: 'auto',
}))

const resolvedDropdownWidth = computed(() => {
    if (props.dropdownWidth != null && props.dropdownWidth !== '') {
        const w = props.dropdownWidth
        return typeof w === 'number' ? w : w
    }
    return autoPopWidth.value
})

function measureInputWidth() {
    nextTick(() => {
        const inst = inputRef.value
        const el = inst?.$el
        if (el && el.offsetWidth) {
            autoPopWidth.value = el.offsetWidth
        }
    })
}

function scheduleSearch() {
    if (debounceTimer) {
        clearTimeout(debounceTimer)
    }
    const q = query.value
    debounceTimer = setTimeout(() => {
        debounceTimer = null
        emit('search', q)
    }, props.debounce)
}

function onInputChange() {
    clearTimeout(blurCloseTimer)
    activeSearchputId.value = instanceId
    scheduleSearch()
    dropdownVisible.value = true
}


function onInputFocus() {
    clearTimeout(blurCloseTimer)
    if (props.openOnFocus) {
        activeSearchputId.value = instanceId
        dropdownVisible.value = true
        scheduleSearch()
    }
    measureInputWidth()
}

function onInputBlur() {
    blurCloseTimer = setTimeout(() => {

        closeDropdown();
    }, props.blurCloseDelay)
}

function onClear() {
    activeSearchputId.value = instanceId
    emit('clear')
    scheduleSearch()
}

function onEnterSearch() {
    clearTimeout(blurCloseTimer)
    activeSearchputId.value = instanceId
    if (debounceTimer) {
        clearTimeout(debounceTimer)
        debounceTimer = null
    }
    dropdownVisible.value = true
    emit('search', query.value)
}

function onAppendMousedown() {
    // 避免点「更多」时输入框抢走焦点逻辑导致异常；stop 阻止事件冒泡到 input
}

function onMoreClick(dp) {
    query.value = '';
    // 先关关键字下拉，避免 teleported popover 盖住嵌套选人弹窗内的输入框
    if (props.closeDropdownOnMore) {
        closeDropdown();
    }
    emit('more', { open: dp.open, close: dp.close })
    //如果mbody插槽被使用，则触发弹窗
    if(props.useMorePanel ){
        dp.open();
        nextTick(() => emit('panel-opened'));
    }
}

function openDropdown() {
    activeSearchputId.value = instanceId
    dropdownVisible.value = true
    measureInputWidth()
}

function closeDropdown() {
    showDropdown.value = false;
    if (activeSearchputId.value === instanceId) {
        activeSearchputId.value = null;
    }
    query.value = '';
    mpopoverRef.value?.close?.()
    //dropdownVisible.value = false
}

function onInputClick() {
    if(props.trigger == 'click'){
        activeSearchputId.value = instanceId
        showDropdown.value = true;
        scheduleSearch();
    }
}

/** 程序化打开「更多」面板（与点击 append 区行为一致） */
function openMorePanel() {
    if (!props.useMorePanel) {
        return
    }
    if (props.closeDropdownOnMore) {
        closeDropdown()
    }
    nextTick(() => {
        anyopenRef.value?.open?.()
        emit('panel-opened');
    })
}

function focusInput() {
    inputRef.value?.focus?.()
}

/*********************点击外部关闭逻辑区域************************************* */
const Vroot = ref(null);
const dropContent = ref(null);
function handleOutsideClick(e) {
   // console.log("handleOutsideClick", e);
  if (Vroot.value!=void 0&& Vroot.value!=null&& Vroot.value.contains(e.target)) {
    return;
  }

  if(dropContent.value!=void 0&& dropContent.value!=null&& dropContent.value.contains(e.target)) {
    return;
  }
  closeDropdown();
    // 手动解绑
   // document.removeEventListener('click', handleOutsideClick);
  
}



/*********************点击外部关闭逻辑区域************************************* */


watch(
    () => props.disabled,
    (v) => {
        if (v) {

            closeDropdown();
        }
    },
)

onMounted(() => {
    measureInputWidth();
    /** 点击外部关闭逻辑区域 */
    document.addEventListener('click', handleOutsideClick);
})


onBeforeUnmount(() => {
    console.log("before unmount");
    query.value = '';
    if (debounceTimer) {
        clearTimeout(debounceTimer)
    }
    if (blurCloseTimer) {
        clearTimeout(blurCloseTimer)
    }
})
onDeactivated(()=>{
    console.log("deactivated");
    query.value = '';
})
onUnmounted(()=>{
    console.log("unmounted");
    query.value = '';
    document.removeEventListener('click', handleOutsideClick);
})

defineExpose({
    openDropdown,
    closeDropdown,
    openMorePanel,
    focus: focusInput,
    /** 输入框组件实例（ElInput） */
    inputRef,
})
</script>

<style scoped>
.searchput {
    width: 100%;
    min-width: 0;
    box-sizing: border-box;
}
/* popover 触发器默认 inline-block 会按内容收缩，导致 wrap 行无法撑满 */

.searchput-input {
    width: 100%;
}
.searchput-append-wrap {
    display: inline-flex;
    vertical-align: middle;
    flex-shrink: 0;
    border-left:1px solid #dcdfe6;    
}
.searchput-append-wrap--wrap {
    border-left: none;
}
.searchput--card .searchput-append-wrap {
    border-left: none;
}
.searchput-more-btn {
    margin: 0;
    border-radius: 0;
}
.searchput-dropdown {
    min-height: 40px;
}
.shput-pt :deep(.el-input-group__append) {
    padding-right: 0px;
    padding-left: 0px;
}
.searchput-more-btn{
    padding: 0 15px;
    cursor: pointer;
}
</style>
