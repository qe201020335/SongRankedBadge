using System;
using HarmonyLib;
using HMUI;
using IPA.Utilities;
using SongDetailsCache.Structs;
using TMPro;
using UnityEngine;
using UObject = UnityEngine.Object;
using Polyglot;

namespace SongRankedBadge
{
    [HarmonyPatch(typeof(LevelListTableCell), nameof(LevelListTableCell.SetDataFromLevelAsync))]
    public class LevelListTableCellPatch
    {
        [HarmonyPrefix]
        static void Prefix(ref IPreviewBeatmapLevel level, ref bool isPromoted, out RankStatus __state)
        {
            __state = RankStatus.None;
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
            try
            {
                var isRanked = __state != RankStatus.None;
                var promoTextGo = ____promoBadgeGo.transform.Find("PromoText").gameObject;
                // UObject.Destroy(promoTextGo.GetComponent<LocalizedTextMeshProUGUI>()); // can't simply destroy the script, this cell may be reused.
                var localization = promoTextGo.GetComponent<LocalizedTextMeshProUGUI>();
                localization.enabled = !isRanked; // no more translation :)

                if (isRanked)
                {
                    // turn off the promotion background
                    ____promoBackgroundGo.SetActive(false);
                    // and change the text
                    var promoText = promoTextGo.GetComponent<TMP_Text>();

                    if (!Equals(promoText, null))
                    {
                        // TODO: Change badge color 
                        switch (__state)
                        {
                            case RankStatus.Ranked:
                                promoText.text = "Ranked";
                                break;
                            case RankStatus.ScoreSaber:
                                promoText.text = "SS Ranked";
                                break;
                            case RankStatus.BeatLeader:
                                promoText.text = "BL Ranked";
                                break;
                        }
                    }
                }
                else
                {
                    // TODO: Change badge color back
                }
            }
            catch (Exception e)
            {
                Plugin.Log.Warn($"Cannot change badge text: {e.Message}");
                Plugin.Log.Debug(e);
            }
        }
    }
}