export const APP_NAME = 'CarAuction';

export const STATUS_COLORS: Record<string, string> = {
  active: 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20',
  sold: 'bg-blue-500/10 text-blue-400 border-blue-500/20',
  completed: 'bg-indigo-500/10 text-indigo-400 border-indigo-500/20',
  expired: 'bg-amber-500/10 text-amber-400 border-amber-500/20',
  cancelled: 'bg-rose-500/10 text-rose-400 border-rose-500/20',
  pending: 'bg-amber-500/10 text-amber-400 border-amber-500/20',
  accepted: 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20',
  rejected: 'bg-rose-500/10 text-rose-400 border-rose-500/20',
};

export const DEFAULT_PAGE_LIMIT = 12;
