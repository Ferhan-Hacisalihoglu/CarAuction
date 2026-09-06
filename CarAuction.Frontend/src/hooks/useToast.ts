import { useState, useCallback } from 'react';

export interface ToastItem {
  id: string;
  type: 'info' | 'success' | 'warning' | 'error';
  title?: string;
  message: string;
  duration?: number;
}

type ToastListener = (toast: ToastItem) => void;
const listeners: Set<ToastListener> = new Set();

export const toast = {
  show: (type: ToastItem['type'], message: string, title?: string, duration = 4000) => {
    const item: ToastItem = {
      id: Math.random().toString(36).substring(2, 9),
      type,
      message,
      title,
      duration,
    };
    listeners.forEach((fn) => fn(item));
  },
  info: (message: string, title?: string) => toast.show('info', message, title),
  success: (message: string, title?: string) => toast.show('success', message, title),
  warning: (message: string, title?: string) => toast.show('warning', message, title),
  error: (message: string, title?: string) => toast.show('error', message, title),
};

export function useToast() {
  const [toasts, setToasts] = useState<ToastItem[]>([]);

  const addToast = useCallback((item: ToastItem) => {
    setToasts((prev) => [...prev, item]);
    setTimeout(() => {
      setToasts((prev) => prev.filter((t) => t.id !== item.id));
    }, item.duration || 4000);
  }, []);

  const removeToast = useCallback((id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  return { toasts, addToast, removeToast };
}

export const subscribeToast = (listener: ToastListener) => {
  listeners.add(listener);
  return () => {
    listeners.delete(listener);
  };
};
