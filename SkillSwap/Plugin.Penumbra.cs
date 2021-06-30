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
            Penumbra(ModName, ModAuthor, ModVersion, Config.SaveLocation, mapping);
        }

        public struct PenumbraMod {
            public string Name;
            public string Author;
            public string Description;
            public string? Version;
            public string? Website;
            public Dictionary<string, string> FileSwaps;
        }

        public void Penumbra(string name, string author, string version, string saveLocation, Dictionary<string, string> mapping) {
            try {
                PenumbraMod mod = new PenumbraMod();
                mod.Name = name;
                mod.Author = author;
                mod.Description = "Exported from SkillSwap";
                mod.Version = version;
                mod.Website = null;
                mod.FileSwaps = mapping;

                string modFolder = Path.Combine(saveLocation, name);
                Directory.CreateDirectory(modFolder);
                string modConfig = Path.Combine(modFolder, "meta.json");
                string configString = JsonConvert.SerializeObject(mod);
                File.WriteAllText(modConfig, configString);

                PluginLog.Log("Exported To: " + saveLocation);
            }
            catch(Exception e ) {
                PluginLog.LogError(e, "Could not export to Penumbra" );
            }
        }
    }
}
