<template>
<div :class="['po-'+position,'cardtab-pan', 'bg-white' ]" :style="panstyle" >

    <div class="ct-labels">
        <div class="ct-label bg-primary portal-bar" :style="labelStyle" v-for="(tab,t) in rendTabs" :class="[tab.name==active?'actived':'sleeped']" @click="focusto(tab)">
            <slot name="label" :row="tab">
                <div v-for="(item, index) in tab.label" :key="index">{{ item }}</div>
            </slot>
        </div>
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
    },
    data(){
        return {
            active:"",
            activeMap:{},
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
        rendTabs(){
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
            return res;
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
    methods:{
        register(key,val){
            this.childs[key]=val;
        },
        focusto(tab){
          this.$emit("cardTabClick",tab)
            this.active=tab.name;
        },
    },
}
</script>

<style scoped>
/* 根据需要添加样式 */
body{
  margin: 0;
  padding: 0;
}
.cardtab-pan {
  display: flex;

  /* background: #FAFAFA; */
  border-radius: 2px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.2);
}

.ct-labels{
  flex: 1 0 0;
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
  padding: 5px;
  flex:1;

  overflow: hidden;
}


</style>
