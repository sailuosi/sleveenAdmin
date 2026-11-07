/**
 * 合并 ClientApp descriptors → sf / magic moddle（纯 JSON，无 jQuery）
 * 对照：原 sfd/ClientApp/app/index.js
 */
import sfBase from './descriptors/sf.json';
import magic from './descriptors/magic.json';
import identity from './descriptors/identity.json';
import transition from './descriptors/transition.json';
import gateway from './descriptors/gateway.json';
import multisign from './descriptors/multisign.json';
import action from './descriptors/action.json';
import section from './descriptors/section.json';
import boundary from './descriptors/boundary.json';
import servicetask from './descriptors/servicetask.json';
import aiservicetask from './descriptors/aiservicetask.json';
import scripttask from './descriptors/scripttask.json';
import ruletask from './descriptors/ruletask.json';
import performers from './descriptors/performers.json';
import assignee from './descriptors/assignee.json';
import buttons from './descriptors/buttons.json';
import nodePolicy from './descriptors/nodePolicy.json';
import forms from './descriptors/forms.json';
import notification from './descriptors/notification.json';
import variable from './descriptors/variable.json';
import subinfoes from './descriptors/subinfoes.json';

type ModdlePackage = {
  name: string;
  prefix: string;
  uri: string;
  xml?: { tagAlias?: string };
  types: unknown[];
  associations?: unknown[];
};

export function buildSfModdle(): ModdlePackage {
  const sf: ModdlePackage = {
    ...sfBase,
    types: []
  };

  sf.types.push(identity.identity);
  sf.types.push(transition.transition);
  sf.types.push(gateway.gateway);
  sf.types.push(multisign.multisign);
  sf.types = sf.types.concat(action.action);
  sf.types = sf.types.concat(section.section);
  sf.types = sf.types.concat(boundary.boundary);
  sf.types = sf.types.concat(servicetask.service);
  sf.types = sf.types.concat(aiservicetask.aiService);
  sf.types = sf.types.concat(scripttask.script);
  sf.types = sf.types.concat(ruletask.ruletask);
  sf.types = sf.types.concat(performers.performers);
  sf.types = sf.types.concat(assignee.assignee);
  sf.types = sf.types.concat(buttons.buttons);
  sf.types = sf.types.concat(nodePolicy.nodePolicy);
  sf.types = sf.types.concat(forms.forms);
  sf.types = sf.types.concat(notification.notifications);
  sf.types = sf.types.concat(variable.variables);
  sf.types = sf.types.concat(subinfoes.subinfoes);

  return sf;
}

export function buildMagicModdle(): ModdlePackage {
  return magic as ModdlePackage;
}
