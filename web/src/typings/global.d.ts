export {};

interface AppMessageApi {
  (content: string | { type?: string; message?: unknown; duration?: number; showClose?: boolean }): unknown;
  success: (content: string, options?: { onLeave?: () => void; duration?: number; showClose?: boolean }) => unknown;
  error: (content: string, options?: { onLeave?: () => void; duration?: number; showClose?: boolean }) => unknown;
  info: (content: string, options?: { onLeave?: () => void; duration?: number; showClose?: boolean }) => unknown;
  warning: (content: string, options?: { onLeave?: () => void; duration?: number; showClose?: boolean }) => unknown;
}

interface AppDialogOptions {
  title?: string;
  content?: string;
  positiveText?: string;
  negativeText?: string;
  maskClosable?: boolean;
  closeOnEsc?: boolean;
  onPositiveClick?: () => void | Promise<void>;
  onClose?: () => void;
}

interface AppDialogApi {
  info: (options: AppDialogOptions) => void;
  error: (options: AppDialogOptions) => void;
  success: (options: AppDialogOptions) => void;
  warning: (options: AppDialogOptions) => void;
}

interface AppNotificationOptions {
  title?: string;
  content?: string | (() => unknown);
  duration?: number;
  action?: () => unknown;
  onClose?: () => void;
}

interface AppNotificationApi {
  success: (options: AppNotificationOptions) => unknown;
  info: (options: AppNotificationOptions) => unknown;
  warning: (options: AppNotificationOptions) => unknown;
  error: (options: AppNotificationOptions) => unknown;
  create: (options: AppNotificationOptions) => { close: () => void; destroy: () => void };
}

interface AppLoadingBarApi {
  start: () => void;
  finish: () => void;
  error: () => void;
}

declare global {
  export interface Window {
    /** NProgress instance */
    NProgress?: import('nprogress').NProgress;
    /** Loading bar (mapped to NProgress) */
    $loadingBar?: AppLoadingBarApi;
    /** Dialog (mapped to ElMessageBox) */
    $dialog?: AppDialogApi;
    /** Message (mapped to ElMessage) */
    $message?: AppMessageApi;
    /** Notification (mapped to ElNotification) */
    $notification?: AppNotificationApi;
  }

  /** Build time of the project */
  export const BUILD_TIME: string;
}
