using Raymond.Shared.Models;

namespace Raymond.Shared.Domain;

public class FlipRules
{
    public FlipResult Flip(Random random) => random.Next(2) == 1 ? FlipResult.Heads : FlipResult.Tails;
}
