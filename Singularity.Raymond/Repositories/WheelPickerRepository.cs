using Microsoft.Extensions.Logging;
using Singularity.Shared.Data;

namespace Singularity.Raymond.Repositories;

public class WheelPickerRepository(ILogger<WheelPickerRepository> logger, IDbConnectionFactory factory) : BaseRepository(logger, factory)
{
    
}