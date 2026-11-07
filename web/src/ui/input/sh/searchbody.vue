<template>
    <div>
        <div class="sh-bar">
            <div>
                <span>{{ title }}</span>
                <el-input v-model="txt" placeholder="输入搜索关键字" @update:modelValue="load" v-if="showTopPut" />
            </div>
            
            <slot></slot>
        </div>
        <div v-if="bodyType=='ul'" class="selectoptions" :style="bdstyle" v-loading="searching">
            <div v-for="(man,m) in options" @click="doselect(man)" :class="['searchli',checkedIds.indexOf(man[rowKey])==-1?'uncheck':'checked']">
                <slot name="option" :row="man" :index="m"></slot>
            </div>
            <el-empty description="无数据" v-if="!options || options.length==0" >
                <el-link @click="load">刷新</el-link>
            </el-empty>
        </div>
        <table border="1" v-if="bodyType=='table'" class="selectoptions" :style="bdstyle" v-loading="searching">
            <tr>
                <slot name="th"></slot>
            </tr>
            <tr v-for="(man,m) in options" @click="doselect(man)" :class="['searchli',checkedIds.indexOf(man[rowKey])==-1?'uncheck':'checked']">
                <slot name="tr" :row="man" :index="m"></slot>
            </tr>
        </table>
        <el-pagination v-if="page"  background layout="prev, pager, next" :total="total" @current-change="handlePageChange" />
    </div>
    </template>
    
<script setup>
/** v-loading="loading"
 * 任意选组件，UI绘制方式为直接绘制，无弹窗。
 * 任意人员选择器
 */
import { ref, computed, watch, onMounted } from 'vue'
import { styleStrToObj } from '@/utils/style.js'

defineOptions({
    name: 'inputsearch',
})

const props = defineProps({
    title: { type: String, default: '' },
    multiple: { type: Boolean, default: false },
    onload: Function,
    rowKey: { type: String, default: '' },
    initLoad: { type: Boolean, default: false },
    height: null,
    orgoid: null,
    page: { type: Boolean, default: false },
    total: null,
    bodyStyle: { type: String, default: '' },
    /** 下拉框类型，ul,table */
    bodyType: { type: String, default: 'ul' },
    showTopPut: { type: Boolean, default: false },
})

const emit = defineEmits(['change', 'input', 'update:modelValue'])

const searching= defineModel('searching',{type: Boolean, default: false});//是否正在搜索
const txt = defineModel('txt', { type: String, default: '' })
const options = defineModel('list', { type: Array, default: () => [] })
const checked = ref([])
const checkedIds = ref([])
const selectId = ref(null)
const selectRow = ref(null)
const map = ref({})
const currentPage = ref(1)
const loading = ref(true)

const bdstyle = computed(() => {
    const t = styleStrToObj(props.bodyStyle)
    if (props.height) {
        t.height = props.height
    }
    return t
})

watch(
    () => props.orgoid,
    () => {
        load()
    }
)

function onchange() {
    if (props.multiple) {
        emit('input', checked.value, checkedIds.value)
        emit('update:modelValue', checked.value, checkedIds.value)
        emit('change', checked.value, checkedIds.value)
    } else {
        emit('input', selectRow.value, selectId.value)
        emit('update:modelValue', selectRow.value, selectId.value)
        emit('change', selectId.value, selectRow.value, selectId.value)
    }
}

function doselect(row) {
    const oid = row[props.rowKey]
    if (props.multiple) {
        if (checkedIds.value.indexOf(oid) === -1) {
            checkedIds.value.push(oid)
            checked.value.push(row)
        } else {
            const index = checkedIds.value.indexOf(oid)
            checkedIds.value.splice(index, 1)
            checked.value.splice(index, 1)
        }
    } else {
        selectId.value = oid
        selectRow.value = row
        checkedIds.value = [oid]
    }
    onchange()
}

function handlePageChange(newPage) {
    currentPage.value = newPage
    changeSelectRestFul(txt.value)
}

function load() {
    changeSelectRestFul(txt.value)
}

function changeSelectRestFul(inputKey) {
    if (typeof props.onload !== 'function') {
        loading.value = false
        return
    }
    loading.value = true
    props.onload(inputKey, currentPage.value).then((data) => {
            loading.value = false
            options.value = data
            data.forEach((man) => {
                map.value[man[props.rowKey]] = man
            })
        })
        .catch(() => {
            loading.value = false
        })
}

onMounted(() => {
    if (props.initLoad) {
        load()
    } else {
        loading.value = false
    }
})
</script>
    <style scoped lang="scss">
    .searchli{
        padding: 5px;
        border-bottom: 1px solid #eee;
        cursor: pointer;
        border-radius:5px;
    }
    .searchli:hover{
        background-color:#efefef;
    }
    .searchli.checked{
        background-color:#C2DEF1;
    }
    .sh-bar{
      display:flex;
      position: sticky;
      top: 0px;
    }
    :deep(.el-input__wrapper) {
      margin-right: 5px !important;
    }
    .selectoptions{
        height:var(--inputsearchsl-height);
        overflow:auto;
    }
    
    </style>
    