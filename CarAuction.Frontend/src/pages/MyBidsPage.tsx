import { Link } from 'react-router-dom';
import { useMyBids } from '@/hooks/useBids';
import { formatCurrency, formatDate } from '@/utils/format';
import { LoadingSpinner } from '@/components/common/LoadingSpinner';
import { EmptyState } from '@/components/common/EmptyState';
import { Card } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { Gavel, ExternalLink } from 'lucide-react';

export function MyBidsPage() {
  const { data: bids, isLoading } = useMyBids();

  return (
    <div className="container mx-auto max-w-6xl px-4 py-10 sm:px-6 lg:px-8 space-y-8">
      <div>
        <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-amber-400">
          <Gavel className="h-4 w-4" />
          <span>Bid History</span>
        </div>
        <h1 className="text-3xl font-black text-foreground tracking-tight mt-1 font-display">
          My Auction Bids
        </h1>
        <p className="text-xs sm:text-sm text-muted-foreground mt-1">
          Review all your placed bids, tracking active lots, won vehicles, and history.
        </p>
      </div>

      {isLoading ? (
        <LoadingSpinner size="lg" message="Loading your bidding history..." />
      ) : !bids || bids.length === 0 ? (
        <EmptyState
          title="No Bids Placed Yet"
          description="You haven't placed bids on any active vehicle lots yet."
          actionLabel="Explore Live Floor"
          onAction={() => window.location.assign('/auctions')}
        />
      ) : (
        <div className="space-y-3">
          {bids.map((bid) => (
            <Card key={bid.id} className="glass-card p-5 hover:border-white/20 transition-all">
              <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
                <div className="space-y-1">
                  <div className="flex items-center gap-2">
                    <span className="font-bold text-base text-foreground">
                      {bid.listingTitle}
                    </span>
                    <Badge
                      variant={
                        bid.listingStatus === 'active'
                          ? 'success'
                          : bid.listingStatus === 'sold'
                          ? 'secondary'
                          : 'warning'
                      }
                      className="capitalize text-[10px]"
                    >
                      {bid.listingStatus}
                    </Badge>
                  </div>

                  <div className="flex items-center gap-4 text-xs text-muted-foreground">
                    <span>
                      My Bid:{' '}
                      <span className="font-extrabold text-amber-400 text-sm font-display">
                        {formatCurrency(bid.amount)}
                      </span>
                    </span>
                    <span>•</span>
                    <span>Placed {formatDate(bid.createdAt)}</span>
                  </div>
                </div>

                <Link to={`/listings/${bid.listingId}`}>
                  <Button variant="outline" size="sm" className="gap-1.5 text-xs">
                    <ExternalLink className="h-3.5 w-3.5" />
                    <span>View Vehicle</span>
                  </Button>
                </Link>
              </div>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
