<template>
<el-card class="steppanel" style="display:flex;flex:1;">

    <div ref="scrollbox" class="navbodyy" style="overflow: auto;flex:1;position:relative;" :style="{height:height,}">
        <slot></slot>
    </div>
</el-card>
</template>

<script>
    export default {
        name:"navpanel",
        emits:["navclick","navto"],
        props: {
            navs: null,
            anchors: null,
            defaultNav: null,
            height: { type: String, default: "100vh" },
            //是否滞后导航操作
            lazy: { type: Boolean, default: false },
            nonav:{ type: Boolean, default: false },
        },
        data() {
            return {
                activeMenu: this.defaultNav,//默认激活的导航
                notip: "数据正在加载中...",

                scrollBox: null,
                focusDevman: null,
            };
        },
        methods: {
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
            scrollToById(id, nav) {
                var vm = this;
                var el = document.getElementById(id);
                if (el != null) {
                    const drag = 10;
                    const tar = el.offsetTop;
                    var gap = this.scrollBox.scrollTop;

                    vm.scrollBox.scrollTo({ top: tar, behavior: "smooth" });
                }
                this.$emit("navto", id, nav);
            },
        },
        mounted() {
            // 获取滚动dom元素
            this.scrollBox = this.$refs.scrollbox;
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
    }


</script>

<style >

</style>