import { Link } from 'react-router-dom';
import { useMyListings, useDeleteListing } from '@/hooks/useListings';
import { formatCurrency, formatDate } from '@/utils/format';
import { LoadingSpinner } from '@/components/common/LoadingSpinner';
import { EmptyState } from '@/components/common/EmptyState';
import { Card, CardContent } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { ConfirmDialog } from '@/components/common/ConfirmDialog';
import { toast } from '@/hooks/useToast';
import { useState } from 'react';
import { Package, PlusCircle, Edit, Trash2, ExternalLink, Gavel } from 'lucide-react';

export function MyListingsPage() {
  const { data: listings, isLoading } = useMyListings();
  const deleteMutation = useDeleteListing();

  const [deleteTargetId, setDeleteTargetId] = useState<number | null>(null);

  const confirmDelete = async () => {
    if (!deleteTargetId) return;
    try {
      await deleteMutation.mutateAsync(deleteTargetId);
      toast.success('Listing cancelled successfully');
      setDeleteTargetId(null);
    } catch {
      toast.error('Failed to cancel listing');
    }
  };

  return (
    <div className="container mx-auto max-w-6xl px-4 py-10 sm:px-6 lg:px-8 space-y-8">
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
        <div>
          <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-brand-400">
            <Package className="h-4 w-4" />
            <span>Seller Dashboard</span>
          </div>
          <h1 className="text-3xl font-black text-foreground tracking-tight mt-1 font-display">
            My Vehicle Garage
          </h1>
          <p className="text-xs sm:text-sm text-muted-foreground mt-1">
            Manage your published vehicles, check statuses, or submit a new lot.
          </p>
        </div>

        <Link to="/listings/create">
          <Button variant="primary" className="gap-2">
            <PlusCircle className="h-4 w-4" />
            <span>Add Vehicle</span>
          </Button>
        </Link>
      </div>

      {isLoading ? (
        <LoadingSpinner size="lg" message="Loading your vehicle listings..." />
      ) : !listings || listings.length === 0 ? (
        <EmptyState
          title="No Vehicles in Your Garage"
          description="You haven't listed any vehicles yet. List a vehicle for direct sale or live auction."
          actionLabel="Sell a Vehicle"
          onAction={() => window.location.assign('/listings/create')}
        />
      ) : (
        <div className="space-y-3">
          {listings.map((listing) => (
            <Card key={listing.id} className="glass-card hover:border-white/20 transition-all p-5">
              <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
                <div className="space-y-1 min-w-0 flex-1">
                  <div className="flex items-center gap-2">
                    <span className="font-bold text-base text-foreground truncate">
                      {listing.title}
                    </span>
                    {listing.isAuction ? (
                      <Badge variant="gold" className="gap-1 text-[10px]">
                        <Gavel className="h-3 w-3" />
                        <span>Live Auction</span>
                      </Badge>
                    ) : (
                      <Badge variant="default" className="text-[10px]">
                        Direct Sale
                      </Badge>
                    )}
                    <Badge
                      variant={
                        listing.status === 'active'
                          ? 'success'
                          : listing.status === 'sold'
                          ? 'secondary'
                          : 'warning'
                      }
                      className="capitalize text-[10px]"
                    >
                      {listing.status}
                    </Badge>
                  </div>

                  <p className="text-xs text-muted-foreground line-clamp-1">
                    {listing.description}
                  </p>

                  <div className="flex items-center gap-4 text-xs text-muted-foreground pt-1">
                    <span className="font-bold text-foreground font-display text-sm">
                      {formatCurrency(listing.price)}
                    </span>
                    <span>•</span>
                    <span>Created {formatDate(listing.createdAt)}</span>
                  </div>
                </div>

                <div className="flex items-center gap-2 shrink-0">
                  <Link to={`/listings/${listing.id}`}>
                    <Button variant="outline" size="sm" className="gap-1 text-xs">
                      <ExternalLink className="h-3.5 w-3.5" />
                      <span>View</span>
                    </Button>
                  </Link>
                  <Link to={`/listings/${listing.id}/edit`}>
                    <Button variant="secondary" size="sm" className="gap-1 text-xs">
                      <Edit className="h-3.5 w-3.5" />
                      <span>Edit</span>
                    </Button>
                  </Link>
                  <Button
                    variant="ghost"
                    size="sm"
                    className="text-xs text-rose-400 hover:text-rose-300"
                    onClick={() => setDeleteTargetId(listing.id)}
                  >
                    <Trash2 className="h-3.5 w-3.5" />
                  </Button>
                </div>
              </div>
            </Card>
          ))}
        </div>
      )}

      <ConfirmDialog
        isOpen={deleteTargetId !== null}
        onClose={() => setDeleteTargetId(null)}
        onConfirm={confirmDelete}
        title="Cancel Vehicle Listing"
        message="Are you sure you want to cancel this listing? The vehicle will be marked as cancelled."
        confirmLabel="Yes, Cancel"
        isDestructive
        isLoading={deleteMutation.isPending}
      />
    </div>
  );
}
