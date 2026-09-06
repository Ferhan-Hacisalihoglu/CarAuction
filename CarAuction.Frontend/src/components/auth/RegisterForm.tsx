import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link, useNavigate } from 'react-router-dom';
import { useAuthStore } from '@/stores/authStore';
import { registerSchema } from '@/utils/validators';
import { RegisterRequest } from '@/types/auth';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { Label } from '@/components/ui/Label';
import { Card, CardHeader, CardTitle, CardDescription, CardContent, CardFooter } from '@/components/ui/Card';
import { toast } from '@/hooks/useToast';
import { Mail, Lock, User, UserPlus } from 'lucide-react';

interface RegisterFormData extends RegisterRequest {
  confirmPassword: string;
}

export function RegisterForm() {
  const [isLoading, setIsLoading] = useState(false);
  const { register: registerAuth } = useAuthStore();
  const navigate = useNavigate();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
  });

  const onSubmit = async (data: RegisterFormData) => {
    setIsLoading(true);
    try {
      await registerAuth({
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        password: data.password,
      });
      toast.success('Your CarAuction account has been created!', 'Registration Complete');
      navigate('/');
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Registration failed. Email might already be taken.';
      toast.error(msg, 'Registration Error');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Card className="border-white/10 shadow-2xl backdrop-blur-2xl">
      <CardHeader className="space-y-1 text-center">
        <CardTitle className="text-2xl font-bold">Create Account</CardTitle>
        <CardDescription>Join CarAuction to bid on live auctions and list vehicles</CardDescription>
      </CardHeader>
      <form onSubmit={handleSubmit(onSubmit)}>
        <CardContent className="space-y-3.5">
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1">
              <Label htmlFor="firstName">First Name</Label>
              <Input
                id="firstName"
                placeholder="Marcus"
                icon={<User className="h-4 w-4" />}
                {...register('firstName')}
              />
              {errors.firstName && <p className="text-[11px] text-rose-400 mt-0.5">{errors.firstName.message}</p>}
            </div>

            <div className="space-y-1">
              <Label htmlFor="lastName">Last Name</Label>
              <Input
                id="lastName"
                placeholder="Vance"
                icon={<User className="h-4 w-4" />}
                {...register('lastName')}
              />
              {errors.lastName && <p className="text-[11px] text-rose-400 mt-0.5">{errors.lastName.message}</p>}
            </div>
          </div>

          <div className="space-y-1">
            <Label htmlFor="email">Email Address</Label>
            <Input
              id="email"
              type="email"
              placeholder="marcus@example.com"
              icon={<Mail className="h-4 w-4" />}
              {...register('email')}
            />
            {errors.email && <p className="text-[11px] text-rose-400 mt-0.5">{errors.email.message}</p>}
          </div>

          <div className="space-y-1">
            <Label htmlFor="password">Password</Label>
            <Input
              id="password"
              type="password"
              placeholder="••••••••"
              icon={<Lock className="h-4 w-4" />}
              {...register('password')}
            />
            {errors.password && <p className="text-[11px] text-rose-400 mt-0.5">{errors.password.message}</p>}
          </div>

          <div className="space-y-1">
            <Label htmlFor="confirmPassword">Confirm Password</Label>
            <Input
              id="confirmPassword"
              type="password"
              placeholder="••••••••"
              icon={<Lock className="h-4 w-4" />}
              {...register('confirmPassword')}
            />
            {errors.confirmPassword && (
              <p className="text-[11px] text-rose-400 mt-0.5">{errors.confirmPassword.message}</p>
            )}
          </div>
        </CardContent>

        <CardFooter className="flex flex-col space-y-4 pt-2">
          <Button type="submit" variant="primary" className="w-full gap-2" isLoading={isLoading}>
            <UserPlus className="h-4 w-4" />
            <span>Create Account</span>
          </Button>

          <p className="text-center text-xs text-muted-foreground">
            Already have an account?{' '}
            <Link to="/login" className="font-semibold text-brand-400 hover:text-brand-300 transition-colors">
              Sign In
            </Link>
          </p>
        </CardFooter>
      </form>
    </Card>
  );
}
