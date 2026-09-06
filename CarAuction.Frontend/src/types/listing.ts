import { Image } from './common';

export interface Listing {
  id: number;
  userId: number;
  title: string;
  description: string;
  price: number;
  isAuction: boolean;
  status: 'active' | 'sold' | 'expired' | 'cancelled';
  createdAt: string;
  updatedAt?: string;
  images?: Image[];
}

export interface CreateListingRequest {
  title: string;
  description: string;
  price: number;
  isAuction: boolean;
  startingPrice?: number;
  startTime?: string;
  endTime?: string;
  minBidIncrement?: number;
}

export interface UpdateListingRequest {
  title: string;
  description: string;
  price: number;
}

export interface ListingFilters {
  search?: string;
  minPrice?: number;
  maxPrice?: number;
  status?: string;
  isAuction?: boolean;
  page: number;
  limit: number;
}
