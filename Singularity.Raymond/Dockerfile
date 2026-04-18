FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Singularity.Raymond/Singularity.Raymond.csproj", "Singularity.Raymond/"]
RUN dotnet restore "Singularity.Raymond/Singularity.Raymond.csproj"
COPY . .
WORKDIR "/src/Singularity.Raymond"
RUN dotnet build "./Singularity.Raymond.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Singularity.Raymond.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Singularity.Raymond.dll"]
