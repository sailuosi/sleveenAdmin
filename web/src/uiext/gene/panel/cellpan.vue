<template lang="">
    <div :class="['cellpan',type,bodyFlex?'fxbd':'atbd',flex?'flexed':'noao']" :style="panStyle">


        <slot name="header"></slot>
        <template v-if="type=='lr'">
            <div class="lr-left cp-body" >
                <slot name="left"></slot>
            </div>
            <div class="lr-right cp-body" >
                <slot name="right"></slot>
            </div>
        </template>
        <template v-else-if="type=='tb'">

            <div class="tb-top cp-body" >
                <slot name="top"></slot>
            </div>
            <div class="tb-bottom cp-body" >
                <slot name="bottom"></slot>
            </div>
        </template>
        <template v-else-if="type=='ltb'">

            <div class="ltb-left cp-body">
                <slot name="left"></slot>
            </div>
            <div class="ltb-top cp-body">
                <slot name="top"></slot>
            </div>
            <div class="ltb-bottom cp-body">
                <slot name="bottom"></slot>
            </div>
        </template>
        <template v-else-if="type=='tbr'">
            <div class="tbr-top cp-body">
                <slot name="top"></slot>
            </div>
            <div class="tbr-bottom cp-body">
                <slot name="bottom"></slot>
            </div>
            <div class="tbr-right cp-body">
                <slot name="right"></slot>
            </div>
        </template>
        <template v-else-if="type=='lmr'">
            <div class="lmr-left cp-body">
                <slot name="left"></slot>
            </div>
            <div class="lmr-middle cp-body">
                <slot name="middle"></slot>
            </div>
            <div class="lmr-right cp-body">
                <slot name="right"></slot>
            </div>
        </template>
        <template v-else>
            <slot name="free"></slot>
        </template>
        <slot></slot>
    </div>
</template>
<script setup>

import { reactive, computed, nextTick, ref, defineComponent,onMounted,defineAsyncComponent ,getCurrentInstance,useAttrs,useSlots} from 'vue';

//布局盒子，格子
const props = defineProps({
    param: { type:Object },
    //布局类型： lr--左右布局，tb--上下布局，ltb--左上下布局，tbr--上下右布局，lmr--左中右布局,one--单格
    type: {type: String, default:""   },
    //盒子的尺寸参数，按盒子类型提供，比如left:1fr,right:1fr
    left: {type: String, default:"1fr"   },
    right: {type: String, default:"1fr"   },
    top: {type: String, default:"1fr"   },
    bottom: {type: String, default:"1fr"   },
    middle: {type: String, default:"1fr"   },
    flex: { type: Boolean, default: false },//是否表体以flex撑开
    bodyFlex: { type: Boolean, default: false },//是否表体以flex撑

});

const panStyle= computed(()=>{
    var res={};
    var t= props.type;
    if(t=="lr"){
        res.display="grid";
        res["grid-template-columns"]= props.left+" "+props.right;
    }
    else if(t=="tb"){
        res.display="grid";
        res["grid-template-rows"]= props.top+" "+props.bottom;
    }
    else if(t=="ltb"){
        res.display="grid";
        res["grid-template-columns"]= props.left+" "+props.right;
        res["grid-template-rows"]= props.top+" "+props.bottom;
    }
    else if(t=="tbr"){
        res.display="grid";
        res["grid-template-columns"]= props.left+" "+props.right;
        res["grid-template-rows"]= props.top+" "+props.bottom;
    }
    else if(t=="lmr"){
        res.display="grid";
        res["grid-template-columns"]= props.left+" "+props.middle+" "+props.right;
    }
    return res;
});

</script>
<style lang="css" scoped>
.cellpan.flexed{
    flex: 1 1 0;
    height: 0;
}
.fxbd.cellpan>.cp-body:deep(>div){
    flex: 1 1 0;
    height: 0;
}
.cellpan>div{
    overflow: auto;
    min-width: 0;
}

.cellpan.lr{
    display: flex;
    flex-direction: row;
    align-items: center;
    justify-content: center;
    align-items: stretch;
}

.ltb-left{
    grid-row: 1/3;
}
.tbr-right{
    grid-row: 3/5;
}

.cp-body{
    display: flex;
    flex-direction: column;

}
.cp-body:deep(>div){
    flex:1 1 0;

}

.cellpan.one.fxbd:deep(>div){
    flex:1 1 0;
    width: 100%;
}
</style>