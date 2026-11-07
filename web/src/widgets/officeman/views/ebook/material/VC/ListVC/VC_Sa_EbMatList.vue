<template>
    <flexpan :flex="flex" :height="height">
        <template #header="p">
            <teleport :to="position" :disabled="!position" v-if="!hideToolbar">
                <div class="vc-top-toolbar">
                    <el-button @click="onBtnAddClick" class="toolbar-action-btn toolbar-btn-add-yellow">
                        <SvgIcon name="fa fa-plus-circle" /> 添加素材链接
                    </el-button>
                    <slot name="toolbar"></slot>
                </div>
            </teleport>
        </template>
        <template #default="p">
            <div class="mat-shell" v-loading="loading">
                <div class="mat-intro">本地收藏的创作灵感素材，支持链接导入与爆款拆解</div>

                <div class="mat-stats">
                    <div class="mat-stat mat-stat--green">
                        <div class="mat-stat-main">
                            <div class="mat-stat-num">{{ stats.collected || 0 }}</div>
                            <div class="mat-stat-label">已收藏</div>
                            <div class="mat-stat-sub">本周 +{{ stats.weekNew || 0 }}</div>
                        </div>
                        <div class="mat-stat-icon"><SvgIcon name="fa fa-bookmark" /></div>
                    </div>
                    <div class="mat-stat mat-stat--orange">
                        <div class="mat-stat-main">
                            <div class="mat-stat-num">{{ platformCount }}</div>
                            <div class="mat-stat-label">覆盖平台</div>
                            <div class="mat-stat-sub" :title="platformHint">{{ platformHint || '—' }}</div>
                        </div>
                        <div class="mat-stat-icon"><SvgIcon name="fa fa-globe" /></div>
                    </div>
                    <div class="mat-stat mat-stat--red">
                        <div class="mat-stat-main">
                            <div class="mat-stat-num">{{ stats.analyzed || 0 }}</div>
                            <div class="mat-stat-label">已拆解</div>
                            <div class="mat-stat-sub">报告已生成</div>
                        </div>
                        <div class="mat-stat-icon"><SvgIcon name="fa fa-bolt" /></div>
                    </div>
                    <div class="mat-stat mat-stat--purple">
                        <div class="mat-stat-main">
                            <div class="mat-stat-num">{{ stats.pending || 0 }}</div>
                            <div class="mat-stat-label">待筛选</div>
                            <div class="mat-stat-sub">等待处理</div>
                        </div>
                        <div class="mat-stat-icon"><SvgIcon name="fa fa-filter" /></div>
                    </div>
                </div>

                <div class="mat-filter-row">
                    <div class="mat-pills">
                        <button
                            type="button"
                            class="mat-pill"
                            :class="{ active: !platformFilter }"
                            @click="setPlatformFilter('')"
                        >全部 ({{ stats.collected || 0 }})</button>
                        <button
                            v-for="p in platformOptions"
                            :key="p.value"
                            type="button"
                            class="mat-pill"
                            :class="{ active: platformFilter === p.value }"
                            @click="setPlatformFilter(p.value)"
                        >{{ p.label }}</button>
                    </div>
                    <el-button class="mat-add-link-btn" @click="onBtnAddClick">
                        <SvgIcon name="fa fa-link" /> 添加素材链接
                    </el-button>
                </div>

                <vclist :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="listHeight(p)"
                    :flex="flex" ref="mlistvm" query :maxLine="1"
                    v-model:queryConditions="queryRowCondtions" @update:focusRow="setFocusRow"
                    v-model:face="listFace" faceKey="VC_Sa_EbMatList"
                    :orderCol="listFace === 'table'" :stripe="true" row-key="Sa_EbMatOID">
                    <template #li="{ row }">
                        <div class="mat-card" @click="openRow(row)">
                            <div class="mat-card-head">
                                <div class="mat-avatar" :style="{ background: platformColor(row.Em_Platform) }">
                                    {{ authorInitial(row.Em_SourceAuthor) }}
                                </div>
                                <div class="mat-card-meta">
                                    <div class="mat-author">@{{ row.Em_SourceAuthor || '未知作者' }}</div>
                                    <div class="mat-platform">
                                        {{ platformLabel(row.Em_Platform) }}
                                        <template v-if="formatDate(row.Em_SourceDate || row.SYS_Created)">
                                            · {{ formatDate(row.Em_SourceDate || row.SYS_Created) }}
                                        </template>
                                    </div>
                                </div>
                                <div class="mat-social">
                                    <span v-if="row.Em_LikeCount"><SvgIcon name="fa fa-thumbs-o-up" /> {{ formatCount(row.Em_LikeCount) }}</span>
                                    <span v-if="row.Em_BookmarkCount"><SvgIcon name="fa fa-star-o" /> {{ formatCount(row.Em_BookmarkCount) }}</span>
                                </div>
                            </div>

                            <div class="mat-card-title">{{ row.Em_Title || '未命名素材' }}</div>

                            <div v-if="row.Em_Summary" class="mat-ai-box">
                                <div class="mat-ai-label"><SvgIcon name="fa fa-magic" /> AI 摘要</div>
                                <div class="mat-ai-text">{{ row.Em_Summary }}</div>
                            </div>

                            <div v-if="parsePoints(row.Em_LearningPoints).length" class="mat-points">
                                <div class="mat-points-label"><SvgIcon name="fa fa-lightbulb-o" /> 值得学习的点</div>
                                <ul>
                                    <li v-for="(pt, i) in parsePoints(row.Em_LearningPoints)" :key="i">
                                        <SvgIcon name="fa fa-check" class="mat-check" /> {{ pt }}
                                    </li>
                                </ul>
                            </div>

                            <div class="mat-card-actions" @click.stop>
                                <el-button size="small" @click="openSource(row)">查看原文</el-button>
                                <el-button size="small" type="warning" plain @click="onDeconstruct(row)">
                                    <SvgIcon name="fa fa-bolt" /> 爆款拆解
                                </el-button>
                                <el-button size="small" type="primary" plain @click="onLinkTopic(row)">
                                    <SvgIcon name="fa fa-link" /> 关联选题
                                </el-button>
                                <el-button size="small" text type="danger" @click="removeRow(row)">删除</el-button>
                            </div>
                        </div>
                    </template>

                    <slot></slot>
                    <vccol field="op" caption="操作" width="140px" type="text" disabled align="center" headerAlign="center">
                        <template #default="cellp">
                            <el-link class="op-edit-link" @click="openRow(cellp.row)" title="编辑" style="margin-right: 12px; color: #338fd1;">
                                <SvgIcon name="fa fa-pencil-square-o" :size="18" />
                            </el-link>
                            <el-link type="danger" @click="removeRow(cellp.row)" title="删除">
                                <SvgIcon name="fa fa-trash-o" :size="18" />
                            </el-link>
                        </template>
                    </vccol>
                    <vccol field="Em_Title" caption="标题" width="220px" type="text" align="left" headerAlign="center"></vccol>
                    <vccol field="Em_Platform" caption="平台" width="120px" type="text" :dictlist="codeTables.EbPlatform" align="center" headerAlign="center"></vccol>
                    <vccol field="Em_SourceAuthor" caption="作者" width="140px" type="text" align="center" headerAlign="center"></vccol>
                    <vccol field="Em_Status" caption="状态" width="100px" type="text" :dictlist="codeTables.EbMaterialStatus" align="center" headerAlign="center"></vccol>
                    <vccol field="Em_LikeCount" caption="点赞" width="90px" type="text" align="center" headerAlign="center"></vccol>
                    <vccol field="Em_BookmarkCount" caption="收藏" width="90px" type="text" align="center" headerAlign="center"></vccol>
                    <vccol field="Em_Summary" caption="摘要" width="240px" type="text" align="left" headerAlign="center"></vccol>
                    <vccol field="SYS_Created" caption="创建日期" width="168px" type="datetime" align="center" headerAlign="center"></vccol>
                </vclist>
            </div>

            <el-dialog title="编辑" class="normal" v-model="diaEditOpened" width="60%" append-to-body @close="closeRowEdit">
                <template #header>
                    <div class="dialog-header">
                        <span>编辑素材</span>
                        <el-button class="toolbar-action-btn toolbar-btn-primary" @click="onDialogSave">
                            <SvgIcon name="fa fa-save" /> 保存
                        </el-button>
                    </div>
                </template>
                <livevc name="VC_Sa_EbMatEdit" :row="editingRow" @close="closeRowEdit" @refresh="onSaved"
                    :api="api" :bus="bus" :page="page" ref="editLiveVCRef">
                </livevc>
            </el-dialog>
        </template>
        <template #footer="p"></template>
    </flexpan>
</template>

<script setup>
import { ref, computed, watch, onMounted, unref } from 'vue'
import flexpan from "@/ui/common/flexpan.vue"
import vclist from "@/ui/common/vclist/vclist.vue"
import vccol from "@/ui/common/vclist/vccol.vue"
import livevc from "@/uiext/gene/live/livevc.vue"
import useListEditor from "@/uiext/gene/hook/listeditor.js"

const PLATFORM_COLORS = {
    xhs: '#ff2442', wechat: '#07c160', bilibili: '#00a1d6', douyin: '#111111',
    x: '#1d9bf0', producthunt: '#da552f', youtube: '#ff0000', weibo: '#e6162d',
}

const props = defineProps({
    page: null, api: null, bus: null, height: null,
    disabled: { type: Boolean, default: false },
    hideToolbar: { type: Boolean, default: false },
    autoLoad: { type: Boolean, default: true },
    position: null, flex: { type: Boolean, default: false },
    conditions: null, params: null,
    pageSize: { type: Number, default: 30 },
    fk: null, childed: { type: Boolean, default: false }, beforeLoad: null,
})

const emit = defineEmits(['change', 'delete', 'update:focusRow'])
const listHolder = useListEditor()
const VCName = "VC_Sa_EbMatList"
const oidField = "Sa_EbMatOID"
const loading = ref(false)
const mlist = ref([])
const total = ref(0)
const queryParams = ref({ pageSize: props.pageSize, pageNum: 1 })
const focusRow = ref({})
const queryRowCondtions = ref([])
const diaEditOpened = ref(false)
const editingRow = ref({})
const editLiveVCRef = ref(null)
const listFace = ref('card')
const platformFilter = ref('')
const stats = ref({ collected: 0, pending: 0, analyzed: 0, weekNew: 0, platforms: [] })

const codeTables = computed(() => props.page?.codeTable || {})
const platformOptions = computed(() => {
    const list = codeTables.value.EbPlatform
    return Array.isArray(list) ? list : []
})
const platformCount = computed(() => (stats.value.platforms || []).length)
const platformHint = computed(() => {
    const plats = stats.value.platforms || []
    if (!plats.length) return ''
    return plats
        .slice(0, 3)
        .map((p) => platformLabel(p.platform))
        .filter(Boolean)
        .join('/') + (plats.length > 3 ? '...' : '')
})

const busData = computed(() => props.bus?.dataMap?.[VCName] || { conditions: [] })
const listpara = computed(() => {
    const res = {
        conditions: [],
        pageSize: queryParams.value.pageSize,
        pageNum: queryParams.value.pageNum,
        page: queryParams.value.pageNum,
        dict: props.params,
        filters: {},
    }
    if (platformFilter.value) res.filters.Em_Platform = platformFilter.value
    if (Array.isArray(busData.value.conditions)) res.conditions = res.conditions.concat(busData.value.conditions)
    if (props.conditions) res.conditions = res.conditions.concat(props.conditions)
    return res
})

function listHeight(p) {
    if (listFace.value === 'card') return null
    return p?.height
}

function setFocusRow(row) { focusRow.value = row; emit('update:focusRow', row) }

function loadStats() {
    if (typeof props.api?.loadMatStats !== 'function') return
    props.api.loadMatStats({}).then((res) => {
        stats.value = {
            collected: res?.collected ?? 0,
            pending: res?.pending ?? 0,
            analyzed: res?.analyzed ?? 0,
            weekNew: res?.weekNew ?? 0,
            platforms: res?.platforms || [],
        }
    }).catch(() => {})
}

const getlist = () => {
    if (props.childed && !props.fk) return
    let para = Object.assign({}, listpara.value)
    if (Array.isArray(queryRowCondtions.value)) para.conditions = para.conditions.concat(queryRowCondtions.value)
    if (typeof props.beforeLoad == "function") para = props.beforeLoad(para)
    loading.value = true
    props.api.queryBizSaEbMatlist(para).then((res) => {
        mlist.value = res.Items || res.records || res.Records || []
        total.value = res.Total ?? res.total ?? 0
        listHolder.init(mlist.value, oidField)
        props.bus?.setList?.(VCName, mlist)
        loading.value = false
    }).catch((err) => {
        swal({ text: err.message, icon: "error", button: "关闭" })
        loading.value = false
    })
}

function saveChange() {
    const tar = listHolder.compare(mlist.value)
    if (!tar.length) { swal({ text: "没有变更的信息", icon: "info", button: "关闭" }); return }
    props.api.saveBizSaEbMatList(tar).then(() => {
        swal({ text: "保存成功", icon: "info", button: "关闭" })
        getlist(); loadStats()
    })
}

const removeRow = (row) => {
    alertIsOK("确认要操作吗？将删除当前行数据！", function (e) {
        if (e == "yes") {
            props.api.removeBizSaEbMatByIds([row[oidField]]).then(() => {
                getlist(); loadStats()
                swal({ text: "删除成功", icon: "success", button: "关闭" })
            })
        }
    }, "删除当前行数据")
}

function setPlatformFilter(val) {
    platformFilter.value = val || ''
    queryParams.value.pageNum = 1
    getlist()
}

function platformLabel(val) {
    const hit = platformOptions.value.find((x) => x.value === val)
    return hit?.label || val || ''
}
function platformColor(val) {
    return PLATFORM_COLORS[val] || '#449FFB'
}
function authorInitial(name) {
    const s = String(name || '素').trim()
    return s ? s[0] : '素'
}
function formatDate(v) {
    if (!v) return ''
    const d = new Date(v)
    if (Number.isNaN(d.getTime())) return String(v).slice(0, 10)
    const m = String(d.getMonth() + 1).padStart(2, '0')
    const day = String(d.getDate()).padStart(2, '0')
    return `${m}.${day}`
}
function formatCount(v) {
    const s = String(v ?? '').trim()
    if (!s) return ''
    const n = Number(s.replace(/,/g, ''))
    if (!Number.isFinite(n)) return s
    if (n >= 10000) return (n / 10000).toFixed(n >= 100000 ? 0 : 1).replace(/\.0$/, '') + 'w'
    if (n >= 1000) return (n / 1000).toFixed(1).replace(/\.0$/, '') + 'k'
    return String(n)
}
function parsePoints(raw) {
    if (!raw) return []
    try {
        const parsed = JSON.parse(raw)
        if (Array.isArray(parsed)) return parsed.map((x) => String(x)).filter(Boolean)
    } catch { /* plain text */ }
    return String(raw).split(/\n|;|；|·/).map((x) => x.trim()).filter(Boolean)
}

function openSource(row) {
    const url = row?.Em_SourceUrl
    if (!url) {
        swal({ text: "暂无原文链接", icon: "info", button: "关闭" })
        return
    }
    window.open(url, '_blank')
}
function onDeconstruct() {
    swal({ text: "爆款拆解即将开放", icon: "info", button: "关闭" })
}
function onLinkTopic() {
    swal({ text: "关联选题即将开放", icon: "info", button: "关闭" })
}

function onSaved() {
    getlist()
    loadStats()
}

watch(() => props.conditions, () => getlist(), { deep: true })
onMounted(() => {
    if (props.bus) {
        props.bus.initVCList(VCName, mlist, total, focusRow)
        props.bus.registerAction(VCName, getlist, saveChange, removeRow)
        props.bus.hub.on("refresh", () => { getlist(); loadStats() })
    }
    loadStats()
    if (props.autoLoad) getlist()
})

function openRow(row) { editingRow.value = row; diaEditOpened.value = true }
function closeRowEdit() { editingRow.value = {}; diaEditOpened.value = false }
function onDialogSave() {
    const inner = unref(editLiveVCRef.value?.vcauto)
    inner?.saveChange?.()
}
function onBtnAddClick() {
    editingRow.value = { Em_Status: 'raw' }
    diaEditOpened.value = true
}
</script>

<style lang="css" scoped>
.vc-top-toolbar .toolbar-action-btn, .dialog-header .toolbar-action-btn {
    height: 32px; padding: 0 12px; border-radius: 3px; font-size: 14px; font-weight: 500;
}
.vc-top-toolbar .toolbar-btn-add-yellow {
    --el-button-text-color: #fff; --el-button-bg-color: #FEB75B; --el-button-border-color: #FEB75B;
    --el-button-hover-text-color: #fff; --el-button-hover-bg-color: #f3ad52; --el-button-hover-border-color: #f3ad52;
}
.dialog-header .toolbar-btn-primary {
    --el-button-text-color: #fff; --el-button-bg-color: #449FFB; --el-button-border-color: #449FFB;
    --el-button-hover-text-color: #fff; --el-button-hover-bg-color: #3d8fe2; --el-button-hover-border-color: #3d8fe2;
}
.dialog-header { display: flex; justify-content: space-between; align-items: center; width: 100%; }

.mat-shell {
    display: flex;
    flex-direction: column;
    gap: 14px;
    min-height: 0;
    height: 100%;
}
.mat-intro {
    font-size: 13px;
    color: #667085;
    line-height: 1.4;
}

.mat-stats {
    display: grid;
    grid-template-columns: repeat(4, minmax(0, 1fr));
    gap: 12px;
}
.mat-stat {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 10px;
    padding: 14px 16px;
    border-radius: 14px;
    background: #fff;
    border: 1px solid #eef0f4;
    box-shadow: 0 1px 2px rgba(16, 24, 40, 0.04);
}
.mat-stat-num {
    font-size: 28px;
    font-weight: 700;
    line-height: 1.1;
    color: #101828;
}
.mat-stat-label {
    margin-top: 2px;
    font-size: 13px;
    color: #344054;
    font-weight: 600;
}
.mat-stat-sub {
    margin-top: 4px;
    font-size: 12px;
    color: #98a2b3;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    max-width: 140px;
}
.mat-stat-icon {
    width: 40px;
    height: 40px;
    border-radius: 12px;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
    font-size: 18px;
}
.mat-stat--green .mat-stat-icon { background: #ecfdf3; color: #12b76a; }
.mat-stat--orange .mat-stat-icon { background: #fff6ed; color: #f79009; }
.mat-stat--red .mat-stat-icon { background: #fef3f2; color: #f04438; }
.mat-stat--purple .mat-stat-icon { background: #f4f3ff; color: #7a5af8; }

.mat-filter-row {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    flex-wrap: wrap;
}
.mat-pills {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
    min-width: 0;
}
.mat-pill {
    border: 1px solid #d0d5dd;
    background: #fff;
    color: #344054;
    border-radius: 999px;
    padding: 6px 14px;
    font-size: 13px;
    cursor: pointer;
    transition: all .15s ease;
}
.mat-pill:hover { border-color: #98a2b3; }
.mat-pill.active {
    background: #1d2939;
    border-color: #1d2939;
    color: #fff;
}
.mat-add-link-btn {
    --el-button-text-color: #fff;
    --el-button-bg-color: #1d2939;
    --el-button-border-color: #1d2939;
    --el-button-hover-bg-color: #344054;
    --el-button-hover-border-color: #344054;
    --el-button-hover-text-color: #fff;
    border-radius: 10px;
}

.mat-card {
    display: flex;
    flex-direction: column;
    gap: 12px;
    padding: 18px 18px 14px;
    background: #fff;
    border: 1px solid #eef0f4;
    border-radius: 16px;
    box-shadow: 0 2px 10px rgba(16, 24, 40, 0.05);
    cursor: pointer;
    transition: box-shadow .2s, border-color .2s;
    height: 100%;
    box-sizing: border-box;
}
.mat-card:hover {
    border-color: #d0d5dd;
    box-shadow: 0 8px 24px rgba(16, 24, 40, 0.08);
}
.mat-card-head {
    display: flex;
    align-items: flex-start;
    gap: 10px;
}
.mat-avatar {
    width: 40px;
    height: 40px;
    border-radius: 50%;
    color: #fff;
    font-weight: 700;
    font-size: 16px;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
}
.mat-card-meta { flex: 1; min-width: 0; }
.mat-author {
    font-size: 14px;
    font-weight: 600;
    color: #101828;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}
.mat-platform {
    margin-top: 2px;
    font-size: 12px;
    color: #98a2b3;
}
.mat-social {
    display: flex;
    gap: 10px;
    color: #667085;
    font-size: 12px;
    flex-shrink: 0;
}
.mat-social span {
    display: inline-flex;
    align-items: center;
    gap: 4px;
}
.mat-card-title {
    font-size: 16px;
    font-weight: 700;
    color: #101828;
    line-height: 1.4;
}
.mat-ai-box {
    background: #eff8ff;
    border-radius: 10px;
    padding: 10px 12px;
}
.mat-ai-label {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    font-size: 12px;
    font-weight: 600;
    color: #1570ef;
    margin-bottom: 6px;
}
.mat-ai-text {
    font-size: 13px;
    color: #344054;
    line-height: 1.55;
}
.mat-points-label {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    font-size: 13px;
    font-weight: 600;
    color: #027a48;
    margin-bottom: 8px;
}
.mat-points ul {
    margin: 0;
    padding: 0;
    list-style: none;
    display: flex;
    flex-direction: column;
    gap: 6px;
}
.mat-points li {
    display: flex;
    align-items: flex-start;
    gap: 8px;
    font-size: 13px;
    color: #344054;
    line-height: 1.45;
}
.mat-check { color: #12b76a; margin-top: 2px; flex-shrink: 0; }
.mat-card-actions {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
    margin-top: auto;
    padding-top: 4px;
}

:deep(.vc-card-list) {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(420px, 1fr));
    gap: 16px;
    padding: 4px 2px 16px;
}
:deep(.vc-card-list .vc-li) {
    margin: 0;
    height: 100%;
}
:deep(.vclistwrap) {
    flex: 1;
    min-height: 0;
}

@media (max-width: 1100px) {
    .mat-stats { grid-template-columns: repeat(2, minmax(0, 1fr)); }
    :deep(.vc-card-list) {
        grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
    }
}
@media (max-width: 640px) {
    .mat-stats { grid-template-columns: 1fr; }
}
</style>
