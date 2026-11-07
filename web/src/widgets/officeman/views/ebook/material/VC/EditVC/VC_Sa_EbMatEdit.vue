<template>
<div class="vcfm sroll" v-loading="loading">
    <div class="top-toolbar" v-if="!hideToolbar">
        <teleport :to="position" :disabled="!position">
            <slot name="toolbar"></slot>
        </teleport>
    </div>
    <el-form class="gridfm fmvcbody" label-width="110px" :disabled="disabled">
        <fminput v-model="focusRow.Em_Title" label="标题" type="text"></fminput>
        <fminput v-model="focusRow.Em_Platform" label="平台" type="dict" :dictlist="codeTables.EbPlatform"></fminput>
        <fminput v-model="focusRow.Em_SourceAuthor" label="作者" type="text"></fminput>
        <fminput v-model="focusRow.Em_SourceUrl" label="原文链接" type="text"></fminput>
        <fminput v-model="focusRow.Em_LikeCount" label="点赞" type="text"></fminput>
        <fminput v-model="focusRow.Em_BookmarkCount" label="收藏" type="text"></fminput>
        <fminput v-model="focusRow.Em_Status" label="状态" type="dict" :dictlist="codeTables.EbMaterialStatus"></fminput>
        <fminput v-model="focusRow.Em_Summary" label="摘要" type="textarea"></fminput>
        <fminput v-model="focusRow.Em_LearningPoints" label="学习点" type="textarea"></fminput>
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
const VCName = "VC_Sa_EbMatEdit"
const oidField = "Sa_EbMatOID"
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
        focusRow.value = props.row
        fmholder.init(props.row, oidField)
        props.bus?.setRow?.(VCName, focusRow)
        emit('update:focusRow', props.row)
        return
    }
    if (!rowpara.value) { focusRow.value = {}; return }
    loading.value = true
    return props.api.queryBizSaEbMatById(rowpara.value).then((res) => {
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
        // 新增行：无 OID 时直接整行保存
        if (!focusRow.value[oidField]) {
            props.api.saveBizSaEbMatList([focusRow.value]).then(() => {
                swal({ text: "保存成功", icon: "info", button: "关闭" })
                emit('refresh')
            })
            return
        }
        swal({ text: "没有数据变更", icon: "info", button: "关闭" })
        return
    }
    props.api.saveBizSaEbMatList([para]).then(() => {
        swal({ text: "保存成功", icon: "info", button: "关闭" })
        loadRow()
        emit('refresh')
    })
}

const removeRow = (row) => {
    alertIsOK("确认要操作吗？将删除当前行数据！", function (e) {
        if (e == "yes") {
            props.api.removeBizSaEbMatByIds([row[oidField]]).then(() => emit('refresh'))
        }
    }, "删除数据行：")
}

defineExpose({ saveChange, loadRow, focusRow })
</script>
