# Stage 1 — build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Tenisu.sln ./
COPY src/Tenisu.Domain/Tenisu.Domain.csproj           src/Tenisu.Domain/
COPY src/Tenisu.Application/Tenisu.Application.csproj src/Tenisu.Application/
COPY src/Tenisu.Infrastructure/Tenisu.Infrastructure.csproj src/Tenisu.Infrastructure/
COPY src/Tenisu.API/Tenisu.API.csproj                 src/Tenisu.API/
COPY tests/Tenisu.Tests/Tenisu.Tests.csproj           tests/Tenisu.Tests/

RUN dotnet restore Tenisu.sln

COPY . .

RUN dotnet publish src/Tenisu.API/Tenisu.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2 — runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Tenisu.API.dll"]
