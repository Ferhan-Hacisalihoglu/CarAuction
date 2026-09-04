# Frontend Architecture & UI Design

> [!NOTE]
> **FRONTEND STACK POLICY:**
> All frontend layers, components, pages, services, and state management **MUST** be implemented strictly in accordance with this architecture.
> Backend integration follows the API specifications defined in `backend.md`.

---

## 1. Technology Stack & Infrastructure

The frontend ecosystem utilizes the following core technologies, with the application running inside a Docker container:

| Technology | Purpose | Environment |
|---|---|---|
| **React 18+** | UI framework with concurrent features | Docker Container (Nginx) |
| **TypeScript** | Type safety across all layers | Build-time |
| **Vite** | Build tool & dev server | Development & Production |
| **React Router v6** | Client-side routing & navigation | Built-in |
| **TanStack Query v5** | Server state, caching, synchronization | Built-in |
| **Zustand** | Client state management (auth, UI) | Built-in |
| **@microsoft/signalr** | Real-time WebSocket communication | Built-in |
| **Axios** | HTTP client with interceptors | Built-in |
| **Tailwind CSS** | Utility-first styling | Built-in |
| **shadcn/ui** | Component library (Radix UI based) | Built-in |
| **React Hook Form + Zod** | Form management & validation | Built-in |
| **Docker & Nginx** | Containerization & static file serving | Production Container |

---

## 2. Project Structure

```
frontend/
├── Dockerfile
├── docker-compose.yml
├── nginx.conf
├── package.json
├── vite.config.ts
├── tsconfig.json
├── tailwind.config.js
├── postcss.config.js
├── .env
├── .env.production
├── src/
│   ├── main.tsx
│   ├── App.tsx
│   ├── vite-env.d.ts
│   │
│   ├── api/                          # API layer
│   │   ├── axios.ts                  # Axios instance, interceptor
│   │   ├── auth.ts                   # Auth API calls
│   │   ├── users.ts                  # Users API
│   │   ├── listings.ts               # Listings API
│   │   ├── auctions.ts               # Auctions API
│   │   ├── bids.ts                   # Bids/Offers API
│   │   ├── images.ts                 # Images API
│   │   ├── groups.ts                 # Groups API
│   │   ├── dm.ts                     # Direct Messages API
│   │   └── signalr.ts                # SignalR connection manager
│   │
│   ├── components/
│   │   ├── ui/                       # shadcn/ui components
│   │   │   ├── button.tsx
│   │   │   ├── input.tsx
│   │   │   ├── dialog.tsx
│   │   │   ├── toast.tsx
│   │   │   ├── avatar.tsx
│   │   │   ├── badge.tsx
│   │   │   ├── card.tsx
│   │   │   ├── dropdown-menu.tsx
│   │   │   ├── textarea.tsx
│   │   │   ├── label.tsx
│   │   │   ├── tabs.tsx
│   │   │   ├── separator.tsx
│   │   │   ├── skeleton.tsx
│   │   │   └── scroll-area.tsx
│   │   │
│   │   ├── layout/                   # Layout components
│   │   │   ├── Header.tsx
│   │   │   ├── Footer.tsx
│   │   │   ├── Sidebar.tsx
│   │   │   ├── MainLayout.tsx
│   │   │   └── AuthLayout.tsx
│   │   │
│   │   ├── auth/                     # Auth components
│   │   │   ├── LoginForm.tsx
│   │   │   ├── RegisterForm.tsx
│   │   │   └── ProtectedRoute.tsx
│   │   │
│   │   ├── listings/                 # Listing components
│   │   │   ├── ListingCard.tsx
│   │   │   ├── ListingGrid.tsx
│   │   │   ├── ListingFilters.tsx
│   │   │   ├── ListingDetail.tsx
│   │   │   ├── CreateListingForm.tsx
│   │   │   ├── EditListingForm.tsx
│   │   │   └── ImageGallery.tsx
│   │   │
│   │   ├── auctions/                 # Auction components
│   │   │   ├── AuctionCard.tsx
│   │   │   ├── AuctionDetail.tsx
│   │   │   ├── LiveBidPanel.tsx
│   │   │   ├── BidHistory.tsx
│   │   │   ├── AuctionTimer.tsx
│   │   │   └── AntiSnipeIndicator.tsx
│   │   │
│   │   ├── bids/                     # Bid components
│   │   │   ├── OfferForm.tsx
│   │   │   ├── OfferList.tsx
│   │   │   └── MyBidsList.tsx
│   │   │
│   │   ├── groups/                   # Group components
│   │   │   ├── GroupCard.tsx
│   │   │   ├── GroupList.tsx
│   │   │   ├── GroupDetail.tsx
│   │   │   ├── CreateGroupForm.tsx
│   │   │   ├── GroupMembers.tsx
│   │   │   └── GroupChat.tsx
│   │   │
│   │   ├── chat/                     # Messaging components
│   │   │   ├── ChatSidebar.tsx
│   │   │   ├── ChatWindow.tsx
│   │   │   ├── MessageBubble.tsx
│   │   │   ├── MessageInput.tsx
│   │   │   ├── TypingIndicator.tsx
│   │   │   └── ConversationList.tsx
│   │   │
│   │   ├── images/                   # Image components
│   │   │   ├── ImageUpload.tsx
│   │   │   ├── ImagePreview.tsx
│   │   │   └── ImageViewer.tsx
│   │   │
│   │   └── common/                   # Shared components
│   │       ├── LoadingSpinner.tsx
│   │       ├── ErrorBoundary.tsx
│   │       ├── Pagination.tsx
│   │       ├── SearchInput.tsx
│   │       ├── EmptyState.tsx
│   │       └── ConfirmDialog.tsx
│   │
│   ├── hooks/                        # Custom hooks
│   │   ├── useAuth.ts
│   │   ├── useListings.ts
│   │   ├── useAuctions.ts
│   │   ├── useBids.ts
│   │   ├── useGroups.ts
│   │   ├── useDirectMessages.ts
│   │   ├── useSignalR.ts
│   │   ├── useAuctionHub.ts
│   │   ├── useChatHub.ts
│   │   ├── useToast.ts
│   │   └── useDebounce.ts
│   │
│   ├── pages/                        # Page components
│   │   ├── HomePage.tsx
│   │   ├── ListingsPage.tsx
│   │   ├── ListingDetailPage.tsx
│   │   ├── CreateListingPage.tsx
│   │   ├── EditListingPage.tsx
│   │   ├── MyListingsPage.tsx
│   │   ├── AuctionsPage.tsx
│   │   ├── AuctionDetailPage.tsx
│   │   ├── MyBidsPage.tsx
│   │   ├── LoginPage.tsx
│   │   ├── RegisterPage.tsx
│   │   ├── ProfilePage.tsx
│   │   ├── GroupsPage.tsx
│   │   ├── GroupDetailPage.tsx
│   │   ├── MessagesPage.tsx
│   │   ├── ConversationPage.tsx
│   │   ├── AdminDashboardPage.tsx
│   │   ├── AdminUsersPage.tsx
│   │   ├── AdminRolesPage.tsx
│   │   └── NotFoundPage.tsx
│   │
│   ├── stores/                       # Zustand stores
│   │   ├── authStore.ts
│   │   ├── uiStore.ts
│   │   └── chatStore.ts
│   │
│   ├── types/                        # TypeScript types
│   │   ├── auth.ts
│   │   ├── user.ts
│   │   ├── listing.ts
│   │   ├── auction.ts
│   │   ├── bid.ts
│   │   ├── group.ts
│   │   ├── message.ts
│   │   └── common.ts
│   │
│   ├── utils/                        # Utility functions
│   │   ├── cn.ts                     # clsx + tailwind-merge
│   │   ├── format.ts                 # Date, currency formatting
│   │   ├── validators.ts             # Zod schemas
│   │   └── constants.ts
│   │
│   └── styles/
│       └── globals.css
```

---

## 3. Routing & Page Structure

### 3.1. Route Definitions

```tsx
// App.tsx - Router structure
<Routes>
  {/* Public Routes */}
  <Route path="/" element={<HomePage />} />
  <Route path="/listings" element={<ListingsPage />} />
  <Route path="/listings/:id" element={<ListingDetailPage />} />
  <Route path="/auctions" element={<AuctionsPage />} />
  <Route path="/auctions/:id" element={<AuctionDetailPage />} />
  
  {/* Auth Routes */}
  <Route path="/login" element={<LoginPage />} />
  <Route path="/register" element={<RegisterPage />} />
  
  {/* Protected Routes */}
  <Route element={<ProtectedRoute />}>
    <Route path="/listings/create" element={<CreateListingPage />} />
    <Route path="/listings/:id/edit" element={<EditListingPage />} />
    <Route path="/my-listings" element={<MyListingsPage />} />
    <Route path="/my-bids" element={<MyBidsPage />} />
    <Route path="/profile" element={<ProfilePage />} />
    <Route path="/groups" element={<GroupsPage />} />
    <Route path="/groups/:id" element={<GroupDetailPage />} />
    <Route path="/messages" element={<MessagesPage />} />
    <Route path="/messages/:dmId" element={<ConversationPage />} />
  </Route>
  
  {/* Admin Routes */}
  <Route element={<ProtectedRoute role="Admin" />}>
    <Route path="/admin" element={<AdminDashboardPage />} />
    <Route path="/admin/users" element={<AdminUsersPage />} />
    <Route path="/admin/roles" element={<AdminRolesPage />} />
  </Route>
  
  <Route path="*" element={<NotFoundPage />} />
</Routes>
```

### 3.2. Route Protection Logic

```typescript
// components/auth/ProtectedRoute.tsx
const ProtectedRoute = ({ role }: { role?: string }) => {
  const { isAuthenticated, user } = useAuthStore();
  
  if (!isAuthenticated) return <Navigate to="/login" />;
  if (role && user?.role !== role) return <Navigate to="/" />;
  
  return <Outlet />;
};
```

---

## 4. API Layer (Axios + Interceptors)

### 4.1. Axios Instance Configuration

```typescript
// api/axios.ts
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || '/api',
  headers: { 'Content-Type': 'application/json' }
});

// Request interceptor - Attach JWT
api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// Response interceptor - Token refresh & ApiResponse envelope unwrapping
api.interceptors.response.use(
  (response) => {
    // Automatically unwrap standard backend ApiResponse<T> envelope
    if (response.data && typeof response.data === 'object' && 'data' in response.data && 'success' in response.data) {
      response.data = response.data.data;
    }
    return response;
  },
  async (error) => {
    if (error.response?.status === 401 && !error.config._retry) {
      error.config._retry = true;
      const newToken = await refreshAccessToken();
      error.config.headers.Authorization = `Bearer ${newToken}`;
      return api(error.config);
    }
    return Promise.reject(error);
  }
);
```

### 4.2. API Service Functions

```typescript
// api/auth.ts
export const authApi = {
  login: (data: LoginRequest) => api.post<AuthResponse>('/auth/login', data),
  register: (data: RegisterRequest) => api.post<AuthResponse>('/auth/register', data),
  refreshToken: (token: string) => api.post<AuthResponse>('/auth/refresh-token', { token }),
  logout: () => api.post('/auth/logout'),
  me: () => api.get<User>('/auth/me'),
};

// api/listings.ts
export const listingsApi = {
  getAll: (params: ListingFilters) => api.get<PaginatedResponse<Listing>>('/listings', { params }),
  getById: (id: number) => api.get<Listing>(`/listings/${id}`),
  create: (data: CreateListingRequest) => api.post<Listing>('/listings', data),
  update: (id: number, data: UpdateListingRequest) => api.put<Listing>(`/listings/${id}`, data),
  delete: (id: number) => api.delete(`/listings/${id}`),
  getMyListings: () => api.get<Listing[]>('/listings/my'),
};

// api/auctions.ts
export const auctionsApi = {
  getAll: () => api.get<Auction[]>('/auctions'),
  getById: (id: number) => api.get<Auction>(`/auctions/${id}`),
  getByListingId: (listingId: number) => api.get<Auction>(`/auctions/listing/${listingId}`),
  placeBid: (id: number, amount: number) => api.post<AuctionDetailResponse>(`/auctions/${id}/bid`, { amount }),
  getBidHistory: (id: number) => api.get<Bid[]>(`/auctions/${id}/bids`),
};

// api/bids.ts
export const bidsApi = {
  makeOffer: (listingId: number, amount: number) => api.post<Bid>(`/listings/${listingId}/offers`, { amount }),
  getOffers: (listingId: number) => api.get<Bid[]>(`/listings/${listingId}/offers`),
  getMyBids: () => api.get<Bid[]>('/bids/my'),
};

// api/groups.ts
export const groupsApi = {
  getAll: () => api.get<Group[]>('/groups'),
  getMyGroups: () => api.get<Group[]>('/groups/my'),
  getById: (id: number) => api.get<Group>(`/groups/${id}`),
  create: (data: CreateGroupRequest) => api.post<Group>('/groups', data),
  join: (id: number) => api.post(`/groups/${id}/join`),
  leave: (id: number) => api.post(`/groups/${id}/leave`),
  getMessages: (id: number, params: PaginationParams) => api.get<PaginatedResponse<GroupMessage>>(`/groups/${id}/messages`, { params }),
  sendMessage: (id: number, content: string) => api.post<GroupMessage>(`/groups/${id}/messages`, { content }),
};

// api/dm.ts
export const dmApi = {
  getConversations: () => api.get<Conversation[]>('/dm'),
  startConversation: (targetUserId: number) => api.post<Conversation>('/dm/start', { targetUserId }),
  getMessages: (dmId: number, params: PaginationParams) => api.get<PaginatedResponse<DmMessage>>(`/dm/${dmId}/messages`, { params }),
  sendMessage: (dmId: number, content: string) => api.post<DmMessage>(`/dm/${dmId}/messages`, { content }),
  markAsRead: (dmId: number) => api.put(`/dm/${dmId}/read`),
};

// api/images.ts
export const imagesApi = {
  upload: (listingId: number, file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    return api.post<Image>(`/listings/${listingId}/images`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
  },
  get: (id: number) => api.get<Blob>(`/images/${id}`, { responseType: 'blob' }),
  delete: (id: number) => api.delete(`/images/${id}`),
};

// api/users.ts
export const usersApi = {
  getAll: (params: PaginationParams) => api.get<PaginatedResponse<User>>('/users', { params }),
  getById: (id: number) => api.get<User>(`/users/${id}`),
  updateProfile: (data: UpdateProfileRequest) => api.put<User>('/users/profile', data),
  updateStatus: (id: number, isActive: boolean) => api.put(`/users/${id}/status`, { isActive }),
  updateRole: (id: number, roleId: number) => api.put(`/users/${id}/role`, { roleId }),
};

// api/roles.ts
export const rolesApi = {
  getAll: () => api.get<Role[]>('/roles'),
  create: (name: string) => api.post<Role>('/roles', { name }),
  delete: (id: number) => api.delete(`/roles/${id}`),
  getPermissions: (roleId: number) => api.get<Permission[]>(`/roles/${roleId}/permissions`),
  assignPermission: (roleId: number, permissionId: number) => api.post(`/roles/${roleId}/permissions`, { permissionId }),
  removePermission: (roleId: number, permissionId: number) => api.delete(`/roles/${roleId}/permissions/${permissionId}`),
};
```

---

## 5. SignalR Connection Management

### 5.1. SignalR Service Class

```typescript
// api/signalr.ts
class SignalRService {
  private auctionHub: HubConnection | null = null;
  private chatHub: HubConnection | null = null;

  // Auction Hub Connection
  async connectAuctionHub(token: string) {
    this.auctionHub = new HubConnectionBuilder()
      .withUrl(`${import.meta.env.VITE_API_URL}/hubs/auction`, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    this.auctionHub.on('ReceiveNewBid', (bid: BidDto) => {
      queryClient.setQueryData(['auction', bid.auctionId], (old: any) => ({
        ...old,
        currentPrice: bid.amount,
        winnerUserId: bid.bidderId
      }));
    });

    this.auctionHub.on('AuctionEnded', (result: AuctionResultDto) => {
      toast.info(`Auction ended! Winner: ${result.winnerName}`);
    });

    this.auctionHub.on('AuctionTimeExtended', (ext: AuctionExtendedDto) => {
      toast.warning(`Auction extended! New end: ${ext.newEndTime}`);
    });

    await this.auctionHub.start();
  }

  // Chat Hub Connection
  async connectChatHub(token: string) {
    this.chatHub = new HubConnectionBuilder()
      .withUrl(`${import.meta.env.VITE_API_URL}/hubs/chat`, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    this.chatHub.on('ReceiveDirectMessage', (msg: DmMessageDto) => {
      queryClient.invalidateQueries({ queryKey: ['dm', msg.dmId] });
    });

    this.chatHub.on('ReceiveGroupMessage', (msg: GroupMessageDto) => {
      queryClient.invalidateQueries({ queryKey: ['group', msg.groupId, 'messages'] });
    });

    this.chatHub.on('UserTyping', (data) => {
      // Show typing indicator
    });

    await this.chatHub.start();
  }

  // Join auction room
  async joinAuction(auctionId: number) {
    await this.auctionHub?.invoke('JoinAuction', auctionId);
  }

  // Leave auction room
  async leaveAuction(auctionId: number) {
    await this.auctionHub?.invoke('LeaveAuction', auctionId);
  }

  // Send direct message via SignalR
  async sendDirectMessage(dmId: number, recipientId: number, content: string) {
    await this.chatHub?.invoke('SendDirectMessage', dmId, recipientId, content);
  }

  // Send group message via SignalR
  async sendGroupMessage(groupId: number, content: string) {
    await this.chatHub?.invoke('SendGroupMessage', groupId, content);
  }

  // Send typing indicator
  async sendTyping(targetId: number, isGroup: boolean) {
    await this.chatHub?.invoke('Typing', targetId, isGroup);
  }

  // Disconnect all hubs
  async disconnect() {
    await this.auctionHub?.stop();
    await this.chatHub?.stop();
  }
}

export const signalRService = new SignalRService();
```

### 5.2. SignalR Hook

```typescript
// hooks/useSignalR.ts
export const useSignalR = () => {
  const { accessToken, isAuthenticated } = useAuthStore();

  useEffect(() => {
    if (isAuthenticated && accessToken) {
      signalRService.connectAuctionHub(accessToken);
      signalRService.connectChatHub(accessToken);
    }

    return () => {
      signalRService.disconnect();
    };
  }, [isAuthenticated, accessToken]);
};
```

---

## 6. State Management (Zustand)

### 6.1. Auth Store

```typescript
// stores/authStore.ts
interface AuthState {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => void;
  refreshAccessToken: () => Promise<string>;
  updateUser: (user: User) => void;
}

export const useAuthStore = create<AuthState>((set, get) => ({
  user: null,
  accessToken: null,
  refreshToken: null,
  isAuthenticated: false,

  login: async (email, password) => {
    const response = await authApi.login({ email, password });
    set({
      user: response.data.user,
      accessToken: response.data.accessToken,
      refreshToken: response.data.refreshToken,
      isAuthenticated: true,
    });
  },

  register: async (data) => {
    const response = await authApi.register(data);
    set({
      user: response.data.user,
      accessToken: response.data.accessToken,
      refreshToken: response.data.refreshToken,
      isAuthenticated: true,
    });
  },

  logout: () => {
    authApi.logout();
    set({ user: null, accessToken: null, refreshToken: null, isAuthenticated: false });
  },

  refreshAccessToken: async () => {
    const currentRefreshToken = get().refreshToken;
    const response = await authApi.refreshToken(currentRefreshToken!);
    set({
      accessToken: response.data.accessToken,
      refreshToken: response.data.refreshToken,
    });
    return response.data.accessToken;
  },

  updateUser: (user) => set({ user }),
}));
```

### 6.2. UI Store

```typescript
// stores/uiStore.ts
interface UIState {
  sidebarOpen: boolean;
  theme: 'light' | 'dark';
  toggleSidebar: () => void;
  setTheme: (theme: 'light' | 'dark') => void;
}

export const useUIStore = create<UIState>((set) => ({
  sidebarOpen: true,
  theme: 'light',
  toggleSidebar: () => set((state) => ({ sidebarOpen: !state.sidebarOpen })),
  setTheme: (theme) => set({ theme }),
}));
```

### 6.3. Chat Store

```typescript
// stores/chatStore.ts
interface ChatState {
  activeConversation: number | null;
  activeGroup: number | null;
  typingUsers: Record<number, string[]>;
  setActiveConversation: (id: number | null) => void;
  setActiveGroup: (id: number | null) => void;
  addTypingUser: (targetId: number, userName: string) => void;
  removeTypingUser: (targetId: number, userName: string) => void;
}

export const useChatStore = create<ChatState>((set) => ({
  activeConversation: null,
  activeGroup: null,
  typingUsers: {},
  setActiveConversation: (id) => set({ activeConversation: id }),
  setActiveGroup: (id) => set({ activeGroup: id }),
  addTypingUser: (targetId, userName) => set((state) => ({
    typingUsers: {
      ...state.typingUsers,
      [targetId]: [...(state.typingUsers[targetId] || []), userName]
    }
  })),
  removeTypingUser: (targetId, userName) => set((state) => ({
    typingUsers: {
      ...state.typingUsers,
      [targetId]: (state.typingUsers[targetId] || []).filter(n => n !== userName)
    }
  })),
}));
```

---

## 7. TypeScript Type Definitions

### 7.1. Auth Types

```typescript
// types/auth.ts
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
}

export interface RefreshTokenRequest {
  token: string;
}
```

### 7.2. User Types

```typescript
// types/user.ts
export interface User {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  roleId?: number;
  roleName?: string;
  isActive?: boolean;
  createdAt?: string;
}

export interface Role {
  id: number;
  name: string;
}

export interface Permission {
  id: number;
  name: string;
  description: string;
}

export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
}
```

### 7.3. Listing Types

```typescript
// types/listing.ts
export interface Listing {
  id: number;
  userId: number;
  title: string;
  description: string;
  price: number;
  isAuction: boolean;
  status: 'active' | 'sold' | 'expired' | 'cancelled';
  createdAt: string;
  updatedAt?: string;
  images: Image[];
}

export interface CreateListingRequest {
  title: string;
  description: string;
  price: number;
  isAuction: boolean;
  startingPrice?: number;
  startTime?: string;
  endTime?: string;
  minBidIncrement?: number;
}

export interface UpdateListingRequest {
  title: string;
  description: string;
  price: number;
}

export interface ListingFilters {
  search?: string;
  minPrice?: number;
  maxPrice?: number;
  status?: string;
  isAuction?: boolean;
  page: number;
  limit: number;
}
```

### 7.4. Auction Types

```typescript
// types/auction.ts
export interface Auction {
  id: number;
  listingId: number;
  title?: string;
  description?: string;
  sellerId?: number;
  startingPrice: number;
  currentPrice: number;
  startTime: string;
  endTime: string;
  minBidIncrement: number;
  winnerUserId: number | null;
  status: 'active' | 'completed' | 'expired' | 'cancelled';
}

export interface AuctionDetailResponse extends Auction {
  title: string;
  sellerId: number;
}

export interface AuctionResultDto {
  auctionId: number;
  winnerUserId: number;
  winnerName: string;
  winningPrice: number;
  status: string;
}

export interface AuctionExtendedDto {
  auctionId: number;
  newEndTime: string;
  extendedSeconds: number;
}

export interface BidDto {
  auctionId: number;
  amount: number;
  bidderId: number;
  bidderName: string;
  timestamp: string;
}
```

### 7.5. Bid Types

```typescript
// types/bid.ts
export interface Bid {
  id: number;
  listingId: number;
  userId: number;
  userName: string;
  amount: number;
  createdAt: string;
}

export interface OfferResponse {
  id: number;
  listingId: number;
  listingTitle: string;
  userId: number;
  userName: string;
  amount: number;
  createdAt: string;
}

export interface MyBidResponse {
  id: number;
  listingId: number;
  listingTitle: string;
  listingPrice: number;
  listingStatus: string;
  amount: number;
  createdAt: string;
}

export interface CreateOfferRequest {
  amount: number;
}

export interface PlaceBidRequest {
  amount: number;
}
```

### 7.6. Group Types

```typescript
// types/group.ts
export interface Group {
  id: number;
  name: string;
  description: string;
  createdBy: number;
  createdAt: string;
  members?: GroupMember[];
}

export interface GroupMember {
  userId: number;
  firstName: string;
  lastName: string;
  joinedAt: string;
}

export interface CreateGroupRequest {
  name: string;
  description: string;
}

export interface GroupMessage {
  id: number;
  groupId: number;
  senderId: number;
  senderName: string;
  content: string;
  sentAt: string;
}

export interface GroupMessageDto {
  id: number;
  groupId: number;
  senderId: number;
  senderName: string;
  content: string;
  sentAt: string;
}
```

### 7.7. Message Types

```typescript
// types/message.ts
export interface Conversation {
  id: number;
  user1Id: number;
  user2Id: number;
  otherUserFirstName: string;
  otherUserLastName: string;
  lastMessage?: string;
  isRead?: boolean;
  createdAt: string;
}

export interface DmMessage {
  id: number;
  conversationId: number;
  senderId: number;
  content: string;
  sentAt: string;
  isRead: boolean;
}

export interface DmMessageDto {
  id: number;
  conversationId: number;
  senderId: number;
  senderName: string;
  content: string;
  sentAt: string;
  isRead: boolean;
}
```

### 7.8. Common Types

```typescript
// types/common.ts
export interface PaginatedResponse<T> {
  data: T[];
  total: number;
  page: number;
  limit: number;
  totalPages: number;
}

export interface PaginationParams {
  page: number;
  limit: number;
}

export interface Image {
  id: number;
  listingId?: number;
  fileName: string;
  mimeType: string;
  uploadedAt: string;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}
```

---

## 8. Page Specifications

### 8.1. HomePage (`/`)
- Hero section with search bar
- Featured listings carousel
- Active auctions preview
- Category quick filters

### 8.2. ListingsPage (`/listings`)
- Grid/list view toggle
- Filters sidebar: price range, status, auction toggle
- Search by title/description
- Pagination controls
- Sort by: price (asc/desc), date (newest/oldest)

### 8.3. ListingDetailPage (`/listings/:id`)
- Image gallery (carousel with thumbnails)
- Listing details: title, description, price, status
- Seller info card
- Offer form (for regular listings)
- Live bid panel (for auction listings)

### 8.4. CreateListingPage (`/listings/create`)
- Form fields: title, description, price
- Auction toggle
- Conditional auction fields: start price, start/end time, min increment
- Multi-image upload (drag & drop)
- Preview before submit

### 8.5. EditListingPage (`/listings/:id/edit`)
- Pre-filled form with existing data
- Same fields as create
- Image management (add/remove)

### 8.6. MyListingsPage (`/my-listings`)
- List of current user's listings
- Status badges
- Quick actions: edit, delete, view

### 8.7. AuctionsPage (`/auctions`)
- Active auctions grid
- Countdown timers
- Current price display
- Filter by status

### 8.8. AuctionDetailPage (`/auctions/:id`)
- Live price display (WebSocket updated)
- Countdown timer
- Bid history list
- Bid form (with min increment validation)
- Anti-snipe warning (last 2 minutes)
- Winner announcement (when ended)

### 8.9. MyBidsPage (`/my-bids`)
- User's bid history
- Grouped by listing
- Status indicators (winning/losing)

### 8.10. LoginPage (`/login`)
- Email/password form
- Validation with Zod
- Error handling
- Link to register

### 8.11. RegisterPage (`/register`)
- Form: firstName, lastName, email, password, confirm password
- Validation with Zod
- Error handling
- Link to login

### 8.12. ProfilePage (`/profile`)
- Display user info
- Edit profile form
- Change password option

### 8.13. GroupsPage (`/groups`)
- All groups list
- My groups section
- Create group button
- Join/leave actions

### 8.14. GroupDetailPage (`/groups/:id`)
- Group info header
- Member list
- Group chat (real-time via SignalR)
- Leave group button

### 8.15. MessagesPage (`/messages`)
- Conversation list (sidebar)
- Last message preview
- Unread indicator
- Start new DM button

### 8.16. ConversationPage (`/messages/:conversationId`)
- Chat window with message bubbles
- Real-time messaging (SignalR)
- Typing indicator
- Read receipts
- Message input with send button

### 8.17. AdminDashboardPage (`/admin`)
- Statistics cards: total users, listings, auctions
- Recent activity feed
- Quick action buttons

### 8.18. AdminUsersPage (`/admin/users`)
- Users table with pagination
- Search and filter
- Status toggle (active/inactive)
- Role assignment dropdown

### 8.19. AdminRolesPage (`/admin/roles`)
- Roles list
- Create/delete roles
- Permission assignment matrix

### 8.20. NotFoundPage (`*`)
- 404 message
- Link to home

---

## 9. Feature Matrix

| Page | Auth | SignalR | API | Special |
|---|---|---|---|---|
| HomePage | ❌ | ❌ | GET listings/auctions | Search |
| ListingsPage | ❌ | ❌ | GET /listings | Filter, pagination |
| ListingDetailPage | ❌ | ❌ | GET /listings/:id | Image gallery |
| AuctionsPage | ❌ | ❌ | GET /auctions | Live countdown |
| AuctionDetailPage | ❌ | ✅ AuctionHub | GET /auctions/:id | Live bid, timer |
| CreateListingPage | ✅ | ❌ | POST /listings | Image upload |
| EditListingPage | ✅ | ❌ | PUT /listings/:id | Image management |
| MyListingsPage | ✅ | ❌ | GET /listings/my | Status badges |
| MyBidsPage | ✅ | ❌ | GET /bids/my | Bid status |
| LoginPage | ❌ | ❌ | POST /auth/login | Form validation |
| RegisterPage | ❌ | ❌ | POST /auth/register | Form validation |
| ProfilePage | ✅ | ❌ | PUT /profile | Form |
| GroupsPage | ✅ | ❌ | GET /groups | Join/leave |
| GroupDetailPage | ✅ | ✅ ChatHub | GET/POST messages | Real-time chat |
| MessagesPage | ✅ | ✅ ChatHub | GET /dm | Last message |
| ConversationPage | ✅ | ✅ ChatHub | GET/POST /dm/:id | Typing indicator |
| AdminDashboardPage | ✅ Admin | ❌ | GET stats | Charts |
| AdminUsersPage | ✅ Admin | ❌ | GET/PUT users | Role management |
| AdminRolesPage | ✅ Admin | ❌ | GET/POST roles | Permissions |

---

## 10. Docker Configuration

### 10.1. Dockerfile

```dockerfile
# Build stage
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

# Serve stage
FROM nginx:alpine AS serve
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

### 10.2. Nginx Configuration

```nginx
server {
    listen 80;
    server_name localhost;
    root /usr/share/nginx/html;
    index index.html;

    # SPA routing
    location / {
        try_files $uri $uri/ /index.html;
    }

    # API proxy
    location /api/ {
        proxy_pass http://carauction_backend:5000/api/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }

    # SignalR WebSocket proxy
    location /hubs/ {
        proxy_pass http://carauction_backend:5000/hubs/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_read_timeout 86400;
    }
}
```

### 10.3. Docker Compose

```yaml
version: '3.8'

services:
  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile
    container_name: carauction_frontend
    ports:
      - "3000:80"
    depends_on:
      - backend
    networks:
      - carauction_network

  backend:
    build:
      context: ./CarAuction.Backend
      dockerfile: Dockerfile
    container_name: carauction_backend
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_URLS=http://+:5000
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=carauction_db;Username=postgres;Password=your_strong_password
      - ConnectionStrings__Redis=redis:6379
      - Jwt__SecretKey=your_jwt_secret_key_here
      - Jwt__Issuer=CarAuction
      - Jwt__Audience=CarAuction
    depends_on:
      - postgres
      - redis
    networks:
      - carauction_network

  postgres:
    image: postgres:16-alpine
    container_name: carauction_postgres
    restart: always
    environment:
      POSTGRES_DB: carauction_db
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: your_strong_password
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - carauction_network

  redis:
    image: redis:7-alpine
    container_name: carauction_redis
    restart: always
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    networks:
      - carauction_network

volumes:
  postgres_data:
  redis_data:

networks:
  carauction_network:
    driver: bridge
```

---

## 11. Environment Variables

### 11.1. Development (`.env`)

```env
VITE_API_URL=http://localhost:5000/api
VITE_SIGNALR_URL=http://localhost:5000/hubs
```

### 11.2. Production (`.env.production`)

```env
VITE_API_URL=/api
VITE_SIGNALR_URL=/hubs
```

---

## 12. Installation & Setup

```bash
# 1. Create project
npm create vite@latest frontend -- --template react-ts

# 2. Install dependencies
npm install @tanstack/react-query zustand axios @microsoft/signalr
npm install react-router-dom react-hook-form @hookform/resolvers zod
npm install tailwindcss postcss autoprefixer clsx tailwind-merge
npm install lucide-react date-fns

# 3. Initialize shadcn/ui
npx shadcn-ui@latest init
npx shadcn-ui@latest add button input dialog toast avatar badge card tabs

# 4. Docker build & run
docker-compose up --build
```

---

## 13. Backend Integration Points

### 13.1. API Base URL
- Development: `http://localhost:5000/api`
- Production: `/api` (via nginx proxy)

### 13.2. SignalR Hubs
- AuctionHub: `/hubs/auction`
- ChatHub: `/hubs/chat`

### 13.3. Authentication Flow
- JWT Access Token: 15-60 minutes lifespan
- Refresh Token: 7 days lifespan
- Token Rotation: New token pair issued on each refresh
- Axios interceptor handles automatic token refresh

### 13.4. Rate Limiting (Backend-Enforced)
| Endpoint | Limit |
|---|---|
| `POST /api/auctions/{id}/bid` | 2 req/s per user |
| `POST /api/dm`, `POST /api/groups` | 5 req/s per user |
| `POST /api/auth/login`, `POST /api/auth/register` | 5 req/min per IP |
| `GET /api/listings`, `GET /api/auctions` | 60 req/min per IP |

---

## 14. Development Phases

| Phase | Features |
|---|---|
| **1. Foundation** | Project setup, routing, layout, auth (login/register) |
| **2. Listings** | List, detail, create, edit, image upload |
| **3. Auctions** | Auction list, detail, live bid, timer, anti-snipe |
| **4. Bids** | Regular offers, bid history |
| **5. Groups** | List, detail, membership, group chat |
| **6. Direct Messages** | Conversation list, real-time messaging |
| **7. Admin** | Dashboard, user/role management |
| **8. Polish** | Responsive design, error handling, loading states, testing |

---

*This document serves as the comprehensive frontend architecture blueprint for the CarAuction project.*
