using Dalamud.Logging;
using Dalamud.Plugin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwap {
    public partial class Plugin {
        public void ExportPenumbra() {
            var mapping = FileMapping();
            Confirm.SetData(ModName, ModAuthor, ModVersion, Config.SaveLocation, mapping, (name, author, version, save, mapping) => Penumbra(name, author, version, save, mapping));
        }

        public struct PenumbraMod {
            public string Name;
            public string Author;
            public string Description;
#nullable enable
            public string? Version;
            public string? Website;
#nullable disable
            public Dictionary<string, string> FileSwaps;
        }

        public void Penumbra(string name, string author, string version, string saveLocation, Dictionary<string, SwapMapping> mapping) {
            try {
                PenumbraMod mod = new();
                mod.Name = name;
                mod.Author = author;
                mod.Description = "Exported from SkillSwap";
                mod.Version = version;
                mod.Website = null;
                mod.FileSwaps = new Dictionary<string, string>();

                string modFolder = Path.Combine(saveLocation, name);
                Directory.CreateDirectory(modFolder);
                string modConfig = Path.Combine(modFolder, "meta.json");
                string configString = JsonConvert.SerializeObject(mod);
                File.WriteAllText(modConfig, configString);

                foreach (var entry in RemoveConflicts(mapping)) {
                    string modFile = Path.Combine(modFolder, entry.Key);
                    string modFileFolder = Path.GetDirectoryName(modFile);
                    Directory.CreateDirectory(modFileFolder);
                    File.WriteAllBytes(modFile, entry.Value);
                }

                PluginLog.Log("Exported To: " + saveLocation);
            }
            catch(Exception e ) {
                PluginLog.LogError(e, "Could not export to Penumbra" );
            }
        }
    }
}
