<template>
    <div class="emptycell" :style="rootStyle">
        <div class="portal-bar">
            {{name}}
        </div>
        <div class="empty-tip">
            没有信息
        </div>
        
    </div>
</template>

<script>

//import { getMyStartlist } from "@/api/search/search.js"



export default {
    name:"emptycell",
    components: {

    },
    props:{
        cell:null,
        height:null,
        width:null,
    },
    emits:[],
    data(){
        return {

        };
    },
    computed:{
        rootStyle(){
            return {
                height:this.height+'px'
            }
        },
        name(){
            var namt= this.cell?.name;
            if(!!namt){
                return namt;
            }
            return "空白标题"
        },
    },
    mounted() {
        //this.getlist();
    },
    methods:{

    }
}
</script>

<style scoped>
.portal-bar{
    border-bottom: 1px solid #eee;
    margin-bottom: 5px;
    height: 40px;
    line-height: 40px;
    padding-left: 10px;
    color: #777;
    font-weight: 700;
}
.empty-tip{
    height: calc(100% - 55px);
    text-align: center;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #999;
    font-size: 14px;
}
</style>