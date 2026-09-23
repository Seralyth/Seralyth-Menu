/*
 * United Goyim College Fund  Mods/Presets.cs
 * A stupid shit hole i fuckjing hate this game with over 1000+ mods
 *
 * Copyright (C) 2026  robin williams
 * https://github.com/iiDk-the-inactual/UnitedGoyimCollegeFund
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>.
 */

using Seralyth.Managers;
using System;
using System.IO;
using static Seralyth.Menu.Main;

namespace Seralyth.Mods
{
    public static class Presets
    {
        public static void LegitimatePreset()
        {
            string[] presetMods = {
                "Joystick Menu",
                "Thin Menu",
                "Disable Enabled GUI",
                "Disable Board Colors",
                "Disable Disconnect Button",
                "Disable Page Buttons",
                "Disable Search Button",
                "Disable Return Button",
                "Hidden on Camera",
                "Hide Notifications on Camera",
                "Hide Text on Camera",
                "Disable FPS Counter",
                "Fix Rig Colors",
            };

            themeType = 28;
            pageButtonType = 1;
            fontCycle = -1;

            Seralyth.Menu.Buttons.GetIndex("Change Menu Theme").cycleValue(true);
            Seralyth.Menu.Buttons.GetIndex("Change Page Type").cycleValue(true);
            Seralyth.Menu.Buttons.GetIndex("Change Font Type").cycleValue(true);

            Settings.Panic();

            foreach (string mod in presetMods)
                Toggle(mod);

            NotificationManager.SendNotification("<color=grey>[</color><color=purple>PRESET</color><color=grey>]</color> Legitimate preset enabled successfully.");
        }

        public static void GhostPreset()
        {
            string[] presetMods = {
                "Ghost <color=grey>[</color><color=green>A</color><color=grey>]</color>",
                "Invisible <color=grey>[</color><color=green>B</color><color=grey>]</color>",
                "Noclip <color=grey>[</color><color=green>T</color><color=grey>]</color>",
                "Steam Long Arms",
                "Break Audio Gun",
                "No Finger Movement",
                "Platforms",
                "Change Arm Length"
            };

            Movement.longarmCycle = 2;

            Settings.Panic();

            foreach (string mod in presetMods)
                Toggle(mod);

            NotificationManager.SendNotification("<color=grey>[</color><color=purple>PRESET</color><color=grey>]</color> Ghost preset enabled successfully.");
        }

        public static void SaveCustomPreset(int id)
        {
            try
            {
                string directory = Path.Combine(PluginInfo.BaseDirectory, "SavedPresets");
                Directory.CreateDirectory(directory);

                string file = Path.Combine(directory, $"Preset_{id}.txt");
                File.WriteAllText(file, Settings.SavePreferencesToText());

                NotificationManager.SendNotification("<color=grey>[</color><color=purple>PRESET</color><color=grey>]</color> Custom preset saved successfully.");
            }
            catch (Exception ex)
            {
                LogManager.LogError($"Failed to save custom preset {id}: {ex}");
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> Failed to save custom preset.");
            }
        }

        public static void LoadCustomPreset(int id)
        {
            try
            {
                string directory = Path.Combine(PluginInfo.BaseDirectory, "SavedPresets");
                string file = Path.Combine(directory, $"Preset_{id}.txt");

                if (!File.Exists(file))
                {
                    NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> Custom preset not found.");
                    return;
                }

                string text = File.ReadAllText(file);
                LogManager.Log(text);
                Settings.LoadPreferencesFromText(text);

                NotificationManager.SendNotification("<color=grey>[</color><color=purple>PRESET</color><color=grey>]</color> Custom preset loaded successfully.");
            }
            catch (Exception ex)
            {
                LogManager.LogError($"Failed to load custom preset {id}: {ex}");
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> Failed to load custom preset.");
            }
        }

        public static void PerformancePreset()
        {
            string[] presetMods = {
                "Disable Enabled GUI",
                "Disable Board Colors",
                "Disable FPS Counter",
                "FPS Boost",
                "Disable Ghostview"
            };

            themeType = 30;
            pageButtonType = 1;
            fontCycle = 0;

            Seralyth.Menu.Buttons.GetIndex("Change Menu Theme").cycleValue(true);
            Seralyth.Menu.Buttons.GetIndex("Change Page Type").cycleValue(true);
            Seralyth.Menu.Buttons.GetIndex("Change Font Type").cycleValue(true);

            Settings.Panic();

            foreach (string mod in presetMods)
                Toggle(mod);

            NotificationManager.SendNotification("<color=grey>[</color><color=purple>PRESET</color><color=grey>]</color> Performance preset enabled successfully.");
        }

        public static void SafetyPreset()
        {
            string[] presetMods = {
                "No Finger Movement",
                "Fake Oculus Menu <color=grey>[</color><color=green>X</color><color=grey>]</color>",
                "Disable Gamemode Buttons",
                "Anti Crash",
                "Anti Moderator",
                "Anti Report <color=grey>[</color><color=green>Disconnect</color><color=grey>]</color>",
                "Show Anti Cheat Reports <color=grey>[</color><color=green>Self</color><color=grey>]</color>"
            };

            themeType = 33;
            pageButtonType = 1;
            fontCycle = 0;

            Seralyth.Menu.Buttons.GetIndex("Change Menu Theme").cycleValue(true);
            Seralyth.Menu.Buttons.GetIndex("Change Page Type").cycleValue(true);
            Seralyth.Menu.Buttons.GetIndex("Change Font Type").cycleValue(true);

            Settings.Panic();

            foreach (string mod in presetMods)
                Toggle(mod);

            NotificationManager.SendNotification("<color=grey>[</color><color=purple>PRESET</color><color=grey>]</color> Safety preset enabled successfully.");
        }

        public static void SimplePreset()
        {
            string[] presetMods = {
                "Disable Enabled GUI",
                "Accept TOS",
                "Player Scale Menu",
                "PC Button Click"
            };

            pageButtonType = 2;
            Seralyth.Menu.Buttons.GetIndex("Change Page Type").cycleValue(true);

            foreach (string mod in presetMods)
                Toggle(mod);

            NotificationManager.SendNotification("<color=grey>[</color><color=purple>PRESET</color><color=grey>]</color> Simple preset enabled successfully.");
        }
    }
}
