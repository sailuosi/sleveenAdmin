<template>
<formcard title="单位流程配置" width="" flex stretch>
    <template #toolbar>
        <div ref="VC_Sail_ProcessListHead"></div>
    </template>
    <div class="sf-proc-layout">
        <OrgTreeSidePane storage-key="process">
            <SailOrgFilterTree :orgs="orgFlat" @select="onUnitSelect" />
        </OrgTreeSidePane>
        <div class="sf-proc-layout__main">
            <VC_Sail_ProcessList
                :api="api" :bus="bus" :page="pageData"
                :showTitle="false"
                title="单位流程配置"
                workspace="unit"
                :pageSize="30"
                :flex="true"
                height="100%"
                :position="VC_Sail_ProcessListHead"
                :filter-unit-id="filterUnitId"
            />
        </div>
    </div>
</formcard>
</template>

<script setup>
import { computed, ref, onMounted } from 'vue'
import useDataBus from "@/uiext/gene/hook/pageDataBus.js"
import formcard from "@/ui/panels/formcard.vue"
import OrgTreeSidePane from '@sail/components/OrgTreeSidePane.vue'
import SailOrgFilterTree from '@sail/components/SailOrgFilterTree.vue'
import { getUnitScopeContext } from '@sail/api/unitScope'
import { unpackSailResult } from '@sail/utils/sailResult'
import * as pageAPI from "./Biz_Sail_ProcessListapi.js"
import VC_Sail_ProcessList from "../VC/ListVC/VC_Sail_ProcessList.vue"

const BizName = "Biz_Sail_ProcessList"
const api = Object.assign({}, pageAPI)
const bus = useDataBus(BizName)
const pageData = ref({})
const VC_Sail_ProcessListHead = ref(null)
const filterUnitId = ref(null)
const unitContext = ref(null)

const orgFlat = computed(() => {
  const orgs = unitContext.value?.orgs || unitContext.value?.units || []
  return (orgs || []).map((o) => ({
    id: String(o.id ?? o.Id ?? ''),
    parentId: o.parentId ?? o.ParentId ?? null,
    name: o.name ?? o.Name ?? o.label,
    label: o.label ?? o.name ?? o.Name
  })).filter((o) => o.id)
})

function onUnitSelect(unitId) {
  filterUnitId.value = unitId
}

onMounted(() => {
    pageAPI.loadPageInit({}).then((res) => {
        pageData.value = { ...(res || {}), codeTable: res?.codeTable || res?.dicts || {} }
    })
    getUnitScopeContext().then((data) => {
      const r = unpackSailResult(data)
      unitContext.value = r.entity || data?.Entity || data?.entity || null
    }).catch(() => {
      unitContext.value = { orgs: [] }
    })
})
</script>

<style scoped>
.sf-proc-layout {
  display: flex;
  flex-direction: row;
  height: 100%;
  min-height: 0;
  gap: 8px;
  padding: 0 8px 8px;
  box-sizing: border-box;
}
.sf-proc-layout__main {
  flex: 1 1 auto;
  min-width: 0;
  min-height: 0;
}
</style>
