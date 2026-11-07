<template>
<tags :list="postMans" name="roleName" :closable="!disabled" @close="removePostMan" :addable="!disabled" action="pop" width="300px" >
    <template #addpop>

        <rolesearch ref="comselect" v-model="addingEmp" multiple>
            <span class="dialog-footer">
                <el-button type="primary" @click="doPostManAdd">确 定</el-button>
            </span>        
        </rolesearch>

    </template>
    <template #focus="p">

    </template>
</tags>
</template>

<script>
/** position="dialog"
      <el-dialog title="提示" :visible="seeAddMan" width="500px" @close="seeAddMan=false" append-to-body>
      <manany ref="comselect" v-model="addingEmp" multiple></manany>
      <span slot="footer" class="dialog-footer">
                  <el-button type="primary" @click="doDutyManAdd">确 定</el-button>
              </span>
      </el-dialog>
*/

import tags from "@/ui/common/tags.vue"

//选人
import rolesearch from "@/views/system/role/selector/rolesearch.vue"

import {
    fetchSysPortalRolePage,
    createSysPortalRole,
    deleteSysPortalRole
} from "@s/api/paotou/paotalapi.js"



export default {
    name: 'postmans',
    components: {
        tags,
        rolesearch,
        
    },
    props: {
        row:null,
        type: null,
        disabled: {
            type: Boolean,
            default: false
        },
        inflow: {
            type: Boolean,
            default: false
        }, //是否为流程审批内显示
    },
    data() {
        return {
            seeAddMan: false, // 选人弹窗控制
            postMans: [], //岗位人员
            addingEmp: [],
        };
    },
    computed:{
        listpara(){
            var tar= {
                PR_Level:this.type,
            };
            if(this.type=="1"){
                tar.spPortalId=this.row.spId;
            }
            return tar;
        }
    },
    watch: {
        "listpara": function () {
            this.loadPostMans();
        }
    },
    mounted() {
        var vm = this;
        setTimeout(() => {
            this.loadPostMans();
        }, 500);

    },

    methods: {
        //责任人交互一组方法
        openManAny() {
            this.seeAddMan = true;
        },
        removePostMan(row) {
            var vm = this;
            alertIsOK("确认要删除吗？删除后无法恢复！", function (e) {
                if (e == "yes") {
                    deleteSysPortalRole(row).then((res) => {
                        vm.loadPostMans();
                    })
                }
            }, "岗位人员：" + row.MC_MainName);

        },
        getBaseObj(){
            var tar= {
                PR_Level:this.type,
            };
            if(this.type=="1"){
                tar.spPortalId=this.row.spId;
            }
            return tar;
        },
        doPostManAdd() {
            //过滤重复添加的人员。
            var addedID = this.postMans.map((li) => li.PR_RleID);
            var para = [];
            this.addingEmp.forEach((man) => {
                if (addedID.indexOf(man.id) != -1) {
                    return;
                }
                var t=this.getBaseObj();
                t.PR_RleID=man.id;
                para.push(t);
            })
            createSysPortalRole(para).then((res) => {
                this.seeAddMan = false;
                this.addingEmp = [];
                this.loadPostMans();
            })
        },
        loadPostMans() {

            if(!this.type){
                this.postMans=[];
                return ;
            }
            fetchSysPortalRolePage(this.listpara).then((res) => {
                this.postMans = res.data;
            })
        },
    },
};
</script>

<style scoped>

</style>
