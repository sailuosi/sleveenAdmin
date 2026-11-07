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

        <vccol field="Status" caption="启用" type="Boolean" width="60px"></vccol>
        <vccol field="Name" caption="标题" align="left" min-width="200px"></vccol>
        <vccol field="OrderNo" caption="序号" align="left" min-width="110px" ></vccol>
        <vccol field="DataScope" caption="数据权限" min-width="300px" align="left" type="dict" dict="HH_RoleDataScope"></vccol>

    </vclist>
</div>
</template>

<script setup>


import { reactive, computed, nextTick, ref, onMounted,watch } from 'vue';
import { loadHisRoles } from "@s/api/system/auth/dutycheck.js"

import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"

const props = defineProps({
    account:null,
    vc:null,
})

const detail=ref();

function load(){

    loadHisRoles({account:props.account}).then((res)=>{
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
