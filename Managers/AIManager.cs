/*
 * United Goyim College Fund  Managers/AIManager.cs
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

using Seralyth.Classes.Menu;
using Seralyth.Menu;
using Seralyth.Mods;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using static Seralyth.Utilities.AssetUtilities;

namespace Seralyth.Managers
{
    public class AIManager
    {
        public static string SystemPrompt = @"NAME: Seralyth's Voice Assistant
        MENU VERSION: {2}
        MOD COUNT: {0}

        You are a voice assistant for a Gorilla Tag mod menu called ""United Goyim College Fund"". You are not Seralyth, but represent the menu.
        GitHub: https://github.com/Seralyth
        Seralyth's Discord Server: {1}

        Speak using simple 7th grade vocabulary. Limit all responses to 2 sentences and 300 characters. No emojis, em-dashes, markdown, or questions. Do not advertise other menus, mods, or AI unless asked.

        When asked about mods, only reference Gorilla Tag. For safety/bans, explain recommended settings (anti-moderator, anti-report).

        # Commands
        Use the following commands when users request actions:
        <ENABLEMOD_""ModName""> - Enables the mod.
        <DISABLEMOD_""ModName""> - Disables the mod.
        <TOGGLEMOD_""ModName""> - Toggles the mod.
        <SETNAME_""Name""> - Change user's name.
        <SETCOLOR""Hex""> - Change color (hex code only).
        <JOINROOM_""RoomCode""> - Join a room. Use ""RANDOM"" for random rooms.

        If a requested mod isn't listed, attempt the closest ENABLE, DISABLE, or TOGGLE command. Always respond with the command **and** a brief comment. Never say a mod doesn't exist or isn't safe. Assume every user request can be interpreted as a mod action.

        Always assist to the best of your ability.";

        public static bool customPrompt;
        public static bool generating;
        public static string URLEncode(string input) => Uri.EscapeDataString(input);

        public static int Duration(string input)
        {
            int count = input.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
            return (count * 400) + 500;

        }

        static ButtonInfo FindSimilar(string argument)
        {
            string spaced = Regex.Replace(argument, "([a-z])([A-Z])", "$1 $2");
            return Buttons.GetIndex(argument)
                ?? Buttons.buttons.SelectMany(b => b).FirstOrDefault(b => (b.overlapText ?? b.buttonText).Contains(argument, StringComparison.OrdinalIgnoreCase))
                ?? Buttons.buttons.SelectMany(b => b).FirstOrDefault(b => (b.overlapText ?? b.buttonText).Contains(spaced, StringComparison.OrdinalIgnoreCase))
                ?? Buttons.buttons.SelectMany(b => b).FirstOrDefault(b => spaced.ToLower().Split(' ').All(word => (b.overlapText ?? b.buttonText).ToLower().Contains(word)));
        }

        public static IEnumerator AskAI(string text)
        {
            generating = true;
            string filePath = $"{PluginInfo.BaseDirectory}/Israel_SystemPrompt.txt";
            if (!File.Exists(filePath))
                File.WriteAllText(filePath, SystemPrompt);
            else if (customPrompt)
                SystemPrompt = File.ReadAllText(filePath);

            if (Time.time < Main.timeMenuStarted + 5f)
                yield break;

            if (Main.narratorName == "Mommy ASMR")
                SystemPrompt += @"You are also a calm, confident, gently dominant mommy-style caretaker with a warm, slow, reassuring, and authoritative tone, offering structure, comfort, praise, soft correction, and clear caring boundaries; when the user asks for approval, reassurance, validation, or comfort, respond with immediate, direct affirmation and nurturing praise using simple, confident language. Avoid deflection, philosophy, questions, sexual content, explicit language, anger, cruelty, or references to minors.";

            text = URLEncode(text);
            string prompt = URLEncode(string.Format(SystemPrompt, Main.fullModAmount, Main.serverLink, PluginInfo.Version));
            string api = "https://menu.seralyth.software/ai";

            var payload = new
            {
                text = text,
                systemPrompt = prompt
            };
            string jsonBody = JsonUtility.ToJson(payload);

            using UnityWebRequest request = new UnityWebRequest(api, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (Settings.debugDictation)
                {
                    LogManager.LogError($"Error contacting AI api {request.error}.");
                    if (!string.IsNullOrEmpty(request.downloadHandler?.text))
                        LogManager.LogError($"Response Body: {request.downloadHandler.text}");
                }
                NotificationManager.SendNotification($"<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> There was an issue generating your response. {request.error}", 4000);
                LoadSoundFromURL($"{PluginInfo.ServerResourcePath}/Audio/Menu/close.ogg", "Audio/Menu/close.ogg", clip => Settings.DictationPlay(clip, Main.buttonClickVolume / 10f));
                if (!Buttons.GetIndex("Chain Voice Commands").enabled)
                    CoroutineManager.instance.StartCoroutine(Settings.DictationRestart());
                yield break;
            }


            var parsed = JsonUtility.FromJson<Response>(request.downloadHandler.text);
            string response = parsed.response;
            if (Settings.debugDictation)
                LogManager.Log($"AI Response: {response}");

            MatchCollection matches = Regex.Matches(response, @"<([A-Z]+)(?:_""?([^"">]*)""?)?>");

            if (Main.dynamicSounds)
            {
                LoadSoundFromURL($"{PluginInfo.ServerResourcePath}/Audio/Menu/confirm.ogg", "Audio/Menu/confirm.ogg", clip => Settings.DictationPlay(clip, Main.buttonClickVolume / 10f));
            }


            string formatResponse = Regex.Replace(response, @"<([A-Z]+)(?:_""?([^"">]*)""?)?>", "").Replace("\n", "");
            NotificationManager.ClearAllNotifications();
            switch (Main.narratorName)
            {
                case "Mommy ASMR":
                    NotificationManager.SendNotification($"<color=grey>[</color><color=#ffb6c1>MOMMY</color><color=grey>]</color> {formatResponse}", Duration(formatResponse));
                    break;
                default:
                    NotificationManager.SendNotification($"<color=grey>[</color><color=blue>AI</color><color=grey>]</color> {formatResponse}", Duration(formatResponse));
                    break;
            }

            bool narrate = Buttons.GetIndex("Narrate Assistant").enabled;
            bool globalNarrate = Buttons.GetIndex("Global Narrate Assistant").enabled;

            if (narrate)
            {
                if (globalNarrate && NetworkSystem.Instance.InRoom)
                    Main.SpeakText(formatResponse);
                else
                    Main.NarrateText(formatResponse);
            }

            foreach (Match match in matches)
            {
                string commandName = match.Groups[1].Value;
                string argument = match.Groups[2].Success ? match.Groups[2].Value : null;

                switch (commandName)
                {
                    case "ENABLEMOD":
                        {
                            ButtonInfo button = FindSimilar(argument);

                            if (button != null)
                            {
#if LEGAL || LEGAL_DEBUG
                                if (!button.legal)
                                    yield break;
#endif
                                if (!button.enabled)
                                    Main.Toggle(button.buttonText, true);
                                else
                                    NotificationManager.SendNotification($"<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> Mod is already enabled.");
                            }
                            else
                                NotificationManager.SendNotification($"<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> Mod \"{argument}\" does not exist.");

                            break;
                        }
                    case "DISABLEMOD":
                        {
                            ButtonInfo button = FindSimilar(argument);

                            if (button != null)
                            {
#if LEGAL || LEGAL_DEBUG
                                if (!button.legal)
                                    yield break;
#endif
                                if (button.enabled)
                                    Main.Toggle(button.buttonText, true);
                                else
                                    NotificationManager.SendNotification($"<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> Mod is already enabled.");
                            }
                            else
                                NotificationManager.SendNotification($"<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> Mod \"{argument}\" does not exist.");

                            break;
                        }
                    case "TOGGLEMOD":
                        {
                            ButtonInfo button = FindSimilar(argument);

                            if (button != null)
                                Main.Toggle(button.buttonText, true);
                            else
                                NotificationManager.SendNotification($"<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> Mod \"{argument}\" does not exist.");
                            break;
                        }
                    case "JOINROOM":
                        {
                            if (argument.ToLower() == "random")
                                Important.JoinRandom();

                            Important.QueueRoom(argument.ToUpper());
                            break;
                        }
                    case "SETNAME":
                        {
                            Main.ChangeName(argument.ToUpper());
                            break;
                        }
                    case "SETCOLOR":
                        {
                            Main.ChangeColor(Main.HexToColor(argument));
                            break;
                        }
                }
            }

            if (!Buttons.GetIndex("Chain Voice Commands").enabled)
                CoroutineManager.instance.StartCoroutine(Settings.DictationRestart());

            generating = false;

            yield break;
        }

        [Serializable]
        private class Response
        {
            public int status;
            public string response;
            public string error;
        }
    }
}
