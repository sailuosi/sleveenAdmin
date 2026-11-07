<template>
<div :class="['po-'+position,'cardbox-pan', 'bg-white']" :style="panstyle" ref="groot">
    <div class="cd-label bg-primary" :style="BoxlabelStyle" :class="labelClass" v-if="position!='none'">
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

<script>
import { styleStrToObj } from "@/utils/style.js"
import lazydiv from "./lazydiv.vue"

export default {
    name: "cardBox",
    components:{
        lazydiv
    },
    props: {
        position: {type: String,default:"top",        },
        label: {type: [Array,String],default:""},
        //指内容体的高度，标题高度为固定。
        height:null,
        minHeight:null,
        maxHeight:null,
        useLabel:{ type:Boolean,default:false },
        labelClass: {type: String, default: ""},
        bodyPadding:{type: String, default: "10px"},
        labelStyle:null,
        bodyStyle:null,
        lazy:{ type:Boolean,default:false },//是否懒加载
    },
    data(){
        return {
            panHeight:null,//容器整体高度
            bodyHeight:null,//主体容器的高度
            bodyWidth:null,
            bodyRect:null,
            listen:null,
        }
    },
    computed:{
        panstyle(){
            var obj={};
            var map={
                top:"column",
                bottom:"column-reverse",
                left:"row",
                right:"row-reverse",
            }
            //obj["flex-direction"]=map[this.position];

            return obj;
        },
        rendLabels(){
            var tar=[];
            if(!!this.label && typeof this.label=="string"){
                tar=this.label.split("");
            }
            if(Array.isArray(this.label)){
                tar= this.label;
            }

            return tar;
        },
        BoxlabelStyle(){
            var obj=styleStrToObj(this.labelStyle);
            if(this.position=="top"|| this.position=="bottom"){
                obj["flex-direction"]="row";
            }
            if(this.position=="left"|| this.position=="right"){
                obj["flex-direction"]="column";
            }
            return obj;
        },
        BoxBodyStyle(){
            var t=styleStrToObj(this.bodyStyle);
            var init={
                padding:this.bodyPadding,
            }
            if(!!this.height){
                init.height=this.height;
                init["flex-basis"]=this.height;
            }
            if(!!this.minHeight){
                init["min-height"]=this.minHeight;
            }
            if(!!this.maxHeight){
                init["max-height"]=this.maxHeight;
            }
            
            return Object.assign(init,t);
        },
    },
    mounted(){
        this.initListen();
        this.loadHeightInfo();
    },
    updated(){
        //this.loadHeightInfo();
    },
    beforeDestroyed () {
        if (this.listen) {
            //console.log("destroy窗口监听");
            this.listen.disconnect()
            this.listen.takeRecords()
            this.listen = null
        }
    },
    methods:{
        loadHeightInfo(){
            if(this.$refs.groot != void 0){
                var sh=this.$refs.groot.clientHeight;
                if(sh!=this.panHeight){
                    this.panHeight= sh;
                }
            }
            if(this.$refs.fbody != void 0){
                var sh=this.$refs.fbody.clientHeight;
                var wh=this.$refs.fbody.clientWidth;
                this.bodyRect=this.$refs.fbody.getBoundingClientRect();
                if(sh!=this.bodyHeight){
                    this.bodyHeight= sh;
                }
                if(wh!=this.bodyWidth){
                    this.bodyWidth=wh;
                }
            }
            if(this.$refs.lazybody != void 0){
                var el=this.$refs.lazybody.getEl();
                var sh=el.clientHeight;
                var wh=el.clientWidth;
                this.bodyRect=el.getBoundingClientRect();
                if(sh!=this.bodyHeight){
                    this.bodyHeight= sh;
                }
                if(wh!=this.bodyWidth){
                    this.bodyWidth=wh;
                }
            }
            //
        },
        initListen(){
            //console.log("init窗口监听");
            //let MutationObserver = window.MutationObserver || window.WebKitMutationObserver || window.MozMutationObserver;
            this.listen = new ResizeObserver((list)=>{
                //console.log("窗口监听感知变化",list);
                this.loadHeightInfo();
            });
            this.listen.observe(this.$el,{attributes:true,attributeFilter:['style'],attributeOldValue:true});
            //ffoot

        },

    }
}
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
}
.cardbox-pan.po-bottom>.cd-label{
    flex-direction: row;
    border-bottom-left-radius: 10px;
    border-bottom-right-radius: 10px;
}

.cd-box {
    flex:1;
    overflow: hidden;
    display: flex;
    flex-direction: column;
}
</style>
