# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY HospitalManagement.slnx .
COPY HospitalManagement.API/HospitalManagement.API.csproj HospitalManagement.API/
COPY HospitalManagement.Application/HospitalManagement.Application.csproj HospitalManagement.Application/
COPY HospitalManagement.Domain/HospitalManagement.Domain.csproj HospitalManagement.Domain/
COPY HospitalManagement.Infrastructure/HospitalManagement.Infrastructure.csproj HospitalManagement.Infrastructure/

# Restore dependencies
RUN dotnet restore HospitalManagement.API/HospitalManagement.API.csproj

# Copy source
COPY . .

# Publish
RUN dotnet publish HospitalManagement.API/HospitalManagement.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "HospitalManagement.API.dll"]
