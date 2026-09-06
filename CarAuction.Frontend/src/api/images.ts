import { api } from './axios';
import { Image } from '@/types/common';

export const imagesApi = {
  upload: async (listingId: number, file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    const res = await api.post<Image>(`/listings/${listingId}/images`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return res.data;
  },
  getImageUrl: (id: number) => {
    const baseUrl = import.meta.env.VITE_API_URL || '/api';
    return `${baseUrl}/images/${id}`;
  },
  delete: async (id: number) => {
    const res = await api.delete(`/images/${id}`);
    return res.data;
  },
};
