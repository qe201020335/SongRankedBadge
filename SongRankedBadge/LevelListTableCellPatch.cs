﻿using System;
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
                    if (Plugin.SongDetails?.songs?.FindByHash(hash, out var song) == true)
                    {
                        __state = song.rankedStatus == RankedStatus.Ranked;
                        isPromoted = isPromoted || __state;
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.Log.Warn($"Cannot get rank status: {e.Message}");
                Plugin.Log.Debug(e);
            }
        }

        [HarmonyPostfix]
        static void Postfix(GameObject ___promoBackgroundGo, GameObject ___promoBadgeGo, bool __state)
        {
            try
            {
                ___promoBackgroundGo.SetActive(!__state);
                var promoTextGo = ___promoBadgeGo.transform.Find("PromoText").gameObject;
                // UObject.Destroy(promoTextGo.GetComponent<LocalizedTextMeshProUGUI>()); // can't simply destroy the script, this cell may be reused.
                var localization = promoTextGo.GetComponent<LocalizedTextMeshProUGUI>();
                localization.enabled = !__state; // no more translation :)
                
                if (__state)
                {
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