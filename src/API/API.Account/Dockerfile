FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/Core/Core.Domain/Core.Domain.csproj src/Core/Core.Domain/
COPY src/Core/Core.Contracts/Core.Contracts.csproj src/Core/Core.Contracts/
COPY src/Core/Core.Application/Core.Application.csproj src/Core/Core.Application/
COPY src/Infra/Infra.Persistence/Infra.Persistence.csproj src/Infra/Infra.Persistence/
COPY src/API/API.Account/API.Account.csproj src/API/API.Account/
RUN dotnet restore src/API/API.Account/API.Account.csproj

COPY src/Core/Core.Domain/ src/Core/Core.Domain/
COPY src/Core/Core.Contracts/ src/Core/Core.Contracts/
COPY src/Core/Core.Application/ src/Core/Core.Application/
COPY src/Infra/Infra.Persistence/ src/Infra/Infra.Persistence/
COPY src/API/API.Account/ src/API/API.Account/

RUN dotnet publish src/API/API.Account/API.Account.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "API.Account.dll"]
