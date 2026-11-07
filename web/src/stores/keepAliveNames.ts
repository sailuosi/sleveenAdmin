import { computed } from 'vue';
import { defineStore } from 'pinia';
import { useTabStore } from '@/store/modules/tab';

/** 旧 keepAliveNames 兼容（多为只读缓存名列表） */
export const useKeepALiveNames = defineStore('legacy-keepAliveNames', () => {
  const tabStore = useTabStore();
  const keepAliveNames = computed(() =>
    (tabStore.tabs || []).map((t: { routeKey?: string; id?: string }) => t.routeKey || t.id).filter(Boolean)
  );
  return { keepAliveNames };
});

export default useKeepALiveNames;
