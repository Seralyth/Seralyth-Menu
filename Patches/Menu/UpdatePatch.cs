/*
 * United Goyim College Fund  Patches/Menu/UpdatePatch.cs
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
using Seralyth.Extensions;
using Seralyth.Menu;

namespace Seralyth.Patches.Menu
{
    [HarmonyPatch(typeof(GorillaPlayerScoreboardLine), nameof(GorillaPlayerScoreboardLine.UpdatePlayerText))]
    public class UpdatePatch
    {
        public static bool enabled;

        private static int GetPing(VRRig rig)
        {
            int ping = rig.GetPing();
            return ping <= 150 ? 5 : ping <= 300 ? 4 : ping <= 450 ? 3 : ping <= 600 ? 2 : 1;
        }

        public static void Postfix(GorillaPlayerScoreboardLine __instance)
        {
            if (enabled)
            {
                string targetName = Main.CleanPlayerName(__instance.linePlayer.NickName) + " ERR";
                try
                {
                    VRRig rig = __instance.linePlayer.VRRig();
                    targetName = $"{Main.CleanPlayerName(__instance.linePlayer.NickName)}<size=50> <sprite name=\"{rig.GetPlatform()}\"> <sprite name=\"Ping{GetPing(rig)}\">{rig.GetFPS()}</size>";
                }
                catch { }
                __instance.playerNameVisible = targetName;
            }
        }
    }
}
