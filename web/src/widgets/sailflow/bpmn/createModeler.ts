import BpmnModeler from 'bpmn-js/lib/Modeler';
import {
  BpmnPropertiesPanelModule,
  BpmnPropertiesProviderModule
} from 'bpmn-js-properties-panel';
import tokenSimulation from 'bpmn-js-token-simulation';

import { buildMagicModdle, buildSfModdle } from './buildSfModdle';
import { SfActionPropertiesProviderModule } from './provider/sfActionProvider';
import { SfAiServiceTaskPropertiesProviderModule } from './provider/sfAiServiceTaskProvider';
import { SfAssigneePropertiesProviderModule } from './provider/sfAssigneeProvider';
import { SfButtonsPropertiesProviderModule } from './provider/sfButtonsProvider';
import { SfBoundaryPropertiesProviderModule } from './provider/sfBoundaryProvider';
import { installNodeConfigHeaderGear } from './ui/installNodeConfigHeaderGear';
import { SfFormsPropertiesProviderModule } from './provider/sfFormsProvider';
import { SfGatewayPropertiesProviderModule } from './provider/sfGatewayProvider';
import { SfIdentityPropertiesProviderModule } from './provider/sfIdentityProvider';
import { SfMultiSignPropertiesProviderModule } from './provider/sfMultiSignProvider';
import { SfNotificationsPropertiesProviderModule } from './provider/sfNotificationsProvider';
import { SfPerformersPropertiesProviderModule } from './provider/sfPerformersProvider';
import { SfRuleTaskPropertiesProviderModule } from './provider/sfRuleTaskProvider';
import { SfScriptTaskPropertiesProviderModule } from './provider/sfScriptTaskProvider';
import { SfServiceTaskPropertiesProviderModule } from './provider/sfServiceTaskProvider';
import { SfSubInfoesPropertiesProviderModule } from './provider/sfSubInfoesProvider';
import { SfTransitionPropertiesProviderModule } from './provider/sfTransitionProvider';
import { SfTriggerPropertiesProviderModule } from './provider/sfTriggerProvider';
import { SfMessageSignalPropertiesProviderModule } from './provider/sfMessageSignalProvider';
import { SfVariablePropertiesProviderModule } from './provider/sfVariableProvider';
import { initWfVariableCacheLoader } from './variable/wfVariableCache';
import { ZhTranslateModule } from './i18n/zhTranslate';
import { installAssigneeDesignDefaults } from './assigneeDefaults';
import { installEventNameDefaults } from './eventNameDefaults';

import 'bpmn-js/dist/assets/diagram-js.css';
import 'bpmn-js/dist/assets/bpmn-js.css';
import 'bpmn-js/dist/assets/bpmn-font/css/bpmn-embedded.css';
import '@bpmn-io/properties-panel/assets/properties-panel.css';
import 'bpmn-js-token-simulation/assets/css/bpmn-js-token-simulation.css';
import './ui/buttonsSetting.css';

export type SailBpmnModeler = InstanceType<typeof BpmnModeler>;

export type CreateModelerOptions = {
  container: HTMLElement;
  /** 属性面板挂载点；不传则仅画布 */
  propertiesPanel?: HTMLElement;
  /** 启用 bpmn-js-token-simulation（默认 true） */
  simulation?: boolean;
};

/**
 * T3-1c/T3-2：Modeler + sf/magic + 属性面板 Provider（无 jQuery）
 */
export function createModeler(options: CreateModelerOptions | HTMLElement): SailBpmnModeler {
  const opts: CreateModelerOptions =
    options instanceof HTMLElement ? { container: options } : options;

  const additionalModules: object[] = [ZhTranslateModule];
  const config: Record<string, unknown> = {
    container: opts.container,
    // diagram-js 新版已隐式绑定键盘，勿再传 keyboard.bindTo（会报 unsupported configuration）
    moddleExtensions: {
      sf: buildSfModdle(),
      magic: buildMagicModdle()
    }
  };

  if (opts.simulation !== false) {
    additionalModules.push(tokenSimulation);
  }

  if (opts.propertiesPanel) {
    config.propertiesPanel = { parent: opts.propertiesPanel };
    additionalModules.push(
      BpmnPropertiesPanelModule,
      BpmnPropertiesProviderModule,
      SfIdentityPropertiesProviderModule,
      SfGatewayPropertiesProviderModule,
      SfTransitionPropertiesProviderModule,
      SfMultiSignPropertiesProviderModule,
      SfAssigneePropertiesProviderModule,
      SfButtonsPropertiesProviderModule,
      SfPerformersPropertiesProviderModule,
      SfNotificationsPropertiesProviderModule,
      SfFormsPropertiesProviderModule,
      SfVariablePropertiesProviderModule,
      SfScriptTaskPropertiesProviderModule,
      SfServiceTaskPropertiesProviderModule,
      SfAiServiceTaskPropertiesProviderModule,
      SfActionPropertiesProviderModule,
      SfBoundaryPropertiesProviderModule,
      SfRuleTaskPropertiesProviderModule,
      SfTriggerPropertiesProviderModule,
      SfMessageSignalPropertiesProviderModule,
      SfSubInfoesPropertiesProviderModule
    );
  }

  if (additionalModules.length) {
    config.additionalModules = additionalModules;
  }

  const modeler = new BpmnModeler(config);
  installEventNameDefaults(modeler);
  if (opts.propertiesPanel) {
    const eventBus = modeler.get('eventBus') as { on: Function; fire: Function };
    initWfVariableCacheLoader(eventBus);
    installAssigneeDesignDefaults(modeler);
    installNodeConfigHeaderGear(modeler);
  }
  return modeler;
}

/** 本地兜底空图（InitNewBPMNFile 不可用时） */
export const EMPTY_BPMN_XML = `<?xml version="1.0" encoding="UTF-8"?>
<bpmn:definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
  xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL"
  xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI"
  xmlns:dc="http://www.omg.org/spec/DD/20100524/DC"
  xmlns:sf="http://www.saiflow.com/schema/sf"
  id="Definitions_SailEmpty"
  targetNamespace="http://bpmn.io/schema/bpmn">
  <bpmn:process id="Process_1" name="Process_1" isExecutable="true" sf:code="Process_1" sf:version="1">
    <bpmn:startEvent id="StartEvent_1" name="开始" />
  </bpmn:process>
  <bpmndi:BPMNDiagram id="BPMNDiagram_1">
    <bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="Process_1">
      <bpmndi:BPMNShape id="StartEvent_1_di" bpmnElement="StartEvent_1">
        <dc:Bounds x="180" y="160" width="36" height="36" />
      </bpmndi:BPMNShape>
    </bpmndi:BPMNPlane>
  </bpmndi:BPMNDiagram>
</bpmn:definitions>`;
