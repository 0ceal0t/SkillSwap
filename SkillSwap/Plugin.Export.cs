using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwap {
    public partial class Plugin {
        public string GetTmbPath(string key) {
            return "chara/action/" + key + ".tmb";
        }

        public string GetPapPath(string key) {
            if(key.StartsWith("ws/")) {
                var split = key.Split('/');
                var weapon = split[1];
                return "chara/human/c0101/animation/a0001/" + weapon + "/" + key + ".pap";
            }
            return "chara/human/c0101/animation/a0001/bt_common/" + key + ".pap";
        }

        public bool FileExists(string path) {
            return PluginInterface.Data.FileExists(path);
        }

        public Dictionary<string, string> FileMapping() {
            Dictionary<string, string> ret = new();
            foreach(var item in Swaps) {
                MapSingle(item.Current.StartKey, item.New.StartKey, ret);
                MapSingle(item.Current.EndKey, item.New.EndKey, ret);
                MapSingle(item.Current.HitKey, item.New.HitKey, ret);
            }
            foreach (var item in ret) {
                PluginLog.Log($"Replacing: {item.Key} With: {item.Value}");
            }
            return ret;
        }

        public void MapSingle(string keyCurrent, string keyNew, Dictionary<string, string> dict) {
            if (string.IsNullOrEmpty(keyCurrent) || string.IsNullOrEmpty(keyNew)) return;

            var tmbCurrent = GetTmbPath(keyCurrent);
            var tmbNew = GetTmbPath(keyNew);

            var papCurrent = GetPapPath(keyCurrent);
            var papNew = GetPapPath(keyNew);

            if(FileExists(tmbCurrent) && FileExists(tmbNew)) {
                dict[tmbCurrent] = tmbNew;
            }
            if(FileExists(papCurrent) && FileExists(papNew)) {
                dict[papCurrent] = papNew;
            }
        }
    }
}
