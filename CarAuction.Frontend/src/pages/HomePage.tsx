import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useListings } from '@/hooks/useListings';
import { useAuctions } from '@/hooks/useAuctions';
import { ListingGrid } from '@/components/listings/ListingGrid';
import { AuctionCard } from '@/components/auctions/AuctionCard';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import {
  Car,
  Gavel,
  ShieldCheck,
  Zap,
  Search,
  ArrowRight,
  TrendingUp,
  Sparkles,
} from 'lucide-react';

export function HomePage() {
  const [searchQuery, setSearchQuery] = useState('');
  const navigate = useNavigate();

  const { data: listingsData, isLoading: listingsLoading } = useListings({ limit: 8 });
  const { data: auctionsData, isLoading: auctionsLoading } = useAuctions();

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      navigate(`/listings?search=${encodeURIComponent(searchQuery.trim())}`);
    } else {
      navigate('/listings');
    }
  };

  const activeAuctions = auctionsData?.filter((a) => a.status === 'active').slice(0, 4);

  return (
    <div className="space-y-16 pb-20">
      {/* Hero Section */}
      <section className="relative min-h-[580px] flex items-center justify-center overflow-hidden border-b border-white/10 bg-gradient-to-b from-background via-card/50 to-background px-4 sm:px-6 lg:px-8 py-20">
        {/* Glow Spheres */}
        <div className="absolute -top-40 left-1/2 -translate-x-1/2 w-[600px] h-[600px] rounded-full bg-brand-600/15 blur-[140px] pointer-events-none" />
        <div className="absolute top-1/3 right-10 w-[350px] h-[350px] rounded-full bg-indigo-600/10 blur-[120px] pointer-events-none" />

        <div className="container mx-auto max-w-5xl text-center space-y-8 z-10">
          {/* Tag Pill */}
          <div className="inline-flex items-center gap-2 px-4 py-1.5 rounded-full glass-panel border-white/15 text-xs font-semibold text-brand-400">
            <Sparkles className="h-3.5 w-3.5 text-amber-400 animate-pulse" />
            <span>Next-Gen Vehicle Auction & Trading Platform</span>
          </div>

          {/* Heading */}
          <h1 className="text-4xl sm:text-6xl lg:text-7xl font-black tracking-tight text-foreground font-display">
            Acquire & Bid on <br />
            <span className="text-gradient">Elite Vehicles in Real-Time</span>
          </h1>

          <p className="mx-auto max-w-2xl text-base sm:text-lg text-muted-foreground leading-relaxed">
            Discover curated luxury supercars, rare classic collectibles, and daily drivers. Participate in sub-second live auctions with anti-sniping protection and instant chat.
          </p>

          {/* Search Bar */}
          <form
            onSubmit={handleSearch}
            className="mx-auto max-w-2xl flex flex-col sm:flex-row gap-2 p-2 rounded-2xl glass-panel border-white/15 shadow-2xl"
          >
            <Input
              type="text"
              placeholder="Search Porsche, Ferrari, BMW M3, Vintage 1969..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="h-12 border-none bg-transparent focus-visible:ring-0 text-base"
              icon={<Search className="h-5 w-5 text-muted-foreground" />}
            />
            <Button type="submit" variant="primary" size="lg" className="gap-2 shrink-0">
              <span>Find Vehicles</span>
              <ArrowRight className="h-4 w-4" />
            </Button>
          </form>
        </div>
      </section>

      {/* Feature Highlights Grid */}
      <section className="container mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div className="p-6 rounded-2xl glass-card border-white/10 space-y-3">
            <div className="h-12 w-12 rounded-xl bg-amber-500/10 flex items-center justify-center text-amber-400">
              <Gavel className="h-6 w-6" />
            </div>
            <h3 className="text-lg font-bold text-foreground">Sub-Second Live Bidding</h3>
            <p className="text-xs text-muted-foreground leading-relaxed">
              Experience real-time interactive auction bidding with automatic anti-sniping protection that extends countdowns on late bids.
            </p>
          </div>

          <div className="p-6 rounded-2xl glass-card border-white/10 space-y-3">
            <div className="h-12 w-12 rounded-xl bg-emerald-500/10 flex items-center justify-center text-emerald-400">
              <ShieldCheck className="h-6 w-6" />
            </div>
            <h3 className="text-lg font-bold text-foreground">Verified Ownership & Security</h3>
            <p className="text-xs text-muted-foreground leading-relaxed">
              Verified seller profiles, trusted vehicle history, and secure account protection ensuring peace of mind on every transaction.
            </p>
          </div>

          <div className="p-6 rounded-2xl glass-card border-white/10 space-y-3">
            <div className="h-12 w-12 rounded-xl bg-brand-500/10 flex items-center justify-center text-brand-400">
              <Zap className="h-6 w-6" />
            </div>
            <h3 className="text-lg font-bold text-foreground">Private Offers & Direct Chat</h3>
            <p className="text-xs text-muted-foreground leading-relaxed">
              Make discrete price offers to sellers or communicate in real-time through private 1-on-1 direct messaging and community car enthusiast clubs.
            </p>
          </div>
        </div>
      </section>

      {/* Live Auctions Section */}
      {activeAuctions && activeAuctions.length > 0 && (
        <section className="container mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 space-y-6">
          <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 border-b border-white/10 pb-4">
            <div>
              <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-amber-400">
                <span className="relative flex h-2 w-2">
                  <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-amber-400 opacity-75"></span>
                  <span className="relative inline-flex rounded-full h-2 w-2 bg-amber-500"></span>
                </span>
                <span>Active Floor</span>
              </div>
              <h2 className="text-2xl sm:text-3xl font-black text-foreground tracking-tight mt-1 font-display">
                Featured Live Auctions
              </h2>
            </div>

            <Link to="/auctions">
              <Button variant="outline" size="sm" className="gap-2">
                <span>View All Auctions</span>
                <ArrowRight className="h-4 w-4" />
              </Button>
            </Link>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {activeAuctions.map((auction) => (
              <AuctionCard key={auction.id} auction={auction} />
            ))}
          </div>
        </section>
      )}

      {/* Latest Listings Section */}
      <section className="container mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 space-y-6">
        <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 border-b border-white/10 pb-4">
          <div>
            <div className="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-brand-400">
              <TrendingUp className="h-3.5 w-3.5" />
              <span>Fresh Inventory</span>
            </div>
            <h2 className="text-2xl sm:text-3xl font-black text-foreground tracking-tight mt-1 font-display">
              Recently Listed Vehicles
            </h2>
          </div>

          <Link to="/listings">
            <Button variant="outline" size="sm" className="gap-2">
              <span>Explore Marketplace</span>
              <ArrowRight className="h-4 w-4" />
            </Button>
          </Link>
        </div>

        <ListingGrid listings={listingsData?.data} isLoading={listingsLoading} />
      </section>

      {/* Seller CTA Banner */}
      <section className="container mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="relative overflow-hidden rounded-3xl glass-panel border-white/15 p-8 sm:p-12 text-center space-y-6 bg-gradient-to-r from-brand-950/40 via-card/50 to-indigo-950/40">
          <div className="mx-auto flex h-14 w-14 items-center justify-center rounded-2xl bg-brand-600 text-white shadow-xl shadow-brand-600/30">
            <Car className="h-7 w-7" />
          </div>
          <h3 className="text-3xl sm:text-4xl font-black text-foreground font-display">
            Ready to Sell Your Vehicle?
          </h3>
          <p className="mx-auto max-w-xl text-sm sm:text-base text-muted-foreground">
            List your car as a direct sale or launch a competitive live auction with automated timer controls and instant buyer messaging.
          </p>
          <div className="flex justify-center gap-4 pt-2">
            <Link to="/listings/create">
              <Button variant="primary" size="lg" className="gap-2">
                <span>Submit Vehicle Now</span>
                <ArrowRight className="h-4 w-4" />
              </Button>
            </Link>
          </div>
        </div>
      </section>
    </div>
  );
}
