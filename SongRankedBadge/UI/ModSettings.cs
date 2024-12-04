using BeatSaberMarkupLanguage.Attributes;
using SongRankedBadge.Configuration;

namespace SongRankedBadge.UI
{
    public class ModSettings
    {
        private static PluginConfig Config => PluginConfig.Instance;

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
    }  
}

