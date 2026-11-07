<script setup lang="ts">
import { ref, nextTick } from 'vue';
import { getToken } from '@/utils/auth.js';

// ========== 类型 ==========
interface ChatMessage {
  role: string;
  content: string;
  toolName?: string;
  toolResult?: string;
  excelData?: { fileName: string; headers: string[]; rowCount: number; rows: any[] };
}

// ========== 状态 ==========
const isOpen = ref(false);
const messages = ref<ChatMessage[]>([]);
const inputText = ref('');
const isStreaming = ref(false);
const conversationId = ref('');
const chatBody = ref<HTMLElement | null>(null);
const pendingExcel = ref<ChatMessage['excelData'] | null>(null);
const isUploading = ref(false);
const fileInput = ref<HTMLInputElement | null>(null);
const streamingStatus = ref(''); // 当前工具调用状态文字
const toolNames: Record<string, string> = {
  'get_current_user_org': '获取用户权限...',
  'search_personnel': '搜索匹配人员...',
  'search_location': '搜索匹配地点...',
  'search_mine_location': '搜索匹配地点...',
  'auto_fill_personnel': '自动填充人员信息...',
  'get_three_violations_code_tables': '加载字典数据...',
  'save_three_violations_draft': '正在保存数据到数据库...',
};

function getToolLabel(name?: string) {
  if (!name) return '';
  return toolNames[name] || name;
}

function getMessageText(msg: ChatMessage, index: number) {
  const text = msg.content?.trim();
  if (text) return msg.content;
  if (isStreaming.value && index === messages.value.length - 1 && !msg.toolName) return '...';
  return '';
}

// ========== 滚动 ==========
async function scrollToBottom() {
  await nextTick();
  if (chatBody.value) {
    chatBody.value.scrollTop = chatBody.value.scrollHeight;
  }
}

// ========== 打开面板 ==========
function openPanel() {
  isOpen.value = true;
  nextTick(() => {
    const ta = document.querySelector('.gc-input-area textarea') as HTMLTextAreaElement;
    ta?.focus();
  });
}

// ========== 关闭面板 ==========
function closePanel() {
  isOpen.value = false;
}

// ========== 新对话 ==========
function newChat() {
  messages.value = [];
  conversationId.value = '';
  pendingExcel.value = null;
}

// ========== 文件上传 ==========
function triggerUpload() {
  fileInput.value?.click();
}

async function onFileSelected(e: Event) {
  const input = e.target as HTMLInputElement;
  const file = input.files?.[0];
  if (!file) return;

  // 检查扩展名
  const ext = file.name.split('.').pop()?.toLowerCase();
  if (ext !== 'xls' && ext !== 'xlsx') {
    messages.value.push({ role: 'assistant', content: '⚠️ 仅支持 .xls 或 .xlsx 格式的 Excel 文件' });
    input.value = '';
    return;
  }

  isUploading.value = true;
  try {
    const token = getToken();
    const baseUrl = getServiceBaseURL();
    const formData = new FormData();
    formData.append('file', file);

    const res = await fetch(`${baseUrl}/api/ai/upload-excel`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${token}` },
      body: formData
    });

    const json = await res.json();
    // 适配不同的响应格式
    const result = json.result || json;
    const data = result.data || result;
    if (data && data.rows && data.rows.length > 0) {
      pendingExcel.value = {
        fileName: data.fileName || file.name,
        headers: data.headers || [],
        rowCount: data.rowCount || data.rows.length,
        rows: data.rows
      };
      // 在聊天中显示预览卡片
      messages.value.push({
        role: 'user',
        content: `📊 已上传: ${file.name}（${pendingExcel.value.rowCount} 行）\n点击发送按钮或输入"录入这些数据"来开始录入`,
        excelData: pendingExcel.value
      });
      await scrollToBottom();
    } else {
      messages.value.push({ role: 'assistant', content: '⚠️ Excel 文件为空或解析失败' });
    }
  } catch (err: any) {
    messages.value.push({ role: 'assistant', content: `⚠️ 上传失败: ${err.message}` });
  } finally {
    isUploading.value = false;
    input.value = '';
  }
}

// ========== 发送 ==========
async function sendMessage() {
  const hasText = inputText.value.trim();
  const hasExcel = pendingExcel.value && pendingExcel.value.rows.length > 0;

  if (!hasText && !hasExcel) return;
  if (isStreaming.value) return;

  // 构建消息内容
  let messageText = inputText.value.trim() || '请录入这些三违数据';

  // 如果有待处理的 Excel 数据，附加到消息中
  if (hasExcel && pendingExcel.value) {
    const tableRows = pendingExcel.value.rows.slice(0, 3); // 只附前3行作为示例
    const headerLine = '| ' + pendingExcel.value.headers.join(' | ') + ' |';
    const sepLine = '|' + pendingExcel.value.headers.map(() => '---').join('|') + '|';
    const dataLines = tableRows.map((r: any) =>
      '| ' + pendingExcel.value!.headers.map((h: string) => r[h] || '').join(' | ') + ' |'
    ).join('\n');
    const moreHint = pendingExcel.value.rowCount > 3
      ? `\n（共 ${pendingExcel.value.rowCount} 行，仅展示前 3 行）` : '';

    messageText += `\n\n以下是 Excel 文件 "${pendingExcel.value.fileName}" 的解析数据：\n${headerLine}\n${sepLine}\n${dataLines}${moreHint}\n\n请逐条匹配人员、地点和字典后录入。`;

    // 如果行数很多，附上完整 JSON
    if (pendingExcel.value.rowCount > 3) {
      messageText += `\n完整数据（JSON）：\n${JSON.stringify(pendingExcel.value.rows, null, 2)}`;
    }

    pendingExcel.value = null;
  }

  inputText.value = '';

  messages.value.push({ role: 'user', content: messageText });
  await scrollToBottom();

  const aiIdx = messages.value.length;
  messages.value.push({ role: 'assistant', content: '' });
  isStreaming.value = true;
  streamingStatus.value = 'AI 思考中...';

  try {
    const token = getToken();
    const baseUrl = getServiceBaseURL();
    const response = await fetch(`${baseUrl}/api/ai/chat/stream`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({
        conversationId: conversationId.value || null,
        message: messageText
      })
    });

    if (!response.ok) {
      const errText = await response.text();
      throw new Error(`HTTP ${response.status}: ${errText.slice(0, 200)}`);
    }
    const reader = response.body?.getReader();
    if (!reader) throw new Error('No stream');

    const decoder = new TextDecoder();
    let buffer = '';

    while (true) {
      const { done, value } = await reader.read();
      if (done) break;

      buffer += decoder.decode(value, { stream: true });
      const lines = buffer.split('\n');
      buffer = lines.pop() || '';

      for (const line of lines) {
        if (!line.startsWith('data: ')) continue;
        const data = line.slice(6);
        try {
          const chunk = JSON.parse(data);
          if (chunk.type === 'text') {
            messages.value[aiIdx].content += chunk.content;
            streamingStatus.value = '正在回复...';
            await scrollToBottom();
          } else if (chunk.type === 'tool_progress') {
            messages.value[aiIdx].toolName = chunk.toolName;
            streamingStatus.value = toolNames[chunk.toolName] || `调用工具: ${chunk.toolName}`;
            if (chunk.toolResult) {
              messages.value[aiIdx].toolResult = chunk.toolResult;
              streamingStatus.value = (toolNames[chunk.toolName] || chunk.toolName) + ' ✓';
            } else {
              messages.value[aiIdx].toolResult = undefined;
            }
          } else if (chunk.type === 'done') {
            if (chunk.conversationId) conversationId.value = chunk.conversationId;
          } else if (chunk.type === 'error') {
            messages.value[aiIdx].content = `❌ ${chunk.error}`;
          }
        } catch { /* skip */ }
      }
    }
  } catch (err: any) {
    messages.value[aiIdx].content = `❌ 请求失败: ${err.message}`;
    streamingStatus.value = '';
  } finally {
    isStreaming.value = false;
    streamingStatus.value = '';
    await scrollToBottom();
  }
}

// ========== 键盘 ==========
function onKeydown(e: KeyboardEvent) {
  if (e.key === 'Enter' && !e.shiftKey) {
    e.preventDefault();
    sendMessage();
  }
}

// ========== 服务地址 ==========
function getServiceBaseURL(): string {
  const env = (import.meta as any).env;
  const base = env?.VITE_SERVICE_BASE_URL || '';
  if (base) return base.replace(/\/$/, '');
  if (env?.DEV) return '';
  return window.location.origin;
}
</script>

<template>
  <div class="global-chat-widget">
    <!-- 悬浮按钮 -->
    <button v-if="!isOpen" class="gc-fab" @click="openPanel" title="AI 助手">
      <span class="gc-fab-icon">💬</span>
    </button>

    <!-- 对话面板 -->
    <div v-if="isOpen" class="gc-panel">
      <!-- 头部 -->
      <div class="gc-header">
        <span class="gc-title">🤖 AI 助手</span>
        <div class="gc-header-actions">
          <button class="gc-btn-text" @click="newChat" title="新对话">+</button>
          <button class="gc-btn-text" @click="closePanel" title="关闭">✕</button>
        </div>
      </div>

      <!-- 状态栏 -->
      <div v-if="streamingStatus" class="gc-status-bar">
        <span class="gc-status-dot"></span>
        {{ streamingStatus }}
      </div>

      <!-- 消息区 -->
      <div ref="chatBody" class="gc-body">
        <div v-if="messages.length === 0" class="gc-empty">
          <div style="font-size:36px;margin-bottom:8px;">💬</div>
          <div>有什么可以帮你的？</div>
          <div style="font-size:12px;color:#bbb;margin-top:4px;">矿山安全 · 三违 · 隐患 · 人员查询</div>
          <div style="font-size:12px;color:#bbb;">上传 Excel 快速录入三违数据</div>
        </div>

        <div v-for="(msg, i) in messages" :key="i" :class="['gc-msg', msg.role]">
          <div class="gc-msg-bubble">
            <div v-if="msg.toolName" class="gc-tool-hint">
              🔧 {{ getToolLabel(msg.toolName) }}
              <span v-if="msg.toolResult" class="gc-tool-done">✓</span>
              <span v-else-if="isStreaming && i === messages.length - 1" class="gc-tool-loading">执行中...</span>
            </div>
            <div v-if="msg.toolResult" class="gc-tool-result">{{ msg.toolResult }}</div>

            <!-- Excel 预览卡片 -->
            <div v-if="msg.excelData" class="gc-excel-card">
              <div class="gc-excel-info">
                📊 {{ msg.excelData.fileName }} — {{ msg.excelData.rowCount }} 行 {{ msg.excelData.headers.length }} 列
              </div>
              <div class="gc-excel-table-wrap">
                <table class="gc-excel-table">
                  <thead>
                    <tr>
                      <th v-for="h in msg.excelData.headers" :key="h">{{ h }}</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="(row, ri) in msg.excelData.rows.slice(0, 5)" :key="ri">
                      <td v-for="h in msg.excelData.headers" :key="h">{{ row[h] || '' }}</td>
                    </tr>
                  </tbody>
                </table>
                <div v-if="msg.excelData.rowCount > 5" class="gc-excel-more">
                  ... 共 {{ msg.excelData.rowCount }} 行，仅展示前 5 行
                </div>
              </div>
            </div>

            <div v-if="getMessageText(msg, i)" class="gc-msg-text">{{ getMessageText(msg, i) }}</div>
          </div>
        </div>

        <!-- 上传中 -->
        <div v-if="isUploading" class="gc-msg assistant">
          <div class="gc-msg-bubble">⏳ 正在解析 Excel 文件...</div>
        </div>
      </div>

      <!-- 输入区 -->
      <div class="gc-input-area">
        <!-- 文件上传按钮 -->
        <button class="gc-btn-upload" :disabled="isStreaming || isUploading" @click="triggerUpload" title="上传 Excel">
          {{ isUploading ? '⏳' : '📎' }}
        </button>
        <input
          ref="fileInput"
          type="file"
          accept=".xls,.xlsx"
          style="display:none"
          @change="onFileSelected"
        />
        <textarea
          v-model="inputText"
          :disabled="isStreaming"
          :placeholder="pendingExcel ? '输入指令或直接发送...' : '输入消息，Enter 发送...'"
          rows="1"
          @keydown="onKeydown"
        ></textarea>
        <button
          class="gc-btn-send"
          :disabled="(!inputText.trim() && !pendingExcel) || isStreaming"
          @click="sendMessage"
        >
          {{ isStreaming ? '⏳' : '➤' }}
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.global-chat-widget {
  position: fixed;
  bottom: 24px;
  right: 24px;
  z-index: 9999;
}

/* FAB 悬浮按钮 */
.gc-fab {
  width: 52px;
  height: 52px;
  border-radius: 50%;
  border: none;
  background: #1677ff;
  color: #fff;
  cursor: pointer;
  box-shadow: 0 4px 16px rgba(22,119,255,0.35);
  display: flex;
  align-items: center;
  justify-content: center;
  transition: transform 0.2s, box-shadow 0.2s;
}
.gc-fab:hover {
  transform: scale(1.08);
  box-shadow: 0 6px 20px rgba(22,119,255,0.45);
}
.gc-fab-icon { font-size: 24px; }

/* 面板 */
.gc-panel {
  position: fixed;
  bottom: 88px;
  right: 24px;
  width: 460px;
  height: 600px;
  max-height: calc(100vh - 120px);
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 8px 40px rgba(0,0,0,0.15);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

/* 头部 */
.gc-header {
  display: flex;
  align-items: center;
  padding: 12px 16px;
  border-bottom: 1px solid #f0f0f0;
  background: #fafafa;
  border-radius: 12px 12px 0 0;
}
.gc-title { font-size: 15px; font-weight: 600; }
.gc-header-actions { display: flex; gap: 4px; margin-left: auto; }
.gc-btn-text {
  width: 28px; height: 28px;
  border: none; background: none;
  font-size: 16px; cursor: pointer;
  border-radius: 6px; color: #666;
  display: flex; align-items: center; justify-content: center;
}
.gc-btn-text:hover { background: #e8e8e8; }

/* 状态栏 */
.gc-status-bar {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  background: #e6f7ff;
  border-bottom: 1px solid #91d5ff;
  font-size: 13px;
  color: #1677ff;
  font-weight: 500;
}
.gc-status-dot {
  width: 8px; height: 8px;
  border-radius: 50%;
  background: #1677ff;
  animation: gc-pulse 1.2s infinite;
}
@keyframes gc-pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.3; }
}

/* 消息区 */
.gc-body {
  flex: 1;
  overflow-y: auto;
  padding: 12px 16px;
  background: #fafbfc;
}
.gc-body::-webkit-scrollbar { width: 4px; }
.gc-body::-webkit-scrollbar-thumb { background: #ddd; border-radius: 2px; }

.gc-empty {
  text-align: center;
  margin-top: 140px;
  color: #999;
  font-size: 14px;
}

.gc-msg { margin-bottom: 12px; }
.gc-msg.user { display: flex; justify-content: flex-end; }
.gc-msg-bubble {
  max-width: 90%;
  padding: 8px 12px;
  border-radius: 10px;
  font-size: 13px;
  line-height: 1.55;
}
.gc-msg.user .gc-msg-bubble {
  background: #1677ff;
  color: #fff;
  border-bottom-right-radius: 3px;
}
.gc-msg.assistant .gc-msg-bubble {
  background: #fff;
  border: 1px solid #eee;
  border-bottom-left-radius: 3px;
}
.gc-tool-hint {
  font-size: 12px;
  color: #fa8c16;
  margin-bottom: 4px;
  font-style: italic;
}
.gc-tool-loading {
  color: #1677ff;
  font-style: normal;
  margin-left: 4px;
}
.gc-tool-done {
  color: #52c41a;
  font-style: normal;
  margin-left: 4px;
}
.gc-tool-result {
  font-size: 11px;
  color: #666;
  background: #fafafa;
  border-radius: 4px;
  padding: 4px 6px;
  margin-bottom: 4px;
  max-height: 80px;
  overflow: auto;
  white-space: pre-wrap;
  word-break: break-all;
}
.gc-msg-text {
  white-space: pre-wrap;
  word-break: break-word;
}

/* Excel 预览卡片 */
.gc-excel-card {
  margin-bottom: 8px;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  overflow: hidden;
  background: #f8f9fa;
}
.gc-excel-info {
  padding: 6px 10px;
  font-size: 12px;
  font-weight: 600;
  background: #eef2ff;
  color: #1677ff;
}
.gc-excel-table-wrap { overflow-x: auto; padding: 4px; }
.gc-excel-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 11px;
}
.gc-excel-table th, .gc-excel-table td {
  padding: 4px 6px;
  border: 1px solid #e0e0e0;
  text-align: left;
  white-space: nowrap;
  max-width: 120px;
  overflow: hidden;
  text-overflow: ellipsis;
}
.gc-excel-table th { background: #f0f0f0; font-weight: 600; }
.gc-excel-more {
  text-align: center;
  font-size: 11px;
  color: #999;
  padding: 4px;
}

/* 输入区 */
.gc-input-area {
  display: flex;
  gap: 6px;
  padding: 10px 12px;
  border-top: 1px solid #f0f0f0;
  background: #fff;
  align-items: flex-end;
}
.gc-btn-upload {
  width: 34px; height: 34px;
  border: 1px solid #e8e8e8;
  border-radius: 50%;
  background: #fff;
  font-size: 16px;
  cursor: pointer;
  flex-shrink: 0;
  display: flex; align-items: center; justify-content: center;
  transition: border-color 0.2s;
}
.gc-btn-upload:hover:not(:disabled) { border-color: #1677ff; }
.gc-btn-upload:disabled { opacity: 0.5; cursor: not-allowed; }
.gc-input-area textarea {
  flex: 1;
  resize: none;
  border: 1px solid #e8e8e8;
  border-radius: 8px;
  padding: 8px 10px;
  font-size: 13px;
  line-height: 1.4;
  outline: none;
  font-family: inherit;
  background: #fafafa;
}
.gc-input-area textarea:focus { border-color: #1677ff; background: #fff; }
.gc-input-area textarea:disabled { opacity: 0.6; }

.gc-btn-send {
  width: 34px; height: 34px;
  border: none;
  border-radius: 50%;
  background: #1677ff;
  color: #fff;
  font-size: 14px;
  cursor: pointer;
  flex-shrink: 0;
  display: flex; align-items: center; justify-content: center;
}
.gc-btn-send:hover:not(:disabled) { background: #4096ff; }
.gc-btn-send:disabled { background: #d9d9d9; cursor: not-allowed; }
</style>
