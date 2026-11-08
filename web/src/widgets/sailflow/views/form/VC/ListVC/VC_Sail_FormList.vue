<template>
    <flexpan :flex="flex" :height="height">
        <template #header>
            <teleport :to="position" :disabled="!position" v-if="!hideToolbar">
                <div class="vc-top-toolbar">
                    <el-input
                        v-model="keyword"
                        clearable
                        placeholder="名称 / 编码"
                        style="width: 200px"
                        @keyup.enter="reload"
                        @clear="reload"
                    />
                    <el-button @click="reload" class="toolbar-action-btn">刷新</el-button>
                    <el-button @click="onBtnAddClick" class="toolbar-action-btn toolbar-btn-add-yellow">
                        <SvgIcon name="fa fa-plus-circle" /> 新建表单
                    </el-button>
                </div>
            </teleport>
        </template>
        <template #default="p">
            <vclist :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="p.height"
                :flex="flex" v-loading="loading" query :maxLine="1"
                v-model:queryConditions="queryRowCondtions" @update:focusRow="setFocusRow"
                :orderCol="true" :stripe="true" empty-text="暂无表单（可先 Probe/init-db）">
                <vccol field="op" caption="操作" width="120px" type="text" disabled align="center" headerAlign="center">
                    <template #default="cellp">
                        <el-link type="primary" @click="openEditor(field(cellp.row, 'FormId', 'formId'))" style="margin-right: 12px;">
                            <SvgIcon name="fa fa-pencil-square-o" :size="18" />
                        </el-link>
                        <el-link type="danger" @click="removeRow(cellp.row)">
                            <SvgIcon name="fa fa-trash-o" :size="18" />
                        </el-link>
                    </template>
                </vccol>
                <vccol field="FormId" caption="Id" width="80px" type="text">
                    <template #default="cellp">{{ field(cellp.row, 'FormId', 'formId') }}</template>
                </vccol>
                <vccol field="FormName" caption="名称" min-width="160px" type="text">
                    <template #default="cellp">{{ field(cellp.row, 'FormName', 'formName') }}</template>
                </vccol>
                <vccol field="FormCode" caption="编码" min-width="120px" type="text">
                    <template #default="cellp">{{ field(cellp.row, 'FormCode', 'formCode') }}</template>
                </vccol>
                <vccol field="Version" caption="版本" width="80px" type="text">
                    <template #default="cellp">{{ field(cellp.row, 'Version', 'version') || '1' }}</template>
                </vccol>
            </vclist>
        </template>
    </flexpan>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"
import useListEditor from "@/uiext/gene/hook/listeditor.js"
import { pickField } from '@sail/utils/sailResult'

const props = defineProps({
    page: null, api: null, bus: null, height: null,
    hideToolbar: { type: Boolean, default: false },
    autoLoad: { type: Boolean, default: true },
    position: null, flex: { type: Boolean, default: false },
    pageSize: { type: Number, default: 30 },
})

const router = useRouter()
const listHolder = useListEditor()
const oidField = "FormId"
const loading = ref(false)
const mlist = ref([])
const total = ref(0)
const keyword = ref('')
const queryParams = ref({ pageSize: props.pageSize, pageNum: 1 })
const queryRowCondtions = ref([])

const listpara = computed(() => ({
    pageSize: queryParams.value.pageSize,
    pageNum: queryParams.value.pageNum,
    page: queryParams.value.pageNum,
    keyword: keyword.value,
}))

function field(row, ...keys) {
    return pickField(row, ...keys) ?? ''
}

const getlist = () => {
    loading.value = true
    props.api.querySailFormlist(listpara.value).then((res) => {
        mlist.value = res.Items || res.records || res.Records || []
        total.value = res.Total ?? res.total ?? 0
        listHolder.init(mlist.value, oidField)
        loading.value = false
    }).catch((err) => {
        mlist.value = []
        total.value = 0
        ElMessage.error(err?.message || String(err))
        loading.value = false
    })
}

function reload() {
    queryParams.value.pageNum = 1
    getlist()
}

function onBtnAddClick() {
    openEditor()
}

function openEditor(id) {
    router.push({
        path: '/sail/form-editor',
        query: id ? { formId: String(id) } : undefined
    })
}

const removeRow = (row) => {
    const name = field(row, 'FormName', 'formName')
    alertIsOK(`确认删除表单 ${name}？`, (e) => {
        if (e === "yes") {
            props.api.removeSailForm(row).then((data) => {
                if ((data?.Status ?? data?.status) === -1) {
                    ElMessage.error(data?.Message || data?.message || '删除失败')
                    return
                }
                getlist()
                ElMessage.success('已删除')
            }).catch((err) => {
                ElMessage.error(err?.message || String(err))
            })
        }
    }, "删除表单")
}

function setFocusRow() {}

onMounted(() => {
    if (props.autoLoad) getlist()
})
</script>

<style lang="css" scoped>
.vc-top-toolbar {
    display: flex;
    gap: 8px;
    align-items: center;
    flex-wrap: wrap;
}
.vc-top-toolbar .toolbar-action-btn {
    height: 32px; padding: 0 12px; border-radius: 3px; font-size: 14px; font-weight: 500;
}
.toolbar-btn-add-yellow {
    --el-button-text-color: #fff; --el-button-bg-color: #FEB75B; --el-button-border-color: #FEB75B;
}
</style>
