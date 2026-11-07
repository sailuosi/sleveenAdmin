<template>
    <lazydiv @init="loadDutyOrgs">
        <div>
            <hrselect max-height="50vh" @select="doDutyOrgAdd" :show="seeDutyOrgPop" @close="seeDutyOrgPop" 
                style="display:inline-block;" placement="top" v-clickout="outsideClickHandler"
                    :disabled="disabled"
                >
                <tags :list="dutyOrgs" name="Vi_Name" :closable="!disabled" @close="doRemoveDutyOrg" :addable="!disabled"
                    @add="openOrgSelect">
                </tags>
                <template #top>
                    <b>单位选择</b>
                </template>
            </hrselect>            
        </div>

    </lazydiv>
</template>

<script>
import tags from "@/ui/common/tags.vue"
import hrselect from "@s/views/org/hrselect.vue"
import { selectHHVisitorItemlist, addHHVisitorItem,  removeHHVisitorItem } from "../../api/HHVisitorItemapi.js"

//import clickOutside from "@/directive/clickoutside.js";

import lazydiv from "@/ui/panels/lazydiv.vue"

export default {
    name: 'WebTagsorg',
    components: {
        tags,
        lazydiv, hrselect
    },
    directives:{
        clickout:  {
            beforeMount(el, binding) {
                el.clickOutsideEvent = (event) => {
                // 检查点击是否发生在el之外
                if (!(el ).contains(event.target )) {
                    binding.value(event);
                }
                };
                document.addEventListener('click', el.clickOutsideEvent);
            },
            unmounted(el) {
                document.removeEventListener('click', el.clickOutsideEvent);
                delete el.clickOutsideEvent;
            },
        }
    },
    emits:["saved","savenotclose"],
    expose:["save"],
    props: {
        role: { type: String, default: "" },
        bagOID: null,
        disabled: {
            type: Boolean,
            default: false
        },
        adding: {
            type: Boolean,
            default: false
        },
        //是否简化，简化模式给用户使用，屏蔽底层外键、高级设置等内容。
        simple: {
            type: Boolean,
            default: false
        },
        addinit: { type: Array, default: [] },
    },
    data() {
        return {
            seeDutyOrgPop: false,
            dutyOrgs: [], //责任部门
        };
    },
    computed:{
        listpara(){
            return {
                HH_VisitorBag_FK: this.bagOID,
                Vi_Type: this.role
            }
        }
    },
    watch:{
        "listpara":function(){
            this.loadDutyOrgs();
        },
        adding:function(){
            this.loadDutyOrgs();
        }
    },
    methods: {
        outsideClickHandler() {
            this.seeDutyOrgPop = !this.seeDutyOrgPop;
        },
        openOrgSelect() {
            this.seeDutyOrgPop = !this.seeDutyOrgPop;
        },
        //责任部门交互一组方法
        doRemoveDutyOrg(row) {
            if(this.adding){
                let index = this.dutyOrgs.findIndex(item => item === row);
                if(index !== -1){
                    this.dutyOrgs.splice(index, 1);
                }
                return;
            }
            var vm = this;
            this.$confirm('此操作将永久删除该记录, 是否继续?', '提示', { confirmButtonText: '确定', cancelButtonText: '取消', type: 'warning' })
            .then(() => {
                removeHHVisitorItem(row).then((res) => {
                    vm.loadDutyOrgs();
                    vm.$emit("saved");
                })

            })
        },
        save(){
            if(this.adding){
                if(this.dutyOrgs.length==0){
                    return;
                }
                addHHVisitorItem(this.dutyOrgs).then((res)=>{

                })
            }
        },
        doDutyOrgAdd(org) {
            var para = [];

            para.push({
                HH_VisitorBag_FK:this.bagOID,
                Vi_IsOn:true,
                Vi_Name:org.O_OrgName,
                Vi_Code:org.O_ClassCode,
                Vi_Type:this.role,
                Vi_OID:org.HR_OrganizeOID,
                Vi_Src:"hrselect",
                Vi_Idx:1,
            });
            // console.log(this.adding, this.dutyOrgs, "doDutyOrgAdd");
            if(this.adding){
                let index = this.dutyOrgs.findIndex(item => item.Vi_Code == para[0].Vi_Code);
                // console.log(index, para, this.dutyOrgs, "doDutyOrgAdd");
                if(index !== -1){
                    this.$message({ type: 'info', message: '该部门已存在' });
                    return;
                }
                this.dutyOrgs=this.dutyOrgs.concat(para);
                return ;
            }
            addHHVisitorItem(para).then((res) => {
                this.seeDutyOrgPop = false;
                //this.addingEmp=[];
                this.loadDutyOrgs();
                this.$emit("saved");
            })
        },
        loadDutyOrgs() {
            if(this.adding){
                this.dutyOrgs = this.addinit;
                return ;
            }
            
            if (this.listpara.HH_VisitorBag_FK && this.listpara.Vi_Type) {//两项信息都有的情况下才加载数据
            	this.dutyOrgs = [];
            	selectHHVisitorItemlist(this.listpara).then((res) => {
	                this.dutyOrgs = res.data;
	            });
            }
        }
    }
};
</script>

<style scoped></style>
