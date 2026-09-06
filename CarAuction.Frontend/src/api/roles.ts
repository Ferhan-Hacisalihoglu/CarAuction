import { api } from './axios';
import { Role, Permission } from '@/types/user';

export const rolesApi = {
  getAll: async () => {
    const res = await api.get<Role[]>('/roles');
    return res.data;
  },
  create: async (name: string) => {
    const res = await api.post<Role>('/roles', { name });
    return res.data;
  },
  delete: async (id: number) => {
    const res = await api.delete(`/roles/${id}`);
    return res.data;
  },
  getAllPermissions: async () => {
    const res = await api.get<Permission[]>('/permissions');
    return res.data;
  },
  getRolePermissions: async (roleId: number) => {
    const res = await api.get<Permission[]>(`/roles/${roleId}/permissions`);
    return res.data;
  },
  assignPermission: async (roleId: number, permissionId: number) => {
    const res = await api.post(`/roles/${roleId}/permissions`, { permissionId });
    return res.data;
  },
  removePermission: async (roleId: number, permissionId: number) => {
    const res = await api.delete(`/roles/${roleId}/permissions/${permissionId}`);
    return res.data;
  },
};
