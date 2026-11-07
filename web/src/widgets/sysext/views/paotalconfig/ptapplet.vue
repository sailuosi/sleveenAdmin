<template>
    <div class="catapan">
        <div class="left">
            <el-button @click="drawer=true">配置</el-button>
            <ptcatatree @nodeclick="onselectcata" ></ptcatatree>
            <el-drawer
                v-model="drawer" size="70vw"
                title="编辑组织库分类"
                direction="ltr">
                <ptcatalist style="height:70vh;" ></ptcatalist>
            </el-drawer>
        </div>
        
        <ptappletlist :cata="focusCata" class="right" :selecting="selecting"
            ref="appet" :checkable="checkable"
        ><slot name="applet"></slot></ptappletlist>
    </div>
</template>

<script>

import ptcatatree from "./ptcatatree.vue"
import ptappletlist from "./ptappletlist.vue"
import ptcatalist from "./ptcatalist.vue"

export default {
    name: 'WebPtapplet',
    components:{
        ptcatatree,ptappletlist,ptcatalist
    },
    props:{
        selecting:{ type:Boolean,default:false },
        checkable:{ type:Boolean,default:false },
    },
    data() {
        return {
            focusCata:{},
            drawer:false,
        };
    },
    expose:["getChecked"],
    mounted() {
        
    },

    methods: {
        onselectcata(e){
            this.focusCata=e;
        },
        getChecked(){
            return this.$refs.appet.getChecked();
        },
    },
};
</script>

<style scoped>
div.catapan{
    display:flex;
    flex-direction:row;
}
div.catapan>.left{
    flex:0 0 200px;
}
div.catapan>.right{
    flex:1;
    width:0;
}
</style>