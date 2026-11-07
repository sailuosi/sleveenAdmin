<template>
    <el-select :style="mystyle"
        :value="checked"  filterable :multiple="multiple"
        placeholder="查询人员(支持模糊查询)"
        :filter-method="changeSelectRestFul"
        @update:modelValue="onchange"
    >
    <el-option
        v-for="item in options"
        :key="item.oid"
        :label="item.ManName"
        :value="item.oid"
    >
    <span class="el-icon-s-custom" style="margin-left:16px;margin-right:4px;"></span>
    <span >{{item.ManName}}</span><span class="el-icon-s-home" style="margin-left:16px;margin-right:4px;"></span><span >{{item.OrgName}}/{{item.divName}}</span>
    </el-option>
    </el-select>
</template>

<script>

import {  loadManSearch } from "@/api/system/mancommonapi.js";

// 任意人员选择器

export default {
    name:"manany",
    props:{
        multiple:{ type:Boolean,default:false, },
    },
    emits:["change","update:modelValue","update:selected"],
    data(){
        return{
            options: [],
            checked:null,
            map:{},
        };
    },
    computed:{

        appver(){
            return false;
        },
        mystyle(){
            return {
                width:this.appver==false?'auto':"300px",
            }
        },
    },
    methods:{
        onchange(e){
            
            this.checked = e;
            var val=null;
            if(Array.isArray(this.checked)){
                val=this.checked.map((o)=>{
                    return this.map[o];
                })
            }
            else{
                val=this.map[this.checked];
            }

            this.$emit("change",val);
            this.$emit("update:modelValue",this.checked,val);
            this.$emit("update:selected",val)
        },

        changeSelectRestFul(inputKey) {
            var canshu = {
                "value": inputKey,
                "deptcode": this.deptcode
            };
            loadManSearch(canshu).then((res) => {
                if (res.status =="success") {
                    res.data.forEach((man)=>{
                        this.map[man.oid]=man;
                    })
                    this.options = res.data;
                }
            })
        },
    }
}
</script>