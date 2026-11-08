<template>
<div style="position:relative;">
    <el-button @click="saveChecked" class="save-btn" type="primary">保存</el-button>
    <div class="portbind">
        <div class="left" >
            <div class="tabbar">
                包含下级<el-switch v-model="hasChild" @change="changeChild"></el-switch>
                <el-link @click="saveAll">全部</el-link>
            </div>
            <hrtree
                @nodeclick="treeclick"
            >

            </hrtree>
        </div>
        <el-transfer class="trans-eeter"
            v-model="bindOIDs"
            filterable :props="tranProp"
            :filter-method="filterMethod"
            filter-placeholder="请输入门户名称以查询"
            :titles="tTitles"
            :data="rendlist"
        >                
            <template #default="p">
                <el-link @click="openCL(p.option)">{{p.option.P_Name}}</el-link>
            </template>
        </el-transfer>
    </div>

    <el-dialog
        v-model="seeLinkDe"
        title="Tips"
        width="500"
    >
        <ptLinkUses :oid="fcousOp.ZH_PortalOID">
            <template #header>
                门户的详情【{{fcousOp.P_Name}}】
            </template>
        </ptLinkUses>
        <template #footer>
        <div class="dialog-footer">
            <el-button @click="dialogVisible = false">Cancel</el-button>
            <el-button type="primary" @click="dialogVisible = false">
            Confirm
            </el-button>
        </div>
        </template>
    </el-dialog>
</div>
</template>

<script>

import { selectZHPortallist } from "@s/api/system/paotalapi.js"
import { loadBindKeys,saveBindKeys } from "../../api/goodsitemapi.js"

import hrtree from "@s/views/org/hrtree.vue"
import ptLinkUses from "./parts/ptLinkUses.vue"

export default {
    name: 'WebPortalbind',
    components:{
        hrtree,ptLinkUses
    },
    props:{
        bagOID:null,
        row:null,
    },
    data() {
        return {
            queryParams:{
                P_IsOn:true,
            },
            mlist:[],
            checkedList:[],
            total:0,
            tranProp:{
                key:"ZH_PortalOID",
                label:"P_Name"
            },
            bindOIDs:[],
            tTitles:["可选门户","已选门户"],
            focusHR:null,
            hasChild:true,
            fcousOp:{},
            seeLinkDe:false,
        };
    },
    computed:{
        rendlist(){
            var tar= this.mlist.concat();
            var nowids= tar.map((li)=>li.ZH_PortalOID)
            this.checkedList.forEach(li => {
                if(nowids.indexOf(li.ZH_PortalOID)==-1){
                    tar.push(li);
                }
            });
            return tar;
        },
    },
    watch:{
        "bagOID":function(){
            this.loadChecked();
        },
    },
    mounted() {
        this.getlist();
        this.loadChecked();
    },

    methods: {
        openCL(e){
            this.fcousOp=e;
            this.seeLinkDe=true;
            
        },
        getlist(){
            if(!!this.page){
                this.queryParams.ZH_PortPage_FK=this.page.ZH_PortPageOID;
            }
            
            selectZHPortallist(this.queryParams).then((res)=>{
                this.mlist=res.data;
                this.total=res.total;
            })
        },
        loadChecked(){
            this.bindOIDs=[]; // 清空选中值

            var para={
                HH_GoodsBag_FK:this.bagOID,
                Gi_Type:"3",
            };

            loadBindKeys(para).then((res)=>{
                this.bindVals=res.data;
                var oids=res.data.map((li)=>li.Gi_OID);
                this.checkedList=res.data.map((li)=>{
                    return {
                        ZH_PortalOID:li.Gi_OID,
                        P_Name:li.Gi_Name,
                    };
                })
                this.bindOIDs=oids;
            })
        },
        saveChecked(){
            var ck= this.bindOIDs;
            var para={
                HH_GoodsBag_FK:this.bagOID,
                Gi_Type:"3",
                checkMenus:this.bindOIDs,
            };

            saveBindKeys(para).then((res)=>{
                this.$message(res.data)
            })
        },
        treeclick(tar){
            this.focusHR=tar;
            this.changeChild();
        },
        changeChild(){
            if(this.focusHR != null){
                if(this.hasChild){
                    this.queryParams.ZH_Organize_FK=this.focusHR.Varchar1;
                }
                else{
                    this.queryParams.ZH_Organize_FK=this.focusHR.HR_OrganizeOID;
                }
                this.getlist();
            }
        },
    },
};
</script>

<style scoped>
.portbind{
    position: relative;
    display: flex;

}
.portbind>.left{
    height:70vh;
    overflow:auto;
    margin-right:22px;
}

.trans-eeter{
    width:100%;
    height:70vh;
    --el-transfer-panel-width: 300px;
    --el-transfer-panel-body-height: calc(70vh - 50px);
}

.save-btn{
    position: absolute;
    top: 10px;
    left: 585px;
    z-index:1;
}
.tabbar{
    position:sticky;
    top:0px;
    z-index:10;
    background:#fff;
    padding:5px;
}

</style>
