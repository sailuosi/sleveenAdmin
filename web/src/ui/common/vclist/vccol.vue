<template>
<template v-if="vface=='card'">
    <queryput :column="colinfo" :cap="cap" :queryable="queryable" :ctype="ctype" :codeTable="dict" @queryinput="onquerychange" @query="onrunquery" :dictlist="rendDictOpts"
    ></queryput>
</template>
<template v-else>
    <el-table-column :label="cap" :prop="field" :width="myWidth" :min-width="minWidth" :fixed="fixed" :align="myAlign" :header-align="headerAlign" :class-name="colcssname" :sortable="sortMode" :sort-by="sortByField" :sort-orders="sortOrders" :label-class-name="field+' vcthead'" :resizable="colResizable">
        <template #header="p">
            <queryhead :param="p" :column="colinfo" :cap="cap" :wraphead="vclist.wraphead" :queryable="queryable" :ctype="ctype" :codeTable="dict" @queryinput="onquerychange" @query="onrunquery" :dictlist="rendDictOpts">
                <span class="vccol-cap-text">
                    <slot name="header" :column="p.header">{{ cap }}</slot>
                </span>
                <span v-if="p.column && p.column.order" class="vccol-sort-icon" aria-hidden="true">
                    <i :class="['vccol-sort-caret', p.column.order]"></i>
                </span>
            </queryhead>
        </template>
        <template #default="p">
            <slot v-bind="p">
                <template v-if="!cellEditable(p)">
                    <div class="vccol-display-cell" @click="onvalueck(p)">
                        <slot name="shower" v-bind="p">
                            <vccell style="min-height:1.5em;" :class="{cvalwrap:true,bclinkcell:link}"
                                :type="showType" :ctype="ctype" :value="getVal(p)" :style="getCellStyle" :maxHeight="maxHeight" :dictlist="rendDictOpts"
                                :maxLine="myMaxLine"
                                :baseVal="p.row[field]"
                                :plainDict="plainDict"
                            >
                            </vccell>
                        </slot>
                    </div>
                </template>
                <template v-else >
                    <slot name="editer" v-bind="p">
                        <seditor :model-value="p.row[field]" :type="ctype" :width="width" :autosize="autosize"
                        ref="editor" :dictlist="rendDictOpts" :lazy="lazyInput"
                        :max="max" :min="min" :step="step" :precision="precision" :strictly="strictly" :controlsPosition="controlsPosition"
                        :multiple="multiple" :seprator="seprator"
                        @update:model-value="onedit(p,$event)" @blur="closeEdit(p)" @change="onchange(p)">
                            <template #editor="p"><slot v-bind="p" name="edit"></slot></template>
                        </seditor>
                    </slot>
                </template>
            </slot>
        </template>
    </el-table-column>    
</template>

</template>
<script>
// v-show="editable && getCellEdState(p)"
//import { defineComponent } from '@vue/composition-api'
import {formatDate} from "./tool.js"
import queryhead from "./queryhead.vue"
import seditor from "../input/seditor.vue"
import vccell from "./vccell.vue"
import queryput from "./queryput.vue"

import { loadCode } from "@/utils/codevalue.js"

export default {
    name:"vccol",
    components:{queryhead,seditor,vccell,queryput},
    props: {
        //BC上的列信息对象。
        column: null,
        //字段名，使用本属性，必须同时传入BC对象。
        field: null,
        /** 查询条件使用的字段名；为空时沿用展示字段 */
        queryField: { type: String, default: "" },
        align: { type: String, default: "" },
        headerAlign:null,
        caption: { type: String, default: "" },
        width: { type:[String,Number] },
        minWidth:null,
        maxHeight:null,
        //查询行控件类型
        type: { type: String, default: "" },
        //展示控件类型 tip,
        showType:{ type:String},
        //是否显示查询行
        query: { type: Boolean, default: null },
        //是否可以单元格编辑。
        editable: { type: Boolean, default: false },
        link: { type: Boolean, default: false },
        dict:{type:String,default:""},
        dictlist:null,
        /** 字典列仅显示纯文字，不使用标签样式 */
        plainDict:{ type:Boolean, default:false },
        //当启用编辑时，必须传入BC
        //BC: null,
        className: null,
        //编辑的触发，主要包含单击、双击。
        editIndex:{type:Number,default:-1},//编辑器默认的打开行号。
        editTrigger: { type: String, default: "click" },
        sortable: { type: [String, Boolean], default: '' },
        sortField: { type: String, default: "" },
        getCellStyle: { type: Function, default: function () { return {}; } },//设置单元格的样式。
        rendCell: { type: Function },
        datefmt: { type: String, default: "" },
        autofocus: { type: Boolean, default: true },
        autosize: { type: [Boolean,Object], default: false },
        fixed:null,
        //单元格的编辑是否是懒加载模式，否则的话，则该列的值一直显示为编辑控件。
        lazyRend:{type:Boolean,default:false},
        //单元格编辑是否懒反馈模式，该模式下优先自行保管编辑值。
        lazyInput:{type:Boolean,default:false},
        editCheck:{type:Function},//单元格是否可以编辑的方法。
        autohide:{type:Boolean,default:false},

        //编辑器属性
        //计数器
                //fullnum计数器特有属性
        max:{type:Number},
        min:{type:Number},
        step:{type:Number},
        precision:{type:Number},
        controlsPosition:null,
        strictly:{type:Boolean,default:false},
        /** 字典多选 */
        multiple: { type: Boolean, default: false },
        seprator: { type: String, default: ";" },

        //样式
        maxLine:null,
        /** 是否允许拖拽调整列宽（需 el-table border=true） */
        resizable: { type: Boolean, default: true },
    },
    inject: ["vclist"],
    computed: {
        vc() {
            return this.vclist;
        },
        vface(){
            if(!!this.vc.face){
                return this.vc.face;
            }
            return "table";
        },
        myAlign(){
            if(!!this.align){
                return this.align;
            }
            if(!!this.vc.align){
                return this.vc.align;
            }
            return "center"
        },
        myMaxLine(){
            if(!!this.maxLine){
                return this.maxLine;
            }
            if(!!this.vc.maxLine){
                return this.vc.maxLine;
            }
            return null;
        },
        myWidth(){
            try{
                if(!!this.vc.widthKeep){
                    var tar=this.vc.vcWidths[this.field].vwWidth;
                    if(tar!=void 0 && tar !=null && tar !=''){
                        return tar+"px";
                    }
                }
            }
            catch(e){

            }

            return this.width;
        },
        colcssname(){
            var baseclass='nowrapcell';
            if(this.vc.wrapcell){
                baseclass="wrapcell";
            }
            if(!!this.className){
                baseclass+=this.className;
            }
            return baseclass;
        },
        cap() {
            if (this.caption != "") return this.caption;
            return this.field;
        },
        ctype() {
            if (this.type != "") return this.type;
            if(Array.isArray(this.dictlist)){
                return "dict";
            }
            if (this.dict!='') {
                return "codetable";
            }
            this.type;
            //return "text";
        },
        queryable() {
            if (!this.vclist) {
                if (this.query == null) return false;
                return this.query;
            }
            //默认时，都是false，
            if (this.query == null) {
                return this.vclist.query;
            }
            return this.query;
        },
        canSort() {
            return this.sortMode !== false;
        },
        sortByField(){
            if(!!this.sortField){
                return this.sortField;
            }
            return this.field;
        },
        sortOrders() {
            return ["ascending", "descending", null];
        },
        sortMode() {
            if(!this.field || this.field === "op"){
                return false;
            }
            var mode = this.sortable;
            if (mode === '') {
                mode = this.vclist ? this.vclist.sortable : false;
            }
            if(mode === false || mode === "false" || mode === "none"){
                return false;
            }
            if(mode === true || mode === "true" || mode === "custom"){
                return "custom";
            }
            return mode || false;
        },
        colinfo(){
            //整合列的信息；
            return {
                field:this.queryField || this.field,
                caption:this.cap,
                ctype:this.ctype,
                sortable:this.canSort,
                queryable:this.queryable,
            };
        },
        rendDictOpts(){
            if(Array.isArray(this.dictlist) && this.dictlist.length>0){
                return this.dictlist;
            }
            return this.mydict
        },
        colResizable(){
            if(this.fixed === "left" && this.field === "op"){
                return false;
            }
            return this.resizable !== false;
        },
    },
    data() {
        return {

            inedit: false,
            eidtboss: {},
            activeRowIndex:this.editIndex,
            mydict:[],
        };
    },
    watch:{
        editIndex:function(){
            this.activeRowIndex=this.editIndex;
        },
        rendDictOpts(){
            if(!!this.vclist){
                this.vclist.registerField({
                    field:this.field,
                    caption:this.cap,
                    ctype:this.ctype,
                    sortable:this.canSort,
                    queryable:this.queryable,
                    dict:this.dict,
                    dictlist:this.rendDictOpts,
                });
            }
        },
    },
    mounted(){
        if(!!this.vclist){
            this.vclist.registerField({
                field:this.field,
                caption:this.cap,
                ctype:this.ctype,
                sortable:this.canSort,
                queryable:this.queryable,
                dict:this.dict,
                dictlist:this.rendDictOpts,
            });
        }
        this.loadDict();
    },
    methods: {
        onquerychange(e){
            console.log("onquerychange",this.field,e);
            var field=this.queryField || this.field;
            e.field=field;
            this.vclist.setQuery(field,e);
            this.vclist.$emit("querychange",e);
            //queryConditions 
        },
        onrunquery(){
            this.vclist.onRunQuery();
        },
        loadDict(){
            if(!!this.dict){
                loadCode(this.dict).then((res)=>{
                    this.mydict=res;
                })
            }
        },
        getVal(p) {
            if (typeof this.rendCell == "function") {
                var customVal = this.rendCell(p);
                return this.normalizeDisplayVal(customVal);
            }
            var val = p.row[this.field];
            // 同时检查 dict 和 dictlist/ctype，支持通过 dictlist 传递的代码表
            if (this.dict!="" || (this.ctype=="dict" && Array.isArray(this.rendDictOpts) && this.rendDictOpts.length>0)) {
                val = this.GetCodeCaption(val);
            }
            if (val == "1900-01-01 00:00:00") { return ""; }
            if (this.ctype == "datetime") {
                val = formatDate(val, this.datefmt == "" ? "YYYY-MM-DD hh:mm:ss" : this.datefmt);
            }
            else if (this.ctype == "date") {
                val = formatDate(val, this.datefmt == "" ? "YYYY-MM-DD" : this.datefmt);
            }
            else if (this.ctype == "fullnum") {
                if (this.precision) {
                    val = Number(val).toFixed(this.precision);
                }
            }
            if (val === null || val === undefined) {
                return "";
            }
            return this.normalizeDisplayVal(val);
        },
        normalizeDisplayVal(v){
            if(v===null || v===undefined){
                return "";
            }
            if(Array.isArray(v)){
                return v
                    .map((item)=>this.normalizeDisplayVal(item))
                    .filter((item)=>item!=="" && item!==null && item!==undefined)
                    .join("、");
            }
            if(typeof v!=="string"){
                return v;
            }
            var raw = v.trim();
            if(raw===""){
                return "";
            }

            // 处理被 JSON 序列化后的文本/标签数组，例如 "第2期"、["H","F"]
            if((raw.startsWith("[") && raw.endsWith("]")) || (raw.startsWith("\"") && raw.endsWith("\""))){
                try{
                    var parsed = JSON.parse(raw);
                    if(Array.isArray(parsed)){
                        return parsed
                            .map((item)=>this.normalizeDisplayVal(item))
                            .filter((item)=>item!=="" && item!==null && item!==undefined)
                            .join("、");
                    }
                    if(parsed===null || parsed===undefined){
                        return "";
                    }
                    if(typeof parsed==="string"){
                        return parsed;
                    }
                    return String(parsed);
                }catch(e){
                    // 非标准 JSON，继续按普通字符串处理
                }
            }

            // 处理包裹引号的普通字符串
            if((raw.startsWith("\"") && raw.endsWith("\"")) || (raw.startsWith("'") && raw.endsWith("'"))){
                return raw.substring(1,raw.length-1);
            }
            return raw;
        },
        GetCodeCaption(v){
            if(v===null || v===undefined || v===""){
                return "";
            }
            if(typeof v === "boolean"){
                v = v ? "1" : "0";
            }
            if(Array.isArray(this.rendDictOpts) && this.rendDictOpts.length>0){
                var valKeys = this.normalizeCodeKeys(v);
                for(var i=0;i<this.rendDictOpts.length;i++){
                    var op=this.rendDictOpts[i];
                    var opKeys = []
                        .concat(this.normalizeCodeKeys(op?.value))
                        .concat(this.normalizeCodeKeys(op?.key))
                        .concat(this.normalizeCodeKeys(op?.Code))
                        .concat(this.normalizeCodeKeys(op?.code))
                        .concat(this.normalizeCodeKeys(op?.id))
                        .concat(this.normalizeCodeKeys(op?.ID))
                        .concat(this.normalizeCodeKeys(op?.Value));
                    for(var j=0;j<valKeys.length;j++){
                        if(opKeys.indexOf(valKeys[j])>=0){
                            return op.label || op.text || op.Name || op.name || op.Caption || op.caption || op.Title || op.title || v;
                        }
                    }
                }
            }
            return v;
        },
        normalizeCodeKeys(v){
            if(v===null || v===undefined){
                return [];
            }
            var raw = String(v).trim();
            if(raw===""){
                return [];
            }
            var res = new Set();
            res.add(raw);
            var num = Number(raw);
            if(!Number.isNaN(num)){
                res.add(String(num));
                if(Number.isInteger(num)){
                    res.add(String(parseInt(raw,10)));
                }
            }
            return Array.from(res);
        },
        cellEditable(p){
            if(this.vclist.editable==false){
                return false;
            }
            if (this.editable == false) return false;
            var res=true;
            if(typeof this.editCheck=="function"){
                res=this.editCheck(p);
                if(res==false) return res;
            }
            if(this.vclist.accordion){
                var oid = this.getCellKey(p);
                return this.vclist.activeCell == oid;
            }
            if(this.lazyRend){
                return this.activeRowIndex==p.$index;
            }
            return res;
        },
        getCellKey(p) {
            //console.log(p);
            //return p.row[this.mybc.PrimaryKey];
            return p.column.id + p.$index;
        },
        getCellEdState(p) {
            if (this.editable == false) return false;
            if(!this.lazyRend) return true;
            var oid = this.getCellKey(p);
            return this.vclist.activeCell == oid;
        },
        closeEdit(p) {
            this.vclist.activeCell=null;
            if(this.lazyRend && this.autohide){
                this.activeRowIndex=-1;
            }
            return;
        },
        onvalueck(p) {
            this.$emit("click", p);
            if (this.editable == false) return false;
            this.activeRowIndex=p.$index;
            if(this.vclist.accordion){
                var oid = this.getCellKey(p);
                this.vclist.activeCell=oid;
            }

            if (this.autofocus && this.$refs.editor!=void 0) {
                this.$nextTick(() => {
                    if(!!this.$refs.editor) this.$refs.editor.focus();
                });
            }

        },
        //单元格编辑输入值时。
        onedit(p, val, e) {
            if (val == null) val = "";
            //对于数值类型，确保转换为数字
            if(this.ctype == "fullnum" || this.ctype == "number"){
                if(val === "" || val === null || val === undefined){
                    val = 0;
                } else {
                    val = Number(val);
                    if(isNaN(val)) val = 0;
                }
            }
            var colname = this.field;
            //直接更新 row 对象，确保 Vue 响应式系统能检测到变化
            p.row[colname] = val;
            //确保添加到变更列表中
            this.vclist.addChange(p.row);
        },
        onchange(p, val, e) {
            this.vclist.addChanged(p.row);
            this.vclist.$emit("cell-changed", {
                row: p.row,
                field: this.field,
                value: p.row[this.field],
                index: p.$index,
            });
        },
    },
}
</script>
<style scoped>
.vccol-display-cell {
    min-height: 1.5em;
    width: 100%;
}
.vccol-cap-text {
    min-width: 0;
    overflow: hidden;
    text-overflow: ellipsis;
}
.vccol-sort-icon {
    flex: 0 0 auto;
    width: 10px;
    height: 12px;
    margin-left: 4px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
}
.vccol-sort-caret {
    width: 0;
    height: 0;
    border-left: 4px solid transparent;
    border-right: 4px solid transparent;
}
.vccol-sort-caret.ascending {
    border-bottom: 5px solid #409eff;
}
.vccol-sort-caret.descending {
    border-top: 5px solid #409eff;
}
</style>
