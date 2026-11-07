<template>

<el-select :modelValue="val" @update:modelValue="onput" placeholder="请选择任务分类" :disabled="disabled" clearable filterable>
    <template v-for="(item, index) in mlist" :key="index">
        <el-option :label="item[caption]" :value="item[pk]" />
    </template>
</el-select>

</template>

<script>

import inputsearch from "@/ui/common/input/inputsearch.vue"
import { Refresh, Delete, Search, View, Pointer, Bottom, Download } from "@element-plus/icons-vue";

export default {
    name: 'WebTagselect',
    components:{
        inputsearch,Refresh
    },
    emits:["update:modelValue"],
    props:{
        modelValue:null,
        listApi:null,
        rowApi:null,
        pk:{ type:String,default:"" },
        caption:{ type:String,default:"" },
        popTitle:{ type:String,default:"" },
        popWidth:{ type:[String,Number],default:200 },
        height:null,
        placeholder:{ type:String,default:"请选择" },
        orgoid:null,
        refresh:{ type:Boolean,default:false },
        disabled:{ type:Boolean,default:false },
    },
    data() {
        return {
            current:{},
            mlist:[],
            total:0,
            map:{},
        };
    },
    computed:{
        val(){
            return this.modelValue;
        },
    },
    mounted() {
        this.loadOpt();
    },
    watch:{
        "modelValue":function(newv){
            var oid=this.current[this.pk];
            if(oid==void 0 || oid != newv){
                this.loadCurrent();
            }
        },
        "orgoid":function(newVal){
            // console.log(this.orgoid, "tagselect--orgoid");
            this.loadOpt();
        }
    },
    methods: {
        loadCurrent(){
            if(typeof this.rowApi !="function") return;
            if(!this.modelValue){
                this.current={};
                return ;
            }
            this.current=this.map[this.modelValue];
            return ;
            this.rowApi(this.modelValue).then((res)=>{
                this.current=res;
            });

        },
        loadOpt(){
            this.listApi().then((res)=>{
                this.mlist = res;
                var map = {};
                
                if (res && res.length > 0) {
                	res.forEach(li => {
	                    map[li[this.pk]] = li;
	                });
                }
                this.map = map;
                if(!!this.modelValue){
                    this.current = this.map[this.modelValue];
                }
            })
        },
        onput(oid){
            this.current=this.map[oid];
            this.$emit('update:modelValue',oid);
        }
    }
};
</script>

<style scoped>

</style>