<template>
    <pan :theme="thee"
        :label="rendLabel ? (vc?.title || '') : ''" class="vc-holder" flex 
        :style="{height:height+'px'}" 
        :position="panPositionProp"  
        labelClass="portal-bar"
        :bodyStyle="vc.style" :labelStyle="vc.titleStyle"
        :lazy="islazy"
        >
        <template #label>
            <portbar v-if="rendLabel" :vc="vc" @titleClick="titleClick" @addClick="addClick" @moreClick="moreClick" @refreshClick="refreshClick" >
                <span v-if="rendLabel" class="vc-title" :style="titleStyle" :class="'theme'+vc.theme">
                    <i :class="vc.icon" class="pb-icon"></i>
                    <span>{{vc.title}}</span>
                </span>
            </portbar>
        </template>
        <template #default="p">
            <applet v-if="ctype=='1'" :name="vcomponent" 
                :vc="vc" :cell="cell" class="vc-body" :height="p.height" :width="p.width" ref="livevc"
                :para="para" :shareState="shareState" :applet="applet"
                :paotou="paotou"
                @setState="$emit('setState',$event)"
                @changePara="$emit('changePara',$event)"
            ></applet>
            <urlapplet v-else-if="ctype=='2'" :src="ckey"></urlapplet>
            <div v-else-if="ctype=='3'" v-html="ckey">            </div>
            <emchart v-else-if="ctype=='5' &&!reload" :option="getOption()"></emchart>
            <childpaotal v-else-if="ctype=='7'" :name="ckey"
                :vc="vc" :cell="cell" class="vc-body" :height="p.height" :width="p.width" ref="childpaotal"
                :para="para" :changeState="shareState" :applet="applet"
            >
            </childpaotal>
        </template>
    </pan>
    
</template>

<script>
/**
            <d2-container-frame v-else-if="ctype=='2' &&!reload" :src="getUrl()" ></d2-container-frame>
            <bpopage v-else-if="ctype=='4' &&!reload" :src="getUrl()" ></bpopage>
 */
import { defineAsyncComponent } from 'vue'

//import { getMyStartlist } from "@/api/search/search.js"
import labeldiv from "@/ui/panels/labeldiv.vue"
import cardbox from "@/ui/panels/cardbox.vue"
import pan from "./part/pan.vue"

import portbar from "./portbar.vue"
//import bpopage from "@/components/ui/BPOPage.vue"
import emchart from "@/ui/portal/charts/emchart.vue"
import applet from "./cell/applet.vue"
import urlapplet from "./cell/urlapplet.vue"


import { ImportPaotalVC } from "@/ui/portal/util.js"

import { styleStrToObj } from "@/utils/style.js"
//内置VC
const innervclist= import.meta.glob(["@/ui/portal/ui/innervc/vc*.vue"]);
//业务VC
const widgetvclist= import.meta.glob(["@/widgets/**/vc*.vue"]);




function dynamicImportVC(dynamicViewsModules, component) {
	if (component == null || component === '') {
		return component;
	}
	// 配置若带 .vue 后缀则去掉，再与 glob 路径做 startsWith 匹配
	var path = String(component).replace(/\.vue$/i, '');
	const keys = Object.keys(dynamicViewsModules);
	const matchKeys = keys.filter((key) => {
		var k = key.replace(/..\/views|../, '');
        if(key.substring(0,13)=='/src/widgets/'){
            k = key.replace("/src/widgets/", '')
        }
		return k.startsWith(`${path}`) || k.startsWith(`/${path}`);
	});
	if (matchKeys?.length === 1) {
		const matchKey = matchKeys[0];
		return dynamicViewsModules[matchKey];
	}
	if (matchKeys?.length > 1) {
		return false;
	}
    return path;
}

const childpaotal = defineAsyncComponent(() =>
  import('./cell/childPaotal.vue')
)


export default {
    name:"vcholder",
    components: {
        portbar,emchart,applet,pan,
        childpaotal,urlapplet,
    },
    props:{
        vc:{type:Object,default:()=>{} },//组件对象
        cell:{type:Object,default:()=>{} },//组件对象
        pose:{type:String,default:"solo"},//固定范围 ： solo--单独显示， tab--在tab中显示  caro--在滚动图中显示
        height:null,
        width:null,
        applet:null,
        para:null,
        shareState:null,
        root:null,
        paotou:null,//门户配置主表对象
    },
    emits:["changePara"],
    data(){

        return {
            reload:false,
        };
    },
    computed:{
        paotalvclist(){
            var paotalvclist={};
            if(Object.keys(innervclist).length>0){
                paotalvclist  =Object.assign(paotalvclist,innervclist);
            }
            if(Object.keys(widgetvclist).length>0){
                paotalvclist  =Object.assign(paotalvclist,widgetvclist);
            }
            return paotalvclist;
        },
        /** P_ShowTitle=0 时传 '0' → pan 映射为 none，labeldiv 不渲染标题行（仅清空 label 仍会占位） */
        isPaotouHideTitle(){
            const showTitle = this.paotou?.P_ShowTitle ?? this.paotou?.p_ShowTitle;
            if(showTitle === undefined || showTitle === null){
                return false;
            }
            return showTitle === 0 || showTitle === '0' || showTitle === false || showTitle === 'false';
        },
        panPositionProp(){
            if(this.isPaotouHideTitle){
                return '0';
            }
            return this.vc.titleAlign;
        },
        thee(){
            if(!!this.vc?.theme){
                return this.vc.theme;
            }
            if(!!this.cell?.theme){
                return this.cell.theme;
            }
            return "1";
        },
        rendLabel(){
            if(this.isPaotouHideTitle){
                return false;
            }

            // pose/theme 切换到某些模式时隐藏 label
            if(this.pose=="tab" || this.thee=="2"){
                return false;
            }

            // 没有标题也不渲染 label
            if(!this.vc?.title){
                return false;
            }
            return true;
        },
        themePan(){
            var t="labeldiv";
            if(this.thee=="2"){
                t="cardbox"
            }
            return t;
        },
        ctype(){
            let type = this.vc.type || this.cell.type;
            if(type == "vue组件") return "1";
            return type;
        },
        ckey(){
            return this.vc.appletKey;
        },
        vcomponent(){
            if(this.ctype!='1') return null;
            return dynamicImportVC(this.paotalvclist,this.ckey);
        },
        vcname(){
            let type = this.vc.type || this.cell.type;
            console.log("rendvc")
            var key=this.vc.appletKey;
            let result = 'iframe';
            if( type == "1"){
                return key;
            }
        },
        titleStyle(){
            return styleStrToObj(this.vc.titleStyle)
        },
        currentPortlet(){
            if(this.ctype=='1'){
                return this.$refs.livevc;
            }
            return null;
        },
        hasTools(){
            var t=this.vc.tools;
            if(t==""||t==null || t==void 0){
                return false;
            }
            return true;
        },
        islazy(){
            if(this.vc.lazy==false){
                return false;
            }
            return true;
        },
    },
    mounted() {
        //this.getlist();
    },
    methods:{
        getComp(){
            var key=this.ckey;
            var tar=ImportPaotalVC(key);
            if(tar===false){
                return key;
            }
            console.log(tar)
            return tar;
        },
        getUrl(){
            return this.vc.appletKey;
        },
        getOption(){
            var tar={};
            try{
                tar=JSON.parse(this.ckey);
            }
            catch(e){}
            return tar;
        },
        titleClick(){

        },
        addClick() {
            if (Array.isArray(this.currentPortlet)) {
                this.currentPortlet[0].addClick()
            } else {
                this.currentPortlet.addClick()
            }
        },
        moreClick() {
            if (Array.isArray(this.currentPortlet)) {
                this.currentPortlet[0].moreClick()
            } else {
                this.currentPortlet.moreClick()
            }
        },
        refreshClick(isCustom) {
            if (this.ctype=='1') {
                if (Array.isArray(this.currentPortlet)) {
                    this.currentPortlet[0].refresh()
                } else {
                    this.currentPortlet.refresh()
                }
            } else {
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
            }
        },
    }
}
</script>

<style scoped>
.vc-title.theme1{
    font-weight: 700;
    color: #3385CC;
    font-size: 16px;
}

.pb-icon{
    margin-right:6px;
}
</style>
