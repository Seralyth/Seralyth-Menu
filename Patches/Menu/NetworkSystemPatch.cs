/*
 * United Goyim College Fund  Patches/Menu/AntiCrashPatches.cs
 * A stupid shit hole i fuckjing hate this game with over 1000+ mods
 *
 * Copyright (C) 2026  robin williams
 * https://github.com/iiDk-the-inactual/UnitedGoyimCollegeFund
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *aa
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using HarmonyLib;
using Seralyth.Menu;

namespace Seralyth.Patches.Menu
{
    public class NetworkSystemPatch
    {
        [HarmonyPatch(typeof(NetworkSystemPUN), nameof(NetworkSystemPUN.ConnectToRoom))]
        public class ConnectToRoom
        {
            public static bool Prefix(string roomName, RoomConfig opts, int regionIndex = -1)
            {
                if (Buttons.GetIndex("Unlock Fan Club Subscription").enabled)
                {
                    if (opts.MaxPlayers == 20)
                        opts.MaxPlayers = 10;
                    if ((string)opts.CustomProps["fan_club"] == "true")
                        opts.CustomProps["fan_club"] = "false";
                }
                return true;
            }
        }
    }
}
