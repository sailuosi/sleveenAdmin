import type { AdminLayoutProps, LayoutCssVars, LayoutCssVarsProps } from '../../types';

/** The id of the scroll element of the layout */
export const LAYOUT_SCROLL_EL_ID = '__SCROLL_EL_ID__';

/** The max z-index of the layout */
export const LAYOUT_MAX_Z_INDEX = 100;

/**
 * Create layout css vars by css vars props
 *
 * @param props Css vars props
 */
function createLayoutCssVarsByCssVarsProps(props: LayoutCssVarsProps) {
  const cssVars: LayoutCssVars = {
    '--soy-header-height': `${props.headerHeight}px`,
    '--soy-header-z-index': props.headerZIndex,
    '--soy-header-inset': `${props.headerInset ?? 0}px`,
    '--soy-header-radius': `${props.headerRadius ?? 0}px`,
    '--soy-tab-height': `${props.tabHeight}px`,
    '--soy-tab-z-index': props.tabZIndex,
    '--soy-sider-width': `${props.siderWidth}px`,
    '--soy-sider-collapsed-width': `${props.siderCollapsedWidth}px`,
    '--soy-sider-inset': `${props.siderInset ?? 0}px`,
    '--soy-sider-radius': `${props.siderRadius ?? 0}px`,
    '--soy-sider-z-index': props.siderZIndex,
    '--soy-mobile-sider-z-index': props.mobileSiderZIndex,
    '--soy-footer-height': `${props.footerHeight}px`,
    '--soy-footer-z-index': props.footerZIndex
  };

  return cssVars;
}

/**
 * Create layout css vars
 *
 * @param props
 */
export function createLayoutCssVars(props: AdminLayoutProps) {
  const {
    mode,
    isMobile,
    maxZIndex = LAYOUT_MAX_Z_INDEX,
    headerHeight,
    tabHeight,
    siderWidth,
    siderCollapsedWidth,
    siderInset = 0,
    siderRadius = 0,
    headerInset = 0,
    headerRadius = 0,
    floatHeader = false,
    footerHeight
  } = props;

  const headerZIndex = floatHeader ? maxZIndex - 1 : maxZIndex - 3;
  const tabZIndex = maxZIndex - 5;
  const siderZIndex = mode === 'vertical' || isMobile ? maxZIndex - 1 : maxZIndex - 4;
  const mobileSiderZIndex = isMobile ? maxZIndex - 2 : 0;
  const footerZIndex = maxZIndex - 5;

  const cssProps: LayoutCssVarsProps = {
    headerHeight,
    headerZIndex,
    headerInset,
    headerRadius,
    tabHeight,
    tabZIndex,
    siderWidth,
    siderZIndex,
    mobileSiderZIndex,
    siderCollapsedWidth,
    siderInset,
    siderRadius,
    footerHeight,
    footerZIndex
  };

  return createLayoutCssVarsByCssVarsProps(cssProps);
}
