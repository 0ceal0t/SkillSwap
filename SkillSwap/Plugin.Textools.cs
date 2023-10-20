using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace SkillSwap {
    public partial class Plugin {
        public void ExportTextools() {
            var mapping = FileMapping();
            Confirm.SetData(ModName, ModAuthor, ModVersion, Config.SaveLocation, mapping, (name, author, version, save, mapping) => Textools(name, author, version, save, mapping));
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

        public void Textools(string name, string author, string version, string saveLocation, Dictionary<string, SwapMapping> mapping) {
            try {
                List<TTMPL_Simple> simpleParts = new();
                byte[] newData;
                var ModOffset = 0;

                using (MemoryStream ms = new())
                using (BinaryWriter writer = new(ms)) {
                    foreach (var entry in RemoveConflicts(mapping)) {
                        var modData = CreateType2Data(entry.Value);
                        simpleParts.Add(CreateModResource(entry.Key, ModOffset, modData.Length));
                        writer.Write(modData);
                        ModOffset += modData.Length;
                    }
                    newData = ms.ToArray();
                }

                TTMPL mod = new();
                mod.TTMPVersion = "1.3s";
                mod.Name = name;
                mod.Author = author;
                mod.Version = version;
                mod.Description = null;
                mod.ModPackPages = null;
                mod.SimpleModsList = simpleParts.ToArray();

                var tempDir = Path.Combine(saveLocation, "TEXTOOLS_TEMP");
                Directory.CreateDirectory(tempDir);
                var mdpPath = Path.Combine(tempDir, "TTMPD.mpd");
                var mplPath = Path.Combine(tempDir, "TTMPL.mpl");
                var mplString = JsonConvert.SerializeObject(mod);
                File.WriteAllText(mplPath, mplString);
                File.WriteAllBytes(mdpPath, newData);

                var zipLocation = Path.Combine(saveLocation, name + ".ttmp2");
                ZipFile.CreateFromDirectory(tempDir, zipLocation);

                Directory.Delete(tempDir, true);
                Services.Log("Exported To: " + zipLocation);
            }
            catch (Exception e) {
                Services.Error(e, "Could not export to TexTools");
            }
        }

        public static TTMPL_Simple CreateModResource(string path, int modOffset, int modSize) {
            TTMPL_Simple simple = new();
            var split = path.Split('/');
            simple.Name = split[^1];
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
