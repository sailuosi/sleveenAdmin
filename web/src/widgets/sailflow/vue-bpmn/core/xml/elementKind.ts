import type { DiagramElement } from '../types.js';

export function isConnection(el: DiagramElement): boolean {
  const t = el.type;
  return t.includes('Flow') || t.includes('Association');
}

/** BPMN XML uses lower-camel local names: startEvent, subProcess, sequenceFlow. */
const TYPE_TO_TAG: Record<string, string> = {
  'bpmn:SequenceFlow': 'sequenceFlow',
  'bpmn:MessageFlow': 'messageFlow',
  'bpmn:Association': 'association',
  'bpmn:DataInputAssociation': 'dataInputAssociation',
  'bpmn:DataOutputAssociation': 'dataOutputAssociation',
  'bpmn:StartEvent': 'startEvent',
  'bpmn:EndEvent': 'endEvent',
  'bpmn:IntermediateCatchEvent': 'intermediateCatchEvent',
  'bpmn:IntermediateThrowEvent': 'intermediateThrowEvent',
  'bpmn:BoundaryEvent': 'boundaryEvent',
  'bpmn:ExclusiveGateway': 'exclusiveGateway',
  'bpmn:InclusiveGateway': 'inclusiveGateway',
  'bpmn:ParallelGateway': 'parallelGateway',
  'bpmn:EventBasedGateway': 'eventBasedGateway',
  'bpmn:ComplexGateway': 'complexGateway',
  'bpmn:UserTask': 'userTask',
  'bpmn:ServiceTask': 'serviceTask',
  'bpmn:ScriptTask': 'scriptTask',
  'bpmn:BusinessRuleTask': 'businessRuleTask',
  'bpmn:SendTask': 'sendTask',
  'bpmn:ReceiveTask': 'receiveTask',
  'bpmn:ManualTask': 'manualTask',
  'bpmn:CallActivity': 'callActivity',
  'bpmn:SubProcess': 'subProcess',
  'bpmn:AdHocSubProcess': 'adHocSubProcess',
  'bpmn:Transaction': 'transaction',
  'bpmn:TextAnnotation': 'textAnnotation',
  'bpmn:DataObjectReference': 'dataObjectReference',
  'bpmn:DataStoreReference': 'dataStoreReference',
  'bpmn:DataObject': 'dataObject',
  'bpmn:Participant': 'participant',
  'bpmn:Lane': 'lane',
  'bpmn:Process': 'process',
  'bpmn:Group': 'group',
  'bpmn:Task': 'task',
  'bpmn:Collaboration': 'collaboration',
};

export function localBpmnTag(type: string): string {
  if (TYPE_TO_TAG[type]) return TYPE_TO_TAG[type]!;
  const bare = type.startsWith('bpmn:') ? type.slice(5) : type;
  return bare.charAt(0).toLowerCase() + bare.slice(1);
}

export function isContainerActivity(el: DiagramElement): boolean {
  return (
    el.type === 'bpmn:SubProcess' ||
    el.type === 'bpmn:Transaction' ||
    el.type === 'bpmn:AdHocSubProcess'
  );
}
