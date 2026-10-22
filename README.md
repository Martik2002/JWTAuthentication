
# 🛡️ JWTAuthentication

[![Build](https://github.com/Martik2002/JWTAuthentication/actions/workflows/build.yml/badge.svg)](https://github.com/Martik2002/JWTAuthentication/actions/workflows/build.yml)

A production-minded **JWT authentication API** built with ASP.NET Core 8, ASP.NET Core Identity, EF Core and PostgreSQL. Besides login and registration, it covers the parts that are easy to get wrong: refresh token rotation, token revocation, brute-force protection and safe error handling. Everything is covered by integration tests against a real database.

---

## 📌 Features

- 🔐 Registration and login with short-lived **JWT access tokens**
- 🔄 **Refresh tokens with rotation**: every refresh token can be used only once, and concurrent reuse is detected
- 🗄️ Refresh tokens stored only as **SHA-256 hashes**; logout revokes them
- 🧑‍💼 Role-based authorization (`User`, `Admin`); new users always get `User`
- 🚫 **Account lockout** after 5 failed logins and **rate limiting** on auth endpoints
- 🕵️ Same response for unknown user and wrong password (no username enumeration)
- ✅ Request validation with FluentValidation, errors as **ProblemDetails** (RFC 7807)
- 🔑 BCrypt password hashing plugged into ASP.NET Core Identity
- ❤️ Health check endpoint, structured logging with Serilog
- 🧪 Integration tests with Testcontainers, run in CI on every push

---

## 🚀 Technologies Used

| Technology | Usage |
|------------|-------|
| ASP.NET Core 8 | Minimal APIs |
| ASP.NET Core Identity | Users, roles, password policy, lockout |
| EF Core + PostgreSQL | Persistence and migrations |
| JWT Bearer | Access tokens (HMAC-SHA512) |
| FluentValidation | Request validation |
| Serilog | Structured logging (console + file) |
| xUnit + Testcontainers | Integration tests against real PostgreSQL |
| Docker, GitHub Actions | Containerization and CI |

---

## 🔌 API

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| `POST` | `/api/auth/register` | – | Create an account (always with the `User` role) |
| `POST` | `/api/auth/login` | – | Returns an access token and a refresh token |
| `POST` | `/api/auth/refresh` | – | Exchanges a refresh token for a new token pair |
| `POST` | `/api/auth/logout` | Bearer | Revokes the current refresh token |
| `GET` | `/api/auth/me` | Bearer | Current user profile and roles |
| `GET` | `/api/auth/admin` | `Admin` role | Example of role-based authorization |
| `GET` | `/health` | – | Health check (includes the database) |

### Authentication flow

```
1. POST /api/auth/login        → { accessToken, accessTokenExpiresAt, refreshToken, refreshTokenExpiresAt }
2. GET  /api/auth/me           → Authorization: Bearer {accessToken}
3. Access token expired?
   POST /api/auth/refresh      → { refreshToken } → new token pair (old refresh token stops working)
4. POST /api/auth/logout       → refresh token revoked
```

---

## 🧠 Design Decisions

- **Short-lived access tokens + rotating refresh tokens.** JWTs can't be revoked, so access tokens live 15 minutes. Refresh tokens are rotated on every use: a stolen refresh token is useful only until the real client refreshes.
- **Refresh tokens are hashed.** A random 64-byte token is stored as SHA-256. A fast hash is safe here (unlike passwords), because the token has too much entropy to brute-force.
- **Race condition on refresh.** Two parallel requests with the same refresh token could both succeed. Identity's `ConcurrencyStamp` is used as an optimistic concurrency token, so only the first one wins.
- **Expected failures are values, not exceptions.** Handlers return `Result<T>`; wrong passwords or duplicate emails are mapped to 400/401/404/409 ProblemDetails. Exceptions are reserved for real errors and are handled by a global handler that never leaks internals.
- **No default credentials.** The admin user is created only when `Seed:AdminPassword` is configured (it is in `appsettings.Development.json` for local use only).

---

## 🛠️ Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- [Docker](https://www.docker.com/) (for PostgreSQL and the tests)

### Run locally

```bash
git clone https://github.com/Martik2002/JWTAuthentication.git
cd JWTAuthentication

# PostgreSQL for local development
docker run -d --name jwt-postgres -p 5432:5432 -e POSTGRES_PASSWORD=postgres postgres:16-alpine

dotnet run
```

Migrations are applied automatically at startup. Open Swagger at `http://localhost:5000/swagger` and log in with the development admin (`admin` / `Admin123!`).

`appsettings.Development.json` contains **local-only** defaults. To use your own values without committing them, use [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets):

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=JWTAuthentication;Username=postgres;Password=<your-password>"
dotnet user-secrets set "JwtSettings:Key" "<random string, at least 64 characters>"
```

### Run the tests

Docker must be running: the integration tests start their own PostgreSQL container.

```bash
dotnet test
```

---

## 🐳 Docker

```bash
docker build -t jwt-auth-api .

docker run -d -p 5000:8080 --name jwt-auth-api \
  -e ConnectionStrings__DefaultConnection="Host=<db-host>;Port=5432;Database=JWTAuthentication;Username=postgres;Password=<password>" \
  -e JwtSettings__Key="<random string, at least 64 characters>" \
  -e Seed__AdminPassword="<admin password>" \
  jwt-auth-api
```

> In production, secrets are never stored in `appsettings*.json`; they come from environment variables. The app fails fast at startup if `JwtSettings:Key` is missing or too short.

---

## ⚙️ Configuration

| Key | Default | Description |
|-----|---------|-------------|
| `ConnectionStrings:DefaultConnection` | – | PostgreSQL connection string (**secret**) |
| `JwtSettings:Key` | – | Signing key, at least 64 characters (**secret**) |
| `JwtSettings:AccessTokenExpiration` | `15` | Access token lifetime in minutes |
| `JwtSettings:RefreshTokenExpiration` | `7` | Refresh token lifetime in days |
| `RateLimiting:AuthPermitLimit` | `10` | Auth requests per minute per IP |
| `Seed:AdminPassword` | – | If set, creates an admin user at startup |

---

## 📤 Project Structure

```
├── Application/
│   ├── Abstractions/        # Custom mediator, request/handler interfaces, current user
│   └── Auth/                # Use cases: Register, Login, Refresh, Logout, Me
├── Common/
│   ├── Errors/              # Global exception handler
│   ├── Results/             # Result<T> and Error → ProblemDetails mapping
│   ├── Security/            # BCrypt hasher, refresh token hashing
│   ├── Services/            # JWT token service
│   └── Validation/          # FluentValidation endpoint filter
├── Database/                # DbContext, migrations runner and seeding
├── Endpoints/               # Minimal API endpoints
├── Entities/                # User entity
├── Migrations/              # EF Core migrations
├── tests/
│   └── JWTAuthentication.Tests/   # Integration (Testcontainers) and unit tests
├── DependencyInjection.cs
└── Program.cs
```

---

## ✅ Future Plans

- [ ] Email confirmation
- [ ] Password reset
- [ ] Refresh token families (revoke all sessions when a reused token is detected)
- [ ] OpenTelemetry tracing

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).

---

## ✍️ Author

- Martik Avagyan – [GitHub Profile](https://github.com/Martik2002)
