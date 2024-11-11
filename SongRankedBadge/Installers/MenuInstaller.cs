using SongRankedBadge.Configuration;
using SongRankedBadge.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace SongRankedBadge.Installers
{
    class MenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            if (!PluginConfig.Instance.SettingsMenuButton)
                Container.BindInterfacesTo<ModSettings>().FromNewComponentOnRoot().AsSingle();
        }
    }
}
