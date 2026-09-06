import { useState } from 'react';
import { useGroups, useMyGroups, useCreateGroup, useJoinGroup, useLeaveGroup } from '@/hooks/useGroups';
import { useAuth } from '@/hooks/useAuth';
import { GroupCard } from '@/components/groups/GroupCard';
import { LoadingSpinner } from '@/components/common/LoadingSpinner';
import { EmptyState } from '@/components/common/EmptyState';
import { Button } from '@/components/ui/Button';
import { Dialog } from '@/components/ui/Dialog';
import { Input } from '@/components/ui/Input';
import { Textarea } from '@/components/ui/Textarea';
import { Label } from '@/components/ui/Label';
import { toast } from '@/hooks/useToast';
import { Users, PlusCircle } from 'lucide-react';

export function GroupsPage() {
  const { isAuthenticated } = useAuth();
  const { data: allGroups, isLoading: allLoading } = useGroups();
  const { data: myGroups } = useMyGroups();

  const createGroupMutation = useCreateGroup();
  const joinGroupMutation = useJoinGroup();
  const leaveGroupMutation = useLeaveGroup();

  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');

  const myGroupIds = new Set(myGroups?.map((g) => g.id) || []);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim() || !description.trim()) {
      toast.error('Please provide a club name and description');
      return;
    }

    try {
      await createGroupMutation.mutateAsync({ name, description });
      toast.success(`Club "${name}" created!`, 'Club Created');
      setIsCreateOpen(false);
      setName('');
      setDescription('');
    } catch {
      toast.error('Failed to create club');
    }
  };

  const handleJoin = async (id: number) => {
    if (!isAuthenticated) {
      toast.error('Please log in to join clubs');
      return;
    }
    try {
      await joinGroupMutation.mutateAsync(id);
      toast.success('Joined club successfully!');
    } catch {
      toast.error('Failed to join club');
    }
  };

  const handleLeave = async (id: number) => {
    try {
      await leaveGroupMutation.mutateAsync(id);
      toast.info('Left club.');
    } catch {
      toast.error('Failed to leave club');
    }
  };

  return (
    <div className="container mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8 space-y-8">
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
        <div>
          <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-brand-400">
            <Users className="h-4 w-4" />
            <span>Community & Clubs</span>
          </div>
          <h1 className="text-3xl font-black text-foreground tracking-tight mt-1 font-display">
            Enthusiast Car Clubs
          </h1>
          <p className="text-xs sm:text-sm text-muted-foreground mt-1">
            Connect with brand aficionados, discuss auctions, and chat in real-time.
          </p>
        </div>

        {isAuthenticated && (
          <Button variant="primary" className="gap-2" onClick={() => setIsCreateOpen(true)}>
            <PlusCircle className="h-4 w-4" />
            <span>Create a Club</span>
          </Button>
        )}
      </div>

      {allLoading ? (
        <LoadingSpinner size="lg" message="Loading clubs directory..." />
      ) : !allGroups || allGroups.length === 0 ? (
        <EmptyState
          title="No Clubs Available"
          description="Be the pioneer to launch the first car club on the platform!"
          actionLabel="Create Club"
          onAction={() => setIsCreateOpen(true)}
        />
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {allGroups.map((group) => {
            const isMember = myGroupIds.has(group.id);
            return (
              <GroupCard
                key={group.id}
                group={group}
                isMember={isMember}
                onJoin={handleJoin}
                onLeave={handleLeave}
                isLoading={joinGroupMutation.isPending || leaveGroupMutation.isPending}
              />
            );
          })}
        </div>
      )}

      {/* Create Modal */}
      <Dialog
        isOpen={isCreateOpen}
        onClose={() => setIsCreateOpen(false)}
        title="Create an Enthusiast Club"
        description="Launch a new car club for fans, owners, and prospective buyers"
      >
        <form onSubmit={handleCreate} className="space-y-4 mt-4">
          <div className="space-y-1">
            <Label htmlFor="clubName">Club Name</Label>
            <Input
              id="clubName"
              placeholder="e.g. M-Performance Track Enthusiasts"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
            />
          </div>

          <div className="space-y-1">
            <Label htmlFor="clubDesc">Club Focus & Description</Label>
            <Textarea
              id="clubDesc"
              rows={4}
              placeholder="Describe the club mission, preferred vehicle generations, track day meets..."
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              required
            />
          </div>

          <div className="flex justify-end gap-3 pt-2">
            <Button type="button" variant="outline" onClick={() => setIsCreateOpen(false)}>
              Cancel
            </Button>
            <Button type="submit" variant="primary" isLoading={createGroupMutation.isPending}>
              Create Club
            </Button>
          </div>
        </form>
      </Dialog>
    </div>
  );
}
