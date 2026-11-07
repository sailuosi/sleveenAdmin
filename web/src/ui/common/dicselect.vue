<template>
  <el-select :modelValue="inputval" @change="oninput" :placeholder="placeholder" :clearable="clearable">
    <el-option
      v-for="dict in renderlist"
      :key="dict.value"
      :label="dict.label"
      :value="dict.value"
    ></el-option>
  </el-select>
</template>

<script >
export default {
  name: "dicselect",
  data() {
    return {
      renderlist:[],
    };
  },
  props:{
    dict: {    type: String},
    modelValue: [String,Number,Array],
    placeholder: {    type: String,    default: "请选择...",  },
    // 
    dictlist: {    type: Array,   },
    clearable:{type:Boolean,default:false},
  },
  computed:{
    inputval(){
      //因为字典全是字符串，这里进行一个转换。
      if(typeof this.modelValue=='number'){
        return this.modelValue+'';
      }
      return this.modelValue;
    },
  },
  created() {

    if(this.dict==void 0){
      this.renderlist=this.dictlist;
    }else{
      var dictres=this.useDict( this.dict);
      this.renderlist== dictres[this.dict];
    }    
  },
  watch:{
    dictlist:function(val){
      this.renderlist=val;
    }
  },
  emits: ['update:modelValue',"input"],
  methods: {
    oninput(e){
      this.$emit("update:modelValue",e);
      this.$emit('input',e);
    }
  }
};

</script>
