/**
 * 附件 API 占位：原实现依赖已移除的 mineone 模块。
 * 业务接入前返回空结果，避免编译失败。
 */
export async function listHHAttachment() {
  return { Items: [] };
}

export async function uploadHHAttachment() {
  throw new Error('[hh-attach] 附件上传 API 未配置（mineone 已移除）');
}

export async function removeHHAttachment() {
  throw new Error('[hh-attach] 附件删除 API 未配置（mineone 已移除）');
}

export async function downloadHHAttachmentBlob() {
  throw new Error('[hh-attach] 附件下载 API 未配置（mineone 已移除）');
}
