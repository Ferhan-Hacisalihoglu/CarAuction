import { create } from 'zustand';

interface ChatState {
  activeConversation: number | null;
  activeGroup: number | null;
  typingUsers: Record<number, string[]>;
  unreadCount: number;
  setActiveConversation: (id: number | null) => void;
  setActiveGroup: (id: number | null) => void;
  setUnreadCount: (count: number) => void;
  addTypingUser: (targetId: number, userName: string) => void;
  removeTypingUser: (targetId: number, userName: string) => void;
}

export const useChatStore = create<ChatState>((set) => ({
  activeConversation: null,
  activeGroup: null,
  typingUsers: {},
  unreadCount: 0,
  setActiveConversation: (id) => set({ activeConversation: id }),
  setActiveGroup: (id) => set({ activeGroup: id }),
  setUnreadCount: (count) => set({ unreadCount: count }),
  addTypingUser: (targetId, userName) =>
    set((state) => {
      const current = state.typingUsers[targetId] || [];
      if (current.includes(userName)) return state;
      return {
        typingUsers: {
          ...state.typingUsers,
          [targetId]: [...current, userName],
        },
      };
    }),
  removeTypingUser: (targetId, userName) =>
    set((state) => ({
      typingUsers: {
        ...state.typingUsers,
        [targetId]: (state.typingUsers[targetId] || []).filter((n) => n !== userName),
      },
    })),
}));
