using Dalamud.Plugin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwap {
    public partial class Plugin {
        public void ExportTextools() {
            var mapping = FileMapping();
            Textools(ModName, ModAuthor, ModVersion, Config.SaveLocation, mapping);
        }

        public struct TTMPL {
            public string TTMPVersion;
            public string Name;
            public string Author;
            public string Version;
#nullable enable
            public string? Description;
            public string? ModPackPages;
#nullable disable
            public TTMPL_Simple[] SimpleModsList;
        }
        public struct TTMPL_Simple {
            public string Name;
            public string Category;
            public string FullPath;
            public bool IsDefault;
            public int ModOffset;
            public int ModSize;
            public string DatFile;
#nullable enable
            public string? ModPackEntry;
#nullable disable
        }

        public void Textools(string name, string author, string version, string saveLocation, Dictionary<string, string> mapping) {
            try {
                List<TTMPL_Simple> simpleParts = new List<TTMPL_Simple>();
                byte[] newData;
                int ModOffset = 0;

                using (MemoryStream ms = new MemoryStream())
                using (BinaryWriter writer = new BinaryWriter(ms)) {
                    foreach(var entry in mapping) {
                        var newFile = PluginInterface.Data.GetFile(entry.Value);
                        var modData = CreateType2Data(newFile.Data);
                        simpleParts.Add(CreateModResource(entry.Key, ModOffset, modData.Length));
                        writer.Write(modData);
                        ModOffset += modData.Length;
                    }
                    newData = ms.ToArray();
                }

                TTMPL mod = new TTMPL();
                mod.TTMPVersion = "1.3s";
                mod.Name = name;
                mod.Author = author;
                mod.Version = version;
                mod.Description = null;
                mod.ModPackPages = null;
                mod.SimpleModsList = simpleParts.ToArray();

                string tempDir = Path.Combine(saveLocation, "TEXTOOLS_TEMP");
                Directory.CreateDirectory(tempDir);
                string mdpPath = Path.Combine(tempDir, "TTMPD.mpd");
                string mplPath = Path.Combine(tempDir, "TTMPL.mpl");
                string mplString = JsonConvert.SerializeObject(mod);
                File.WriteAllText(mplPath, mplString);
                File.WriteAllBytes(mdpPath, newData);

                string zipLocation = Path.Combine(saveLocation, name + ".ttmp2");
                ZipFile.CreateFromDirectory(tempDir, zipLocation);

                Directory.Delete(tempDir, true);
                PluginLog.Log("Exported To: " + zipLocation);
            }
            catch (Exception e) {
                PluginLog.LogError(e, "Could not export to TexTools");
            }
        }

        public TTMPL_Simple CreateModResource(string path, int modOffset, int modSize) {
            TTMPL_Simple simple = new TTMPL_Simple();
            string[] split = path.Split('/');
            simple.Name = split[split.Length - 1];
            simple.Category = "Raw File Import";
            simple.FullPath = path;
            simple.IsDefault = false;
            simple.ModOffset = modOffset;
            simple.ModSize = modSize;
            simple.DatFile = "040000";
            simple.ModPackEntry = null;
            return simple;
        }
    }
}
