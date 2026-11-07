<template>

<component :is="vcBody" :bpo="bpo"
    ref="vcauto"
    v-bind="attrs"
    :loadingConfig="loadingOption"
>
    <template #[it]="data" v-for="it in Object.keys(slots)">
      <slot :name="it" v-bind="data || {}"></slot>
    </template>
</component>

</template>

<script setup>
/**
    <template v-for="(li) in slotNames">
        <template v-slot:[li]>
            <slot :name="li" ></slot>
        </template>
    </template>
    <template #[it]="data" v-for="it in Object.keys(slots).filter((p) => p != 'default')">
      <slot :name="it" v-bind="data || {}"></slot>
    </template>
 */

import { reactive, computed, nextTick, ref, defineComponent,onMounted,defineAsyncComponent ,getCurrentInstance,useAttrs,useSlots} from 'vue';

import { loadUcmlVC} from "./dynamicImport.js"
import vcloading from "./vcloading.vue"

const props = defineProps({
    name: null, //上级BPO
    bpo: null,
    type:{ type:String,default:"vc" },
    lazy: { type: Boolean, default: false },
    loadingOption: { type: Object,  },
});

const attrs=useAttrs();
const emit = defineEmits(['update:loading', 'fail','loaded'])

const vcauto= ref(null);

const loading= ref(true);

const moreOpt= computed(()=>{
    if(loading.value){
        return { loadingConfig: props.loadingOption};
    }
    return attrs;
})

const vcBody= computed(()=>{
    var bodyName=props.name;
    //var vcComp = vccomponentMap.get(bodyName);

    var t= loadUcmlVC(bodyName);
    //console.log(t)
    if(typeof t=="function"){

        return defineAsyncComponent( {
            loader:function(){
                return new Promise((onok,onno)=>{
                    t().then((res)=>{
                        emit("loaded");
                        emit("update:loading",false)
                        onok(res);
                        //loading.value=false;
                        //onok(vcloading);
                    })
                    .catch((err)=>{
                        emit("fail");
                        emit("update:loading",false)
                        onno(err);
                        //loading.value=false;
                    })
                })
            },
            loadingComponent:vcloading

        });


    }
    return t;

});

const vm=getCurrentInstance();
const slots = useSlots()

const slotNames=computed(()=>{
    return Object.keys(slots);
})

onMounted(()=>{
    //console.log(vm)
    //绘制进行BPO注册

})

defineExpose({
    vcauto
});

</script>
