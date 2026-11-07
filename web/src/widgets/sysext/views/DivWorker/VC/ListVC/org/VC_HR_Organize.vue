<template>
    <flexpan :flex="flex" :height="height">
        <template #header="p">
            <teleport  :to="position" :disabled="!position" v-if="!hideToolbar">        
                <div class="vc-top-toolbar" >
                    

                    <slot name="toolbar"></slot>
                </div>
            </teleport>
        </template>
        <template #default="p">
            <vctree :load="loadNode" :total="total" :para="queryParams" :height="p.height"
                pk="HR_OrganizeOID" fk="HR_Parent_FK"
                @update:focusRow="onTreeNodeClicked"
                
                @nodeclick="onNodeClick"
            >
                <template #node="nd">
                    <div>
    <span>{{ nd.row.O_OrgName }}</span>
  </div>
                </template>
            </vctree>
        </template>
        <template #footer="p">
        </template>
    </flexpan>
</template>
<script setup>
import { ref, computed, watch, onMounted,useAttrs } from 'vue';
//基础UI
import flexpan from "@/ui/common/flexpan.vue";

import livevc from "@/uiext/gene/live/livevc.vue";
import vctree from "@/uiext/gene/vcbase/vctree.vue"
//编辑控制
import useListEditor from "@/uiext/gene/hook/listeditor.js"
//其它引用



const props = defineProps({
    page: null,
    api: null,
    bus:null,
    nav: null,
    height: null,
    vc: null,
	disabled:{ type:Boolean,default:false },
    hideToolbar:{ type:Boolean,default:false },
    autoLoad:{ type:Boolean,default:true },
    flex:{ type:Boolean,default:false },
    //工具栏迁移的目标
    position:null,
    conditions:null,//查询参数
    params:null,//额外的自由参数，为对象
    
})

const listHolder = useListEditor();
const VCName="VC_HR_Organize";
//视图显示前

// 定义响应式数据
const mlist = ref([]);
const total = ref(0);
const queryParams = ref({
    pageSize: 30,
    pageNum: 1,
});
const focusRow = ref({});
const diaRow = ref(false); // 行编辑表单
const adding = ref(false);
const emit = defineEmits([])

function onTreeNodeClicked(tar){
    focusRow.value = tar;
    emit('update:focusRow', tar);
}

// 计算属性
const listpara = computed(() => {
    return { ...queryParams.value };
});

// 监听 listpara 变化
watch(listpara, () => {
    //getlist();
});

// 挂载时调用 getlist
onMounted(() => {
    
    //getlist();
});

// 方法
const loadNode = (row) => {
    var para={
        pk:null,
        deep:2,
        dict:props.params,
    }
    if(row !=null){
        para.pk=row.HR_OrganizeOID;
    }
    return props.api.queryBCHROrganizeTree(para).then((res) => {
        return res.Nodes;
    });
};

const resetQuery = () => {
    queryParams.value = {
        pageSize: 30,
        pageNum: 1,
    };
};

const onclosefm = () => {
    diaRow.value = false;
};

const openAdd = () => {
    adding.value = true;
    focusRow.value = {
        //FK_channel:this.channel.pc_id,
    };
    diaRow.value = true;
};

const openRow = (row) => {
    adding.value = false;
    focusRow.value = row;
    diaRow.value = true;
};
function saveChange() {
    var tar = listHolder.compare(mlist.value);
    if(tar.length==0 ){
        swal({ text: "没有变更的信息", icon: "info", button: "关闭", });
        return ;
    }


    props.api.saveBCHROrganizeList(tar).then((res) => {
        swal({ text: res.data, icon: "info", button: "关闭", });
        getlist();
    });
    
}

const removeRow = (row) => {
    const para = [row.HR_OrganizeOID];
    alertIsOK("确认要操作吗？将删除当前列表组件吗！",function(e){
        if(e=="yes"){
            props.api.removeBCHROrganizeByIds(para).then((res) => {
                proxy.$message({ type: 'success', message: res.msg });
                getlist();
            });
        }
    },"删除数据行：");
};

/**-|方法|-start**/
    function onNodeClick(row){
    emit('nodeclick',row)
}
/**-|方法|-end**/

/***工具栏响应方法--开始***/




/***工具栏响应方法--结束***/

</script>