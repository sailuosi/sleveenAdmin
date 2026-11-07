<template>
<div>
    <el-button @click="load" type="success">刷新</el-button>
    查看被缓存的<el-switch v-model="seeCached"  size="large" />
    <span></span>
    
    <vclist :list="seeCached?detail.cached:detail.now" @query="getlist" orderCol :maxLine="1" 
    >
        <el-table-column v-if="!selecting"   fixed="left" label="操作" width="110px">
            <template #default="p">
                <el-link type="primary" @click="openRow(p.row)" >编辑</el-link>
            </template>
        </el-table-column>                
        <slot></slot>

        <vccol field="Vb_Type" caption="类型" type="Boolean" width="60px"></vccol>
        <vccol field="Vb_Name" caption="标题" align="left" min-width="200px"></vccol>
        <vccol field="Vb_Note" caption="说明" align="left" min-width="110px" ></vccol>
        <vccol field="SYS_Created" caption="创建时间" ></vccol>

    </vclist>
</div>
</template>

<script setup>


import { reactive, computed, nextTick, ref, onMounted,watch } from 'vue';
import { loadhisVisitors } from "@s/api/system/auth/dutycheck.js"

import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"

const props = defineProps({
    account:null,
    vc:null,
})

const detail=ref({
    cached:[],
    now:[]
});
const seeCached=ref(false);

function load(){

    loadhisVisitors({account:props.account}).then((res)=>{
        detail.value=res.data;

    })
}

watch(()=>props.account,()=>{
    load();
})
onMounted(()=>{
    load();
})

</script>
