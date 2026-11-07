<template>
    <div :style="mstyle" class="cihldpaotal-root">
        <div :style="gridpanStyle" class="grid-pan"
        >
            <div v-for="(applet,a) in applets" class="grid-applet"
                    :style="getApplestyle(applet)"
            >
                <portlet :applet="applet" :index="a" :para="rendPara" :shareState="shareState" :root="me"
                    @setState="onSetState" :paotou="portal"
                >
                </portlet>                
            </div>
        </div>
    </div>
</template>

<script>
//<el-backtop target="div.layout-main-scroll> div"></el-backtop>
//&& mydisable
import { mapState,mapWritableState} from "pinia";
//import {usePaotal} from "@/stores/paotou.js"

import portlet from "../portlet.vue"

import { getPortConfig,savePortCells } from "@/api/system/portalCommon.js"
import { styleStrToObj,objToStyleStr } from "@/utils/style.js"
import { parUrlPara } from "../../util.js"

export default {
    name:"childpaotal",
    components: {
        portlet,
    },
    provide(){
        var vm=this;
        return {
            paotal:vm,
            paotalConfig:this.configInfo,
            applets:this.applets,
            para:this.para,
            shareState:this.shareState,
            setState:this.setState,
        }
    },
    expose:["setState"],
    props:{
        disabled:{ type:Boolean,default:false},

        //共享参数，传递给每个portlet
        para:{ type:Object,default:()=>{} },
        //是否使用grid布局
        name: {        default:null	},
        framed:{ type:Boolean,default:false },
        animation: { type: String, default: 'fadeLeft' },
        cell:null,
        vc:null,
        root:null,
        paotalConfig:null,
        width:null,
        height:null,
    },
    data() {
        return {
            me:this,
            applets: [

            ],
            draggable: true,
            resizable: true,
            index: 0,
            eventLog: [],
            configInfo:{
                portal:{},
            },
            shareState:{},
            gmagin:[10,10],
        }
    },
    computed:{
        //...mapWritableState(usePaotal, ['paotalMap']),
        portal(){
            var val=this.configInfo.portal;
            if(!!val){
                return val;
            }
            return {
                colnum:12,

            };
        },
        mstyle(){
            var tar=this.portal.style;
            var obj=styleStrToObj(tar);
            obj.height=this.height+'px';
            return obj;
        },
        rendPara(){
            var para = this.para;
            var res={};
            if(para !=null && para !=void 0 && typeof para=="object"){
                res=Object.assign(res,para);
            }
            //解析参数
            if(!!this.portal.para){
                var urlPara=parUrlPara(this.portal.para);
                res=Object.assign(res,urlPara);
            }
            return res;
        },
        mydisable(){
            if(this.disabled){
                return true;
            }
            if(this.portal.editable=="0"){
                return true;
            }
            return false;
        },
        gridpanStyle(){
            var rowHeight=(this.portal.rowheight*1) + this.gmagin[1];
            var obj={
                display:"grid",
                "grid-template-columns":"repeat("+this.portal.colnum+", 1fr)",
                "grid-auto-rows":rowHeight*1+'px',
            }
            return obj;
        },
    },
    watch: {
        eventLog: function() {
            const eventsDiv = this.$refs.eventsDiv;
            eventsDiv.scrollTop = eventsDiv.scrollHeight;
        },
        name:function(){
            this.loadConfig();
        },
    },
    mounted(){
        this.loadConfig();
    },
    methods: {
        getApplestyle(p){
            var rowHeight=this.portal.rowheight;
            var obj={};
            obj["grid-column-start"]=(p.x*1+1);
            obj["grid-column-end"]="span "+(p.w*1);
            obj["grid-row-start"]=(p.y*1+1);
            obj["grid-row-end"]="span "+(p.h*1);
            var height= Math.round(
                rowHeight * p.h + Math.max(0, p.h - 1) * this.gmagin[1]
                //10 * 24 + Math.max(0, 24 - 1) * 10
            )
            //obj["height"]=height+'px';
            return obj;
        },
        appendStyle(){
            var myid= "css"+this.name;
            var t=document.getElementById(myid);
            if(t==null  || t== void 0){
                var ele = document.createElement("style");
                ele.innerHTML = ":root{" +objToStyleStr(this.mstyle)+"}";
                ele.id=myid;
                document.getElementsByTagName('head')[0].appendChild(ele);
            }
            else{
                t.innerHTML = ":root{" +objToStyleStr(this.mstyle)+"}";
            }

                
        },
        onSetState(e){
            this.setState(e.key,e.value);
        },
        setState(key,val){
            this.shareState[key]=val;
        },
        loadConfig(){
            //console.log("loadPaotalCOnfig",this)
            if(!this.name){
                return;
            }
            
            //尝试从缓存中获取
            var tar=this.paotalMap[this.name];
            if(!!tar){
                this.configInfo=tar;
                this.applets=this.geneApplets();
                this.appendStyle();      
                return ;
            }
            getPortConfig(this.name).then((res)=>{
                var cfg = null;
                if (res && res.status === 'success' && res.data) cfg = res.data;
                else if (res && (res.portal || res.cells || res.applets)) cfg = res;
                else if (res?.data && (res.data.portal || res.data.cells)) cfg = res.data;
                if (!cfg) return;
                this.configInfo = cfg;
                this.applets = this.geneApplets();
                this.paotalMap[this.name] = cfg;
                this.appendStyle();
            })
        },
        geneApplets(){
            var val=this.configInfo.cells;
            if(Array.isArray(val)){
                var applets= this.configInfo.applets;

                if(!Array.isArray(applets)){
                    applets=[];
                }
                var tar=val.map((app)=>{
                    var obj=Object.assign({},app)
                    obj.x=obj.x*1;
                    obj.y=obj.y*1;
                    obj.w=obj.w*1;
                    obj.h=obj.h*1;
                    return obj;
                })
                var res=tar.map((cell)=>{
                    cell.vclist=applets.filter((app)=>{
                        return app.link==cell.ckey;
                    })
                    return cell;
                })
                return res;
            }
            return this.appletsDemo;
        },
        save(){
            var str=JSON.stringify(this.applets);
            savePortCells(str,this.portal.oid).then((e)=>{
                this.$message(e.data);
            })
        },
        moveEvent: function(i, newX, newY){
            const msg = "MOVE i=" + i + ", X=" + newX + ", Y=" + newY;
            this.eventLog.push(msg);
            console.log(msg);

        },
        movedEvent: function(i, newX, newY){
            const msg = "MOVED i=" + i + ", X=" + newX + ", Y=" + newY;
            this.eventLog.push(msg);
            console.log(msg);

        },
        resizeEvent: function(i, newH, newW, newHPx, newWPx){
            const msg = "RESIZE i=" + i + ", H=" + newH + ", W=" + newW + ", H(px)=" + newHPx + ", W(px)=" + newWPx;
            this.eventLog.push(msg);
            console.log(msg);
        },
        resizedEvent: function(i, newX, newY, newHPx, newWPx){
            const msg = "RESIZED i=" + i + ", X=" + newX + ", Y=" + newY + ", H(px)=" + newHPx + ", W(px)=" + newWPx;
            this.eventLog.push(msg);
            console.log(msg);

        },
        containerResizedEvent: function(i, newH, newW, newHPx, newWPx){
            const msg = "CONTAINER RESIZED i=" + i + ", H=" + newH + ", W=" + newW + ", H(px)=" + newHPx + ", W(px)=" + newWPx;
            this.eventLog.push(msg);
            console.log(msg);
        },

        layoutCreatedEvent: function(newLayout){
            this.eventLog.push("Created layout");
            console.log("Created layout: ", newLayout)
        },
        layoutBeforeMountEvent: function(newLayout){
            this.eventLog.push("beforeMount layout");
            console.log("beforeMount layout: ", newLayout)
        },
        layoutMountedEvent: function(newLayout){
            this.eventLog.push("Mounted layout");
            console.log("Mounted layout: ", newLayout)
        },
        layoutReadyEvent: function(newLayout){
            this.eventLog.push("Ready layout");
            console.log("Ready layout: ", newLayout)
        },
        layoutUpdatedEvent: function(newLayout){
            this.eventLog.push("Updated layout");
            console.log("Updated layout: ", newLayout)
        },
    }
}

/**
 * 全局样式控制逻辑，通过在门户层元素上定义css变量，来对门户下内容样式进行控制。
 */

</script>
<style scoped>
.cihldpaotal-root{
    overflow:auto;
}
.grid-pan{

}
.grid-applet{
    margin:5px;
}

.applet-panel{
    height:100%;
}

.paotal-tool{
    position: fixed;
    right: 40px;
    bottom: 120px;
    background-color:#fff;

    z-index:100;
}


</style>