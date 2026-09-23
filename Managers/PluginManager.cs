/*
 * United Goyim College Fund  Managers/PluginManager.cs
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using static Seralyth.Menu.Main;
using static Seralyth.Utilities.FileUtilities;

namespace Seralyth.Managers
{
    public class PluginManager
    {
        public class Plugin
        {
            public string FileName;
            public bool Enabled;

            public string Name;
            public string Description;

            public Assembly Assembly;
        }


        private class PluginHooks
        {
            public MethodInfo OnEnable;
            public MethodInfo OnDisable;
            public MethodInfo[] OnGUI;
            public MethodInfo[] Update;
        }

        private static readonly HttpClient httpClient = new HttpClient();

        public static readonly List<Plugin> Plugins = new List<Plugin>();
        public static void LoadPlugins()
        {
            Buttons.buttons[Buttons.GetCategory("Plugin Settings")] = new[] { new ButtonInfo { buttonText = "Exit Plugin Settings", method = () => Buttons.CurrentCategoryName = "Settings", isTogglable = false, toolTip = "Returns you back to the settings menu." } };

            if (Plugins.Count > 0)
            {
                foreach (var plugin in Plugins.Where(plugin => plugin.Enabled))
                    DisablePlugin(plugin.Assembly);
            }

            cacheHooks.Clear();
            cacheAssembly.Clear();
            Plugins.Clear();

            if (!Directory.Exists($"{PluginInfo.BaseDirectory}/Plugins"))
                Directory.CreateDirectory($"{PluginInfo.BaseDirectory}/Plugins");

            string[] disabledPlugins = { };
            if (!File.Exists($"{PluginInfo.BaseDirectory}/Plugins/DisabledPlugins.txt"))
                File.WriteAllText($"{PluginInfo.BaseDirectory}/Plugins/DisabledPlugins.txt", "");
            else
            {
                string text = File.ReadAllText($"{PluginInfo.BaseDirectory}/Plugins/DisabledPlugins.txt");
                if (text.Length > 1)
                    disabledPlugins = text
                        .Split('\n')
                        .Select(line => line.Trim())
                        .Where(line => line.Length > 0)
                        .ToArray();
            }

            string[] files = Directory.GetFiles($"{PluginInfo.BaseDirectory}/Plugins");
            foreach (string file in files)
            {
                try
                {
                    if (!GetFileExtension(file).Equals("dll", StringComparison.OrdinalIgnoreCase)) continue;
                    string pluginName = file.Replace($"{PluginInfo.BaseDirectory}/Plugins/", "");

                    Assembly assembly = GetAssembly(file);
                    string[] pluginData = GetPluginInfo(assembly);

                    Plugin plugin = new Plugin()
                    {
                        FileName = pluginName,
                        Name = pluginData[0],
                        Description = pluginData[1],
                        Assembly = assembly,
                        Enabled = !disabledPlugins.Contains(pluginName)
                    };

                    if (plugin.Enabled)
                        EnablePlugin(plugin.Assembly);

                    Plugins.Add(plugin);
                }
                catch (Exception e) { LogManager.Log("Error with loading plugin " + file + ": " + e); }
            }

            foreach (Plugin plugin in Plugins)
            {
                try
                {
                    Buttons.AddButton(Buttons.GetCategory("Plugin Settings"), new ButtonInfo { buttonText = plugin.FileName, overlapText = (plugin.Enabled ? "<color=grey>[</color><color=green>ON</color><color=grey>]</color>" : "<color=grey>[</color><color=red>OFF</color><color=grey>]</color>") + " " + plugin.Name, method = () => TogglePlugin(plugin), isTogglable = false, toolTip = plugin.Description });
                }
                catch (Exception e) { LogManager.Log("Error with enabling plugin " + plugin.Name + ": " + e); }
            }

            Buttons.AddButton(Buttons.GetCategory("Plugin Settings"), new ButtonInfo { buttonText = "Open Plugins Folder", method = OpenPluginsFolder, isTogglable = false, toolTip = "Opens a folder containing all of your plugins." });
            Buttons.AddButton(Buttons.GetCategory("Plugin Settings"), new ButtonInfo { buttonText = "Reload Plugins", method = ReloadPlugins, isTogglable = false, toolTip = "Reloads all of your plugins." });
            Buttons.AddButton(Buttons.GetCategory("Plugin Settings"), new ButtonInfo { buttonText = "Get More Plugins", method = LoadPluginLibrary, isTogglable = false, toolTip = "Opens a public plugin library, where you can download your own plugins." });
        }

        private static string SanitizeFileName(string name)
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            string cleaned = new string(name.Where(c => !invalid.Contains(c)).ToArray());
            cleaned = cleaned.Replace("..", "");
            return cleaned;
        }

        public static void DownloadPlugin(string name, string url)
        {
            // just in case
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri) || uri.Scheme != Uri.UriSchemeHttps)
            {
                LogManager.Log($"Refused to download plugin '{name}': URL is not a valid HTTPS address ({url}).");
                NotificationManager.SendNotification("<color=grey>[</color><color=red>FAILED</color><color=grey>]</color> Refused to download " + name + ": invalid or insecure URL.");
                return;
            }

            string filename = SanitizeFileName(uri.Segments[^1]);
            if (string.IsNullOrWhiteSpace(filename) || !filename.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                LogManager.Log($"Refused to download plugin '{name}': resolved filename '{filename}' is not a valid dll name.");
                NotificationManager.SendNotification("<color=grey>[</color><color=red>FAILED</color><color=grey>]</color> Refused to download " + name + ": invalid file.");
                return;
            }

            string destination = Path.Combine($"{PluginInfo.BaseDirectory}/Plugins", filename);

            try
            {
                byte[] data = httpClient.GetByteArrayAsync(uri).GetAwaiter().GetResult();

                if (File.Exists(destination))
                    File.Delete(destination);

                File.WriteAllBytes(destination, data);
            }
            catch (Exception e)
            {
                LogManager.Log("Error downloading plugin " + name + ": " + e);
                NotificationManager.SendNotification("<color=grey>[</color><color=red>FAILED</color><color=grey>]</color> Could not download " + name + ".");
                return;
            }

            LoadPlugins();
            NotificationManager.SendNotification("<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> Successfully downloaded " + name + " to your plugins.");
        }

        public static void TogglePlugin(Plugin plugin)
        {
            if (plugin.Enabled)
                DisablePlugin(plugin.Assembly);
            else
                EnablePlugin(plugin.Assembly);

            plugin.Enabled = !plugin.Enabled;

            string disabledPluginsString = Plugins.Where(p => !p.Enabled).Select(p => p.FileName).Aggregate("", (current, disabledPlugin) => current + (disabledPlugin + "\n"));

            File.WriteAllText($"{PluginInfo.BaseDirectory}/Plugins/DisabledPlugins.txt", disabledPluginsString);

            Buttons.GetIndex(plugin.FileName).overlapText = (plugin.Enabled ? "<color=grey>[</color><color=green>ON</color><color=grey>]</color>" : "<color=grey>[</color><color=red>OFF</color><color=grey>]</color>") + " " + plugin.Name;
        }

        public static void ExecuteUpdate()
        {
            foreach (Plugin plugin in Plugins.Where(plugin => plugin.Enabled))
            {
                try
                {
                    foreach (MethodInfo method in ResolveHooks(plugin.Assembly).Update)
                        method.Invoke(null, null);
                }
                catch (Exception e) { LogManager.Log("Error with Update() with plugin " + plugin.Name + ": " + e); }
            }
        }

        public static void ExecuteOnGUI()
        {
            foreach (Plugin plugin in Plugins.Where(plugin => plugin.Enabled))
            {
                try
                {
                    foreach (MethodInfo method in ResolveHooks(plugin.Assembly).OnGUI)
                        method.Invoke(null, null);
                }
                catch (Exception e) { LogManager.Log("Error with OnGUI() with plugin " + plugin.Name + ": " + e); }
            }
        }

        private static readonly Dictionary<string, Assembly> cacheAssembly = new Dictionary<string, Assembly>();
        private static Assembly GetAssembly(string dllName)
        {
            if (cacheAssembly.TryGetValue(dllName, out var assembly))
                return assembly;

            Assembly loaded = Assembly.Load(File.ReadAllBytes(dllName.Replace("/", "\\")));
            cacheAssembly.Add(dllName, loaded);
            return loaded;
        }

        private static string[] GetPluginInfo(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                FieldInfo name = type.GetField("Name", BindingFlags.Public | BindingFlags.Static);
                FieldInfo description = type.GetField("Description", BindingFlags.Public | BindingFlags.Static);
                if (name != null && description != null)
                    return new[] { (string)name.GetValue(null), (string)description.GetValue(null) };
            }

            return new[] { "null", "null" };
        }

        private static readonly Dictionary<Assembly, PluginHooks> cacheHooks = new Dictionary<Assembly, PluginHooks>();
        private static PluginHooks ResolveHooks(Assembly assembly)
        {
            if (cacheHooks.TryGetValue(assembly, out var cached))
                return cached;

            Type[] types = assembly.GetTypes();

            PluginHooks hooks = new PluginHooks
            {
                OnEnable = types
                    .Select(type => type.GetMethod("OnEnable", BindingFlags.Public | BindingFlags.Static))
                    .FirstOrDefault(method => method != null),
                OnDisable = types
                    .Select(type => type.GetMethod("OnDisable", BindingFlags.Public | BindingFlags.Static))
                    .FirstOrDefault(method => method != null),
                OnGUI = types
                    .Select(type => type.GetMethod("OnGUI", BindingFlags.Public | BindingFlags.Static))
                    .Where(method => method != null)
                    .ToArray(),
                Update = types
                    .Select(type => type.GetMethod("Update", BindingFlags.Public | BindingFlags.Static))
                    .Where(method => method != null)
                    .ToArray()
            };

            cacheHooks.Add(assembly, hooks);
            return hooks;
        }

        private static void EnablePlugin(Assembly assembly)
        {
            try
            {
                ResolveHooks(assembly).OnEnable?.Invoke(null, null);
            }
            catch (Exception e) { LogManager.Log($"Error invoking OnEnable() on {assembly.GetName().Name}: {e}"); }
        }

        private static void DisablePlugin(Assembly assembly)
        {
            try
            {
                ResolveHooks(assembly).OnDisable?.Invoke(null, null);
            }
            catch (Exception e) { LogManager.Log($"Error invoking OnDisable() on {assembly.GetName().Name}: {e}"); }
        }

        #region Menu Integration
        public static void ReloadPlugins()
        {
            Dictionary<string, bool> snapshot = Plugins.ToDictionary(p => p.FileName, p => p.Enabled);

            LoadPlugins();

            foreach (Plugin plugin in Plugins)
                if (snapshot.TryGetValue(plugin.FileName, out bool wasEnabled) && plugin.Enabled != wasEnabled)
                    TogglePlugin(plugin);

            if (isSearching)
                Mods.Settings.Search();

            Buttons.CurrentCategoryName = "Main";
        }

        public static void OpenPluginsFolder() =>
            Process.Start(GetGamePath() + $"/{PluginInfo.BaseDirectory}/Plugins");

        public static void LoadPluginLibrary()
        {
            string library = GetHttp($"{PluginInfo.ServerResourcePath}/Plugins/PluginLibrary.txt");
            string[] plugins = AlphabetizeNoSkip(library.Split("\n"));

            List<ButtonInfo> buttonInfos = new List<ButtonInfo> { new ButtonInfo { buttonText = "Exit Plugin Library", method = () => Buttons.CurrentCategoryName = "Plugin Settings", isTogglable = false, toolTip = "Returns you back to the plugin settings." } };
            int index = 0;

            foreach (string plugin in plugins)
            {
                if (plugin.Length <= 2) continue;
                index++;
                string[] data = plugin.Split(";");
                buttonInfos.Add(new ButtonInfo { buttonText = "PluginDownload" + index, overlapText = data[0], method = () => DownloadPlugin(data[0], data[2]), isTogglable = false, toolTip = data[1] });
            }

            Buttons.buttons[Buttons.GetCategory("Temporary Category")] = buttonInfos.ToArray();
            Buttons.CurrentCategoryName = "Temporary Category";
        }
        #endregion
    }
}