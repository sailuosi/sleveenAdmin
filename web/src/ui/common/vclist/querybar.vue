<template>
    <div class="query-bar" v-if="visible">
        <div class="query-bar__main">
            <div class="query-bar__left">
                <span class="query-bar__label">{{ label }}</span>
                <div class="query-bar__tags">
                    <slot name="tags" :tags="displayTags" :remove="removeTag">
                        <template v-if="displayTags.length">
                            <span
                                v-for="tag in displayTags"
                                :key="tag.field"
                                class="query-bar__tag"
                            >
                                <span class="query-bar__tag-text">{{ tag.label }}: {{ tag.display }}</span>
                                <span
                                    v-if="tag.closable !== false"
                                    class="query-bar__tag-close"
                                    title="移除"
                                    @click.stop="removeTag(tag)"
                                >×</span>
                            </span>
                        </template>
                        <span v-else-if="showEmptyHint" class="query-bar__empty">{{ emptyText }}</span>
                    </slot>
                    <fastbar v-if="fastFieldItems.length" :fields="fastFieldItems" />
                    <slot name="bar"></slot>
                </div>
            </div>
            <div class="query-bar__right">
                <slot name="search" :search="searchText" :onSearch="onSearch">
                    <div v-if="showSearch" class="query-bar__search">
                        <el-input
                            v-model="searchText"
                            class="query-bar__search-input"
                            :placeholder="searchPlaceholder"
                            clearable
                            @keydown.enter="onSearch"
                            @clear="onSearch"
                        >
                            <template #prefix>
                                <span class="query-bar__search-icon" aria-hidden="true">
                                    <svg viewBox="0 0 1024 1024" xmlns="http://www.w3.org/2000/svg"><path fill="currentColor" d="M909.6 854.5 649.9 594.8C690.2 542.7 712 479 712 412c0-80.2-31.3-155.4-87.9-212.1-56.6-56.7-132-87.9-212.1-87.9s-155.5 31.3-212.1 87.9C143.2 256.6 112 331.8 112 412s31.3 155.5 87.9 212.1C256.5 680.7 331.8 712 412 712c67 0 130.6-21.8 182.7-62l259.7 259.6a8.2 8.2 0 0 0 11.6 0l43.6-43.5a8.2 8.2 0 0 0 0-11.6zM570.4 570.4C528 612.7 471.8 636 412 636s-116-23.3-158.4-65.6C211.3 528 188 471.8 188 412s23.3-116.1 65.6-158.4C296 211.3 352.2 188 412 188s116.1 23.3 158.5 65.6C612.7 296 636 352.2 636 412s-23.3 116.1-65.6 158.4z"/></svg>
                                </span>
                            </template>
                        </el-input>
                    </div>
                </slot>
                <el-popover
                    v-model:visible="popoverOpen"
                    :trigger="popoverTrigger"
                    :placement="placement"
                    :width="popoverWidth"
                    :show-after="showAfter"
                    :hide-after="hideAfter"
                    :teleported="teleported"
                    popper-class="query-bar-popper"
                >
                    <div class="query-bar__panel">
                        <div class="query-bar__panel-body">
                            <el-form class="query-bar__form" label-position="right" size="small" @submit.prevent>
                                <slot :register="onRegister"></slot>
                            </el-form>
                        </div>
                        <div v-if="showPanelActions" class="query-bar__panel-foot">
                            <el-button size="small" @click="onReset">{{ resetText }}</el-button>
                            <el-button type="primary" size="small" @click="onApply">{{ applyText }}</el-button>
                        </div>
                    </div>
                    <template #reference>
                        <slot name="expand" :open="popoverOpen" :toggle="togglePopover">
                            <span
                                class="query-bar__expand"
                                :class="{ 'is-open': popoverOpen }"
                                @click="onExpandClick"
                            >
                                {{ popoverOpen ? collapseText : expandText }}
                                <span class="query-bar__expand-arrow" aria-hidden="true">▾</span>
                            </span>
                        </slot>
                    </template>
                </el-popover>
            </div>
        </div>
    </div>
</template>

<script setup>
import {
    ref,
    computed,
    watch,
    provide,
    inject,
} from 'vue'
import fastbar from './fastbar.vue'

defineOptions({
    name: 'querybar',
})

const props = defineProps({
    /** 是否显示查询栏 */
    visible: { type: Boolean, default: true },
    /** 左侧标题 */
    label: { type: String, default: '筛选条件' },
    /** 无已选条件时的占位 */
    emptyText: { type: String, default: '暂无筛选' },
    /** 展开按钮文案 */
    expandText: { type: String, default: '展开过滤' },
    collapseText: { type: String, default: '收起过滤' },
    resetText: { type: String, default: '重置' },
    applyText: { type: String, default: '查询' },
    /**
     * 浮层触发方式：click | hover
     * hover 时仍可通过点击「展开过滤」锁定打开（click 模式）
     */
    trigger: { type: String, default: 'click' },
    placement: { type: String, default: 'bottom-end' },
    popoverWidth: { type: [String, Number], default: 560 },
    showAfter: { type: Number, default: 120 },
    hideAfter: { type: Number, default: 200 },
    teleported: { type: Boolean, default: true },
    /** 浮层底部是否显示查询/重置 */
    showPanelActions: { type: Boolean, default: true },
    /** 右侧快捷搜索框 */
    showSearch: { type: Boolean, default: false },
    searchPlaceholder: { type: String, default: '搜索...' },
    /** 外部传入的已选标签（优先于 vclist.queryData 自动汇总） */
    filters: { type: Array, default: null },
    /**
     * 快捷筛选字段：字符串数组，或对象数组
     * { field, width?, op?|defaultOp?, showLabel?, label?|title?|caption?, holder? }
     * 与 fieldMap 合并后交给 fastbar 绘制
     */
    fields: { type: Array, default: () => [] },
    //字段映射
    fieldMap: { type: Object, default: null },
})

const emit = defineEmits(['search', 'apply', 'reset', 'remove', 'expand-change'])

const vclist = inject('vclist', null)
const searchText = defineModel('txt', { default: '' })
const popoverOpen = ref(false)
const fieldRegistry = ref({})

const popoverTrigger = computed(() => {
    return props.trigger === 'hover' ? 'hover' : 'click'
})

function onRegister(meta) {
    registerField(meta)
}

function registerField(meta) {
    if (!meta?.field) return
    fieldRegistry.value[meta.field] = {
        ...fieldRegistry.value[meta.field],
        ...meta,
    }
}

function unregisterField(field) {
    if (!field) return
    const next = { ...fieldRegistry.value }
    delete next[field]
    fieldRegistry.value = next
}

function formatValue(value, meta) {
    if (value == null || value === '') return ''
    if (Array.isArray(value)) {
        if (!value.length) return ''
        const opts = meta?.dictlist
        if (Array.isArray(opts) && opts.length) {
            return value
                .map((v) => {
                    const hit = opts.find((o) => o.value === v)
                    return hit ? hit.label : String(v)
                })
                .join('、')
        }
        return value.join('、')
    }
    const opts = meta?.dictlist
    if (Array.isArray(opts) && opts.length) {
        const hit = opts.find((o) => o.value === value)
        if (hit) return hit.label
    }
    if (typeof value === 'boolean') {
        return value ? '是' : '否'
    }
    return String(value)
}

function buildTagFromQuery(field, info) {
    const meta = fieldRegistry.value[field] || {}
    const val = info?.value
    if (val == null || val === '' || (Array.isArray(val) && !val.length)) {
        return null
    }
    const display = formatValue(val, meta)
    if (!display) return null
    return {
        field,
        label: meta.caption || field,
        display,
        raw: val,
        closable: meta.closable !== false,
    }
}

const autoTags = computed(() => {
    const qd = vclist?.queryData
    const data = qd && typeof qd === 'object' && 'value' in qd ? qd.value : qd
    if (!data || typeof data !== 'object') return []
    const tags = []
    Object.keys(data).forEach((field) => {
        const tag = buildTagFromQuery(field, data[field])
        if (tag) tags.push(tag)
    })
    return tags
})

const activeTags = computed(() => {
    if (Array.isArray(props.filters)) {
        return props.filters
    }
    return autoTags.value
})

const resolvedFieldMap = computed(() => {
    const map = props.fieldMap
    if (!map || typeof map !== 'object') return {}
    if ('value' in map && map.value != null && typeof map.value === 'object') {
        return map.value
    }
    return map
})

function parseFastFieldEntry(entry) {
    if (typeof entry === 'string') {
        return { field: entry }
    }
    if (entry && typeof entry === 'object' && entry.field) {
        return entry
    }
    return null
}

const fastFieldItems = computed(() => {
    const names = props.fields
    if (!Array.isArray(names) || !names.length) return []
    const map = resolvedFieldMap.value
    return names
        .map((entry) => {
            const cfg = parseFastFieldEntry(entry)
            if (!cfg?.field) return null
            const field = cfg.field
            const meta = map[field] || {}
            const defaultOp = cfg.op ?? cfg.defaultOp ?? null
            const label = cfg.label ?? cfg.title ?? cfg.caption ?? meta.caption ?? field
            const showLabel =
                cfg.showLabel !== false && cfg.showCaption !== false
            return {
                field,
                label,
                caption: label,
                showLabel,
                ctype: cfg.ctype || meta.ctype || 'text',
                dict: cfg.dict || meta.dict || '',
                dictlist: cfg.dictlist || meta.dictlist || null,
                holder: cfg.holder || meta.holder || label,
                queryable: meta.queryable,
                width: cfg.width,
                defaultOp,
            }
        })
        .filter(Boolean)
})

const fastFieldSet = computed(() => {
    return new Set(
        fastFieldItems.value.map((item) => item.field).filter(Boolean),
    )
})

/** 标签区展示：排除已在 fastbar 中绘制的字段 */
const displayTags = computed(() => {
    const fast = fastFieldSet.value
    if (!fast.size) return activeTags.value
    return activeTags.value.filter((tag) => !fast.has(tag.field))
})

/** 无标签且无快捷字段时显示「暂无筛选」 */
const showEmptyHint = computed(() => {
    return displayTags.value.length === 0 && fastFieldItems.value.length === 0
})

function removeTag(tag) {
    const field = tag?.field
    if (!field) return
    const meta = fieldRegistry.value[field]
    if (typeof meta?.clear === 'function') {
        meta.clear()
    } else if (vclist?.setQuery) {
        vclist.setQuery(field, { field, value: null, op: tag.op || 'like' })
    }
    emit('remove', tag)
    if (vclist?.onRunQuery) {
        vclist.onRunQuery()
    }
}

function onSearch() {
    console.log(searchText.value);
    emit('search', searchText.value)
}

function onApply() {
    popoverOpen.value = false
    emit('apply')
    if (vclist?.onRunQuery) {
        vclist.onRunQuery()
    }
}

function onReset() {
    emit('reset')
    // 先清空快捷搜索框
    searchText.value = ''
    const fields = Object.keys(fieldRegistry.value)
    // 只清 UI，避免每个 dict 字段各自触发一次查询
    fields.forEach((field) => {
        const meta = fieldRegistry.value[field]
        if (typeof meta?.resetUi === 'function') {
            meta.resetUi()
        } else if (typeof meta?.clear === 'function') {
            meta.clear()
        }
    })
    if (vclist?.clearQuery) {
        // 传入字段名：从 queryConditions 中移除这些列条件并单次 refresh
        vclist.clearQuery(fields.length ? fields : null)
    } else {
        Object.keys(fieldRegistry.value).forEach((field) => {
            const meta = fieldRegistry.value[field]
            if (typeof meta?.clear === 'function') {
                meta.clear()
            }
        })
    }
}

function togglePopover() {
    popoverOpen.value = !popoverOpen.value
}

function onExpandClick(e) {
    if (props.trigger === 'hover') {
        e.preventDefault()
        popoverOpen.value = !popoverOpen.value
    }
}

watch(popoverOpen, (val) => {
    emit('expand-change', val)
})

provide('querybar', {
    registerField,
    unregisterField,
})

defineExpose({
    registerField,
    unregisterField,
    activeTags,
    displayTags,
    fastFieldItems,
    removeTag,
    onApply,
    onReset,
    togglePopover,
    popoverOpen,
    searchText,
})
</script>

<style scoped>
.query-bar {
    padding: 8px 12px;
    background: #fff;
    border-bottom: 1px solid #ebeef5;
}
.query-bar__main {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 16px;
    min-height: 32px;
}
.query-bar__left {
    display: flex;
    align-items: center;
    flex: 1;
    min-width: 0;
    gap: 8px;
}
.query-bar__label {
    flex-shrink: 0;
    color: #606266;
    font-size: 13px;
}
.query-bar__tags {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 8px;
    min-width: 0;
}
.query-bar__tag {
    display: inline-flex;
    align-items: center;
    max-width: 100%;
    padding: 2px 8px;
    font-size: 12px;
    color: #303133;
    background: #f5f7fa;
    border: 1px solid #dcdfe6;
    border-radius: 2px;
    line-height: 20px;
}
.query-bar__tag-text {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}
.query-bar__tag-close {
    margin-left: 6px;
    color: #909399;
    cursor: pointer;
    font-size: 14px;
    line-height: 1;
    user-select: none;
}
.query-bar__tag-close:hover {
    color: #303133;
}
.query-bar__empty {
    color: #c0c4cc;
    font-size: 12px;
}
.query-bar__right {
    display: flex;
    align-items: center;
    flex-shrink: 0;
    gap: 12px;
}
.query-bar__search {
    width: 200px;
}
.query-bar__search-input:deep(.el-input__wrapper) {
    box-shadow: none;
    border-radius: 0;
    border-bottom: 1px solid #dcdfe6;
    padding-left: 0;
    padding-right: 0;
}
.query-bar__search-icon {
    display: inline-flex;
    width: 14px;
    height: 14px;
    color: #909399;
}
.query-bar__search-icon svg {
    width: 100%;
    height: 100%;
}
.query-bar__expand {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    color: #409eff;
    font-size: 13px;
    cursor: pointer;
    user-select: none;
    white-space: nowrap;
}
.query-bar__expand:hover {
    opacity: 0.85;
}
.query-bar__expand.is-open .query-bar__expand-arrow {
    transform: rotate(180deg);
}
.query-bar__expand-arrow {
    display: inline-block;
    font-size: 12px;
    transition: transform 0.2s;
}
.query-bar__panel-body {
    max-height: min(60vh, 480px);
    overflow: auto;
    padding: 4px 2px 8px;
}
.query-bar__form {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
    gap: 4px 16px;
}
.query-bar__panel-body:deep(.el-form-item) {
    margin-bottom: 12px;
}
.query-bar__panel-foot {
    display: flex;
    justify-content: flex-end;
    gap: 8px;
    padding-top: 8px;
    border-top: 1px solid #ebeef5;
}
</style>

<style>
.query-bar-popper {
    padding: 12px 16px !important;
}
</style>
