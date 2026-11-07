<template>
<div ref="myrootpan">
    <slot v-if="disabled" :open="openIt" :close="closeIt">
    </slot>
    <el-popover v-else-if="type=='popover'" :placement="placement" :title="title" :width="width" :trigger="trigger" :visible="opened" content="" 
        :popper-style="popStyle" :disabled="disabled"
    >
        <template #default="p">
            <div ref="pandiv" class="poppan" :style="bodyStyle">
                <slot name="header" :open="openIt" :close="closeIt"></slot>
                <slot name="body" :open="openIt" :close="closeIt"></slot>
                <slot name="footer" :open="openIt" :close="closeIt"></slot>
            </div>
        </template>
        <template #reference>
            <slot :open="openIt" :close="closeIt">
            </slot>
        </template>
    </el-popover>
    <div v-else-if="type=='dialog'">
        <slot :open="openIt" :close="closeIt">
        </slot>
        <!-- append-to-body：嵌套在外层 el-dialog（如班级表单）内时，避免焦点陷阱导致弹内输入框无法键入 -->
        <el-dialog v-model="opened" title="title" :width="width"
            append-to-body
            destroy-on-close
            :closeOnClickModal="closeOnClickModal"
        >
            <template #header="diah">
                <slot name="header" :open="openIt" :close="closeIt"><span>{{title}}</span></slot>
            </template>
            <div class="anyopen-dialog-body" :style="dialogBodyStyle">
                <slot name="body" :open="openIt" :close="closeIt"></slot>
            </div>
            <template #footer>
                <slot name="footer" :open="openIt" :close="closeIt"></slot>
            </template>
        </el-dialog>
    </div>
    <div  v-else-if="type=='drawer'">
        <slot :open="openIt" :close="closeIt">
        </slot>
        <el-drawer v-model="opened" :direction="direction" :size="width"
            append-to-body
            destroy-on-close
            :closeOnClickModal="closeOnClickModal"
        >
            <template #header>
                <slot name="header" :open="openIt" :close="closeIt"><span>{{title}}</span></slot>
            </template>
            <template #default>
                <slot name="body" :open="openIt" :close="closeIt"></slot>
            </template>
            <template #footer>
                <slot name="footer" :open="openIt" :close="closeIt"></slot>
            </template>
        </el-drawer>
    </div>
    <div v-else>
        <slot :open="openIt" :close="closeIt">
        </slot>
        <dialog  ref="protodia" >
            <div>
                <slot name="header" :open="openIt" :close="closeIt"><span>{{title}}</span></slot>
                <slot name="body" :open="openIt" :close="closeIt"></slot>
                <slot name="footer" :open="openIt" :close="closeIt"></slot>
            </div>
        </dialog>
    </div>
</div>
</template>

<script setup>
/** 使用案例
 <anyopen>
    <template #default="dp">
        <slot></slot>
    </template>
    <template #header="hp">
        <slot name="header"></slot>
    </template>
    <template #body="bp">
        <slot name="body"></slot>
    </template>
    <template #footer="fp">
        <slot name="footer"></slot>
    </template>
</anyopen>
 */


import { reactive, computed, nextTick, ref, onMounted  } from 'vue';

const props = defineProps({
    // popover  drawer  base dialog
    type: {type: String,default: "dialog"},
    height: {
        type: String,
        default: ""
    },
    maxHeight: {
        type: String,
        default: "90vh"
    },
    scroll: {
        type: String,
        default: "auto"
    },

    trigger: {
        type: String,
        default: "click"
    },
    show: {
        type: Boolean,
        default: false,
    },
    placement: {
        type: String,
        default: "auto"
    },
    title: {
        type: String,
        default: ""
    },
    width: {
        type: String,
        default: "auto"
    },
		zIndex: {
        type: [String,Number],
        default: null
    },
    top:{},
    direction:null,
    closeOnClickModal:{type:Boolean,default:true},
    disabled: { type: Boolean, default: false },
})

const pandiv= ref();
const myrootpan=ref();

const vClickout = {
    mounted(el, binding) {
        el.clickOutsideEvent = (event) => {
            // 检查点击是否发生在el之外
            if (!(el ).contains(event.target )) {
                binding.value(event);
            }
        };
        document.addEventListener('click', el.clickOutsideEvent);
    },
    unmounted(el) {
        document.removeEventListener('click', el.clickOutsideEvent);
        delete el.clickOutsideEvent;
    },
}

const opened= ref(false);
const protodia= ref();

const mode= computed(()=>{
    if(props.type){
        return props.type;
    }
    return "base"
})

const bodyStyle= computed(()=>{
    var res={};
    if(props.height){
        res.height= props.height;
    }
    return res;
});

/** dialog 模式：将 mheight 落到 body 容器，避免子组件 height:100% 塌陷为 0 */
const dialogBodyStyle = computed(() => {
    // 未传高度时给默认值，否则嵌套 BPO（height:100%）会塌成一条细条
    const h = props.height || "70vh";
    return {
        height: h,
        minHeight: h,
        maxHeight: props.maxHeight || h,
        overflow: "hidden",
        display: "flex",
        flexDirection: "column",
    };
});

const popStyle= computed(()=>{
    var res={};
    if(props.top){
        res.top=props.top;
    }
		if(props.zIndex){
			res.zIndex=props.zIndex
		}
		
    return res;
})

function modalClose(){
    if(props.closeOnClickModal){
        closeIt();
    }
}


function startCheckOutClick(){
    document.addEventListener('click', onOutClick);
}
function onOutClick(e){
    //console.log(arguments);
    var tar= e.target;
    if(!!pandiv.value && (pandiv.value.contains(tar)) || tar.contains(pandiv.value)||(myrootpan.value!=null && myrootpan.value.contains(tar))){
        //console.log("外部点击！")
        return;
    }
    closeIt();
}



function openIt(){
    if(props.disabled){
        // 禁用
        console.log('[DEBUG anyopen] openIt: 已禁用，忽略');
        return;
    }

    if(opened.value){
        //已打开；
        console.log('[DEBUG anyopen] openIt: 已打开，忽略');
        return;
    }
    console.log('[DEBUG anyopen] openIt: 打开弹窗, type=', props.type, ', title=', props.title);
    //console.log("callOpen")
    if(mode.value !="base"){
        opened.value=true;
    }
    else{
        protodia.value.showModal();
    }
    if(props.closeOnClickModal&& ( mode.value=="popover"||mode.value=="base")){
        setTimeout(()=>{
            startCheckOutClick();
        },300)
    }
    
}

function closeIt(){
    //console.log("cloaas")
    if(mode.value !="base"){
        opened.value=false;
    }
    else{
        protodia.value.hidden = true;
    }
    if(props.closeOnClickModal&& ( mode.value=="popover"||mode.value=="base")){
        document.removeEventListener('click', onOutClick);
    }
}

defineExpose({
    open:openIt,
    close:closeIt,
    opened:opened,
    mode:mode,
})

</script>

<style scoped>
.poppan{
    max-height:calc(100vh - 50px);
    overflow:auto;
}
</style>