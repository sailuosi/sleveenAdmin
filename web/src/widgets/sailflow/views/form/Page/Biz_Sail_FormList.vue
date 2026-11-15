<template>
<formcard title="流程表单" width="" flex stretch>
    <template #toolbar>
        <div class="sf-form-toolbar">
            <div ref="VC_Sail_FormListHead"></div>
        </div>
    </template>
    <div class="sf-form-layout">
        <OrgTreeSidePane storage-key="form">
            <SailOrgFilterTree :orgs="orgFlat" @select="onUnitSelect" />
        </OrgTreeSidePane>
        <div class="sf-form-layout__main">
            <VC_Sail_FormList
                :api="api" :bus="bus" :page="pageData"
                :showTitle="false"
                title="流程表单"
                :pageSize="30"
                :flex="true"
                height="100%"
                :position="VC_Sail_FormListHead"
                :unit-id="filterUnitId"
                :is-platform-admin="isPlatformAdmin"
                @create-public="onCreatePublic"
                @create-unit="onCreateUnit"
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
import * as pageAPI from "./Biz_Sail_FormListapi.js"
import VC_Sail_FormList from "../VC/ListVC/VC_Sail_FormList.vue"

const BizName = "Biz_Sail_FormList"
const api = Object.assign({}, pageAPI)
const bus = useDataBus(BizName)
const pageData = ref({})
const VC_Sail_FormListHead = ref(null)
const filterUnitId = ref(null)
const unitContext = ref(null)

const isPlatformAdmin = computed(() => !!unitContext.value?.isPlatformAdmin)
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

function onCreatePublic() {
  bus?.emit?.('sail-form-create', { authType: 'PUBLIC' })
}
function onCreateUnit() {
  bus?.emit?.('sail-form-create', { authType: 'UNIT', authId: filterUnitId.value || '' })
}

onMounted(() => {
    pageAPI.loadPageInit({}).then((res) => {
        pageData.value = { ...(res || {}), codeTable: res?.codeTable || res?.dicts || {} }
    })
    getUnitScopeContext().then((data) => {
      const r = unpackSailResult(data)
      unitContext.value = r.entity || data?.Entity || data?.entity || null
    }).catch(() => {
      unitContext.value = { isPlatformAdmin: false, orgs: [] }
    })
})
</script>

<style scoped>
.sf-form-layout {
  display: flex;
  flex-direction: row;
  height: 100%;
  min-height: 0;
  gap: 8px;
  padding: 0 8px 8px;
  box-sizing: border-box;
}
.sf-form-layout__main {
  flex: 1 1 auto;
  min-width: 0;
  min-height: 0;
}
</style>
