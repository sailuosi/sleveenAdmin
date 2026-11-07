<template>
    <iframe  class="mi-frame" :src="url"  frameborder="0" lazy />
</template>

<script>
export default {
    name:"linkframe",
    props:{
        url:null,
    },
    emits:[],
    computed:{
    },
    methods:{
      
    }
}
</script>