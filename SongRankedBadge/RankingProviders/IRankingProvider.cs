using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SongRankedBadge.RankingProviders
{
    public interface IRankingProvider
    {
        Task<HashSet<string>> GetRankedStatus(CancellationToken cancellationToken);
    }
}