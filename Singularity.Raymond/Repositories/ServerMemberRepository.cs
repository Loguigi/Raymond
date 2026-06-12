using Singularity.Raymond.Models;
using Singularity.Shared.Data;

namespace Singularity.Raymond.Repositories;

public class ServerMemberRepository(IDbConnectionFactory factory) : BaseRepository(factory)
{
    private const int ExpGain = 1;
}