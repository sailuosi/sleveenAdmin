<template>
    <el-form class="gridfm" label-width="120px">
        <div class="fm-toolbar" >
            <el-button @click="save">保存</el-button>
        </div>    
        <fminput v-model="focusRow.PF_Caption" label="中文名" ></fminput>
        <fminput v-model="focusRow.PF_Field" label="字段名" ></fminput>
        <fminput v-model="focusRow.PF_Idx" label="排序" type="number"></fminput>
        
        <fminput v-model="focusRow.PF_IsOn" label="启用" type="boolean"></fminput>
        
        <fminput v-model="focusRow.PF_Show" label="是否显示" type="boolean"></fminput>
        <fminput v-model="focusRow.PF_Disabled" label="是否禁用" type="boolean"></fminput>
        <fminput v-model="focusRow.PF_ReadOnly" label="是否只读" type="radiobtn" dict="KB_Wether"></fminput>

        <fminput v-model="focusRow.PF_Type" label="类型" ></fminput>
        <fminput v-model="focusRow.PF_Width" label="列宽" ></fminput>
        <fminput v-model="focusRow.PF_Align" label="对齐" ></fminput>
        <fminput v-model="focusRow.PF_TitleAlign" label="标题对齐" ></fminput>
        <fminput v-model="focusRow.PF_CType" label="内容类型" ></fminput>
        <fminput v-model="focusRow.PF_Tip" label="提示" class="rospan"></fminput>
        <fminput v-model="focusRow.PF_ListIdx" label="列表显示序号" type="number"></fminput>
        <fminput v-model="focusRow.PF_Sortable" label="列表点击排序" ></fminput>
        
        <fminput v-model="focusRow.PF_Todo_ListIdx" label="待办列表显示序号" type="number"></fminput>
        <fminput v-model="focusRow.PF_Done_ListIdx" label="已办列表显示序号" type="number"></fminput>
        <fminput v-model="focusRow.PF_Node_ListIdx" label="节点列表显示序号" type="number"></fminput>
        
        <fminput v-model="focusRow.PF_TitleStyle" label="标题样式" class="rospan" type="textarea"></fminput>
        <fminput v-model="focusRow.PF_ValueStyle" label="内容样式" class="rospan" type="textarea"></fminput>
        <fminput v-model="focusRow.PF_DefaultVal" label="默认值" ></fminput>
        <fminput v-model="focusRow.PF_CodeTable" label="代码表" ></fminput>
        <fminput v-model="focusRow.PF_Min" label="最小" ></fminput>
        <fminput v-model="focusRow.PF_Max" label="最大" ></fminput>
        <fminput v-model="focusRow.PF_Remark" label="备注" class="rospan"></fminput>
        <fminput v-model="focusRow.spsId" label="页面查询" noedit></fminput>
        <fminput v-model="focusRow.spPortPageId" label="门户页面" noedit></fminput>
        <template v-if="!adding">
            <fminput v-model="focusRow.spfId" label="oid" noedit></fminput>
        </template>
    </el-form>
</template>

<script>

//import { fetchSysPageField,addZHPageField,updateZHPageField } from "@/api/system/paotou/pagefieldapi.js"

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
            if(this.focusRow.spfId != this.row.spfId){
                this.getRowById();
            }
        },
        getRowById(){
            fetchSysPageField(this.row.spfId).then((res)=>{
                this.focusRow=res.data;
            })
        },
        save(){
            var para=[this.focusRow];
            if(this.adding){
                
                addZHPageField(para).then((res)=>{
                    this.$message(res.data);
                    this.$emit("close");
                    this.$emit("refresh");
                })
            }
            else{
                updateZHPageField(para).then((res)=>{
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