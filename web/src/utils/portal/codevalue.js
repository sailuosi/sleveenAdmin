import request from '@/utils/portal/requestAPI6.js';

const codes = {};

export function loadCode(code) {
  return new Promise((resolve, reject) => {
    if (codes[code]) {
      resolve(codes[code]);
      return;
    }
    request({ url: '/api/sysDict/data/byCode', method: 'get', params: { code } })
      .then(res => {
        const list = Array.isArray(res?.records) ? res.records : Array.isArray(res) ? res : [];
        const mapped = list.map(item => ({
          value: item.value ?? item.sddValue ?? item.Sdd_Value,
          label: item.label ?? item.sddLabel ?? item.Sdd_Label,
          text: item.label ?? item.sddLabel ?? item.Sdd_Label
        }));
        codes[code] = mapped;
        resolve(mapped);
      })
      .catch(reject);
  });
}
