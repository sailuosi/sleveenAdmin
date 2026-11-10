import { ElMessageBox } from 'element-plus';
import {
  extractSubInfoBindingsFromXml,
  fetchProcessDefRows,
  findStaleSubBindings,
  formatStaleMessage
} from './subProcessVersionCheck';

/**
 * 保存前检测图中子流程绑定是否落后于最新定义行。
 * 不自动改关联；用户可「仍要保存」或取消。
 * @returns true 继续保存；false 用户取消
 */
export async function confirmStaleSubBindingsOnSave(xml: string): Promise<boolean> {
  try {
    const bindings = extractSubInfoBindingsFromXml(xml);
    if (!bindings.length) return true;
    const rows = await fetchProcessDefRows();
    const stale = findStaleSubBindings(bindings, rows);
    if (!stale.length) return true;
    await ElMessageBox.confirm(formatStaleMessage(stale), '子流程关联未跟最新版', {
      type: 'warning',
      confirmButtonText: '仍要保存',
      cancelButtonText: '取消',
      distinguishCancelAndClose: true
    });
    return true;
  } catch {
    return false;
  }
}
