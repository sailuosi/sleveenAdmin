<template>
<el-tree :data="mlist" default-expand-all :props="defaultProps" @node-click="handleNodeClick" />
</template>

<script>

import { fetchSysAppletCataPage } from "@s/api/paotou/paotalapi.js"

export default {
    name: 'WebPtcatatree',

    data() {
        return {
            mlist:[],
            total:0,
            defaultProps :{
                children: 'children',
                label: 'AC_Name',
            }
        };
    },

    mounted() {
        this.getlist();
    },
    methods:{
        getlist(){
            var para={
                pageSize:30,
                pageNum:1,
                asTree:true,
            }
            
            fetchSysAppletCataPage(para).then((res)=>{
                this.mlist=res.data;
                this.total=res.total;
            })
        },
        handleNodeClick(e){
            //console.log(e);
            this.$emit("nodeclick",e);
        },
    }
};
</script>

<style scoped>

</style>