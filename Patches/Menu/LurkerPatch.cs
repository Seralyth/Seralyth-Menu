/*
 * United Goyim College Fund  Patches/Menu/LurkerPatch.cs
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

using GorillaTagScripts;
using HarmonyLib;

namespace Seralyth.Patches.Menu
{
    [HarmonyPatch(typeof(LurkerGhost), nameof(LurkerGhost.ChangeState))]
    public class LurkerPatch
    {
        public static bool enabled;

        public static bool Prefix(LurkerGhost __instance, LurkerGhost.ghostState newState) =>
            !(enabled &&
                newState == LurkerGhost.ghostState.possess &&
                __instance.targetPlayer == NetworkSystem.Instance.LocalPlayer);

    }
}
