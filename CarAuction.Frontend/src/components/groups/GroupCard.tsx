import { Link } from 'react-router-dom';
import { Group } from '@/types/group';
import { Card, CardContent } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Users, MessageSquare } from 'lucide-react';

export function GroupCard({
  group,
  isMember = false,
  onJoin,
  onLeave,
  isLoading = false,
}: {
  group: Group;
  isMember?: boolean;
  onJoin?: (id: number) => void;
  onLeave?: (id: number) => void;
  isLoading?: boolean;
}) {
  return (
    <Card className="glass-card overflow-hidden hover:border-brand-500/40 transition-all flex flex-col justify-between">
      <CardContent className="p-6 space-y-4">
        <div className="flex items-start justify-between gap-3">
          <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-brand-500/10 text-brand-400 font-bold shrink-0">
            <Users className="h-6 w-6" />
          </div>

          {isMember ? (
            <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-emerald-500/10 text-emerald-400 border border-emerald-500/20">
              Joined
            </span>
          ) : (
            <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-secondary text-muted-foreground border border-border">
              Public Club
            </span>
          )}
        </div>

        <div>
          <Link to={`/groups/${group.id}`} className="group-hover:text-brand-400 transition-colors">
            <h4 className="text-lg font-bold text-foreground truncate">{group.name}</h4>
          </Link>
          <p className="mt-1 text-xs text-muted-foreground line-clamp-3 leading-relaxed">
            {group.description}
          </p>
        </div>

        <div className="flex items-center justify-between border-t border-white/5 pt-4 text-xs">
          <div className="text-muted-foreground flex items-center gap-1.5">
            <Users className="h-3.5 w-3.5" />
            <span>{group.members?.length || 1} Members</span>
          </div>

          <div className="flex gap-2">
            <Link to={`/groups/${group.id}`}>
              <Button variant="outline" size="sm" className="gap-1.5 text-xs">
                <MessageSquare className="h-3.5 w-3.5" />
                <span>Chat</span>
              </Button>
            </Link>

            {isMember && onLeave ? (
              <Button
                variant="ghost"
                size="sm"
                className="text-xs text-rose-400 hover:text-rose-300"
                onClick={() => onLeave(group.id)}
                isLoading={isLoading}
              >
                Leave
              </Button>
            ) : onJoin ? (
              <Button
                variant="primary"
                size="sm"
                className="text-xs"
                onClick={() => onJoin(group.id)}
                isLoading={isLoading}
              >
                Join
              </Button>
            ) : null}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
