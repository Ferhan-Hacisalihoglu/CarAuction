import { SearchInput } from '@/components/common/SearchInput';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Label } from '@/components/ui/Label';
import { SlidersHorizontal, RotateCcw } from 'lucide-react';

interface FilterProps {
  search: string;
  setSearch: (val: string) => void;
  minPrice?: number;
  setMinPrice: (val?: number) => void;
  maxPrice?: number;
  setMaxPrice: (val?: number) => void;
  status: string;
  setStatus: (val: string) => void;
  isAuction?: boolean;
  setIsAuction: (val?: boolean) => void;
  onReset: () => void;
}

export function ListingFilters({
  search,
  setSearch,
  minPrice,
  setMinPrice,
  maxPrice,
  setMaxPrice,
  status,
  setStatus,
  isAuction,
  setIsAuction,
  onReset,
}: FilterProps) {
  return (
    <div className="p-5 rounded-2xl glass-panel border-white/10 mb-8 space-y-4">
      <div className="flex flex-col md:flex-row gap-4 items-center justify-between">
        <div className="w-full md:w-80">
          <SearchInput
            value={search}
            onChange={setSearch}
            placeholder="Search make, model, year..."
          />
        </div>

        <div className="flex flex-wrap items-center gap-3 w-full md:w-auto">
          {/* Auction toggle */}
          <div className="flex items-center space-x-1 p-1 rounded-xl bg-secondary/60 border border-border/50">
            <button
              type="button"
              onClick={() => setIsAuction(undefined)}
              className={`px-3 py-1.5 rounded-lg text-xs font-semibold transition-colors ${
                isAuction === undefined ? 'bg-card text-foreground shadow-sm' : 'text-muted-foreground'
              }`}
            >
              All Types
            </button>
            <button
              type="button"
              onClick={() => setIsAuction(true)}
              className={`px-3 py-1.5 rounded-lg text-xs font-semibold transition-colors ${
                isAuction === true ? 'bg-amber-500/20 text-amber-300 shadow-sm' : 'text-muted-foreground'
              }`}
            >
              Auctions Only
            </button>
            <button
              type="button"
              onClick={() => setIsAuction(false)}
              className={`px-3 py-1.5 rounded-lg text-xs font-semibold transition-colors ${
                isAuction === false ? 'bg-card text-foreground shadow-sm' : 'text-muted-foreground'
              }`}
            >
              Direct Sale
            </button>
          </div>

          {/* Status selector */}
          <select
            value={status}
            onChange={(e) => setStatus(e.target.value)}
            className="h-10 rounded-xl border border-border/80 bg-background/60 px-3 py-1 text-xs font-medium focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-500"
          >
            <option value="">All Statuses</option>
            <option value="active">Active</option>
            <option value="sold">Sold</option>
            <option value="expired">Expired</option>
          </select>

          {/* Reset button */}
          <Button variant="ghost" size="sm" onClick={onReset} className="gap-1.5 text-muted-foreground text-xs">
            <RotateCcw className="h-3.5 w-3.5" />
            <span>Reset</span>
          </Button>
        </div>
      </div>

      {/* Price range row */}
      <div className="flex items-center gap-3 pt-2 border-t border-white/5">
        <SlidersHorizontal className="h-4 w-4 text-muted-foreground hidden sm:inline" />
        <span className="text-xs font-semibold text-muted-foreground">Price Range:</span>
        <div className="flex items-center gap-2 max-w-xs">
          <Input
            type="number"
            placeholder="Min $"
            value={minPrice ?? ''}
            onChange={(e) => setMinPrice(e.target.value ? Number(e.target.value) : undefined)}
            className="h-8 text-xs"
          />
          <span className="text-muted-foreground text-xs">-</span>
          <Input
            type="number"
            placeholder="Max $"
            value={maxPrice ?? ''}
            onChange={(e) => setMaxPrice(e.target.value ? Number(e.target.value) : undefined)}
            className="h-8 text-xs"
          />
        </div>
      </div>
    </div>
  );
}
