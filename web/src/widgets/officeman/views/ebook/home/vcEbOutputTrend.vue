<template>
  <div class="eb-card" :style="{ minHeight: height ? height + 'px' : undefined }">
    <div class="eb-card__head">
      <div class="eb-card__title">创作产出</div>
      <el-select v-model="range" size="small" style="width: 88px" @change="onRangeChange">
        <el-option label="近7日" :value="7" />
        <el-option label="近14日" :value="14" />
      </el-select>
    </div>
    <div class="eb-card__hero">
      <div class="eb-card__value">{{ totalSum }}</div>
      <div class="eb-card__sub">近{{ range }}日新增合计 · 比上周 {{ weekCompareText }}</div>
    </div>
    <div class="eb-card__chart" ref="chartRef"></div>
  </div>
</template>

<script setup>
import { computed, ref, watch } from 'vue';
import { useEcharts } from '@/hooks/common/echarts';
import { useEbHomeDashboard } from './useEbHomeDashboard.js';

defineOptions({ name: 'vcEbOutputTrend' });

defineProps({
  vc: Object,
  cell: Object,
  para: Object,
  height: { default: null },
  width: { default: null }
});

const range = ref(7);
const { data, refresh } = useEbHomeDashboard();

const trend = computed(() => data.value?.trend || { labels: [], total: [], topic: [], mat: [], note: [] });

const totalSum = computed(() => (trend.value.total || []).reduce((a, b) => a + (Number(b) || 0), 0));

const weekCompareText = computed(() => {
  const arr = trend.value.total || [];
  if (arr.length < 2) return '+0%';
  const mid = Math.floor(arr.length / 2);
  const recent = arr.slice(mid).reduce((a, b) => a + (Number(b) || 0), 0);
  const prev = arr.slice(0, mid).reduce((a, b) => a + (Number(b) || 0), 0);
  if (prev <= 0) return recent > 0 ? '+100%' : '+0%';
  const pct = Math.round(((recent - prev) / prev) * 1000) / 10;
  return `${pct >= 0 ? '+' : ''}${pct}%`;
});

const { domRef: chartRef, updateOptions } = useEcharts(() => ({
  tooltip: { trigger: 'axis' },
  grid: { left: 28, right: 12, top: 20, bottom: 28 },
  xAxis: { type: 'category', boundaryGap: false, data: [], axisLine: { show: false }, axisTick: { show: false } },
  yAxis: { type: 'value', splitLine: { lineStyle: { color: 'rgba(74,144,226,0.12)' } }, axisLabel: { color: '#8a97ad' } },
  series: [
    {
      name: '产出',
      type: 'line',
      smooth: true,
      symbol: 'circle',
      symbolSize: 6,
      color: '#4A90E2',
      areaStyle: {
        color: {
          type: 'linear',
          x: 0,
          y: 0,
          x2: 0,
          y2: 1,
          colorStops: [
            { offset: 0, color: 'rgba(74,144,226,0.35)' },
            { offset: 1, color: 'rgba(74,144,226,0.02)' }
          ]
        }
      },
      data: []
    }
  ]
}));

watch(
  () => [trend.value.labels, trend.value.total],
  async () => {
    await updateOptions((opts) => {
      opts.xAxis.data = trend.value.labels || [];
      opts.series[0].data = trend.value.total || [];
      return opts;
    });
  },
  { immediate: true, deep: true }
);

function onRangeChange(days) {
  refresh(days);
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

.eb-card__hero {
  margin-top: 8px;
}

.eb-card__value {
  font-size: 28px;
  font-weight: 750;
  color: #1f2a44;
}

.eb-card__sub {
  margin-top: 2px;
  font-size: 12px;
  color: #3ecf8e;
  font-weight: 600;
}

.eb-card__chart {
  flex: 1;
  min-height: 140px;
  margin-top: 8px;
}
</style>
