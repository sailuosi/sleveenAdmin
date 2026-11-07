<template>
<formcard title="笔记" width="" flex stretch>
    <template #toolbar>
        <div ref="VC_Sa_EbNoteListHead"></div>
    </template>
    <VC_Sa_EbNoteList
        :api="api" :bus="bus" :page="pageData"
        :showTitle="false"
        title="笔记"
        ref="VC_Sa_EbNoteListBody"
        :pageSize="30"
        :flex="true"
        height="100%"
        :position="VC_Sa_EbNoteListHead"
    />
</formcard>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import useDataBus from "@/uiext/gene/hook/pageDataBus.js"
import formcard from "@/ui/panels/formcard.vue"
import * as pageAPI from "./Biz_Sa_EbNoteListapi.js"
import VC_Sa_EbNoteList from "../VC/ListVC/VC_Sa_EbNoteList.vue"

const BizName = "Biz_Sa_EbNoteList"
const api = Object.assign({}, pageAPI)
const bus = useDataBus(BizName)
const pageData = ref({})
const pageLoading = ref(true)
const VC_Sa_EbNoteListHead = ref(null)
const VC_Sa_EbNoteListBody = ref(null)

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
