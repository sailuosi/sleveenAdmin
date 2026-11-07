/**
 * mitt 事件类型定义
 *
 * @method onHomePageChanged 顶栏切换默认首页/门户时同步首页
 */
declare type MittType<T = any> = {
  onHomePageChanged?: string;
  [key: string]: T;
};
