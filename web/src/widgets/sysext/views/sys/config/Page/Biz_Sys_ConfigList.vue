<template>
<formcard title="系统配置" width="" flex stretch>
    <template #toolbar>
        <div ref="VC_Sys_ConfigListHead"></div>
    </template>
    <VC_Sys_ConfigList
        :api="api" :bus="bus" :page="pageData"
        :showTitle="false"
        title="系统配置"
        ref="VC_Sys_ConfigListBody"
        :pageSize="30"
        :flex="true"
        height="100%"
        :position="VC_Sys_ConfigListHead"
    />
</formcard>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import useDataBus from "@/uiext/gene/hook/pageDataBus.js"
import formcard from "@/ui/panels/formcard.vue"
import * as pageAPI from "./Biz_Sys_ConfigListapi.js"
import VC_Sys_ConfigList from "../VC/ListVC/VC_Sys_ConfigList.vue"

const BizName = "Biz_Sys_ConfigList"
const api = Object.assign({}, pageAPI)
const bus = useDataBus(BizName)
const pageData = ref({})
const VC_Sys_ConfigListHead = ref(null)

function loadPageInit() {
    pageAPI.loadPageInit({}).then((res) => {
        pageData.value = {
            ...(res || {}),
            codeTable: res?.codeTable || res?.dicts || {}
        }
    })
}

onMounted(() => loadPageInit())
</script>
