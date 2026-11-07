/**
 * BPMN 导入 · api/FineUpload（经本机 Web.Api · requestAPI6）
 * 契约对齐 sfdapi：成功体为 { success, Message }（可能再被 Unify 包一层，拦截器解包后仍为此形）
 */
import request from '@/utils/requestAPI6.js';

export type FineUploadResult = {
  success?: boolean;
  Message?: string;
  message?: string;
};

/** POST api/FineUpload/ImportFromText · { XmlContent } */
export function importFromText(xmlContent: string) {
  return request({
    url: '/api/FineUpload/ImportFromText',
    method: 'post',
    data: { XmlContent: xmlContent }
  }) as Promise<FineUploadResult | undefined>;
}

/** POST api/FineUpload/Import · multipart 文件（字段名随意，服务端取首个 file disposition） */
export function importFromFile(file: File) {
  const form = new FormData();
  form.append('qqfile', file, file.name);
  return request({
    url: '/api/FineUpload/Import',
    method: 'post',
    data: form,
    headers: { 'Content-Type': 'multipart/form-data' }
  }) as Promise<FineUploadResult | undefined>;
}

export function isImportOk(data: FineUploadResult | undefined | null): boolean {
  return data?.success === true;
}

export function importMessage(data: FineUploadResult | undefined | null): string {
  return String(data?.Message || data?.message || '');
}
