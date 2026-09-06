import { Bid } from '@/types/bid';
import { formatCurrency, formatDate } from '@/utils/format';
import { Gavel, User } from 'lucide-react';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/Card';

export function BidHistory({ bids, isLoading }: { bids?: Bid[]; isLoading?: boolean }) {
  if (isLoading) {
    return (
      <Card className="glass-card">
        <CardHeader>
          <CardTitle className="text-base flex items-center gap-2">
            <Gavel className="h-4 w-4 text-brand-400" />
            <span>Bid History</span>
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-2 py-4">
            {[1, 2, 3].map((i) => (
              <div key={i} className="h-10 rounded-xl bg-white/5 animate-pulse" />
            ))}
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className="glass-card">
      <CardHeader className="pb-3 border-b border-white/5">
        <div className="flex items-center justify-between">
          <CardTitle className="text-base flex items-center gap-2">
            <Gavel className="h-4 w-4 text-brand-400" />
            <span>Bid Activity</span>
          </CardTitle>
          <span className="text-xs font-semibold px-2 py-0.5 rounded-full bg-brand-500/10 text-brand-400">
            {bids?.length || 0} Bids Placed
          </span>
        </div>
      </CardHeader>

      <CardContent className="p-0">
        {!bids || bids.length === 0 ? (
          <div className="p-6 text-center text-xs text-muted-foreground">
            No bids placed yet. Be the first to place a bid!
          </div>
        ) : (
          <div className="max-h-72 overflow-y-auto divide-y divide-white/5">
            {bids.map((bid, index) => {
              const isHighest = index === 0;
              return (
                <div
                  key={bid.id || index}
                  className={`flex items-center justify-between px-5 py-3 text-xs transition-colors ${
                    isHighest ? 'bg-amber-500/5' : 'hover:bg-white/5'
                  }`}
                >
                  <div className="flex items-center gap-3">
                    <div
                      className={`flex h-7 w-7 items-center justify-center rounded-full ${
                        isHighest ? 'bg-amber-500/20 text-amber-300' : 'bg-secondary text-muted-foreground'
                      }`}
                    >
                      <User className="h-3.5 w-3.5" />
                    </div>
                    <div>
                      <div className="font-semibold text-foreground flex items-center gap-1.5">
                        <span>{bid.userName || `Bidder #${bid.userId}`}</span>
                        {isHighest && (
                          <span className="text-[10px] font-bold text-amber-400 bg-amber-400/10 px-1.5 py-0.2 rounded">
                            HIGHEST
                          </span>
                        )}
                      </div>
                      <div className="text-[10px] text-muted-foreground">{formatDate(bid.createdAt)}</div>
                    </div>
                  </div>

                  <div className="text-right">
                    <div className="font-mono font-bold text-sm text-foreground">
                      {formatCurrency(bid.amount)}
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
