<template>
<div class="vcfm sroll" v-loading="loading">
    <el-form class="gridfm fmvcbody" label-width="110px">
        <fminput v-model="focusRow.pid" label="上级Id" type="text"></fminput>
        <fminput v-model="focusRow.name" label="路由名" type="text"></fminput>
        <fminput v-model="focusRow.title" label="标题" type="text"></fminput>
        <fminput v-model="focusRow.path" label="路径" type="text"></fminput>
        <fminput v-model="focusRow.component" label="组件" type="text"></fminput>
        <fminput v-model="focusRow.type" label="类型" type="number"></fminput>
        <fminput v-model="focusRow.orderNo" label="排序" type="number"></fminput>
        <fminput v-model="focusRow.permission" label="权限码" type="text"></fminput>
        <fminput v-model="focusRow.status" label="状态" type="number"></fminput>
    </el-form>
</div>
</template>

<script setup>
import fminput from "@/ui/common/input/fminput.vue"
import { ref, computed, watch, onMounted } from 'vue'

const props = defineProps({ api: null, row: null, oid: null, autoLoad: { type: Boolean, default: true } })
const emit = defineEmits(['refresh', 'close'])
const loading = ref(false)
const focusRow = ref({ type: 2, orderNo: 0, status: 1 })
const rowpara = computed(() => props.oid || props.row?.id || null)
watch(rowpara, () => loadRow())
onMounted(() => { if (props.autoLoad) loadRow() })

const loadRow = () => {
    if (props.row) { focusRow.value = { ...props.row }; return }
    if (!rowpara.value) return
    loading.value = true
    props.api.queryBizSysMenuById(rowpara.value).then((res) => { focusRow.value = res || {}; loading.value = false })
}

function saveChange() {
    props.api.saveBizSysMenuList([focusRow.value]).then(() => {
        swal({ text: "保存成功", icon: "success", button: "关闭" })
        emit('refresh'); emit('close')
    })
}
defineExpose({ saveChange, focusRow })
</script>
