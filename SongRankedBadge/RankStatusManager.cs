using System.Threading.Tasks;
using SongDetailsCache;
using SongDetailsCache.Structs;

namespace SongRankedBadge
{
    internal class RankStatusManager
    {
        internal static readonly RankStatusManager Instance = new RankStatusManager();
        
        private SongDetails? _songDetails = null;


        internal void Init()
        {
            Task.Factory.StartNew(async () =>
            {
                Plugin.Log.Debug("Loading song details...");
                _songDetails = await SongDetails.Init(); 
                Plugin.Log.Debug("Song details loaded.");
            });
        }

        internal RankStatus GetSongRankedStatus(string hash)
        {
            if (_songDetails == null)
            {
                // Data not ready yet
                return RankStatus.None;
            }
            
            hash = hash.ToLower();
            if (_songDetails.songs.FindByHash(hash, out var song))
            {
                var rankedStates = song.rankedStates;
                
                var ssRank = rankedStates.HasFlag(RankedStates.ScoresaberRanked);
                var blRank = rankedStates.HasFlag(RankedStates.BeatleaderRanked);
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
                    return RankStatus.ScoreSaber;
                }
            }

            return RankStatus.None;
        }
    }

    internal enum RankStatus
    {
        None,
        ScoreSaber,
        BeatLeader,
        Ranked // just ranked, means both
    }
}