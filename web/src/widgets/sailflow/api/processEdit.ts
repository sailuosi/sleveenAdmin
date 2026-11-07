import request from '@/utils/requestAPI6.js';

export type ProcessEditSavePayload = {
  ProcessId?: string;
  Version?: string;
  ProcessName: string;
  ProcessCode: string;
  Description?: string;
  AppType?: string;
  Icon?: string;
  Status?: number;
  FormType?: number;
  FormId?: string;
  FormCustomCreatePath?: string;
  FormCustomViewPath?: string;
  Visible?: number;
  StartUserType?: number;
  StartUserIds?: string;
  StartDeptIds?: string;
  ManagerUserIds?: string;
  ExtraJson?: string;
};

export function getProcessEdit(data: { ProcessId: string; Version?: string }) {
  return request({
    url: '/api/WfProcessEdit/Get',
    method: 'post',
    data
  });
}

export function saveProcessEdit(data: ProcessEditSavePayload) {
  return request({
    url: '/api/WfProcessEdit/Save',
    method: 'post',
    data
  });
}
