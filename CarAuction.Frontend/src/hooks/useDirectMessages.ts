import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { dmApi } from '@/api/dm';

export function useConversations() {
  return useQuery({
    queryKey: ['conversations'],
    queryFn: () => dmApi.getConversations(),
    refetchInterval: 10000,
  });
}

export function useConversationMessages(conversationId: number) {
  return useQuery({
    queryKey: ['dm-messages', conversationId],
    queryFn: () => dmApi.getMessages(conversationId),
    enabled: !isNaN(conversationId) && conversationId > 0,
    refetchInterval: 4000,
  });
}

export function useStartConversation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (targetUserId: number) => dmApi.startConversation({ targetUserId }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['conversations'] });
    },
  });
}

export function useSendDirectMessage() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ conversationId, content }: { conversationId: number; content: string }) =>
      dmApi.sendMessage(conversationId, content),
    onSuccess: (_, { conversationId }) => {
      queryClient.invalidateQueries({ queryKey: ['dm-messages', conversationId] });
      queryClient.invalidateQueries({ queryKey: ['conversations'] });
    },
  });
}

export function useMarkConversationRead() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (conversationId: number) => dmApi.markAsRead(conversationId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['conversations'] });
    },
  });
}
