<template>
<formcard title="门户页面" width="" flex stretch>
    <template #toolbar>
        <div ref="VC_Sys_PortPageListHead"></div>
    </template>
    <VC_Sys_PortPageList
        :api="api" :bus="bus" :page="pageData"
        :showTitle="false" title="门户页面"
        ref="VC_Sys_PortPageListBody"
        :pageSize="30" :flex="true" height="100%"
        :position="VC_Sys_PortPageListHead"
        :filters="filters"
    />
</formcard>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import useDataBus from "@/uiext/gene/hook/pageDataBus.js"
import formcard from "@/ui/panels/formcard.vue"
import * as pageAPI from "./Biz_Sys_PortPageListapi.js"
import VC_Sys_PortPageList from "../VC/ListVC/VC_Sys_PortPageList.vue"

const props = defineProps({ filters: { type: Object, default: null } })
const BizName = "Biz_Sys_PortPageList"
const api = Object.assign({}, pageAPI)
const bus = useDataBus(BizName)
const pageData = ref({})
const pageLoading = ref(true)
const VC_Sys_PortPageListHead = ref(null)
const VC_Sys_PortPageListBody = ref(null)

function loadPageInit() {
    pageAPI.loadPageInit({}).then((res) => {
        pageLoading.value = false
        pageData.value = { ...(res || {}), codeTable: res?.codeTable || res?.dicts || {} }
    })
}
onMounted(() => loadPageInit())
</script>
