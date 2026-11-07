
<template>
    <div v-html="rendval" @input="ondivInput($event)" :placeholder="placeholder==''?'请输入内容':placeholder" @keyup.enter="onenter" @blur="ondivblur"
     :style="mycss" :contenteditable="!disabled" :class="['divinput',disabled?'disabled':'enabled']">
    </div>
</template>
<script>
//由div构建的文本编辑器。
export default {

    name:"divhtml",
    props:{
        modelValue:null,
        lazy:{type:Boolean,default:true},
        width:null,
        height:null,
        disabled:{type:Boolean,default:false},
        html:{type:Boolean,default:false},
        clearable:{type:Boolean,default:false},
        placeholder: { type: String, default: "" },
    },
    emits:["update:modelValue","input","change"],

    data(){
        return{
            initValue:"",
            valtemp:null,
            broadcast:null,//向外呼出的值。
            editing:false,
        };
    },
    computed:{
        rendval(){
            if(this.disabled){
                return this.modelValue;
            }
            //当输入
            if(this.modelValue!=this.broadcast){
                //如果输入的不是本事向外广播的变化值，需要对绘制的表征值进行修改。
                this.initValue=this.modelValue;
            }
            return this.initValue;
        },
        mycss(){
            return {
                width:this.width?this.width:'100%'
            }
        },
    },
    methods:{
        ondivInput(e){
            //console.log(e);
            this.editing=true;
            var val = e.target.innerHTML;
            if(this.html==false){
                val=e.target.innerText;
            }
            this.valtemp = val;
            if(this.lazy==false){
                this.broadcast=val;
                this.$emit("update:modelValue",this.broadcast);
                this.$emit("input", this.broadcast);                
            }
        },
        ondivblur(e){//懒加载模式下，当用户输入完毕时，才返回变化数据。
            if(this.lazy){
                if(this.valtemp!=this.broadcast){
                    this.broadcast=this.valtemp;
                    this.$emit("update:modelValue",this.broadcast);
                    this.$emit("input", this.broadcast);
                }
            }
            //不管是不是懒加载，如果处于编辑结束的失焦状态，扔出变化事件。
            if(this.editing){
                this.editing=false;
                this.broadcast=this.valtemp;
                this.$emit("change", this.broadcast);
            }
        },
        onenter(){

        },
    }

}
</script>

<style scoped>
div.divinput{
    height:v-bind(height);
    padding:5px 15px;
    line-height:1.5;
    border-radius:3px;
    border-color:#E4E7ED;
    background-color: #FFFFFF;
    border:1px solid rgb(168,171,178);
    text-align: left;
    white-space: pre-wrap;
    min-height:32px;
}
div.divinput:focus{
  border: 1px solid #409EFF;
  
}
div.divinput.disabled{
    cursor: not-allowed;
    border-color:#E4E7ED;
    background-color: #F5F7FA;
}
</style>