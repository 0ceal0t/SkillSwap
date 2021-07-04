using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwap.UI {
    public class ConfirmDialog {
        private string Name;
        private string Author;
        private string Version;
        private string SaveLocation;
        private Dictionary<string, SwapMapping> Mapping;
        private Action<string, string, string, string, Dictionary<string, SwapMapping>> OnConfirm;

        private bool Visible = false;

        public ConfirmDialog() { }

        public void SetData(string name, string author, string version, string saveLocation, Dictionary<string, SwapMapping> mapping, Action<string, string, string, string, Dictionary<string, SwapMapping>> onConfirm) {
            Visible = true;

            Name = name;
            Author = author;
            Version = version;
            SaveLocation = saveLocation;
            Mapping = mapping;
            OnConfirm = onConfirm;
        }

        public void Draw() {
            if (!Visible) return;

            var _ID = "##ConfirmDialog";
            ImGui.SetNextWindowSize(new Vector2(550, 400), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("Confirm Export", ref Visible)) {
                ImGui.Text("Exporting to: ");
                ImGui.SameLine();
                ImGui.TextColored(new Vector4(0.1f, 0.9f, 0.1f, 1.0f), SaveLocation);

                var size = ImGui.GetContentRegionAvail() - new Vector2(0, ImGui.GetTextLineHeightWithSpacing() + 8);
                ImGui.BeginChild(_ID + "-Child", size, true);

                foreach(var item in Mapping) {
                    ImGui.TextWrapped($"TMB: {item.Value.OldTmb} -> {item.Value.NewTmb}");
                    ImGui.TextWrapped($"PAP: {item.Value.OldPap} -> {item.Value.NewPap}");

                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 5);
                }

                ImGui.EndChild();

                if(ImGui.Button("Export" + _ID)) {
                    Visible = false;
                    OnConfirm(Name, Author, Version, SaveLocation, Mapping);
                }

                ImGui.End();
            }
        }
    }
}
