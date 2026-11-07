import request from '@/utils/requestAPI6.js';

/** 按 BPO 页面编码获取 init 权限信息（v-auth 模式 3） */
export function fetchPageInitByBpoName(bpoName) {
  return request({
    url: '/bpo/common/pageinit',
    method: 'post',
    data: { bpoName }
  });
}
