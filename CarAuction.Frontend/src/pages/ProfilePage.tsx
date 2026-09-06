import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useAuth } from '@/hooks/useAuth';
import { usersApi } from '@/api/users';
import { profileSchema, changePasswordSchema } from '@/utils/validators';
import { UpdateProfileRequest } from '@/types/user';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { Label } from '@/components/ui/Label';
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui/Card';
import { Avatar } from '@/components/ui/Avatar';
import { toast } from '@/hooks/useToast';
import { User, Lock, ShieldCheck } from 'lucide-react';

interface ChangePasswordFormData {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

export function ProfilePage() {
  const { user, updateUser } = useAuth();
  const [isUpdatingProfile, setIsUpdatingProfile] = useState(false);
  const [isUpdatingPassword, setIsUpdatingPassword] = useState(false);

  // Profile Form
  const {
    register: regProfile,
    handleSubmit: handleProfileSubmit,
    formState: { errors: profileErrors },
  } = useForm<UpdateProfileRequest>({
    resolver: zodResolver(profileSchema),
    defaultValues: {
      firstName: user?.firstName || '',
      lastName: user?.lastName || '',
    },
  });

  // Password Form
  const {
    register: regPass,
    handleSubmit: handlePassSubmit,
    reset: resetPass,
    formState: { errors: passErrors },
  } = useForm<ChangePasswordFormData>({
    resolver: zodResolver(changePasswordSchema),
  });

  const onProfileSubmit = async (data: UpdateProfileRequest) => {
    setIsUpdatingProfile(true);
    try {
      const updated = await usersApi.updateProfile(data);
      updateUser(updated);
      toast.success('Your profile has been updated.', 'Profile Saved');
    } catch {
      toast.error('Failed to update profile');
    } finally {
      setIsUpdatingProfile(false);
    }
  };

  const onPassSubmit = async (data: ChangePasswordFormData) => {
    setIsUpdatingPassword(true);
    try {
      await usersApi.changePassword({
        currentPassword: data.currentPassword,
        newPassword: data.newPassword,
      });
      toast.success('Password changed successfully.', 'Security Updated');
      resetPass();
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Failed to update password');
    } finally {
      setIsUpdatingPassword(false);
    }
  };

  return (
    <div className="container mx-auto max-w-4xl px-4 py-10 sm:px-6 lg:px-8 space-y-8">
      <div>
        <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-brand-400">
          <User className="h-4 w-4" />
          <span>Account Center</span>
        </div>
        <h1 className="text-3xl font-black text-foreground tracking-tight mt-1 font-display">
          Personal Profile & Security
        </h1>
        <p className="text-xs sm:text-sm text-muted-foreground mt-1">
          Manage your personal details, email credentials, and cryptographic security.
        </p>
      </div>

      {/* User Card */}
      <Card className="glass-panel border-white/10 p-6 flex flex-col sm:flex-row items-center sm:items-start gap-6 text-center sm:text-left">
        <Avatar
          name={`${user?.firstName} ${user?.lastName}`}
          size="xl"
          isOnline
        />
        <div className="space-y-1 min-w-0 flex-1">
          <h2 className="text-2xl font-bold text-foreground">
            {user?.firstName} {user?.lastName}
          </h2>
          <p className="text-sm text-muted-foreground">{user?.email}</p>
          <div className="pt-2 flex flex-wrap items-center justify-center sm:justify-start gap-2">
            {user?.roleName && (
              <span className="px-3 py-1 rounded-full text-xs font-bold bg-brand-500/20 text-brand-400 border border-brand-500/30 uppercase">
                {user.roleName}
              </span>
            )}
            <span className="px-3 py-1 rounded-full text-xs font-semibold bg-emerald-500/10 text-emerald-400 border border-emerald-500/20 flex items-center gap-1">
              <ShieldCheck className="h-3.5 w-3.5" />
              <span>Verified Account</span>
            </span>
          </div>
        </div>
      </Card>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
        {/* Profile Edit Card */}
        <Card className="glass-card p-6 space-y-6">
          <CardHeader className="p-0 pb-4 border-b border-white/5">
            <CardTitle className="text-lg flex items-center gap-2">
              <User className="h-5 w-5 text-brand-400" />
              <span>Personal Information</span>
            </CardTitle>
            <CardDescription>Update your public seller / bidder display name</CardDescription>
          </CardHeader>

          <form onSubmit={handleProfileSubmit(onProfileSubmit)} className="space-y-4">
            <div className="space-y-1">
              <Label htmlFor="firstName">First Name</Label>
              <Input id="firstName" {...regProfile('firstName')} />
              {profileErrors.firstName && (
                <p className="text-xs text-rose-400 mt-1">{profileErrors.firstName.message}</p>
              )}
            </div>

            <div className="space-y-1">
              <Label htmlFor="lastName">Last Name</Label>
              <Input id="lastName" {...regProfile('lastName')} />
              {profileErrors.lastName && (
                <p className="text-xs text-rose-400 mt-1">{profileErrors.lastName.message}</p>
              )}
            </div>

            <Button type="submit" variant="primary" className="w-full" isLoading={isUpdatingProfile}>
              Save Name
            </Button>
          </form>
        </Card>

        {/* Change Password Card */}
        <Card className="glass-card p-6 space-y-6">
          <CardHeader className="p-0 pb-4 border-b border-white/5">
            <CardTitle className="text-lg flex items-center gap-2">
              <Lock className="h-5 w-5 text-amber-400" />
              <span>Change Password</span>
            </CardTitle>
            <CardDescription>Update your account authentication secret</CardDescription>
          </CardHeader>

          <form onSubmit={handlePassSubmit(onPassSubmit)} className="space-y-4">
            <div className="space-y-1">
              <Label htmlFor="currentPassword">Current Password</Label>
              <Input
                id="currentPassword"
                type="password"
                placeholder="••••••••"
                {...regPass('currentPassword')}
              />
              {passErrors.currentPassword && (
                <p className="text-xs text-rose-400 mt-1">{passErrors.currentPassword.message}</p>
              )}
            </div>

            <div className="space-y-1">
              <Label htmlFor="newPassword">New Password</Label>
              <Input
                id="newPassword"
                type="password"
                placeholder="••••••••"
                {...regPass('newPassword')}
              />
              {passErrors.newPassword && (
                <p className="text-xs text-rose-400 mt-1">{passErrors.newPassword.message}</p>
              )}
            </div>

            <div className="space-y-1">
              <Label htmlFor="confirmNewPassword">Confirm New Password</Label>
              <Input
                id="confirmNewPassword"
                type="password"
                placeholder="••••••••"
                {...regPass('confirmNewPassword')}
              />
              {passErrors.confirmNewPassword && (
                <p className="text-xs text-rose-400 mt-1">{passErrors.confirmNewPassword.message}</p>
              )}
            </div>

            <Button type="submit" variant="secondary" className="w-full" isLoading={isUpdatingPassword}>
              Update Password
            </Button>
          </form>
        </Card>
      </div>
    </div>
  );
}
