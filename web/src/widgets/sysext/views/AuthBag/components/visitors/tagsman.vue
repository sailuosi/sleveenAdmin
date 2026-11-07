<template>
    <tags :list="dutyMans" name="Vi_Name" class="tagsinfo" :closable="!disabled" @close="removeDutyMan" :addable="!disabled" action="pop" width="730px" :placement="placement">
        <template #addpop="p">
            <manseach v-if="anyman" ref="comselect" v-model="addingEmp" multiple class="mansh">
                <span class="dialog-footer">
                    <el-button type="primary" @click="doDutyManAdd">确 定</el-button>
                </span>
            </manseach>
            <div v-if="!anyman">
                <deptman  v-model="addingEmp" multiple :deptCode="deptCode" ref="deptman" @load="p.resize()">
                    <template #default>
                        <el-button type="primary" @click="doDutyManAdd" style="margin-left:22px">确 定</el-button>
                    </template>
                </deptman>
            </div>
        </template>
    </tags>
</template>

<script>
/**
   访客下的人员集合
 */

import tags from "@/ui/common/tags.vue"

//选人
import manseach from "@s/views/man/manseach.vue"
import deptman from "@s/views/man/deptman.vue"

import { addHHVisitorItem,removeHHVisitorItem,selectHHVisitorItemlist } from "../../api/HHVisitorItemapi.js"

export default {
    name: 'tagsman',
    components:{
        tags,manseach,deptman
    },
    emits:["saved","savenotclose","update:mans"],
    expose:["save"],
    props:{
        deptCode:null,
        bagOID:null,
        role:{ type:String,default:"" },
        disabled:{ type:Boolean,default:false },
        inflow:{type:Boolean,default:false},//是否为流程审批内显示
        adding: { type: Boolean, default: false },
        placement:{ type:String,default:"top" },
        addinit:{ type:Array,default:()=>[] },//添加时的初始化值。
        anyman:{ type:Boolean,default:false },
    },
    data() {
        return {
            seeAddMan:false,// 选人弹窗控制
            dutyMans:[],//责任人
            addingEmp:[],

        };
    },
    watch:{
        "bagOID":function(){
            this.loadDutyMans();
        },
        "adding":function(){
            this.loadDutyMans();
        },
        "addinit":function(){
            this.loadDutyMans();
        },
    },
    mounted() {
        var vm=this;
        setTimeout(() => {
            this.loadDutyMans();
        }, 500);

    },

    methods: {
        //责任人交互一组方法
        openManAny(){
            this.seeAddMan=true;
        },
        removeDutyMan(row){
            if(this.adding){
                let index = this.dutyMans.findIndex(item => item === row);
                if(index !== -1){
                    this.dutyMans.splice(index, 1);
                }
                return ;
            }
            var vm=this;
            this.$confirm('此操作将永久删除该记录, 是否继续?', '提示', { confirmButtonText: '确定', cancelButtonText: '取消', type: 'warning' })
            .then(() => {
                removeHHVisitorItem(row).then((res) => {

                        vm.loadDutyMans();
                        vm.$emit("savenotclose");
                })

            })

        },
        save(){
            if(this.adding){
                if(this.dutyMans.length==0){
                    return;
                }
                addHHVisitorItem(this.dutyMans).then((res)=>{

                })
            }
        },
        doDutyManAdd(){
            //过滤重复添加的人员。
            var addedID=this.dutyMans.map((li)=>li.Vi_Code);
            var para=[];
            var count = 0;
            this.addingEmp.forEach((man)=>{
                if(addedID.indexOf(man.CON_EMP_NUM)!=-1){
                    count++;
                    return;
                }
                para.push({
                    HH_VisitorBag_FK:this.bagOID,
                    Vi_IsOn:true,
                    Vi_Name:man.ManName,
                    Vi_Code:man.H_MainCode,
                    Vi_Type:this.role,
                    Vi_OID:man.oid,
                    Vi_Src:"anyselect",
                    Vi_Idx:1,
                });
            })
            if(this.adding){
                this.dutyMans=this.dutyMans.concat(para);
                return ;
            }
            addHHVisitorItem(para).then((res)=>{
                this.seeAddMan=false;
                this.addingEmp=[];
                this.loadDutyMans();
                this.$refs.deptman.checkIds = [];
                if(count==0){
                    this.$message({ type: 'success', message: '添加成功！' });
                }
                else{
                    this.$message({ type: 'success', message: '添加成功！（过滤掉'+count+'条重复人员）' });
                }
                this.$emit("saved");
            })
        },
        loadDutyMans(){
            if(this.adding){
                this.dutyMans=this.addinit;
                return ;
            }
            
            if (this.bagOID && this.role) {//两项信息都有的情况下才加载数据
            	selectHHVisitorItemlist({
            		HH_VisitorBag_FK: this.bagOID,
            		Vi_Type: this.role
            	}).then((res)=>{
	                this.dutyMans=res.data;
	                this.$emit("update:mans",res.data);
	            })
            }
        }
    }
};
</script>

<style scoped>
.mansh{
    max-height:40vh;
    overflow:auto;
}
</style>
