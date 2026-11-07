<template>
    <div ref="mediv" :style="myStyle" :class="['lazydiv',inited?'waked':'sleeped']">
        <collapser v-show="showing" :many="many">
            <template v-if="inited"  >
                <template v-if="wrap">
                    <div :style="bodyCss">
                        <slot :toggle="toggle" ></slot>
                    </div>
                </template>
                <template v-else>
                    <slot :toggle="toggle" :style="bodyCss"></slot>                
                </template>
            </template>
        </collapser>
        <template v-if="!inited || !showing">
            <slot name="hide" :toggle="toggle"></slot>
        </template>
    </div>
</template>
<script>
//懒加载div,提供3种懒惰模式。
/**
 <div v-if="inited" v-show="showing" :style="bodyStyle"><slot :toggle="toggle"></slot></div>   
 */

import { objToStyleStr } from "@/utils/style.js"

export default {
    name:"lazydiv",
    props:{
        root:null,
        keep:{type:Boolean,default:false},
        margin:{type:String,default:"0px 0px 0px 0px"},
        emptyHeight:null,
        minHeight:null,
        //seen--出现在视口内时展开，click--点击标题的展开按钮或空缺插槽时展开，man--被控制时展开。 open--立即打开
        mode:{type:String,default:"seen"},
        visible:{type:Boolean},//由外界控制的展示、收起句柄。
        delay:{type:Number,default:-1},//延时，如果设置了，到达延时自动打开
        bodyStyle:{type:[String,Object],default:""},//内容体样式
        many:{type:Boolean,default:false},//是否多个子元素，切换动画模式
        wrap:{type:Boolean,default:false},//是否将多个子元素进行包裹，强行更正只单个子元素
        display:null,        
    },
    emits:["init","reset"],
    expose: ['toggle',"getEl"],
    data(){
        return{
            observer:null,
            inited:false,
            showing:true,
        };
    },
    computed:{
        myStyle(){
            var res={};
            if(!this.inited && !!this.emptyHeight){
                res.height=this.emptyHeight;
            }
            if(!!this.minHeight){
                res.minHeight=this.minHeight;
            }
            if(!!this.display){
                res.display=this.display;
            }
            return res;
        },
        bodyCss(){
            if(typeof this.bodyStyle=="string"){
                return this.bodyStyle
            }
            if(typeof this.bodyStyle=="object"){
                return objToStyleStr(this.bodyStyle);
            }
            return "";
        },
    },
    created(){
        if(this.mode=="open"){
            //立即打开，不懒加载
            this.initOpen();
        }
    },
    mounted() {
        var opts={
            rootMargin:this.margin,
            threshold:[0.1,0.1],
        };
        if(this.root!=null){
            opts.root=this.root;
        }
        if(this.mode=="seen"){
            this.observer = new IntersectionObserver(this.onchange, opts);
            this.observer.observe(this.$refs.mediv);            
        }
        if(this.delay>0){
            this.lazyer=setTimeout(() => {
                this.layOpen();
            }, this.delay);
        }
    },
    watch:{
        'visible':{
            handler:function(e){
                if(e==true){
                    if(this.inited==false){
                        this.initOpen();                
                    }else{
                        //已经挂载的情况下，执行内容切换
                        this.showing=e;
                    }                    
                }
                else{
                    this.showing=false;
                }

            },
            immediate: true
        },
    },
    methods:{
        getEl(){
            return this.$el;
        },
        initOpen(){
            //console.log('i say init');
            this.inited=true;
            this.showing=true;
            this.$emit("init");
        },
        layOpen(){
            //延时到达，触发自动打开
            if(this.inited==false){
                this.initOpen();
            }
        },
        toggle(val){
            if(typeof val=="boolean"){
                if(this.inited==false && val==true){
                    this.initOpen();
                    return ;
                }else if(this.inited==true && val==false){
                    this.showing=false;
                    return ;
                }
            }else{
                if(this.inited==false){
                    this.initOpen();
                    return ;
                }else {
                    this.showing=!this.showing;
                    return ;
                }
            }
        },
        onchange(arr) {
            var mv=this;
            if(arr.length<1) return ;
            var tar=arr[0];
            if(tar.isIntersecting){
                //console.log('isIntersecting',this.$el,tar);
                this.initOpen();
                if(this.keep==false){
                    this.observer.disconnect();
                }
            }
            //console.log(arr);
        },

    }

}
</script>

<script setup>
import collapser from "@/ui/common/collapser.vue"
</script>