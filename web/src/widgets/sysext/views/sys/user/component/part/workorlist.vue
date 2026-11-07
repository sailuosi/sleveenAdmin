<template>
<div>
    <el-button @click="load" type="success">刷新</el-button>
    <vclist :list="detail" @query="getlist" orderCol :maxLine="1" 
    >
        <el-table-column fixed="left" label="操作" width="110px">
            <template #default="p">
                <el-link type="primary" @click="removeRow(p.row)" >删除</el-link>
            </template>
        </el-table-column>                
        <slot></slot>

        <vccol field="OrgName" caption="姓名" width="150px" ></vccol>
        <vccol field="Varchar1" caption="编码" width="90px" ></vccol>
        
        <vccol field="Dw_Task" caption="任务管理" type="dict" dict="KB_Usable" width="80px"></vccol>
        <vccol field="Dw_Config" caption="部门配置" type="dict" dict="KB_Usable" width="80px"></vccol>	            
        <vccol field="Dw_UseDept" caption="有部门看板" type="dict" dict="KB_Wether" width="80px"></vccol>
        <vccol field="Dw_UsePost" caption="有岗位看板" type="dict" dict="KB_Wether" width="80px"></vccol>
        <vccol field="Dw_PostScope" caption="岗位看板查看范围" type="dict" dict="KBPostLookScope" width="120px"></vccol>
        <vccol field="Dw_Main" caption="是否主岗" type="Boolean" width="80px"></vccol>
        <vccol field="Dw_Boss1" caption="行政负责人" type="Boolean" width="80px"></vccol>
        <vccol field="Dw_Boss2" caption="党政负责人" type="Boolean" width="80px"></vccol>
        <vccol field="Dw_Belong" caption="本部门人员" type="Boolean" width="80px"></vccol>
        <vccol field="Dw_Lv2Boss" caption="副职" type="Boolean" width="80px"></vccol>
        <vccol field="Dw_Type" caption="类型" type="dict" dict="KB_DeptWorkType" width="120px"></vccol>
        
        <vccol field="Dw_Scope" caption="业务范围"></vccol>

        <vccol field="Dw_Duty" caption="职责"></vccol>

        <vccol field="Dw_Moode" caption="类型"></vccol>
        <vccol field="SYS_Created" caption="创建日期" width="170px"></vccol>
        <vccol field="SYS_LAST_UPD" caption="最后修改日期" width="170px"></vccol>

    </vclist>
</div>
</template>

<script setup>
import { ElMessage, ElMessageBox } from 'element-plus'

import { reactive, computed, nextTick, ref, onMounted,watch } from 'vue';
import { loadHisWorkors } from "@s/api/system/auth/dutycheck.js"



import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"

const props = defineProps({
    account:null,
    vc:null,
})

const detail=ref();

function load(){

    loadHisWorkors({account:props.account}).then((res)=>{
        detail.value=res.data;

    })
}

watch(()=>props.account,()=>{
    load();
})
onMounted(()=>{
    load();
})


function removeRow(row){
    ElMessageBox.confirm('此操作将永久删除该记录, 是否继续?', '提示', {confirmButtonText: '确定',cancelButtonText: '取消',type: 'warning'})
    .then(() => {
        removeKB_DeptWorkor(row).then((res)=>{
            ElMessage({ type: 'success', message: res.msg});
            load();
        })
        
    })
}

</script>
