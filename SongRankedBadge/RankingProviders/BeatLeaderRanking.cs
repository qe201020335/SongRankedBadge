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
        private const string RankedPlaylist = @"https://api.beatleader.xyz/playlist/ranked";

        private readonly string DataFolderPath = Path.Combine(UnityGame.UserDataPath, "SongRankedBadge");
        private const string RankedSongDataName = @"BeatLeaderRankedSongs.json";

        private const int Retry = 3;
        private const int RetryDelayMilli = 30 * 1000;

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
                catch (OperationCanceledException)
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

                cancellationToken.ThrowIfCancellationRequested();

                var newRanked = res?["songs"]?.AsJEnumerable()?.Values<string>("hash")?.ToHashSet();
                Plugin.Log.Debug($"Fetched {newRanked?.Count} ranked song hashes");
                
                if (newRanked != null && newRanked.Count > 0)
                {
                    cached = newRanked;
                    
                    cancellationToken.ThrowIfCancellationRequested();

                    Plugin.Log.Debug("Saving cached data");
                    using var writer = new StreamWriter(filePath, false);
                    using var jsonWriter = new JsonTextWriter(writer);
                    var ser = new JsonSerializer();
                    ser.Serialize(jsonWriter, newRanked);
                    return cached;
                }
                
                Plugin.Log.Warn("Fetch returned 0 ranked songs!");
            }
            catch (OperationCanceledException)
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