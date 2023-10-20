using Dalamud.Game.Command;
using Dalamud.Plugin;
using SkillSwap.UI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SkillSwap {
    public partial class Plugin : IDalamudPlugin {
        public string Name => "SkillSwap";
        private const string commandName = "/skillswap";

        public static List<SwapItem> AllActions { get; private set; }

        private readonly Configuration Config;
        public string AssemblyLocation { get => assemblyLocation; set => assemblyLocation = value; }
        private string assemblyLocation = Assembly.GetExecutingAssembly().Location;
        private readonly ConfirmDialog Confirm;

        public Plugin(DalamudPluginInterface pluginInterface) {
            pluginInterface.Create<Services>();

            AllActions = new();

            Config = Services.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Config.Initialize(Services.PluginInterface);

            Services.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand) {
                HelpMessage = "Open mod creation menu"
            });

            Confirm = new ConfirmDialog();

            Init();

            Services.PluginInterface.UiBuilder.Draw += Draw;
            Services.PluginInterface.UiBuilder.Draw += Confirm.Draw;
            Services.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        private void Init() {
            var sheet = Services.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>().Where(x => !string.IsNullOrEmpty(x.Name) && !x.AffectsPosition);
            foreach (var item in sheet) {
                var startKey = item.AnimationStart?.Value?.Name?.Value?.Key.ToString();
                var endKey = item.AnimationEnd?.Value?.Key.ToString();
                var hitKey = item.ActionTimelineHit?.Value?.Key.ToString();

                var endValid = SwapItem.ValidKey(endKey);
                var startValid = SwapItem.ValidKey(startKey);
                var hitValid = SwapItem.ValidKey(hitKey) && !hitKey.Contains("normal_hit");

                AllActions.Add(new SwapItem {
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
            Services.PluginInterface.UiBuilder.Draw -= Draw;
            Services.PluginInterface.UiBuilder.Draw -= Confirm.Draw;
            Services.PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;

            Services.CommandManager.RemoveHandler(commandName);
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

        public string StartKey;
        public string EndKey;
        public string HitKey;

        public static string[] ValidStart = new[] { "ws", "limitbreak", "rol_common", "magic", "ability", "craft", "gather" };

        public static bool ValidKey(string key) {
            if (string.IsNullOrEmpty(key)) return false;
            foreach (var valid in ValidStart) {
                if (key.StartsWith(valid)) return true;
            }
            return false;
        }
    }
}
