using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SongRankedBadge.RankingProviders
{
    public interface IRankingProvider
    {
        Task<IDictionary<string, bool>> GetRankedStatus(ICollection<CustomPreviewBeatmapLevel> levels, CancellationToken cancellationToken);
    }
}