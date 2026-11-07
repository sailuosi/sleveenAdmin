<template>
<div class="menubind-wrap">
    <div class="menubind-toolbar">
        <el-button type="primary" @click="saveChecked" :disabled="loading || saveDisabled">保存</el-button>
        <el-switch v-model="cascadeCheck" active-text="级联选择" inactive-text="独立选择" />
    </div>
    <div class="menubind-tree">
        <!-- 始终 check-strictly：内置级联一切换就会改写已有勾选，无法做到「只影响之后新操作」 -->
        <el-tree v-loading="loading"
            ref="treeRef"
            :data="menuData"
            check-strictly
            node-key="Id"
            show-checkbox
            :props="{ children: 'Children', label: 'Title', class: 'menu-ck' }"
            highlight-current
            :default-expand-all="defaultExpandAll"
            :default-expanded-keys="defaultExpandedKeys"
            :default-checked-keys="bindOIDs"
            @check-change="onCheckChange"/>
    </div>
</div>
</template>

<script>

import { apiSysMenuListGet } from '@s/views/menu/api/sysmenu.js';
import { loadBindKeys,saveBindKeys } from "../../api/goodsitemapi.js"

export default {
    name: 'WebMenubind',
    props: {
        row: null,
        /** 默认展开全部；为 false 时仅展开根节点 */
        defaultExpandAll: { type: Boolean, default: true },
    },
    data() {
        return {
            menuData: [],
            bindVals: [],
            bindOIDs: [],
            loading: false,
            saveDisabled: false,
            /** true=勾选时手动级联子孙；false=纯独立。切换本身不改已有勾选 */
            cascadeCheck: false,
            /** 手写级联时跳过子节点触发的 check-change，避免递归 */
            cascading: false,
        };
    },
    computed: {
        defaultExpandedKeys() {
            if (this.defaultExpandAll) {
                return [];
            }
            return (this.menuData || []).map((n) => n.Id).filter((id) => id != null);
        },
    },
    watch: {
        "row": function() {
            this.loadChecked();
        }
    },
    mounted() {
        this.loadMenu();
        this.loadChecked();
    },
    methods: {
        loadMenu() {
            apiSysMenuListGet().then((res)=>{
                this.menuData = res?.data?.result ?? res ?? [];
                this.$nextTick(() => {
                    if (!this.defaultExpandAll) {
                        const keys = (this.menuData || []).map((n) => n.Id).filter((id) => id != null);
                        this.$refs.treeRef?.setExpandedKeys?.(keys);
                    }
                    if (this.bindOIDs?.length) {
                        this.$refs.treeRef?.setCheckedKeys?.(this.bindOIDs);
                    }
                });
            });
        },
        loadChecked(){
        	this.loading = true;
        	
            this.$refs.treeRef?.setCheckedKeys?.([]);

            var para = {
                HH_GoodsBag_FK: this.row.HH_GoodsBagOID,
                Gi_Type: "1"
            };

            loadBindKeys(para).then((res) => {
                const rows = res?.data ?? res ?? [];
                this.bindVals=rows;
                var oids=rows.map((li)=>li.Gi_OID);
                this.bindOIDs=oids;
                this.$nextTick(() => {
                    this.$refs.treeRef?.setCheckedKeys?.(oids ?? []);
                });
            }).finally(() => {
            	this.loading = false;
            });
        },
        saveChecked() {
        	this.saveDisabled = true;
            this.bindOIDs = this.$refs.treeRef?.getCheckedKeys?.() ?? this.bindOIDs;
            var para = {
                HH_GoodsBag_FK: this.row.HH_GoodsBagOID,
                Gi_Type: "1",
                checkMenus: this.bindOIDs,
            };

            saveBindKeys(para).then((res) => {
                this.$message(res?.data ?? res ?? "保存成功")
            }).finally(() => {
            	this.saveDisabled = false;
            });
        },
        onCheckChange(data, checked) {
            if (this.cascading) return;

            if (this.cascadeCheck) {
                const tree = this.$refs.treeRef;
                const node = tree?.getNode?.(data?.Id ?? data);
                if (node?.childNodes?.length) {
                    this.cascading = true;
                    try {
                        this.setDescendantsChecked(node, checked);
                    } finally {
                        this.cascading = false;
                    }
                }
            }

            this.bindOIDs = this.$refs.treeRef?.getCheckedKeys?.() ?? [];
        },
        setDescendantsChecked(node, checked) {
            const tree = this.$refs.treeRef;
            if (!tree || !node) return;
            (node.childNodes || []).forEach((child) => {
                tree.setChecked(child.key, checked, false);
                this.setDescendantsChecked(child, checked);
            });
        },
    }
};
</script>

<style scoped>
.menubind-wrap {
    height: 100%;
    min-height: 240px;
    max-height: 100%;
    display: flex;
    flex-direction: column;
    overflow: hidden;
    box-sizing: border-box;
}
.menubind-toolbar {
    position: sticky;
    top: 0;
    flex: 0 0 auto;
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding-bottom: 8px;
    background: var(--el-bg-color, #fff);
    z-index: 2;
}
.menubind-tree {
    flex: 1 1 0;
    min-height: 0;
    overflow: auto;
}
</style>
