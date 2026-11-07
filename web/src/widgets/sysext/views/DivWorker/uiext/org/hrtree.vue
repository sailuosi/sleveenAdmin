<template>
    <el-input
        v-if="showSearch"
        v-model="filterText"
        style="width: 240px;"
        placeholder="请输入单位名称..."
        @input="filterNodes"
    />
    <div v-show="!isTreeHidden" class="h-[600px] overflow-y-scroll"  >
        <el-tree ref="eltree" highlight-current :empty-text="emptymsg" :props="treeKey" node-key="HR_OrganizeOID" 
        :default-expanded-keys="expandedKeys" :default-checked-keys="checked" @node-click="onNodeClick" @check="orgcheck" 
        :show-checkbox="showCheck" :expand-on-click-node="false" :check-strictly="strict"
            :load="loadOrg" lazy
            :filter-node-method="filterNode"
            @node-expand="onNodeExpand($event,'expand')"
            @node-collapse="onNodeExpand($event,'collapse')"
        >
        <template #default="p">
            <span class="hr-tree-node" >
                <span :class="getPartyIcon(p)"></span>
                <span class="myonode">{{ p.node.label }}</span>
            </span>    
        </template>
        </el-tree>
    </div>
    <div v-show="isTreeHidden">
        <el-table :data="searchResults" style="width: 100%" @row-click="handleRowClick">
            <el-table-column prop="OrgName" label="单位名称">
                <template #default="p">
                    <span class="hr-tree-node" >
                        <div class="myonode" style="color:#333;font-weight: 700;font-size: 16px;">{{ p.row.OrgName }}</div>
                        <div class="myonode" style="color:#777;font-size: 12px;">{{ p.row.PathName }}</div>
                    </span>    
                </template>
            </el-table-column>
            <!-- 其他需要展示的字段 -->
        </el-table>
    </div>

</template>

<script>
import { ElMessage } from 'element-plus';
import { loadOrgTree, queryorglist } from "@s/api/system/mancommonapi.js"
import { loadFastPutOrgPanelAncestorPath, loadFastPutOrgPanelOrgLinkBinds, loadFastPutOrgPanelSearch } from "../../api/fastorgputapi.js"
import { ORG_LEAF_ONLY_TIP, parseOrgLeafFlag, resolveOrgIsLeaf } from './orgLeafUtil.js';


export default {
    name: 'Hrtree',
    emits: ["nodeclick", "focuschange", "orgcheck","toggle"],
    data() {
        return {
            rows: [],
            orgData: {},
            focusRow: {},
            expandedKeys: [],
            emptymsg: "正在加载组织信息...",
            treeKey: {
                children: 'children',
                label: 'O_OrgName',
                isLeaf: 'isLeaf',
            },
            filterText: "", // 新增搜索关键词
            isTreeHidden: false, // 控制树结构是否隐藏
            searchResults: [], // 存储搜索结果
            orgAncestorPathMem: null,
            orgAncestorPathInflight: null,
            orgLoadMem: null,
            orgLoadInflight: null,
            autoSelectedFirstRoot: false,
            linkBindOrgOids: [],
            linkBindFetchInflight: null,
        }
    },
    created() {
        this.orgAncestorPathMem = new Map();
        this.orgAncestorPathInflight = new Map();
        this.orgLoadMem = new Map();
        this.orgLoadInflight = new Map();
    },
    props: {
        checked: {
            type: Array,
        },
        showCheck: {
            type: Boolean,
            default: false
        },
        showSearch: {
            type: Boolean,
            default: true
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
        //绑定的页面id
        bindPage: null,
        authcode:null,
        useAuth: {
        	type: Boolean,
        	default: false
        },
        expandLevel:{ type:Number,default:1 },
        unitname: { type: String, default: "" },// 新增属性，用于根据单位过滤组织节点，只显示该单位
        /** 组织树根节点限定为指定 HR_OrganizeOID（与 hrorginputV2 一致） */
        hrOrgFk: { type: String, default: "" },
        /** 组织关系组编码（HH_OrgGroup.Og_Code）；传入且 hrOrgFk 有效时额外展示绑定组织树 */
        linkCode: { type: String, default: "" },
        /** 根层加载完成后自动选中权限范围内最外层第一个节点 */
        autoSelectFirstRoot: { type: Boolean, default: false },
        /** 为 true 时仅允许选择最底层部门（末级组织） */
        leafOnly: { type: Boolean, default: false },
        /**
         * 组织树最大级数（HR_Organize.O_Lv）。如 2=只到二级单位。
         * 0 表示不限制。超出级数的节点不展示；等于该级数的节点标记为叶子不可再展开。
         */
        maxOrgLv: { type: Number, default: 0 },
        /** 为 true 时仅展示 O_OrgFlag=1 的单位节点（隐藏普通部门） */
        orgFlagOnly: { type: Boolean, default: false },
        mineFk:null,// 限定矿井
    },
    mounted() {
    	//this.loadTreeData();
    },
    watch:{
        unitname: {
            handler: function (newVal, oldVal) {
                if (newVal !== oldVal) {
                    // this.reloadTreeData();
                }
            },
            deep: true,
            immediate: true
        }
    }, 
    methods: {
        isLinkCodeMode() {
            const code = `${this.linkCode ?? ''}`.trim();
            const hrOrg = `${this.hrOrgFk ?? ''}`.trim();
            return !!code && !!hrOrg && !this.isNilGuid(hrOrg);
        },
        parseOrgLv(row) {
            const v = row?.O_Lv ?? row?.o_Lv ?? row?.O_LV;
            if (v === null || v === undefined || v === '') return null;
            const n = Number(v);
            return Number.isFinite(n) ? n : null;
        },
        isOrgFlagOn(row) {
            const v = row?.O_OrgFlag ?? row?.o_OrgFlag;
            if (v === true || v === 1 || v === '1') return true;
            if (v === false || v === 0 || v === '0') return false;
            if (typeof v === 'string') {
                const s = v.trim().toLowerCase();
                return s === 'true' || s === 'yes';
            }
            return !!v;
        },
        /** 父节点已达 maxOrgLv 时不再懒加载下级 */
        shouldStopExpand(node) {
            const maxLv = Number(this.maxOrgLv) || 0;
            if (maxLv <= 0 || !node || node.level < 1) return false;
            const lv = this.parseOrgLv(node.data);
            if (lv != null) return lv >= maxLv;
            // O_Lv 缺失时：el-tree 上可见层级数达到 maxOrgLv 即停
            return node.level >= maxLv;
        },
        applyOrgTreeLimits(rows, parentNode) {
            let list = Array.isArray(rows) ? [...rows] : [];
            if (this.orgFlagOnly) {
                list = list.filter((row) => this.isOrgFlagOn(row));
            }
            const maxLv = Number(this.maxOrgLv) || 0;
            if (maxLv > 0) {
                list = list.filter((row) => {
                    const lv = this.parseOrgLv(row);
                    if (lv != null) return lv <= maxLv;
                    return true;
                });
                list = list.map((row) => {
                    const next = { ...row };
                    const lv = this.parseOrgLv(next);
                    if (lv != null) {
                        if (lv >= maxLv) next.isLeaf = true;
                    } else if (parentNode && parentNode.level + 1 >= maxLv) {
                        next.isLeaf = true;
                    }
                    return next;
                });
            }
            return list;
        },
        passOrgTreeSearchFilter(row) {
            if (this.orgFlagOnly && !this.isOrgFlagOn(row)) return false;
            const maxLv = Number(this.maxOrgLv) || 0;
            if (maxLv > 0) {
                const lv = this.parseOrgLv(row);
                if (lv != null && lv > maxLv) return false;
            }
            return true;
        },
        dedupeOrgRowsByOid(rows) {
            const seen = new Set();
            const out = [];
            (rows ?? []).forEach((row) => {
                const oid = `${row?.HR_OrganizeOID ?? ''}`.trim();
                const key = oid.toLowerCase();
                if (!oid || this.isNilGuid(oid) || seen.has(key)) {
                    return;
                }
                seen.add(key);
                out.push(row);
            });
            return out;
        },
        normalizeArr(res) {
            const raw = this.unwrapAjax(res);
            if (Array.isArray(raw)) return raw;
            if (raw && Array.isArray(raw.rows)) return raw.rows;
            return [];
        },
        async fetchLinkBindOrgOids() {
            if (!this.isLinkCodeMode()) {
                this.linkBindOrgOids = [];
                return [];
            }
            if (this.linkBindFetchInflight) {
                return this.linkBindFetchInflight;
            }
            this.linkBindFetchInflight = (async () => {
                try {
                    const res = await loadFastPutOrgPanelOrgLinkBinds({
                        linkCode: `${this.linkCode}`.trim(),
                        hrOrgFk: `${this.hrOrgFk}`.trim(),
                    });
                    const rows = this.normalizeArr(res);
                    const seen = new Set();
                    const oids = [];
                    rows.forEach((row) => {
                        const fk = `${row.HR_BindOrg_FK ?? ''}`.trim();
                        const key = fk.toLowerCase();
                        if (!fk || this.isNilGuid(fk) || seen.has(key)) {
                            return;
                        }
                        seen.add(key);
                        oids.push(fk);
                    });
                    this.linkBindOrgOids = oids;
                    return oids;
                } catch (_e) {
                    this.linkBindOrgOids = [];
                    return [];
                } finally {
                    this.linkBindFetchInflight = null;
                }
            })();
            return this.linkBindFetchInflight;
        },
        async buildLinkBindTreeMeta(bindOids) {
            const validOids = (bindOids ?? []).filter((id) => !this.isNilGuid(id));
            if (!validOids.length) {
                return { extraRoots: [], expandedKeys: [] };
            }
            const paths = await Promise.all(validOids.map((oid) => this.fetchAncestorOidPath(oid)));
            const extraRoots = [];
            paths.forEach(({ leafRow }) => {
                if (leafRow?.HR_OrganizeOID) {
                    extraRoots.push(leafRow);
                }
            });
            return { extraRoots, expandedKeys: [] };
        },
        onNodeExpand(e,type){
            this.$emit("toggle",e,type);
        },
        isNilGuid(v) {
            const s = `${v ?? ''}`.trim().toLowerCase();
            return !s || s === '00000000-0000-0000-0000-000000000000';
        },
        orgAncestorPathCacheKey(id) {
            return `${id ?? ''}`.trim().toLowerCase();
        },
        unwrapAjax(res) {
            if (res && typeof res === 'object' && res.data !== undefined) {
                return res.data;
            }
            return res;
        },
        async fetchAncestorOidPath(leafOid) {
            if (this.isNilGuid(leafOid)) {
                return { chain: [], leafRow: null };
            }
            const ck = this.orgAncestorPathCacheKey(leafOid);
            if (this.orgAncestorPathMem.has(ck)) {
                return this.orgAncestorPathMem.get(ck);
            }
            let inflight = this.orgAncestorPathInflight.get(ck);
            if (inflight) {
                return inflight;
            }
            inflight = (async () => {
                try {
                    const res = await loadFastPutOrgPanelAncestorPath({ HR_OrganizeOID: `${leafOid}`.trim() });
                    const data = this.unwrapAjax(res);
                    const chain = Array.isArray(data?.chain) ? data.chain.map((x) => `${x}`) : [];
                    const leafRow = data?.leaf && typeof data.leaf === 'object' ? data.leaf : null;
                    const out = { chain, leafRow };
                    this.orgAncestorPathMem.set(ck, out);
                    return out;
                } catch (_e) {
                    const empty = { chain: [], leafRow: null };
                    this.orgAncestorPathMem.set(ck, empty);
                    return empty;
                } finally {
                    this.orgAncestorPathInflight.delete(ck);
                }
            })();
            this.orgAncestorPathInflight.set(ck, inflight);
            return inflight;
        },
        maybeAutoSelectFirstRoot(rows, nodeLevel) {
            if (!this.autoSelectFirstRoot || this.autoSelectedFirstRoot || nodeLevel !== 0) {
                return;
            }
            const list = Array.isArray(rows) ? rows : [];
            const row = list.find((item) => item?.HR_OrganizeOID);
            if (!row?.HR_OrganizeOID) {
                return;
            }
            this.autoSelectedFirstRoot = true;
            const oid = row.HR_OrganizeOID;
            this.$nextTick(() => {
                this.$nextTick(() => {
                    this.$refs.eltree?.setCurrentKey?.(oid);
                    if (!this.focusRow || this.focusRow.HR_OrganizeOID !== oid) {
                        this.focusRow = row;
                        this.$emit('focuschange', row);
                    }
                    this.$emit('nodeclick', row);
                });
            });
        },
        loadOrg(node, resolve, reject) {
            if (this.shouldStopExpand(node)) {
                resolve([]);
                return;
            }

            const scopeRoot = `${this.hrOrgFk ?? ''}`.trim();
            const hasScopeRoot = scopeRoot && !this.isNilGuid(scopeRoot);
            const linkMode = this.isLinkCodeMode();

            if (node.level === 0 && (hasScopeRoot || linkMode)) {
                (async () => {
                    try {
                        const scopeRows = [];
                        if (hasScopeRoot) {
                            const { leafRow: row } = await this.fetchAncestorOidPath(scopeRoot);
                            if (row?.HR_OrganizeOID) {
                                scopeRows.push(row);
                            }
                        }

                        let rows = scopeRows;
                        if (linkMode) {
                            const bindOids = this.linkBindOrgOids.length
                                ? this.linkBindOrgOids
                                : await this.fetchLinkBindOrgOids();
                            const { extraRoots } = await this.buildLinkBindTreeMeta(bindOids);
                            rows = this.dedupeOrgRowsByOid([...scopeRows, ...extraRoots]);
                        }

                        rows = this.applyOrgTreeLimits(rows, node);

                        if (rows.length > 0 || hasScopeRoot) {
                            resolve(rows);
                            this.maybeAutoSelectFirstRoot(rows, node.level);
                            this.$nextTick(() => {
                                this.$nextTick(() => {
                                    rows.forEach((row) => {
                                        if (row?.isLeaf) return;
                                        const tn = this.$refs.eltree?.getNode?.(row.HR_OrganizeOID);
                                        tn?.expand?.();
                                    });
                                });
                            });
                            return;
                        }
                        resolve([]);
                    } catch (_e) {
                        resolve([]);
                    }
                })();
                return;
            }

            var para = {
                useAuth: this.useAuth,
                authcode: this.authcode,
                bpo: this.authcode,
                mineFk: this.mineFk,
            };
            if (node.level >= 1) {
                para.oid = node.data.HR_OrganizeOID;
            }
            const loadKey = JSON.stringify({
                level: node.level,
                para,
                maxOrgLv: this.maxOrgLv,
                orgFlagOnly: this.orgFlagOnly,
            });
            const handleRows = (rows) => {
                const limited = this.applyOrgTreeLimits(rows, node);
                if (node.level < this.expandLevel) {
                    limited.forEach((o) => {
                        if (o?.isLeaf) return;
                        if (o?.HR_OrganizeOID && !this.expandedKeys.includes(o.HR_OrganizeOID)) {
                            this.expandedKeys.push(o.HR_OrganizeOID);
                        }
                    });
                }
                resolve(limited);
                this.maybeAutoSelectFirstRoot(limited, node.level);
            };
            if (this.orgLoadMem.has(loadKey)) {
                handleRows(this.orgLoadMem.get(loadKey));
                return;
            }
            let inflight = this.orgLoadInflight.get(loadKey);
            if (!inflight) {
                inflight = loadOrgTree(para)
                    .then((res) => {
                        const rows = res.data ?? [];
                        this.orgLoadMem.set(loadKey, rows);
                        return rows;
                    })
                    .finally(() => {
                        this.orgLoadInflight.delete(loadKey);
                    });
                this.orgLoadInflight.set(loadKey, inflight);
            }
            inflight.then(handleRows).catch(() => resolve([]));
        },
        scrollToNode() {
            this.$nextTick(() => {
                // 使用 ElementUI Tree 组件提供的 getNode 方法来获取节点对象
                if (this.$refs.eltree) {
                	const node = this.$refs.eltree.getNode(this.deptOID);
	                if (node) {
	                   var id = node.id;
	                    // 获取节点的 DOM 元素
	                    var nodeEl = this.$refs.eltree.$el.querySelector(`.el-tree-node[data-key="${this.deptOID}"]`);
	                  // var nodeEl = document.getElementById(id);
	                    if (nodeEl) {
	                        // 滚动到节点
	                        nodeEl.scrollIntoView({ behavior: 'smooth', block: 'center' });
	                    }
	                }
                }
            });
        },
        orgProbePara() {
            return {
                useAuth: this.useAuth,
                authcode: this.authcode,
                bpo: this.authcode,
                mineFk: this.mineFk,
            };
        },
        async ensureLeafSelectable(row, node) {
            if (!this.leafOnly) {
                return true;
            }
            const isLeaf = await resolveOrgIsLeaf(row, node, this.orgProbePara());
            if (!isLeaf) {
                ElMessage.warning(ORG_LEAF_ONLY_TIP);
            }
            return isLeaf;
        },
        async onNodeClick(tar, node, me) {
            if (this.canfatherClick == false) {
            	return;
            }
            if (!(await this.ensureLeafSelectable(tar, node))) {
                return;
            }
            if (!this.focusRow || this.focusRow.HR_OrganizeOID != tar.HR_OrganizeOID) {
            	this.focusRow = tar;
            	this.$emit("focuschange", tar);
            }
            this.$emit("nodeclick", tar, node, me);
        },
        getPartyIcon(p) {
            return "partyname fa fa-sitemap partyic" + p.data.Varchar7 + " partlev" + p.data.treeDeep;
        },
        orgcheck(e, n) {
            this.$emit("orgcheck", e, n);
        },
        filterNodes() {
            if (this.filterText.trim() === "") {
                // 如果搜索框为空，显示树结构，清空搜索结果
                this.isTreeHidden = false;
                this.searchResults = [];
            } else {
                // 如果搜索框有内容，隐藏树结构，调用接口查询
                this.isTreeHidden = true;
                this.searchOrg(this.filterText);
            }
        },
        // filterNodes() {
        //     this.$refs.eltree.filter(this.filterText);
        // },
        filterNode(value, data) {
            if (!value) return true;
            return data.OrgName.includes(value);
        },
        async searchOrg(filterText) {
            const kw = `${filterText ?? ''}`.trim();
            if (!kw) {
                this.searchResults = [];
                return;
            }
            const scopeRoot = `${this.hrOrgFk ?? ''}`.trim();
            const hasScopeRoot = scopeRoot && !this.isNilGuid(scopeRoot);
            let scopes = hasScopeRoot ? [scopeRoot] : [];

            if (this.isLinkCodeMode()) {
                const bindOids = this.linkBindOrgOids.length
                    ? this.linkBindOrgOids
                    : await this.fetchLinkBindOrgOids();
                const seen = new Set(scopes.map((s) => `${s}`.trim().toLowerCase()));
                bindOids.forEach((oid) => {
                    const k = `${oid ?? ''}`.trim();
                    const key = k.toLowerCase();
                    if (k && !this.isNilGuid(k) && !seen.has(key)) {
                        seen.add(key);
                        scopes.push(k);
                    }
                });
            }

            if (scopes.length) {
                try {
                    const results = await Promise.all(
                        scopes.map(async (scope) => {
                            const para = { txt: kw, hrOrgFk: scope };
                            const res = await loadFastPutOrgPanelSearch(para);
                            return this.normalizeArr(res);
                        }),
                    );
                    const seen = new Set();
                    const merged = [];
                    results.forEach((rows) => {
                        rows.forEach((row) => {
                            const key = String(row.HR_OrganizeOID ?? row.UCML_OrganizeOID ?? row.OrgName ?? '');
                            if (!key || seen.has(key)) {
                                return;
                            }
                            seen.add(key);
                            merged.push(row);
                        });
                    });
                    this.searchResults = merged.filter((row) => this.passOrgTreeSearchFilter(row));
                } catch (_e) {
                    this.searchResults = [];
                }
                return;
            }
            queryorglist({
                OrgName: kw,
                txt: kw,
                useAuth: this.useAuth,
                authcode: this.authcode,
                bpo: this.authcode,
            }).then((res) => {
                this.searchResults = (res.data ?? []).filter((row) => this.passOrgTreeSearchFilter(row));
            }).catch(() => {
                this.searchResults = [];
            });
        },
        async handleRowClick(row){
            if (!(await this.ensureLeafSelectable(row, null))) {
                return;
            }
            this.$emit("nodeclick", row);
        }

    }
};
</script>

<style scoped>
.partyname {

    font-size: 14px;
}
.hr-tree-node{
    font-size:12px;
}
.is-current>div {
    /*border: 2px solid #185C37;*/
    border-radius: 5px;
    background-color: #CCE8CF;
    font-weight: bold;
}

.el-tree-node__content:hover {
    /*border: 2px solid #185C37;*/
    color: #185C37;
    font-weight: bold;
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
