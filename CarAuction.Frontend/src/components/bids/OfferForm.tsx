import { useState } from 'react';
import { useAuth } from '@/hooks/useAuth';
import { useMakeOffer } from '@/hooks/useBids';
import { formatCurrency } from '@/utils/format';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import { toast } from '@/hooks/useToast';
import { HeartHandshake } from 'lucide-react';
import { Link } from 'react-router-dom';

export function OfferForm({ listingId, askingPrice }: { listingId: number; askingPrice: number }) {
  const { isAuthenticated } = useAuth();
  const [amount, setAmount] = useState<string>('');
  const makeOfferMutation = useMakeOffer();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const num = Number(amount);
    if (isNaN(num) || num <= 0) {
      toast.error('Please enter a valid offer amount', 'Invalid Offer');
      return;
    }

    try {
      await makeOfferMutation.mutateAsync({ listingId, amount: num });
      toast.success(`Your offer of ${formatCurrency(num)} was sent to the seller!`, 'Offer Submitted');
      setAmount('');
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Failed to submit offer. Please try again.';
      toast.error(msg, 'Offer Error');
    }
  };

  return (
    <Card className="glass-panel border-white/10 shadow-xl">
      <CardHeader className="pb-3 border-b border-white/5">
        <CardTitle className="text-base flex items-center gap-2">
          <HeartHandshake className="h-4 w-4 text-emerald-400" />
          <span>Make a Private Offer</span>
        </CardTitle>
      </CardHeader>
      <CardContent className="p-5">
        {isAuthenticated ? (
          <form onSubmit={handleSubmit} className="space-y-4">
            <p className="text-xs text-muted-foreground">
              Asking Price: <span className="font-semibold text-foreground">{formatCurrency(askingPrice)}</span>. You can submit a direct offer to the seller.
            </p>

            <div className="flex gap-2">
              <Input
                type="number"
                placeholder="Offer amount in USD"
                value={amount}
                onChange={(e) => setAmount(e.target.value)}
                className="h-11"
              />
              <Button
                type="submit"
                variant="primary"
                className="shrink-0 gap-1.5"
                isLoading={makeOfferMutation.isPending}
              >
                <span>Send Offer</span>
              </Button>
            </div>
          </form>
        ) : (
          <div className="text-center py-2 space-y-2">
            <p className="text-xs text-muted-foreground">
              Sign in to send direct offers to vehicle owners.
            </p>
            <Link to="/login" className="inline-block">
              <Button variant="outline" size="sm">
                Log In to Make Offer
              </Button>
            </Link>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
