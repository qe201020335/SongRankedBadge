using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.MenuButtons;
using BeatSaberMarkupLanguage.Settings;
using SongRankedBadge.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace SongRankedBadge.UI
{
    public class ModSettings : MonoBehaviour, IInitializable, INotifyPropertyChanged
    {
        private static PluginConfig Config => PluginConfig.Instance;
        private GameplaySetupViewController? gameplaySetupViewController;
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
        
        [UIValue("ShowCurated")]
        public bool ShowCurated
        {
            get => Config.ShowCurated;
            set => Config.ShowCurated = value;
        }
        
        [UIValue("MenuSettings")]
        public bool MenuSettings
        {
            get => Config.SettingsMenuButton;
            set {
                Config.SettingsMenuButton = value;

                if(value)
                    MenuButtons.Instance.RegisterButton(Plugin.Instance.MenuButton);
                else
                    MenuButtons.Instance.UnregisterButton(Plugin.Instance.MenuButton);
            }
        }

        public void Initialize()
        {
            BSMLSettings.Instance.AddSettingsMenu("Ranked Badge", "SongRankedBadge.UI.configMenu.bsml", this);
        }
    }  
}

