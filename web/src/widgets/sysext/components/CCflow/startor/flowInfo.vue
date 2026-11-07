<template>
    <div >
        <el-tabs v-model="activeName" style="height:100%;" v-if="!empty">
            <slot name="pre"></slot>
            <el-tab-pane label="流程图" name="second">
                <track-chart v-if="trackInfo"
                            :nodes="trackInfo.WF_Node"
                            :labels="trackInfo.WF_LabNote"
                            :relations="trackInfo.WF_Direction"
                            :prev-nodes="nodeMans"/>
            </el-tab-pane>
            <el-tab-pane label="时间轴" name="third" v-if="!!chartPara.WorkID">
                <timeBase :para="chartPara" ></timeBase>
            </el-tab-pane>
            <el-tab-pane label="时间表" name="fourth" v-if="!!chartPara.WorkID">
                <timeTable :para="chartPara"></timeTable>
            </el-tab-pane>
            <slot name="after" :track="trackInfo"></slot>
        </el-tabs>
        <el-empty description="流程信息不存在，可能是流程已已删除。如果是隐患数据，也许是因为数据同步自全面风险管理系统并自动锁定。您可以咨询系统管理员以获取更多信息~" v-if="empty"></el-empty>
    </div>
</template>

<script>

//import { getMyStartlist } from "@/api/search/search.js"


import timeBase from '@f/client/views/ccform/sendbar/timeBase.vue'//时间轴
import timeTable from '@f/client/views/ccform/sendbar/timeTable.vue'
import TrackChart from "@f/client/components/TrackChart.vue";

import {Chart_Init} from "@f/client/api/form/track.js"


export default {
    name:"flowinfo",
    components: {
        timeBase,timeTable,TrackChart
    },
    props:{
        row:null,
        para:null,
        defaultTab:{ type:String,default:"second" }
    },
    emits:[],
    data(){
        return {
            activeName:this.defaultTab,
            mlist:[],
            total:0,  
            queryParams:{
                pageSize:30,
                pageNum:1,
            },
            trackInfo: null,
            nodeMans:[],
        };
    },
    computed:{
        chartPara(){
            if(this.para!=null && this.para != void 0){
                return this.para;
            }
            return {
               FK_Flow:this.row.bfFlowno,
               FK_Node:"",
               WorkID:null,
            };
        },
        empty(){
            return !this.chartPara.FK_Flow && !this.chartPara.WorkID
        },
    },
    mounted() {
        this.getlist();
    },
    watch:{
        "chartPara":function(){
            this.getlist();
        },
    },
    methods:{
        getlist(){
            Chart_Init(this.chartPara).then((res) => {
                if (res.code === 200) {
                this.trackInfo=JSON.parse(res.data);
                this.nodeMans= this.calcNodeMan();
                console.log(this.trackInfo)
                }
            })
        },
        calcNodeMan(){
            if(!this.trackInfo) return [];
            var t=this.trackInfo?.WF_GenerWorkerList;
            if(Array.isArray(t)){
                return t;
            }

            var track= this.trackInfo.Track;
            var list=[];
            for(var i=0;i<track.length;i++){
                var item=track[i];
                if(item.ActionType==1||item.ActionType==8){
                    var obj={
                        FK_Emp:item.EmpFrom,
                        FK_EmpText:item.EmpFromT,
                        FK_Node:item.NDFrom,
                        FK_NodeText:item.NDFromT,
                        IsPass:1,
                        RDT:"",
                        Sender:"",
                        SDT:"",
                        CDT:item.RDT,
                        EmpToT:item.EmpToT,
                        NDToT:item.NDToT,
                        Type:"2",//代表轨迹
                        Action:item.ActionTypeText,
                        Msg:item.Msg,
                    }
                    if(obj.Msg.indexOf("WorkCheck@"!=-1)){
                        obj.Msg=obj.Msg.replace("WorkCheck@","")
                    }
                    list.push(obj);
                }
            }
            return list;
        },
    }
}
</script>