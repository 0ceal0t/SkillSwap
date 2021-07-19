using Dalamud.Interface;
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

        private HashSet<string> Selected;

        public ConfirmDialog() { }

        public void SetData(string name, string author, string version, string saveLocation, Dictionary<string, SwapMapping> mapping, Action<string, string, string, string, Dictionary<string, SwapMapping>> onConfirm) {
            Visible = true;

            Name = name;
            Author = author;
            Version = version;
            SaveLocation = saveLocation;
            Mapping = mapping;
            OnConfirm = onConfirm;

            Selected = new();
            foreach(var item in Mapping) {
                Selected.Add(item.Key);
            }
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
                ImGui.PushTextWrapPos(size.X);
                ImGui.BeginChild(_ID + "-Child", size, true);

                foreach(var item in Mapping) {
                    var selected = Selected.Contains(item.Key);
                    if(ImGui.Checkbox($"{item.Key}{_ID}", ref selected)) {
                        if(selected) {
                            Selected.Add(item.Key);
                        }
                        else {
                            Selected.Remove(item.Key);
                        }
                    }

                    PrintLine(item.Value.OldTmb, item.Value.NewTmb);
                    if(item.Value.SwapPap) {
                        PrintLine(item.Value.OldPap, item.Value.NewPap);
                    }
                    else if(!item.Value.NoPap) {
                        ImGui.TextWrapped("This .tmb will have its animations stripped (if it has any) because .pap files to swap could not be found. VFXs and sounds will be left intact.");
                    }
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 5);
                }

                ImGui.EndChild();
                ImGui.PopTextWrapPos();

                if(ImGui.Button("Export" + _ID)) {
                    Visible = false;
                    OnConfirm(Name, Author, Version, SaveLocation, Mapping.Where(item => Selected.Contains(item.Key)).ToDictionary(item => item.Key, item => item.Value));
                }

                ImGui.End();
            }
        }

        private void PrintLine(string currentValue, string newValue) {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.9f, 0.1f, 0.1f, 1.0f));
            ImGui.TextWrapped(currentValue);
            ImGui.PopStyleColor();

            ImGui.Text($">  ");
            ImGui.SameLine();

            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.1f, 0.9f, 0.1f, 1.0f));
            ImGui.TextWrapped(newValue);
            ImGui.PopStyleColor();
        }
    }
}
