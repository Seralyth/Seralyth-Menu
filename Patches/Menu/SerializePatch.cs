/*
 * United Goyim College Fund  Patches/Menu/SerializePatch.cs
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

using HarmonyLib;
using Photon.Pun;
using Seralyth.Managers;
using System;

namespace Seralyth.Patches.Menu
{
    [HarmonyPatch(typeof(PhotonNetwork), nameof(PhotonNetwork.RunViewUpdate))]
    public class SerializePatch
    {
        /// <summary>
        /// Occurs when a serialization process is initiated.
        /// </summary>
        public static event Action OnSerialize;

        /// <summary>
        /// Delegate that determines whether serialization should be overridden.
        /// </summary>
        public static Func<bool> OverrideSerialization;

        public static bool Prefix()
        {
            if (!NetworkSystem.Instance.InRoom)
                return true;

            try
            {
                OnSerialize?.Invoke();
            }
            catch (Exception e)
            {
                LogManager.LogError($"Error in SerializePatch.OnSerialize: {e}");
            }

            if (OverrideSerialization == null)
                return true;

            try
            {
                return OverrideSerialization();
            }
            catch (Exception e)
            {
                LogManager.LogError($"Error in SerializePatch.OverrideSerialization: {e}");
                return false;
            }
        }
    }
}
