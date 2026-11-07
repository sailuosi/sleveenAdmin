/** 列表/卡片视图 face 本地缓存键前缀 */
const FACE_CACHE_SUFFIX = ":face";

function buildFaceCacheKey(cacheKey) {
    if (!cacheKey) return "";
    return `${String(cacheKey)}${FACE_CACHE_SUFFIX}`;
}

/**
 * 读取缓存的 face 值
 * @param {string} cacheKey 业务缓存键（如 VCName）
 * @returns {string|null}
 */
export function getFaceCache(cacheKey) {
    const key = buildFaceCacheKey(cacheKey);
    if (!key) return null;
    try {
        return localStorage.getItem(key);
    } catch {
        return null;
    }
}

/**
 * 写入 face 到浏览器缓存
 * @param {string} cacheKey 业务缓存键（如 VCName）
 * @param {string} face card | table
 */
export function setFaceCache(cacheKey, face) {
    const key = buildFaceCacheKey(cacheKey);
    if (!key) return;
    try {
        if (face == null || face === "") {
            localStorage.removeItem(key);
        } else {
            localStorage.setItem(key, String(face));
        }
    } catch {
        /* ignore quota / private mode */
    }
}
