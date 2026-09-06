import { useAcceptOffer, useRejectOffer } from '@/hooks/useBids';
import { useQuery } from '@tanstack/react-query';
import { bidsApi } from '@/api/bids';
import { formatCurrency, formatDate } from '@/utils/format';
import { LoadingSpinner } from '@/components/common/LoadingSpinner';
import { EmptyState } from '@/components/common/EmptyState';
import { Card } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { toast } from '@/hooks/useToast';
import { HeartHandshake, Check, X } from 'lucide-react';
import { Link } from 'react-router-dom';

export function MyOffersPage() {
  const { data: offers, isLoading, refetch } = useQuery({
    queryKey: ['my-offers'],
    queryFn: async () => {
      // Fetch received offers or bids
      return bidsApi.getMyBids();
    },
  });

  const acceptMutation = useAcceptOffer();
  const rejectMutation = useRejectOffer();

  const handleAccept = async (id: number) => {
    try {
      await acceptMutation.mutateAsync(id);
      toast.success('Offer accepted! Vehicle marked as sold.', 'Offer Accepted');
      refetch();
    } catch {
      toast.error('Failed to accept offer');
    }
  };

  const handleReject = async (id: number) => {
    try {
      await rejectMutation.mutateAsync(id);
      toast.info('Offer has been rejected.');
      refetch();
    } catch {
      toast.error('Failed to reject offer');
    }
  };

  return (
    <div className="container mx-auto max-w-6xl px-4 py-10 sm:px-6 lg:px-8 space-y-8">
      <div>
        <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-emerald-400">
          <HeartHandshake className="h-4 w-4" />
          <span>Offers Inbox</span>
        </div>
        <h1 className="text-3xl font-black text-foreground tracking-tight mt-1 font-display">
          Vehicle Offers Management
        </h1>
        <p className="text-xs sm:text-sm text-muted-foreground mt-1">
          Review incoming private purchase offers on your direct sale vehicles.
        </p>
      </div>

      {isLoading ? (
        <LoadingSpinner size="lg" message="Loading vehicle offers..." />
      ) : !offers || offers.length === 0 ? (
        <EmptyState
          title="No Active Offers"
          description="You currently have no incoming offers on your vehicles."
          actionLabel="View My Listings"
          onAction={() => window.location.assign('/my-listings')}
        />
      ) : (
        <div className="space-y-3">
          {offers.map((offer) => (
            <Card key={offer.id} className="glass-card p-5 hover:border-white/20 transition-all">
              <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
                <div className="space-y-1">
                  <div className="flex items-center gap-2">
                    <span className="font-bold text-base text-foreground">
                      {offer.listingTitle}
                    </span>
                    <Badge variant="default" className="text-[10px]">
                      Offer #{offer.id}
                    </Badge>
                  </div>

                  <div className="flex items-center gap-4 text-xs text-muted-foreground">
                    <span>
                      Offered Amount:{' '}
                      <span className="font-extrabold text-emerald-400 text-sm font-display">
                        {formatCurrency(offer.amount)}
                      </span>
                    </span>
                    <span>•</span>
                    <span>Received {formatDate(offer.createdAt)}</span>
                  </div>
                </div>

                <div className="flex items-center gap-2">
                  <Link to={`/listings/${offer.listingId}`}>
                    <Button variant="outline" size="sm" className="text-xs">
                      View Vehicle
                    </Button>
                  </Link>
                  <Button
                    variant="primary"
                    size="sm"
                    className="gap-1 text-xs bg-emerald-600 hover:bg-emerald-500"
                    onClick={() => handleAccept(offer.id)}
                    isLoading={acceptMutation.isPending}
                  >
                    <Check className="h-3.5 w-3.5" />
                    <span>Accept</span>
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    className="gap-1 text-xs text-rose-400 hover:text-rose-300"
                    onClick={() => handleReject(offer.id)}
                    isLoading={rejectMutation.isPending}
                  >
                    <X className="h-3.5 w-3.5" />
                    <span>Reject</span>
                  </Button>
                </div>
              </div>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
