using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;

namespace Doorbell; 

public class ConfigWindow : Window {
    public ConfigWindow(string name) : base(name, ImGuiWindowFlags.AlwaysAutoResize) {
        
    }

    private void DrawAlertConfig(Alert alert) {
        ImGui.Checkbox("播放音效", ref alert.SoundEnabled);

        if (alert.SoundEnabled) {
            ImGui.SameLine();
            if (ImGuiComponents.IconButton(FontAwesomeIcon.Music)) {
                alert.PlaySound();
            }

            if (ImGui.IsItemHovered()) {
                ImGui.SetTooltip("測試音效");
            }
        } else {
            ImGui.BeginDisabled();
        }
        
        ImGui.Indent();
        if (ImGui.InputTextWithHint("##Sound File", "預設門鈴音效", ref alert.SoundFile, 512)) {
            alert.DisposeSound();
        }
        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.FolderOpen)) {
            Plugin.FileDialogManager.OpenFileDialog("選擇音效檔案", "音效檔案 (wav mp3 ogg){.wav,.mp3,.ogg}", (b, s) => {
                alert.SoundFile = s;
            });
        }
        ImGui.SameLine();
        ImGui.Text("音效檔案");

        if (ImGui.SliderFloat("音量", ref alert.SoundVolume, 0, alert.SoundFile.IsHttpUrl() ? 1 : MathF.Max(3, alert.SoundVolume + 0.01f))) {
            if (alert.SoundVolume < 0) alert.SoundVolume = 0;
            if (alert.SoundVolume > 1 && alert.SoundFile.IsHttpUrl()) alert.SoundVolume = 1;
            alert.DisposeSound();
        }
        
        ImGui.Unindent();
        if (!alert.SoundEnabled) ImGui.EndDisabled();
        
        ImGui.Checkbox("顯示聊天訊息", ref alert.ChatEnabled);
        ImGui.Indent();
        if (!alert.ChatEnabled) ImGui.BeginDisabled();
        ImGui.InputText("訊息", ref alert.ChatFormat, 200);
        if (ImGui.IsItemHovered()) { 
            ImGui.BeginTooltip();
            ImGui.Text("使用 <link> 插入可點選的玩家連結。");
            ImGui.Text("使用 <name> 插入玩家名稱。");
            ImGui.Text("使用 <world> 插入玩家所在的世界。");
            ImGui.EndTooltip();
        }
        if (!alert.ChatEnabled) ImGui.EndDisabled();
        ImGui.Unindent();
    }
    
    private int silenceMinutesSlider;
    public override void Draw() {
        if (Plugin.Silenced) {
            ImGui.PushStyleColor(ImGuiCol.FrameBg, ImGui.ColorConvertFloat4ToU32(ImGuiColors.DalamudRed) & 0x55FFFFFF);
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            var timeRemaining = (Plugin.SilenceTimeSpan == null ? TimeSpan.Zero : Plugin.SilenceTimeSpan - Plugin.SilencedFor.Elapsed).Value;
            var str = string.Join(':', new[] { timeRemaining.Days, timeRemaining.Hours, timeRemaining.Minutes, timeRemaining.Seconds }.Select(i => $"{i:00}")).TrimStart('0', ':');
            ImGui.DragInt("##silenceMinutes", ref silenceMinutesSlider, 0, 0, 0, Plugin.SilenceTimeSpan == null ? "已靜音，直到離開房屋。" : $"已靜音 {str}", ImGuiSliderFlags.NoInput);
            ImGui.PopStyleColor();
            if (ImGui.Button("取消門鈴靜音", new Vector2(ImGui.GetContentRegionAvail().X, 24 * ImGuiHelpers.GlobalScale))) {
                Plugin.UnSilence();
            }
        } else {
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            var timeSpan = TimeSpan.FromMinutes(silenceMinutesSlider);
            var str = string.Join(':', new[] { timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds }.Select(i => $"{i:00}")).TrimStart('0', ':');
            ImGui.SliderInt("##silenceMinutes", ref silenceMinutesSlider, 0, Math.Max(60, silenceMinutesSlider + 1), silenceMinutesSlider <= 0 ? "直到離開房屋" : $"持續 {str}", ImGuiSliderFlags.NoInput);
            if (ImGui.Button("將門鈴靜音", new Vector2(ImGui.GetContentRegionAvail().X, 24 * ImGuiHelpers.GlobalScale))) {
                if (silenceMinutesSlider <= 0) {
                    Plugin.Silence();
                } else {
                    Plugin.Silence(TimeSpan.FromMinutes(silenceMinutesSlider));
                }
            }
        }

        ImGui.Separator();
        ImGui.PushID("Doorbell_Config_Entered");;
        ImGui.Text("玩家進入房屋時：");
        ImGui.Indent();
        DrawAlertConfig(Plugin.Config.Entered);
        ImGui.Unindent();
        
        ImGui.Separator();
        
        ImGui.PushID("Doorbell_Config_Left");
        ImGui.Text("玩家離開房屋時：");
        ImGui.Indent();
        DrawAlertConfig(Plugin.Config.Left);
        ImGui.Unindent();
        ImGui.PopID();
        
        ImGui.Separator();
        
        ImGui.PushID("Doorbell_Config_AlreadyHere");
        ImGui.Text("進入已有玩家在內的房屋時：");
        ImGui.Indent();
        DrawAlertConfig(Plugin.Config.AlreadyHere);
        ImGui.Unindent();
        ImGui.PopID();
        
        ImGui.Separator();

        if (ImGui.Button("儲存")) {
            Plugin.PluginInterface.SavePluginConfig(Plugin.Config);
        }

        ImGui.SameLine();
        if (ImGui.Button("儲存並關閉")) {
            Plugin.PluginInterface.SavePluginConfig(Plugin.Config);
            IsOpen = false;
        }
    }
}
