using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.IO;

namespace SkillSwap
{
    [Serializable]
    public class Configuration : IPluginConfiguration {
        public int Version { get; set; } = 0;

        public string SaveLocation = Path.Combine(new[] {
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "XIVLauncher"
        });

        [NonSerialized]
        private DalamudPluginInterface pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface) {
            this.pluginInterface = pluginInterface;
        }

        public void Save()  {
            pluginInterface.SavePluginConfig(this);
        }
    }
}
