/*
 * United Goyim College Fund  Patches/Menu/TorsoPatch.cs
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
using Seralyth.Mods;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Seralyth.Patches.Menu
{
    [HarmonyPatch(typeof(VRRig), nameof(VRRig.PostTick))]
    public class TorsoPatch
    {
        public static event Action VRRigLateUpdate;
        public static bool enabled;
        public static int mode = 0;

        public static void Postfix(VRRig __instance)
        {
            if (__instance.isLocal)
            {
                if (enabled)
                {
                    Quaternion rotation = Quaternion.identity;
                    switch (mode)
                    {
                        case 0:
                            rotation = Quaternion.Euler(0f, Time.time * 180f % 360, 0f);
                            break;
                        case 1:
                            rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                            break;
                        case 2:
                            rotation = Quaternion.Euler(0f, GorillaTagger.Instance.headCollider.transform.rotation.eulerAngles.y + 180f, 0f);
                            break;
                        case 3:
                            rotation = Quaternion.Euler(0f, Movement.recBodyRotary.transform.rotation.eulerAngles.y, 0f);
                            break;
                    }

                    __instance.transform.rotation = rotation;
                    __instance.head.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);
                    __instance.leftHand.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);
                    __instance.rightHand.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);
                }

                VRRigLateUpdate?.Invoke();
            }
        }
    }
}
