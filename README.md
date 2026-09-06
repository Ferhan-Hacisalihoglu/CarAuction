<div align="center">

# CarAuction

**A full-stack, real-time luxury and enthusiast vehicle auction platform**

![.NET 8](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet&logoColor=white)
![React](https://img.shields.io/badge/React-18-61DAFB?logo=react&logoColor=black)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-7-DC382D?logo=redis&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-Real--time-FF6F00?logo=dotnet&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)

</div>

---

## 📸 Screenshots

<img width="1919" height="3438" alt="Screenshot 2026-09-06 at 17-55-49 CarAuction — Premium Vehicle Marketplace   Live Auctions" src="https://github.com/user-attachments/assets/3a847b80-b706-4111-8876-7acef658bfcc" />
<img width="1919" height="2023" alt="Screenshot 2026-09-06 at 17-55-56 CarAuction — Premium Vehicle Marketplace   Live Auctions" src="https://github.com/user-attachments/assets/07a9dfe2-9360-4073-bf1b-7ab600ee06e2" />
<img width="1919" height="1445" alt="Screenshot 2026-09-06 at 17-56-07 CarAuction — Premium Vehicle Marketplace   Live Auctions" src="https://github.com/user-attachments/assets/93ced7fb-35cd-4c4c-a81e-9d4059307f23" />
<img width="1919" height="1329" alt="Screenshot 2026-09-06 at 18-01-27 CarAuction — Premium Vehicle Marketplace   Live Auctions" src="https://github.com/user-attachments/assets/6c383262-d74e-4a49-87a6-256dfce2e0bb" />

---

## 🚀 What is CarAuction?

CarAuction is a real-time online auction platform designed for luxury and enthusiast vehicles. Users can list their cars, place bids on live auctions, make direct offers, and communicate with other users through instant messaging and community car clubs.

The platform supports two types of sales:
- **Live Auctions** — Real-time bidding with anti-sniping protection and automatic countdown extensions
- **Direct Offers** — Fixed-price listings where buyers can send offers to sellers

The entire system runs inside Docker containers. You do not need to install Node.js, .NET SDK, PostgreSQL, or Redis on your host machine. Everything is containerized and ready to go.

---

## 🏗️ Technology Stack

### Backend

| Technology | Purpose |
|---|---|
| **.NET 8 / C#** | REST Web API, SignalR hubs, business logic |
| **PostgreSQL 16** | Primary relational database |
| **Redis 7** | Distributed cache, SignalR backplane, session storage |
| **Pure ADO.NET (Npgsql)** | Zero-ORM high-performance data access |
| **ASP.NET Core SignalR** | Real-time WebSocket communication |
| **Docker & Docker Compose** | Container orchestration |

### Frontend

| Technology | Purpose |
|---|---|
| **React 18** | User interface framework |
| **TypeScript** | Type-safe development |
| **Vite** | Build tool and development server |
| **Tailwind CSS** | Utility-first styling |
| **shadcn/ui** | Component library (Radix UI based) |
| **Zustand** | Client-side state management |
| **TanStack Query v5** | Server state caching and synchronization |
| **@microsoft/signalr** | Real-time WebSocket client |
| **Axios** | HTTP client with interceptors |
| **React Hook Form + Zod** | Form validation |
| **Docker + Nginx** | Containerized production deployment |

---

## 🗄️ Database Schema

The database contains **13 normalized tables** organized into three main areas:

### Authentication & Authorization
- `users` — User accounts with hashed passwords and refresh tokens
- `roles` — Role definitions (Admin, User, etc.)
- `permissions` — Granular permission definitions
- `role_permissions` — Many-to-many mapping between roles and permissions

### Vehicles & Commerce
- `listings` — Vehicle listings (auction or direct sale)
- `auctions` — Live auction details with current price and winner tracking
- `bids` — Bid history and direct offers with idempotency support
- `images` — Binary image storage (BYTEA) for vehicle photos

### Social & Messaging
- `groups` — Car enthusiast clubs
- `group_members` — Group membership tracking
- `group_messages` — Group chat messages
- `conversations` — Direct message conversations between two users
- `messages` — Direct message contents

---

## 🔌 API Controllers

The backend exposes **10 REST API controllers**:

| Controller | Base Route | Description |
|---|---|---|
| `AuthController` | `/api/auth` | Register, login, token refresh, logout, profile |
| `UsersController` | `/api/users` | User profiles, admin user management |
| `RolesPermissionsController` | `/api/roles`, `/api/permissions` | RBAC role and permission management |
| `ListingsController` | `/api/listings` | Vehicle CRUD, search, filter, status |
| `AuctionsController` | `/api/auctions` | Auction lifecycle, live bidding |
| `BidsController` | `/api/bids` | Direct offers, bid history, offer accept/reject |
| `ImagesController` | `/api/images` | Image upload, download, deletion |
| `GroupsController` | `/api/groups` | Enthusiast club management |
| `DirectMessagesController` | `/api/dm` | 1-on-1 private messaging |
| `HealthController` | `/health` | Docker health probes (liveness/readiness) |

### Real-Time SignalR Hubs

| Hub | Route | Purpose |
|---|---|---|
| `AuctionHub` | `/hubs/auction` | Live bid notifications, auction timer updates, anti-snipe extensions |
| `ChatHub` | `/hubs/chat` | Direct messages, group messages, typing indicators, read receipts |

---

## 📱 Frontend Pages (22 Pages)

| Page | Route | Description |
|---|---|---|
| Home | `/` | Hero section, featured listings, active auctions |
| Listings | `/listings` | Browse all vehicles with filters and search |
| Listing Detail | `/listings/:id` | Vehicle details, image gallery, offer/bid form |
| Create Listing | `/listings/create` | Create new vehicle listing (auth required) |
| Edit Listing | `/listings/:id/edit` | Edit existing listing (owner only) |
| My Listings | `/my-listings` | View and manage your own listings |
| Auctions | `/auctions` | Browse active live auctions |
| Auction Detail | `/auctions/:id` | Live auction with real-time bidding panel |
| My Bids | `/my-bids` | View your bidding history |
| My Offers | `/my-offers` | View offers you have made or received |
| Groups | `/groups` | Browse car enthusiast clubs |
| Group Detail | `/groups/:id` | Group page with chat and member list |
| Messages | `/messages` | Direct message conversation list |
| Conversation | `/messages/:dmId` | Chat with another user |
| Profile | `/profile` | View and edit your profile |
| Login | `/login` | User authentication |
| Register | `/register` | Create new account |
| Admin Dashboard | `/admin` | System statistics and activity feed |
| Admin Users | `/admin/users` | User management (Admin only) |
| Admin Roles | `/admin/roles` | Role and permission management (Admin only) |

---

## 🛠️ Getting Started

### Prerequisites

- [Docker Desktop]([https://www.docker.com/products/docker-dockerbox/](https://docs.docker.com/get-started/get-docker/))
- Windows, macOS, or Linux operating system

### Quick Start

The project includes convenient **runner.bat** scripts for one-click deployment.

#### 1. Start Everything (Full Stack)

```cmd
runner.bat up
```

This command:
- Creates the shared Docker network (`carauction_network`)
- Starts **PostgreSQL 16** (port 5432)
- Starts **Redis 7** (port 6379)
- Builds and starts the **.NET 8 Backend** (port 5000)
- Builds and starts the **React Frontend** (port 3000)

#### 2. Access the Application

| Service | URL |
|---|---|
| Frontend | `http://localhost:3000` |
| Backend API | `http://localhost:5000` |
| Swagger Docs | `http://localhost:5000/swagger` |

#### 3. Seed the Database (Optional)

To populate the database with sample data (12 luxury cars, demo users, active auctions, groups):

```cmd
SeedData\seed.bat
```

#### 4. Stop All Services

```cmd
runner.bat down
```

### Service-Specific Runners

You can also manage services individually:

```cmd
# Backend only
CarAuction.Backend\runner.bat up

# Frontend only
CarAuction.Frontend\runner.bat up

# Check status
runner.bat status

# View logs
runner.bat logs
```

---

## 🧪 Testing

The project includes **104 unit tests** written with xUnit, FakeItEasy, and FluentAssertions.

### Running Tests

```cmd
cd CarAuction.Backend
dotnet test
```

### Test Coverage

| Test Suite | Tests |
|---|---|
| `AuthControllerTests` | Authentication, registration, token management |
| `UsersControllerTests` | User profiles, admin operations |
| `RolesPermissionsControllerTests` | RBAC management |
| `ListingsControllerTests` | Listing CRUD, search, filtering |
| `AuctionsControllerTests` | Live auction operations |
| `BidsControllerTests` | Direct offers, bid history |
| `ImagesControllerTests` | Image upload and retrieval |
| `GroupsControllerTests` | Group management and messaging |
| `DirectMessagesControllerTests` | Private messaging |
| `AdminControllerTests` | Admin dashboard and statistics |
| `HealthControllerTests` | Container health probes |
| `RedisServicesTests` | Redis fallback resilience |

---

## 🏗️ Architecture Overview

### Clean Architecture (Onion Model)

```
CarAuction.Presentation  →  REST API Controllers + SignalR Hubs
         ↓
CarAuction.Application   →  Business Logic, DTOs, Validators
         ↓
CarAuction.Domain        →  Entities, Enums, Exceptions
         ↑
CarAuction.Infrastructure →  ADO.NET Data Access, Redis, Security
```

### Key Design Decisions

- **Zero ORM Policy**: All database operations use pure ADO.NET with parameterized SQL queries. This eliminates ORM overhead and gives full control over query performance.
- **Anti-Sniping Protection**: When a bid is placed in the final 2 minutes, the auction timer automatically extends by 120 seconds, giving other bidders a fair chance.
- **Idempotency Keys**: Bid requests support idempotency keys to prevent duplicate charges from network retries.
- **Pessimistic Row Locking**: Critical operations like bid placement use `SELECT ... FOR UPDATE` to prevent race conditions.
- **Redis Backplane**: SignalR uses Redis as a backplane to support multiple backend instances.
- **Token Rotation**: Refresh tokens are rotated on each use with a 10-second grace period for parallel requests.

---

## 📁 Project Structure

```
CarAuction/
├── CarAuction.Backend/
│   ├── CarAuction.Domain/          # Entities, Enums, Exceptions
│   ├── CarAuction.Application/     # DTOs, Services, Validators
│   ├── CarAuction.Infrastructure/  # ADO.NET, Redis, Security
│   ├── CarAuction.Presentation/    # Controllers, Hubs, Middleware
│   ├── CarAuction.UnitTest/        # 104 unit tests
│   ├── Dockerfile
│   ├── docker-compose.yml
│   └── runner.bat
│
├── CarAuction.Frontend/
│   ├── src/
│   │   ├── api/                    # API layer (Axios + SignalR)
│   │   ├── components/             # UI components
│   │   ├── pages/                  # 22 page components
│   │   ├── stores/                 # Zustand stores
│   │   ├── hooks/                  # Custom React hooks
│   │   ├── types/                  # TypeScript interfaces
│   │   └── utils/                  # Utilities
│   ├── Dockerfile
│   ├── docker-compose.yml
│   ├── nginx.conf
│   └── runner.bat
│
├── SeedData/                       # Database seeder (local only)
│   ├── seed.bat
│   ├── seed.sql
│   └── 01.png - 12.png             # Vehicle images
│
├── Index.md                        # Project documentation
├── backend.md                      # Backend architecture specs
├── frontend.md                     # Frontend architecture specs
└── runner.bat                      # Full-stack runner
```

---

## 🔒 Security Features

- **Password Hashing**: HMACSHA512 with random 64-byte salt
- **JWT Authentication**: Short-lived access tokens (15-60 minutes)
- **Refresh Token Rotation**: New refresh token issued on every use
- **Rate Limiting**: Configured per endpoint (bidding, messaging, auth)
- **SQL Injection Prevention**: All queries use parameterized commands
- **CORS Configuration**: Strict origin policies
- **Role-Based Access Control (RBAC)**: Dynamic permission system

---

## 📝 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

## 👤 Author

**Ferhan Hacisalihoglu**

- GitHub: [@Ferhan-Hacisalihoglu](https://github.com/Ferhan-Hacisalihoglu)
- LinkedIn: [Ferhan Hacisalihoglu](https://www.linkedin.com/in/ferhan-hacisalihoglu/)

---

<div align="center">

⭐ **If you find this project useful, please give it a star!** ⭐

</div>
