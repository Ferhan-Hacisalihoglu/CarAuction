import { Link } from 'react-router-dom';
import { Auction } from '@/types/auction';
import { formatCurrency } from '@/utils/format';
import { AuctionTimer } from './AuctionTimer';
import { Card, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { Gavel, Car, ArrowUpRight } from 'lucide-react';
import { imagesApi } from '@/api/images';

export function AuctionCard({ auction }: { auction: Auction }) {
  const isCompleted = auction.status === 'completed' || auction.status === 'expired';
  const imageId = auction.imageId || (auction.images && auction.images.length > 0 ? auction.images[0].id : null);
  const mainImageUrl = imageId ? imagesApi.getImageUrl(imageId) : null;

  return (
    <Link to={`/auctions/${auction.id}`} className="group block">
      <Card className="overflow-hidden glass-card transition-all duration-300 hover:translate-y-[-4px] hover:shadow-xl hover:shadow-amber-500/10 border-white/10">
        {/* Banner Area */}
        <div className="relative aspect-[16/10] w-full overflow-hidden bg-muted/40">
          {mainImageUrl ? (
            <img
              src={mainImageUrl}
              alt={auction.title || `Auction Lot #${auction.id}`}
              className="h-full w-full object-cover transition-transform duration-500 group-hover:scale-105"
            />
          ) : (
            <div className="flex h-full w-full items-center justify-center bg-gradient-to-br from-slate-900 via-slate-800 to-indigo-950 text-muted-foreground/30">
              <Car className="h-16 w-16 group-hover:scale-105 transition-transform" />
            </div>
          )}

          {/* Badges Overlay */}
          <div className="absolute top-3 left-3 flex items-center gap-1.5">
            <Badge variant="gold" className="font-bold flex items-center gap-1">
              <Gavel className="h-3 w-3" />
              <span>Auction #{auction.id}</span>
            </Badge>
            <Badge
              variant={auction.status === 'active' ? 'success' : 'secondary'}
              className="capitalize"
            >
              {auction.status}
            </Badge>
          </div>

          {/* Live Timer Overlay */}
          <div className="absolute bottom-3 left-3">
            <AuctionTimer endTime={auction.endTime} size="sm" />
          </div>

          <div className="absolute bottom-2 right-2 flex items-center justify-center h-8 w-8 rounded-full bg-black/60 text-white backdrop-blur-md opacity-0 group-hover:opacity-100 transition-opacity">
            <ArrowUpRight className="h-4 w-4" />
          </div>
        </div>

        {/* Card Content */}
        <CardContent className="p-5">
          <h4 className="text-base font-bold text-foreground truncate group-hover:text-amber-400 transition-colors">
            {auction.title || `Vehicle Lot #${auction.listingId}`}
          </h4>

          {auction.description && (
            <p className="mt-1 text-xs text-muted-foreground line-clamp-2 leading-relaxed">
              {auction.description}
            </p>
          )}

          <div className="mt-4 flex items-end justify-between border-t border-white/5 pt-3">
            <div>
              <span className="text-[10px] uppercase font-semibold text-muted-foreground tracking-wider block">
                Current Bid
              </span>
              <span className="text-xl font-extrabold text-amber-400 tracking-tight font-display">
                {formatCurrency(auction.currentPrice)}
              </span>
            </div>

            <Button
              variant={isCompleted ? 'secondary' : 'gold'}
              size="sm"
              className="pointer-events-none"
            >
              {isCompleted ? 'View Result' : 'Bid Now'}
            </Button>
          </div>
        </CardContent>
      </Card>
    </Link>
  );
}
