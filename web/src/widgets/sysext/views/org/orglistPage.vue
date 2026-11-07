<template>
<div>
    <!-- -->
    <div>
    <el-button type="primary" size="default" @click="refreshCodes">刷新单位层次码</el-button>    
    </div>

    
</div>
</template>

<script setup>
import { ElMessage, ElMessageBox } from 'element-plus'
import { calcOrgDeep } from '@s/api/system/mancommonapi.js'

function refreshCodes(){
    ElMessageBox.confirm(
        '确定要执行组织树的边界码生成码?将消耗一段时间！?',
        'Warning',
        {
        confirmButtonText: 'OK',
        cancelButtonText: 'Cancel',
        type: 'warning',
        }
    )
    .then(() => {
            calcOrgDeep({a:1}).then((res)=>{
                this.$message(res.data);
            })
    })



}
</script>
