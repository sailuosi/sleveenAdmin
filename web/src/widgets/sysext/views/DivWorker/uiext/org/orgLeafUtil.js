import { loadOrgTree } from '@s/api/system/mancommonapi.js';

/** 解析 HR_Organize.O_Leaf；未知时返回 null */
export function parseOrgLeafFlag(row) {
    const v = row?.O_Leaf ?? row?.o_Leaf ?? row?.O_LEAF;
    if (v === true || v === 1 || v === '1' || v === 'true') {
        return true;
    }
    if (v === false || v === 0 || v === '0' || v === 'false') {
        return false;
    }
    return null;
}

function buildProbePara(probePara, oid) {
    return {
        useAuth: probePara?.useAuth,
        authcode: probePara?.authcode,
        bpo: probePara?.bpo ?? probePara?.authcode,
        mineFk: probePara?.mineFk,
        oid,
    };
}

/**
 * 判断组织是否为可选末级节点（优先 O_Leaf，其次 el-tree node，最后懒加载探测子节点）
 */
export async function resolveOrgIsLeaf(row, node, probePara) {
    const flag = parseOrgLeafFlag(row);
    if (flag === true) {
        return true;
    }
    if (flag === false) {
        return false;
    }
    if (node) {
        if (node.isLeaf === true) {
            return true;
        }
        if (node.loaded && node.childNodes && node.childNodes.length > 0) {
            return false;
        }
    }
    const oid = row?.HR_OrganizeOID;
    if (!oid) {
        return false;
    }
    try {
        const res = await loadOrgTree(buildProbePara(probePara, oid));
        const rows = res?.data ?? [];
        return rows.length === 0;
    } catch {
        return false;
    }
}

export const ORG_LEAF_ONLY_TIP = '只能选择最底层部门，不能选择上级单位';
