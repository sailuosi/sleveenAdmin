<template>

<component :is="vcBody" :bpo="bpo"
    ref="bpoauto"
    v-bind="attrs"
    :param="para"
></component>

</template>

<script setup>
;
import { reactive, computed, nextTick, ref, onMounted,defineAsyncComponent ,getCurrentInstance,useAttrs} from 'vue';

import { loadUcmlBPO} from "./dynamicImport.js"

const props = defineProps({
    name: null, //上级BPO
    bpo: null,
    type:{ type:String,default:"bpo" },
});

const attrs=useAttrs();

const para= computed(()=>{
    if(attrs.param!= void 0){
        return attrs.param;
    }
    var res={};
    Object.keys(attrs).forEach((k)=>{
        res[k]=attrs[k];
    })
    return res;
});

const bpoauto= ref(null);

const vcBody= computed(()=>{
    var bodyName=props.name;
    //var vcComp = vccomponentMap.get(bodyName);

    var t= loadUcmlBPO(bodyName);
    //console.log(t)
    if(typeof t=="function"){
        return defineAsyncComponent( t);
    }
    return t;

});

const vm=getCurrentInstance();

onMounted(()=>{
    //console.log(vm)

})

// 暴露 bpoauto ref 供外部访问
defineExpose({
    bpoauto
});
</script>
