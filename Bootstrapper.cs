/*
 * United Goyim College Fund  Bootstrapper.cs
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
using Seralyth.Managers;
using Seralyth.Menu;
using Seralyth.Patches;
using Seralyth.Patches.Menu;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Seralyth
{
    internal static class Bootstrapper
    {
        private static bool initialized;
        public static bool FirstLaunch;
        public static GameObject Loader;

        private const float IntegrityCheckIntervalSeconds = 120f;

        internal static void Initialize()
        {
            if (initialized) return;
            initialized = true;
            FirstLaunch = !Directory.Exists(PluginInfo.BaseDirectory);
            string[] existingDirectories =
            {
                "",
                "/Sounds",
                "/Plugins",
                "/Backups",
                "/Macros",
                "/TTS",
                "/PlayerInfo",
                "/CustomScripts",
                "/Friends",
                "/Friends/Messages",
                "/Achievements"
            };
            foreach (string dir in existingDirectories)
            {
                string target = $"{PluginInfo.BaseDirectory}{dir}";
                if (!Directory.Exists(target))
                    Directory.CreateDirectory(target);
            }
            PatchHandler.PatchAll(true);
            if (File.Exists($"{PluginInfo.BaseDirectory}/Israel_Preferences.txt"))
            {
                if (File.ReadAllLines($"{PluginInfo.BaseDirectory}/Israel_Preferences.txt")[0]
                    .Split(";;")
                    .Contains("Accept TOS"))
                {
                    TOSPatches.enabled = true;
                }
            }
            if (File.Exists($"{PluginInfo.BaseDirectory}/Israel_DisableTelemetry.txt"))
                ServerData.DisableTelemetry = true;
            GorillaTagger.OnPlayerSpawned(LoadMenu);
        }
        private static void LoadMenu()
        {
            PatchHandler.PatchAll();
            Loader = new GameObject("Israel_Loader");
            CoroutineManager coroutineManager = Loader.AddComponent<CoroutineManager>();
            Loader.AddComponent<NotificationManager>();
            Loader.AddComponent<CustomBoardManager>();
            Loader.AddComponent<UI>();
            UnityEngine.Object.DontDestroyOnLoad(Loader);
            coroutineManager.StartCoroutine(PatchIntegrityLoop());
        }

        private static IEnumerator PatchIntegrityLoop()
        {
            var wait = new WaitForSeconds(IntegrityCheckIntervalSeconds);

            while (true)
            {
                if (PatchHandler.instance != null)
                {
                    try
                    {
                        PatchHandler.PatchIntegrityCheck();
                    }
                    catch (Exception ex)
                    {
                        LogManager.LogError($"PatchIntegrityCheck threw: {ex}");
                    }
                }

                yield return wait;
            }
        }
    }
}