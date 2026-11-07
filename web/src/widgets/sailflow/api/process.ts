/**
 * Sailflow 设计器 API（经本机 Web.Api · 与现网一致用 requestAPI6）
 * Unify 包装后拦截器返回 result = Sail ResponseResult（Status / Entity / Message）
 */
import request from '@/utils/requestAPI6.js';

/** GET /api/sail/Probe/ping */
export function probePing() {
  return request({
    url: '/api/sail/Probe/ping',
    method: 'get'
  });
}

export function initDb(seed = true) {
  return request({
    url: `/api/sail/Probe/init-db?seed=${seed ? 'true' : 'false'}`,
    method: 'get'
  });
}

/** GET api/Wf2Xml/GetProcessListSimple */
export function getProcessListSimple() {
  return request({
    url: '/api/Wf2Xml/GetProcessListSimple',
    method: 'get'
  });
}

/** POST api/Wf2Xml/QueryProcessFile · { ProcessId, Version } */
export function queryProcessFile(data: { ProcessId: string; Version?: string }) {
  return request({
    url: '/api/Wf2Xml/QueryProcessFile',
    method: 'post',
    data
  });
}

/** GET api/Wf2Xml/GetProcessFile/{id} */
export function getProcessFileById(id: number | string) {
  return request({
    url: `/api/Wf2Xml/GetProcessFile/${id}`,
    method: 'get'
  });
}

/** @deprecated 用 queryProcessFile */
export function getProcessFile(data: { ProcessId: string; Version?: string }) {
  return queryProcessFile(data);
}

/** POST api/Wf2Xml/SaveProcessFile */
export function saveProcessFile(data: Record<string, unknown>) {
  return request({
    url: '/api/Wf2Xml/SaveProcessFile',
    method: 'post',
    data
  });
}

/** GET api/Wf2Xml/InitNewBPMNFile */
export function initNewBpmnFile() {
  return request({
    url: '/api/Wf2Xml/InitNewBPMNFile',
    method: 'get'
  });
}

/** POST api/Wf2Xml/ValidateProcess · body 至少含 XmlContent */
export function validateProcess(data: Record<string, unknown>) {
  return request({
    url: '/api/Wf2Xml/ValidateProcess',
    method: 'post',
    data
  });
}

/** POST api/Wf2Xml/CreateProcessByTemplate · { TemplateName } */
export function createProcessByTemplate(templateName: string) {
  return request({
    url: '/api/Wf2Xml/CreateProcessByTemplate',
    method: 'post',
    data: { TemplateName: templateName }
  });
}

/** POST api/Wf2Xml/UpdateProcessUsingState · UsingState 1启用 0停用 */
export function updateProcessUsingState(data: {
  ProcessId: string;
  Version?: string;
  UsingState: number;
}) {
  return request({
    url: '/api/Wf2Xml/UpdateProcessUsingState',
    method: 'post',
    data
  });
}

/** POST api/Wf2Xml/UpgradeProcess */
export function upgradeProcess(data: {
  ProcessId: string;
  Version?: string;
  NewVersion?: string;
}) {
  return request({
    url: '/api/Wf2Xml/UpgradeProcess',
    method: 'post',
    data
  });
}

/** POST api/Wf2Xml/DeleteProcess · { ProcessId, Version } */
export function deleteProcess(data: { ProcessId: string; Version?: string }) {
  return request({
    url: '/api/Wf2Xml/DeleteProcess',
    method: 'post',
    data
  });
}

/** GET api/Wf2Xml/DeleteProcessById/{id} */
export function deleteProcessById(id: number | string) {
  return request({
    url: `/api/Wf2Xml/DeleteProcessById/${id}`,
    method: 'get'
  });
}
