import { api } from './axios';
import { Auction, AuctionDetailResponse } from '@/types/auction';
import { Bid } from '@/types/bid';

export const auctionsApi = {
  getAll: async () => {
    const res = await api.get<Auction[]>('/auctions');
    return res.data;
  },
  getById: async (id: number) => {
    const res = await api.get<AuctionDetailResponse>(`/auctions/${id}`);
    return res.data;
  },
  getByListingId: async (listingId: number) => {
    const res = await api.get<Auction>(`/auctions/listing/${listingId}`);
    return res.data;
  },
  placeBid: async (id: number, amount: number, idempotencyKey?: string) => {
    const headers: Record<string, string> = {};
    if (idempotencyKey) {
      headers['X-Idempotency-Key'] = idempotencyKey;
    }
    const res = await api.post<AuctionDetailResponse>(
      `/auctions/${id}/bid`,
      { amount },
      { headers }
    );
    return res.data;
  },
  getBidHistory: async (id: number) => {
    const res = await api.get<Bid[]>(`/auctions/${id}/bids`);
    return res.data;
  },
};
