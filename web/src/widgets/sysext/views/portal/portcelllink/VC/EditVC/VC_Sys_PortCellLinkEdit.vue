<template>
<div class="vcfm sroll" v-loading="loading">

    <el-form class="gridfm fmvcbody" label-width="110px" :disabled="disabled">
        <fminput v-model="focusRow.CL_Link" label="区块key" type="text"></fminput>
        <fminput v-model="focusRow.CL_IsOn" label="启用" type="boolean"></fminput>
        <fminput v-model="focusRow.CL_Title" label="标题" type="text"></fminput>
        <fminput v-model="focusRow.CL_TitleAlign" label="显示标题" type="dict" :dictlist="codeTables.SysAppletTitle"></fminput>
        <fminput v-model="focusRow.CL_Theme" label="主题" type="dict" :dictlist="codeTables.SysPtCellTheme"></fminput>
        <fminput v-model="focusRow.CL_Type" label="内容类型" type="dict" :dictlist="codeTables.SysAppletType"></fminput>
        <fminput v-model="focusRow.CL_Tool" label="标题工具栏" type="dict" :dictlist="codeTables.SysApplteTool"></fminput>
        <fminput v-model="focusRow.CL_Key" label="组件key" type="text"></fminput>
        <fminput v-model="focusRow.CL_Disabled" label="可用性" type="dict" :dictlist="codeTables.SysCellDisabled"></fminput>
        <fminput v-model="focusRow.CL_Color" label="颜色" type="text"></fminput>
        <fminput v-model="focusRow.CL_Para" label="组件参数" type="text"></fminput>
        <fminput v-model="focusRow.CL_Idx" label="序号" type="number"></fminput>
        <fminput v-model="focusRow.CL_Remark" label="备注" type="text"></fminput>
        <fminput v-model="focusRow.spPortalId" label="门户" type="text" disabled></fminput>
        <fminput v-model="focusRow.spPortCellId" label="区块" type="text" disabled></fminput>
        <fminput v-model="focusRow.spclId" label="主键" type="text" disabled></fminput>
        <fminput v-model="focusRow.SYS_Created" label="创建日期" type="datetime" disabled></fminput>
        <fminput v-model="focusRow.SYS_LAST_UPD" label="最后修改日期" type="datetime" disabled></fminput>
        <slot></slot>
    </el-form>
</div>
</template>

<script setup>
import fminput from "@/ui/common/input/fminput.vue"
import { ref, computed, watch, onMounted } from 'vue'
import useFormEditor from "@/uiext/gene/hook/useFormEditor.js"


const props = defineProps({
    page: null, api: null, bus: null, height: null,
    disabled: { type: Boolean, default: false },
    hideToolbar: { type: Boolean, default: false },
    autoLoad: { type: Boolean, default: true },
    position: null, oid: null, row: null,
})
const fmholder = useFormEditor()
const emit = defineEmits(['change', 'delete', 'refresh', 'update:focusRow'])
const VCName = "VC_Sys_PortCellLinkEdit"
const oidField = "spclId"
const loading = ref(false)
const focusRow = ref({})
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
    return props.api.queryBizSysPortCellLinkById(rowpara.value).then((res) => {
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
    props.api.saveBizSysPortCellLinkList([payload]).then(() => {
        swal({ text: "保存成功", icon: "info", button: "关闭" })
        loadRow(); emit('refresh')
    })
}

const removeRow = (row) => {
    alertIsOK("确认要操作吗？将删除当前行数据！", function (e) {
        if (e == "yes") {
            props.api.removeBizSysPortCellLinkByIds([row[oidField]]).then(() => emit('refresh'))
        }
    }, "删除数据行：")
}

defineExpose({ saveChange, loadRow, focusRow })
</script>

<style scoped>
.gridfm { display: grid; grid-template-columns: 1fr 1fr; gap: 8px 12px; padding: 8px; }
</style>
