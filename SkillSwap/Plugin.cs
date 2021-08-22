using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using SkillSwap.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SkillSwap {
    public partial class Plugin : IDalamudPlugin {
        public string Name => "SkillSwap";
        private const string commandName = "/skillswap";

        public static List<SwapItem> AllActions { get; private set; }

        public static DalamudPluginInterface PluginInterface { get; private set; }
        public static ClientState ClientState { get; private set; }
        public static CommandManager CommandManager { get; private set; }
        public static DataManager DataManager { get; private set; }

        private readonly Configuration Config;
        public string AssemblyLocation { get => assemblyLocation; set => assemblyLocation = value; }
        private string assemblyLocation = Assembly.GetExecutingAssembly().Location;
        private readonly ConfirmDialog Confirm;

        public Plugin(
                DalamudPluginInterface pluginInterface,
                ClientState clientState,
                CommandManager commandManager,
                DataManager dataManager
            ) {
            PluginInterface = pluginInterface;
            ClientState = clientState;
            CommandManager = commandManager;
            DataManager = dataManager;

            AllActions = new();

            Config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Config.Initialize(PluginInterface);

            CommandManager.AddHandler(commandName, new CommandInfo(OnCommand) {
                HelpMessage = "Open mod creation menu"
            });

            Confirm = new ConfirmDialog();

            Init();

            PluginInterface.UiBuilder.Draw += Draw;
            PluginInterface.UiBuilder.Draw += Confirm.Draw;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        private void Init() {
            var sheet = DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().Where(x => !string.IsNullOrEmpty(x.Name) && !x.AffectsPosition);
            foreach(var item in sheet) {
                string startKey = item.AnimationStart?.Value?.Name?.Value?.Key.ToString();
                string endKey = item.AnimationEnd?.Value?.Key.ToString();
                string hitKey = item.ActionTimelineHit?.Value?.Key.ToString();

                var endValid = SwapItem.ValidKey(endKey);
                var startValid = SwapItem.ValidKey(startKey);
                var hitValid = SwapItem.ValidKey(hitKey) && !hitKey.Contains("normal_hit");

                AllActions.Add(new SwapItem
                {
                    Id = item.RowId,
                    Icon = item.Icon,
                    Name = item.Name.ToString(),
                    
                    EndKey = endValid ? endKey : "",
                    StartKey = startValid ? startKey : "",
                    HitKey = hitValid ? hitKey : "",
                });
            }
        }

        public void Dispose() {
            PluginInterface.UiBuilder.Draw -= Draw;
            PluginInterface.UiBuilder.Draw -= Confirm.Draw;
            PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;

            CommandManager.RemoveHandler(commandName);
        }

        private void OnCommand(string command, string args) {
            Visible = true;
        }

        private void DrawConfigUI(object sender, EventArgs eventArgs) {
            Visible = true;
        }
    }

    public class SwapItem {
        public ushort Icon;
        public uint Id;
        public string Name;

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
