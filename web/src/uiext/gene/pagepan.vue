<template>
    <div :class="{'pagepan':true,'flexed':flex}" ref="groot">
        <div class="f-header" ref="fhead"><slot name="header"></slot></div>
        <div :class="{'f-body':true,'hasbg':backgrounded,'bdflex':bodyFlex}" ref="fbody" :style="bodystyle">
            <slot :height="bodyHeight" ></slot>
        </div>
        <div class="f-footer" ref="ffoot"><slot name="footer"></slot></div>
    </div>
</template>

<script>
/**
 * 更新 2022-12-13 更新 增加逻辑，当内容高度已获取后，再执行插槽的绘制。
 * 
    <pagepan flex>
        <template #header="p">
        </template>
        <template #default="p">
        </template>
        <template #footer="p">
        </template>
    </pagepan>
 */

import { initListen,stopListen } from "@/ui/hooks/useSizeObserver.js"

export default {
    name:"pagepan",
    expose: ['resize'],
    components:{
        
    },
    props:{
        flex: { type: Boolean, default: false },//是否表体以flex撑开
        bodyFlex: { type: Boolean, default: false },//是否表体以flex撑
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
            footHeight:null,//未开启全屏时的表体高度。
            headHeight:null,//弹窗容器的高度
            panHeight:null,//容器整体高度
            listen:null,
        };
    },
    computed:{
        liveTBHeight(){
            return this.liveHeight
        },
        bodyHeight(){
            return this.panHeight-this.headHeight-this.footHeight;

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
        var els=[this.$el];
        initListen(els,
            {
                resize:true,
                //mutation:true,
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
            if(this.$refs.groot != void 0){
                var sh=this.$refs.groot.clientHeight;
                if(sh!=this.panHeight){
                    this.panHeight= sh;
                }
            }
            if(this.$refs.fhead != void 0){
                var sh=this.$refs.fhead.clientHeight;
                if(sh!=this.headHeight){
                    this.headHeight= sh;
                }             
            }
            if(this.$refs.ffoot != void 0){
                var sh=this.$refs.ffoot.clientHeight;
                if(sh!=this.footHeight){
                    this.footHeight= sh;
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
            // this.listen.observe(this.$refs.ffoot,{attributes:true,attributeFilter:['style'],attributeOldValue:false,subtree:false});
        },
        resize(){
            this.loadHeightInfo();
        }
    }
    
}
</script>
<style>
.pagepan.flexed{
    display: flex;
    flex-direction: column;
    height: 0;
    flex:1 1 0;
}

.pagepan.flexed>.f-header{
    flex:0 0 auto;
}
.pagepan.flexed>.f-body{
    height:0;
    flex:1 ;
     overflow:auto; 
}
.pagepan.flexed>.f-footer{
    flex:0 0 auto;
}

.hasbg.f-body{
    border-radius:5px;

}

.f-body.bdflex{
    display: flex;
    flex-direction: column;

}
.f-body.bdflex>div{
    flex:1 1 0;

}
</style>