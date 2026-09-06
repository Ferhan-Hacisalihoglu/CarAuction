import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { AuctionExtendedDto, AuctionResultDto, BidDto } from '@/types/auction';
import { DmMessageDto } from '@/types/message';
import { GroupMessageDto } from '@/types/group';

type BidListener = (bid: BidDto) => void;
type AuctionEndedListener = (result: AuctionResultDto) => void;
type AuctionExtendedListener = (extension: AuctionExtendedDto) => void;
type DmListener = (message: DmMessageDto) => void;
type GroupMessageListener = (message: GroupMessageDto) => void;
type TypingListener = (data: { senderId: number; senderName: string; targetId: number; isGroup: boolean }) => void;

class SignalRService {
  private auctionHub: HubConnection | null = null;
  private chatHub: HubConnection | null = null;

  private bidListeners: Set<BidListener> = new Set();
  private endedListeners: Set<AuctionEndedListener> = new Set();
  private extendedListeners: Set<AuctionExtendedListener> = new Set();
  private dmListeners: Set<DmListener> = new Set();
  private groupListeners: Set<GroupMessageListener> = new Set();
  private typingListeners: Set<TypingListener> = new Set();

  public isAuctionConnected = false;
  public isChatConnected = false;

  async connectAuctionHub(token: string) {
    if (this.auctionHub) {
      await this.auctionHub.stop();
    }

    const signalrBase = import.meta.env.VITE_SIGNALR_URL || '/hubs';
    const hubUrl = `${signalrBase}/auction`;

    this.auctionHub = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    this.auctionHub.on('ReceiveNewBid', (bid: BidDto) => {
      this.bidListeners.forEach((fn) => fn(bid));
    });

    this.auctionHub.on('AuctionEnded', (result: AuctionResultDto) => {
      this.endedListeners.forEach((fn) => fn(result));
    });

    this.auctionHub.on('AuctionTimeExtended', (ext: AuctionExtendedDto) => {
      this.extendedListeners.forEach((fn) => fn(ext));
    });

    try {
      await this.auctionHub.start();
      this.isAuctionConnected = true;
    } catch (err) {
      console.warn('AuctionHub connection error:', err);
    }
  }

  async connectChatHub(token: string) {
    if (this.chatHub) {
      await this.chatHub.stop();
    }

    const signalrBase = import.meta.env.VITE_SIGNALR_URL || '/hubs';
    const hubUrl = `${signalrBase}/chat`;

    this.chatHub = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    this.chatHub.on('ReceiveDirectMessage', (msg: DmMessageDto) => {
      this.dmListeners.forEach((fn) => fn(msg));
    });

    this.chatHub.on('ReceiveGroupMessage', (msg: GroupMessageDto) => {
      this.groupListeners.forEach((fn) => fn(msg));
    });

    this.chatHub.on('UserTyping', (senderId: number, senderName: string, targetId: number, isGroup: boolean) => {
      this.typingListeners.forEach((fn) => fn({ senderId, senderName, targetId, isGroup }));
    });

    try {
      await this.chatHub.start();
      this.isChatConnected = true;
    } catch (err) {
      console.warn('ChatHub connection error:', err);
    }
  }

  // Auction Hub Room Actions
  async joinAuction(auctionId: number) {
    if (this.auctionHub && this.auctionHub.state === 'Connected') {
      try {
        await this.auctionHub.invoke('JoinAuction', auctionId);
      } catch (err) {
        console.warn('Failed to join auction room:', err);
      }
    }
  }

  async leaveAuction(auctionId: number) {
    if (this.auctionHub && this.auctionHub.state === 'Connected') {
      try {
        await this.auctionHub.invoke('LeaveAuction', auctionId);
      } catch (err) {
        console.warn('Failed to leave auction room:', err);
      }
    }
  }

  // Chat Hub Actions
  async sendDirectMessage(conversationId: number, recipientId: number, content: string) {
    if (this.chatHub && this.chatHub.state === 'Connected') {
      await this.chatHub.invoke('SendDirectMessage', conversationId, recipientId, content);
    }
  }

  async sendGroupMessage(groupId: number, content: string) {
    if (this.chatHub && this.chatHub.state === 'Connected') {
      await this.chatHub.invoke('SendGroupMessage', groupId, content);
    }
  }

  async sendTyping(targetId: number, isGroup: boolean) {
    if (this.chatHub && this.chatHub.state === 'Connected') {
      await this.chatHub.invoke('Typing', targetId, isGroup);
    }
  }

  // Listener subscriptions
  onNewBid(fn: BidListener) {
    this.bidListeners.add(fn);
    return () => this.bidListeners.delete(fn);
  }

  onAuctionEnded(fn: AuctionEndedListener) {
    this.endedListeners.add(fn);
    return () => this.endedListeners.delete(fn);
  }

  onAuctionExtended(fn: AuctionExtendedListener) {
    this.extendedListeners.add(fn);
    return () => this.extendedListeners.delete(fn);
  }

  onDirectMessage(fn: DmListener) {
    this.dmListeners.add(fn);
    return () => this.dmListeners.delete(fn);
  }

  onGroupMessage(fn: GroupMessageListener) {
    this.groupListeners.add(fn);
    return () => this.groupListeners.delete(fn);
  }

  onUserTyping(fn: TypingListener) {
    this.typingListeners.add(fn);
    return () => this.typingListeners.delete(fn);
  }

  async disconnect() {
    this.isAuctionConnected = false;
    this.isChatConnected = false;
    if (this.auctionHub) {
      await this.auctionHub.stop();
      this.auctionHub = null;
    }
    if (this.chatHub) {
      await this.chatHub.stop();
      this.chatHub = null;
    }
  }
}

export const signalRService = new SignalRService();
