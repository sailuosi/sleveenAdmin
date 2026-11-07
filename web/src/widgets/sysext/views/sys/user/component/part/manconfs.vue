<template>
<div>
    <el-button @click="load" type="success">刷新</el-button>
    <vclist :list="detail" @query="getlist" orderCol :maxLine="1" 
    >
        <el-table-column v-if="!selecting"   fixed="left" label="操作" width="110px">
            <template #default="p">
                <el-link type="primary" @click="openRow(p.row)" >编辑</el-link>
            </template>
        </el-table-column>                
        <slot></slot>
        <vccol field="SYS_Deleted" caption="删除标记" type="Boolean" width="60px"></vccol>
        <vccol field="MC_MainName" caption="姓名"  width="60px"></vccol>
        <vccol field="MC_Code" caption="编号" align="left" width="90px"></vccol>
        <vccol field="KD_Name" caption="部门" align="left" min-width="110px" ></vccol>
        <vccol field="KD_Code" caption="部门编号" align="left" width="90px" ></vccol>
        <vccol field="KP_Name" caption="岗位名称" align="left" min-width="110px" ></vccol>
        <vccol field="KP_Info" caption="岗位描述" min-width="300px" align="left" ></vccol>
        

    </vclist>
</div>
</template>

<script setup>


import { reactive, computed, nextTick, ref, onMounted,watch } from 'vue';
import { loadHisManConfs } from "@s/api/system/auth/dutycheck.js"

import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"

const props = defineProps({
    account:null,
    vc:null,
})

const detail=ref();

function load(){

    loadHisManConfs({account:props.account}).then((res)=>{
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
