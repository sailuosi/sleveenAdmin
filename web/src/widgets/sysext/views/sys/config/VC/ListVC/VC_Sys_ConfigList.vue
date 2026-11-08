<template>
    <flexpan :flex="flex" :height="height">
        <template #header="p">
            <teleport :to="position" :disabled="!position" v-if="!hideToolbar">
                <div class="vc-top-toolbar">
                    <el-button @click="onBtnAddClick" class="toolbar-action-btn toolbar-btn-add-yellow">
                        <SvgIcon name="fa fa-plus-circle" /> 添加
                    </el-button>
                </div>
            </teleport>
        </template>
        <template #default="p">
            <vclist :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="p.height"
                :flex="flex" ref="mlistvm" v-loading="loading" query :maxLine="1"
                v-model:queryConditions="queryRowCondtions" @update:focusRow="setFocusRow"
                :orderCol="true" :stripe="true">
                <vccol field="op" caption="操作" width="100px" type="text" disabled align="center" headerAlign="center">
                    <template #default="cellp">
                        <el-link class="op-edit-link" @click="openRow(cellp.row)" title="编辑" style="margin-right: 20px; color: #338fd1;">
                            <SvgIcon name="fa fa-pencil-square-o" :size="18" />
                        </el-link>
                        <el-link type="danger" @click="removeRow(cellp.row)" title="删除">
                            <SvgIcon name="fa fa-trash-o" :size="18" />
                        </el-link>
                    </template>
                </vccol>
                <vccol field="code" caption="编码" width="160px" type="text" align="left" headerAlign="center"></vccol>
                <vccol field="value" caption="值" min-width="200px" type="text" align="left" headerAlign="center"></vccol>
                <vccol field="remark" caption="备注" min-width="160px" type="text" align="left" headerAlign="center"></vccol>
            </vclist>

            <el-dialog title="编辑" class="normal" v-model="diaEditOpened" width="560px" append-to-body @close="closeRowEdit">
                <template #header>
                    <div class="dialog-header">
                        <span>编辑配置</span>
                        <el-button class="toolbar-action-btn toolbar-btn-primary" @click="onDialogSave">
                            <SvgIcon name="fa fa-save" /> 保存
                        </el-button>
                    </div>
                </template>
                <livevc name="VC_Sys_ConfigEdit" :row="editingRow" @close="closeRowEdit" @refresh="getlist"
                    :api="api" :bus="bus" :page="page" ref="editLiveVCRef">
                </livevc>
            </el-dialog>
        </template>
    </flexpan>
</template>

<script setup>
import { ref, computed, watch, onMounted, unref } from 'vue'
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
    position: null,
    flex: { type: Boolean, default: false },
    conditions: null, params: null,
    pageSize: { type: Number, default: 30 },
    fk: null, childed: { type: Boolean, default: false }, beforeLoad: null,
})

const emit = defineEmits(['change', 'delete', 'update:focusRow'])
const listHolder = useListEditor()
const VCName = "VC_Sys_ConfigList"
const oidField = "id"
const loading = ref(false)
const mlist = ref([])
const total = ref(0)
const queryParams = ref({ pageSize: props.pageSize, pageNum: 1 })
const focusRow = ref({})
const queryRowCondtions = ref([])
const diaEditOpened = ref(false)
const editingRow = ref({})
const editLiveVCRef = ref(null)

const busData = computed(() => {
    if (!props.bus) return { conditions: [] }
    return props.bus.dataMap[VCName] || { conditions: [] }
})

const listpara = computed(() => {
    const res = {
        conditions: [],
        pageSize: queryParams.value.pageSize,
        pageNum: queryParams.value.pageNum,
        page: queryParams.value.pageNum,
        keyword: queryParams.value.keyword,
    }
    if (Array.isArray(busData.value.conditions)) {
        res.conditions = res.conditions.concat(busData.value.conditions)
    }
    if (props.conditions) res.conditions = res.conditions.concat(props.conditions)
    return res
})

function setFocusRow(row) {
    focusRow.value = row
    emit('update:focusRow', row)
}

const getlist = () => {
    let para = Object.assign({}, listpara.value)
    if (Array.isArray(queryRowCondtions.value)) {
        para.conditions = para.conditions.concat(queryRowCondtions.value)
    }
    if (typeof props.beforeLoad == "function") para = props.beforeLoad(para)
    loading.value = true
    props.api.queryBizSysConfiglist(para).then((res) => {
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
    if (tar.length == 0) {
        swal({ text: "没有变更的信息", icon: "info", button: "关闭" })
        return
    }
    props.api.saveBizSysConfigList(tar).then(() => {
        swal({ text: "保存成功", icon: "info", button: "关闭" })
        getlist()
    })
}

const removeRow = (row, cap) => {
    const para = [row[oidField]]
    if (!cap) cap = "删除当前行数据"
    alertIsOK("确认要操作吗？将删除当前行数据！", function (e) {
        if (e == "yes") {
            props.api.removeBizSysConfigByIds(para).then(() => {
                getlist()
                swal({ text: "删除成功", icon: "success", button: "关闭" })
            })
        }
    }, cap)
}

onMounted(() => {
    if (props.bus) {
        props.bus.initVCList(VCName, mlist, total, focusRow)
        props.bus.registerAction(VCName, getlist, saveChange, removeRow)
        props.bus.hub.on("refresh", getlist)
    }
    if (props.autoLoad == true) getlist()
})

function openRow(row) {
    editingRow.value = row
    diaEditOpened.value = true
}
function closeRowEdit() {
    editingRow.value = {}
    diaEditOpened.value = false
}
function onDialogSave() {
    const live = editLiveVCRef.value
    if (!live?.vcauto) return
    const inner = unref(live.vcauto)
    inner?.saveChange?.()
}
function onBtnAddClick() {
    editingRow.value = { code: '', value: '', remark: '' }
    diaEditOpened.value = true
}
</script>

<style lang="css" scoped>
.vc-top-toolbar .toolbar-action-btn, .dialog-header .toolbar-action-btn {
    height: 32px; padding: 0 12px; border-radius: 3px; font-size: 14px; font-weight: 500;
}
.toolbar-btn-add-yellow {
    --el-button-text-color: #fff; --el-button-bg-color: #FEB75B; --el-button-border-color: #FEB75B;
}
.toolbar-btn-primary {
    --el-button-text-color: #fff; --el-button-bg-color: #449FFB; --el-button-border-color: #449FFB;
}
.dialog-header { display: flex; justify-content: space-between; align-items: center; width: 100%; }
</style>
