<template>
    <li class="rmn-item" :style="getmenuStyle" @click.stop="onclick">
        <slot :item="item">
            <i v-if="item.icon" :class="item.icon" style="margin-right:6px"></i>
            <div class="d2-contentmenu-item-title" flex-box="1">
                {{item.label}}
            </div>        
        </slot>
        <ul v-if="Array.isArray(item.children) && item.children.length>0" class="next-menu">
            <menuItem v-for="(menu,m) in item.children" :item="menu" :panel="panel"></menuItem>
        </ul>
    </li>
</template>

<script>



export default {
    name:"menuItem",
    components: {
        
    },
    props:{
        item:null,
        panel:null,
    },
    emits:[],
    data(){
        return {

        };
    },
    computed:{
        getmenuStyle(){
            var res= {};
            if(!!this.item.color){
                res.color=this.item.color;
            }
            return res;
        },
    },
    mounted() {

    },
    methods:{
        onclick(){
            this.panel.$emit("click",this.item);
        },
    }
}
</script>

<style scoped>
.right-pan{
    background:#fff;
}
.rmn-item{
    cursor:pointer;
    position: relative;
    display: flex;
    align-items: center;
    padding: 0 30px 0 20px;
    height: 34px;
    line-height: 34px;
    outline: 0;
}
.rmn-item:hover{
    background: #F5F7FA;
}
.next-menu{
    margin-block-start: 0;
    margin-block-end: 0;
    background:#fff;
    display:none;
    position:absolute;
    width:100%;
    left:70%;
    padding-inline-start: 5px;
}
.rmn-item:hover >.next-menu{
    display:block;
    border: 1px solid #cfd7e5;
    border-radius: 4px;
    box-shadow: 0 2px 12px 0 rgb(0 0 0 / 10%);
}
</style>