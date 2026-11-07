<template>
    <flexpan :flex="flex" :height="height">
        <template #header="p">
            <teleport :to="position" :disabled="!position" v-if="!hideToolbar">
                <div class="vc-top-toolbar">
                    <el-button @click="onBtnAddClick" class="toolbar-action-btn toolbar-btn-add-yellow">
                        <SvgIcon name="fa fa-plus-circle" /> 添加
                    </el-button>
                    <slot name="toolbar"></slot>
                </div>
            </teleport>
        </template>
        <template #default="p">
            <div class="note-shell" v-loading="loading">
                <div class="note-preview-head">
                    <span>笔记列表</span>
                    <span class="note-preview-count">共 {{ total }} 份</span>
                </div>

                <vclist :list="mlist" :total="total" :para="queryParams" @query="getlist" :height="listHeight(p)"
                    :flex="flex" ref="mlistvm" query :maxLine="1"
                    v-model:queryConditions="queryRowCondtions" @update:focusRow="setFocusRow"
                    v-model:face="listFace" faceKey="VC_Sa_EbNoteList"
                    :orderCol="listFace === 'table'" :stripe="true" row-key="Sa_EbNoteOID">
                    <template #li="{ row }">
                        <div class="note-card" @click="openRow(row)">
                            <div class="note-card-title">{{ row.En_Title || '未命名笔记' }}</div>

                            <div class="note-metrics">
                                <div class="note-metric">
                                    <div class="note-metric-val">{{ formatCount(row.En_LikeCount) || '—' }}</div>
                                    <div class="note-metric-lab">点赞</div>
                                </div>
                                <div class="note-metric">
                                    <div class="note-metric-val">{{ formatCount(row.En_BookmarkCount) || '—' }}</div>
                                    <div class="note-metric-lab">收藏</div>
                                </div>
                                <div class="note-metric">
                                    <div class="note-metric-val">{{ formatCount(row.En_CommentCount) || '—' }}</div>
                                    <div class="note-metric-lab">评论</div>
                                </div>
                                <div class="note-metric">
                                    <div class="note-metric-val">{{ formatCount(row.En_ViewCount) || '—' }}</div>
                                    <div class="note-metric-lab">浏览</div>
                                </div>
                            </div>

                            <div class="note-kv-list">
                                <div v-for="item in previewKvs(row)" :key="item.label" class="note-kv">
                                    <span class="note-kv-lab">{{ item.label }}</span>
                                    <span class="note-kv-val">{{ item.value }}</span>
                                </div>
                            </div>

                            <div v-if="reportOf(row).takeaways" class="note-takeaways">
                                <div class="note-takeaways-lab">可借鉴点</div>
                                <div class="note-takeaways-txt">{{ reportOf(row).takeaways }}</div>
                            </div>

                            <div class="note-card-foot" @click.stop>
                                <div class="note-cats">
                                    <span class="note-cats-lab">分类:</span>
                                    <span v-for="(c, i) in categoriesOf(row)" :key="i" class="note-cat-tag">{{ c }}</span>
                                    <span v-if="!categoriesOf(row).length" class="note-cat-empty">未分类</span>
                                </div>
                                <div class="note-card-actions">
                                    <el-button size="small" type="primary" plain @click="openRow(row)">查看详情</el-button>
                                    <el-button size="small" text type="danger" @click="removeRow(row)">删除</el-button>
                                </div>
                            </div>
                        </div>
                    </template>

                    <slot></slot>
                    <vccol field="op" caption="操作" width="100px" type="text" disabled align="center" headerAlign="center">
                        <template #default="cellp">
                            <el-link class="op-edit-link" @click="openRow(cellp.row)" title="编辑" style="margin-right: 20px; color: #338fd1;">
                                <SvgIcon name="fa fa-pencil-square-o" :size="18" />
                            </el-link>
                            <el-link type="danger" @click="removeRow(cellp.row)" title="删除">
                                <SvgIcon name="fa fa-trash-o" :size="18" />
                            </el-link>
                        </template>
                    </vccol>
                    <vccol field="En_Title" caption="标题" width="220px" type="text" align="left" headerAlign="center"></vccol>
                    <vccol field="En_Status" caption="状态" width="100px" type="text" :dictlist="codeTables.EbTopicStatus" align="center" headerAlign="center"></vccol>
                    <vccol field="En_LikeCount" caption="点赞" width="90px" type="text" align="center" headerAlign="center"></vccol>
                    <vccol field="SYS_Created" caption="创建日期" width="168px" type="datetime" align="center" headerAlign="center"></vccol>
                    <vccol field="SYS_LAST_UPD" caption="最后修改日期" width="168px" type="datetime" align="center" headerAlign="center"></vccol>
                </vclist>
            </div>

            <el-dialog title="编辑" class="normal note-edit-dialog" v-model="diaEditOpened" width="72%" top="4vh"
                append-to-body @close="closeRowEdit">
                <template #header>
                    <div class="dialog-header">
                        <span>笔记详情</span>
                        <el-button class="toolbar-action-btn toolbar-btn-primary" @click="onDialogSave">
                            <SvgIcon name="fa fa-save" /> 保存
                        </el-button>
                    </div>
                </template>
                <livevc name="VC_Sa_EbNoteEdit" :row="editingRow" @close="closeRowEdit" @refresh="getlist"
                    :api="api" :bus="bus" :page="page" ref="editLiveVCRef"></livevc>
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
const VCName = "VC_Sa_EbNoteList"
const oidField = "Sa_EbNoteOID"
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

const codeTables = computed(() => props.page?.codeTable || {})
const busData = computed(() => props.bus?.dataMap?.[VCName] || { conditions: [] })
const listpara = computed(() => {
    const res = {
        conditions: [], pageSize: queryParams.value.pageSize,
        pageNum: queryParams.value.pageNum, page: queryParams.value.pageNum, dict: props.params,
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

function reportOf(row) {
    const raw = row?.En_ReportJson
    if (!raw) return {}
    if (typeof raw === 'object') return raw
    try { return JSON.parse(raw) || {} } catch { return {} }
}
function categoriesOf(row) {
    const cats = reportOf(row).categories
    if (Array.isArray(cats)) return cats.map(String).filter(Boolean)
    if (typeof cats === 'string' && cats.trim()) return cats.split(/[,，/|]/).map((x) => x.trim()).filter(Boolean)
    return []
}
function previewKvs(row) {
    const r = reportOf(row)
    return [
        { label: '标题亮点', value: r.titleHighlight || '—' },
        { label: '封面设计', value: r.coverDesign || '—' },
        { label: '内容结构', value: r.contentStructure || '—' },
        { label: '互动引导', value: r.engageGuide || '—' },
        { label: '适合选题', value: r.suitTopics || '—' },
    ]
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

const getlist = () => {
    if (props.childed && !props.fk) return
    let para = Object.assign({}, listpara.value)
    if (Array.isArray(queryRowCondtions.value)) para.conditions = para.conditions.concat(queryRowCondtions.value)
    if (typeof props.beforeLoad == "function") para = props.beforeLoad(para)
    loading.value = true
    props.api.queryBizSaEbNotelist(para).then((res) => {
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
    props.api.saveBizSaEbNoteList(tar).then(() => { swal({ text: "保存成功", icon: "info", button: "关闭" }); getlist() })
}

const removeRow = (row) => {
    alertIsOK("确认要操作吗？将删除当前行数据！", function (e) {
        if (e == "yes") {
            props.api.removeBizSaEbNoteByIds([row[oidField]]).then(() => {
                getlist(); swal({ text: "删除成功", icon: "success", button: "关闭" })
            })
        }
    }, "删除当前行数据")
}

watch(() => props.conditions, () => getlist(), { deep: true })
onMounted(() => {
    if (props.bus) {
        props.bus.initVCList(VCName, mlist, total, focusRow)
        props.bus.registerAction(VCName, getlist, saveChange, removeRow)
        props.bus.hub.on("refresh", getlist)
    }
    if (props.autoLoad) getlist()
})

function openRow(row) { editingRow.value = row; diaEditOpened.value = true }
function closeRowEdit() { editingRow.value = {}; diaEditOpened.value = false }
function onDialogSave() { unref(editLiveVCRef.value?.vcauto)?.saveChange?.() }
function onBtnAddClick() {
    editingRow.value = {
        En_Status: 'draft',
        En_ReportJson: JSON.stringify({
            titleHighlight: '', coverDesign: '', contentStructure: '', engageGuide: '', suitTopics: '',
            takeaways: '', categories: [], topicDirection: '', contentFormat: '', coreSell: '',
            contentFramework: '', engageDesign: '', opsAdvice: '',
        }),
    }
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
    --el-button-text-color: #fff; --el-button-bg-color: #7A5AF8; --el-button-border-color: #7A5AF8;
    --el-button-hover-text-color: #fff; --el-button-hover-bg-color: #6938ef; --el-button-hover-border-color: #6938ef;
}
.dialog-header { display: flex; justify-content: space-between; align-items: center; width: 100%; }

.note-shell { display: flex; flex-direction: column; gap: 12px; min-height: 0; height: 100%; }
.note-preview-head {
    display: flex; align-items: center; justify-content: space-between;
    font-size: 14px; font-weight: 700; color: #101828;
}
.note-preview-count { font-size: 12px; font-weight: 500; color: #98a2b3; }

.note-card {
    display: flex; flex-direction: column; gap: 12px;
    padding: 18px; background: #fff; border: 1px solid #eef0f4; border-radius: 16px;
    box-shadow: 0 2px 10px rgba(16, 24, 40, 0.05); cursor: pointer; height: 100%; box-sizing: border-box;
}
.note-card:hover { border-color: #d0d5dd; box-shadow: 0 8px 24px rgba(16, 24, 40, 0.08); }
.note-card-title { font-size: 16px; font-weight: 700; color: #101828; line-height: 1.4; }

.note-metrics {
    display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 8px;
}
.note-metric {
    background: #f8fafc; border-radius: 10px; padding: 10px 8px; text-align: center;
}
.note-metric-val { font-size: 16px; font-weight: 700; color: #101828; }
.note-metric-lab { margin-top: 2px; font-size: 12px; color: #98a2b3; }

.note-kv-list { display: flex; flex-direction: column; gap: 8px; }
.note-kv { display: flex; gap: 10px; font-size: 13px; line-height: 1.45; }
.note-kv-lab { flex: 0 0 72px; color: #98a2b3; }
.note-kv-val { flex: 1; color: #344054; min-width: 0; }

.note-takeaways {
    background: #fff6ed; border-radius: 10px; padding: 10px 12px;
}
.note-takeaways-lab { font-size: 12px; font-weight: 700; color: #dc6803; margin-bottom: 4px; }
.note-takeaways-txt { font-size: 13px; color: #344054; line-height: 1.5; }

.note-card-foot {
    margin-top: auto; display: flex; align-items: center; justify-content: space-between; gap: 8px; flex-wrap: wrap;
}
.note-cats { display: flex; flex-wrap: wrap; align-items: center; gap: 6px; min-width: 0; }
.note-cats-lab { font-size: 12px; color: #98a2b3; }
.note-cat-tag {
    font-size: 12px; padding: 2px 8px; border-radius: 999px; background: #f2f4f7; color: #344054;
}
.note-cat-empty { font-size: 12px; color: #98a2b3; }
.note-card-actions { display: flex; gap: 6px; }

:deep(.vc-card-list) {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(420px, 1fr));
    gap: 16px; padding: 4px 2px 16px;
}
:deep(.vc-card-list .vc-li) { margin: 0; height: 100%; }
:deep(.vclistwrap) { flex: 1; min-height: 0; }

@media (max-width: 1100px) {
    :deep(.vc-card-list) { grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); }
}
</style>
