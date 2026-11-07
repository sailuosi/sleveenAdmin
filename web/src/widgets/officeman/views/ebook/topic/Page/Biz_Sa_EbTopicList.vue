<template>
<formcard title="选题池" width="" flex stretch>
    <template #toolbar>
        <div ref="VC_Sa_EbTopicListHead"></div>
    </template>
    <VC_Sa_EbTopicList
        :api="api" :bus="bus" :page="pageData"
        :showTitle="false"
        title="选题池"
        ref="VC_Sa_EbTopicListBody"
        :pageSize="30"
        :flex="true"
        height="100%"
        :position="VC_Sa_EbTopicListHead"
    />
</formcard>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import useDataBus from "@/uiext/gene/hook/pageDataBus.js"
import formcard from "@/ui/panels/formcard.vue"
import * as pageAPI from "./Biz_Sa_EbTopicListapi.js"
import VC_Sa_EbTopicList from "../VC/ListVC/VC_Sa_EbTopicList.vue"

const BizName = "Biz_Sa_EbTopicList"
const api = Object.assign({}, pageAPI)
const bus = useDataBus(BizName)
const pageData = ref({})
const pageLoading = ref(true)
const VC_Sa_EbTopicListHead = ref(null)
const VC_Sa_EbTopicListBody = ref(null)

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
