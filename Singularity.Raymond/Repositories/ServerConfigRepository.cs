using Microsoft.Extensions.Logging;
using Singularity.Shared.Data;

namespace Singularity.Raymond.Repositories;

public class ServerConfigRepository(ILogger<ServerConfigRepository> logger, IDbConnectionFactory factory) : BaseRepository(logger, factory)
{
    
}