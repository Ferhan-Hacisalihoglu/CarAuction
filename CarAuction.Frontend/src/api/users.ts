import { api } from './axios';
import { User, UpdateProfileRequest, UpdateUserStatusRequest, UpdateUserRoleRequest } from '@/types/user';
import { PaginatedResponse, PaginationParams } from '@/types/common';

export const usersApi = {
  getAll: async (params?: PaginationParams & { search?: string }) => {
    const res = await api.get<PaginatedResponse<User>>('/users', { params });
    return res.data;
  },
  getById: async (id: number) => {
    const res = await api.get<User>(`/users/${id}`);
    return res.data;
  },
  updateProfile: async (data: UpdateProfileRequest) => {
    const res = await api.put<User>('/users/profile', data);
    return res.data;
  },
  changePassword: async (data: { currentPassword: string; newPassword: string }) => {
    const res = await api.post('/users/change-password', data);
    return res.data;
  },
  updateStatus: async (id: number, data: UpdateUserStatusRequest) => {
    const res = await api.put(`/users/${id}/status`, data);
    return res.data;
  },
  updateRole: async (id: number, data: UpdateUserRoleRequest) => {
    const res = await api.put(`/users/${id}/role`, data);
    return res.data;
  },
};
