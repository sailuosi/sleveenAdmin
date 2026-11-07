<template>
    <div>
        <el-button @click="dook" type="primary">确 定</el-button>
        <el-button @click="loadData" style="margin-left: 10px;">刷新</el-button>
        <div style="max-height: 60vh;overflow: auto;">
           
        <el-checkbox-group v-model="checkList" @change="onChange">
            <el-checkbox v-for="(li,i) in opts" :label="li.WT_Title" :value="li.HH_WorkorTypeOID" 
                :disabled="added.indexOf(li.HH_WorkorTypeOID)!=-1"
            >
            <span :title="li.WT_Note">{{li.WT_Title}}</span>
            </el-checkbox>
        </el-checkbox-group>
        </div>   
    </div>
</template>

<script setup>
import { useSlots, useAttrs, provide, inject,ref,onMounted } from 'vue'



const props = defineProps({
    added: { type:Array,default:function(){ return []; } },
    api:null,
    src:null,
})

const emit = defineEmits(['change', 'delete'])

const count = defineModel("count", { type: Number, default: 0 })

provide('path', '/project/')
const path = inject('path')

defineOptions({
    inheritAttrs: false,
    customOptions: {
        /* ... */
    }
})

const slots = useSlots()
const attrs = useAttrs()
const vMyDirective = {
    beforeMount: (el) => {
        // 在元素上做些操作
    }
}



const opts=ref([]);
const checkList=ref([]);
function loadData(){
    props.api.loadWorkTypeSelect({src:props.src}).then((res)=>{
        opts.value=res.data;
    })
}
function onChange(){
    emit('change',checkList.value);
}
function dook(){
    var list=[];
    var val=[];
    opts.value.forEach((li)=>{
        if(checkList.value.indexOf(li.HH_WorkorTypeOID)!=-1 && props.added.indexOf(li.HH_WorkorTypeOID)==-1){
            list.push(li);
            val.push(li.HH_WorkorTypeOID);
        }
    })
    emit('ok',list,val);
}

onMounted(()=>{
    loadData();
})

defineExpose({

})
</script>