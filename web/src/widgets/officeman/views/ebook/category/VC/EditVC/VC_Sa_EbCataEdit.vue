<template>
<div class="vcfm sroll" v-loading="loading">
    <div class="top-toolbar" v-if="!hideToolbar">
        <teleport :to="position" :disabled="!position">
            <slot name="toolbar"></slot>
        </teleport>
    </div>
    <el-form class="gridfm fmvcbody" label-width="110px" :disabled="disabled">
        <fminput v-model="focusRow.Ec_Name" label="名称" type="text"></fminput>
        <fminput v-model="focusRow.Ec_Code" label="编码" type="text"></fminput>
        <fminput v-model="focusRow.Ec_Scope" label="作用域" type="dict" :dictlist="scopeOpts"></fminput>
        <fminput v-model="focusRow.Ec_Idx" label="排序" type="number"></fminput>
        <fminput v-model="focusRow.Ec_Remark" label="备注" type="textarea"></fminput>
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
    page: null,
    api: null,
    bus: null,
    height: null,
    disabled: { type: Boolean, default: false },
    hideToolbar: { type: Boolean, default: false },
    autoLoad: { type: Boolean, default: true },
    position: null,
    oid: null,
    row: null,
})

const fmholder = useFormEditor()
const emit = defineEmits(['change', 'delete', 'refresh', 'update:focusRow'])
const VCName = "VC_Sa_EbCataEdit"
const loading = ref(false)
const focusRow = ref({})
const oidField = "Sa_EbCataOID"

const scopeOpts = [
    { value: 'all', label: '通用' },
    { value: 'material', label: '素材' },
    { value: 'topic', label: '选题' },
    { value: 'note', label: '笔记' },
]

const rowpara = computed(() => {
    if (props.oid) return props.oid
    if (props.row) return props.row[oidField]
    return null
})

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
        focusRow.value = props.row
        fmholder.init(props.row, oidField)
        props.bus?.setRow?.(VCName, focusRow)
        emit('update:focusRow', props.row)
        return
    }
    if (!rowpara.value) {
        focusRow.value = {}
        return
    }
    loading.value = true
    return props.api.queryBizSaEbCataById(rowpara.value).then((res) => {
        focusRow.value = res || {}
        loading.value = false
        fmholder.init(res, oidField)
        props.bus?.setRow?.(VCName, focusRow)
        emit('update:focusRow', res)
    }).catch(() => { loading.value = false })
}

function saveChange() {
    const para = fmholder.compare(focusRow.value)
    if (para == null) {
        if (!focusRow.value[oidField]) {
            props.api.saveBizSaEbCataList([focusRow.value]).then(() => {
                swal({ text: "保存成功", icon: "info", button: "关闭" })
                emit('refresh')
            })
            return
        }
        swal({ text: "没有数据变更", icon: "info", button: "关闭" })
        return
    }
    props.api.saveBizSaEbCataList([para]).then(() => {
        swal({ text: "保存成功", icon: "info", button: "关闭" })
        loadRow()
        emit('refresh')
    })
}

const removeRow = (row) => {
    const para = [row[oidField]]
    alertIsOK("确认要操作吗？将删除当前行数据！", function (e) {
        if (e == "yes") {
            props.api.removeBizSaEbCataByIds(para).then(() => {
                emit('refresh')
            })
        }
    }, "删除数据行：")
}

defineExpose({ saveChange, loadRow, focusRow })
</script>
