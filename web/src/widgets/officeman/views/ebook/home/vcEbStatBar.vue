<template>
  <div class="eb-statbar" :style="{ minHeight: height ? height + 'px' : undefined }">
    <div
      v-for="card in cards"
      :key="card.key"
      class="eb-statbar__card"
      @click="go(card.path)"
    >
      <div class="eb-statbar__icon" :style="{ background: card.iconBg, color: card.color }">
        <SvgIcon :name="card.icon" />
      </div>
      <div class="eb-statbar__main">
        <div class="eb-statbar__label">{{ card.label }}</div>
        <div class="eb-statbar__value">{{ card.value }}</div>
        <div class="eb-statbar__sub" :style="{ color: card.color }">
          +{{ card.weekNew }} 本周新增
        </div>
      </div>
      <EbMiniSpark :values="card.spark" :color="card.color" />
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue';
import { useRouter } from 'vue-router';
import { useEbHomeDashboard } from './useEbHomeDashboard.js';
import EbMiniSpark from './EbMiniSpark.vue';

defineOptions({ name: 'vcEbStatBar' });

defineProps({
  vc: Object,
  cell: Object,
  para: Object,
  height: { default: null },
  width: { default: null }
});

const router = useRouter();
const { data, refresh } = useEbHomeDashboard();

const cards = computed(() => {
  const s = data.value?.stats || {};
  const sparks = s.sparklines || {};
  return [
    {
      key: 'topic',
      label: '选题总数',
      value: s.topicTotal ?? 0,
      weekNew: s.topicWeekNew ?? 0,
      spark: sparks.topic || [],
      path: '/officeman/topic',
      icon: 'fa fa-lightbulb-o',
      color: '#4A90E2',
      iconBg: 'rgba(74,144,226,0.12)'
    },
    {
      key: 'mat',
      label: '素材收藏',
      value: s.matTotal ?? 0,
      weekNew: s.matWeekNew ?? 0,
      spark: sparks.mat || [],
      path: '/officeman/material',
      icon: 'fa fa-folder-o',
      color: '#f5a623',
      iconBg: 'rgba(245,166,35,0.14)'
    },
    {
      key: 'note',
      label: '笔记份数',
      value: s.noteTotal ?? 0,
      weekNew: s.noteWeekNew ?? 0,
      spark: sparks.note || [],
      path: '/officeman/note',
      icon: 'fa fa-file-text-o',
      color: '#4A90E2',
      iconBg: 'rgba(74,144,226,0.12)'
    },
    {
      key: 'published',
      label: '已发布',
      value: s.published ?? 0,
      weekNew: s.publishedWeek ?? 0,
      spark: sparks.published || [],
      path: '/officeman/topic',
      icon: 'fa fa-check-circle',
      color: '#3ecf8e',
      iconBg: 'rgba(62,207,142,0.14)'
    }
  ];
});

function go(path) {
  router.push(path);
}

defineExpose({ refresh });
</script>

<style scoped>
.eb-statbar {
  height: 100%;
  box-sizing: border-box;
  padding: 4px;
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 12px;
}

.eb-statbar__card {
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.88);
  border: 1px solid rgba(230, 238, 248, 0.95);
  box-shadow: 0 6px 18px rgba(74, 144, 226, 0.08);
  padding: 14px 14px 12px;
  display: grid;
  grid-template-columns: auto 1fr auto;
  gap: 10px;
  align-items: center;
  cursor: pointer;
  transition: transform 0.15s ease, box-shadow 0.15s ease;
}

.eb-statbar__card:hover {
  transform: translateY(-1px);
  box-shadow: 0 10px 22px rgba(74, 144, 226, 0.14);
}

.eb-statbar__icon {
  width: 36px;
  height: 36px;
  border-radius: 12px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 16px;
}

.eb-statbar__label {
  font-size: 12px;
  color: #7a889f;
}

.eb-statbar__value {
  margin-top: 2px;
  font-size: 22px;
  font-weight: 700;
  color: #1f2a44;
  line-height: 1.1;
}

.eb-statbar__sub {
  margin-top: 4px;
  font-size: 12px;
  font-weight: 600;
}

@media (max-width: 1100px) {
  .eb-statbar {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
</style>
