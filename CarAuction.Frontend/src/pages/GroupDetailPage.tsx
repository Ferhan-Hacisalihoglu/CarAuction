import { useState, useRef, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useGroup, useGroupMessages, useSendGroupMessage, useLeaveGroup } from '@/hooks/useGroups';
import { useAuth } from '@/hooks/useAuth';
import { MessageBubble } from '@/components/chat/MessageBubble';
import { LoadingSpinner } from '@/components/common/LoadingSpinner';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import { Avatar } from '@/components/ui/Avatar';
import { toast } from '@/hooks/useToast';
import { Users, Send, ArrowLeft, LogOut, Shield } from 'lucide-react';

export function GroupDetailPage() {
  const { id } = useParams<{ id: string }>();
  const groupId = Number(id);
  const navigate = useNavigate();
  const { user } = useAuth();

  const { data: group, isLoading: groupLoading, isError } = useGroup(groupId);
  const { data: messagesData, isLoading: messagesLoading } = useGroupMessages(groupId);

  const sendMessageMutation = useSendGroupMessage();
  const leaveGroupMutation = useLeaveGroup();

  const [content, setContent] = useState('');
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const messages = Array.isArray(messagesData) ? messagesData : messagesData?.data || [];

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const handleSend = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!content.trim()) return;

    const text = content.trim();
    setContent('');
    try {
      await sendMessageMutation.mutateAsync({ id: groupId, content: text });
    } catch {
      toast.error('Failed to send message');
      setContent(text);
    }
  };

  const handleLeave = async () => {
    try {
      await leaveGroupMutation.mutateAsync(groupId);
      toast.info('Left club');
      navigate('/groups');
    } catch {
      toast.error('Failed to leave club');
    }
  };

  if (groupLoading) {
    return <LoadingSpinner size="lg" message="Loading club room..." className="min-h-[50vh]" />;
  }

  if (isError || !group) {
    return (
      <div className="container mx-auto max-w-4xl py-20 text-center space-y-4">
        <h2 className="text-2xl font-bold">Club Not Found</h2>
        <Link to="/groups">
          <Button variant="primary">Return to Clubs</Button>
        </Link>
      </div>
    );
  }

  return (
    <div className="container mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8 space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 p-6 rounded-2xl glass-panel border-white/10">
        <div className="flex items-center gap-4">
          <Link to="/groups">
            <Button variant="ghost" size="icon" className="rounded-xl">
              <ArrowLeft className="h-5 w-5" />
            </Button>
          </Link>
          <div>
            <h1 className="text-2xl font-black text-foreground font-display flex items-center gap-2">
              <span>{group.name}</span>
            </h1>
            <p className="text-xs text-muted-foreground mt-0.5 max-w-xl">{group.description}</p>
          </div>
        </div>

        <Button
          variant="outline"
          size="sm"
          className="text-xs text-rose-400 hover:text-rose-300 gap-1.5"
          onClick={handleLeave}
          isLoading={leaveGroupMutation.isPending}
        >
          <LogOut className="h-3.5 w-3.5" />
          <span>Leave Club</span>
        </Button>
      </div>

      {/* Grid: Chat Room + Member List */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Chat Window (8-9 cols) */}
        <div className="lg:col-span-8 xl:col-span-9 flex flex-col h-[650px] rounded-2xl glass-card border-white/10 overflow-hidden">
          <div className="p-4 border-b border-white/5 bg-secondary/30 flex items-center justify-between">
            <span className="text-xs font-bold text-muted-foreground uppercase tracking-wider">
              Live Club Channel
            </span>
            <span className="text-xs text-emerald-400 font-semibold flex items-center gap-1.5">
              <span className="h-2 w-2 rounded-full bg-emerald-400 animate-pulse" />
              <span>Live Chat Active</span>
            </span>
          </div>

          {/* Messages Area */}
          <div className="flex-1 overflow-y-auto p-4 space-y-2">
            {messagesLoading ? (
              <LoadingSpinner size="sm" message="Loading messages..." />
            ) : messages.length === 0 ? (
              <div className="h-full flex items-center justify-center text-xs text-muted-foreground">
                No messages yet. Say hello to your fellow club members!
              </div>
            ) : (
              messages.map((msg) => (
                <MessageBubble
                  key={msg.id}
                  content={msg.content}
                  sentAt={msg.sentAt}
                  senderName={msg.senderName || `Member #${msg.senderId}`}
                  isOwn={user ? user.id === msg.senderId : false}
                />
              ))
            )}
            <div ref={messagesEndRef} />
          </div>

          {/* Input Box */}
          <form onSubmit={handleSend} className="p-3 border-t border-white/5 bg-card/60 flex gap-2">
            <Input
              value={content}
              onChange={(e) => setContent(e.target.value)}
              placeholder="Type message to club..."
              className="h-11"
            />
            <Button
              type="submit"
              variant="primary"
              className="h-11 px-5 shrink-0 gap-1.5"
              disabled={!content.trim()}
              isLoading={sendMessageMutation.isPending}
            >
              <Send className="h-4 w-4" />
              <span className="hidden sm:inline">Send</span>
            </Button>
          </form>
        </div>

        {/* Member Sidebar (3-4 cols) */}
        <div className="lg:col-span-4 xl:col-span-3 space-y-4">
          <Card className="glass-card">
            <CardHeader className="p-4 border-b border-white/5">
              <CardTitle className="text-sm flex items-center justify-between">
                <span className="flex items-center gap-2">
                  <Users className="h-4 w-4 text-brand-400" />
                  <span>Members</span>
                </span>
                <span className="text-xs text-muted-foreground">
                  {group.members?.length || 0}
                </span>
              </CardTitle>
            </CardHeader>
            <CardContent className="p-3 max-h-[550px] overflow-y-auto divide-y divide-white/5">
              {!group.members || group.members.length === 0 ? (
                <div className="p-3 text-xs text-muted-foreground text-center">No member list</div>
              ) : (
                group.members.map((m) => (
                  <div key={m.userId} className="flex items-center gap-2.5 py-2.5">
                    <Avatar name={`${m.firstName} ${m.lastName}`} size="sm" isOnline />
                    <div className="min-w-0 flex-1">
                      <p className="text-xs font-semibold text-foreground truncate">
                        {m.firstName} {m.lastName}
                      </p>
                      {m.userId === group.createdBy && (
                        <span className="text-[10px] text-amber-400 font-bold flex items-center gap-1">
                          <Shield className="h-3 w-3" /> Club Creator
                        </span>
                      )}
                    </div>
                  </div>
                ))
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
