<template>
<div class="steppanel" :style="{ display: printMode ? 'block' : 'flex', flex: printMode ? 'none' : 1 }">
    <div class="leftnav" style="display:flex;flex-direction: column;overflow:auto;" v-if="navs?.length>0">
        <slot name="navtop"></slot>
        <el-menu :default-active="activeMenu" class="navmenu" style="width:160px;" @open="onMenuOpen" @close="onMenuClose">
            <template v-for="(nav,n) in navs" >
                <el-sub-menu v-if="Array.isArray(nav.children)&& nav.children.length>0" :index="nav.id" @click="navclick(nav.anchor)" :key="n + '_1'">
                    <template #title>
                        <i :class="nav.icon"></i>
                        <span v-html="nav.title" :style="nav.style"></span>
                        <span v-if="!!nav.badge" class="navpbadge">{{nav.badge}}</span>
                    </template>
                    <template #default>
                        <el-menu-item v-for="(cn,c) in nav.children" :index="cn.id" @click="navclick(cn.anchor,cn)" :key="n*c">
                            <i :class="cn.icon" v-if="cn.icon"></i>
                            <span>{{cn.title}}</span>
                            <span v-if="!!cn.badge" class="navpbadge">{{cn.badge}}</span>
                        </el-menu-item>
                    </template>

                </el-sub-menu>
                <el-menu-item v-else :key="n + '_2'" :index="nav.id" @click="navclick(nav.anchor,nav)" :class="nav.css">
                    <template #default>
                        <i :class="nav.icon" v-html="!!nav.pre?nav.pre:''"></i>
                        <span v-html="nav.title" :style="nav.style"></span>
                        <span v-if="!!nav.badge" class="navpbadge">{{nav.badge}}</span>                    
                    </template>

                </el-menu-item>
            </template>
        </el-menu>
        <slot name="navtail"></slot>
    </div>
    <div ref="navbodyy" class="navbodyy" :style="{
        overflow: printMode ? 'visible' : 'auto',
        flex: printMode ? 'none' : 1,
        position: printMode ? 'static' : 'relative',
        height: height
    }">
        <slot></slot>
    </div>
</div>

</template>

<script>
// import jQuery from 'jquery';

export default {
    name: 'navpanel',

    props: {
        navs: null,
        anchors: null,
        defaultNav: null,
        height: {
            type: [String,Number],
            default: "100vh"
        },
        //是否滞后导航操作
        lazy: {
            type: Boolean,
            default: false
        },
        printMode: {
            type: Boolean,
            default: false
        },

    },
    data() {
        return {

            activeMenu: this.defaultNav, //默认激活的导航
            notip: "数据正在加载中...",

            scrollBox: null,
            focusDevman: null,

        };
    },
    methods: {

        onMenuOpen(e) {
            //console.log(arguments);
        },
        onMenuClose(e) {
            //console.log(arguments);
        },
        //滚动协同。
        checkScroll(e) {
            var vm = this;
            var anchors = this.anchors;
            //document.getElementById("outline").scrollTop
            const curTop = this.scrollBox.scrollTop;
            for (let i = 0; i < anchors.length; i++) {
                var el = document.getElementById(anchors[i]);
                if (el != null && curTop <= el.offsetTop) { // 根据滚动距离判断应该滚动到第几个导航的位置
                    vm.activeMenu = anchors[i];
                    break;
                }
            }
        },
        navclick(id, nav) {
            this.$emit("navclick", id, nav);
            if (this.lazy) {
                this.$nextTick(() => {
                    this.scrollToById(id, nav);
                });
            } else {
                this.scrollToById(id, nav);
            }
        },
        scrollToById(id, nav) {

            var vm = this;
            var el = document.getElementById(id);
            if (el != null) {
                const drag = 10;
                const tar = el.offsetTop;
                var gap = this.scrollBox.scrollTop;

                vm.scrollBox.scrollTo({
                    top: tar,
                    behavior: "smooth"
                });
            }
            this.$emit("navto", id, nav);
        },
    },
    mounted() {
        // 获取滚动dom元素
        this.scrollBox = this.$refs.navbodyy;
        // const jump = jQuery('.do-jump')
        // const topArr = []
        // for (let i = 0; i < jump.length; i++) {
        //     topArr.push(jump.eq(i).position().top)
        // }
        // 监听dom元素的scroll事件
        function throttling(fn, wait, maxTimelong) {
            var timeout = null,
                startTime = Date.parse(new Date);

            return function () {
                if (timeout !== null) clearTimeout(timeout);
                var curTime = Date.parse(new Date);
                if (curTime - startTime >= maxTimelong) {
                    fn();
                    startTime = curTime;
                } else {
                    timeout = setTimeout(fn, wait);
                }
            }
        }
        this.scrollBox.addEventListener('scroll', throttling(this.checkScroll, 300, 1000), true);
    },
    created() {

    },
};
</script>

<style scoped>
.navpbadge {
    background-color: #f56c6c;
    border-radius: 10px;
    color: #fff;
    display: inline-block;
    font-size: 12px;
    height: 18px;
    line-height: 18px;
    padding: 0 6px;
    text-align: center;
    white-space: nowrap;
    border: 1px solid #fff;
    transform: translateY(-10px);
}

.navmenu {
    flex: 1;
}

.leftnav .navmenu .el-menu-item {
    padding-right: 6px;
}

.navbodyy {
    border-left: solid 1px #e6e6e6;
    background-color: rgb(246, 247, 249);
    padding:var(--navbody-pad,0);
}
.navbodyy :deep(.anchtitle) {
    margin-bottom: 1em;
    background: #f3f5f7;
    padding: 10px 8px;
    font-size: 18px;
    font-weight: bold;
    margin-left: 0;
    border-left-width: 1em;
    border-left-style: solid;
    border-left-color: #1e88e5;
    border-bottom: 2px solid #1e88e5;
    top:0;
    z-index:1;
}
.navbodyy :deep(.anchpan>.anchlabel) {
    border-left: 4px solid rgb(39, 86, 156);
    line-height: 16px;
    margin: 0px 16px;
    height: 16px;
    position: relative;
    font-size: 17px;
    padding-left:12px;
    font-weight: bold;
    top:0;
    z-index:1;
    border-bottom: 1px solid rgb(239, 240, 244);
    align-self: center;
}

.navbodyy :deep(.anchpan) {
    border-bottom: 1px solid rgb(101 101 101 / 48%);
    margin-bottom: 10px;
    top:0;
    z-index:1;
    display: flex;
    align-items: center;
    justify-content: space-between;
    min-height:40px;
    background: #f3f5f7;
}
.navbodyy :deep(.anchpan>.ucml-vc-toolbar) {
    flex:0 0 auto;
}

.navbodyy :deep(.anchtitle.sticky) {
    position:sticky; 
}
.navbodyy :deep(.anchpan.sticky) {
    position:sticky; 
}
.navbodyy :deep(.anchpan.span2) {
    display:grid;
    grid-column:1fr 1fr;
}
.navbodyy :deep(.anchpan.span3) {
    display:grid;
    grid-column:1fr 1fr 1fr;
}

.navbodyy :deep(.mb8) {
    margin-bottom:8px;
}
</style>
