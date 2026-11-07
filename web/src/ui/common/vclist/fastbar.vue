<template>
    <div v-if="fields.length" class="fastbar">
        <div
            v-for="item in fields"
            v-show="stateMap[item.field]"
            :key="item.field"
            class="fastbar__item"
        >
            <span
                v-if="item.showLabel !== false"
                class="fastbar__label"
                :title="fieldLabel(item)"
            >{{ fieldLabel(item) }}</span>
            <div class="fastbar__control" :style="fieldStyle(item)">
                <el-input
                    v-if="resolveFtype(item) === 'text' || resolveFtype(item) === 'number'"
                    v-model="stateMap[item.field].value"
                    class="fastbar__input shinput"
                    :type="resolveFtype(item) === 'number' ? 'number' : 'text'"
                    :placeholder="item.holder || item.caption"
                    clearable
                    size="small"
                    @change="onFieldChange(item)"
                    @keydown.enter="onFieldQuery(item)"
                    @clear="onFieldQuery(item)"
                    @blur="checkSayQuery(item)"
                >
                    <template #append>
                        <FastbarOp
                            v-model="stateMap[item.field].logic"
                            @change="onFieldChange(item)"
                        />
                    </template>
                </el-input>

                <div v-else-if="resolveFtype(item) === 'dict'" class="fastbar__group">
                    <el-select
                        v-model="stateMap[item.field].value"
                        class="fastbar__select shinput"
                        :placeholder="item.holder || item.caption"
                        multiple
                        collapse-tags
                        clearable
                        filterable
                        size="small"
                        @change="onFieldChange(item)"
                    >
                        <el-option
                            v-for="opt in getOptions(item)"
                            :key="opt.value"
                            :label="opt.label"
                            :value="opt.value"
                        />
                    </el-select>
                    <div class="fastbar__append">
                        <FastbarOp
                            v-model="stateMap[item.field].logic"
                            @change="onFieldChange(item)"
                        />
                    </div>
                </div>

                <div v-else-if="resolveFtype(item) === 'bool'" class="fastbar__group">
                    <el-select
                        v-model="stateMap[item.field].value"
                        class="fastbar__select shinput"
                        :placeholder="item.holder || item.caption"
                        multiple
                        collapse-tags
                        clearable
                        size="small"
                        @change="onFieldChange(item)"
                    >
                        <el-option label="是" :value="true" />
                        <el-option label="否" :value="false" />
                    </el-select>
                    <div class="fastbar__append">
                        <FastbarOp
                            v-model="stateMap[item.field].logic"
                            @change="onFieldChange(item)"
                        />
                    </div>
                </div>

                <div
                    v-else-if="resolveFtype(item) === 'date' || resolveFtype(item) === 'datetime'"
                    class="fastbar__group"
                >
                    <el-date-picker
                        v-model="stateMap[item.field].value"
                        class="fastbar__date shinput"
                        type="daterange"
                        range-separator="--"
                        start-placeholder="开始"
                        end-placeholder="结束"
                        size="small"
                        unlink-panels
                        format="YYYY-MM-DD"
                        value-format="YYYY-MM-DD"
                        @change="onFieldChange(item)"
                    />
                    <div class="fastbar__append">
                        <FastbarOp
                            v-model="stateMap[item.field].logic"
                            @change="onFieldChange(item)"
                        />
                    </div>
                </div>

                <el-input
                    v-else
                    v-model="stateMap[item.field].value"
                    class="fastbar__input shinput"
                    :placeholder="item.holder || item.caption"
                    clearable
                    size="small"
                    @change="onFieldChange(item)"
                    @keydown.enter="onFieldQuery(item)"
                    @clear="onFieldQuery(item)"
                    @blur="checkSayQuery(item)"
                >
                    <template #append>
                        <FastbarOp
                            v-model="stateMap[item.field].logic"
                            @change="onFieldChange(item)"
                        />
                    </template>
                </el-input>
            </div>
        </div>
    </div>
</template>

<script setup>
/**
:queryFastFields="[
    'BusinessState',
    { field: 'MR_Title', label: '需求标题', width: 220, op: 'like' },
    { field: 'IN_Name', showLabel: false, width: 200, holder: '送培机构' },
    { field: 'MR_Year', title: '年度', width: 90, op: '=' },
]"
 */


import {
    ref,
    reactive,
    watch,
    onMounted,
    onBeforeUnmount,
    inject,
} from 'vue'
import { loadCode } from '@/utils/codevalue.js'
import FastbarOp from './fastbar-op.vue'
import { isDateQueryType, normalizeListDateRangeValue } from './tool.js'

defineOptions({
    name: 'fastbar',
})

const props = defineProps({
    /** 由 querybar 根据字段名 + fieldMap 转换后的元数据数组 */
    fields: { type: Array, default: () => [] },
})

const vclist = inject('vclist', null)
const querybar = inject('querybar', null)

const stateMap = reactive({})
const changePending = reactive({})
const dictCache = ref({})

function fieldLabel(item) {
    return item?.label ?? item?.caption ?? item?.field ?? ''
}

function resolveFtype(item) {
    const ctype = item?.ctype
    if (!ctype) return 'text'
    const c = String(ctype).toLowerCase()
    if (c === 'dict' || c === 'select' || c === 'checkbox' || c === 'codetable') return 'dict'
    if (c === 'bool' || c === 'boolean') return 'bool'
    if (c === 'date') return 'date'
    if (c === 'datetime') return 'datetime'
    if (c === 'number' || c === 'money' || c === 'fullnum') return 'number'
    return 'text'
}

function defaultValueFor(item) {
    const ft = resolveFtype(item)
    if (ft === 'dict' || ft === 'bool') return []
    if (ft === 'date' || ft === 'datetime') return null
    return ''
}

function initialLogicFor(item) {
    const op = item?.defaultOp
    if (op != null && op !== '') return op
    return ''
}

function fieldStyle(item) {
    if (item.width == null || item.width === '') return undefined
    const w = typeof item.width === 'number' ? `${item.width}px` : item.width
    return { width: w }
}

function ensureFieldState(field, item) {
    if (!stateMap[field]) {
        stateMap[field] = {
            value: defaultValueFor(item),
            logic: initialLogicFor(item),
        }
        changePending[field] = false
    }
}

function syncAllFieldStates() {
    props.fields.forEach((item) => {
        if (!item?.field) return
        ensureFieldState(item.field, item)
        loadDictForItem(item)
        syncQuerybarField(item)
    })
}

function getOptions(item) {
    if (Array.isArray(item.dictlist) && item.dictlist.length) {
        return item.dictlist
    }
    if (item.dict && dictCache.value[item.dict]) {
        return dictCache.value[item.dict]
    }
    return []
}

function loadDictForItem(item) {
    const code = item.dict
    if (!code || dictCache.value[code]) return
    loadCode(code).then((res) => {
        dictCache.value = { ...dictCache.value, [code]: res || [] }
    })
}

function normalizeValue(value, ftype, op) {
    if (Array.isArray(value)) {
        if (!value.length) return null
        if (isDateQueryType(ftype) && (op === 'between' || !op)) {
            return normalizeListDateRangeValue(value)
        }
        return value
    }
    if (value === '') return null
    return value
}

function defaultOpFor(item) {
    if (item?.defaultOp != null && item.defaultOp !== '') {
        return item.defaultOp
    }
    const ft = resolveFtype(item)
    if (ft === 'date' || ft === 'datetime') return 'between'
    return 'like'
}

function buildQueryPara(item) {
    const st = stateMap[item.field]
    if (!st) return null
    let op = defaultOpFor(item)
    if (st.logic) {
        op = st.logic
    }
    return {
        field: item.field,
        value: normalizeValue(st.value, resolveFtype(item), op),
        op,
    }
}

function applyQueryPara(item) {
    const para = buildQueryPara(item)
    if (!para || !vclist?.setQuery) return
    vclist.setQuery(item.field, para)
}

function onFieldChange(item) {
    applyQueryPara(item)
    const ft = resolveFtype(item)
    if (ft === 'dict' || ft === 'bool' || ft === 'date' || ft === 'datetime') {
        onFieldQuery(item)
    } else {
        changePending[item.field] = true
    }
}

function onFieldQuery(item) {
    setTimeout(() => {
        changePending[item.field] = false
        if (vclist?.onRunQuery) {
            vclist.onRunQuery()
        }
    }, 100)
}

function checkSayQuery(item) {
    if (changePending[item.field]) {
        onFieldQuery(item)
    }
}

function clearField(item) {
    const st = stateMap[item.field]
    if (!st) return
    st.value = defaultValueFor(item)
    st.logic = initialLogicFor(item)
    changePending[item.field] = false
    applyQueryPara(item)
}

function resetFieldUi(item) {
    const st = stateMap[item.field]
    if (!st) return
    st.value = defaultValueFor(item)
    st.logic = initialLogicFor(item)
    changePending[item.field] = false
}

function syncQuerybarField(item) {
    if (!querybar || !item?.field) return
    querybar.registerField({
        field: item.field,
        caption: item.caption,
        dictlist: getOptions(item),
        clear: () => clearField(item),
        resetUi: () => resetFieldUi(item),
    })
}

watch(
    () => props.fields,
    () => syncAllFieldStates(),
    { deep: true, immediate: true },
)

watch(
    dictCache,
    () => {
        props.fields.forEach((item) => syncQuerybarField(item))
    },
    { deep: true },
)

onMounted(() => {
    syncAllFieldStates()
})

onBeforeUnmount(() => {
    if (!querybar) return
    props.fields.forEach((item) => {
        if (item?.field) {
            querybar.unregisterField(item.field)
        }
    })
})
</script>

<style scoped>
.fastbar {
    display: inline-flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 8px 12px;
}
.fastbar__item {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    min-width: 0;
}
.fastbar__item:has(.fastbar__control:only-child) {
    gap: 0;
}
.fastbar__label {
    flex-shrink: 0;
    color: #606266;
    font-size: 12px;
    max-width: 6em;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}
.fastbar__control {
    width: 160px;
    min-width: 120px;
    flex-shrink: 0;
}
.fastbar__input {
    width: 100%;
}
.fastbar__group {
    display: inline-flex;
    align-items: stretch;
    width: 100%;
    border: 1px solid #dcdfe6;
    border-radius: 4px;
    overflow: hidden;
    background: #fff;
}
.fastbar__select {
    flex: 1;
    min-width: 120px;
}
.fastbar__select:deep(.el-select__wrapper) {
    box-shadow: none;
    border: none;
}
.fastbar__date {
    flex: 1;
    min-width: 0;
}
.fastbar__date:deep(.el-input__wrapper) {
    box-shadow: none;
    border: none;
}
.fastbar__append {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    padding: 0 8px;
    background: #f5f7fa;
    border-left: 1px solid #dcdfe6;
}
.fastbar__input:deep(.el-input-group__append) {
    padding: 0 8px;
    background: #f5f7fa;
}
.shinput {
    height: 26px;
}
</style>
