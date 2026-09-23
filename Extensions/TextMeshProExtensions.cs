/*
 * United Goyim College Fund  Extensions/TextMeshProExtensions.cs
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

using Seralyth.Utilities;
using System;
using TMPro;
using UnityEngine;

namespace Seralyth.Extensions
{
    public static class TextMeshProExtensions
    {
        public static void SafeSetText(this TMP_Text tmp, string text)
        {
            if (tmp == null)
                return;

            if (tmp.text != text)
                tmp.text = text;
        }

        public static void SafeSetFont(this TMP_Text tmp, TMP_FontAsset font)
        {
            if (tmp == null)
                return;

            if (font == null)
                return;

            if (tmp.font == null || tmp.font.hashCode != font.hashCode)
                tmp.font = font;
        }

        public static void SafeSetFontSize(this TMP_Text tmp, float size)
        {
            if (tmp == null)
                return;

            if (Math.Abs(tmp.fontSize - size) > 0.01f)
                tmp.fontSize = size;
        }

        public static void SafeSetFontStyle(this TMP_Text tmp, FontStyles style)
        {
            if (tmp == null)
                return;

            if (tmp.fontStyle != style)
                tmp.fontStyle = style;
        }

        public static void SafeSetCharacterSpacing(this TMP_Text tmp, float targetSpacing)
        {
            if (tmp == null)
                return;

            if (!Mathf.Approximately(tmp.characterSpacing, targetSpacing))
                tmp.characterSpacing = targetSpacing;
        }

        public static void SafeSetWrappingMode(this TMP_Text tmp, TextWrappingModes mode)
        {
            if (tmp == null)
                return;
            if (tmp.textWrappingMode != mode)
                tmp.textWrappingMode = mode;
        }

        public static void SafeSetOverflowMode(this TMP_Text tmp, TextOverflowModes mode)
        {
            if (tmp == null)
                return;
            if (tmp.overflowMode != mode)
                tmp.overflowMode = mode;
        }
        public static void SafeSetAlignment(this TMP_Text tmp, TextAlignmentOptions mode)
        {
            if (tmp == null)
                return;
            if (tmp.alignment != mode)
                tmp.alignment = mode;
        }


        private static Shader _tmpShader;
        public static Shader TmpShader
        {
            get
            {
                if (_tmpShader == null)
                    _tmpShader = AssetUtilities.LoadAsset<Shader>("TMP_SDF-Mobile Overlay");

                return _tmpShader;
            }
        }

        public static void Chams(this TMP_Text tmp)
        {
            if (tmp == null)
                return;

            var mat = tmp.fontMaterial;
            if (mat != null && mat.shader != TmpShader)
                mat.shader = TmpShader;
        }
    }
}
