<template>
<div class="vcfm sroll" v-loading="loading">
    <div class="top-toolbar" v-if="!hideToolbar">
        <teleport :to="position" :disabled="!position"><slot name="toolbar"></slot></teleport>
    </div>
    <el-form class="gridfm fmvcbody" label-width="110px" :disabled="disabled">
        <fminput v-model="focusRow.Et_Title" label="标题" type="text"></fminput>
        <fminput v-model="focusRow.Et_Desc" label="描述" type="textarea"></fminput>
        <fminput v-model="focusRow.Et_Audience" label="目标读者" type="text"></fminput>
        <fminput v-model="focusRow.Et_Format" label="发布形式" type="dict" :dictlist="codeTables.EbPublishFormat"></fminput>
        <fminput v-model="focusRow.Et_Pipeline" label="流水线" type="dict" :dictlist="pipelineOpts"></fminput>
        <fminput v-model="focusRow.Et_Stage" label="当前阶段" type="text"></fminput>
        <fminput v-model="focusRow.Et_Status" label="状态" type="dict" :dictlist="codeTables.EbTopicStatus"></fminput>
        <fminput v-model="focusRow.Et_Platforms" label="发布平台" type="textarea"></fminput>
        <fminput v-model="focusRow.Et_ExpectAt" label="预期发布" type="date"></fminput>
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
const VCName = "VC_Sa_EbTopicEdit"
const oidField = "Sa_EbTopicOID"
const loading = ref(false)
const focusRow = ref({})
const pipelineOpts = [{ value: 'graphic', label: '图文' }, { value: 'video', label: '视频' }]
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
        focusRow.value = props.row
        fmholder.init(props.row, oidField)
        props.bus?.setRow?.(VCName, focusRow)
        emit('update:focusRow', props.row)
        return
    }
    if (!rowpara.value) { focusRow.value = {}; return }
    loading.value = true
    return props.api.queryBizSaEbTopicById(rowpara.value).then((res) => {
        focusRow.value = res || {}
        loading.value = false
        fmholder.init(res, oidField)
        props.bus?.setRow?.(VCName, focusRow)
        emit('update:focusRow', res)
    }).catch(() => { loading.value = false })
}

function saveChange() {
    let para = fmholder.compare(focusRow.value)
    if (para == null) {
        if (!focusRow.value[oidField]) {
            props.api.saveBizSaEbTopicList([focusRow.value]).then(() => {
                swal({ text: "保存成功", icon: "info", button: "关闭" })
                emit('refresh')
            })
            return
        }
        swal({ text: "没有数据变更", icon: "info", button: "关闭" })
        return
    }
    props.api.saveBizSaEbTopicList([para]).then(() => {
        swal({ text: "保存成功", icon: "info", button: "关闭" })
        loadRow(); emit('refresh')
    })
}

const removeRow = (row) => {
    alertIsOK("确认要操作吗？将删除当前行数据！", function (e) {
        if (e == "yes") props.api.removeBizSaEbTopicByIds([row[oidField]]).then(() => emit('refresh'))
    }, "删除数据行：")
}

defineExpose({ saveChange, loadRow, focusRow })
</script>
