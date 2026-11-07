<template>
<div>

    <el-tabs type="border-card">
        <el-tab-pane label="权限条目">
            <dutyitemlist class="pageplus">

            </dutyitemlist>
        </el-tab-pane>
        <el-tab-pane label="资源集合" lazy>
            <goodsbaglist class="pageplus" opWidth="250px">
                <template #opmore="p">
                    <el-link type="primary" @click="openMenuBind(p.row)" class="linkop">菜单</el-link>
                    <el-link type="primary" @click="openPageBind(p.row)" class="linkop">页面</el-link>
                    <el-link type="primary" @click="openPortalBind(p.row)" class="linkop">门户</el-link>
                    <el-link type="primary" @click="openVcLinkBind(p.row)" class="linkop">组件</el-link>
                </template>
            </goodsbaglist>
        </el-tab-pane>
        <el-tab-pane label="访客集合" lazy>
            <HHVisitorBaglist class="pageplus"></HHVisitorBaglist>
        </el-tab-pane>
        <el-tab-pane label="词条集合" lazy>
            <HHWordBaglist class="pageplus"></HHWordBaglist>
        </el-tab-pane>
        <el-tab-pane label="词条定义" lazy>
            <HHWordDefinelist class="pageplus" opWidth="150px">
                <template #opmore="p">
                    <el-link type="primary" @click="openRuleEdit(p.row)" class="linkop">语义</el-link>
                </template>
            </HHWordDefinelist>
        </el-tab-pane>
    </el-tabs>

    <el-dialog v-model="seeMenu" title="设置绑定菜单" width="400" class="menu-bind-dialog">
        <div class="menu-bind-body">
            <menubind :row="curGooos"></menubind>
        </div>
    </el-dialog>
    <el-dialog    v-model="seePage"    title="设置绑定页面"    width="1100px"  >
        <pagebind
            :bagOID="curGooos?.HH_GoodsBagOID"
        ></pagebind>
    </el-dialog>
    <el-dialog    v-model="seePaotal"    title="设置绑定门户"    width="1100px"  >
        <portalbind
            :bagOID="curGooos?.HH_GoodsBagOID"
        ></portalbind>
    </el-dialog>
    <el-dialog    v-model="seeVC"    title="设置绑定组件"    width="1100px"  >
        <vclinkbind
            :bagOID="curGooos?.HH_GoodsBagOID"
        ></vclinkbind>
    </el-dialog>
    <el-dialog    v-model="seeRuleEdit"    title="设置语义解析引擎"    width="900px"  >
        <ruleEdit
            :rule="wordRule" ref="rulesetter"
        ></ruleEdit>
        <template #footer>
            <el-button type="primary" @click="saveRule">保存</el-button>
        </template>
    </el-dialog>
</div>
</template>

<script setup>
import { reactive, computed, nextTick, ref, onMounted } from 'vue';
import { ElNotification } from 'element-plus'

import dutyitemlist from "../components/basis/dutyitemlist.vue"

import goodsbaglist from "../components/basis/goods/goodsbaglist.vue"

import goodsitemlist from "../components/basis/goods/goodsitemlist.vue"

import HHVisitorBaglist from "../components/basis/visitorbag/HHVisitorBaglist.vue"

import HHWordBaglist from "../components/basis/wordbag/HHWordBaglist.vue"

import HHWordDefinelist from "../components/basis/worddefine/HHWordDefinelist.vue"

//菜单绑定配置页面
import menubind from "../components/menu/menubind.vue"
//页面绑定配置
import pagebind from "../components/portal/pagebind.vue"
//门户绑定配置页面
import portalbind from "../components/portal/portalbind.vue"
//link绑定配置页面
import vclinkbind from "../components/portal/vclinkbind.vue"
import ruleEdit from "../components/word/ruleEdit.vue"
import { updateHHWordDefine } from "../api/HHWordDefineapi.js"

//配置菜单控制属性
const seeMenu=ref(false);
const curGooos=ref(null);
function openMenuBind(row){
    curGooos.value=row;
    seeMenu.value=true;
}
//配置页面控制属性
const seePage=ref(false);
function openPageBind(row){
    curGooos.value=row;
    seePage.value=true;
}
//配置门户控制属性
const seePaotal=ref(false);
function openPortalBind(row){
    curGooos.value=row;
    seePaotal.value=true;
}
//配置门户控制属性
const seeVC=ref(false);
function openVcLinkBind(row){
    curGooos.value=row;
    seeVC.value=true;
}
//配置语义定义控制
const seeRuleEdit=ref(false);
const curRule=ref(null);
const wordRule=ref({});
const rulesetter=ref();
function openRuleEdit(row){
    curRule.value=row;
    try{
        var rule=JSON.parse(row.Wd_Parser);
        wordRule.value=rule;
    }
    catch(e){

    }
    
    seeRuleEdit.value=true;
}
function saveRule(){
    var val=rulesetter.value.getValue();

    var para=Object.assign({}, curRule.value,{
        HH_WordDefineOID:curRule.value.HH_WordDefineOID,
        Wd_Parser:JSON.stringify(val)
    })
    updateHHWordDefine([para]).then((res)=>{
        seeRuleEdit.value=false;
        ElNotification({
            title:"操作成功",
            message:res.data,
            type:"success"
        });
        
    })
}



</script>

<style scoped>
.pageplus {
    height: 80vh;
}

.linkop{
    margin-left:10px;
}

.menu-bind-body {
    height: 70vh;
    min-height: 0;
    overflow: hidden;
}
</style>
