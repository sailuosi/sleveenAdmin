/** Sail 办理台只读列表：biz/sailworkbench/{todo|done|process|copyto}/qlist */
import request from '@/utils/requestAPI6.js';

const base = '/biz/sailworkbench';

export type SailWorkbenchQuery = {
  pageNum?: number;
  page?: number;
  pageSize?: number;
  keyword?: string | null;
  /** true=当前用户；false=管理端全量 */
  mine?: boolean;
  processState?: number | null;
};

function qlist(path: string, para?: SailWorkbenchQuery) {
  return request({ url: `${base}/${path}/qlist`, method: 'post', data: para || {} });
}

/** 待办 Ready */
export function querySailTodoList(para?: SailWorkbenchQuery) {
  return qlist('todo', para);
}

/** 我的已办 */
export function querySailDoneList(para?: SailWorkbenchQuery) {
  return qlist('done', para);
}

/** 流程实例 */
export function querySailProcessList(para?: SailWorkbenchQuery) {
  return qlist('process', para);
}

/** 抄送我的 */
export function querySailCopytoList(para?: SailWorkbenchQuery) {
  return qlist('copyto', para);
}
