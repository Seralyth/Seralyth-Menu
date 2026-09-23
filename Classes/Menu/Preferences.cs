/*
 * United Goyim College Fund  Mods/Preferences.cs
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
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Photon.Pun;
using Seralyth.Managers;
using Seralyth.Menu;
using Seralyth.Mods;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using Valve.Newtonsoft.Json;
using static Seralyth.Menu.Main;

namespace Seralyth.Classes.Menu
{
    public static class Preferences
    {
        private const string FileName = "Israel_Preferences.json";
        private const string LegacyFileName = "Israel_Preferences.txt";
        internal static bool DisableAutoSave = false;

        private const int MinWriteIntervalMs = 250;

        public class SavedButtonState
        {
            public bool? enabled;
            public object value;
            public string rebindKey;
        }

        public class RgbColor
        {
            public byte r;
            public byte g;
            public byte b;

            public RgbColor() { }

            public RgbColor(Color c)
            {
                r = ToByte(c.r);
                g = ToByte(c.g);
                b = ToByte(c.b);
            }

            private static byte ToByte(float channel) =>
                (byte)Mathf.Clamp(Mathf.Round(channel * 255f), 0f, 255f);

            public Color32 ToColor32() => new Color32(r, g, b, 255);
        }

        public class CustomThemeData
        {
            public RgbColor backgroundFirst;
            public RgbColor backgroundSecond;
            public RgbColor buttonDisabledFirst;
            public RgbColor buttonDisabledSecond;
            public RgbColor buttonEnabledFirst;
            public RgbColor buttonEnabledSecond;
            public RgbColor textTitle;
            public RgbColor textDisabled;
            public RgbColor textEnabled;
        }

        public class PreferencesData
        {
            public Dictionary<string, SavedButtonState> buttons = new Dictionary<string, SavedButtonState>();
            public List<string> favorites = new List<string>();
            public List<string> quickActions = new List<string>();
            public List<string> skipButtons = new List<string>();
            public Dictionary<string, List<string>> modBindings = new Dictionary<string, List<string>>();
            public Dictionary<string, string> sounds = new Dictionary<string, string>();
            public bool? disableLocalSoundboard;
            public string oldId;
            public CustomThemeData customTheme;
            public Dictionary<string, object> misc = new Dictionary<string, object>();
        }

        public static string JsonPath => Path.Combine(PluginInfo.BaseDirectory, FileName);
        public static string LegacyPath => Path.Combine(PluginInfo.BaseDirectory, LegacyFileName);

        private static PreferencesData _cache;

        private static readonly Stopwatch _writeClock = Stopwatch.StartNew();
        private static long _lastWriteMs = long.MinValue / 2;
        private static bool _writePending;

        public static bool IsApplyingPreferences { get; private set; }

        public static void RunWithoutSaving(Action action)
        {
            if (action == null) return;

            bool previous = IsApplyingPreferences;
            IsApplyingPreferences = true;
            try { action(); }
            finally { IsApplyingPreferences = previous; }
        }

        private static PreferencesData BuildFullSnapshot()
        {
            var data = new PreferencesData();

            foreach (ButtonInfo[] buttonList in Buttons.buttons)
            {
                foreach (ButtonInfo b in buttonList)
                {
                    if (b.detected || b.excludeFromSave || b.label)
                        continue;

                    var state = ToSavedState(b);
                    if (state != null)
                        data.buttons[b.buttonText] = state;
                }
            }

            data.favorites = favorites.ToList();
            data.quickActions = quickActions.ToList();
            data.skipButtons = skipButtons.ToList();
            data.modBindings = ModBindings.ToDictionary(kv => kv.Key, kv => kv.Value);

            data.sounds["Button"] = SoundManager.DefaultSounds.TryGetValue("Button", out string btn) ? btn : "Default";
            data.sounds["Notification"] = SoundManager.DefaultSounds.TryGetValue("Notification", out string notif) ? notif : "None";
            data.disableLocalSoundboard = Sound.disableLocalSoundboard;
            data.customTheme = Settings.ExportCustomTheme();
            data.oldId = Important.oldId ?? "";

            data.misc["pageButtonType"] = pageButtonType;
            data.misc["themeType"] = themeType;
            data.misc["fontCycle"] = fontCycle;
            data.misc["pageSize"] = _pageSize;
            data.misc["playTime"] = (int)MathF.Ceiling(playTime);
            data.misc["userId"] = PhotonNetwork.LocalPlayer?.UserId ?? "null";

            return data;
        }

        private static SavedButtonState ToSavedState(ButtonInfo b)
        {
            bool hasValue = b.isSetting && b.value != null;
            bool hasRebind = !string.IsNullOrEmpty(b.rebindKey);
            bool hasEnabledInfo = b.isTogglable;

            if (!hasEnabledInfo && !hasValue && !hasRebind)
                return null;

            return new SavedButtonState
            {
                enabled = b.enabled,
                value = b.value,
                rebindKey = b.rebindKey
            };
        }

        private static void WriteToDisk(PreferencesData data)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(JsonPath, json);

            _lastWriteMs = _writeClock.ElapsedMilliseconds;
            _writePending = false;
        }

        private static void FlushNow()
        {
            if (_cache == null) return;

            try { WriteToDisk(_cache); }
            catch (Exception e) { LogManager.Log("Error writing preferences: " + e.Message); }
        }

        private static void RequestWrite()
        {
            if (_cache == null || DisableAutoSave) return;

            long elapsed = _writeClock.ElapsedMilliseconds - _lastWriteMs;
            if (elapsed >= MinWriteIntervalMs)
            {
                FlushNow();
            }
            else
            {
                _writePending = true;
            }
        }

        public static void FlushPendingWrites()
        {
            if (_writePending)
                FlushNow();
        }

        public static void Save()
        {
            if (IsApplyingPreferences) return;

            try
            {
                _cache = BuildFullSnapshot();
                FlushNow();
            }
            catch (Exception e) { LogManager.Log("Error saving preferences: " + e.Message); }
        }

        public static void SaveButton(ButtonInfo b)
        {
            if (IsApplyingPreferences) return;
            if (b == null || b.detected || b.excludeFromSave)
                return;

            try
            {
                _cache ??= BuildFullSnapshot();

                var state = ToSavedState(b);
                if (state != null)
                    _cache.buttons[b.buttonText] = state;
                else
                    _cache.buttons.Remove(b.buttonText);

                RequestWrite();
            }
            catch (Exception e) { LogManager.Log($"Error saving button '{b.buttonText}': " + e.Message); }
        }

        public static void SaveCustomTheme(CustomThemeData theme)
        {
            if (IsApplyingPreferences) return;

            try
            {
                _cache ??= BuildFullSnapshot();
                _cache.customTheme = theme;
                FlushNow();
            }
            catch (Exception e) { LogManager.Log("Error saving custom theme: " + e.Message); }
        }

        public static CustomThemeData GetCustomTheme() => _cache?.customTheme;

        public static string ExportToText() =>
            JsonConvert.SerializeObject(BuildFullSnapshot(), Formatting.None);

        public static void ImportFromText(string json)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<PreferencesData>(json);
                Apply(data);
                Save();
            }
            catch (Exception e) { LogManager.Log("Error importing preferences from text: " + e.Message); }
        }

        public static void Load()
        {
            try
            {
                if (!File.Exists(JsonPath))
                {
                    if (File.Exists(LegacyPath))
                    {
                        MigrateLegacy();
                        hasLoadedPreferences = true;
                        return;
                    }

                    hasLoadedPreferences = true;
                    return;
                }

                string json = File.ReadAllText(JsonPath);
                var data = JsonConvert.DeserializeObject<PreferencesData>(json);
                Apply(data);
            }
            catch (Exception e) { LogManager.Log("Error loading preferences: " + e.Message); }

            hasLoadedPreferences = true;
        }

        private static void Apply(PreferencesData data)
        {
            if (data == null)
            {
                LogManager.Log("preferences not found!");
                return;
            }

            var stale = new List<string>();
            RunWithoutSaving(() =>
            {
                try { Settings.Panic(); }
                catch (Exception e) { LogManager.Log("error resetting menu: " + e.Message); }

                foreach (KeyValuePair<string, SavedButtonState> kv in data.buttons ?? new Dictionary<string, SavedButtonState>())
                {
                    ButtonInfo b = Buttons.GetIndex(kv.Key);
                    if (b == null)
                    {
                        LogManager.LogError($"could not find button '{kv.Key}', so we just gonna remove it from preferences.");
                        stale.Add(kv.Key);
                        continue;
                    }

                    try
                    {
                        if (!string.IsNullOrEmpty(kv.Value.rebindKey))
                            b.rebindKey = kv.Value.rebindKey;

                        if (kv.Value.enabled == true && b.isTogglable && !b.enabled && !b.label)
                            Toggle(b.buttonText);

                        if (b.isSetting && kv.Value.value != null)
                        {
                            b.value = kv.Value.value;
                            b.onValueChanged?.Invoke();
                        }
                    }
                    catch (Exception e)
                    {
                        LogManager.Log($"Failed to restore button '{kv.Key}' (value type: {kv.Value.value?.GetType().Name}, value: {kv.Value.value}): {e.Message}");
                    }
                }

                foreach (var key in stale)
                    data.buttons.Remove(key);

                try
                {
                    favorites.Clear();
                    favorites.AddRange(data.favorites ?? new List<string>());

                    quickActions.Clear();
                    quickActions.AddRange(data.quickActions ?? new List<string>());

                    skipButtons.Clear();
                    skipButtons.AddRange(data.skipButtons ?? new List<string>());
                }
                catch (Exception e) { LogManager.Log("Error restoring favorites/quickActions/skipButtons: " + e.Message); }

                try
                {
                    ModBindings.Clear();
                    foreach (var kv in data.modBindings ?? new Dictionary<string, List<string>>())
                        ModBindings[kv.Key] = kv.Value;
                }
                catch (Exception e) { LogManager.Log("Error restoring mod bindings: " + e.Message); }

                try
                {
                    RestoreSound("Change Button Sound", "Button", data.sounds);
                    RestoreSound("Change Notification Sound", "Notification", data.sounds);

                    if (data.disableLocalSoundboard.HasValue)
                        Sound.disableLocalSoundboard = data.disableLocalSoundboard.Value;

                    ButtonInfo disableLocalBtn = Buttons.GetIndex("Disable Local Soundboard");
                    if (disableLocalBtn != null)
                        disableLocalBtn.enabled = Sound.disableLocalSoundboard;
                }
                catch (Exception e) { LogManager.Log("Error restoring sound settings: " + e.Message); }

                try
                {
                    if (!string.IsNullOrEmpty(data.oldId))
                        Important.oldId = data.oldId;
                }
                catch (Exception e) { LogManager.Log("Error restoring oldId: " + e.Message); }

                try
                {
                    if (data.misc != null)
                    {
                        if (data.misc.TryGetValue("pageButtonType", out object pbt)) pageButtonType = SafeInt(pbt, pageButtonType);
                        if (data.misc.TryGetValue("themeType", out object tt)) themeType = SafeInt(tt, themeType);
                        if (data.misc.TryGetValue("fontCycle", out object fc)) fontCycle = SafeInt(fc, fontCycle);
                        if (data.misc.TryGetValue("pageSize", out object ps)) _pageSize = SafeInt(ps, _pageSize);
                        if (data.misc.TryGetValue("playTime", out object pt)) playTime = SafeInt(pt, (int)playTime);

                        if (data.misc.TryGetValue("userId", out object uid) && uid is string uidStr && !string.IsNullOrEmpty(uidStr) && uidStr != "null")
                            Important.oldId = uidStr;
                    }
                }
                catch (Exception e) { LogManager.Log("Error restoring misc settings: " + e.Message); }

                try
                {
                    if (data.customTheme != null)
                        Settings.ApplyTheme(data.customTheme);
                }
                catch (Exception e) { LogManager.Log("Error applying custom theme: " + e.Message); }
            });

            try
            {
                var liveSnapshot = BuildFullSnapshot();
                var merged = new PreferencesData
                {
                    buttons = new Dictionary<string, SavedButtonState>(liveSnapshot.buttons)
                };

                if (data.buttons != null)
                {
                    foreach (var kv in data.buttons)
                    {
                        if (Buttons.GetIndex(kv.Key) != null && !merged.buttons.ContainsKey(kv.Key))
                            merged.buttons[kv.Key] = kv.Value;
                    }
                }

                merged.favorites = liveSnapshot.favorites;
                merged.quickActions = liveSnapshot.quickActions;
                merged.skipButtons = liveSnapshot.skipButtons;
                merged.modBindings = liveSnapshot.modBindings;
                merged.sounds = liveSnapshot.sounds;
                merged.disableLocalSoundboard = liveSnapshot.disableLocalSoundboard;
                merged.customTheme = liveSnapshot.customTheme;
                merged.oldId = liveSnapshot.oldId;
                merged.misc = liveSnapshot.misc;

                _cache = merged;
            }
            catch (Exception e)
            {
                LogManager.Log("Error applying preferences: " + e.Message);
            }
        }

        private static int SafeInt(object value, int fallback)
        {
            try { return Convert.ToInt32(value); }
            catch { return fallback; }
        }

        private static void RestoreSound(string buttonName, string soundKey, Dictionary<string, string> sounds)
        {
            if (sounds == null || !sounds.TryGetValue(soundKey, out string saved) || string.IsNullOrEmpty(saved))
                return;

            SoundManager.DefaultSounds[soundKey] = saved;

            ButtonInfo button = Buttons.GetIndex(buttonName);
            if (button != null)
                button.overlapText = $"{buttonName} <color=grey>[</color><color=green>{saved}</color><color=grey>]</color>";
        }

        private static void MigrateLegacy()
        {
            try
            {
                LogManager.Log("Migrating legacy preferences");

                string text = File.ReadAllText(LegacyPath);

                RunWithoutSaving(() => Settings.LoadPreferencesFromText(text));

                _cache = BuildFullSnapshot();
                FlushNow();

                File.Move(LegacyPath, LegacyPath + ".migrated");
            }
            catch (Exception e) { LogManager.Log("Error migrating legacy preferences: " + e.Message); }
        }
    }
}