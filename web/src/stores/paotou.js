import { defineStore } from 'pinia';

/** 门户配置缓存（原 mineone/旧项目 paotou store） */
export const usePaotal = defineStore('paotal', {
  state: () => ({
    paotalMap: {}
  })
});

export default usePaotal;
