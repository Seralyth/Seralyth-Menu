/*
 * United Goyim College Fund  Mods/CustomMaps/Manager.cs
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

using GorillaTagScripts.VirtualStumpCustomMaps;
using Seralyth.Classes.Menu;
using Seralyth.Managers;
using Seralyth.Menu;
using Seralyth.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Seralyth.Mods.CustomMaps
{
    public static class Manager
    {
        public static readonly Dictionary<long, string> mapScriptArchives = new Dictionary<long, string>();
        public static readonly Dictionary<long, CustomMap> mapCache = new Dictionary<long, CustomMap>();

        public static long? currentMapId;

        public static void UpdateCustomMapsTab(long? overwriteId = null)
        {
            currentMapId = overwriteId;

            int category = Buttons.GetCategory("Custom Map Mods");
            List<ButtonInfo> buttons = new List<ButtonInfo> { new ButtonInfo { buttonText = "Exit Custom Map Mods", method = () => Buttons.CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns you back to the main page." } };

            if (overwriteId != null)
            {
                long mapID = overwriteId.Value;
                if (!mapScriptArchives.ContainsKey(mapID))
                    mapScriptArchives.Add(mapID, CustomGameMode.LuaScript);

                CustomMap map = GetMapByID(mapID);
                if (map == null)
                    buttons.Add(new ButtonInfo { buttonText = "This map is not supported yet.", label = true });
                else
                    buttons.AddRange(map.Buttons);

                buttons.AddRange(new[]
                {
                    new ButtonInfo { buttonText = " ", label = true },
                    new ButtonInfo { buttonText = "Edit Custom Script", method = EditUserScript, isTogglable = false, toolTip = "Opens your custom script for this map." },
                    new ButtonInfo { buttonText = "Delete Custom Script", method = DeleteUserScript, isTogglable = false, toolTip = "Deletes your custom script for this map." },
                    new ButtonInfo { buttonText = "Run Custom Script", enableMethod = StartUserScript, disableMethod = StopUserScript, toolTip = "Runs your custom script for this map." }
                });
            }
            else
                buttons.Add(new ButtonInfo { buttonText = "You have not loaded a map.", label = true });

            Buttons.buttons[category] = buttons.ToArray();
        }

        public static void ModifyCustomScript(Dictionary<int, string> replacements)
        {
            string input = CustomGameMode.LuaScript;
            string[] lines = input.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (var (lineIndex, value) in replacements)
            {
                if (lineIndex < 0 || lineIndex >= lines.Length) continue;
                LogManager.Log("Replacing " + lines[lineIndex] + " with " + value);
                lines[lineIndex] = value;
            }

            CustomGameMode.LuaScript = string.Join(Environment.NewLine, lines);

            if (NetworkSystem.Instance.InRoom)
                LuauHud.Instance.RestartLuauScript();

            CustomMapManager.ReturnToVirtualStump();
        }

        public static void EditUserScript()
        {
            string DirectoryTarget = FileUtilities.GetGamePath() + $"/{PluginInfo.BaseDirectory}/CustomScripts/{CustomMapLoader.LoadedMapModId}.luau";
            if (!File.Exists(DirectoryTarget))
            {
                string script = CustomGameMode.LuaScript;

                if (!string.IsNullOrEmpty(script) && mapScriptArchives.TryGetValue(CustomMapManager.currentRoomMapModId, out string archived))
                    script = archived;
                else if (string.IsNullOrEmpty(script))
                    script = "-- This map does not have a Lua script.\n-- You can write your own script here and click \"Run Custom Script\" to execute it.";

                File.WriteAllText(DirectoryTarget, script);
            }
            Process.Start(DirectoryTarget);
        }

        public static void DeleteUserScript()
        {
            string DirectoryTarget = FileUtilities.GetGamePath() + $"/{PluginInfo.BaseDirectory}/CustomScripts/{CustomMapLoader.LoadedMapModId}.luau";
            if (File.Exists(DirectoryTarget))
                File.Delete(DirectoryTarget);
        }

        public static void StartUserScript()
        {
            string DirectoryTarget = FileUtilities.GetGamePath() + $"/{PluginInfo.BaseDirectory}/CustomScripts/{CustomMapLoader.LoadedMapModId}.luau";
            if (File.Exists(DirectoryTarget))
                CustomGameMode.LuaScript = File.ReadAllText(DirectoryTarget);

            if (NetworkSystem.Instance.InRoom)
                LuauHud.Instance.RestartLuauScript();

            CustomMapManager.ReturnToVirtualStump();
        }

        public static void StopUserScript()
        {
            CustomGameMode.LuaScript = mapScriptArchives[CustomMapManager.currentRoomMapModId];

            if (NetworkSystem.Instance.InRoom)
                LuauHud.Instance.RestartLuauScript();

            CustomMapManager.ReturnToVirtualStump();
        }

        public static void RevertCustomScript(int[] lines)
        {
            Dictionary<int, string> replacements = lines.ToDictionary(line => line, line => mapScriptArchives[CustomMapManager.currentRoomMapModId].Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)[line]);
            ModifyCustomScript(replacements);
        }

        public static void RevertCustomScript(int line) =>
            RevertCustomScript(new[] { line });

        public static CustomMap GetMapByID(long id)
        {
            if (mapCache.TryGetValue(id, out var instance)) return instance;

            Type[] types;
            try
            {
                types = Assembly.GetExecutingAssembly().GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types.Where(t => t != null).ToArray();
            }

            var mapTypes = types
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(CustomMap)));

            foreach (var type in mapTypes)
            {
                CustomMap mapInstance = (CustomMap)Activator.CreateInstance(type);
                if (mapInstance.MapID != id) continue;
                instance = mapInstance;
                break;
            }

            mapCache[id] = instance;
            return instance;
        }
    }
}
