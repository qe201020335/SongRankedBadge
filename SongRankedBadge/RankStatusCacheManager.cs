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

        private readonly Dictionary<string, RankStatus> _cache = new Dictionary<string, RankStatus>();

        private readonly IRankingProvider _beatLeader = new BeatLeaderRanking();
        
        private CancellationTokenSource? _tokenSource = null;
        
        // This will be called everytime SongCore finishes refreshing
        internal void Init(ICollection<CustomPreviewBeatmapLevel> levels)
        {
            Plugin.Log.Info("Start refreshing song ranked status");
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            var cancel =  new CancellationTokenSource();
            _tokenSource = cancel;
            Task.Run( async () =>
            {
                var blRankings = await _beatLeader.GetRankedStatus(levels, cancel.Token);

                if (cancel.IsCancellationRequested) return;

                foreach (var level in levels)
                {
                    var hash = SongCore.Utilities.Hashing.GetCustomLevelHash(level);
                    var ssRank = Plugin.SongDetails.songs.FindByHash(hash, out var song) && song.rankedStatus == RankedStatus.Ranked;
                    var blRank = blRankings.TryGetValue(hash, out var ranked) && ranked;
                    if (ssRank && blRank)
                    {
                        _cache[hash] = RankStatus.Both;
                    }
                    else if (blRank)
                    {
                        _cache[hash] = RankStatus.BeatLeader;
                    }
                    else if (ssRank)
                    {
                        _cache[hash] = RankStatus.ScoreSaber;
                    }
                    else
                    {
                        _cache[hash] = RankStatus.None;
                    }
                    
                    if (_tokenSource.IsCancellationRequested) break;
                }
                Plugin.Log.Info("Finished refreshing ranked status or cancelled");
            }, _tokenSource.Token);
            Plugin.Log.Debug("Start Refreshing method returned");
        }

        internal RankStatus GetSongRankedStatus(string hash)
        {
            return _cache.TryGetValue(hash, out var status) ? status : RankStatus.None;
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