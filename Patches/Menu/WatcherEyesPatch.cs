/*
 * United Goyim College Fund  Patches/Menu/WatcherEyesPatch.cs
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

using GorillaLocomotion;
using HarmonyLib;
using UnityEngine;

namespace Seralyth.Patches.Menu
{
    [HarmonyPatch(typeof(HalloweenWatcherEyes), nameof(HalloweenWatcherEyes.Update))]
    public class WatcherEyesPatch
    {
        public static bool enabled;

        public static bool Prefix(HalloweenWatcherEyes __instance)
        {
            if (enabled)
            {
                Quaternion normalized = Quaternion.LookRotation((GTPlayer.Instance.headCollider.transform.position - __instance.transform.position).normalized);
                __instance.leftEye.transform.rotation = normalized;
                __instance.rightEye.transform.rotation = normalized;
                return false;
            }
            return true;
        }
    }
}
