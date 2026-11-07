<template>
    <el-form class="gridfm" label-width="110px">
        <div class="fm-toolbar" >
            <el-button @click="save">保存</el-button>
        </div>    

        <fminput v-model="focusRow.Gi_Name" label="名称" ></fminput>
        <fminput v-model="focusRow.Gi_Code" label="编号" ></fminput>
        <fminput v-model="focusRow.Gi_Type" label="类型" type="dict" dict="HH_GoodsTypes"></fminput>
        <fminput v-model="focusRow.Gi_OID" label="OID" ></fminput>
        <fminput v-model="focusRow.HH_GoodsBag_FK" label="资源集合" disabled></fminput>
        <fminput v-model="focusRow.Gi_Role" label="角色" type="radiobtn" :dictlist="roleList"></fminput>
        <template v-if="!adding">
        
        </template>
    </el-form>
</template>

<script>
/**
        <fminput v-model="focusRow.HH_GoodsItemOID" label="" ></fminput>
 */
import { getHHGoodsItemById,addHHGoodsItem,updateHHGoodsItem } from "../../../api/goodsitemapi.js"

import fminput from "@/ui/common/input/fminput.vue"

export default {
    name:"cnPageForm",
    components: {
        fminput
    },
    props:{
        row:null,
        disabled:{ type:Boolean,default:false },
        adding:{ type:Boolean,default:false },
    },
    emits:[],
    data(){
        return {
            focusRow:{},
            diaRow:false,//行编辑表单
            pageTypes:[
                { label:"url链接",value:"1"},
                { label:"静态路由",value:"2"},
                { label:"已注册vue组件",value:"3"},
            ],
            onoff:[
                { label:"启用",value:"1"},
                { label:"停用",value:"0"},
            ],
            roleList:[
                { label:"可用",value:"1"},
                { label:"只读",value:"2"},
            ],
        };
    },
    computed:{
    },
    mounted() {
        //this.getlist();
         this.load();
    },
    watch:{
        "row":function(val){
            this.load();
        },
    },    
    methods:{
        load(){
            if(this.adding){
                this.focusRow=this.row;
                return ;
            }
            if(this.focusRow.HH_GoodsItemOID != this.row.HH_GoodsItemOID){
                this.getRowById();
            }
        },
        getRowById(){
            getHHGoodsItemById(this.row.HH_GoodsItemOID).then((res)=>{
                this.focusRow=res.data;
            })
        },
        save(){
            var para=[this.focusRow];
            if(this.adding){
                
                addHHGoodsItem(para).then((res)=>{
                    this.$message(res.data);
                    this.$emit("close");
                    this.$emit("refresh");
                })
            }
            else{
                updateHHGoodsItem(para).then((res)=>{
                    this.$message(res.data);
                    this.$emit("close");
                    this.$emit("refresh");
                })
            }
        },
    }
}
</script>

<style scoped>
.gridfm{
    display:grid;
    grid-template-columns:1fr 1fr;
}
.fm-toolbar{
    grid-column:1/-1;
    margin-bottom:15px;
}
</style>