FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Auth/Auth.Presentation/Auth.Presentation.csproj", "Auth/Auth.Presentation/"]
COPY ["Common/CommonMiddleware/CommonMiddleware.csproj", "Common/CommonMiddleware/"]
COPY ["Common/CommonServiceCollectionExtensions/CommonServiceCollectionExtensions.csproj", "Common/CommonServiceCollectionExtensions/"]
COPY ["Auth/Auth.Application/Auth.Application.csproj", "Auth/Auth.Application/"]
COPY ["Auth/Auth.Domain/Auth.Domain.csproj", "Auth/Auth.Domain/"]
COPY ["Common/UserRooles/UserRooles.csproj", "Common/UserRooles/"]
COPY ["Auth/Auth.Infrastructure/Auth.Infrastructure.csproj", "Auth/Auth.Infrastructure/"]
COPY ["Common/DateTimeProvider/DateTimeProvider.Core/DateTimeManager.Core.csproj", "Common/DateTimeProvider/DateTimeProvider.Core/"]
COPY ["Common/DateTimeProvider/DateTimeProvider.Abstractions/DateTimeManager.Abstractions.csproj", "Common/DateTimeProvider/DateTimeProvider.Abstractions/"]
RUN dotnet restore "./Auth/Auth.Presentation/Auth.Presentation.csproj"
COPY . .
WORKDIR "/src/Auth/Auth.Presentation"
RUN dotnet build "./Auth.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Auth.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auth.Presentation.dll"]