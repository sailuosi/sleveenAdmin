<template>
    <div class="hrtree-v2">
        <el-input
            v-if="showSearch"
            v-model="filterText"
            class="hrtree-v2__search"
            clearable
            placeholder="请输入单位名称..."
            @input="onSearchInput"
            @clear="clearSearch"
        />
        <div v-show="!isTreeHidden" class="hrtree-v2__tree-wrap">
            <el-tree
                ref="eltree"
                highlight-current
                :empty-text="emptymsg"
                :props="treeKey"
                node-key="HR_OrganizeOID"
                :default-expanded-keys="expandedKeys"
                :default-checked-keys="checked"
                :show-checkbox="showCheck"
                :expand-on-click-node="false"
                :check-strictly="strict"
                :load="loadOrg"
                lazy
                @node-click="onNodeClick"
                @check="orgcheck"
                @node-expand="onNodeExpand($event, 'expand')"
                @node-collapse="onNodeExpand($event, 'collapse')"
            >
                <template #default="p">
                    <span class="hr-tree-node">
                        <span :class="getPartyIcon(p)"></span>
                        <span class="myonode">{{ p.node.label }}</span>
                    </span>
                </template>
            </el-tree>
        </div>
        <div v-show="isTreeHidden" class="hrtree-v2__hit-wrap">
            <div v-if="searchLoading" class="hrtree-v2__muted">查询中…</div>
            <div v-else-if="!searchResults.length" class="hrtree-v2__muted">未匹配到组织</div>
            <div v-else class="hrtree-v2__hit-list">
                <div
                    v-for="row in searchResults"
                    :key="hitRowKey(row)"
                    class="hrtree-v2__hit-row"
                    @click="handleRowClick(row)"
                >
                    <div class="hrtree-v2__hit-name">{{ row.O_OrgName || row.OrgName }}</div>
                    <div v-if="row.PathName || row.O_Path" class="hrtree-v2__hit-path">
                        {{ row.PathName || row.O_Path }}
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
/**
 * hrtreeV2 — 懒加载组织树 + 服务端搜索列表
 * 与 hrtree.vue 同 props/events，搜索改为列表展示（不走 el-tree filter）。
 */
import { ElMessage } from 'element-plus';
import { loadOrgTree } from '@s/api/system/mancommonapi.js';
import {
    loadFastPutOrgPanelAncestorPath,
    loadFastPutOrgPanelOrgLinkBinds,
    loadFastPutOrgPanelSearch,
} from '../../api/fastorgputapi.js';
import { ORG_LEAF_ONLY_TIP, resolveOrgIsLeaf } from './orgLeafUtil.js';

const SEARCH_DEBOUNCE_MS = 280;

export default {
    name: 'HrtreeV2',
    emits: ['nodeclick', 'focuschange', 'orgcheck', 'toggle'],
    props: {
        checked: { type: Array },
        showCheck: { type: Boolean, default: false },
        showSearch: { type: Boolean, default: true },
        canfatherClick: { type: Boolean, default: true },
        strict: { type: Boolean, default: false },
        deptOID: { type: String, default: '' },
        bindPage: null,
        authcode: null,
        useAuth: { type: Boolean, default: false },
        expandLevel: { type: Number, default: 1 },
        unitname: { type: String, default: '' },
        hrOrgFk: { type: String, default: '' },
        linkCode: { type: String, default: '' },
        autoSelectFirstRoot: { type: Boolean, default: false },
        leafOnly: { type: Boolean, default: false },
        mineFk: null,
    },
    data() {
        return {
            rows: [],
            orgData: {},
            focusRow: {},
            expandedKeys: [],
            emptymsg: '正在加载组织信息...',
            treeKey: {
                children: 'children',
                label: 'O_OrgName',
            },
            filterText: '',
            isTreeHidden: false,
            searchResults: [],
            searchLoading: false,
            searchSeq: 0,
            searchTimer: null,
            orgAncestorPathMem: null,
            orgAncestorPathInflight: null,
            orgLoadMem: null,
            orgLoadInflight: null,
            autoSelectedFirstRoot: false,
            linkBindOrgOids: [],
            linkBindFetchInflight: null,
            authRootOidMem: null,
            authRootOidInflight: null,
        };
    },
    created() {
        this.orgAncestorPathMem = new Map();
        this.orgAncestorPathInflight = new Map();
        this.orgLoadMem = new Map();
        this.orgLoadInflight = new Map();
    },
    beforeUnmount() {
        if (this.searchTimer) {
            clearTimeout(this.searchTimer);
            this.searchTimer = null;
        }
    },
    methods: {
        isLinkCodeMode() {
            const code = `${this.linkCode ?? ''}`.trim();
            const hrOrg = `${this.hrOrgFk ?? ''}`.trim();
            return !!code && !!hrOrg && !this.isNilGuid(hrOrg);
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
        onNodeExpand(e, type) {
            this.$emit('toggle', e, type);
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
                    const res = await loadFastPutOrgPanelAncestorPath({
                        HR_OrganizeOID: `${leafOid}`.trim(),
                    });
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
        loadOrg(node, resolve) {
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

                        if (rows.length > 0 || hasScopeRoot) {
                            resolve(rows);
                            this.maybeAutoSelectFirstRoot(rows, node.level);
                            this.$nextTick(() => {
                                this.$nextTick(() => {
                                    rows.forEach((row) => {
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

            const para = {
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
            });
            const handleRows = (rows) => {
                if (node.level < this.expandLevel) {
                    rows.forEach((o) => {
                        if (o?.HR_OrganizeOID && !this.expandedKeys.includes(o.HR_OrganizeOID)) {
                            this.expandedKeys.push(o.HR_OrganizeOID);
                        }
                    });
                }
                resolve(rows);
                this.maybeAutoSelectFirstRoot(rows, node.level);
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
                if (!this.$refs.eltree) {
                    return;
                }
                const node = this.$refs.eltree.getNode(this.deptOID);
                if (!node) {
                    return;
                }
                const nodeEl = this.$refs.eltree.$el.querySelector(
                    `.el-tree-node[data-key="${this.deptOID}"]`,
                );
                if (nodeEl) {
                    nodeEl.scrollIntoView({ behavior: 'smooth', block: 'center' });
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
            if (this.canfatherClick === false) {
                return;
            }
            if (!(await this.ensureLeafSelectable(tar, node))) {
                return;
            }
            if (!this.focusRow || this.focusRow.HR_OrganizeOID !== tar.HR_OrganizeOID) {
                this.focusRow = tar;
                this.$emit('focuschange', tar);
            }
            this.$emit('nodeclick', tar, node, me);
        },
        getPartyIcon(p) {
            return `partyname fa fa-sitemap partyic${p.data.Varchar7} partlev${p.data.treeDeep}`;
        },
        orgcheck(e, n) {
            this.$emit('orgcheck', e, n);
        },
        hitRowKey(row) {
            return String(row.HR_OrganizeOID ?? row.O_OrgName ?? row.OrgName ?? '');
        },
        clearSearch() {
            if (this.searchTimer) {
                clearTimeout(this.searchTimer);
                this.searchTimer = null;
            }
            this.searchSeq += 1;
            this.filterText = '';
            this.isTreeHidden = false;
            this.searchResults = [];
            this.searchLoading = false;
        },
        onSearchInput() {
            if (this.searchTimer) {
                clearTimeout(this.searchTimer);
            }
            const kw = `${this.filterText ?? ''}`.trim();
            if (!kw) {
                this.clearSearch();
                return;
            }
            this.isTreeHidden = true;
            this.searchTimer = setTimeout(() => {
                this.searchOrg(kw);
            }, SEARCH_DEBOUNCE_MS);
        },
        /** useAuth / mineFk 时用树根 OID 作搜索 scope，与懒加载根层权限对齐 */
        async resolveAuthRootScopes() {
            const need =
                this.useAuth ||
                (this.mineFk !== null && this.mineFk !== undefined && `${this.mineFk}`.trim() !== '');
            if (!need) {
                return [];
            }
            const cacheKey = JSON.stringify({
                useAuth: this.useAuth,
                authcode: this.authcode,
                mineFk: this.mineFk,
            });
            if (this.authRootOidMem?.key === cacheKey) {
                return this.authRootOidMem.oids;
            }
            if (this.authRootOidInflight?.key === cacheKey) {
                return this.authRootOidInflight.promise;
            }
            const promise = (async () => {
                try {
                    const res = await loadOrgTree({
                        useAuth: this.useAuth,
                        authcode: this.authcode,
                        bpo: this.authcode,
                        mineFk: this.mineFk,
                    });
                    const rows = res?.data ?? [];
                    const oids = [];
                    const seen = new Set();
                    rows.forEach((row) => {
                        const oid = `${row?.HR_OrganizeOID ?? ''}`.trim();
                        const key = oid.toLowerCase();
                        if (!oid || this.isNilGuid(oid) || seen.has(key)) {
                            return;
                        }
                        seen.add(key);
                        oids.push(oid);
                    });
                    this.authRootOidMem = { key: cacheKey, oids };
                    return oids;
                } catch (_e) {
                    this.authRootOidMem = { key: cacheKey, oids: [] };
                    return [];
                } finally {
                    this.authRootOidInflight = null;
                }
            })();
            this.authRootOidInflight = { key: cacheKey, promise };
            return promise;
        },
        async resolveSearchScopes() {
            const scopeRoot = `${this.hrOrgFk ?? ''}`.trim();
            const hasScopeRoot = scopeRoot && !this.isNilGuid(scopeRoot);
            const scopes = hasScopeRoot ? [scopeRoot] : [];
            const seen = new Set(scopes.map((s) => `${s}`.trim().toLowerCase()));

            const pushScope = (oid) => {
                const k = `${oid ?? ''}`.trim();
                const key = k.toLowerCase();
                if (!k || this.isNilGuid(k) || seen.has(key)) {
                    return;
                }
                seen.add(key);
                scopes.push(k);
            };

            if (this.isLinkCodeMode()) {
                const bindOids = this.linkBindOrgOids.length
                    ? this.linkBindOrgOids
                    : await this.fetchLinkBindOrgOids();
                bindOids.forEach(pushScope);
            }

            if (!scopes.length) {
                const authRoots = await this.resolveAuthRootScopes();
                authRoots.forEach(pushScope);
            }

            return scopes;
        },
        async searchOrg(filterText) {
            const kw = `${filterText ?? ''}`.trim();
            const seq = ++this.searchSeq;
            if (!kw) {
                this.searchResults = [];
                this.searchLoading = false;
                return;
            }
            this.searchLoading = true;
            try {
                const scopes = await this.resolveSearchScopes();
                let merged = [];
                if (scopes.length) {
                    const results = await Promise.all(
                        scopes.map(async (scope) => {
                            const res = await loadFastPutOrgPanelSearch({ txt: kw, hrOrgFk: scope });
                            return this.normalizeArr(res);
                        }),
                    );
                    const seen = new Set();
                    results.forEach((rows) => {
                        rows.forEach((row) => {
                            const key = this.hitRowKey(row);
                            if (!key || seen.has(key)) {
                                return;
                            }
                            seen.add(key);
                            merged.push(row);
                        });
                    });
                } else {
                    const res = await loadFastPutOrgPanelSearch({ txt: kw });
                    merged = this.normalizeArr(res);
                }
                if (seq !== this.searchSeq) {
                    return;
                }
                this.searchResults = merged;
            } catch (_e) {
                if (seq !== this.searchSeq) {
                    return;
                }
                this.searchResults = [];
            } finally {
                if (seq === this.searchSeq) {
                    this.searchLoading = false;
                }
            }
        },
        async handleRowClick(row) {
            if (this.canfatherClick === false) {
                return;
            }
            if (!(await this.ensureLeafSelectable(row, null))) {
                return;
            }
            if (!this.focusRow || this.focusRow.HR_OrganizeOID !== row.HR_OrganizeOID) {
                this.focusRow = row;
                this.$emit('focuschange', row);
            }
            this.$emit('nodeclick', row);
        },
    },
};
</script>

<style scoped>
.hrtree-v2 {
    display: flex;
    flex-direction: column;
    min-height: 0;
}

.hrtree-v2__search {
    width: 240px;
    margin-bottom: 8px;
    flex-shrink: 0;
}

.hrtree-v2__tree-wrap {
    height: 600px;
    overflow-y: auto;
}

.hrtree-v2__hit-wrap {
    height: 600px;
    overflow-y: auto;
}

.hrtree-v2__muted {
    padding: 16px 8px;
    color: #909399;
    font-size: 13px;
}

.hrtree-v2__hit-row {
    padding: 10px 8px;
    cursor: pointer;
    border-bottom: 1px solid #f0f0f0;
}

.hrtree-v2__hit-row:hover {
    background: #f5f7fa;
}

.hrtree-v2__hit-name {
    color: #303133;
    font-weight: 700;
    font-size: 14px;
    line-height: 1.4;
}

.hrtree-v2__hit-path {
    margin-top: 2px;
    color: #909399;
    font-size: 12px;
    line-height: 1.4;
}

.partyname {
    font-size: 14px;
}

.hr-tree-node {
    font-size: 12px;
}

.is-current > div {
    border-radius: 5px;
    background-color: #cce8cf;
    font-weight: bold;
}

.el-tree-node__content:hover {
    color: #185c37;
    font-weight: bold;
}

.partlev5 {
    color: #0d86d5;
}

.partlev10 {
    color: #53c1c1;
}

.partlev15 {
    color: #b19c4a;
}

.partlev20 {
    color: #5db0e8;
}

.partlev25 {
    color: #00b474;
}

.partlev30 {
    color: #b19c4a;
}
</style>
