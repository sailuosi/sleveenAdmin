<template>

    <el-dialog :title='flowStarted==false?"启动流程":"查看流程"' class="normal" v-model="seeFlow" lock-scroll
        top="5vh" destroy-on-close
        :width="page=='start'?'700px':'70vw'"
    >
        <template #default>
            <div style="min-height: 50vh;">
                <flowStartor v-if="page=='start'"
                    :flows="detail"
                    @refresh="load"
                    @close="closeme"
                    @sended="onIsSended"
                    @okclick="onOKClick"
                    :biz="biz" :pk="pk"
                    :bizTitle="bizTitle"
                    :bizOrg="bizOrg" :bizOrgName="bizOrgName"
                    :sendpara="sendpara"
                ></flowStartor>
                <flowInfo v-else :para="chartPara" @cancel="seeFlow=false"></flowInfo>                
            </div>

        </template>
    </el-dialog>

</template>

<script setup>
/**
 * 组件功能：向VC提供一个打开流程启动器的方法。
 */
//:top="page=='start'?'10h':'2vh'"
import { reactive, computed, nextTick, ref, onMounted,watch } from 'vue';
import { loadStartFlow } from "@s/api/ccflow/CCBizFlowapi.js"

import flowStartor from "@s/components/CCflow/startor/flowStartor.vue"
import flowInfo from "@s/components/CCflow/startor/flowInfo.vue"

import {parUrlPara} from "@/utils/loadJs.js"


const props = defineProps({
    bc:null,
    pk:null,
    biz:null,
    bizTitle:null,
    vcconfig:null,
    flowFilter:null,
    //流程发送参数
    sendpara:null,
})

const page=ref("start")

//流程状态解析
const emit = defineEmits(['ready', 'selectionChange', 'sortHeader']);
const flowStarted=computed(()=>{
    if(props.row?.KB_FlowFlag=="1"){
        return true;
    }
    return false;
});
const chartPara=ref({
    FK_Flow:"",
    FK_Node:"",
    WorkID:"",
})

///弹窗控制
const seeFlow=ref(false);
function openFlow(){
    page.value="start";
    seeFlow.value=true;

}
function openMarkPic(){
    page.value="pic";
    seeFlow.value=true;
}
/// 启动参数
const detail=ref([]);
const seeLinkDe=ref(false);

const last=ref(null);
const pk= ref("");
const biz= ref("");
const bizTitle= ref("");
const bizOrg= ref("");
const bizOrgName= ref("");
const flowFilter= ref();
const configBase=ref();
const sendpara=ref({})

function load(config){
    var para=Object.assign({
        pk:pk.value,
        biz:biz.value,
    },config);
    loadStartFlow(para).then((res)=>{
        if(res.status=="success"){
            var tar=res.data;
            if(typeof flowFilter.value=="function"){
                tar.forEach((t)=>{
                    t.para={};
                    if(!!t.bf_condi){
                        t.para=parUrlPara(t.bf_condi);
                    }
                })
                tar=flowFilter.value(tar);
            }
            //flowFilter
            detail.value=tar;
            seeLinkDe.value=true;            
        }
        openFlow();
    })
}

const onSended= ref()

function closeme(){
    seeFlow.value=false;
}

function onIsSended(e){
    if( typeof onSended.value =="function"){
        var t= onSended.value(e,closeme);
        if(t!== false){
            closeme();
        }
    }
    else{
        swal({ text: "流程已启动！", icon: "success", button: "关闭", });
        closeme();
    }
}

const onSendOK= ref()
function onOKClick(e){
    if( typeof onSendOK.value =="function"){
        onSendOK.value(e)
    }
    else{
        swal({ text: "流程已启动！", icon: "success", button: "关闭", });
        closeme();
    }
}


const outFunc={
    openFlow:function(config){
        if(!config.pk){
            swal({ text: "业务主键信息不得为空", icon: "error", button: "关闭", });
            return ;
        }
        if(!config.biz){
            swal({ text: "业务类型信息不得为空", icon: "error", button: "关闭", });
            return ;
        }
        configBase.value=config;
        last.value=config;

        pk.value= config.pk;
        biz.value= config.biz;
        bizTitle.value= config.title;
        bizOrg.value= config.org;
        bizOrgName.value= config.orgName;
        onSended.value= config.onSended;
        onSendOK.value= config.onSendOK;
        flowFilter.value= config.flowFilter;
        sendpara.value= config.sendpara||{};
        //开始加载业务对象的流程
        load(config);

    },
    openCChart:function(config){

        chartPara.value.FK_Flow=config.flowNo;
        chartPara.value.FK_Node=config.nodeID;
        chartPara.value.WorkID=config.workID;
        openMarkPic();
    }
}

// watch(()=>props.row,()=>{
//     //load();
// })
onMounted(()=>{
    //load();
    emit("ready",outFunc)
})
defineExpose({
    openFlow:outFunc.openFlow,
    openCChart:outFunc.openCChart,
})
</script>
