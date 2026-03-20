using Seralyth.Classes.Menu;
using Seralyth.Managers;
using Seralyth.Menu;
using Seralyth.Patches;
using Seralyth.Patches.Menu;
using System;
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

            if (File.Exists($"{PluginInfo.BaseDirectory}/Seralyth_Preferences.txt"))
            {
                if (File.ReadAllLines($"{PluginInfo.BaseDirectory}/Seralyth_Preferences.txt")[0]
                    .Split(";;")
                    .Contains("Accept TOS"))
                {
                    TOSPatches.enabled = true;
                }
            }

            if (File.Exists($"{PluginInfo.BaseDirectory}/Seralyth_DisableTelemetry.txt"))
                ServerData.DisableTelemetry = true;

            GorillaTagger.OnPlayerSpawned(LoadMenu);
        }

        private static void LoadMenu()
        {
            PatchHandler.PatchAll();

            Loader = new GameObject("Seralyth_Loader");
            Loader.AddComponent<CoroutineManager>();
            Loader.AddComponent<NotificationManager>();
            Loader.AddComponent<CustomBoardManager>();
            Loader.AddComponent<UI>();
            UnityEngine.Object.DontDestroyOnLoad(Loader);
        }
    }
}
