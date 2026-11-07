<template>
<div
    class="formcard-shell"
    :class="{ 'formcard-shell--vc': isCardShell, 'formcard-shell--flex': flex && isCardShell }"
>
<div class="formcard" :class="[opened?'opened':'closed',theme,flex?'flexed':'',stretch?'stretch':'' ]">
    <div class="fc-head">
        <slot name="label">
            <div class="lazycard-header" :style="titleStyle" >
                <span @click="togglecontent" style="cursor:pointer;" class="head-tt">
                    <svg v-if="icon" :style="{transform:opened?'rotate(0deg)':'rotate(180deg)'}" class="icon" style="transition:all .5s ease-out;width: 1em; height: 1em;vertical-align: middle;fill: currentColor;" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="1566"><path d="M512 830.4l402.4-401.9c10.4-10.4 27.4-10.4 37.8 0 10.4 10.4 10.4 27.4 0 37.8L531.1 887c-5.2 5.2-12.2 7.8-19.1 7.8-6.9 0-13.8-2.6-19.1-7.8L71.8 466.4c-10.4-10.4-10.4-27.4 0-37.8 10.4-10.4 27.4-10.4 37.8 0L512 830.4z" fill="#040000" p-id="1567"></path><path d="M512 538.9L914.4 137c10.4-10.4 27.4-10.4 37.8 0 10.4 10.4 10.4 27.4 0 37.8L531.1 595.4c-5.2 5.2-12.2 7.8-19.1 7.8-6.9 0-13.8-2.6-19.1-7.8L71.8 174.8c-10.4-10.4-10.4-27.4 0-37.8 10.4-10.4 27.4-10.4 37.8 0L512 538.9z" fill="#040000" p-id="1568"></path></svg>
                    <slot name="tail"><span v-html="title" ></span></slot>
                </span>            
                <slot name="header"></slot>
            </div>
            <div class="toolbar">
                <slot name="toolbar"></slot>
            </div>
        </slot>
    </div>

    <div class="fc-body" ref="cbody">
        <lazydiv v-if="lazy" ref="content" :delay="delay" 
            :bodyStyle="bodycss" wrap :mode="mode"
        >
            <slot :height="bodyheight"></slot>
        </lazydiv>
        <div v-if="!lazy" :style="bodycss" class="wrap-bd">
            <slot :height="bodyheight"></slot>
        </div>

        <slot v-if="!rended || !opened" name="empty"></slot>
    </div>

</div>
</div>
</template>


<script>

/**
主要为表单分区所使用的卡片



*/
import lazydiv from "@/ui/panels/lazydiv.vue"
import { initListen,stopListen } from "@/ui/hooks/useSizeObserver.js"

export default {
    name:"formcard",
    components:{
        lazydiv
    },
    props: {
        title: null,
        //seen--出现在视口内时展开，click--点击标题的展开按钮或空缺插槽时展开，prop--被控制时展开。
        mode:{type:String,default:"seen"},
        minHeight: { type: String, default: "1px" },
        height: { type: String, default: "auto" },
        titleStyle: null,
        togglable: { type: Boolean, default: true },
        icon: { type: Boolean, default: false },
        //主题样式
        theme :{ type: String, default: "sim card" },
        span:{type:[Number,String],default:""},
        gap:{type:String,default:"15px 15px"},
        //如果设置了懒加载，则初始必然不绘制。设置了open，则必然不是懒加载
        lazy: { type: Boolean, default: false },
        delay:null,
        //状态输出与受控
        name: null,
        value: null,
        minSpanSize:null,
        minSpan:null,
        maxSpan:null,
        initBodyHeight:null,
        listen:{ type: Boolean, default: false },
        flex:{ type: Boolean, default: false },
        stretch: { type: Boolean, default: false },
    },
    data: function () {
        var opened = !this.lazy;

        return {
            opened: opened,
            rended: opened,
            myheight: (opened ? this.height : this.minHeight),
            myWidth:null,
            bodyheight:this.initBodyHeight,
        };
    },
    computed: {
        contStyle() {
            var show = this.opened ? 1 : 0;
            var res = {
                height: this.myheight,
                opacity: show,
                transform: 'scale(1,' + show + ')'
            };
            return res;
        },
        bodycss(){
            var res={
                gap:this.gap,
            }
            if(this.rendSpan>0){
                res.display="grid";
                //: repeat(3, 200px);
                if(isNaN(this.rendSpan)==false){
                    res['grid-template-columns']="repeat("+this.rendSpan+",1fr)";
                }
                else{
                    res['grid-template-columns']=this.rendSpan;
                }
                
            }
            return res;
        },
        isCardShell() {
            return String(this.theme || "")
                .trim()
                .split(/\s+/)
                .filter(Boolean)
                .includes("card");
        },
        rendSpan(){
            //未定义槽宽时，为固定
            if(this.minSpanSize ==void 0  || !this.minSpanSize){
                return this.span;
            }
            //根据宽度进行计算
            if(this.myWidth==void 0|| this.myWidth==null||  this.myWidth<100){
                return this.span;
            }
            var pw=this.myWidth*1;
            var sw=this.minSpanSize*1;
            var size= Math.floor(pw/sw);
            if(isNaN(size) ){
                return this.span;
            }
            if(size<1){
                return 1;
            }
            if(!isNaN(this.minSpan)){
                var ms=this.minSpan;
                if(size<ms){
                    return ms;
                }
            }
            if(!isNaN(this.maxSpan)){
                var ms=this.maxSpan*1;
                if(size>ms){
                    return ms;
                }
            }
            return size;
        }
    },
    expose:["toggle"],
    methods: {
        toggle(val){
            if(val==null||val==void 0){
                this.togglecontent();
                return ;
            }
            if(typeof val == 'Boolean' && val != this.opened){
                this.togglecontent();
            }
            
        },
        togglecontent() {
            if (!this.togglable) return;
            if(this.lazy){
                this.$refs.content.toggle();
            }
            var vm = this;
            if (this.rended == false) {
                this.rended = true;
            }
            if (this.opened) {
                this.opened = false;
                this.myheight = this.minHeight;
            } else {
                vm.myheight = vm.height;
                this.opened = true;
            }
            var val = this.opened;
            if (!!this.name) {
                var i = -1;
                var tar = [];
                if(Array.isArray(this.value)){
                    tar=this.value.concat();
                }
                i = tar.indexOf(this.name);
                if (val && i == -1){
                    tar.push(this.name);
                }
                if (!val && i != -1) {
                    tar.splice(i,1);
                }
                this.$emit('input', tar);
            }else{
                this.$emit('input', this.opened);
            }
            this.$emit('change', this.opened);
        },
        loadPanSize(){
            console.log("changsize")
            var el=this.$el;
            if(el != void 0){
                var sw=el.clientWidth;
                if(sw!=this.myWidth){
                    this.myWidth= sw;
                }
            }
            //cbody
            var cbody= this.$refs.cbody;
            if(cbody != void 0){
                var sw=el.clientHeight;
                if(sw!=this.bodyheight){
                    this.bodyheight= sw;
                }
            }
        },
    },
    mounted(){
        if(this.listen){
            var cbody= this.$refs.cbody;
            initListen([this.$el,cbody],{resize:true},this.loadPanSize);
            this.loadPanSize();            
        }

    },
    unmounted(){
        stopListen()
    },
    watch: {
        value: {
            handler(newval, oldval) {
                if (newval == void 0 || newval == null) return;
                var needOpen = (typeof newval == 'Boolean' && newval == true) || newval == this.name || (Array.isArray(newval) && newval.indexOf(this.name) != -1);
                if (needOpen != this.opened) {
                    this.togglecontent();
                }
            },
            immediate: true
        },
    },
}
</script>

<style scoped>
/* 与 VC sum-page-shell 一致：灰底 + 外边距 padding，白卡片在内部 .formcard.card */
.formcard-shell {
    box-sizing: border-box;
    width: 100%;
}
.formcard-shell--vc {
    padding: 12px 16px 16px;
    background: linear-gradient(180deg, #eef1f6 0%, #f5f7fa 48%, #f0f2f5 100%);
}
.formcard-shell--flex {
    flex: 1;
    min-height: 0;
    display: flex;
    flex-direction: column;
    height: 100%;
}
.formcard-shell--flex > .formcard {
    flex: 1;
    min-height: 0;
}

div.lazycard-header{
    display: flex;
    flex-direction: row;
}

.head-tt{
    display: inline-flex;
    align-items: center;
    gap: 10px;
    min-width: 0;
    font-weight: 700;
    letter-spacing: 0.02em;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}

/* 标题栏：与 VC .sum-card-toolbar / .sum-toolbar-title 同源样式 */
.formcard.sim > .fc-head {
    top: 0;
    z-index: 1;
    display: flex;
    align-items: center;
    justify-content: space-between;
    flex-wrap: wrap;
    gap: 10px 12px;
    box-sizing: border-box;
    flex-shrink: 0;
}

/* 白卡片内顶栏：对齐 VC_PS_*MonthPlan 等页的 .sum-card-toolbar */
.formcard.card.sim > .fc-head {
    margin: 0 18px 12px;
    padding: 10px 12px;
    background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
    border: 1px solid var(--el-border-color-lighter);
    border-radius: 12px;
}

.formcard.card .lazycard-header {
    flex: 1;
    min-width: 0;
    align-items: center;
    flex-wrap: wrap;
    gap: 10px;
}

.formcard.sim:not(.card) > .fc-head {
    padding: 10px 12px;
    margin: 0;
    background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
    border: 1px solid var(--el-border-color-lighter);
    border-radius: 12px;
}

/* 标题文字：对齐 .sum-toolbar-title__text */
.formcard.card.sim > .fc-head > .lazycard-header > .head-tt,
.formcard.sim:not(.card) > .fc-head > .lazycard-header > .head-tt {
    position: relative;
    z-index: 1;
    align-self: center;
    line-height: 22px;
    height: 22px;
    font-size: 18px;
    font-weight: 700;
    color: #303133;
    letter-spacing: 0.02em;
    padding-left: 0;
    margin: 0;
}

/* 左侧色条：对齐 .sum-toolbar-title__bar */
.formcard.sim > .fc-head > .lazycard-header > .head-tt::before {
    content: "";
    width: 4px;
    height: 22px;
    border-radius: 2px;
    background: var(--el-color-primary);
    flex-shrink: 0;
}

.formcard.sim>.fc-head.sticky {
    position:sticky; 
}

.toolbar>:deep(.ucml-vc-toolbar){
    padding-bottom:0;
}

.toolbar{
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    justify-content: flex-end;
    gap: 10px;
}

.formcard.card.sim > .fc-head .toolbar {
    flex-shrink: 0;
}

/* 右侧：对齐 .sum-toolbar__right */
.toolbar :deep(.el-button){
    border-radius: 10px;
}

.formcard.card{
    border-radius: var(--fccard-border-radius,16px);
    border: 1px solid rgba(235, 238, 245, 0.95);
    background-color: var(--fccard-bg-color,#ffffff);
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.07), 0 1px 3px rgba(0, 0, 0, 0.04);
    overflow: hidden;
    padding-top: 16px;
    box-sizing: border-box;
}
.formcard.card > .fc-head {
    border-top-left-radius: 0;
    border-top-right-radius: 0;
}
.formcard.card>.fc-body{
    border-bottom-left-radius: var(--fccard-border-radius,16px);
    border-bottom-right-radius: var(--fccard-border-radius,16px);
    padding: var(--fccard-bdpad,0 18px 18px);
}

.formcard.flexed{
    display: flex;
    flex-direction: column;
}
.formcard.flexed>.fc-head{
    flex: 0 0 auto;
}
.formcard.flexed>.fc-body{
    flex: 1 1 0;
    display: flex;
    flex-direction: column;
}
.formcard.flexed>.fc-body:deep(>div){
    flex: 1 1 0;
}
.formcard.stretch>.fc-body:deep(>div.wrap-bd){
    height:0;
}

</style>