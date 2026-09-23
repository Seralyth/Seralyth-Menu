/*
 * United Goyim College Fund  Mods/CustomMaps/Maps/MiningSimulator.cs
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

using Seralyth.Classes.Menu;
using System.Collections.Generic;
using static Seralyth.Mods.CustomMaps.Manager;

namespace Seralyth.Mods.CustomMaps.Maps
{
    public class MiningSimulator : CustomMap
    {
        public override long MapID => 4977315;
        public override ButtonInfo[] Buttons => new[]
        {
            new ButtonInfo { buttonText = "Instant Mine", enableMethod = InstantMine, disableMethod = DisableInstantMine, toolTip = "Instantly mines any blocks with your pickaxe."},
            new ButtonInfo { buttonText = "Mine Anything", enableMethod = MineAnything, disableMethod = DisableMineAnything, toolTip = "Lets you mine any block."},
            new ButtonInfo { buttonText = "Infinite Backpack", enableMethod = InfiniteBackpack, disableMethod = DisableInfiniteBackpack, toolTip = "Lets you mine more blocks even if your inventory is full."},
        };

        public static void InstantMine()
        {
            ModifyCustomScript(new Dictionary<int, string>
                    {
                        { 974, "lastMinedTime = 0" }
                    });
        }
        public static void DisableInstantMine() =>
            RevertCustomScript(974);

        public static void MineAnything()
        {
            ModifyCustomScript(new Dictionary<int, string>
                    {
                        { 965, "if true then" }
                    });
        }
        public static void DisableMineAnything() =>
            RevertCustomScript(965);

        public static void InfiniteBackpack()
        {
            ModifyCustomScript(new Dictionary<int, string>
                    {
                        { 981, "if false then" }
                    });
        }
        public static void DisableInfiniteBackpack() =>
            RevertCustomScript(981);
    }
}
