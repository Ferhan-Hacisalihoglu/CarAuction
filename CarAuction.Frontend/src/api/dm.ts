import { api } from './axios';
import { Conversation, DmMessage, StartConversationRequest } from '@/types/message';
import { PaginatedResponse, PaginationParams } from '@/types/common';

export const dmApi = {
  getConversations: async () => {
    const res = await api.get<Conversation[]>('/dm');
    return res.data;
  },
  startConversation: async (data: StartConversationRequest) => {
    const res = await api.post<Conversation>('/dm/start', data);
    return res.data;
  },
  getMessages: async (conversationId: number, params?: PaginationParams) => {
    const res = await api.get<PaginatedResponse<DmMessage> | DmMessage[]>(`/dm/${conversationId}/messages`, { params });
    if (Array.isArray(res.data)) {
      return { data: res.data, total: res.data.length, page: 1, limit: res.data.length, totalPages: 1 };
    }
    return res.data;
  },
  sendMessage: async (conversationId: number, content: string) => {
    const res = await api.post<DmMessage>(`/dm/${conversationId}/messages`, { content });
    return res.data;
  },
  markAsRead: async (conversationId: number) => {
    const res = await api.put(`/dm/${conversationId}/read`);
    return res.data;
  },
};
