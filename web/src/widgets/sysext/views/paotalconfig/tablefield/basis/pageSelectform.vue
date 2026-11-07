<template>
    <el-form class="gridfm" label-width="110px">
        <div class="fm-toolbar" >
            <el-button @click="save">保存</el-button>
        </div>    
        <fminput v-model="focusRow.PS_Task" label="业务名称" ></fminput>
        <fminput v-model="focusRow.PS_IsOn" label="启用" type="boolean"></fminput>

        <fminput v-model="focusRow.PS_Title" label="标题" class="rospan"></fminput>
        <fminput v-model="focusRow.PS_Nick" label="简称" ></fminput>
        <fminput v-model="focusRow.PS_Table" label="表名" ></fminput>
        <fminput v-model="focusRow.PS_Key" label="唯一编码" ></fminput>
        <fminput v-model="focusRow.PS_SQL" label="查询SQL" class="rospan"  type="textarea"></fminput>

        <fminput v-model="focusRow.PS_Note" label="说明" class="rospan" type="textarea"></fminput>
        
        
        <fminput v-model="focusRow.PS_Para" label="参数" class="rospan"  type="textarea"></fminput>
        <selectconfig class="rospan" :pageOID="focusRow.spsId" v-model="focusRow.PS_Content" v-if="!!focusRow.spsId">
        </selectconfig>
        <fminput v-model="focusRow.PS_Content" label="内容" class="rospan"  type="textarea" fake></fminput>
        <fminput v-model="focusRow.PS_Remark" label="备注" class="rospan"></fminput>
        <fminput v-model="focusRow.spPortPageId" label="门户页面" ></fminput>
        <template v-if="!adding">
            <fminput v-model="focusRow.spsId" label="oid" noedit></fminput>
        </template>
    </el-form>
</template>

<script>

//import { fetchSysPageSelect,addZHPageSelect,updateZHPageSelect } from "@/api/system/paotou/pageselectapi.js"

import fminput from "@/ui/common/input/fminput.vue"
import selectconfig from "../parts/selectconfig.vue"


export default {
    name:"cnPageForm",
    components: {
        fminput,selectconfig
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
            if(this.focusRow.spsId != this.row.spsId){
                this.getRowById();
            }
        },
        getRowById(){
            fetchSysPageSelect(this.row.spsId).then((res)=>{
                this.focusRow=res.data;
            })
        },
        save(){
            var para=[this.focusRow];
            if(this.adding){
                
                addZHPageSelect(para).then((res)=>{
                    this.$message(res.data);
                    this.$emit("close");
                    this.$emit("refresh");
                })
            }
            else{
                updateZHPageSelect(para).then((res)=>{
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
.rospan{
    grid-column:1/-1;
}

</style>