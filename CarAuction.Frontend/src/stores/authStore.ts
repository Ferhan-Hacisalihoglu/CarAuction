import { create } from 'zustand';
import { User } from '@/types/user';
import { LoginRequest, RegisterRequest } from '@/types/auth';
import { authApi } from '@/api/auth';

interface AuthState {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  login: (data: LoginRequest) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
  refreshAccessToken: () => Promise<string>;
  updateUser: (user: User) => void;
  initializeAuth: () => void;
}

const STORAGE_KEY = 'carauction_auth';

export const useAuthStore = create<AuthState>((set, get) => ({
  user: null,
  accessToken: null,
  refreshToken: null,
  isAuthenticated: false,

  initializeAuth: () => {
    try {
      const stored = localStorage.getItem(STORAGE_KEY);
      if (stored) {
        const parsed = JSON.parse(stored);
        if (parsed.accessToken && parsed.user) {
          set({
            user: parsed.user,
            accessToken: parsed.accessToken,
            refreshToken: parsed.refreshToken,
            isAuthenticated: true,
          });
        }
      }
    } catch (e) {
      console.error('Failed to restore auth session:', e);
    }
  },

  login: async (data: LoginRequest) => {
    const res = await authApi.login(data);
    const authData = {
      user: res.user,
      accessToken: res.accessToken,
      refreshToken: res.refreshToken,
      isAuthenticated: true,
    };
    localStorage.setItem(STORAGE_KEY, JSON.stringify(authData));
    set(authData);
  },

  register: async (data: RegisterRequest) => {
    const res = await authApi.register(data);
    const authData = {
      user: res.user,
      accessToken: res.accessToken,
      refreshToken: res.refreshToken,
      isAuthenticated: true,
    };
    localStorage.setItem(STORAGE_KEY, JSON.stringify(authData));
    set(authData);
  },

  logout: async () => {
    try {
      await authApi.logout();
    } catch {
      // Ignore errors on logout
    }
    localStorage.removeItem(STORAGE_KEY);
    set({
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,
    });
  },

  refreshAccessToken: async () => {
    const currentRefreshToken = get().refreshToken;
    if (!currentRefreshToken) {
      throw new Error('No refresh token available');
    }
    const res = await authApi.refreshToken(currentRefreshToken);
    const updated = {
      ...get(),
      accessToken: res.accessToken,
      refreshToken: res.refreshToken,
      user: res.user || get().user,
    };
    localStorage.setItem(STORAGE_KEY, JSON.stringify(updated));
    set({
      accessToken: res.accessToken,
      refreshToken: res.refreshToken,
      user: res.user || get().user,
    });
    return res.accessToken;
  },

  updateUser: (user: User) => {
    set({ user });
    const current = get();
    localStorage.setItem(
      STORAGE_KEY,
      JSON.stringify({
        user,
        accessToken: current.accessToken,
        refreshToken: current.refreshToken,
        isAuthenticated: current.isAuthenticated,
      })
    );
  },
}));
