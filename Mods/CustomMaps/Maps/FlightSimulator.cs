/*
 * United Goyim College Fund  Mods/CustomMaps/Maps/FlightSimulator.cs
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
    public class FlightSimulator : CustomMap
    {
        public override long MapID => 5024157;
        public override ButtonInfo[] Buttons => new[]
        {
            new ButtonInfo { buttonText = "Steal Pilot", enableMethod = StealPilot, disableMethod = DisableStealPilot, toolTip = "Allows you to steal the pilot position from other people's planes."},
        };

        public static void StealPilot()
        {
            ModifyCustomScript(new Dictionary<int, string>
                    {
                        { 373, "if isButtonPressed(pilotClaimButtonJet) then" }
                    });
        }
        public static void DisableStealPilot() =>
            RevertCustomScript(373);
    }
}
