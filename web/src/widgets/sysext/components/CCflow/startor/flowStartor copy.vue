<template>
<div v-loading="loading">
    <div v-if="mlist.length==0">
        <el-empty description="没有可启动的流程">
            <el-button icon="el-icon-refresh"  @click="onrefresh">刷新</el-button>  
        </el-empty>
    </div>
    <el-form  ref="form" :model="form" label-width="120px" v-else-if="sended==false">
        <el-form-item label="要启动的流程">
            <el-radio-group v-model="focusFlow">
                <el-radio :label="flow.flowNo" :value="flow.flowNo" v-for="(flow,f) in mlist">
                    {{flow.flowNo}}.{{flow.flowName}}
                    <el-button icon="el-icon-search" circle @click="openChart(flow)"></el-button>
                </el-radio>
            </el-radio-group>
        </el-form-item>
        <el-form-item label="启动的节点" v-if="showNodes">
            <el-select v-model="toNode" placeholder="请选择启动的节点" :disabled="disabledNode">
                <template v-for="item in nodes">
                    <el-option v-if="chosableNodes.length==0||chosableNodes.indexOf(item.NodeID) ==-1" :key="item.NodeID" :label="item.Name" :value="item.NodeID"></el-option>
                </template>
                
            </el-select>
        </el-form-item>
        <el-form-item label="选择办理人" v-if="focusConfig.toMan !='0'">
            <nodeSelector :nodeId="toNode"  v-model="checkedUser" v-if="focusConfig.toMan =='1'"
                :to="toNodeInfo"
                :bizflow="focusConfig"
            ></nodeSelector>
            <div v-if="focusConfig.toMan =='2'">
                <el-tag v-for="(man,m) in checkedUser" closable @close="removeman(man,m)" style="margin-right:6px;" :key="m">{{man.nickName}}</el-tag>
                <el-button  icon="el-icon-plus" @click="selectUserOpen()"  style="padding:7px 14px;"></el-button>            
            </div>
        </el-form-item>        
        <el-form-item label="是否立即启动" v-if="focusConfig.bf_draftable =='1'">
            <el-switch v-model="form.startNow"></el-switch>
        </el-form-item>
        <el-form-item label="待办名称" v-if="custTitle">
            <el-input v-model="form.name"></el-input>
        </el-form-item>
        <el-form-item>
            <el-button type="warning" round @click="doStartFlow" :loading="starting">立即启动</el-button>
            <el-button @click="onclose" :loading="starting">取消</el-button>
        </el-form-item>
    </el-form>
    <div class="flowlist" v-if="sended">
        <div v-for="(re,r) in sendRes">
            <h3>流程{{focusFlow}}{{focusConfig.bfFlowname}}已启动：</h3>
            <div>
                <sendResult :msg="re.msg"></sendResult>
                <span v-if="jumptSend>=0">{{jumptSend}}秒后自动跳转页面</span>
            </div>
        </div>

    </div>
    <div class="dbworkor">
    </div>
    <div style="text-align:center;">
        
    </div>
    <el-dialog title="流程查看" :visible="seeChart" width="70vw" append-to-body @close="seeChart=false">
        <flowInfo :para="chartPara" @cancel="seeChart=false"></flowInfo>
    </el-dialog>
    <el-dialog title="选人" :visible="seebpoman" width="90vw" append-to-body @close="seebpoman=false" destroy-on-close>
        <iframe :src="selectmanUrl" height="100%" width="100%" style="height:70vh;border: none;"></iframe>
    </el-dialog>
</div>
</template>

<script>

import { startToNode,getFlowNodes,getNodeSelector } from "@f/client/api/send/start.js"
//import { listBizflow, saveGeneflow } from "@/api/bpo/ccflow.js";

import sendStart from "@f/client/views/ccform/sendbar/send/sendStart.vue"
//import nodeSelector from "./nodeSelector.vue"
import sendResult from "@f/client/views/ccform/sendbar/send/sendResult.vue"
import flowInfo from "./flowInfo.vue"

export default {
    name: 'flowstartor',
    components:{
        sendStart,sendResult,flowInfo
    },
   props:{
        flows:null,
        biz:{type:String,default:""},//业务标识
        id:{type:Number,default:0},
        pk:null,
        bizOrg:{type:String,default:""},
        bizOrgName:{type:String,default:""},
        bizPara:{type:String,default:""},
        bizTitle:{type:String,default:""},
        selectNode:{type:Boolean,default:false},//是否选择节点
        draftable:{type:Boolean,default:false},//是否可以存为草稿
        custTitle:{type:Boolean,default:false},
    },
    emits:["cancel","sended","saved","refresh"],
        data() {

            return {
            loading:true,
            seeChart:false,
            starting:false,//是否正在启动流程
            mlist:[],
            nodes:[],
            total:0,
            queryParams:{
                pageSize:30,
                pageNum:1,
            },
            focusFlow:"",
            toNode:"",
            checkedUser:[],
            form:{
                startNow:true,
            },
            focusBFId:0,
            chosableNodes:[],
            focusConfig:{},
            //启动后的数据
            sended:false,//已启动驰骋流程
            saved:false,
            sendRes:[],//流程启动的结果
            saveRes:"",//
            chartPara:{},
            jumptSend:-1,//自动跳转的时间提示
            selectmanUrl:"",
            seebpoman:false,
        };
    },
    computed:{
        showNodes(){
            if(this.selectNode || this.focusConfig.nodeSelect==1){
                return true;
            }
            if(this.toNode == ""|| this.toNode== null || this.toNode == void 0){
                return true;
            }
            return false;
        },
        disabledNode(){
            if(this.focusConfig.bf_nodeCheck=='2'){
                return true;
            }
            return false;
        },
        toNodeInfo(){
            for(var i=0;i<this.nodes.length;i++){
                var nod=this.nodes[i];
                if(nod.NodeID==this.toNode){
                    return nod;
                }
            }
            return {};
        },
    },
    mounted() {
        this.getlist();
    },
    watch:{
        'biz':function(){
            this.getlist();
        },
        "focusFlow":function(val){
            this.getNodes(val);
        },
        "pk":function(val){
            this.sended=false;
            this.saved=false;
            this.starting=false;
        },
    },
    methods:{
        openChart(flow){
            this.chartPara={
                FK_Flow:flow.bf_flowNo,
                FK_Node:"",
                WorkID:null,
            };
    
            this.seeChart=true;
        },
        onclose(){
            this.$emit("cancel")
        },
        onrefresh(){
            this.$emit("refresh")
        },
        getlist(){
            if(Array.isArray(this.flows)){
                this.mlist=this.flows;
                this.loading=false;
                return ;
            }
            this.loading=true;
            this.queryParams.bfCode=this.biz;
            var vm=this;
            listBizflow(this.biz).then((res)=>{
                vm.loading = false;
                if (res.status == "error") {
                    swal({ text: res.data, icon: "error", button: "我知道了", timer: 8000, });
                    return;
                }

                vm.mlist=res.data;
                //this.total=res.total;
                if(vm.focusFlow=="" && res.data.length==1){
                    vm.focusFlow=res.data[0].bf_flowNo;
                }
            })

        },
        getNodes(fowNo){
            getFlowNodes({FK_Flow:fowNo}).then((res)=>{
                this.nodes=res.data;
            });
            this.setFocusConfig();

        },
        setFocusConfig(){
            for(var i=0;i<this.mlist.length;i++){
                var row=this.mlist[i];
                if(row.flowNo==this.focusFlow){
                    this.focusBFId= row.C_bizflowOID;
                    this.focusConfig=row;
                    if(/^\d+$/.test (row.nodeIDs)){
                        this.toNode=row.nodeIDs*1;
                    }
                    else if(/,/.test(row.nodeIDs)){
                        this.chosableNodes=row.nodeIDs.split(",");
                    }
                }
            }
        },
        doStartFlow(){
            var vm=this;
            this.jumptSend=-1;
            this.starting=false;
            this.$confirm('确定要启动流程吗, 是否继续?', '提示', {confirmButtonText: '确定',cancelButtonText: '取消',type: 'warning'})
            .then(() => {
                
                if(this.pk==void 0 || this.pk==null){
                    this.$message({ type: 'success', message: "业务信息为空，无法启动流程~"});
                    return;
                }
                this.starting=true;
                var addedId=this.checkedUser.map((man)=>man.userId);
                var para=[{
                    id:this.id,
                    pk:this.pk,
                    biz:this.biz,
                    bizOrg:this.bizOrg,
                    bizOrgName:this.bizOrgName,
                    bizPara:this.bizPara,
                    bizTitle:this.bizTitle,
                    channel:this.$store.state.ccflow.user.channelId,
                    flowNo:this.focusFlow,
                    nodeId:this.toNode,
                    emps:addedId.join(",")
                }];
                startToNode(para).then((res)=>{
                    console.log(res);
                    var rlist=res.data;
                    this.sendRes=rlist;
                    this.sended=true;
                    this.$emit("sended",res);

                    vm.saved=true;
                    vm.$emit("saved",res);
                    vm.starting=false;   
                    vm.saveRes=sres.msg;
                    vm.doFlowNextAct(res,sres)
                    vm.$message({ type: 'success', message: "流程启动成功！"});

                })
                
            })
        },
        doFlowNextAct(flowRes,saveRes){
            //检查配置并执行动作
            var li=flowRes.data[0];
            var vm=this;
            var act=this.focusConfig.bfNextact;
            var note=this.focusConfig.bfNextnote;
            var myid= this.$store.state.user.name
            if(li.TodoEmps.indexOf(myid)==-1){
                //当前人员不是待办办理人。停止跳转动作
                return ;
            }
            if(act=="2"){
                vm.toDaibanPage(flowRes);
                return ;
            }
            if(/^11__[0-9]+/.test(act)){
                //如果是延时跳转
                var val=act.substring(4)*1;
                this.jumptSend=val;
                
                setTimeout(function(){
                    vm.toDaibanPage(flowRes);
                },val)
                var myInterval = setInterval(function(){
                    vm.jumptSend--;
                    if(vm.jumptSend<0){
                        //清除计时器
                        clearInterval(myInterval)
                    }
                },1)
            }
        },
        toDaibanPage(flowRes){
            var li=flowRes.data[0];
            //myflow?IsRead=1&FK_Flow=018&FK_Node=1802&FID=0&WorkID=135&PWorkID=135
            var para={
                FID:0,
                fkBizid:this.focusBFId,
                WorkID:li.WorkID,
                PWorkID:0,
                FK_Node:li.nodeId,
                FK_Flow:this.focusFlow,
                FromPage:"ccStart",
            };
            // this.$router.push({
            //     path:"/cchomewk/myflow",
            //     query:para
            // })
            let routeData = this.$router.resolve({
                path:"/cchomewk/myflow",
                query:para
            });
            window.open(routeData.href,  '待办办理');
        },
        selectUserOpen(index) {
            var vm=this;
            //this.$refs.selectUser.visible = true;
            window.setseleeeman=function(vc){
                var bc=vc.dataTable;
                var ck= vc.getCheckOID();
                var addedId=vm.checkedUser.map((man)=>man.userId);
                ck.forEach(function(li,i){
                     rowIndex = bc.dataStore.findLocateOID("", li); 
                     if(rowIndex != -1){
                         bc.setIndexNoEvent(rowIndex );
                         var id=bc.getFieldValue("Varchar8");
                         if(addedId.indexOf(id)==-1){
                            vm.checkedUser.push({
                                nickName:bc.getFieldValue("PersonName"),
                                userId:id,
                                oid:bc.getOID(),
                            });
                        }  
                     }
              
                });
                vm.seebpoman=false;
                window.setseleeeman=null;
            }
            if(window.localResourcePath != void 0 ){
                this.selectmanUrl=localResourcePath+"sln_commonWork/BusinessUnit/BPO_CON_Select_CheckX.html?callback=setseleeeman"
            }
            else{
                this.selectmanUrl=UCMLLocalResourcePath+"sln_commonWork/BusinessUnit/BPO_CON_Select_CheckX.html?callback=setseleeeman"
            }
            
            this.seebpoman=true;
            // var w = new UCML.OpenShowWindow({ frameMode: "frame",modal:true, maximizable: true,maximized:true,
            // collapsible: true,closable:true,collapsible:false, URL: "sln_commonWork/BusinessUnit/BPO_CON_Select_CheckX.html?callback=setseleeeman", scroll: "yes", draggable: true,
            // resizable: true, width: 800, height: 600, title: "计划编辑"});
            // w.on("close",function(){
            //     //BC_P_PlanBase.Refresh();
            //     window.setseleeeman=null;
            // })
            // w.open();
        },
        addUser(ids,mans){
            if(this.checkedUser.length==0){
                this.checkedUser=mans;
                return ;
            }
            else{
                var addedId=this.checkedUser.map((man)=>man.userId);
                mans.forEach((man)=>{
                    if(addedId.indexOf(man.userId)==-1){
                        this.checkedUser.push(man);
                    }
                })
            }
            
        },
        removeman(man,m){
            this.checkedUser.splice(m,1);
        },
    }
};
</script>

<style scoped>

</style>