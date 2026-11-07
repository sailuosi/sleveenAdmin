import { computed } from 'vue';
import { defineStore } from 'pinia';
import { useTabStore } from '@/store/modules/tab';

/**
 * 旧 tagsViewRoutes 兼容：ccflow request 用 current.id 作为 pageId 头。
 */
export const useTagsViewRoutes = defineStore('legacy-tagsViewRoutes', () => {
  const tabStore = useTabStore();

  const current = computed(() => {
    const id = tabStore.activeTabId;
    if (!id) return null;
    const tab = tabStore.tabs.find(t => t.id === id);
    return { id, ...(tab || {}) };
  });

  return { current };
});

export default useTagsViewRoutes;
