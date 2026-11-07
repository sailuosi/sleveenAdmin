<template>
    <div class='emchart'>

        <div class="echart-root" ref="eroot" :style="{ width: width + 'px', height: height + 'px' }"></div>
    </div>
</template>

<script>
//        <el-button @click="rendChart">刷新</el-button>

//最新项目进展 
import { loadJs } from "@/utils/loadJs.js"

export default {
    name: "emchart",
    components: {

    },
    props: {
        option: null,
    },
    emits: [],
    data() {
        return {
            mlist: [],
            chart: null,
            sizeListener: null,
            height: 0,
            width: 0,
        };
    },
    computed: {
        myid() {
            return "emchart" + Math.random();
        },
    },
    mounted() {
        this.readSize();
        this.lestenResize();
        this.initChart();
    },
    watch: {
        'option': function () {
            this.rendChart();
        },
    },
    destroyed() {
        if (this.sizeListener != null) {
            this.sizeListener.disconnect();
        }
    },
    methods: {
        initChart() {
            this.checkEnv(() => {
                var root = this.$refs.eroot;
                this.chart = echarts.init(root);
                this.rendChart();
            })

        },
        rendChart() {
            var opt = this.option;
            if (opt != null && this.chart != null) {
                this.chart.setOption(opt);
            }
        },
        lestenResize() {
            var vm = this;
            var objResizeObserver = new ResizeObserver(function (entries) {
                vm.$nextTick(() => {
                    vm.readSize();
                });
            });
            // 观察图片元素
            objResizeObserver.observe(this.$el);
            this.sizeListener = objResizeObserver;
        },
        readSize() {
            var vm = this;
            this.height = this.$el.clientHeight;
            this.width = this.$el.clientWidth;
            this.$nextTick(() => {
                if (vm.chart != null) {
                    vm.chart.resize();
                }
            });
        },
        checkEnv(callback) {
            var p4 = loadJs("/js/echarts.min.4.9.js");
            // Promise.all([p4]).then(()=>{
            //     //return loadJs("/ccflow/sys/js/flowdesign/leipi.flowdesign.v3.js");
            // }).then((res)=>{
            //     callback();
            // });
            p4.then((res) => {
                callback();
            })
        },
        
    }
}
</script>

<style scoped>
.emchart {

    height: 100%;
}
</style>