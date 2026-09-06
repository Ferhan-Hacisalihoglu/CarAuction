import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { adminApi } from '@/api/admin';
import { LoadingSpinner } from '@/components/common/LoadingSpinner';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { formatDate } from '@/utils/format';
import {
  ShieldAlert,
  Users,
  Car,
  Gavel,
  CheckCircle,
  Activity,
  ArrowRight,
  TrendingUp,
} from 'lucide-react';

export function AdminDashboardPage() {
  const { data: stats, isLoading: statsLoading } = useQuery({
    queryKey: ['admin-stats'],
    queryFn: () => adminApi.getStats(),
  });

  const { data: activity, isLoading: activityLoading } = useQuery({
    queryKey: ['admin-activity'],
    queryFn: () => adminApi.getActivity(),
  });

  if (statsLoading || activityLoading) {
    return <LoadingSpinner size="lg" message="Loading administrative intelligence..." className="min-h-[50vh]" />;
  }

  return (
    <div className="container mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8 space-y-8">
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
        <div>
          <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-purple-400">
            <ShieldAlert className="h-4 w-4" />
            <span>Management Center</span>
          </div>
          <h1 className="text-3xl font-black text-foreground tracking-tight mt-1 font-display">
            Administrator Dashboard
          </h1>
          <p className="text-xs sm:text-sm text-muted-foreground mt-1">
            System health, active live auctions, user moderation, and RBAC policies.
          </p>
        </div>

        <div className="flex items-center gap-3">
          <Link to="/admin/users">
            <Button variant="outline" size="sm" className="gap-2">
              <Users className="h-4 w-4" />
              <span>User Management</span>
            </Button>
          </Link>
          <Link to="/admin/roles">
            <Button variant="outline" size="sm" className="gap-2">
              <ShieldAlert className="h-4 w-4" />
              <span>Roles & Permissions</span>
            </Button>
          </Link>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4">
        <Card className="glass-card p-4">
          <span className="text-[11px] font-semibold text-muted-foreground uppercase block">
            Total Users
          </span>
          <div className="text-2xl font-black text-foreground mt-1 font-display">
            {stats?.totalUsers || 0}
          </div>
          <span className="text-[10px] text-emerald-400 font-semibold mt-1 block">
            +{stats?.newUsers24h || 0} in 24h
          </span>
        </Card>

        <Card className="glass-card p-4">
          <span className="text-[11px] font-semibold text-muted-foreground uppercase block">
            Total Listings
          </span>
          <div className="text-2xl font-black text-foreground mt-1 font-display">
            {stats?.totalListings || 0}
          </div>
          <span className="text-[10px] text-muted-foreground mt-1 block">
            All vehicles
          </span>
        </Card>

        <Card className="glass-card p-4">
          <span className="text-[11px] font-semibold text-muted-foreground uppercase block">
            Active Auctions
          </span>
          <div className="text-2xl font-black text-amber-400 mt-1 font-display">
            {stats?.activeAuctions || 0}
          </div>
          <span className="text-[10px] text-amber-400 font-semibold mt-1 block">
            Live bidding active
          </span>
        </Card>

        <Card className="glass-card p-4">
          <span className="text-[11px] font-semibold text-muted-foreground uppercase block">
            Sold Vehicles
          </span>
          <div className="text-2xl font-black text-blue-400 mt-1 font-display">
            {stats?.soldListings || 0}
          </div>
          <span className="text-[10px] text-blue-400 font-semibold mt-1 block">
            Completed lots
          </span>
        </Card>

        <Card className="glass-card p-4">
          <span className="text-[11px] font-semibold text-muted-foreground uppercase block">
            24h Bids
          </span>
          <div className="text-2xl font-black text-purple-400 mt-1 font-display">
            {stats?.totalBids24h || 0}
          </div>
          <span className="text-[10px] text-purple-400 font-semibold mt-1 block">
            Real-time volume
          </span>
        </Card>

        <Card className="glass-card p-4">
          <span className="text-[11px] font-semibold text-muted-foreground uppercase block">
            New Registrations
          </span>
          <div className="text-2xl font-black text-emerald-400 mt-1 font-display">
            {stats?.newUsers24h || 0}
          </div>
          <span className="text-[10px] text-emerald-400 font-semibold mt-1 block">
            Last 24 hours
          </span>
        </Card>
      </div>

      {/* Activity Feed */}
      <Card className="glass-panel border-white/10">
        <CardHeader className="border-b border-white/5 pb-4 flex flex-row items-center justify-between">
          <div className="flex items-center gap-2">
            <Activity className="h-5 w-5 text-brand-400" />
            <CardTitle className="text-base">Real-Time Platform Activity</CardTitle>
          </div>
          <span className="text-xs text-muted-foreground">Most recent 20 audit events</span>
        </CardHeader>

        <CardContent className="p-0 divide-y divide-white/5">
          {!activity || activity.length === 0 ? (
            <div className="p-6 text-center text-xs text-muted-foreground">No recent system activity.</div>
          ) : (
            activity.map((item, idx) => {
              const icon =
                item.type === 'new_user' ? (
                  <Users className="h-4 w-4 text-emerald-400" />
                ) : item.type === 'new_listing' ? (
                  <Car className="h-4 w-4 text-blue-400" />
                ) : (
                  <Gavel className="h-4 w-4 text-amber-400" />
                );

              const typeLabel =
                item.type === 'new_user'
                  ? 'New Member Joined'
                  : item.type === 'new_listing'
                  ? 'Vehicle Listed'
                  : 'Live Bid Placed';

              return (
                <div key={idx} className="flex items-center justify-between px-6 py-3.5 text-xs">
                  <div className="flex items-center gap-3">
                    <div className="h-8 w-8 rounded-xl bg-secondary flex items-center justify-center">
                      {icon}
                    </div>
                    <div>
                      <span className="font-semibold text-foreground">{typeLabel}: </span>
                      <span className="text-muted-foreground">{item.detail}</span>
                    </div>
                  </div>

                  <span className="text-muted-foreground text-[11px]">{formatDate(item.timestamp)}</span>
                </div>
              );
            })
          )}
        </CardContent>
      </Card>
    </div>
  );
}
