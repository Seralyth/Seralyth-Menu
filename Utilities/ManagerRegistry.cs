/*
 * United Goyim College Fund  Utilities/ManagerRegistry.cs
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

namespace Seralyth.Utilities
{
    public class ManagerRegistry
    {
        #region Public Properties
        public class GhostReactor
        {
            public static GhostReactorManager GhostReactorManager
            {
                get => global::GhostReactor.instance.grManager;
            }

            public static GameEntityManager GameEntityManager
            {
                get => GameEntityManager.GetManagerForZone(global::GhostReactor.instance.zone);
            }
        }

        public class SuperInfection
        {
            public static SuperInfectionManager SuperInfectionManager
            {
                get => SuperInfectionManager.activeSuperInfectionManager;
            }

            public static global::SuperInfection ZoneSuperInfection
            {
                get => SuperInfectionManager.zoneSuperInfection;
            }

            public static GameEntityManager GameEntityManager
            {
                get => SuperInfectionManager.gameEntityManager;
            }
        }

        public class CustomMaps
        {
            public static CustomMapsGameManager CustomMapsGameManager
            {
                get => CustomMapsGameManager.instance;
            }

            public static GameEntityManager GameEntityManager
            {
                get => CustomMapsGameManager.gameEntityManager;
            }
        }

        public static BuilderTable BuilderTable
        {
            get => GetBuilderTable();
        }

        private static LightningManager _lightningManager;
        public static LightningManager LightningManager
        {
            get
            {
                if (_lightningManager == null)
                    _lightningManager = Menu.Main.GetObject("Environment Objects/05Maze_PersistentObjects/2025_Halloween1_PersistentObjects/LightningManager").GetComponent<LightningManager>();

                return _lightningManager;
            }
            set => _lightningManager = value;
        }
        #endregion

        #region Private Methods
        private static BuilderTable GetBuilderTable()
        {
            BuilderTable.TryGetBuilderTableForZone(VRRig.LocalRig.zoneEntity.currentZone, out BuilderTable table);
            return table;
        }
        #endregion
    }
}
