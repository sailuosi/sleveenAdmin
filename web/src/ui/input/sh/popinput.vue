<template>
    <!-- 外层 div 各 mode 共用，供 el-popover 锚定；内部按 mode v-if 隔离布局 -->
    <div
        :style="rootStyle"
        :class="[
            'divput-p',
            mode === 'wrap' ? 'mode-wrap' : 'mode-inline',
            play,
            disabled ? 'disabled' : 'enabled',
            multiple ? 'multiple' : 'single',
            theme === 'card' ? 'theme-card' : 'theme-line',
        ]"
    >
        <!-- inline：tag + 搜索 + 更多 同一行 -->
        <template v-if="mode === 'inline'">
            <slot name="prefix"></slot>
            <poptags
                v-if="multiple && !hideTag"
                :list="list"
                :closable="!disabled"
                :addable="tagAddable"
                :clearable="clearable"
                v-bind="attrs"
                :tagStyle="tagStyle"
                :tagWidth="tagWidth"
                :tagWrap="effectiveTagWrap"
                @close="$emit('remove', $event)"
                class="p-tags"
            >
                <template #tail>
                    <input
                        class="popput ml"
                        v-if="!disabled"
                        autocomplete="new-password"
                        v-model="focusValue"
                        :placeholder="placeholder"
                        @keyup.enter="onenter"
                        @blur="ondivblur"
                        @input="onpopput"
                    />
                </template>
                <template #default="opt">
                    <slot name="tag" v-bind="opt"></slot>
                </template>
            </poptags>
            <template v-else>
                <input
                    class="popput"
                    v-if="!disabled"
                    autocomplete="new-password"
                    v-model="focusValue"
                    :placeholder="placeholder"
                    @keyup.enter="onenter"
                    @blur="ondivblur"
                    @input="onpopput"
                />
            </template>
            <slot name="append"></slot>
        </template>

        <!-- wrap：首行 tag，次行 搜索 + 更多 -->
        <template v-else-if="mode === 'wrap'">
            <div v-if="slots.prefix" class="put-tag-row">
                <slot name="prefix"></slot>
            </div>
            <div class="put-search-row">
                <poptags
                    v-if="multiple && !hideTag"
                    :list="list"
                    :closable="!disabled"
                    :addable="tagAddable"
                    :clearable="clearable"
                    v-bind="attrs"
                    :tagStyle="tagStyle"
                    :tagWidth="tagWidth"
                    :tagWrap="effectiveTagWrap"
                    @close="$emit('remove', $event)"
                    class="p-tags"
                >
                    <template #tail>
                        <input
                            class="popput ml"
                            :disabled="disabled"
                            autocomplete="new-password"
                            v-model="focusValue"
                            :placeholder="placeholder"
                            @keyup.enter="onenter"
                            @blur="ondivblur"
                            @input="onpopput"
                        />
                    </template>
                    <template #default="opt">
                        <slot name="tag" v-bind="opt"></slot>
                    </template>
                </poptags>
                <template v-else>
                    <input
                        class="popput ml"
                        v-if="!disabled"
                        autocomplete="new-password"
                        v-model="focusValue"
                        :placeholder="placeholder"
                        @keyup.enter="onenter"
                        @blur="ondivblur"
                        @input="onpopput"
                    />
                </template>
                <slot name="append"></slot>
            </div>
        </template>
    </div>
</template>
<script setup>
/** 设计说明
组件负责搜索输入功能，主要包含单选模式下的前置展示、后置搜索、中间的文本输入框。
外层 divput-p 各 mode 共用；内部按 mode 用 v-if 隔离布局，便于后续按模式细化。
 */

import { computed, toRef, useAttrs, useSlots } from 'vue'

import poptags from "./poptags.vue"

defineOptions({
    name: 'popinput',
})

const props = defineProps({
    lazy: { type: Boolean, default: true },
    width: null,
    height: null,
    disabled: { type: Boolean, default: false },
    html: { type: Boolean, default: false },
    clearable: { type: Boolean, default: false },
    placeholder: { type: String, default: '搜索...' },
    play:{type:String,default:'flex'},
    multiple: { type: Boolean, default: false },
    tagAddable: { type: Boolean, default: false },
    hideTag: { type: Boolean, default: false },
    tagStyle:null,
    /** tag 最大宽度，透传 poptags；如 160 / '160px' */
    tagWidth: { type: [String, Number], default: null },
    /** true：多选 tag 换行；null 时由上层按 mode 决议 */
    tagWrap: { type: Boolean, default: null },
    /** 布局：inline 单行；wrap 首行 tag，次行搜索+更多 */
    mode: { type: String, default: 'inline' },
    /** UI 风格：line 线框（默认）；card 色块 */
    theme: { type: String, default: 'line' },
})

const attrs = useAttrs();
const slots = useSlots();

const height = toRef(props, 'height')

const emit = defineEmits(['update:modelValue', 'input', 'change', 'remove'])

const focusValue= defineModel();
const list = defineModel('list', { type: Array, default: [] })

/** wrap 默认开启 tag 换行 */
const effectiveTagWrap = computed(() => {
    if (props.tagWrap != null) return !!props.tagWrap
    return props.mode === 'wrap'
})

const mycss = computed(() => {
    return {
        width: props.width ? props.width : '100%',
        boxSizing: 'border-box',
    }
})

/** 共用外层样式；tagWrap 时附加换行对齐 */
const rootStyle = computed(() => {
    const style = { ...mycss.value }
    if (effectiveTagWrap.value) {
        style.alignItems = 'flex-start'
        if (props.mode === 'inline') {
            style.flexWrap = 'wrap'
        }
    }
    return style
})

function onpopput(e) {
    console.log(e);
    focusValue.value = e.target.value
    emit('input', focusValue.value)
}

function ondivblur(e) {
    //懒加载模式下，当用户输入完毕时，才返回变化数据。
    if (props.lazy) {

    }
    //不管是不是懒加载，如果处于编辑结束的失焦状态，扔出变化事件。

}

function onenter() {}
</script>

<style scoped>
.divput-p{
    border-radius:3px;
    border-color:#E4E7ED;
    background-color: #FFFFFF;
    border:1px solid rgb(168,171,178);   
    overflow: hidden; 
    padding-left:5px;
    box-sizing: border-box;
    width: 100%;
    max-width: 100%;
    min-width: 0;
}
/* card：无边框色块，变量可由外层覆盖 */
.divput-p.theme-card {
    border: none;
    background-color: var(--searchput-card-bg, #e6f4fc);
    color: var(--searchput-card-color, #303133);
    border-radius: var(--searchput-card-radius, 8px);
}
.divput-p.theme-card.disabled {
    background-color: var(--searchput-card-disabled-bg, #f0f4f8);
}

.divput-p.flex{
    display: flex;
}
.divput-p.single{
    align-items:center;
}
.popput.ml{
    border: none;
    border-bottom: 1px solid #E4E7ED;
    border-radius: 0;
    outline: none;
    background: transparent;
    box-sizing: border-box;
}
.divput-p.mode-wrap .put-search-row > .popput.ml:focus{
    border-bottom-color: #409eff;
}
.divput-p.multiple{
    align-items:stretch;
}
/* inline：原单行 flex 子项 */
.divput-p.mode-inline.flex > .popput{
    flex:1;
    width: 0;
    min-width: 60px;
}
.divput-p.mode-inline.flex > .p-tags{
    flex:1;
}
/* wrap：列布局，两行均撑满 */
.divput-p.mode-wrap{
    flex-direction: column;
    align-items: stretch !important;
    padding: 4px 8px 6px;
    overflow: visible;
}
.divput-p.mode-wrap .put-tag-row{
    display: flex;
    flex-wrap: wrap;
    align-items: flex-start;
    width: 100%;
    max-width: 100%;
    min-width: 0;
    box-sizing: border-box;
    margin-bottom: 4px;
}
.divput-p.mode-wrap .put-tag-row :deep(.el-tag){
    max-width: 100%;
    height: auto;
    white-space: normal;
    word-break: break-word;
    line-height: 1.4;
    padding-top: 4px;
    padding-bottom: 4px;
}
.divput-p.mode-wrap .put-tag-row :deep(.el-tag .el-tag__content){
    white-space: normal;
    word-break: break-word;
    overflow: visible;
    text-overflow: unset;
}
.divput-p.mode-wrap .put-search-row{
    display: flex;
    flex-direction: row;
    align-items: center;
    width: 100%;
    max-width: 100%;
    min-width: 0;
    box-sizing: border-box;
}
.divput-p.mode-wrap .put-search-row > .popput{
    flex:1 1 auto;
    width: auto;
    min-width: 0;
}
.divput-p.mode-wrap .put-search-row > .p-tags{
    flex:1 1 auto;
    min-width: 0;
}
.popput:focus-visible {
    outline: none;
}
div.popput{
    height:v-bind(height);

    padding:5px 15px;
    line-height:1.5;

    text-align: left;
    white-space: pre-wrap;
    min-height:32px;
}
div.popput:focus{

  
}
div.popput.disabled{
    cursor: not-allowed;
    border-color:#E4E7ED;
    background-color: #F5F7FA;
}
</style>
