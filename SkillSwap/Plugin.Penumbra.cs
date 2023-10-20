using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

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

                var modFolder = Path.Combine(saveLocation, name);
                Directory.CreateDirectory(modFolder);
                var modConfig = Path.Combine(modFolder, "meta.json");
                var configString = JsonConvert.SerializeObject(mod);
                File.WriteAllText(modConfig, configString);

                foreach (var entry in RemoveConflicts(mapping)) {
                    var modFile = Path.Combine(modFolder, entry.Key);
                    var modFileFolder = Path.GetDirectoryName(modFile);
                    Directory.CreateDirectory(modFileFolder);
                    File.WriteAllBytes(modFile, entry.Value);
                }

                Services.Log("Exported To: " + saveLocation);
            }
            catch (Exception e) {
                Services.Error(e, "Could not export to Penumbra");
            }
        }
    }
}
