const vcModules = import.meta.glob(['@/widgets/**/VC/**/VC_*.vue']);
const bizModules = import.meta.glob(['@/widgets/**/Biz_*.vue']);

function buildVcMap(src) {
  const keys = Object.keys(src);
  const body = {};
  const tool = {};
  keys.forEach(li => {
    const idx = li.lastIndexOf('/');
    const nameRaw = li.substring(idx + 1);
    const name = nameRaw.endsWith('.vue') ? nameRaw.substring(0, nameRaw.length - 4) : nameRaw;
    body[name] = src[li];
  });
  return { body, tool };
}

function buildBizMap(src) {
  const keys = Object.keys(src);
  const body = {};
  keys.forEach(li => {
    const idx = li.lastIndexOf('/');
    const nameRaw = li.substring(idx + 1);
    const name = nameRaw.endsWith('.vue') ? nameRaw.substring(0, nameRaw.length - 4) : nameRaw;
    body[name] = src[li];
  });
  return body;
}

const vcMap = buildVcMap(vcModules);
const vcBodyMap = vcMap.body;
const vcToolMap = vcMap.tool;
const bizMap = buildBizMap(bizModules);

export function loadVC(component) {
  if (vcBodyMap[component]) {
    return vcBodyMap[component];
  }
  return component;
}

export function loadVCTool(component) {
  if (vcToolMap[component]) {
    return vcToolMap[component];
  }
  return component;
}

export function loadBiz(component) {
  if (bizMap[component]) {
    return bizMap[component];
  }
  return component;
}
