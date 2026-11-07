<template>
<div class="vcfm sroll" v-loading="loading">
    <el-tabs v-model="activeName">
        <el-tab-pane label="区块表单" name="cell">
            <el-form class="gridfm fmvcbody" label-width="110px" :disabled="disabled">
                <fminput v-model="focusRow.PC_Name" label="名称" type="text"></fminput>
                <fminput v-model="focusRow.PC_IsOn" label="启用" type="boolean"></fminput>
                <fminput v-model="focusRow.PC_Key" label="区块key" type="text"></fminput>
                <fminput v-model="focusRow.PC_Idx" label="序号" type="number"></fminput>
                <fminput v-model="focusRow.PC_Many" label="多播" type="dict" :dictlist="codeTables.SysAppletMany"></fminput>
                <fminput v-model="focusRow.PC_Theme" label="主题" type="dict" :dictlist="codeTables.SysPtCellTheme"></fminput>
                <fminput v-model="focusRow.PC_TitleAlign" label="位置" type="dict" :dictlist="codeTables.SysAppletTitle"></fminput>
                <fminput v-model="focusRow.PC_I" label="i" type="text"></fminput>
                <fminput v-model="focusRow.PC_X" label="x" type="number"></fminput>
                <fminput v-model="focusRow.PC_Y" label="y" type="number"></fminput>
                <fminput v-model="focusRow.PC_W" label="w" type="number"></fminput>
                <fminput v-model="focusRow.PC_H" label="h" type="number"></fminput>
                <fminput v-model="focusRow.PC_Margin" label="外边距" type="text"></fminput>
                <fminput v-model="focusRow.PC_Padding" label="内边距" type="text"></fminput>
                <fminput v-model="focusRow.PC_Style" label="自定义样式" type="textarea"></fminput>
                <fminput v-model="focusRow.PC_Remark" label="备注" type="textarea"></fminput>
                <fminput v-model="focusRow.PC_Css" label="Css" type="text"></fminput>
                <fminput v-model="focusRow.PC_CellPara" label="额外配置参数" type="textarea"></fminput>
                <fminput v-model="focusRow.spPortalId" label="门户" type="text" disabled></fminput>
                <fminput v-model="focusRow.spcId" label="主键" type="text" disabled></fminput>
                <fminput v-model="focusRow.SYS_Created" label="创建日期" type="datetime" disabled></fminput>
                <fminput v-model="focusRow.SYS_LAST_UPD" label="最后修改日期" type="datetime" disabled></fminput>
                <slot></slot>
            </el-form>
        </el-tab-pane>
        <el-tab-pane label="控件" name="link" lazy v-if="focusRow.spcId">
            <div class="cell-link-pane">
                <VC_Sys_PortCellLinkList
                    :api="linkApi" :bus="bus" :page="page"
                    :filters="{ SpPortCellId: focusRow.spcId, SpPortalId: focusRow.spPortalId }"
                    :flex="true"
                    height="100%"
                />
            </div>
        </el-tab-pane>
    </el-tabs>
</div>
</template>

<script setup>
import fminput from "@/ui/common/input/fminput.vue"
import { ref, computed, watch, onMounted } from 'vue'
import useFormEditor from "@/uiext/gene/hook/useFormEditor.js"
import VC_Sys_PortCellLinkList from "../../../portcelllink/VC/ListVC/VC_Sys_PortCellLinkList.vue"
import * as linkPageAPI from "../../../portcelllink/Page/Biz_Sys_PortCellLinkListapi.js"

const props = defineProps({
    page: null, api: null, bus: null, height: null,
    disabled: { type: Boolean, default: false },
    hideToolbar: { type: Boolean, default: false },
    autoLoad: { type: Boolean, default: true },
    position: null, oid: null, row: null,
})
const fmholder = useFormEditor()
const emit = defineEmits(['change', 'delete', 'refresh', 'update:focusRow'])
const VCName = "VC_Sys_PortCellEdit"
const oidField = "spcId"
const loading = ref(false)
const focusRow = ref({})
const activeName = ref("cell")
const linkApi = Object.assign({}, linkPageAPI)
const codeTables = computed(() => props.page?.codeTable || {})
const rowpara = computed(() => props.oid || props.row?.[oidField] || null)

watch(rowpara, () => loadRow())
onMounted(() => {
    if (props.bus) {
        props.bus.initVCEdit(VCName, focusRow)
        props.bus.registerAction(VCName, loadRow, saveChange, removeRow)
        props.bus.hub.on("refresh", loadRow)
    }
    if (props.autoLoad) loadRow()
})

const loadRow = () => {
    if (props.row != null) {
        focusRow.value = { ...props.row }
        fmholder.init(focusRow.value, oidField)
        props.bus?.setRow?.(VCName, focusRow)
        emit('update:focusRow', focusRow.value)
        return
    }
    if (!rowpara.value) { focusRow.value = {}; return }
    loading.value = true
    return props.api.queryBizSysPortCellById(rowpara.value).then((res) => {
        focusRow.value = res || {}
        loading.value = false
        fmholder.init(focusRow.value, oidField)
        props.bus?.setRow?.(VCName, focusRow)
        emit('update:focusRow', res)
    }).catch(() => { loading.value = false })
}

function saveChange() {
    const para = fmholder.compare(focusRow.value)
    const payload = para == null
        ? (!focusRow.value[oidField] ? { ...focusRow.value } : null)
        : para
    if (payload == null) {
        swal({ text: "没有数据变更", icon: "info", button: "关闭" })
        return
    }
    props.api.saveBizSysPortCellList([payload]).then(() => {
        swal({ text: "保存成功", icon: "info", button: "关闭" })
        loadRow(); emit('refresh')
    })
}

const removeRow = (row) => {
    alertIsOK("确认要操作吗？将删除当前行数据！", function (e) {
        if (e == "yes") {
            props.api.removeBizSysPortCellByIds([row[oidField]]).then(() => emit('refresh'))
        }
    }, "删除数据行：")
}

defineExpose({ saveChange, loadRow, focusRow })
</script>

<style scoped>
.cell-link-pane { height: 55vh; }
.gridfm { display: grid; grid-template-columns: 1fr 1fr; gap: 8px 12px; padding: 8px; }
</style>
