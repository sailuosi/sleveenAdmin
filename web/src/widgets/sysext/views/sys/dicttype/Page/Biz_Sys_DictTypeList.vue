<template>
<formcard title="字典类型" width="" flex stretch>
    <template #toolbar>
        <div ref="VC_Sys_DictTypeListHead"></div>
    </template>
    <VC_Sys_DictTypeList
        :api="api" :bus="bus" :page="pageData" :dataApi="dataApi"
        :showTitle="false"
        :pageSize="30"
        :flex="true"
        height="100%"
        :position="VC_Sys_DictTypeListHead"
    />
</formcard>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import useDataBus from "@/uiext/gene/hook/pageDataBus.js"
import formcard from "@/ui/panels/formcard.vue"
import * as pageAPI from "./Biz_Sys_DictTypeListapi.js"
import * as dataAPI from "../../dictdata/Page/Biz_Sys_DictDataListapi.js"
import VC_Sys_DictTypeList from "../VC/ListVC/VC_Sys_DictTypeList.vue"

const BizName = "Biz_Sys_DictTypeList"
const api = Object.assign({}, pageAPI)
const dataApi = Object.assign({}, dataAPI)
const bus = useDataBus(BizName)
const pageData = ref({})
const VC_Sys_DictTypeListHead = ref(null)

onMounted(() => {
    pageAPI.loadPageInit({}).then((res) => {
        pageData.value = { ...(res || {}), codeTable: res?.codeTable || res?.dicts || {} }
    })
})
</script>
