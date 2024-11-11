using IPA;
using IPA.Config;
using IPA.Config.Stores;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using SongDetailsCache;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;
using Conf = IPA.Config.Config;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using BeatSaberMarkupLanguage.Util;
using SongRankedBadge.Installers;
using SongRankedBadge.Configuration;
using SiraUtil.Zenject;

namespace SongRankedBadge
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    [NoEnableDisable]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; } = null!;
        internal static IPALogger Log { get; private set; } = null!;

        private readonly Harmony _harmony = new Harmony("com.github.qe201020335.SongRankedBadge");
        
        internal MenuButton MenuButton = new MenuButton("Ranked Badge", "PromoBadge? RankedBadge!", OnMenuButtonClick);
        
        private UI.ConfigViewFlowCoordinator? _configViewFlowCoordinator;

        [Init]
        public Plugin(IPALogger logger, Conf conf, Zenjector zenjector)
        {
            Instance = this;
            Log = logger;
            Log.Info("SongRankedBadge initialized.");
            RankStatusManager.Instance.Init();
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            Log.Debug("Config loaded");
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
            MainMenuAwaiter.MainMenuInitializing += OnMenuLoad;

            zenjector.UseLogger(logger);
            zenjector.Install<MenuInstaller>(Location.Menu);
        }
        
        private void OnMenuLoad()
        {
            if(PluginConfig.Instance.SettingsMenuButton)
                MenuButtons.Instance.RegisterButton(MenuButton);
        }
        
        private static void OnMenuButtonClick()
        {
            if (Instance._configViewFlowCoordinator == null)
            {
                Instance._configViewFlowCoordinator = BeatSaberUI.CreateFlowCoordinator<UI.ConfigViewFlowCoordinator>();
            }
            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(Instance._configViewFlowCoordinator);
        }
    }
}
