<template>


        <transition :name="setTransitionName" mode="out-in">
                <component :is="rendTarget" class="applet-cont" 
                    :cell="cell" 
                    :vc="vc"
                    :root="root"
                    :paotalConfig="paotalConfig"
                    :para="rendPara"
                    :shareState="shareState"
                    :width="width"
                    :height="height"
                    :disabled="rendDisabled"
                    :paotou="paotou"
                    @setState="$emit('setState',$event)"
                    @changePara="sayChangePara"
                    v-bind="rendPara"
                />
        </transition>
	
</template>

<script setup  name="applet">
/**
	<div class="applet-parent">
    </div>
 */


import { defineAsyncComponent, computed, reactive, onBeforeMount, onUnmounted, nextTick, watch, onMounted } from 'vue';


const props = defineProps({
	// 刷新 iframe
	name: {        default:null	},
    framed:{ type:Boolean,default:false },
    animation: { type: String, default: 'fadeLeft' },
    cell:null,
    vc:null,
    root:null,
    paotalConfig:null,
    para:null,
    shareState:null,
    width:null,
    height:null,
    paotou:null,//门户配置主表对象
});
// 引入组件


const emit= defineEmits(["changePara"]);

function sayChangePara(e){
    emit("changePara",e)
}

// 设置主界面切换动画
const setTransitionName = computed(() => {
	return props.animation;
});
 
const rendTarget= computed(() => {
    var t=props.name;
    if(typeof t=="function"){
        return defineAsyncComponent( props.name);
    }
    if(t==void 0|| t==null){
        console.log("门户组件配置错误：",props)
        return "div";
    }
    return t;
	
});
const rendPara= computed(() => {
	var para = props.para;
    var res={};
    if(para !=null && para !=void 0 && typeof para=="object"){
        res=Object.assign(res,para);
    }
    //解析参数
    if(!!props.vc.para){
        var urlPara=parUrlPara(props.vc.para);
        res=Object.assign(res,urlPara);
    }
    try{
        //尝试解析参数2
        var para2= JSON.parse(props.vc?.para2);
        if(Array.isArray(para2)){
            //
            var manCode=res.manCode;
            var deptOID=res.deptOID;
            para2.forEach((li)=>{
                
                if(!!manCode){
                    if(li.mode=="man" ||li.mode=="manorg"){
                        var codes=li.mans.filter((m)=>m.code==manCode);
                        if(codes.length>0){
                            var liPara=parUrlPara(li.condition);
                            res=Object.assign(res,liPara);
                        }
                    }
                }
                if(!!deptOID){
                    if(li.mode=="org" ||li.mode=="manorg"){
                        var codes=li.orgs.filter((m)=>m.oid==deptOID);
                        if(codes.length>0){
                            var liPara=parUrlPara(li.condition);
                            res=Object.assign(res,liPara);
                        }
                    }
                }
            })
        }

    }
    catch(e){}
    return res;
});
//disabled
const rendDisabled= computed(() => {
	var vc = props.vc;
    if(vc.disabled=="1"){
        return true;
    }
    else if(vc.disabled=="2"){
        return true;
    }
    return false;
});



function parUrlPara(txt){
    var res={};
    if(txt !=null && txt !=void 0 && typeof txt=="string"){
        var paraArr =txt.split('&');
        paraArr.forEach((item)=>{
            var itemArr = item.split('=');
            res[itemArr[0]] = itemArr[1];
        });
        
    }
    return res;
}


</script>


<style scoped>
.applet-cont{
    flex:1;
}
</style>