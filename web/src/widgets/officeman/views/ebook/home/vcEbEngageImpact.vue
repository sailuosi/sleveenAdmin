<template>
  <div class="eb-card" :style="{ minHeight: height ? height + 'px' : undefined }">
    <div class="eb-card__head">
      <div class="eb-card__title">互动影响</div>
      <el-select v-model="scope" size="small" style="width: 88px" disabled>
        <el-option label="累计" value="all" />
      </el-select>
    </div>
    <div class="eb-engage__list">
      <div v-for="item in items" :key="item.key" class="eb-engage__item" @click="goNote">
        <div class="eb-engage__icon" :style="{ background: item.bg, color: item.color }">
          <SvgIcon :name="item.icon" />
        </div>
        <div class="eb-engage__meta">
          <div class="eb-engage__label">{{ item.label }}</div>
          <div class="eb-engage__hint">来自笔记互动字段汇总</div>
        </div>
        <div class="eb-engage__value">{{ formatNum(item.value) }}</div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useEbHomeDashboard } from './useEbHomeDashboard.js';

defineOptions({ name: 'vcEbEngageImpact' });

defineProps({
  vc: Object,
  cell: Object,
  para: Object,
  height: { default: null },
  width: { default: null }
});

const router = useRouter();
const scope = ref('all');
const { data, refresh } = useEbHomeDashboard();

const items = computed(() => {
  const e = data.value?.engage || {};
  return [
    {
      key: 'likes',
      label: '总点赞',
      value: e.likes ?? 0,
      icon: 'fa fa-thumbs-o-up',
      color: '#3ecf8e',
      bg: 'rgba(62,207,142,0.14)'
    },
    {
      key: 'bookmarks',
      label: '总收藏',
      value: e.bookmarks ?? 0,
      icon: 'fa fa-bookmark-o',
      color: '#4A90E2',
      bg: 'rgba(74,144,226,0.12)'
    },
    {
      key: 'views',
      label: '总浏览',
      value: e.views ?? 0,
      icon: 'fa fa-eye',
      color: '#f5a623',
      bg: 'rgba(245,166,35,0.14)'
    }
  ];
});

function formatNum(n) {
  const v = Number(n) || 0;
  return v.toLocaleString('zh-CN');
}

function goNote() {
  router.push('/officeman/note');
}

defineExpose({ refresh });
</script>

<style scoped>
.eb-card {
  height: 100%;
  box-sizing: border-box;
  margin: 4px;
  padding: 14px 16px;
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.9);
  border: 1px solid rgba(230, 238, 248, 0.95);
  box-shadow: 0 6px 18px rgba(74, 144, 226, 0.08);
  display: flex;
  flex-direction: column;
}

.eb-card__head {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.eb-card__title {
  font-size: 15px;
  font-weight: 700;
  color: #1f2a44;
}

.eb-engage__list {
  margin-top: 18px;
  display: flex;
  flex-direction: column;
  gap: 14px;
  flex: 1;
}

.eb-engage__item {
  display: grid;
  grid-template-columns: auto 1fr auto;
  gap: 12px;
  align-items: center;
  padding: 10px 8px;
  border-radius: 12px;
  cursor: pointer;
  transition: background 0.15s ease;
}

.eb-engage__item:hover {
  background: rgba(74, 144, 226, 0.06);
}

.eb-engage__icon {
  width: 42px;
  height: 42px;
  border-radius: 14px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 18px;
}

.eb-engage__label {
  font-size: 14px;
  font-weight: 600;
  color: #1f2a44;
}

.eb-engage__hint {
  margin-top: 2px;
  font-size: 12px;
  color: #8a97ad;
}

.eb-engage__value {
  font-size: 20px;
  font-weight: 700;
  color: #1f2a44;
}
</style>
