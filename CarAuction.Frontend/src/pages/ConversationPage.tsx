import { useState, useRef, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useConversationMessages, useSendDirectMessage, useMarkConversationRead } from '@/hooks/useDirectMessages';
import { useAuth } from '@/hooks/useAuth';
import { useChatStore } from '@/stores/chatStore';
import { signalRService } from '@/api/signalr';
import { MessageBubble } from '@/components/chat/MessageBubble';
import { LoadingSpinner } from '@/components/common/LoadingSpinner';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { toast } from '@/hooks/useToast';
import { Send, ArrowLeft, ShieldCheck } from 'lucide-react';

export function ConversationPage() {
  const { dmId } = useParams<{ dmId: string }>();
  const conversationId = Number(dmId);
  const { user } = useAuth();
  const { typingUsers } = useChatStore();

  const { data: messagesData, isLoading } = useConversationMessages(conversationId);
  const sendMutation = useSendDirectMessage();
  const markReadMutation = useMarkConversationRead();

  const [content, setContent] = useState('');
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const messages = Array.isArray(messagesData) ? messagesData : messagesData?.data || [];

  useEffect(() => {
    if (conversationId) {
      markReadMutation.mutate(conversationId);
    }
  }, [conversationId]);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const handleSend = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!content.trim()) return;

    const text = content.trim();
    setContent('');
    try {
      await sendMutation.mutateAsync({ conversationId, content: text });
    } catch {
      toast.error('Failed to send message');
      setContent(text);
    }
  };

  const handleTyping = (e: React.ChangeEvent<HTMLInputElement>) => {
    setContent(e.target.value);
    // Send typing notification
    signalRService.sendTyping(conversationId, false);
  };

  const isOtherTyping = (typingUsers[conversationId] || []).length > 0;

  return (
    <div className="container mx-auto max-w-4xl px-4 py-6 sm:px-6 lg:px-8 space-y-4">
      {/* Header */}
      <div className="flex items-center justify-between p-4 rounded-2xl glass-panel border-white/10">
        <div className="flex items-center gap-3">
          <Link to="/messages">
            <Button variant="ghost" size="icon" className="rounded-xl">
              <ArrowLeft className="h-5 w-5" />
            </Button>
          </Link>
          <div>
            <h2 className="text-base font-bold text-foreground">Direct Conversation</h2>
            <p className="text-xs text-muted-foreground flex items-center gap-1.5">
              <ShieldCheck className="h-3.5 w-3.5 text-emerald-400" />
              <span>Encrypted 1-on-1 Channel</span>
            </p>
          </div>
        </div>
      </div>

      {/* Chat Box */}
      <div className="flex flex-col h-[650px] rounded-2xl glass-card border-white/10 overflow-hidden">
        {/* Messages Stream */}
        <div className="flex-1 overflow-y-auto p-5 space-y-2">
          {isLoading ? (
            <LoadingSpinner size="sm" message="Loading messages..." />
          ) : messages.length === 0 ? (
            <div className="h-full flex items-center justify-center text-xs text-muted-foreground">
              Send a message to begin the conversation.
            </div>
          ) : (
            messages.map((msg) => (
              <MessageBubble
                key={msg.id}
                content={msg.content}
                sentAt={msg.sentAt}
                isOwn={user ? user.id === msg.senderId : false}
                isRead={msg.isRead}
              />
            ))
          )}

          {isOtherTyping && (
            <div className="text-xs text-muted-foreground italic flex items-center gap-2 pt-2">
              <span className="h-2 w-2 rounded-full bg-brand-500 animate-ping" />
              <span>User is typing...</span>
            </div>
          )}

          <div ref={messagesEndRef} />
        </div>

        {/* Input */}
        <form onSubmit={handleSend} className="p-3.5 border-t border-white/5 bg-card/60 flex gap-2">
          <Input
            value={content}
            onChange={handleTyping}
            placeholder="Type your message..."
            className="h-11"
          />
          <Button
            type="submit"
            variant="primary"
            className="h-11 px-6 shrink-0 gap-2"
            disabled={!content.trim()}
            isLoading={sendMutation.isPending}
          >
            <Send className="h-4 w-4" />
            <span>Send</span>
          </Button>
        </form>
      </div>
    </div>
  );
}
