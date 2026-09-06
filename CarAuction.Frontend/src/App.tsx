import { useEffect } from 'react';
import { Routes, Route } from 'react-router-dom';
import { useAuthStore } from '@/stores/authStore';

// Layouts
import { MainLayout } from '@/components/layout/MainLayout';
import { AuthLayout } from '@/components/layout/AuthLayout';
import { ProtectedRoute } from '@/components/auth/ProtectedRoute';

// Pages
import { HomePage } from '@/pages/HomePage';
import { ListingsPage } from '@/pages/ListingsPage';
import { ListingDetailPage } from '@/pages/ListingDetailPage';
import { CreateListingPage } from '@/pages/CreateListingPage';
import { EditListingPage } from '@/pages/EditListingPage';
import { MyListingsPage } from '@/pages/MyListingsPage';
import { AuctionsPage } from '@/pages/AuctionsPage';
import { AuctionDetailPage } from '@/pages/AuctionDetailPage';
import { MyBidsPage } from '@/pages/MyBidsPage';
import { MyOffersPage } from '@/pages/MyOffersPage';
import { LoginPage } from '@/pages/LoginPage';
import { RegisterPage } from '@/pages/RegisterPage';
import { ProfilePage } from '@/pages/ProfilePage';
import { UserProfilePage } from '@/pages/UserProfilePage';
import { GroupsPage } from '@/pages/GroupsPage';
import { GroupDetailPage } from '@/pages/GroupDetailPage';
import { MessagesPage } from '@/pages/MessagesPage';
import { ConversationPage } from '@/pages/ConversationPage';
import { AdminDashboardPage } from '@/pages/AdminDashboardPage';
import { AdminUsersPage } from '@/pages/AdminUsersPage';
import { AdminRolesPage } from '@/pages/AdminRolesPage';
import { NotFoundPage } from '@/pages/NotFoundPage';

export function App() {
  const { initializeAuth } = useAuthStore();

  useEffect(() => {
    initializeAuth();
  }, [initializeAuth]);

  return (
    <Routes>
      {/* Standalone Auth Routes with dynamic luxury vehicle backdrop */}
      <Route element={<AuthLayout />}>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
      </Route>

      {/* Main Application Layout Routes */}
      <Route element={<MainLayout />}>
        {/* Public Marketplace & Auction Floor */}
        <Route path="/" element={<HomePage />} />
        <Route path="/listings" element={<ListingsPage />} />
        <Route path="/listings/:id" element={<ListingDetailPage />} />
        <Route path="/auctions" element={<AuctionsPage />} />
        <Route path="/auctions/:id" element={<AuctionDetailPage />} />
        <Route path="/users/:id" element={<UserProfilePage />} />

        {/* Authenticated Member Routes */}
        <Route element={<ProtectedRoute />}>
          <Route path="/listings/create" element={<CreateListingPage />} />
          <Route path="/listings/:id/edit" element={<EditListingPage />} />
          <Route path="/my-listings" element={<MyListingsPage />} />
          <Route path="/my-offers" element={<MyOffersPage />} />
          <Route path="/my-bids" element={<MyBidsPage />} />
          <Route path="/profile" element={<ProfilePage />} />
          <Route path="/groups" element={<GroupsPage />} />
          <Route path="/groups/:id" element={<GroupDetailPage />} />
          <Route path="/messages" element={<MessagesPage />} />
          <Route path="/messages/:dmId" element={<ConversationPage />} />
        </Route>

        {/* Protected Administrator Routes */}
        <Route element={<ProtectedRoute role="Admin" />}>
          <Route path="/admin" element={<AdminDashboardPage />} />
          <Route path="/admin/users" element={<AdminUsersPage />} />
          <Route path="/admin/roles" element={<AdminRolesPage />} />
        </Route>

        {/* 404 Catch-All */}
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}
