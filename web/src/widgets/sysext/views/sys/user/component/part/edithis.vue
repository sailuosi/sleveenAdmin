<template>
<div>
    <!-- -->
    <el-button @click="clearAllHisCache" type="warning">清理他的所有缓存</el-button>

    <el-card style="max-width: 480px">
        <template #header>
        <div class="card-header">
            <span>测试区</span> <el-link @click="reset">清空</el-link>
        </div>
        </template>
        <el-button @click="loadAppPage" type="warning">APP页面接口</el-button>
        <div>
            {{testRestult}}
        </div>
    </el-card>


</div>
</template>

<script setup>


import { reactive, computed, nextTick, ref, onMounted,watch } from 'vue';
import { clearHisCache } from "@s/api/system/auth/dutycheck.js"
import { ElNotification } from 'element-plus'

const props = defineProps({
    account:null,
    vc:null,
})


/// APP门户接口测试
const testRestult=ref("");
function reset(){
    testRestult.value="";
}
function loadAppPage(){
    loadAppPageconfig({PP_AuthCode:props.account}).then((res)=>{
        testRestult.value=res;
    })

}

/// 清缓存
const detail=ref();

function clearAllHisCache(){

    clearHisCache({account:props.account}).then((res)=>{
        ElNotification({
            title: '提示',
            message:res.data,
            position: 'top-right',
            duration:4500,
        })
    })
}

watch(()=>props.link,()=>{
    //load();
})
onMounted(()=>{
    //load();
})

</script>