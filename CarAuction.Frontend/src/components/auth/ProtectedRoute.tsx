import { Navigate, Outlet } from 'react-router-dom';
import { useAuthStore } from '@/stores/authStore';

export function ProtectedRoute({ role }: { role?: string }) {
  const { isAuthenticated, user } = useAuthStore();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (role && user?.roleName?.toLowerCase() !== role.toLowerCase()) {
    return <Navigate to="/" replace />;
  }

  return <Outlet />;
}
