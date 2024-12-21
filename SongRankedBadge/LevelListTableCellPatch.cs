using System;
using System.Collections.Generic;
using HarmonyLib;
using HMUI;
using IPA.Utilities;
using SongDetailsCache.Structs;
using TMPro;
using UnityEngine;
using UObject = UnityEngine.Object;
using BGLib.Polyglot;
using SongRankedBadge.Configuration;

namespace SongRankedBadge
{
    [HarmonyPatch(typeof(LevelListTableCell), nameof(LevelListTableCell.SetDataFromLevelAsync))]
    public class LevelListTableCellPatch
    {
        // DF166FFF
        private static readonly Color c_promoOG = new Color32(0xDF, 0x16, 0x6F, 0xFF);
        private static readonly Color c_curated = new Color32(0x00,0xBC,0x8C, 0xFF);
        private static readonly Color c_ranked = c_promoOG; // same as the og promo 
        private static readonly Color c_blranked = new Color32(0x8B, 0x63, 0xBB, 0xFF);
        private static readonly Color c_ssranked = new Color32(0xED, 0xCC, 0x08, 0xFF);

        private static readonly Dictionary<RankStatus, Color> Colors = new Dictionary<RankStatus, Color>
        {
            [RankStatus.None] = c_promoOG,
            [RankStatus.Curated] = c_curated,
            [RankStatus.Ranked] = c_ranked,
            [RankStatus.BeatLeader] = c_blranked,
            [RankStatus.ScoreSaber] = c_ssranked
        };

        private static readonly Dictionary<RankStatus, string> Texts = new Dictionary<RankStatus, string>
        {
            [RankStatus.None] = "",
            [RankStatus.Curated] = "Curated",
            [RankStatus.Ranked] = "Ranked",
            [RankStatus.BeatLeader] = "BL Ranked",
            [RankStatus.ScoreSaber] = "SS Ranked"
        };

        [HarmonyPostfix]
        static void Postfix(ref BeatmapLevel beatmapLevel, ref bool isPromoted, GameObject ____promoBadgeGo)
        {
            if (!PluginConfig.Instance.ModEnable) return;
            
            RankStatus rankedStatus = RankStatus.None;
            try
            {
                var hash = SongCore.Utilities.Hashing.GetCustomLevelHash(beatmapLevel);
                rankedStatus = RankStatusManager.Instance.GetSongRankedStatus(hash);
            }
            catch (Exception e)
            {
                Plugin.Log.Warn($"Cannot get rank status: {e.Message}");
                Plugin.Log.Debug(e);
                return;
            }

#if DEBUG
            Plugin.Log.Debug($"{beatmapLevel.songName}: {rankedStatus}");
#endif
            
            try
            {
                if (rankedStatus == RankStatus.Curated && !PluginConfig.Instance.ShowCurated)
                {
                    rankedStatus = RankStatus.None;
                }
                
                var shouldApply = rankedStatus != RankStatus.None;

                ____promoBadgeGo.SetActive(isPromoted || shouldApply);

                var promoTextGo = ____promoBadgeGo.transform.Find("PromoText").gameObject;
                var localization = promoTextGo.GetComponent<LocalizedTextMeshProUGUI>();
                var promoTextBg = ____promoBadgeGo.GetComponent<ImageView>();
                // can't simply destroy the script, this cell may be reused.
                localization.enabled = !shouldApply;

                if (shouldApply)
                {
                    // change the text and badge color
                    var promoText = promoTextGo.GetComponent<TMP_Text>();

                    promoTextBg.color = PluginConfig.Instance.DifferentColor ? Colors[rankedStatus] : c_promoOG;

                    promoText.text = rankedStatus == RankStatus.Curated || PluginConfig.Instance.DifferentText ? Texts[rankedStatus] : Texts[RankStatus.Ranked];
                }
                else
                {
                    // the original text will be restored by the localization script
                    promoTextBg.color = c_promoOG;
                }
            }
            catch (NullReferenceException e)
            {
                Plugin.Log.Debug(e);
            }
            catch (Exception e)
            {
                Plugin.Log.Warn($"Cannot change badge text: {e.Message}");
                Plugin.Log.Debug(e);
            }
        }
    }
}