# Raymond

Raymond is a Discord bot for server administration and community features. It stores guild configuration, member data, and wheel-picker data in SQL Server, and can manage Minecraft server containers through Docker.

## Features

- Birthday management and birthday reminders.
- Guild scheduled-event reminders.
- Server configuration for notification channels, messages, time zones, birthday notification hours, and feature toggles.
- Member names, levels, leaderboards, and utility commands.
- Custom wheel pickers.
- Minecraft server status, start/stop, and command operations through Docker.

The bot registers slash commands, including `/birthday`, `/config`, `/mc`, and `/wheel`, as well as `/level`, `/leaderboard`, `/who`, `/whois`, `/iam`, and `/flip`.

## Solution layout

| Project | Purpose |
| --- | --- |
| `Raymond.AppHost` | .NET Aspire AppHost for local orchestration and the Docker Compose environment. |
| `Raymond.Bot` | Discord bot, commands, event handlers, and background scheduled tasks. |
| `Raymond.Data` | SQL-backed data repositories. |
| `Raymond.Database` | SQL database project used to build and publish the database schema. |
| `Raymond.Shared` | Shared utilities and services, including Docker container operations. |

## Requirements

- .NET 10 SDK.
- A Discord application and bot token, with the intents and permissions needed by the bot. The bot requests `DiscordIntents.All`; enable the required privileged intents in the Discord Developer Portal.
- A reachable SQL Server instance with the Raymond database schema deployed.
- Docker Engine access for Minecraft container management. Docker Compose support is also used by the AppHost's Compose environment.

## Database setup

The AppHost expects an externally available SQL Server connection named `raymond-sql`; it does not provision SQL Server. Build and publish the schema from the `Raymond.Database` project. See [`Raymond.Database/README.md`](Raymond.Database/README.md) for the DACPAC build and publish instructions.

## Configure secrets

Store credentials in the AppHost project's .NET User Secrets rather than committing them to configuration files. From the repository root, set the AppHost parameters and connection string:

```bash
dotnet user-secrets set --project Raymond.AppHost/Raymond.AppHost.csproj "Parameters:RaymondToken" "<DISCORD_BOT_TOKEN>"
dotnet user-secrets set --project Raymond.AppHost/Raymond.AppHost.csproj "Parameters:DiscordAdminId" "<DISCORD_USER_ID>"
dotnet user-secrets set --project Raymond.AppHost/Raymond.AppHost.csproj "ConnectionStrings:RaymondSql" "Server=localhost;Database=Raymond.Database;User Id=<SQL_USER>;Password=<SQL_PASSWORD>;TrustServerCertificate=True"
```

## Run locally

After deploying the database and configuring the secrets, start the AppHost from the repository root:

```bash
dotnet run --project Raymond.AppHost/Raymond.AppHost.csproj
```

The AppHost launches the bot and exposes the Aspire dashboard at [http://localhost:18888](http://localhost:18888). The bot must be able to reach Discord and the configured SQL Server. Docker daemon access is needed for Minecraft container operations.

To build the solution without starting it:

```bash
dotnet build Raymond.sln
```

## Scheduled notifications

The AppHost defaults are defined in `Raymond.AppHost/appsettings.json`:

| Parameter | Default | Purpose |
| --- | --- | --- |
| `SchedulingTimeZone` | `America/New_York` | Time zone used to interpret the scheduler's cron expressions. |
| `SchedulingBirthdays` | `0 * * * *` | Check birthday notifications at the start of each hour. |
| `SchedulingGuildEvents` | `30 * * * *` | Check guild events at half past each hour. |

Each Discord server also has its own configured time zone and birthday notification hour. Birthday reminders are sent on the birthday and one, seven, and fourteen days beforehand; guild-event reminders are sent one, seven, and fourteen days before an event. To customize the scheduler defaults locally, override the matching `Parameters:scheduling-timezone`, `Parameters:scheduling-birthdays`, or `Parameters:scheduling-guild-events` keys in AppHost configuration or User Secrets.
