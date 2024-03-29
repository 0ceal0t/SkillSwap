﻿using Dalamud.Interface;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Plugin;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

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
                if (ImGui.Button("Export to Textools")) {
                    ExportTextools();
                }
                ImGui.SameLine();
                if (ImGui.Button("Export to Penumbra")) {
                    ExportPenumbra();
                }

                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.9f, 0.1f, 0.1f, 1.0f));
                ImGui.TextWrapped("DO NOT modify movement abilities (dashes, backflips, etc.)");
                ImGui.PopStyleColor();

                ImGui.TextWrapped("Note: some skill combinations cannot be swapped");

                ImGui.InputText("Name", ref ModName, 100);
                ImGui.InputText("Author", ref ModAuthor, 100);
                ImGui.InputText("Version", ref ModVersion, 100);
                if (ImGui.InputText("Save Location", ref Config.SaveLocation, 200)) {
                    Config.Save();
                }

                var space = ImGui.GetContentRegionAvail();
                ImGui.BeginChild("ItemChild", space, true);

                foreach (var item in Swaps) {
                    item.Draw();
                }
                Swaps.RemoveAll(x => x.ToDelete);

                if (ImGui.Button("+ NEW")) {
                    Swaps.Add(new Swap(Services.PluginInterface));
                }

                ImGui.EndChild();

                ImGui.End();
            }
        }

        public class Swap {
            private static int IDX = 0;

            public bool ToDelete = false;

            private readonly string Id;
            private readonly ActionSelect _Current;
            private readonly ActionSelect _New;

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

            private readonly DalamudPluginInterface PluginInterface;
            private readonly string Text;
            private readonly string Id;
            public IDalamudTextureWrap Icon;

            public SwapItem SearchSelect = null;

            private string SelectedText = "[NONE]";
            public SwapItem Selected = null;

            private string SearchText = "";
            private List<SwapItem> _Searched = null;
            private List<SwapItem> Searched => _Searched ?? AllActions;

            public ActionSelect(string text, DalamudPluginInterface pluginInterface) {
                Text = text;
                PluginInterface = pluginInterface;
                Id = "##" + (IDX++).ToString();
            }

            public void Draw() {
                ImGui.Text(Text);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(200f);
                if (ImGui.BeginCombo(Id, SelectedText, ImGuiComboFlags.HeightLargest)) {

                    var ResetScroll = false;
                    if (ImGui.InputText("Search" + Id, ref SearchText, 100)) {
                        if (SearchText.Length == 0) {
                            _Searched = null;
                        }
                        else {
                            _Searched = AllActions.Where(x => x.Name.ToLower().Contains(SearchText.ToLower())).ToList();
                        }
                        ResetScroll = true;
                    }

                    ImGui.BeginChild("Select" + Id, new Vector2(ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X, 200), true);

                    DisplayVisible(Searched.Count, out var preItems, out var showItems, out var postItems, out var itemHeight);
                    if (ResetScroll) { ImGui.SetScrollHereY(); };
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + preItems * itemHeight);

                    var idx = 0;
                    foreach (var item in Searched) {
                        if (idx < preItems || idx > (preItems + showItems)) { idx++; continue; }
                        if (ImGui.Selectable($"{item.Name}{Id}{item.Id}", item == SearchSelect)) {
                            SearchSelect = item;
                            LoadIcon(item.Icon);
                        }
                        idx++;
                    }

                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + postItems * itemHeight);
                    ImGui.EndChild();

                    if (SearchSelect != null) {
                        if (Icon != null) {
                            ImGui.Image(Icon.ImGuiHandle, new Vector2(24, 24));
                            ImGui.SameLine();
                        }

                        if (ImGui.Button("Select" + Id)) {
                            Selected = SearchSelect;
                            SelectedText = Selected.Name;
                        }
                        ImGui.SameLine();

                        DisplayYesNo("START", !string.IsNullOrEmpty(SearchSelect.StartKey));
                        ImGui.SameLine();

                        DisplayYesNo("END", !string.IsNullOrEmpty(SearchSelect.EndKey));
                        ImGui.SameLine();

                        DisplayYesNo("HIT", !string.IsNullOrEmpty(SearchSelect.HitKey));
                    }

                    ImGui.EndCombo();
                }
            }

            public void LoadIcon(ushort iconId) {
                Icon = Services.TextureProvider.GetIcon((uint)(iconId > 0 ? iconId : 0));
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

            private static void DisplayYesNo(string text, bool value) {
                ImGui.TextColored(value ? new Vector4(0.1f, 0.9f, 0.1f, 1.0f) : new Vector4(0.9f, 0.1f, 0.1f, 1.0f), text);
            }
        }
    }
}
