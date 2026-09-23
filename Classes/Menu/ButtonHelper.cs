/*
 * United Goyim College Fund  Classes/Menu/CycleSetting.cs
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

using System;
using UnityEngine;

namespace Seralyth.Classes.Menu
{
    public static class ButtonHelper // Hell
    {
        public static int Wrap(int current, int min, int max, bool positive)
        {
            int v = positive ? current + 1 : current - 1;
            if (v > max) v = min;
            if (v < min) v = max;
            return v;
        }

        public static ButtonInfo Create(
            string buttonText, Func<string[]> getNames, string defaultName, Action<int> apply,
            string toolTip = null, bool legal = false, string overlapText = null, Action<bool> onCycle = null)
        {
            string label = overlapText ?? buttonText;

            var button = new ButtonInfo
            {
                buttonText = buttonText,
                isTogglable = false,
                isSetting = true,
                incremental = true,
                value = defaultName,
                toolTip = toolTip,
                legal = legal
            };

            if (!string.IsNullOrEmpty(overlapText))
                button.overlapText = overlapText;

            int CurrentIndex(string[] names)
            {
                if (names == null || names.Length == 0) return -1;

                if (button.value is string s)
                {
                    int idx = Array.IndexOf(names, s);
                    return idx >= 0 ? idx : 0;
                }
                if (button.value is int i && i >= 0 && i < names.Length)
                    return i;

                return 0;
            }

            void RefreshLabel()
            {
                var names = getNames();
                if (names == null || names.Length == 0)
                {
                    button.overlapText = $"{label} <color=grey>[</color><color=green>Loading..</color><color=grey>]</color>";
                    return;
                }

                int v = CurrentIndex(names);
                button.value = names[v];

                button.overlapText = $"{label} <color=grey>[</color><color=green>{names[v]}</color><color=grey>]</color>";
            }

            button.onValueChanged = () =>
            {
                var names = getNames();
                if (names == null || names.Length == 0)
                {
                    RefreshLabel();
                    return;
                }

                int v = CurrentIndex(names);
                RefreshLabel();
                apply(v);
            };

            button.cycleValue = positive =>
            {
                var names = getNames();
                if (names == null || names.Length == 0) return;

                int current = CurrentIndex(names);
                int next = Wrap(current, 0, names.Length - 1, positive);
                button.value = names[next];

                button.onValueChanged();
                Preferences.SaveButton(button);
                onCycle?.Invoke(positive);
            };

            RefreshLabel();

            return button;
        }

        public static ButtonInfo Create(
            string buttonText, Func<string[]> getNames, int defaultIndex, Action<int> apply,
            string toolTip = null, bool legal = false, string overlapText = null, Action<bool> onCycle = null)
        {
            var names = getNames();
            string defaultName = (names != null && defaultIndex >= 0 && defaultIndex < names.Length)
                ? names[defaultIndex]
                : null;

            return Create(buttonText, getNames, defaultName, apply, toolTip, legal, overlapText, onCycle);
        }

        public static ButtonInfo CreateNumeric(
            string buttonText, int min, int max, int defaultValue, Action<int> apply,
            Func<int, string> display = null, string toolTip = null, bool legal = false,
            string overlapText = null, Action<bool> onCycle = null, Func<int> getStep = null, bool persist = true)
        {
            string label = overlapText ?? buttonText;
            getStep ??= () => 1;

            var button = new ButtonInfo
            {
                buttonText = buttonText,
                isTogglable = false,
                isSetting = persist,
                incremental = true,
                value = defaultValue,
                toolTip = toolTip,
                legal = legal,
                excludeFromSave = !persist
            };

            void RefreshLabel()
            {
                int v = Mathf.Clamp(button.GetValue<int>(), min, max);
                button.value = v;

                if (display != null)
                    button.overlapText = $"{label} <color=grey>[</color><color=green>{display(v)}</color><color=grey>]</color>";
            }

            button.onValueChanged = () =>
            {
                RefreshLabel();
                apply(button.GetValue<int>());
            };

            button.cycleValue = positive =>
            {
                int step = Mathf.Max(1, getStep());
                int raw = positive ? button.GetValue<int>() + step : button.GetValue<int>() - step;

                int range = max - min + 1;
                int offset = (raw - min) % range;
                if (offset < 0) offset += range;
                button.value = min + offset;

                button.onValueChanged();
                Preferences.SaveButton(button);
                onCycle?.Invoke(positive);
            };

            RefreshLabel();

            return button;
        }
    }
}