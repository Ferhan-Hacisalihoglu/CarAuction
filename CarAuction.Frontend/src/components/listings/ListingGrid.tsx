import { Listing } from '@/types/listing';
import { ListingCard } from './ListingCard';
import { EmptyState } from '@/components/common/EmptyState';
import { LoadingSpinner } from '@/components/common/LoadingSpinner';

export function ListingGrid({
  listings,
  isLoading,
  emptyMessage = 'No vehicles currently matching your criteria.',
}: {
  listings?: Listing[];
  isLoading?: boolean;
  emptyMessage?: string;
}) {
  if (isLoading) {
    return <LoadingSpinner size="lg" message="Loading vehicle inventory..." />;
  }

  if (!listings || listings.length === 0) {
    return <EmptyState title="No Vehicles Found" description={emptyMessage} />;
  }

  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
      {listings.map((listing) => (
        <ListingCard key={listing.id} listing={listing} />
      ))}
    </div>
  );
}
