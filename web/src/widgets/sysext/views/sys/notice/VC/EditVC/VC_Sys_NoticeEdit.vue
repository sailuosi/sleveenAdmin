<template>
<div class="vcfm sroll" v-loading="loading">
    <el-form class="gridfm fmvcbody" label-width="110px" :disabled="disabled">
        <fminput v-model="focusRow.title" label="标题" type="text"></fminput>
        <fminput v-model="focusRow.content" label="内容" type="textarea"></fminput>
        <fminput v-model="focusRow.type" label="类型" type="number"></fminput>
        <fminput v-model="focusRow.status" label="状态" type="number" disabled></fminput>
    </el-form>
</div>
</template>

<script setup>
import fminput from "@/ui/common/input/fminput.vue"
import { ref, computed, watch, onMounted } from 'vue'
import useFormEditor from "@/uiext/gene/hook/useFormEditor.js"

const props = defineProps({
    api: null, disabled: { type: Boolean, default: false },
    autoLoad: { type: Boolean, default: true }, oid: null, row: null,
})
const emit = defineEmits(['refresh', 'close'])
const loading = ref(false)
const focusRow = ref({})
const oidField = "id"
const rowpara = computed(() => props.oid || props.row?.[oidField] || null)
watch(rowpara, () => loadRow())
onMounted(() => { if (props.autoLoad) loadRow() })

const loadRow = () => {
    if (props.row != null) { focusRow.value = { ...props.row }; return }
    if (!rowpara.value) { focusRow.value = { type: 1, status: 1 }; return }
    loading.value = true
    props.api.queryBizSysNoticeById(rowpara.value).then((res) => {
        focusRow.value = res || {}; loading.value = false
    }).catch(() => { loading.value = false })
}

function saveChange() {
    props.api.saveBizSysNoticeList([focusRow.value]).then(() => {
        swal({ text: "保存成功", icon: "success", button: "关闭" })
        emit('refresh'); emit('close')
    })
}
defineExpose({ saveChange, focusRow })
</script>
