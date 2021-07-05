using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SkillSwap {
    public struct SwapMapping {
        public string OldTmb;
        public string OldPap;
        public string NewTmb;
        public string NewPap;
        public string UniqueId;
    }

    public partial class Plugin {
        public static Regex rx = new Regex(@"cbbm(_[a-zA-Z0-9]+)+", RegexOptions.Compiled);

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

        public Dictionary<string, SwapMapping> FileMapping() {
            Dictionary<string, SwapMapping> ret = new();
            foreach(var item in Swaps) {
                MapSingle(item.Current?.StartKey, item.New?.StartKey, item.Current.Id + "s", ret);
                MapSingle(item.Current?.EndKey, item.New?.EndKey, item.Current.Id + "e", ret);
                MapSingle(item.Current?.HitKey, item.New?.HitKey, item.Current.Id + "h", ret);
            }
            foreach (var item in ret) {
                PluginLog.Log($"Replacing: {item.Value.OldPap} With: {item.Value.NewPap}");
                PluginLog.Log($"Replacing: {item.Value.OldTmb} With: {item.Value.NewTmb}");
            }
            return ret;
        }

        public void MapSingle(string keyCurrent, string keyNew, string uniqueId, Dictionary<string, SwapMapping> dict) {
            if (string.IsNullOrEmpty(keyCurrent) || string.IsNullOrEmpty(keyNew)) return;

            var tmbCurrent = GetTmbPath(keyCurrent);
            var tmbNew = GetTmbPath(keyNew);

            var papCurrent = GetPapPath(keyCurrent);
            var papNew = GetPapPath(keyNew);

            var swapTmb = FileExists(tmbCurrent) && FileExists(tmbNew);
            var swapPap = FileExists(papCurrent) && FileExists(papNew);

            if (!swapPap || !swapTmb) {
                return;
            }

            dict[keyCurrent] = new SwapMapping
            {
                OldTmb = tmbCurrent,
                OldPap = papCurrent,
                NewTmb = tmbNew,
                NewPap = papNew,
                UniqueId = uniqueId
            };
        }

        public Dictionary<string, byte[]> RemoveConflicts(Dictionary<string, SwapMapping> mappings) {
            Dictionary<string, byte[]> ret = new();

            foreach(var entry in mappings) {
                Dictionary<string, string> entryMapping = new();

                var newPap = PluginInterface.Data.GetFile(entry.Value.NewPap);
                var papString = Encoding.UTF8.GetString(newPap.Data);
                MatchCollection papMatches = rx.Matches(papString);

                int idx = 0;
                foreach (Match m in papMatches) {
                    var match = m.Value;
                    if (entryMapping.ContainsKey(match)) continue;

                    var oldSuffix = match.Replace("cbbm_","");
                    var targetLength = oldSuffix.Length;

                    var newSuffix = (entry.Value.UniqueId + idx).PadLeft(targetLength, '0');
                    var cuttOff = newSuffix.Length - targetLength;
                    newSuffix = newSuffix.Substring(cuttOff, targetLength);

                    entryMapping[match] = "cbbm_" + newSuffix;

                    PluginLog.Log($"{match} {entryMapping[match]}");

                    idx++;
                }

                var newTmb = PluginInterface.Data.GetFile(entry.Value.NewTmb);

                ret[entry.Value.OldPap] = ReplaceAll(newPap.Data, entryMapping);
                ret[entry.Value.OldTmb] = ReplaceAll(newTmb.Data, entryMapping);
            }

            return ret;
        }

        private byte[] ReplaceAll(byte[] data, Dictionary<string, string> mapping) {
            var ret = data.ToArray();
            foreach (var entry in mapping) {
                var match = Encoding.ASCII.GetBytes(entry.Key);
                var replace = Encoding.ASCII.GetBytes(entry.Value);

                for (int i = 0; i < ret.Length; i++) {
                    if(!IsMatch(ret, i, match)) {
                        continue;
                    }
                    Buffer.BlockCopy(replace, 0, ret, i, replace.Length);
                }
            }
            return ret;
        }

        private static bool IsMatch(byte[] array, int position, byte[] candidate) {
            if (candidate.Length > (array.Length - position))
                return false;

            for (int i = 0; i < candidate.Length; i++)
                if (array[position + i] != candidate[i])
                    return false;

            return true;
        }
    }
}
