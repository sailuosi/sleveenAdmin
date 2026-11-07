<template>
<formcard title="门户配置" width="" flex stretch>
    <template #toolbar>
        <div ref="VC_Sys_PortalListHead"></div>
    </template>
    <VC_Sys_PortalList
        :api="api" :bus="bus" :page="pageData"
        :showTitle="false"
        title="门户配置"
        ref="VC_Sys_PortalListBody"
        :pageSize="30"
        :flex="true"
        height="100%"
        :position="VC_Sys_PortalListHead"
        :filters="filters"
    />
</formcard>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import useDataBus from "@/uiext/gene/hook/pageDataBus.js"
import formcard from "@/ui/panels/formcard.vue"
import * as pageAPI from "./Biz_Sys_PortalListapi.js"
import VC_Sys_PortalList from "../VC/ListVC/VC_Sys_PortalList.vue"

const props = defineProps({ filters: { type: Object, default: null } })
const BizName = "Biz_Sys_PortalList"
const api = Object.assign({}, pageAPI)
const bus = useDataBus(BizName)
const pageData = ref({})
const pageLoading = ref(true)
const VC_Sys_PortalListHead = ref(null)
const VC_Sys_PortalListBody = ref(null)

function loadPageInit() {
    pageAPI.loadPageInit({}).then((res) => {
        pageLoading.value = false
        pageData.value = {
            ...(res || {}),
            codeTable: res?.codeTable || res?.dicts || {}
        }
    })
}

onMounted(() => loadPageInit())
</script>
