<template>
<div class="vcfm sroll" v-loading="loading">
    <el-form class="gridfm fmvcbody" label-width="110px" :disabled="disabled">
        <fminput v-model="focusRow.account" label="账号" type="text"></fminput>
        <fminput v-model="focusRow.password" label="密码" type="password" placeholder="留空则不修改"></fminput>
        <fminput v-model="focusRow.realName" label="姓名" type="text"></fminput>
        <fminput v-model="focusRow.status" label="状态" type="number"></fminput>
        <fminput v-model="focusRow.orgId" label="组织Id" type="text"></fminput>
        <fminput v-model="focusRow.tenantId" label="租户Id" type="text"></fminput>
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
})
const fmholder = useFormEditor()
const emit = defineEmits(['refresh', 'close'])
const VCName = "VC_Sys_UserEdit"
const loading = ref(false)
const focusRow = ref({})
const oidField = "id"
const rowpara = computed(() => props.oid || props.row?.[oidField] || null)
watch(rowpara, () => loadRow())

onMounted(() => {
    props.bus?.initVCEdit?.(VCName, focusRow)
    props.bus?.registerAction?.(VCName, loadRow, saveChange, () => {})
    if (props.autoLoad) loadRow()
})

const loadRow = () => {
    if (props.row != null) {
        focusRow.value = { ...props.row }
        fmholder.init(props.row, oidField)
        return
    }
    if (!rowpara.value) { focusRow.value = { status: 1 }; return }
    loading.value = true
    props.api.queryBizSysUserById(rowpara.value).then((res) => {
        focusRow.value = res || {}; fmholder.init(focusRow.value, oidField); loading.value = false
    }).catch(() => { loading.value = false })
}

function saveChange() {
    const row = { ...focusRow.value }
    if (row.password) row.password = row.password
    props.api.saveBizSysUserList([row]).then(() => {
        swal({ text: "保存成功", icon: "success", button: "关闭" })
        emit('refresh'); emit('close')
    })
}
defineExpose({ saveChange, loadRow, focusRow })
</script>
