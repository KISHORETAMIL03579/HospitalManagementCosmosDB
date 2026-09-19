# -----------------------------------------------------------------------------
# Stage 1: Build .NET 10 Clean Architecture application
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy solution and project files first (improves Docker layer caching)
COPY HospitalManagement.slnx ./

COPY HospitalManagement.API/HospitalManagement.API.csproj HospitalManagement.API/
COPY HospitalManagement.Application/HospitalManagement.Application.csproj HospitalManagement.Application/
COPY HospitalManagement.Domain/HospitalManagement.Domain.csproj HospitalManagement.Domain/
COPY HospitalManagement.Infrastructure/HospitalManagement.Infrastructure.csproj HospitalManagement.Infrastructure/

# Restore NuGet packages
RUN dotnet restore HospitalManagement.API/HospitalManagement.API.csproj

# Copy the remaining source code
COPY . .

# Publish the application
RUN dotnet publish HospitalManagement.API/HospitalManagement.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# -----------------------------------------------------------------------------
# Stage 2: Production ASP.NET Core Runtime
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

# Create a non-root user for production security
RUN useradd --create-home --uid 10001 appuser

# Copy published application from build stage
COPY --from=build /app/publish .

# Create runtime folders and assign ownership
RUN mkdir -p logs uploads traces vectorstore \
    && chown -R appuser:appuser /app

USER appuser

# Application listens on port 8080 inside the container
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Start the ASP.NET Core application
ENTRYPOINT ["dotnet", "HospitalManagement.API.dll"]
