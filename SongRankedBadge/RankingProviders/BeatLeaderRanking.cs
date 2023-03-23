using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using IPA.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SongRankedBadge.RankingProviders
{
    public class BeatLeaderRanking : IRankingProvider
    {
        private const string RequestURL = @"https://api.beatleader.xyz/map/hash/";
        private const string RankedPlaylist = @"https://api.beatleader.xyz/playlist/ranked";

        private readonly string DataFolderPath = Path.Combine(UnityGame.UserDataPath, "SongRankedStatus");
        private const string RankedSongDataName = @"RankedSongs.json";

        private const int Retry = 3;
        private const int RetryDelayMilli = 30 * 1000;

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

        public async Task<HashSet<string>> GetRankedStatus(CancellationToken cancellationToken)
        {
            int tries = 0;
            Plugin.Log.Info("Get ranked song data for BeatLeader with Retry");
            while (tries < Retry)
            {
                Plugin.Log.Debug($"Trail {tries}");
                try
                {
                    var result = await GetRankedStatusInternal(cancellationToken);
                    if (result.Count > 0) return result;
                    Plugin.Log.Debug($"Trail {tries} returned nothing");
                    await Task.Delay(RetryDelayMilli, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    Plugin.Log.Warn("GetRankedStatus Cancelled");
                    break;
                }
                catch (Exception e)
                {
                    Plugin.Log.Warn($"Trail {tries} errored: {e.Message}");
                    Plugin.Log.Debug(e);
                }
                tries++;
            }
            Plugin.Log.Warn($"Failed to fetch any ranked songs after all {Retry} trails");
            return new HashSet<string>();
        }
        
        private async Task<HashSet<string>> GetRankedStatusInternal(CancellationToken cancellationToken)
        {
            Plugin.Log.Info("Get ranked song data for BeatLeader");
            var filePath = Path.Combine(DataFolderPath, RankedSongDataName);
            HashSet<string>? cached = null;
            
            // first check our cached data
            if (File.Exists(filePath))
            {
                try
                {
                    var fileInfo = new FileInfo(filePath);
                    using var file = File.OpenText(filePath);
                    using var jsonReader = new JsonTextReader(file);
                    cached = JToken.ReadFrom(jsonReader)?.ToObject<HashSet<string>>();

                    if (fileInfo.LastWriteTime - DateTime.Now < new TimeSpan(3, 0, 0, 0))
                    {
                        // if it is not too old (3 days) and not empty
                        if (cached != null && cached.Count != 0) return cached;
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.Warn("Exception loading cached data: " + e.Message);
                    Plugin.Log.Debug(e);
                }
            }
            else
            {
                if (!Directory.Exists(DataFolderPath)) Directory.CreateDirectory(DataFolderPath);
            }

            Plugin.Log.Info("Cached data not exist or too old, fetching new");
            
            try
            {
                using var client = new WebClient();
#if DEBUG
                await Task.Delay(3000, cancellationToken); // simulate a slow internet
#endif
                var res = JObject.Parse(await client.DownloadStringTaskAsync(RankedPlaylist));

                if (cancellationToken.IsCancellationRequested) throw new TaskCanceledException();

                var newRanked = res?["songs"]?.AsJEnumerable()?.Values<string>("hash")?.ToHashSet();
                Plugin.Log.Debug($"Fetched {newRanked?.Count} ranked song hashes");
                
                if (newRanked != null && newRanked.Count > 0)
                {
                    cached = newRanked;
                    
                    if (cancellationToken.IsCancellationRequested) throw new TaskCanceledException();

                    Plugin.Log.Debug("Saving cached data");
                    using var writer = new StreamWriter(filePath, false);
                    using var jsonWriter = new JsonTextWriter(writer);
                    var ser = new JsonSerializer();
                    ser.Serialize(jsonWriter, newRanked);
                    return cached;
                }
                
                Plugin.Log.Warn("Fetch returned 0 ranked songs!");
            }
            catch (TaskCanceledException)
            {
                Plugin.Log.Warn("Fetch task cancelled, returning what we had before");
            }
            catch (Exception e)
            {
                Plugin.Log.Warn("Exception fetching ranked songs: " + e.Message);
                Plugin.Log.Debug(e);
            }

            return cached ?? new HashSet<string>();
        }
    }
}