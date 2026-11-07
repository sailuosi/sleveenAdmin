<template>

    <inputsearch :multiple="multiple" @input="onput" @click="changeSelectRestFul" :onload="changeSelectRestFul" rowKey="oid">
        <template #option="{ row, index }">
            <div class="checkbox-line" >
                <span >{{index + 1}}</span>
                <span class="el-icon-s-home" style="margin-left:16px;margin-right:4px;">
                    <svg class="icon" style="width: 1em;height: 1em;vertical-align: middle;fill: currentColor;overflow: hidden;" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="2149"><path d="M576 832h128v64H576zM512 640c212.08 0 384 171.92 384 384h64c0-247.42-200.58-448-448-448S64 776.58 64 1024h64c0-212.08 171.92-384 384-384zM512 0C335.27 0 192 143.27 192 320s143.27 320 320 320 320-143.27 320-320S688.73 0 512 0z m0 576c-141.38 0-256-114.62-256-256S370.62 64 512 64s256 114.62 256 256-114.62 256-256 256z" p-id="2150"></path></svg>                    
                </span>
                <span >{{row.ManName}}</span>
                <span class="el-icon-s-home" style="margin-left:16px;margin-right:4px;">
                    <svg class="icon" style="width: 1em;height: 1em;vertical-align: middle;fill: currentColor;overflow: hidden;" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="5401"><path d="M585.14 864V531.85a59.33 59.33 0 0 1 29.27-51.13l79.3-46.65a59.37 59.37 0 0 1 60.18 0l79.29 46.65a59.34 59.34 0 0 1 29.24 51.13V864z m0 0" fill="#1792E5" p-id="5402"></path><path d="M543.1 978.55H179.57a90 90 0 0 1-89.94-89.94v-619a116.46 116.46 0 0 1 50-95.6L295 66.13a116.85 116.85 0 0 1 132.76 0L583 174a116.44 116.44 0 0 1 50 95.6v619a90 90 0 0 1-89.94 89.94zM361.35 115.48a46.46 46.46 0 0 0-26.4 8.23l-155.3 107.86a46.29 46.29 0 0 0-19.87 38v619a19.85 19.85 0 0 0 19.79 19.83H543.1a19.86 19.86 0 0 0 19.83-19.83v-619a46.29 46.29 0 0 0-19.88-38L387.74 123.71a46.4 46.4 0 0 0-26.39-8.23z m0 0" fill="#424242" p-id="5403"></path><path d="M898.89 967.21a35.06 35.06 0 0 1-35.06-35.07V468.65a52.4 52.4 0 0 0-23.84-44L525.45 219.37a35.07 35.07 0 0 1 38.35-58.73l314.51 205.24A122.33 122.33 0 0 1 934 468.64v463.49a35.08 35.08 0 0 1-35.06 35.07z m-449-503.7H262.75a35.06 35.06 0 0 1 0-70.11h187.11a35.06 35.06 0 0 1 0 70.11z m0 210.81H262.75a35.07 35.07 0 0 1 0-70.12h187.11a35.07 35.07 0 1 1 0 70.12z m0 0" fill="#424242" p-id="5404"></path><path d="M748.31 798.34a35.07 35.07 0 0 1-35.07-35.05V550.66a35.07 35.07 0 0 1 70.12 0v212.63a35.07 35.07 0 0 1-35.05 35.05z m0 0" fill="#424242" p-id="5405"></path></svg>
                </span>
                <span >{{row.OrgName}}/{{row.divName}}
                    <span class="el-icon-s-home" style="margin-left:16px;margin-right:4px;">
                    <svg class="icon" style="width: 1em;height: 1em;vertical-align: middle;fill: currentColor;overflow: hidden;" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="4461"><path d="M866.346667 89.6H207.573333C167.253333 89.6 134.4 122.453333 134.4 162.773333v146.346667h64V162.773333c0-5.12 4.266667-9.173333 9.173333-9.173333h658.773334c5.12 0 9.173333 4.053333 9.173333 9.173333v699.946667c0 5.12-4.053333 9.173333-9.173333 9.173333H207.573333c-5.12 0-9.173333-4.053333-9.173333-9.173333v-144h-64v144c0 40.32 32.853333 73.173333 73.173333 73.173333h658.773334c40.32 0 73.173333-32.853333 73.173333-73.173333V162.773333c0-40.32-32.853333-73.173333-73.173333-73.173333zM134.4 428.16h64V597.333333h-64zM120.96 400.64h82.346667c17.706667 0 32-14.293333 32-32s-14.293333-32-32-32H120.96c-17.706667 0-32 14.293333-32 32s14.506667 32 32 32zM233.386667 657.493333c0-17.706667-14.293333-32-32-32H119.04c-17.706667 0-32 14.293333-32 32s14.293333 32 32 32h82.346667c17.706667 0 32-14.293333 32-32zM384.213333 655.146667l146.346667 139.306666c6.186667 5.973333 14.08 8.746667 21.973333 8.746667s15.786667-2.986667 21.973334-8.746667l148.053333-139.306666c8.746667-8.106667 12.16-20.48 8.746667-32l-64.853334-228.053334 39.68-122.666666c3.2-9.813333 1.493333-20.48-4.48-28.586667s-15.573333-13.226667-25.813333-13.226667H428.586667a31.872 31.872 0 0 0-30.293334 41.813334l40.96 126.933333-63.786666 224c-3.2 11.306667 0.213333 23.68 8.746666 31.786667z m168.533334 72.32l-110.506667-105.173334 56.96-199.893333h108.373333l56.96 199.893333-111.786666 105.173334z m78.933333-432.64l-19.413333 60.16h-120.32l-19.413334-60.16h159.146667z" p-id="4462"></path></svg>
                </span>
                    {{ row.H_PostName }}</span>     
            </div>
        </template>
        <slot></slot>
    </inputsearch>


</template>

<script>
/** 任意选人组件，UI绘制方式为直接绘制，无弹窗。
 * 
 */
import {  loadManSearch } from "@s/api/system/mancommonapi.js";

// 任意人员选择器
import inputsearch from "@/ui/common/input/inputsearch.vue"
export default {
    name:"mansearch",
    components:{
        inputsearch
    },
    props:{
        multiple:{ type:Boolean,default:false, },
        deptcode:{ type:String,default:""},
    },
    emits:["change","update:modelValue"],
    data(){
        return{

        };
    },
    //inject: ['deptcode'],
    methods:{
        onput(tar,tar2){
            this.$emit("update:modelValue",tar,tar2);
            this.$emit("change",tar,tar2);
        },
        changeSelectRestFul(inputKey,cb) {
            var canshu = {
                "value": inputKey,
                deptcode: this.deptcode
            };
            return loadManSearch(canshu).then((res) => {
                if (res.status =="success") {
                     return res.data;
                }
                return [];
            })
        },
    }
}
</script>
<style scoped>
.checkbox-line {
    border-bottom: 1px solid #ccc;
}
.el-icon-s-home{
    display: inline-block;
}
</style>