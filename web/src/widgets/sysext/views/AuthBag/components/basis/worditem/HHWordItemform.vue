<template>
<div>
    <el-form class="gridfm" label-width="110px">
        <div class="fm-toolbar" >
            <el-button @click="save">保存</el-button>
        </div>    
        
        <fminput v-model="focusRow.Wi_Name" label="名称" ></fminput>
        <fminput v-model="focusRow.Wi_Code" label="编号" ></fminput>
        <fminput v-model="focusRow.Wi_Type" label="类型" ></fminput>
        <fminput v-model="focusRow.Wi_Note" label="说明" ></fminput>
        
        <el-form-item label="指定单位" class="spanrow"
        	v-if="focusRow.Wi_Code == '20' || focusRow.Wi_Code == '21'">
            <tags :list="focusRow.orgs" name="Name" :closable="true" :addable="true"
                @add="openOrgSelect" @close="doRemoveDutyOrg">
            </tags>
        </el-form-item>
		
        <template v-if="!adding">


        </template>
    </el-form>
    
    <el-dialog v-model="showHrTree" width="300px" top="5vh" :close-on-click-modal="false" append-to-body>
        <hrtree showCheck strict @orgcheck="setChecked" :checked="selectedOrgs"></hrtree>
        <template #footer>
        	<el-button type="info" @click="showHrTree = false">关闭</el-button>
        	<el-button type="primary" @click="submitOrgSelect">确定</el-button>
        </template>
    </el-dialog>
</div>
</template>

<script>
/*
            <fminput v-model="focusRow.HH_WordBag_FK" label="词条集合" noedit></fminput>
            <fminput v-model="focusRow.HH_WordDefine_FK" label="词条定义" noedit></fminput>
            <fminput v-model="focusRow.HH_WordItemOID" label="主键" noedit></fminput>
*/
import { getHHWordItemById, saveHHWordItem, selectHHWordItemOrglist } from "../../../api/HHWordItemapi.js";

import fminput from "@/ui/common/input/fminput.vue";
import tags from "@/ui/common/tags.vue";
import hrtree from "@s/views/org/hrtree.vue";

export default {
    name: "HHWordItemform",
    components: {
        fminput,
        tags,
        hrtree
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
            showHrTree: false,
            checkedNodes: [],
            selectedOrgs: []
        };
    },
    computed:{
    	
    },
    mounted() {
         this.load();
    },
    watch:{
        "row": function(val){
            this.load();
        }
    },
    methods:{
        load() {
            if (this.adding) {
                this.focusRow = this.row;
                this.loadOrgs(this.row.HH_WordItemOID);
            } else  if(this.focusRow.HH_WordItemOID != this.row.HH_WordItemOID) {
                this.getRowById();
            }
        },
        loadOrgs(HH_WordItemOID) {
        	if (HH_WordItemOID) {
        		selectHHWordItemOrglist({
	            	HH_WordItemOID: HH_WordItemOID
	            }).then(res => {
	            	this.focusRow.orgs = res.data;
	            });
        	} else {
        		this.focusRow.orgs = [];
        	}
        },
        getRowById() {
            getHHWordItemById(this.row.HH_WordItemOID).then((res) => {
                this.focusRow = res.data;
                
                this.loadOrgs(this.row.HH_WordItemOID);
            });
        },
        save() {
            saveHHWordItem(this.focusRow).then((res)=>{
                this.$message(res.data);
                this.$emit("close");
                this.$emit("refresh");
            });
        },
        openOrgSelect() {
        	this.selectedOrgs = [];
        	if (this.focusRow.orgs && this.focusRow.orgs.length > 0) {
        		this.selectedOrgs = this.focusRow.orgs.map((o) => o.HROID);
        	}
        	this.showHrTree = true;
        },
        doRemoveDutyOrg(node) {
        	let index = this.focusRow.orgs.indexOf(node);
        	this.focusRow.orgs.splice(index, 1);
        },
        setChecked(node, tree) {
        	this.checkedNodes = tree.checkedNodes;
        },
        submitOrgSelect() {
        	if (!this.focusRow.orgs) {
        		this.focusRow.orgs = [];
        	}
        	
        	this.checkedNodes.forEach((node) => {
        		if (this.selectedOrgs.indexOf(node.HR_OrganizeOID) < 0) {
        			this.focusRow.orgs.push({
        				HROID: node.HR_OrganizeOID,
        				HRCode: node.Varchar1,
        				OrgNo: node.OrgNO,
        				Name: node.OrgName
        			});
        		}
        	});
        	
        	this.showHrTree = false;
        }
    }
}
</script>

<style scoped>
.gridfm{
    display:grid;
    grid-template-columns:1fr;
}
.fm-toolbar{
    grid-column:1/-1;
    margin-bottom:15px;
}
</style>