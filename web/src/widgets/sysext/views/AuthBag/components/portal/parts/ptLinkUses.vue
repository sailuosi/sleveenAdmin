<template>
<div>

    <el-card class="box-card">
        <template #header>
            <div class="card-header">
                <slot name="header"></slot>
            </div>
        </template>
        <el-collapse >
            <el-collapse-item v-for="(lin,l) in detail?.PortalLinks" title="Consistency">
                <template #title>
                    应用在【{{lin.PL_Title}}】
                </template>
                <div>
                    名称：[{{lin.PL_Code}}]{{lin.PL_Title}}，位置：{{places[lin.PL_Place]?.label}}
                </div>
                <div>
                    
                </div>
            </el-collapse-item>

        </el-collapse>        
        <el-divider />
        <div class="info">
            <label class="lb">名称</label>
            <span class="vt">{{detail?.paotao?.P_Name}}</span>
            <label class="lb">key</label>
            <span class="vt">{{detail?.paotao?.P_Key}}</span>
            <label class="lb">参数</label>
            <span class="vt">{{detail?.paotao?.P_Para}}</span>
            <label class="lb">参数2</label>
            <span class="vt">{{detail?.paotao?.P_Style}}</span>
        </div>
        <el-divider />

    </el-card>

</div>
</template>

<script setup>
import {
    reactive,
    computed,
    nextTick,
    ref,
    onMounted,
    watch
} from 'vue';
import {
    getPoartalUses
} from "@s/api/system/paotalapi.js"

//
import {loadCode} from "@/utils/codevalue.js"

const props = defineProps({
    oid: null,
    vc: null,
})

const detail = ref();

function load() {

    getPoartalUses({
        ZH_PortalOID: props.oid
    }).then((res) => {
        detail.value = res.data;
    })
}


//
const places=ref([])
function loadCodeVals(){
    loadCode("KB_HOPtLinkPlace").then((res)=>{
        var obj={};
        res.forEach((li)=>{
            obj[li.value]=li;
        });
        places.value=obj;
    })    
}



watch(() => props.oid, () => {
    load();
})
onMounted(() => {
    load();
    loadCodeVals();
})
</script>

<style scoped>
.info {
    display: grid;
    grid-template-columns: 100px 1fr;
}
</style>
