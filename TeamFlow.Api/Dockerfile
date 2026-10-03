FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TeamFlow.Api/TeamFlow.Api.csproj", "TeamFlow.Api/"]
COPY ["TeamFlow.Application/TeamFlow.Application.csproj", "TeamFlow.Application/"]
COPY ["TeamFlow.Domain/TeamFlow.Domain.csproj", "TeamFlow.Domain/"]
COPY ["TeamFlow.Infrastructure/TeamFlow.Infrastructure.csproj", "TeamFlow.Infrastructure/"]
COPY ["TeamFlow.Infrastructure.Persistence/TeamFlow.Infrastructure.Persistence.csproj", "TeamFlow.Infrastructure.Persistence/"]
RUN dotnet restore "./TeamFlow.Api/TeamFlow.Api.csproj"
COPY . .
WORKDIR "/src/TeamFlow.Api"
RUN dotnet build "./TeamFlow.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TeamFlow.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TeamFlow.Api.dll"]