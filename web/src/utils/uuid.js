/**
 * 与 GUID.ts 中 Guid.create / Guid.raw 相同的生成算法（Math.random + 16 进制分段拼接）。
 * @returns {string} 形如 xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx 的 GUID 字符串
 */
function gen(count) {
  let out = '';
  for (let i = 0; i < count; i++) {
    out += (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
  }
  return out;
}

export function newguid() {
  return [gen(2), gen(1), gen(1), gen(1), gen(3)].join('-');
}
