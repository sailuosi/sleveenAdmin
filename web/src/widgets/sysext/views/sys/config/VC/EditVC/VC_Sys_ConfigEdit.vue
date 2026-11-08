<template>
<div class="vcfm sroll" v-loading="loading">
    <el-form class="gridfm fmvcbody" label-width="110px" :disabled="disabled">
        <fminput v-model="focusRow.code" label="编码" type="text"></fminput>
        <fminput v-model="focusRow.value" label="值" type="text"></fminput>
        <fminput v-model="focusRow.remark" label="备注" type="textarea"></fminput>
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
const VCName = "VC_Sys_ConfigEdit"
const loading = ref(false)
const focusRow = ref({})
const oidField = "id"

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
        focusRow.value = { ...props.row }
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
    props.api.queryBizSysConfigById(rowpara.value).then((res) => {
        focusRow.value = res || {}
        fmholder.init(focusRow.value, oidField)
        loading.value = false
    }).catch(() => { loading.value = false })
}

function saveChange() {
    props.api.saveBizSysConfigList([focusRow.value]).then(() => {
        swal({ text: "保存成功", icon: "success", button: "关闭" })
        emit('refresh')
        emit('close')
    })
}

function removeRow() {}

defineExpose({ saveChange, loadRow, focusRow })
</script>
