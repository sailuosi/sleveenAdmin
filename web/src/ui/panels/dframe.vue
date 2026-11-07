<template>
  <div >
    <iframe
      class="d-frame"
      :src="tarUrl" height="100%" width="100%"
      frameborder="0"/>
  </div>
</template>

<script>


export default {
  name: 'd-frame',
  props: {
    src: {
      type: String,
      required: false,
      default: ''
    }
  },
  data(){
    return {
      env: process.env,
      isdev:process.env.NODE_ENV=="development"
    };
  },
  computed:{
    tarUrl(){
        var src=this.src;
        if(!src){
            src=this.$route.params.url;
        }
        if(!src){
            src=this.$route.meta.url;
        }
        if(src.startsWith('http')){
            return src;
        }
        return src;
    },
  },
}
</script>

<style >

</style>
