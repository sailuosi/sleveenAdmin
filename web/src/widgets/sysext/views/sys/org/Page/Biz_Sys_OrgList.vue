<template>
<formcard title="组织管理" width="" flex stretch>
    <template #toolbar><div ref="headRef"></div></template>
    <VC_Sys_OrgList :api="api" :bus="bus" :page="pageData" :pageSize="30" :flex="true" height="100%" :position="headRef" />
</formcard>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import useDataBus from "@/uiext/gene/hook/pageDataBus.js"
import formcard from "@/ui/panels/formcard.vue"
import * as pageAPI from "./Biz_Sys_OrgListapi.js"
import VC_Sys_OrgList from "../VC/ListVC/VC_Sys_OrgList.vue"

const api = Object.assign({}, pageAPI)
const bus = useDataBus("Biz_Sys_OrgList")
const pageData = ref({})
const headRef = ref(null)
onMounted(() => pageAPI.loadPageInit({}).then((res) => { pageData.value = res || {} }))
</script>
