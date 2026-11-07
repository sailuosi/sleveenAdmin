<template lang="">
    <div>
        
    </div>
</template>
<script setup>

import { reactive, computed, nextTick, ref, defineComponent,onMounted,defineAsyncComponent ,getCurrentInstance,useAttrs,useSlots} from 'vue';

//布局盒子，格子
const props = defineProps({
    param: { type:Object },
    //布局类型： lr--左右布局，tb--上下布局，ltb--左上下布局，tbr--上下右布局，lmr--左中右布局
    type: {type: String, default:""   },
});


</script>
<style lang="">
    
</style>