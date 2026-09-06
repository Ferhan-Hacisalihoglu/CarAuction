import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { rolesApi } from '@/api/roles';
import { LoadingSpinner } from '@/components/common/LoadingSpinner';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Label } from '@/components/ui/Label';
import { Dialog } from '@/components/ui/Dialog';
import { toast } from '@/hooks/useToast';
import { Shield, PlusCircle, Trash2, ArrowLeft, Check, Lock } from 'lucide-react';
import { Link } from 'react-router-dom';

export function AdminRolesPage() {
  const queryClient = useQueryClient();
  const [selectedRoleId, setSelectedRoleId] = useState<number | null>(null);
  const [isCreateRoleOpen, setIsCreateRoleOpen] = useState(false);
  const [newRoleName, setNewRoleName] = useState('');

  // Fetch all roles
  const { data: roles, isLoading: rolesLoading } = useQuery({
    queryKey: ['roles'],
    queryFn: () => rolesApi.getAll(),
  });

  // Fetch all available permissions
  const { data: allPermissions } = useQuery({
    queryKey: ['permissions'],
    queryFn: () => rolesApi.getAllPermissions(),
  });

  // Fetch permissions for selected role
  const { data: rolePermissions, isLoading: rolePermsLoading } = useQuery({
    queryKey: ['role-permissions', selectedRoleId],
    queryFn: () => (selectedRoleId ? rolesApi.getRolePermissions(selectedRoleId) : Promise.resolve([])),
    enabled: selectedRoleId !== null,
  });

  const createRoleMutation = useMutation({
    mutationFn: (name: string) => rolesApi.create(name),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['roles'] });
      toast.success('New role created successfully');
      setIsCreateRoleOpen(false);
      setNewRoleName('');
    },
    onError: () => toast.error('Failed to create role'),
  });

  const deleteRoleMutation = useMutation({
    mutationFn: (id: number) => rolesApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['roles'] });
      toast.success('Role removed');
      setSelectedRoleId(null);
    },
    onError: () => toast.error('Cannot delete role. May still be assigned to active users.'),
  });

  const assignPermMutation = useMutation({
    mutationFn: ({ roleId, permId }: { roleId: number; permId: number }) =>
      rolesApi.assignPermission(roleId, permId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['role-permissions', selectedRoleId] }),
  });

  const removePermMutation = useMutation({
    mutationFn: ({ roleId, permId }: { roleId: number; permId: number }) =>
      rolesApi.removePermission(roleId, permId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['role-permissions', selectedRoleId] }),
  });

  const activePermIds = new Set(rolePermissions?.map((p) => p.id) || []);

  const handleTogglePerm = (permId: number) => {
    if (!selectedRoleId) return;
    if (activePermIds.has(permId)) {
      removePermMutation.mutate({ roleId: selectedRoleId, permId });
    } else {
      assignPermMutation.mutate({ roleId: selectedRoleId, permId });
    }
  };

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
            Roles & Permission Matrix
          </h1>
          <p className="text-xs sm:text-sm text-muted-foreground mt-1">
            Configure system roles and grant granular permission capabilities.
          </p>
        </div>

        <Button variant="primary" className="gap-2" onClick={() => setIsCreateRoleOpen(true)}>
          <PlusCircle className="h-4 w-4" />
          <span>Create Role</span>
        </Button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-12 gap-6">
        {/* Roles List (4 cols) */}
        <div className="md:col-span-5 lg:col-span-4 space-y-3">
          <h3 className="text-xs font-bold uppercase tracking-wider text-muted-foreground">
            System Roles
          </h3>
          {rolesLoading ? (
            <LoadingSpinner size="sm" />
          ) : (
            roles?.map((role) => {
              const isSelected = selectedRoleId === role.id;
              return (
                <Card
                  key={role.id}
                  onClick={() => setSelectedRoleId(role.id)}
                  className={`cursor-pointer transition-all p-4 flex items-center justify-between ${
                    isSelected
                      ? 'border-purple-500/50 bg-purple-500/10 shadow-lg shadow-purple-500/10'
                      : 'glass-card hover:border-white/20'
                  }`}
                >
                  <div className="flex items-center gap-3">
                    <Shield className={`h-5 w-5 ${isSelected ? 'text-purple-400' : 'text-muted-foreground'}`} />
                    <span className="font-bold text-sm text-foreground">{role.name}</span>
                  </div>

                  {role.name !== 'Admin' && role.name !== 'User' && (
                    <Button
                      variant="ghost"
                      size="sm"
                      className="text-rose-400 hover:text-rose-300 p-1 h-auto"
                      onClick={(e) => {
                        e.stopPropagation();
                        deleteRoleMutation.mutate(role.id);
                      }}
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  )}
                </Card>
              );
            })
          )}
        </div>

        {/* Permissions Assignment Matrix (8 cols) */}
        <div className="md:col-span-7 lg:col-span-8 space-y-3">
          <h3 className="text-xs font-bold uppercase tracking-wider text-muted-foreground">
            Permissions for Selected Role
          </h3>

          {!selectedRoleId ? (
            <Card className="glass-panel border-white/10 p-12 text-center text-xs text-muted-foreground">
              Select a role from the left list to view and toggle its assigned permissions.
            </Card>
          ) : rolePermsLoading ? (
            <LoadingSpinner size="md" message="Loading permission matrix..." />
          ) : (
            <Card className="glass-panel border-white/10 p-5 space-y-4">
              <div className="grid grid-cols-1 gap-2.5">
                {allPermissions?.map((perm) => {
                  const isAssigned = activePermIds.has(perm.id);
                  return (
                    <div
                      key={perm.id}
                      onClick={() => handleTogglePerm(perm.id)}
                      className={`flex items-center justify-between p-3.5 rounded-xl border cursor-pointer transition-colors ${
                        isAssigned
                          ? 'border-emerald-500/40 bg-emerald-500/10'
                          : 'border-white/5 bg-secondary/30 hover:bg-secondary/50'
                      }`}
                    >
                      <div>
                        <div className="font-semibold text-xs text-foreground flex items-center gap-2">
                          <Lock className="h-3.5 w-3.5 text-muted-foreground" />
                          <span>{perm.name}</span>
                        </div>
                        <p className="text-[11px] text-muted-foreground mt-0.5">{perm.description}</p>
                      </div>

                      <div
                        className={`h-6 w-6 rounded-lg flex items-center justify-center border transition-colors ${
                          isAssigned
                            ? 'bg-emerald-500 border-emerald-400 text-black font-bold'
                            : 'border-border'
                        }`}
                      >
                        {isAssigned && <Check className="h-4 w-4" />}
                      </div>
                    </div>
                  );
                })}
              </div>
            </Card>
          )}
        </div>
      </div>

      {/* Create Role Modal */}
      <Dialog
        isOpen={isCreateRoleOpen}
        onClose={() => setIsCreateRoleOpen(false)}
        title="Create New System Role"
      >
        <form
          onSubmit={(e) => {
            e.preventDefault();
            if (newRoleName.trim()) createRoleMutation.mutate(newRoleName.trim());
          }}
          className="space-y-4 mt-4"
        >
          <div className="space-y-1">
            <Label htmlFor="roleName">Role Identifier</Label>
            <Input
              id="roleName"
              placeholder="e.g. Moderator, Auditor, VIP"
              value={newRoleName}
              onChange={(e) => setNewRoleName(e.target.value)}
              required
            />
          </div>

          <div className="flex justify-end gap-3 pt-2">
            <Button type="button" variant="outline" onClick={() => setIsCreateRoleOpen(false)}>
              Cancel
            </Button>
            <Button type="submit" variant="primary" isLoading={createRoleMutation.isPending}>
              Create Role
            </Button>
          </div>
        </form>
      </Dialog>
    </div>
  );
}
