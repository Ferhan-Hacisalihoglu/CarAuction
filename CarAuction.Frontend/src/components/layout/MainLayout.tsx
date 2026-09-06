import { Outlet } from 'react-router-dom';
import { Header } from './Header';
import { Footer } from './Footer';
import { Sidebar } from './Sidebar';
import { ToastContainer } from '@/components/ui/Toast';
import { useSignalR } from '@/hooks/useSignalR';

export function MainLayout() {
  // Global SignalR WebSocket manager
  useSignalR();

  return (
    <div className="flex min-h-screen flex-col bg-background text-foreground selection:bg-brand-600 selection:text-white">
      <Header />
      <Sidebar />
      <main className="flex-1">
        <Outlet />
      </main>
      <Footer />
      <ToastContainer />
    </div>
  );
}
