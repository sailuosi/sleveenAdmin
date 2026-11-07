import request from '@/utils/portal/requestAPI6.js';

export function getPortPageConfig(data) {
  return request({
    url: '/api/sysPortal/common/loadportalconfig',
    method: 'post',
    data
  });
}

export function loadConfigApplets(data) {
  return request({
    url: '/api/sysPortal/common/loadapplets',
    method: 'post',
    data
  });
}

export function getPortConfig(key) {
  return request({
    url: '/api/sysPortal/common/loadportalconfig',
    method: 'post',
    data: { value: key }
  });
}

export function saveApplets(data) {
  return request({
    url: '/api/sysPortal/common/saveapplets',
    method: 'post',
    data
  });
}
