<template>
    <div class='emchart'>
        <g2render v-if="rending" 
            :option="option" :data="data"
            :height="height"
            :width="width"
            @eleclick="$emit('eleclick',$event)"
        ></g2render>
    </div>
</template>

<script>
//        <el-button @click="rendChart">刷新</el-button>

//最新项目进展 
import { loadJs } from "@/utils/loadJs.js"

import g2render from "./g2render.vue"

export default {
    name: "g2chart",
    components: {
        g2render
    },
    props: {
        type:{ type:String,default:"pie" },
        option: null,
        data:null,
    },
    emits: ["eleclick"],
    data() {
        return {
            mlist: [],
            chart: null,
            sizeListener: null,
            height: 0,
            width: 0,
            rending:false,
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
        this.rendChart();

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
        close(){
            this.chart.destroy();
        },
        rendChart() {
            this.rending=false;

            this.$nextTick(()=>{
                this.rending=true;
            })
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
                    vm.chart.render();
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