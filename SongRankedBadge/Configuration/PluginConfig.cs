
using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace SongRankedBadge.Configuration
{
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; } = null!;
        public virtual bool ModEnable { get; set; } = true;
        public virtual bool DifferentText { get; set; } = true;
        public virtual bool DifferentColor { get; set; } = true;
        public virtual bool SettingsMenuButton { get; set; } = false;
        public virtual bool ShowCurated { get; set; } = true;
    }
}
