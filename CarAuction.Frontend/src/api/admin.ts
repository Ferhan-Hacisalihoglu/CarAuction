import { api } from './axios';
import { AdminStats, ActivityItem } from '@/types/common';

export const adminApi = {
  getStats: async () => {
    const res = await api.get<AdminStats>('/admin/stats');
    return res.data;
  },
  getActivity: async () => {
    const res = await api.get<ActivityItem[]>('/admin/activity');
    return res.data;
  },
};
