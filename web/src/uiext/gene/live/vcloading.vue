<template>
    <div :style="mstyle">
        <el-skeleton :rows="rows" />
    </div>
</template>
    
<script setup>

import { computed } from 'vue'

defineOptions({
    inheritAttrs: false,
});

const props = defineProps({
    loadingConfig: { type:Object, },

})

const mstyle= computed(()=>{
    var c=props.loadingConfig||{};
    return {
        width:c.width,
        height:c.height,
    }
})

const rows= computed(()=>{
    try{
        return props.loadingConfig.rows;
    }
    catch(e){
        return 6;
    }
})
</script>
