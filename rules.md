# CarAuction — Project Rules, Policies & Approved Libraries

> [!CAUTION]
> **GOVERNANCE POLICY:**
> All code, tests, schemas, and configurations across the CarAuction platform **MUST** strictly adhere to the policies defined in this document. Any deviation requires formal review and documentation updates.

---

## 1. Database & Persistence Rules

1. **Zero-ORM Policy:**
   - No Object-Relational Mappers (ORMs) such as Entity Framework Core, Dapper, or NHibernate are permitted.
   - All database interactions must use pure **ADO.NET** (`NpgsqlConnection`, `NpgsqlCommand`, `NpgsqlDataReader`, `NpgsqlTransaction`).
2. **Strict SQL Parameterization:**
   - Raw string concatenation or interpolated SQL queries are strictly prohibited.
   - Every query parameter must be bound using strongly-typed `NpgsqlParameter` instances to guarantee zero SQL injection vulnerabilities.
3. **Naming & Schema Conventions:**
   - PostgreSQL table names, column names, constraints, and indexes must follow clean `snake_case` in English.
   - Exact column names and types defined in `backend.md` §2 and `init.sql` must be preserved.
4. **Transaction Safety & Locking:**
   - Concurrent updates and bid submissions must execute inside an `NpgsqlTransaction` using pessimistic row locks (`SELECT ... FOR UPDATE`).
   - Connection pooling must be handled via `NpgsqlDataSource` with `using` blocks to prevent connection leaks.

---

## 2. Architecture & Clean Layer Boundaries

The platform adheres to Clean Architecture with 4 strictly decoupled layers:

```
Presentation (API Controllers, SignalR Hubs, Middleware)
       │
       ▼
Application (Business Logic, DTOs, Validators, Contracts)
       │
       ▼
Domain (Entities, Enums, Value Objects, Domain Exceptions)
       ▲
       │
Infrastructure (Pure ADO.NET Data Access, Redis Caching, Security, Background Workers)
```

1. **Domain Layer (`CarAuction.Domain`):**
   - Zero external framework dependencies.
   - POCO entities matching database schema, domain enums, base records, and domain exceptions.
2. **Application Layer (`CarAuction.Application`):**
   - Contains business logic, DTOs, FluentValidation validators, and service/repository interfaces.
   - Completely independent of specific web frameworks and database providers.
3. **Infrastructure Layer (`CarAuction.Infrastructure`):**
   - Implements repositories using raw parameterized SQL.
   - Implements distributed caching and distributed locking with `StackExchange.Redis`.
   - Handles password hashing (HMACSHA512 with 64-byte salt) and JWT token generation.
   - Hosts periodic background workers (e.g., `AuctionExpiryWorker`).
4. **Presentation Layer (`CarAuction.Presentation`):**
   - Hosts ASP.NET Core REST API controllers and SignalR WebSocket hubs.
   - Implements cross-cutting concerns (Global Exception Handling, JWT validation, Rate Limiting, CORS, Swagger).

---

## 3. Approved Libraries & Dependencies

### 3.1. Backend Libraries
| Package | Approved Version | Usage |
|---|---|---|
| `Npgsql` | `8.0.3` | Pure ADO.NET PostgreSQL data access |
| `StackExchange.Redis` | `2.7.33` | Redis caching & distributed lock |
| `Microsoft.AspNetCore.SignalR.StackExchangeRedis` | `8.0.1` | Redis SignalR backplane |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | `8.0.1` | JWT bearer authentication |
| `FluentValidation.AspNetCore` | `11.3.0` | Request input validation |
| `Swashbuckle.AspNetCore` | `6.6.2` | OpenAPI / Swagger docs |

### 3.2. Frontend Libraries
| Package | Approved Version | Usage |
|---|---|---|
| `react`, `react-dom` | `^18.2.0` | Core UI framework |
| `typescript` | `^5.2.0` | Type safety |
| `vite` | `^5.0.0` | Build tool & dev server |
| `react-router-dom` | `^6.22.0` | Client-side routing |
| `@tanstack/react-query` | `^5.28.0` | Server state & query caching |
| `zustand` | `^4.5.0` | Client state management |
| `@microsoft/signalr` | `^8.0.0` | Real-time WebSocket hubs |
| `axios` | `^1.6.0` | HTTP client with interceptors |
| `tailwindcss` | `^3.4.0` | Utility CSS styling |
| `lucide-react` | `^0.350.0` | Icons |
| `zod` | `^3.22.0` | Schema validation |

---

## 4. Testing Policy (`CarAuction.UnitTest`)

> [!CAUTION]
> **STRICT TEST SCOPE POLICY:**
> - **API Endpoints & Controllers Only:** Unit tests in `CarAuction.UnitTest` are strictly scoped to validating presentation controllers (HTTP status codes, response wrapping, error flows) and essential infrastructure fallbacks.
> - **Approved Test Stack Only:**
>   - `xUnit` (v2.5.3)
>   - `FakeItEasy` (v8.3.0) — used exclusively for faking dependencies (`A.Fake<T>()`, `A.CallTo(...)`).
>   - `FluentAssertions` (v8.10.0) — used for expressive, fluent assertions (`result.Should().BeOfType<...>()`).
> - **Forbidden in Unit Tests:** No database integration tests, live SQL queries, or third-party mock libraries (Moq, NSubstitute, AutoFixture).

All controllers have 1-to-1 corresponding test suites:
1. `AuthControllerTests`
2. `UsersControllerTests`
3. `RolesPermissionsControllerTests`
4. `ListingsControllerTests`
5. `AuctionsControllerTests`
6. `BidsControllerTests`
7. `ImagesControllerTests`
8. `GroupsControllerTests`
9. `DirectMessagesControllerTests`
10. `HealthControllerTests`

---

## 5. Security & Authentication Policies

1. **Password Hashing:**
   - A unique, cryptographically random 64-byte salt is generated per user (`users.salt`).
   - Passwords are combined with the salt and hashed using HMACSHA512 (`users.password_hash`).
2. **JWT Token Rotation & Grace Cache:**
   - Access tokens have a short lifespan (15–60 minutes).
   - Refresh tokens are 64-byte cryptographically secure strings stored with expiry in `users.refresh_token` and `users.refresh_token_expiry`.
   - On `/api/auth/refresh-token`, a new token pair is issued, and the previous refresh token is cached in Redis with a **10-second grace period** to accommodate parallel in-flight requests.
3. **Session Revocation & Blacklisting:**
   - On `/api/auth/logout`, refresh tokens are cleared from the database, and the user identifier is cached in Redis (`blacklist:user:{id}`) to immediately invalidate active JWTs.

---

## 6. Live Auction & Bidding Policies

1. **Pessimistic Concurrency:**
   - All bids must execute with `SELECT ... FOR UPDATE` row locks inside an ADO.NET transaction.
2. **Anti-Sniping Rule:**
   - If a valid bid is placed within the final 120 seconds of an auction (`end_time - NOW() <= 120s`), `end_time` is automatically extended by 120 seconds.
   - SignalR broadcasts `AuctionTimeExtended` alongside `ReceiveNewBid`.
3. **Idempotency Protection:**
   - Optional `X-Idempotency-Key` header checks Redis (`idempotency:bid:{key}`) and database `bids.idempotency_key` to eliminate duplicate bids from retried HTTP requests.

---

## 7. API & Serialization Conventions

1. **Uniform Response Envelope (`ApiResponse<T>`):**
   ```json
   {
     "success": true,
     "message": "Operation completed successfully",
     "data": { ... }
   }
   ```
2. **CamelCase Naming Convention:**
   - Backend serialization configured globally with `System.Text.Json` using `JsonNamingPolicy.CamelCase`.
   - Frontend TypeScript interfaces use `camelCase` properties matching backend JSON fields.

---

## 8. Git & GitHub Synchronization Policy

> [!IMPORTANT]
> **Continuous Progress Syncing Rule:**
> All work must be regularly committed and pushed to GitHub at distinct development checkpoints:
> 1. **Feature / Endpoint Completion:** When an API controller, service, or feature is completed and tested.
> 2. **Bug Fixes & Defect Remediation:** Whenever bugs, schema mismatches, or test failures are resolved.
> 3. **Milestone / Phase Transitions:** Upon completing major architectural layers (e.g., Backend Complete, Frontend Foundation, Real-Time SignalR).
> 4. **Documentation Updates:** Whenever architecture docs (`Index.md`, `backend.md`, `frontend.md`, `rules.md`) or database schemas are updated.
> 5. **Session Wrap-Up:** Before ending any major work session, the working tree must be verified clean and all passing tests pushed to the remote repository.

