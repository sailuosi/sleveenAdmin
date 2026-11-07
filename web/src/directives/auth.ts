import type { App, Directive, DirectiveBinding } from 'vue';
import { unref, watch } from 'vue';
import { useAuthStore } from '@/store/modules/auth';
import {
  checkAuth,
  checkAuthAsync,
  hasPageAuth,
  isPageInitPending,
  resolveAuthBinding,
  type AuthBinding
} from '@/utils/authPermission';

const PENDING_KEY = 'vAuthPending';

async function applyAuth(el: HTMLElement, value: AuthBinding, generation: number, getGeneration: () => number) {
  if (!el.isConnected) {
    return;
  }

  const resolved = resolveAuthBinding(value);

  if (resolved.mode === 'pageByBpo') {
    el.style.display = 'none';
    el.dataset[PENDING_KEY] = '1';
    const ok = await checkAuthAsync(value);
    if (getGeneration() !== generation || !el.isConnected) {
      return;
    }
    delete el.dataset[PENDING_KEY];
    if (ok) {
      el.style.display = '';
    } else {
      el.remove();
    }
    return;
  }

  if (resolved.mode === 'page') {
    if (isPageInitPending(resolved.page)) {
      el.style.display = 'none';
      el.dataset[PENDING_KEY] = '1';
      return;
    }
    delete el.dataset[PENDING_KEY];
    const ok = hasPageAuth(resolved.code, resolved.page);
    if (ok) {
      el.style.display = '';
    } else if (el.parentNode) {
      el.remove();
    }
    return;
  }

  const ok = checkAuth(value);
  if (!ok && el.parentNode) {
    el.remove();
  } else if (ok && el.dataset[PENDING_KEY] !== '1') {
    el.style.display = '';
  }
}

function watchPageSource(value: AuthBinding) {
  if (!Array.isArray(value)) {
    return null;
  }
  const second = unref(value[1]);
  if (typeof second === 'string' && second.startsWith('BPO_')) {
    return null;
  }
  return second;
}

function bindAuthDirective(el: HTMLElement, binding: DirectiveBinding<AuthBinding>) {
  let generation = 0;
  const getGeneration = () => generation;

  const run = () => {
    generation += 1;
    const current = generation;
    void applyAuth(el, binding.value, current, getGeneration);
  };

  run();

  const authStore = useAuthStore();
  const stopButtons = watch(
    () => authStore.userInfo.buttons,
    () => {
      if (el.isConnected) {
        run();
      }
    },
    { deep: true }
  );
  const stopPage = watch(
    () => watchPageSource(binding.value),
    () => {
      if (el.isConnected) {
        run();
      }
    },
    { deep: true }
  );

  return () => {
    stopButtons();
    stopPage();
  };
}

function setupAuthDirective(app: App) {
  const authDirective: Directive<HTMLElement, AuthBinding> = {
    mounted(el: HTMLElement, binding: DirectiveBinding<AuthBinding>) {
      (el as HTMLElement & { _vAuthStop?: () => void })._vAuthStop = bindAuthDirective(el, binding);
    },
    updated(el: HTMLElement, binding: DirectiveBinding<AuthBinding>) {
      (el as HTMLElement & { _vAuthStop?: () => void })._vAuthStop?.();
      (el as HTMLElement & { _vAuthStop?: () => void })._vAuthStop = bindAuthDirective(el, binding);
    },
    unmounted(el: HTMLElement) {
      (el as HTMLElement & { _vAuthStop?: () => void })._vAuthStop?.();
    }
  };

  app.directive('auth', authDirective);
}

export { setupAuthDirective };
