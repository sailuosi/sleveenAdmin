import type { LocationQueryRaw } from 'vue-router';
import { router } from '@/router';
import { myflow, insertReadTime } from '@f/client/api/basis/toDoList';
import { praseStrEmpty } from '@f/common/utils/stringutil.js';
import { parseTime } from '@f/common/utils/date.js';

export type HomeTaskKey = 'todo' | 'running' | 'finished' | 'toRead' | 'readed';

function openWindow(path: string, query: LocationQueryRaw, title: string) {
  const routeData = router.resolve({ path, query });
  window.open(routeData.href, title);
}

export function openHomePortalTask(work: Record<string, any>, taskKey: HomeTaskKey) {
  if (taskKey === 'todo') {
    openTodoFlow(work);
    return;
  }
  if (taskKey === 'toRead' || taskKey === 'readed') {
    openCcFlow(work, taskKey);
    return;
  }
  openViewFlow(work, taskKey === 'running' ? 'ccRunning' : 'ccComplete');
}

function openTodoFlow(work: Record<string, any>) {
  const params: Record<string, any> = {
    IsRead: 1,
    FK_Flow: work.FK_Flow,
    FK_Node: work.FK_Node,
    FID: work.FID,
    WorkID: work.WorkID,
    PWorkID: work.PWorkID,
    Paras: work.AtPara ? work.AtPara : '',
    ReadTime: work.ReadTime,
    FromPage: 'Todolist',
  };
  params.Paras = `1${params.Paras}`;
  if (praseStrEmpty(work.ReadTime).length === 0) {
    params.ReadTime = parseTime(new Date());
  }
  myflow(JSON.stringify(params)).then((res: { code: number }) => {
    if (res.code === 200) {
      insertReadTime(JSON.stringify(params)).then(() => {
        const routeData = router.resolve({
          path: '/ccflow/todothing',
          query: params,
        });
        window.open(routeData.href, '待办办理');
      });
    }
  });
}

function openViewFlow(work: Record<string, any>, fromPage: string) {
  const params = {
    WorkID: work.WorkID,
    FK_Flow: work.FK_Flow,
    FK_Node: work.FK_Node,
    FID: work.FID,
    FromPage: fromPage,
  };
  openWindow('/cchomepan/MyView', params, `待办查看_${Date.now()}`);
}

function openCcFlow(work: Record<string, any>, taskKey: 'toRead' | 'readed') {
  const params = {
    WorkID: work.WorkID,
    FK_Flow: work.FK_Flow,
    FK_Node: work.FK_Node,
    FID: work.FID,
    CCSta: 1,
    id: work.MyPK,
    FromPage: taskKey === 'toRead' ? 'toread' : 'readed',
  };
  openWindow('/cchomewk/MyCC', params, '_blank');
}
