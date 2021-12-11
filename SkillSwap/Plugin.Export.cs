using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SkillSwap {
    public struct SwapMapping {
        public string OldTmb;
        public string OldPap;
        public string NewTmb;
        public string NewPap;

        public string UniqueId;
        public bool SwapPap;
        public bool NoPap;
    }

    public partial class Plugin {
        public static readonly Regex rx = new(@"cbbm(_[a-zA-Z0-9]+)+", RegexOptions.Compiled);
        private static readonly Random random = new Random();

        public static string GetTmbPath(string key) {
            return "chara/action/" + key + ".tmb";
        }

        public static string GetPapPath(string key) {
            // ability/2gl_astro/abl023 -> chara/human/c0101/animation/a0001/bt_common/ability/2gl_astro/abl023.pap
            // magic/2gl_astro/mgc012 -> chara/human/c0101/animation/a0001/bt_common/resident/action.pap
            // ^ not much that can be done about this one

            if (key.StartsWith("ws/")) {
                var split = key.Split('/');
                var weapon = split[1];
                return "chara/human/c0101/animation/a0001/" + weapon + "/" + key + ".pap";
            }
            return "chara/human/c0101/animation/a0001/bt_common/" + key + ".pap";
        }

        public bool FileExists(string path) {
            return DataManager.FileExists(path);
        }

        public Dictionary<string, SwapMapping> FileMapping() {
            Dictionary<string, SwapMapping> ret = new();
            foreach(var item in Swaps) {
                if (item.Current == null || item.New == null) continue;

                var uniqueId = RandomString(10);

                MapSingle(item.Current?.StartKey, item.New?.StartKey, "s" + uniqueId, ret);
                MapSingle(item.Current?.EndKey, item.New?.EndKey, "e" + uniqueId, ret);
                MapSingle(item.Current?.HitKey, item.New?.HitKey, "h" + uniqueId, ret);
            }
            foreach (var item in ret) {
                PluginLog.Log($"Replacing: {item.Value.OldTmb} With: {item.Value.NewTmb}");
                if(item.Value.SwapPap) {
                    PluginLog.Log($"Replacing: {item.Value.OldPap} With: {item.Value.NewPap}");
                }
                else {
                    PluginLog.Log("Pap not being replaced");
                }
            }
            return ret;
        }

        public void MapSingle(string keyCurrent, string keyNew, string uniqueId, Dictionary<string, SwapMapping> dict) {
            if (string.IsNullOrEmpty(keyCurrent) || string.IsNullOrEmpty(keyNew)) return;

            var tmbCurrent = GetTmbPath(keyCurrent);
            var tmbNew = GetTmbPath(keyNew);

            var papCurrent = GetPapPath(keyCurrent);
            var papNew = GetPapPath(keyNew);

            if(!FileExists(tmbCurrent) || !FileExists(tmbNew)) {
                return;
            }

            var swapPap = FileExists(papCurrent) && FileExists(papNew);
            var noPap = !FileExists(papCurrent) && !FileExists(papNew);

            dict[keyCurrent] = new SwapMapping
            {
                OldTmb = tmbCurrent,
                OldPap = papCurrent,
                NewTmb = tmbNew,
                NewPap = papNew,
                UniqueId = uniqueId,
                SwapPap = swapPap,
                NoPap = noPap,
            };
        }

        public Dictionary<string, byte[]> RemoveConflicts(Dictionary<string, SwapMapping> mappings) {
            Dictionary<string, byte[]> ret = new();

            foreach(var entry in mappings) {
                var newTmb = DataManager.GetFile(entry.Value.NewTmb);

                if(!entry.Value.SwapPap) {
                    if(entry.Value.NoPap) {
                        ret[entry.Value.OldTmb] = newTmb.Data; // whatever, just keep going
                        continue;
                    }

                    // we need to make sure that there aren't any PAP entries in the new tmb, since there isn't a new PAP file to cary over
                    Dictionary<string, string> papMapping = new();
                    papMapping.Add("C010", "0000"); // this is mega scuffed
                    ret[entry.Value.OldTmb] = ReplaceAll(newTmb.Data, papMapping);
                    continue;
                }

                // swapping PAPs, which means that we need to make the ids of the new PAP unique
                Dictionary<string, string> entryMapping = new();
                var newPap = DataManager.GetFile(entry.Value.NewPap);
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

                ret[entry.Value.OldPap] = ReplaceAll(newPap.Data, entryMapping);
                ret[entry.Value.OldTmb] = ReplaceAll(newTmb.Data, entryMapping);
            }

            return ret;
        }

        private static byte[] ReplaceAll(byte[] data, Dictionary<string, string> mapping) {
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

        public static string RandomString(int length) {
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
