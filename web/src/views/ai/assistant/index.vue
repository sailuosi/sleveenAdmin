<script setup lang="ts">
import { ref, watch } from 'vue';
import { BubbleList, XSender, useXStream } from 'vue-element-plus-x';
import { fetchAiChatStream, type AiChatMessage } from '@/service/api';
import 'element-plus/dist/index.css';
import 'vue-element-plus-x/styles/index.css';

interface BubbleItem {
  content: string;
  role: AiChatMessage['role'];
  placement: 'start' | 'end';
  loading?: boolean;
}

type XSenderExpose = {
  getModelValue: () => { text: string };
  clear: () => void;
};

type SseChunk = {
  data?: string;
};

const list = ref<BubbleItem[]>([
  {
    content: '你好，我是 sleveen-admin AI 助手。登录后可开始多轮对话；未配置 OpenAI 时将使用占位模型。',
    role: 'assistant',
    placement: 'start'
  }
]);

const senderRef = ref<XSenderExpose | null>(null);
const { startStream, cancel, data, error, isLoading } = useXStream();

let assistantIndex = -1;
let processedCount = 0;

function patchBubble(index: number, patch: Partial<BubbleItem>) {
  list.value[index] = { ...list.value[index], ...patch };
}

function appendAssistantChunk(index: number, chunk: string) {
  const item = list.value[index];
  patchBubble(index, {
    content: item.content + chunk,
    // Bubble 在 loading=true 时会隐藏 content，收到首包后必须关闭 loading 才能逐字显示
    loading: false
  });
}

/** useXStream 通过 push 追加 chunk，浅层 watch 不会触发，必须 deep: true */
watch(
  data,
  (items: SseChunk[]) => {
    if (assistantIndex < 0) return;

    while (processedCount < items.length) {
      const chunk = items[processedCount]?.data;
      processedCount += 1;
      if (typeof chunk === 'string' && chunk !== '[DONE]') {
        appendAssistantChunk(assistantIndex, chunk);
      }
    }
  },
  { deep: true }
);

watch(error, err => {
  if (!err || assistantIndex < 0) return;
  patchBubble(assistantIndex, {
    content: err.message || '流式请求失败',
    loading: false
  });
});

function buildMessages(): AiChatMessage[] {
  return list.value
    .filter(item => item.content.trim())
    .map(item => ({ role: item.role, content: item.content }));
}

async function handleSend() {
  const text = senderRef.value?.getModelValue()?.text?.trim() ?? '';
  if (!text || isLoading.value) return;

  list.value.push({ content: text, role: 'user', placement: 'end' });
  list.value.push({ content: '', role: 'assistant', placement: 'start', loading: true });
  assistantIndex = list.value.length - 1;
  processedCount = 0;
  senderRef.value?.clear();

  const history = buildMessages();
  const currentIndex = assistantIndex;

  try {
    const response = await fetchAiChatStream({ messages: history });
    if (response.status === 401) {
      patchBubble(currentIndex, { content: '请先登录后再使用 AI 助手。', loading: false });
      return;
    }
    if (!response.ok || !response.body) {
      patchBubble(currentIndex, {
        content: '请求失败，请确认后端已启动且 AI 配置正确。',
        loading: false
      });
      return;
    }

    await startStream({ readableStream: response.body });

    if (!list.value[currentIndex].content.trim()) {
      patchBubble(currentIndex, { content: '未收到回复，请重试。', loading: false });
    }
  } catch {
    patchBubble(currentIndex, { content: '无法连接 AI 服务。', loading: false });
  } finally {
    patchBubble(currentIndex, { loading: false });
    assistantIndex = -1;
  }
}

function handleCancel() {
  cancel();
}
</script>

<template>
  <div class="h-full flex flex-col gap-16px p-16px">
    <div class="flex-1 min-h-0 overflow-auto">
      <BubbleList :list="list" :btn-loading="isLoading" auto-scroll />
    </div>
    <XSender ref="senderRef" :loading="isLoading" @submit="handleSend" @cancel="handleCancel" />
  </div>
</template>
