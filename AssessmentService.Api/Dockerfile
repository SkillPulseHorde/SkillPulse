FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["SkillPulse.sln", "."]
COPY ["AssessmentService.Api/AssessmentService.Api.csproj", "AssessmentService.Api/"]
COPY ["AssessmentService.Application/AssessmentService.Application.csproj", "AssessmentService.Application/"]
COPY ["AssessmentService.Domain/AssessmentService.Domain.csproj", "AssessmentService.Domain/"]
COPY ["AssessmentService.Infrastructure/AssessmentService.Infrastructure.csproj", "AssessmentService.Infrastructure/"]
COPY ["Common/Common.csproj", "Common/"]
RUN dotnet restore "AssessmentService.Api/AssessmentService.Api.csproj"

COPY . .
WORKDIR "/src/AssessmentService.Api"
RUN dotnet build "./AssessmentService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AssessmentService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AssessmentService.Api.dll"]
