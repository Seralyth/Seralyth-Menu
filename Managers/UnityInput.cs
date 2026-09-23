/*
 * United Goyim College Fund  Managers/UnityInput.cs
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

using UnityEngine;
using UnityEngine.InputSystem;

namespace Seralyth.Managers
{
    internal static class UnityInput
    {
        // Keyboard (KeyCode)
        internal static bool GetKey(Key key) => Keyboard.current[key].isPressed;
        internal static bool GetKeyDown(Key key) => Keyboard.current[key].wasPressedThisFrame;
        internal static bool GetKeyUp(Key key) => Keyboard.current[key].wasReleasedThisFrame;

        // Keyboard (string key names)
        internal static bool GetKey(string keyName) => Input.GetKey(keyName);
        internal static bool GetKeyDown(string keyName) => Input.GetKeyDown(keyName);
        internal static bool GetKeyUp(string keyName) => Input.GetKeyUp(keyName);

        // Mouse
        internal static Vector3 mousePosition => Input.mousePosition;

        internal static bool GetMouseButton(int button) => Input.GetMouseButton(button);

        internal static bool GetMouseButtonDown(int button) => Input.GetMouseButtonDown(button);

        internal static bool GetMouseButtonUp(int button) => Input.GetMouseButtonUp(button);

        internal static Vector2 MouseScrollDelta => Input.mouseScrollDelta;
    }
}