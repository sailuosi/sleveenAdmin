import { onMounted, ref } from 'vue';
import { loadEbHomeDashboard } from './Biz_EbHomeapi.js';

const shared = ref(null);
let inflight = null;

export function useEbHomeDashboard() {
  const loading = ref(false);

  async function refresh(days = 7) {
    if (inflight) return inflight;
    loading.value = true;
    inflight = loadEbHomeDashboard({ days })
      .then((res) => {
        shared.value = res || null;
        return shared.value;
      })
      .catch(() => {
        shared.value = shared.value || emptyDashboard();
        return shared.value;
      })
      .finally(() => {
        loading.value = false;
        inflight = null;
      });
    return inflight;
  }

  onMounted(() => {
    if (!shared.value) refresh();
  });

  return { data: shared, loading, refresh };
}

function emptyDashboard() {
  const zeros = [0, 0, 0, 0, 0, 0, 0];
  return {
    welcome: {
      topicTotal: 0,
      inProgress: 0,
      published: 0,
      noteTotal: 0,
      weekTopicNew: 0,
      weekMatNew: 0,
      weekNoteNew: 0
    },
    stats: {
      topicTotal: 0,
      topicWeekNew: 0,
      matTotal: 0,
      matWeekNew: 0,
      noteTotal: 0,
      noteWeekNew: 0,
      published: 0,
      publishedWeek: 0,
      sparklines: { topic: zeros, mat: zeros, note: zeros, published: zeros }
    },
    trend: {
      labels: [],
      topic: zeros,
      mat: zeros,
      note: zeros,
      total: zeros
    },
    engage: { likes: 0, bookmarks: 0, views: 0 },
    health: {
      matDigestRate: 0,
      topicFinishRate: 0,
      noteReportRate: 0
    }
  };
}
