<template>
    <flexpan :flex="flex" :height="height">
        <template #header="p">
            <teleport :to="position" :disabled="!position" v-if="!hideToolbar">
                <div class="vc-top-toolbar">
                    <el-button @click="onBtnAddClick" class="toolbar-action-btn toolbar-btn-add-yellow">
                        <SvgIcon name="fa fa-plus-circle" /> 添加
                    </el-button>
                    <slot name="toolbar"></slot>
                </div>
            </teleport>
        </template>
        <template #default="p">
            <vclist :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="p.height"
                :flex="flex" ref="mlistvm" v-loading="loading" query :maxLine="1"
                v-model:queryConditions="queryRowCondtions" @update:focusRow="setFocusRow"
                :orderCol="true" :stripe="true" row-key="spId">
                <slot></slot>
                <vccol field="op" caption="操作" width="180px" type="text" disabled align="center" headerAlign="center">
                    <template #default="cellp">
                        <el-link class="op-edit-link" @click="designRow(cellp.row, false)" title="设计" style="margin-right: 12px; color: #338fd1;">设计</el-link>
                        <el-link class="op-edit-link" @click="openRow(cellp.row)" title="编辑" style="margin-right: 12px; color: #338fd1;">
                            <SvgIcon name="fa fa-pencil-square-o" :size="18" />
                        </el-link>
                        <el-link type="danger" @click="removeRow(cellp.row)" title="删除">
                            <SvgIcon name="fa fa-trash-o" :size="18" />
                        </el-link>
                    </template>
                </vccol>
                <vccol field="OrgName" caption="单位" width="150px" align="left"></vccol>
                <vccol field="P_IsOn" caption="启用" width="60px" type="Boolean"></vccol>
                <vccol field="P_Name" caption="名称" width="200px" align="left"></vccol>
                <vccol field="P_Key" caption="唯一编码" width="160px" align="left"></vccol>
                <vccol field="P_Task" caption="业务" type="dict" :dictlist="codeTables.SysPtTask"></vccol>
                <vccol field="P_Pulic" caption="共享" width="70px" type="dict" :dictlist="codeTables.SysShareStatus"></vccol>
                <vccol field="P_Theme" caption="主题" width="70px" type="dict" :dictlist="codeTables.SysPtCellTheme"></vccol>
                <vccol field="P_Display" caption="布局" width="80px" type="dict" :dictlist="codeTables.SysPtDisplay"></vccol>
                <vccol field="P_Note" caption="说明" align="left" min-width="200px"></vccol>
                <vccol field="SYS_Created" caption="创建日期" width="160px" type="datetime"></vccol>
                <vccol field="SYS_LAST_UPD" caption="最后修改日期" width="160px" type="datetime"></vccol>
            </vclist>

            <el-dialog title="编辑门户" class="normal" v-model="diaEditOpened" width="72%" top="4vh"
                append-to-body @close="closeRowEdit">
                <template #header>
                    <div class="tv-dialog-header">
                        <span class="tv-dialog-title">编辑门户</span>
                        <el-button class="toolbar-action-btn toolbar-btn-primary" @click="onDialogSave">
                            <SvgIcon name="fa fa-save" /> 保存
                        </el-button>
                    </div>
                </template>
                <livevc name="VC_Sys_PortalEdit" :row="editingRow" @close="closeRowEdit" @refresh="getlist"
                    :api="api" :bus="bus" :page="page" ref="editLiveVCRef"></livevc>
            </el-dialog>
        </template>
    </flexpan>
</template>

<script setup>
import { ref, computed, watch, onMounted, unref } from 'vue'
import { useRouter } from 'vue-router'
import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"
import livevc from "@/uiext/gene/live/livevc.vue"
import useListEditor from "@/uiext/gene/hook/listeditor.js"

const props = defineProps({
    page: null, api: null, bus: null, height: null,
    disabled: { type: Boolean, default: false },
    hideToolbar: { type: Boolean, default: false },
    autoLoad: { type: Boolean, default: true },
    position: null, flex: { type: Boolean, default: false },
    conditions: null, params: null, filters: null,
    pageSize: { type: Number, default: 30 },
    fk: null, childed: { type: Boolean, default: false }, beforeLoad: null,
})
const emit = defineEmits(['change', 'delete', 'update:focusRow'])
const router = useRouter()
const listHolder = useListEditor()
const VCName = "VC_Sys_PortalList"
const oidField = "spId"
const loading = ref(false)
const mlist = ref([])
const total = ref(0)
const queryParams = ref({ pageSize: props.pageSize, pageNum: 1 })
const focusRow = ref({})
const queryRowCondtions = ref([])
const diaEditOpened = ref(false)
const editingRow = ref({})
const editLiveVCRef = ref(null)
const codeTables = computed(() => props.page?.codeTable || {})
const busData = computed(() => props.bus?.dataMap?.[VCName] || { conditions: [] })
const listpara = computed(() => {
    const res = {
        conditions: [], pageSize: queryParams.value.pageSize,
        pageNum: queryParams.value.pageNum, page: queryParams.value.pageNum,
        filters: { ...(props.filters || {}) },
        keyword: queryParams.value.keyword || undefined,
    }
    if (Array.isArray(busData.value.conditions)) res.conditions = res.conditions.concat(busData.value.conditions)
    if (props.conditions) res.conditions = res.conditions.concat(props.conditions)
    return res
})

function setFocusRow(row) { focusRow.value = row; emit('update:focusRow', row) }

const getlist = () => {
    if (props.childed && !props.fk) return
    let para = Object.assign({}, listpara.value)
    if (Array.isArray(queryRowCondtions.value)) para.conditions = para.conditions.concat(queryRowCondtions.value)
    if (typeof props.beforeLoad == "function") para = props.beforeLoad(para)
    loading.value = true
    props.api.queryBizSysPortallist(para).then((res) => {
        mlist.value = res.Items || res.records || res.Records || []
        total.value = res.Total ?? res.total ?? 0
        listHolder.init(mlist.value, oidField)
        props.bus?.setList?.(VCName, mlist)
        loading.value = false
    }).catch((err) => {
        swal({ text: err.message, icon: "error", button: "关闭" })
        loading.value = false
    })
}

function saveChange() {
    const tar = listHolder.compare(mlist.value)
    if (!tar.length) { swal({ text: "没有变更的信息", icon: "info", button: "关闭" }); return }
    props.api.saveBizSysPortalList(tar).then(() => { swal({ text: "保存成功", icon: "info", button: "关闭" }); getlist() })
}

const removeRow = (row) => {
    alertIsOK("确认要操作吗？将删除当前行数据！", function (e) {
        if (e == "yes") {
            props.api.removeBizSysPortalByIds([row[oidField]]).then(() => {
                getlist(); swal({ text: "删除成功", icon: "success", button: "关闭" })
            })
        }
    }, "删除当前行数据")
}

watch(() => props.filters, () => getlist(), { deep: true })
watch(() => props.conditions, () => getlist(), { deep: true })
onMounted(() => {
    if (props.bus) {
        props.bus.initVCList(VCName, mlist, total, focusRow)
        props.bus.registerAction(VCName, getlist, saveChange, removeRow)
        props.bus.hub.on("refresh", getlist)
    }
    if (props.autoLoad) getlist()
})

function openRow(row) { editingRow.value = row; diaEditOpened.value = true }
function closeRowEdit() { editingRow.value = {}; diaEditOpened.value = false }
function onDialogSave() { unref(editLiveVCRef.value?.vcauto)?.saveChange?.() }
function onBtnAddClick() {
    editingRow.value = {
        P_Display: "1", P_IsOn: true, P_Name: "新建门户", P_Theme: "1",
        P_ShowTitle: "1", P_ColNum: 24, P_Rows: 1, P_Editable: false,
        ...(props.filters || {}),
    }
    diaEditOpened.value = true
}
function designRow(row) {
    router.push({ path: "/portal/design", query: { name: row.P_Key } })
}
</script>

<style lang="css" scoped>
.vc-top-toolbar .toolbar-action-btn, .tv-dialog-header .toolbar-action-btn {
    height: 32px; padding: 0 12px; border-radius: 3px; font-size: 14px; font-weight: 500;
}
.vc-top-toolbar .toolbar-btn-add-yellow {
    --el-button-text-color: #fff; --el-button-bg-color: #FEB75B; --el-button-border-color: #FEB75B;
    --el-button-hover-text-color: #fff; --el-button-hover-bg-color: #f3ad52; --el-button-hover-border-color: #f3ad52;
}
.tv-dialog-header .toolbar-btn-primary {
    --el-button-text-color: #fff; --el-button-bg-color: #449FFB; --el-button-border-color: #449FFB;
    --el-button-hover-text-color: #fff; --el-button-hover-bg-color: #3d8fe2; --el-button-hover-border-color: #3d8fe2;
}
</style>
