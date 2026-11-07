import request from '@/utils/requestAPI6.js';

/**
 * 富文本插图独立上传（FluentFTP）
 * @param {File|Blob} file
 * @param {string} [prefix='/wang']
 * @param {(e: ProgressEvent) => void} [onUploadProgress]
 */
export function uploadWangImage(file, prefix = '/wang', onUploadProgress) {
	const fd = new FormData();
	fd.append('file', file);
	const p = `${prefix ?? ''}`.trim() || '/wang';
	fd.append('prefix', p.replace(/^\/+/, '').replace(/\/+$/, '') || 'wang');
	return request({
		url: '/filegene/upload/wangImage',
		method: 'post',
		data: fd,
		timeout: 120000,
		onUploadProgress,
	});
}

/**
 * 从上传响应中取出 FileOutput（兼容 AjaxResult / Furion 包装）
 */
export function unwrapWangImageResult(res) {
	const layer = res?.data ?? res?.result ?? res;
	if (layer?.status === 'error') {
		throw new Error(layer.info || layer.message || '上传失败');
	}
	const fileOut = layer?.data ?? layer?.result ?? layer;
	if (!fileOut || typeof fileOut !== 'object') {
		throw new Error('上传失败：无返回数据');
	}
	return fileOut;
}

/**
 * 解析可插入编辑器的图片 URL
 */
export function resolveWangImageUrl(fileOut) {
	const url = `${fileOut?.Url ?? fileOut?.url ?? ''}`.trim();
	if (url) return url;
	const path = `${fileOut?.FilePath ?? fileOut?.filePath ?? ''}`.trim();
	const name = `${fileOut?.Name ?? fileOut?.name ?? ''}`.trim();
	const suffix = `${fileOut?.Suffix ?? fileOut?.suffix ?? ''}`.trim();
	const id = fileOut?.Id ?? fileOut?.id;
	const fileName = name || (id != null ? `${id}${suffix}` : '');
	const rel = [path, fileName].filter(Boolean).join('/');
	if (!rel) return '';
	const prefix = (import.meta.env.VITE_FILE_DOWNLOAD_PREFIX || '').trim().replace(/\/+$/, '');
	return prefix ? `${prefix}/${rel.replace(/^\/+/, '')}` : rel;
}
