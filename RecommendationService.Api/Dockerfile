FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RecommendationService.Api/RecommendationService.Api.csproj", "RecommendationService.Api/"]
COPY ["RecommendationService.Application/RecommendationService.Application.csproj", "RecommendationService.Application/"]
COPY ["RecommendationService.Infrastructure/RecommendationService.Infrastructure.csproj", "RecommendationService.Infrastructure/"]
COPY ["RecommendationService.Domain/RecommendationService.Domain.csproj", "RecommendationService.Domain/"]
RUN dotnet restore "RecommendationService.Api/RecommendationService.Api.csproj"
COPY . .
RUN dotnet build "RecommendationService.Api/RecommendationService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RecommendationService.Api/RecommendationService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Копирование сертификата
COPY --from=build /src/RecommendationService.Infrastructure/AI/Certificates/russian_trusted_root_ca.cer ./AI/Certificates/

ENTRYPOINT ["dotnet", "RecommendationService.Api.dll"]
