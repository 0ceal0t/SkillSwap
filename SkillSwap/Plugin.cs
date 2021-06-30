﻿using Dalamud.Game.Command;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SkillSwap {
    public partial class Plugin : IDalamudPlugin {
        public string Name => "SkillSwap";
        private const string commandName = "/skillswap";

        private DalamudPluginInterface PluginInterface;
        private Configuration Config;

        public static List<SwapItem> AllActions = new();
        
        public string AssemblyLocation { get => assemblyLocation; set => assemblyLocation = value; }
        private string assemblyLocation = Assembly.GetExecutingAssembly().Location;

        public void Initialize(DalamudPluginInterface pluginInterface) {
            PluginInterface = pluginInterface;

            Config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Config.Initialize(PluginInterface);

            PluginInterface.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand) {
                HelpMessage = "Open mod creation menu"
            });

            Init();

            PluginInterface.UiBuilder.OnBuildUi += Draw;
            PluginInterface.UiBuilder.OnOpenConfigUi += (sender, args) => DrawConfigUI();
        }

        private void Init() {
            var sheet = PluginInterface.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().Where(x => !string.IsNullOrEmpty(x.Name));
            foreach(var item in sheet) {
                string startKey = item.AnimationStart?.Value?.Name.ToString();
                string endKey = item.AnimationEnd?.Value?.Key.ToString();
                string hitKey = item.ActionTimelineHit?.Value?.Key.ToString();

                var endValid = SwapItem.ValidKey(endKey);
                if (!endValid) continue;

                var startValid = SwapItem.ValidKey(startKey);
                var hitValid = SwapItem.ValidKey(hitKey) && hitKey != "normal_hit/normal_hit";

                AllActions.Add(new SwapItem
                {
                    Id = item.RowId,
                    Icon = item.Icon,
                    IsPvp = item.IsPvP,
                    PlayerAction = item.IsPlayerAction,
                    Name = item.Name.ToString(),
                    
                    EndKey = endKey,
                    StartKey = startValid ? startKey : "",
                    HitKey = hitValid ? hitKey : ""
                });
            }
        }

        public void Dispose() {
            PluginInterface.CommandManager.RemoveHandler(commandName);
            PluginInterface.Dispose();
        }

        private void OnCommand(string command, string args) {
            Visible = true;
        }

        private void DrawConfigUI() {
            Visible = true;
        }
    }

    public class SwapItem {
        public ushort Icon;
        public uint Id;
        public string Name;
        public bool IsPvp;
        public bool PlayerAction;

        public string StartKey;
        public string EndKey;
        public string HitKey;

        public static string[] ValidStart = new[] { "ws", "limitbreak", "rol_common", "magic", "ability", "craft", "gather" };

        public static bool ValidKey(string key) {
            if (string.IsNullOrEmpty(key)) return false;
            foreach(var valid in ValidStart) {
                if (key.StartsWith(valid)) return true;
            }
            return false;
        }
    }
}
