import { useParams, Link, useNavigate } from 'react-router-dom';
import { useListing, useDeleteListing } from '@/hooks/useListings';
import { useAuctionByListing } from '@/hooks/useAuctions';
import { useAuth } from '@/hooks/useAuth';
import { useStartConversation } from '@/hooks/useDirectMessages';
import { ImageGallery } from '@/components/listings/ImageGallery';
import { OfferForm } from '@/components/bids/OfferForm';
import { LiveBidPanel } from '@/components/auctions/LiveBidPanel';
import { LoadingSpinner } from '@/components/common/LoadingSpinner';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import { ConfirmDialog } from '@/components/common/ConfirmDialog';
import { formatCurrency, formatDate } from '@/utils/format';
import { toast } from '@/hooks/useToast';
import { useState } from 'react';
import {
  Car,
  Gavel,
  Calendar,
  User,
  MessageSquare,
  Edit,
  Trash2,
  ArrowLeft,
  ShieldCheck,
} from 'lucide-react';

export function ListingDetailPage() {
  const { id } = useParams<{ id: string }>();
  const listingId = Number(id);
  const navigate = useNavigate();
  const { user, isAuthenticated, isAdmin } = useAuth();

  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);

  const { data: listing, isLoading, isError } = useListing(listingId);
  const { data: auction } = useAuctionByListing(listingId);

  const deleteListingMutation = useDeleteListing();
  const startConversationMutation = useStartConversation();

  const isOwner = user && listing && user.id === listing.userId;
  const canManage = isOwner || isAdmin;

  const handleDelete = async () => {
    try {
      await deleteListingMutation.mutateAsync(listingId);
      toast.success('Vehicle listing cancelled successfully', 'Listing Deleted');
      navigate('/my-listings');
    } catch (err: any) {
      toast.error('Failed to cancel listing', 'Delete Error');
    }
  };

  const handleContactSeller = async () => {
    if (!isAuthenticated) {
      toast.error('Please log in to contact the seller', 'Login Required');
      navigate('/login');
      return;
    }

    if (isOwner) {
      toast.info('This is your own listing!');
      return;
    }

    try {
      const conv = await startConversationMutation.mutateAsync(listing!.userId);
      navigate(`/messages/${conv.id}`);
    } catch (err: any) {
      toast.error('Failed to start chat with seller', 'Chat Error');
    }
  };

  if (isLoading) {
    return <LoadingSpinner size="lg" message="Loading vehicle details..." className="min-h-[50vh]" />;
  }

  if (isError || !listing) {
    return (
      <div className="container mx-auto max-w-4xl py-20 text-center space-y-4">
        <h2 className="text-2xl font-bold">Vehicle Not Found</h2>
        <p className="text-muted-foreground">This listing may have been sold, removed, or does not exist.</p>
        <Link to="/listings">
          <Button variant="primary">Return to Marketplace</Button>
        </Link>
      </div>
    );
  }

  return (
    <div className="container mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8 space-y-8">
      {/* Back button & Breadcrumbs */}
      <div className="flex items-center justify-between">
        <button
          onClick={() => navigate(-1)}
          className="inline-flex items-center gap-2 text-xs font-semibold text-muted-foreground hover:text-foreground transition-colors"
        >
          <ArrowLeft className="h-4 w-4" />
          <span>Back to Results</span>
        </button>

        {canManage && (
          <div className="flex items-center gap-2">
            <Link to={`/listings/${listing.id}/edit`}>
              <Button variant="outline" size="sm" className="gap-1.5 text-xs">
                <Edit className="h-3.5 w-3.5" />
                <span>Edit Vehicle</span>
              </Button>
            </Link>
            <Button
              variant="destructive"
              size="sm"
              className="gap-1.5 text-xs"
              onClick={() => setIsDeleteDialogOpen(true)}
            >
              <Trash2 className="h-3.5 w-3.5" />
              <span>Cancel Listing</span>
            </Button>
          </div>
        )}
      </div>

      {/* Main Details Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">
        {/* Left Col: Images & Description (8 cols) */}
        <div className="lg:col-span-7 xl:col-span-8 space-y-6">
          {/* Gallery */}
          <ImageGallery images={listing.images} />

          {/* Details Card */}
          <Card className="glass-card p-6 space-y-6">
            <div className="space-y-2">
              <div className="flex flex-wrap items-center gap-2">
                {listing.isAuction ? (
                  <Badge variant="gold" className="gap-1">
                    <Gavel className="h-3.5 w-3.5" />
                    <span>Live Auction</span>
                  </Badge>
                ) : (
                  <Badge variant="default">Direct Sale</Badge>
                )}
                <Badge
                  variant={listing.status === 'active' ? 'success' : 'secondary'}
                  className="capitalize"
                >
                  {listing.status}
                </Badge>
              </div>

              <h1 className="text-2xl sm:text-4xl font-black text-foreground font-display tracking-tight">
                {listing.title}
              </h1>

              <div className="flex items-center gap-4 text-xs text-muted-foreground pt-1">
                <span className="flex items-center gap-1.5">
                  <Calendar className="h-3.5 w-3.5" />
                  <span>Listed on {formatDate(listing.createdAt)}</span>
                </span>
                <span className="flex items-center gap-1.5">
                  <ShieldCheck className="h-3.5 w-3.5 text-emerald-400" />
                  <span>Verified Listing</span>
                </span>
              </div>
            </div>

            {/* Description */}
            <div className="border-t border-white/5 pt-5 space-y-2">
              <h3 className="text-sm font-bold uppercase tracking-wider text-muted-foreground">
                Vehicle Overview
              </h3>
              <p className="text-sm text-foreground/90 leading-relaxed whitespace-pre-wrap">
                {listing.description}
              </p>
            </div>
          </Card>
        </div>

        {/* Right Col: Price, Live Bid or Offer Panel, Seller Card (4-5 cols) */}
        <div className="lg:col-span-5 xl:col-span-4 space-y-6">
          {/* If Auction, show LiveBidPanel */}
          {listing.isAuction && auction ? (
            <LiveBidPanel auction={auction} />
          ) : (
            <>
              {/* Asking Price Card */}
              <Card className="glass-panel border-white/10 p-6 text-center space-y-3">
                <span className="text-xs uppercase font-semibold text-muted-foreground tracking-wider block">
                  Fixed Asking Price
                </span>
                <div className="text-4xl font-black text-foreground font-display tracking-tight">
                  {formatCurrency(listing.price)}
                </div>
                <div className="pt-2">
                  <Button
                    variant="primary"
                    size="lg"
                    className="w-full gap-2"
                    onClick={handleContactSeller}
                    isLoading={startConversationMutation.isPending}
                  >
                    <MessageSquare className="h-4 w-4" />
                    <span>Chat with Seller</span>
                  </Button>
                </div>
              </Card>

              {/* Offer Form */}
              <OfferForm listingId={listing.id} askingPrice={listing.price} />
            </>
          )}

          {/* Seller Card */}
          <Card className="glass-card p-5 space-y-4">
            <h4 className="text-xs font-bold uppercase tracking-wider text-muted-foreground">
              Seller Information
            </h4>
            <div className="flex items-center gap-3">
              <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-brand-600 text-white font-bold">
                <User className="h-6 w-6" />
              </div>
              <div className="flex-1 min-w-0">
                <p className="text-sm font-bold text-foreground truncate">
                  User #{listing.userId}
                </p>
                <p className="text-xs text-muted-foreground">Verified CarAuction Member</p>
              </div>
              <Link to={`/users/${listing.userId}`}>
                <Button variant="outline" size="sm" className="text-xs">
                  Profile
                </Button>
              </Link>
            </div>
          </Card>
        </div>
      </div>

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={isDeleteDialogOpen}
        onClose={() => setIsDeleteDialogOpen(false)}
        onConfirm={handleDelete}
        title="Cancel Vehicle Listing"
        message="Are you sure you want to cancel this listing? The vehicle will be marked as cancelled and removed from active search."
        confirmLabel="Yes, Cancel Listing"
        isDestructive
        isLoading={deleteListingMutation.isPending}
      />
    </div>
  );
}
