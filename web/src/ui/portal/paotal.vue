<template>
    <div :style="mstyle" class="paotal-top">
        <div v-if="!mydisable" class="paotal-tool">
            <el-button class="pt-save"  @click="save" round>保存</el-button>

        </div>
        <template v-if="grid && mydisable">
            <div :style="gridpanStyle" class="grid-pan"
            >
                <div v-for="(applet,a) in applets" class="grid-applet"
                        :style="getApplestyle(applet)"
                >
                    <portlet :applet="applet" :index="a" :para="rendPara" :shareState="shareState" :root="me"
                        :paotou="portal"
                        @setState="onSetState"
                        @changePara="$emit('changePara',$event)"
                    >
                    </portlet>
                </div>
            </div>
        </template>

        <GridLayout v-else v-model:layout="applets"
                     :col-num="portal.colnum*1"
                     :row-height="Math.max(1, portal.rowheight*1 || 30)"
                     :is-draggable="!mydisable"
                     :is-resizable="!mydisable"
                     :vertical-compact="true"
                     :use-css-transforms="true"
        >
            <GridItem v-for="(applet,a) in applets"
                       :key="applet.i"
                       :x="applet.x*1"
                       :y="applet.y*1"
                       :w="applet.w*1"
                       :h="applet.h*1"
                       :i="applet.i"
                       drag-allow-from=".portal-bar"
                       drag-ignore-from=".no-drag"
            >
                <portlet :applet="applet" :index="a" :para="rendPara" :shareState="shareState"
                    :paotou="portal"
                    @setState="onSetState"
                    @changePara="$emit('changePara',$event)"
                />
            </GridItem>
        </GridLayout>
    </div>
</template>

<script>
//<el-backtop target="div.layout-main-scroll> div"></el-backtop>
//&& mydisable
//:root="me"
import { mapState,mapWritableState} from "pinia";
import {usePaotal} from "@/stores/paotou.js"

// vue-grid-layout@3 beta 的 CJS 命名导出损坏，改用 Vue3 可用的 grid-layout-plus（局部注册）
import { GridLayout, GridItem } from "grid-layout-plus"

import portlet from "./ui/portlet.vue"

import { getPortConfig,getPortConfigByOid,savePortCells } from "@/api/system/portalCommon.js"
import { styleStrToObj,objToStyleStr } from "@/utils/style.js"
import { parUrlPara } from "./util.js"
import {useDefer} from "@/utils/useDefer.js"
//let defer=useDefer();
export default {
    name:"paotal",
    components: {
        GridLayout,GridItem,
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
    emits:["changePara"],
    expose:["setState"],
    props:{
        disabled:{ type:Boolean,default:false},
        name:{type:String,default:""},
        oid:{type:String,default:""},
        //共享参数，传递给每个portlet
        para:{ type:Object,default:()=>{} },
        //是否使用grid布局
        grid:{ type:Boolean,default:true,},
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
        ...mapWritableState(usePaotal, ['paotalMap']),
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
            return false;
            if(this.disabled){
                return true;
            }
            if(this.portal.editable=="0"||this.portal.editable==false){
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
        oid:function(){
            this.loadConfig();
        },
    },
    mounted(){
        this.loadConfig();
    },
    methods: {
        defer(n){
          return defer(n);
        },
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
            var sid = this.oid ? ("oid_"+this.oid) : this.name;
            var myid= "css"+sid;
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
            var obj=Object.assign( {},this.shareState);
            obj[key]=val;
            this.shareState=obj;
        },
        loadConfig(){
            var byOid = !!this.oid;
            if(!byOid && !this.name){
                return;
            }
            var cacheId = byOid ? ("oid:"+this.oid) : ("key:"+this.name);

            var tar=this.paotalMap[cacheId];
            if(!!tar){
                this.configInfo=tar;
                this.applets=this.geneApplets();
                this.appendStyle();
                return ;
            }
            var req = byOid ? getPortConfigByOid(this.oid) : getPortConfig(this.name);
            req.then((res)=>{
                // minehub: { status:'success', data }；sleveen requestAPI6 已解包为配置本体 { portal, cells, applets }
                var cfg = null;
                if (res && res.status === 'success' && res.data) cfg = res.data;
                else if (res && (res.portal || res.cells || res.applets)) cfg = res;
                else if (res?.data && (res.data.portal || res.data.cells)) cfg = res.data;
                if (!cfg) return;
                this.configInfo = cfg;
                this.applets = this.geneApplets();
                this.paotalMap[cacheId] = cfg;
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
            var portalId = this.portal?.oid || this.oid;
            if (!portalId) {
                this.$message.error("门户 OID 缺失，无法保存");
                return;
            }
            // 只提交格子几何；去掉运行时 vclist，避免无用大包
            var cells = (this.applets || []).map((c) => ({
                oid: c.oid,
                x: c.x,
                y: c.y,
                w: c.w,
                h: c.h,
                i: c.i,
                ckey: c.ckey,
                name: c.name,
                theme: c.theme,
                mode: c.mode
            }));
            savePortCells(cells, portalId).then(()=>{
                var sid = this.oid ? ("oid_"+this.oid) : this.name;
                var cacheId = this.oid ? ("oid:"+this.oid) : ("key:"+this.name);
                delete this.paotalMap[cacheId];
                var portalKey = this.portal?.P_Key || this.portal?.key || this.name;
                if (portalKey) delete this.paotalMap["key:"+portalKey];
                if (portalId) delete this.paotalMap["oid:"+portalId];
                var myid = "css" + sid;
                var t = document.getElementById(myid);
                if(t){
                    t.innerHTML = "";
                }
                this.$message.success("保存成功");
            }).catch((err)=>{
                this.$message.error(err?.message || "保存失败");
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

<style>
/* 这里是 全局样式控制部分 */
.paotal-top .el-table__cell{
    font-size:var(--paotal-cell-font-size,14px);
}
.paotal-top div.el-input--small{
    font-size:var(--paotal-input-font-size,14px);
}
.post-pan .el-tree-node{
    font-size:var(--paotal-cell-font-size,14px);
}

/* 当门户的弹窗弹出时，内部的元素挂到根级，需要特殊设置。 */
.card-dia div.el-input--small{
    font-size:var(--paotal-input-font-size,14px);
}
.card-dia  .el-radio.el-radio--small .el-radio__label{
    font-size:var(--paotal-input-font-size,14px);
}
.card-dia .el-form-item__label{
    font-size:var(--paotal-input-font-size,14px);
}
.card-dia .el-form-item__content{
    font-size:var(--paotal-input-font-size,14px);
}
.card-dia .el-tag{
    font-size:var(--paotal-input-font-size,14px);
}

.card-dia .el-table__cell{
    font-size:var(--paotal-cell-font-size,14px);
}
</style>
<style scoped>
.paotal-top{
    font-size:var(--paotal-cell-font-size,14px);
    min-height: 100%;
	--portal-gap: 8px;
    --portal-page-bg:
        linear-gradient(180deg, rgba(245, 249, 255, 0.92), rgba(245, 247, 250, 0.96)),
        repeating-linear-gradient(135deg, rgba(17, 120, 204, 0.035) 0 1px, transparent 1px 96px);
    --portal-card-bg: #fff;
    --portal-card-border: #e6edf5;
    --portal-card-radius: 8px;
    --portal-card-shadow: 0 1px 2px rgba(15, 23, 42, 0.04);
    --portal-title-color: #0f172a;
    --portal-blue: #118bd0;
    border-radius: var(--paotal-page-radius, 0);
    background: var(--portal-page-bg);
    box-sizing: border-box;
    padding: var(--portal-gap);
    color: var(--portal-title-color);
}
.grid-pan{
	margin: calc(var(--portal-gap) / -2);
}
.grid-applet{
	margin: calc(var(--portal-gap) / 2);
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

.pt-save{
    border-radius: 20px;
    padding: 12px 12px;
    width: 40px;
    height: 40px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 20px;
    box-shadow: 0 0 6px rgba(0,0,0,.12);
    cursor: pointer;
}

.vue-grid-layout,
.vgl-layout {
    background: transparent;
	margin: calc(var(--portal-gap) / -2);
}

:deep(.port-applet) {
    height: 100%;
    min-width: 0;
    border: none;
    border-radius: 0;
    background: transparent;
    box-shadow: none;
    overflow: hidden;
    box-sizing: border-box;
}

:deep(.portlet-content) {
    height: 100%;
    border-radius: inherit;
}

:deep(.train-metrics__card),
:deep(.exam-overview__card),
:deep(.hh-study-rank),
:deep(.hh-score-rank),
:deep(.student-study),
:deep(.teacher-work__main),
:deep(.teacher-work__side),
:deep(.class-trend),
:deep(.train-trend),
:deep(.train-risk),
:deep(.train-recent),
:deep(.exam-schedule),
:deep(.exam-trend),
:deep(.my-class),
:deep(.my-exam),
:deep(.portal-notice),
:deep(.hh-pt-metrics__card),
:deep(.hh-pt-quick__item),
:deep(.hh-pt-access__item),
:deep(.hh-pt-ongoing),
:deep(.hh-pt-course),
:deep(.hh-pt-kpi) {
    background: var(--portal-card-bg);
    border: 1px solid var(--portal-card-border);
    border-radius: var(--portal-card-radius);
    box-shadow: var(--portal-card-shadow);
    box-sizing: border-box;
}

:deep(.labeldiv),
:deep(.cardbox-pan) {
    height: 100%;
    border-radius: 0;
    background: transparent;
    box-shadow: none;
    overflow: hidden;
}

:deep(.lz-label),
:deep(.cd-label.portal-bar) {
    min-height: 36px;
    background: transparent;
    border-bottom: 1px solid var(--portal-card-border);
    color: var(--portal-title-color);
}

:deep(.portal-bar .vc-title),
:deep(.vc-title) {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    font-size: 15px;
    line-height: 22px;
    font-weight: 700;
    color: var(--portal-title-color);
}

:deep(.portal-bar .pb-icon),
:deep(.vc-title .pb-icon) {
    width: 3px;
    height: 15px;
    border-radius: 0;
    background: var(--portal-blue);
    color: transparent;
    margin-right: 2px;
}

:deep(.lz-value),
:deep(.cd-box) {
    background: transparent;
    overflow: hidden;
}

:deep(.quick-business__item),
:deep(.train-metrics__card),
:deep(.exam-overview__card),
:deep(.hh-study-rank),
:deep(.hh-score-rank),
:deep(.student-study),
:deep(.teacher-work__main),
:deep(.teacher-work__side),
:deep(.class-trend),
:deep(.train-trend),
:deep(.train-risk),
:deep(.train-recent),
:deep(.exam-schedule),
:deep(.exam-trend),
:deep(.my-class),
:deep(.my-exam),
:deep(.portal-notice),
:deep(.hh-pt-metrics__card),
:deep(.hh-pt-quick__item),
:deep(.hh-pt-access__item),
:deep(.hh-pt-ongoing),
:deep(.hh-pt-course),
:deep(.hh-pt-kpi) {
    box-shadow: var(--portal-card-shadow);
}

:deep(.quick-business__item:hover),
:deep(.train-metrics__card:hover),
:deep(.exam-overview__card:hover),
:deep(.hh-pt-quick__item:hover),
:deep(.hh-pt-access__item:hover) {
    box-shadow: 0 8px 16px rgba(13, 58, 105, 0.1);
}

:deep(.quick-business::before),
:deep(.quick-business::after),
:deep(.quick-business__item::before),
:deep(.quick-business__item::after),
:deep(.train-metrics__card::before),
:deep(.train-metrics__card::after),
:deep(.exam-overview__card::before),
:deep(.exam-overview__card::after),
:deep(.hh-pt-metrics__card::before),
:deep(.hh-pt-metrics__card::after),
:deep(.hh-pt-quick__item::before),
:deep(.hh-pt-quick__item::after),
:deep(.hh-pt-access__item::before),
:deep(.hh-pt-access__item::after),
:deep(.student-study::before),
:deep(.student-study::after),
:deep(.teacher-work__main::before),
:deep(.teacher-work__main::after),
:deep(.teacher-work__side::before),
:deep(.teacher-work__side::after) {
    content: none;
    display: none;
    background: none;
    filter: none;
}


.vue-grid-item .resizing,
.vgl-item.vgl-item--resizing {
    opacity: 0.9;
}

.vue-grid-item .static,
.vgl-item.vgl-item--static {
    background: #cce;
}

.vue-grid-item .text,
.vgl-item .text {
    font-size: 24px;
    text-align: center;
    position: absolute;
    top: 0;
    bottom: 0;
    left: 0;
    right: 0;
    margin: auto;
    height: 100%;
    width: 100%;
}

.vue-grid-item .no-drag,
.vgl-item .no-drag {
    height: 100%;
    width: 100%;
}

.vue-grid-item .minMax,
.vgl-item .minMax {
    font-size: 12px;
}

.vue-grid-item .add,
.vgl-item .add {
    cursor: pointer;
}

.layoutJSON {
    background: #ddd;
    border: 1px solid black;
    margin-top: 10px;
    padding: 10px;
}

.eventsJSON {
    background: #ddd;
    border: 1px solid black;
    margin-top: 10px;
    padding: 10px;
    height: 100px;
    overflow-y: scroll;
}
</style>
