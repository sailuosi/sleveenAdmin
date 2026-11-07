<template>
    <div class="echart-root" ref="eroot" :style="{ width: width + 'px', height: height + 'px' }"></div>
</template>

<script>
//        <el-button @click="rendChart">刷新</el-button>

//最新项目进展 
import { loadJs } from "@/utils/loadJs.js"

import { Pie } from '@antv/g2plot';

export default {
    name: "g2render",
    components: {

    },
    props: {
        type:{ type:String,default:"pie" },
        option: null,
        data:null,
        height:null,
        width:null,
    },
    emits: ["eleclick"],
    data() {
        return {

        };
    },
    computed: {
        myid() {
            return "emchart" + Math.random();
        },
        rendOpt(){},
    },
    mounted() {
        this.rendChart();
    },
    watch: {
        'option': function () {
            this.rendChart();
        },
    },
    methods: {
        close(){
            this.chart.destroy();
        },
        rendChart(opt) {
            // this.checkEnv(() => {
            //     var root = this.$refs.eroot;
            //     this.chart = echarts.init(root);
            //     this.rendChart();
            // })
            if(opt==void 0){
                opt = Object.assign(this.option);
            }
            //{"type":"法律合规风险","value":18},
            /*
            var data=[
                
                {"type":"战略风险","value":1},
                {"type":"投资风险","value":1},
                {"type":"运营风险","value":1},
                {"type":"法律合规风险","value":15},
            ];*/
            
            opt.data=this.data;
            var op=JSON.parse(JSON.stringify(opt));
            if(!Array.isArray(op.data)){
                return ;
            }
            //console.log(op);
            var root = this.$refs.eroot;
            if(this.type=="pie"){

                
                this.chart = new Pie(root, op);
                //window.chart = this;
            }
            this.chart.on('element:click', (evt) => {
                this.$emit("eleclick",evt)
            });
            this.chart.render();
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