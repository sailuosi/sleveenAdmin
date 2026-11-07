import { getExtension, getBo } from './sfExtension';

export function isSplitSource(gateway: { incoming?: unknown[]; outgoing?: unknown[] }) {
  return (gateway.incoming?.length ?? 0) <= (gateway.outgoing?.length ?? 0);
}

export function isJoinTarget(gateway: { incoming?: unknown[]; outgoing?: unknown[] }) {
  return (gateway.incoming?.length ?? 0) >= (gateway.outgoing?.length ?? 0);
}

export function isGatewayType(type: string) {
  return (
    type === 'bpmn:InclusiveGateway' ||
    type === 'bpmn:ExclusiveGateway' ||
    type === 'bpmn:ParallelGateway'
  );
}

function gatewayDetail(element: unknown) {
  return getExtension(getBo(element), 'sf:GatewayDetail');
}

export function isOrGateway(el: { type?: string }) {
  return el.type === 'bpmn:InclusiveGateway';
}
export function isXOrGateway(el: { type?: string }) {
  return el.type === 'bpmn:ExclusiveGateway';
}
export function isOrSplit(el: { type?: string; incoming?: unknown[]; outgoing?: unknown[] }) {
  return isOrGateway(el) && isSplitSource(el);
}
export function isXOrSplit(el: { type?: string; incoming?: unknown[]; outgoing?: unknown[] }) {
  return isXOrGateway(el) && isSplitSource(el);
}
export function isApprovalOrSplit(el: unknown) {
  return gatewayDetail(el)?.extraSplitType === 'ApprovalOrSplit';
}
export function isEOrJoin(el: unknown) {
  return gatewayDetail(el)?.extraJoinType === 'EOrJoin';
}
