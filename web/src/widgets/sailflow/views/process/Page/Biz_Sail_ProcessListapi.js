import {
  getProcessListSimple,
  deleteProcess,
  deleteProcessById,
  updateProcessUsingState,
  upgradeProcess
} from '@sail/api/process';
import { pickField } from '@sail/utils/sailResult';

export function loadPageInit() {
  return Promise.resolve({});
}

function normalizeList(data) {
  const status = data?.Status ?? data?.status;
  if (status === -1 || status === '-1') {
    const err = new Error(data?.Message || data?.message || '列表失败');
    err.sailData = data;
    throw err;
  }
  const list = data?.Entity ?? data?.entity ?? data?.EntityList ?? data?.entityList ?? data;
  const items = Array.isArray(list) ? list : [];
  return { Items: items, Total: items.length, records: items, total: items.length };
}

/** 全量拉取后按 pageNum/pageSize 前端切片 */
export function querySailProcesslist(para) {
  const pageNum = Number(para?.pageNum || para?.page || 1);
  const pageSize = Number(para?.pageSize || 30);
  const keyword = String(para?.keyword || '').trim().toLowerCase();

  return getProcessListSimple().then((data) => {
    const all = normalizeList(data);
    let items = all.Items;
    if (keyword) {
      items = items.filter((r) => JSON.stringify(r).toLowerCase().includes(keyword));
    }
    const total = items.length;
    const start = Math.max(0, (pageNum - 1) * pageSize);
    const pageItems = items.slice(start, start + pageSize);
    return { Items: pageItems, Total: total, records: pageItems, total };
  });
}

export function removeSailProcess(row) {
  const id = pickField(row, 'Id', 'id');
  if (id != null && id !== '') {
    return deleteProcessById(String(id));
  }
  return deleteProcess({
    ProcessId: String(pickField(row, 'ProcessId', 'processId') || ''),
    Version: String(pickField(row, 'Version', 'version') || '1')
  });
}

export function updateSailProcessUsingState(data) {
  return updateProcessUsingState(data);
}

export function upgradeSailProcess(data) {
  return upgradeProcess(data);
}
