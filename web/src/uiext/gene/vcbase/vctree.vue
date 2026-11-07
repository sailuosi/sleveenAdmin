<template>
<div>
    <div >
        <el-tree ref="eltree"  :empty-text="emptymsg" :props="treeKey" node-key="PKValue" 
        :default-expanded-keys="expandedKeys" :default-checked-keys="checked" @node-click="onNodeClick" @check="orgcheck" 
        :show-checkbox="showCheck" :expand-on-click-node="false" :check-strictly="strict"
            :load="loadOrg" lazy
            :filter-node-method="filterNode"
            @node-expand="onNodeExpand($event,'expand')"
            @node-collapse="onNodeExpand($event,'collapse')"
        >
        <template #default="p">
            <div
                :class="{
                    'node-content': true,
                    'node-selected': isNodeSelected(p.data),
                }"
            >
                <slot v-bind="p" name="node" :row="p.data.Record"></slot>
            </div>
        </template>
        </el-tree>
    </div>
</div>

</template>

<script setup>
/***
 * 本组件为基础组件，因此高频自定义的内容，全部向外开放
 * 功能：
 * 1. 层次展开支持，按层次展示
 * 2. 搜索支持，前端搜索，过滤具体方式暴露给调用方
 * 3. 所有常用事件，直接向外抛出，
 * 4. 选择支持，持有选择状态，并支持单选、多选。 

 */


import { ref, watch, onMounted, nextTick } from 'vue';


const props = defineProps({
    checked: {
        type: Array,
    },
    showCheck: {
        type: Boolean,
        default: false
    },
    canfatherClick: {
        type: Boolean,
        default: true
    },
    strict: {
        type: Boolean,
        default: false
    },
    deptOID: {
        type: String,
        default: ""
    },
    bindPage: null,
    authcode: null,
    useAuth: {
        type: Boolean,
        default: false
    },
    expandLevel: { type: Number, default: 1 },
    expandAuthPathOnly: { type: Boolean, default: false },
    unitname: { type: String, default: "" },
    filterNode: {
        type: Function,
        default: (value, data, node) => {
            if (!value) return true;
            return data.label.indexOf(value) !== -1;
        }
    },

    //加载节点数据
    load:null,
    pk: {
        type: String,
        default: ""
    },
    fk: {
        type: String,
        default: ""
    },
    hideSearch: {
        type: Boolean,
        default: false
    },

})

const emit = defineEmits(["nodeclick", "focuschange", "orgcheck", "toggle","update:focusRow"])

// 响应式数据
const filterText = ref("")
const isTreeHidden = ref(false)
const searchResults = ref([])
const rows = ref([])
const orgData = ref({})
const focusRow = ref({})
const expandedKeys = ref([])
const emptymsg = ref("正在加载组织信息...")
const treeKey = ref({
    children: 'children',
    label: 'OrgName'
})
const eltree = ref(null)

// 生命周期钩子
onMounted(() => {
    // this.loadTreeData();
})

// 监听
watch(() => props.unitname, (newVal, oldVal) => {
    if (newVal !== oldVal) {
        // this.reloadTreeData();
    }
}, { deep: true, immediate: true })



function onNodeExpand(e, type) {
    emit("toggle", e, type);
}

function shouldAutoExpand(node, item) {
    if (props.expandAuthPathOnly) {
        return item?.Record?._authNavOnly === true;
    }
    return node.level < props.expandLevel;
}

function loadOrg(node, resolve, reject) {
    // 如果节点已有子节点数据，直接使用
    if(Array.isArray(node.data.Children) && node.data.Children.length>0){
        var chid=node.data.Children;
        if (node.level < props.expandLevel) {
            chid.forEach((o) => {
                if (shouldAutoExpand(node, o)) expandedKeys.value.push(o.PKValue);
            })
        }
        resolve(chid);
        return;
    }
    // 懒加载子节点
    props.load(node.data?.Record,node,node.data).then(res => {
        const list = Array.isArray(res) ? res : [];
        if (node.level === 0 && list.length === 0) {
            emptymsg.value = "暂无组织信息";
        }
        if (node.level < props.expandLevel) {
            list.forEach((o) => {
                if (shouldAutoExpand(node, o)) expandedKeys.value.push(o.PKValue);
            })
        }
        resolve(list);
    }).catch((err) => {
        console.error('加载树节点失败:', err);
        if (node.level === 0) {
            emptymsg.value = "暂无组织信息";
        }
        resolve([]);
    })
}

function treeNodeKey(data) {
    if (!data || typeof data !== 'object') return '';
    const v = data.PKValue != null ? data.PKValue : data.Record && data.Record.PKValue;
    return v == null ? '' : String(v);
}

function pickPkField(data, key) {
    if (!data || !key) return undefined;
    const top = data[key];
    if (top != null) return top;
    return data.Record && data.Record[key];
}

function isNodeSelected(data) {
    if (!focusRow.value || !data) return false;
    const pk = props.pk && String(props.pk).trim() !== '' ? props.pk : null;
    if (pk) {
        const a = pickPkField(focusRow.value, pk);
        const b = pickPkField(data, pk);
        return a != null && b != null && String(a) === String(b);
    }
    const a = treeNodeKey(focusRow.value);
    const b = treeNodeKey(data);
    return a !== '' && b !== '' && a === b;
}

function scrollToNode(key) {
    const oid = key != null && String(key).trim() !== '' ? String(key) : props.deptOID;
    nextTick(() => {
        if (!eltree.value || !oid) return;
        const node = eltree.value.getNode(oid);
        if (!node) return;
        var nodeEl = eltree.value.$el.querySelector(`.el-tree-node[data-key="${oid}"]`);
        if (nodeEl) {
            nodeEl.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    });
}

/** 由外部设置当前聚焦节点（用于弹窗按班级科目 OID 同步左侧树选中态，无需用户先点击节点） */
function assignFocusRow(tar) {
    if (!tar || typeof tar !== 'object') return;
    focusRow.value = tar;
    emit("update:focusRow", tar);
}

function expandNodeParents(node) {
    let p = node?.parent;
    while (p && p.level > 0) {
        if (!p.expanded && typeof p.expand === 'function') {
            p.expand();
        }
        p = p.parent;
    }
}

/**
 * 定位到指定 node-key：高亮选中、展开父级并滚动到可视区域。
 * options.expandPath(eltree) 可在节点尚未懒加载时先展开路径。
 */
async function locateByKey(key, options = {}) {
    if (key == null || String(key).trim() === '') return false;
    const keyStr = String(key);
    const focusTar = options.focusTar && typeof options.focusTar === 'object'
        ? options.focusTar
        : { PKValue: keyStr };
    assignFocusRow(focusTar);

    const tee = eltree.value;
    if (!tee) return false;

    let node = tee.getNode(keyStr);
    if (!node && typeof options.expandPath === 'function') {
        await options.expandPath(tee);
        node = tee.getNode(keyStr);
    }
    if (!node) return false;

    expandNodeParents(node);
    if (typeof tee.setCurrentKey === 'function') {
        tee.setCurrentKey(keyStr);
    }
    await nextTick();
    scrollToNode(keyStr);
    return true;
}

function onNodeClick(tar, node, me) {
    if (!props.canfatherClick) {
        return;
    }
    const usePk = props.pk && String(props.pk).trim() !== '';
    const prevKey = focusRow.value
        ? (usePk ? pickPkField(focusRow.value, props.pk) : treeNodeKey(focusRow.value))
        : '';
    const nextKey = usePk ? pickPkField(tar, props.pk) : treeNodeKey(tar);
    if (String(prevKey ?? '') !== String(nextKey ?? '')) {
        emit("focuschange", tar);
    }
    focusRow.value = tar;
    emit("update:focusRow", tar);
    emit("nodeclick", tar.Record, tar, node, me);
}



function orgcheck(e, n) {
    emit("orgcheck", e, n);
}

function filterNodes() {
    if (filterText.value.trim() === "") {
        isTreeHidden.value = false;
        searchResults.value = [];
    } else {
        isTreeHidden.value = true;
        searchOrg(filterText.value);
    }
}



function searchOrg(filterText) {
    let params = {
        OrgName: filterText
    };
    queryorglist(params).then((res) => {
        searchResults.value = res.data;
        
    });
}

function handleRowClick(row) {
    emit("nodeclick", row);
}

function insertBefore(data,refNode){
    if (eltree.value) {
        eltree.value.insertBefore(data, refNode);
    }
}
function insertAfter(data,refNode){ 
    if (eltree.value) {
        if(refNode==null){
            refNode=eltree.value.root;
            //eltree.value.root.childNodes.push(data);
            //return;
        }
        eltree.value.insertAfter(data, refNode);
    }
}
function  append(data,parent){
    if (eltree.value) {
        eltree.value.append(data, parent);
    }
}
function remove(data){
    if (eltree.value) {
        eltree.value.remove(data);
    }
}
const refreshNode = (oid, isself) => {
    const tee = eltree.value;
    if (!tee || !tee.store) return;

    if (isself === true && oid === 0) {
        // 清空根节点数据
        tee.store.root.childNodes = [];
        // 重置根节点状态
        tee.store.root.loaded = false;
        tee.store.root.isLeaf = false;
        tee.store.root.expand();
        return;
    }

    // 使用 node-key 的值来查找节点（node-key="PKValue"）
    const node = tee.getNode(oid);
    
    if (!node || node === null) {
        // 如果节点不存在，刷新根节点
        if (tee.store.root) {
            tee.store.root.childNodes = [];
            tee.store.root.loaded = false;
            tee.store.root.isLeaf = false;
            tee.store.root.expand();
        }
        return;
    }
    
    // 清空子节点，强制重新加载
    if (node.childNodes && node.childNodes.length > 0) {
        node.childNodes = [];
    }
    node.loaded = false;    // 标记为未加载
    node.isLeaf = false;   // 重置叶子节点状态
    
    // 如果节点已展开，先收起再展开，确保触发重新加载
    if (node.expanded) {
        node.collapse();
        setTimeout(() => {
            node.expand();
        }, 100);
    } else {
        node.expand();          // 重新展开（触发 loadNode）
    }
};

/** 按关键字过滤树节点（依赖 filterNode；Element Plus el-tree.filter） */
function filterTree(keyword) {
    nextTick(() => {
        if (eltree.value && typeof eltree.value.filter === "function") {
            eltree.value.filter(keyword == null ? "" : String(keyword));
        }
    });
}

// 方法定义
defineExpose({
    scrollToNode,
    locateByKey,
    insertBefore,
    insertAfter,
    append,
    remove,
    refreshNode,
    assignFocusRow,
    filterTree,
    getElTree: () => eltree.value,
})
</script>

<style scoped>
.partyname {

    font-size: 14px;
}
.hr-tree-node{
    font-size:12px;
}
/* 选中 / 悬浮样式 */
.node-content {
    flex: 1;
    min-width: 0;
    width: 100%;
    padding: 2px 8px;
    box-sizing: border-box;
}
:deep(.el-tree-node__content:has(.node-selected)) {
    background-color: var(--el-color-primary-light-4);
    border-radius: 4px;
}
:deep(.el-tree-node__content:hover:not(:has(.node-selected))) {
    background-color: var(--el-color-primary-light-5);
    border-radius: 4px;
}

.partlev5 {
    color: #0D86D5;
}

.partlev10 {
    color: #53C1C1;
}

.partlev15 {
    color: #B19C4A;
}

.partlev20 {
    color: #5DB0E8;
}

.partlev25 {
    color: #00B474;
}

.partlev30 {
    color: #B19C4A;
}
</style>
