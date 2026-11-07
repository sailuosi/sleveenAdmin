import type { App } from 'vue';
import { setupAuthDirective } from './auth';

export function setupDirectives(app: App) {
  setupAuthDirective(app);
}
