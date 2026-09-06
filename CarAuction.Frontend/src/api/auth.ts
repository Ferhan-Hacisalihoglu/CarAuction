import { api } from './axios';
import { AuthResponse, LoginRequest, RegisterRequest } from '@/types/auth';
import { User } from '@/types/user';

export const authApi = {
  login: async (data: LoginRequest) => {
    const res = await api.post<AuthResponse>('/auth/login', data);
    return res.data;
  },
  register: async (data: RegisterRequest) => {
    const res = await api.post<AuthResponse>('/auth/register', data);
    return res.data;
  },
  refreshToken: async (token: string) => {
    const res = await api.post<AuthResponse>('/auth/refresh-token', { token });
    return res.data;
  },
  logout: async () => {
    const res = await api.post('/auth/logout');
    return res.data;
  },
  me: async () => {
    const res = await api.get<User>('/auth/me');
    return res.data;
  },
};
