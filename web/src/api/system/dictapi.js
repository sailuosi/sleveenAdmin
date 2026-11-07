import request from '@/utils/portal/requestAPI6.js';

export function getCodeValue(code) {
  return request({ url: '/api/sysDict/data/byCode', method: 'get', params: { code } }).then(data => ({
    status: 'success',
    data: Array.isArray(data) ? data.map(item => ({
      value: item.value,
      label: item.label,
      text: item.label
    })) : []
  }));
}

export function getEumValue(code) {
  return getCodeValue(code);
}
