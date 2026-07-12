FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/Core/Core.Domain/Core.Domain.csproj src/Core/Core.Domain/
COPY src/Core/Core.Contracts/Core.Contracts.csproj src/Core/Core.Contracts/
COPY src/API/API.Gateway/API.Gateway.csproj src/API/API.Gateway/
RUN dotnet restore src/API/API.Gateway/API.Gateway.csproj

COPY src/Core/Core.Domain/ src/Core/Core.Domain/
COPY src/Core/Core.Contracts/ src/Core/Core.Contracts/
COPY src/API/API.Gateway/ src/API/API.Gateway/

RUN dotnet publish src/API/API.Gateway/API.Gateway.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "API.Gateway.dll"]
