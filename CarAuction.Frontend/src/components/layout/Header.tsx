import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import { useUIStore } from '@/stores/uiStore';
import { Button } from '@/components/ui/Button';
import { Avatar } from '@/components/ui/Avatar';
import { DropdownMenu, DropdownItem } from '@/components/ui/DropdownMenu';
import {
  Car,
  Gavel,
  Users,
  MessageSquare,
  PlusCircle,
  ShieldAlert,
  LogOut,
  User as UserIcon,
  Sun,
  Moon,
  Menu,
  X,
  Package,
  HeartHandshake,
} from 'lucide-react';

export function Header() {
  const { user, isAuthenticated, isAdmin, logout } = useAuth();
  const { theme, toggleTheme, sidebarOpen, toggleSidebar } = useUIStore();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/');
  };

  return (
    <header className="sticky top-0 z-40 w-full border-b border-white/10 bg-background/80 backdrop-blur-xl transition-all">
      <div className="container mx-auto flex h-16 max-w-7xl items-center justify-between px-4 sm:px-6 lg:px-8">
        {/* Brand Logo */}
        <div className="flex items-center gap-6">
          <Link to="/" className="flex items-center gap-2.5 group">
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-gradient-to-tr from-brand-600 to-indigo-600 text-white shadow-lg shadow-brand-600/30 group-hover:scale-105 transition-transform">
              <Car className="h-5 w-5" />
            </div>
            <div className="flex flex-col">
              <span className="text-xl font-black tracking-tight text-foreground font-display">
                CAR<span className="text-brand-500">AUCTION</span>
              </span>
              <span className="text-[10px] tracking-widest font-semibold uppercase text-muted-foreground -mt-1">
                Luxury & Live Bids
              </span>
            </div>
          </Link>

          {/* Desktop Nav */}
          <nav className="hidden md:flex items-center gap-1 text-sm font-medium">
            <Link
              to="/listings"
              className="flex items-center gap-1.5 px-3 py-2 rounded-xl text-muted-foreground hover:text-foreground hover:bg-white/5 transition-colors"
            >
              <Car className="h-4 w-4" />
              <span>Browse Cars</span>
            </Link>
            <Link
              to="/auctions"
              className="flex items-center gap-1.5 px-3 py-2 rounded-xl text-amber-400 hover:text-amber-300 hover:bg-amber-500/10 transition-colors font-semibold"
            >
              <Gavel className="h-4 w-4" />
              <span>Live Auctions</span>
              <span className="relative flex h-2 w-2">
                <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-amber-400 opacity-75"></span>
                <span className="relative inline-flex rounded-full h-2 w-2 bg-amber-500"></span>
              </span>
            </Link>
            <Link
              to="/groups"
              className="flex items-center gap-1.5 px-3 py-2 rounded-xl text-muted-foreground hover:text-foreground hover:bg-white/5 transition-colors"
            >
              <Users className="h-4 w-4" />
              <span>Clubs</span>
            </Link>
            {isAuthenticated && (
              <Link
                to="/messages"
                className="flex items-center gap-1.5 px-3 py-2 rounded-xl text-muted-foreground hover:text-foreground hover:bg-white/5 transition-colors"
              >
                <MessageSquare className="h-4 w-4" />
                <span>Chat</span>
              </Link>
            )}
            {isAdmin && (
              <Link
                to="/admin"
                className="flex items-center gap-1.5 px-3 py-2 rounded-xl text-purple-400 hover:text-purple-300 hover:bg-purple-500/10 transition-colors font-semibold"
              >
                <ShieldAlert className="h-4 w-4" />
                <span>Admin</span>
              </Link>
            )}
          </nav>
        </div>

        {/* Right Actions */}
        <div className="flex items-center gap-3">
          {/* Theme toggle */}
          <button
            onClick={toggleTheme}
            className="rounded-xl p-2 text-muted-foreground hover:bg-accent hover:text-foreground transition-colors"
            title="Toggle Theme"
          >
            {theme === 'dark' ? <Sun className="h-5 w-5" /> : <Moon className="h-5 w-5" />}
          </button>

          {isAuthenticated ? (
            <>
              <Link to="/listings/create" className="hidden sm:inline-flex">
                <Button variant="primary" size="sm" className="gap-1.5 font-semibold">
                  <PlusCircle className="h-4 w-4" />
                  <span>Sell Vehicle</span>
                </Button>
              </Link>

              {/* User Dropdown */}
              <DropdownMenu
                trigger={
                  <div className="flex items-center gap-2 p-1 rounded-xl hover:bg-white/5 transition-colors">
                    <Avatar name={`${user?.firstName} ${user?.lastName}`} size="sm" isOnline />
                    <span className="hidden lg:inline text-sm font-semibold text-foreground max-w-[120px] truncate">
                      {user?.firstName}
                    </span>
                  </div>
                }
              >
                <div className="px-3 py-2 border-b border-border/50">
                  <p className="text-sm font-bold text-foreground truncate">
                    {user?.firstName} {user?.lastName}
                  </p>
                  <p className="text-xs text-muted-foreground truncate">{user?.email}</p>
                  {user?.roleName && (
                    <span className="inline-block mt-1 text-[10px] uppercase font-bold tracking-wider px-2 py-0.5 rounded-full bg-brand-500/20 text-brand-400">
                      {user.roleName}
                    </span>
                  )}
                </div>

                <div className="py-1">
                  <DropdownItem onClick={() => navigate('/profile')}>
                    <UserIcon className="h-4 w-4" />
                    <span>My Profile</span>
                  </DropdownItem>
                  <DropdownItem onClick={() => navigate('/my-listings')}>
                    <Package className="h-4 w-4" />
                    <span>My Listings</span>
                  </DropdownItem>
                  <DropdownItem onClick={() => navigate('/my-bids')}>
                    <Gavel className="h-4 w-4" />
                    <span>My Bids</span>
                  </DropdownItem>
                  <DropdownItem onClick={() => navigate('/my-offers')}>
                    <HeartHandshake className="h-4 w-4" />
                    <span>My Offers</span>
                  </DropdownItem>
                  {isAdmin && (
                    <DropdownItem onClick={() => navigate('/admin')}>
                      <ShieldAlert className="h-4 w-4 text-purple-400" />
                      <span className="text-purple-400 font-semibold">Admin Panel</span>
                    </DropdownItem>
                  )}
                </div>

                <div className="pt-1 border-t border-border/50">
                  <DropdownItem onClick={handleLogout} destructive>
                    <LogOut className="h-4 w-4" />
                    <span>Sign Out</span>
                  </DropdownItem>
                </div>
              </DropdownMenu>
            </>
          ) : (
            <div className="flex items-center gap-2">
              <Link to="/login">
                <Button variant="ghost" size="sm">
                  Log In
                </Button>
              </Link>
              <Link to="/register">
                <Button variant="primary" size="sm">
                  Register
                </Button>
              </Link>
            </div>
          )}

          {/* Mobile hamburger */}
          <button
            onClick={toggleSidebar}
            className="md:hidden rounded-xl p-2 text-muted-foreground hover:bg-accent hover:text-foreground transition-colors"
          >
            {sidebarOpen ? <X className="h-6 w-6" /> : <Menu className="h-6 w-6" />}
          </button>
        </div>
      </div>
    </header>
  );
}
