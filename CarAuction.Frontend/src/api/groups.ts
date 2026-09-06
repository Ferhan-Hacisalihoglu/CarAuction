import { api } from './axios';
import { Group, CreateGroupRequest, GroupMessage } from '@/types/group';
import { PaginatedResponse, PaginationParams } from '@/types/common';

export const groupsApi = {
  getAll: async () => {
    const res = await api.get<Group[]>('/groups');
    return res.data;
  },
  getMyGroups: async () => {
    const res = await api.get<Group[]>('/groups/my');
    return res.data;
  },
  getById: async (id: number) => {
    const res = await api.get<Group>(`/groups/${id}`);
    return res.data;
  },
  create: async (data: CreateGroupRequest) => {
    const res = await api.post<Group>('/groups', data);
    return res.data;
  },
  join: async (id: number) => {
    const res = await api.post(`/groups/${id}/join`);
    return res.data;
  },
  leave: async (id: number) => {
    const res = await api.post(`/groups/${id}/leave`);
    return res.data;
  },
  getMessages: async (id: number, params?: PaginationParams) => {
    const res = await api.get<PaginatedResponse<GroupMessage> | GroupMessage[]>(`/groups/${id}/messages`, { params });
    // In some backend controllers messages might be returned as an array or paginated object
    if (Array.isArray(res.data)) {
      return { data: res.data, total: res.data.length, page: 1, limit: res.data.length, totalPages: 1 };
    }
    return res.data;
  },
  sendMessage: async (id: number, content: string) => {
    const res = await api.post<GroupMessage>(`/groups/${id}/messages`, { content });
    return res.data;
  },
};
