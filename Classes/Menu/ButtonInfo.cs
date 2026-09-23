/*
 * United Goyim College Fund  Classes/Menu/ButtonInfo.cs
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

using Seralyth.Managers;
using System;

namespace Seralyth.Classes.Menu
{
    public class ButtonInfo
    {
        public string buttonText = "-"; // Button code name, displayed in menu if overlapText is null
        public string overlapText;      // Button text displayed on menu, overrides buttonText

        public string[] aliases;        // Other terms a button can go by for searching, not displayed in menu

        public string toolTip = "This button doesn't have a tooltip/tutorial."; // Tooltip of button, a short description

        public Action method;           // Every frame before GTPlayer.LateUpdate is called
        public Action postMethod;       // Every frame after GTPlayer.LateUpdate is called

        public Action enableMethod;     // Once before method on enable
        public Action disableMethod;    // Once on disable

        public bool enabled;
        public bool isTogglable = true;

        public bool hideFromArraylist;
        public bool label;
        public bool incremental;
        public bool detected;

        public bool legal;

        public string customBind;
        public string rebindKey;


        public bool isSetting;
        public object value;

        public Action onValueChanged;
        public Action<bool> cycleValue;

        public bool excludeFromSave;

        internal bool firstFrame = true;

        public T GetValue<T>()
        {
            if (value == null)
                return default;

            Type targetType = typeof(T);

            try
            {
                if (targetType.IsEnum)
                {
                    return value is string s
                        ? (T)Enum.Parse(targetType, s, ignoreCase: true)
                        : (T)Enum.ToObject(targetType, value);
                }

                return (T)Convert.ChangeType(value, targetType);
            }
            catch (Exception e)
            {
                LogManager.Log($"GetValue<{targetType.Name}> failed: value was '{value}' (actual type: {value.GetType().Name}). {e.Message}");
                throw;
            }
        }
        public void SetValue<T>(T v) => value = v;

        public void SetEnabled(bool value, bool save = true)
        {
            enabled = value;

            if (save && !excludeFromSave)
                Preferences.SaveButton(this);
        }
    }
}