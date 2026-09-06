import { Link } from 'react-router-dom';
import { useConversations } from '@/hooks/useDirectMessages';
import { formatDate } from '@/utils/format';
import { LoadingSpinner } from '@/components/common/LoadingSpinner';
import { EmptyState } from '@/components/common/EmptyState';
import { Card } from '@/components/ui/Card';
import { Avatar } from '@/components/ui/Avatar';
import { MessageSquare, ArrowRight } from 'lucide-react';

export function MessagesPage() {
  const { data: conversations, isLoading } = useConversations();

  return (
    <div className="container mx-auto max-w-4xl px-4 py-10 sm:px-6 lg:px-8 space-y-8">
      <div>
        <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-brand-400">
          <MessageSquare className="h-4 w-4" />
          <span>Direct Inquiries</span>
        </div>
        <h1 className="text-3xl font-black text-foreground tracking-tight mt-1 font-display">
          Direct Messages
        </h1>
        <p className="text-xs sm:text-sm text-muted-foreground mt-1">
          Private communications with vehicle sellers and prospective buyers.
        </p>
      </div>

      {isLoading ? (
        <LoadingSpinner size="lg" message="Loading your conversations..." />
      ) : !conversations || conversations.length === 0 ? (
        <EmptyState
          title="No Messages Yet"
          description="When you contact a vehicle seller or receive inquiries, your direct messages will appear here."
          actionLabel="Browse Vehicles"
          onAction={() => window.location.assign('/listings')}
        />
      ) : (
        <div className="space-y-3">
          {conversations.map((conv) => {
            const fullName =
              `${conv.otherUserFirstName || conv.otherFirstName || 'User'} ${
                conv.otherUserLastName || conv.otherLastName || ''
              }`.trim() || `User #${conv.user2Id}`;

            return (
              <Link key={conv.id} to={`/messages/${conv.id}`} className="block group">
                <Card className="glass-card hover:border-brand-500/40 transition-all p-4">
                  <div className="flex items-center justify-between gap-4">
                    <div className="flex items-center gap-3.5 min-w-0">
                      <Avatar name={fullName} size="md" isOnline />
                      <div className="min-w-0">
                        <div className="flex items-center gap-2">
                          <span className="font-bold text-sm text-foreground truncate group-hover:text-brand-400 transition-colors">
                            {fullName}
                          </span>
                          {!conv.isRead && (
                            <span className="h-2 w-2 rounded-full bg-brand-500" />
                          )}
                        </div>
                        <p className="text-xs text-muted-foreground truncate mt-0.5 max-w-md">
                          {conv.lastMessage || 'Click to view conversation'}
                        </p>
                      </div>
                    </div>

                    <div className="flex items-center gap-3 shrink-0">
                      <span className="text-[11px] text-muted-foreground hidden sm:inline">
                        {formatDate(conv.createdAt)}
                      </span>
                      <div className="flex h-8 w-8 items-center justify-center rounded-xl bg-secondary text-muted-foreground group-hover:text-foreground group-hover:bg-brand-600 transition-colors">
                        <ArrowRight className="h-4 w-4" />
                      </div>
                    </div>
                  </div>
                </Card>
              </Link>
            );
          })}
        </div>
      )}
    </div>
  );
}
