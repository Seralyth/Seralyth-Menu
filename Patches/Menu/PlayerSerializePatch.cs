/*
 * United Goyim College Fund  Patches/Menu/PlayerSerializePatch.cs
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
using Seralyth.Managers;
using System;

namespace Seralyth.Patches.Menu
{
    [HarmonyPatch(typeof(VRRig), nameof(VRRig.SerializeReadShared))]
    public class PlayerSerializePatch
    {
        public static bool stopSerialization;
        public static float? delay;

        public static event Action<VRRig> OnPlayerSerialize;
        public static bool Prefix(VRRig __instance, InputStruct data)
        {
            if (stopSerialization)
                return false;

            if (delay != null)
            {
                CoroutineManager.instance.StartCoroutine(
                    Seralyth.Menu.Main.SerializationDelay(() =>
                    {
                        float oldDelay = delay.Value;
                        delay = null;
                        try
                        {
                            __instance.SerializeReadShared(data);
                        }
                        catch { }
                        delay = oldDelay;
                    }, delay.Value)
                );

                return false;
            }

            return true;
        }

        public static void Postfix(VRRig __instance, InputStruct data) =>
            OnPlayerSerialize?.Invoke(__instance);
    }
}
