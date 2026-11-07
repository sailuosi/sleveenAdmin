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

        <vccol field="Di_Name" caption="责任名称" ></vccol>
        <vccol field="Di_Note" caption="责任说明" align="left" min-width="200px"></vccol>
        <vccol field="Wi_Name" caption="词条名称" align="left" min-width="110px" ></vccol>
        <vccol field="Wi_Code" caption="词条编号" align="left" width="90px" ></vccol>
        <vccol field="Wi_Note" caption="词条说明" ></vccol>

    </vclist>
</div>
</template>

<script setup>


import { reactive, computed, nextTick, ref, onMounted,watch } from 'vue';
import { loadhisDutys } from "@s/api/system/auth/dutycheck.js"

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

    loadhisDutys({account:props.account}).then((res)=>{
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
