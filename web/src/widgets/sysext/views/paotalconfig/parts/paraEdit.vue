<template>
<div>
    <el-button @click="add" type="warning">添加</el-button>
    <el-button @click="save" type="primary">应用</el-button>
    <fieldset v-for="(li,i) in rules" class="rule-row">
        <legend>规则</legend>
        <el-form class="rrule-fm normal" label-width="80px" label-position="left">
            <fminput v-model="li.mode" label="模式" type="dict" :dictlist="modes"></fminput>
            <el-form-item label="适用人员" v-if="li.mode.indexOf('man') !=-1">
                <tags :list="li.mans" name="name" class="tagsinfo" :closable="!disabled" 
                    @close="removeDutyMan" @deleteall="deleteallman(li)" 
                    :addable="!disabled" action="pop" width="430px" :placement="placement">
                    <template #addpop="p">
                        <manseach ref="comselect" v-model="addingEmp" multiple class="mansh">
                            <span class="dialog-footer">
                                <el-button type="primary" @click="doDutyManAdd(li)">确 定</el-button>
                            </span>
                        </manseach>

                    </template>
                </tags>
            </el-form-item>
            <el-form-item label="适用单位" v-if="li.mode.indexOf('org') !=-1">
                <tags :list="li.orgs" name="name" :closable="!disabled" @close="doRemoveDutyOrg($event,li)" :addable="!disabled"
                    @add="openOrgSelect(li)">
                </tags>
            </el-form-item>
            
            <fminput v-model="li.condition" label="参数定义" class="parabody" type="textarea"></fminput>
        </el-form>
    </fieldset>
    <el-dialog   v-model="seeHR"  width="300px" top="5vh"   :close-on-click-modal="false" append-to-body>
        <template #title><div>
            <el-button type="info" icon="el-icon-close" @click="seecfg=false"></el-button>
        </div></template>
        <hrtree @nodeclick="addOrg"></hrtree>
    </el-dialog>
</div>
</template>

<script setup>

//参数配置组件。
/**
[
    {
        mode:'man',//org-- 按组织; man -- 按人;manorg-- 按组织+人:
        mans:[
            {
                code:80045348,
                name:"韩辉"
            }
        ],
        orgs:[
            {
                oid:"",
                code:"",
                name:"",
            }
        ],
        condition:"",
    }
]
 */
import { reactive, computed, nextTick, ref, onMounted,watch } from 'vue';
import fminput from "@/ui/common/input/fminput.vue"

import tags from "@/ui/common/tags.vue"

//选人
import manseach from "@s/views/OrgCommon/man/manseach.vue"
import hrtree from "@s/views/OrgCommon/org/hrtree.vue"

const props = defineProps({
    modelValue:null,
    vc:null,
    disabled:{ type:Boolean,default:false },
})
const emit = defineEmits(['change','update:modelValue']);

const modes=ref([
    {label:"按人",value:"man"},
    {label:"按组织",value:"org"},
    {label:"按组织+人",value:"manorg"},
]);
//数据解析
const readedV=ref();
const rules=ref([])

function parseVal(){
    try{
        var t= JSON.parse(props.modelValue);
        if(Array.isArray(t)){
            rules.value=t;
            readedV.value=props.modelValue;
            return ;
        }
        rules.value=[];
    }
    catch(e){
        rules.value=[];
    }

}

function add(){
    rules.value.push({
        mode:'man',//org-- 按组织; man -- 按人;manorg-- 按组织+人:
        mans:[
        ],
        orgs:[
        ],
        condition:"",
    })
}

function save(){
    var para=JSON.stringify(rules.value);
    emit("update:modelValue",para);
}

//人员的添加逻辑
function deleteallman(li){
    li.mans=[];
}
const addingEmp=ref([]);
function doDutyManAdd(li){
    let maxIndex = 0;
    if (li.mans && li.mans.length > 0) {
        li.mans.forEach((dutyMan, index) => {
            if (!dutyMan.KM_SORT || dutyMan.KM_SORT < 0) {
                dutyMan.KM_SORT = index + 1;
            }
            maxIndex = dutyMan.KM_SORT;
        });
        
        if (maxIndex < li.mans.length) {
            maxIndex = li.mans.length;
        }
    }
    
    //过滤重复添加的人员。
    var addedID=li.mans.map((li)=>li.code);
    var para=[];
    var count = 0;
    this.addingEmp.forEach((man)=>{
        if(addedID.indexOf(man.CON_EMP_NUM)!=-1){
            count++;
            return;
        }
        
        maxIndex++;
        
        li.mans.push({
            name: man.ManName,
            code: man.CON_EMP_NUM,
        });
    })
}


//弹窗组织
const seeHR=ref(false);
const focusRow=ref(false);
function openOrgSelect(e){
    focusRow.value=e;
    seeHR.value=true;
}
function addOrg(e){
    //console.log(e);
    focusRow.value.orgs.push({
        oid:e.UCML_OrganizeOID,
        code:e.Varchar1,
        name:e.OrgName
    })
    seeHR.value=false;
}
function doRemoveDutyOrg(e,li){
    var t=li.orgs.indexOf(e);
    li.orgs.splice(t,1);
}

watch(()=>props.modelValue,()=>{
    if(readedV.value !=props.modelValue){
        parseVal();
    }
})


onMounted(()=>{
    parseVal();
});

</script>


<style scoped>

.rule-row {
    border-radius: 5px;
    margin-top: 22px;
    border: 1px solid #ebeef5;
    background-color: #fff;
    border: 1px solid #9A9A9A;
    box-shadow: 0 2px 12px 0 rgb(0 0 0 / 10%);
}
.rule-row>legend{
    margin-left:10px;
    padding:0 5px;
}

.rrule-fm{
    display: grid;
    grid-template-columns:1fr 1fr;

    border-radius: 4px;

    padding:10px;
}

.parabody{
    grid-column:1/-1;
}
.rrule-fm:deep(.el-form-item){
    margin-bottom:0;
}
</style>