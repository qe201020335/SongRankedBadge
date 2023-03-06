using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SongRankedBadge.RankingProviders
{
    public class BeatLeaderRanking : IRankingProvider
    {
        private const string RequestURL = @"https://api.beatleader.xyz/map/hash/";

        public Task<IDictionary<string, bool>> GetRankedStatus(ICollection<CustomPreviewBeatmapLevel> levels, CancellationToken cancellationToken)
        {
            Plugin.Log.Debug("Start Get rank status from BeatLeader");
            var result = new ConcurrentDictionary<string, bool>(8, levels.Count);
            var options = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 64
            };

            try
            {
                Parallel.ForEach(levels, options, level =>
                {
                    if (level.levelID.ToLower().EndsWith("wip")) return;
                    var hash = SongCore.Utilities.Hashing.GetCustomLevelHash(level);
#if DEBUG
                    Plugin.Log.Info($"Getting rank status for {level.levelID}");
#endif
                    try
                    {
                        var client = new WebClient();
                        var res = JObject.Parse(client.DownloadString(RequestURL + hash));
                        var ranked = res?["difficulties"]?.Any(token => token["status"]?.ToObject<int>() == 3) ?? false;
                        result[hash] = ranked;
                    }
                    catch (Exception e)
                    {
                        Plugin.Log.Warn($"Cannot get BeatLeader rank status for {level.levelID}: {e.Message}");
                        if (e.Message.Contains("404")) result[hash] = false;
                        if (!(e is WebException))
                        {
                            Plugin.Log.Debug(e);
                        }
                    }

                    if (cancellationToken.IsCancellationRequested) throw new Exception("Operation Cancelled");
                });
            }
            catch (AggregateException _)
            {
                Plugin.Log.Warn("Get rank status from BeatLeader is Cancelled");
            }

            Plugin.Log.Debug("Get rank status from BeatLeader Finished");
            return Task.FromResult(result as IDictionary<string, bool>);
        }
    }
}