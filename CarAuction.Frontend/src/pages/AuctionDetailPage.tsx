import { useParams, Link } from 'react-router-dom';
import { useAuction, useAuctionBids } from '@/hooks/useAuctions';
import { useListing } from '@/hooks/useListings';
import { useAuctionHub } from '@/hooks/useAuctionHub';
import { LiveBidPanel } from '@/components/auctions/LiveBidPanel';
import { BidHistory } from '@/components/auctions/BidHistory';
import { ImageGallery } from '@/components/listings/ImageGallery';
import { LoadingSpinner } from '@/components/common/LoadingSpinner';
import { Card, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { formatCurrency, formatDate } from '@/utils/format';
import { Gavel, Calendar, ArrowLeft, ShieldCheck, Car } from 'lucide-react';

export function AuctionDetailPage() {
  const { id } = useParams<{ id: string }>();
  const auctionId = Number(id);

  // Hook into live SignalR WebSocket for this specific auction
  useAuctionHub(auctionId);

  const { data: auction, isLoading: auctionLoading, isError } = useAuction(auctionId);
  const { data: listing, isLoading: listingLoading } = useListing(auction?.listingId || 0);
  const { data: bids, isLoading: bidsLoading } = useAuctionBids(auctionId);

  if (auctionLoading || (auction && listingLoading)) {
    return <LoadingSpinner size="lg" message="Connecting to live auction room..." className="min-h-[50vh]" />;
  }

  if (isError || !auction) {
    return (
      <div className="container mx-auto max-w-4xl py-20 text-center space-y-4">
        <h2 className="text-2xl font-bold">Auction Not Found</h2>
        <p className="text-muted-foreground">This auction lot does not exist or has expired.</p>
        <Link to="/auctions">
          <Button variant="primary">Return to Auctions</Button>
        </Link>
      </div>
    );
  }

  return (
    <div className="container mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8 space-y-8">
      {/* Breadcrumb / Back */}
      <div className="flex items-center justify-between">
        <Link
          to="/auctions"
          className="inline-flex items-center gap-2 text-xs font-semibold text-muted-foreground hover:text-foreground transition-colors"
        >
          <ArrowLeft className="h-4 w-4" />
          <span>Back to Live Floor</span>
        </Link>

        <div className="flex items-center gap-2">
          <span className="relative flex h-2.5 w-2.5">
            <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-emerald-400 opacity-75"></span>
            <span className="relative inline-flex rounded-full h-2.5 w-2.5 bg-emerald-500"></span>
          </span>
          <span className="text-xs font-semibold text-emerald-400">Live Connected</span>
        </div>
      </div>

      {/* Main Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">
        {/* Left Column: Media Gallery, Vehicle Specs (7 cols) */}
        <div className="lg:col-span-7 xl:col-span-7 space-y-6">
          <ImageGallery images={listing?.images} />

          {/* Vehicle Information */}
          <Card className="glass-card p-6 space-y-6">
            <div className="space-y-2">
              <div className="flex items-center gap-2">
                <Badge variant="gold" className="gap-1 font-bold">
                  <Gavel className="h-3.5 w-3.5" />
                  <span>Auction Lot #{auction.id}</span>
                </Badge>
                <Badge
                  variant={auction.status === 'active' ? 'success' : 'secondary'}
                  className="capitalize"
                >
                  {auction.status}
                </Badge>
              </div>

              <h1 className="text-2xl sm:text-3xl font-black text-foreground font-display tracking-tight">
                {auction.title || listing?.title || `Vehicle Lot #${auction.listingId}`}
              </h1>

              <div className="flex flex-wrap items-center gap-4 text-xs text-muted-foreground pt-1">
                <span className="flex items-center gap-1.5">
                  <Calendar className="h-3.5 w-3.5" />
                  <span>Started {formatDate(auction.startTime)}</span>
                </span>
                <span className="flex items-center gap-1.5">
                  <ShieldCheck className="h-3.5 w-3.5 text-emerald-400" />
                  <span>Reserve & Anti-Snipe Protected</span>
                </span>
              </div>
            </div>

            {/* Description */}
            <div className="border-t border-white/5 pt-5 space-y-2">
              <h3 className="text-sm font-bold uppercase tracking-wider text-muted-foreground">
                Lot Specifications
              </h3>
              <p className="text-sm text-foreground/90 leading-relaxed whitespace-pre-wrap">
                {auction.description || listing?.description || 'No description provided for this vehicle lot.'}
              </p>
            </div>

            {/* Specs Summary */}
            <div className="grid grid-cols-2 sm:grid-cols-3 gap-3 border-t border-white/5 pt-4 text-xs">
              <div className="p-3 rounded-xl bg-secondary/40 border border-white/5">
                <span className="text-muted-foreground block text-[10px] uppercase font-semibold">
                  Starting Price
                </span>
                <span className="font-bold text-foreground text-sm font-display">
                  {formatCurrency(auction.startingPrice)}
                </span>
              </div>
              <div className="p-3 rounded-xl bg-secondary/40 border border-white/5">
                <span className="text-muted-foreground block text-[10px] uppercase font-semibold">
                  Min Increment
                </span>
                <span className="font-bold text-foreground text-sm font-display">
                  +{formatCurrency(auction.minBidIncrement)}
                </span>
              </div>
              <div className="p-3 rounded-xl bg-secondary/40 border border-white/5 col-span-2 sm:col-span-1">
                <span className="text-muted-foreground block text-[10px] uppercase font-semibold">
                  Seller
                </span>
                <span className="font-bold text-foreground text-sm font-display">
                  User #{auction.sellerId || listing?.userId || 'N/A'}
                </span>
              </div>
            </div>
          </Card>
        </div>

        {/* Right Column: Live Bid Controls, Bid History (5 cols) */}
        <div className="lg:col-span-5 xl:col-span-5 space-y-6">
          {/* Live Bid Panel */}
          <LiveBidPanel auction={auction} />

          {/* Bid History Stream */}
          <BidHistory bids={bids} isLoading={bidsLoading} />
        </div>
      </div>
    </div>
  );
}
