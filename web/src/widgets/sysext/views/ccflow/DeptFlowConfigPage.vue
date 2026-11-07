<template>
<KBDeptFlowConfiglist>
    <!-- -->
</KBDeptFlowConfiglist>
</template>

<script setup>
import { reactive, computed, nextTick, ref, onMounted } from 'vue';

import KBDeptFlowConfiglist from "@s/components/CCflow/deptflowconfig/KBDeptFlowConfiglist.vue"


const props = defineProps({
    cell:null,
    vc:null,
})

</script>
