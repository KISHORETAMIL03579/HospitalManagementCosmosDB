# Hospital Management API 🏥

An enterprise-grade **ASP.NET Core (.NET 10)** Web API solution implementing **Clean Architecture**, **Azure Cosmos DB**, **Google OAuth 2.0**, and **JWT Authentication with Refresh Token Rotation**.

---

## 🌟 Key Architectural Features

- **Clean Architecture**: Strict layer separation (`API` -> `Infrastructure` -> `Application` -> `Domain`).
- **Google OAuth 2.0 Integration**: Single unified token exchange endpoint (`/api/auth/google`).
- **JWT & Refresh Tokens**: Standard HMAC SHA-256 JWT generation with hashed SHA-256 refresh tokens stored in Cosmos DB and constant-time token validation (`CryptographicOperations.FixedTimeEquals`).
- **Azure Cosmos DB Integration**: Automated database and container initialization, custom `CosmosContainerFactory`, resilience retry policies, and partition key strategy (`/id`).
- **Production-Grade Security**: Fail-fast 256-bit JWT key validation, environment-aware HTTPS metadata checks, non-root Docker container configuration.
- **Docker & CI/CD**: Multi-stage production `Dockerfile` with non-root user execution, `.dockerignore`, and GitHub Actions CI build check pipeline.

---

## 📁 Project Structure

```text
hospital-management-api/
├── .github/
│   └── workflows/
│       └── deploy.yml             # GitHub Actions CI workflow
├── HospitalManagement.API/        # Presentation Layer (Controllers, Program.cs, Middleware)
├── HospitalManagement.Application/  # Use Cases, Interfaces, DTOs, Services
├── HospitalManagement.Domain/       # Domain Entities (User, Patient, Enums, Factories)
├── HospitalManagement.Infrastructure/ # Cosmos DB, Repositories, JWT & OAuth Services
├── .dockerignore                  # Docker build exclusion rules
├── .env.example                   # Local environment variable template
├── Dockerfile                     # Multi-stage production Docker build
├── HospitalManagement.slnx        # Solution File (.NET 10 format)
└── README.md                      # Project Documentation
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (optional)
- [Azure Cosmos DB Emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/emulator) (or Azure Cosmos DB account)

### Local Configuration Setup

1. Copy `.env.example` to create `.env` in the repository root:

```powershell
Copy-Item .env.example .env
```

2. Fill in your configuration values in `.env`:

```env
# Google OAuth Configuration
Google__ClientId=YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com
Google__ClientSecret=YOUR_GOOGLE_CLIENT_SECRET

# JWT Security (Must be at least 32 characters / 256 bits)
Jwt__Key=ThisIsMySuperSecretJwtKey1234567890Secure
Jwt__Issuer=HospitalAPI
Jwt__Audience=HospitalUsers
Jwt__ExpiryMinutes=30
Jwt__RefreshTokenDays=7

# Cosmos DB Connection
CosmosDb__AccountEndpoint=https://127.0.0.1:8081
CosmosDb__AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==
CosmosDb__DatabaseId=HospitalDb
```

---

## 💻 Running the Application

### Option 1: Run via .NET CLI

```powershell
# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build

# Run the API project
dotnet run --project HospitalManagement.API
```

Access Swagger UI at: `http://localhost:5000/swagger` or `https://localhost:5001/swagger`.

### Option 2: Run via Docker

Build the Docker image locally:

```powershell
docker build -t hospitalmanagement:latest .
```

Run the container listening on port `8080`:

```powershell
docker run -p 8080:8080 hospitalmanagement:latest
```

---

## 🔐 API Endpoints

### Authentication & Authorization (`/api/auth`)

| Method | Endpoint | Description | Auth Required |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/google` | Exchange Google OAuth ID Token for JWT Access & Refresh Tokens | ❌ No |
| `POST` | `/api/auth/refresh-token` | Exchange active Refresh Token for new Access & Refresh Tokens | ❌ No |
| `POST` | `/api/auth/logout` | Revoke active Refresh Token | ❌ No |

### Patients Management (`/api/patient`)

| Method | Endpoint | Description | Auth Required |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/patient` | Get all patients | 🔒 Bearer JWT |
| `GET` | `/api/patient/{id}` | Get patient by ID | 🔒 Bearer JWT |
| `POST` | `/api/patient` | Create new patient record | 🔒 Bearer JWT |
| `PUT` | `/api/patient/{id}` | Update existing patient record | 🔒 Bearer JWT |
| `DELETE` | `/api/patient/{id}` | Delete patient record | 🔒 Bearer JWT |

---

## 🛠 Tech Stack & Dependencies

- **Framework**: .NET 10.0 (ASP.NET Core Web API)
- **Database**: Azure Cosmos DB NoSQL SDK (`Microsoft.Azure.Cosmos` v3.55+)
- **Security & Authentication**:
  - `Microsoft.AspNetCore.Authentication.JwtBearer`
  - `Google.Apis.Auth` (Google ID Token Validation)
- **Tooling**:
  - `DotNetEnv` (Local `.env` loader)
  - `Swashbuckle.AspNetCore` (OpenAPI / Swagger UI)
  - `CSharpier` (Code Formatting)

---

## 📜 License

Distributed under the MIT License.
