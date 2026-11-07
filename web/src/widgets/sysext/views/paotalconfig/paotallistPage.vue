<template>
    <paotallist :page="page"></paotallist>
</template>

<script>
import paotallist from "./paotallist.vue"

export default {
    name: "paotallistPage",
    components: {
        paotallist
    },
    props: {
        page: null,
    }
}
</script>