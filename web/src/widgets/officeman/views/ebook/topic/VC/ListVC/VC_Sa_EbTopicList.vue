<template>
    <flexpan :flex="flex" :height="height">
        <template #header="p">
            <teleport :to="position" :disabled="!position" v-if="!hideToolbar">
                <div class="vc-top-toolbar">
                    <el-button @click="onBtnAddClick" class="toolbar-action-btn toolbar-btn-add-yellow">
                        <SvgIcon name="fa fa-plus-circle" /> 新建选题
                    </el-button>
                    <slot name="toolbar"></slot>
                </div>
            </teleport>
        </template>
        <template #default="p">
            <div class="topic-shell" v-loading="loading">
                <div class="topic-intro">每一个选题都是一个创作项目，从灵感到发布的完整历程</div>

                <div class="topic-stats">
                    <div class="topic-stat">
                        <div class="topic-stat-main">
                            <div class="topic-stat-num">{{ stats.total || 0 }}</div>
                            <div class="topic-stat-label">选题总数</div>
                            <div class="topic-stat-sub">本周 +{{ stats.weekNew || 0 }}</div>
                        </div>
                        <div class="topic-stat-icon topic-stat-icon--doc"><SvgIcon name="fa fa-file-text-o" /></div>
                    </div>
                    <div class="topic-stat">
                        <div class="topic-stat-main">
                            <div class="topic-stat-num">{{ stats.inProgress || 0 }}</div>
                            <div class="topic-stat-label">进行中</div>
                            <div class="topic-stat-sub">{{ stats.writing || 0 }} 篇写作中</div>
                        </div>
                        <div class="topic-stat-icon topic-stat-icon--clock"><SvgIcon name="fa fa-clock-o" /></div>
                    </div>
                    <div class="topic-stat">
                        <div class="topic-stat-main">
                            <div class="topic-stat-num">{{ stats.inspiration || 0 }}</div>
                            <div class="topic-stat-label">灵感源</div>
                            <div class="topic-stat-sub">{{ stats.inspiration || 0 }} 待转化</div>
                        </div>
                        <div class="topic-stat-icon topic-stat-icon--bulb"><SvgIcon name="fa fa-lightbulb-o" /></div>
                    </div>
                    <div class="topic-stat">
                        <div class="topic-stat-main">
                            <div class="topic-stat-num">{{ stats.published || 0 }}</div>
                            <div class="topic-stat-label">已发布</div>
                            <div class="topic-stat-sub">本周完成 {{ stats.publishedWeek || 0 }}</div>
                        </div>
                        <div class="topic-stat-icon topic-stat-icon--ok"><SvgIcon name="fa fa-check-circle" /></div>
                    </div>
                </div>

                <div class="topic-filter-row">
                    <div class="topic-pills">
                        <button type="button" class="topic-pill" :class="{ active: !stageFilter }"
                            @click="setStageFilter('')">全部 {{ stats.total || 0 }}</button>
                        <button v-for="tab in stageTabs" :key="tab.value" type="button" class="topic-pill"
                            :class="{ active: stageFilter === tab.value }"
                            @click="setStageFilter(tab.value)">
                            {{ tab.label }}{{ stageCount(tab.value) ? ' ' + stageCount(tab.value) : '' }}
                        </button>
                    </div>
                    <el-button class="topic-add-btn" @click="onBtnAddClick">
                        <SvgIcon name="fa fa-plus" /> 新建选题
                    </el-button>
                </div>

                <vclist :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="listHeight(p)"
                    :flex="flex" ref="mlistvm" query :maxLine="1"
                    v-model:queryConditions="queryRowCondtions" @update:focusRow="setFocusRow"
                    v-model:face="listFace" faceKey="VC_Sa_EbTopicList"
                    :orderCol="listFace === 'table'" :stripe="true" row-key="Sa_EbTopicOID">
                    <template #li="{ row }">
                        <div class="topic-card" @click="openRow(row)">
                            <div class="topic-card-top">
                                <div class="topic-tags">
                                    <span class="topic-tag" :class="'tag-' + (row.Et_Stage || 'draft')">{{ stageLabel(row) }}</span>
                                    <span v-if="row.Et_Format" class="topic-tag tag-format">{{ formatLabel(row.Et_Format) }}</span>
                                </div>
                                <div class="topic-time">{{ relativeTime(row.SYS_LAST_UPD || row.SYS_Created) }}</div>
                            </div>

                            <div class="topic-card-title">{{ row.Et_Title || '未命名选题' }}</div>
                            <div class="topic-card-desc">{{ row.Et_Desc || '暂无描述' }}</div>

                            <div class="topic-pipeline">
                                <div v-for="(st, i) in pipelineSteps(row)" :key="st.value" class="pipe-step"
                                    :class="{ done: st.done, current: st.current }">
                                    <div class="pipe-node">
                                        <SvgIcon v-if="st.done" name="fa fa-check" />
                                        <span v-else>{{ i + 1 }}</span>
                                    </div>
                                    <div class="pipe-label">{{ st.label }}</div>
                                    <div v-if="i < pipelineSteps(row).length - 1" class="pipe-line" :class="{ done: st.done }"></div>
                                </div>
                            </div>

                            <div class="topic-card-foot" @click.stop>
                                <el-button text class="topic-mat-link" @click="openMats(row)">
                                    <SvgIcon name="fa fa-folder-o" /> 查看选题素材 {{ Number(row.MatCount) || 0 }}
                                </el-button>
                                <el-button class="topic-advance-btn" :disabled="isPublished(row)" @click="advanceRow(row)">
                                    继续推进
                                </el-button>
                            </div>
                        </div>
                    </template>

                    <slot></slot>
                    <vccol field="op" caption="操作" width="180px" type="text" disabled align="center" headerAlign="center">
                        <template #default="cellp">
                            <el-link class="op-edit-link" @click="openRow(cellp.row)" title="编辑" style="margin-right: 10px; color: #338fd1;">
                                <SvgIcon name="fa fa-pencil-square-o" :size="18" />
                            </el-link>
                            <el-link type="warning" @click="advanceRow(cellp.row)" title="推进" style="margin-right: 10px;">推进</el-link>
                            <el-link type="danger" @click="removeRow(cellp.row)" title="删除">
                                <SvgIcon name="fa fa-trash-o" :size="18" />
                            </el-link>
                        </template>
                    </vccol>
                    <vccol field="Et_Title" caption="标题" width="220px" type="text" align="left" headerAlign="center"></vccol>
                    <vccol field="Et_Status" caption="状态" width="100px" type="text" :dictlist="codeTables.EbTopicStatus" align="center" headerAlign="center"></vccol>
                    <vccol field="Et_Format" caption="形式" width="100px" type="text" :dictlist="codeTables.EbPublishFormat" align="center" headerAlign="center"></vccol>
                    <vccol field="Et_Pipeline" caption="流水线" width="100px" type="text" align="center" headerAlign="center"></vccol>
                    <vccol field="Et_Stage" caption="阶段" width="110px" type="text" align="center" headerAlign="center"></vccol>
                    <vccol field="Et_Audience" caption="读者" width="140px" type="text" align="center" headerAlign="center"></vccol>
                    <vccol field="Et_ExpectAt" caption="预期发布" width="140px" type="date" align="center" headerAlign="center"></vccol>
                    <vccol field="SYS_Created" caption="创建日期" width="168px" type="datetime" align="center" headerAlign="center"></vccol>
                </vclist>
            </div>

            <el-dialog title="编辑" class="normal" v-model="diaEditOpened" width="60%" append-to-body @close="closeRowEdit">
                <template #header>
                    <div class="dialog-header">
                        <span>编辑选题</span>
                        <el-button class="toolbar-action-btn toolbar-btn-primary" @click="onDialogSave">
                            <SvgIcon name="fa fa-save" /> 保存
                        </el-button>
                    </div>
                </template>
                <livevc name="VC_Sa_EbTopicEdit" :row="editingRow" @close="closeRowEdit" @refresh="onSaved"
                    :api="api" :bus="bus" :page="page" ref="editLiveVCRef"></livevc>
            </el-dialog>

            <el-drawer v-model="matDrawerOpened" title="本选题积累的素材" size="420px" append-to-body>
                <div v-loading="matLoading" class="mat-drawer-body">
                    <el-empty v-if="!matLoading && !matList.length" description="暂无关联素材" />
                    <div v-for="m in matList" :key="m.Sa_EbMatOID || m.Sa_EbTpMatOID" class="mat-drawer-card"
                        @click="openMatUrl(m)">
                        <div class="mat-drawer-badge" :style="{ background: platformColor(m.Em_Platform) }">
                            {{ platformLabel(m.Em_Platform).slice(0, 1) || '素' }}
                        </div>
                        <div class="mat-drawer-main">
                            <div class="mat-drawer-title">{{ m.Em_Title || '未命名素材' }}</div>
                            <div class="mat-drawer-meta">
                                {{ platformLabel(m.Em_Platform) || '素材' }}
                                <template v-if="formatShortDate(m.Em_SourceDate || m.SYS_Created)">
                                    · {{ formatShortDate(m.Em_SourceDate || m.SYS_Created) }}
                                </template>
                            </div>
                        </div>
                        <SvgIcon name="fa fa-external-link" class="mat-drawer-ext" />
                    </div>
                </div>
            </el-drawer>
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

const GRAPHIC_STAGES = [
    { value: 'inspiration', label: '灵感' },
    { value: 'draft', label: '初稿' },
    { value: 'final', label: '定稿' },
    { value: 'graphic', label: '图文' },
    { value: 'publish', label: '发布' },
]
const VIDEO_STAGES = [
    { value: 'inspiration', label: '灵感' },
    { value: 'outline', label: '大纲' },
    { value: 'script', label: '脚本' },
    { value: 'filming', label: '拍摄' },
    { value: 'editing', label: '剪辑' },
    { value: 'publish', label: '发布' },
]
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
const VCName = "VC_Sa_EbTopicList"
const oidField = "Sa_EbTopicOID"
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
const stageFilter = ref('')
const stats = ref({
    total: 0, inProgress: 0, writing: 0, inspiration: 0, published: 0,
    weekNew: 0, publishedWeek: 0, byStage: {},
})
const matDrawerOpened = ref(false)
const matLoading = ref(false)
const matList = ref([])
const matTopic = ref(null)

const stageTabs = [
    { value: 'draft', label: '草稿' },
    { value: 'final', label: '定稿' },
    { value: 'graphic', label: '图文' },
    { value: 'publish', label: '已发布' },
]

const codeTables = computed(() => props.page?.codeTable || {})
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
    if (stageFilter.value === 'publish') {
        res.filters.Et_Status = 'published'
    } else if (stageFilter.value) {
        res.filters.Et_Stage = stageFilter.value
    }
    if (Array.isArray(busData.value.conditions)) res.conditions = res.conditions.concat(busData.value.conditions)
    if (props.conditions) res.conditions = res.conditions.concat(props.conditions)
    return res
})

function listHeight(p) {
    if (listFace.value === 'card') return null
    return p?.height
}
function setFocusRow(row) { focusRow.value = row; emit('update:focusRow', row) }
function stageCount(val) {
    if (val === 'publish') return stats.value.published || 0
    return stats.value.byStage?.[val] || 0
}

function loadStats() {
    if (typeof props.api?.loadTopicStats !== 'function') return
    props.api.loadTopicStats({}).then((res) => {
        stats.value = {
            total: res?.total ?? 0,
            inProgress: res?.inProgress ?? 0,
            writing: res?.writing ?? 0,
            inspiration: res?.inspiration ?? 0,
            published: res?.published ?? 0,
            weekNew: res?.weekNew ?? 0,
            publishedWeek: res?.publishedWeek ?? 0,
            byStage: res?.byStage || {},
        }
    }).catch(() => {})
}

const getlist = () => {
    if (props.childed && !props.fk) return
    let para = Object.assign({}, listpara.value)
    if (Array.isArray(queryRowCondtions.value)) para.conditions = para.conditions.concat(queryRowCondtions.value)
    if (typeof props.beforeLoad == "function") para = props.beforeLoad(para)
    loading.value = true
    props.api.queryBizSaEbTopiclist(para).then((res) => {
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
    props.api.saveBizSaEbTopicList(tar).then(() => {
        swal({ text: "保存成功", icon: "info", button: "关闭" })
        getlist(); loadStats()
    })
}

const removeRow = (row) => {
    alertIsOK("确认要操作吗？将删除当前行数据！", function (e) {
        if (e == "yes") {
            props.api.removeBizSaEbTopicByIds([row[oidField]]).then(() => {
                getlist(); loadStats()
                swal({ text: "删除成功", icon: "success", button: "关闭" })
            })
        }
    }, "删除当前行数据")
}

function advanceRow(row) {
    props.api.advanceTopic({ id: row[oidField] }).then((res) => {
        swal({ text: res?.done ? "已到最终阶段" : `已推进到：${stageLabelByValue(res?.stage, row)}`, icon: "success", button: "关闭" })
        getlist(); loadStats()
    })
}

function setStageFilter(val) {
    stageFilter.value = val || ''
    queryParams.value.pageNum = 1
    getlist()
}

function isPublished(row) {
    return row?.Et_Status === 'published' || row?.Et_Stage === 'publish'
}
function dictLabel(list, val) {
    const hit = (list || []).find((x) => x.value === val)
    return hit?.label || val || ''
}
function formatLabel(val) {
    return dictLabel(codeTables.value.EbPublishFormat, val)
}
function stageLabelByValue(val, row) {
    const steps = isVideo(row) ? VIDEO_STAGES : GRAPHIC_STAGES
    const hit = steps.find((s) => s.value === val)
    if (hit) return hit.label
    return dictLabel(codeTables.value.EbTopicStageGraphic, val)
        || dictLabel(codeTables.value.EbTopicStageVideo, val)
        || val || ''
}
function stageLabel(row) {
    return stageLabelByValue(row?.Et_Stage, row)
}
function isVideo(row) {
    return row?.Et_Pipeline === 'video' || row?.Et_Format === 'video'
}
function pipelineSteps(row) {
    const stages = isVideo(row) ? VIDEO_STAGES : GRAPHIC_STAGES
    const cur = row?.Et_Stage || 'inspiration'
    let idx = stages.findIndex((s) => s.value === cur)
    if (idx < 0) idx = 0
    if (row?.Et_Status === 'published') idx = stages.length - 1
    return stages.map((s, i) => ({
        ...s,
        done: i < idx || (i === idx && (cur === 'publish' || row?.Et_Status === 'published')),
        current: i === idx && cur !== 'publish' && row?.Et_Status !== 'published',
    }))
}
function relativeTime(v) {
    if (!v) return ''
    const d = new Date(v)
    if (Number.isNaN(d.getTime())) return ''
    const diff = Date.now() - d.getTime()
    const m = Math.floor(diff / 60000)
    if (m < 1) return '刚刚'
    if (m < 60) return `${m} 分钟前`
    const h = Math.floor(m / 60)
    if (h < 24) return `${h} 小时前`
    const day = Math.floor(h / 24)
    if (day < 30) return `${day} 天前`
    return formatShortDate(v)
}
function formatShortDate(v) {
    if (!v) return ''
    const d = new Date(v)
    if (Number.isNaN(d.getTime())) return String(v).slice(0, 10)
    const mm = String(d.getMonth() + 1).padStart(2, '0')
    const dd = String(d.getDate()).padStart(2, '0')
    return `${mm}.${dd}`
}
function platformLabel(val) {
    return dictLabel(codeTables.value.EbPlatform, val)
}
function platformColor(val) {
    return PLATFORM_COLORS[val] || '#F79009'
}

function openMats(row) {
    matTopic.value = row
    matDrawerOpened.value = true
    matLoading.value = true
    matList.value = []
    props.api.listTopicMat({ topicId: row[oidField] }).then((res) => {
        matList.value = Array.isArray(res) ? res : (res?.Items || res?.records || [])
        matLoading.value = false
    }).catch((err) => {
        matLoading.value = false
        swal({ text: err.message || '加载素材失败', icon: 'error', button: '关闭' })
    })
}
function openMatUrl(m) {
    if (m?.Em_SourceUrl) window.open(m.Em_SourceUrl, '_blank')
    else swal({ text: '暂无原文链接', icon: 'info', button: '关闭' })
}

function onSaved() { getlist(); loadStats() }

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
function onDialogSave() { unref(editLiveVCRef.value?.vcauto)?.saveChange?.() }
function onBtnAddClick() {
    editingRow.value = { Et_Status: 'draft', Et_Pipeline: 'graphic', Et_Stage: 'inspiration', Et_Format: 'graphic' }
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

.topic-shell {
    display: flex; flex-direction: column; gap: 14px; min-height: 0; height: 100%;
}
.topic-intro { font-size: 13px; color: #667085; line-height: 1.4; }

.topic-stats {
    display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 12px;
}
.topic-stat {
    display: flex; align-items: center; justify-content: space-between; gap: 10px;
    padding: 14px 16px; border-radius: 14px; background: #fff;
    border: 1px solid #eef0f4; box-shadow: 0 1px 2px rgba(16, 24, 40, 0.04);
}
.topic-stat-num { font-size: 28px; font-weight: 700; line-height: 1.1; color: #101828; }
.topic-stat-label { margin-top: 2px; font-size: 13px; color: #344054; font-weight: 600; }
.topic-stat-sub { margin-top: 4px; font-size: 12px; color: #98a2b3; }
.topic-stat-icon {
    width: 40px; height: 40px; border-radius: 12px;
    display: flex; align-items: center; justify-content: center; flex-shrink: 0; font-size: 18px;
}
.topic-stat-icon--doc { background: #eff8ff; color: #1570ef; }
.topic-stat-icon--clock { background: #fff6ed; color: #f79009; }
.topic-stat-icon--bulb { background: #fef7c3; color: #ca8a04; }
.topic-stat-icon--ok { background: #ecfdf3; color: #12b76a; }

.topic-filter-row {
    display: flex; align-items: center; justify-content: space-between; gap: 12px; flex-wrap: wrap;
}
.topic-pills { display: flex; flex-wrap: wrap; gap: 8px; min-width: 0; }
.topic-pill {
    border: 1px solid #d0d5dd; background: #fff; color: #344054;
    border-radius: 999px; padding: 6px 14px; font-size: 13px; cursor: pointer;
}
.topic-pill:hover { border-color: #98a2b3; }
.topic-pill.active { background: #1d2939; border-color: #1d2939; color: #fff; }
.topic-add-btn {
    --el-button-text-color: #fff;
    --el-button-bg-color: #F79009;
    --el-button-border-color: #F79009;
    --el-button-hover-bg-color: #dc6803;
    --el-button-hover-border-color: #dc6803;
    --el-button-hover-text-color: #fff;
    border-radius: 10px;
}

.topic-card {
    display: flex; flex-direction: column; gap: 12px;
    padding: 18px; background: #fff; border: 1px solid #eef0f4; border-radius: 16px;
    box-shadow: 0 2px 10px rgba(16, 24, 40, 0.05); cursor: pointer; height: 100%;
    box-sizing: border-box; transition: box-shadow .2s, border-color .2s;
}
.topic-card:hover { border-color: #d0d5dd; box-shadow: 0 8px 24px rgba(16, 24, 40, 0.08); }
.topic-card-top { display: flex; align-items: center; justify-content: space-between; gap: 8px; }
.topic-tags { display: flex; flex-wrap: wrap; gap: 6px; }
.topic-tag {
    display: inline-flex; align-items: center; padding: 2px 8px; border-radius: 999px;
    font-size: 12px; font-weight: 600; background: #f2f4f7; color: #344054;
}
.topic-tag.tag-final { background: #fef3f2; color: #d92d20; }
.topic-tag.tag-graphic { background: #fff6ed; color: #dc6803; }
.topic-tag.tag-draft, .topic-tag.tag-script, .topic-tag.tag-outline { background: #fffaeb; color: #b54708; }
.topic-tag.tag-publish { background: #ecfdf3; color: #027a48; }
.topic-tag.tag-inspiration { background: #f4f3ff; color: #5925dc; }
.topic-tag.tag-format { background: #fff6ed; color: #b54708; }
.topic-time { font-size: 12px; color: #98a2b3; flex-shrink: 0; }
.topic-card-title { font-size: 17px; font-weight: 700; color: #101828; line-height: 1.35; }
.topic-card-desc {
    font-size: 13px; color: #667085; line-height: 1.5;
    display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden;
}

.topic-pipeline {
    display: flex; align-items: flex-start; gap: 0; padding: 8px 0 4px; overflow-x: auto;
}
.pipe-step {
    position: relative; display: flex; flex-direction: column; align-items: center;
    min-width: 56px; flex: 1;
}
.pipe-node {
    width: 22px; height: 22px; border-radius: 50%; border: 2px solid #d0d5dd;
    background: #fff; color: #98a2b3; font-size: 11px; font-weight: 700;
    display: flex; align-items: center; justify-content: center; z-index: 1;
}
.pipe-step.done .pipe-node { background: #12b76a; border-color: #12b76a; color: #fff; }
.pipe-step.current .pipe-node { border-color: #F79009; color: #F79009; }
.pipe-label { margin-top: 6px; font-size: 11px; color: #667085; white-space: nowrap; }
.pipe-step.done .pipe-label, .pipe-step.current .pipe-label { color: #344054; font-weight: 600; }
.pipe-line {
    position: absolute; top: 10px; left: calc(50% + 14px); right: calc(-50% + 14px);
    height: 2px; background: #e4e7ec; z-index: 0;
}
.pipe-line.done { background: #12b76a; }

.topic-card-foot {
    margin-top: auto; display: flex; align-items: center; justify-content: space-between; gap: 8px;
}
.topic-mat-link { color: #344054 !important; font-weight: 500; }
.topic-advance-btn {
    --el-button-text-color: #fff;
    --el-button-bg-color: #F79009;
    --el-button-border-color: #F79009;
    --el-button-hover-bg-color: #dc6803;
    --el-button-hover-border-color: #dc6803;
    --el-button-hover-text-color: #fff;
    border-radius: 10px;
}

.mat-drawer-body { display: flex; flex-direction: column; gap: 10px; }
.mat-drawer-card {
    display: flex; align-items: center; gap: 12px; padding: 12px;
    border: 1px solid #eef0f4; border-radius: 12px; cursor: pointer; background: #fff;
}
.mat-drawer-card:hover { border-color: #d0d5dd; }
.mat-drawer-badge {
    width: 36px; height: 36px; border-radius: 10px; color: #fff; font-weight: 700;
    display: flex; align-items: center; justify-content: center; flex-shrink: 0;
}
.mat-drawer-main { flex: 1; min-width: 0; }
.mat-drawer-title {
    font-size: 14px; font-weight: 600; color: #101828;
    white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
}
.mat-drawer-meta { margin-top: 2px; font-size: 12px; color: #98a2b3; }
.mat-drawer-ext { color: #98a2b3; }

:deep(.vc-card-list) {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(420px, 1fr));
    gap: 16px; padding: 4px 2px 16px;
}
:deep(.vc-card-list .vc-li) { margin: 0; height: 100%; }
:deep(.vclistwrap) { flex: 1; min-height: 0; }

@media (max-width: 1100px) {
    .topic-stats { grid-template-columns: repeat(2, minmax(0, 1fr)); }
    :deep(.vc-card-list) { grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); }
}
@media (max-width: 640px) {
    .topic-stats { grid-template-columns: 1fr; }
}
</style>
