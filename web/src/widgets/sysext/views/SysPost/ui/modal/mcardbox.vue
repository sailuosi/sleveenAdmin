<template>
<div :class="['po-'+position,'cardbox-pan', 'bg-white',theme,fixed?'fixed':'autofw']" :style="panstyle" ref="groot">
    <div class="cd-label bg-header" :style="BoxlabelStyle" :class="labelClass" v-if="position!='none'">
        <template v-if="useLabel">
            <slot name="prelabel" ></slot>
            <div v-for="(item, index) in rendLabels" :key="index">{{ item }}</div>
            <slot name="label" :label="rendLabels"></slot>
        </template>
        <slot v-else name="label" :label="rendLabels" >
            <div v-for="(item, index) in rendLabels" :key="index">{{ item }}</div>
        </slot>

    </div>
    <div v-if="!lazy" class="cd-box bg-white" :style="BoxBodyStyle" ref="fbody">
        <slot :height="bodyHeight" :panelHeight="panHeight" :width="bodyWidth"></slot>
    </div>
    <lazydiv v-if="lazy" class="cd-box bg-white" :style="BoxBodyStyle" ref="lazybody">
        <slot :height="bodyHeight" :panelHeight="panHeight" :width="bodyWidth"></slot>
    </lazydiv>
</div>
</template>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount } from 'vue';
import { styleStrToObj } from '@/utils/style.js';
import lazydiv from '@/ui/panels/lazydiv.vue';

defineOptions({ name: 'cardBox' });

const props = defineProps({
    position: { type: String, default: 'top' },
    label: { type: [Array, String], default: '' },
    //指内容体的高度，标题高度为固定。
    height: null,
    minHeight: null,
    maxHeight: null,
    useLabel: { type: Boolean, default: false },
    labelClass: { type: String, default: '' },
    bodyPadding: { type: String, default: '10px' },
    labelStyle: null,
    bodyStyle: null,
    lazy: { type: Boolean, default: false }, //是否懒加载
    // fblue 全色圆角风格 ,fgray 灰色圆角风格
    theme: { type: String, default: 'fblue' },
});

const fixed= computed(() => {
    return !!props.height;
});

const groot = ref(null);
const fbody = ref(null);
const lazybody = ref(null);

const panHeight = ref(null); //容器整体高度
const bodyHeight = ref(null); //主体容器的高度
const bodyWidth = ref(null);
const bodyRect = ref(null);
const listen = ref(null);

const panstyle = computed(() => {
    const obj = {};
    // var map={ top:"column", bottom:"column-reverse", left:"row", right:"row-reverse", }
    //obj["flex-direction"]=map[props.position];
    return obj;
});

const rendLabels = computed(() => {
    let tar = [];
    if (!!props.label && typeof props.label == 'string') {
        tar = props.label.split('');
    }
    if (Array.isArray(props.label)) {
        tar = props.label;
    }
    return tar;
});

const BoxlabelStyle = computed(() => {
    const obj = styleStrToObj(props.labelStyle);
    if (props.position == 'top' || props.position == 'bottom') {
        obj['flex-direction'] = 'row';
    }
    if (props.position == 'left' || props.position == 'right') {
        obj['flex-direction'] = 'column';
    }
    return obj;
});

const BoxBodyStyle = computed(() => {
    const t = styleStrToObj(props.bodyStyle);
    const init = {
        padding: props.bodyPadding,
    };
    if (!!props.height) {
        init.height = props.height;
        init['flex-basis'] = props.height;
    }
    if (!!props.minHeight) {
        init['min-height'] = props.minHeight;
    }
    if (!!props.maxHeight) {
        init['max-height'] = props.maxHeight;
    }
    return Object.assign(init, t);
});

function loadHeightInfo() {
    if (groot.value != void 0) {
        var sh = groot.value.clientHeight;
        if (sh != panHeight.value) {
            if(props.theme == 'fgray'){
                sh -= 32;
            }
            panHeight.value = sh;
        }
    }
    if (fbody.value != void 0) {
        var sh = fbody.value.clientHeight;
        const wh = fbody.value.clientWidth;
        bodyRect.value = fbody.value.getBoundingClientRect();
        if (sh != bodyHeight.value) {
            if(props.theme == 'fgray'){
                sh -= 32;
            }
            bodyHeight.value = sh;
        }
        if (wh != bodyWidth.value) {
            bodyWidth.value = wh;
        }
    }
    if (lazybody.value != void 0) {
        const el = lazybody.value.getEl();
        const sh = el.clientHeight;
        const wh = el.clientWidth;
        bodyRect.value = el.getBoundingClientRect();
        if (sh != bodyHeight.value) {
            bodyHeight.value = sh;
        }
        if (wh != bodyWidth.value) {
            bodyWidth.value = wh;
        }
    }
    //
}

function initListen() {
    listen.value = new ResizeObserver(() => {
        loadHeightInfo();
    });
    listen.value.observe(groot.value, {
        attributes: true,
        attributeFilter: ['style'],
        attributeOldValue: true,
    });
}

onMounted(() => {
    initListen();
    loadHeightInfo();
});

onBeforeUnmount(() => {
    if (listen.value) {
        listen.value.disconnect();
        //listen.value.takeRecords();
        listen.value = null;
    }
});
</script>
<style lang="scss">
.cardbox-pan {
    .el-tabs--left, .el-tabs--right {
        overflow: auto; 
    }
}
</style>
<style scoped>
/* 根据需要添加样式 */
.fblue>.bg-header{
    background-color: #4799e7;
}
.fgray>.bg-header{
    padding: 10px 12px !important;
    background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%) !important;
    border: 1px solid var(--el-border-color-lighter) !important;
    border-radius: 12px !important;
}
.cardbox-pan.fgray{
    padding:16px;
}
.cardbox-pan {
    display: flex;

    /* background: #FAFAFA; */
    border-radius: 10px;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.2);
}
.cardbox-pan.po-left{
    flex-direction: row;
}
.cardbox-pan.po-right{
    flex-direction: row-reverse;
}
.cardbox-pan.po-top{
    flex-direction: column;
}
.cardbox-pan.po-bottom{
    flex-direction: column-reverse;
}



.cd-label {
    flex:0 0 40px;
    color: #fff;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    position:relative;
}

.cardbox-pan.po-left>.cd-label{
    flex-direction: column;
    border-top-left-radius: 10px;
    border-bottom-left-radius: 10px;
}
.cardbox-pan.po-right>.cd-label{
    flex-direction: column;
    border-top-right-radius: 10px;
    border-bottom-right-radius: 10px;
}
.cardbox-pan.po-top>.cd-label{
    flex-direction: row;
    border-top-left-radius: 10px;
    border-top-right-radius: 10px;
    justify-content: start;
    padding-left: 32px;
}
.cardbox-pan.po-bottom>.cd-label{
    flex-direction: row;
    border-bottom-left-radius: 10px;
    border-bottom-right-radius: 10px;
}

.fixed .cd-box {
    flex:1;
    overflow: hidden;
    display: flex;
    flex-direction: column;
}
</style>
