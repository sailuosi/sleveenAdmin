<template>
<div :class="['po-'+position,'cardtab-pan', 'bg-white' ]" :style="panstyle" >
    <div class="ct-labels">
        <div class="ct-label bg-primary portal-bar" :style="labelStyle" v-for="(tab,t) in rendTabs" :class="[tab.name==active?'actived':'sleeped']" @click="onclickTab(tab)">
            <slot name="label" :row="tab">
                <div v-for="(item, index) in tab.label" :key="index">{{ item }}</div>
            </slot>
        </div>    
    </div>

    <div class="cd-box bg-white">
        <slot name="head" :row="activeTab"></slot>
        <template v-if="!nobody" >
            <template v-for="(tab,t) in rendTabs">
                <div v-if="activeMap[tab.name]==true ||tab.name===active" v-show="tab.name==active">
                    <slot :row="tab" :index="t"></slot>
                </div>        
            </template>        
        </template>

        <slot name="foot" :row="activeTab"></slot>
    </div>
</div>
</template>

<script>
export default {
    name: "cardTab",
    props: {
        position: {type: String,default:"top",        },
        tabs:{ type:Array,default:function(){ return []; } },
        labelKey: {type: String,default:"label"},
        nameKey: {type: String,default:""},
        height:null,
        nobody:{ type:Boolean,default:false },
        modelValue:null,
    },
    data(){
        return {
            active:0,
            activeMap:{},
            activeTab:null,
            rendTabs:[],
        };
    },
    expose:["register"],
    computed:{
        panstyle(){
            var obj={};
            var map={
                top:"column",
                bottom:"column-reverse",
                left:"row",
                right:"row-reverse",
            }
            //obj["flex-direction"]=map[this.position];
            if(!!this.height){
                obj.height=this.height;
            }
            return obj;
        },
        labelStyle(){
            var obj={};
            if(this.position=="top"|| this.position=="bottom"){
                obj["flex-direction"]="row";
            }
            if(this.position=="left"|| this.position=="right"){
                obj["flex-direction"]="column";
            }
            return obj;
        },
    },
    mounted(){
        this.init();
    },
    watch:{
        "tabs":function(){
            this.init();
        },
        "modelValue":function(v){
            if(v.name==this.active){
                return ;
            }
            this.focusto(v);
        }
    },
    methods:{
        register(key,val){
            this.childs[key]=val;
        },
        init(){
            var res=[];
            this.tabs.forEach((tab,t)=>{
                var labelstr=[];
                var tar= tab;
                if(!!tar.label && typeof tar.label=="string"){
                    labelstr=tar.label.split("");
                }
                if(Array.isArray(tar.label)){
                    labelstr= tar.label;
                }
                var obj= {
                    label:labelstr,
                    data:tar
                };
                if(!!this.nameKey){
                    obj.name= tab[this.nameKey];
                }
                if(!!this.labelKey){
                    obj.label= tab[this.labelKey];
                }
                if(!obj.name){
                    obj.name=t;
                }
                res.push(obj);
            })
            this.rendTabs=res;
            if(res.length>0){
                this.active=res[0].name;
                this.activeMap[this.active]=true;
                this.activeTab=res[0];
            }
            
        },
        onclickTab(e){
            this.focusto(e);
            this.$emit("update:modelValue",e);
        },
        focusto(tab){
            this.activeMap[tab.name]=true;
            this.active=tab.name;
            this.activeTab=tab;
        },

    },
}
</script>

<style scoped>
/* 根据需要添加样式 */
.cardtab-pan {
    display: flex;

    /* background: #FAFAFA; */
    border-radius: 10px;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.2);
}

.ct-labels{
    flex:0 0 40px;
    color: #fff;
    display: flex;
    flex-direction: column;
    justify-content: stretch;
    align-items: stretch;
}

.ct-label.sleeped{
    background-color:#efefef;
    color:#666;
}

.cardtab-pan.po-left{
    flex-direction: row;
}
.cardtab-pan.po-right{
    flex-direction: row-reverse;
}
.cardtab-pan.po-top{
    flex-direction: column;
}
.cardtab-pan.po-bottom{
    flex-direction: column-reverse;
}



.ct-label {
    flex:1;
    color: #fff;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    cursor:pointer;
}

.cardtab-pan.po-left>.ct-labels{
    flex-direction: column;
    border-top-left-radius: 10px;
    border-bottom-left-radius: 10px;
}
.cardtab-pan.po-right>.ct-labels{
    flex-direction: column;
    border-top-right-radius: 10px;
    border-bottom-right-radius: 10px;
}
.cardtab-pan.po-top>.ct-labels{
    flex-direction: row;
    border-top-left-radius: 10px;
    border-top-right-radius: 10px;
    border-bottom:1px #fff solid;
}
.cardtab-pan.po-bottom>.ct-labels{
    flex-direction: row;
    border-bottom-left-radius: 10px;
    border-bottom-right-radius: 10px;
}


.cardtab-pan.po-left>.ct-labels>.ct-label{
    flex-direction: column;
    border-top-left-radius: 10px;
    border-bottom-left-radius: 10px;
}
.cardtab-pan.po-right>.ct-labels>.ct-label{
    flex-direction: column;
    border-top-right-radius: 10px;
    border-bottom-right-radius: 10px;
}
.cardtab-pan.po-top>.ct-labels>.ct-label{
    flex-direction: row;
    border-top-left-radius: 10px;
    border-top-right-radius: 10px;
}
.cardtab-pan.po-bottom>.ct-labels>.ct-label{
    flex-direction: row;
    border-bottom-left-radius: 10px;
    border-bottom-right-radius: 10px;
}


.ct-box {
    padding: 20px;
    flex:1;

    overflow: hidden;
}
</style>
