/*
 * United Goyim College Fund  Patches/Menu/FPSPatch.cs
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
    [HarmonyPatch(typeof(GTPlayerStats), nameof(GTPlayerStats.DelayedUpdate))]
    public class GTPlayerStatsPatch
    {
        public static bool SpoofFPS;
        public static bool SpoofTargetFPS;
        public static bool SpoofPing;
        public static short FPS = 90;
        public static short TargetFPS = 90;
        public static short Ping = 60;

        public static bool Prefix()
        {
            if (SpoofFPS || SpoofTargetFPS || SpoofPing)
            {
                if (SpoofFPS)
                    GTPlayerStats.FPS = FPS;
                if (SpoofTargetFPS)
                    GTPlayerStats.TargetFPS = TargetFPS;
                if (SpoofPing)
                    GTPlayerStats.Ping = Ping;
                return false;
            }
            return true;
        }
    }
}
