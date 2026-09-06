import { useEffect, useState } from 'react';
import { formatTimeRemaining } from '@/utils/format';
import { Clock, AlertTriangle } from 'lucide-react';
import { cn } from '@/utils/cn';

export function AuctionTimer({
  endTime,
  onExpire,
  className,
  size = 'md',
}: {
  endTime: string;
  onExpire?: () => void;
  className?: string;
  size?: 'sm' | 'md' | 'lg';
}) {
  const [timeLeft, setTimeLeft] = useState(formatTimeRemaining(endTime));

  useEffect(() => {
    const timer = setInterval(() => {
      const remaining = formatTimeRemaining(endTime);
      setTimeLeft(remaining);
      if (remaining.isExpired && onExpire) {
        onExpire();
        clearInterval(timer);
      }
    }, 1000);

    return () => clearInterval(timer);
  }, [endTime, onExpire]);

  const isUrgent = !timeLeft.isExpired && timeLeft.totalSeconds <= 120; // Last 2 minutes (anti-snipe window)

  const sizes = {
    sm: 'text-xs px-2.5 py-1',
    md: 'text-sm px-3.5 py-1.5',
    lg: 'text-lg px-5 py-2.5 font-bold',
  };

  return (
    <div
      className={cn(
        'inline-flex items-center gap-2 rounded-xl font-mono font-semibold transition-all backdrop-blur-md border',
        timeLeft.isExpired
          ? 'bg-muted/80 text-muted-foreground border-border'
          : isUrgent
          ? 'bg-rose-500/20 text-rose-400 border-rose-500/40 animate-pulse shadow-lg shadow-rose-500/20'
          : 'bg-amber-500/15 text-amber-300 border-amber-500/30',
        sizes[size],
        className
      )}
    >
      {isUrgent ? (
        <AlertTriangle className="h-4 w-4 animate-bounce text-rose-400" />
      ) : (
        <Clock className="h-4 w-4 text-current opacity-80" />
      )}
      <span>{timeLeft.formatted}</span>
      {isUrgent && (
        <span className="text-[10px] uppercase font-bold tracking-wider px-1.5 py-0.5 rounded bg-rose-500/30 text-rose-300">
          Anti-Snipe Active
        </span>
      )}
    </div>
  );
}
