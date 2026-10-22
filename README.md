
# 🛡️ JWTAuthentication

This project demonstrates how to implement **JWT (JSON Web Token) based authentication** using ASP.NET Core and Entity Framework Core.

---

## 📌 Features

- 🔐 User registration and login with JWT access tokens  
- 🔄 Token refresh mechanism (access + refresh tokens)  
- 🧑‍💼 Role-based authorization  
- 💾 PostgreSQL with Entity Framework Core  
- 🛠️ Docker support using `Dockerfile`  
- 🗃️ Database migrations via EF Core  
- 🌍 CQRS enabled for frontend communication

---

## 🚀 Technologies Used

| Technology        | Description                            |
|-------------------|-----------------------------------------|
| ASP.NET Core      | .NET 8 / Web API                        |
| EF Core           | PostgreSQL database ORM                 |
| PostgreSQL        | Main relational database                |
| JWT               | Access and Refresh Token based auth     |
| Serilog           | Structured logging to file              |
| Dockerfile        | For containerizing the API              |
| Swagger           | API documentation and testing           |

---

## 🛠️ Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- [Docker](https://www.docker.com/)
- [PostgreSQL](https://www.postgresql.org/) (running locally or on server)
---

## ⚙️ Run Locally

1. **Clone the repository**

```bash
git clone https://github.com/Martik2002/JWTAuthentication.git
cd JWTAuthentication
```

2. **Configure settings**  
`appsettings.Development.json` contains local-only defaults (PostgreSQL `postgres/postgres` on `localhost:5432` and a development signing key), so the project runs out of the box.  
To use your own values without committing them, use [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets):

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=JWTAuthentication;Username=postgres;Password=<your-password>"
dotnet user-secrets set "JwtSettings:Key" "<random string, at least 64 characters>"
```

3. **Apply Migrations**

```bash
dotnet ef database update
```

4. **Run the API**

```bash
dotnet run
```

> Default port: `http://localhost:5000`

---

## 🐳 Docker Build & Run

1. **Build the Docker image**

```bash
docker build -t jwt-auth-api .
```

2. **Run the Docker container**

```bash
docker run -d -p 5000:8080 --name jwt-auth-api \
  -e ConnectionStrings__DefaultConnection="Host=<db-host>;Port=5432;Database=JWTAuthentication;Username=postgres;Password=<password>" \
  -e JwtSettings__Key="<random string, at least 64 characters>" \
  jwt-auth-api
```

> In production, secrets are never stored in `appsettings*.json` — they are provided through environment variables. The app fails fast at startup if `JwtSettings:Key` is missing.

---

## 📊 Swagger Endpoints

Once running, go to:
```
http://localhost:5000/swagger/index.html
```
You can test:

- `POST /api/Auth/register`
- `POST /api/Auth/login`
- `POST /api/Auth/refresh`
---

## 🔐 Authentication Flow

1. **Register** → `/api/Auth/register`  
2. **Login** → `/api/Auth/login`  
   → Returns `accessToken` + `refreshToken`  
3. **Use token in protected requests**  
   `Authorization: Bearer {token}`  
4. **Refresh token** → `/api/Auth/refresh`

---

## 🪵 Logging

**Serilog** is used for logging with file output:

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

Logs are saved in `Logs/log.txt`.

---

## 🧪 Testing With Postman

1. Register a new user  
2. Login and copy `accessToken`  
3. Use in request header:

```
Authorization: Bearer {your_access_token}
```

4. Test protected endpoint

---

## 📤 Project Structure

```
├── Application/        # CQRS commands & handlers (Login, Register, Refresh, SignOut, ActiveUser)
│   └── Abstractions/   # Custom mediator, request/handler interfaces, current user service
├── Common/             # JWT settings, token & password services, exception middleware
├── Database/           # ApplicationDbContext and seed data
├── Endpoints/          # Minimal API endpoint groups
├── Entities/           # Domain entities
├── Infrastructure/     # Endpoint registration helpers
├── Interfaces/         # Service abstractions
├── Migrations/         # EF Core migrations
├── DependencyInjection.cs
├── Program.cs
└── Dockerfile
```

---

## ✅ TODO / Future Plans

- [ ] Add Unit Tests
- [ ] Implement Email Confirmation
- [ ] Password Reset functionality
- [ ] Enable HTTPS support in Docker

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).

---

## ✍️ Author

- Martik Avagyan – [GitHub Profile](https://github.com/Martik2002)
