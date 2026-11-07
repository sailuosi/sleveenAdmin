<template>
    <div>
        <el-form class="gridfm" ref="ptpageformRef" label-width="110px" :rules="rules" :model="focusRow">
            <div class="fm-toolbar" >
                <el-button @click="save">保存</el-button>
                <div class="tiped">
                    提示：所属单位参数将作为页面的基础单位参数，务必设置！位置为服务支持机构时，使用下级组织的门户配置的作为页面的门户配置；参数配置将传递给所有下级；而内容参数只有自己使用；
                </div>
            </div>    

            <fminput v-model="focusRow.PP_Title" label="标题" prop="PP_Title" @change="chineseToPinyinFirstLetterJoin"></fminput>
            <fminput v-model="focusRow.PP_Code" label="编码" prop="PP_Code" placeholder=" "></fminput>
            <el-form-item label="所属单位"  class="fminput" prop="PP_OrgOID">
                <hrselect max-height="50vh" v-model="focusRow.PP_OrgOID" @select="setDept">
                    <el-input v-model="focusRow.PP_OrgName" placeholder="选择..."  :disabled="disabled"></el-input>
                </hrselect>
            </el-form-item>
            <fminput v-model="focusRow.PP_IsOn" label="启用" type="boolean" prop="PP_IsOn"></fminput>              
            
            <fminput v-model="focusRow.PP_Pose" label="位置" type="dict" dict="KB_PtPagePose" prop="PP_Pose"></fminput>
            <fminput v-model="focusRow.PP_Disabled" label="禁用" type="boolean"></fminput>

            <!-- <fminput v-model="focusRow.centerPortal" label="单位级门户"></fminput>
            <fminput v-model="focusRow.deptPortal" label="部门门户"></fminput>
            <fminput v-model="focusRow.postManPortal" label="岗位人员门户"></fminput>
            <fminput v-model="focusRow.postPortal" label="岗位门户"></fminput> -->
            <el-form-item class="fm-toolbar" label="复制门户模板">
                <el-row style="width: 100%; border: 1px solid #ddd; border-radius: 5px; padding: 5px">
                    <el-col :span="12">
                        <el-form-item label="单位级门户" label-width="100px" class="spanrow">
                            <tags :list="focusRow.centerPortal" name="name" :closable="!disabled" @close="removePortal('4')" :addable="!disabled"
                                @add="openPaotal('4')">
                                <template #default="p">
                                    【{{p.row.P_Key}}】{{p.row.P_Name}}
                                </template>
                            </tags>
                        </el-form-item>
                    </el-col>
                    <el-col :span="12">
                        <el-form-item label="部门门户" label-width="100px" class="spanrow">
                            <tags :list="focusRow.deptPortal" name="name" :closable="!disabled" @close="removePortal('1')" :addable="!disabled"
                                @add="openPaotal('1')">
                                <template #default="p">
                                    【{{p.row.P_Key}}】{{p.row.P_Name}}
                                </template>
                            </tags>
                        </el-form-item>
                    </el-col>
                    <el-col :span="12">
                        <el-form-item label="岗位门户" label-width="100px" class="spanrow">
                            <tags :list="focusRow.postPortal" name="name" :closable="!disabled" @close="removePortal('2')" :addable="!disabled"
                                @add="openPaotal('2')">
                                <template #default="p">
                                    【{{p.row.P_Key}}】{{p.row.P_Name}}
                                </template>
                            </tags>
                        </el-form-item>
                    </el-col>
                    <el-col :span="12">
                        <el-form-item label="岗位人员门户" label-width="100px" class="spanrow">
                            <tags :list="focusRow.postManPortal" name="name" :closable="!disabled" @close="removePortal('3')" :addable="!disabled"
                                @add="openPaotal('3')">
                                <template #default="p">
                                    【{{p.row.P_Key}}】{{p.row.P_Name}}
                                </template>
                            </tags>
                        </el-form-item>
                    </el-col>
                    <el-col :span="12">
                        <el-form-item label="APP门户" label-width="100px" class="spanrow">
                            <tags :list="focusRow.appPortal" name="name" :closable="!disabled" @close="removePortal('7')" :addable="!disabled"
                                @add="openPaotal('7')">
                                <template #default="p">
                                    【{{p.row.P_Key}}】{{p.row.P_Name}}
                                </template>
                            </tags>
                        </el-form-item>
                    </el-col>
                </el-row>
                <div class="fm-toolbar tiped">
                    提示：选择门户后，将自动复制该门户配置信息，生成该所属单位的门户。门户编码根据页面编码+固定后缀生成，已经生成门户后若页面编码不变则不会重复生成。
                </div>
            </el-form-item>

            <fminput v-model="focusRow.PP_Theme" label="主题" type="dict" dict="SysPtCellTheme"></fminput>
            <fminput v-model="focusRow.PP_AuthCode" label="权限码"></fminput>

            <fminput v-model="focusRow.PP_Para" label="参数" type="textarea"></fminput>
            <fminput v-model="focusRow.PP_Content" label="内容参数" type="textarea" placeholder="页面内容体参数，使用url格式"></fminput>
            
            <fminput v-model="focusRow.PP_Style" label="自定义样式" type="textarea"></fminput>
            <fminput v-model="focusRow.PP_Css" label="自定义css" type="textarea"></fminput>
            
            <fminput v-model="focusRow.PP_Router" label="路由地址" class="rospan"></fminput>
            <fminput v-model="focusRow.PP_ReportUrl" label="使用情况报表地址" class="rospan"></fminput>
            <fminput v-model="focusRow.PP_Note" label="说明" type="textarea" autosize class="rospan"></fminput>

            <fminput v-model="focusRow.PP_Dept" label="单位编码" disabled></fminput>
            <fminput v-model="focusRow.PP_OrgName" label="单位名称" disabled></fminput>
            
            <template v-if="!adding">
	            <fminput v-model="focusRow.SYS_Deleted" label="记录删除标记" noedit></fminput>
	            <fminput v-model="focusRow.SYS_Created" label="创建日期" noedit></fminput>
	            <fminput v-model="focusRow.SYS_LAST_UPD" label="最后修改日期" noedit></fminput>
            </template>
        </el-form>
        <el-dialog title="选择门户" class="normal" v-model="seePortal" width="70vw" append-to-body @close="seePortal=false">
            <portalselectlist style="height:70vh;" :para="ptPara"
                @select="addPaotalLink"
            >
            </portalselectlist>
        </el-dialog>
    </div>
</template>

<script>
/**
<fminput v-model="focusRow.sppId" label="" ></fminput>
        <fminput v-model="focusRow.SYS_CreatedBy" label="创建用户" ></fminput>
        <fminput v-model="focusRow.SYS_REPLACEMENT" label="授权用户" ></fminput>
        <fminput v-model="focusRow.SYS_POSTN" label="所属岗位" ></fminput>
        <fminput v-model="focusRow.SYS_DIVISION" label="所属部门" ></fminput>
        <fminput v-model="focusRow.SYS_ORG" label="所属组织" ></fminput>
        <fminput v-model="focusRow.SYS_LAST_UPD_BY" label="最后修改用户" ></fminput>
 */
// 接口
import { fetchSysPortPage, saveSysPortPage } from "@s/api/paotou/paotalapi.js"
// 组件
import fminput from "@/ui/common/input/fminput.vue"
import tags from "@/ui/common/tags.vue"
//import hrselect from "@k/views/OrgCommon/org/hrselect.vue"
//import portalselectlist from "@/views/paotalconfig/selector/portalselectlist.vue"
// 插件
import pinyin from 'pinyin';

export default {
    name:"ptpageform",
    components: {
        fminput,tags,//portalselectlist
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
            rules:{
                PP_Title:[{required: true, trigger: 'blur',message:"请输入标题"}],
                PP_Code:[{required: true, trigger: 'blur',message:"请输入编码"}],
                PP_OrgOID:[{required: true, trigger: 'change',message:"请选择所属单位"}],
                PP_IsOn:[{required: true, trigger: 'change',message:"请选择是否启用"}],
                PP_Pose:[{required: true, trigger: 'change',message:"请选择位置"}],
            },
            place:"",
            seePortal:false,
            ptPara:{},
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
        addPaotalLink(v){
            if(this.place=="1"){
                // 部门门户
                this.focusRow.deptPortal=[v[0]];
            }
            if(this.place=="2"){
                // 岗位门户
                this.focusRow.postPortal=[v[0]];
            }
            if(this.place=="3"){
                // 岗位人员门户
                this.focusRow.postManPortal=[v[0]];
            }
            if(this.place=="4"){
                // 单位级门户
                this.focusRow.centerPortal=[v[0]];
            }
            if(this.place=="7"){
                // APP门户
                this.focusRow.appPortal=[v[0]];
            }
            this.seePortal=false;
        },
        removePortal(v){
            if(v=="1"){
                // 部门门户
                this.focusRow.deptPortal=[];
            }
            if(v=="2"){
                // 岗位门户
                this.focusRow.postPortal=[];
            }
            if(v=="3"){
                // 岗位人员门户
                this.focusRow.postManPortal=[];
            }
            if(v=="4"){
                // 单位级门户
                this.focusRow.centerPortal=[];
            }
            if(v=="7"){
                // 单位级门户
                this.focusRow.appPortal=[];
            }
        },
        openPaotal(e){
            this.place=e;
            this.ptPara={
                P_Task: this.place,
            };
            this.seePortal=true;
        },
        chineseToPinyinFirstLetterJoin() {
            if(!this.focusRow.PP_Code){
                if(this.focusRow?.PP_Title){
                    // 使用pinyin库将中文字符串转换为拼音数组
                    const pinyinArray = pinyin(this.focusRow?.PP_Title, {
                        style: pinyin.STYLE_FIRST_LETTER // 获取拼音首字母
                    });
                    // 将拼音数组转换为字符串
                    this.focusRow.PP_Code = pinyinArray.map(item => item[0]).join('');
                }else{
                    this.focusRow.PP_Code = '';
                }
            }
        },
        // 重置表单
        resetForm(refName) {
            if (this.$refs[refName]) {
                this.$refs[refName].resetFields();
            }
        },
        load(){
            this.resetForm("ptpageformRef");
            if(this.adding){
                this.focusRow=this.row;
                return ;
            }
            if(this.focusRow.sppId != this.row.sppId){
                this.getRowById();
            }else{
                this.focusRow=this.row;
            }
        },
        getRowById(){
            fetchSysPortPage(this.row.sppId).then((res)=>{
                this.focusRow=res.data;
            })
        },
        save(){
            this.$refs["ptpageformRef"].validate(valid => {
                if (valid) {
                    var para=[this.focusRow];
                    // console.log(para, "ptpageform--save");
                    if(this.adding){
                        
                        saveSysPortPage(para).then((res)=>{
                            if(res.status == 'success'){
                                this.$message(res.data);
                            }
                            if(res.status == 'error'){
                                this.$message.error(res.info);
                            }
                            this.$emit("close");
                            this.$emit("refresh");
                        })
                    }
                    else{
                        saveSysPortPage(para).then((res)=>{
                            if(res.status == 'success'){
                                this.$message(res.data);
                            }
                            if(res.status == 'error'){
                                this.$message.error(res.info);
                            }
                            this.$emit("close");
                            this.$emit("refresh");
                        })
                    }
                }
            });
        },
        setDept(tar) {
           //console.log(tar);
           this.focusRow.PP_OrgName = tar.OrgName;
           this.focusRow.PP_Dept = tar.Varchar1;
           this.focusRow.PP_Title = this.focusRow.PP_OrgName;
           this.chineseToPinyinFirstLetterJoin();
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
    grid-column: 1/-1;
    margin-bottom: 5px;
}
.rospan{
    grid-column:1/-1;
}
.tiped{
    color:green;
    font-size:14px;
}
</style>

