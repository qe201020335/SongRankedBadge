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

        private readonly Dictionary<string, bool> _blCache = new Dictionary<string, bool>();

        private readonly IRankingProvider _beatLeader = new BeatLeaderRanking();

        private CancellationTokenSource? _tokenSource;

        // This will be called everytime SongCore finishes refreshing
        internal void Init(ICollection<CustomPreviewBeatmapLevel> levels)
        {
            Plugin.Log.Info("Start refreshing song ranked status cache");
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            var cancel = new CancellationTokenSource();
            _tokenSource = cancel;
            Task.Run(async () =>
            {
                var missing = levels.Where(level => !_blCache.ContainsKey(SongCore.Utilities.Hashing.GetCustomLevelHash(level))).ToList();

                if (missing.Count == 0)
                {
                    Plugin.Log.Info("Nothing to update");
                    return;
                }
                
                if (cancel.IsCancellationRequested)
                {
                    Plugin.Log.Info("Refresh rank status cache cancelled");
                }

                var blRankings = await _beatLeader.GetRankedStatus(missing, cancel.Token);

                if (cancel.IsCancellationRequested)
                {
                    Plugin.Log.Info("Refresh rank status cache cancelled");
                }

                foreach (var (hash, ranked) in blRankings)
                {
                    _blCache[hash] = ranked;
                    if (cancel.IsCancellationRequested) break;
                }

                Plugin.Log.Info(cancel.IsCancellationRequested ? "Refresh rank status cancelled" : "Finished refreshing rank status");
            }, cancel.Token);
        }

        internal RankStatus GetSongRankedStatus(string hash)
        {
            var ssRank = Plugin.SongDetails.songs.FindByHash(hash, out var song) && song.rankedStatus == RankedStatus.Ranked;
            var blRank = _blCache.TryGetValue(hash, out var ranked) && ranked;
            if (ssRank && blRank)
            {
                return RankStatus.Both;
            }

            if (blRank)
            {
                return RankStatus.BeatLeader;
            }

            if (ssRank)
            {
                return RankStatus.ScoreSaber;
            }

            {
                return RankStatus.None;
            }
        }
    }

    internal enum RankStatus
    {
        None,
        ScoreSaber,
        BeatLeader,
        Both
    }
}