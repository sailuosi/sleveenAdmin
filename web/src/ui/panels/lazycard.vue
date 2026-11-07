<template>
<el-card class="titlediv" style=" min-inline-size: auto;" :class="{'opened':opened}">
    <template #header>
        <div class="lazycard-header" :style="titleStyle" >
            <span @click="togglecontent" style="cursor:pointer;">
                <svg v-if="togglable" :style="{transform:opened?'rotate(0deg)':'rotate(180deg)'}" class="icon" style="transition:all .5s ease-out;width: 1em; height: 1em;vertical-align: middle;fill: currentColor;" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="1566"><path d="M512 830.4l402.4-401.9c10.4-10.4 27.4-10.4 37.8 0 10.4 10.4 10.4 27.4 0 37.8L531.1 887c-5.2 5.2-12.2 7.8-19.1 7.8-6.9 0-13.8-2.6-19.1-7.8L71.8 466.4c-10.4-10.4-10.4-27.4 0-37.8 10.4-10.4 27.4-10.4 37.8 0L512 830.4z" fill="#040000" p-id="1567"></path><path d="M512 538.9L914.4 137c10.4-10.4 27.4-10.4 37.8 0 10.4 10.4 10.4 27.4 0 37.8L531.1 595.4c-5.2 5.2-12.2 7.8-19.1 7.8-6.9 0-13.8-2.6-19.1-7.8L71.8 174.8c-10.4-10.4-10.4-27.4 0-37.8 10.4-10.4 27.4-10.4 37.8 0L512 538.9z" fill="#040000" p-id="1568"></path></svg>
                <slot name="tail"><span v-html="title" ></span></slot>
            </span>            
            <slot name="header"></slot>
        </div>      
    </template>
    <template #default>
        <lazydiv v-if="rended" :visible="opened" ref="content" :delay="delay">
            <slot></slot>
        </lazydiv>
        <slot v-if="!rended || !opened" name="empty"></slot>
    </template>

</el-card>
</template>
<script>

/**
 主体内容可以收缩，且懒加载的卡片组件。
 懒惰模式分为：点击展开，被看见时展开，由prop控制等，
        <transition name="tdfade">
            <div v-if="rended" :style="contStyle" style="transition:all .5s ease-out;transform-origin:50% 0% 0;" ref="content" >
                <slot></slot>
            </div>
        </transition>  

*/

export default {
    name:"lazycard",
    props: {
        title: null,
        //seen--出现在视口内时展开，click--点击标题的展开按钮或空缺插槽时展开，prop--被控制时展开。
        mode:{type:String,default:"seen"},
        minHeight: { type: String, default: "1px" },
        height: { type: String, default: "auto" },
        titleStyle: null,
        togglable: { type: Boolean, default: true },
        //如果设置了懒加载，则初始必然不绘制。设置了open，则必然不是懒加载
        lazy: { type: Boolean, default: false },
        delay:null,
        //状态输出与受控
        name: null,
        value: null,
    },
    data: function () {
        var opened = !this.lazy;

        return {
            opened: opened,
            rended: opened,
            myheight: (opened ? this.height : this.minHeight),
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
<script setup>
import lazydiv from "./lazydiv.vue"

</script>
<style scoped>
div.lazycard-header{
    display: flex;
    flex-direction: row;
}
</style>