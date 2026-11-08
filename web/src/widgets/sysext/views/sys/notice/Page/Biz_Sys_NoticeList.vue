<template>
<formcard title="通知管理" width="" flex stretch>
    <template #toolbar>
        <div ref="VC_Sys_NoticeListHead"></div>
    </template>
    <VC_Sys_NoticeList
        :api="api" :bus="bus" :page="pageData"
        :pageSize="30" :flex="true" height="100%"
        :position="VC_Sys_NoticeListHead"
    />
</formcard>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import useDataBus from "@/uiext/gene/hook/pageDataBus.js"
import formcard from "@/ui/panels/formcard.vue"
import * as pageAPI from "./Biz_Sys_NoticeListapi.js"
import VC_Sys_NoticeList from "../VC/ListVC/VC_Sys_NoticeList.vue"

const api = Object.assign({}, pageAPI)
const bus = useDataBus("Biz_Sys_NoticeList")
const pageData = ref({})
const VC_Sys_NoticeListHead = ref(null)

onMounted(() => {
    pageAPI.loadPageInit({}).then((res) => {
        pageData.value = { ...(res || {}), codeTable: res?.codeTable || res?.dicts || {} }
    })
})
</script>
