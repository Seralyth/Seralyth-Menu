/*
 * United Goyim College Fund  PluginInfo.cs
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

namespace Seralyth
{
    public class PluginInfo
    {
        public const string GUID = "org.stupid.fuckign.idiot";
        public const string Name = "United Goyim College Fund";
        public const string Description = "Help lend a child to eat hotdogs this year mayonnaise and corned beef";
        public const string BuildTimestamp = "2026-08-14T22:16:39Z";
        public const string Version = "5.0.2";

        public const string BaseDirectory =
#if LEGAL || LEGAL_DEBUG
            "UNITEDGOYIMCOLLEGEFUND/Legal";
#else
            "UNITEDGOYIMCOLLEGEFUND";
#endif
        public const string ClientResourcePath = "Seralyth.Resources.Client";
        public const string ServerResourcePath = "https://raw.githubusercontent.com/iiDk-the-inactual/UnitedGoyimCollegeFund/master/Resources/Server";
        public const string ServerAPI = "https://menu.seralyth.software";
        public const string Logo = @"
dfgoiuyghsfivybSDFIUVHBSDJKHbkjhsdfbkjhsdfgkjhSDBFkjhbkdjhbdf ";

#if DEBUG || LEGAL_DEBUG
        public static bool BetaBuild = true;
#else
        public static bool BetaBuild = false;
#endif
    }
}
