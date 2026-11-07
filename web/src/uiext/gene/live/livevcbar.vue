<template>

<component :is="vcBody" :bpo="bpo"
    :openLayer="openLayer"
    :closeLayer="closeLayer"
    :position="position" ref="vcbar"
    v-bind="attrs"
></component>

</template>

<script setup>

import { bpocomponentMap, vccomponentMap, vcAndBpoComponentPathMap } from '/@/views/componentMap';
import { reactive, computed, nextTick, ref,useAttrs, onMounted,defineAsyncComponent,watch } from 'vue';

import { loadUcmlVCTool} from "./dynamicImport.js"

const props = defineProps({
    name: null, //上级BPO
    bpo: null,
    type:{ type:String,default:"vc" },
    position:{ type:String,default:"tabPage" },
    disabled:{ type:Boolean,default:false },
    target:{ type:[Array,Number],default:-1 },
    hides:{ type:[Array,Number],default:function(){ return []; } },
});
const attrs=useAttrs();

const vcbar= ref(null);

const vcBody= computed(()=>{
    var bodyName=props.name;
    //var vcComp = vccomponentMap.get(bodyName);

    var t= loadUcmlVCTool(bodyName);

    if(typeof t=="function"){
        return defineAsyncComponent( t);
    }
    return t;

});

const openLayer= computed(()=>{
    //console.log("dd")
    if(!!props.bpo.vmpan){
        return props.bpo.vmpan.exposed
.openLayer;
    }
    var bpoVM= props.bpo.currentInstance;
    if(!!bpoVM.$root){
        return bpoVM.$root.openLayer;
    }
    return null;
})
const closeLayer= computed(()=>{
    if(!!props.bpo.vmpan){
        return props.bpo.vmpan.closeLayer;
    }
    var bpoVM= props.bpo.currentInstance;
    return null;
})

onMounted(()=>{
    //console.log(vm)
    //绘制进行BPO注册
    var nameAll="Toolbar"+props.name;
    if (props.bpo.viewComponents[nameAll] == undefined) {
      props.bpo.viewComponents[nameAll] = vcbar;
    }

    nextTick(()=>{
        waitBarInited();
    })
})

watch(()=>props.hides,()=>{
    waitBarInited();
})
watch(()=>props.target,()=>{
    waitBarInited();
})
watch(()=>props.disabled,()=>{
    waitBarInited();
})

function waitBarInited(){
    if(vcbar.value==void 0||vcbar.value==null){
        setTimeout(() => {
            waitBarInited()
        }, 100);
        return ;
    }

    if(props.disabled){
        if(Array.isArray(props.target)){
            props.target.forEach((t)=>{
                vcbar.value.disable(t);
            })
        }
        else{
            vcbar.value.disable(props.target);
        }
        
    }
    else if(typeof vcbar.value.enable=="function"){
        vcbar.value.enable();
    }
    if(props.hides.length>0){

        vcbar.value.hides(props.hides);
        // props.hides.forEach((t)=>{
        //     vcbar.value.hide(t);
        // })
    }
    if(vcbar.value !=void 0){
        //console.log(vcbar.value)
    }
}

</script>
