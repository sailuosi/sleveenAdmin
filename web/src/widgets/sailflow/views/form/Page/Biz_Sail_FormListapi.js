import { getFormList, deleteForm } from '@sail/api/form';
import { unpackSailResult, pickField } from '@sail/utils/sailResult';

export function loadPageInit() {
  return Promise.resolve({});
}

/** 全量拉取后按 keyword + pageNum/pageSize 前端切片 */
export function querySailFormlist(para) {
  const pageNum = Number(para?.pageNum || para?.page || 1);
  const pageSize = Number(para?.pageSize || 30);
  const keyword = String(para?.keyword || '').trim().toLowerCase();

  return getFormList().then((data) => {
    if ((data?.Status ?? data?.status) === -1) {
      const r = unpackSailResult(data);
      throw new Error(r.message || '加载失败');
    }
    const r = unpackSailResult(data);
    let items = Array.isArray(r.entity) ? r.entity : [];
    if (keyword) {
      items = items.filter((row) => {
        const name = String(pickField(row, 'FormName', 'formName') || '').toLowerCase();
        const code = String(pickField(row, 'FormCode', 'formCode') || '').toLowerCase();
        return name.includes(keyword) || code.includes(keyword) || JSON.stringify(row).toLowerCase().includes(keyword);
      });
    }
    const total = items.length;
    const start = Math.max(0, (pageNum - 1) * pageSize);
    const pageItems = items.slice(start, start + pageSize);
    return { Items: pageItems, Total: total, records: pageItems, total };
  });
}

export function removeSailForm(rowOrId) {
  const id = typeof rowOrId === 'object'
    ? pickField(rowOrId, 'FormId', 'formId')
    : rowOrId;
  return deleteForm(String(id));
}
