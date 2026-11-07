<template>
<div>
    <el-popover
        placement="bottom"
        :title="popTitle"
        :width="popWidth"
        trigger="click"
        content=""
        :disabled="disabled">
        <template #reference>
            <el-tag type="primary">
                <span v-if="!current[pk]">{{placeholder}}</span>
                <span v-else>{{ current[caption] }}</span>
            </el-tag>
        </template>
        <slot>
        </slot>
    </el-popover>
</div>
</template>

<script>
/** 结合了 tag  和 popover的组件
 * 未聚焦时显示tag，点击时，弹出 popover 进行选择
 */

import inputsearch from "@/ui/common/input/inputsearch.vue"
import { Refresh } from "@element-plus/icons-vue";

export default {
    name: 'WebTagselect',
    components:{
        inputsearch, Refresh
    },
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
        disabled:{ type:Boolean,default:false }
    },
    data() {
        return {
            current:{},
            mlist:[],
            total:0
        };
    },
    watch:{
        "modelValue":function(newv){
            var oid=this.current[this.pk];
            if(oid==void 0 || oid != newv){
                this.loadCurrent();
            }
        }
    },
    methods: {
        loadCurrent(){
            if(typeof this.rowApi !="function") return;
            if(!this.modelValue){
                this.current={};
                return ;
            }
            this.rowApi(this.modelValue).then((res)=>{
                this.current=res;
            });

        },
        onput(row,oid){
            this.current=row;
            this.$emit('update:modelValue',oid);
            // console.log(row, "tagselect");
            this.$emit('updateRow', row);
        }
    }
};
</script>
