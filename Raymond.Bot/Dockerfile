FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Raymond.Bot/Raymond.Bot.csproj", "Raymond.Bot/"]
COPY ["Raymond.Data/Raymond.Data.csproj", "Raymond.Data/"]
RUN --mount=type=secret,id=nuget_config,target=/root/.nuget/NuGet/NuGet.Config \
    dotnet restore "Raymond.Bot/Raymond.Bot.csproj"
COPY . .
WORKDIR "/src/Raymond.Bot"
RUN dotnet build "./Raymond.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Raymond.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Raymond.Bot.dll"]
