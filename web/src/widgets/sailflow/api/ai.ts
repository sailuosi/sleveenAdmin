/**
 * AI 模型配置 · api/Wf2Ai（经本机 Web.Api · requestAPI6）
 * 路由类名 Wf2Ai；文档契约名 Wf2AI（大小写不敏感）
 */
import request from '@/utils/requestAPI6.js';

export type AiModelRow = {
  Id?: number;
  ModelProvider?: string;
  ModelName?: string;
  ModelType?: string;
  Description?: string;
  BaseUrl?: string;
  ApiUUID?: string;
  ApiKey?: string;
  IsActive?: boolean;
  CreatedDateTime?: string;
  UpdatedDateTime?: string;
};

export type KbDocumentRow = {
  Id?: number;
  Content?: string;
  Metadata?: string;
  Embedding?: string;
  IndustryId?: number | null;
  CreatedAt?: string;
};

export function getModelList() {
  return request({ url: '/api/Wf2Ai/GetModelList', method: 'get' });
}

export function getModel(id: number) {
  return request({ url: `/api/Wf2Ai/GetModel/${id}`, method: 'get' });
}

export function saveModel(entity: AiModelRow) {
  return request({
    url: '/api/Wf2Ai/SaveModel',
    method: 'post',
    data: entity
  });
}

export function deleteModel(id: number) {
  return request({
    url: `/api/Wf2Ai/DeleteModel/${id}`,
    method: 'get'
  });
}

export function testModelConnection(payload: {
  BaseUrl: string;
  ApiKey: string;
  ModelProvider: string;
  ApiUUID?: string | null;
}) {
  return request({
    url: '/api/Wf2Ai/TestModelConnection',
    method: 'post',
    data: payload
  });
}

export type AxConfigRow = {
  Id?: number;
  ProcessId?: string;
  Version?: string;
  ActivityId?: string;
  ServiceType?: string;
  ModelProviderId?: number | null;
  ModelName?: string;
  Description?: string;
  Temperature?: number;
  MaxTokens?: number;
  SystemPrompt?: string;
  UserMessage?: string;
  ResponseFormat?: string;
  Timeout?: number;
  MaxRetries?: number;
  ErrorHandling?: string;
  FallbackAgent?: string;
  LogLevel?: string;
  CustomInstructions?: string;
  RagSearchStrategy?: string;
  RagSearchCount?: number | null;
  RagSimilarityThreshold?: number | null;
  RagSearchMode?: string;
  RagFunction?: string;
  RagEmbeddingModelId?: number | null;
  RagEmbeddingDimensions?: number | null;
  MemoryTurns?: number;
};

export type KbDocumentRow = {
  Id?: number;
  Content?: string;
  Metadata?: string;
  IndustryId?: number | null;
  Embedding?: string;
};

export function getAxConfig(processId: string, version: string, activityId: string) {
  return request({
    url: '/api/Wf2Ai/GetAxConfig',
    method: 'get',
    params: { processId, version, activityId }
  });
}

export function saveAxConfig(entity: AxConfigRow) {
  return request({
    url: '/api/Wf2Ai/SaveAxConfig',
    method: 'post',
    data: entity
  });
}

export function deleteAxConfig(processId: string, version: string, activityId: string) {
  return request({
    url: '/api/Wf2Ai/DeleteAxConfig',
    method: 'delete',
    params: { processId, version, activityId }
  });
}

export function getModelListByType(modelType: string) {
  return request({
    url: '/api/Wf2Ai/GetModelListByType',
    method: 'get',
    params: { modelType }
  });
}

export function getKnowledgeBaseDocuments() {
  return request({
    url: '/api/Wf2Ai/GetKnowledgeBaseDocuments',
    method: 'get'
  });
}

export function searchKnowledgeBase(payload: {
  Query: string;
  Threshold?: number;
  Count?: number;
}) {
  return request({
    url: '/api/Wf2Ai/SearchKnowledgeBase',
    method: 'post',
    data: payload
  });
}

export function saveKnowledgeBaseDocument(payload: {
  Id?: number;
  Content: string;
  Metadata?: string;
  IndustryId?: number | null;
}) {
  return request({
    url: '/api/Wf2Ai/SaveKnowledgeBaseDocument',
    method: 'post',
    data: payload
  });
}

export function deleteKnowledgeBaseDocument(id: number | string) {
  return request({
    url: `/api/Wf2Ai/DeleteKnowledgeBaseDocument/${id}`,
    method: 'post'
  });
}

export function unwrapEntity<T>(data: any): T | null {
  if (!data) return null;
  return (data.Entity ?? data.entity ?? data) as T;
}

export function unwrapList<T>(data: any): T[] {
  if (Array.isArray(data)) return data;
  const entity = data?.Entity ?? data?.entity ?? data;
  return Array.isArray(entity) ? entity : [];
}
