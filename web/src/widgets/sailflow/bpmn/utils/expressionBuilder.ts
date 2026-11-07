/** 条件表达式可视化构建 / 简易解析（对齐 ClientApp expression-editor 语义） */

export type ExprOperator =
  | '=='
  | '!='
  | '>'
  | '>='
  | '<'
  | '<='
  | 'contains'
  | 'startsWith'
  | 'endsWith';

export type ExprItem = {
  field: string;
  operator: ExprOperator;
  value: string;
  logic: '' | 'AND' | 'OR';
};

export type ExprGroup = {
  id: string;
  operator: 'AND' | 'OR';
  items: ExprItem[];
};

export function newGroupId() {
  return `g-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
}

function formatValue(value: string) {
  if (!Number.isNaN(Number(value)) && value.trim() !== '') return value.trim();
  return `"${value.replace(/"/g, '\\"')}"`;
}

function itemToExpr(item: ExprItem): string {
  const field = item.field.trim();
  const value = item.value.trim();
  if (!field || value === '') return '';
  switch (item.operator) {
    case 'contains':
      return `${field}.contains(${formatValue(value)})`;
    case 'startsWith':
      return `${field}.startsWith(${formatValue(value)})`;
    case 'endsWith':
      return `${field}.endsWith(${formatValue(value)})`;
    default:
      return `${field} ${item.operator} ${formatValue(value)}`;
  }
}

export function generateExpression(groups: ExprGroup[]): string {
  const groupExprs = groups
    .map(g => {
      const parts: string[] = [];
      g.items.forEach((item, index) => {
        const expr = itemToExpr(item);
        if (!expr) return;
        if (index > 0 && item.logic) parts.push(` ${item.logic} `);
        parts.push(expr);
      });
      return parts.join('');
    })
    .filter(Boolean);

  if (!groupExprs.length) return '';

  let out = '';
  groupExprs.forEach((gExpr, index) => {
    if (index > 0 && groups[index]?.operator) out += ` ${groups[index].operator} `;
    out += `(${gExpr})`;
  });
  return out.trim();
}

/** 尽力解析；失败则返回空组，由 UI 保留原文 */
export function parseExpression(expression: string): ExprGroup[] {
  const text = (expression || '').trim();
  if (!text) return [];

  const group: ExprGroup = {
    id: newGroupId(),
    operator: 'AND',
    items: []
  };

  const methodPattern = /(\w+)\.(contains|startsWith|endsWith)\s*\(\s*([^)]+)\s*\)/g;
  let match: RegExpExecArray | null;
  const matched = new Set<number>();

  while ((match = methodPattern.exec(text)) !== null) {
    matched.add(match.index);
    group.items.push({
      field: match[1].trim(),
      operator: match[2].trim() as ExprOperator,
      value: match[3].trim().replace(/^["']|["']$/g, ''),
      logic: group.items.length > 0 ? 'AND' : ''
    });
  }

  const comparisonPattern =
    /(\w+)\s*(>=|<=|==|!=|>|<)\s*([^\s&|()]+(?:\s+[^\s&|()]+)*?)(?=\s*(?:AND|OR|$|\)|&|\|))/g;
  while ((match = comparisonPattern.exec(text)) !== null) {
    if (matched.has(match.index)) continue;
    group.items.push({
      field: match[1].trim(),
      operator: match[2].trim() as ExprOperator,
      value: match[3].trim().replace(/^["']|["']$/g, ''),
      logic: group.items.length > 0 ? 'AND' : ''
    });
  }

  return group.items.length ? [group] : [];
}
