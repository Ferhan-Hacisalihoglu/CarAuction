export interface Auction {
  id: number;
  listingId: number;
  title?: string;
  description?: string;
  sellerId?: number;
  startingPrice: number;
  currentPrice: number;
  startTime: string;
  endTime: string;
  minBidIncrement: number;
  winnerUserId: number | null;
  status: 'active' | 'completed' | 'expired' | 'cancelled';
  imageId?: number | null;
  images?: import('./common').Image[];
}

export interface AuctionDetailResponse extends Auction {
  title: string;
  sellerId: number;
}

export interface AuctionResultDto {
  auctionId: number;
  winnerUserId: number;
  winnerName: string;
  winningPrice: number;
  status: string;
}

export interface AuctionExtendedDto {
  auctionId: number;
  newEndTime: string;
  extendedSeconds: number;
}

export interface BidDto {
  auctionId: number;
  amount: number;
  bidderId: number;
  bidderName: string;
  timestamp: string;
}
