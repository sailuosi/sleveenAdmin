<template>
<formcard title="菜单管理" width="" flex stretch>
    <template #toolbar><div ref="headRef"></div></template>
    <VC_Sys_MenuList :api="api" :bus="bus" :page="pageData" :pageSize="30" :flex="true" height="100%" :position="headRef" />
</formcard>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import useDataBus from "@/uiext/gene/hook/pageDataBus.js"
import formcard from "@/ui/panels/formcard.vue"
import * as pageAPI from "./Biz_Sys_MenuListapi.js"
import VC_Sys_MenuList from "../VC/ListVC/VC_Sys_MenuList.vue"

const api = Object.assign({}, pageAPI)
const bus = useDataBus("Biz_Sys_MenuList")
const pageData = ref({})
const headRef = ref(null)
onMounted(() => pageAPI.loadPageInit({}).then((res) => { pageData.value = res || {} }))
</script>
