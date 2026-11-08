<template>
<formcard title="流程定义" width="" flex stretch>
    <template #toolbar>
        <div ref="VC_Sail_ProcessListHead"></div>
    </template>
    <VC_Sail_ProcessList
        :api="api" :bus="bus" :page="pageData"
        :showTitle="false"
        title="流程定义"
        :pageSize="30"
        :flex="true"
        height="100%"
        :position="VC_Sail_ProcessListHead"
    />
</formcard>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import useDataBus from "@/uiext/gene/hook/pageDataBus.js"
import formcard from "@/ui/panels/formcard.vue"
import * as pageAPI from "./Biz_Sail_ProcessListapi.js"
import VC_Sail_ProcessList from "../VC/ListVC/VC_Sail_ProcessList.vue"

const BizName = "Biz_Sail_ProcessList"
const api = Object.assign({}, pageAPI)
const bus = useDataBus(BizName)
const pageData = ref({})
const VC_Sail_ProcessListHead = ref(null)

onMounted(() => {
    pageAPI.loadPageInit({}).then((res) => {
        pageData.value = { ...(res || {}), codeTable: res?.codeTable || res?.dicts || {} }
    })
})
</script>
