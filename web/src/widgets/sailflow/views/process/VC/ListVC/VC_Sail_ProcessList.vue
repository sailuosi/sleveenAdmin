<template>
    <flexpan :flex="flex" :height="height">
        <template #header>
            <teleport :to="position" :disabled="!position" v-if="!hideToolbar">
                <div class="vc-top-toolbar">
                    <el-button @click="onBtnAddClick" class="toolbar-action-btn toolbar-btn-add-yellow">
                        <SvgIcon name="fa fa-plus-circle" /> 新建流程
                    </el-button>
                    <el-button @click="importDialogRef?.open()" class="toolbar-action-btn">
                        <SvgIcon name="fa fa-upload" /> 导入
                    </el-button>
                    <el-button @click="templateDialogRef?.open()" class="toolbar-action-btn">
                        <SvgIcon name="fa fa-th-large" /> 从模板创建
                    </el-button>
                </div>
            </teleport>
        </template>
        <template #default="p">
            <vclist :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="p.height"
                :flex="flex" v-loading="loading" query :maxLine="1"
                v-model:queryConditions="queryRowCondtions" @update:focusRow="setFocusRow"
                :orderCol="true" :stripe="true" empty-text="暂无流程（可先调 Probe/init-db）">
                <vccol field="op" caption="操作" width="320px" type="text" disabled align="center" headerAlign="center">
                    <template #default="cellp">
                        <el-link type="primary" @click="goEdit(cellp.row)" style="margin-right: 10px;">修改</el-link>
                        <el-link @click="goCopy(cellp.row)" style="margin-right: 10px;">复制</el-link>
                        <el-link type="primary" @click="goDesigner(cellp.row)" style="margin-right: 10px;">设计</el-link>
                        <el-link type="primary" @click="goDesignerVue(cellp.row)" style="margin-right: 10px;">Vue设计</el-link>
                        <el-link
                            v-if="Number(field(cellp.row, 'Status', 'status')) !== 1"
                            type="success"
                            style="margin-right: 10px;"
                            @click="setUsing(cellp.row, 1)"
                        >启用</el-link>
                        <el-link
                            v-else
                            type="warning"
                            style="margin-right: 10px;"
                            @click="setUsing(cellp.row, 0)"
                        >停用</el-link>
                        <el-link style="margin-right: 10px;" @click="onUpgrade(cellp.row)">升级</el-link>
                        <el-link type="danger" @click="removeRow(cellp.row)">删除</el-link>
                    </template>
                </vccol>
                <vccol field="ProcessName" caption="名称" min-width="160px" type="text">
                    <template #default="cellp">{{ field(cellp.row, 'ProcessName', 'processName') }}</template>
                </vccol>
                <vccol field="ProcessCode" caption="编码" width="140px" type="text">
                    <template #default="cellp">{{ field(cellp.row, 'ProcessCode', 'processCode') }}</template>
                </vccol>
                <vccol field="Version" caption="版本" width="80px" type="text">
                    <template #default="cellp">{{ field(cellp.row, 'Version', 'version') }}</template>
                </vccol>
                <vccol field="Status" caption="状态" width="90px" type="text" align="center" headerAlign="center">
                    <template #default="cellp">
                        <el-tag :type="Number(field(cellp.row, 'Status', 'status')) === 1 ? 'success' : 'info'" size="small">
                            {{ Number(field(cellp.row, 'Status', 'status')) === 1 ? '启用' : '停用' }}
                        </el-tag>
                    </template>
                </vccol>
                <vccol field="ProcessId" caption="ProcessId" min-width="180px" type="text">
                    <template #default="cellp">{{ field(cellp.row, 'ProcessId', 'processId') }}</template>
                </vccol>
            </vclist>
            <ImportProcessDialog ref="importDialogRef" @imported="getlist" />
            <TemplateGalleryDialog ref="templateDialogRef" @created="getlist" />
            <SailProcessEditDialog ref="editDialogRef" @saved="getlist" />
            <SailDesignerVueDialog ref="designerVueDialogRef" @saved="getlist" />
        </template>
    </flexpan>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"
import useListEditor from "@/uiext/gene/hook/listeditor.js"
import { pickField } from '@sail/utils/sailResult'
import ImportProcessDialog from '@sail/components/ImportProcessDialog.vue'
import TemplateGalleryDialog from '@sail/components/TemplateGalleryDialog.vue'
import SailProcessEditDialog from '@sail/components/SailProcessEditDialog.vue'
import SailDesignerVueDialog from '@sail/components/SailDesignerVueDialog.vue'

const props = defineProps({
    page: null, api: null, bus: null, height: null,
    hideToolbar: { type: Boolean, default: false },
    autoLoad: { type: Boolean, default: true },
    position: null, flex: { type: Boolean, default: false },
    pageSize: { type: Number, default: 30 },
})

const router = useRouter()
const listHolder = useListEditor()
const oidField = "ProcessDefId"
const loading = ref(false)
const mlist = ref([])
const total = ref(0)
const queryParams = ref({ pageSize: props.pageSize, pageNum: 1 })
const queryRowCondtions = ref([])
const importDialogRef = ref(null)
const templateDialogRef = ref(null)
const editDialogRef = ref(null)
const designerVueDialogRef = ref(null)

const listpara = computed(() => ({
    pageSize: queryParams.value.pageSize,
    pageNum: queryParams.value.pageNum,
    page: queryParams.value.pageNum,
    keyword: queryParams.value.keyword,
}))

function field(row, ...keys) {
    return pickField(row, ...keys) ?? ''
}

const getlist = () => {
    loading.value = true
    props.api.querySailProcesslist(listpara.value).then((res) => {
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

function onBtnAddClick() {
    editDialogRef.value?.open({ type: 'create' })
}

function goEdit(row) {
    editDialogRef.value?.open({
        type: 'update',
        processId: String(field(row, 'ProcessId', 'processId')),
        version: String(field(row, 'Version', 'version') || '1')
    })
}

function goCopy(row) {
    router.push({
        path: '/sail/process-edit',
        query: {
            type: 'copy',
            processId: String(field(row, 'ProcessId', 'processId')),
            version: String(field(row, 'Version', 'version') || '1')
        }
    })
}

function goDesigner(row) {
    router.push({
        path: '/sail/designer',
        query: {
            processId: String(field(row, 'ProcessId', 'processId')),
            version: String(field(row, 'Version', 'version') || '1')
        }
    })
}

function goDesignerVue(row) {
    designerVueDialogRef.value?.open({
        processId: String(field(row, 'ProcessId', 'processId')),
        version: String(field(row, 'Version', 'version') || '1')
    })
}

async function setUsing(row, usingState) {
    try {
        const data = await props.api.updateSailProcessUsingState({
            ProcessId: String(field(row, 'ProcessId', 'processId')),
            Version: String(field(row, 'Version', 'version') || '1'),
            UsingState: usingState
        })
        if ((data?.Status ?? data?.status) === -1) {
            ElMessage.error(data?.Message || data?.message || '操作失败')
            return
        }
        ElMessage.success(usingState === 1 ? '已启用' : '已停用')
        getlist()
    } catch (e) {
        ElMessage.error(e instanceof Error ? e.message : String(e))
    }
}

async function onUpgrade(row) {
    const ver = String(field(row, 'Version', 'version') || '1')
    try {
        await ElMessageBox.confirm(`将从版本 ${ver} 升级到下一版本？`, '升级流程', { type: 'info' })
    } catch {
        return
    }
    try {
        const data = await props.api.upgradeSailProcess({
            ProcessId: String(field(row, 'ProcessId', 'processId')),
            Version: ver
        })
        if ((data?.Status ?? data?.status) === -1) {
            ElMessage.error(data?.Message || data?.message || '升级失败')
            return
        }
        ElMessage.success(data?.Message || data?.message || '升级成功')
        getlist()
    } catch (e) {
        ElMessage.error(e instanceof Error ? e.message : String(e))
    }
}

const removeRow = (row) => {
    const name = field(row, 'ProcessName', 'processName')
    alertIsOK(`确认删除流程「${name}」？此操作不可恢复。`, (e) => {
        if (e === "yes") {
            props.api.removeSailProcess(row).then((data) => {
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
    }, "删除流程")
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
