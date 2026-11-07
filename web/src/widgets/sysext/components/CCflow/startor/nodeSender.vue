<template>
    <div >
        <OpenDialog v-if="!loading" :judgeOperation="opration" style="min-height:400px;width:100%;" :para="para"
            :from="currNode" :to="sendTo"
            :mainData="initPara.mainData"
            :flow="initPara.flow" :work="initPara.work"
        ></OpenDialog>

    </div>
</template>

<script>
/**
        <template  v-if="mlist.mode=='anyselect'">
            <slot name="anyselect">
                <selectman @select="addUser" :multi="isMulti"></selectman>
                <div>
                    <el-tag v-for="(man,m) in checkedUser" closable @close="removeman(man,m)" style="margin-right:6px;">{{man.Name}}</el-tag>
                </div>
            </slot>
        </template>
        <template  v-else-if="mlist.mode=='checkbox'">
            <slot name="checkbox">
                <el-collapse v-model="expanded" >
                    <el-collapse-item title="一致性 Consistency" name="1">
                        <template #title>
                            <div>
                                已选：<el-tag v-for="(man,m) in checkedUser" closable @close="removeman(man,m)" style="margin-right:6px;">{{man.Name}}</el-tag>
                            </div>                        
                        </template>
                        <checkman :depts="mlist.Depts" :emps="mlist.Emps" :value="checkedIds" @input="setUser" :multi="isMulti"></checkman>
                    </el-collapse-item>
                </el-collapse>
            </slot>
        </template>
        <template v-else>
            <slot name="empty"></slot>
        </template>
 */

import { getNodeSelector,initParaForStartFlow } from "@f/client/api/send/start.js"

import selectman from "@f/client/views/ccform/sendbar/workopt/ui/selectman.vue"
import checkman from "@f/client/views/ccform/sendbar/workopt/ui/checkman.vue"

import OpenDialog from "@f/client/views/ccform/sendbar/OpenDialog.vue";



export default {
    name: 'NodeSelector',
    components: {
        selectman,checkman,OpenDialog
    },
   props:{
        nodeId:null,
        value:null,
        to:null,
        bizflow:null,
    },
    emits:[],
    data(){
        return {
            expanded:"1",
            mlist:{
                mode:"",//为空时，表示不需要选择
            },
            loading:true,  
            queryParams:{
                pageSize:30,
                pageNum:1,
            },
            checkedUser:[],
            selectVal:"",
            work:{},
            initPara:{},
        };
    },
    computed:{
        isMulti(){
            return this.mlist.IsSimpleSelector==0;
        },
        checkedIds(){
            return this.checkedUser.map((man)=>man.No);
        },
        flowNo(){
            return this.bizflow.flowNo;
        },
        flow(){
            return {};
        },
        mainData(){
            return {};
        },
        opration(){
            if(this.mlist.DeliveryWay=="44"){
                return "BySelfUrl";
            }
            return "sendAccepter";
        },
        para(){
            return {
                FID:0,
                FK_Flow:this.bizflow.flowNo,
                FK_Node:this.nodeId,
                NodeID:this.nodeId,
                disabled:false,
                ToNode:this.to.NodeID,
            }
        },
        sendTo(){
            return {
                No:this.nodeId,
                Name:this.to.Name,
            };
        },
    },
    mounted() {
        this.getlist();
        this.initStart();
    },
    watch:{
        "nodeId":function(){
            this.getlist();
        },
        "flowNo":function(){
            this.initStart();
        },
    },
    methods:{
        addUser(mans){
            if(!Array.isArray(this.checkedUser)){
                this.checkedUser=[];
            }
            if(this.isMulti==false){
                this.checkedUser=[mans];
                this.fireInput();
                return ;
            }

            if(this.checkedUser.length==0){
                this.checkedUser=mans;
                this.fireInput();
                return ;
            }
            else{
                var addedId=this.checkedIds;
                mans.forEach((man)=>{
                    if(addedId.indexOf(man.No)==-1){
                        this.checkedUser.push(man);
                    }
                })
                this.fireInput();
            }
            
        },
        fireInput(){

            var tar=this.checkedUser.map((man)=>{
                man.userId=man.No;
                man.nickName=man.Name;
                return man;
            })
            this.$emit("input",tar);
        },
        setUser(ids,mans){
            this.checkedUser=mans;
            this.fireInput();
            return ;
        },
        removeman(man,m){
            this.checkedUser.splice(m,1);
            this.fireInput();
        },
        getlist(){
            if(this.nodeId == void 0|| this.nodeId==null){
                this.mlist={mode:""};
                return;
            }
            var para={
                NodeID:this.nodeId
            }
            getNodeSelector(para).then((res)=>{
                this.mlist=res.data;
            })
        },
        initStart(){
            this.loading=true;
            initParaForStartFlow({No:this.flowNo}).then((res)=>{
                this.initPara=res.data;
                this.loading=false;
            });
        },
    }
};
</script>

<style scoped>

</style>