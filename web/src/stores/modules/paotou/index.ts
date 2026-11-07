import { defineStore } from 'pinia';

export const usePaotal = defineStore('paotalInfo', {
  state: () => ({
    paotalMap: {} as Record<string, unknown>
  }),
  actions: {}
});
