<template>
    <ul class="right-pan">
        <menuItem v-for="(item,i) in list" :item="item" :panel="vm">
            <template #default="p">
                <slot v-bind="p"></slot>
            </template>
        </menuItem>
    </ul>
</template>

<script>


import menuItem from "./menuItem.vue"

export default {
    name:"rightMenu",
    components: {
        menuItem
    },
    props:{
        list:null,
    },
    emits:[],
    data(){
        return {
            vm:this,
        };
    },
    computed:{
    },
    mounted() {

    },
    methods:{
        getlist(){

        },
    }
}
/**
    border: 1px solid #cfd7e5;
    border-radius: 4px;
    box-shadow: 0 2px 12px 0 rgb(0 0 0 / 10%);
 */
</script>

<style scoped>
.right-pan{
    background:#fff;
    margin-block-start: 0;
    margin-block-end: 0;
    padding-inline-start: 5px;
}

</style>