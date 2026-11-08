<template>
    <div style="position:relative;">
        <el-button @click="saveChecked" class="save-btn" type="primary">保存</el-button>
        <div class="portbind">
            <div class="left">
                <el-table :data="mlist" style="width: 100%" highlight-current-row @current-change="handleCurrentChange">
                    <el-table-column prop="Gi_Name" label="门户" width="220" />
                </el-table>
            </div>
            <el-transfer class="trans-eeter" v-model="bindOIDs" filterable :props="tranProp"
                :filter-method="filterMethod" filter-placeholder="请输名称以查询" :titles="tTitles" :data="rendlist" >
                <template #default="p">
                    <el-link @click="openCL(p.option)">{{p.option.CL_Title}}</el-link>
                </template>
            </el-transfer>
        </div>
        <el-dialog
            v-model="seeLinkDe"
            title="Tips"
            width="500"
        >
            <linkUses :link="fcousOp">
                <template #header>
                    VC的详情【{{fcousOp.CL_Title}}】
                </template>
            </linkUses>
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

import { selectZHPortallist, loadPaotallist, selectZHCellLinklist } from "@s/api/system/paotalapi.js"
import { loadBindKeys, saveBindKeys } from "../../api/goodsitemapi.js"



import hrtree from "@s/views/org/hrtree.vue"
import linkUses from "./parts/linkUses.vue"

export default {
    name: 'vclinkbind',
    components: {
        hrtree,linkUses
    },
    props: {
        bagOID: null,
        row: null,
    },
    data() {
        return {
            queryParams: {

                P_IsOn: true,
            },
            vclinklist: [],
            currentRow: {},
            mlist: [],
            checkedList: [],
            total: 0,
            tranProp: {
                key: "ZH_CellLinkOID",
                label: "CL_Title"
            },
            bindOIDs: [],
            tTitles: ["可选组件", "已选组件"],
            focusHR: null,
            hasChild: true,
            detail:{},
            fcousOp:{},
            seeLinkDe:false,
        };
    },
    computed: {
        rendlist() {
            var tar = this.vclinklist.concat();
            var nowids = tar.map((li) => li.vclinklist)
            this.checkedList.forEach(li => {
                if (nowids.indexOf(li.vclinklist) == -1) {
                    tar.push(li);
                }
            });
            return tar;
        },
    },
    watch: {
        "bagOID": function () {
            this.getpaotallist();
            this.loadChecked();
        },
    },
    mounted() {
        this.getpaotallist();
        this.loadChecked();
    },

    methods: {
        openCL(e){
            this.fcousOp=e;
            this.seeLinkDe=true;
            
        },
        handleCurrentChange(val) {
            this.currentRow = val;
            this.getlist();
        },
        getpaotallist() {
            if (!!this.page) {
                this.queryParams.ZH_PortPage_FK = this.page.ZH_PortPageOID;
            }
            this.queryParams.HH_GoodsBag_FK = this.bagOID;
            loadPaotallist(this.queryParams).then((res) => {
                this.mlist = res.data;
                //this.total = res.total;
            })
        },
        getlist() {
            if (!this.currentRow || !this.currentRow.Gi_OID) {
                this.vclinklist = [];
                return;
            }
            var para = {
                ZH_Portal_FK: this.currentRow.Gi_OID,
            };

            selectZHCellLinklist(para).then((res) => {
                this.vclinklist = res.data;
                this.total = res.total;
            })
        },
        loadChecked() {
            this.bindOIDs = []; // 清空选中值

            var para = {
                HH_GoodsBag_FK: this.bagOID,
                Gi_Type: "4",
            };

            loadBindKeys(para).then((res) => {
                this.bindVals = res.data;
                var oids = res.data.map((li) => li.Gi_OID);
                this.checkedList = res.data.map((li) => {
                    return {
                        ZH_CellLinkOID: li.Gi_OID,
                        CL_Title: li.Gi_Name,
                    };
                })
                this.bindOIDs = oids;
            })
        },
        saveChecked() {
            var ck = this.bindOIDs;
            var para = {
                HH_GoodsBag_FK: this.bagOID,
                Gi_Type: "4",
                checkMenus: this.bindOIDs,
            };

            saveBindKeys(para).then((res) => {
                this.$message(res.data)
            })
        },
        treeclick(tar) {
            this.focusHR = tar;
            this.changeChild();
        },
        changeChild() {
            if (this.focusHR != null) {
                if (this.hasChild) {
                    this.queryParams.ZH_Organize_FK = this.focusHR.Varchar1;
                }
                else {
                    this.queryParams.ZH_Organize_FK = this.focusHR.HR_OrganizeOID;
                }
                this.getpaotallist();
            }
        },
    },
};
</script>

<style scoped>
.portbind {
    position: relative;
    display: flex;

}

.portbind>.left {
    height: 70vh;
    overflow: auto;
    margin-right: 22px;
}

.trans-eeter {
    width: 100%;
    height: 70vh;
    --el-transfer-panel-width: 300px;
    --el-transfer-panel-body-height: calc(70vh - 50px);
}

.save-btn {
    position: absolute;
    top: 10px;
    left: 585px;
    z-index: 1;
}

.tabbar {
    position: sticky;
    top: 0px;
    z-index: 10;
    background: #fff;
    padding: 5px;
}
</style>
