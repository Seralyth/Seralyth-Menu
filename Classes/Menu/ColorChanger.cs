/*
 * United Goyim College Fund  Classes/Menu/ColorChanger.cs
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

using GorillaExtensions;
using Seralyth.Menu;
using UnityEngine;

namespace Seralyth.Classes.Menu
{
    public class ColorChanger : MonoBehaviour
    {
        public void Start()
        {
            if (colors == null)
            {
                Destroy(this);
                return;
            }

            targetRenderer = GetComponent<Renderer>();

            if (colors.IsFlat())
            {
                Update();
                Destroy(this);
                return;
            }

            Update();
        }

        public void Update()
        {
            targetRenderer.enabled = overrideTransparency ?? !colors.transparent;

            if (colors.transparent)
                return;

            if (!Main.dynamicGradients)
                targetRenderer.sharedMaterial.color = colors.GetCurrentColor();
            else
            {
                if (colors.IsFlat())
                    targetRenderer.sharedMaterial.color = colors.GetColor(0);
                else
                {
                    if (targetRenderer.sharedMaterial.shader.name != "Universal Render Pipeline/Unlit" && targetRenderer.sharedMaterial.mainTexture == null)
                    {
                        targetRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"))
                        {
                            mainTexture = Main.GetGradientTexture(colors.GetColor(0), colors.GetColor(1))
                        };

                        if (Main.scrollingGradients)
                            gameObject.GetOrAddComponent<ScrollMaterial>();
                    }
                }
            }

            if (!Main.transparentMenu) return;
            Color color = targetRenderer.sharedMaterial.color;
            color.a = 0.5f;
            targetRenderer.material.color = color;
        }

        public Renderer targetRenderer;
        public ExtGradient colors;
        public bool? overrideTransparency;
    }
}
