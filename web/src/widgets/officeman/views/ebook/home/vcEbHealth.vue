<template>
  <div class="eb-card" :style="{ minHeight: height ? height + 'px' : undefined }">
    <div class="eb-card__head">
      <div class="eb-card__title">创作健康</div>
      <el-link type="primary" :underline="false" @click="goTopic">全部 &gt;</el-link>
    </div>
    <div class="eb-health__list">
      <div v-for="item in items" :key="item.key" class="eb-health__item" @click="go(item.path)">
        <div class="eb-health__gauge" :ref="(el) => setGaugeRef(item.key, el)"></div>
        <div class="eb-health__meta">
          <div class="eb-health__label">{{ item.label }}</div>
          <div class="eb-health__status" :class="item.level">{{ item.status }}</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, nextTick, onMounted, reactive, watch } from 'vue';
import { useRouter } from 'vue-router';
import * as echarts from 'echarts/core';
import { GaugeChart } from 'echarts/charts';
import { CanvasRenderer } from 'echarts/renderers';
import { useEbHomeDashboard } from './useEbHomeDashboard.js';

echarts.use([GaugeChart, CanvasRenderer]);

defineOptions({ name: 'vcEbHealth' });

defineProps({
  vc: Object,
  cell: Object,
  para: Object,
  height: { default: null },
  width: { default: null }
});

const router = useRouter();
const { data, refresh } = useEbHomeDashboard();
const gaugeEls = reactive({});
const gaugeCharts = reactive({});

const items = computed(() => {
  const h = data.value?.health || {};
  return [
    {
      key: 'mat',
      label: '素材消化率',
      value: Number(h.matDigestRate) || 0,
      path: '/officeman/material',
      ...levelOf(Number(h.matDigestRate) || 0)
    },
    {
      key: 'topic',
      label: '选题完结率',
      value: Number(h.topicFinishRate) || 0,
      path: '/officeman/topic',
      ...levelOf(Number(h.topicFinishRate) || 0)
    },
    {
      key: 'note',
      label: '笔记报告完整度',
      value: Number(h.noteReportRate) || 0,
      path: '/officeman/note',
      ...levelOf(Number(h.noteReportRate) || 0)
    }
  ];
});

function levelOf(v) {
  if (v >= 90) return { status: '优秀', level: 'excellent' };
  if (v >= 70) return { status: '良好', level: 'good' };
  return { status: '待提升', level: 'warn' };
}

function setGaugeRef(key, el) {
  if (el) gaugeEls[key] = el;
}

function renderGauge(key, value) {
  const el = gaugeEls[key];
  if (!el) return;
  let chart = gaugeCharts[key];
  if (!chart) {
    chart = echarts.init(el);
    gaugeCharts[key] = chart;
  }
  const color = value >= 90 ? '#3ecf8e' : value >= 70 ? '#4A90E2' : '#f5a623';
  chart.setOption({
    series: [
      {
        type: 'gauge',
        startAngle: 90,
        endAngle: -270,
        radius: '90%',
        pointer: { show: false },
        progress: {
          show: true,
          overlap: false,
          roundCap: true,
          itemStyle: { color }
        },
        axisLine: { lineStyle: { width: 8, color: [[1, 'rgba(74,144,226,0.12)']] } },
        splitLine: { show: false },
        axisTick: { show: false },
        axisLabel: { show: false },
        data: [{ value }],
        detail: {
          fontSize: 13,
          fontWeight: 700,
          color: '#1f2a44',
          offsetCenter: [0, '4%'],
          formatter: `${value}%`
        }
      }
    ]
  });
}

watch(
  items,
  async (list) => {
    await nextTick();
    list.forEach((it) => renderGauge(it.key, it.value));
  },
  { immediate: true, deep: true }
);

onMounted(async () => {
  await nextTick();
  items.value.forEach((it) => renderGauge(it.key, it.value));
});

function go(path) {
  router.push(path);
}

function goTopic() {
  router.push('/officeman/topic');
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

.eb-health__list {
  margin-top: 8px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  flex: 1;
}

.eb-health__item {
  display: grid;
  grid-template-columns: 72px 1fr;
  gap: 10px;
  align-items: center;
  padding: 4px 2px;
  cursor: pointer;
  border-radius: 10px;
}

.eb-health__item:hover {
  background: rgba(74, 144, 226, 0.05);
}

.eb-health__gauge {
  width: 72px;
  height: 72px;
}

.eb-health__label {
  font-size: 14px;
  font-weight: 600;
  color: #1f2a44;
}

.eb-health__status {
  margin-top: 4px;
  font-size: 12px;
  font-weight: 600;
}

.eb-health__status.excellent { color: #3ecf8e; }
.eb-health__status.good { color: #4a90e2; }
.eb-health__status.warn { color: #f5a623; }
</style>
