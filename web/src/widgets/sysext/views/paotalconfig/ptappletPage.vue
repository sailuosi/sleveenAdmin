<template>
    <Biz_Sys_AppletList :filters="filters" />
</template>

<script setup>
import { computed } from 'vue'
import Biz_Sys_AppletList from '@/widgets/sysext/views/portal/applet/Page/Biz_Sys_AppletList.vue'

const props = defineProps({ cata: null, selecting: { type: Boolean, default: false } })
const filters = computed(() => {
    if (!props.cata?.sacId) return null
    return { SaAppletCataId: props.cata.sacId }
})
</script>
