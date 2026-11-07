import { getServiceBaseURL } from '@/utils/service';
import { getToken } from '@/store/modules/auth/shared';

export type AiChatMessage = {
  role: 'user' | 'assistant' | 'system';
  content: string;
};

export type AiChatInput = {
  prompt?: string;
  messages?: AiChatMessage[];
};

/** 发起 AI 流式对话，返回原始 fetch Response（body 为 SSE）。 */
export async function fetchAiChatStream(input: AiChatInput) {
  const isHttpProxy = import.meta.env.DEV && import.meta.env.VITE_HTTP_PROXY === 'Y';
  const { baseURL } = getServiceBaseURL(import.meta.env, isHttpProxy);
  const token = getToken();

  return fetch(`${baseURL}/api/ai/chat`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {})
    },
    body: JSON.stringify(input)
  });
}
