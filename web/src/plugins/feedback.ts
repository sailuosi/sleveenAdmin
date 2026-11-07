import { h } from 'vue';
import { ElButton, ElMessage, ElMessageBox, ElNotification } from 'element-plus';
import type { MessageHandler, NotificationHandle } from 'element-plus';

type MessageType = 'success' | 'error' | 'info' | 'warning';

interface MessageOptions {
  onLeave?: () => void;
  duration?: number;
  showClose?: boolean;
}

interface DialogOptions {
  title?: string;
  content?: string;
  positiveText?: string;
  negativeText?: string;
  maskClosable?: boolean;
  closeOnEsc?: boolean;
  onPositiveClick?: () => void | Promise<void>;
  onClose?: () => void;
}

interface NotificationOptions {
  title?: string;
  content?: string | (() => unknown);
  duration?: number;
  action?: () => unknown;
  onClose?: () => void;
}

function createMessageApi() {
  function show(type: MessageType, content: string, options?: MessageOptions): MessageHandler {
    return ElMessage({
      type,
      message: content,
      duration: options?.duration,
      showClose: options?.showClose,
      onClose: options?.onLeave
    });
  }

  /** 兼容 Element Plus / 旧 Options API：可 $message('文案')、$message({ type, message })，并带 .success/.error 等 */
  function message(
    contentOrOptions: string | { type?: MessageType; message?: unknown; duration?: number; showClose?: boolean },
    options?: MessageOptions
  ): MessageHandler {
    if (typeof contentOrOptions === 'string') {
      return show('info', contentOrOptions, options);
    }
    if (contentOrOptions && typeof contentOrOptions === 'object') {
      const type = contentOrOptions.type || 'info';
      const text = contentOrOptions.message == null ? '' : String(contentOrOptions.message);
      return show(type, text, {
        duration: contentOrOptions.duration ?? options?.duration,
        showClose: contentOrOptions.showClose ?? options?.showClose,
        onLeave: options?.onLeave
      });
    }
    return show('info', String(contentOrOptions ?? ''), options);
  }

  message.success = (content: string, options?: MessageOptions) => show('success', content, options);
  message.error = (content: string, options?: MessageOptions) => show('error', content, options);
  message.info = (content: string, options?: MessageOptions) => show('info', content, options);
  message.warning = (content: string, options?: MessageOptions) => show('warning', content, options);

  return message;
}

function createDialogApi() {
  function open(type: MessageType, options: DialogOptions) {
    const {
      title = '',
      content = '',
      positiveText = 'OK',
      negativeText,
      maskClosable = true,
      closeOnEsc = true,
      onPositiveClick,
      onClose
    } = options;

    const common = {
      confirmButtonText: positiveText,
      cancelButtonText: negativeText,
      closeOnClickModal: maskClosable,
      closeOnPressEscape: closeOnEsc,
      type: type === 'error' ? ('error' as const) : type === 'warning' ? ('warning' as const) : type === 'success' ? ('success' as const) : ('info' as const)
    };

    const runPositive = async () => {
      await onPositiveClick?.();
    };

    if (negativeText) {
      ElMessageBox.confirm(content, title, common)
        .then(runPositive)
        .catch(() => {
          onClose?.();
        });
      return;
    }

    ElMessageBox.alert(content, title, {
      confirmButtonText: positiveText,
      closeOnClickModal: maskClosable,
      closeOnPressEscape: closeOnEsc,
      type: common.type,
      callback: action => {
        if (action === 'confirm') {
          void runPositive();
        } else {
          onClose?.();
        }
      }
    });
  }

  return {
    info: (options: DialogOptions) => open('info', options),
    error: (options: DialogOptions) => open('error', options),
    success: (options: DialogOptions) => open('success', options),
    warning: (options: DialogOptions) => open('warning', options)
  };
}

function createNotificationApi() {
  return {
    success(options: NotificationOptions) {
      return ElNotification.success({
        title: options.title,
        message: typeof options.content === 'string' ? options.content : undefined,
        duration: options.duration,
        onClose: options.onClose
      });
    },
    info(options: NotificationOptions) {
      return ElNotification.info({
        title: options.title,
        message: typeof options.content === 'string' ? options.content : undefined,
        duration: options.duration,
        onClose: options.onClose
      });
    },
    warning(options: NotificationOptions) {
      return ElNotification.warning({
        title: options.title,
        message: typeof options.content === 'string' ? options.content : undefined,
        duration: options.duration,
        onClose: options.onClose
      });
    },
    error(options: NotificationOptions) {
      return ElNotification.error({
        title: options.title,
        message: typeof options.content === 'string' ? options.content : undefined,
        duration: options.duration,
        onClose: options.onClose
      });
    },
    create(options: NotificationOptions): NotificationHandle & { destroy: () => void } {
      const message =
        typeof options.action === 'function'
          ? () =>
              h('div', { class: 'feedback-notification-body' }, [
                h('div', { class: 'mb-12px' }, typeof options.content === 'string' ? options.content : ''),
                options.action!()
              ])
          : typeof options.content === 'string'
            ? options.content
            : undefined;

      const handle = ElNotification({
        title: options.title,
        message,
        duration: options.duration ?? 0,
        onClose: options.onClose
      });

      return Object.assign(handle, {
        destroy: () => handle.close()
      });
    }
  };
}

function createLoadingBarApi() {
  return {
    start() {
      window.NProgress?.start();
    },
    finish() {
      window.NProgress?.done();
    },
    error() {
      window.NProgress?.done();
    }
  };
}

/** Install Naive-compatible window.$message / $dialog / $notification / $loadingBar on Element Plus. */
export function setupFeedback() {
  window.$message = createMessageApi();
  window.$dialog = createDialogApi();
  window.$notification = createNotificationApi();
  window.$loadingBar = createLoadingBarApi();
}

export function createUpdateNotificationAction(onCancel: () => void, cancelText: string, confirmText: string) {
  return () =>
    h('div', { style: { display: 'flex', justifyContent: 'end', gap: '12px', width: '325px' } }, [
      h(
        ElButton,
        {
          onClick: onCancel
        },
        () => cancelText
      ),
      h(
        ElButton,
        {
          type: 'primary',
          onClick: () => location.reload()
        },
        () => confirmText
      )
    ]);
}
