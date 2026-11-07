<template>
  <div :class="{ vclistwrap: true, vcflexed: flex }" ref="sroot">
    <bccard
      :autoload="autoload"
      :BPO="BPO"
      :BC="BC"
      :pageSize="pageSize"
      :pageNum="pageNum"
      :field="BCField"
      :value="BCFVal"
      :op="BCFOp"
      :condi="SQLCondi"
      @load="onload"
      ref="bcardvm"
      plain
    >
      <template #default="p">
        <teleport :to="modalRoot" :disabled="!teleported">
          <div>
            <slot name="header"></slot>
            <el-table
              v-if="!notable"
              :row-key="rowKey"
              ref="table"
              :stripe="stripe"
              :border="border"
              :show-header="showHeader"
              :fit="fit"
              :highlight-current-row="lightRow"
              :empty-text="emptyText"
              :show-summary="haveTotalRow"
              :summary-method="totalfunc"
              class="vcgrid"
              :row-class-name="rowClassName"
              :row-style="rowStyle"
              :span-method="spanfunc"
              :max-height="maxHeight"
              :height="tbheight"
              :data="tbdata"
              :expand-row-keys="expandRowKeys"
              :header-cell-class-name="getHeadClassName"
              @row-click="onrowclk"
              @selection-change="onselect"
              @sort-change="sortbc"
              @current-change="onrowchange"
              @expand-change="onexpand"
              @header-dragend="onWidthchange"
              :style="tbStyle"
            >
              <template #default="p">
                <el-table-column
                  v-if="checkable"
                  type="selection"
                  align="center"
                  width="55"
                >
                </el-table-column>
                <el-table-column
                  v-if="orderCol"
                  type="index"
                  label="序号"
                  width="65"
                ></el-table-column>
                <slot></slot>
              </template>
            </el-table>
            <slot name="table" :vc="me" :list="tbdata"></slot>
            <slot name="pager" v-if="!teleported">
              <vcpager
                v-if="para != null"
                :para="para"
                :layout="pagerLayout"
                :vc="me"
                :total="total"
                :small="smallPager"
              >
                <span
                  v-if="maxable"
                  @click="openTomodal"
                  class="el-button"
                  style="margin-left: 15px"
                  title="最大化显示列表"
                >
                  <el-icon><FullScreen /></el-icon>
                </span>

                <span
                  v-if="exportable"
                  @click="exportByEle"
                  class="el-button"
                  style="margin-left: 15px"
                  title="导出列表为excel"
                >
                  <el-icon><Download /></el-icon>
                </span>
              </vcpager>
            </slot>
            <slot name="foot"></slot>
          </div>
        </teleport>
        <modal
          :show="seemodal"
          @update:model-value="closeMoald"
          title="最大化"
          width="95vw"
        >
          <template #header>
            <slot name="maxtop">
              <div>
                {{ maxTitle }}
              </div>
            </slot>
          </template>
          <div ref="groot" style="height: 100%"></div>
          <template #footer>
            <slot name="maxfoot" v-if="teleported">
              <vcpager
                v-if="para != null"
                :para="para"
                :layout="pagerLayout"
                :vc="me"
                :total="total"
              >
                <span
                  @click="closeMoald"
                  class="el-button"
                  style="margin-left: 15px"
                  title="还原列表显示"
                >
                  <el-icon><Close /></el-icon>
                </span>
              </vcpager>
              <div v-if="para == null" style="text-align: center">
                <span
                  @click="closeMoald"
                  class="el-button"
                  style="margin-left: 15px"
                  title="还原列表显示"
                >
                  <el-icon><Close /></el-icon>关闭
                </span>
              </div>
            </slot>
          </template>
        </modal>
      </template>
    </bccard>
  </div>
</template>
<script>
/**
 *
 * 更新 2022-9-22 增加列表保持功能，需配合后端接口。
 * 更新 2022-9-16 增加全屏化显示功能，增加flex模式下的高度自控制功能。此时滚动条为table的滚动条。
 */

import bccard from "@/components/ui/bccard.vue";

import teleport from "@f/components/common/teleport.vue";
import modal from "@f/components/common/vclist/modal.vue";
import {
  sheetToBlob,
  openDownloadDialog,
  writeSheetColumnsConfig,
} from "@f/components/common/excel/tool.js";

import { exportVC } from "@/libs/invoke.js"

//<el-button @click="closeMoald">关闭</el-button>
export default {
  name: "vgrid",
  emits: [
    "spanbuild",
    "rowclick",
    "check",
    "query",
    "pagination",
    "update:para",
    "current-change",
    "expand-change",
  ],
  expose: ["getChangedRows", "toggle", "clearChange", "getCheckValues"],
  components: {
    teleport,
    modal,
    bccard,
  },
  props: {
    BPO: { type: String, default: "" },
    BC: { type: String, default: "" },
    pageSize: { type: Number, default: 50 },
    pageNum: { type: Number, default: 1 },
    field: { type: Array, default: () => [] },
    val: { type: Array, default: () => [] },
    op: { type: Array, default: () => [] },
    condi: { type: String, default: "" },
    autoload: {
      type: Boolean,
      default: true,
    },
    //核心数据列表

    //翻页及查询的参数
    para: null,
    total: null,
    rowKey: { type: String },
    height: {},
    maxHeight: null,
    query: { type: Boolean, default: false }, //是否有查询行
    checkable: { type: Boolean, default: false }, //是否有复选框
    sortable: { type: [String, Boolean], default: false },
    stripe: { type: Boolean, default: true }, //斑马纹
    border: { type: Boolean, default: true }, //垂直边框
    fit: { type: Boolean, default: true }, //列宽自动撑开
    flex: { type: Boolean, default: false }, //是否表体以flex撑开
    showHeader: { type: Boolean, default: true }, //显示表头
    lightRow: { type: Boolean, default: true }, //高亮选中行
    haveTotalRow: { type: Boolean, default: false }, //合计行
    notable: { type: Boolean, default: false }, //是否自定义数据体内容。
    wraphead: { type: Boolean, default: false }, //标题是否换行
    wrapcell: { type: Boolean, default: false }, //单元格内容是否换行
    orderCol: { type: Boolean, default: false }, //是否有序号列
    emptyText: { type: String, default: "暂无数据" },
    totalfunc: null, //合计行方法
    spanMethod: null, //单元格合并方法
    spanCols: {
      type: Array,
      default: function () {
        return [];
      },
    }, //自动合并的列。定义此属性时，不能同时自定义合并方法。
    spanEmpty: { type: Boolean, default: true }, //空值是否合并
    tbStyle: {
      type: Object,
      default: function () {
        return { width: "100%" };
      },
    },
    bcEvent: { type: Boolean, default: false }, //bc行变化时，是否需要事件

    //自定义的BC数据过滤器
    filter: null,
    pagerLayout: null,
    smallPager: { type: Boolean, default: false },

    accordion: { type: Boolean, default: false }, //是否同时只允许一个单元格编辑。

    rowSave: null, //行数据变更，保存的方法，传入时，不再执行批量变更的缓存。
    lazySave: { type: Boolean, default: false }, //行数据的保存，是否延迟保存。

    rowClassName: null, //el-table的原始行样式
    rowStyle: null, //el-table的原始行样式
    expandRowKeys: null,
    maxable: { type: Boolean, default: false }, //是否可以全屏。
    maxTitle: { type: String, default: "最大化" }, //全屏的标题

    widthKeep: { type: String, default: "" }, //列宽拖动保存的识别id，无效时不保存。

    exportable: { type: Boolean, default: false }, //是否可以导出。
    exconfig: { type: Array }, //导出的列数组 columns:[{field,caption,width }]等基本属性。mode:replace/append
    rowSize: { type: [Number, String], default: "" }, //行高控制
    celltip:{ type:String,default:"auto" },//默认为自动判断
  },
  //mixins: [bcmix],
  data: function () {
    return {
      list: [],
      checkValues: [],
      queryData: {},
      me: this,
      mergRows: {}, //列名为key的数组。
      mergSpans: {}, //相同数组，内放合并的区域数据。

      changedIds: [], //发生单元格编辑的行id
      changedRows: {}, //发生变化的值集合。
      activeCell: null, //正在编辑的单元格。
      oldOrderBy: "",
      oldAsc: "",
      focusRow: null,
      //弹窗最大化相关功能
      seemodal: false, //是否显示弹窗
      teleported: false, //是否传送
      modalRoot: null, //传送时的根节点
      rootHeight: null,
      liveHeight: null, //表体的高度
      normalHeight: null, //未开启全屏时的表体高度。
      modalHeight: null, //弹窗容器的高度
      vcWidths: {}, //列宽自定义的集合
      vcols:[],//
      columns: [],
      columnMap: {}, //从element中获取的列信息集合
      //查询行需要用的
      myCondition: [],
      myField: [],
      myFval: [],
      myFOp: [],
    };
  },
  mounted() {
    //this.loadHeightInfo();
    //console.log(this.$refs.table)
  },
  updated() {
    this.loadHeightInfo();
  },
  computed: {
    SQLCondi() {
      var res = [];

      if (!!this.myCondition && this.myCondition.length > 0) {
        res = this.myCondition;
      }
      if (!!this.condi) {
        res.push(this.condi);
      }
      return res.join(" and ");
    },
    BCField() {
      return this.field.concat(this.myField);
    },
    BCFVal() {
      return this.val.concat(this.myFval);
    },
    BCFOp() {
      return this.op.concat(this.myFOp);
    },
    tbdata() {
      var bcdata = this.list;
      if (typeof this.filter == "function") {
        bcdata = this.filter(bcdata);
      }
      this.buildSpan(bcdata);
      return bcdata;
    },
    spanfunc() {
      if (this.spanCols.length > 0) {
        return this.spancolfunc;
      }
      if (this.spanMethod != null) {
        return this.spanMethod;
      }
      return null;
    },
    liveTBHeight() {
      return this.liveHeight;
    },
    tbheight() {
      var he = this.height;
      //console.log(he);
      if (this.teleported) {
        if (this.modalHeight != null) {
          return this.modalHeight;
        }
        return "100%";
      }
      if (this.flex) {
        return this.liveTBHeight;
      } else if (this.seemodal) {
        return this.liveTBHeight;
      }
      if (!!this.para) {
        if (/^\d+$/.test(he)) {
          var h = he * 1 - 41;
          return h;
        }
      }
      // if(!he){
      //     return "100%";
      // }
      return he;
    },
  },
  provide() {
    return {
      vcgrid: this,
    };
  },
  watch: {
    widthKeep: {
      handler(val, oldVal) {
        this.loadVcWidths();
      },
      immediate: true,
    },
  },
  methods: {
    refresh() {
      this.$refs.bcardvm.refresh();
    },
    onload(list, total) {
      this.list = list;
    },
    log(p) {
      //console.log(p);
      return p;
    },
    reportVcol(obj){
        this.vcols.push(obj);
    },
    /**编辑功能 */
    getChangedRows() {
      if (this.changedIds.length == 0) return [];
      var res = this.changedIds.map((id) => {
        return this.changedRows[id];
      });
      return res;
    },
    addChange(row) {
      if (typeof this.rowSave == "function") {
        if (!this.lazySave) {
          this.callSave(row);
        }
        return;
      }
      this.addToChange(row);
    },
    //当输入结束时，调用本方法，其它特性与上方法相同
    addChanged(row) {
      if (this.lazySave && typeof this.rowSave == "function") {
        this.callSave(row);
        return;
      }
    },
    addToChange(row) {
      var id;
      if (!!this.rowKey) {
        id = row[this.rowKey];
      } else {
        id = row;
      }
      if (this.changedIds.indexOf(id) == -1) {
        this.changedIds.push(id);
      }
      this.changedRows[id] = row;
    },
    removeChange(row) {
      var id;
      if (!!this.rowKey) {
        id = row[this.rowKey];
      } else {
        id = row;
      }
      var index = this.changedIds.indexOf(id);
      if (index != -1) {
        this.changedIds.splice(index, 1);
      }
      this.changedRows[id] = void 0;
    },
    callSave(row) {
      var vm = this;
      this.rowSave(row, function () {
        vm.removeChange(row);
      });
    },
    clearChange() {
      this.changedIds = [];
      this.changedRows = {};
    },

    /**自动的列合并相关功能 */
    buildSpan(list) {
      if (this.spanCols.length == 0 || list.length == 0) return;
      var margedRows = {};
      var mergetRange = {};
      for (var i = 0; i < this.spanCols.length; i++) {
        var col = this.spanCols[i];
        var mergRows = [];
        var ranges = {};
        //开始循环行数据。
        var preval = list[0][col];
        var samecount = 1;
        var first = 0;

        for (var j = 1; j < list.length; j++) {
          var val = list[j][col];
          var emptNo =
            this.spanEmpty == false && (val == void 0 || val == null || val == "");
          if (!emptNo && preval == val) {
            //值相同
            samecount++;
            if (j == list.length - 1) {
              //如果是最后一个。
              var range = {
                rowspan: samecount,
                colspan: 1,
              };
              ranges[first] = range;
              mergRows.push(first);
              for (var s = first + 1; s <= j; s++) {
                ranges[s] = { colspan: 0, rowspan: 0 };
                mergRows.push(s);
              }
            }
          } else {
            if (samecount > 1) {
              //此时区域结束。存储区域信息，并重新初始化；
              var range = {
                rowspan: samecount,
                colspan: 1,
              };
              ranges[first] = range;
              mergRows.push(first);
              for (var s = first + 1; s < j; s++) {
                ranges[s] = { colspan: 0, rowspan: 0 };
                mergRows.push(s);
              }
              //初始化
            }
            first = j;
            preval = val;
            samecount = 1;
          }
        }
        margedRows[col] = mergRows;
        mergetRange[col] = ranges;
      }
      this.mergRows = margedRows;
      this.mergSpans = mergetRange;
      this.$emit("spanbuild", margedRows, mergetRange);
    },
    spancolfunc(para) {
      //将所有要合并的单元格位置放到一个数组里面，然后用另一个对象，存储这个合并区域的边界信息。
      //用另一个完全相同长度的数组，存放结果的span对象。
      //为允许同时外界自定义，当外界自定义，但返回null时，将启用自动合并方法
      if (typeof this.spanMethod == "function") {
        var res = this.spanMethod(para, this);
        if (res != null) {
          return res;
        }
      }
      return this.getspanVal(para.column, para.rowIndex);
    },
    getspanVal(col, r) {
      var colname = col.property;
      var tar = this.mergRows[colname];
      if (Array.isArray(tar) && tar.indexOf(r) != -1) {
        var res = this.mergSpans[colname][r];
        //
        return res;
      }
      return [1, 1];
    },
    getHeadClassName(p) {
      var column = p.column;
      if (column.level == 1) {
        this.columns[column.no] = column;
      }
      this.columnMap[column.id] = column;
      return "vccap";
    },
    getLeafColumns() {
      //获取按顺序的叶子节点列信息数组。
      var res = [];
      function readColist(list) {
        for (var i = 0; i < list.length; i++) {
          var tar = list[i];
          if (Array.isArray(tar.children) && tar.children.length > 0) {
            readColist(tar.children);
          } else {
            res.push(tar);
          }
        }
      }
      readColist(this.columns);
      return res;
    },
    /**事件 */
    onrowclk(row, col, e) {
      this.locaBCByRow(row);
      this.$emit("rowclick", row, col, e);
    },
    onrowchange(currow, oldcur) {
      this.focusRow = currow;
      this.$emit("current-change", currow, oldcur);
    },
    onexpand(e, state) {
      this.$emit("expand-change", e, state);
    },
    onWidthchange(newval, oldval, col, e) {
      //console.log(arguments);
      var fkey = col.property;
      if (fkey == void 0 || fkey == null || fkey == "") return;
      if (!!this.widthKeep) {
        saveVclistWidth({
          vwKey: this.widthKeep,
          vwField: col.property,
          vwWidth: newval,
          vwCaption: col.label,
          vwNo: col.no,
        });
      }
      //saveVclistWidth
    },
    loadVcWidths() {
      if (!!this.widthKeep) {
        loadVclistWidth(this.widthKeep).then((res) => {
          this.vcWidths = res.data;
        });
      }
    },
    getCheckValues(fieldName) {
      if (fieldName == void 0) {
        return this.checkValues;
      }
      return this.checkValues.map((obj) => {
        return obj[fieldName];
      });
    },
    locaBCByRow(row) {},
    onselect(val) {
      this.checkValues = val;
      this.$emit("check", val);
    },
    setQuery(info, id) {
      this.queryData[id] = info;
      //将参数修改，反馈给外界。
      if (typeof this.para == "object") {
        var paraKeys = Object.keys(this.para);
        if (paraKeys.indexOf(info.column.field) != -1) {
          var newval = Object.assign({}, this.para);
          newval[info.column.field] = info.value;
          this.$emit("para", newval);
        }
      }
    },
    onRunQuery() {
      this.$emit("query");
      //由于前段不具备自行拼接条件的功能。此处直接交由外界处理。
      //return;
      console.log("query vcgrid");
      //调用BC，执行查询行的查询。
      var fieldList = [];
      var valueList = [];
      var condiIndentList = [];

      var dataTable = this.BC;
      //var bpo = this.BusinessObject;

      var flagCodeTable = false;
      var colkeys = Object.keys(this.queryData);
      var queryCondis = [];
      for (var i = 0; i < colkeys.length; i++) {
        var colid = colkeys[i];
        var queryInfo = this.queryData[colid];
        var column = queryInfo.column;
        var value = queryInfo.value;
        var cmd = queryInfo.cmd || "";
        var fieldKind = column.fieldKind;

        if (queryInfo.you.queryable != true || cmd == "") {
          continue;
        }

        //如果没有字段对应的SQL列部分信息，无法执行查询
        var fieldName = column.field;
        if (!fieldName) {
          continue;
        }

        if (value != "") {
          if (column.isCodeTable && column.codeTable) {
            var condi = codeTableWhereByValue(fieldName, value);
            queryCondis.push(condi);
          } else {
            if (cmd == "BETWEEN") {
              var v = value.toUpperCase();
              var r = v.match("AND");
              if (r == null) {
                alert('请输入"value1 and value2"格式条件');
                return;
              }
            }
            fieldList.push(fieldName);
            valueList.push(value);
            condiIndentList.push(cmd);
          }
        } else {
          if (cmd == "IS" || cmd == "IS NOT") {
            var condi = fieldName + " " + cmd + " NULL";
            queryCondis.push(condi);

            continue;
          } else if (cmd == "Empty" || cmd == "Not Empty") {
            var condi = fieldName + " <> ''";
            if (cmd == "Empty") {
              condi = fieldName + " = ''";
            }
            queryCondis.push(condi);
            continue;
          }
        }
      }
      //将条件赋值，触发查询
      this.myCondition = queryCondis;
      this.myField = fieldList;
      this.myFval = valueList;
      this.myFOp = condiIndentList;

      this.refresh();
      this.$message("正在查询...");
      function codeTableWhereByValue(fieldName, value) {
        value = value.replace(/\'/g, "");
        var values = value.split(";");
        var logicConnect = " or ";
        var sql = "";
        if (values.length > 0) {
          sql = " ( ";
          for (var i = 0; i < values.length; i++) {
            if (i > 0) {
              sql += " " + logicConnect + " ";
            }
            sql += fieldName + " = '" + values[i] + "'";
          }
          sql += " )";
        }
        return sql;
      }
    },
    clearQuery() {
      //先通知每个列清理，然后把自己清理
      var colkeys = Object.keys(this.queryData);
      var queryCondis = [];
      for (var i = 0; i < colkeys.length; i++) {
        var tar = this.queryData[colkeys[i]];
        tar.you.resetQuery();
      }
      this.queryData = {};
      this.onRunQuery();
    },
    sortbc(e) {
      if (!this.sortable) return;
      if (this.oldOrderBy == "") {
        this.oldOrderBy = this.para.orderByColumn;
      }
      if (this.oldAsc == "") {
        this.oldAsc = this.para.isAsc || "";
      }
      var newval = Object.assign({}, this.c);
      newval.orderByColumn = e.column.sortBy;
      if (e.order == "ascending") {
        newval.isAsc = "asc";
      } else if (e.order == "descending") {
        newval.isAsc = "desc";
      } else {
        newval.orderByColumn = this.oldOrderBy;
        newval.isAsc = this.oldAsc;
      }
      this.$emit("update:para", newval);
      this.$emit("query", newval);
      this.$message("正在排序...");
    },
    //弹窗最大化的功能
    loadHeightInfo() {
      var tbel = this.$refs.table;
      if (this.maxable && !this.seemodal && tbel != void 0) {
        var h = getComputedStyle(tbel.$el).height;
        if (this.teleported == false && this.liveHeight != h) {
          this.liveHeight = h;
        }
        /*
                if(this.teleported==false && this.normalHeight!=h){
                    this.normalHeight=h;
                }*/
      }
      //var h=getComputedStyle(this.$refs.groot).height;
      //var rect=this.$refs.groot.getBoundingClientRect();
      //console.log(rect);
      //return h
      if (this.seemodal && this.$refs.groot != void 0) {
        var sh = this.$refs.groot.scrollHeight;
        if (sh != this.modalHeight) {
          this.modalHeight = sh;
        }
      }
      //rootHeight sroot
      if (this.$refs.sroot != void 0) {
        var sh = this.$refs.sroot.clientHeight;
        if (sh != this.rootHeight) {
          this.rootHeight = sh;
        }
      }
    },
    toggle(val) {
      if (typeof val == "boolean") {
        if (val) {
          this.openTomodal();
        } else {
          this.closeMoald();
        }
      }
    },
    openTomodal() {
      //先开启弹窗，再开启传送，为了能够正确计算弹窗的高度。
      //if(this.liveHeight==null){
      this.loadHeightInfo();
      //}
      this.seemodal = true;
      this.$nextTick(() => {
        this.openTele();
      });
    },
    closeMoald() {
      //关闭时，先关传送，再关弹窗。 这样可以先还原table的高度，
      this.teleported = false;
      this.$nextTick(() => {
        this.seemodal = false;
      });
    },
    openTele() {
      var goot = this.$refs.groot;
      if (goot != void 0) {
        this.modalRoot = goot;
        this.teleported = true;
      } else {
        this.$nextTick(() => {
          this.openTele();
        });
      }
    },
    //excel导出相关功能。
    exportByEle() {
      try {
        var sheet = this.getSheetByConfig(); //将一个table对象转换成一个sheet对象
        this.wrapExcelWiths(sheet);
        openDownloadDialog(sheetToBlob(sheet), "下载.xlsx");
      } catch (e) {
        console.log(e);
      }
    },
    wrapExcelWiths(sheet) {
      var cols = this.getLeafColumns();
      var excelColstyle = [];
      for (var k = 0; k < cols.length; k++) {
        var col = cols[k];
        var woth = col.width;
        if (!woth && !!col.realWidth) {
          woth = col.realWidth;
        }
        if (!woth) {
          woth = 9;
        }
        if (typeof woth == "string") {
          woth = woth.replace("px", "");
        }
        var width = woth;
        width = (width * 1) / 8;
        var obj = { wch: width };
        excelColstyle.push(obj);
      }
      sheet["!cols"] = excelColstyle;
      return sheet;
    },
    getSheetByConfig() {
      //exconfig
      var sheet = {};
      if (this.exconfig == void 0) {
        var tbel = this.$refs.table.$el;
        sheet = XLSX.utils.table_to_sheet(tbel); //将一个table对象转换成一个sheet对象
        return sheet;
      }
      var config = this.exconfig;
      //默认为追加模式，如果在列数组设置列的mode为replace，则该列为替换模式。
      //如果整体为替换模式，则列数组中强制为替换模式。
      if (config.mode == void 0 || config.mode == "append") {
        sheet = XLSX.utils.table_to_sheet(this.$refs.table.$el);
        config.startColumn = this.columns.length + 1;
        writeSheetColumnsConfig(sheet, this.list, this.getLeafColumns(), config);
      } else if (config.mode == "replace") {
        writeSheetColumnsConfig(sheet, this.list, this.getLeafColumns(), config);
      }
      return sheet;
    },
    exportByData() {
      var rows = this.list;
      var sheet = exportVueTable(rows);
      const wb = { SheetNames: ["Sheet1"], Sheets: {}, Props: {} };
      wb.Sheets["Sheet1"] = sheet;

      var flname = "评先评优汇总表.xlsx";
      //XLSX.writeFile(wb, year+"年"+month+"月月度需求统计"+".xlsx");
      XLSX.writeFile(wb, flname, { bookType: "xlsx", bookSST: false });

      function exportVueTable(sends) {
        //循环每个列进行创建即可。
        var colCode = [
          "A",
          "B",
          "C",
          "D",
          "E",
          "F",
          "G",
          "H",
          "I",
          "J",
          "K",
          "L",
          "M",
          "N",
          "O",
          "P",
          "Q",
          "R",
          "S",
          "T",
          "U",
          "V",
          "W",
          "X",
          "Y",
          "Z",
        ];
        //先获取行高
        var sheet = {};

        var titleRowCount = 1;
        var baseColStyle = {
          font: { name: "宋体", sz: 14, bold: false },
          alignment: { horizontal: "center", vertical: "center", wrapText: true },
          border: {
            top: { style: "thin" },
            bottom: { style: "thin" },
            left: { style: "thin", color: { auto: 1 } },
            right: { style: "thin", color: { auto: 1 } },
          },
        };

        var merges = [];

        var maxRows = 1;
        var maxCols = 1;
        var maxKey = "A1";

        //执行表体数据的创建
        sheet["A2"] = {
          t: "s", //类型
          v: "序号",
          s: Object.assign({}, baseColStyle),
        };

        var mergeObj = {
          //坐标从0开始
          s: { r: 1, c: 0 }, //合并开始坐标start
          e: { r: 2, c: 0 }, //合并结束坐标end
        };
        merges.push(mergeObj);

        var startrow = 4;
        for (var i = 0; i < sends.length; i++) {
          var send = sends[i];
          var rows = 1;
          var max = Math.max(
            send.gootpt1.length,
            send.gootpt2.length,
            send.gootpt3.length,
            send.gootpw.length
          );
          if (send.rows > 1) {
            rows = send.rows;
          }
          sheet["A" + startrow] = {
            t: "s", //类型
            v: i + 1,
            s: Object.assign({}, baseColStyle),
          };

          for (var o = 0; o < rows; o++) {
            sheet["A" + (startrow + o + 1)] = {
              t: "s", //类型
              v: "--",
              s: Object.assign({}, baseColStyle),
            };
          }

          mergeObj = {
            //坐标从0开始
            s: { r: startrow - 1, c: 0 }, //合并开始坐标start
            e: { r: startrow + rows - 2, c: 0 }, //合并结束坐标end
          };
          merges.push(mergeObj);

          sheet["B" + startrow] = {
            t: "s", //类型
            v: send.SO_Name,
            s: Object.assign({}, baseColStyle),
          };

          mergeObj = {
            //坐标从0开始
            s: { r: startrow - 1, c: 1 }, //合并开始坐标start
            e: { r: startrow + rows - 2, c: 1 }, //合并结束坐标end
          };
          merges.push(mergeObj);

          startrow = startrow + rows;
        }
        var excelColstyle = [
          { wch: 5 },
          { wch: 30 },
          { wch: 30 },
          { wch: 30 },
          { wch: 30 },
          { wch: 14 },
          { wch: 30 },
          { wch: 14 },
          { wch: 67 },
          { wch: 30 },
        ];

        function cellWork(rowIndex, rowobj, sheet) {
          //遍历基础列，进行创建

          for (var c = 0; c < cols.length; c++) {
            var colobj = cols[c];
            var colNum = "";
            //拼接表格坐标AA12
            if (c < colCode.length) {
              colNum = colCode[c];
            } else {
              //不考虑超过2个字母编码的数据。
              var num1 = Math.floor(c / colCode.length);
              var num2 = c % colCode.length;
              colNum = colCode[num1] + colCode[num2];
            }
            var myrow = rowIndex;
            var cellKey = colNum + myrow;
            if (myrow > maxRows || c > maxCols) {
              maxCols = c;
              maxRows = myrow;
              maxKey = cellKey;
            }
            var cellVal = rowobj[colobj.key];
            if (typeof colobj.getValue == "function") {
              cellVal = colobj.getValue(rowobj, colobj, rows, r);
            }
            if (cellVal == null || cellVal == void 0) {
              cellVal = "";
            }
            var cellObj = {
              t: "s", //类型
              v: cellVal,
              s: Object.assign({}, baseColStyle),
            };
            sheet[cellKey] = cellObj;
          }
        }

        sheet["!merges"] = merges;
        sheet["!ref"] = "A1:" + "Z1000";
        sheet["!cols"] = excelColstyle;
        console.log(sheet);
        return sheet;
      }
    },
    exportByBC(kv,FileName){
        if(kv==null || kv==void 0 ||kv.length==0){
            kv=this.vcols.map((li)=>{
                var width="200";
                if(!!li.width){
                    width=li.width.replace("px","")
                }
                return {
                    field:li.field,
                    caption:li.caption,
                    width:width,
                }
            });
        }
        var para={
            BPOName:this.BPO,
            VCName:"",
            BCName:this.BC,
            kvs:kv,
            Rows:"",
            FileName:FileName,
            mergeColumns:"",
            condiField:this.BCField,
            condiValue:this.BCFVal,
            condiIndent:this.BCFOp,
            sqlCondi:this.SQLCondi,
            sqlCondiType:0,
            nStartPos:0,
            nRecord:-1,
        }
        exportVC(para).then((res)=>{

        })
    },
  },
};
</script>

<style>
.nowrapcell.cell {
  white-space: nowrap;
  word-wrap: normal;
}
.vclist .vccap {
  font-size: 13px;
}
.vcqyrwse > .el-input > .el-input__inner {
  display: none;
}
.vcqyrwse > .el-input > .el-input__suffix {
  display: none;
}
.vcqyrwsedit .el-input__icon {
  line-height: 26px;
}
.vcqyrwse {
  flex: 0 0 20px;
  min-width: 20px;
  min-height: 14px;
  position: absolute;
  right: 1px;
}
.vcqyrwpp > div > .el-scrollbar__wrap {
  max-height: 600px;
}
.vcqyrwpp .el-select-dropdown__item {
  height: 27px;
  line-height: 26px;
}
.el-table th {
  padding: 0 0 0 0;
}
.vcqyrwsedit.seeditpa {
  padding: 0;
  grid-column: 1/-1;
  display: flex;
  align-items: center;
}
.vcqyrwsedit .el-input__inner {
  height: 26px;
  line-height: 26px;
}
.el-table th > .cell.vcthead {
  display: grid;
  grid-template-columns: 1fr auto;
  padding-right: 0px;
  padding-left: 0;
}
.vcthead > .caret-wrapper {
  grid-column: 2/3;
  grid-row: 1/2;
  position: absolute;
  top: -5px;
  right: 0px;
  z-index: -1;
}

.vclist.el-table {
}

.vclist .bclinkcell {
  color: #3163a5;
  text-decoration: underline;
  cursor: pointer;
}
.vcthead .vccap {
  z-index: 1;
}
.vcflexed.vclistwrap {
  display: flex;
  flex-direction: column;
  height: 100%;
}
.vcflexed.vclistwrap > .el-table.vclist {
  flex: 1;
}
.el-table .pangcell > .cell {
  padding-left: 1px;
  padding-right: 1px;
}
</style>
