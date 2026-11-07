<template>
    <div class="manauth-root">
        <teleport  :to="toolbarTo">
            <div class="manauth-toolbar">
                <el-button type="primary" plain :disabled="!menuViewHumanOid" @click="openMenuView">
                    <SvgIcon name="hugeicons:view" /> 查看菜单权限
                </el-button>
                <el-button type="success" @click="loadData" style="margin-left: 12px;"> <SvgIcon name="fa fa-refresh" /> 刷新</el-button>
                <el-button
                    v-if="man?.HR_HumanOID"
                    :type="isManActivated ? 'info' : 'success'"
                    plain
                    style="margin-left: 12px;"
                    @click="toggleManActivate"
                >
                    <SvgIcon :name="isManActivated ? 'fa fa-ban' : 'fa fa-check-circle'" />
                    {{ isManActivated ? '禁用' : '激活' }}
                </el-button>
                <el-button type="primary" @click="rmhisCacheU" style="margin-left: 12px;"><SvgIcon name="fa fa-refresh" /> 刷新他的权限</el-button>
                <el-button type="primary" @click="rmAllCacheU" style="margin-left: 12px;"><SvgIcon name="fa fa-refresh" /> 清空缓存</el-button>
                <el-button type="warning" @click="resetPwd" style="margin-left: 12px;" v-if="isBoss"><SvgIcon name="fa fa-key" /> 重置密码</el-button>
            </div>
        </teleport>

        <div v-if="$slots.header" class="manauth-header">
            <slot name="header"></slot>
        </div>

        <div v-for="(group, index) in rendGroups.groups" :key="index" class="group-card">
            <el-card class="group-panel" :class="'gp' + group.type">
                <template #header>
                    <div class="group-header">
                        {{ group.title }}
                        <!-- 添加添加按钮 -->
                        <el-button style="float: right; padding: 3px 0" type="text" @click="openAddDialog(group)">
                            添加
                        </el-button>
                    </div>
                </template>
                <div class="org-list">
                    <div v-for="org in group.children" :key="org.id" class="org-card">
                        <div class="org-card__header">
                            <div class="org-card__title">
                                <div class="org-card__name">{{ org.DW_OrgName }}</div>
                                <div v-if="org.DW_Note" class="org-card__sub">{{ org.DW_Note }}</div>
                            </div>

                            <div class="org-card__ops">
                                <div class="org-flag">
                                    <span class="org-flag__label">主岗</span>
                                    <el-switch size="small" v-model="org.DW_IsMain" @change="() => onOrgFlagChange(org)" />
                                </div>
                                <div class="org-flag">
                                    <span class="org-flag__label">负责人</span>
                                    <el-switch size="small" v-model="org.DW_IsDuty" @change="() => onOrgFlagChange(org)" />
                                </div>
                            </div>
                        </div>
                        <el-button
                            class="org-card__close"
                            :icon="Close"
                            circle
                            size="small"
                            type="danger"
                            plain
                            @click="removeWorker(org)"
                        />

                        <div class="org-card__body">
                            <div class="tag-list">
                                <tags :list="org.children" name="WT_Title" :closable="(row) => canCloseWorkorTypeTag(row, org)"
                                    @close="(event) => removeWType(event, org)" :addable="!disabled" action="dialog"
                                    title="选择管理员类别"
                                    width="760px"
                                    tagStyle="margin: 0 5px 5px 0;">
                                    <template #default="p">
                                        {{ p.row.WT_Title }}
                                    </template>
                                    <template #addpop="a">
                                        <!-- 原 checkbox 选择组件
                                        <worktypeselect @ok="doAddType(org, $event)" :added="getBindTypeOID(org.children)"
                                            :src="group.type"
                                            :api="api">
                                        </worktypeselect>
                                        -->
                                        <worktypeselectList @ok="doAddType(org, $event)" :added="getBindTypeOID(org.children)"
                                            :src="group.type"
                                            :orgOid="org.HH_Organize_FK"
                                            :api="api">
                                        </worktypeselectList>
                                    </template>
                                </tags>
                            </div>
                        </div>
                    </div>
                </div>
            </el-card>
        </div>
        <div>
            <tags :list="rendGroups.outed" name="WT_Title" :closable="canCloseWorkorTypeTag" @close="removeWType($event)"
                action="dialog" title="选择管理员类别" width="760px" tagStyle="margin: 0 5px 5px 0;">
                <template #default="p">
                    {{ p?.row?.WT_Title }}
                </template>
                <template #addpop="a">
                    <!-- 原 checkbox 选择组件
                    <worktypeselect @ok="doAddType(org, $event)" :added="getBindTypeOID(org.children)" :api="api">
                    </worktypeselect>
                    -->
                    <worktypeselectList @ok="doAddType(org, $event)" :added="getBindTypeOID(org.children)" :api="api">
                    </worktypeselectList>
                </template>
            </tags>
        </div>

        <div class="manauth-help">
            <div class="manauth-help__title">使用说明</div>
            <div class="manauth-help__body">
                <p>
                    当前页面用于配置用户个人的权限。权限组织分为 <b>行政组织</b>、<b>矿区地点</b> 两种。
                </p>
                <ul class="manauth-help__list">
                    <li>
                        <b>行政组织</b>：全集团通用的、按人力资源管理口径维护的组织体系。
                    </li>
                    <li>
                        <b>矿区地点</b>：矿区（尤其是煤矿井下）各类地点主数据。
                    </li>
                </ul>
                <p>
                    在相应的行政组织/矿区地点下配置权限后，登录人将获得该组织/地点的数据范围；随后在组织卡片内点击
                    <b>+</b> 选择模块功能范围，即可设置人员的模块使用范围。
                </p>
                <p class="manauth-help__flags">
                    组织/矿区旁边的两个开关含义如下：
                </p>
                <ul class="manauth-help__list">
                    <li>
                        <b>主岗</b>：设置为主岗时，人员进入模块后默认显示或使用该组织/地点的数据。
                    </li>
                    <li>
                        <b>负责人</b>：设置为负责人时，作为后续审批功能的依据使用。
                    </li>
                </ul>
            </div>
        </div>

        <div v-if="isBoss && man?.HR_HumanOID" class="manauth-changelog">
            <div class="manauth-changelog__title">授权变更记录</div>
            <el-table
                v-loading="changeLogLoading"
                :data="changeLogList"
                size="small"
                stripe
                border
                empty-text="暂无变更记录"
            >
                <el-table-column prop="AL_OpTime" label="时间" width="168">
                    <template #default="{ row }">{{ formatChangeLogTime(row) }}</template>
                </el-table-column>
                <el-table-column prop="AL_Src" label="来源" width="96">
                    <template #default="{ row }">{{ changeLogSrcLabel(row.AL_Src) }}</template>
                </el-table-column>
                <el-table-column prop="AL_Action" label="动作" width="140">
                    <template #default="{ row }">{{ changeLogActionLabel(row.AL_Action) }}</template>
                </el-table-column>
                <el-table-column prop="AL_Summary" label="摘要" min-width="260" show-overflow-tooltip />
                <el-table-column prop="AL_OpMan" label="变更人" width="120" show-overflow-tooltip />
                <el-table-column label="可恢复" width="80" align="center">
                    <template #default="{ row }">
                        <el-tag v-if="row.AL_HasRestore" type="success" size="small">是</el-tag>
                        <span v-else>-</span>
                    </template>
                </el-table-column>
                <el-table-column label="操作" width="160" fixed="right">
                    <template #default="{ row }">
                        <el-button link type="primary" size="small" @click="openChangeLogDetail(row)">详情</el-button>
                        <el-button
                            v-if="row.AL_HasRestore"
                            link
                            type="primary"
                            size="small"
                            @click="copyChangeLogRestoreSql(row)"
                        >复制SQL</el-button>
                    </template>
                </el-table-column>
            </el-table>
            <div class="manauth-changelog__pager">
                <el-pagination
                    :current-page="changeLogPage"
                    :page-size="changeLogPageSize"
                    :total="changeLogTotal"
                    :page-sizes="[10, 20, 50, 100]"
                    layout="total, sizes, prev, pager, next"
                    background
                    small
                    @current-change="onChangeLogPageChange"
                    @size-change="onChangeLogSizeChange"
                />
            </div>
        </div>

        <el-drawer v-model="changeLogDrawerVisible" title="授权变更详情" size="520px" destroy-on-close>
            <div v-if="changeLogCurrent" class="manauth-changelog-detail">
                <p><b>摘要：</b>{{ changeLogCurrent.AL_Summary }}</p>
                <p><b>变更人：</b>{{ changeLogCurrent.AL_OpMan }}（{{ changeLogCurrent.AL_OpManCode || '-' }}）</p>
                <p><b>时间：</b>{{ formatChangeLogTime(changeLogCurrent) }}</p>
                <p v-if="changeLogCurrent.AL_BatchNo"><b>批次：</b>{{ changeLogCurrent.AL_BatchNo }}</p>
                <div class="manauth-changelog-detail__json">
                    <div class="manauth-changelog-detail__json-title">DetailJson</div>
                    <pre>{{ formatChangeLogDetailJson(changeLogCurrent.AL_DetailJson) }}</pre>
                </div>
                <div v-if="changeLogCurrent.AL_RestoreSql" class="manauth-changelog-detail__json">
                    <div class="manauth-changelog-detail__json-title">
                        RestoreSql
                        <el-button link type="primary" size="small" @click="copyChangeLogRestoreSql(changeLogCurrent)">复制</el-button>
                    </div>
                    <pre>{{ changeLogCurrent.AL_RestoreSql }}</pre>
                </div>
            </div>
        </el-drawer>

        <!-- 原：VC_MD_Tree 单选点击即提交
        <el-dialog v-model="orgsimDialogVisible" :title="`分配地点主数据`" width="30%">
            <VC_MD_Tree :api="treeApi"
                    :showTitle="false"
                    title="导航树"
                    ref="VC_MD_TreeBody"
                    :expandLevel="0"
                    @node-click="onPlaceClick"
                    style="height:100%;"
                    disabled
                    :pageSize="30"
                    :flex="true"
                    height="100%"
                >
                </VC_MD_Tree>
            <template #footer>
                <span class="dialog-footer">
                    <el-button @click="orgsimDialogVisible = false">取消</el-button>
                </span>
            </template>
        </el-dialog>
        -->
        <el-dialog v-model="orgsimDialogVisible" :title="`分配地点主数据`" width="30%" @closed="resetPlaceDialog">
            <manauthPlaceTree
                ref="placeTreeRef"
                :api="treeApi"
                :expandLevel="0"
                height="420px"
                @check-change="syncSelectAllMines"
                @node-click="onPlaceClick"
                bpo="BPO_MineSelect"
            />
            <template #footer>
                <div class="place-dialog-footer">
                    <el-checkbox v-model="selectAllMines" @change="onSelectAllMinesChange">全选矿井</el-checkbox>
                    <span class="place-dialog-footer__btns">
                        <el-button type="primary" @click="submitPlaceSelection">提交</el-button>
                        <el-button @click="orgsimDialogVisible = false">取消</el-button>
                    </span>
                </div>
            </template>
        </el-dialog>
        <!-- 原：点击节点即添加并关闭
        <el-dialog v-model="seeHROrg" :title="`分配行政组织`" width="30%">
            <hrtreeV2 @nodeclick="addHROrg" authcode="BPO_UseAuthTo" useAuth :api="api" :page="pageData">
            </hrtreeV2>
            <template #footer>
                <span class="dialog-footer">
                    <el-button @click="seeHROrg = false">取消</el-button>
                </span>
            </template>
        </el-dialog>
        -->
        <el-dialog v-model="seeHROrg" :title="`分配行政组织`" width="30%" top="5vh">
            <!-- <hrtree @nodeclick="addHROrg" authcode="BPO_UseAuthTo" useAuth :api="api" :page="pageData">
            </hrtree> -->
            <hrtreeV2
                :key="hrOrgTreeKey"
                ref="hrOrgTreeRef"
                showCheck
                strict
                :checked="hrOrgPreChecked"
                authcode="BPO_UseAuthTo"
                useAuth
                :api="api"
                :page="pageData"
                @nodeclick="addHROrg"
                @orgcheck="onHROrgCheck"
            />
            <template #footer>
                <span class="dialog-footer">
                    <el-button type="primary" :disabled="!hrOrgCheckedNodes.length" @click="confirmHROrgs" style="margin-right: 10px;">
                        确定添加{{ hrOrgCheckedNodes.length ? `（${hrOrgCheckedNodes.length}）` : '' }}
                    </el-button>
                    <el-button @click="seeHROrg = false">取消</el-button>
                </span>
            </template>
        </el-dialog>

		<WorkorRoleMenuViewDialog
			v-model="menuViewOpened"
			mode="human"
			:row="menuViewRow"
			@close="closeMenuView"
		/>
    </div>
</template>

<script setup>
import { useSlots, useAttrs, provide, inject, computed, ref, onMounted, onBeforeMount, onActivated, reactive, watch, nextTick } from 'vue'
import { useAuthStore } from '@/store/modules/auth';
import { ElMessage } from 'element-plus'
import { Close } from '@element-plus/icons-vue'


const authStore = useAuthStore();

const props = defineProps({
    api: null,
    bus: null,
    page: null,
    oid: null,
    man:null,//被授权人
    toolbarTo: { type: [String, Object], default: "" }, // 允许外部将顶部工具栏迁移到指定容器
})

const emit = defineEmits(['change', 'delete', 'refresh'])

const count = defineModel("count", { type: Number, default: 0 })

import { useManyEditor } from "@/uiext/gene/hook/manyeditor.js"
import tags from "@/ui/common/tags.vue"
// import worktypeselect from "./uipart/worktypeselect.vue" // 原 checkbox 选择组件
import worktypeselectList from "./uipart/worktypeselectList.vue"

// import hrtree from "@m/src/uiext/org/hrtree.vue"
import hrtreeV2 from "../../uiext/org/hrtreeV2.vue"
// import VC_MD_Tree from "@m/src/views/sln_BusiBase/VC/TreeVC/place/VC_MD_Tree.vue"
import manauthPlaceTree from "./uipart/manauthPlaceTree.vue"
import * as treeAPIRaw from "../../api/BPO_Treeapi.js"
import WorkorRoleMenuViewDialog from "./auth/WorkorRoleMenuViewDialog.vue"

const manyHolder = useManyEditor();
const loading = ref(false);
const wklist = ref([]);
const scopelist = ref([]);
const changeLogList = ref([]);
const changeLogLoading = ref(false);
const changeLogDrawerVisible = ref(false);
const changeLogCurrent = ref(null);
const changeLogPage = ref(1);
const changeLogPageSize = ref(20);
const changeLogTotal = ref(0);
let loadReqSeq = 0;
let saveWkTimer = null;

const treeApi= function(){
	return Object.assign({}, treeAPIRaw);
}();

const isBoss = computed(() => {
    const fromPage = props.page?.user?.account ?? props.page?.user?.Account;
    const fromStore = authStore.userInfo?.userId ?? authStore.userInfo?.userAccount;
    const acc = `${fromPage || fromStore || ''}`.trim().toLowerCase();
    return acc === 'superadmin';
});

const changeLogActionMap = {
    AddOrg: '新增授权组织',
    RemoveOrg: '删除授权组织',
    AddRole: '新增角色',
    RemoveRole: '删除角色',
    SetMain: '设置主岗',
    SetDuty: '设置负责人',
    MdmOrgClear: '主数据部门变更清权',
};
const changeLogSrcMap = {
    UI: '页面操作',
    MDM: '主数据同步',
    SYS: '系统',
};

function changeLogActionLabel(action) {
    return changeLogActionMap[action] || action || '-';
}
function changeLogSrcLabel(src) {
    return changeLogSrcMap[src] || src || '-';
}
function formatChangeLogTime(row) {
    const t = row?.AL_OpTime || row?.SYS_Created;
    if (!t) return '-';
    const d = new Date(t);
    if (Number.isNaN(d.getTime())) return String(t);
    const pad = (n) => `${n}`.padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`;
}
function formatChangeLogDetailJson(json) {
    if (!json) return '';
    try {
        return JSON.stringify(JSON.parse(json), null, 2);
    } catch {
        return json;
    }
}
function openChangeLogDetail(row) {
    changeLogCurrent.value = row;
    changeLogDrawerVisible.value = true;
}
async function copyChangeLogRestoreSql(row) {
    const sql = row?.AL_RestoreSql;
    if (!sql) {
        ElMessage.warning('无可复制的恢复脚本');
        return;
    }
    try {
        await navigator.clipboard.writeText(sql);
        ElMessage.success('已复制恢复脚本');
    } catch {
        ElMessage.error('复制失败，请手动复制');
    }
}
async function loadChangeLogList() {
    if (!isBoss.value || !props.man?.HR_HumanOID || !props.api?.queryManAuthChangeLogList) {
        changeLogList.value = [];
        changeLogTotal.value = 0;
        return;
    }
    changeLogLoading.value = true;
    try {
        const res = await props.api.queryManAuthChangeLogList({
            humanOid: props.man.HR_HumanOID,
            pageSize: changeLogPageSize.value,
            pageNum: changeLogPage.value,
        });
        const payload = res?.data !== undefined ? res : (res?.result ?? res);
        changeLogList.value = Array.isArray(payload?.data)
            ? payload.data
            : (Array.isArray(payload?.Items) ? payload.Items : (Array.isArray(payload) ? payload : []));
        changeLogTotal.value = Number(payload?.total ?? payload?.Total ?? changeLogList.value.length) || 0;
    } catch (err) {
        changeLogList.value = [];
        changeLogTotal.value = 0;
    } finally {
        changeLogLoading.value = false;
    }
}

function onChangeLogPageChange(page) {
    changeLogPage.value = page;
    loadChangeLogList();
}

function onChangeLogSizeChange(size) {
    changeLogPageSize.value = size;
    changeLogPage.value = 1;
    loadChangeLogList();
}

/** 无 org 时的可分配集合（仅普通 Boss 的 Child） */
const assignableTypeIdSet = ref(new Set());
/** orgOid(lower) -> 该卡可分配集合（含命中的单卡片 Child） */
const assignableByOrg = ref({});

function toTypeIdKey(id) {
    return id == null || id === '' ? null : String(id).toLowerCase();
}

function unwrapWorkTypeList(res) {
    if (Array.isArray(res)) return res;
    if (Array.isArray(res?.data)) return res.data;
    if (Array.isArray(res?.result)) return res.result;
    return [];
}

function collectTypeIds(res) {
    const set = new Set();
    unwrapWorkTypeList(res).forEach((li) => {
        const id = toTypeIdKey(li?.HH_WorkorTypeOID);
        if (id) set.add(id);
    });
    return set;
}

async function loadAssignableTypeIds() {
    if (isBoss.value) {
        assignableTypeIdSet.value = new Set();
        assignableByOrg.value = {};
        return;
    }
    if (!props.api?.loadWorkTypeSelect) return;
    try {
        const [r1, r2] = await Promise.all([
            props.api.loadWorkTypeSelect({ src: '1' }),
            props.api.loadWorkTypeSelect({ src: '2' }),
        ]);
        const globalSet = new Set([...collectTypeIds(r1), ...collectTypeIds(r2)]);
        assignableTypeIdSet.value = globalSet;

        const byOrg = {};
        const orgs = wklist.value || [];
        await Promise.all(
            orgs.map(async (org) => {
                const oid = org?.HH_Organize_FK;
                if (!oid) return;
                const key = toTypeIdKey(oid);
                if (!key || byOrg[key]) return;
                const src = org.DW_OrgSrc || undefined;
                const res = await props.api.loadWorkTypeSelect({ src, orgOid: oid });
                byOrg[key] = collectTypeIds(res);
            }),
        );
        assignableByOrg.value = byOrg;
    } catch (e) {
        assignableTypeIdSet.value = new Set();
        assignableByOrg.value = {};
    }
}

/** 超级管理员可删任意；其他人仅可删当前卡片可分配范围内的类别 */
function canCloseWorkorTypeTag(row, org) {
    if (isBoss.value) return true;
    const id = toTypeIdKey(row?.HH_WorkorType_FK);
    if (id == null) return false;
    const orgKey = toTypeIdKey(org?.HH_Organize_FK || row?.HH_Org_FK);
    if (orgKey && assignableByOrg.value[orgKey]) {
        return assignableByOrg.value[orgKey].has(id);
    }
    return assignableTypeIdSet.value.has(id);
}

function isRowActivated(row) {
    const v = row?.H_Disabled;
    return v === true || v === 1 || v === '1' || v === 'true';
}

const isManActivated = computed(() => isRowActivated(props.man));

const menuViewOpened = ref(false);
const menuViewRow = ref(null);
const menuViewHumanOid = computed(() => props.man?.HR_HumanOID || props.oid || null);

function openMenuView() {
    const oid = menuViewHumanOid.value;
    if (!oid) {
        swal({ text: '缺少人员主键，无法查看', icon: 'info', button: '关闭' });
        return;
    }
    menuViewRow.value = {
        HR_HumanOID: oid,
        H_Name: props.man?.H_Name,
    };
    menuViewOpened.value = true;
}
function closeMenuView() {
    menuViewOpened.value = false;
    menuViewRow.value = null;
}

function toggleManActivate() {
    const row = props.man;
    if (!row?.HR_HumanOID) {
        swal({ text: '缺少人员主键，无法操作', icon: 'error', button: '关闭' });
        return;
    }

    const activated = isRowActivated(row);
    const nextState = activated ? 0 : 1;
    const cap = activated ? '禁用' : '激活';
    const tip = activated ? '确认要禁用该人员吗？' : '确认要激活该人员吗？';

    alertIsOK(tip, function (e) {
        if (e !== 'yes') return;
        props.api.toggleUserState({
            contactoid: row.HR_HumanOID,
            newstate: nextState,
        }).then((res) => {
            const msg = res?.data || `${cap}操作成功`;
            swal({ text: msg, icon: 'success', button: '关闭' });
            props.bus?.hub?.fire('refresh');
            emit('refresh');
        }).catch((err) => {
            swal({ text: err.message, icon: 'error', button: '关闭' });
        });
    }, cap, activated, 'warning');
}

function loadData() {
    if (!props.oid) return;
    const currentReqSeq = ++loadReqSeq;
    loading.value = true;
    var para = {
        masterFK: props.oid,
        pageSize: 1000,
    };
    var p1 = props.api.queryBCHHDivWorkerlist(para).then((res) => {
        if (currentReqSeq !== loadReqSeq) return;
        wklist.value = res.Items;
        manyHolder.initList("wklist", res.Items, "HH_DivWorkerOID");

    })
        .catch((err) => {
            swal({ text: err.message, icon: "error", button: "关闭", });

        });

    var p2 = props.api.queryBCHHWorkorOfScopelist(para).then((res) => {
        if (currentReqSeq !== loadReqSeq) return;
        scopelist.value = res.Items;
        manyHolder.initList("scopelist", res.Items, "HH_WorkorScopeOID");
    })

    Promise.all([p1, p2]).then(() => {
        if (currentReqSeq !== loadReqSeq) return;
        loading.value = false;
        loadAssignableTypeIds();
        loadChangeLogList();
    })

}
// 保存修改
function saveWKChangeByOrg(refresh,val) {
    var tar = manyHolder.compareList("wklist", val);
    if (tar.length == 0) {
        swal({ text: "没有变更的信息", icon: "info", button: "关闭", });
        return;
    }
    console.log(props.api,'api');
    props.api.saveBCHHDivWorkerList(tar).then((res) => {
        ElMessage.success({message:res.data,type:'success',placement: 'top-right'});
        if (refresh !== false) {
            loadData();
        }
    });
}

function saveWKChange(refresh) {
    var tar = manyHolder.compareList("wklist", wklist.value);
    if (tar.length == 0) {
        swal({ text: "没有变更的信息", icon: "info", button: "关闭", });
        return;
    }
    console.log(props.api,'api');
    props.api.saveBCHHDivWorkerList(tar).then((res) => {
        swal({ text: res.data, icon: "info", button: "关闭", });
        if (refresh !== false) {
            loadData();
        }
    });
}

function onOrgFlagChange(org) {
    // 轻量防抖，避免频繁点击触发多次保存
    if (saveWkTimer) clearTimeout(saveWkTimer);
    saveWkTimer = setTimeout(() => {
        saveWkTimer = null;
        saveWKChangeByOrg(true,[org]);
    }, 300);
}
function saveScopeChange() {
    var tar = manyHolder.compareList("scopelist", scopelist.value);
    if (tar.length == 0) {
        swal({ text: "没有变更的信息", icon: "info", button: "关闭", });
        return;
    }
    props.api.saveBCHHWorkorOfScopeList(tar).then((res) => {
        swal({ text: res.data, icon: "info", button: "关闭", });
        loadData();
    });
}



function createdTimeMs(row) {
    const v = row?.SYS_Created ?? row?.WT_Created;
    if (v == null || v === '') return Number.MAX_SAFE_INTEGER;
    const t = new Date(v).getTime();
    return Number.isFinite(t) ? t : Number.MAX_SAFE_INTEGER;
}

/** 创建时间升序；同时间再用 Order / 标题兜底 */
function sortByCreatedAsc(list) {
    return [...(list || [])].sort((a, b) => {
        const d = createdTimeMs(a) - createdTimeMs(b);
        if (d !== 0) return d;
        const oa = a?.DW_Order ?? a?.WT_Order ?? 0;
        const ob = b?.DW_Order ?? b?.WT_Order ?? 0;
        if (oa !== ob) return oa - ob;
        const na = a?.DW_OrgName || a?.WT_Title || '';
        const nb = b?.DW_OrgName || b?.WT_Title || '';
        return String(na).localeCompare(String(nb), 'zh-CN');
    });
}

const rendGroups = computed(() => {
    var org = [];
    var budget = [];
    var account = [];
    var gotedWTOID = [];
    wklist.value.forEach((li, i) => {
        var group = Object.assign({}, li);
        var sco = scopelist.value.filter((sc) => {
            var isHe = sc.HH_DivWorker_FK == li.HH_DivWorkerOID;
            if (isHe) {
                gotedWTOID.push(sc.HH_DivWorker_FK);
            }
            return isHe;
        });
        group.children = sortByCreatedAsc(sco);
        if (group.DW_OrgSrc == "1") {
            org.push(group);
        }
        if (group.DW_OrgSrc == "2") {
            budget.push(group);
        }
        if (group.DW_OrgSrc == "3") {
            account.push(group);
        }

    })

    var outedTypes = [];
    scopelist.value.forEach((scope) => {
        if (gotedWTOID.indexOf(scope.HH_DivWorker_FK) < 0) {
            outedTypes.push(scope);
        }
    })

    var res = {
        outed: sortByCreatedAsc(outedTypes),
        groups: [
            { type: "1", title: "行政组织", children: sortByCreatedAsc(org) },
            { type: "2", title: "矿区地点", children: sortByCreatedAsc(budget) },
            //{ type:"3",title:"核算主体", children:account},
        ]
    };
    console.log(res);
    return res;
})



// 控制 orgsim 弹窗显示隐藏
const orgsimDialogVisible = ref(false);
const placeTreeRef = ref(null);
const selectAllMines = ref(false);
// 控制原弹窗显示隐藏
const dialogVisible = ref(false);
// 记录当前点击的分组索引
const currentGroup = ref(null);

// 打开弹窗
const openAddDialog = (group) => {
    currentGroup.value = group;
    if (group.type == "1") {
        // seeHROrg.value = true;
        hrOrgCheckedNodes.value = [];
        hrOrgPreChecked.value = [];
        hrOrgTreeKey.value += 1;
        seeHROrg.value = true;
    }
    else if (group.type == "2") {
        // 原逻辑：直接打开弹窗，点击节点即提交
        // orgsimDialogVisible.value = true;
        selectAllMines.value = false;
        orgsimDialogVisible.value = true;
        nextTick(() => {
            placeTreeRef.value?.clearChecked?.();
        });
    }
    else {
        seeBudgetOrg.value = true;
    }
};

function resetPlaceDialog() {
    selectAllMines.value = false;
    placeTreeRef.value?.clearChecked?.();
}

function syncSelectAllMines() {
    selectAllMines.value = placeTreeRef.value?.isAllRootChecked?.() ?? false;
}

function onSelectAllMinesChange(checked) {
    placeTreeRef.value?.toggleAllRoot?.(checked);
    syncSelectAllMines();
}

function buildPlaceWorkerRow(rec) {
    const user = authStore.userInfo;
    return {
        DW_Name: props.man.H_Name,
        DW_Card: props.man.H_MainCode,
        DW_IsOn: true,
        DW_IsMain: false,
        DW_IsDuty: false,
        DW_GiveMan: user.userName,
        DW_GiveMancode: user.userId,
        HH_User_FK: props.oid,
        HH_Organize_FK: rec.MD_TreeOID,
        DW_OrgSrc: "2",
        DW_OrgName: rec.MD_Name,
        DW_Note: rec.MD_FullName,
    };
}

function savePlaceWorkers(toAdd, skippedNames, closeDialog) {
    if (toAdd.length === 0) return;
    let needMain = !checkMainOrg("2");
    toAdd.forEach((rec) => {
        const row = buildPlaceWorkerRow(rec);
        if (needMain) {
            row.DW_IsMain = true;
            needMain = false;
        }
        wklist.value.push(row);
    });

    const tar = manyHolder.compareList("wklist", wklist.value);
    if (tar.length === 0) {
        swal({ text: "没有变更的信息", icon: "info", button: "关闭", });
        return;
    }

    props.api.saveBCHHDivWorkerList(tar).then((res) => {
        let msg = res.data || "保存成功";
        if (skippedNames?.length > 0) {
            msg += `\n以下地点已添加，已跳过：${skippedNames.join("、")}`;
        }
        swal({ text: msg, icon: "info", button: "关闭", });
        if (closeDialog) {
            orgsimDialogVisible.value = false;
        }
        loadData();
    });
}

function onPlaceClick(node) {
    const rec = node?.Record;
    if (!rec?.MD_TreeOID) return;
    if (isOrgDuplicate(rec.MD_TreeOID, "2")) {
        swal({
            text: `该矿区地点已存在，请勿重复添加：${rec.MD_Name || rec.MD_FullName}`,
            icon: "error",
            button: "关闭",
        });
        return;
    }
    savePlaceWorkers([rec], [], true);
}

// 原：VC_MD_Tree 点击节点直接提交
// const onPlaceClick = (data) => {
//     console.log(data)
//     var row = data.Record;
//     if (isOrgDuplicate(row.MD_TreeOID, "2")) {
//         swal({ text: "该矿区地点已存在，请勿重复添加", icon: "error", button: "关闭", });
//         return;
//     }
//     var user = authStore.userInfo;
//     var row = {
//         DW_Name: props.man.H_Name,
//         DW_Card: props.man.H_MainCode,
//         DW_IsOn: true,
//         DW_IsMain: false,
//         DW_IsDuty: false,
//         DW_GiveMan: user.userName,
//         DW_GiveMancode: user.userId,
//         HH_User_FK: props.oid,
//         HH_Organize_FK: row.MD_TreeOID,
//         DW_OrgSrc: "2",
//         DW_OrgName: row.MD_Name,
//         DW_Note: row.MD_FullName,
//     };
//     if (checkMainOrg(row.DW_OrgSrc) == false) {
//         row.DW_IsMain = true;
//     }
//     wklist.value.push(row);
//     saveWKChange();
//     seeHROrg.value = false;
//     orgsimDialogVisible.value = false;
// };

function submitPlaceSelection() {
    const nodes = placeTreeRef.value?.getSelectableCheckedNodes?.() || [];
    if (nodes.length === 0) {
        swal({ text: "请先勾选要分配的矿区地点", icon: "info", button: "关闭", });
        return;
    }

    const duplicates = [];
    const toAdd = [];
    nodes.forEach((node) => {
        const rec = node.Record;
        if (isOrgDuplicate(rec.MD_TreeOID, "2")) {
            duplicates.push(rec.MD_Name || rec.MD_FullName || rec.MD_TreeOID);
        } else {
            toAdd.push(rec);
        }
    });

    if (toAdd.length === 0) {
        swal({
            text: `所选地点均已添加，无法重复分配：${duplicates.join("、")}`,
            icon: "error",
            button: "关闭",
        });
        return;
    }

    savePlaceWorkers(toAdd, duplicates, true);
};
function checkMainOrg(src) {
    var mainOrg = wklist.value.find((x) => x.DW_OrgSrc == src && x.DW_IsMain == true);
    if (mainOrg) {
        return true;
    }
    return false;
}

function isOrgDuplicate(orgFK, orgSrc) {
    if (!orgFK) return false;
    const fk = String(orgFK).toUpperCase();
    return wklist.value.some(
        (x) => String(x.HH_Organize_FK || "").toUpperCase() === fk && String(x.DW_OrgSrc) === String(orgSrc)
    );
}

const seeHROrg = ref(false);
const hrOrgTreeRef = ref(null);
const hrOrgTreeKey = ref(0);
const hrOrgCheckedNodes = ref([]);
const hrOrgPreChecked = ref([]);

function onHROrgCheck(_node, tree) {
    hrOrgCheckedNodes.value = tree?.checkedNodes ?? [];
}

function buildHROrgRow(data) {
    const user = authStore.userInfo;
    const row = {
        DW_Name: props.man.H_Name,
        DW_Card: props.man.H_MainCode,
        DW_IsOn: true,
        DW_IsMain: false,
        DW_IsDuty: false,
        DW_GiveMan: user.userName,
        DW_GiveMancode: user.userId,
        HH_User_FK: props.oid,
        HH_Organize_FK: data.HR_OrganizeOID,
        DW_OrgSrc: '1',
        DW_OrgName: data.O_OrgName,
    };
    if (checkMainOrg(row.DW_OrgSrc) === false) {
        row.DW_IsMain = true;
    }
    return row;
}

// 原：点击节点即添加并关闭
// function addHROrg(data) {
//     console.log(data)
//     if (isOrgDuplicate(data.HR_OrganizeOID, "1")) {
//         swal({ text: "该行政组织已存在，请勿重复添加", icon: "error", button: "关闭", });
//         return;
//     }
//     var user = authStore.userInfo;
//     var row = {
//         DW_Name: props.man.H_Name,
//         DW_Card: props.man.H_MainCode,
//         DW_IsOn: true,
//         DW_IsMain: false,
//         DW_IsDuty: false,
//         DW_GiveMan: user.userName,
//         DW_GiveMancode: user.userId,
//         HH_User_FK: props.oid,
//         HH_Organize_FK: data.HR_OrganizeOID,
//         DW_OrgSrc: "1",
//         DW_OrgName: data.O_OrgName,
//     };
//     if (checkMainOrg(row.DW_OrgSrc)==false) {
//         row.DW_IsMain=true;
//     }
//     wklist.value.push(row);
//     saveWKChange();
//     seeHROrg.value = false;
// }

/** 点击节点名称：立即添加并关闭（保留原交互） */
function addHROrg(data) {
    if (isOrgDuplicate(data.HR_OrganizeOID, '1')) {
        swal({ text: '该行政组织已存在，请勿重复添加', icon: 'error', button: '关闭' });
        return;
    }
    wklist.value.push(buildHROrgRow(data));
    saveWKChange();
    seeHROrg.value = false;
}

/** 勾选后点确定：批量添加 */
function confirmHROrgs() {
    const nodes = hrOrgCheckedNodes.value ?? [];
    if (!nodes.length) {
        swal({ text: '请至少勾选一个组织，或直接点击组织名称添加', icon: 'info', button: '关闭' });
        return;
    }
    let added = 0;
    let skipped = 0;
    nodes.forEach((data) => {
        if (!data?.HR_OrganizeOID) {
            return;
        }
        if (isOrgDuplicate(data.HR_OrganizeOID, '1')) {
            skipped += 1;
            return;
        }
        wklist.value.push(buildHROrgRow(data));
        added += 1;
    });
    if (!added) {
        swal({
            text: skipped ? '所选组织均已存在，请重新选择' : '请至少勾选一个组织',
            icon: 'info',
            button: '关闭',
        });
        return;
    }
    saveWKChange();
    seeHROrg.value = false;
    if (skipped) {
        ElMessage.warning(`已添加 ${added} 个，跳过重复 ${skipped} 个`);
    }
}


const seeBudgetOrg = ref(false);
function addBudgetOrg(data) {
    console.log(data)
    var bc = bpo.BC_HH_DivWorkerBase;
    var man = bpo.BC_UserAuthedBase;
    bc.Insert();
    bc.setFieldValue("DW_Name", man.getFieldValue("PersonName"));
    bc.setFieldValue("DW_Card", man.getFieldValue("MainCode"));
    bc.setFieldValue("DW_IsOn", true);
    bc.setFieldValue("DW_IsMain", false);
    bc.setFieldValue("DW_IsDuty", false);
    bc.setFieldValue("DW_GiveMan", bpo.getUserInfo().personName);
    bc.setFieldValue("DW_GiveMancode", bpo.getUserInfo().userID);
    bc.setFieldValue("HH_User_FK", man.getOID());
    bc.setFieldValue("HH_Organize_FK", data.SK_BudgetOrgOID);
    bc.setFieldValue("DW_OrgSrc", "2");
    bc.setFieldValue("DW_OrgName", data.BO_Nick);

    bpo.BusinessSubmit();
    seeBudgetOrg.value = false;
}

function doAddType(org, list) {


    list.forEach((li) => {
        var row = {
            HH_DivWorker_FK: org.HH_DivWorkerOID,
            HH_WorkorType_FK: li.HH_WorkorTypeOID,
            HH_User_FK: props.oid,
            HH_Org_FK: org.HH_Organize_FK,
            WT_Title: li.WT_Title,
            WT_Created: li.WT_Created || new Date().toISOString(),
            WT_Order: li.WT_Order,
        };
        scopelist.value.push(row);
    })
    //bpo.BusinessSubmit();
    saveScopeChange();
    // 分配角色后自动刷新权限
    rmhisCacheU();
}

function getBindTypeOID(list) {
    return list.map((li) => {
        return li.HH_WorkorType_FK;
    })
}

async function removeWorker(org) {
    const children = org.children || [];
    if (children.some((row) => !canCloseWorkorTypeTag(row))) {
        swal({ text: "该组织下存在无权删除的角色权限，无法删除", icon: "warning", button: "关闭" });
        return;
    }
    const hasScopes = children.length > 0;
    const confirmText = hasScopes
        ? "确认清空角色权限并删除该授权组织吗？"
        : "确认要操作吗？将删除授权组织！";
    alertIsOK(confirmText, function (e) {
        if (e == "yes") {
            props.api.removeBCHHDivWorkerByIds([org.HH_DivWorkerOID]).then((res) => {
                loadData();
                swal({ text: res.data, icon: "success", button: "关闭", });
            });
        }
    }, org.DW_OrgName);
}

function removeWType(row, org) {
    if (!canCloseWorkorTypeTag(row)) {
        swal({ text: "无权删除该管理员类别，仅可操作自己可分配的类别", icon: "info", button: "关闭" });
        return;
    }
    const para = [row.HH_WorkorScopeOID];

    alertIsOK("确认要操作吗？将删除当前行数据！", function (e) {

        if (e == "yes") {
            props.api.removeBCHHWorkorOfScopeByIds(para).then((res) => {
                // 删除权限后自动刷新权限
                rmhisCacheU();
                loadData();
                swal({ text: res.data, icon: "success", button: "关闭", });
                
            });
        }
    }, row.WT_Title);


}

function rmhisCacheU(key) {
    //更新个人的兼职类型，传人的OID
    // console.log(bpo.BC_UserAuthedBase.getOID(),"更新个人的兼职类型，传人的OID");
    // console.log(props.man, 'props.man');
    // console.log(key, 'key');
    if(!props.man?.H_MainCode){
        swal({ text: "请先选择人员", icon: "error", button: "关闭", });
        return;
    }
    // var code = VC_UserAuthedEditFocusRow.value.MainCode
    var code = props.man?.H_MainCode
    props.api.deleteHisAllKeyU({
        key: code,
    })
        .then((t) => {

            try {

                if (t.status == "success") {
                    swal({ text: t.data, icon: "success", button: "关闭", });
                    //loadCache()
                }
                else {
                    swal({ text: t.data, icon: "error", button: "关闭", });
                }
            }
            catch (e) { }
            //context.close();
        });
}

//清空缓存 clearAllCache
function rmAllCacheU(key) {
    //更新个人的兼职类型，传人的OID
    // console.log(bpo.BC_UserAuthedBase.getOID(),"更新个人的兼职类型，传人的OID");
    // var code = VC_UserAuthedEditFocusRow.value.MainCode
    props.api.clearAllCache({ a: 1 })
        .then((t) => {

            try {

                if (t.status == "success") {
                    swal({ text: t.data, icon: "success", button: "关闭", });
                    //loadCache()
                }
                else {
                    swal({ text: t.data, icon: "error", button: "关闭", });
                }
            }
            catch (e) { }
            //context.close();
        });
}

watch(
    () => [props.oid, props.man?.H_MainCode, props.man?.H_Name, isBoss.value],
    ([newOid]) => {
        if (!newOid) {
            wklist.value = [];
            scopelist.value = [];
            changeLogList.value = [];
            changeLogTotal.value = 0;
            changeLogPage.value = 1;
            return;
        }
        wklist.value = [];
        scopelist.value = [];
        changeLogPage.value = 1;
        loadData();
    },
    { immediate: true }
)
defineExpose({

})
</script>
<style lang="css" scoped>
.manauth-root{
    background: #f6f7fb;
    padding: 12px;
    border-radius: 12px;
    padding:22px 52px 50px 52px;
}
.manauth-header{
    margin-bottom: 16px;
}
.group-card{
    margin-bottom: 12px;
}
.group-panel{
    border-radius: 12px;
    border: 1px solid var(--el-border-color-lighter, #f0f2f5);
    box-shadow: 0 6px 18px rgba(15, 23, 42, .06);
}
.group-panel:deep(.el-card__header){
    background: linear-gradient(180deg, rgba(255,255,255,.96), rgba(255,255,255,.88));
    border-bottom: 1px solid var(--el-border-color-lighter, #f0f2f5);
}
.group-panel:deep(.el-card__body){
    padding: 12px;
}
.group-header{
    color: var(--el-text-color-primary, #303133);
    font-weight: 600;
}
.manauth-help{
    margin-top: 12px;
    padding: 12px 14px;
    background: linear-gradient(180deg, rgba(255,255,255,.92), rgba(255,255,255,.82));
    border: 1px solid var(--el-border-color-lighter, #f0f2f5);
    border-radius: 12px;
    box-shadow: 0 6px 18px rgba(15, 23, 42, .04);
}
.manauth-help__title{
    font-weight: 700;
    color: var(--el-text-color-primary, #303133);
    margin-bottom: 8px;
    position: relative;
    padding-left: 10px;
}
.manauth-help__title::before{
    content: "";
    position: absolute;
    left: 0;
    top: 3px;
    bottom: 3px;
    width: 3px;
    border-radius: 2px;
    background: var(--el-color-primary, #409eff);
}
.manauth-help__body{
    color: var(--el-text-color-regular, #606266);
    font-size: 13px;
    line-height: 1.65;
}
.manauth-help__body p{
    margin: 6px 0;
}
.manauth-help__list{
    margin: 6px 0 6px 18px;
    padding: 0;
}
.manauth-help__list li{
    margin: 4px 0;
}
.manauth-help__flags{
    margin-top: 10px;
}
.org-list{
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(360px, 1fr));
    gap: 12px;
    align-items: start;
}

.org-card{
    background: var(--el-bg-color, #fff);
    border: 1px solid var(--el-border-color-light, #ebeef5);
    border-radius: 10px;
    overflow: hidden;
    position: relative;
    box-shadow: 0 2px 10px rgba(15, 23, 42, .06);
}
.org-card::before{
    content: "";
    position: absolute;
    left: 0;
    top: 0;
    bottom: 0;
    width: 4px;
    background: linear-gradient(180deg, var(--el-color-primary, #409eff), rgba(64,158,255,.35));
}
.org-card__header{
    display: flex;
    justify-content: space-between;
    gap: 12px;
    padding: 10px 44px 8px 16px; /* 右侧给X按钮预留空间 */
}
.org-card__title{
    min-width: 0;
}
.org-card__name{
    font-weight: 600;
    color: var(--el-text-color-primary, #303133);
    line-height: 20px;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    max-width: 220px;
}
.org-card__sub{
    margin-top: 4px;
    font-size: 12px;
    color: var(--el-text-color-secondary, #909399);
    line-height: 16px;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    max-width: 260px;
}
.org-card__ops{
    display: flex;
    align-items: center;
    gap: 10px;
    flex-wrap: wrap;
    justify-content: flex-end;
    margin-right: 6px; /* 避免贴到X按钮 */
}
.org-card__close{
    position: absolute;
    right: 10px;
    top: 10px;
    z-index: 2;
}
.org-card__close:deep(.el-icon){
    font-size: 14px;
}
.org-flag{
    display: inline-flex;
    align-items: center;
    gap: 6px;
    padding: 4px 8px;
    border-radius: 999px;
    background: var(--el-fill-color-light, #f5f7fa);
}
.org-flag__label{
    font-size: 12px;
    color: var(--el-text-color-regular, #606266);
}
.org-card__body{
    padding: 0 12px 10px 16px;
}
.place-dialog-footer{
    display: flex;
    align-items: center;
    justify-content: space-between;
    width: 100%;
}
.place-dialog-footer__btns{
    display: inline-flex;
    align-items: center;
    gap: 8px;
}
.manauth-changelog{
    margin-top: 16px;
    padding: 12px 14px;
    background: #fff;
    border: 1px solid var(--el-border-color-lighter, #f0f2f5);
    border-radius: 12px;
    box-shadow: 0 6px 18px rgba(15, 23, 42, .04);
}
.manauth-changelog__title{
    font-weight: 700;
    color: var(--el-text-color-primary, #303133);
    margin-bottom: 10px;
}
.manauth-changelog__pager{
    display: flex;
    justify-content: flex-end;
    margin-top: 10px;
}
.manauth-changelog-detail p{
    margin: 0 0 8px;
    line-height: 1.6;
    color: var(--el-text-color-regular, #606266);
}
.manauth-changelog-detail__json{
    margin-top: 12px;
}
.manauth-changelog-detail__json-title{
    display: flex;
    align-items: center;
    justify-content: space-between;
    font-weight: 600;
    margin-bottom: 6px;
}
.manauth-changelog-detail pre{
    margin: 0;
    padding: 10px;
    background: #f6f8fa;
    border-radius: 8px;
    font-size: 12px;
    line-height: 1.5;
    max-height: 320px;
    overflow: auto;
    white-space: pre-wrap;
    word-break: break-all;
}
</style>