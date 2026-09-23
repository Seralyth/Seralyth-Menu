/*
 * United Goyim College Fund  Classes/Menu/ExtGradient.cs
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

using Seralyth.Extensions;
using Seralyth.Menu;
using System;
using System.Linq;
using UnityEngine;
using static Seralyth.Utilities.RandomUtilities;

namespace Seralyth.Classes.Menu
{
    public class ExtGradient
    {
        public static GradientColorKey[] GetSolidGradient(Color color) =>
            new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) };

        public static GradientColorKey[] GetSimpleGradient(Color a, Color b) =>
            new[] { new GradientColorKey(a, 0f), new GradientColorKey(b, 0.5f), new GradientColorKey(a, 1f) };

        public GradientColorKey[] colors = GetSolidGradient(Color.magenta);

        public Color GetColor(int index)
        {
            if (rainbow)
                return Color.HSVToRGB((Time.time + index / 8) % 1f, 1f, 1f);

            if (pastelRainbow)
                return Color.HSVToRGB(Time.time + index / 8, 0.3f, 1f);

            if (epileptic)
                return RandomColor();

            if (copyRigColor)
                return VRRig.LocalRig.GetColor();

            if (!transparent) return customColor != null ? customColor?.Invoke() ?? Color.magenta : colors[index].color;
            Color targetColor = colors[index].color;
            targetColor.a = 0f;

            return targetColor;

        }

        public void SetColor(int index, Color color, bool setMirror = true)
        {
            rainbow = false;
            pastelRainbow = false;

            epileptic = false;
            copyRigColor = false;

            customColor = null;

            if (colors.Length <= 2)
                colors = GetSimpleGradient(colors[0].color, colors[^1].color);

            if (setMirror && index == 0)
            {
                colors[0].color = color;
                colors[^1].color = color;
            }
            else
                colors[index].color = color;
        }

        public void SetColors(Color color)
        {
            rainbow = false;
            pastelRainbow = false;

            epileptic = false;
            copyRigColor = false;

            customColor = null;

            for (int i = 0; i < colors.Length; i++)
                colors[i].color = color;
        }

        private static Gradient getColorGradient;
        public Color GetColorTime(float time)
        {
            getColorGradient ??= new Gradient();

            if (rainbow)
                return Color.HSVToRGB(time, 1f, 1f);

            if (pastelRainbow)
                return Color.HSVToRGB(time, 0.3f, 1f);

            if (epileptic)
                return RandomColor();

            if (copyRigColor)
                return VRRig.LocalRig.GetColor();

            if (transparent)
            {
                Color targetColor = getColorGradient.Evaluate(time);
                targetColor.a = 0f;

                return targetColor;
            }

            if (customColor != null)
                return customColor?.Invoke() ?? Color.magenta;

            getColorGradient.colorKeys = colors;
            return getColorGradient.Evaluate(time);
        }

        public Color GetCurrentColor(float offset = 0f) =>
            GetColorTime((offset + Time.time / (Main.slowFadeColors ? 10f : 2f)) % 1f);

        public bool IsFlat()
        {
            if (rainbow || pastelRainbow || epileptic || copyRigColor || colors.Length == 0)
                return false;
            Color first = colors[0].color;
            for (int i = 1; i < colors.Length; i++)
                if (colors[i].color != first) return false;
            return true;
        }

        public ExtGradient Clone()
        {
            return new ExtGradient
            {
                rainbow = rainbow,
                pastelRainbow = pastelRainbow,
                epileptic = epileptic,
                copyRigColor = copyRigColor,
                customColor = customColor,
                colors = colors.Select(c => new GradientColorKey(c.color, c.time)).ToArray()
            };
        }

        public bool rainbow;
        public bool pastelRainbow;

        public bool epileptic;
        public bool copyRigColor;

        public bool transparent;

        public Func<Color> customColor;
    }
}
