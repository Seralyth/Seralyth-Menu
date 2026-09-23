/*
 * United Goyim College Fund  Patches/Safety/RPCProtection.cs
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

using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Seralyth.Patches.Safety
{
    public class RPCProtection
    {
        [HarmonyPatch(typeof(LoadBalancingClient), nameof(LoadBalancingClient.OpRaiseEvent))]
        public class OpRaiseEventPatch
        {
            public static bool enabled = true;
            const int MaxRPCs = 500;
            internal static float startTime = Time.unscaledTime;
            internal static int rpcCount = 0;
            private static bool Prefix(byte eventCode, object customEventContent, RaiseEventOptions raiseEventOptions, SendOptions sendOptions)
            {
                if (enabled)
                {
                    float currentTime = Time.unscaledTime;
                    if (currentTime - startTime > 1f)
                    {
                        startTime = currentTime;
                        rpcCount = 0;
                    }
                    rpcCount++;
                    if (rpcCount > MaxRPCs)
                    {
                        if ((eventCode == PunEvent.RPC || eventCode == 201) && customEventContent != null && customEventContent is Hashtable rpcData)
                        {
                            foreach (var key in rpcData.Keys)
                            {
                                if (key is byte keyByte && keyByte == 0)
                                {
                                    if (rpcData[key] is string rpcName)
                                    {
                                        Debug.LogWarning($"Blocked RPC {rpcName} as we are sending too much traffic over the network!");
                                    }
                                    break;
                                }
                            }
                        }
                        else
                            Debug.LogWarning($"Blocked event {eventCode} as we are sending too much traffic over the network!");
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
