<template>
<el-table-column :label="cap" :prop="field" :width="myWidth" :min-width="minWidth" :fixed="fixed" :align="align" :header-align="headerAlign" :class-name="colcssname" :sortable="canSort" :sort-by="field" :label-class-name="field + ' vcthead'">
    <template #header="p">
        <slot name="header" :column="p.header">
            <vquery :param="p" :column="colinfo" :cap="cap" :wraphead="vcgrid.wraphead" :queryable="queryable" :ctype="ctype" :dictlist="codevals" allopts></vquery>
        </slot>
    </template>
    <template #default="p">
        <slot v-bind="p">
            <template v-if="!cellEditable(p)">
                <slot name="shower" v-bind="p">
                    <vcell @click.native="onvalueck(p)" style="min-height: 1.5em" :class="{ cvalwrap: true, bclinkcell: link }" :type="stype" :ctype="ctype" :value="getVal(p)" :style="getCellStyle" :maxHeight="maxHeight" :dictlist="dictlist" :size="myRowSize"  :tip="myCelltip">
                    </vcell>
                </slot>
            </template>
            <template v-else>
                <slot name="editer">
                    <seditor :model-value="p.row[field]" :type="ctype" :width="width" :autosize="autosize" :dict="dict" ref="editor" :dictlist="dictlist" :lazy="lazyInput" :max="max" :min="min" :step="step" :precision="precision" :strictly="strictly" :controlsPosition="controlsPosition" @update:model-value="onedit(p, $event)" @blur="closeEdit(p)" @change="onchange(p)">
                        <template #editor="p">
                            <slot v-bind="p" name="edit"></slot>
                        </template>
                    </seditor>
                </slot>
            </template>
        </slot>
    </template>
</el-table-column>
</template>

<script>
// v-show="editable && getCellEdState(p)"
//import { defineComponent } from '@vue/composition-api'
import {
    formatDate
} from "@f/components/common/vclist/tool.js";
import vquery from "./vquery.vue";
import seditor from "@f/components/common/input/seditor.vue";
import vcell from "./vcell.vue";


import {
    loadcodeValueSimple
} from "@/api/zhapi/gene/geneapi.js"


export default {
    name: "vcol",
    components: {
        vquery,
        seditor,
        vcell
    },
    props: {
        //BC上的列信息对象。
        column: null,
        //字段名，使用本属性，必须同时传入BC对象。
        field: null,
        th: {
            type: String,
            default: ""
        }, //查询列查询时的SQL列名
        align: {
            type: String,
            default: "center"
        },
        headerAlign: null,
        caption: {
            type: String,
            default: ""
        },
        width: {
            type: [String, Number]
        },
        minWidth: null,
        maxHeight: null,
        rowSize: null,
        //auto --自动判断  
        celltip:{ type:String, default:""},//默认为自动判断
        //查询行控件类型
        type: {
            type: String,
            default: ""
        },
        //展示控件类型 tip,
        stype: {
            type: String
        },
        //是否显示查询行
        query: {
            type: Boolean,
            default: null
        },
        //是否可以单元格编辑。
        editable: {
            type: Boolean,
            default: false
        },
        link: {
            type: Boolean,
            default: false
        },
        dict: {
            type: String,
            default: ""
        },
        dictlist: null,
        //当启用编辑时，必须传入BC
        //BC: null,
        className: null,
        //编辑的触发，主要包含单击、双击。
        editIndex: {
            type: Number,
            default: -1
        }, //编辑器默认的打开行号。
        editTrigger: {
            type: String,
            default: "click"
        },
        sortable: {
            type: [String, Boolean],
            default: ""
        },
        getCellStyle: {
            type: Function,
            default: function () {
                return {};
            },
        }, //设置单元格的样式。
        rendCell: {
            type: Function
        },
        datefmt: {
            type: String,
            default: ""
        },
        autofocus: {
            type: Boolean,
            default: true
        },
        autosize: {
            type: [Boolean, Object],
            default: false
        },
        fixed: null,
        //单元格的编辑是否是懒加载模式，否则的话，则该列的值一直显示为编辑控件。
        lazyRend: {
            type: Boolean,
            default: false
        },
        //单元格编辑是否懒反馈模式，该模式下优先自行保管编辑值。
        lazyInput: {
            type: Boolean,
            default: false
        },
        editCheck: {
            type: Function
        }, //单元格是否可以编辑的方法。
        autohide: {
            type: Boolean,
            default: false
        },

        //编辑器属性
        //计数器
        //fullnum计数器特有属性
        max: {
            type: Number
        },
        min: {
            type: Number
        },
        step: {
            type: Number
        },
        precision: {
            type: Number
        },
        controlsPosition: null,
        strictly: {
            type: Boolean,
            default: false
        },
    },
    inject: ["vcgrid"],

    computed: {
        codes(){
            return this.$store.state.hhny.cachee.zhcode;
        },
        vc() {
            return this.vcgrid;
        },
        myRowSize() {
            if (this.rowSize != null && this.rowSize != void 0) {
                return this.rowSize;
            }
            if (this.vc.rowSize != null && this.vc.rowSize != void 0) {
                return this.vc.rowSize;
            }
            return "";
        },
        myCelltip() {
            if (this.celltip != null && this.celltip != void 0) {
                return this.celltip;
            }
            if (this.vc.celltip != null && this.vc.celltip != void 0) {
                return this.vc.celltip;
            }
            return "";
        },
        codevals() {
            if (Array.isArray(this.dictlist) && this.dictlist.length > 0) {
                return this.dictlist;
            }
            if (Array.isArray(this.myDictlist) && this.myDictlist.length > 0) {
                return this.myDictlist;
            }
            return [];
        },
        myWidth() {
            try {
                if (!!this.vc.widthKeep) {
                    var tar = this.vc.vcWidths[this.field].vwWidth;
                    if (tar != void 0 && tar != null && tar != "") {
                        return tar + "px";
                    }
                }
            } catch (e) {}

            return this.width;
        },
        colcssname() {
            var baseclass = "nowrapcell";
            if (this.vc.wrapcell) {
                baseclass = "wrapcell";
            }
            if (!!this.className) {
                baseclass += this.className;
            }
            return baseclass;
        },
        cap() {
            if (this.caption != "") return this.caption;
            return this.field;
        },
        ctype() {
            if (this.type != "") return this.type;
            if (Array.isArray(this.dictlist)) {
                return "dict";
            }
            if (this.dict != "") {
                return "codetable";
            }
            this.type;
            //return "text";
        },
        queryable() {
            if (!this.vcgrid) {
                if (this.query == null) return false;
                return this.query;
            }
            //默认时，都是false，
            if (this.query == null) {
                return this.vcgrid.query;
            }
            if (this.query != this.vcgrid.query) {
                return this.query;
            }
        },
        canSort() {
            if (this.sortable == "") {
                return this.vcgrid.sortable;
            }
            return this.sortable;
        },
        colinfo() {
            //整合列的信息；
            var th = this.th;
            if (th == void 0 || th == null || th == "") {
                th = this.field;
            }
            return {
                field: th,
                caption: this.cap,
                ctype: this.ctype,
                sortable: this.canSort,
                queryable: this.queryable,
            };
        },
    },
    data() {
        return {
            myDictlist: [],
            inedit: false,
            eidtboss: {},
            activeRowIndex: this.editIndex,
        };
    },
    watch: {
        editIndex: function () {
            this.activeRowIndex = this.editIndex;
        },
    },
    mounted() {
        this.loadDict();
        if(this.vcgrid !=null){

            this.vcgrid.reportVcol({
                field:this.field,
                caption:this.caption,
                width:this.width,
            })
        }
    },
    methods: {
        loadDict() {
            if (!this.dict) {
                return;
            }

            if(this.codes[this.dict] !=void 0){
                this.myDictlist = this.codes[this.dict] ;
            }
            
            loadcodeValueSimple(this.dict).then((res) => {
                this.myDictlist = res.data;
                this.$store.commit("hhny/setCode",{
                    key:this.dict,
                    value:res.data
                })
            });
        },
        GetCodeCaption(val) {
            var vals = this.codevals;
            for (var i = 0; i < vals.length; i++) {
                var row = vals[i];
                if (row.value == val) {
                    return row.label;
                }
            }
            return val;
        },
        getVal(p) {
            var val="";
            if (typeof this.rendCell == "function"){
                val= this.rendCell(p);
            }
            else{
                val = p.row[this.field];
            }
            //console.log(val)
            
            if (this.dict != "") {
                val = this.GetCodeCaption(val);
            }

            if (this.ctype == "datetime") {
                if (val == "1900-01-01 00:00:00"||val=="1900-01-01T00:00:00") {
                    return "";
                }
                val = formatDate(val, this.datefmt == "" ? "YYYY-MM-DD hh:mm:ss" : this.datefmt);
            } else if (this.ctype == "date") {
                if (val == "1900-01-01 00:00:00"||val=="1900-01-01T00:00:00") {
                    return "";
                }
                val = formatDate(val, this.datefmt == "" ? "YYYY-MM-DD" : this.datefmt);
            } else if (this.ctype == "fullnum") {
                if (this.precision) {
                    val = Number(val).toFixed(this.precision);
                }
            }
            return val;
        },
        cellEditable(p) {
            if (this.editable == false) return false;
            var res = true;
            if (typeof this.editCheck == "function") {
                res = this.editCheck(p);
                if (res == false) return res;
            }
            if (this.vcgrid.accordion) {
                var oid = this.getCellKey(p);
                return this.vcgrid.activeCell == oid;
            }
            if (this.lazyRend) {
                return this.activeRowIndex == p.$index;
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
            if (!this.lazyRend) return true;
            var oid = this.getCellKey(p);
            return this.vcgrid.activeCell == oid;
        },
        closeEdit(p) {
            this.vcgrid.activeCell = null;
            if (this.lazyRend && this.autohide) {
                this.activeRowIndex = -1;
            }
            return;
        },
        onvalueck(p) {
            this.$emit("click", p);
            if (this.editable == false) return false;
            this.activeRowIndex = p.$index;
            if (this.vcgrid.accordion) {
                var oid = this.getCellKey(p);
                this.vcgrid.activeCell = oid;
            }

            if (this.autofocus && this.$refs.editor != void 0) {
                this.$nextTick(() => {
                    if (!!this.$refs.editor) this.$refs.editor.focus();
                });
            }
        },
        //单元格编辑输入值时。
        onedit(p, val, e) {
            if (val == null) val = "";
            var colname = this.field;
            p.row[colname] = val;
            this.vcgrid.addChange(p.row);
        },
        onchange(p, val, e) {
            //if (val == null) val = "";
            //var colname = this.field;
            //p.row[colname]=val;
            this.vcgrid.addChanged(p.row);
        },
    },
};
</script>
