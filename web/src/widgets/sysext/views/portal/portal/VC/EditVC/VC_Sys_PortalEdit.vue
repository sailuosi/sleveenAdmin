<template>
<div class="vcfm sroll" v-loading="loading">
    <el-tabs v-model="activeName">
        <el-tab-pane label="门户表单" name="pao">
            <div class="fm-toolbar" v-if="focusRow.spId">
                <el-button type="warning" :loading="clearingCache" :disabled="disabled" @click="clearCache">
                    清理缓存
                </el-button>
            </div>
            <el-form class="gridfm fmvcbody" label-width="110px" :disabled="disabled">
                <fminput v-model="focusRow.P_Name" label="名称" type="text"></fminput>
                <fminput v-model="focusRow.P_IsOn" label="启用" type="boolean"></fminput>
                <fminput v-model="focusRow.P_Key" label="唯一编码" type="text"></fminput>
                <fminput v-model="focusRow.P_Pulic" label="共享" type="dict" :dictlist="codeTables.SysShareStatus"></fminput>
                <fminput v-model="focusRow.P_Theme" label="主题" type="dict" :dictlist="codeTables.SysPtCellTheme"></fminput>
                <fminput v-model="focusRow.P_ColNum" label="列数" type="number"></fminput>
                <fminput v-model="focusRow.P_Rows" label="行高" type="number"></fminput>
                <fminput v-model="focusRow.P_Style" label="自定义样式" type="textarea"></fminput>
                <fminput v-model="focusRow.P_Para" label="参数" type="text"></fminput>
                <fminput v-model="focusRow.P_Note" label="说明" type="textarea"></fminput>
                <fminput v-model="focusRow.P_Task" label="业务" type="dict" :dictlist="codeTables.SysPtTask"></fminput>
                <fminput v-model="focusRow.P_ShowTitle" label="显示标题" type="dict" :dictlist="codeTables.SysAppletTitle"></fminput>
                <fminput v-model="focusRow.P_Display" label="布局方式" type="dict" :dictlist="codeTables.SysPtDisplay"></fminput>
                <fminput v-model="focusRow.P_Editable" label="可拖动" type="boolean"></fminput>
                <fminput v-model="focusRow.P_Idx" label="序号" type="number"></fminput>
                <fminput v-model="focusRow.P_Group" label="分组" type="text"></fminput>
                <fminput v-model="focusRow.P_Icon" label="图标" type="text"></fminput>
                <fminput v-model="focusRow.P_Css" label="类" type="text"></fminput>
                <fminput v-model="focusRow.P_Remark" label="备注" type="text"></fminput>
                <fminput v-model="focusRow.spId" label="主键" type="text" disabled></fminput>
                <fminput v-model="focusRow.SYS_Created" label="创建日期" type="datetime" disabled></fminput>
                <fminput v-model="focusRow.SYS_LAST_UPD" label="最后修改日期" type="datetime" disabled></fminput>
                <slot></slot>
            </el-form>
        </el-tab-pane>
        <el-tab-pane label="区块" name="cell" lazy v-if="focusRow.spId">
            <div class="portal-cell-pane">
                <VC_Sys_PortCellList
                    :api="cellApi" :bus="bus" :page="page"
                    :filters="{ SpPortalId: focusRow.spId }"
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
import VC_Sys_PortCellList from "../../../portcell/VC/ListVC/VC_Sys_PortCellList.vue"
import * as cellPageAPI from "../../../portcell/Page/Biz_Sys_PortCellListapi.js"
import { clearPortConfigByOid } from "@/widgets/sysext/api/system/portalCommon.js"

const props = defineProps({
    page: null, api: null, bus: null, height: null,
    disabled: { type: Boolean, default: false },
    hideToolbar: { type: Boolean, default: false },
    autoLoad: { type: Boolean, default: true },
    position: null, oid: null, row: null,
})
const fmholder = useFormEditor()
const emit = defineEmits(['change', 'delete', 'refresh', 'update:focusRow'])
const VCName = "VC_Sys_PortalEdit"
const oidField = "spId"
const loading = ref(false)
const clearingCache = ref(false)
const focusRow = ref({})
const activeName = ref("pao")
const cellApi = Object.assign({}, cellPageAPI)
const codeTables = computed(() => props.page?.codeTable || {})
const rowpara = computed(() => props.oid || props.row?.[oidField] || null)

function clearCache() {
    const oid = focusRow.value.spId
    if (!oid) {
        swal({ text: "请先保存门户后再清理缓存", icon: "info", button: "关闭" })
        return
    }
    clearingCache.value = true
    clearPortConfigByOid(oid, focusRow.value.P_Key).then(() => {
        swal({ text: "缓存已清除", icon: "success", button: "关闭" })
    }).catch((err) => {
        swal({ text: err?.message || "清理缓存失败", icon: "error", button: "关闭" })
    }).finally(() => {
        clearingCache.value = false
    })
}

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
    return props.api.queryBizSysPortalById(rowpara.value).then((res) => {
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
    props.api.saveBizSysPortalList([payload]).then(() => {
        swal({ text: "保存成功", icon: "info", button: "关闭" })
        loadRow(); emit('refresh')
    })
}

const removeRow = (row) => {
    alertIsOK("确认要操作吗？将删除当前行数据！", function (e) {
        if (e == "yes") {
            props.api.removeBizSysPortalByIds([row[oidField]]).then(() => emit('refresh'))
        }
    }, "删除数据行：")
}

defineExpose({ saveChange, loadRow, focusRow })
</script>

<style scoped>
.fm-toolbar { padding: 0 8px 8px; }
.portal-cell-pane { height: 60vh; }
.gridfm { display: grid; grid-template-columns: 1fr 1fr; gap: 8px 12px; padding: 8px; }
</style>
