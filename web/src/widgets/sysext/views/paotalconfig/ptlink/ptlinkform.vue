<template>
    <el-form class="gridfm" label-width="110px">
        <div class="fm-toolbar" >
            <el-button @click="save">保存</el-button>
        </div>    
        

        <fminput v-model="focusRow.PL_Target_FK" label="关联业务主键" noedit v-if="isAdmin"></fminput>
        <fminput v-model="focusRow.PL_Name" label="关联对象名称" ></fminput>
        <fminput v-model="focusRow.PL_Code" label="关联对象编号" ></fminput>
        <fminput v-model="focusRow.PL_Type" label="关联类型" ></fminput>
        <fminput v-model="focusRow.PL_IsOn" label="启用" type="boolean"></fminput>
        <fminput v-model="focusRow.PL_Idx" label="排序" ></fminput>
        <fminput v-model="focusRow.PL_Title" label="标题" ></fminput>
        <fminput v-model="focusRow.PL_Style" label="样式" ></fminput>
        <fminput v-model="focusRow.PL_Place" label="位置" type="dict" dict="KB_HOPtLinkPlace"></fminput>
        <fminput v-model="focusRow.PL_Css" label="Css" ></fminput>
        <fminput v-model="focusRow.PL_Para" label="参数" class="spanrow"></fminput>
        <fminput v-model="focusRow.PL_Note" label="说明" class="spanrow"></fminput>
        <fminput v-model="focusRow.PL_Key" label="识别码" ></fminput>

        <fminput v-model="focusRow.spPortalId" label="门户" v-if="isAdmin" noedit></fminput>
        <template v-if="!adding">
            <fminput v-model="focusRow.splId" label="主键" noedit v-if="isAdmin"></fminput>
            <fminput v-model="focusRow.SYS_Created" label="创建日期" noedit></fminput>
            <fminput v-model="focusRow.SYS_LAST_UPD" label="最后修改日期" noedit></fminput>
            
        </template>
    </el-form>
</template>

<script>
/**
        <fminput v-model="focusRow.SYS_CreatedBy" label="创建用户" ></fminput>
        <fminput v-model="focusRow.SYS_REPLACEMENT" label="授权用户" ></fminput>
        <fminput v-model="focusRow.SYS_POSTN" label="所属岗位" ></fminput>
        <fminput v-model="focusRow.SYS_DIVISION" label="所属部门" ></fminput>
        <fminput v-model="focusRow.SYS_ORG" label="所属组织" ></fminput>
        <fminput v-model="focusRow.SYS_LAST_UPD_BY" label="最后修改用户" ></fminput>
        <fminput v-model="focusRow.SYS_Deleted" label="记录删除标记" ></fminput>
 */
//import { getZHPaotalLinkById,addZHPaotalLink,updateZHPaotalLink } from "@/api/system/paotou/portalinkapi.js"

import fminput from "@/ui/common/input/fminput.vue"

import { mapState,mapStores,mapWritableState} from "pinia";
import {useUserInfo} from "@/stores/userInfo.ts"

export default {
    name:"ptlinkform",
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
        ...mapState(useUserInfo, ['userInfos', 'constList']),
        isAdmin(){
            return this.userInfos.account=="superadmin"
        },
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
            if(this.focusRow.splId != this.row.splId){
                this.getRowById();
            }
        },
        getRowById(){
            getZHPaotalLinkById(this.row.splId).then((res)=>{
                this.focusRow=res.data;
            })
        },
        save(){
            var para=[this.focusRow];
            if(this.adding){
                
                addZHPaotalLink(para).then((res)=>{
                    this.$message(res.data);
                    this.$emit("close");
                    this.$emit("refresh");
                })
            }
            else{
                updateZHPaotalLink(para).then((res)=>{
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