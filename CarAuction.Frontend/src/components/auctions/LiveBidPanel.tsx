import { useState } from 'react';
import { Auction } from '@/types/auction';
import { useAuth } from '@/hooks/useAuth';
import { usePlaceBid } from '@/hooks/useAuctions';
import { formatCurrency } from '@/utils/format';
import { AuctionTimer } from './AuctionTimer';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import { toast } from '@/hooks/useToast';
import { Gavel, Trophy, ShieldAlert } from 'lucide-react';
import { Link } from 'react-router-dom';

export function LiveBidPanel({ auction }: { auction: Auction }) {
  const { isAuthenticated, user } = useAuth();
  const placeBidMutation = usePlaceBid();

  const minBid = (auction.currentPrice || auction.startingPrice) + (auction.minBidIncrement || 50);
  const [customAmount, setCustomAmount] = useState<string>(minBid.toString());

  const isCompleted = auction.status === 'completed' || auction.status === 'expired';
  const isWinner = isCompleted && auction.winnerUserId && user?.id === auction.winnerUserId;

  const handleBidSubmit = async (amount: number) => {
    if (!isAuthenticated) {
      toast.error('Please log in to place bids on live auctions', 'Login Required');
      return;
    }

    if (amount < minBid) {
      toast.error(`Minimum bid must be at least ${formatCurrency(minBid)}`, 'Invalid Bid');
      return;
    }

    try {
      const idempotencyKey = crypto.randomUUID ? crypto.randomUUID() : Math.random().toString();
      await placeBidMutation.mutateAsync({
        auctionId: auction.id,
        amount,
        idempotencyKey,
      });
      toast.success(`Your bid of ${formatCurrency(amount)} was accepted!`, 'Bid Placed');
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Failed to submit bid. Please check price and retry.';
      toast.error(msg, 'Bid Rejected');
    }
  };

  return (
    <Card className="glass-panel border-white/10 shadow-2xl relative overflow-hidden">
      {/* Glow highlight */}
      <div className="absolute -top-24 -right-24 w-48 h-48 rounded-full bg-amber-500/10 blur-3xl pointer-events-none" />

      <CardHeader className="border-b border-white/5 pb-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <span className="relative flex h-3 w-3">
              <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-amber-400 opacity-75"></span>
              <span className="relative inline-flex rounded-full h-3 w-3 bg-amber-500"></span>
            </span>
            <CardTitle className="text-lg">Live Auction Floor</CardTitle>
          </div>

          <AuctionTimer endTime={auction.endTime} size="sm" />
        </div>
      </CardHeader>

      <CardContent className="p-6 space-y-6">
        {/* Completed Winner Announcement */}
        {isCompleted && (
          <div className="p-4 rounded-2xl bg-amber-500/10 border border-amber-500/30 text-center space-y-2">
            <Trophy className="h-8 w-8 text-amber-400 mx-auto animate-bounce" />
            <div className="text-sm font-bold text-amber-300 uppercase tracking-wider">
              Auction Ended
            </div>
            <div className="text-xs text-muted-foreground">
              {isWinner
                ? '🎉 Congratulations! You won this vehicle auction!'
                : auction.winnerUserId
                ? `Winning bid: ${formatCurrency(auction.currentPrice)}`
                : 'Auction ended with no bids.'}
            </div>
          </div>
        )}

        {/* Price Display */}
        <div className="rounded-2xl bg-black/40 border border-white/5 p-5 text-center">
          <span className="text-xs uppercase font-semibold text-muted-foreground tracking-wider block">
            Current High Bid
          </span>
          <div className="text-4xl sm:text-5xl font-black text-amber-400 font-display tracking-tight mt-1">
            {formatCurrency(auction.currentPrice)}
          </div>
          <div className="text-xs text-muted-foreground mt-2 flex items-center justify-center gap-1.5">
            <span>Minimum next bid:</span>
            <span className="font-semibold text-foreground">{formatCurrency(minBid)}</span>
            <span className="text-[10px] text-muted-foreground">
              (+{formatCurrency(auction.minBidIncrement)})
            </span>
          </div>
        </div>

        {/* Bidding Controls */}
        {!isCompleted && (
          <>
            {isAuthenticated ? (
              <div className="space-y-4">
                {/* Quick Bid Increment Buttons */}
                <div className="grid grid-cols-3 gap-2">
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    className="text-xs font-semibold hover:border-amber-500/50"
                    onClick={() => {
                      setCustomAmount(minBid.toString());
                      handleBidSubmit(minBid);
                    }}
                    isLoading={placeBidMutation.isPending}
                  >
                    +{formatCurrency(auction.minBidIncrement)}
                  </Button>
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    className="text-xs font-semibold hover:border-amber-500/50"
                    onClick={() => {
                      const val = minBid + 250;
                      setCustomAmount(val.toString());
                      handleBidSubmit(val);
                    }}
                    isLoading={placeBidMutation.isPending}
                  >
                    +${(auction.minBidIncrement + 250).toLocaleString()}
                  </Button>
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    className="text-xs font-semibold hover:border-amber-500/50"
                    onClick={() => {
                      const val = minBid + 500;
                      setCustomAmount(val.toString());
                      handleBidSubmit(val);
                    }}
                    isLoading={placeBidMutation.isPending}
                  >
                    +${(auction.minBidIncrement + 500).toLocaleString()}
                  </Button>
                </div>

                {/* Custom Bid Input */}
                <div className="flex gap-2">
                  <Input
                    type="number"
                    min={minBid}
                    step={auction.minBidIncrement || 50}
                    value={customAmount}
                    onChange={(e) => setCustomAmount(e.target.value)}
                    placeholder={`Min ${formatCurrency(minBid)}`}
                    className="h-12 text-base font-semibold"
                  />
                  <Button
                    variant="gold"
                    size="lg"
                    className="gap-2 px-6 shrink-0"
                    onClick={() => handleBidSubmit(Number(customAmount))}
                    isLoading={placeBidMutation.isPending}
                  >
                    <Gavel className="h-5 w-5" />
                    <span>Place Bid</span>
                  </Button>
                </div>

                <div className="flex items-center gap-1.5 text-[11px] text-muted-foreground justify-center">
                  <ShieldAlert className="h-3.5 w-3.5 text-amber-400" />
                  <span>Anti-sniping protection extends countdown by 120s on late bids.</span>
                </div>
              </div>
            ) : (
              <div className="p-4 rounded-2xl bg-secondary/60 text-center space-y-3">
                <p className="text-xs text-muted-foreground">
                  Sign in with your CarAuction account to place bids and track lot history.
                </p>
                <div className="flex justify-center gap-3">
                  <Link to="/login">
                    <Button variant="primary" size="sm">
                      Log In to Bid
                    </Button>
                  </Link>
                  <Link to="/register">
                    <Button variant="outline" size="sm">
                      Register
                    </Button>
                  </Link>
                </div>
              </div>
            )}
          </>
        )}
      </CardContent>
    </Card>
  );
}
