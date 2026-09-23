/*
 * United Goyim College Fund  Patches/Menu/GuardianPatches.cs
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
using UnityEngine;

namespace Seralyth.Patches.Menu
{
    [HarmonyPatch(typeof(VRRig), nameof(VRRig.GrabbedByPlayer))]
    public class GrabPatch
    {
        public static bool enabled;

        public static bool Prefix(VRRig __instance, VRRig grabbedByRig, bool grabbedBody, bool grabbedLeftHand, bool grabbedWithLeftHand) =>
            !enabled;
    }

    [HarmonyPatch(typeof(VRRig), nameof(VRRig.DroppedByPlayer))]
    public class DropPatch
    {
        public static bool Prefix(VRRig __instance, VRRig grabbedByRig, Vector3 throwVelocity) =>
            !GrabPatch.enabled;
    }

    [HarmonyPatch(typeof(GuardianRPCs), nameof(GuardianRPCs.GuardianLaunchPlayer))]
    public class LaunchPatch
    {
        public static bool Prefix(Vector3 velocity) =>
            !GrabPatch.enabled;
    }
}
