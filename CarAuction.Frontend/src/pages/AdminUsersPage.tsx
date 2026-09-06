import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { usersApi } from '@/api/users';
import { rolesApi } from '@/api/roles';
import { useDebounce } from '@/hooks/useDebounce';
import { SearchInput } from '@/components/common/SearchInput';
import { LoadingSpinner } from '@/components/common/LoadingSpinner';
import { Pagination } from '@/components/common/Pagination';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { Avatar } from '@/components/ui/Avatar';
import { toast } from '@/hooks/useToast';
import { Users, Shield, ArrowLeft } from 'lucide-react';
import { Link } from 'react-router-dom';

export function AdminUsersPage() {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const debouncedSearch = useDebounce(search, 300);

  const { data: usersData, isLoading } = useQuery({
    queryKey: ['admin-users', page, debouncedSearch],
    queryFn: () => usersApi.getAll({ page, limit: 15, search: debouncedSearch || undefined }),
  });

  const { data: roles } = useQuery({
    queryKey: ['roles'],
    queryFn: () => rolesApi.getAll(),
  });

  const updateStatusMutation = useMutation({
    mutationFn: ({ id, isActive }: { id: number; isActive: boolean }) =>
      usersApi.updateStatus(id, { isActive }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-users'] });
      toast.success('User status updated');
    },
    onError: () => toast.error('Failed to update status'),
  });

  const updateRoleMutation = useMutation({
    mutationFn: ({ id, roleId }: { id: number; roleId: number }) =>
      usersApi.updateRole(id, { roleId }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-users'] });
      toast.success('User role updated');
    },
    onError: () => toast.error('Failed to update role'),
  });

  const users = usersData?.data || [];

  return (
    <div className="container mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8 space-y-8">
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
        <div>
          <Link
            to="/admin"
            className="inline-flex items-center gap-1.5 text-xs text-muted-foreground hover:text-foreground mb-2"
          >
            <ArrowLeft className="h-3.5 w-3.5" />
            <span>Admin Dashboard</span>
          </Link>
          <h1 className="text-3xl font-black text-foreground tracking-tight font-display">
            User Moderation & Access Control
          </h1>
          <p className="text-xs sm:text-sm text-muted-foreground mt-1">
            Activate/deactivate user accounts and modify RBAC role assignments.
          </p>
        </div>

        <div className="w-full sm:w-80">
          <SearchInput
            value={search}
            onChange={(val) => {
              setSearch(val);
              setPage(1);
            }}
            placeholder="Search by name or email..."
          />
        </div>
      </div>

      <Card className="glass-panel border-white/10 overflow-hidden">
        <CardContent className="p-0">
          {isLoading ? (
            <LoadingSpinner size="lg" message="Loading users..." />
          ) : users.length === 0 ? (
            <div className="p-12 text-center text-xs text-muted-foreground">No users match your search.</div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs border-collapse">
                <thead>
                  <tr className="border-b border-white/10 bg-secondary/40 text-muted-foreground">
                    <th className="p-4 font-semibold">User</th>
                    <th className="p-4 font-semibold">Email</th>
                    <th className="p-4 font-semibold">Status</th>
                    <th className="p-4 font-semibold">Role</th>
                    <th className="p-4 font-semibold text-right">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-white/5">
                  {users.map((u) => (
                    <tr key={u.id} className="hover:bg-white/5 transition-colors">
                      <td className="p-4">
                        <div className="flex items-center gap-3">
                          <Avatar name={`${u.firstName} ${u.lastName}`} size="sm" />
                          <div className="font-semibold text-foreground">
                            {u.firstName} {u.lastName}
                          </div>
                        </div>
                      </td>

                      <td className="p-4 text-muted-foreground font-mono">{u.email}</td>

                      <td className="p-4">
                        <Badge variant={u.isActive !== false ? 'success' : 'destructive'}>
                          {u.isActive !== false ? 'Active' : 'Suspended'}
                        </Badge>
                      </td>

                      <td className="p-4">
                        <select
                          value={u.roleId || ''}
                          onChange={(e) =>
                            updateRoleMutation.mutate({
                              id: u.id,
                              roleId: Number(e.target.value),
                            })
                          }
                          className="h-8 rounded-lg border border-border bg-background/60 px-2 text-xs font-semibold focus-visible:outline-none"
                        >
                          <option value="">No Role</option>
                          {roles?.map((r) => (
                            <option key={r.id} value={r.id}>
                              {r.name}
                            </option>
                          ))}
                        </select>
                      </td>

                      <td className="p-4 text-right">
                        <Button
                          variant={u.isActive !== false ? 'destructive' : 'primary'}
                          size="sm"
                          className="text-xs h-8"
                          onClick={() =>
                            updateStatusMutation.mutate({
                              id: u.id,
                              isActive: u.isActive === false ? true : false,
                            })
                          }
                        >
                          {u.isActive !== false ? 'Suspend' : 'Activate'}
                        </Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </CardContent>
      </Card>

      {usersData && (
        <Pagination
          currentPage={page}
          totalPages={usersData.totalPages || Math.ceil((usersData.total || 0) / 15)}
          onPageChange={setPage}
        />
      )}
    </div>
  );
}
