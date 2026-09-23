/*
 * United Goyim College Fund  Classes/Menu/ScrollMaterial.cs
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

using Seralyth.Menu;
using UnityEngine;

namespace Seralyth.Classes.Menu
{
    public class ScrollMaterial : MonoBehaviour
    {
        private Renderer renderer;
        private Material mat;

        void Awake()
        {
            renderer = GetComponent<Renderer>();
            mat = renderer.material;

            Update();
        }

        void Update()
        {
            float offset = Main.slowFadeColors ? Time.time / 10f : Time.time;
            Vector4 st = new Vector4(1, 1, offset, offset);
            mat.SetVector("_BaseMap_ST", st);
        }
    }
}