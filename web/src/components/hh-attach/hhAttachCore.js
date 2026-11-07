export const HH_ATTACH_KEY = Symbol("hhAttach")

import {
    downloadHHAttachmentBlob,
    listHHAttachment,
    removeHHAttachment,
    uploadHHAttachment,
} from "./hhAttachApi.js"
import { ElMessage } from "element-plus"
import { ref, unref } from "vue"

export const FR_ATTACH_PREVIEW_BASE =
    "http://10.10.10.10/webroot/decision/view/form?viewlet=FR/YuLan_FR.frm&Data="

export const DEFAULT_ATTACH_EXTS = ["doc", "pdf", "xls", "docx", "xlsx", "png", "jpg", "jpeg", "gif", "zip", "rar", "7z", "zar", "dwg"]
export const DEFAULT_UPLOAD_HINT =
    "只能上传.doc/.pdf/.xls/.docx/.xlsx/.png/.jpg/.jpeg/.gif/.zip/.rar/.7z/.dwg，且不超过20MB"
export const DEFAULT_PENDING_SAVE_TIP = (count) => `已暂存 ${count} 个附件，点击「保存」后将上传并落库`
const IMAGE_EXTS = ["PNG", "JPG", "JPEG", "GIF", "WEBP"]
const ATTACH_CACHE = new Map()
const ATTACH_INFLIGHT = new Map()

function getAttachCacheKey(task, oid) {
    const taskText = (task ?? "").toString().trim()
    const oidText = (oid ?? "").toString().trim()
    return `${taskText}::${oidText}`
}

function cloneAttachRows(rows) {
    return Array.isArray(rows) ? rows.map((row) => ({ ...row })) : []
}

function setAttachCache(task, oid, rows) {
    const oidText = (oid ?? "").toString().trim()
    if (!oidText) return
    ATTACH_CACHE.set(getAttachCacheKey(task, oidText), cloneAttachRows(rows))
}

export function clearHHAttachCache(task, oid) {
    const oidText = (oid ?? "").toString().trim()
    if (oidText) {
        ATTACH_CACHE.delete(getAttachCacheKey(task, oidText))
        return
    }
    const prefix = `${(task ?? "").toString().trim()}::`
    Array.from(ATTACH_CACHE.keys()).forEach((key) => {
        if (!prefix || key.startsWith(prefix)) ATTACH_CACHE.delete(key)
    })
}

export function buildAttachFileUrl(fjPath) {
    const raw = (fjPath ?? "").toString().trim()
    if (!raw || raw.startsWith("blob:")) return raw
    if (/^https?:\/\//i.test(raw)) return raw
    const prefix = (import.meta.env.VITE_FILE_DOWNLOAD_PREFIX || "").trim()
    if (!prefix) return raw
    return `${prefix.replace(/\/+$/, "")}/${raw.replace(/^\/+/, "")}`
}

export function buildAttachPreviewUrl(fjPath) {
    return FR_ATTACH_PREVIEW_BASE + encodeURIComponent(buildAttachFileUrl(fjPath))
}

export function extractAttachmentList(res) {
    if (res == null) return []
    if (Array.isArray(res)) return res
    if (Array.isArray(res.Items)) return res.Items
    if (Array.isArray(res.items)) return res.items
    if (Array.isArray(res.data)) return res.data
    if (res.data && Array.isArray(res.data.data)) return res.data.data
    if (Array.isArray(res.data?.Items)) return res.data.Items
    if (Array.isArray(res.data?.items)) return res.data.items
    if (Array.isArray(res.rows)) return res.rows
    if (Array.isArray(res.data?.rows)) return res.data.rows
    if (res.result && Array.isArray(res.result.data)) return res.result.data
    return []
}

export function normalizeAttRow(raw) {
    if (!raw || typeof raw !== "object") return {}
    const oid = raw.HH_AttachmentOID ?? raw.hh_AttachmentOID ?? raw.HH_ATTACHMENTOID
    return {
        ...raw,
        HH_AttachmentOID: oid,
        FJ_Name: raw.FJ_Name ?? raw.fJ_Name ?? raw.fj_name,
        FJ_Type: raw.FJ_Type ?? raw.fJ_Type ?? raw.fj_type,
        FJ_Path: raw.FJ_Path ?? raw.fJ_Path ?? raw.fj_path,
        FJ_BusiType: raw.FJ_BusiType ?? raw.fJ_BusiType ?? raw.fj_busitype,
    }
}

export function displayAttName(att) {
    const n = att?.FJ_Name ?? ""
    const t = att?.FJ_Type ?? ""
    return (n + t) || "附件"
}

export function getAttExt(att) {
    const type = (att?.FJ_Type ?? "").toString().trim()
    if (type) {
        return type.startsWith(".") ? type.slice(1).toUpperCase() : type.toUpperCase()
    }
    const name = (att?.FJ_Name ?? "").toString().trim()
    const idx = name.lastIndexOf(".")
    if (idx >= 0 && idx < name.length - 1) return name.slice(idx + 1).toUpperCase()
    return ""
}

export function isImageAttachment(att) {
    const ext = getAttExt(att)
    return !!ext && IMAGE_EXTS.includes(ext)
}

export function isCadAttachment(att) {
    return getDocType(att) === "cad"
}

export function resolveAttachImageSrc(att) {
    const path = String(att?.FJ_Path ?? "").trim()
    if (!path) {
        return ""
    }
    if (path.startsWith("blob:") || /^https?:\/\//i.test(path)) {
        return path
    }
    const prefix = String(import.meta.env.VITE_FILE_DOWNLOAD_PREFIX ?? "").trim().replace(/\/$/, "")
    const normalized = path.replace(/^\//, "")
    return prefix ? `${prefix}/${normalized}` : normalized
}

function getAttachPrimaryKey(att) {
    return att?.HH_AttachmentOID ?? att?.hh_AttachmentOID ?? att?.HH_ATTACHMENTOID ?? ""
}

/** 解析下载接口返回的 Blob，失败时返回 null */
export async function parseHHAttachmentBlob(axiosRes) {
    const blob = axiosRes instanceof Blob ? axiosRes : axiosRes?.data
    if (!(blob instanceof Blob) || blob.size === 0) {
        return null
    }
    if (blob.type && String(blob.type).includes("json")) {
        return null
    }
    return blob
}

/**
 * 已落库图片附件：走鉴权下载接口并返回 blob URL（供 img 预览/缩略图）。
 * 待上传 blob: 或已是 http(s) 时直接返回原地址。
 */
export async function fetchAttachImageBlobUrl(att) {
    const path = String(att?.FJ_Path ?? "").trim()
    if (!path) {
        return ""
    }
    if (path.startsWith("blob:") || /^https?:\/\//i.test(path)) {
        return path
    }
    const pk = getAttachPrimaryKey(att)
    if (!pk) {
        return resolveAttachImageSrc(att)
    }
    try {
        const axiosRes = await downloadHHAttachmentBlob(String(pk))
        const blob = await parseHHAttachmentBlob(axiosRes)
        if (!blob) {
            return ""
        }
        return URL.createObjectURL(blob)
    } catch {
        return ""
    }
}

export function getDocType(att) {
    const ext = getAttExt(att)
    if (!ext) return "other"
    if (["DOC", "DOCX"].includes(ext)) return "word"
    if (["XLS", "XLSX"].includes(ext)) return "excel"
    if (["PDF"].includes(ext)) return "pdf"
    if (["DWG", "DXF"].includes(ext)) return "cad"
    return "other"
}

export function getDocBadgeText(att) {
    const t = getDocType(att)
    if (t === "word") return "Word"
    if (t === "excel") return "Excel"
    if (t === "pdf") return "PDF"
    if (t === "cad") return "CAD"
    return "File"
}

export function getDocIconClass(att) {
    return `att-doc-icon--${getDocType(att)}`
}

export function attachRowKey(row) {
    if (row?.localKey) return String(row.localKey)
    const k = row?.HH_AttachmentOID || row?.hh_AttachmentOID || row?.HH_ATTACHMENTOID
    return k ? String(k) : String(Math.random())
}

export function revokePendingAttUrl(att) {
    if (!att?.FJ_Path || typeof att.FJ_Path !== "string" || !att.FJ_Path.startsWith("blob:")) return
    try {
        URL.revokeObjectURL(att.FJ_Path)
    } catch {
        /* ignore */
    }
}

const EMPTY_GUID = "00000000-0000-0000-0000-000000000000"

function normalizeOidText(raw) {
    if (raw == null || raw === "") return ""
    const s = String(unref(raw)).trim()
    if (!s || s.toLowerCase() === EMPTY_GUID) return ""
    return s
}

function resolveBusinessOid(getBusinessOid, overrideOid) {
    // 显式传入的业务主键优先（保存后上传场景：面板 prop 可能尚未刷新）
    const fromOverride = normalizeOidText(overrideOid)
    if (fromOverride) return fromOverride
    const raw = typeof getBusinessOid === "function" ? getBusinessOid() : unref(getBusinessOid)
    return normalizeOidText(raw)
}

export function useHHAttach(options) {
    const {
        getBusinessOid,
        task,
        attachPanelRef: externalPanelRef,
        uploadMissingOidMessage = "未找到单据主键，无法上传附件；请先保存单据后再重试",
        onChanged,
    } = options

    const attachPanelRef = externalPanelRef ?? ref(null)
    const hhAttachList = ref([])
    const hhPendingAttachList = ref([])
    const hhAttachLoading = ref(false)
    const hhUploading = ref(false)

    async function loadHHAttachments() {
        const oid = resolveBusinessOid(getBusinessOid)
        if (!oid) {
            hhAttachList.value = []
            return
        }
        const cacheKey = getAttachCacheKey(task, oid)
        if (ATTACH_CACHE.has(cacheKey)) {
            hhAttachList.value = cloneAttachRows(ATTACH_CACHE.get(cacheKey))
            return
        }
        hhAttachLoading.value = true
        try {
            let req = ATTACH_INFLIGHT.get(cacheKey)
            if (!req) {
                const listParams = { pageNum: 1, pageSize: 200, oid }
                if (task) {
                    listParams.task = task
                }
                req = listHHAttachment(listParams).finally(() => {
                    ATTACH_INFLIGHT.delete(cacheKey)
                })
                ATTACH_INFLIGHT.set(cacheKey, req)
            }
            const res = await req
            let rows = extractAttachmentList(res).map(normalizeAttRow)
            if (task) {
                const taskText = String(task).trim()
                rows = rows.filter((row) => {
                    const t = String(row.FJ_BusiType ?? "").trim()
                    return !t || t === taskText
                })
            }
            setAttachCache(task, oid, rows)
            hhAttachList.value = rows
        } catch {
            hhAttachList.value = []
        } finally {
            hhAttachLoading.value = false
        }
    }

    async function onDownloadAtt(att) {
        const pk = att.HH_AttachmentOID || att.hh_AttachmentOID
        if (!pk) {
            ElMessage.warning("无法下载：缺少附件主键")
            return
        }
        const fileName = displayAttName(att) || "attachment"
        try {
            const axiosRes = await downloadHHAttachmentBlob(String(pk))
            const blob = axiosRes instanceof Blob ? axiosRes : axiosRes?.data
            if (!(blob instanceof Blob)) {
                ElMessage.error("下载失败")
                return
            }
            if (blob.size === 0) {
                ElMessage.error("文件为空")
                return
            }
            if (blob.type && String(blob.type).includes("json")) {
                const text = await blob.text()
                try {
                    const j = JSON.parse(text)
                    ElMessage.error(j.message || j.msg || "下载失败")
                } catch {
                    ElMessage.error("下载失败")
                }
                return
            }
            const url = URL.createObjectURL(blob)
            const a = document.createElement("a")
            a.href = url
            a.download = fileName
            document.body.appendChild(a)
            a.click()
            document.body.removeChild(a)
            URL.revokeObjectURL(url)
        } catch {
            ElMessage.error("下载失败")
        }
    }

    async function uploadHHAttachmentFile(file) {
        const businessOid = resolveBusinessOid(getBusinessOid)
        if (!businessOid) {
            ElMessage.warning(uploadMissingOidMessage)
            return false
        }
        if (!file) return false

        hhUploading.value = true
        try {
            const fd = new FormData()
            fd.append("file", file)
            fd.append("BusinessOID", businessOid)
            fd.append("task", task)
            fd.append("idx", String(hhAttachList.value.length))

            await uploadHHAttachment(fd, {})
            ElMessage.success("上传成功")
            clearHHAttachCache(task, businessOid)
            await loadHHAttachments()
            onChanged?.()
            return true
        } catch {
            ElMessage.error("附件上传失败，请稍后重试")
            return false
        } finally {
            hhUploading.value = false
        }
    }

    function unwrapHhUploadFileOutput(res) {
        const layer = res?.data ?? res?.result ?? res
        if (layer?.status === "error") {
            throw new Error(layer.info || layer.message || "附件上传失败")
        }
        const payload = layer?.status ? (layer.data ?? layer.result) : layer
        return payload?.data ?? payload?.result ?? payload ?? null
    }

    /** 雪花 Id 不可 Number()，保持字符串以免精度丢失 */
    function pickSysFileId(raw) {
        const v = raw?.Id ?? raw?.id ?? raw?.sysFileId ?? raw
        if (v == null || v === "") return ""
        const s = String(v).trim()
        if (!/^\d+$/.test(s) || s === "0") return ""
        return s
    }

    async function uploadPendingHHAttachments(oid) {
        const businessOid = resolveBusinessOid(getBusinessOid, oid)
        if (!businessOid) {
            ElMessage.warning(uploadMissingOidMessage)
            return false
        }
        if (hhPendingAttachList.value.length === 0) return true

        hhUploading.value = true
        const uploads = []
        try {
            const uploadNext = async (uploadIdx) => {
                const pending = hhPendingAttachList.value[0]
                if (!pending) return

                if (!(pending.file instanceof File) && !(pending.file instanceof Blob)) {
                    ElMessage.error(`附件「${pending.FJ_Name || "未命名"}」文件已失效，请重新选择后再保存`)
                    throw new Error("附件文件已失效")
                }

                // 优先用行上已指定的 FJ_Idx（如规程措施主文件=0），否则按上传顺序
                const rowIdx = Number(pending?.FJ_Idx)
                const idxVal = Number.isFinite(rowIdx) ? rowIdx : uploadIdx

                const fd = new FormData()
                fd.append("file", pending.file)
                fd.append("BusinessOID", businessOid)
                fd.append("task", task)
                fd.append("idx", String(idxVal))

                const res = await uploadHHAttachment(fd, {})
                const fo = unwrapHhUploadFileOutput(res)
                const sysFileId = pickSysFileId(fo)
                if (sysFileId) {
                    const extRaw = `${fo?.Suffix ?? fo?.suffix ?? pending?.FJ_Type ?? ""}`.trim()
                    uploads.push({
                        sysFileId,
                        fileName:
                            pending?.file?.name ||
                            `${pending?.FJ_Name || ""}${pending?.FJ_Type || ""}` ||
                            fo?.FileName ||
                            fo?.fileName ||
                            "file",
                        extension: extRaw.replace(/^\./, ""),
                        mimeType: pending?.file?.type || "",
                        sizeBytes: Number(pending?.file?.size || 0) || 0,
                        previewUrl: fo?.PreviewUrl ?? fo?.previewUrl ?? "",
                    })
                } else {
                    console.warn("[hh-attach] 上传成功但未解析到 SysFileId", fo)
                }

                revokePendingAttUrl(pending)
                hhPendingAttachList.value.shift()
                return uploadNext(uploadIdx + 1)
            }

            await uploadNext(0)
            ElMessage.success("附件上传成功")
            clearHHAttachCache(task, businessOid)
            await loadHHAttachments()
            onChanged?.()
            return { ok: true, uploads }
        } catch (err) {
            ElMessage.error(err?.message || "附件上传失败，请稍后重试")
            return false
        } finally {
            hhUploading.value = false
        }
    }

    function onRemoveHHAttach(row) {
        const pk = row.HH_AttachmentOID || row.hh_AttachmentOID
        if (!pk) return
        alertIsOK(
            "确定删除该附件吗？",
            (e) => {
                if (e !== "yes") return
                removeHHAttachment(String(pk))
                    .then((res) => {
                        if (res?.status === "success") {
                            ElMessage.success(res?.data || "已删除")
                            clearHHAttachCache(task, resolveBusinessOid(getBusinessOid))
                            loadHHAttachments()
                            onChanged?.()
                            return
                        }
                        ElMessage.error(res?.info || "删除失败")
                    })
                    .catch(() => {
                        ElMessage.error("删除失败")
                    })
            },
            (row.FJ_Name || "") + (row.FJ_Type || ""),
        )
    }

    function disposeHHAttach() {
        attachPanelRef.value?.revokeAllPending?.()
        hhPendingAttachList.value.forEach(revokePendingAttUrl)
        hhPendingAttachList.value = []
    }

    return {
        attachPanelRef,
        hhAttachList,
        hhPendingAttachList,
        hhAttachLoading,
        hhUploading,
        loadHHAttachments,
        uploadHHAttachmentFile,
        uploadPendingHHAttachments,
        onDownloadAtt,
        onRemoveHHAttach,
        disposeHHAttach,
    }
}
