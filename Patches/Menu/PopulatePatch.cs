/*
 * United Goyim College Fund  Patches/Menu/PopulatePatch.cs
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

namespace Seralyth.Patches.Menu
{
    [HarmonyPatch(typeof(FriendCard), nameof(FriendCard.Populate), new[] { typeof(FriendBackendController.Friend), typeof(bool) })]
    public class PopulatePatch
    {
        public static bool enabled;

        public static void Postfix(FriendCard __instance, FriendBackendController.Friend friend)
        {
            if (enabled)
            {
                bool custom = friend.Presence.RoomId[0] == '@';

                __instance.SetRoom((custom ? friend.Presence.RoomId[1..] : friend.Presence.RoomId).ToUpper());
                __instance.SetZone((custom ? "CUSTOM" : friend.Presence.Zone).ToUpper());
                __instance.joinable = true;

                __instance.UpdateComponentStates();
            }
        }
    }
}
