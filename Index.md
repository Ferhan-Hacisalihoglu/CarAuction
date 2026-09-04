# CarAuction — Project Index

> **Project Path:** `C:/Project/Other/CarAuction`
> **GitHub:** https://github.com/Ferhan-Hacisalihoglu/CarAuction
> **Status:** Backend Complete (10 Controllers, 104 Tests) | Frontend Planning & Architecture Complete

---

## 📋 Project Overview

CarAuction is a full-stack vehicle auction platform built with:
- **Backend:** .NET 8, C#, PostgreSQL 16+, Redis 7+, SignalR, Docker
- **Frontend:** React 18, TypeScript, Vite, Tailwind CSS, shadcn/ui, Docker/Nginx
- **Architecture:** Clean Architecture (4 layers: Presentation → Application → Domain → Infrastructure)

---

## 📁 Source Files

| File | Description |
|---|---|
| `backend.md` | Full backend architecture, database schema, API design |
| `frontend.md` | Frontend architecture, component structure, routing, aligned TypeScript DTOs |
| `rules.md` | Project rules, policies, approved libraries, git workflow |
| `Index.md` | This file — project index |

---

## 🔄 Git & GitHub Synchronization Policy

> [!IMPORTANT]
> **Continuous Progress Syncing Rule:**
> In this project, changes **MUST** be committed and pushed to GitHub at regular and meaningful progression milestones:
> 1. **Feature / Endpoint Completion:** When an API controller, service, or feature is completed and tested.
> 2. **Bug Fixes & Defect Remediation:** Whenever bugs, schema mismatches, or test failures are resolved.
> 3. **Milestone / Phase Transitions:** Upon completing major architectural layers (e.g., Backend Complete, Frontend Foundation, Real-Time SignalR).
> 4. **Documentation Updates:** Whenever architecture docs (`Index.md`, `backend.md`, `frontend.md`, `rules.md`) or database schemas are updated.
> 5. **Session Wrap-Up:** Before ending any major work session, the working tree must be verified clean and all passing tests pushed to the remote repository.

---

## 🏗️ Tech Stack

### Backend
| Technology | Purpose |
|---|---|
| .NET 8 / C# | Web API framework |
| PostgreSQL 16+ | Primary database |
| Redis 7+ | Cache, sessions, SignalR backplane |
| SignalR | Real-time WebSocket |
| Pure ADO.NET (Npgsql) | Data access (no ORM) |
| Docker & Docker Compose | Containerization |

### Frontend
| Technology | Purpose |
|---|---|
| React 18+ | UI framework |
| TypeScript | Type safety |
| Vite | Build tool |
| Tailwind CSS | Styling |
| shadcn/ui | Component library |
| Zustand | State management |
| TanStack Query | Server state |
| SignalR | Real-time communication |

---

## 🗄️ Database (13 Tables)

`users`, `roles`, `permissions`, `role_permissions`, `listings`, `auctions`, `bids`, `images`, `groups`, `group_members`, `conversations`, `messages`, `group_messages`

---

## 🔌 API (10 Controllers)

`Auth`, `Users`, `RolesPermissions`, `Listings`, `Auctions`, `Bids`, `Images`, `Groups`, `DirectMessages`, `Health`

---

## 🧪 Testing

- **Framework:** xUnit + FakeItEasy + FluentAssertions
- **Test Count:** 104 unit tests (100% pass)
- **Scope:** API controllers (10 test suites) and Redis infrastructure fallbacks

---

## 🚀 Deployment

- **Backend:** Docker Container (Kestrel on port 5000)
- **Frontend:** Docker Container (Nginx on port 3000)
- **Database:** PostgreSQL 16 Alpine
- **Cache:** Redis 7 Alpine
- **Orchestration:** Root `docker-compose.yml` (full-stack) & `CarAuction.Backend/docker-compose.yml` (backend-only)

---

## 📊 Progress

| Component | Status |
|---|---|
| Backend API | ✅ Complete (10 Controllers, 125 files) |
| Unit Tests | ✅ Complete (104 tests passing) |
| SignalR Hubs | ✅ Complete (AuctionHub, ChatHub) |
| Background Services | ✅ Complete (AuctionExpiryWorker) |
| Docker Config | ✅ Complete (Root & Backend compose) |
| Architecture Rules | ✅ Complete (`rules.md`) |
| Frontend Specs | ✅ Complete (`frontend.md` schema aligned) |
| Frontend App | 📋 Ready for Implementation Phase |

---

*Last updated: 2026-09-04*
