import { Link, Outlet } from 'react-router-dom';
import { Car } from 'lucide-react';
import { ToastContainer } from '@/components/ui/Toast';

export function AuthLayout() {
  return (
    <div className="relative min-h-screen flex flex-col items-center justify-center p-4 sm:p-6 lg:p-8 overflow-hidden bg-background">
      {/* Dynamic Background Gradients */}
      <div className="absolute top-[-10%] left-[-10%] w-[500px] h-[500px] rounded-full bg-brand-600/15 blur-[120px] pointer-events-none" />
      <div className="absolute bottom-[-10%] right-[-10%] w-[500px] h-[500px] rounded-full bg-indigo-600/15 blur-[120px] pointer-events-none" />

      {/* Brand Header */}
      <Link to="/" className="mb-8 flex items-center gap-2.5 group z-10">
        <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-gradient-to-tr from-brand-600 to-indigo-600 text-white shadow-xl shadow-brand-600/30 group-hover:scale-105 transition-transform">
          <Car className="h-6 w-6" />
        </div>
        <div className="flex flex-col">
          <span className="text-2xl font-black tracking-tight text-foreground font-display">
            CAR<span className="text-brand-500">AUCTION</span>
          </span>
          <span className="text-[10px] tracking-widest font-semibold uppercase text-muted-foreground -mt-1">
            Luxury & Live Bids
          </span>
        </div>
      </Link>

      {/* Card Body */}
      <div className="w-full max-w-md z-10">
        <Outlet />
      </div>

      <ToastContainer />
    </div>
  );
}
