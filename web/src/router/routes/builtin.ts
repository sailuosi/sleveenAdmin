import type { CustomRoute } from '@elegant-router/types';

export const ROOT_ROUTE: CustomRoute = {
  name: 'root',
  path: '/',
  redirect: '/home',
  meta: {
    title: 'root',
    constant: true
  }
};

const NOT_FOUND_ROUTE: CustomRoute = {
  name: 'not-found',
  path: '/:pathMatch(.*)*',
  component: 'layout.blank$view.404',
  meta: {
    title: 'not-found',
    constant: true
  }
};

/** builtin routes, it must be constant and setup in vue-router */
//静态内置路由：403、404、500、login、ssojump、home
const builtinRoutes: CustomRoute[] = [
  ROOT_ROUTE,
  NOT_FOUND_ROUTE,
  {
    name: 'baisse',
    path: '/home',
    component: () => import('@/layouts/base-layout/index.vue'),
    meta: {
      title: 'base',
      // 需登录（账号密码或 SSO 拿到 token）后才能访问；与 guard/route.ts 中 !to.meta.constant 一致
      constant: false
    },
    children: [
      {
        name: 'minehome',
        path: '',
        component: () => import('@/views/home/homePage.vue'),
        meta: {
          title: 'Sleveen Admin',
          constant: false,
          // 无界 Vue：内容满屏，侧栏/页签悬浮
          menuType: '4'
        }
      },
      {
        name: 'mduser',
        path: 'user',
        component: () => import('@/views/home/homePage.vue'),
        meta: {
          title: 'user',
          constant: false,
          menuType: '4'
        }
      }
    ]
  },
  {
    name: '403',
    path: '/403',
    component: () => import('@/views/_builtin/403/403Page.vue'),
    meta: {
      title: '403',
      constant: true
    }
  },
  {
    name: '404',
    path: '/404',
    component: () => import('@/views/_builtin/404/404Page.vue'),
    meta: {
      title: '404',
      constant: true
    }
  },
  {
    name: '500',
    path: '/500',
    component: () => import('@/views/_builtin/500/500Page.vue'),
    meta: {
      title: '500',
      constant: true
    }
  },
  {
    name: 'login',
    path: '/login',
    component: () => import('@/views/_builtin/login/loginPage.vue'),
    meta: {
      title: 'login',
      constant: true
    }
  },
  {
    name: 'ssojump',
    path: '/ssojump',
    component: () => import('@/views/_builtin/ssojump/ssojumpPage.vue'),
    meta: {
      title: 'ssojump',
      constant: true
    }
  },
  // Sailflow：侧栏页用 base-layout；设计器 / 表单设计器 / 办理详情保持 blank 全屏
  {
    name: 'sail-process-list',
    path: '/sail/processes',
    component: () => import('@/layouts/base-layout/index.vue'),
    meta: {
      title: 'Sailflow 流程',
      constant: false
    },
    children: [
      {
        name: 'sail-process-list-page',
        path: '',
        component: () => import('@sail/views/process/Page/Biz_Sail_ProcessList.vue'),
        meta: {
          title: 'Sailflow 流程',
          constant: false
        }
      }
    ]
  },
  {
    name: 'sail-designer',
    path: '/sail/designer',
    component: () => import('@/layouts/blank-layout/index.vue'),
    meta: {
      title: 'Sailflow 设计器',
      constant: false,
      hideInMenu: true
    },
    children: [
      {
        name: 'sail-designer-page',
        path: '',
        component: () => import('@sail/views/SailDesignerPage.vue'),
        meta: {
          title: 'Sailflow 设计器',
          constant: false
        }
      }
    ]
  },
  // 门户设计：列表「设计」入口；blank-layout + 全屏（对齐 minehub Teleport/embedded）
  {
    name: 'portal-design',
    path: '/portal/design',
    component: () => import('@/layouts/blank-layout/index.vue'),
    meta: {
      title: '门户设计',
      constant: false,
      hideInMenu: true
    },
    children: [
      {
        name: 'portal-design-page',
        path: '',
        component: () => import('@/views/portal/design/index.vue'),
        meta: {
          title: '门户设计',
          constant: false,
          hideInMenu: true
        }
      }
    ]
  },
  {
    name: 'sail-ruleset',
    path: '/sail/rulesets',
    component: () => import('@/layouts/base-layout/index.vue'),
    meta: {
      title: 'Sailflow 规则集',
      constant: false
    },
    children: [
      {
        name: 'sail-ruleset-page',
        path: '',
        component: () => import('@sail/views/SailRuleSetPage.vue'),
        meta: {
          title: 'Sailflow 规则集',
          constant: false
        }
      }
    ]
  },
  {
    name: 'sail-ai-models',
    path: '/sail/ai-models',
    component: () => import('@/layouts/base-layout/index.vue'),
    meta: {
      title: 'Sailflow AI 模型',
      constant: false
    },
    children: [
      {
        name: 'sail-ai-models-page',
        path: '',
        component: () => import('@sail/views/SailAiModelPage.vue'),
        meta: {
          title: 'Sailflow AI 模型',
          constant: false
        }
      }
    ]
  },
  {
    name: 'sail-knowledge',
    path: '/sail/knowledge',
    component: () => import('@/layouts/base-layout/index.vue'),
    meta: {
      title: 'Sailflow 知识库',
      constant: false
    },
    children: [
      {
        name: 'sail-knowledge-page',
        path: '',
        component: () => import('@sail/views/SailKnowledgeBasePage.vue'),
        meta: {
          title: 'Sailflow 知识库',
          constant: false
        }
      }
    ]
  },
  // S1 办理闭环：待办 / 已办 / 发起 / 我的流程 / 详情
  {
    name: 'sail-task-todo',
    path: '/sail/task-todo',
    component: () => import('@/layouts/base-layout/index.vue'),
    meta: { title: '我的待办', constant: false },
    children: [
      {
        name: 'sail-task-todo-page',
        path: '',
        component: () => import('@sail/views/task/SailTaskTodoPage.vue'),
        meta: { title: '我的待办', constant: false }
      }
    ]
  },
  {
    name: 'sail-task-done',
    path: '/sail/task-done',
    component: () => import('@/layouts/base-layout/index.vue'),
    meta: { title: '我的已办', constant: false },
    children: [
      {
        name: 'sail-task-done-page',
        path: '',
        component: () => import('@sail/views/task/SailTaskDonePage.vue'),
        meta: { title: '我的已办', constant: false }
      }
    ]
  },
  {
    name: 'sail-process-create',
    path: '/sail/process-create',
    component: () => import('@/layouts/base-layout/index.vue'),
    meta: { title: '发起流程', constant: false },
    children: [
      {
        name: 'sail-process-create-page',
        path: '',
        component: () => import('@sail/views/process/SailProcessCreatePage.vue'),
        meta: { title: '发起流程', constant: false }
      }
    ]
  },
  {
    name: 'sail-process-my',
    path: '/sail/process-my',
    component: () => import('@/layouts/base-layout/index.vue'),
    meta: { title: '我的流程', constant: false },
    children: [
      {
        name: 'sail-process-my-page',
        path: '',
        component: () => import('@sail/views/process/SailProcessMyPage.vue'),
        meta: { title: '我的流程', constant: false }
      }
    ]
  },
  {
    name: 'SailProcessInstanceDetail',
    path: '/sail/process-detail',
    // 与待办等同壳 base-layout：从列表 router.push 时切 blank 易首屏白屏，刷新才正常
    component: () => import('@/layouts/base-layout/index.vue'),
    meta: { title: '流程详情', constant: false, hideInMenu: true },
    children: [
      {
        name: 'sail-process-detail-page',
        path: '',
        component: () => import('@sail/views/process/SailProcessDetailPage.vue'),
        meta: { title: '流程详情', constant: false, hideInMenu: true }
      }
    ]
  },
  {
    name: 'sail-forms',
    path: '/sail/forms',
    component: () => import('@/layouts/base-layout/index.vue'),
    meta: { title: '流程表单', constant: false },
    children: [
      {
        name: 'sail-forms-page',
        path: '',
        component: () => import('@sail/views/form/Page/Biz_Sail_FormList.vue'),
        meta: { title: '流程表单', constant: false }
      }
    ]
  },
  {
    name: 'sail-form-editor',
    path: '/sail/form-editor',
    component: () => import('@/layouts/blank-layout/index.vue'),
    meta: { title: '表单设计器', constant: false, hideInMenu: true },
    children: [
      {
        name: 'sail-form-editor-page',
        path: '',
        component: () => import('@sail/views/form/SailFormEditorPage.vue'),
        meta: { title: '表单设计器', constant: false }
      }
    ]
  },
  {
    name: 'sail-task-copy',
    path: '/sail/task-copy',
    component: () => import('@/layouts/base-layout/index.vue'),
    meta: { title: '抄送我的', constant: false },
    children: [
      {
        name: 'sail-task-copy-page',
        path: '',
        component: () => import('@sail/views/task/SailTaskCopyPage.vue'),
        meta: { title: '抄送我的', constant: false }
      }
    ]
  },
  {
    name: 'sail-task-manager',
    path: '/sail/task-manager',
    component: () => import('@/layouts/base-layout/index.vue'),
    meta: { title: '任务管理', constant: false },
    children: [
      {
        name: 'sail-task-manager-page',
        path: '',
        component: () => import('@sail/views/task/SailTaskManagerPage.vue'),
        meta: { title: '任务管理', constant: false }
      }
    ]
  },
  {
    name: 'sail-process-manager',
    path: '/sail/process-manager',
    component: () => import('@/layouts/base-layout/index.vue'),
    meta: { title: '流程实例管理', constant: false },
    children: [
      {
        name: 'sail-process-manager-page',
        path: '',
        component: () => import('@sail/views/process/SailProcessManagerPage.vue'),
        meta: { title: '流程实例管理', constant: false }
      }
    ]
  },
  {
    name: 'sail-outbox',
    path: '/sail/outbox',
    component: () => import('@/layouts/base-layout/index.vue'),
    meta: { title: '回调死信', constant: false },
    children: [
      {
        name: 'sail-outbox-page',
        path: '',
        component: () => import('@sail/views/ops/SailOutboxPage.vue'),
        meta: { title: '回调死信', constant: false }
      }
    ]
  },
  {
    name: 'sail-templates',
    path: '/sail/templates',
    component: () => import('@/layouts/base-layout/index.vue'),
    meta: { title: '流程模板库', constant: false },
    children: [
      {
        name: 'sail-templates-page',
        path: '',
        component: () => import('@sail/views/SailTemplateGalleryPage.vue'),
        meta: { title: '流程模板库', constant: false }
      }
    ]
  },
  {
    name: 'sail-leave',
    path: '/sail/leave',
    component: () => import('@/layouts/base-layout/index.vue'),
    meta: { title: '请假样例', constant: false },
    children: [
      {
        name: 'sail-leave-page',
        path: '',
        component: () => import('@sail/views/demo/SailLeavePage.vue'),
        meta: { title: '请假样例', constant: false }
      }
    ]
  },
  {
    name: 'sail-report',
    path: '/sail/report',
    component: () => import('@/layouts/base-layout/index.vue'),
    meta: { title: '流程报表', constant: false },
    children: [
      {
        name: 'sail-report-page',
        path: '',
        component: () => import('@sail/views/ops/SailReportPage.vue'),
        meta: { title: '流程报表', constant: false }
      }
    ]
  },
  {
    name: 'sail-categories',
    path: '/sail/categories',
    component: () => import('@/layouts/base-layout/index.vue'),
    meta: { title: '流程分类', constant: false },
    children: [
      {
        name: 'sail-categories-page',
        path: '',
        component: () => import('@sail/views/ops/SailCategoryPage.vue'),
        meta: { title: '流程分类', constant: false }
      }
    ]
  },
  {
    name: 'sail-process-edit',
    path: '/sail/process-edit',
    component: () => import('@/layouts/blank-layout/index.vue'),
    meta: {
      title: '流程编辑',
      constant: false,
      hideInMenu: true
    },
    children: [
      {
        name: 'sail-process-edit-page',
        path: '',
        component: () => import('@sail/views/model/form/SailProcessEditPage.vue'),
        meta: {
          title: '流程编辑',
          constant: false
        }
      }
    ]
  }
  // 原 ...ccflowRoute / ...nflowRoute 已移除：widgets/ccflow、nflow 未入仓，属死引用（任务3 R5）
];

/** create builtin vue routes */
export function createBuiltinVueRoutes() {
  return builtinRoutes;
  //移除魔法，直接配置
  //return transformToVueRoutes(builtinRoutes, layouts, views);
}
