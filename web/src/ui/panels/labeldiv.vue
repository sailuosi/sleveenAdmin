<template>
    <div ref="mediv" :style="myStyle" :class="['labeldiv',inited?'waked':'sleeped']">
        <template v-if="!inited || !showing">
            <slot name="hide" :toggle="toggle"></slot>
        </template>
        <template v-else-if="!isBackOrder">
            <div :style="calcLabelStyle" class="lz-label" v-if="hasLabel">
                <slot name="label">
                    <label v-html="label" class="fool-label"></label>
                </slot>
            </div>
            <div :style="calcValueStyle" class="lz-value"  ref="lzval" v-if="inited">
                <slot :width="valWidth" :height="valHeight"></slot>
            </div>
        </template>
        <template v-else>
            <collapser>
                <div :style="calcValueStyle" class="lz-value" ref="lzval" v-if="inited">
                    <slot :width="valWidth" :height="valHeight"></slot>
                </div>              
            </collapser>
            <div :style="calcLabelStyle" class="lazy-label" v-if="hasLabel">
                <slot name="label">
                    <label v-html="label" class="fool-label"></label>
                </slot>
            </div>
        </template>
    </div>
</template>
<script>
//懒加载div,提供3种懒惰模式。
import collapser from  "@/ui/common/collapser.vue"
//import collapser from "./collapser.vue"
export default {
    name:"labeldiv",
    components:{collapser},
    props:{
        root:null,
        keep:{type:Boolean,default:false},
        margin:{type:String,default:"0px 0px 0px 0px"},
        emptyHeight:null,
        //seen--出现在视口内时展开，click--点击标题的展开按钮或空缺插槽时展开，man--被控制时展开。
        mode:{type:String,default:"seen"},
        visible:{type:Boolean},//由外界控制的展示、收起句柄。
        delay:{type:Number,default:-1},//延时，如果设置了，到达延时自动打开
        bodyStyle:{type:String,default:""},//内容体样式
        position:{type:String,default:"top"},
        label:{type:String,default:""},
        flex:{type:Boolean,default:false},//使用flex，此时为适应父级的大小
    },
    emits:["start","reset","init"],
    expose: ['toggle'],
    data(){
        return{
            observer:null,
            inited:false,
            showing:true,
            height:0,
            width:0,
            valHeight:0,
            valWidth:0,
            sizeListener:null,
        };
    },
    computed:{
        hasLabel(){
            return this.position != 'none';
        },
        myStyle(){
            if(this.flex){
                var tar= {
                    display:'flex',
                }
                if(this.isVertical){
                    tar["flex-direction"]="column"
                }
                return tar;
            }
            var res={
                display:'grid',
            };
            if(!this.inited && !!this.emptyHeight){
                res.height=this.emptyHeight;
            }
            if(this.isBackOrder){
                res["grid-template-rows"] = "1fr auto";
            }else{
                res["grid-template-rows"] =  "auto 1fr";
            }
            return res;
        },
        isVertical(){
            return this.position=="top"||this.position=="bottom"
        },
        isBackOrder(){
            return this.position=="right"||this.position=="bottom"
        },
        calcLabelStyle() {
            if(this.flex){
                return {
                    flex:'0 0 auto',
                }
            }
            return {}; 
        },
        //控制值部分的样式
        calcValueStyle() {
            if(this.flex){
                var tar= {
                    flex:'1',
                    overflow:'auto'
                }
                if(this.isVertical){
                    tar.height=0;
                }
                else{
                    tar.width=0;
                }
                return tar;
            }
            return {
                //'grid-column':"span "+width,
                //'grid-row':"span "+height
            };
        },
    },
    mounted() {
        //console.log("label-mount")
        var opts={
            rootMargin:this.margin,
            threshold:[0,0],
        };
        if(this.root!=null){
            opts.root=this.root;
        }
        if(this.mode=="seen"){
            if(this.observer !=null){
                this.observer.observe(this.$refs.mediv); 
            }
            else{
                this.observer = new IntersectionObserver(this.onchange, opts);
                this.observer.observe(this.$refs.mediv);                    
            }
        
        }
        if(this.delay>0){
            this.lazyer=setTimeout(() => {
                this.layOpen();
            }, this.delay);
        }

        this.lestenResize();
        
    },
    destroyed() {
        if(this.sizeListener!=null){
            this.sizeListener.disconnect();
        }
    },
    watch:{
        'visible':{
            handler:function(e){
                if(e==true){
                    if(this.inited==false ){
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
        'inited':function(){
            this.$nextTick((e)=>{
                this.readSize();
            })
        }
    },
    methods:{
        initOpen(){
            //console.log('i say init');
            this.inited=true;
            this.$emit("init");
            this.showing=true;
        },
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
            if(this.$refs.lzval != void 0){
                this.valHeight=this.$refs.lzval.clientHeight;
                this.valWidth=this.$refs.lzval.clientWidth;
            }
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
            //console.log('changelazy',arr);
            var mv=this;
            if(arr.length<1) return ;
            
            for(var i=0;i<arr.length;i++){
                var tar=arr[i];
                if(tar.isIntersecting){
                    //console.log('i am seen',tar);
                    this.initOpen();
                    if(this.keep==false){
                        this.observer.disconnect();
                    }
                    break;
                }                
            }

            //this.$
            //console.log(arr);
        },

    }

}
</script>

<script setup>

</script>
<style scoped>
.lz-value{

}
</style>