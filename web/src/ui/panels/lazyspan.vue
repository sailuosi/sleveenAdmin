<template>
    <span ref="mediv" :class="['lazyspan',inited?'waked':'sleeped']">
        <span v-if="inited" v-show="showing" :style="bodyStyle"><slot :toggle="toggle"></slot></span>        
        <template v-if="!inited || !showing">
            <slot name="hide" :toggle="toggle"></slot>
        </template>
    </span>
</template>
<script>
//懒加载div,提供3种懒惰模式。

export default {
    name:"lazyspan",
    props:{
        root:null,
        keep:{type:Boolean,default:false},
        //seen--出现在视口内时展开，click--点击标题的展开按钮或空缺插槽时展开，man--被控制时展开。
        mode:{type:String,default:"seen"},
        visible:{type:Boolean},//由外界控制的展示、收起句柄。
        delay:{type:Number,default:-1},//延时，如果设置了，到达延时自动打开
        bodyStyle:{type:String,default:""},//内容体样式
    },
    emits:["init","reset"],
    expose: ['toggle'],
    data(){
        return{
            observer:null,
            inited:false,
            showing:true,
        };
    },
    computed:{
    },
    mounted() {
        var opts={
            rootMargin:this.margin,
            threshold:[0,0],
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
                if(this.inited==false){
                    this.inited=e;                    
                }else{
                    //已经挂载的情况下，执行内容切换
                    this.showing=e;
                }

            },
            immediate: true
        },
    },
    methods:{
        initOpen(){
            this.inited=true;
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
<style scoped>
.lazyspan{
    display:inline-block;
    width:100%;
}
.lazyspan>span{
    display:inline-block;
    width:100%;
}
</style>