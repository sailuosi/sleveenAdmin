/** 从 BPMN XmlContent 提取本流程引用的规则集 / AI 配置（只读关联）。 */

export type ProcessAssetRefs = {
  ruleSetCodes: string[];
  aiConfigUuids: string[];
};

function uniq(list: string[]): string[] {
  return [...new Set(list.map(s => s.trim()).filter(Boolean))];
}

/** 兼容 attribute / element 文本里的 ruleSetCode、configUUID */
export function parseProcessAssets(xml: string | null | undefined): ProcessAssetRefs {
  if (!xml) return { ruleSetCodes: [], aiConfigUuids: [] };
  const ruleSetCodes: string[] = [];
  const aiConfigUuids: string[] = [];

  for (const m of xml.matchAll(/ruleSetCode\s*=\s*["']([^"']+)["']/gi)) {
    ruleSetCodes.push(m[1]);
  }
  for (const m of xml.matchAll(/<[^>]*ruleSetCode[^>]*>\s*([^<\s]+)\s*</gi)) {
    ruleSetCodes.push(m[1]);
  }
  for (const m of xml.matchAll(/configUUID\s*=\s*["']([^"']+)["']/gi)) {
    aiConfigUuids.push(m[1]);
  }
  for (const m of xml.matchAll(/<[^>]*configUUID[^>]*>\s*([^<\s]+)\s*</gi)) {
    aiConfigUuids.push(m[1]);
  }

  return {
    ruleSetCodes: uniq(ruleSetCodes),
    aiConfigUuids: uniq(aiConfigUuids)
  };
}
