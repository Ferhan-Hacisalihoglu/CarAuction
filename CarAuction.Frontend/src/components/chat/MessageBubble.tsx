import { formatDate } from '@/utils/format';
import { Avatar } from '@/components/ui/Avatar';
import { Check, CheckCheck } from 'lucide-react';
import { cn } from '@/utils/cn';

export function MessageBubble({
  content,
  sentAt,
  senderName,
  isOwn,
  isRead,
}: {
  content: string;
  sentAt: string;
  senderName?: string;
  isOwn: boolean;
  isRead?: boolean;
}) {
  return (
    <div className={cn('flex items-end gap-2 my-2.5', isOwn ? 'justify-end' : 'justify-start')}>
      {!isOwn && (
        <Avatar name={senderName || 'U'} size="sm" className="mb-1" />
      )}

      <div
        className={cn(
          'max-w-md rounded-2xl px-4 py-2.5 text-sm shadow-md transition-all',
          isOwn
            ? 'bg-brand-600 text-white rounded-br-none'
            : 'bg-card/90 text-foreground border border-white/10 rounded-bl-none'
        )}
      >
        {!isOwn && senderName && (
          <div className="text-[10px] font-bold text-brand-400 mb-0.5">
            {senderName}
          </div>
        )}

        <p className="leading-relaxed whitespace-pre-wrap break-words">{content}</p>

        <div
          className={cn(
            'flex items-center justify-end gap-1 mt-1 text-[10px]',
            isOwn ? 'text-white/70' : 'text-muted-foreground'
          )}
        >
          <span>{formatDate(sentAt)}</span>
          {isOwn && (
            <span>
              {isRead ? (
                <CheckCheck className="h-3 w-3 text-emerald-300 inline" />
              ) : (
                <Check className="h-3 w-3 inline" />
              )}
            </span>
          )}
        </div>
      </div>
    </div>
  );
}
