<template>
<div>

    <el-button type="primary" size="default" @click="addRule">添加新的配置分组</el-button>
    <el-button type="warning" size="default" @click="apply">应用</el-button>
    
    <div v-for="(row,r) in config" class="conf-group">
        <div>
            <el-radio-group v-model="row.type" size="small">
                <el-radio-button v-for="(li,i) in ruleTypes" :label="li.value" :value="li.value" >{{li.label}}</el-radio-button>
            </el-radio-group>
        </div>

        <editconfig :fields="fields" :rule="row.rule"/>
        <el-transfer v-model="row.fields" :data="fields" :props="tranProp" />    
    </div>

</div>
</template>

<script setup>
//字段配置组件，读取字段，解析和保存配置结果
//v-model="[]"
import { reactive, computed, nextTick, ref, onMounted,watch } from 'vue';
import editconfig from "./editconfig.vue"

//import { fetchSysPageFieldPage ,removeZHPageField} from "@/api/system/paotou/pagefieldapi.js"

const props = defineProps({
    modelValue:null,
    pageOID:null,
})
const emit = defineEmits(['update:modelValue',]);

//主配置属性
const innerVal=ref("");
const config=ref();

function loadConfig(){
    if(innerVal.value != props.modelValue ){
        innerVal.value = props.modelValue;
        try{
            config.value=JSON.parse(props.modelValue);
        }
        catch(e){}
        
        if(!Array.isArray(config.value)){
            config.value=[{
                rule:{},
                fields:[],
            }]
        }
    }
}

function addRule(){
    config.value.push({
        rule:{},
        fields:[],
    });
}

function apply(){
    var tar=JSON.stringify(config.value);
    emit('update:modelValue',tar);
    
}

const tranProp=ref({
    label:"PF_Caption",
    key:"PF_Field",
});

const ruleTypes=ref([
    {label:"可见控制",value:"visible"},
    {label:"编辑控制",value:"editable"},
]);

//字段查询属性

const fields=ref([]);
const fieldTotal=ref(0);

function getlist(){
    var para={
        spsId:props.pageOID,
    }
    fetchSysPageFieldPage(para).then((res)=>{
        fields.value=res.data;
        fieldTotal.value=res.total;
    })
}

watch(()=>props.modelValue,()=>{
    loadConfig();
})
watch(()=>props.pageOID,()=>{
    getlist();
})

onMounted(()=>{
    loadConfig();
    getlist();
});

</script>

<style scoped>
.conf-group{
    margin:10px 0;
}
</style>
