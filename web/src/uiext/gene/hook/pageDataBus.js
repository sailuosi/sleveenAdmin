
import { reactive} from 'vue'

import { createEventHub } from '@/utils/eventHub.js'

export default function useDataBus(bpo) {
    //已注册的VC
    var vcNames= reactive([]);

    var BPOName = bpo;

    var toAdd = [];

    var dataMap =reactive({});

    var keyField="";

    const hub=createEventHub();

    function init(bpo){
        BPOName = bpo;
    }

    function initVC(vc,state){
        if(!vcNames.includes(vc)){
            vcNames.push(vc);
        }
        else{
            return;
        }
        if(state==void 0){
            state={
                mlist:[],//数据列表
                focusRow:{},//当前行
                conditions:[],//查询条件
            }
        }
        dataMap[vc] = state;
    }

    function initVCList(vc,mlist,total,focusRow,queryRowCondtions){
        if(!vcNames.includes(vc)){
            vcNames.push(vc);
        }
        else{
            return;
        }

        var state={
            mlist:mlist,//数据列表
            focusRow:focusRow,//当前行
            total:total,
            conditions:[],//查询条件
            queryRowCondtions:queryRowCondtions || [],
        }
        
        dataMap[vc] = state;
    }
    function initVCEdit(vc,focusRow){
        if(!vcNames.includes(vc)){
            vcNames.push(vc);
        }
        else{
            return;
        }

        var state={
            focusRow:focusRow,//当前行
            conditions:[],//查询条件
        }
        
        dataMap[vc] = state;
    }

    function setList(vcname, list){
        initVC(vcname);
        dataMap[vcname].mlist=list;
    }

    function registerAction(vcname, load,save,remove){
        initVC(vcname);
        dataMap[vcname].refresh=load;
        dataMap[vcname].save=save;
        dataMap[vcname].remove=remove;
        //console.log("registerAction",dataMap[vcname]);
    }

    function setRow(vcname, row){
        initVC(vcname);
        dataMap[vcname].focusRow=row;

    }

    function save(vcname,showtip){
        if(vcname !=void 0){
            dataMap[vcname].save(showtip);
            return;
        }
        var tasks=[];
        for(var i=0;i<vcNames.length;i++){
            var t= dataMap[vcNames[i]].save(showtip);
            if(t instanceof Promise){
                tasks.push(t);
            }
            
        }
        //console.log(tasks);
        if(tasks.length==0){
            if(showtip!=false){
                swal({ text: "没有需要提交的变更信息！", icon: "info", button: "关闭", });
            }
            return Promise.resolve(true);
        }
        return Promise.all(tasks).then(function(){
            if(showtip!=false){ 
                swal({ text: "提交成功", icon: "info", button: "关闭", });
            }
            return true;
        })
    }

    return {
        hub,dataMap,
        init, setList,setRow,initVC,registerAction,initVCList,initVCEdit,save
    }
}