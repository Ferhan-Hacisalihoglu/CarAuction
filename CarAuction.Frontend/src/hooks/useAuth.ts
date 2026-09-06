import { useAuthStore } from '@/stores/authStore';

export function useAuth() {
  const { user, accessToken, isAuthenticated, login, register, logout, updateUser } = useAuthStore();

  const isAdmin = user?.roleName?.toLowerCase() === 'admin';

  return {
    user,
    accessToken,
    isAuthenticated,
    isAdmin,
    login,
    register,
    logout,
    updateUser,
  };
}
