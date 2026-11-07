<template>
    <ptapplet :cata="cata" :selecting="selecting"></ptapplet>
</template>

<script>
import ptapplet from "./ptapplet.vue"

export default {
    name: "cnPtAppletPage",
    components: {
        ptapplet
    },
    props: {
        cata: null,
        selecting: {
        	type: Boolean,
        	default: false
        }
    }
}
</script>