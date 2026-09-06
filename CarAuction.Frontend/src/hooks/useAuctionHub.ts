import { useEffect } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { signalRService } from '@/api/signalr';
import { AuctionExtendedDto, AuctionResultDto, BidDto } from '@/types/auction';
import { toast } from './useToast';

export function useAuctionHub(auctionId?: number) {
  const queryClient = useQueryClient();

  useEffect(() => {
    if (!auctionId) return;

    signalRService.joinAuction(auctionId);

    const unsubBid = signalRService.onNewBid((bid: BidDto) => {
      if (bid.auctionId === auctionId) {
        // Update auction query cache
        queryClient.setQueryData(['auction', auctionId], (old: any) => {
          if (!old) return old;
          return {
            ...old,
            currentPrice: bid.amount,
            winnerUserId: bid.bidderId,
          };
        });

        // Invalidate bids history
        queryClient.invalidateQueries({ queryKey: ['auction-bids', auctionId] });

        toast.info(
          `New bid of $${bid.amount.toLocaleString()} placed by ${bid.bidderName || 'a bidder'}!`,
          'Live Auction Update'
        );
      }
    });

    const unsubEnded = signalRService.onAuctionEnded((res: AuctionResultDto) => {
      if (res.auctionId === auctionId) {
        queryClient.invalidateQueries({ queryKey: ['auction', auctionId] });
        toast.success(
          `Auction ended! Winner: ${res.winnerName || 'Winner'} for $${res.winningPrice.toLocaleString()}`,
          'Auction Concluded'
        );
      }
    });

    const unsubExtended = signalRService.onAuctionExtended((ext: AuctionExtendedDto) => {
      if (ext.auctionId === auctionId) {
        queryClient.setQueryData(['auction', auctionId], (old: any) => {
          if (!old) return old;
          return {
            ...old,
            endTime: ext.newEndTime,
          };
        });
        toast.warning(
          `Anti-snipe triggered! Auction extended by ${ext.extendedSeconds}s!`,
          'Time Extended'
        );
      }
    });

    return () => {
      signalRService.leaveAuction(auctionId);
      unsubBid();
      unsubEnded();
      unsubExtended();
    };
  }, [auctionId, queryClient]);
}
