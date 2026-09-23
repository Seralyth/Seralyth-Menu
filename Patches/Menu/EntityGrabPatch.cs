/*
 * United Goyim College Fund  Patches/Menu/EntityGrabPatch.cs
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
using System.Collections.Generic;
using UnityEngine;

namespace Seralyth.Patches.Menu
{
    [HarmonyPatch(typeof(GameEntityManager), nameof(GameEntityManager.TryGrabLocal))]
    public class EntityGrabPatch
    {
        public static bool enabled;

        public static bool Prefix(GameEntityManager __instance, Vector3 handPosition, bool isLeftHand, Vector3 closestPointOnBoundingBox, ref GameEntityId __result)
        {
            if (enabled)
            {
                List<GameEntity> entities = __instance.entities;

                GameEntityId gameEntityId = GameEntityId.Invalid;
                float closestDist = float.MaxValue;
                for (int i = 0; i < entities.Count; i++)
                {
                    GameEntity entity = entities[i];
                    if (entity != null && __instance.ValidateGrab(entity, NetworkSystem.Instance.LocalPlayer.ActorNumber, isLeftHand))
                    {
                        double reach = 16;

                        float distance = (handPosition - entity.transform.position).sqrMagnitude;
                        if (distance < reach && distance < closestDist)
                        {
                            gameEntityId = entity.id;
                            closestDist = distance;
                        }
                    }
                }

                __result = gameEntityId;

                return false;
            }

            return true;
        }
    }
}
