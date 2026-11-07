/** 设计器当前流程元数据（供 Provider / 变量 API 读取，替代 ClientApp kmain） */
export type DesignerProcessMeta = {
  processId: string;
  version: string;
};

let meta: DesignerProcessMeta = { processId: '', version: '1' };

export function setDesignerProcessMeta(next: Partial<DesignerProcessMeta>) {
  meta = {
    processId: next.processId ?? meta.processId,
    version: next.version ?? meta.version
  };
}

export function getDesignerProcessMeta(): DesignerProcessMeta {
  return { ...meta };
}
