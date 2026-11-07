<template>
<formcard title="分类设置" width="" flex stretch>
    <template #toolbar>
        <div ref="VC_Sa_EbCataListHead"></div>
    </template>
    <VC_Sa_EbCataList
        :api="api" :bus="bus" :page="pageData"
        :showTitle="false"
        title="分类设置"
        ref="VC_Sa_EbCataListBody"
        :pageSize="30"
        :flex="true"
        height="100%"
        :position="VC_Sa_EbCataListHead"
    />
</formcard>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import useDataBus from "@/uiext/gene/hook/pageDataBus.js"
import formcard from "@/ui/panels/formcard.vue"
import * as pageAPI from "./Biz_Sa_EbCataListapi.js"
import VC_Sa_EbCataList from "../VC/ListVC/VC_Sa_EbCataList.vue"

const BizName = "Biz_Sa_EbCataList"
const api = Object.assign({}, pageAPI)
const bus = useDataBus(BizName)
const pageData = ref({})
const pageLoading = ref(true)
const VC_Sa_EbCataListHead = ref(null)
const VC_Sa_EbCataListBody = ref(null)

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
