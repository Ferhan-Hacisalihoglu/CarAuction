export interface Bid {
  id: number;
  listingId: number;
  userId: number;
  userName?: string;
  amount: number;
  createdAt: string;
}

export interface OfferResponse {
  id: number;
  listingId: number;
  listingTitle?: string;
  userId: number;
  userName?: string;
  amount: number;
  createdAt: string;
  status?: 'pending' | 'accepted' | 'rejected';
}

export interface MyBidResponse {
  id: number;
  listingId: number;
  listingTitle: string;
  listingPrice: number;
  listingStatus: string;
  amount: number;
  createdAt: string;
}

export interface CreateOfferRequest {
  amount: number;
}

export interface PlaceBidRequest {
  amount: number;
}
