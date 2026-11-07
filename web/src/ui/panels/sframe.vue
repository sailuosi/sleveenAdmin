<template>
  <div >
    <iframe v-if="valid"
      class="sframe"
      :src="tarUrl" height="100%" width="100%"
      frameborder="0"/>
  </div>
</template>

<script>

import { getRootPath,getPublicPath,getBPOPath } from "@f/common/utils/jflow.js"
export default {
    name: 'sframe',
    props: {
        src: {   type: String,default: ''}
    },
    data(){
        return {
            valid:true,
        };
    },
    computed:{
        tarUrl(){
            var src=this.src;
            if(src.startsWith('http')){
                return src;
            }
            if(src.startsWith('/ccflow/webapp')){
                return getPublicPath()+src;
            }
            if(src.startsWith('/jxkhfm/')){
                return getRootPath()+src;
            }
            if(src.startsWith('@BPOPath')){
                var url2= src.replace("@BPOPath","");
                return getBPOPath()+url2;
            }
            return src;
        },

    },
    watch:{
        "src":function(){
            this.valid=false;
            this.$nextTick(()=>{
                this.valid=true;
            })
        }
    },
}
</script>

<style >

</style>
