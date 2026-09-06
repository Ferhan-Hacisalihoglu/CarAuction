import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { auctionsApi } from '@/api/auctions';

export function useAuctions() {
  return useQuery({
    queryKey: ['auctions'],
    queryFn: () => auctionsApi.getAll(),
  });
}

export function useAuction(id: number) {
  return useQuery({
    queryKey: ['auction', id],
    queryFn: () => auctionsApi.getById(id),
    enabled: !isNaN(id) && id > 0,
  });
}

export function useAuctionByListing(listingId: number) {
  return useQuery({
    queryKey: ['auction-listing', listingId],
    queryFn: () => auctionsApi.getByListingId(listingId),
    enabled: !isNaN(listingId) && listingId > 0,
  });
}

export function useAuctionBids(id: number) {
  return useQuery({
    queryKey: ['auction-bids', id],
    queryFn: () => auctionsApi.getBidHistory(id),
    enabled: !isNaN(id) && id > 0,
  });
}

export function usePlaceBid() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      auctionId,
      amount,
      idempotencyKey,
    }: {
      auctionId: number;
      amount: number;
      idempotencyKey?: string;
    }) => auctionsApi.placeBid(auctionId, amount, idempotencyKey),
    onSuccess: (_, { auctionId }) => {
      queryClient.invalidateQueries({ queryKey: ['auction', auctionId] });
      queryClient.invalidateQueries({ queryKey: ['auction-bids', auctionId] });
      queryClient.invalidateQueries({ queryKey: ['my-bids'] });
    },
  });
}
