import { useEffect } from 'react';
import { useAuthStore } from '@/stores/authStore';
import { signalRService } from '@/api/signalr';

export function useSignalR() {
  const { accessToken, isAuthenticated } = useAuthStore();

  useEffect(() => {
    if (isAuthenticated && accessToken) {
      signalRService.connectAuctionHub(accessToken);
      signalRService.connectChatHub(accessToken);
    } else {
      signalRService.disconnect();
    }

    return () => {
      // Keep connected across page navigations if authenticated
    };
  }, [isAuthenticated, accessToken]);
}
