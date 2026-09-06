import { useParams, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { usersApi } from '@/api/users';
import { useStartConversation } from '@/hooks/useDirectMessages';
import { useAuth } from '@/hooks/useAuth';
import { LoadingSpinner } from '@/components/common/LoadingSpinner';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Avatar } from '@/components/ui/Avatar';
import { formatDate } from '@/utils/format';
import { toast } from '@/hooks/useToast';
import { User, MessageSquare, ArrowLeft, Calendar, ShieldCheck } from 'lucide-react';

export function UserProfilePage() {
  const { id } = useParams<{ id: string }>();
  const userId = Number(id);
  const navigate = useNavigate();
  const { user: currentUser, isAuthenticated } = useAuth();

  const { data: user, isLoading, isError } = useQuery({
    queryKey: ['user-public', userId],
    queryFn: () => usersApi.getById(userId),
    enabled: !isNaN(userId) && userId > 0,
  });

  const startConvMutation = useStartConversation();

  const handleContact = async () => {
    if (!isAuthenticated) {
      toast.error('Please log in to contact this user');
      navigate('/login');
      return;
    }
    if (currentUser?.id === userId) {
      toast.info('You cannot send direct messages to yourself');
      return;
    }

    try {
      const conv = await startConvMutation.mutateAsync(userId);
      navigate(`/messages/${conv.id}`);
    } catch {
      toast.error('Failed to start chat');
    }
  };

  if (isLoading) {
    return <LoadingSpinner size="lg" message="Loading member profile..." className="min-h-[50vh]" />;
  }

  if (isError || !user) {
    return (
      <div className="container mx-auto max-w-4xl py-20 text-center space-y-4">
        <h2 className="text-2xl font-bold">User Not Found</h2>
        <Button variant="primary" onClick={() => navigate(-1)}>
          Go Back
        </Button>
      </div>
    );
  }

  return (
    <div className="container mx-auto max-w-4xl px-4 py-10 sm:px-6 lg:px-8 space-y-8">
      <button
        onClick={() => navigate(-1)}
        className="inline-flex items-center gap-2 text-xs font-semibold text-muted-foreground hover:text-foreground transition-colors"
      >
        <ArrowLeft className="h-4 w-4" />
        <span>Back</span>
      </button>

      {/* Member Card */}
      <Card className="glass-panel border-white/10 p-8 flex flex-col sm:flex-row items-center gap-6 text-center sm:text-left">
        <Avatar name={`${user.firstName} ${user.lastName}`} size="xl" isOnline />

        <div className="space-y-2 flex-1">
          <h1 className="text-3xl font-black text-foreground font-display">
            {user.firstName} {user.lastName}
          </h1>
          <div className="flex flex-wrap items-center justify-center sm:justify-start gap-3 text-xs text-muted-foreground">
            <span className="flex items-center gap-1.5">
              <Calendar className="h-3.5 w-3.5" />
              <span>Joined {formatDate(user.createdAt)}</span>
            </span>
            <span className="flex items-center gap-1.5 text-emerald-400">
              <ShieldCheck className="h-3.5 w-3.5" />
              <span>Verified CarAuction Member</span>
            </span>
          </div>
        </div>

        {currentUser?.id !== user.id && (
          <Button
            variant="primary"
            className="gap-2"
            onClick={handleContact}
            isLoading={startConvMutation.isPending}
          >
            <MessageSquare className="h-4 w-4" />
            <span>Send Direct Message</span>
          </Button>
        )}
      </Card>
    </div>
  );
}
