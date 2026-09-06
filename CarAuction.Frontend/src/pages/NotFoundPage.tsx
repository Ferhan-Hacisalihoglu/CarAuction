import { Link } from 'react-router-dom';
import { Button } from '@/components/ui/Button';
import { Car, Home } from 'lucide-react';

export function NotFoundPage() {
  return (
    <div className="min-h-[70vh] flex flex-col items-center justify-center text-center p-6 space-y-6">
      <div className="flex h-20 w-20 items-center justify-center rounded-3xl bg-brand-500/10 text-brand-400 border border-brand-500/20 shadow-2xl">
        <Car className="h-10 w-10" />
      </div>

      <div className="space-y-2">
        <h1 className="text-6xl font-black font-display tracking-tight text-gradient">404</h1>
        <h2 className="text-2xl font-bold text-foreground">Road Not Found</h2>
        <p className="text-sm text-muted-foreground max-w-sm">
          The vehicle, lot, or platform page you are looking for has taken a wrong turn or does not exist.
        </p>
      </div>

      <Link to="/">
        <Button variant="primary" size="lg" className="gap-2">
          <Home className="h-4 w-4" />
          <span>Return Home</span>
        </Button>
      </Link>
    </div>
  );
}
