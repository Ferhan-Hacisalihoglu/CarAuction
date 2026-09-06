import { useEffect } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { signalRService } from '@/api/signalr';
import { useChatStore } from '@/stores/chatStore';
import { DmMessageDto } from '@/types/message';
import { GroupMessageDto } from '@/types/group';
import { toast } from './useToast';

export function useChatHub() {
  const queryClient = useQueryClient();
  const { addTypingUser, removeTypingUser } = useChatStore();

  useEffect(() => {
    const unsubDm = signalRService.onDirectMessage((msg: DmMessageDto) => {
      queryClient.invalidateQueries({ queryKey: ['dm-messages', msg.conversationId] });
      queryClient.invalidateQueries({ queryKey: ['conversations'] });
      toast.info(`${msg.senderName || 'Someone'}: ${msg.content.substring(0, 40)}...`, 'New Direct Message');
    });

    const unsubGroup = signalRService.onGroupMessage((msg: GroupMessageDto) => {
      queryClient.invalidateQueries({ queryKey: ['group-messages', msg.groupId] });
    });

    const unsubTyping = signalRService.onUserTyping((data) => {
      addTypingUser(data.targetId, data.senderName);
      setTimeout(() => {
        removeTypingUser(data.targetId, data.senderName);
      }, 3000);
    });

    return () => {
      unsubDm();
      unsubGroup();
      unsubTyping();
    };
  }, [queryClient, addTypingUser, removeTypingUser]);
}
