import request from '@/utils/requestAPI6.js';

const base = '/biz/saebhome';

export function loadEbHomeDashboard(para) {
  return request({
    url: `${base}/ebook/ebhome/dashboard`,
    method: 'post',
    data: para || { days: 7 }
  });
}
