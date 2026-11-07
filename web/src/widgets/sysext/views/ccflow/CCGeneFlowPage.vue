<template>
<CCGeneFlowlist>
    <!-- -->
</CCGeneFlowlist>
</template>

<script setup>
import { reactive, computed, nextTick, ref, onMounted } from 'vue';

import CCGeneFlowlist from "@s/components/CCflow/geneflow/CCGeneFlowlist.vue"


const props = defineProps({
    cell:null,
    vc:null,
})

</script>
