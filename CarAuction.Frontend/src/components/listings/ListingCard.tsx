import { Link } from 'react-router-dom';
import { Listing } from '@/types/listing';
import { formatCurrency, formatDate } from '@/utils/format';
import { Badge } from '@/components/ui/Badge';
import { Card, CardContent } from '@/components/ui/Card';
import { Car, Gavel, ArrowUpRight } from 'lucide-react';
import { imagesApi } from '@/api/images';

export function ListingCard({ listing }: { listing: Listing }) {
  const hasImages = listing.images && listing.images.length > 0;
  const mainImageUrl = hasImages ? imagesApi.getImageUrl(listing.images![0].id) : null;

  const targetLink = listing.isAuction ? `/auctions/${listing.id}` : `/listings/${listing.id}`;

  return (
    <Link to={targetLink} className="group block">
      <Card className="overflow-hidden glass-card transition-all duration-300 hover:translate-y-[-4px] hover:shadow-xl hover:shadow-brand-600/10 border-white/10">
        {/* Image / Banner */}
        <div className="relative aspect-[16/10] w-full overflow-hidden bg-muted/40">
          {mainImageUrl ? (
            <img
              src={mainImageUrl}
              alt={listing.title}
              className="h-full w-full object-cover transition-transform duration-500 group-hover:scale-105"
            />
          ) : (
            <div className="flex h-full w-full items-center justify-center bg-gradient-to-br from-slate-900 to-slate-800 text-muted-foreground/30">
              <Car className="h-16 w-16" />
            </div>
          )}

          {/* Badges Overlay */}
          <div className="absolute top-3 left-3 flex items-center gap-1.5">
            {listing.isAuction ? (
              <Badge variant="gold" className="flex items-center gap-1 font-bold backdrop-blur-md">
                <Gavel className="h-3 w-3" />
                <span>Live Auction</span>
              </Badge>
            ) : (
              <Badge variant="default" className="backdrop-blur-md">
                Direct Sale
              </Badge>
            )}
            <Badge
              variant={
                listing.status === 'active'
                  ? 'success'
                  : listing.status === 'sold'
                  ? 'secondary'
                  : 'warning'
              }
              className="backdrop-blur-md capitalize"
            >
              {listing.status}
            </Badge>
          </div>

          <div className="absolute bottom-2 right-2 flex items-center justify-center h-8 w-8 rounded-full bg-black/60 text-white backdrop-blur-md opacity-0 group-hover:opacity-100 transition-opacity">
            <ArrowUpRight className="h-4 w-4" />
          </div>
        </div>

        {/* Content */}
        <CardContent className="p-5">
          <div className="flex items-baseline justify-between gap-2">
            <h4 className="text-base font-bold text-foreground truncate group-hover:text-brand-400 transition-colors">
              {listing.title}
            </h4>
          </div>

          <p className="mt-1 text-xs text-muted-foreground line-clamp-2 leading-relaxed">
            {listing.description}
          </p>

          <div className="mt-4 flex items-center justify-between border-t border-white/5 pt-3">
            <div>
              <span className="text-[10px] uppercase font-semibold text-muted-foreground tracking-wider block">
                {listing.isAuction ? 'Current / Start' : 'Asking Price'}
              </span>
              <span className="text-lg font-extrabold text-foreground tracking-tight font-display">
                {formatCurrency(listing.price)}
              </span>
            </div>

            <span className="text-[11px] text-muted-foreground">
              {formatDate(listing.createdAt)}
            </span>
          </div>
        </CardContent>
      </Card>
    </Link>
  );
}
