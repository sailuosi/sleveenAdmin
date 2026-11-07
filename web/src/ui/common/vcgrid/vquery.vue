<template>
        <div>
            <div :style="{'padding-left':'5px','white-space':wraphead?'normal':'nowrap'}" class="vccap">{{cap}}</div>
            <template v-if="queryable">
                    <seditor :value="searchtxt" :type="ctype" :width="width" @keyup.enter.native="runquery"
                    :dictlist="dictlist" class="vcqyrwsedit" placeholder="搜索..."
                    @input="oninput" >
                    <el-select :value="focusCmd" @input="setcmd" placeholder="." name="queryrowhead" class="vcqyrwse" popper-class="vcqyrwpp" style="width:20px;">
                        <template #prefix>
                            <span  style="CURSOR: hand;position: relative;" class="queryEditImg">
                            <svg class="icon" style="width: 1em;height: 1em;fill: currentColor;overflow: hidden;" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="1702"><path d="M909.5 242.3V217c0-13.9-11.3-25.2-25.3-25.2H730.8v0.1h-0.3c-13.9 0-25.2 11.3-25.2 25.2s11.3 25.2 25.2 25.2h0.3v0.2H832l-86.7 101.1s-32.3-20.1-74.9-10.1c-78.5 18.4-66.2 63.8-133.1 60.7-60.3-2.8-82.8-57.7-179.9-70.6-58.5-7.8-92.7 20.1-92.7 20.1L178 242.3h456.4v-0.2c13.6-0.2 24.6-11.3 24.6-25.1s-11-24.9-24.7-25.2v-0.1H125.8c-13.9 0-25.3 11.3-25.3 25.2v25.3h25.3-25.7l323.8 353.9v202.2l161.4 100.3V596.3l323.8-353.9h-24.9 25.3z" fill="#2197EF" p-id="1703"></path><path d="M748.2 340.4s-32.3-20.1-74.9-10.1c-78.5 18.4-66.2 63.8-133.1 60.7-60.3-2.8-82.8-57.7-179.9-70.6-58.5-7.8-92.7 20.1-92.7 20.1s-3.3 3-2.9 3l177.1 201.8h117.9l188.5-204.9z" fill="#A9DEFF" p-id="1704"></path></svg>
                            </span>
                        </template>
                        
                        <el-option-group v-for="(group,g) in rendOpts" :key="g"  :label="group.label">
                            <el-option   v-for="(item,i) in group.options" :key="g*i" :label="item.label" :value="item.value">
                                <i class="fa-check fa" v-if="item.value==focusCmd" style="position: absolute;top: 0.4em;left: 2px;"></i><span v-html="item.label"></span>
                            </el-option>
                        </el-option-group>
                    </el-select>
                </seditor>                       
            </template>
        </div>
</template>
<script>

import seditor from "@f/components/common/input/seditor.vue"
export default {
    components:{seditor},
    name:"queryhead",
    inject: ["vcgrid"],
    props: {
        column: null,
        param: null,
        queryable: { type: Boolean, default: false },//是否有查询行
        cap: { type: String, default: "" },
        wraphead: { type: Boolean, default: false },//是否事件停止冒泡
        ctype: null,
        width: null,
        dictlist: null,
        //是否可以显示全部的查询选项。
        custopts:null,
        allopts:{ type: Boolean, default: false },
        
    },
    data() {
        return {
            randId:"rid"+Math.random(),
            searchtxt: "",
            focusCmd: "cmd_LIKE",
            cmd: "LIKE",
            cmdSee: false,
            cmdopts: [
                {
                    label: "",
                    options: [
                        { value: "cmd_NOTSET", label: "未设置", cmd: "" },
                    ],
                },
                {
                    label: "",
                    options: [
                        { value: "cmd_GreaterEQU", label: "大于等于(>=)", cmd: ">=" },
                        { value: "cmd_Greater", label: "大于(>)", cmd: ">" },
                        { value: "cmd_EQU", label: "等于(=)", cmd: "=" },
                        { value: "cmd_LessEQU", label: "小于等于(<=)", cmd: "<=" },
                        { value: "cmd_Less", label: "小于(<)", cmd: "<" },
                        { value: "cmd_NOTEQU", label: "不等于(<>)", cmd: "<>" },
                        { value: "cmd_LIKE", label: "包含(Like)", cmd: "LIKE" },
                        { value: "cmd_BETWEEN", label: "之间(Between)", cmd: "BETWEEN" },
                    ],
                },
                {
                    label: "",
                    options: [
                        { value: "cmd_IS", label: "IS NULL", cmd: "IS" },
                        { value: "cmd_ISNOT", label: "IS NOT NULL", cmd: "IS NOT" },
                        { value: "cmd_EQUEmpty", label: "等于空", cmd: "Empty" },
                        { value: "cmd_NOTEQUEmpty", label: "不等于空", cmd: "Not Empty" },
                    ],
                },
                {
                    label: "",
                    options: [
                        { value: "cmd_RunQuery", label: "执行查询", cmd: "" },
                        { value: "cmd_ClearThisQuery", label: "清除当前条件", cmd: "" },
                        { value: "cmd_ClearQuery", label: "清除所有条件", cmd: "" },
                    ],
                },
            ],
        };
    },
    computed:{
        myId(){
            var tar=this.column.field;
            if(!!tar){
                return tar;
            }
            return this.randId;
        },
        searchVal(){
            try{
                return this.vcgrid.para[this.column.field];
            }
            catch(e){
                return this.searchtxt;
            }
        },
        rendOpts(){
            
            if(this.custopts!=null){
                var cop=this.custopts;
                return this.cmdopts.map((group)=>{
                    var li=grop.options.fitler((opt)=>cop.indexOf(opt)!=-1);
                    return {
                        label:group.label,
                        options:li
                    };
                });
            }
            if(this.allopts){
                return this.cmdopts;
            }
            return [
                {
                    label: "",
                    options: [
                        { value: "cmd_RunQuery", label: "执行查询", cmd: "" },
                        { value: "cmd_ClearThisQuery", label: "清除当前条件", cmd: "" },
                        { value: "cmd_ClearQuery", label: "清除所有条件", cmd: "" },
                    ],
                }
            ];
        },
    },
    watch:{
        /*
        'vcgrid.para':function(val){
            if(val[this.column.field]!=this.searchtxt){
                this.searchtxt=val[this.column.field];
            }
        },
        */
    },
    methods: {
        //查询行控件的输入事件，抛出给父级处理
        oninput(val, e) {
            this.searchtxt = val;
            this.$emit("querychange", val, e);
            //console.log(this);
            this.upQuery();
        },
        resetQuery() {
            this.searchtxt = null;
            this.focusCmd = "cmd_LIKE";
            this.cmd = "LIKE";
        },
        upQuery() {
            var info = {
                value: this.searchtxt,
                cmd: this.cmd,
                column: this.column,
                you: this,
                th:this.column.th,
            };
            
            this.vcgrid.setQuery(info, this.myId);
        },
        runquery() {
            this.vcgrid.onRunQuery();
        },
        setcmd(menuId) {
            if (menuId == "cmd_NOTSET") {
                this.cmd = "";
                this.focusCmd = menuId;
                this.dataTable.SetCondiList("", "", "");
            }
            else if (menuId == "cmd_GreaterEQU") {
                this.cmd = ">=";
                this.focusCmd = menuId;
            }
            else if (menuId == "cmd_Greater") {
                this.cmd = ">";
                this.focusCmd = menuId;
            }
            else if (menuId == "cmd_EQU") {
                this.cmd = "=";
                this.focusCmd = menuId;
            }
            else if (menuId == "cmd_NOTEQU") {
                this.cmd = "<>";
                this.focusCmd = menuId;
            }
            else if (menuId == "cmd_LessEQU") {
                this.cmd = "<=";
                this.focusCmd = menuId;
            }
            else if (menuId == "cmd_Less") {
                this.cmd = "<";
                this.focusCmd = menuId;
            }
            else if (menuId == "cmd_LIKE") {
                this.cmd = "LIKE";
                this.focusCmd = menuId;
            }
            else if (menuId == "cmd_IS") {
                this.cmd = "IS";
                this.focusCmd = menuId;
            } else if (menuId == "cmd_ISNOT") {
                this.cmd = "IS NOT";
                this.focusCmd = menuId;
            } else if (menuId == "cmd_EQUEmpty") {
                this.cmd = "Empty";
                this.focusCmd = menuId;
            } else if (menuId == "cmd_NOTEQUEmpty") {
                this.cmd = "Not Empty";
                this.focusCmd = menuId;
            }
            else if (menuId == "cmd_BETWEEN") {
                this.cmd = "BETWEEN";
                this.focusCmd = menuId;
            }
            else if (menuId == "cmd_RunQuery") {
                this.vcgrid.onRunQuery();
            }
            else if (menuId == "cmd_ClearThisQuery") {
                this.resetQuery();
                this.upQuery();
            }
            else if (menuId == "cmd_ClearQuery") {
                this.vcgrid.clearQuery();
            }
        },
    }
}
//.queryEditImg background: url(../src/assets/images/wg_filter.gif);
</script>
<style >
.queryEditImg {
    cursor: pointer;
    width: 13px;
    height: 14px;
    display: block;
    
}
.vcqyrwse span.el-input__prefix{
    left:5px;
}
.vcqyrwse .el-input__inner{
    width:20px;
    padding: 0 !important;
}

</style>