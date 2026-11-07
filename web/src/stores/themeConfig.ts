import { computed } from 'vue';
import { defineStore } from 'pinia';
import { useThemeStore } from '@/store/modules/theme';

/**
 * 旧 useThemeConfig 兼容：themeConfig.globalTitle / globalComponentSize
 */
export const useThemeConfig = defineStore('legacy-themeConfig', () => {
  const theme = useThemeStore();

  const themeConfig = computed(() => ({
    globalTitle: (import.meta.env.VITE_APP_TITLE as string) || 'Admin',
    globalComponentSize: 'default',
    isIsDark: theme.darkMode,
    primary: theme.themeColors.primary
  }));

  return { themeConfig };
});

export default useThemeConfig;
