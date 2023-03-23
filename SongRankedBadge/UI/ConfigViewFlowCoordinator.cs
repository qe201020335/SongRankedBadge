using BeatSaberMarkupLanguage;
using HMUI;
using System;

namespace SongRankedBadge.UI
{
    public class ConfigViewFlowCoordinator : FlowCoordinator
    {
        private SettingMenuController _mainPanel;

        public void Awake()
        {
            if (_mainPanel == null)
            {
                _mainPanel = BeatSaberUI.CreateViewController<SettingMenuController>();
            }
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            try
            {
                if (firstActivation)
                {
                    SetTitle("SongRankedBadge");
                    showBackButton = true;
                }

                if (addedToHierarchy)
                {
                    ProvideInitialViewControllers(_mainPanel);
                }
            }
            catch (Exception e)
            {
                Plugin.Log.Error(e);
            }
        }

        protected override void BackButtonWasPressed(ViewController topController)
        {
            base.BackButtonWasPressed(topController);
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}