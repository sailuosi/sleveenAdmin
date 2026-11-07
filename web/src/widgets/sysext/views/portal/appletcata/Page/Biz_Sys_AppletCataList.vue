<template>
<formcard title="组件分类" width="" flex stretch>
    <template #toolbar>
        <div ref="VC_Sys_AppletCataListHead"></div>
    </template>
    <VC_Sys_AppletCataList
        :api="api" :bus="bus" :page="pageData"
        :showTitle="false" title="组件分类"
        ref="VC_Sys_AppletCataListBody"
        :pageSize="30" :flex="true" height="100%"
        :position="VC_Sys_AppletCataListHead"
        :filters="filters"
    />
</formcard>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import useDataBus from "@/uiext/gene/hook/pageDataBus.js"
import formcard from "@/ui/panels/formcard.vue"
import * as pageAPI from "./Biz_Sys_AppletCataListapi.js"
import VC_Sys_AppletCataList from "../VC/ListVC/VC_Sys_AppletCataList.vue"

const props = defineProps({ filters: { type: Object, default: null } })
const BizName = "Biz_Sys_AppletCataList"
const api = Object.assign({}, pageAPI)
const bus = useDataBus(BizName)
const pageData = ref({})
const pageLoading = ref(true)
const VC_Sys_AppletCataListHead = ref(null)
const VC_Sys_AppletCataListBody = ref(null)

function loadPageInit() {
    pageAPI.loadPageInit({}).then((res) => {
        pageLoading.value = false
        pageData.value = { ...(res || {}), codeTable: res?.codeTable || res?.dicts || {} }
    })
}
onMounted(() => loadPageInit())
</script>
