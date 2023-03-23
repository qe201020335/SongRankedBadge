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

        private CancellationTokenSource? _tokenSource;

        // This will be called everytime SongCore finishes refreshing
        // internal void Init(ICollection<CustomPreviewBeatmapLevel> levels)
        // {
        //     Plugin.Log.Info("Start refreshing song ranked status cache");
        //     _tokenSource?.Cancel();
        //     _tokenSource?.Dispose();
        //     var cancel = new CancellationTokenSource();
        //     _tokenSource = cancel;
        //     Task.Run(async () =>
        //     {
        //         var missing = levels.Where(level => !_blCache.ContainsKey(SongCore.Utilities.Hashing.GetCustomLevelHash(level))).ToList();
        //
        //         if (missing.Count == 0)
        //         {
        //             Plugin.Log.Info("Nothing to update");
        //             return;
        //         }
        //         
        //         if (cancel.IsCancellationRequested)
        //         {
        //             Plugin.Log.Info("Refresh rank status cache cancelled");
        //         }
        //
        //         var blRankings = await _beatLeader.GetRankedStatus(missing, cancel.Token);
        //
        //         if (cancel.IsCancellationRequested)
        //         {
        //             Plugin.Log.Info("Refresh rank status cache cancelled");
        //         }
        //
        //         foreach (var (hash, ranked) in blRankings)
        //         {
        //             _blCache[hash] = ranked;
        //             if (cancel.IsCancellationRequested) break;
        //         }
        //
        //         Plugin.Log.Info(cancel.IsCancellationRequested ? "Refresh rank status cancelled" : "Finished refreshing rank status");
        //     }, cancel.Token);
        // }

        internal void Init()
        {
            Task.Factory.StartNew(async () => { _blRanked = await _beatLeader.GetRankedStatus(CancellationToken.None); });
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