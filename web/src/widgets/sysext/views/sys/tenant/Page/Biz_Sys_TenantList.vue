<template>
<formcard title="租户管理" width="" flex stretch>
    <template #toolbar>
        <div ref="VC_Sys_TenantListHead"></div>
    </template>
    <VC_Sys_TenantList
        :api="api" :bus="bus" :page="pageData"
        :showTitle="false"
        title="租户管理"
        :pageSize="30"
        :flex="true"
        height="100%"
        :position="VC_Sys_TenantListHead"
    />
</formcard>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import useDataBus from "@/uiext/gene/hook/pageDataBus.js"
import formcard from "@/ui/panels/formcard.vue"
import * as pageAPI from "./Biz_Sys_TenantListapi.js"
import VC_Sys_TenantList from "../VC/ListVC/VC_Sys_TenantList.vue"

const BizName = "Biz_Sys_TenantList"
const api = Object.assign({}, pageAPI)
const bus = useDataBus(BizName)
const pageData = ref({})
const VC_Sys_TenantListHead = ref(null)

onMounted(() => {
    pageAPI.loadPageInit({}).then((res) => {
        pageData.value = { ...(res || {}), codeTable: res?.codeTable || res?.dicts || {} }
    })
})
</script>
