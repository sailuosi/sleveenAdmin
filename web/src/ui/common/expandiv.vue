<template>
<div :class="{'opened':opened}" ref="mediv">
    <div class="ttdheader" style="titleStyle" >
        <span @click="togglecontent">
            <svg v-if="!!title && togglable" :style="{transform:opened?'rotate(0deg)':'rotate(180deg)'}" class="icon" style="transition:all .5s ease-out;width: 1em; height: 1em;vertical-align: middle;fill: currentColor;" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="1566"><path d="M512 830.4l402.4-401.9c10.4-10.4 27.4-10.4 37.8 0 10.4 10.4 10.4 27.4 0 37.8L531.1 887c-5.2 5.2-12.2 7.8-19.1 7.8-6.9 0-13.8-2.6-19.1-7.8L71.8 466.4c-10.4-10.4-10.4-27.4 0-37.8 10.4-10.4 27.4-10.4 37.8 0L512 830.4z" fill="#040000" p-id="1567"></path><path d="M512 538.9L914.4 137c10.4-10.4 27.4-10.4 37.8 0 10.4 10.4 10.4 27.4 0 37.8L531.1 595.4c-5.2 5.2-12.2 7.8-19.1 7.8-6.9 0-13.8-2.6-19.1-7.8L71.8 174.8c-10.4-10.4-10.4-27.4 0-37.8 10.4-10.4 27.4-10.4 37.8 0L512 538.9z" fill="#040000" p-id="1568"></path></svg>
            <slot name="title"><span v-html="title" ></span></slot>
        </span>            
        <slot name="tail"></slot>
    </div>
    <transition name="tdfade">
        <div  :style="contStyle" style="transition:all .3s ease-in-out;transform-origin:50% 0% 0;" ref="content" @transitionend="ontoggleEnd">
            <template v-if="rended">
                <slot></slot>
            </template>
        </div>
    </transition>
</div>
</template>
<script>


export default {
    name:"expandiv",
    props: {
        title: null,
        minHeight: { type: String, default: "0px" },
        initHeight: { type: String, default: "auto" },
        titleStyle: null,
        togglable: { type: Boolean, default: true },
        //状态输出与受控
        root:null,
        keep:{type:Boolean,default:false},
        margin:{type:String,default:"0px 0px 0px 0px"},
        emptyHeight:null,
        //seen--出现在视口内时展开，click--点击标题的展开按钮或空缺插槽时展开，man--被控制时展开。
        mode:{type:String,default:"seen"},
        name:null,
        visible:{type:Boolean},//由外界控制的展示、收起句柄。
    },
    data: function () {
        var opened = this.visible;
        return {
            opened: false,
            rended: false,
            myheight: (opened ? this.initHeight : this.minHeight),
            oldHeight:0,
            oldPaddingTop:0,
            oldPaddingBottom:0,
        };
    },
    computed: {
        contStyle() {
            var show = this.opened ? 1 : 0;
            var height=isNaN(this.myheight)?this.myheight:this.myheight+'px';
            var res = {
                height: height,
                //opacity: show,
                //transform: 'scale(1,' + show + ')'
            };
            if(!this.opened){
                res.overflow="hidden";
            }
            return res;
        },
    },
    methods: {
        togglecontent() {
            if (!this.togglable) return;
            var vm = this;
            var el=this.$refs.content;
            if (this.rended == false) {
                this.rended = true;
            }
            if (this.opened) {//执行关闭，关闭前，把元素的高度属性设置为样式高度
                this.opened = false;
                if (el.scrollHeight !== 0) {
                    el.style.height = el.scrollHeight + 'px';
                }
                this.myheight = this.minHeight;
                if(el!=void 0){
                    vm.oldHeight = el.scrollHeight;
                    vm.oldPaddingTop = el.style.paddingTop;
                    vm.oldPaddingBottom = el.style.paddingBottom;                       
                }
            } else {
                //执行打开
                if(this.oldHeight==0){
                    this.myheight=this.initHeight;
                }else{
                    this.myheight=this.oldHeight;
                }
                this.opened = true;
                
            }
            this.$emit('change', this.opened);
        },
        ontoggleEnd(){
            //在打开动作完成之后，如果是打开，需要把高度还原为auto;
            if(this.opened){
                this.myheight="auto";
            }
        },
        onchange(arr) {
            var mv=this;
            if(arr.length<1) return ;
            var tar=arr[0];
            if(tar.isIntersecting){
                if(this.rended==false){
                    //初次打开
                    this.rended=true;
                    this.opened=true;
                    this.myheight=this.initHeight;
                    this.$emit('change', this.opened);                    
                }
                else{
                    this.opened=true;
                    this.myheight=this.oldHeight==0?this.initHeight:this.oldHeight;
                    this.$emit('change', this.opened);   
                }

                if(this.keep==false){
                    this.observer.disconnect();
                }
            }
            //console.log(arr);
        },
    },
    watch: {
        visible: {
            handler(newval, oldval) {
                if (newval == void 0 || newval == null) return;
                var needOpen = (typeof newval == 'boolean' && newval == true) || newval == this.name || (Array.isArray(newval) && newval.indexOf(this.name) != -1);
                if (needOpen != this.opened) {
                    this.togglecontent();
                }
            },
            immediate: true
        },
    },
    mounted() {
        var opts={
            rootMargin:this.margin,
            threshold:[0,0]
        };
        if(this.root!=null){
            opts.root=this.root;
        }
        if(this.mode=="seen"){
            this.observer = new IntersectionObserver(this.onchange, opts);
            this.observer.observe(this.$refs.mediv);            
        }
    },
}
</script>

<style scoped>
.ttdheader{
    line-height:32px;
    padding:5px;
    border-bottom:1px solid #efefef;
    cursor:pointer;
}
</style>
