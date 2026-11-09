/** Minimal import/export contract — full BPMN 2.0 mapping comes in D01 implementation. */
export interface ImportResult {
  warnings: string[];
  rootId: string;
  definitionsId: string;
  /** Extra definitions-level attributes (xmlns:sf, targetNamespace, …) */
  definitionsAttrs?: Record<string, string>;
}

export interface ExportResult {
  xml: string;
}
