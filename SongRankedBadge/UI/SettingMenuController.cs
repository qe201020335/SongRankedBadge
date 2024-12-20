using BeatSaberMarkupLanguage.Attributes;
using System.Collections.Generic;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.ViewControllers;
using SongRankedBadge.Configuration;
using TMPro;
using BeatSaberMarkupLanguage.MenuButtons;
using BeatSaberMarkupLanguage.Components;
using UnityEngine;
using UnityEngine.PlayerLoop;
using System.ComponentModel;

namespace SongRankedBadge.UI
{
   // setting controller for menu button
    [HotReload(RelativePathToLayout = @"configMenu.bsml")]
    [ViewDefinition("SongRankedBadge.UI.configMenu.bsml")]
    internal class SettingMenuController : BSMLAutomaticViewController
    {
        private static PluginConfig Config => PluginConfig.Instance;
        public event PropertyChangedEventHandler PropertyChanged = null!;

        [UIValue("ModEnable")]
        private bool ModEnable
        {
            get => Config.ModEnable;
            set => Config.ModEnable = value;
        }

        [UIValue("DiffText")]
        public bool DiffText
        {
            get => Config.DifferentText;
            set => Config.DifferentText = value;
        }


        [UIValue("DiffColor")]
        public bool DiffColor
        {
            get => Config.DifferentColor;
            set => Config.DifferentColor = value;
        }

        [UIValue("MenuSettings")]
        public bool MenuSettings
        {
            get => Config.SettingsMenuButton;
            set => Config.SettingsMenuButton = value;
        }
        
        [UIValue("ShowCurated")]
        public bool ShowCurated
        {
            get => Config.ShowCurated;
            set => Config.ShowCurated = value;
        }
    }
}