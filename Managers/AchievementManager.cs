/*
 * United Goyim College Fund  Managers/AchievementManager.cs
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
using Seralyth.Extensions;
using Seralyth.Menu;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Valve.Newtonsoft.Json.Linq;
using static Seralyth.Menu.Main;
using static Seralyth.Utilities.AssetUtilities;

namespace Seralyth.Managers
{
    public static class AchievementManager
    {
        private static List<Achievement> _achievements;
        public static List<Achievement> Achievements
        {
            get
            {
                if (_achievements != null) return _achievements;
                _achievements = new List<Achievement>();

                string[] files = Directory.GetFiles($"{PluginInfo.BaseDirectory}/Achievements");
                foreach (string file in files)
                {
                    if (file.EndsWith(".json"))
                        _achievements.Add(Achievement.FromJObject(JObject.Parse(File.ReadAllText(file))));
                }

                return _achievements;
            }
            set => _achievements = value;
        }

        public static void EnterAchievementTab()
        {
            int achievementCount = Achievements.Count;

            List<ButtonInfo> achievementButtons = new List<ButtonInfo> { new ButtonInfo { buttonText = "Exit Achievements", method = () => Buttons.CurrentCategoryName = "Main", isTogglable = false, toolTip = "Returns you back to the main page." } };

            if (achievementCount <= 0)
                achievementButtons.Add(
                    new ButtonInfo
                    {
                        buttonText = "You have no achievements.",
                        label = true,
                        legal = true
                    });
            else
                for (int i = 0; i < achievementCount; i++)
                {
                    Achievement achievement = Achievements[i];
                    achievementButtons.Add(
                        new ButtonInfo
                        {
                            buttonText = $"Achievement{i}",
                            overlapText = achievement.name,
                            method = () => PromptSingle($"{achievement.description}<{PluginInfo.ServerResourcePath}/{achievement.icon}>", null, "Done"),
                            isTogglable = false,
                            toolTip = achievement.description,
                            legal = true
                        });
                }

            Buttons.buttons[Buttons.GetCategory("Achievements")] = achievementButtons.ToArray();
            Buttons.CurrentCategoryName = "Achievements";
        }

        public static bool HasAchievement(string name) =>
            Achievements.Any(a => a.name == name);

        public static void UnlockAchievement(Achievement achievement)
        {
            if (HasAchievement(achievement.name))
                return;

            LoadSoundFromURL($"{PluginInfo.ServerResourcePath}/Audio/Menu/achievement.ogg", "Audio/Menu/achievement.ogg", clip => Play2DAudio(clip, buttonClickVolume / 10f));
            NotificationManager.SendNotification($"<color=grey>[</color><color=purple>ACHIEVEMENT</color><color=grey>]</color> Achievement unlocked! \"{achievement.name}\"");

            Achievements.Add(achievement);
            File.WriteAllText($"{PluginInfo.BaseDirectory}/Achievements/{achievement.name.Hash()}.json", achievement.ToJObject().ToString());
        }

        public struct Achievement
        {
            public string name;

            public string description;
            public string icon;

            public readonly JObject ToJObject() => new JObject
            {
                ["name"] = name,

                ["description"] = description,
                ["icon"] = icon
            };

            public static Achievement FromJObject(JObject obj) => new Achievement
            {
                name = (string)obj["name"],
                description = (string)obj["description"],
                icon = (string)obj["icon"]
            };
        }
    }
}
