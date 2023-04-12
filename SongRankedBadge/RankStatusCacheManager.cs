using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SongDetailsCache.Structs;
using SongRankedBadge.RankingProviders;

namespace SongRankedBadge
{
    internal class RankStatusCacheManager
    {
        internal static readonly RankStatusCacheManager Instance = new RankStatusCacheManager();

        // private readonly Dictionary<string, bool> _blCache = new Dictionary<string, bool>();

        private HashSet<string> _blRanked = new HashSet<string>();

        private readonly IRankingProvider _beatLeader = new BeatLeaderRanking();

        internal void Init()
        {
            Task.Factory.StartNew(async () =>
            {
                _blRanked = await _beatLeader.GetRankedStatus(CancellationToken.None); 
                Plugin.Log.Info($"Loaded {_blRanked.Count} ranked songs from BeatLeader");
            });
        }

        internal RankStatus GetSongRankedStatus(string hash)
        {
            hash = hash.ToLower();
            var ssRank = Plugin.SongDetails.songs.FindByHash(hash, out var song) && song.rankedStatus == RankedStatus.Ranked;
            var haveBlData = _blRanked.Count != 0;
            var blRank = haveBlData && (_blRanked.Contains(hash) || _blRanked.Contains(hash.ToUpper()));
            if (ssRank && blRank)
            {
                return RankStatus.Ranked;
            }

            if (blRank)
            {
                return RankStatus.BeatLeader;
            }

            if (ssRank)
            {
                return haveBlData ? RankStatus.ScoreSaber : RankStatus.Ranked;
            }

            return RankStatus.None;
        }
    }

    internal enum RankStatus
    {
        None,
        ScoreSaber,
        BeatLeader,
        Ranked // just ranked, means both or we have incomplete data (i.e bl data is not ready)
    }
}