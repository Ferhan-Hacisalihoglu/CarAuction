# Backend Architecture, Database Schema & API Design

> [!CAUTION]
> **STRICT DATABASE SCHEMA POLICY:**
> All application layers, entities, DTOs, queries, services, and API controllers **MUST** be implemented strictly in accordance with this exact schema.
> Any subsequent changes must follow the database versioning and migration rules defined in the project.

---

## 1. Technology Stack & Infrastructure

The backend ecosystem utilizes the following core technologies, running either as isolated Docker containers or on the local host during debugging:

| Technology | Purpose | Hosting / Environment |
|---|---|---|
| **.NET 8 / C#** | Web API backend framework, SignalR WebSocket server, business logic | **Docker Container** (`Dockerfile`) / Kestrel |
| **PostgreSQL (v16+)** | Primary relational database hosting the schema | **Docker Container** (`postgres:16-alpine`) |
| **Redis (v7+)** | High-speed distributed cache, live auction state caching, SignalR Redis backplane, Rate Limiting | **Docker Container** (`redis:7-alpine`) |
| **Docker & Docker Compose** | Multi-container orchestration, isolated bridge networking, persistent storage | Local Dev & Production Containers |
| **Pure ADO.NET (`Npgsql`)** | High-performance, zero-ORM data access layer using raw parameterized SQL queries | NuGet package (`Npgsql`) in .NET |
| **ASP.NET Core SignalR** | Real-time WebSocket hubs for live auction bidding and instant chat | Built-in ASP.NET Core + StackExchange.Redis |

### 1.1. Backend Dockerfile (`Dockerfile`)

Multi-stage build optimizing the ASP.NET Core .NET 8 Web API image:

```dockerfile
# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["CarAuction.Presentation/CarAuction.Presentation.csproj", "CarAuction.Presentation/"]
COPY ["CarAuction.Application/CarAuction.Application.csproj", "CarAuction.Application/"]
COPY ["CarAuction.Domain/CarAuction.Domain.csproj", "CarAuction.Domain/"]
COPY ["CarAuction.Infrastructure/CarAuction.Infrastructure.csproj", "CarAuction.Infrastructure/"]

RUN dotnet restore "CarAuction.Presentation/CarAuction.Presentation.csproj"

# Copy remaining source code and publish
COPY . .
WORKDIR "/src/CarAuction.Presentation"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5000

ENTRYPOINT ["dotnet", "CarAuction.Presentation.dll"]
```

### 1.2. Docker Environment (`docker-compose.yml`)

The backend, database, and Redis cache are orchestrated together with robust health checks:

```yaml
version: '3.8'

services:
  backend:
    build:
      context: ./CarAuction.Backend
      dockerfile: Dockerfile
    container_name: carauction_backend
    restart: always
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:5000
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=carauction_db;Username=postgres;Password=your_strong_password
      - ConnectionStrings__Redis=redis:6379
      - Jwt__SecretKey=your_super_secret_jwt_key_at_least_64_characters_long_123456
      - Jwt__Issuer=CarAuction
      - Jwt__Audience=CarAuction
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
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
      - ./init.sql:/docker-entrypoint-initdb.d/01-init.sql:ro
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres -d carauction_db"]
      interval: 5s
      timeout: 5s
      retries: 5
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
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 5s
      timeout: 5s
      retries: 5
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

## 2. Database Schema (PostgreSQL)

All table names, column names, constraints, and indexes are defined **strictly in English** following clean `snake_case` PostgreSQL conventions.

### 2.1. Users Table
```sql
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,         -- password hash (HMACSHA512)
    salt VARCHAR(255) NOT NULL,                  -- cryptographic salt
    role_id INTEGER REFERENCES roles(id) ON DELETE SET NULL,
    refresh_token VARCHAR(500),
    refresh_token_expiry TIMESTAMP WITH TIME ZONE,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);
```

### 2.2. Roles Table
```sql
CREATE TABLE roles (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) UNIQUE NOT NULL
);
```

### 2.3. Permissions Table
```sql
CREATE TABLE permissions (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) UNIQUE NOT NULL,
    description TEXT
);
```

### 2.4. Role-Permission Junction Table (Many-to-Many)
```sql
CREATE TABLE role_permissions (
    role_id INTEGER REFERENCES roles(id) ON DELETE CASCADE,
    permission_id INTEGER REFERENCES permissions(id) ON DELETE CASCADE,
    PRIMARY KEY (role_id, permission_id)
);
```

### 2.5. Listings Table
```sql
CREATE TABLE listings (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    price DECIMAL(18,2),                         -- Upgraded to DECIMAL(18,2) for luxury/hypercar valuation
    is_auction BOOLEAN DEFAULT FALSE,            -- is auction listing?
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE,
    status VARCHAR(50) DEFAULT 'active'          -- active, sold, expired, cancelled
);
```

### 2.6. Auction Details (1-to-1 with listings)
```sql
CREATE TABLE auctions (
    id SERIAL PRIMARY KEY,
    listing_id INTEGER UNIQUE NOT NULL REFERENCES listings(id) ON DELETE CASCADE,
    starting_price DECIMAL(18,2) NOT NULL,
    current_price DECIMAL(18,2) NOT NULL,
    start_time TIMESTAMP WITH TIME ZONE NOT NULL,
    end_time TIMESTAMP WITH TIME ZONE NOT NULL,
    min_bid_increment DECIMAL(18,2) DEFAULT 1.00,
    winner_user_id INTEGER REFERENCES users(id) ON DELETE SET NULL,
    status VARCHAR(50) DEFAULT 'active'          -- active, completed, expired, cancelled
);
```

### 2.7. Bids / Offers (For regular listings or auctions)
```sql
CREATE TABLE bids (
    id SERIAL PRIMARY KEY,
    listing_id INTEGER NOT NULL REFERENCES listings(id) ON DELETE CASCADE,
    user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    amount DECIMAL(18,2) NOT NULL,
    idempotency_key VARCHAR(100),               -- Client deduplication key (X-Idempotency-Key)
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);
```

### 2.8. Listing Images
```sql
CREATE TABLE images (
    id SERIAL PRIMARY KEY,
    listing_id INTEGER NOT NULL REFERENCES listings(id) ON DELETE CASCADE,
    image_data BYTEA NOT NULL,                   -- binary image data
    file_name VARCHAR(255),
    mime_type VARCHAR(100),
    uploaded_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);
```

### 2.9. Groups
```sql
CREATE TABLE groups (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    created_by INTEGER REFERENCES users(id) ON DELETE SET NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);
```

### 2.10. Group Members (Many-to-Many)
```sql
CREATE TABLE group_members (
    group_id INTEGER REFERENCES groups(id) ON DELETE CASCADE,
    user_id INTEGER REFERENCES users(id) ON DELETE CASCADE,
    joined_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    PRIMARY KEY (group_id, user_id)
);
```

### 2.11. Direct Message Conversations (Between two users)
```sql
CREATE TABLE conversations (
    id SERIAL PRIMARY KEY,
    user1_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    user2_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    CONSTRAINT check_distinct_conversation_users CHECK (user1_id != user2_id),
    CONSTRAINT unique_conversation_user_pair UNIQUE (user1_id, user2_id)
);
```
> *Rule:* When querying or inserting conversations, application logic always ensures `user1_id < user2_id` (`user1_id = LEAST(u1, u2)` and `user2_id = GREATEST(u1, u2)`), guaranteeing bidirectional uniqueness.

### 2.12. Direct Message Contents
```sql
CREATE TABLE messages (
    id SERIAL PRIMARY KEY,
    conversation_id INTEGER NOT NULL REFERENCES conversations(id) ON DELETE CASCADE,
    sender_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    content TEXT NOT NULL,
    sent_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    is_read BOOLEAN DEFAULT FALSE
);
```

### 2.13. Group Messages
```sql
CREATE TABLE group_messages (
    id SERIAL PRIMARY KEY,
    group_id INTEGER NOT NULL REFERENCES groups(id) ON DELETE CASCADE,
    sender_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    content TEXT NOT NULL,
    sent_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);
```

### 2.14. Performance Indexes
```sql
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_listings_user_id ON listings(user_id);
CREATE INDEX idx_listings_status ON listings(status);
CREATE INDEX idx_auctions_listing_id ON auctions(listing_id);
CREATE INDEX idx_auctions_end_time_status ON auctions(end_time, status);
CREATE INDEX idx_auctions_winner_user_id ON auctions(winner_user_id);
CREATE INDEX idx_bids_listing_id ON bids(listing_id);
CREATE INDEX idx_bids_user_id ON bids(user_id);
CREATE INDEX idx_bids_created_at ON bids(created_at DESC);
CREATE INDEX idx_bids_idempotency ON bids(idempotency_key) WHERE idempotency_key IS NOT NULL;
CREATE INDEX idx_images_listing_id ON images(listing_id);
CREATE INDEX idx_conversations_user_pair ON conversations(user1_id, user2_id);
CREATE INDEX idx_messages_conversation_id ON messages(conversation_id);
CREATE INDEX idx_messages_sent_at ON messages(sent_at ASC);
CREATE INDEX idx_group_messages_group_id ON group_messages(group_id);
CREATE INDEX idx_group_messages_sent_at ON group_messages(sent_at ASC);
```

### 2.15. Automated Database Initialization (`init.sql`)
Mounted directly into `/docker-entrypoint-initdb.d/01-init.sql` on the PostgreSQL container, this script creates default roles (`Admin`, `User`) and sample permissions upon initial container boot:

```sql
-- Initial roles seed
INSERT INTO roles (name) VALUES ('Admin'), ('User') ON CONFLICT (name) DO NOTHING;

-- Initial permissions seed
INSERT INTO permissions (name, description) VALUES
  ('auction.bid', 'Ability to place bids on live auctions'),
  ('listing.create', 'Ability to create vehicle listings'),
  ('admin.manage', 'Access to administrative panel and user moderation')
ON CONFLICT (name) DO NOTHING;
```

---

## 3. Backend Architecture & Working Principles

The backend is built on **.NET 8** using **Pure ADO.NET (Npgsql)** for PostgreSQL data access, **Redis** for distributed caching / real-time synchronization, and **Clean Architecture (Onion / Layered Architecture)** principles.

```
CarAuction.Presentation (API Controllers & Real-Time SignalR WebSockets)
       │
       ▼
CarAuction.Application (Business Logic, DTOs & Service Interfaces)
       │
       ▼
CarAuction.Domain (Entities, Value Objects & Enums)
       ▲
       │
CarAuction.Infrastructure (Pure ADO.NET Data Access, Redis Caching, Security, Background Workers)
```

### 3.1. Data & Caching Strategy
- **PostgreSQL with Pure ADO.NET:** All database operations use pure **ADO.NET (`NpgsqlConnection`, `NpgsqlCommand`, `NpgsqlDataReader`, `NpgsqlTransaction`)**. No ORM (No EF Core, No Dapper).
- **Redis Caching:**
  - **Live Auction State:** Active auction current prices, winner IDs, and timer states cached in Redis keys (e.g., `auction:{id}:state`) for sub-millisecond lookups.
  - **SignalR Backplane:** Redis backplane distributes WebSocket events across multiple application instances.
  - **Refresh Tokens & Sessions:** Fast session lookups, blacklist revocation, and rate limiting buckets.
- **SQL Parameterization:** Every query strictly uses strongly-typed parameters (`NpgsqlParameter`) to eliminate SQL injection risks.
- **Transaction Safety & Pessimistic Locks:** Multi-step operations and bid submissions use `NpgsqlTransaction` and row locks (`SELECT ... FOR UPDATE`).
- **Connection Pooling:** Managed via `NpgsqlDataSource` with `using` blocks to prevent connection leaks.

### 3.2. Layer Responsibilities

1. **Domain Layer (`CarAuction.Domain`)**:
   - Contains POCO entity models matching the exact database schema.
   - Contains business enums (`ListingStatus`, `AuctionStatus`, `UserRoles`), domain exceptions, and base types.
   - Zero external framework dependencies.

2. **Application Layer (`CarAuction.Application`)**:
   - Contains business logic, DTOs, request validation rules (FluentValidation), and service interfaces.
   - Defines contracts for repositories, Redis cache service, authentication, and real-time messaging.
   - Independent of specific database providers or web frameworks.

3. **Infrastructure Layer (`CarAuction.Infrastructure`)**:
   - Implements data access using **Pure ADO.NET** with raw SQL queries matching exact database tables and columns.
   - Implements **Redis Cache Service** using `StackExchange.Redis`.
   - Handles password hashing (HMACSHA512 with random salt), JWT token creation, and background worker jobs (e.g., auction expiration monitor).

4. **Presentation Layer (`CarAuction.Presentation`)**:
   - Hosts ASP.NET Core REST API controllers.
   - Hosts **SignalR Hubs** (with Redis backplane support) for real-time live auction updates and chat messaging.
   - Configures middleware (Global Exception Handling, JWT Authentication, Rate Limiting, CORS, Swagger).

---

## 4. Backend Folder Structure (Directories Only)

```
CarAuction.Backend/
├── CarAuction.Domain/
│   ├── Entities/                   # Models matching exact database tables
│   ├── Enums/                      # Status enums and role types
│   ├── Exceptions/                 # Domain-specific business exceptions
│   └── Common/                     # Shared base types
│
├── CarAuction.Application/
│   ├── Interfaces/
│   │   ├── Repositories/           # Data access repository contracts
│   │   ├── Services/               # Application business service contracts
│   │   ├── Caching/                # Redis distributed cache service contracts
│   │   └── Hubs/                   # SignalR hub client contracts
│   ├── DTOs/
│   │   ├── Auth/                   # Login, register, token DTOs
│   │   ├── Users/                  # User management and profile DTOs
│   │   ├── Listings/               # Vehicle listing and filter DTOs
│   │   ├── Auctions/               # Live auction and bidding DTOs
│   │   ├── Images/                 # Binary image upload and response DTOs
│   │   ├── Groups/                 # Group and group membership DTOs
│   │   └── Chat/                   # Direct messaging and group chat DTOs
│   ├── Validators/                 # Input validation rules (FluentValidation)
│   └── Services/                   # Application service implementations
│
├── CarAuction.Infrastructure/
│   ├── Data/
│   │   ├── Connection/             # ADO.NET Connection factory (NpgsqlDataSource) and transaction manager
│   │   └── Helpers/                # Data reader extensions and SQL query helpers
│   ├── Repositories/               # ADO.NET repository implementations (NpgsqlCommand & DataReader)
│   ├── Caching/                    # Redis cache implementation (StackExchange.Redis)
│   ├── Security/                   # Password hasher (salt + hash) and JWT Token service
│   └── BackgroundServices/         # Periodic auction expiration and status workers
│
└── CarAuction.Presentation/
    ├── Controllers/                # REST API controllers
    ├── Hubs/                       # SignalR WebSockets for live bids and messaging
    ├── Middlewares/                # Global exception handling, rate limiting and auth middlewares
    └── Extensions/                 # Dependency injection and service configuration extensions
```

---

## 5. Controllers & API Endpoint Specifications

All endpoints follow RESTful standards, return standard JSON responses, and use appropriate HTTP status codes (`200 OK`, `201 Created`, `400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, `429 Too Many Requests`, `500 Internal Server Error`).

> [!IMPORTANT]
> **API Serialization Convention:**
> ASP.NET Core is configured with `System.Text.Json` using `JsonNamingPolicy.CamelCase` globally:
> ```csharp
> builder.Services.AddControllers().AddJsonOptions(opts => {
>     opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
> });
> ```
> This ensures seamless compatibility between backend C# models (`CurrentPrice`, `WinnerUserId`) and frontend TypeScript interfaces (`currentPrice`, `winnerUserId`).

```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { ... }
}
```

---

### 5.1. AuthController (`/api/auth`)
Handles user authentication, token issuance, token rotation, and session management.

| Method | Endpoint | Auth | Description | ADO.NET & Redis Operations |
|---|---|---|---|---|
| `POST` | `/api/auth/register` | Public | Register new user | Generates random salt (`salt`), hashes password (`password_hash`), executes `INSERT INTO users (first_name, last_name, email, password_hash, salt, role_id, is_active) VALUES (...)` |
| `POST` | `/api/auth/login` | Public | Login with email & password | Executes `SELECT id, first_name, last_name, email, password_hash, salt, role_id, is_active FROM users WHERE email = @email`, validates password hash. Generates JWT access token and cryptographically secure refresh token (with expiry, e.g. 7 days). Updates `UPDATE users SET refresh_token = @token, refresh_token_expiry = @expiry WHERE id = @id` and stores session key in Redis |
| `POST` | `/api/auth/refresh-token` | Public | Refresh expired access token (with Token Rotation & Grace Cache) | In an ADO.NET transaction: executes `SELECT id, email, role_id FROM users WHERE refresh_token = @token AND refresh_token_expiry > NOW() AND is_active = TRUE`. If valid, generates a new JWT access token and a **brand-new refresh token**, updates DB with new refresh token and expiry, places old token in Redis with a 10s grace period to accommodate in-flight parallel requests, and returns new tokens |
| `POST` | `/api/auth/logout` | `[Authorize]` | Invalidate user session | Executes `UPDATE users SET refresh_token = NULL, refresh_token_expiry = NULL WHERE id = @userId` and removes session key from Redis |
| `GET` | `/api/auth/me` | `[Authorize]` | Get current user profile | Executes `SELECT u.id, u.first_name, u.last_name, u.email, r.name as role_name FROM users u LEFT JOIN roles r ON u.role_id = r.id WHERE u.id = @userId` |

---

### 5.2. UsersController (`/api/users`)
Handles user account management and administration.

| Method | Endpoint | Auth | Description | ADO.NET Operations |
|---|---|---|---|---|
| `GET` | `/api/users` | `[Authorize(Roles="Admin")]` | Get paginated user list | Executes `SELECT u.*, r.name as role_name FROM users u LEFT JOIN roles r ON u.role_id = r.id ORDER BY u.created_at DESC LIMIT @limit OFFSET @offset` |
| `GET` | `/api/users/{id}` | `[Authorize]` | Get user profile by ID | Executes `SELECT id, first_name, last_name, email, role_id, is_active, created_at FROM users WHERE id = @id` |
| `PUT` | `/api/users/profile` | `[Authorize]` | Update own profile | Executes `UPDATE users SET first_name = @firstName, last_name = @lastName WHERE id = @userId` |
| `PUT` | `/api/users/{id}/status` | `[Authorize(Roles="Admin")]` | Toggle user active status | Executes `UPDATE users SET is_active = @isActive WHERE id = @id` |
| `PUT` | `/api/users/{id}/role` | `[Authorize(Roles="Admin")]` | Change user role | Executes `UPDATE users SET role_id = @roleId WHERE id = @id` |

---

### 5.3. Roles & PermissionsController (`/api/roles`, `/api/permissions`)
Handles RBAC (Role-Based Access Control) configuration.

| Method | Endpoint | Auth | Description | ADO.NET Operations |
|---|---|---|---|---|
| `GET` | `/api/roles` | `[Authorize(Roles="Admin")]` | List all roles | Executes `SELECT id, name FROM roles ORDER BY id ASC` |
| `POST` | `/api/roles` | `[Authorize(Roles="Admin")]` | Create new role | Executes `INSERT INTO roles (name) VALUES (@name) RETURNING id` |
| `DELETE` | `/api/roles/{id}` | `[Authorize(Roles="Admin")]` | Delete role | Checks if role is assigned to active users. If safe, executes `DELETE FROM roles WHERE id = @id` |
| `GET` | `/api/permissions` | `[Authorize(Roles="Admin")]` | List all permissions | Executes `SELECT id, name, description FROM permissions ORDER BY id ASC` |
| `GET` | `/api/roles/{roleId}/permissions` | `[Authorize(Roles="Admin")]` | Get permissions of role | Executes `SELECT p.* FROM permissions p INNER JOIN role_permissions rp ON p.id = rp.permission_id WHERE rp.role_id = @roleId` |
| `POST` | `/api/roles/{roleId}/permissions` | `[Authorize(Roles="Admin")]` | Assign permissions to role | Executes `INSERT INTO role_permissions (role_id, permission_id) VALUES (@roleId, @permissionId) ON CONFLICT DO NOTHING` |
| `DELETE` | `/api/roles/{roleId}/permissions/{permissionId}` | `[Authorize(Roles="Admin")]` | Remove permission from role | Executes `DELETE FROM role_permissions WHERE role_id = @roleId AND permission_id = @permissionId` |

---

### 5.4. ListingsController (`/api/listings`)
Handles vehicle listing creation, management, search, and filtering.

| Method | Endpoint | Auth | Description | ADO.NET & Redis Operations |
|---|---|---|---|---|
| `GET` | `/api/listings` | Public | Get listings with filters (search, price range, status, is_auction, pagination) | Executes parameterized dynamic SQL on `listings` with optional filters, ordering, pagination |
| `GET` | `/api/listings/{id}` | Public | Get listing details | Checks Redis cache first; if cache miss, executes `SELECT * FROM listings WHERE id = @id` with associated images and caches result |
| `POST` | `/api/listings` | `[Authorize]` | Create new listing | **ADO.NET Transaction:** Executes `INSERT INTO listings (user_id, title, description, price, is_auction, status) VALUES (...) RETURNING id`. If `is_auction = TRUE`, executes `INSERT INTO auctions (listing_id, starting_price, current_price, start_time, end_time, min_bid_increment, status) VALUES (...)` |
| `PUT` | `/api/listings/{id}` | `[Authorize]` | Update listing details | Executes `UPDATE listings SET title = @title, description = @desc, price = @price, updated_at = NOW() WHERE id = @id AND user_id = @userId`, invalidates Redis cache |
| `DELETE` | `/api/listings/{id}` | `[Authorize]` | Cancel / Delete listing (Soft delete) | Executes `UPDATE listings SET status = 'cancelled', updated_at = NOW() WHERE id = @id AND user_id = @userId`, invalidates Redis cache |
| `GET` | `/api/listings/my` | `[Authorize]` | Get current user's listings | Executes `SELECT * FROM listings WHERE user_id = @userId ORDER BY created_at DESC` |

---

### 5.5. AuctionsController (`/api/auctions`)
Handles real-time and scheduled auction operations.

| Method | Endpoint | Auth | Description | ADO.NET & Redis Operations |
|---|---|---|---|---|
| `GET` | `/api/auctions` | Public | List active auctions | Executes `SELECT a.*, l.title, l.description, l.user_id as seller_id FROM auctions a INNER JOIN listings l ON a.listing_id = l.id WHERE a.end_time > NOW() AND a.status = 'active'` |
| `GET` | `/api/auctions/{id}` | Public | Get auction details by ID | Reads live price & status from Redis / PostgreSQL fallback |
| `GET` | `/api/auctions/listing/{listingId}` | Public | Get auction by listing ID | Executes `SELECT * FROM auctions WHERE listing_id = @listingId` |
| `POST` | `/api/auctions/{id}/bid` | `[Authorize]` | Place bid on auction (Anti-sniping & Idempotency protected) | **ADO.NET Transaction with Row Lock:**<br>1. Checks optional `X-Idempotency-Key` header in Redis/DB to reject duplicate network requests<br>2. `SELECT current_price, end_time, min_bid_increment, listing_id, status FROM auctions WHERE id = @id FOR UPDATE`<br>3. Validates `status = 'active'` and `NOW() BETWEEN start_time AND end_time`<br>4. Validates `amount >= current_price + min_bid_increment`<br>5. **Anti-sniping check:** If `end_time - NOW() <= INTERVAL '120 seconds'`, extends `end_time = NOW() + INTERVAL '120 seconds'`<br>6. `INSERT INTO bids (listing_id, user_id, amount, idempotency_key) VALUES (@listingId, @userId, @amount, @key)`<br>7. `UPDATE auctions SET current_price = @amount, winner_user_id = @userId, end_time = @newEndTime WHERE id = @id`<br>8. Updates Redis live auction cache<br>9. Commits transaction and broadcasts `ReceiveNewBid` (and `AuctionTimeExtended` if extended) via `AuctionHub` |
| `GET` | `/api/auctions/{id}/bids` | Public | Get bid history for auction | Executes `SELECT b.*, u.first_name, u.last_name FROM bids b INNER JOIN auctions a ON b.listing_id = a.listing_id INNER JOIN users u ON b.user_id = u.id WHERE a.id = @id ORDER BY b.created_at DESC` |

---

### 5.6. BidsController (`/api/bids`)
Handles regular listing offers and user bidding history.

| Method | Endpoint | Auth | Description | ADO.NET Operations |
|---|---|---|---|---|
| `POST` | `/api/listings/{listingId}/offers` | `[Authorize]` | Send price offer on normal listing | Executes `INSERT INTO bids (listing_id, user_id, amount) VALUES (@listingId, @userId, @amount)` |
| `GET` | `/api/listings/{listingId}/offers` | `[Authorize]` | Get offers for a listing | Executes `SELECT b.*, u.first_name, u.last_name FROM bids b INNER JOIN users u ON b.user_id = u.id WHERE b.listing_id = @listingId ORDER BY b.created_at DESC` |
| `GET` | `/api/bids/my` | `[Authorize]` | Get current user's bid/offer history | Executes `SELECT b.*, l.title, l.price, l.status FROM bids b INNER JOIN listings l ON b.listing_id = l.id WHERE b.user_id = @userId ORDER BY b.created_at DESC` |

---

### 5.7. ImagesController (`/api/images`)
Handles binary image storage directly in PostgreSQL as `BYTEA` using high-performance streaming.

| Method | Endpoint | Auth | Description | ADO.NET Operations |
|---|---|---|---|---|
| `POST` | `/api/listings/{listingId}/images` | `[Authorize]` | Upload image for listing | Validates caller ownership or Admin role. Enforces max 10MB limit. Executes `INSERT INTO images (listing_id, image_data, file_name, mime_type) VALUES (@listingId, @imageData, @fileName, @mimeType) RETURNING id` |
| `GET` | `/api/images/{id}` | Public | Stream/download image binary | Executes `SELECT image_data, mime_type, file_name FROM images WHERE id = @id` using `CommandBehavior.SequentialAccess` and `NpgsqlDataReader.GetBytes` to stream directly to HTTP response without Large Object Heap (LOH) memory bloat |
| `DELETE` | `/api/images/{id}` | `[Authorize]` | Delete listing image | Executes `DELETE FROM images WHERE id = @id AND (listing_id IN (SELECT id FROM listings WHERE user_id = @userId) OR EXISTS (SELECT 1 FROM users u INNER JOIN roles r ON u.role_id = r.id WHERE u.id = @userId AND r.name = 'Admin'))` |

---

### 5.8. GroupsController (`/api/groups`)
Handles community groups and group messaging.

| Method | Endpoint | Auth | Description | ADO.NET Operations |
|---|---|---|---|---|
| `GET` | `/api/groups` | `[Authorize]` | List all groups | Executes `SELECT * FROM groups ORDER BY created_at DESC` |
| `GET` | `/api/groups/my` | `[Authorize]` | List groups user has joined | Executes `SELECT g.* FROM groups g INNER JOIN group_members gm ON g.id = gm.group_id WHERE gm.user_id = @userId` |
| `POST` | `/api/groups` | `[Authorize]` | Create new group | **ADO.NET Transaction:**<br>1. `INSERT INTO groups (name, description, created_by) VALUES (@name, @desc, @userId) RETURNING id`<br>2. `INSERT INTO group_members (group_id, user_id) VALUES (@groupId, @userId)` |
| `GET` | `/api/groups/{id}` | `[Authorize]` | Get group info & member list | Executes `SELECT * FROM groups WHERE id = @id` and `SELECT u.id, u.first_name, u.last_name, gm.joined_at FROM group_members gm INNER JOIN users u ON gm.user_id = u.id WHERE gm.group_id = @id` |
| `POST` | `/api/groups/{id}/join` | `[Authorize]` | Join group | Executes `INSERT INTO group_members (group_id, user_id) VALUES (@groupId, @userId) ON CONFLICT DO NOTHING` |
| `POST` | `/api/groups/{id}/leave` | `[Authorize]` | Leave group | Executes `DELETE FROM group_members WHERE group_id = @groupId AND user_id = @userId` |
| `GET` | `/api/groups/{id}/messages` | `[Authorize]` | Get group chat message history | Executes `SELECT gm.*, u.first_name, u.last_name FROM group_messages gm INNER JOIN users u ON gm.sender_id = u.id WHERE gm.group_id = @groupId ORDER BY gm.sent_at ASC LIMIT @limit OFFSET @offset` |
| `POST` | `/api/groups/{id}/messages` | `[Authorize]` | Send message to group | Validates group membership. Executes `INSERT INTO group_messages (group_id, sender_id, content) VALUES (@groupId, @userId, @content) RETURNING id, sent_at`, broadcasts to group via `ChatHub` |

---

### 5.9. DirectMessagesController (`/api/dm`)
Handles private 1-on-1 direct conversations between users.

| Method | Endpoint | Auth | Description | ADO.NET Operations |
|---|---|---|---|---|
| `GET` | `/api/dm` | `[Authorize]` | List user's active DM conversations | Executes `SELECT c.id, c.user1_id, c.user2_id, c.created_at, u.first_name as other_first_name, u.last_name as other_last_name, (SELECT content FROM messages WHERE conversation_id = c.id ORDER BY sent_at DESC LIMIT 1) as last_message, (SELECT is_read FROM messages WHERE conversation_id = c.id AND sender_id != @userId ORDER BY sent_at DESC LIMIT 1) as is_read FROM conversations c INNER JOIN users u ON (CASE WHEN c.user1_id = @userId THEN c.user2_id ELSE c.user1_id END) = u.id WHERE c.user1_id = @userId OR c.user2_id = @userId` |
| `POST` | `/api/dm/start` | `[Authorize]` | Start or get existing DM conversation with user | Sets `@u1 = LEAST(@userId, @targetUserId)` and `@u2 = GREATEST(@userId, @targetUserId)`. Executes `SELECT id FROM conversations WHERE user1_id = @u1 AND user2_id = @u2`. If not found, executes `INSERT INTO conversations (user1_id, user2_id) VALUES (@u1, @u2) ON CONFLICT (user1_id, user2_id) DO UPDATE SET created_at = conversations.created_at RETURNING id` |
| `GET` | `/api/dm/{conversationId}/messages` | `[Authorize]` | Get conversation message history | Validates that caller is participant (`user1_id = @userId OR user2_id = @userId`). Executes `SELECT * FROM messages WHERE conversation_id = @conversationId ORDER BY sent_at ASC LIMIT @limit OFFSET @offset` |
| `POST` | `/api/dm/{conversationId}/messages` | `[Authorize]` | Send private direct message | Validates that caller is participant. Executes `INSERT INTO messages (conversation_id, sender_id, content) VALUES (@conversationId, @userId, @content) RETURNING id, sent_at`, broadcasts to recipient via `ChatHub` |
| `PUT` | `/api/dm/{conversationId}/read` | `[Authorize]` | Mark messages as read | Executes `UPDATE messages SET is_read = TRUE WHERE conversation_id = @conversationId AND sender_id != @userId AND is_read = FALSE` |

---

### 5.10. HealthController (`/health`)
Provides container liveness and readiness probes for Docker and orchestration health monitoring.

| Method | Endpoint | Auth | Description | Checks Executed |
|---|---|---|---|---|
| `GET` | `/health/live` | Public | Liveness probe | Returns `200 OK` if the ASP.NET Core Kestrel process is responding |
| `GET` | `/health/ready` | Public | Readiness probe | Verifies active connection to PostgreSQL (`NpgsqlDataSource.OpenConnectionAsync()`) and Redis (`IConnectionMultiplexer.IsConnected`). Returns `200 OK` or `503 Service Unavailable` |

---

## 6. Real-Time SignalR Hubs (`/hubs`)

### 6.1. `AuctionHub` (`/hubs/auction`)
- **Connection & Groups:** Clients join room `Auction_{auctionId}` when viewing a live auction.
- **Methods Invoked by Client:**
  - `JoinAuction(int auctionId)`
  - `LeaveAuction(int auctionId)`
- **Events Broadcasted to Clients (via Redis Backplane):**
  - `ReceiveNewBid(BidDto bid)`: Triggered immediately when a bid is accepted (contains `auctionId`, `amount`, `bidderId`, `bidderName`, `timestamp`).
  - `AuctionEnded(AuctionResultDto result)`: Triggered when auction reaches `end_time` or is completed (contains `auctionId`, `winnerUserId`, `winnerName`, `winningPrice`, `status`).
  - `AuctionTimeExtended(AuctionExtendedDto extension)`: Triggered if bids placed in the final 2 minutes extend the auction duration (contains `auctionId`, `newEndTime`, `extendedSeconds`).

### 6.2. `ChatHub` (`/hubs/chat`)
- **Connection & Groups:** Authenticated via JWT token (via query string or header). Users automatically join their user room `User_{userId}` and group rooms `Group_{groupId}`.
- **Methods Invoked by Client:**
  - `SendDirectMessage(int conversationId, int recipientId, string content)`
  - `SendGroupMessage(int groupId, string content)`
  - `Typing(int targetId, bool isGroup)`
- **Events Broadcasted to Clients (via Redis Backplane):**
  - `ReceiveDirectMessage(DmMessageDto message)`
  - `ReceiveGroupMessage(GroupMessageDto message)`
  - `UserTyping(int senderId, string senderName, int targetId, bool isGroup)`
  - `MessageReadReceipt(int conversationId, int messageId)`

---

## 7. Execution & Business Logic Workflows

### 7.1. Authentication & Security Flow
1. **Password Storage:** When a user registers, a 64-byte cryptographic random `salt` is generated and saved in `users.salt`. The password is combined with the salt and hashed using HMACSHA512 (or PBKDF2), saved in `users.password_hash`.
2. **JWT & Refresh Tokens (Token Rotation):**
   - On login, a JWT Access Token (short lifespan: 15-60 mins) containing `userId`, `email`, and `role` claims is generated.
   - A cryptographically secure random 64-byte base64 refresh token string is saved in `users.refresh_token` along with `users.refresh_token_expiry` (e.g., 7 days).
   - Active session identifier is cached in Redis with a TTL matching token expiry.
3. **Token Refresh Rotation & Grace Period:**
   - When `/api/auth/refresh-token` is executed, the previous refresh token is validated.
   - A new access/refresh token pair is issued and persisted in the database.
   - To accommodate parallel in-flight HTTP requests from the client (e.g. initial dashboard load triggering multiple simultaneous 401s), the old refresh token is placed in a **Redis Grace Cache** with a short 10-second TTL, returning the newly issued tokens if presented within this window rather than throwing an immediate security violation.

### 7.2. Live Auction Bidding Flow (Pessimistic Row Lock & Anti-Sniping)
1. User submits bid via `POST /api/auctions/{id}/bid` or `AuctionHub`, optionally including `X-Idempotency-Key` in request headers.
2. **Idempotency Verification:** If `X-Idempotency-Key` is present, check Redis key `idempotency:bid:{key}`. If key exists, return the cached previous response immediately without re-processing.
3. Backend opens an `NpgsqlConnection` and begins an `NpgsqlTransaction`:
   - Executes:
     ```sql
     SELECT id, listing_id, current_price, start_time, end_time, min_bid_increment, status
     FROM auctions
     WHERE id = @id
     FOR UPDATE;
     ```
   - Validates that `status = 'active'` and `NOW() BETWEEN start_time AND end_time`.
   - Validates that `amount >= current_price + min_bid_increment`.
   - **Anti-Sniping Logic (Ensures at least 120 seconds remain):**
     - If `(end_time - NOW()) <= INTERVAL '120 seconds'`:
       - `new_end_time = NOW() + INTERVAL '120 seconds'`.
       - Flag `is_extended = true`.
       - Calculates `extended_seconds = EXTRACT(EPOCH FROM (new_end_time - end_time))::int`.
     - Else:
       - `new_end_time = end_time`.
       - Flag `is_extended = false`.
   - Executes:
     ```sql
     INSERT INTO bids (listing_id, user_id, amount, idempotency_key, created_at)
     VALUES (@listingId, @userId, @amount, @idempotencyKey, NOW());
     ```
   - Executes:
     ```sql
     UPDATE auctions
     SET current_price = @amount,
         winner_user_id = @userId,
         end_time = @newEndTime
     WHERE id = @id;
     ```
   - Commits transaction.
4. Backend updates current live auction price, leader ID, and `end_time` in **Redis** cache (`auction:{id}:state`).
5. If `X-Idempotency-Key` was provided, cache the result in Redis with a 24-hour expiration.
6. SignalR `AuctionHub` (using Redis backplane) broadcasts:
   - `ReceiveNewBid` with updated price and bidder info to all clients in `Auction_{id}`.
   - If `is_extended = true`, broadcasts `AuctionTimeExtended` with `@newEndTime` and `extendedSeconds`.

### 7.3. Background Auction Expiry Worker (ADO.NET Periodic Worker)
- A hosted background service (`AuctionExpiryWorker`) runs periodically (e.g., every 3-5 seconds).
- Executes:
  ```sql
  SELECT a.id, a.listing_id, a.current_price, a.winner_user_id
  FROM auctions a
  INNER JOIN listings l ON a.listing_id = l.id
  WHERE a.end_time <= NOW() AND a.status = 'active';
  ```
- For each expired auction inside an `NpgsqlTransaction`:
  - If `winner_user_id IS NOT NULL`:
    - `UPDATE auctions SET status = 'completed' WHERE id = @id;`
    - `UPDATE listings SET status = 'sold', updated_at = NOW() WHERE id = @listingId;`
  - Else (no bids placed):
    - `UPDATE auctions SET status = 'expired' WHERE id = @id;`
    - `UPDATE listings SET status = 'expired', updated_at = NOW() WHERE id = @listingId;`
  - Invalidates/updates Redis cache (`auction:{id}:state`).
  - Broadcasts `AuctionEnded` via `AuctionHub` notifying participants, seller, and winner.

### 7.4. Rate Limiting & Anti-Abuse Policies
To prevent bid flooding, spamming, and credential stuffing, rate limiting is configured via ASP.NET Core RateLimiting middleware / Redis Token Bucket:
- **Bidding (`/api/auctions/{id}/bid`):** Max 2 bid submissions per second per authenticated user.
- **Direct & Group Messaging (`/api/dm`, `/api/groups`):** Max 5 messages per second per user.
- **Authentication (`/api/auth/login`, `/api/auth/register`):** Max 5 attempts per minute per IP address.
- **Public Listing Queries (`/api/listings`, `/api/auctions`):** Max 60 requests per minute per IP.

---

## 8. Unit Testing Strategy & API Test Suite (`CarAuction.UnitTest`)

### 8.1. Frameworks & Approved Libraries
The unit test suite is strictly constrained to the following pre-installed packages:
- **Test Framework:** `xUnit` (v2.5.3) & `Microsoft.NET.Test.Sdk` (v17.8.0)
- **Mocking / Isolation:** `FakeItEasy` (v8.3.0) — used exclusively to fake dependencies (application services, managers, loggers) for API controllers (`A.Fake<T>()`, `A.CallTo(...)`).
- **Assertions:** `FluentAssertions` (v8.10.0) — used for expressive, fluent HTTP result and DTO assertions (`result.Should().BeOfType<...>()`).
- **Code Coverage:** `coverlet.collector` (v6.0.0).

> [!CAUTION]
> **STRICT TEST SCOPE POLICY:**
> - **Only API Endpoints:** Unit tests are strictly limited to verifying whether API Controllers and endpoints execute and return expected HTTP status codes (`200 OK`, `201 Created`, `400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found`) and response models correctly.
> - **No Other Tests:** No database integration tests, raw SQL execution tests, domain-only tests, or background worker tests are permitted in this test project.
> - **No Extra Libraries:** No additional mock or test packages (such as Moq, NSubstitute, AutoFixture) shall be introduced.

### 8.2. Controller Test Mapping
All unit test classes reside in `CarAuction.UnitTest/Controllers/` and map 1-to-1 with presentation controllers:
- `AuthControllerTests`: Validates `/api/auth` endpoints (`register`, `login`, `refresh-token`, `logout`, `me`).
- `UsersControllerTests`: Validates `/api/users` endpoints (`list`, `get by id`, `profile update`, `status toggle`, `role update`).
- `RolesPermissionsControllerTests`: Validates `/api/roles` and `/api/permissions` CRUD and role-permission mappings.
- `ListingsControllerTests`: Validates `/api/listings` endpoints (`search`, `get`, `create`, `update`, `delete`, `my`).
- `AuctionsControllerTests`: Validates `/api/auctions` endpoints (`list active`, `get by id`, `place bid`, `bid history`).
- `BidsControllerTests`: Validates `/api/bids` endpoints (`regular offers`, `listing offers`, `user bid history`).
- `ImagesControllerTests`: Validates `/api/images` endpoints (`upload`, `stream/download`, `delete`).
- `GroupsControllerTests`: Validates `/api/groups` endpoints (`list`, `create`, `join/leave`, `messages`).
- `DirectMessagesControllerTests`: Validates `/api/dm` endpoints (`conversations`, `start`, `message history`, `send`, `mark read`).
- `HealthControllerTests`: Validates `/health` container probes (`live`, `ready`).

In addition to controller tests, `CarAuction.UnitTest/Infrastructure/` houses unit tests verifying fallback and prefix clearing mechanics (`RedisServicesTests`).

---

*This document serves as the comprehensive backend architecture blueprint for the CarAuction project.*
