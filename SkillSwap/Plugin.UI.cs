using Dalamud.Interface;
using Dalamud.Plugin;
using ImGuiNET;
using Lumina.Data.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwap {
    public partial class Plugin {
        private bool Visible = false;

        public List<Swap> Swaps = new();
        public string ModName = "";
        public string ModAuthor = "";
        public string ModVersion = "";

        private void Draw() {
            if (!Visible) return;

            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGui.SetNextWindowSize(new Vector2(850, 500), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("SkillSwap", ref Visible)) {
                if(ImGui.Button("Export to Textools")) {
                    ExportTextools();
                }
                ImGui.SameLine();
                if(ImGui.Button("Export to Penumbra")) {
                    ExportPenumbra();
                }

                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.9f, 0.1f, 0.1f, 1.0f));
                ImGui.TextWrapped("DO NOT modify movement abilities (dashes, backflips, etc.)");
                ImGui.PopStyleColor();

                ImGui.InputText("Name", ref ModName, 100);
                ImGui.InputText("Author", ref ModAuthor, 100);
                ImGui.InputText("Version", ref ModVersion, 100);
                if(ImGui.InputText("Save Location", ref Config.SaveLocation, 200)) {
                    Config.Save();
                }

                var space = ImGui.GetContentRegionAvail();
                ImGui.BeginChild("ItemChild", space, true);

                foreach(var item in Swaps) {
                    item.Draw();
                }
                Swaps.RemoveAll(x => x.ToDelete);

                if(ImGui.Button("+ NEW")) {
                    Swaps.Add(new Swap(PluginInterface));
                }

                ImGui.EndChild();

                ImGui.End();
            }
        }

        public class Swap {
            private static int IDX = 0;

            private string Id;

            public bool ToDelete = false;

            private ActionSelect _Current;
            private ActionSelect _New;

            public SwapItem Current => _Current.Selected;
            public SwapItem New => _New.Selected;

            public Swap(DalamudPluginInterface pluginInterface) {
                Id = "##" + (IDX++).ToString();

                _Current = new ActionSelect("Current", pluginInterface);
                _New = new ActionSelect("New", pluginInterface);
            }

            public void Draw() {
                _Current.Draw();
                ImGui.SameLine();
                _New.Draw();
                ImGui.SameLine();

                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.80f, 0.10f, 0.10f, 1.0f));
                if (ImGui.Button($"{(char)FontAwesomeIcon.Trash}" + Id)) {
                    Delete();
                }
                ImGui.PopStyleColor();
                ImGui.PopFont();
            }

            private void Delete() {
                ToDelete = true;
                _Current.Icon?.Dispose();
                _New.Icon?.Dispose();
            }
        }

        public class ActionSelect {
            private static int IDX = 0;

            private DalamudPluginInterface PluginInterface;
            private string Text;
            private string Id;
            public ImGuiScene.TextureWrap Icon;

            public SwapItem SearchSelect = null;

            private string SelectedText = "[NONE]";
            public SwapItem Selected = null;

            private string SearchText = "";
            private List<SwapItem> _Searched = null;
            private List<SwapItem> Searched => _Searched == null ? AllActions : _Searched;

            public ActionSelect(string text, DalamudPluginInterface pluginInterface) {
                Text = text;
                PluginInterface = pluginInterface;
                Id = "##" + (IDX++).ToString();
            }

            public void Draw() {
                ImGui.Text(Text);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(200f);
                if(ImGui.BeginCombo(Id, SelectedText, ImGuiComboFlags.HeightLargest)) {

                    bool ResetScroll = false;
                    if (ImGui.InputText("Search" + Id, ref SearchText, 100)) {
                        if(SearchText.Length == 0) {
                            _Searched = null;
                        }
                        else {
                            _Searched = AllActions.Where(x => x.Name.ToLower().Contains(SearchText.ToLower())).ToList();
                        }
                        ResetScroll = true;
                    }

                    ImGui.BeginChild("Select" + Id, new Vector2(ImGui.GetWindowContentRegionWidth(), 200), true);

                    DisplayVisible(Searched.Count, out int preItems, out int showItems, out int postItems, out float itemHeight);
                    if (ResetScroll) { ImGui.SetScrollHereY(); };
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + preItems * itemHeight);

                    int idx = 0;
                    foreach (var item in Searched) {
                        if (idx < preItems || idx > (preItems + showItems)) { idx++; continue; }
                        if(ImGui.Selectable($"{item.Name}{Id}{item.Id}", item == SearchSelect)) {
                            SearchSelect = item;
                            LoadIcon(item.Icon);
                        }
                        idx++;
                    }

                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + postItems * itemHeight);
                    ImGui.EndChild();

                    if(SearchSelect != null) {
                        if (Icon != null) {
                            ImGui.Image(Icon.ImGuiHandle, new Vector2(22, 22));
                            ImGui.SameLine();
                        }

                        if (ImGui.Button("Select" + Id)) {
                            Selected = SearchSelect;
                            SelectedText = Selected.Name;
                        }
                    }

                    ImGui.EndCombo();
                }
            }

            public void LoadIcon(ushort iconId) {
                Icon?.Dispose();
                Icon = null;
                if (iconId > 0) {
                    TexFile tex;
                    try {
                        tex = PluginInterface.Data.GetIcon(iconId);
                    }
                    catch(Exception) {
                        tex = PluginInterface.Data.GetIcon(0);
                    }
                    Icon = PluginInterface.UiBuilder.LoadImageRaw(BGRA_to_RGBA(tex.ImageData), tex.Header.Width, tex.Header.Height, 4);
                }
            }

            public static byte[] BGRA_to_RGBA(byte[] data) {
                byte[] ret = new byte[data.Length];
                for (int i = 0; i < data.Length / 4; i++) {
                    var idx = i * 4;
                    ret[idx + 0] = data[idx + 2];
                    ret[idx + 1] = data[idx + 1];
                    ret[idx + 2] = data[idx + 0];
                    ret[idx + 3] = data[idx + 3];
                }
                return ret;
            }

            public static void DisplayVisible(int count, out int preItems, out int showItems, out int postItems, out float itemHeight) {
                float childHeight = 200;
                var scrollY = ImGui.GetScrollY();
                var style = ImGui.GetStyle();
                itemHeight = ImGui.GetTextLineHeight() + style.ItemSpacing.Y;
                preItems = (int)Math.Floor(scrollY / itemHeight);
                showItems = (int)Math.Ceiling(childHeight / itemHeight);
                postItems = count - showItems - preItems;
            }
        }
    }
}
