/*
 * United Goyim College Fund  Managers/SoundManager.cs
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
using Seralyth.Menu;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Seralyth.Menu.Main;
using static Seralyth.Utilities.AssetUtilities;

namespace Seralyth.Managers
{
    public class SoundManager
    {
        public static readonly Dictionary<string, Dictionary<string, object>> Sounds = new Dictionary<string, Dictionary<string, object>>
        {
            ["Buttons"] = new Dictionary<string, object>
            {
                { "Wood", 9 },
                { "Keyboard", 66 },
                { "Default", 67 },
                { "Bubble", 84 },
                { "Steal", "Audio/Menu/Buttons/steal.ogg" },
                { "Anthrax", "Audio/Menu/Buttons/anthrax.ogg" },
                { "Lever", "Audio/Menu/Buttons/lever.ogg" },
                { "Minecraft", "Audio/Menu/Buttons/minecraft.ogg" },
                { "Rec Room", "Audio/Menu/Buttons/lever$1.ogg" },
                { "Watch", "Audio/Menu/Buttons/watch.ogg" },
                { "Membrane", "Audio/Menu/Buttons/membrane.ogg" },
                { "Jar", 106 },
                { "Slider", "Audio/Menu/Buttons/slider.ogg" },
                { "Can", "Audio/Menu/Buttons/can.ogg" },
                { "Cut", "Audio/Menu/Buttons/cut.ogg" },
                { "Creamy", "Audio/Menu/Buttons/creamy.ogg" },
                { "Roblox Button", "Audio/Menu/Buttons/robloxbutton.ogg" },
                { "Roblox Tick", "Audio/Menu/Buttons/robloxtick.ogg" },
                { "Mouse", "Audio/Menu/Buttons/mouse.ogg" },
                { "Valve", "Audio/Menu/Buttons/valve.ogg" },
                { "Nintendo", "Audio/Menu/Buttons/nintendo.ogg" },
                { "Windows", "Audio/Menu/Buttons/windows.ogg" },
                { "Destiny", "Audio/Menu/Buttons/destiny.ogg" },
                { "Untitled", "Audio/Menu/Buttons/untitled.ogg" },
                { "Slap", 338 },
                { "Dog", "Audio/Menu/Buttons/dog.ogg" },
                { "GMod Spawn", "Audio/Menu/Buttons/gmod.ogg" },
                { "GMod Undo", "Audio/Menu/Buttons/undo.ogg" },
                { "Half Life", "Audio/Menu/Buttons/hl1.ogg" },
                { "Mine", "Audio/Menu/Buttons/mine.ogg" },
                { "Sensation", "Audio/Menu/Buttons/sensation.ogg" }
            },

            ["Menu"] = new Dictionary<string, object>
            {
                { "Next", "Audio/Menu/next.ogg" },
                { "Previous", "Audio/Menu/prev.ogg" },
                { "Up", "Audio/Menu/up.ogg" },
                { "Down", "Audio/Menu/down.ogg" },
                { "Open", "Audio/Menu/open.ogg" },
                { "Close", "Audio/Menu/close.ogg" },
                { "Select", "Audio/Menu/select.ogg" },
                { "Achievement", "Audio/Menu/achievement.ogg" },
                { "Admin", "Audio/Menu/admin.ogg" },
                { "Patreon", "Audio/Menu/patreon.ogg" }
            },

            ["Notifications"] = new Dictionary<string, object>
            {
                { "None", "" },
                { "Pop", "Audio/Menu/Notifications/pop.ogg" },
                { "Ding", "Audio/Menu/Notifications/ding.ogg" },
                { "Twitter", "Audio/Menu/Notifications/twitter.ogg" },
                { "Discord", "Audio/Menu/Notifications/discord.ogg" },
                { "Whatsapp", "Audio/Menu/Notifications/whatsapp.ogg" },
                { "Grindr", "Audio/Menu/Notifications/grindr.ogg" },
                { "iOS", "Audio/Menu/Notifications/ios.ogg" },
                { "XP Notify", "Audio/Menu/Notifications/xpnotify.ogg" },
                { "XP Ding", "Audio/Menu/Notifications/xptrueding.ogg" },
                { "XP Question", "Audio/Menu/Notifications/xpding.ogg" },
                { "XP Error", "Audio/Menu/Notifications/xperror.ogg" },
                { "Roblox Bass", "Audio/Menu/Notifications/robloxbass.ogg" },
                { "Oculus", "Audio/Menu/Notifications/oculus.ogg" },
                { "Nintendo", "Audio/Menu/Notifications/nintendo.ogg" },
                { "Telegram", "Audio/Menu/Notifications/telegram.ogg" },
                { "7 Ding", "Audio/Menu/Notifications/win7-ding.ogg" },
                { "7 Error", "Audio/Menu/Notifications/win7-error.ogg" },
                { "7 Exclamation", "Audio/Menu/Notifications/win7-exc.ogg" },
                { "AOL Alert", "Audio/Menu/Notifications/aol-alert.ogg" },
                { "AOL Message", "Audio/Menu/Notifications/aol-msg.ogg" },
                { "Thunderbird", "Audio/Menu/Notifications/thunderbird.ogg" },
                { "Pixie Dust", "Audio/Menu/Notifications/pixiedust.ogg" },
                { "Moon Beam", "Audio/Menu/Notifications/moonbeam.ogg" },
                { "Dog", "Audio/Menu/Notifications/dog.ogg" },
                { "GMod Error", "Audio/Menu/Notifications/gmod-error.ogg" }
            }
        };

        public static Dictionary<string, string> DefaultSounds = new Dictionary<string, string>
        {
            { "Button", "Default" },
            { "Notification", "None" },
            { "Next", "Next" },
            { "Previous", "Previous" },
            { "Up", "Up" },
            { "Down", "Down" },
            { "Open", "Open" },
            { "Close", "Close" },
            { "Select", "Select" },
            { "Return", "Default" }
        };

        public static readonly Dictionary<string, Dictionary<string, object>> Soundpacks = new Dictionary<string, Dictionary<string, object>>
        {
            ["None"] = null,

            ["SteamVR"] = new Dictionary<string, object>
            {
                ["Buttons"] = new Dictionary<string, object>
                {
                    { "Default", "Audio/Menu/Preset/SteamVR/click.ogg" }
                },
                ["Menu"] = new Dictionary<string, object>
                {
                    { "Open", "Audio/Menu/Preset/SteamVR/open.ogg" },
                    { "Close", "Audio/Menu/Preset/SteamVR/close.ogg" },
                    { "Achievement", "Audio/Menu/Preset/SteamVR/achievement.ogg" },
                    { "Admin", "Audio/Menu/Preset/SteamVR/patreon.ogg" },
                    { "Patreon", "Audio/Menu/Preset/SteamVR/patreon.ogg" }
                },

                ["Notifications"] = new Dictionary<string, object>
                {
                    { "None", "Audio/Menu/Preset/SteamVR/notification.ogg" },
                }
            },
            ["Minecraft Legacy Edition"] = new Dictionary<string, object>
            {
                ["Buttons"] = new Dictionary<string, object>
                {
                    { "Default", "Audio/Menu/Preset/MinecraftLegacyEdition/click.ogg" }
                },

                ["Menu"] = new Dictionary<string, object>
                {
                    { "Admin", "Audio/Menu/Preset/MinecraftLegacyEdition/patreon.ogg" },
                    { "Patreon", "Audio/Menu/Preset/MinecraftLegacyEdition/patreon.ogg" },
                    { "Return",  "Audio/Menu/Preset/MinecraftLegacyEdition/back.ogg" }
                },
                ["Notifications"] = new Dictionary<string, object>
                {
                    { "None", "Audio/Menu/Preset/MinecraftLegacyEdition/notification.ogg" },
                }
            },
            ["Playstation 2"] = new Dictionary<string, object>
            {
                ["Buttons"] = new Dictionary<string, object>
                {
                    { "Default", "Audio/Menu/Preset/Playstation2/click.ogg" }
                },
                ["Notifications"] = new Dictionary<string, object>
                {
                    { "None", "Audio/Menu/Preset/Playstation2/notification.ogg" },
                }
            },
            ["Nintendo Switch"] = new Dictionary<string, object>
            {
                ["Buttons"] = new Dictionary<string, object>
                {
                    { "Default", "Audio/Menu/Preset/NintendoSwitch/click.ogg" }
                },

                ["Menu"] = new Dictionary<string, object>
                {
                    { "Achievement", "Audio/Menu/Preset/NintendoSwitch/achievement.ogg" },
                    { "Return",  "Audio/Menu/Preset/NintendoSwitch/back.ogg" }
                },
                ["Notifications"] = new Dictionary<string, object>
                {
                    { "None", "Audio/Menu/Preset/NintendoSwitch/notification.ogg" },
                }
            },

        };

        public static string DefaultSoundpack = "None";

        public static void Play(string sound, string outputPath = null, Action<AudioClip> action = null, string buttonText = null, bool overlapHand = false, bool leftOverlap = false, bool global = false)
        {
            if (string.IsNullOrEmpty(sound)) return;

            try
            {
                bool archiveRightHand = rightHand;
                if (overlapHand) rightHand = leftOverlap;

                if (doButtonsVibrate)
                    GorillaTagger.Instance.StartVibration(rightHand, GorillaTagger.Instance.tagHapticStrength / 2f, GorillaTagger.Instance.tagHapticDuration / 2f);

                object path = ResolveSoundPath(sound);
                if (path == null)
                {
                    rightHand = archiveRightHand;
                    return;
                }

                switch (path)
                {
                    case int rpcId:
                        VRRig.LocalRig.PlayHandTapLocal(rpcId, rightHand, buttonClickVolume / 10f);
                        if (NetworkSystem.Instance.InRoom && serversidedButtonSounds)
                            GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.Others, rpcId, rightHand, buttonClickVolume / 10f);
                        RPCProtection();
                        break;

                    case string s:
                        if (string.IsNullOrEmpty(s))
                        {
                            rightHand = archiveRightHand;
                            return;
                        }
                        AudioSource audioSource = rightHand ? VRRig.LocalRig.leftHandPlayer : VRRig.LocalRig.rightHandPlayer;
                        audioSource.volume = buttonClickVolume / 10f;
                        LoadSoundFromURL($"{PluginInfo.ServerResourcePath}/{s}", outputPath ?? s,
                            action ?? (clip =>
                            {
                                if (shouldBePC || global)
                                    AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, buttonClickVolume / 10f);
                                else
                                    audioSource.PlayOneShot(clip, buttonClickVolume / 10f);
                            }
                        ));
                        break;
                }

                rightHand = archiveRightHand;
            }
            catch (Exception ex)
            {
                LogManager.LogError(ex.Message);
            }
        }

        private static object ResolveSoundPath(string sound, string buttonText = null)
        {
            if (string.IsNullOrEmpty(sound)) return null;

            Dictionary<string, object> pack = null;
            bool hasPack = !string.IsNullOrEmpty(DefaultSoundpack) && DefaultSoundpack != "None" && Soundpacks.TryGetValue(DefaultSoundpack, out pack) && pack != null;

            if (sound == "Return")
            {
                if (hasPack && pack.TryGetValue("Menu", out var returnMenuObj) && returnMenuObj is Dictionary<string, object> returnMenu && returnMenu.TryGetValue("Return", out var returnPathObj) && returnPathObj is string returnPath && !string.IsNullOrEmpty(returnPath))
                    return returnPath;

                string fallbackButtonName = DefaultSounds.TryGetValue("Button", out var btnName) ? btnName : "Default";
                if (Sounds.TryGetValue("Buttons", out var buttonsDict) && buttonsDict.TryGetValue(fallbackButtonName, out var buttonPath))
                    return buttonPath;
                if (Sounds.TryGetValue("Buttons", out var defaultDict) && defaultDict.TryGetValue("Default", out var buttonDefaultObj) && buttonDefaultObj is string defaultPath && !string.IsNullOrEmpty(defaultPath))
                    return defaultPath;

                return null;
            }

            string category = null;
            foreach (var kvp in Sounds)
            {
                if (kvp.Value == null) continue;
                if (!kvp.Value.ContainsKey(sound)) continue;
                category = kvp.Key;
                break;
            }

            object baseRequestedPath = null;
            if (category != null && Sounds.TryGetValue(category, out var baseCategory))
            {
                baseCategory.TryGetValue(sound, out baseRequestedPath);
            }

            object rawPath;
            if (hasPack && category != null && pack.TryGetValue(category, out var packCategoryObj) && packCategoryObj is Dictionary<string, object> packCategory)
            {
                packCategory.TryGetValue(sound, out rawPath);
                if (rawPath == null && !packCategory.TryGetValue("Default", out rawPath))
                    packCategory.TryGetValue("None", out rawPath);
                rawPath ??= baseRequestedPath;
            }
            else
                rawPath = baseRequestedPath;

            if (rawPath is string s)
            {
                if (string.IsNullOrEmpty(s)) return null;

                if (s.Contains("lever$1"))
                {
                    if (!string.IsNullOrEmpty(buttonText))
                    {
                        var button = Buttons.GetIndex(buttonText);
                        s = s.Replace("$1", button != null && button.enabled ? "up" : "down");
                    }
                    else
                    {
                        s = s.Replace("lever$1", "leverup");
                    }
                }

                return s;
            }

            if (rawPath is int)
                return rawPath;

            return null;
        }

    }
}

