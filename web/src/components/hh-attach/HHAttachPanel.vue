<template>
    <div
        class="hh-attach-panel"
        :class="{
            'hh-attach-panel--plus': uploadMode === 'plus',
            'hh-attach-panel--drag': uploadMode === 'drag',
            'hh-attach-panel--cards': cardLayout,
            'hh-attach-panel--icon-only': actionIconOnly,
        }"
    >
        <div v-if="showUploadBlock || showFilesBlock" class="hh-attach-block">
            <div v-if="showFilesHeader" class="hh-attach-header-row">
                <span class="hh-attach-label">{{ filesHeaderLabel }}</span>
                <div v-if="showToolbarUpload || showPendingTip" class="hh-attach-toolbar">
                    <span v-if="showPendingTip" class="pending-tip">{{ pendingSaveTipText }}</span>
                    <el-upload
                        v-if="showToolbarUpload"
                        class="hh-attach-upload-inline"
                        name="file"
                        :show-file-list="false"
                        :before-upload="beforeAttachUpload"
                        :disabled="locked"
                        multiple
                    >
                        <el-button
                            type="primary"
                            :disabled="locked || uploadingFlag"
                            :loading="uploadingFlag"
                        >
                            添加附件
                        </el-button>
                    </el-upload>
                </div>
            </div>

            <el-upload
                v-if="showDragUpload"
                class="hh-attach-upload-drag"
                name="file"
                drag
                multiple
                :show-file-list="false"
                :accept="acceptAttr"
                :before-upload="beforeAttachUpload"
                :disabled="locked || uploadingFlag"
            >
                <div class="hh-attach-drag-inner">
                    <el-icon :size="42" class="hh-attach-drag-icon"><UploadFilled /></el-icon>
                    <div class="hh-attach-drag-text">
                        将文件拖到此处，或<em>点击选择</em>
                    </div>
                    <div class="hh-attach-drag-hint">{{ dragHintText }}</div>
                </div>
            </el-upload>

            <div v-if="showFilesBlock" class="upload-files" v-loading="loadingFlag">
                    <div
                        v-for="att in allAttachList"
                        :key="attachRowKey(att)"
                        class="attach-line"
                        :class="{
                            'attach-line--pending': isPendingAtt(att),
                            'attach-line--previewable': canOpenPreview(att),
                        }"
                        @click="canOpenPreview(att) && onPreview(att)"
                    >
                        <div class="attach-left">
                            <template v-if="isImageAttachment(att)">
                                <img
                                    v-if="!att.thumbFailed"
                                    class="att-thumb"
                                    :class="{ 'att-thumb--previewable': canOpenPreview(att) }"
                                    :src="getThumbSrc(att)"
                                    :alt="displayAttName(att)"
                                    :title="canOpenPreview(att) ? '点击预览' : displayAttName(att)"
                                    @error="onAttThumbError(att)"
                                    @click.stop="canOpenPreview(att) && onPreview(att)"
                                />
                                <div v-else class="att-thumb-fallback" title="图片预览失败">
                                    <DocumentChecked />
                                </div>
                            </template>
                            <template v-else>
                                <div
                                    class="att-doc-icon"
                                    :class="[getDocIconClass(att), { 'att-doc-icon--previewable': canOpenPreview(att) }]"
                                    :title="canOpenPreview(att) ? '点击预览' : displayAttName(att)"
                                    @click.stop="canOpenPreview(att) && onPreview(att)"
                                >
                                    <span class="att-doc-icon__text">{{ getDocBadgeText(att) }}</span>
                                </div>
                            </template>
                            <span v-if="!isImageAttachment(att) && getAttExt(att)" class="attach-ext">{{ getAttExt(att) }}</span>
                            <span class="attach-name" :title="displayAttName(att)">{{ displayAttName(att) }}</span>
                        </div>
                        <span
                            class="attach-actions"
                            :class="{ 'attach-actions--icon-only': actionIconOnly }"
                            @click.stop
                        >
                            <template v-if="isPendingAtt(att)">
                                <button
                                    v-if="canPreviewPending(att)"
                                    type="button"
                                    class="attach-action-btn attach-action-btn--primary"
                                    title="预览"
                                    @click="onPreview(att)"
                                >
                                    <el-icon class="att-action__icon" :class="{ 'att-action__icon--alone': actionIconOnly }"><ZoomIn /></el-icon>
                                    <span v-if="!actionIconOnly">预览</span>
                                </button>
                                <el-button
                                    v-if="!locked"
                                    link
                                    type="danger"
                                    size="small"
                                    title="移除"
                                    @click="onRemovePending(att)"
                                >
                                    <el-icon class="att-action__icon" :class="{ 'att-action__icon--alone': actionIconOnly }"><Delete /></el-icon>
                                    <span v-if="!actionIconOnly">移除</span>
                                </el-button>
                            </template>
                            <template v-else>
                                <button
                                    v-if="(customPreview || !isCadAttachment(att)) && !(imageDirectPreview && isImageAttachment(att))"
                                    type="button"
                                    class="attach-action-btn attach-action-btn--primary"
                                    title="预览"
                                    @click="onPreview(att)"
                                >
                                    <el-icon class="att-action__icon" :class="{ 'att-action__icon--alone': actionIconOnly }"><ZoomIn /></el-icon>
                                    <span v-if="!actionIconOnly">预览</span>
                                </button>
                                <button
                                    type="button"
                                    class="attach-action-btn attach-action-btn--primary"
                                    title="下载"
                                    @click="onDownload(att)"
                                >
                                    <el-icon class="att-action__icon" :class="{ 'att-action__icon--alone': actionIconOnly }"><Download /></el-icon>
                                    <span v-if="!actionIconOnly">下载</span>
                                </button>
                                <el-button
                                    v-if="!locked"
                                    link
                                    type="danger"
                                    size="small"
                                    title="删除"
                                    @click="onRemoveUploaded(att)"
                                >
                                    <el-icon class="att-action__icon" :class="{ 'att-action__icon--alone': actionIconOnly }"><Delete /></el-icon>
                                    <span v-if="!actionIconOnly">删除</span>
                                </el-button>
                            </template>
                        </span>
                    </div>
                    <div v-if="showPlusUpload" class="attach-line attach-line--add">
                        <el-upload
                            class="hh-attach-upload-plus"
                            name="file"
                            :show-file-list="false"
                            :before-upload="beforeAttachUpload"
                            :disabled="locked || uploadingFlag"
                            multiple
                        >
                            <div
                                class="att-add-tile"
                                :class="{ 'is-loading': uploadingFlag }"
                                title="上传图片"
                            >
                                <span class="att-add-tile__plus">+</span>
                            </div>
                        </el-upload>
                    </div>
                    <span v-if="allAttachCount === 0 && !showPlusUpload && !showDragUpload" class="no-file">暂无附件</span>
                    <span v-else-if="allAttachCount === 0 && showDragUpload" class="no-file">暂未选择文件，可拖拽或点击上方区域添加</span>
            </div>
        </div>

        <el-dialog
            v-if="showFilesBlock"
            v-model="previewVisible"
            :title="previewTitle || '附件预览'"
            width="min(96vw, 1200px)"
            top="2vh"
            :class="['hh-attach-preview-dialog', previewDialogClass]"
            :z-index="previewZIndex"
            append-to-body
            destroy-on-close
            @closed="onPreviewClosed"
        >
            <div class="attach-preview-wrap" v-loading="previewImageLoading">
                <img
                    v-if="previewImageUrl"
                    :src="previewImageUrl"
                    class="attach-preview-image"
                    :alt="previewTitle || '图片预览'"
                />
                <video
                    v-else-if="previewVideoUrl"
                    :src="previewVideoUrl"
                    class="attach-preview-video"
                    controls
                    playsinline
                />
                <iframe
                    v-else-if="previewUrl"
                    :src="previewUrl"
                    class="attach-preview-iframe"
                    title="附件预览"
                    frameborder="0"
                    allowfullscreen
                />
            </div>
        </el-dialog>
    </div>
</template>

<script setup>
import { computed, inject, onBeforeUnmount, provide, ref, unref, watch } from "vue"
import { ElMessage } from "element-plus"
import { Download, Delete, DocumentChecked, UploadFilled, ZoomIn } from "@element-plus/icons-vue"
import {
    attachRowKey,
    buildAttachPreviewUrl,
    buildAttachFileUrl,
    DEFAULT_ATTACH_EXTS,
    DEFAULT_PENDING_SAVE_TIP,
    DEFAULT_UPLOAD_HINT,
    displayAttName,
    getAttExt,
    getDocBadgeText,
    getDocIconClass,
    HH_ATTACH_KEY,
    fetchAttachImageBlobUrl,
    isImageAttachment,
    isCadAttachment,
    resolveAttachImageSrc,
    revokePendingAttUrl,
    useHHAttach,
} from "./hhAttachCore.js"

const props = defineProps({
    /** 业务 task；单独使用时必填 */
    task: { type: String, default: "" },
    businessOid: { type: [String, Object], default: "" },
    disabled: { type: Boolean, default: false },
    readOnly: { type: Boolean, default: false },
    section: {
        type: String,
        default: "all",
        validator: (v) => ["all", "upload", "files"].includes(v),
    },
    allowExts: {
        type: Array,
        default: () => DEFAULT_ATTACH_EXTS,
    },
    maxSizeMb: { type: Number, default: 20 },
    uploadHint: { type: String, default: "" },
    /** 附件区标题；不传时沿用「附件上传」/「附件查看」 */
    title: { type: String, default: "" },
    pendingSaveTip: { type: String, default: "" },
    uploadMissingOidMessage: { type: String, default: "" },
    /** 格式不支持时的提示文案 */
    formatErrorTip: { type: String, default: "" },
    /**
     * 选文件后、入暂存/上传前的异步处理（如视频压缩）
     * (file: File) => File | Blob | Promise<File|Blob>
     */
    transformFile: { type: Function, default: null },
    buildPreviewUrl: { type: Function, default: buildAttachPreviewUrl },
    /** 为 true 时图片附件在弹窗内直接大图预览，不走 FineReport iframe */
    imageDirectPreview: { type: Boolean, default: false },
    /** 预览弹窗层级；嵌套在自定义编辑层(如 z-index:3000)内时需抬高 */
    previewZIndex: { type: Number, default: 5600 },
    previewDialogClass: {
        type: String,
        default: "special-risk-attach-preview-dialog",
    },
    /**
     * 上传触发方式：
     * - button：工具栏「添加附件」按钮（默认，不影响其它业务页）
     * - plus：文件区内「+」瓦片上传
     * - drag：Element Plus 拖拽上传区域
     */
    uploadMode: {
        type: String,
        default: "button",
        validator: (v) => ["button", "plus", "drag"].includes(v),
    },
    /** 下载/删除/预览等操作仅显示图标 */
    actionIconOnly: { type: Boolean, default: false },
    /** 卡片网格布局：大图/文档卡片，点击可预览 */
    cardLayout: { type: Boolean, default: false },
    /** 为 true 时已落库附件预览交由父级处理（如打开业务画廊），暂存附件仍走面板内预览 */
    customPreview: { type: Boolean, default: false },
    autoLoad: { type: Boolean, default: true },
    pending: { type: Array, default: undefined },
    uploaded: { type: Array, default: undefined },
    loading: { type: Boolean, default: undefined },
    uploading: { type: Boolean, default: undefined },
})

const emit = defineEmits([
    "file-added",
    "update:pending",
    "remove-uploaded",
    "download",
    "upload-immediate",
    "changed",
    "preview",
    "preview-visible-change",
])

const injected = inject(HH_ATTACH_KEY, null)

function resolveBusinessOid() {
    const v = props.businessOid
    if (v == null || v === "") return ""
    const s = String(unref(v)).trim()
    if (!s || s.toLowerCase() === "00000000-0000-0000-0000-000000000000") return ""
    return s
}

const isStoreRoot = !injected && !!props.task

const localStore = isStoreRoot
    ? useHHAttach({
          task: props.task,
          getBusinessOid: resolveBusinessOid,
          uploadMissingOidMessage: props.uploadMissingOidMessage || undefined,
          onChanged: () => emit("changed"),
      })
    : null

if (isStoreRoot) {
    provide(HH_ATTACH_KEY, localStore)
}

const store = injected ?? localStore
const isControlled = computed(() => Array.isArray(props.pending) || Array.isArray(props.uploaded))

if (!store && !isControlled.value) {
    throw new Error("[HHAttachPanel] 请传入 task+businessOid，或传入受控的 pending/uploaded 状态")
}

if (isStoreRoot && props.autoLoad) {
    watch(
        () => resolveBusinessOid(),
        (oid) => {
            if (oid) {
                // 新增场景：主键刚生成但待传附件尚未上传时，勿用空列表覆盖暂存
                if ((store?.hhPendingAttachList?.value?.length ?? 0) > 0) {
                    return
                }
                store.loadHHAttachments()
            } else {
                store.hhPendingAttachList.value.forEach(revokePendingAttUrl)
                store.hhPendingAttachList.value = []
                store.hhAttachList.value = []
            }
        },
        { immediate: true },
    )
}

const pendingList = computed({
    get() {
        if (Array.isArray(props.pending)) return props.pending
        return store?.hhPendingAttachList?.value ?? []
    },
    set(v) {
        if (Array.isArray(props.pending)) {
            emit("update:pending", v)
            return
        }
        if (store?.hhPendingAttachList) {
            store.hhPendingAttachList.value = v
        }
    },
})
const uploadedList = computed(() => {
    if (Array.isArray(props.uploaded)) return props.uploaded
    return store?.hhAttachList?.value ?? []
})
const loadingFlag = computed(() => props.loading ?? store?.hhAttachLoading?.value ?? false)
const uploadingFlag = computed(
    () => (props.uploading ?? store?.hhUploading?.value ?? false) || transformingFlag.value,
)

const previewVisible = ref(false)
const previewUrl = ref("")
const previewImageUrl = ref("")
const previewVideoUrl = ref("")
const previewImageLoading = ref(false)
const previewOwnedBlobUrl = ref("")
const previewTitle = ref("")
const transformingFlag = ref(false)

watch(previewVisible, (visible) => {
    emit("preview-visible-change", !!visible)
})
const thumbSrcMap = ref({})
const thumbOwnedBlobUrls = ref([])

const locked = computed(() => props.disabled || props.readOnly)
const showUploadBlock = computed(() => !locked.value && (props.section === "all" || props.section === "upload"))
const showFilesBlock = computed(() => props.section === "all" || props.section === "files")
const showToolbarUpload = computed(() => showUploadBlock.value && props.uploadMode === "button")
const showPlusUpload = computed(() => showUploadBlock.value && props.uploadMode === "plus")
const showDragUpload = computed(() => showUploadBlock.value && props.uploadMode === "drag")
const hasBusinessOid = computed(() => !!resolveBusinessOid())
const showPendingTip = computed(
    () => showUploadBlock.value && !hasBusinessOid.value && pendingList.value.length > 0,
)
const showFilesHeader = computed(() => {
    if (showToolbarUpload.value) return true
    if (showDragUpload.value) return true
    if (showPendingTip.value) return true
    if (locked.value && showFilesBlock.value) return true
    return false
})
const filesHeaderLabel = computed(() => {
    const custom = String(props.title ?? "").trim()
    if (custom) return custom
    return showUploadBlock.value ? "附件上传" : "附件查看"
})
const allAttachCount = computed(() => uploadedList.value.length + pendingList.value.length)
const allAttachList = computed(() => [...pendingList.value, ...uploadedList.value])
const pendingSaveTipText = computed(() => {
    if (props.pendingSaveTip) {
        return props.pendingSaveTip.replaceAll("{count}", String(pendingList.value.length))
    }
    return DEFAULT_PENDING_SAVE_TIP(pendingList.value.length)
})
const acceptAttr = computed(() =>
    (Array.isArray(props.allowExts) ? props.allowExts : [])
        .map((ext) => `.${String(ext || "").replace(/^\./, "")}`)
        .filter(Boolean)
        .join(","),
)
const dragHintText = computed(() => String(props.uploadHint || "").trim() || DEFAULT_UPLOAD_HINT)

function isPendingAtt(att) {
    return !!(att?._isPending || att?.localKey)
}

function getPendingLocalUrl(att) {
    const path = String(att?.FJ_Path ?? "").trim()
    if (path.startsWith("blob:") || path.startsWith("data:")) return path
    if (att?.file instanceof File || att?.file instanceof Blob) {
        try {
            return URL.createObjectURL(att.file)
        } catch {
            return ""
        }
    }
    return ""
}

function canPreviewPending(att) {
    if (!isPendingAtt(att)) return false
    if (isCadAttachment(att)) return false
    // 暂存附件一律提供预览入口：图片/PDF 本地预览，其它格式提示下发后再预览
    return isImageAttachment(att)
        || !!(att?.file instanceof File || att?.file instanceof Blob)
        || String(att?.FJ_Path ?? "").trim() !== ""
}

function canOpenPreview(att) {
    if (!att) return false
    if (isPendingAtt(att)) return canPreviewPending(att)
    if (props.customPreview) return true
    if (isCadAttachment(att)) return false
    return true
}

function onAttThumbError(att) {
    if (!att) return
    att.thumbFailed = true
}

function getThumbCacheKey(att) {
    if (isPendingAtt(att) && att?.localKey) {
        return `pending:${att.localKey}`
    }
    const pk = att?.HH_AttachmentOID ?? att?.hh_AttachmentOID ?? att?.HH_ATTACHMENTOID
    return pk ? `oid:${pk}` : `path:${String(att?.FJ_Path ?? "")}`
}

function getThumbSrc(att) {
    const cached = thumbSrcMap.value[getThumbCacheKey(att)]
    if (cached) {
        return cached
    }
    // blob 异步加载前先用直链，避免空白导致误判失败
    return resolveAttachImageSrc(att)
}

function revokeThumbOwnedBlobUrls() {
    thumbOwnedBlobUrls.value.forEach((url) => {
        if (!url || !url.startsWith("blob:")) return
        try {
            URL.revokeObjectURL(url)
        } catch {
            /* ignore */
        }
    })
    thumbOwnedBlobUrls.value = []
}

async function refreshThumbSrcMap() {
    revokeThumbOwnedBlobUrls()
    const nextMap = {}
    const owned = []
    const tasks = allAttachList.value
        .filter((att) => isImageAttachment(att))
        .map(async (att) => {
            const key = getThumbCacheKey(att)
            if (isPendingAtt(att)) {
                const pendingSrc = String(att?.FJ_Path ?? "").trim()
                if (pendingSrc) {
                    nextMap[key] = pendingSrc
                }
                return
            }
            if (!props.imageDirectPreview) {
                nextMap[key] = resolveAttachImageSrc(att)
                return
            }
            const url = await fetchAttachImageBlobUrl(att)
            if (!url) return
            nextMap[key] = url
            if (url.startsWith("blob:")) {
                owned.push(url)
            }
        })
    await Promise.all(tasks)
    thumbSrcMap.value = nextMap
    thumbOwnedBlobUrls.value = owned
}

watch(
    () => [allAttachList.value, props.imageDirectPreview],
    () => {
        refreshThumbSrcMap()
    },
    { immediate: true, deep: true },
)

function isVideoExt(ext) {
    const e = String(ext || "").replace(/^\./, "").toUpperCase()
    return ["MP4", "WEBM", "MOV", "AVI", "MKV", "M4V"].includes(e)
}

function validateAttachExt(file) {
    const ext = file.name.split(".").pop()?.toLowerCase() || ""
    if (!props.allowExts.includes(ext)) {
        swal({
            text: props.formatErrorTip || "附件格式不支持，请上传文档或图片文件",
            icon: "warning",
            button: "关闭",
        })
        return false
    }
    return true
}

function validateAttachSize(file) {
    const maxSize = props.maxSizeMb * 1024 * 1024
    if (file.size > maxSize) {
        swal({ text: `附件大小不能超过${props.maxSizeMb}MB`, icon: "warning", button: "关闭" })
        return false
    }
    return true
}

function toUploadFile(input, fallbackName) {
    if (input instanceof File) return input
    if (input instanceof Blob) {
        const name = fallbackName || `file_${Date.now()}.bin`
        return new File([input], name, { type: input.type || "application/octet-stream" })
    }
    return null
}

function buildPendingRow(file) {
    const ext = file.name.split(".").pop()?.toLowerCase() || ""
    const upperExt = ext ? ext.toUpperCase() : ""
    const isImg = ["PNG", "JPG", "JPEG", "GIF", "WEBP"].includes(upperExt)
    const canLocalPreview = isImg || upperExt === "PDF" || isVideoExt(upperExt)
    const objectUrl = canLocalPreview ? URL.createObjectURL(file) : ""
    const localKey = `pending_${Date.now()}_${Math.random().toString(16).slice(2)}`
    const idx = file.name.lastIndexOf(".")
    const baseName = idx > 0 ? file.name.slice(0, idx) : file.name
    const typeWithDot = upperExt ? `.${upperExt}` : ""
    return {
        localKey,
        _isPending: true,
        file,
        FJ_Name: baseName,
        FJ_Type: typeWithDot,
        FJ_Path: objectUrl,
        thumbFailed: false,
    }
}

function addPendingFile(file) {
    const row = buildPendingRow(file)
    emit("file-added", file, row)
    pendingList.value = [...pendingList.value, row]
}

async function beforeAttachUpload(file) {
    if (props.disabled || props.readOnly) return false
    if (!validateAttachExt(file)) return false

    let finalFile = file
    if (typeof props.transformFile === "function") {
        transformingFlag.value = true
        try {
            const transformed = await props.transformFile(file)
            const next = toUploadFile(transformed, file?.name)
            if (next) finalFile = next
        } catch (err) {
            swal({
                text: err?.message || "文件处理失败，请换较小文件重试",
                icon: "error",
                button: "关闭",
            })
            return false
        } finally {
            transformingFlag.value = false
        }
    }

    if (!validateAttachSize(finalFile)) return false

    if (hasBusinessOid.value) {
        if (store?.uploadHHAttachmentFile) {
            await store.uploadHHAttachmentFile(finalFile)
        } else {
            emit("upload-immediate", finalFile)
        }
        return false
    }

    addPendingFile(finalFile)
    return false
}

function onRemovePending(att) {
    const idx = pendingList.value.findIndex((x) => x.localKey === att?.localKey)
    if (idx < 0) return
    const next = [...pendingList.value]
    revokePendingAttUrl(next[idx])
    next.splice(idx, 1)
    pendingList.value = next
}

function onDownload(att) {
    if (isControlled.value) {
        emit("download", att)
        return
    }
    store?.onDownloadAtt?.(att)
}

function onRemoveUploaded(att) {
    if (isControlled.value) {
        emit("remove-uploaded", att)
        return
    }
    store?.onRemoveHHAttach?.(att)
}

async function onPreview(att) {
    // 自定义预览：交由父组件统一处理（含暂存/已上传）
    if (props.customPreview) {
        emit("preview", att)
        return
    }

    const filePath = (att?.FJ_Path ?? "").toString().trim()

    // 暂存未落库附件：用本地 blob 预览
    if (isPendingAtt(att)) {
        const localUrl = getPendingLocalUrl(att)
        if (!localUrl) {
            ElMessage.warning("无法预览：缺少本地文件")
            return
        }
        if (previewOwnedBlobUrl.value) {
            try {
                URL.revokeObjectURL(previewOwnedBlobUrl.value)
            } catch {
                /* ignore */
            }
            previewOwnedBlobUrl.value = ""
        }
        previewTitle.value = displayAttName(att) || "附件预览"
        previewVisible.value = true
        previewImageLoading.value = false
        if (isImageAttachment(att)) {
            previewUrl.value = ""
            previewVideoUrl.value = ""
            previewImageUrl.value = localUrl
            if (!filePath.startsWith("blob:") && localUrl.startsWith("blob:")) {
                previewOwnedBlobUrl.value = localUrl
            }
            return
        }
        const ext = String(getAttExt(att) || "").replace(/^\./, "").toUpperCase()
        if (isVideoExt(ext)) {
            previewImageUrl.value = ""
            previewUrl.value = ""
            previewVideoUrl.value = localUrl
            if (!filePath.startsWith("blob:") && localUrl.startsWith("blob:")) {
                previewOwnedBlobUrl.value = localUrl
            }
            return
        }
        if (ext && ext !== "PDF") {
            previewVisible.value = false
            ElMessage.warning("该格式暂存时无法在线预览，请下发保存后再试")
            if (!filePath.startsWith("blob:") && localUrl.startsWith("blob:")) {
                try {
                    URL.revokeObjectURL(localUrl)
                } catch {
                    /* ignore */
                }
            }
            return
        }
        previewImageUrl.value = ""
        previewVideoUrl.value = ""
        previewUrl.value = localUrl
        if (!filePath.startsWith("blob:") && localUrl.startsWith("blob:")) {
            previewOwnedBlobUrl.value = localUrl
        }
        return
    }

    if (props.customPreview) {
        emit("preview", att)
        return
    }

    if (!filePath) {
        ElMessage.warning("无法预览：缺少附件路径")
        return
    }
    if ((props.imageDirectPreview || filePath.startsWith("blob:")) && isImageAttachment(att)) {
        previewUrl.value = ""
        previewVideoUrl.value = ""
        previewTitle.value = displayAttName(att) || "图片预览"
        previewVisible.value = true
        previewImageLoading.value = true
        previewImageUrl.value = ""
        if (previewOwnedBlobUrl.value) {
            try {
                URL.revokeObjectURL(previewOwnedBlobUrl.value)
            } catch {
                /* ignore */
            }
            previewOwnedBlobUrl.value = ""
        }
        try {
            if (filePath.startsWith("blob:") || filePath.startsWith("data:")) {
                previewImageUrl.value = filePath
                previewImageLoading.value = false
                return
            }
            const url = await fetchAttachImageBlobUrl(att)
            if (!url) {
                ElMessage.error("图片加载失败")
                previewVisible.value = false
                return
            }
            previewImageUrl.value = url
            if (url.startsWith("blob:") && !filePath.startsWith("blob:")) {
                previewOwnedBlobUrl.value = url
            }
        } catch {
            ElMessage.error("图片加载失败")
            previewVisible.value = false
        } finally {
            previewImageLoading.value = false
        }
        return
    }
    if (filePath.startsWith("blob:")) {
        ElMessage.warning("附件尚未保存，无法使用在线预览；请先保存后再试")
        return
    }
    const uploadedExt = String(getAttExt(att) || "").replace(/^\./, "").toUpperCase()
    if (isVideoExt(uploadedExt)) {
        previewImageUrl.value = ""
        previewUrl.value = ""
        previewVideoUrl.value = buildAttachFileUrl(filePath)
        previewTitle.value = displayAttName(att) || "视频预览"
        previewVisible.value = true
        return
    }
    previewImageUrl.value = ""
    previewVideoUrl.value = ""
    previewUrl.value = props.buildPreviewUrl(filePath)
    previewTitle.value = displayAttName(att) || "附件预览"
    previewVisible.value = true
}

function onPreviewClosed() {
    previewUrl.value = ""
    previewImageUrl.value = ""
    previewVideoUrl.value = ""
    previewImageLoading.value = false
    previewTitle.value = ""
    if (previewOwnedBlobUrl.value) {
        try {
            URL.revokeObjectURL(previewOwnedBlobUrl.value)
        } catch {
            /* ignore */
        }
        previewOwnedBlobUrl.value = ""
    }
}

function revokeAllPending() {
    pendingList.value.forEach(revokePendingAttUrl)
    pendingList.value = []
}

function exposedLoadHHAttachments(...args) {
    return store?.loadHHAttachments?.(...args)
}

function exposedUploadPendingHHAttachments(...args) {
    return store?.uploadPendingHHAttachments?.(...args)
}

function exposedUploadHHAttachmentFile(...args) {
    return store?.uploadHHAttachmentFile?.(...args)
}

function exposedDisposeHHAttach(...args) {
    return (store?.disposeHHAttach ?? clearHHPendingAttachList)?.(...args)
}

function exposedLoad(...args) {
    return store?.loadHHAttachments?.(...args)
}

function exposedUploadPending(...args) {
    return store?.uploadPendingHHAttachments?.(...args)
}

function clearHHAttachList() {
    if (isControlled.value) return
    store?.hhAttachList && (store.hhAttachList.value = [])
}

function clearHHPendingAttachList() {
    revokeAllPending()
}

function getPendingAttachList() {
    return pendingList.value
}

function getPendingAttachCount() {
    return pendingList.value.length
}

function hasPendingAttachments() {
    return pendingList.value.length > 0
}

/** 写入暂存附件列表（不撤销原列表 URL，调用方如需清理请先 clearHHPendingAttachList） */
function setPendingAttachList(list) {
    pendingList.value = Array.isArray(list) ? [...list] : []
}

if (isStoreRoot) {
    onBeforeUnmount(() => {
        store.disposeHHAttach()
        revokeThumbOwnedBlobUrls()
        onPreviewClosed()
    })
} else {
    onBeforeUnmount(() => {
        revokeThumbOwnedBlobUrls()
        onPreviewClosed()
    })
}

const loadHHAttachments = exposedLoadHHAttachments
const uploadPendingHHAttachments = exposedUploadPendingHHAttachments
const uploadHHAttachmentFile = exposedUploadHHAttachmentFile
const disposeHHAttach = exposedDisposeHHAttach
const load = exposedLoad
const uploadPending = exposedUploadPending

function getUploadedAttachCount() {
    return uploadedList.value.length
}

function hasUploadedAttachments() {
    return uploadedList.value.length > 0
}

function getUploadedAttachList() {
    return uploadedList.value
}

function removePendingAttach(att) {
    onRemovePending(att)
}

defineExpose({
    loadHHAttachments,
    uploadPendingHHAttachments,
    uploadHHAttachmentFile,
    disposeHHAttach,
    load,
    uploadPending,
    revokeAllPending,
    clearHHAttachList,
    clearHHPendingAttachList,
    getPendingAttachList,
    getPendingAttachCount,
    hasPendingAttachments,
    setPendingAttachList,
    getUploadedAttachCount,
    hasUploadedAttachments,
    getUploadedAttachList,
    removePendingAttach,
})

</script>

<style scoped>
.hh-attach-panel {
    display: block;
    width: 100%;
}

.hh-attach-block {
    display: flex;
    flex-direction: column;
    gap: 10px;
    width: 100%;
}

.hh-attach-header-row {
    display: flex;
    flex-direction: row;
    align-items: center;
    flex-wrap: wrap;
    gap: 10px 12px;
    width: 100%;
}

.hh-attach-label {
    flex-shrink: 0;
    white-space: nowrap;
    writing-mode: horizontal-tb;
    font-size: 13px;
    color: #475569;
    font-weight: 600;
    line-height: 32px;
}

.hh-attach-toolbar {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 10px;
    margin-left: auto;
    justify-content: flex-end;
}

.hh-attach-upload-inline {
    display: inline-flex;
}

.hh-attach-upload-inline :deep(.el-upload) {
    display: inline-flex;
}

.hh-attach-upload-drag {
    width: 100%;
}

.hh-attach-upload-drag :deep(.el-upload) {
    width: 100%;
}

.hh-attach-upload-drag :deep(.el-upload-dragger) {
    width: 100%;
    padding: 28px 16px;
    border-radius: 10px;
    background: #fafbfc;
}

.hh-attach-upload-drag :deep(.el-upload-dragger:hover) {
    border-color: #409eff;
}

.hh-attach-drag-inner {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 8px;
    pointer-events: none;
}

.hh-attach-drag-icon {
    color: #409eff;
}

.hh-attach-drag-text {
    font-size: 14px;
    color: #606266;
    line-height: 1.5;
}

.hh-attach-drag-text em {
    font-style: normal;
    color: #409eff;
}

.hh-attach-drag-hint {
    max-width: 420px;
    font-size: 12px;
    color: #909399;
    line-height: 1.5;
    text-align: center;
}

.hh-attach-panel--drag .upload-files {
    margin-top: 2px;
}

.pending-tip {
    font-size: 14px;
    color: #e6a23c;
    line-height: 1.35;
}

.upload-files {
    display: flex;
    flex-direction: column;
    gap: 6px;
    min-height: 28px;
    align-items: stretch;
    width: 100%;
    padding: 10px;
    border-radius: 8px;
    background: #f7f8fa;
    border: 1px solid #ebedf0;
    box-sizing: border-box;
}

.no-file {
    display: block;
    width: 100%;
    box-sizing: border-box;
    color: #969799;
    padding: 8px 0;
    text-align: center;
    font-size: 13px;
}

.att-thumb {
    width: 40px;
    height: 40px;
    object-fit: cover;
    border-radius: 6px;
    border: 1px solid #ebedf0;
    flex-shrink: 0;
}

.att-thumb--previewable {
    cursor: pointer;
}

.att-thumb--previewable:hover {
    border-color: #409eff;
    box-shadow: 0 0 0 1px rgba(64, 158, 255, 0.35);
}

.att-thumb-fallback {
    width: 40px;
    height: 40px;
    border-radius: 6px;
    border: 1px solid #ebedf0;
    background: #fff;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #409eff;
    flex-shrink: 0;
    font-size: 16px;
}

.att-doc-icon {
    width: 40px;
    height: 40px;
    border-radius: 6px;
    border: 1px solid #ebedf0;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
    font-weight: 900;
    overflow: hidden;
}

.att-doc-icon__text {
    font-size: 11px;
    letter-spacing: 0.2px;
}

.att-doc-icon--word {
    background: rgba(37, 99, 235, 0.08);
    border-color: rgba(37, 99, 235, 0.25);
    color: #2563eb;
}

.att-doc-icon--excel {
    background: rgba(5, 150, 105, 0.08);
    border-color: rgba(5, 150, 105, 0.25);
    color: #059669;
}

.att-doc-icon--pdf {
    background: rgba(219, 39, 119, 0.08);
    border-color: rgba(219, 39, 119, 0.25);
    color: #db2777;
}

.att-doc-icon--cad {
    background: rgba(234, 88, 12, 0.08);
    border-color: rgba(234, 88, 12, 0.25);
    color: #ea580c;
}

.att-doc-icon--other {
    background: #f1f5f9;
    border-color: #e2e8f0;
    color: #64748b;
}

.attach-line {
    display: flex;
    flex-flow: row nowrap;
    justify-content: space-between;
    align-items: center;
    width: 100%;
    padding: 8px 10px;
    border-radius: 8px;
    background: #fff;
    box-sizing: border-box;
}

.attach-line--pending {
    background: #fffbf0;
}

.attach-left {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    flex: 1;
    min-width: 0;
}

.attach-ext {
    flex-shrink: 0;
    background: #eef2ff;
    color: #4f46e5;
    border-radius: 6px;
    padding: 2px 6px;
    font-size: 11px;
    line-height: 16px;
    font-weight: 700;
}

.attach-name {
    flex: 1;
    min-width: 0;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    color: #323233;
    font-size: 14px;
}

.attach-actions {
    flex-shrink: 0;
    display: inline-flex;
    align-items: center;
    gap: 4px;
}

.attach-action-btn {
    display: inline-flex;
    align-items: center;
    border: 0;
    background: transparent;
    padding: 0 4px;
    font-size: 12px;
    line-height: 1;
    cursor: pointer;
}

.attach-action-btn--primary {
    color: var(--el-color-primary);
}

.attach-action-btn--primary:hover {
    color: var(--el-color-primary-light-3);
}

.att-action__icon {
    margin-right: 4px;
    font-size: 14px;
}

.att-action__icon--alone {
    margin-right: 0;
}

.attach-actions--icon-only .attach-action-btn,
.attach-actions--icon-only :deep(.el-button) {
    padding: 0 2px;
    min-height: 22px;
}

.attach-line--add {
    width: auto;
    padding: 0;
    background: transparent;
    justify-content: center;
}

.hh-attach-upload-plus {
    display: inline-flex;
}

.hh-attach-upload-plus :deep(.el-upload) {
    display: inline-flex;
}

.att-add-tile {
    width: 64px;
    height: 64px;
    border-radius: 6px;
    border: 1px dashed #c0c4cc;
    background: #fff;
    color: #909399;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    box-sizing: border-box;
    transition: border-color 0.15s ease, color 0.15s ease, background 0.15s ease;
}

.att-add-tile:hover {
    border-color: var(--el-color-primary);
    color: var(--el-color-primary);
    background: var(--el-color-primary-light-9, #ecf5ff);
}

.att-add-tile.is-loading {
    opacity: 0.65;
    pointer-events: none;
}

.att-add-tile__plus {
    font-size: 28px;
    line-height: 1;
    font-weight: 300;
    user-select: none;
}

.hh-attach-panel--plus .upload-files {
    flex-direction: row;
    flex-wrap: wrap;
    align-items: flex-start;
    gap: 8px;
}

.hh-attach-panel--cards .upload-files {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(148px, 1fr));
    gap: 12px;
    padding: 12px;
    background: #f8fafc;
}

.hh-attach-panel--cards .attach-line {
    flex-direction: column;
    align-items: stretch;
    gap: 8px;
    width: auto;
    min-height: 168px;
    padding: 10px;
    border: 1px solid #e2e8f0;
    border-radius: 12px;
    background: #fff;
    box-shadow: 0 1px 2px rgba(15, 23, 42, 0.04);
    transition: border-color 0.15s ease, box-shadow 0.15s ease, transform 0.15s ease;
}

.hh-attach-panel--cards .attach-line--previewable {
    cursor: pointer;
}

.hh-attach-panel--cards .attach-line--previewable:hover {
    border-color: #93c5fd;
    box-shadow: 0 6px 16px rgba(37, 99, 235, 0.12);
    transform: translateY(-1px);
}

.hh-attach-panel--cards .attach-line--pending {
    background: linear-gradient(180deg, #fffbeb 0%, #ffffff 48%);
    border-color: #fde68a;
}

.hh-attach-panel--cards .attach-left {
    flex-direction: column;
    align-items: stretch;
    gap: 8px;
}

.hh-attach-panel--cards .att-thumb,
.hh-attach-panel--cards .att-thumb-fallback,
.hh-attach-panel--cards .att-doc-icon {
    width: 100%;
    height: 108px;
    border-radius: 10px;
}

.hh-attach-panel--cards .att-doc-icon {
    font-size: 22px;
}

.hh-attach-panel--cards .att-doc-icon__text {
    font-size: 16px;
    font-weight: 800;
    letter-spacing: 0.02em;
}

.hh-attach-panel--cards .att-doc-icon--previewable {
    cursor: pointer;
}

.hh-attach-panel--cards .attach-ext {
    align-self: flex-start;
}

.hh-attach-panel--cards .attach-name {
    width: 100%;
    font-size: 12px;
    line-height: 1.4;
    white-space: normal;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
}

.hh-attach-panel--cards .attach-actions {
    justify-content: flex-end;
    width: 100%;
    padding-top: 2px;
    border-top: 1px dashed #e2e8f0;
}

.hh-attach-panel--cards .attach-line--add {
    min-height: 168px;
    justify-content: center;
}

.hh-attach-panel--cards .hh-attach-upload-plus,
.hh-attach-panel--cards .hh-attach-upload-plus :deep(.el-upload),
.hh-attach-panel--cards .att-add-tile {
    width: 100%;
    height: 100%;
    min-height: 148px;
}

.attach-preview-wrap {
    min-height: min(82vh, 880px);
    display: flex;
    align-items: stretch;
    justify-content: stretch;
}

.attach-preview-iframe {
    flex: 1;
    width: 100%;
    min-height: min(82vh, 880px);
    height: min(82vh, 880px);
    border: 0;
    border-radius: 8px;
    background: #f1f5f9;
}

.attach-preview-image {
    display: block;
    max-width: 100%;
    max-height: min(82vh, 880px);
    margin: 0 auto;
    object-fit: contain;
    background: #f1f5f9;
    border-radius: 8px;
}

.attach-preview-video {
    display: block;
    width: 100%;
    max-height: min(82vh, 880px);
    margin: 0 auto;
    background: #0f172a;
    border-radius: 8px;
}

:deep(.special-risk-attach-preview-dialog .el-dialog__body) {
    padding: 10px 18px 20px;
    max-height: 86vh;
    overflow: auto;
    box-sizing: border-box;
}
</style>

<style lang="css">
/* 附件大图预览需盖过自定义编辑层(常见 z-index:3000) / popover */
.hh-attach-preview-dialog,
.special-risk-attach-preview-dialog,
.order-issuer-attach-preview-dialog,
.ss-regulations-attach-preview-dialog {
    z-index: 5600 !important;
}
.el-overlay:has(.hh-attach-preview-dialog),
.el-overlay:has(.special-risk-attach-preview-dialog),
.el-overlay:has(.order-issuer-attach-preview-dialog),
.el-overlay:has(.ss-regulations-attach-preview-dialog) {
    z-index: 5600 !important;
}
</style>
