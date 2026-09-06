import { api } from './axios';
import { Bid, OfferResponse, MyBidResponse } from '@/types/bid';

export const bidsApi = {
  makeOffer: async (listingId: number, amount: number) => {
    const res = await api.post<OfferResponse>(`/listings/${listingId}/offers`, { amount });
    return res.data;
  },
  getOffers: async (listingId: number) => {
    const res = await api.get<OfferResponse[]>(`/listings/${listingId}/offers`);
    return res.data;
  },
  getMyBids: async () => {
    const res = await api.get<MyBidResponse[]>('/bids/my');
    return res.data;
  },
  acceptOffer: async (offerId: number) => {
    const res = await api.post<OfferResponse>(`/bids/offers/${offerId}/accept`);
    return res.data;
  },
  rejectOffer: async (offerId: number) => {
    const res = await api.post(`/bids/offers/${offerId}/reject`);
    return res.data;
  },
};
