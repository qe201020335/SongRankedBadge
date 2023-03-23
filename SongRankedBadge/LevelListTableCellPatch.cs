using System;
using System.Collections.Generic;
using HarmonyLib;
using HMUI;
using IPA.Utilities;
using SongDetailsCache.Structs;
using TMPro;
using UnityEngine;
using UObject = UnityEngine.Object;
using Polyglot;
using SongRankedBadge.Configuration;

namespace SongRankedBadge
{
    [HarmonyPatch(typeof(LevelListTableCell), nameof(LevelListTableCell.SetDataFromLevelAsync))]
    public class LevelListTableCellPatch
    {
        // DF166FFF
        private static readonly Color c_promoOG = new Color32(0xDF, 0x16, 0x6F, 0xFF);
        private static readonly Color c_ranked = c_promoOG; // same as the og promo 
        private static readonly Color c_blranked = new Color32(0x8B, 0x63, 0xBB, 0xFF);
        private static readonly Color c_ssranked = new Color32(0xED, 0xCC, 0x08, 0xFF);

        private static readonly Dictionary<RankStatus, Color> Colors = new Dictionary<RankStatus, Color>
        {
            [RankStatus.None] = c_promoOG,
            [RankStatus.Ranked] = c_ranked,
            [RankStatus.BeatLeader] = c_blranked,
            [RankStatus.ScoreSaber] = c_ssranked
        };

        private static readonly Dictionary<RankStatus, string> Texts = new Dictionary<RankStatus, string>
        {
            [RankStatus.None] = "",
            [RankStatus.Ranked] = "Ranked",
            [RankStatus.BeatLeader] = "BL Ranked",
            [RankStatus.ScoreSaber] = "SS Ranked"
        };

        [HarmonyPrefix]
        static void Prefix(ref IPreviewBeatmapLevel level, ref bool isPromoted, out RankStatus __state)
        {
            __state = RankStatus.None;
            if (!PluginConfig.Instance.ModEnable) return;
            try
            {
                if (level is CustomPreviewBeatmapLevel customLevel)
                {
                    var hash = SongCore.Utilities.Hashing.GetCustomLevelHash(customLevel);
                    __state = RankStatusCacheManager.Instance.GetSongRankedStatus(hash);
                    isPromoted = isPromoted || __state != RankStatus.None;
                }
            }
            catch (Exception e)
            {
                Plugin.Log.Warn($"Cannot get rank status: {e.Message}");
                Plugin.Log.Debug(e);
            }
        }

        [HarmonyPostfix]
        static void Postfix(GameObject ____promoBackgroundGo, GameObject ____promoBadgeGo, RankStatus __state)
        {
            if (!PluginConfig.Instance.ModEnable) return;
            try
            {
                var isRanked = __state != RankStatus.None;
                var promoTextGo = ____promoBadgeGo.transform.Find("PromoText").gameObject;
                // UObject.Destroy(promoTextGo.GetComponent<LocalizedTextMeshProUGUI>()); // can't simply destroy the script, this cell may be reused.
                var localization = promoTextGo.GetComponent<LocalizedTextMeshProUGUI>();
                var promoTextBg = ____promoBadgeGo.GetComponent<ImageView>();
                localization.enabled = !isRanked; // no more translation :)

                if (isRanked)
                {
                    // turn off the promotion background
                    ____promoBackgroundGo.SetActive(false);

                    // and change the text and badge color
                    var promoText = promoTextGo.GetComponent<TMP_Text>();
                    if (PluginConfig.Instance.DifferentColor)
                    {
                        promoTextBg.color = Colors[__state];
                    }

                    promoText.text = PluginConfig.Instance.DifferentText ? Texts[__state] : Texts[RankStatus.Ranked];
                }
                else
                {
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