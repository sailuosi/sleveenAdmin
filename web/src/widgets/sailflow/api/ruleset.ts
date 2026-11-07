/**
 * 规则集 · api/RuleSet（经本机 Web.Api · requestAPI6）
 */
import request from '@/utils/requestAPI6.js';

export type RuleSetRow = {
  Id?: number;
  RuleSetCode?: string;
  RuleSetName?: string;
  Description?: string;
  RuleContent?: string;
  Mode?: string;
  IsEnabled?: number;
  CreatedDateTime?: string;
  UpdatedDateTime?: string;
};

export type RuleExecutionRequest = {
  RuleSetCode: string;
  Variables?: Record<string, unknown>;
};

export const RULE_MODE_RULE_TYPES = 'ruleTypes';
export const RULE_MODE_BINDINGS_JSON = 'bindingsJson';

export const RULE_CONTENT_SAMPLES: Record<string, string> = {
  [RULE_MODE_RULE_TYPES]: `{
  "ruleTypes": [
    "saiflow.hub.BusinessRule.Approval.LeaveApproveRule, sailflow.bizRule"
  ]
}
`,
  [RULE_MODE_BINDINGS_JSON]: `{
  "rules": [
    {
      "when": "Amount >= 1000",
      "set": { "ApprovalLevel": "2", "Route": "Manager" }
    },
    {
      "when": "Amount < 1000",
      "set": { "ApprovalLevel": "1", "Route": "Self" }
    }
  ]
}
`
};

export function getRuleSetList() {
  return request({ url: '/api/RuleSet/GetList', method: 'get' });
}

export function getRuleSet(ruleSetCode: string) {
  return request({
    url: '/api/RuleSet/Get',
    method: 'get',
    params: { ruleSetCode }
  });
}

export function saveRuleSet(entity: RuleSetRow) {
  return request({
    url: '/api/RuleSet/Save',
    method: 'post',
    data: entity
  });
}

export function deleteRuleSet(ruleSetCode: string) {
  return request({
    url: '/api/RuleSet/Delete',
    method: 'delete',
    params: { ruleSetCode }
  });
}

export function executeRuleSet(body: RuleExecutionRequest) {
  return request({
    url: '/api/RuleSet/Execute',
    method: 'post',
    data: body
  });
}
