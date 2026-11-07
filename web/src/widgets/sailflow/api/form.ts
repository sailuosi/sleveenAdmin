import request from '@/utils/requestAPI6.js';

export type SailFormPayload = {
  FormId?: string;
  FormName?: string;
  FormCode?: string;
  Version?: string;
  Conf?: string;
  FieldsJson?: string;
  Description?: string;
};

export function getFormList() {
  return request({ url: '/api/WfForm/GetList', method: 'get' });
}

export function getForm(id: string | number) {
  return request({ url: `/api/WfForm/Get/${id}`, method: 'get' });
}

export function saveForm(data: SailFormPayload) {
  return request({ url: '/api/WfForm/Save', method: 'post', data });
}

export function deleteForm(id: string | number) {
  return request({ url: `/api/WfForm/Delete/${id}`, method: 'post' });
}
