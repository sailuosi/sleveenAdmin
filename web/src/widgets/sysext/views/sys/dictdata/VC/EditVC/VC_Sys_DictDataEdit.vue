<template>
<div class="vcfm sroll" v-loading="loading">
    <el-form class="gridfm fmvcbody" label-width="110px" :disabled="disabled">
        <fminput v-model="focusRow.typeId" label="类型Id" type="text" v-if="!typeIdLocked"></fminput>
        <fminput v-model="focusRow.label" label="标签" type="text"></fminput>
        <fminput v-model="focusRow.value" label="值" type="text"></fminput>
        <fminput v-model="focusRow.orderNo" label="排序" type="number"></fminput>
    </el-form>
</div>
</template>

<script setup>
import fminput from "@/ui/common/input/fminput.vue"
import { ref, computed, watch, onMounted } from 'vue'
import useFormEditor from "@/uiext/gene/hook/useFormEditor.js"

const props = defineProps({
    page: null, api: null, bus: null, disabled: { type: Boolean, default: false },
    autoLoad: { type: Boolean, default: true }, oid: null, row: null,
    typeIdLocked: { type: Boolean, default: false },
})
const fmholder = useFormEditor()
const emit = defineEmits(['refresh', 'close'])
const loading = ref(false)
const focusRow = ref({})
const oidField = "id"
const rowpara = computed(() => props.oid || props.row?.[oidField] || null)
watch(rowpara, () => loadRow())

onMounted(() => { if (props.autoLoad) loadRow() })

const loadRow = () => {
    if (props.row != null) {
        focusRow.value = { ...props.row }
        fmholder.init(props.row, oidField)
        return
    }
    if (!rowpara.value) { focusRow.value = { orderNo: 0 }; return }
    loading.value = true
    props.api.queryBizSysDictDataById(rowpara.value).then((res) => {
        focusRow.value = res || {}; fmholder.init(focusRow.value, oidField); loading.value = false
    }).catch(() => { loading.value = false })
}

function saveChange() {
    props.api.saveBizSysDictDataList([focusRow.value]).then(() => {
        swal({ text: "保存成功", icon: "success", button: "关闭" })
        emit('refresh'); emit('close')
    })
}
defineExpose({ saveChange, loadRow, focusRow })
</script>
