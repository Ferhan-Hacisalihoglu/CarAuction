export interface Conversation {
  id: number;
  user1Id: number;
  user2Id: number;
  otherUserFirstName?: string;
  otherUserLastName?: string;
  otherFirstName?: string;
  otherLastName?: string;
  lastMessage?: string;
  isRead?: boolean;
  createdAt: string;
}

export interface DmMessage {
  id: number;
  conversationId: number;
  senderId: number;
  content: string;
  sentAt: string;
  isRead: boolean;
}

export interface DmMessageDto {
  id: number;
  conversationId: number;
  senderId: number;
  senderName?: string;
  content: string;
  sentAt: string;
  isRead: boolean;
}

export interface StartConversationRequest {
  targetUserId: number;
}
