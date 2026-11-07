<template>
    <div class="queryth">
        <div :style="{'padding-left':'5px','white-space':wraphead?'normal':'nowrap'}" class="vccap">    
            <slot>{{ caption }}</slot>
        </div>

        <div class="qhead">
            <el-input v-if="ftype=='text'" v-model="searchtxt" class="shinput" @change="onchange"
                @keydown.enter="onquery"
                @clear="onquery"
                @blur="checkSayQuery"
                clearable :placeholder="holder" />
            <div v-else-if="ftype=='number'" style="width: 100%;">
                <el-input  v-model="searchtxt" class="shinput" 
                    clearable @change="onchange" @keydown.enter="onquery"
                    :placeholder="holder" type="number" />   
    
            </div>

            <el-select v-else-if="ftype=='dict'"
                v-model="searchtxt" @change="onchange" @keydown.enter="onquery"
                :placeholder="holder" multiple collapse-tags clearable
                size="small"
                class="shinput"
            >
                <el-option v-for="item in options"
                    :key="item.value"
                    :label="item.label"
                    :value="item.value"
                ></el-option>
            </el-select>
            <el-date-picker v-else-if="ftype=='date'||ftype=='datetime'" class="shinput"
                v-model="searchtxt" @change="onchange" @keydown.enter="onquery"
                type="daterange" style="width: 100%;"
                range-separator="--"
                start-placeholder="开始"
                end-placeholder="结束"
                size="small"
                format="YYYY-MM-DD hh:mm:ss"
            />
            <el-select v-else-if="ftype=='bool'"
                v-model="searchtxt" @change="onchange" @keydown.enter="onquery"
                :placeholder="holder" multiple collapse-tags clearable
                size="small"
                class="shinput"
            >
                <el-option :label="'是'" :value="true" ></el-option>
                <el-option :label="'否'" :value="false" ></el-option>
            </el-select>
        </div>     

    </div>
</template>
<script>
    

    export default {
        name:"queryth",
        props: {
            column: null,
            param: null,
            caption: { type: String, default: "" },
            wraphead: { type: Boolean, default: false },//是否事件停止冒泡
            ctype: null,
            width: null,
            dict: null,
            dictlist:null,
            //是否可以显示全部的查询选项。
            custopts:null,
            allopts:{ type: Boolean, default: false },
            holder: { type: String, default: "筛选..." },
        },
        data() {
            return {
                searchtxt: "",
                //是否存在了变化，没有执行查询
                changeNotSay:false,
                focusCmd: "cmd_LIKE",
                cmd: "like",
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
            options(){
                return this.dictlist;
            },
            ftype(){
                if(!!this.ctype){
                    return this.ctype;
                }
                return "text";
            },
            searchVal(){
                try{
                    return this.vclist.para[this.column.field];
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
        mounted() { 
            if(this.ftype=="dict"){
                this.cmd="in";
            }
        },
        watch:{
            /*
            'vclist.para':function(val){
                if(val[this.column.field]!=this.searchtxt){
                    this.searchtxt=val[this.column.field];
                }
            },
            */
        },
        methods: {
            checkSayQuery(){
                if(this.changeNotSay){
                    this.onquery();
                }
            },
            onchange() {
                var para={
                    value:this.searchtxt,
                    op:this.cmd,
                    field:this.column.field
                };
                if(para.value==''){
                    para.value=null;
                }
                console.log("onchange",para);
                this.$emit('queryinput', para);
                var logic="=";
                if(this.ctype=="DatePicker"){
                    logic="between";
                }
                else if(this.ctype=="Input"){
                    logic="contain";
                }
                this.$emit('logicChange',logic);
                //选择型的立即执行
                if(this.ctype=="Select"|| this.ctype=="Checkbox"|| this.ctype=="DatePicker"){
                    this.onquery();
                }
                else{
                    this.changeNotSay=true;
                }
            },
            onquery() {
                setTimeout(() => {
                    this.changeNotSay=false;
                    this.$emit('query');
                },100);
                //this.$emit('query');
            },
            resetQuery() {
                this.searchtxt = null;
                this.focusCmd = "cmd_LIKE";
                this.cmd = "like";
            },
            upQuery() {
                var info = {
                    value: this.searchtxt,
                    cmd: this.cmd,
                    column: this.column,
                    you: this,
                };
                this.vclist.setQuery(info, this._uid);
            },
            runquery() {
                this.vclist.onRunQuery();
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
                    this.cmd = "like";
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
                    this.vclist.onRunQuery();
                }
                else if (menuId == "cmd_ClearThisQuery") {
                    this.resetQuery();
                    this.upQuery();
                }
                else if (menuId == "cmd_ClearQuery") {
                    this.vclist.clearQuery();
                }
            },
        }
    }
    //.queryEditImg background: url(../src/assets/images/wg_filter.gif);
    </script>
    <style scoped>
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
    .vcthead>.cell{
        display:flex;
        align-items:center;
    }
    .shinput{
        width: 100%;
        height:26px;
    }
    .shinput:deep(>div){
      box-shadow: none;
    }
    .shinput{
      box-shadow: none;
    }
    .qhead:deep(>div){
      box-shadow: none; 
    }
    </style>