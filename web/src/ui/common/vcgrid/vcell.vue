<template>
<div :class="{ overdot: !!maxHeight, vcelval: true }">
    <template v-if="type == 'tip'">
        <el-tooltip placement="top" trigger="hover">
            <template #content>
                <div class="celltipval">{{ value }}</div>
            </template>
            <div :style="css" v-text="value"></div>
        </el-tooltip>
    </template>
    <template v-else-if="ctype == 'Boolean'||ctype == 'boolean'">
        <el-switch :value="value == 'true' || value == 1"> </el-switch>
    </template>
    <template v-else-if="ctype == 'dict'">
        <el-tag class="vcell-tag">{{ rendVal }}</el-tag>
    </template>
    
    <el-tooltip v-else-if="tip=='auto'" content="" :placement="tipconfig[0]" :effect="tipconfig[1]" :disabled="!overflowed" popper-class="vcellvalpp" v-trunced="overflowed" >
        <div  :style="css" v-html="value" class="celltxt" :val="value"></div>
        <div slot="content" v-html="value"></div>
    </el-tooltip>
    <div  v-else :style="css" v-html="value" class="celltxt" :val="value"></div>
</div>
</template>

<script>
// 
const syncVar = (node, binding, vnode,el) => {
    //console.log(arguments)
    //var overflowed=node.offsetWidth < node.scrollWidth;
    var overflowed=false; //el.offsetWidth < el.scrollWidth;
    if(el.offsetWidth < el.scrollWidth || el.offsetHeight < el.scrollHeight){
        overflowed=true;
    }
    var noewclaSS=el.className;
    

    if(overflowed && noewclaSS.indexOf("ovfloe")==-1){
        el.className += "ovfloe"
    }
    else if(!overflowed && noewclaSS.indexOf("ovfloe")!=-1){
        el.className = noewclaSS.replace("ovfloe","");
    }
    if (binding.arg) {
        binding.value[binding.arg] = overflowed
    } else {
        vnode.context[binding.expression] = overflowed
    }
}

export default {
    name: "vccell",
    props: {
        type: null,
        ctype: null,
        value: null,
        getCellStyle: null,
        maxHeight: null,
        dictlist: null,
        size: null,
        tip:{ type:String,default:"" },
    },
    data(){
        return {
            overflowed:false,
            ro:null,
        }
    },
    directives: {
        trunced: {
            inserted: function (el, binding, vnode) {
                const node = binding.modifiers.parent ? el.parentNode : el
                syncVar(node, binding, vnode,el);

                vnode.context.ro = new ResizeObserver((entries, observer) => {
                    entries[0].target && syncVar(node, binding, vnode,el)
                });
                vnode.context.ro.observe(node);
            },
            componentUpdated: function (el, binding, vnode) {
                const node = binding.modifiers.parent ? el.parentNode : el
                syncVar(node, binding, vnode,el);
            }
        },
    },
    computed: {
        tipconfig(){
            var arr=this.tip.split(",");
            if(arr.length>1){
                return arr;
            }
            return ["bottom","light"];
        },
        css() {
            var res = {};
            if (!!this.maxHeight) {
                res["max-height"] = this.maxHeight;
            }
            if (this.size == 1 || this.size == "1") {
                res["text-overflow"] = "ellipsis";
            } else if (!isNaN(this.size) && this.size * 1 > 1) {
                res["display"] = "-webkit-box";
                res["-webkit-box-orient"] = "vertical";
                res["-webkit-line-clamp"] = this.size;
                res["text-overflow"] = "ellipsis";
            }
            return res;
        },
        dictMap() {
            var res = {};
            if (Array.isArray(this.dictlist)) {
                this.dictlist.forEach((dict) => {
                    res[dict.value] = dict;
                });
            }
            return res;
        },
        rendVal() {
            if (!!this.dictMap[this.value]) {
                return this.dictMap[this.value].label;
            }
            return this.value;
        },
    },
    unmounted() {
        if(this.ro!=null){
            this.ro.unobserve();
        }
    },
};
</script>

<style scoped>
.celltipval {
    max-width: 60vw;
}
.vcell-tag{
    height:auto;
    line-height:26px;
}
:deep(.ovfloe) {
    color:red;
}
.celltxt{
    position:relative;
    overflow:hidden;
}
.celltxt:hover{
    /* overflow:initial; */
    
}
.celltxt:hover::after{
    display:none;
}
.celltxt:after{
    background:#fff;
    display:none;
    content:attr(val);
    position:absolute;
    max-width: 50vw;
    min-height:calc(100% + 20px);
    max-height:50vh;
    width:200%;
    top:-10px;
    left:-50%;
    z-index:1;
    border:1px solid #ECF5FF;
    box-shadow: 0 2px 12px 0 rgb(0 0 0 / 10%);



}



</style>
