import { api } from './axios';
import { Listing, CreateListingRequest, UpdateListingRequest, ListingFilters } from '@/types/listing';
import { PaginatedResponse } from '@/types/common';

export const listingsApi = {
  getAll: async (params?: Partial<ListingFilters>) => {
    const res = await api.get<PaginatedResponse<Listing>>('/listings', { params });
    return res.data;
  },
  getById: async (id: number) => {
    const res = await api.get<Listing>(`/listings/${id}`);
    return res.data;
  },
  create: async (data: CreateListingRequest) => {
    const res = await api.post<Listing>('/listings', data);
    return res.data;
  },
  update: async (id: number, data: UpdateListingRequest) => {
    const res = await api.put<Listing>(`/listings/${id}`, data);
    return res.data;
  },
  delete: async (id: number) => {
    const res = await api.delete(`/listings/${id}`);
    return res.data;
  },
  getMyListings: async () => {
    const res = await api.get<Listing[]>('/listings/my');
    return res.data;
  },
};
