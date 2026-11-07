import { designerEvents } from './designerEvents';
import { detectElementKind } from '../elementConfig/loadSave';

type ModelerLike = {
  get: (name: string) => unknown;
};

const BTN_CLASS = 'sf-node-config-gear';

const GEAR_SVG =
  '<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 1 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06A1.65 1.65 0 0 0 4.68 15a1.65 1.65 0 0 0-1.51-1H3a2 2 0 1 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06A1.65 1.65 0 0 0 9 4.68a1.65 1.65 0 0 0 1-1.51V3a2 2 0 1 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06A1.65 1.65 0 0 0 19.4 9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 1 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z"/></svg>';

const TITLES: Record<string, string> = {
  node: '节点配置',
  transition: '连接线配置',
  gateway: '网关配置',
  service: '服务任务配置',
  script: '脚本任务配置',
  rule: '规则任务配置',
  subprocess: '子流程配置',
  event: '事件配置'
};

const EVENTS: Record<string, keyof typeof TITLES extends never ? string : any> = {
  node: 'nodeconfig:open',
  transition: 'transitionconfig:open',
  gateway: 'gatewayconfig:open',
  service: 'servicetaskconfig:open',
  script: 'scripttaskconfig:open',
  rule: 'ruletaskconfig:open',
  subprocess: 'subprocessconfig:open',
  event: 'eventconfig:open'
};

function ensureGear(actionsEl: HTMLElement, title: string, onClick: () => void) {
  let btn = actionsEl.querySelector<HTMLButtonElement>(`.${BTN_CLASS}`);
  if (!btn) {
    btn = document.createElement('button');
    btn.type = 'button';
    btn.className = BTN_CLASS;
    btn.innerHTML = GEAR_SVG;
    actionsEl.insertBefore(btn, actionsEl.firstChild);
  }
  btn.title = title;
  btn.setAttribute('aria-label', title);
  btn.onclick = e => {
    e.preventDefault();
    e.stopPropagation();
    onClick();
  };
  return btn;
}

export function installNodeConfigHeaderGear(modeler: ModelerLike) {
  const eventBus = modeler.get('eventBus') as {
    on: (e: string, fn: (...args: any[]) => void) => void;
  };
  const selection = modeler.get('selection') as { get: () => unknown[] };
  const modeling = modeler.get('modeling') as {
    updateProperties: (el: unknown, props: object) => void;
    updateModdleProperties?: (el: unknown, m: unknown, props: object) => void;
  };
  const moddle = modeler.get('moddle') as { create: (type: string, props?: object) => any };

  let observer: MutationObserver | null = null;
  let observedHeader: HTMLElement | null = null;
  let syncScheduled = false;
  let injecting = false;

  const openByKind = (kind: string) => {
    const el = selection.get()?.[0];
    if (detectElementKind(el) !== kind) return;
    const ev = EVENTS[kind];
    if (!ev) return;
    designerEvents.emit(ev as any, { element: el, modeling, moddle });
  };

  const attachObserver = (header: HTMLElement) => {
    if (observedHeader === header && observer) return;
    observer?.disconnect();
    observedHeader = header;
    observer = new MutationObserver(() => {
      if (injecting) return;
      scheduleSync();
    });
    observer.observe(header, { childList: true, subtree: true });
  };

  const sync = () => {
    syncScheduled = false;
    const panel = document.querySelector('.bio-properties-panel');
    if (!panel) return;
    const header = panel.querySelector('.bio-properties-panel-header') as HTMLElement | null;
    if (!header) return;
    attachObserver(header);

    let headerActions = header.querySelector(
      '.bio-properties-panel-header-actions'
    ) as HTMLElement | null;
    if (!headerActions) {
      injecting = true;
      headerActions = document.createElement('div');
      headerActions.className = 'bio-properties-panel-header-actions';
      header.appendChild(headerActions);
      injecting = false;
    }

    const selected = selection.get()?.[0];
    const kind = detectElementKind(selected);
    const existing = headerActions.querySelector<HTMLButtonElement>(`.${BTN_CLASS}`);

    if (!kind) {
      if (existing) {
        injecting = true;
        existing.remove();
        injecting = false;
      }
      return;
    }

    injecting = true;
    ensureGear(headerActions, TITLES[kind] || '配置', () => openByKind(kind));
    injecting = false;
  };

  const scheduleSync = () => {
    if (syncScheduled) return;
    syncScheduled = true;
    requestAnimationFrame(sync);
  };

  eventBus.on('selection.changed', scheduleSync);
  eventBus.on('propertiesPanel.updated', scheduleSync);
  eventBus.on('propertiesPanel.layoutChanged', scheduleSync);
  setTimeout(sync, 100);
}
