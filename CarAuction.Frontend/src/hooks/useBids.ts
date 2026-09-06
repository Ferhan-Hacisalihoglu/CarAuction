import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { bidsApi } from '@/api/bids';

export function useListingOffers(listingId: number) {
  return useQuery({
    queryKey: ['listing-offers', listingId],
    queryFn: () => bidsApi.getOffers(listingId),
    enabled: !isNaN(listingId) && listingId > 0,
  });
}

export function useMyBids() {
  return useQuery({
    queryKey: ['my-bids'],
    queryFn: () => bidsApi.getMyBids(),
  });
}

export function useMakeOffer() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ listingId, amount }: { listingId: number; amount: number }) =>
      bidsApi.makeOffer(listingId, amount),
    onSuccess: (_, { listingId }) => {
      queryClient.invalidateQueries({ queryKey: ['listing-offers', listingId] });
      queryClient.invalidateQueries({ queryKey: ['my-bids'] });
    },
  });
}

export function useAcceptOffer() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (offerId: number) => bidsApi.acceptOffer(offerId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['listing-offers'] });
      queryClient.invalidateQueries({ queryKey: ['my-listings'] });
      queryClient.invalidateQueries({ queryKey: ['listings'] });
    },
  });
}

export function useRejectOffer() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (offerId: number) => bidsApi.rejectOffer(offerId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['listing-offers'] });
    },
  });
}
