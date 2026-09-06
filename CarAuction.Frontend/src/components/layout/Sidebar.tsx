import { Link } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import { useUIStore } from '@/stores/uiStore';
import {
  Car,
  Gavel,
  Users,
  MessageSquare,
  PlusCircle,
  ShieldAlert,
  X,
  Package,
  HeartHandshake,
  User,
} from 'lucide-react';
import { Button } from '@/components/ui/Button';

export function Sidebar() {
  const { isAuthenticated, isAdmin, logout } = useAuth();
  const { sidebarOpen, setSidebarOpen } = useUIStore();

  if (!sidebarOpen) return null;

  return (
    <div className="fixed inset-0 z-50 md:hidden flex">
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black/80 backdrop-blur-sm transition-opacity"
        onClick={() => setSidebarOpen(false)}
      />

      {/* Drawer */}
      <div className="relative ml-auto flex h-full w-4/5 max-w-xs flex-col bg-card/95 border-l border-white/10 p-6 shadow-2xl backdrop-blur-xl animate-fade-in">
        <div className="flex items-center justify-between pb-6 border-b border-white/10">
          <Link
            to="/"
            onClick={() => setSidebarOpen(false)}
            className="flex items-center gap-2"
          >
            <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-brand-600 text-white font-bold">
              <Car className="h-4 w-4" />
            </div>
            <span className="font-bold text-foreground">CARAUCTION</span>
          </Link>
          <button
            onClick={() => setSidebarOpen(false)}
            className="rounded-lg p-1.5 text-muted-foreground hover:bg-secondary"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        <nav className="mt-6 flex flex-col space-y-2 text-sm font-medium">
          <Link
            to="/listings"
            onClick={() => setSidebarOpen(false)}
            className="flex items-center gap-3 px-3 py-2.5 rounded-xl hover:bg-accent text-muted-foreground hover:text-foreground"
          >
            <Car className="h-5 w-5" />
            <span>Browse Vehicles</span>
          </Link>
          <Link
            to="/auctions"
            onClick={() => setSidebarOpen(false)}
            className="flex items-center gap-3 px-3 py-2.5 rounded-xl hover:bg-amber-500/10 text-amber-400 font-semibold"
          >
            <Gavel className="h-5 w-5" />
            <span>Live Auctions</span>
          </Link>
          <Link
            to="/groups"
            onClick={() => setSidebarOpen(false)}
            className="flex items-center gap-3 px-3 py-2.5 rounded-xl hover:bg-accent text-muted-foreground hover:text-foreground"
          >
            <Users className="h-5 w-5" />
            <span>Clubs & Groups</span>
          </Link>
          {isAuthenticated && (
            <>
              <Link
                to="/messages"
                onClick={() => setSidebarOpen(false)}
                className="flex items-center gap-3 px-3 py-2.5 rounded-xl hover:bg-accent text-muted-foreground hover:text-foreground"
              >
                <MessageSquare className="h-5 w-5" />
                <span>Messages</span>
              </Link>
              <Link
                to="/my-listings"
                onClick={() => setSidebarOpen(false)}
                className="flex items-center gap-3 px-3 py-2.5 rounded-xl hover:bg-accent text-muted-foreground hover:text-foreground"
              >
                <Package className="h-5 w-5" />
                <span>My Listings</span>
              </Link>
              <Link
                to="/my-bids"
                onClick={() => setSidebarOpen(false)}
                className="flex items-center gap-3 px-3 py-2.5 rounded-xl hover:bg-accent text-muted-foreground hover:text-foreground"
              >
                <Gavel className="h-5 w-5" />
                <span>My Bids</span>
              </Link>
              <Link
                to="/my-offers"
                onClick={() => setSidebarOpen(false)}
                className="flex items-center gap-3 px-3 py-2.5 rounded-xl hover:bg-accent text-muted-foreground hover:text-foreground"
              >
                <HeartHandshake className="h-5 w-5" />
                <span>My Offers</span>
              </Link>
              <Link
                to="/profile"
                onClick={() => setSidebarOpen(false)}
                className="flex items-center gap-3 px-3 py-2.5 rounded-xl hover:bg-accent text-muted-foreground hover:text-foreground"
              >
                <User className="h-5 w-5" />
                <span>Profile</span>
              </Link>
              {isAdmin && (
                <Link
                  to="/admin"
                  onClick={() => setSidebarOpen(false)}
                  className="flex items-center gap-3 px-3 py-2.5 rounded-xl hover:bg-purple-500/10 text-purple-400 font-semibold"
                >
                  <ShieldAlert className="h-5 w-5" />
                  <span>Admin Panel</span>
                </Link>
              )}
            </>
          )}
        </nav>

        <div className="mt-auto pt-6 border-t border-white/10 space-y-3">
          {isAuthenticated ? (
            <>
              <Link
                to="/listings/create"
                onClick={() => setSidebarOpen(false)}
                className="w-full block"
              >
                <Button variant="primary" className="w-full gap-2">
                  <PlusCircle className="h-4 w-4" />
                  <span>Sell Vehicle</span>
                </Button>
              </Link>
              <Button
                variant="outline"
                className="w-full"
                onClick={async () => {
                  setSidebarOpen(false);
                  await logout();
                }}
              >
                Sign Out
              </Button>
            </>
          ) : (
            <>
              <Link to="/login" onClick={() => setSidebarOpen(false)} className="w-full block">
                <Button variant="outline" className="w-full">
                  Log In
                </Button>
              </Link>
              <Link to="/register" onClick={() => setSidebarOpen(false)} className="w-full block">
                <Button variant="primary" className="w-full">
                  Create Account
                </Button>
              </Link>
            </>
          )}
        </div>
      </div>
    </div>
  );
}
