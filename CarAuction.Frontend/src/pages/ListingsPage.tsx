import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useListings } from '@/hooks/useListings';
import { useDebounce } from '@/hooks/useDebounce';
import { ListingGrid } from '@/components/listings/ListingGrid';
import { ListingFilters } from '@/components/listings/ListingFilters';
import { Pagination } from '@/components/common/Pagination';
import { DEFAULT_PAGE_LIMIT } from '@/utils/constants';
import { Car } from 'lucide-react';

export function ListingsPage() {
  const [searchParams, setSearchParams] = useSearchParams();

  const [search, setSearch] = useState(searchParams.get('search') || '');
  const [minPrice, setMinPrice] = useState<number | undefined>(
    searchParams.get('minPrice') ? Number(searchParams.get('minPrice')) : undefined
  );
  const [maxPrice, setMaxPrice] = useState<number | undefined>(
    searchParams.get('maxPrice') ? Number(searchParams.get('maxPrice')) : undefined
  );
  const [status, setStatus] = useState<string>(searchParams.get('status') || '');
  const [isAuction, setIsAuction] = useState<boolean | undefined>(
    searchParams.get('isAuction') !== null ? searchParams.get('isAuction') === 'true' : undefined
  );
  const [page, setPage] = useState(1);

  const debouncedSearch = useDebounce(search, 300);

  const { data, isLoading } = useListings({
    search: debouncedSearch || undefined,
    minPrice,
    maxPrice,
    status: status || undefined,
    isAuction,
    page,
    limit: DEFAULT_PAGE_LIMIT,
  });

  const handleReset = () => {
    setSearch('');
    setMinPrice(undefined);
    setMaxPrice(undefined);
    setStatus('');
    setIsAuction(undefined);
    setPage(1);
    setSearchParams({});
  };

  return (
    <div className="container mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8 space-y-8">
      <div>
        <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-brand-400">
          <Car className="h-4 w-4" />
          <span>Vehicle Marketplace</span>
        </div>
        <h1 className="text-3xl font-black text-foreground tracking-tight mt-1 font-display">
          Explore Inventory
        </h1>
        <p className="text-xs sm:text-sm text-muted-foreground mt-1">
          Browse verified vehicles, filtered by auction style, pricing, and availability.
        </p>
      </div>

      {/* Filter Toolbar */}
      <ListingFilters
        search={search}
        setSearch={setSearch}
        minPrice={minPrice}
        setMinPrice={setMinPrice}
        maxPrice={maxPrice}
        setMaxPrice={setMaxPrice}
        status={status}
        setStatus={setStatus}
        isAuction={isAuction}
        setIsAuction={setIsAuction}
        onReset={handleReset}
      />

      {/* Grid */}
      <ListingGrid listings={data?.data} isLoading={isLoading} />

      {/* Pagination */}
      {data && (
        <Pagination
          currentPage={data.page || page}
          totalPages={data.totalPages || Math.ceil((data.total || 0) / DEFAULT_PAGE_LIMIT)}
          onPageChange={(newPage) => {
            setPage(newPage);
            window.scrollTo({ top: 0, behavior: 'smooth' });
          }}
        />
      )}
    </div>
  );
}
