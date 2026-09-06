export interface Group {
  id: number;
  name: string;
  description: string;
  createdBy: number;
  createdAt: string;
  members?: GroupMember[];
}

export interface GroupMember {
  userId: number;
  firstName: string;
  lastName: string;
  joinedAt: string;
}

export interface CreateGroupRequest {
  name: string;
  description: string;
}

export interface GroupMessage {
  id: number;
  groupId: number;
  senderId: number;
  senderName?: string;
  content: string;
  sentAt: string;
}

export interface GroupMessageDto {
  id: number;
  groupId: number;
  senderId: number;
  senderName: string;
  content: string;
  sentAt: string;
}
