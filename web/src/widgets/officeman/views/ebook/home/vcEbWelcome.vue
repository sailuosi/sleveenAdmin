<template>
  <div class="eb-welcome" :style="{ minHeight: height ? height + 'px' : undefined }">
    <div class="eb-welcome__hero">
      <div class="eb-welcome__top">
        <div class="eb-welcome__greet">
          <div class="eb-welcome__hello">{{ greeting }}，{{ userName }}</div>
          <div class="eb-welcome__tagline">本周创作节奏不错</div>
        </div>
        <div class="eb-welcome__actions">
          <el-input
            v-model="keyword"
            class="eb-welcome__search"
            clearable
            placeholder="搜索笔记标题"
            @keyup.enter="goNoteSearch"
          >
            <template #prefix>
              <el-icon><Search /></el-icon>
            </template>
          </el-input>
          <button type="button" class="eb-welcome__icon-btn" title="笔记" @click="go('/officeman/note')">
            <el-icon><Bell /></el-icon>
          </button>
          <button type="button" class="eb-welcome__icon-btn" title="选题池" @click="go('/officeman/topic')">
            <el-icon><Calendar /></el-icon>
          </button>
        </div>
      </div>

      <div class="eb-welcome__body">
        <div class="eb-welcome__summary glass-card">
          <div class="eb-welcome__summary-left">
            <div class="eb-welcome__summary-title">总选题数 {{ welcome.topicTotal }}</div>
            <div class="eb-welcome__summary-rows">
              <div class="eb-welcome__summary-row">
                <span class="dot dot--ok"></span>
                进行中 {{ welcome.inProgress }}
              </div>
              <div class="eb-welcome__summary-row">
                <span class="dot dot--pub"></span>
                已发布 {{ welcome.published }}
              </div>
            </div>
          </div>
          <div class="eb-welcome__ring" ref="ringRef"></div>
        </div>

        <div class="eb-welcome__side glass-card">
          <div class="eb-welcome__date">{{ todayText }}</div>
          <div class="eb-welcome__side-metrics">
            <div class="eb-welcome__side-item">
              <span class="label">本周选题</span>
              <strong>{{ welcome.weekTopicNew }}</strong>
            </div>
            <div class="eb-welcome__side-item">
              <span class="label">本周素材</span>
              <strong>{{ welcome.weekMatNew }}</strong>
            </div>
            <div class="eb-welcome__side-item">
              <span class="label">本周笔记</span>
              <strong>{{ welcome.weekNoteNew }}</strong>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { Bell, Calendar, Search } from '@element-plus/icons-vue';
import { useEcharts } from '@/hooks/common/echarts';
import { useAuthStore } from '@/store/modules/auth';
import { useEbHomeDashboard } from './useEbHomeDashboard.js';

defineOptions({ name: 'vcEbWelcome' });

defineProps({
  vc: Object,
  cell: Object,
  para: Object,
  height: { default: null },
  width: { default: null }
});

const router = useRouter();
const authStore = useAuthStore();
const { data, refresh } = useEbHomeDashboard();
const keyword = ref('');

const userName = computed(() => authStore.userInfo?.userName || authStore.userInfo?.userAccount || '创作者');

const greeting = computed(() => {
  const h = new Date().getHours();
  if (h < 12) return '早上好';
  if (h < 18) return '下午好';
  return '晚上好';
});

const weekdays = ['星期日', '星期一', '星期二', '星期三', '星期四', '星期五', '星期六'];
const todayText = computed(() => {
  const d = new Date();
  return `${d.getFullYear()}-${d.getMonth() + 1}-${d.getDate()} ${weekdays[d.getDay()]}`;
});

const welcome = computed(() => data.value?.welcome || {
  topicTotal: 0, inProgress: 0, published: 0, noteTotal: 0,
  weekTopicNew: 0, weekMatNew: 0, weekNoteNew: 0
});

const { domRef: ringRef, updateOptions } = useEcharts(() => ({
  series: [
    {
      type: 'gauge',
      startAngle: 90,
      endAngle: -270,
      radius: '95%',
      pointer: { show: false },
      progress: {
        show: true,
        overlap: false,
        roundCap: true,
        clip: false,
        itemStyle: { color: '#4A90E2' }
      },
      axisLine: { lineStyle: { width: 10, color: [[1, 'rgba(74,144,226,0.15)']] } },
      splitLine: { show: false },
      axisTick: { show: false },
      axisLabel: { show: false },
      data: [{ value: 0 }],
      detail: {
        fontSize: 13,
        fontWeight: 700,
        color: '#1f2a44',
        offsetCenter: [0, '0%'],
        formatter: () => '笔记\n0'
      },
      title: { show: false }
    }
  ]
}));

watch(
  () => [welcome.value.noteTotal, welcome.value.topicTotal],
  async ([noteTotal, topicTotal]) => {
    const pct = Math.min(100, Math.round((Number(noteTotal) || 0) * 100 / Math.max(Number(topicTotal) || 1, 1)));
    await updateOptions((opts) => {
      opts.series[0].data = [{ value: pct }];
      opts.series[0].detail.formatter = () => `笔记\n${noteTotal || 0}`;
      return opts;
    });
  },
  { immediate: true }
);

function go(path) {
  router.push(path);
}

function goNoteSearch() {
  const q = keyword.value?.trim();
  router.push({ path: '/officeman/note', query: q ? { keyword: q } : undefined });
}

defineExpose({ refresh });
</script>

<style scoped>
.eb-welcome {
  height: 100%;
  box-sizing: border-box;
  padding: 4px;
}

.eb-welcome__hero {
  height: 100%;
  border-radius: 18px;
  padding: 18px 20px;
  background: transparent;
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.eb-welcome__top {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
  flex-wrap: wrap;
}

.eb-welcome__hello {
  font-size: 22px;
  font-weight: 700;
  color: #1f2a44;
}

.eb-welcome__tagline {
  margin-top: 4px;
  color: #5b6b86;
  font-size: 14px;
}

.eb-welcome__actions {
  display: flex;
  align-items: center;
  gap: 10px;
}

.eb-welcome__search {
  width: 220px;
}

.eb-welcome__search :deep(.el-input__wrapper) {
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.72);
  box-shadow: none;
  border: 1px solid rgba(255, 255, 255, 0.85);
}

.eb-welcome__icon-btn {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  border: 1px solid rgba(255, 255, 255, 0.8);
  background: rgba(255, 255, 255, 0.7);
  color: #4a90e2;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
}

.eb-welcome__body {
  display: grid;
  grid-template-columns: minmax(280px, 1.4fr) minmax(180px, 0.8fr);
  gap: 14px;
  flex: 1;
  min-height: 0;
}

.glass-card {
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.72);
  border: 1px solid rgba(255, 255, 255, 0.9);
  box-shadow: 0 8px 24px rgba(74, 144, 226, 0.12);
  backdrop-filter: blur(10px);
}

.eb-welcome__summary {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  gap: 12px;
}

.eb-welcome__summary-title {
  font-size: 18px;
  font-weight: 700;
  color: #1f2a44;
}

.eb-welcome__summary-rows {
  margin-top: 14px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  color: #5b6b86;
  font-size: 13px;
}

.eb-welcome__summary-row {
  display: flex;
  align-items: center;
  gap: 8px;
}

.dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

.dot--ok { background: #3ecf8e; }
.dot--pub { background: #4a90e2; }

.eb-welcome__ring {
  width: 120px;
  height: 120px;
  flex-shrink: 0;
}

.eb-welcome__side {
  padding: 16px 18px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}

.eb-welcome__date {
  font-size: 13px;
  color: #5b6b86;
}

.eb-welcome__side-metrics {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.eb-welcome__side-item {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
}

.eb-welcome__side-item .label {
  color: #7a889f;
  font-size: 12px;
}

.eb-welcome__side-item strong {
  font-size: 18px;
  color: #1f2a44;
}

@media (max-width: 900px) {
  .eb-welcome__body {
    grid-template-columns: 1fr;
  }
}
</style>
