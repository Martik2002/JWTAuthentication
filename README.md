
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

2. **Update `appsettings.Development.json`**  
Provide correct PostgreSQL connection string.

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
docker run -d -p 5000:80 --name jwt-auth-api jwt-auth-api
```

> Make sure PostgreSQL is accessible from the container.

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
├── Controllers/
├── Services/
├── Models/
├── Middleware/
├── Settings/
├── Program.cs
├── appsettings.json
├── Dockerfile
├── README.md
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
