<template>
<span>
    <el-select v-model="value" @change="sayInput">
        <el-option v-for="(li,i) in mlist" :key="i" :label="li.name" :value="li.oid" />
    </el-select>
</span>
</template>

<script>

import { loadWordsForSelect } from "../../api/HHWordDefineapi.js"

export default {
    name: 'WebWordselect',
    emits:["select","update:word"],
    props:{

    },
    data() {
        return {
            mlist:[],
            value:null,
        };
    },

    mounted() {
        this.getList();
    },

    methods: {
        getList(){
            var para={};
            loadWordsForSelect(para).then((res)=>{
                this.mlist=res.data;
            })
        },
        sayInput(){
            var tar= this.mlist.filter((li)=>li.oid==this.value);
            this.$emit("select",this.value,tar[0]);
            this.$emit("update:word",tar[0]);
        },
    },
};
</script>
