<template>
<div >
    <el-tabs type="border-card">
        <el-tab-pane label="角色" lazy>
            <rolelist :account="account" class="tuh-pan"></rolelist>
        </el-tab-pane>
        <el-tab-pane label="所属访客" lazy>
            <visitlist :account="account" class="tuh-pan"></visitlist>
        </el-tab-pane>
        <el-tab-pane label="责任条目" lazy>
            <wordlist :account="account" class="tuh-pan"></wordlist>
        </el-tab-pane>
        <el-tab-pane label="单位管理员" lazy>
            <workorlist :account="account" class="tuh-pan"></workorlist>
        </el-tab-pane>
        <el-tab-pane label="岗位人员" lazy>
            <manconfs :account="account" class="tuh-pan"></manconfs>
        </el-tab-pane>
        <el-tab-pane label="操作" lazy>
            <edithis :account="account" class="tuh-pan"></edithis>
        </el-tab-pane>

    </el-tabs>
</div>
</template>

<script setup>

import { reactive, computed, nextTick, ref, onMounted,watch } from 'vue';


import rolelist from "./part/rolelist.vue"
import visitlist from "./part/visitlist.vue"
import wordlist from "./part/wordlist.vue"
import workorlist from "./part/workorlist.vue"
import manconfs from "./part/manconfs.vue"
import edithis from "./part/edithis.vue"


const props = defineProps({
    account:null,
    vc:null,
})

const detail=ref();

function load(){


}

watch(()=>props.link,()=>{
    //load();
})
onMounted(()=>{
    //load();
})

</script>
<style scoped>
.tuh-pan{
    height:75vh;
    overflow:auto;
}
</style>