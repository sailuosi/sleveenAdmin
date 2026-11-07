<template>
    <div :class="{'flexpan':true,'flexed':flex}" ref="groot">
        <div class="f-header"><slot name="header"></slot></div>
        <div :class="{'f-body':true,'hasbg':backgrounded}" ref="fbody" :style="bodystyle">
            <template v-if="lazy&& showbody">
                <lazydiv>
                    <slot :height="tbheight" :panHeight="panHeight" :rect="bodyRect"></slot>
                </lazydiv>
            </template>
            <slot v-else-if="showbody" :height="tbheight" :panHeight="panHeight" :rect="bodyRect"></slot>
        </div>
        <div class="f-footer" ref="ffoot"><slot name="footer"></slot></div>
    </div>
</template>

<script>
/**
 * 更新 2022-12-13 更新 增加逻辑，当内容高度已获取后，再执行插槽的绘制。
 * 
    <flexpan flex>
        <template #header="p">
        </template>
        <template #default="p">
        </template>
        <template #footer="p">
        </template>
    </flexpan>
 */
import lazydiv from "@/ui/panels/lazydiv.vue"
import { initListen,stopListen } from "@/ui/hooks/useSizeObserver.js"

export default {
    name:"flexpan",
    expose: ['resize'],
    components:{
        lazydiv
    },
    props:{
        flex: { type: Boolean, default: false },//是否表体以flex撑开
        height: {},
        maxHeight:null,
        maxable:{type:Boolean,default:true},//是否可以全屏。
        maxTitle:{type:String,default:"最大化"},//全屏的标题
        backgrounded:{ type: [Boolean,String], default: false },//主体部分是否带背景色。    background:rgba(255,255,255,99%);
        delay:{ type: Boolean, default: true },
        lazy:{ type: Boolean, default: false },//是否懒加载内容体
    },
    emits:[],
    data(){
        return {
            //弹窗最大化相关功能
            seemodal:false,//是否显示弹窗
            teleported:false,//是否传送
            modalRoot:null,//传送时的根节点
            liveHeight:null,//表体的高度
            normalHeight:null,//未开启全屏时的表体高度。
            modalHeight:null,//弹窗容器的高度
            panHeight:null,//容器整体高度
            bodyHeight:null,//主体容器的高度
            bodyRect:null,
            listen:null,
        };
    },
    computed:{
        liveTBHeight(){
            return this.liveHeight
        },
        tbheight(){
            if(this.flex){
                return this.bodyHeight==null?'100%':this.bodyHeight;             
            }
            else{
                //自动高度模式下，如果设置了最大高度，则监测
                if(!!this.maxHeight){
                    if(this.bodyHeight>this.maxHeight){
                        return this.maxHeight;
                    }
                }
            }
            return this.height;
        },
        showbody(){
            if(this.flex){
                return this.delay && !isNaN(this.tbheight);
            }
            return true;
        },
        bodystyle(){
            var res={};
            if(this.backgrounded !=false){
                var bg="rgba(255,255,255,99%)";
                if(typeof this.backgrounded=="string"){
                    bg=this.backgrounded;
                }
                res.background=bg;
            }
            return res;
        },
    },
    mounted(){
        //this.initListen();
        var els=[this.$el,this.$refs.ffoot];
        initListen(els,
            {
                resize:true,
                mutation:true,
            },
            ()=>{
            this.loadHeightInfo();
        })
        this.loadHeightInfo();
    },
    updated(){
        //this.loadHeightInfo();
    },
    onUnmounted () {
        stopListen();
        // if (this.listen) {
        //     //console.log("destroy窗口监听");
        //     this.listen.disconnect()
        //     this.listen.takeRecords()
        //     this.listen = null
        // }
    },
    methods:{
        loadHeightInfo(){
            //console.log("destroy窗口监听");
            var tbel=this.$refs.table;
            if(this.maxable && !this.seemodal && tbel!=void 0){
                var h=getComputedStyle(tbel.$el).height;
                if(this.teleported==false && this.liveHeight!=h){
                    requestAnimationFrame(()=>{
                        this.liveHeight=h;
                    })
                }
            }
            if(this.seemodal && this.$refs.groot !=void 0){
                var sh=this.$refs.groot.scrollHeight;
                if(sh!=this.modalHeight){
                    requestAnimationFrame(()=>{
                        this.modalHeight= sh;
                    })
                }
            }
            if(this.$refs.groot != void 0){
                var sh=this.$refs.groot.clientHeight;
                if(sh!=this.panHeight){
                    requestAnimationFrame(()=>{
                        this.panHeight= sh;
                    })
                }
            }
            if(this.$refs.fbody != void 0){
                var sh=this.$refs.fbody.clientHeight;
                if(sh!=this.bodyHeight){
                    requestAnimationFrame(()=>{
                        this.bodyHeight= sh;
                        this.bodyRect=this.$refs.fbody.getBoundingClientRect();
                    })
                    
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
            this.listen.observe(this.$refs.ffoot,{attributes:true,attributeFilter:['style'],attributeOldValue:false,subtree:false});
        },
        resize(){
            this.loadHeightInfo();
        }
    }
    
}
</script>
<style>
.flexpan.flexed{
    display: flex;
    flex-direction: column;
}

.flexpan.flexed>.f-header{
    flex:0 0 auto;
}
.flexpan.flexed>.f-body{
    height:0;
    flex:1 ;
     overflow:auto; 
}
.flexpan.flexed>.f-footer{
    flex:0 0 auto;
}

.hasbg.f-body{
    border-radius:5px;

}
</style>