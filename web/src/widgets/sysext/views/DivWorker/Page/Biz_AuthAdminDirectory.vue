<template>
  <div class="auth-admin-dir" v-loading="loading">
    <leftright left="0 0 300px" right="1 1 auto" class="layout" :labelL="leftPaneLabel" labelR="权限管理员">
      <template #lbody>
        <div class="tree-pane" :class="authMode === 'place' ? 'is-place' : 'is-org'">
          <div class="mode-seg" :data-mode="authMode" role="tablist" aria-label="权限类型">
            <span class="mode-seg__thumb" aria-hidden="true" />
            <button
              type="button"
              class="mode-seg__btn"
              :class="{ 'is-active': authMode === 'org' }"
              role="tab"
              :aria-selected="authMode === 'org'"
              @click="setAuthMode('org')"
            >
              <el-icon><OfficeBuilding /></el-icon>
              <span>组织</span>
            </button>
            <button
              type="button"
              class="mode-seg__btn"
              :class="{ 'is-active': authMode === 'place' }"
              role="tab"
              :aria-selected="authMode === 'place'"
              @click="setAuthMode('place')"
            >
              <el-icon><Location /></el-icon>
              <span>地点</span>
            </button>
          </div>
          <div class="mode-hint">
            {{ authMode === 'place' ? '浏览矿区地点树，查含地点主数据菜单权限的人' : '浏览行政组织树，查组织挂靠管理员' }}
          </div>

          <template v-if="authMode === 'org'">
            <hrtree
              v-if="treeReady"
              class="org-tree"
              :showSearch="true"
              :useAuth="false"
              :hrOrgFk="belongRootOid"
              :expandLevel="1"
              :max-org-lv="2"
              :org-flag-only="true"
              @nodeclick="onOrgClick"
            />
            <div v-else class="tree-loading">
              <el-icon class="is-loading"><Loading /></el-icon>
              <span>正在加载所属单位…</span>
            </div>
          </template>

          <template v-else>
            <el-input
              v-model="placeFilterText"
              clearable
              placeholder="请输入地点名称..."
              class="place-search"
              @input="onPlaceSearchInput"
              @clear="clearPlaceSearch"
            >
              <template #prefix>
                <el-icon><Search /></el-icon>
              </template>
            </el-input>
            <div v-show="!placeSearchActive" class="place-tree-wrap">
              <el-tree
                v-if="treeReady"
                :key="`place-tree-${belongOrgNo || 'all'}`"
                ref="placeTreeRef"
                class="place-tree"
                node-key="MD_TreeOID"
                lazy
                highlight-current
                :load="loadPlaceNode"
                :props="{ label: 'MD_Name', children: 'children', isLeaf: 'isLeaf' }"
                :expand-on-click-node="false"
                @node-click="onPlaceClick"
              />
              <div v-else class="tree-loading">
                <el-icon class="is-loading"><Loading /></el-icon>
                <span>正在加载地点树…</span>
              </div>
            </div>
            <div v-show="placeSearchActive" class="place-hit-wrap">
              <div v-if="placeSearchLoading" class="tree-loading">查询中…</div>
              <div
                v-for="row in placeHitRows"
                :key="row.MD_TreeOID"
                class="place-hit-row"
                @click="onPlaceHitClick(row)"
              >
                <div class="place-hit-name">{{ row.MD_Name }}</div>
                <div class="place-hit-path">{{ row.MD_FullName || row.MD_Path || '' }}</div>
              </div>
              <div v-if="!placeSearchLoading && !placeHitRows.length" class="tree-loading">未匹配到地点</div>
            </div>
          </template>
        </div>
      </template>

      <template #rbody>
        <div class="right-pane" :class="authMode === 'place' ? 'is-place' : 'is-org'">
          <div class="status-bar">
            <div
              class="sel-card"
              :class="[
                authMode === 'place' ? 'is-place' : 'is-org',
                selectedOrg.orgName ? '' : 'is-empty',
              ]"
            >
              <div class="sel-card__badge">
                <el-icon v-if="authMode === 'org'"><OfficeBuilding /></el-icon>
                <el-icon v-else><Location /></el-icon>
                <span>{{ authMode === 'place' ? '地点' : '组织' }}</span>
              </div>
              <div class="sel-card__body">
                <template v-if="selectedOrg.orgName">
                  <div class="sel-card__name">{{ selectedOrg.orgName }}</div>
                  <div v-if="selectedOrg.pathName" class="sel-card__path">{{ selectedOrg.pathName }}</div>
                </template>
                <div v-else class="sel-card__placeholder">{{ emptySelectTip }}</div>
              </div>
            </div>
            <div class="rhead-tools">
              <el-input
                v-model="keyword"
                clearable
                placeholder="姓名"
                class="tool-input tool-input--sm"
                @keyup.enter="onSearch"
              >
                <template #prefix>
                  <el-icon><Search /></el-icon>
                </template>
              </el-input>
              <el-input
                v-model="grantOrgKeyword"
                clearable
                :placeholder="grantKeywordPlaceholder"
                class="tool-input"
                @keyup.enter="onSearch"
              />
              <el-button type="primary" :disabled="!canSearch" @click="onSearch">查询</el-button>
              <el-button :disabled="!canSearch" @click="onSearch">刷新</el-button>
            </div>
          </div>

          <div class="tip-line" :class="authMode === 'place' ? 'is-place' : 'is-org'">
            <el-icon><InfoFilled /></el-icon>
            <span>{{ tipText }}</span>
            <span class="tip-line__count">共 <b>{{ total }}</b> 人</span>
          </div>

          <div class="table-wrap">
            <el-table
              :data="pageList"
              stripe
              height="100%"
              style="width: 100%"
              class="admin-table"
              :header-cell-style="headerCellStyle"
              empty-text="暂无权限管理员"
            >
              <el-table-column type="index" width="52" label="#" :index="indexMethod" align="center" />
              <el-table-column prop="empName" label="姓名" width="120">
                <template #default="{ row }">
                  <div class="name-cell">
                    <el-icon class="name-icon"><User /></el-icon>
                    <span class="name-text">{{ row.empName || '-' }}</span>
                  </div>
                </template>
              </el-table-column>
              <el-table-column prop="belongOrgName" label="单位" min-width="140" show-overflow-tooltip>
                <template #default="{ row }">
                  <span v-if="row.belongOrgName">{{ row.belongOrgName }}</span>
                  <span v-else class="muted">-</span>
                </template>
              </el-table-column>
              <el-table-column prop="divName" label="部门" min-width="140" show-overflow-tooltip>
                <template #default="{ row }">
                  <span v-if="row.divName">{{ row.divName }}</span>
                  <span v-else class="muted">-</span>
                </template>
              </el-table-column>
              <el-table-column prop="grantOrgNames" :label="grantColLabel" min-width="140" show-overflow-tooltip />
              <el-table-column prop="bossTitles" label="所持分配角色" min-width="130" show-overflow-tooltip>
                <template #default="{ row }">
                  <span v-if="row.bossTitles" class="role-text">{{ row.bossTitles }}</span>
                  <span v-else class="muted">-</span>
                </template>
              </el-table-column>
              <el-table-column label="可分配业务模块" min-width="420">
                <template #default="{ row }">
                  <div v-if="row.modules && row.modules.length" class="tag-wrap">
                    <el-tag
                      v-for="(m, i) in visibleModules(row.modules)"
                      :key="i"
                      size="small"
                      :type="authMode === 'place' ? 'success' : 'primary'"
                      effect="light"
                      round
                      class="mod-tag"
                    >{{ m }}</el-tag>
                    <el-tag
                      v-if="row.modules.length > maxTagShow"
                      size="small"
                      type="info"
                      effect="light"
                      round
                      class="mod-tag"
                    >+{{ row.modules.length - maxTagShow }}</el-tag>
                  </div>
                  <span v-else class="muted">暂无</span>
                </template>
              </el-table-column>
              <el-table-column label="操作" width="88" fixed="right" align="center">
                <template #default="{ row }">
                  <el-button type="primary" link @click="showDetail(row)">明细</el-button>
                </template>
              </el-table-column>
            </el-table>
          </div>

          <div class="pager">
            <el-pagination
              v-model:current-page="page"
              :page-size="pageSize"
              :total="total"
              layout="total, prev, pager, next, jumper"
              background
              small
              @current-change="onPageChange"
            />
          </div>
        </div>
      </template>
    </leftright>

    <el-dialog
      v-model="detailVisible"
      title="可分配模块明细"
      width="720px"
      destroy-on-close
      class="detail-dialog"
      align-center
    >
      <div v-if="detailRow" class="detail-head">
        <el-icon class="name-icon name-icon--lg"><User /></el-icon>
        <div>
          <div class="detail-head__name">{{ detailRow.empName }}</div>
          <div v-if="detailRow.belongOrgName" class="detail-head__sub">单位：{{ detailRow.belongOrgName }}</div>
          <div v-if="detailRow.divName" class="detail-head__sub">部门：{{ detailRow.divName }}</div>
          <div v-if="detailRow.bossTitles" class="detail-head__sub">所持分配角色：{{ detailRow.bossTitles }}</div>
        </div>
      </div>
      <el-table :data="detailByOrg" border size="small" max-height="420" class="admin-table">
        <el-table-column prop="orgSrcLabel" label="类型" width="90" align="center">
          <template #default="{ row }">
            <el-tag
              size="small"
              :type="row.orgSrc === '1' ? 'primary' : 'success'"
              effect="light"
              round
            >
              {{ row.orgSrcLabel || '-' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="orgName" label="组织/地点" min-width="140" show-overflow-tooltip />
        <el-table-column prop="bossTitle" label="分配角色" min-width="120" show-overflow-tooltip />
        <el-table-column prop="title" label="可分配模块" min-width="140" />
      </el-table>
    </el-dialog>
  </div>
</template>

<script setup>
///////////////////////////////////////////////////////////
//  Biz_AuthAdminDirectory.vue
//  权限管理员查询：按所属单位找可二次分配权限的人
///////////////////////////////////////////////////////////
import { ref, reactive, computed, onMounted } from 'vue';
import { ElMessage } from 'element-plus';
import { Search, InfoFilled, Loading, User, OfficeBuilding, Location } from '@element-plus/icons-vue';
import leftright from '@/ui/panels/leftright.vue';
import hrtree from '../uiext/org/hrtree.vue';
import { loadFastMdTreeChildren, loadFastMdTreeSearchHits } from '../api/fastmdtreeapi.js';
import { fetchMyBelongOrg, fetchAuthAdminByOrg } from './Biz_AuthAdminDirectoryapi.js';

const loading = ref(false);
const keyword = ref('');
const grantOrgKeyword = ref('');
/** org=行政组织权限；place=矿区地点权限（左侧切换对应树） */
const authMode = ref('org');
const allList = ref([]);
const total = ref(0);
const page = ref(1);
const pageSize = 15;
const maxTagShow = 8;
const selectedOrg = reactive({
  orgOid: '',
  placeOid: '',
  orgName: '',
  pathName: '',
});
/** 本人所属单位作为组织树根；超管为空表示不限根 */
const belongRootOid = ref('');
/** 地点树按所属单位 OrgNo 收窄（超管为空） */
const belongOrgNo = ref('');
const isSuperAdmin = ref(false);
const treeReady = ref(false);
const belongSnapshot = ref(null);

const placeTreeRef = ref(null);
const placeFilterText = ref('');
const placeHitRows = ref([]);
const placeSearchLoading = ref(false);
let placeSearchTimer = null;

const detailVisible = ref(false);
const detailRow = ref(null);
const detailByOrg = computed(() => detailRow.value?.moduleByOrg || []);

const leftPaneLabel = computed(() => (authMode.value === 'place' ? '矿区地点' : '行政组织'));
const grantColLabel = computed(() => (authMode.value === 'place' ? '权限挂靠地点' : '权限挂靠单位'));
const grantKeywordPlaceholder = computed(() => (authMode.value === 'place' ? '挂靠地点' : '挂靠单位'));
const emptySelectTip = computed(() =>
  authMode.value === 'place' ? '请在左侧选择矿区地点' : '请在左侧选择行政组织',
);
const tipText = computed(() =>
  authMode.value === 'place'
    ? '当前为地点权限：按所选地点过滤；仅列出所持类别绑定了「地点类主数据/地点主数据导航」菜单的人；一人多挂靠会一并展示。'
    : '当前为组织权限：左侧为行政组织树；仅显示本级及下级行政组织挂靠（不含上级）。',
);
const canSearch = computed(() =>
  authMode.value === 'place' ? !!selectedOrg.placeOid : !!selectedOrg.orgOid,
);
const placeSearchActive = computed(() => `${placeFilterText.value ?? ''}`.trim().length > 0);

const pageList = computed(() => {
  const start = (page.value - 1) * pageSize;
  return allList.value.slice(start, start + pageSize);
});

const headerCellStyle = {
  background: '#f7f9fc',
  color: '#4a5568',
  fontWeight: '600',
  fontSize: '13px',
};

function indexMethod(index) {
  return (page.value - 1) * pageSize + index + 1;
}

function visibleModules(list) {
  return (list || []).slice(0, maxTagShow);
}

function clearSelection() {
  selectedOrg.orgOid = '';
  selectedOrg.placeOid = '';
  selectedOrg.orgName = '';
  selectedOrg.pathName = '';
  allList.value = [];
  total.value = 0;
  page.value = 1;
}

function setAuthMode(mode) {
  if (authMode.value === mode) return;
  authMode.value = mode;
  onAuthModeChange();
}

function onAuthModeChange() {
  clearSelection();
  clearPlaceSearch();
  if (authMode.value === 'org' && belongSnapshot.value?.orgOid) {
    applyOrgAndLoad({
      HR_OrganizeOID: belongSnapshot.value.orgOid,
      O_OrgName: belongSnapshot.value.orgName,
      O_Path: belongSnapshot.value.pathName,
    });
  }
}

function onOrgClick(data) {
  applyOrgAndLoad(data);
}

function applyOrgAndLoad(data) {
  selectedOrg.placeOid = '';
  selectedOrg.orgOid = data?.HR_OrganizeOID || data?.UCML_OrganizeOID || '';
  selectedOrg.orgName = data?.O_OrgName || data?.OrgName || '';
  selectedOrg.pathName = data?.O_Path || data?.PathName || data?.Text1 || '';
  if (selectedOrg.orgOid) onSearch();
}

function normalizePlaceRows(res) {
  const raw = res?.data ?? res;
  const list = Array.isArray(raw) ? raw : Array.isArray(raw?.rows) ? raw.rows : Array.isArray(raw?.data) ? raw.data : [];
  return list.map((row) => {
    const leaf = row?.isLeaf ?? row?.IsLeaf;
    const isLeaf = leaf === true || leaf === 1 || leaf === '1' || leaf === 'true';
    return { ...row, isLeaf };
  });
}

function buildPlaceTreePara(parentOid) {
  const para = { onlyApproved: true };
  // 仅传真实单位编号；勿用层次码，否则 MD_Tree.OrgNo 精确匹配会把树滤空
  const orgNo = `${belongOrgNo.value ?? ''}`.trim();
  if (orgNo && !isSuperAdmin.value) {
    para.orgNo = orgNo;
  }
  const pid = `${parentOid ?? ''}`.trim();
  if (pid) para.MD_ParentOID = pid;
  return para;
}

async function loadPlaceNode(node, resolve) {
  try {
    const parentOid = node.level === 0 ? '' : node.data?.MD_TreeOID;
    const res = await loadFastMdTreeChildren(buildPlaceTreePara(parentOid));
    resolve(normalizePlaceRows(res));
  } catch (_e) {
    resolve([]);
  }
}

function onPlaceClick(data) {
  if (!data?.MD_TreeOID) return;
  selectedOrg.orgOid = '';
  selectedOrg.placeOid = String(data.MD_TreeOID);
  selectedOrg.orgName = data.MD_Name || '';
  selectedOrg.pathName = data.MD_FullName || data.MD_Path || '';
  onSearch();
}

function onPlaceSearchInput() {
  clearTimeout(placeSearchTimer);
  placeSearchTimer = setTimeout(runPlaceSearch, 280);
}

function clearPlaceSearch() {
  clearTimeout(placeSearchTimer);
  placeFilterText.value = '';
  placeHitRows.value = [];
  placeSearchLoading.value = false;
}

async function runPlaceSearch() {
  const kw = `${placeFilterText.value ?? ''}`.trim();
  if (!kw) {
    placeHitRows.value = [];
    return;
  }
  placeSearchLoading.value = true;
  try {
    const para = { keyword: kw, matchCode: true, onlyApproved: true };
    const orgNo = `${belongOrgNo.value ?? ''}`.trim();
    if (orgNo && !isSuperAdmin.value) {
      para.orgNo = orgNo;
    }
    const res = await loadFastMdTreeSearchHits(para);
    placeHitRows.value = normalizePlaceRows(res);
  } catch (_e) {
    placeHitRows.value = [];
  } finally {
    placeSearchLoading.value = false;
  }
}

function onPlaceHitClick(row) {
  clearPlaceSearch();
  onPlaceClick(row);
}

/** 进入页面：按人事所属单位加载树并查询管理员 */
async function autoSelectBelongOrg() {
  try {
    const res = await fetchMyBelongOrg();
    const payload = unwrapAjax(res);
    if (payload?.status === 'error' || (!payload?.data?.orgOid && payload?.status !== 'success')) {
      ElMessage.warning(payload?.info || payload?.message || '未找到所属行政单位');
      return;
    }
    const org = payload?.data || payload || {};
    if (!org.orgOid) {
      ElMessage.warning(payload?.info || '未找到所属行政单位');
      return;
    }
    isSuperAdmin.value = !!org.isSuperAdmin;
    belongRootOid.value = org.isSuperAdmin ? '' : (org.orgOid || '');
    // 地点树按单位编号过滤；没有 O_OrgNo 时不传，避免误用层次码把树滤空
    belongOrgNo.value = org.isSuperAdmin ? '' : `${org.orgNo ?? ''}`.trim();
    belongSnapshot.value = {
      orgOid: org.orgOid,
      orgName: org.orgName,
      pathName: org.pathName,
    };
    treeReady.value = true;
    if (authMode.value === 'org') {
      applyOrgAndLoad({
        HR_OrganizeOID: org.orgOid,
        O_OrgName: org.orgName,
        O_Path: org.pathName,
      });
    }
  } catch (e) {
    ElMessage.error(e?.message || '加载所属单位失败');
  }
}

function unwrapAjax(res) {
  if (!res) return res;
  if (res.status === 'success' || res.status === 'error' || res.data !== undefined) return res;
  if (res.result) return res.result;
  return res;
}

onMounted(() => {
  autoSelectBelongOrg();
});

function onSearch() {
  page.value = 1;
  loadList();
}

function onPageChange() {
  // 翻页仅切前端切片
}

async function loadList() {
  if (authMode.value === 'place') {
    if (!selectedOrg.placeOid) {
      ElMessage.warning('请先选择矿区地点');
      return;
    }
  } else if (!selectedOrg.orgOid) {
    ElMessage.warning('请先选择行政组织');
    return;
  }
  loading.value = true;
  try {
    const para =
      authMode.value === 'place'
        ? {
            placeOid: selectedOrg.placeOid,
            keyword: keyword.value,
            grantOrgKeyword: grantOrgKeyword.value,
            orgSrc: '2',
          }
        : {
            orgOid: selectedOrg.orgOid,
            keyword: keyword.value,
            grantOrgKeyword: grantOrgKeyword.value,
            orgSrc: '1',
          };
    const res = await fetchAuthAdminByOrg(para);
    const payload = unwrapAjax(res);
    if (payload?.status === 'success' || Array.isArray(payload?.data?.list) || Array.isArray(payload?.list)) {
      const data = payload?.data || payload || {};
      allList.value = data.list || [];
      total.value = data.total || allList.value.length;
      if (data.orgPath) selectedOrg.pathName = data.orgPath;
      if (data.orgName) selectedOrg.orgName = data.orgName;
      const maxPage = Math.max(1, Math.ceil(total.value / pageSize) || 1);
      if (page.value > maxPage) page.value = maxPage;
    } else {
      ElMessage.error(payload?.info || payload?.message || '查询失败');
      allList.value = [];
      total.value = 0;
    }
  } catch (e) {
    ElMessage.error(e?.message || '查询失败');
  } finally {
    loading.value = false;
  }
}

function showDetail(row) {
  detailRow.value = row;
  detailVisible.value = true;
}
</script>

<style scoped>
.auth-admin-dir {
  --aad-org: #3b82f6;
  --aad-org-deep: #1d4ed8;
  --aad-org-soft: rgba(59, 130, 246, 0.12);
  --aad-place: #14b8a6;
  --aad-place-deep: #0f766e;
  --aad-text: #1f2937;
  --aad-muted: #94a3b8;
  --aad-line: #e8edf3;
  --aad-bg: #f3f5f8;
  height: 100%;
  min-height: 520px;
  max-height: calc(100vh - 100px);
  padding: 10px;
  box-sizing: border-box;
  overflow: hidden;
  background: var(--aad-bg);
}
.layout {
  height: 100%;
  min-height: 0;
}
.tree-pane {
  height: 100%;
  padding: 2px 2px 8px;
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
  min-height: 0;
}

/* 滑动分段：干净、好点 */
.mode-seg {
  --seg-pad: 3px;
  position: relative;
  display: grid;
  grid-template-columns: 1fr 1fr;
  flex-shrink: 0;
  margin-bottom: 8px;
  padding: var(--seg-pad);
  border-radius: 999px;
  background: #e9eef5;
  isolation: isolate;
}
.mode-seg__thumb {
  position: absolute;
  top: var(--seg-pad);
  bottom: var(--seg-pad);
  left: var(--seg-pad);
  width: calc(50% - var(--seg-pad));
  border-radius: 999px;
  background: #fff;
  box-shadow: 0 1px 3px rgba(15, 23, 42, 0.12), 0 1px 1px rgba(15, 23, 42, 0.04);
  transition: transform 0.22s cubic-bezier(0.22, 1, 0.36, 1), background 0.22s ease;
  z-index: 0;
}
.mode-seg[data-mode='place'] .mode-seg__thumb {
  transform: translateX(100%);
}
.mode-seg__btn {
  position: relative;
  z-index: 1;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  height: 34px;
  margin: 0;
  padding: 0 8px;
  border: 0;
  border-radius: 999px;
  background: transparent;
  color: #64748b;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: color 0.18s ease;
}
.mode-seg__btn .el-icon {
  font-size: 15px;
}
.mode-seg__btn.is-active {
  color: var(--aad-org-deep);
}
.mode-seg[data-mode='place'] .mode-seg__btn.is-active {
  color: var(--aad-place-deep);
}
.mode-hint {
  flex-shrink: 0;
  margin: 0 4px 10px;
  font-size: 12px;
  line-height: 1.4;
  color: var(--aad-muted);
}

.place-search {
  flex-shrink: 0;
  margin-bottom: 8px;
}
.place-tree-wrap,
.place-hit-wrap {
  flex: 1;
  min-height: 0;
  overflow: auto;
}
.place-tree {
  background: transparent;
}
.place-hit-row {
  padding: 8px 6px;
  border-radius: 8px;
  cursor: pointer;
}
.place-hit-row:hover {
  background: rgba(20, 184, 166, 0.08);
}
.place-hit-name {
  font-size: 13px;
  font-weight: 600;
  color: var(--aad-text);
}
.place-hit-path {
  margin-top: 2px;
  font-size: 12px;
  color: var(--aad-muted);
  word-break: break-all;
}
.tree-loading {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 16px 8px;
  color: var(--aad-muted);
  font-size: 13px;
}
.rhead-tools {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-left: auto;
  flex-wrap: wrap;
}
.tool-input {
  width: 160px;
}
.tool-input--sm {
  width: 130px;
}
.right-pane {
  height: 100%;
  min-width: 0;
  min-height: 0;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  padding: 4px 4px 0;
  box-sizing: border-box;
}
.status-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 10px;
  flex-shrink: 0;
  flex-wrap: wrap;
}

.sel-card {
  display: flex;
  align-items: center;
  gap: 12px;
  min-width: 0;
  flex: 1 1 240px;
  padding: 10px 12px;
  border-radius: 12px;
  background: #fff;
  border: 1px solid var(--aad-line);
  box-shadow: 0 1px 2px rgba(15, 23, 42, 0.04);
}
.sel-card__badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  flex-shrink: 0;
  padding: 4px 10px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 700;
  line-height: 1;
}
.sel-card.is-org .sel-card__badge {
  color: var(--aad-org-deep);
  background: rgba(59, 130, 246, 0.12);
}
.sel-card.is-place .sel-card__badge {
  color: var(--aad-place-deep);
  background: rgba(20, 184, 166, 0.12);
}
.sel-card__badge .el-icon {
  font-size: 13px;
}
.sel-card__body {
  min-width: 0;
  flex: 1;
}
.sel-card__name {
  font-size: 14px;
  font-weight: 650;
  color: var(--aad-text);
  line-height: 1.35;
}
.sel-card__path {
  margin-top: 2px;
  font-size: 12px;
  color: var(--aad-muted);
  line-height: 1.35;
  word-break: break-all;
}
.sel-card__placeholder {
  font-size: 13px;
  color: var(--aad-muted);
}

.tip-line {
  display: flex;
  align-items: flex-start;
  gap: 6px;
  margin-bottom: 10px;
  padding: 8px 10px;
  border-radius: 8px;
  background: #fff;
  color: #64748b;
  font-size: 12px;
  line-height: 1.5;
  flex-shrink: 0;
  border: 1px solid var(--aad-line);
}
.tip-line.is-org .el-icon {
  color: var(--aad-org);
}
.tip-line.is-place .el-icon {
  color: var(--aad-place);
}
.tip-line__count {
  margin-left: auto;
  flex-shrink: 0;
  white-space: nowrap;
  color: var(--aad-muted);
}
.tip-line__count b {
  font-size: 14px;
}
.tip-line.is-org .tip-line__count b {
  color: var(--aad-org-deep);
}
.tip-line.is-place .tip-line__count b {
  color: var(--aad-place-deep);
}
.tip-line .el-icon {
  margin-top: 2px;
  flex-shrink: 0;
}
.table-wrap {
  flex: 1;
  min-height: 0;
  overflow: hidden;
  border-radius: 8px;
  border: 1px solid #ebeef5;
  background: #fff;
}
.admin-table :deep(.el-table__inner-wrapper::before) {
  display: none;
}
.admin-table :deep(td.el-table__cell) {
  padding: 10px 0;
}
.name-cell {
  display: flex;
  align-items: center;
  gap: 8px;
}
.name-icon {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  font-size: 14px;
  color: var(--aad-org-deep);
  background: var(--aad-org-soft);
}
.name-icon--lg {
  width: 40px;
  height: 40px;
  font-size: 20px;
}
.name-text {
  font-weight: 500;
  color: var(--aad-text);
}
.role-text {
  color: #4a5568;
  font-size: 13px;
}
.tag-wrap {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  padding: 2px 0;
}
.mod-tag {
  margin: 0 !important;
}
.muted {
  color: var(--aad-muted);
  font-size: 13px;
}
.pager {
  margin-top: 10px;
  padding: 4px 0 6px;
  flex-shrink: 0;
  display: flex;
  justify-content: flex-end;
}
.detail-head {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
  padding-bottom: 12px;
  border-bottom: 1px solid #eef2f6;
}
.detail-head__name {
  font-size: 16px;
  font-weight: 600;
  color: var(--aad-text);
}
.detail-head__sub {
  margin-top: 4px;
  font-size: 13px;
  color: var(--aad-muted);
}
.org-tree :deep(.h-\[600px\]) {
  height: calc(100vh - 240px) !important;
  max-height: 680px;
}
.org-tree :deep(.el-input) {
  margin-bottom: 8px;
}
</style>
