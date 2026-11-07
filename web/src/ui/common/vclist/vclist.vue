<template>
    <div :class="['vclistwrap', flex ? 'vcflexed' : '', 'vc-'+face]" ref="sroot">
        <teleport :to="modalRoot" :disabled="!teleported">
            <div :class="{'vclist-inner':true,'vclist-inner-flex':flex,'vcard-ppan':face === 'card'}" >
                <slot name="header"></slot>
                <el-table v-if="RendTable" :row-key="rowKey" ref="table" :stripe="stripe" :border="border" :show-header="showHeader" :fit="fit" 
                    :highlight-current-row="lightRow"
                    :empty-text="emptyText"
                    :show-summary="haveTotalRow"
                    :summary-method="totalfunc" class="vclist" :row-class-name="rowClassName" :row-style="rowStyle" :cell-style="handlerCellStyle"
                    :span-method="spanfunc" :max-height="maxHeight" :height="tbheight"
                    :data="tbdata" :expand-row-keys="expandRowKeys"
                    :header-cell-class-name="getHeadClassName"
                    :tree-props="treeProps" :lazy="lazy" :load="load" :default-expand-all="defaultExpandAll"
                    :scrollbar-always-on="scrollbarAlwaysOn"
                    @row-click="onrowclk" @selection-change="onselect" @sort-change="sortbc" @current-change="onrowchange" @expand-change="onexpand" @header-dragend="onWidthchange"
                    :style="tbStyle">

                    <template #default="p">
                        <el-table-column v-if="dragable" fixed="left" width="44" align="center" class-name="vc-drag-col">
                            <template #default>
                                <span class="vc-drag-handle" title="拖动调整顺序">⋮⋮</span>
                            </template>
                        </el-table-column>
                        <el-table-column v-if="checkable" type="selection" fixed="left" align="center"  width="55"> </el-table-column>
                        <el-table-column v-if="orderCol" fixed="left" type="index" label="序号"  width="65" :align="'center'"></el-table-column>
                        <!-- 树表展开/缩进默认占第一列，会吞掉首列单元格点击；占位列承接树形，后续 vccol 首列可正常编辑 -->
                        <el-table-column
                            v-if="treeLeadBlank && treeProps"
                            label=""
                            width="52"
                            align="left"
                            class-name="vc-tree-lead-blank"
                            :resizable="false"
                        >
                            <template #default><span class="vc-tree-lead-blank-inner"></span></template>
                        </el-table-column>
                        <slot ></slot>
                    </template>
                    <template #empty>
                        <template v-if="face=='list'">
                            <div class="vc-li" v-for="(row,r) in list" :key="r">
                                <slot name="li" :row="row" :index="r"></slot>
                            </div>
                        </template>
                        <template v-else>
                            <slot name="empty">
                                <span class="el-table__empty-text">{{ emptyText }}</span>
                            </slot>
                        </template>
                    </template>
                </el-table>
                <div v-else-if="notable" class="vclist-notable-wrap">
                    <slot name="table" :list="list">
                        <slot :list="list" :face="face"></slot>
                    </slot>
                </div>
                <div v-if="face=='card'" :style="vccardPanStyle" class="vcard-pan">
                    <querybar
                        :trigger="queryTrigger"
                        :label="queryBarLabel"
                        :expand-text="queryExpandText"
                        :collapse-text="queryCollapseText"
                        :show-search="queryShowSearch"
                        :search-placeholder="querySearchPlaceholder"
                        :popover-width="queryPopoverWidth"
                        :show-panel-actions="queryShowPanelActions"
                        :filters="queryFilters"
                        :fields="queryFastFields"
                        :fieldMap="fieldMap"
                        @search="onQueryBarSearch"
                        v-model:txt="queryTxt"
                    >
                        <template #default="qbp">
                            <slot name="query" :list="list" :face="face" v-bind="qbp">
                                <slot :list="list" :face="face" v-bind="qbp"></slot>
                            </slot>
                        </template>

                        <template v-if="$slots.querySearch" #search="p">
                            <slot name="querySearch" v-bind="p"></slot>
                        </template>
                        <template v-if="$slots.queryTags" #tags="p">
                            <slot name="queryTags" v-bind="p"></slot>
                        </template>
                        <template v-if="$slots.queryExpand" #expand="p">
                            <slot name="queryExpand" v-bind="p"></slot>
                        </template>
                        <template #bar>
                            <slot name="query-bar"></slot>
                        </template>
                    </querybar>
                    <div class="vc-card-list">
                        <slot name="card" :list="list">
                            <div class="vc-li" v-for="(row,r) in list" :key="r">
                                <slot name="li" :row="row" :index="r"></slot>
                            </div>                            
                        </slot>
                    </div>
                </div>
                <slot name="pager" v-if="showPagination && !teleported">
                    <vcpager v-if="para!=null" :para="para" :layout="pagerLayout" :vc="me" :total="total" :small="smallPager"  :background="background">
                        <span v-if="maxable" @click="openTomodal" class="el-button vclist-max-btn" style="margin-left: 15px;" title="最大化显示列表">
                            <svg class="icon vclist-max-icon" style="width: 1em;height: 1em;vertical-align: middle;fill: currentColor;overflow: hidden;" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg"><path d="M368.896 192H224a32 32 0 0 0-32 32v137.888a32 32 0 0 0 64 0V256h112.896a32 32 0 0 0 0-64zM784.864 192H640a32 32 0 1 0 0 64h112.864v105.888a32 32 0 1 0 64 0V224a32 32 0 0 0-32-32zM368.896 777.92H256V672a32 32 0 1 0-64 0v137.92a32 32 0 0 0 32 32h144.896a32 32 0 1 0 0-64zM784.864 640a32 32 0 0 0-32 32v105.92H640a32 32 0 1 0 0 64h144.864a32 32 0 0 0 32-32V672a32 32 0 0 0-32-32z" fill="currentColor"></path><path d="M912 48h-800c-35.296 0-64 28.704-64 64v800c0 35.296 28.704 64 64 64h800c35.296 0 64-28.704 64-64v-800c0-35.296-28.704-64-64-64z m-800 864v-800h800l0.064 800H112z" fill="currentColor"></path></svg>
                        </span>

                        <span v-if="exportable" @click="exportByEle" class="el-button" style="margin-left: 15px;" title="导出列表为excel">
                            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1024 1024" data-v-ea893728=""><path fill="currentColor" d="M160 832h704a32 32 0 1 1 0 64H160a32 32 0 1 1 0-64m384-253.696 236.288-236.352 45.248 45.248L508.8 704 192 387.2l45.248-45.248L480 584.704V128h64z"></path></svg>
                        </span>
                        <mfaceToggle
                            v-if="faceKey"
                            v-model="face"
                            :cache-key="faceKey"
                            style="margin-left: 15px;"
                        />
                        <slot name="pagetail"></slot>
                    </vcpager>
                </slot>
                <slot name="foot"></slot>
            </div>
        </teleport>

        <modal :show="seemodal" @update:model-value="closeMoald" title="最大化" width="95vw">
            <template #header>
                <slot name="maxtop">
                    <div >
                        {{maxTitle}}
                    </div>
                </slot>
            </template>
            <div ref="groot" style="height:100%;"></div>
            <template #footer>
                <slot name="maxfoot" v-if="teleported">
                    <vcpager v-if="para!=null" :para="para" :layout="pagerLayout" :vc="me" :total="total" :background="background">
                        <span @click="closeMoald" class="el-button vclist-max-btn" style="margin-left: 15px;" title="还原列表显示">
                            <svg class="icon vclist-max-icon" style="width: 1em;height: 1em;vertical-align: middle;fill: currentColor;overflow: hidden;" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg"><path d="M704 864v-96c0-54.4 41.6-96 96-96h96c19.2 0 32-12.8 32-32s-12.8-32-32-32h-96c-89.6 0-160 70.4-160 160v96c0 19.2 12.8 32 32 32s32-12.8 32-32z m-64-704v96c0 89.6 70.4 160 160 160h96c19.2 0 32-12.8 32-32s-12.8-32-32-32h-96c-54.4 0-96-41.6-96-96v-96c0-19.2-12.8-32-32-32s-32 12.8-32 32z m-256 704v-96c0-89.6-70.4-160-160-160h-96c-19.2 0-32 12.8-32 32s12.8 32 32 32h96c54.4 0 96 41.6 96 96v96c0 19.2 12.8 32 32 32s32-12.8 32-32z m-64-704v96c0 54.4-41.6 96-96 96h-96c-19.2 0-32 12.8-32 32s12.8 32 32 32h96c89.6 0 160-70.4 160-160v-96c0-19.2-12.8-32-32-32s-32 12.8-32 32z" fill="currentColor"></path></svg>
                        </span>
                    </vcpager>
                    <div v-if="para==null" style="text-align:center;">
                        <span @click="closeMoald" class="el-button vclist-max-btn" style="margin-left: 15px;" title="还原列表显示">
                            <svg class="icon vclist-max-icon" style="width: 1em;height: 1em;vertical-align: middle;fill: currentColor;overflow: hidden;" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg"><path d="M704 864v-96c0-54.4 41.6-96 96-96h96c19.2 0 32-12.8 32-32s-12.8-32-32-32h-96c-89.6 0-160 70.4-160 160v96c0 19.2 12.8 32 32 32s32-12.8 32-32z m-64-704v96c0 89.6 70.4 160 160 160h96c19.2 0 32-12.8 32-32s-12.8-32-32-32h-96c-54.4 0-96-41.6-96-96v-96c0-19.2-12.8-32-32-32s-32 12.8-32 32z m-256 704v-96c0-89.6-70.4-160-160-160h-96c-19.2 0-32 12.8-32 32s12.8 32 32 32h96c54.4 0 96 41.6 96 96v96c0 19.2 12.8 32 32 32s32-12.8 32-32z m-64-704v96c0 54.4-41.6 96-96 96h-96c-19.2 0-32 12.8-32 32s12.8 32 32 32h96c89.6 0 160-70.4 160-160v-96c0-19.2-12.8-32-32-32s-32 12.8-32 32z" fill="currentColor"></path></svg>关闭
                        </span>
                    </div>

                </slot>
            </template>
        </modal>
    </div>
</template>
<script setup>
/**
 * vclist — 通用数据列表容器（基于 el-table 封装）。
 *
 * 显示模式（face）：table 表格（默认）、card 卡片+查询栏、list 列表项。
 * 列定义：默认插槽内嵌 vccol；通过 provide('vclist') 向子列注入查询、编辑、变更追踪等能力。
 *
 * 主要能力：分页（vcpager / para）、多选、服务端排序、表头/卡片查询栏、行拖拽排序、
 * 单元格合并（spanCols / spanMethod）、树表（treeProps）、行编辑与变更缓存、列宽记忆、
 * 全屏最大化、Excel 导出、flex 自适应高度、自定义表体（notable）。
 *
 * 常用事件：query、update:para、check、rowclick、drag-sort、querybar-search。
 * 对外暴露：getChangedRows、getCheckValues、setQuery、clearQuery、refreshLayout 等。
 */

import {
    ref,
    reactive,
    computed,
    watch,
    provide,
    onMounted,
    onUpdated,
    onBeforeUnmount,
    getCurrentInstance,
    nextTick,
} from "vue";
import vcpager from "./vcpager.vue";
import teleport from "../teleport.vue";
import modal from "./modal.vue";
import Sortable from "sortablejs";
import querybar from "./querybar.vue";
import mfaceToggle from "@/ui/common/listCardView/mfaceToggle.vue";

import { sheetToBlob, openDownloadDialog, writeSheetColumnsConfig } from "../excel/tool.js";
import { useRemenberColumn } from "@/ui/common/vclist/useRemenberColumn";

const { headerDragend, setColumnWidth } = useRemenberColumn();

defineOptions({
    name: "vclist",
    components: {
        vcpager,
        teleport,
        modal,
    },
});

/** 显示模式：table | card | list；配合 faceKey 可在翻页栏内置切换并缓存 */
const face = defineModel("face", { type: String, default: "table" });

const emit = defineEmits([
    "spanbuild",
    "rowclick",
    "check",
    "update:checked",
    "update:focusRow",
    "query",
    "pagination",
    "update:para",
    "current-change",
    "expand-change",
    "drag-sort",
    "querybar-search",
    "update:face",
    "cell-changed",
]);

const props = defineProps({
    /** 传入时在翻页栏显示 face 切换按钮，并作为缓存 key（如 VCName） */
    faceKey: { type: String, default: "" },
    background: null,
    //核心数据列表
    list: null,
    //翻页及查询的参数
    para: null,
    total: null,
    rowKey: { type: String },
    height: {},
    maxHeight: null,
    query: { type: Boolean, default: false }, //是否有查询行
    queryConditions: null,
    checkable: { type: Boolean, default: false }, //是否有复选框
    checkOnRowClick: { type: Boolean, default: false }, //点击行切换复选框选中
    checked: {
        type: Array,
        default: function () {
            return [];
        },
    }, //已选中。
    sortable: { type: [String, Boolean], default: true },
    stripe: { type: Boolean, default: false }, //斑马纹
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
    dragable: { type: Boolean, default: false }, //是否启用拖拽换序（最左侧手柄列）
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
    align: { type: String },
    SQLCondi: { type: String, default: "" }, //外界的BC查询条件。
    //自定义的BC数据过滤器
    filter: null,
    pagerLayout: null,
    smallPager: { type: Boolean, default: false },
    editable: { type: Boolean, default: false },
    accordion: { type: Boolean, default: true }, //是否同时只允许一个单元格编辑。

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
    //树表属性
    treeProps: null,
    /** 为 true 时在业务列前插入窄占位列，树形展开占该列，避免首列 vccol 无法点编辑 */
    treeLeadBlank: { type: Boolean, default: false },
    lazy: { type: Boolean, default: false },
    load: null,
    defaultExpandAll: { type: Boolean, default: false },

    maxLine: null,
    cellStyle: "",
    tableId: "",

    showPagination: { type: Boolean, default: true },
    /** 表格滚动条是否始终显示 */
    scrollbarAlwaysOn: { type: Boolean, default: false },

    /** card 模式查询栏：浮层触发 click | hover */
    queryTrigger: { type: String, default: "click" },
    queryBarLabel: { type: String, default: "筛选条件" },
    queryExpandText: { type: String, default: "展开过滤" },
    queryCollapseText: { type: String, default: "收起过滤" },
    queryShowSearch: { type: Boolean, default: false },
    querySearchPlaceholder: { type: String, default: "搜索..." },
    queryPopoverWidth: { type: [String, Number], default: 560 },
    queryShowPanelActions: { type: Boolean, default: true },
    /** 外部自定义已选标签，传入时覆盖 queryData 自动汇总 */
    queryFilters: { type: Array, default: null },
    /**
     * card 查询栏快捷字段：字段名字符串数组，或对象数组
     * { field, width?, op?|defaultOp?, showLabel?, label?|title?|caption?, holder?, ctype?, dict?, dictlist? }
     */
    queryFastFields: { type: Array, default: () => [] },
    //快捷过滤的实现
    fastFilter: { type: Function, default: null },
});

const inst = getCurrentInstance();
const me = inst.proxy;
const sroot = ref(null);
const table = ref(null);
const groot = ref(null);

const checkValues = ref([]);
const applyingSelection = ref(false);
const suppressEmptySelectionEmit = ref(false);
const queryData = ref({});
const mergRows = ref({});
const mergSpans = ref({});
const changedIds = ref([]);
const changedRows = ref({});
const editState = reactive({ activeCell: null });
const oldOrderBy = ref("");
const oldAsc = ref("");
const oldOrderBys = ref([]);
const oldSortCaptured = ref(false);
const focusRow = ref(null);
const seemodal = ref(false);
const teleported = defineModel('maxed', { default: false });
const modalRoot = ref(null);
const rootHeight = ref(null);
const liveHeight = ref(null);
const normalHeight = ref(null);
const modalHeight = ref(null);
const vcWidths = ref({});
const columns = ref([]);
const columnMap = ref({});
const dragSortBody = ref(null);
const fieldMap = ref({});
let heightInfoRaf = null;

function registerField(meta) {
    if (!meta?.field) return
    fieldMap.value[meta.field] = meta
}

const vclistInnerStyle = computed(() => {
    var res = {};
    if(face.value === "grid"){
        res.height = tbheight.value+"px";
    }
    return res;
})

const vccardPanStyle = computed(() => {
    var obj = {};
    if(face.value === "card"){
        obj.width = "100%";
        var h=tbheight.value;
        //var h=props.height;
        if(!isNaN(h) && h > 0){
            obj.height = h + "px";
        }
        else if(!!h){
            obj.height = h;
        }
    }
    
    return obj;
});

function scheduleLoadHeightInfo() {
    if (heightInfoRaf != null) {
        cancelAnimationFrame(heightInfoRaf);
    }
    heightInfoRaf = requestAnimationFrame(() => {
        heightInfoRaf = null;
        loadHeightInfo();
    });
}

const RendTable= computed(() => {
    if(props.notable){
        return false;
    }
    return face.value !== "card";
})

const queryTxt = defineModel('txt', { default: '' })

function buildSpan(list) {
    if (props.spanCols.length == 0 || list.length == 0) return;
    var margedRows = {};
    var mergetRange = {};
    for (var i = 0; i < props.spanCols.length; i++) {
        var col = props.spanCols[i];
        var mergRowsArr = [];
        var ranges = {};
        //开始循环行数据。
        var preval = list[0][col];
        var samecount = 1;
        var first = 0;

        for (var j = 1; j < list.length; j++) {
            var val = list[j][col];
            var emptNo = props.spanEmpty == false && (val == void 0 || val == null || val == "");
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
                    mergRowsArr.push(first);
                    for (var s = first + 1; s <= j; s++) {
                        ranges[s] = { colspan: 0, rowspan: 0 };
                        mergRowsArr.push(s);
                    }
                }
            } else {
                if (samecount > 1) {
                    //此时区域结束。存储区域信息，并重新初始化；
                    var range2 = {
                        rowspan: samecount,
                        colspan: 1,
                    };
                    ranges[first] = range2;
                    mergRowsArr.push(first);
                    for (var s2 = first + 1; s2 < j; s2++) {
                        ranges[s2] = { colspan: 0, rowspan: 0 };
                        mergRowsArr.push(s2);
                    }
                    //初始化
                }
                first = j;
                preval = val;
                samecount = 1;
            }
        }
        margedRows[col] = mergRowsArr;
        mergetRange[col] = ranges;
    }
    mergRows.value = margedRows;
    mergSpans.value = mergetRange;
    emit("spanbuild", margedRows, mergetRange);
}

function spancolfunc(para) {
    //将所有要合并的单元格位置放到一个数组里面，然后用另一个对象，存储这个合并区域的边界信息。
    //用另一个完全相同长度的数组，存放结果的span对象。
    //为允许同时外界自定义，当外界自定义，但返回null时，将启用自动合并方法
    if (typeof props.spanMethod == "function") {
        var res = props.spanMethod(para, me);
        if (res != null) {
            return res;
        }
    }
    return getspanVal(para.column, para.rowIndex);
}

function getspanVal(col, r) {
    var colname = col.property;
    var tar = mergRows.value[colname];
    if (Array.isArray(tar) && tar.indexOf(r) != -1) {
        var res = mergSpans.value[colname][r];
        //
        return res;
    }
    return [1, 1];
}

const spanfunc = computed(function () {
    if (props.spanCols.length > 0) {
        return spancolfunc;
    }
    if (props.spanMethod != null) {
        return props.spanMethod;
    }
    return null;
});

const tbdata = computed(function () {
    if (face.value == "list") {
        return [];
    }
    var bcdata = props.list;
    if (typeof props.filter == "function") {
        bcdata = props.filter(bcdata);
    }
    buildSpan(bcdata);
    return bcdata;
});

const liveTBHeight = computed(function () {
    return liveHeight.value;
});

const tbheight = computed(function () {
    var he = props.height;
    //console.log(he);//!props.showPagination || 
    if (teleported.value) {
        if (modalHeight.value != null) {
            return modalHeight.value;
        }
        return "100%";
    }

    if (seemodal.value) {
        return liveTBHeight.value;
    }
    if (!!props.height) {
        if (/^\d+$/.test(he)) {
            var h = he * 1 - 41;
            return h;
        }
    }
    if (props.flex) {
        // flex 场景下 liveHeight 可能尚未计算出来，兜底为 100% 避免表格塌成内容高度
        return liveTBHeight.value || "100%";
    }
    // if(!he){
    //     return "100%";
    // }
    return he;
});

function applySelectionByChecked() {
    if (!props.checkable) {
        return;
    }
    nextTick(() => {
        const tbel = table.value;
        if (!tbel || typeof tbel.clearSelection !== "function") {
            return;
        }
        const selected = Array.isArray(checkValues.value) ? checkValues.value : [];
        applyingSelection.value = true;
        suppressEmptySelectionEmit.value = selected.length > 0;
        tbel.clearSelection();
        if (selected.length === 0) {
            setTimeout(() => {
                applyingSelection.value = false;
                suppressEmptySelectionEmit.value = false;
            }, 0);
            return;
        }

        if (!!props.rowKey) {
            const keySet = new Set(selected.map((row) => String(row?.[props.rowKey] ?? "")));
            const rows = Array.isArray(tbdata.value) ? tbdata.value : [];
            rows.forEach((row) => {
                const rowKey = String(row?.[props.rowKey] ?? "");
                if (rowKey && keySet.has(rowKey)) {
                    tbel.toggleRowSelection(row, true);
                }
            });
        } else {
            selected.forEach((row) => {
                tbel.toggleRowSelection(row, true);
            });
        }
        setTimeout(() => {
            applyingSelection.value = false;
            suppressEmptySelectionEmit.value = false;
        }, 0);
    });
}

function clearDragSort() {
    if (dragSortBody.value) {
        dragSortBody.value.destroy();
        dragSortBody.value = null;
    }
}

function tryInitDragSort() {
    if (!props.dragable) {
        clearDragSort();
        return;
    }
    nextTick(() => {
        const t = table.value;
        const root = t && t.$el;
        if (!root) return;

        const ftbody = root.querySelector(".el-table__fixed-body-wrapper tbody");
        const tbody = root.querySelector(".el-table__body-wrapper tbody");
        /* el-table 固定列与主表各有一套 tr；两处同时绑 Sortable 会只拖动一侧 tbody，另一侧不同步，表现为列错位、合并行错乱。只绑一处：优先固定区（拖拽手柄在左侧固定列）。无固定列时用主表体。 */
        const target = ftbody || tbody;
        if (target && !dragSortBody.value) {
            dragSortBody.value = Sortable.create(target, {
                animation: 150,
                handle: ".vc-drag-handle",
                draggable: "tr",
                ghostClass: "vc-drag-ghost",
                onEnd: (evt) => {
                    const oi = evt && typeof evt.oldIndex === "number" ? evt.oldIndex : -1;
                    const ni = evt && typeof evt.newIndex === "number" ? evt.newIndex : -1;
                    if (oi < 0 || ni < 0 || oi === ni) return;
                    /** 供父组件在校验失败时撤回 Sortable 已移动的 tr，与数据顺序恢复一致 */
                    const revertDragDom = () => {
                        try {
                            const from = evt.from;
                            const item = evt.item;
                            if (!from || !item || !from.contains(item)) return;
                            if (oi < ni) {
                                from.insertBefore(item, from.children[oi] || null);
                            } else {
                                from.insertBefore(item, from.children[oi + 1] || null);
                            }
                        } catch (e) {
                            /* noop */
                        }
                    };
                    emit("drag-sort", oi, ni, revertDragDom);
                },
            });
        }
    });
}

//设置头部样式
function handlerCellStyle({ column }) {
    if (props.tableId) {
        setColumnWidth(column, props.tableId);
    }
    return props.cellStyle;
}

function log(p) {
    //console.log(p);
    return p;
}

/**编辑功能 */
function getChangedRows() {
    if (changedIds.value.length == 0) return [];
    var res = changedIds.value.map((id) => {
        return changedRows.value[id];
    });
    return res;
}

function addChange(row) {
    if (typeof props.rowSave == "function") {
        if (!props.lazySave) {
            callSave(row);
        }
        return;
    }
    addToChange(row);
}

//当输入结束时，调用本方法，其它特性与上方法相同
function addChanged(row) {
    if (props.lazySave && typeof props.rowSave == "function") {
        callSave(row);
        return;
    }
}

function addToChange(row) {
    var id;
    if (!!props.rowKey) {
        id = row[props.rowKey];
    } else {
        id = row;
    }
    if (changedIds.value.indexOf(id) == -1) {
        changedIds.value.push(id);
    }
    changedRows.value[id] = row;
}

function removeChange(row) {
    var id;
    if (!!props.rowKey) {
        id = row[props.rowKey];
    } else {
        id = row;
    }
    var index = changedIds.value.indexOf(id);
    if (index != -1) {
        changedIds.value.splice(index, 1);
    }
    changedRows.value[id] = void 0;
}

function callSave(row) {
    props.rowSave(row, function () {
        removeChange(row);
    });
}

function clearChange() {
    changedIds.value = [];
    changedRows.value = {};
}

function getHeadClassName(p) {
    var column = p.column;
    if (column.level == 1) {
        columns.value[column.no] = column;
    }
    columnMap.value[column.id] = column;
    return "vccap";
}

function getLeafColumns() {
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
    readColist(columns.value);
    return res;
}

/**事件 */
function onrowclk(row, col, e) {
    locaBCByRow(row);
    if (props.checkable && props.checkOnRowClick) {
        if (col?.type !== "selection" && !e?.target?.closest?.(".el-checkbox")) {
            const tbel = table.value;
            if (tbel && typeof tbel.toggleRowSelection === "function") {
                tbel.toggleRowSelection(row);
            }
        }
    }
    emit("rowclick", row, col, e);
    emit("update:focusRow", row, col, e);
}

function onrowchange(currow, oldcur) {
    focusRow.value = currow;
    emit("current-change", currow, oldcur);
}

function onexpand(e, state) {
    emit("expand-change", e, state);
}

function onWidthchange(newval, oldval, col, e) {
    //console.log(arguments);
    var fkey = col.property;
    if (fkey == void 0 || fkey == null || fkey == "") return;
    if (!!props.widthKeep) {
        saveVclistWidth({
            vwKey: props.widthKeep,
            vwField: col.property,
            vwWidth: newval,
            vwCaption: col.label,
            vwNo: col.no,
        });
    }
    //saveVclistWidth
    if (props.tableId) {
        headerDragend(newval, oldval, col, props.tableId);
    }
}

function loadVcWidths() {
    if (!!props.widthKeep) {
        loadVclistWidth(props.widthKeep).then((res) => {
            vcWidths.value = res.data;
        });
    }
}

function getCheckValues(fieldName) {
    if (fieldName == void 0) {
        return checkValues.value;
    }
    return checkValues.value.map((obj) => {
        return obj[fieldName];
    });
}

function locaBCByRow(row) {}

function refresh() {
    me.BC.Refresh();
    me.$message("正在刷新数据...");
}

function onselect(val) {
    checkValues.value = val;
    if (applyingSelection.value && suppressEmptySelectionEmit.value) {
        const incoming = Array.isArray(val) ? val : [];
        if (incoming.length === 0) {
            return;
        }
    }
    if (applyingSelection.value && !suppressEmptySelectionEmit.value) {
        return;
    }
    emit("check", val);
    emit("update:checked", val);
}

function setQuery(id, info) {
    console.log("setQuery", id, info);
    queryData.value[id] = info;
    sayQueryChange();
}

function sayQueryChange() {
    var nowCondtions = [];
    if (Array.isArray(props.queryConditions)) {
        nowCondtions = props.queryConditions;
    }
    var old = JSON.stringify(nowCondtions);
    var keys = Object.keys(queryData.value);
    keys.forEach((field) => {
        var has = false;
        var info = queryData.value[field];
        for (var i = 0; i < nowCondtions.length; i++) {
            var item = nowCondtions[i];
            if (item.field == field) {
                item.op = info.op;
                item.value = info.value;
                has = true;
                break;
            }
        }
        if (!has) {
            nowCondtions.push(info);
        }
    });
    var para = fmtAPIConditions(nowCondtions);
    emit("update:queryConditions", para);
}

function fmtAPIConditions(list) {
    return list.map((item) => {
        var val = item.value;
        if (Array.isArray(val)) {
            val = val.join(",");
        }
        return {
            field: item.field,
            op: item.op,
            value: val,
        };
    });
}

function onRunQuery() {
    emit("query");
    //由于前段不具备自行拼接条件的功能。此处直接交由外界处理。
    return;
}


function onQueryBarSearch(text) {
    if(typeof props.fastFilter == "function"){
        var t= props.fastFilter(text);
        if(Array.isArray(t)){
            t.forEach((item) => {
                queryData.value[item.field] = item;
            });
        }
    }
    emit("search", text);
}

/**
 * 清空列筛选条件并刷新列表。
 * @param {string[]|null} fieldsFromBar 若传入（如 querybar 浮层内各字段名），则从 queryConditions 中移除这些 field；否则按当前 queryData 的 key 清理。
 * 说明：setQuery 存入的对象历史上未带 you.resetQuery，原 tar.you.resetQuery() 会抛错导致「重置」无效。
 */
function clearQuery(fieldsFromBar) {
    const drop = new Set(
        Array.isArray(fieldsFromBar) && fieldsFromBar.length
            ? fieldsFromBar
            : Object.keys(queryData.value)
    );
    for (const field of drop) {
        const tar = queryData.value[field];
        if (tar?.you && typeof tar.you.resetQuery === 'function') {
            try {
                tar.you.resetQuery();
            } catch (e) {
                /* ignore */
            }
        }
    }
    const nextQd = { ...queryData.value };
    drop.forEach((f) => {
        delete nextQd[f];
    });
    queryData.value = nextQd;
    let nowCondtions = [];
    if (Array.isArray(props.queryConditions)) {
        nowCondtions = props.queryConditions.filter((item) => item && !drop.has(item.field));
    }
    emit('update:queryConditions', fmtAPIConditions(nowCondtions));
    onRunQuery();
}

function cloneOrderBys(list) {
    if (!Array.isArray(list)) {
        return [];
    }
    return list
        .filter((item) => item && item.field)
        .map((item) => ({
            field: item.field,
            order: item.order === "desc" ? "desc" : "asc",
        }));
}

function captureInitialSort() {
    if (oldSortCaptured.value) {
        return;
    }
    oldOrderBy.value = props.para?.orderByColumn || "";
    oldAsc.value = props.para?.isAsc || "";
    oldOrderBys.value = cloneOrderBys(props.para?.orderBys);
    oldSortCaptured.value = true;
}

function normalizeSortField(sortBy, prop) {
    var field = sortBy || prop || "";
    if (Array.isArray(field)) {
        field = field[0] || "";
    }
    if (typeof field !== "string") {
        field = String(field || "");
    }
    return field.trim();
}

function applySortPara(newval) {
    if (props.para && typeof props.para === "object") {
        Object.assign(props.para, newval);
    }
    emit("update:para", newval);
}

function sortbc(e) {
    if (!props.para || !e || !e.column) return;
    captureInitialSort();

    var sortField = normalizeSortField(e.column.sortBy, e.prop);
    if (!sortField) return;

    var newval = Object.assign({}, props.para);
    newval.pageNum = 1;
    if (e.order == "ascending" || e.order == "descending") {
        var order = e.order == "descending" ? "desc" : "asc";
        newval.orderByColumn = sortField;
        newval.isAsc = order;
        newval.orderBys = [{ field: sortField, order }];
    } else {
        newval.orderByColumn = oldOrderBy.value;
        newval.isAsc = oldAsc.value;
        newval.orderBys = cloneOrderBys(oldOrderBys.value);
        if (newval.orderBys.length === 0 && oldOrderBy.value) {
            newval.orderBys = [{
                field: oldOrderBy.value,
                order: oldAsc.value === "desc" ? "desc" : "asc",
            }];
        }
    }
    applySortPara(newval);
    nextTick(() => {
        emit("query", newval);
        me.$message("正在排序...");
    });
}

function loadHeightInfo() {
    var tbel = table.value;
    if (props.maxable && !seemodal.value && tbel != void 0) {
        var h = getComputedStyle(tbel.$el).height;
        if (props.showPagination && teleported.value == false && liveHeight.value != h) {
            liveHeight.value = h;
        }
    }
    if (seemodal.value && groot.value != void 0) {
        var sh = groot.value.scrollHeight;
        if (sh != modalHeight.value) {
            modalHeight.value = sh;
        }
    }
    if (sroot.value != void 0) {
        var sh2 = sroot.value.clientHeight;
        if (sh2 != rootHeight.value) {
            rootHeight.value = sh2;
        }
    }
}

function toggle(val) {
    if (typeof val == "boolean") {
        if (val) {
            openTomodal();
        } else {
            closeMoald();
        }
    }
}

function refreshLayout() {
    loadHeightInfo();
    nextTick(() => {
        const tbel = table.value;
        if (tbel && typeof tbel.doLayout === "function") {
            tbel.doLayout();
        }
    });
}

function openTomodal() {
    loadHeightInfo();
    seemodal.value = true;
    nextTick(() => {
        openTele();
    });
}

function closeMoald() {
    teleported.value = false;
    nextTick(() => {
        seemodal.value = false;
    });
}

function openTele() {
    var goot = groot.value;
    if (goot != void 0) {
        modalRoot.value = goot;
        teleported.value = true;
    } else {
        nextTick(() => {
            openTele();
        });
    }
}

function exportByEle() {
    try {
        var sheet = getSheetByConfig();
        wrapExcelWiths(sheet);
        openDownloadDialog(sheetToBlob(sheet), "下载.xlsx");
    } catch (e) {
        console.log(e);
    }
}

function wrapExcelWiths(sheet) {
    var cols = getLeafColumns();
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
}

function getSheetByConfig() {
    var sheet = {};
    if (props.exconfig == void 0) {
        var tbel = table.value.$el;
        sheet = XLSX.utils.table_to_sheet(tbel);
        return sheet;
    }
    var config = props.exconfig;
    if (config.mode == void 0 || config.mode == "append") {
        sheet = XLSX.utils.table_to_sheet(table.value.$el);
        config.startColumn = columns.value.length + 1;
        writeSheetColumnsConfig(sheet, props.list, getLeafColumns(), config);
    } else if (config.mode == "replace") {
        writeSheetColumnsConfig(sheet, props.list, getLeafColumns(), config);
    }
    return sheet;
}

function exportByData() {
    var rows = props.list;
    var sheet = exportVueTable(rows);
    const wb = { SheetNames: ["Sheet1"], Sheets: {}, Props: {} };
    wb.Sheets["Sheet1"] = sheet;

    var flname = "评先评优汇总表.xlsx";
    XLSX.writeFile(wb, flname, { bookType: "xlsx", bookSST: false });

    function exportVueTable(sends) {
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

        sheet["A2"] = {
            t: "s",
            v: "序号",
            s: Object.assign({}, baseColStyle),
        };

        var mergeObj = {
            s: { r: 1, c: 0 },
            e: { r: 2, c: 0 },
        };
        merges.push(mergeObj);

        var startrow = 4;
        for (var i = 0; i < sends.length; i++) {
            var send = sends[i];
            var rows = 1;
            var max = Math.max(send.gootpt1.length, send.gootpt2.length, send.gootpt3.length, send.gootpw.length);
            if (send.rows > 1) {
                rows = send.rows;
            }
            sheet["A" + startrow] = {
                t: "s",
                v: i + 1,
                s: Object.assign({}, baseColStyle),
            };

            for (var o = 0; o < rows; o++) {
                sheet["A" + (startrow + o + 1)] = {
                    t: "s",
                    v: "--",
                    s: Object.assign({}, baseColStyle),
                };
            }

            mergeObj = {
                s: { r: startrow - 1, c: 0 },
                e: { r: startrow + rows - 2, c: 0 },
            };
            merges.push(mergeObj);

            sheet["B" + startrow] = {
                t: "s",
                v: send.SO_Name,
                s: Object.assign({}, baseColStyle),
            };

            mergeObj = {
                s: { r: startrow - 1, c: 1 },
                e: { r: startrow + rows - 2, c: 1 },
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
            for (var c = 0; c < cols.length; c++) {
                var colobj = cols[c];
                var colNum = "";
                if (c < colCode.length) {
                    colNum = colCode[c];
                } else {
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
                    t: "s",
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
}

watch(
    () => props.dragable,
    () => {
        tryInitDragSort();
    },
    { immediate: true }
);

watch(
    () => props.widthKeep,
    () => {
        loadVcWidths();
    },
    { immediate: true }
);

watch(
    () => props.checked,
    (val, oldVal) => {
        if (Array.isArray(val)) {
            if (JSON.stringify(val) == JSON.stringify(checkValues.value)) {
                return;
            } else {
                checkValues.value = val;
                applySelectionByChecked();
            }
        }
    },
    { immediate: true }
);

watch(
    () => props.list,
    () => {
        applySelectionByChecked();
    },
    { deep: true }
);

onMounted(() => {
    tryInitDragSort();
});

onUpdated(() => {
    if (!props.flex) {
        scheduleLoadHeightInfo();
    }
    tryInitDragSort();
});

onBeforeUnmount(() => {
    if (heightInfoRaf != null) {
        cancelAnimationFrame(heightInfoRaf);
        heightInfoRaf = null;
    }
    clearDragSort();
});

// script setup 内的方法不会自动挂到 proxy，inject 子组件（vccol/queryhead）需显式挂载
me.setQuery = setQuery;
me.onRunQuery = onRunQuery;
me.clearQuery = clearQuery;
me.addChange = addChange;
me.addChanged = addChanged;
me.sayQueryChange = sayQueryChange;
me.queryData = queryData;
me.registerField = registerField;
Object.defineProperty(me, "activeCell", {
    enumerable: true,
    configurable: true,
    get() { return editState.activeCell; },
    set(v) { editState.activeCell = v; },
});

provide("vclist", me);

defineExpose({
    getChangedRows,
    toggle,
    clearChange,
    getCheckValues,
    getLeafColumns,
    setQuery,
    onRunQuery,
    clearQuery,
    registerField,
    queryData,
    refreshLayout,
});
</script>



<style>
.vclist-max-btn {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    vertical-align: middle;
    cursor: pointer;
}
.vclist-max-icon {
    width: 16px;
    height: 16px;
    flex-shrink: 0;
}
.nowrapcell.cell{
    white-space: nowrap;
    word-wrap: normal;
}
.vclist .vccap{
    font-size:14px;
    font-weight: bold;
}
.vclist th.is-left .vccap {
    justify-content: flex-start;
}
.vclist th.is-center .vccap {
    justify-content: center;
}
.vclist th.is-right .vccap {
    justify-content: flex-end;
}

.vc-card-list{
    overflow: auto;
}



/* 表头底色交给 global.css 的 .vccap / .vchead.noquery（白底深色字） */
/* 表头内边距归零，避免与 queryhead 固定行高叠加导致列间错位 */
.vclist .el-table__header th > .cell {
    padding-top: 0;
    padding-bottom: 0;
    line-height: normal;
}
.vcqyrwse>.el-input>.el-input__inner{
    display:none;
}
.vcqyrwse>.el-input>.el-input__suffix{
    display:none;
}
.vcqyrwsedit .el-input__icon{
    line-height:26px;
}
.vcqyrwse{
    flex: 0 0 20px;
    min-width: 20px;
    min-height:14px;
    position: absolute;
    right: 1px;

}
.vcqyrwpp>div>.el-scrollbar__wrap{

    max-height:600px;
}
.vcqyrwpp .el-select-dropdown__item{
    height:27px;
    line-height:26px;
}
.el-table th{
    padding:0 0 0 0;
}
/* 列宽拖拽：在 th 右缘保留透明热区，不缩小 .cell，避免标题灰底断层 */
.vclist.el-table th.el-table__cell {
    position: relative;
}
.vclist.el-table th.el-table__cell::after {
    content: "";
    position: absolute;
    top: 0;
    right: 0;
    width: 8px;
    height: 100%;
    cursor: col-resize;
    z-index: 3;
    background: transparent;
}
.vclist.el-table th.el-table__cell .caret-wrapper {
    display: none;
}
.vcqyrwsedit.seeditpa{
    padding:0;
    grid-column: 1/-1;
    display: flex;
    align-items: center;
}
.vcqyrwsedit .el-input__inner{
    height:26px;
    line-height:26px;
}
.el-table th > .cell.vcthead {
    display: flex;
    align-items: center;
    justify-content: center;
    padding-left: 0;
    padding-right: 0;
    box-sizing: border-box;
}
/* 仅带列筛选的表头才使用 grid，避免普通/分组表头被强行撑高 */
.el-table th > .cell.vcthead.qy {
    display: grid;
    grid-template-columns: minmax(0, 1fr) auto;
    column-gap: 4px;
}

.vclist.el-table{
    font-size: 14px;
    border-radius: 14px;
    border: 1px solid #e4e7ed;
    --el-table-text-color: #333;
}

.vclist .bclinkcell{
    color:#3163a5;
    text-decoration: underline;
    cursor: pointer;
}
.vcthead .vccap{
    z-index:1;
}
.vcflexed.vclistwrap{
    display: flex;
    flex-direction: column;
}
.vclist-inner-flex{
    display: flex;
    flex-direction: column;
    height: 100%;
    min-height: 0;
    position: relative;
}
.vclist-inner-flex>.el-table.vclist{
    flex:1 1 0;
    min-height: 0;
}
.vclist-inner > .vclist-notable-wrap{
    flex: 1 1 0;
    min-height: 0;
    display: flex;
    flex-direction: column;
    overflow: hidden;
}
.vclist-inner > .el-pagination.vcpager{
    flex: 0 0 auto;
}
.vclist-inner-flex>.el-pagination.vcpager{
    flex: 0 0 auto;
}
.el-table .pangcell>.cell{
    padding-left: 1px;
    padding-right:1px;
}

.nowrapcell>.cell>.el-table__expand-icon+.vcelval{
    display:inline-block;
}

.vclist  th.vcthead.vccap{
    padding-top:0;
    padding-bottom:0;
}
.vclist  th.vcthead.vccap>.cell{
    padding-top:0;
    padding-bottom:0;
    padding-left: 0;
    padding-right: 0;
}

.vclist .vc-drag-col .cell{
    padding-left: 0;
    padding-right: 0;
}
.vclist .vc-drag-handle{
    display:inline-block;
    width: 24px;
    height: 20px;
    line-height: 20px;
    border-radius: 6px;
    cursor: move;
    user-select: none;
    color: #909399;
    opacity: 1;
}
.vclist .vc-drag-ghost{
    opacity: .6;
}

/* 与学员考勤 VC_PX_StuCheckEditList 的 .stu-check-empty-standalone* 同比例、同字色，便于各列表空态一致 */
.vc-nodatelist-empty {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    box-sizing: border-box;
    width: 100%;
    min-height: min(56vh, 420px);
    padding: 16px 12px 28px;
    gap: 12px;
    background: transparent;
}
.vc-nodatelist-empty__img {
    width: min(72%, 380px);
    max-width: 100%;
    height: auto;
    max-height: min(36vh, 280px);
    object-fit: contain;
    display: block;
}
.vc-nodatelist-empty__text {
    margin: 0;
    font-size: 14px;
    color: #909399;
}
/* 空表区域底与表体一致 */
.vclist.el-table .el-table__empty-block {
    background-color: #fff;
}

/**卡片的样式撑开功能 */
.vcard-pan{
    display: flex;
    flex-direction: column;
}
</style>
