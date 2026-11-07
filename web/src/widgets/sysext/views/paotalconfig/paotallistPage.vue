<template>
    <Biz_Sys_PortalList :filters="filters" />
</template>

<script setup>
import { computed } from 'vue'
import Biz_Sys_PortalList from '@/widgets/sysext/views/portal/portal/Page/Biz_Sys_PortalList.vue'

const props = defineProps({ page: null })
const filters = computed(() => {
    if (!props.page) return null
    const f = {}
    if (props.page.sppId) f.spPortPageId = props.page.sppId
    if (props.page.PP_OrgOID || props.page.spOrgId) f.spOrgId = props.page.PP_OrgOID || props.page.spOrgId
    return Object.keys(f).length ? f : null
})
</script>
