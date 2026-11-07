<template>
<div :class="className" :style="cellStyle" ref="porlet" >
    <div class="portlet-content" :style="contentStyle" >
       <el-icon v-if="cell.para.fullscreen" size="13" style=" float:right; bottom: -2px; color: darkseagreen ;" @click="handleFullscreen"><FullScreen /></el-icon>
        <emptycell v-if="cell.vclist.length == 0" :cell="cell" :height="height" :width="width"></emptycell>
        <vcholder v-else-if="cell.vclist.length == 1" ref="portlet-0" :vc="cell.vclist[0]" :cell="cell" pose="solo" :height="height" :width="width"
            :root="root"
            :para="para" :shareState="shareState"
            :paotou="paotou"
            @setState="$emit('setState',$event)"
            @changePara="$emit('changePara',$event)"
        ></vcholder>
        <pan v-else
            :label="portalShowTitle ? cell.name : ''"
            :theme="cell.theme"
            :position="portalShowTitle ? cell.titleAlign : '0'"
            :style="{height:appletHeight+'px',width:appletWidth+'px'}"
        >
            <template #default="panp">
                <manyplay  :mode="cell.mode"
                    :vclist="cell.vclist"
                    :virtical="virtical"
                    :width="panp.width"
                    :height="panp.height"
                    :position="cell.tabPosition"
                    :stretch="cell.tabStretch"
                >
                    <template #default="p">
                        <vcholder :ref="'portlet-' + p.index" :vc="p.vc" :cell="cell" pose="tab"  :root="root"
                            :para="para" :shareState="shareState"  :width="p.width" :height="p.height"
                            :paotou="paotou"
                            @setState="$emit('setState',$event)"
                            @changePara="$emit('changePara',$event)"
                        ></vcholder>
                    </template>
                </manyplay>
            </template>

        </pan>

    </div>
    <slot></slot>
</div>
</template>

<script>
import vcholder from "./vcholder.vue"
import emptycell from "./cell/emptycell.vue"
import { styleStrToObj } from "@/utils/style.js"
import {FullScreen } from "@element-plus/icons-vue";
import manyplay from "./part/manyplay.vue"
import pan from "./part/pan.vue"
//import useFullscreen from '@/utils/hooks/fullscreen/useFullscreen.js'

import { parUrlPara } from "../util.js"

export default {
    name: "portlet",
    components: {
        vcholder,emptycell,manyplay,pan,FullScreen
    },
    emits:["setState","changePara"],
    props: {
        sz:null,
        index: {
            type: Number,
            default: 0
        },
        applet: {
            type: Object,
            default: () => {
                return {}
            }
        },
        portletStyle: {
            type: Object,
            default: () => {
                return {}
            },
        },
        para:null,
        shareState:null,
        root:null,
        paotou:null,//门户配置主表对象
    },
    data() {
        return {
            activeTab: "vc0",
            timer: null,
            height:0,
            width:0,
            sizeListener:null,
        }
    },
    created() {
        // this.portletOption = this.option;
    },

    computed: {
        cell() {
            var tar = this.applet;
            if(tar.vclist ==void 0){
                tar.vclist=[];
            }
            if(tar.PC_CellPara !=void 0){
                tar.para = parUrlPara(tar.PC_CellPara);
            }
            else{
                tar.para = {};
            }
            return tar;
        },
        currentPortlet() {
            return this.$refs['portlet-' + this.activeTab]
        },
        contentStyle() {

        },
        cellStyle(){
            var tar=this.cell.style;
            var obj=styleStrToObj(tar);
            return obj;
        },
        portletStyles() {
            return 'border: ' + this.portletStyle.borderWidth + 'px ' + this.portletStyle.borderColor + ' ' + this.portletStyle.borderStyle + ';' + this.portletStyle.other;
        },
        className() {
            return 'port-applet theme' + this.cell.theme+" "+this.cell.css ;
        },
        showSetting() {
            return this.currentPortlet.showSetting || false;
        },
        labelPostion(){

            var map={
                '0':'none',
                '1':'top',
                '2':'left',
                '3':'right',
                '4':'bottom'
            }
            var tar=map[this.applet.titleAlign]
            if(tar != void 0){
                return tar;
            }
            return "top";
        },
        thee(){
            return this.applet.theme;
        },
        mytabcss(){
            //content.isTab != false || content.isCarousel == false? 'portal-content-tab' : 'portal-content-tab portal-content-notab'
            var res=["vc-tab","applet-tab"];
            res.push("thee"+this.thee);
            res.push("pos-"+this.applet.titleAlign);
            return res;
        },
        virtical(){
            var t=this.applet.titleAlign;
            if(t=="2"||t=="3"){
                return true;
            }
            return false;
        },
        tabed(){
            if(this.cell.vclist.length < 2) return false;
            if(this.cell.mode=='4' ||this.cell.mode=='1' ){
                return true;
            }
            return false;
        },
        appletWidth(){
            // if(this.virtical==false && this.tabed){
            //     return this.width-40;
            // }
            return this.width;
        },
        appletHeight(){
            // if(this.virtical && this.tabed){
            //     return this.height-40;
            // }
            return this.height;
        },
        portalShowTitle(){
            const showTitle = this.paotou?.P_ShowTitle ?? this.paotou?.p_ShowTitle;
            if(showTitle === undefined || showTitle === null){
                return true;
            }
            return !(showTitle === 0 || showTitle === '0' || showTitle === false || showTitle === 'false');
        },
    },
    mounted() {
        if (this.cell.refreshTimer) {
            if (this.cell.refreshInterval) {
                this.timer = setInterval(this.refreshContent, this.cell.refreshInterval)
            } else {
                this.timer = setInterval(this.refreshContent, 60000)
            }
        }
        this.lestenResize();
    },
    beforeDestroy() {
        clearInterval(this.timer)
        this.timer = null;
        if(this.sizeListener!=null){
            this.sizeListener.disconnect();
        }
    },
    methods: {

        lestenResize() {
            var vm = this;
            var objResizeObserver = new ResizeObserver(function (entries) {
                vm.$nextTick(()=>{
                    vm.readSize();
                });
            });
            // 观察图片元素
            objResizeObserver.observe(this.$el);
            this.sizeListener=objResizeObserver;
        },
        readSize(){
            this.height=this.$el.clientHeight;
            this.width=this.$el.clientWidth;
        },
        titleClick() {
            if (Array.isArray(this.currentPortlet)) {
                this.currentPortlet[0].titleClick()
            } else {
                this.currentPortlet.titleClick()
            }
        },

        handleCarouselChange(index) {
            this.activeTab = index + '';
        },
        handleTabChange(tab) {
            this.$refs.carousel.setActiveItem(tab.index);
        },
        refreshContent() {
            if (Array.isArray(this.currentPortlet)) {
                this.currentPortlet[0].reload = false;
                this.$nextTick(() => {
                    this.currentPortlet[0].reload = true;
                })
            } else {
                this.currentPortlet.reload = false;
                this.$nextTick(() => {
                    this.currentPortlet.reload = true;
                })
            }
        },
      handleFullscreen(){
        const {toggleFullscreen} = useFullscreen();
        toggleFullscreen(this.$refs.porlet)
      }

    },

};
/**
<style>
.thee2.pos-2 .el-tabs__item{
    height: auto;
    padding: 0;
    width: 40px;
    justify-content: center;
    --tw-bg-opacity: 1;
    background-color: rgb(30 136 229 / var(--tw-bg-opacity));
}
.thee2.pos-2 div.el-tabs__item.is-left:last-child{
    padding-right:0;
 }
 .thee2.pos-2 div.el-tabs__item.is-left:nth-child(2){
    padding-left:0;
 }

 .thee2.pos-2 .el-tabs__nav{

 }
</style>
.vctitle.ver{
    display: flex;
    flex-direction: column;
}

 */

</script>

<style lang="scss" scoped>



.port-applet{
    height:100%;
}

.port-applet.theme1{
    background:#fff;
    border:1px solid #eee;
    border-radius:10px;
}


.portlet-content{
    height:100%;
}

.portal-content-tab {
    height: 100%;
    overflow: hidden;

    :deep(.el-tabs__nav-wrap::after) {
        background-color: #ebf3fa;
    }

    :deep(.el-tabs__header) {
        margin: 0;
    }

    :deep(.el-tabs__item) {

    }

    :deep(.el-tabs__nav.is-top) {
        padding: 0;
        position: relative;
        margin: 0 0 0 0px;
    }

    :deep(.el-tab-pane) {
        height: 100%;
    }

    :deep(.el-tabs__content) {
        height: calc(100% - 40px);
    }

    :deep(.el-carousel) {
        height: 100%;
    }

    :deep(.el-carousel__container) {
        height: 100%;
    }

    :deep(.el-tabs__nav-scroll) {
        overflow: hidden;
        margin: 0px 10px 0px 10px;
    }
}

.portal-content-notab {
    :deep(.el-tabs__content) {
        height: 100% !important;
    }
}
</style>
