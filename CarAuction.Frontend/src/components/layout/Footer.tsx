import { Link } from 'react-router-dom';
import { Car, ShieldCheck, Zap, MessageSquare } from 'lucide-react';

export function Footer() {
  return (
    <footer className="border-t border-white/10 bg-card/40 backdrop-blur-xl mt-auto">
      <div className="container mx-auto max-w-7xl px-4 py-12 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
          {/* Brand Col */}
          <div className="space-y-4 md:col-span-1">
            <Link to="/" className="flex items-center gap-2.5">
              <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-gradient-to-tr from-brand-600 to-indigo-600 text-white shadow-md">
                <Car className="h-5 w-5" />
              </div>
              <span className="text-lg font-black tracking-tight text-foreground">
                CAR<span className="text-brand-500">AUCTION</span>
              </span>
            </Link>
            <p className="text-xs text-muted-foreground leading-relaxed">
              The premier platform for high-performance vehicles, classic collectors, and live real-time auctions.
            </p>
            <div className="flex items-center gap-2 text-xs text-emerald-400 font-medium">
              <span className="relative flex h-2 w-2">
                <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-emerald-400 opacity-75"></span>
                <span className="relative inline-flex rounded-full h-2 w-2 bg-emerald-500"></span>
              </span>
              <span>All Systems Operational</span>
            </div>
          </div>

          {/* Quick Links */}
          <div className="space-y-3">
            <h4 className="text-sm font-bold text-foreground tracking-wider uppercase">Marketplace</h4>
            <ul className="space-y-2 text-xs text-muted-foreground">
              <li>
                <Link to="/listings" className="hover:text-foreground transition-colors">
                  All Vehicles
                </Link>
              </li>
              <li>
                <Link to="/auctions" className="hover:text-foreground transition-colors">
                  Live Auctions
                </Link>
              </li>
              <li>
                <Link to="/listings/create" className="hover:text-foreground transition-colors">
                  Submit a Vehicle
                </Link>
              </li>
              <li>
                <Link to="/groups" className="hover:text-foreground transition-colors">
                  Car Enthusiast Clubs
                </Link>
              </li>
            </ul>
          </div>

          {/* Account */}
          <div className="space-y-3">
            <h4 className="text-sm font-bold text-foreground tracking-wider uppercase">User Portal</h4>
            <ul className="space-y-2 text-xs text-muted-foreground">
              <li>
                <Link to="/my-listings" className="hover:text-foreground transition-colors">
                  My Garage Listings
                </Link>
              </li>
              <li>
                <Link to="/my-bids" className="hover:text-foreground transition-colors">
                  My Active Bids
                </Link>
              </li>
              <li>
                <Link to="/my-offers" className="hover:text-foreground transition-colors">
                  Received Offers
                </Link>
              </li>
              <li>
                <Link to="/profile" className="hover:text-foreground transition-colors">
                  Account Settings
                </Link>
              </li>
            </ul>
          </div>

          {/* Platform Features */}
          <div className="space-y-3">
            <h4 className="text-sm font-bold text-foreground tracking-wider uppercase">Platform Features</h4>
            <div className="space-y-2 text-xs text-muted-foreground">
              <div className="flex items-center gap-2">
                <Zap className="h-4 w-4 text-amber-400" />
                <span>Sub-second live bidding with anti-sniping</span>
              </div>
              <div className="flex items-center gap-2">
                <ShieldCheck className="h-4 w-4 text-emerald-400" />
                <span>Verified seller profiles & secure accounts</span>
              </div>
              <div className="flex items-center gap-2">
                <MessageSquare className="h-4 w-4 text-blue-400" />
                <span>Direct 1-on-1 chat & enthusiast clubs</span>
              </div>
            </div>
          </div>
        </div>

        <div className="mt-10 border-t border-white/5 pt-6 flex flex-col sm:flex-row items-center justify-between text-xs text-muted-foreground">
          <p>© {new Date().getFullYear()} CarAuction Platform. All rights reserved.</p>
        </div>
      </div>
    </footer>
  );
}
