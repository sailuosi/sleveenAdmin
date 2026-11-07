<template>
    <el-form class="gridfm" label-width="110px">
        <div class="fm-toolbar" >
            <el-button @click="save">保存</el-button>
        </div>    

        <fminput v-model="focusRow.Gb_Name" label="名称" ></fminput>
        <fminput v-model="focusRow.Gb_Code" label="编号" ></fminput>
        <fminput v-model="focusRow.Gb_Note" label="说明" class="spanrow" type="textarea"></fminput>
        
        <fminput v-model="focusRow.Gb_Type" label="类型" ></fminput>
        <fminput v-model="focusRow.Gb_Idx" label="序号" ></fminput>
        <fminput v-model="focusRow.Gb_Remark" label="备注" class="spanrow"></fminput>
        <template v-if="!adding">
            
            <fminput v-model="focusRow.SYS_Created" label="创建日期" noedit></fminput>
            <fminput v-model="focusRow.SYS_LAST_UPD" label="最后修改日期" noedit></fminput>
               
        </template>
    </el-form>
</template>

<script>
/*<fminput v-model="focusRow.HH_GoodsBagOID" label="主键" noedit></fminput>
            <fminput v-model="focusRow.SYS_Deleted" label="记录删除标记" noedit></fminput>
            <fminput v-model="focusRow.SYS_CreatedBy" label="创建用户" noedit></fminput>
            <fminput v-model="focusRow.SYS_REPLACEMENT" label="授权用户" noedit></fminput>
            <fminput v-model="focusRow.SYS_POSTN" label="所属岗位" noedit></fminput>
            <fminput v-model="focusRow.SYS_DIVISION" label="所属部门" noedit></fminput>
            <fminput v-model="focusRow.SYS_ORG" label="所属组织" noedit></fminput>
            <fminput v-model="focusRow.SYS_LAST_UPD_BY" label="最后修改用户" noedit></fminput>  
*/
import { getHHGoodsBagById,addHHGoodsBag,updateHHGoodsBag } from "../../../api/goodsbagapi.js"

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
            if(this.focusRow.HH_GoodsBagOID != this.row.HH_GoodsBagOID){
                this.getRowById();
            }
        },
        getRowById(){
            getHHGoodsBagById(this.row.HH_GoodsBagOID).then((res)=>{
                this.focusRow=res.data;
            })
        },
        save(){
            var para=[this.focusRow];
            if(this.adding){
                
                addHHGoodsBag(para).then((res)=>{
                    this.$message(res.data);
                    this.$emit("close");
                    this.$emit("refresh");
                })
            }
            else{
                updateHHGoodsBag(para).then((res)=>{
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

.spanrow{
    grid-column:1/-1;
}

</style>