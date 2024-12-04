using IPA;
using IPA.Config.Stores;
using System.Reflection;
using HarmonyLib;
using BeatSaberMarkupLanguage.Settings;
using BeatSaberMarkupLanguage.Util;
using SongRankedBadge.Configuration;
using SongRankedBadge.UI;
using IPALogger = IPA.Logging.Logger;
using Conf = IPA.Config.Config;

namespace SongRankedBadge
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    [NoEnableDisable]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; } = null!;
        internal static IPALogger Log { get; private set; } = null!;

        private readonly Harmony _harmony = new Harmony("com.github.qe201020335.SongRankedBadge");

        private readonly ModSettings _modSettings;

        [Init]
        public Plugin(IPALogger logger, Conf conf)
        {
            Instance = this;
            Log = logger;
            RankStatusManager.Instance.Init();
            PluginConfig.Instance = conf.Generated<PluginConfig>();
            _modSettings = new ModSettings();
            Log.Debug("Config loaded");
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
            MainMenuAwaiter.MainMenuInitializing += OnMenuLoad;
            Log.Info("SongRankedBadge initialized.");
        }

        private void OnMenuLoad()
        {
            BSMLSettings.Instance.AddSettingsMenu("Ranked Badge", "SongRankedBadge.UI.configMenu.bsml", _modSettings);
        }
    }
}