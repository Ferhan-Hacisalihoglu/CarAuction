import { useState } from 'react';
import { useAuctions } from '@/hooks/useAuctions';
import { AuctionCard } from '@/components/auctions/AuctionCard';
import { LoadingSpinner } from '@/components/common/LoadingSpinner';
import { EmptyState } from '@/components/common/EmptyState';
import { SearchInput } from '@/components/common/SearchInput';
import { Gavel } from 'lucide-react';

export function AuctionsPage() {
  const { data: auctions, isLoading } = useAuctions();
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<'all' | 'active' | 'completed'>('active');

  const filteredAuctions = auctions?.filter((auction) => {
    const matchesSearch =
      !search ||
      (auction.title && auction.title.toLowerCase().includes(search.toLowerCase())) ||
      (auction.description && auction.description.toLowerCase().includes(search.toLowerCase()));

    if (!matchesSearch) return false;

    if (statusFilter === 'active') return auction.status === 'active';
    if (statusFilter === 'completed') return auction.status === 'completed' || auction.status === 'expired';
    return true;
  });

  return (
    <div className="container mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8 space-y-8">
      <div>
        <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-amber-400">
          <Gavel className="h-4 w-4" />
          <span>Live Bidding Floor</span>
        </div>
        <h1 className="text-3xl font-black text-foreground tracking-tight mt-1 font-display">
          Live Vehicle Auctions
        </h1>
        <p className="text-xs sm:text-sm text-muted-foreground mt-1">
          Participate in real-time competitive auctions. Countdown timers feature anti-sniping extension protection.
        </p>
      </div>

      {/* Filter Bar */}
      <div className="flex flex-col sm:flex-row gap-4 items-center justify-between p-4 rounded-2xl glass-panel border-white/10">
        <div className="w-full sm:w-80">
          <SearchInput
            value={search}
            onChange={setSearch}
            placeholder="Search active lots..."
          />
        </div>

        <div className="flex items-center space-x-1 p-1 rounded-xl bg-secondary/80 border border-border/50">
          <button
            type="button"
            onClick={() => setStatusFilter('active')}
            className={`px-3.5 py-1.5 rounded-lg text-xs font-semibold transition-colors ${
              statusFilter === 'active'
                ? 'bg-amber-500/20 text-amber-300 shadow-sm'
                : 'text-muted-foreground hover:text-foreground'
            }`}
          >
            Active Auctions
          </button>
          <button
            type="button"
            onClick={() => setStatusFilter('completed')}
            className={`px-3.5 py-1.5 rounded-lg text-xs font-semibold transition-colors ${
              statusFilter === 'completed'
                ? 'bg-card text-foreground shadow-sm'
                : 'text-muted-foreground hover:text-foreground'
            }`}
          >
            Ended Lots
          </button>
          <button
            type="button"
            onClick={() => setStatusFilter('all')}
            className={`px-3.5 py-1.5 rounded-lg text-xs font-semibold transition-colors ${
              statusFilter === 'all'
                ? 'bg-card text-foreground shadow-sm'
                : 'text-muted-foreground hover:text-foreground'
            }`}
          >
            All Lots
          </button>
        </div>
      </div>

      {/* Grid */}
      {isLoading ? (
        <LoadingSpinner size="lg" message="Loading live auction inventory..." />
      ) : !filteredAuctions || filteredAuctions.length === 0 ? (
        <EmptyState
          title="No Auctions Found"
          description="There are currently no active lots matching your search filter."
        />
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          {filteredAuctions.map((auction) => (
            <AuctionCard key={auction.id} auction={auction} />
          ))}
        </div>
      )}
    </div>
  );
}
