<template>
<formcard title="区块控件关联" width="" flex stretch>
    <template #toolbar>
        <div ref="VC_Sys_PortCellLinkListHead"></div>
    </template>
    <VC_Sys_PortCellLinkList
        :api="api" :bus="bus" :page="pageData"
        :showTitle="false" title="区块控件关联"
        ref="VC_Sys_PortCellLinkListBody"
        :pageSize="30" :flex="true" height="100%"
        :position="VC_Sys_PortCellLinkListHead"
        :filters="filters"
    />
</formcard>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import useDataBus from "@/uiext/gene/hook/pageDataBus.js"
import formcard from "@/ui/panels/formcard.vue"
import * as pageAPI from "./Biz_Sys_PortCellLinkListapi.js"
import VC_Sys_PortCellLinkList from "../VC/ListVC/VC_Sys_PortCellLinkList.vue"

const props = defineProps({ filters: { type: Object, default: null } })
const BizName = "Biz_Sys_PortCellLinkList"
const api = Object.assign({}, pageAPI)
const bus = useDataBus(BizName)
const pageData = ref({})
const pageLoading = ref(true)
const VC_Sys_PortCellLinkListHead = ref(null)
const VC_Sys_PortCellLinkListBody = ref(null)

function loadPageInit() {
    pageAPI.loadPageInit({}).then((res) => {
        pageLoading.value = false
        pageData.value = { ...(res || {}), codeTable: res?.codeTable || res?.dicts || {} }
    })
}
onMounted(() => loadPageInit())
</script>
