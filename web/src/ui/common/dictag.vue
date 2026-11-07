<template>
<span>
    <template v-if="multi">
        <el-tag v-for="(na,n) in inputval" :key="n">{{na}}</el-tag>
    </template>
    <template v-else>
        <el-tag>{{inputval}}</el-tag>
    </template>
</span>
</template>

<script >

import { loadCode } from "@/utils/codevalue.js";

export default {
    name: "dictag",
    data() {
        return {
            renderlist:[],
        };
    },
    props:{
        dict: {    type: String},
        value: [String,Number,Array],
        placeholder: {    type: String,    default: "无",  },
        // 
        dictlist: {    type: Array,   },
        multi:{type:Boolean,default:false},
    },
    computed:{
        nameMap(){
            var res={};
            this.renderlist.forEach((dict)=>{
                res[dict.value+'']=dict.label;
            })
            return res;
        },
        inputval(){
            //因为字典全是字符串，这里进行一个转换。
            if(this.multi){
                if(Array.isArray(this.value)){
                    var res=[];
                    res=this.value.map((val)=>this.getValName(val));
                    return res;
                }
                return [this.placeholder];
            }
            else{
                return this.getValName(this.value);
            }
        },
    },
    created() {

        if(this.dict==void 0){
            this.renderlist=this.dictlist;
        }else{
            loadCode(this.dict).then((res)=>{
                this.renderlist=res;
            })
        }    
    },
    watch:{
        dictlist:function(val){
            this.renderlist=val;
        }
    },

    methods: {
        getValName(val){
            var tar=val+'';
            if(this.nameMap[tar] !=void 0){
                return this.nameMap[tar];
            }
            else{
                return this.placeholder;
            }
        }
    }
};

</script>
