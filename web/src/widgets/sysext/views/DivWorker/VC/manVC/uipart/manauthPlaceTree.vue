<template>
    <div class="manauth-place-tree" :style="{ height: height || '420px' }">
        <vctree
            ref="vctreeRef"
            :load="loadNode"
            pk="MD_TreeOID"
            fk="MD_ParentOID"
            :expandLevel="expandLevel"
            :filterNode="customFilterNode"
            :showCheck="true"
            :strict="true"
            @orgcheck="onOrgCheck"
            @update:focusRow="onNodeFocus"
        >
            <template #node="nd">
                <span v-if="nd.data.isVirtualFolder" class="virtual-folder-label">{{ nd.row.MD_Name }}</span>
                <span v-else :title="nd.row.MD_Name">{{ nd.row.MD_Name }}</span>
            </template>
        </vctree>
    </div>
</template>

<script setup>
import { ref } from "vue";
import vctree from "@/uiext/gene/vcbase/vctree.vue";

const props = defineProps({
    api: { type: Object, required: true },
    expandLevel: { type: Number, default: 0 },
    height: { type: String, default: "420px" },
    bpo:null,
});

const emit = defineEmits(["check-change", "node-click"]);

const vctreeRef = ref(null);

const customFilterNode = (value, data) => {
    if (!value) return true;
    const label = data.Record?.MD_Name || "";
    return label.toLowerCase().includes(value.toLowerCase());
};

function groupIntoVirtualFolders(nodes, parentRow) {
    const parentClass = parentRow?.MD_Class;
    if (parentClass === "7" || parentClass === "8") {
        return nodes;
    }

    const type6Nodes = [];
    const type7Nodes = [];
    const type8Nodes = [];
    const otherNodes = [];

    for (const node of nodes) {
        const mdClass = node.Record?.MD_Class;
        if (mdClass === "6") {
            type6Nodes.push(node);
        } else if (mdClass === "7") {
            type7Nodes.push(node);
        } else if (mdClass === "8") {
            type8Nodes.push(node);
        } else {
            otherNodes.push(node);
        }
    }

    if (type6Nodes.length === 0 && type7Nodes.length === 0 && type8Nodes.length === 0) {
        return nodes;
    }

    const result = [...otherNodes];
    if (type6Nodes.length > 0) {
        result.push(createVirtualFolder("巷道", "6", type6Nodes, parentRow));
    }
    if (type7Nodes.length > 0) {
        result.push(createVirtualFolder("硐室", "7", type7Nodes, parentRow));
    }
    if (type8Nodes.length > 0) {
        result.push(createVirtualFolder("地点", "8", type8Nodes, parentRow));
    }
    return result;
}

function createVirtualFolder(name, type, children, parentRow) {
    const parentPK = parentRow?.MD_TreeOID || parentRow?.PKValue || "root";
    return {
        PKValue: "virtual_" + type + "_" + parentPK,
        Record: {
            MD_Name: name,
            MD_Class: type,
            MD_TreeOID: "virtual_" + type + "_" + parentPK,
        },
        isVirtualFolder: true,
        Children: children,
        leaf: children.length === 0,
        disabled: true,
    };
}

const loadNode = (row) => {
    const para = {
        pk: null,
        deep: 2,
        BPOName: props.bpo,
    };
    if (row != null) {
        para.pk = row.MD_TreeOID;
    }
    return props.api.queryBCMDTreeTree(para).then((res) => {
        if (res.Nodes && Array.isArray(res.Nodes)) {
            res.Nodes.sort((a, b) => {
                const classA = a.Record?.MD_Class || "";
                const classB = b.Record?.MD_Class || "";
                const classCompare = classA.localeCompare(classB);
                if (classCompare !== 0) {
                    return classCompare;
                }
                const idxA = a.Record?.MD_Idx ?? 0;
                const idxB = b.Record?.MD_Idx ?? 0;
                return idxA - idxB;
            });
        }
        return groupIntoVirtualFolders(res.Nodes, row);
    });
};

function onOrgCheck() {
    emit("check-change");
}

function onNodeFocus(tar) {
    if (!tar || tar.isVirtualFolder) return;
    emit("node-click", tar);
}

function getSelectableCheckedNodes() {
    const nodes = vctreeRef.value?.getCheckedNodes() || [];
    return nodes.filter((n) => !n.isVirtualFolder && n.Record?.MD_TreeOID && !String(n.Record.MD_TreeOID).startsWith("virtual_"));
}

function getCheckedKeys() {
    return vctreeRef.value?.getCheckedKeys() || [];
}

function setCheckedKeys(keys) {
    vctreeRef.value?.setCheckedKeys(keys);
}

function clearChecked() {
    vctreeRef.value?.setCheckedKeys([]);
}

function getRootNodeKeys() {
    const eltree = vctreeRef.value?.eltree;
    if (!eltree?.store?.root?.childNodes) {
        return [];
    }
    return eltree.store.root.childNodes
        .filter((n) => !n.data?.isVirtualFolder && n.data?.PKValue)
        .map((n) => n.data.PKValue);
}

function isAllRootChecked() {
    const rootKeys = getRootNodeKeys();
    if (rootKeys.length === 0) return false;
    const checked = getCheckedKeys();
    return rootKeys.every((k) => checked.includes(k));
}

function toggleAllRoot(checked) {
    const rootKeys = getRootNodeKeys();
    if (rootKeys.length === 0) return;
    const current = getCheckedKeys();
    if (checked) {
        setCheckedKeys([...new Set([...current, ...rootKeys])]);
    } else {
        setCheckedKeys(current.filter((k) => !rootKeys.includes(k)));
    }
    emit("check-change");
}

defineExpose({
    getSelectableCheckedNodes,
    getCheckedKeys,
    setCheckedKeys,
    clearChecked,
    getRootNodeKeys,
    isAllRootChecked,
    toggleAllRoot,
});
</script>

<style scoped>
.manauth-place-tree {
    width: 100%;
    min-height: 320px;
    display: flex;
    flex-direction: column;
}
.manauth-place-tree :deep(.vctree-container) {
    height: 100%;
    flex: 1;
    min-height: 0;
}
.virtual-folder-label {
    font-weight: 500;
    color: #5e6164;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 4px;
}
.virtual-folder-label::before {
    content: "📂";
    font-size: 14px;
}
</style>
