<template>


    <CCBizFlowlist dict="a"></CCBizFlowlist>


</template>

<script setup>
/**
        <fminput v-model="focusRow.bf_code" label="业务编码" type="dict" dict="KB_CCflowTask"></fminput>
<CCBizFlowlist>
    <!-- -->
</CCBizFlowlist>
 */


import { reactive, computed, nextTick, ref, onMounted } from 'vue';

import { ElForm,ElFormItem,ElLink,ElInput } from 'element-plus'
import fminput from "@/ui/common/input/fminput.vue"
import CCBizFlowlist from "@s/components/CCflow/bizflow/CCBizFlowlist.vue"

import dicselect from "@/ui/common/input/dicselect.vue"

const props = defineProps({
    cell:null,
    vc:null,
})

const focusRow=ref({})

</script>
