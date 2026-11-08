<template>
<formcard title="流程表单" width="" flex stretch>
    <template #toolbar>
        <div ref="VC_Sail_FormListHead"></div>
    </template>
    <VC_Sail_FormList
        :api="api" :bus="bus" :page="pageData"
        :showTitle="false"
        title="流程表单"
        :pageSize="30"
        :flex="true"
        height="100%"
        :position="VC_Sail_FormListHead"
    />
</formcard>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import useDataBus from "@/uiext/gene/hook/pageDataBus.js"
import formcard from "@/ui/panels/formcard.vue"
import * as pageAPI from "./Biz_Sail_FormListapi.js"
import VC_Sail_FormList from "../VC/ListVC/VC_Sail_FormList.vue"

const BizName = "Biz_Sail_FormList"
const api = Object.assign({}, pageAPI)
const bus = useDataBus(BizName)
const pageData = ref({})
const VC_Sail_FormListHead = ref(null)

onMounted(() => {
    pageAPI.loadPageInit({}).then((res) => {
        pageData.value = { ...(res || {}), codeTable: res?.codeTable || res?.dicts || {} }
    })
})
</script>
