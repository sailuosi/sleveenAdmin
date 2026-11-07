<template>
  <el-popover
    :placement="placement"
    title=""
    :width="width"
    :trigger="trigger" :visible="rendshow"  @input="$emit('close')"
    content="" @show="hrtreeScroll">

    <template #default="p">
        <div>
            <slot name="top"><span>{{title}}</span></slot>
            <hrtree ref="hrtree" class="pop-body" :style="bodystyle"
                @nodeclick="onnodeclick" :deptOID="deptOID"
            ></hrtree>        
        </div>

    </template>
    <template #reference>
        <slot>
        </slot>
    </template>
  </el-popover>
</template>

<script>

import hrtree from "@/widgets/sysext/views/org/hrtree.vue"
//:visible="trigger=='manual'?null:show"
export default {
    name: 'hrselect',
    components:{
        hrtree
    },
    props:{
        height:{ type:String,default:"" },
        maxHeight:{ type:String,default:"90vh" },
        scroll:{ type:String,default:"auto" },  

        trigger:{ type:String,default:"click" },
        show:{ type:Boolean,default:false, },
        placement:{ type:String,default:"bottom" },
        title:{ type:String,default:"" },
        width:{ type:String,default:"250px" },
        deptOID: { type: String, default: "" }
    },
    data() {
        return {
            
        };
    },
    computed:{
        bodystyle(){
            var res= {
                "max-height":this.maxHeight,
                "overflow":this.scroll
            }
            if(!!this.height){
                res.height=this.height;
            }
            return res;
        },
        rendshow(){
            if(this.trigger=="manual"){
                return this.show;
            }
            return null;
        },
    },

    mounted() {
        
    },

    methods: {
        onnodeclick(tar,node,vm){
            //console.log(tar)
            this.$emit("update:modelValue", tar.UCML_OrganizeOID || tar.HR_OrganizeOID, tar);
            this.$emit("select",tar,node,vm);
        },
        hrtreeScroll() {
        	this.$refs.hrtree.scrollToNode();
        }
    }
};
</script>

<style scoped>
.pop-body{

}
</style>