export interface PaginatedResponse<T> {
  data: T[];
  total: number;
  page: number;
  limit: number;
  totalPages: number;
}

export interface PaginationParams {
  page: number;
  limit: number;
}

export interface Image {
  id: number;
  listingId?: number;
  fileName: string;
  mimeType: string;
  uploadedAt: string;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

export interface AdminStats {
  totalUsers: number;
  totalListings: number;
  activeAuctions: number;
  soldListings: number;
  totalBids24h: number;
  newUsers24h: number;
}

export interface ActivityItem {
  type: 'new_user' | 'new_listing' | 'new_bid';
  detail: string;
  timestamp: string;
}
