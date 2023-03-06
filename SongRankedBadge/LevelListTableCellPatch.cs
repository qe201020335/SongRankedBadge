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
        static void Prefix(ref IPreviewBeatmapLevel level, ref bool isPromoted, out bool __state)
        {
            __state = false;
            try
            {
                if (level is CustomPreviewBeatmapLevel customLevel)
                {
                    var hash = SongCore.Utilities.Hashing.GetCustomLevelHash(customLevel);
                    __state = RankStatusCacheManager.Instance.GetSongRankedStatus(hash) != RankStatus.None;
                    isPromoted = isPromoted || __state;
                }
            }
            catch (Exception e)
            {
                Plugin.Log.Warn($"Cannot get rank status: {e.Message}");
                Plugin.Log.Debug(e);
            }
        }

        [HarmonyPostfix]
        static void Postfix(GameObject ____promoBackgroundGo, GameObject ____promoBadgeGo, bool __state)
        {
            try
            {
                var promoTextGo = ____promoBadgeGo.transform.Find("PromoText").gameObject;
                // UObject.Destroy(promoTextGo.GetComponent<LocalizedTextMeshProUGUI>()); // can't simply destroy the script, this cell may be reused.
                var localization = promoTextGo.GetComponent<LocalizedTextMeshProUGUI>();
                localization.enabled = !__state; // no more translation :)

                if (__state)
                {
                    // turn off the promotion background
                    ____promoBackgroundGo.SetActive(false);
                    // and change the text
                    var promoText = promoTextGo.GetComponent<TMP_Text>();

                    if (!Equals(promoText, null))
                    {
                        promoText.text = "Ranked!";
                    }
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