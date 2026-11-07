<template>
    <ptpagelist :channel="channel"></ptpagelist>
</template>

<script>
import ptpagelist from "./ptpagelist.vue"

export default {
    name: "cnPtPagePage",
    components: {
        ptpagelist
    },
    props: {
        channel: null
    }
}
</script>
