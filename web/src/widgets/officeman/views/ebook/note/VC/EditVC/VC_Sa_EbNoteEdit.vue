<template>
<div class="note-report" v-loading="loading">
    <div class="top-toolbar" v-if="!hideToolbar">
        <teleport :to="position" :disabled="!position"><slot name="toolbar"></slot></teleport>
    </div>

    <div class="note-report-crumb">AI 拆解报告 &gt; 内容数据分析</div>

    <div class="note-report-head">
        <el-input v-model="focusRow.En_Title" class="note-report-title" placeholder="报告标题" :disabled="disabled" />
        <div class="note-report-meta">
            <el-input v-model="categoryText" class="note-report-cats" placeholder="分类，逗号分隔，如：小红书,干货 / 教程" :disabled="disabled" />
            <el-date-picker
                v-model="focusRow.En_PublishedAt"
                type="date"
                value-format="YYYY-MM-DD"
                placeholder="发布日期"
                :disabled="disabled"
            />
        </div>
    </div>

    <div class="note-report-stats">
        <div class="note-report-stat">
            <el-input v-model="focusRow.En_LikeCount" placeholder="—" :disabled="disabled" />
            <div class="lab">点赞</div>
        </div>
        <div class="note-report-stat">
            <el-input v-model="focusRow.En_BookmarkCount" placeholder="—" :disabled="disabled" />
            <div class="lab">收藏</div>
        </div>
        <div class="note-report-stat">
            <el-input v-model="focusRow.En_CommentCount" placeholder="—" :disabled="disabled" />
            <div class="lab">评论</div>
        </div>
        <div class="note-report-stat">
            <el-input v-model="focusRow.En_ShareCount" placeholder="—" :disabled="disabled" />
            <div class="lab">分享</div>
        </div>
    </div>

    <div class="note-report-section">
        <div class="note-report-section-title">基础信息</div>
        <div class="note-report-table">
            <div class="note-report-row" v-for="field in basicFields" :key="field.key">
                <div class="note-report-row-lab">{{ field.label }}</div>
                <el-input v-model="report[field.key]" type="textarea" :rows="field.rows || 1" :disabled="disabled"
                    :placeholder="field.placeholder || ''" />
            </div>
        </div>
    </div>

    <div class="note-report-section">
        <div class="note-report-section-title">拆解要点</div>
        <div class="note-report-table">
            <div class="note-report-row" v-for="field in previewFields" :key="field.key">
                <div class="note-report-row-lab">{{ field.label }}</div>
                <el-input v-model="report[field.key]" type="textarea" :rows="2" :disabled="disabled" />
            </div>
            <div class="note-report-row">
                <div class="note-report-row-lab">可借鉴点</div>
                <el-input v-model="report.takeaways" type="textarea" :rows="2" :disabled="disabled" />
            </div>
        </div>
    </div>

    <div class="note-ops-box">
        <div class="note-ops-title"><SvgIcon name="fa fa-pencil" /> 运营建议</div>
        <el-input v-model="report.opsAdvice" type="textarea" :rows="3" :disabled="disabled"
            placeholder="例如：可复用「痛点提问 + 步骤拆解」结构，适合技能类选题…" />
    </div>

    <el-form class="note-extra-form" label-width="110px" :disabled="disabled">
        <fminput v-model="focusRow.En_SourceUrl" label="原文链接" type="text"></fminput>
        <fminput v-model="focusRow.En_ViewCount" label="浏览" type="text"></fminput>
        <fminput v-model="focusRow.En_Status" label="状态" type="dict" :dictlist="codeTables.EbTopicStatus"></fminput>
        <fminput v-model="focusRow.En_TopicOID" label="关联选题OID" type="text"></fminput>
        <fminput v-model="focusRow.En_Content" label="原文/备注" type="textarea"></fminput>
        <slot></slot>
    </el-form>
</div>
</template>

<script setup>
import fminput from "@/ui/common/input/fminput.vue"
import { ref, computed, watch, onMounted } from 'vue'
import useFormEditor from "@/uiext/gene/hook/useFormEditor.js"

const basicFields = [
    { key: 'topicDirection', label: '选题方向', placeholder: '如：技能提升 / 效率工具' },
    { key: 'contentFormat', label: '内容形式', placeholder: '如：图文步骤卡' },
    { key: 'coreSell', label: '核心卖点', rows: 2 },
    { key: 'contentFramework', label: '内容框架', rows: 2 },
    { key: 'engageDesign', label: '互动设计', rows: 2 },
]
const previewFields = [
    { key: 'titleHighlight', label: '标题亮点' },
    { key: 'coverDesign', label: '封面设计' },
    { key: 'contentStructure', label: '内容结构' },
    { key: 'engageGuide', label: '互动引导' },
    { key: 'suitTopics', label: '适合选题' },
]

const emptyReport = () => ({
    titleHighlight: '', coverDesign: '', contentStructure: '', engageGuide: '', suitTopics: '',
    takeaways: '', categories: [], topicDirection: '', contentFormat: '', coreSell: '',
    contentFramework: '', engageDesign: '', opsAdvice: '',
})

const props = defineProps({
    page: null, api: null, bus: null, height: null,
    disabled: { type: Boolean, default: false },
    hideToolbar: { type: Boolean, default: false },
    autoLoad: { type: Boolean, default: true },
    position: null, oid: null, row: null,
})
const fmholder = useFormEditor()
const emit = defineEmits(['change', 'delete', 'refresh', 'update:focusRow'])
const VCName = "VC_Sa_EbNoteEdit"
const oidField = "Sa_EbNoteOID"
const loading = ref(false)
const focusRow = ref({})
const report = ref(emptyReport())
const codeTables = computed(() => props.page?.codeTable || {})
const rowpara = computed(() => props.oid || props.row?.[oidField] || null)

const categoryText = computed({
    get() {
        const cats = report.value.categories
        if (Array.isArray(cats)) return cats.join('、')
        return cats || ''
    },
    set(v) {
        report.value.categories = String(v || '')
            .split(/[,，、|/]/)
            .map((x) => x.trim())
            .filter(Boolean)
    },
})

function parseReport(raw) {
    if (!raw) return emptyReport()
    if (typeof raw === 'object') return { ...emptyReport(), ...raw }
    try { return { ...emptyReport(), ...(JSON.parse(raw) || {}) } } catch { return emptyReport() }
}

function syncReportToRow() {
    focusRow.value.En_ReportJson = JSON.stringify(report.value)
}

watch(rowpara, () => loadRow())
watch(report, () => syncReportToRow(), { deep: true })

onMounted(() => {
    if (props.bus) {
        props.bus.initVCEdit(VCName, focusRow)
        props.bus.registerAction(VCName, loadRow, saveChange, removeRow)
        props.bus.hub.on("refresh", loadRow)
    }
    if (props.autoLoad) loadRow()
})

const loadRow = () => {
    if (props.row != null) {
        focusRow.value = { ...props.row }
        report.value = parseReport(props.row.En_ReportJson)
        syncReportToRow()
        fmholder.init(focusRow.value, oidField)
        props.bus?.setRow?.(VCName, focusRow)
        emit('update:focusRow', focusRow.value)
        return
    }
    if (!rowpara.value) { focusRow.value = {}; report.value = emptyReport(); return }
    loading.value = true
    return props.api.queryBizSaEbNoteById(rowpara.value).then((res) => {
        focusRow.value = res || {}
        report.value = parseReport(res?.En_ReportJson)
        syncReportToRow()
        loading.value = false
        fmholder.init(focusRow.value, oidField)
        props.bus?.setRow?.(VCName, focusRow)
        emit('update:focusRow', res)
    }).catch(() => { loading.value = false })
}

function saveChange() {
    syncReportToRow()
    const para = fmholder.compare(focusRow.value)
    const payload = para == null
        ? (!focusRow.value[oidField] ? { ...focusRow.value } : null)
        : { ...para, En_ReportJson: focusRow.value.En_ReportJson }

    if (payload == null) {
        // report-only edits still need save when compare misses nested json
        if (focusRow.value[oidField]) {
            props.api.saveBizSaEbNoteList([{
                [oidField]: focusRow.value[oidField],
                En_Title: focusRow.value.En_Title,
                En_SourceUrl: focusRow.value.En_SourceUrl,
                En_Status: focusRow.value.En_Status,
                En_LikeCount: focusRow.value.En_LikeCount,
                En_BookmarkCount: focusRow.value.En_BookmarkCount,
                En_CommentCount: focusRow.value.En_CommentCount,
                En_ViewCount: focusRow.value.En_ViewCount,
                En_ShareCount: focusRow.value.En_ShareCount,
                En_PublishedAt: focusRow.value.En_PublishedAt,
                En_TopicOID: focusRow.value.En_TopicOID,
                En_Content: focusRow.value.En_Content,
                En_ReportJson: focusRow.value.En_ReportJson,
            }]).then(() => {
                swal({ text: "保存成功", icon: "info", button: "关闭" })
                loadRow(); emit('refresh')
            })
            return
        }
        swal({ text: "没有数据变更", icon: "info", button: "关闭" })
        return
    }

    props.api.saveBizSaEbNoteList([payload]).then(() => {
        swal({ text: "保存成功", icon: "info", button: "关闭" })
        loadRow(); emit('refresh')
    })
}

const removeRow = (row) => {
    alertIsOK("确认要操作吗？将删除当前行数据！", function (e) {
        if (e == "yes") props.api.removeBizSaEbNoteByIds([row[oidField]]).then(() => emit('refresh'))
    }, "删除数据行：")
}

defineExpose({ saveChange, loadRow, focusRow })
</script>

<style scoped>
.note-report {
    display: flex;
    flex-direction: column;
    gap: 16px;
    padding: 4px 4px 20px;
}
.note-report-crumb {
    font-size: 12px;
    color: #98a2b3;
}
.note-report-head { display: flex; flex-direction: column; gap: 10px; }
.note-report-title :deep(.el-input__wrapper) {
    box-shadow: none;
    background: transparent;
    padding: 0;
}
.note-report-title :deep(.el-input__inner) {
    font-size: 22px;
    font-weight: 700;
    color: #101828;
    height: auto;
}
.note-report-meta {
    display: flex;
    flex-wrap: wrap;
    gap: 10px;
    align-items: center;
}
.note-report-cats { flex: 1; min-width: 220px; }

.note-report-stats {
    display: grid;
    grid-template-columns: repeat(4, minmax(0, 1fr));
    gap: 12px;
}
.note-report-stat {
    background: #f8fafc;
    border: 1px solid #eef0f4;
    border-radius: 12px;
    padding: 12px;
    text-align: center;
}
.note-report-stat :deep(.el-input__wrapper) {
    box-shadow: none;
    background: transparent;
    padding: 0;
}
.note-report-stat :deep(.el-input__inner) {
    text-align: center;
    font-size: 22px;
    font-weight: 700;
    color: #101828;
    height: auto;
}
.note-report-stat .lab {
    margin-top: 4px;
    font-size: 12px;
    color: #98a2b3;
}

.note-report-section-title {
    font-size: 14px;
    font-weight: 700;
    color: #101828;
    margin-bottom: 10px;
}
.note-report-table {
    border: 1px solid #eef0f4;
    border-radius: 12px;
    overflow: hidden;
    background: #fff;
}
.note-report-row {
    display: grid;
    grid-template-columns: 110px 1fr;
    gap: 12px;
    padding: 12px 14px;
    border-bottom: 1px solid #f2f4f7;
    align-items: start;
}
.note-report-row:last-child { border-bottom: none; }
.note-report-row-lab {
    font-size: 13px;
    color: #667085;
    padding-top: 8px;
}
.note-report-row :deep(.el-textarea__inner),
.note-report-row :deep(.el-input__wrapper) {
    box-shadow: none;
    background: #f8fafc;
}

.note-ops-box {
    background: #ecfdf3;
    border: 1px solid #abefc6;
    border-radius: 12px;
    padding: 12px 14px;
}
.note-ops-title {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    font-size: 13px;
    font-weight: 700;
    color: #027a48;
    margin-bottom: 8px;
}
.note-ops-box :deep(.el-textarea__inner) {
    box-shadow: none;
    background: rgba(255,255,255,0.7);
}

.note-extra-form {
    margin-top: 8px;
    padding-top: 8px;
    border-top: 1px dashed #e4e7ec;
}

@media (max-width: 720px) {
    .note-report-stats { grid-template-columns: repeat(2, minmax(0, 1fr)); }
    .note-report-row { grid-template-columns: 1fr; }
}
</style>
