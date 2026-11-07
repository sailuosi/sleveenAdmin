<template>

    <inputsearch :multiple="multiple" @input="onput" @click="changeSelectRestFul" :onload="changeSelectRestFul" rowKey="oid">
        <template #option="{ row, index }">
            <div class="checkbox-line" >
                <span >{{index + 1}}</span>
                <span class="el-icon-s-custom" style="margin-left:16px;margin-right:4px;"></span>
                <span >{{row.ManName}}</span>
                <span class="el-icon-s-home" style="margin-left:16px;margin-right:4px;"></span>
                <span >{{row.OrgName}}/{{row.divName}}</span>     
            </div>
        </template>
        <slot></slot>
    </inputsearch>


</template>

<script>
/** 任意选人组件，UI绘制方式为直接绘制，无弹窗。
 * 
 */
import {  loadManSearch } from "@/api/system/mancommonapi.js";

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

</style>