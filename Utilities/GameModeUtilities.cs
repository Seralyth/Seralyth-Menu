/*
 * United Goyim College Fund  Utilities/GameModeUtilities.cs
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

using GorillaGameModes;
using GorillaTagScripts;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;

namespace Seralyth.Utilities
{
    public class GameModeUtilities
    {
        public static List<NetPlayer> InfectedList()
        {
            List<NetPlayer> infected = new List<NetPlayer>();

            if (!NetworkSystem.Instance.InRoom || GorillaGameManager.instance == null)
                return infected;

            switch (GorillaGameManager.instance.GameType())
            {
                case GameModeType.Infection:
                case GameModeType.InfectionCompetitive:
                case GameModeType.SuperInfect:
                case GameModeType.FreezeTag:
                case GameModeType.PropHunt:
                    if (GorillaGameManager.instance is GorillaTagManager tagManager)
                    {
                        if (tagManager.isCurrentlyTag)
                        {
                            if (tagManager.currentIt != null)
                                infected.Add(tagManager.currentIt);
                        }
                        else if (tagManager.currentInfected != null)
                        {
                            infected.AddRange(tagManager.currentInfected.Where(element => element != null));
                        }
                    }
                    break;

                case GameModeType.Ghost:
                case GameModeType.Ambush:
                    if (GorillaGameManager.instance is GorillaAmbushManager ghostManager)
                    {
                        if (ghostManager.isCurrentlyTag)
                        {
                            if (ghostManager.currentIt != null)
                                infected.Add(ghostManager.currentIt);
                        }
                        else if (ghostManager.currentInfected != null)
                        {
                            infected.AddRange(ghostManager.currentInfected.Where(element => element != null));
                        }
                    }
                    break;

                case GameModeType.Paintbrawl:
                    if (GorillaGameManager.instance is GorillaPaintbrawlManager paintbrawlManager && paintbrawlManager.playerLives != null)
                    {
                        infected.AddRange(
                            paintbrawlManager.playerLives
                                .Where(element => element.Value <= 0)
                                .Select(element => PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(element.Key))
                                .Where(dummy => dummy != null)
                                .Select(dummy => (NetPlayer)dummy)
                        );

                        if (NetworkSystem.Instance?.LocalPlayer != null && !infected.Contains(NetworkSystem.Instance.LocalPlayer))
                            infected.Add(NetworkSystem.Instance.LocalPlayer);
                    }
                    break;
            }

            return infected;
        }

        public static void AddInfected(NetPlayer plr)
        {
            if (!NetworkSystem.Instance.InRoom || GorillaGameManager.instance == null || plr == null)
                return;

            switch (GorillaGameManager.instance.GameType())
            {
                case GameModeType.Infection:
                case GameModeType.InfectionCompetitive:
                case GameModeType.SuperInfect:
                case GameModeType.FreezeTag:
                case GameModeType.PropHunt:
                    if (GorillaGameManager.instance is GorillaTagManager tagManager)
                    {
                        if (tagManager.isCurrentlyTag)
                            tagManager.ChangeCurrentIt(plr);
                        else if (tagManager.currentInfected != null && !tagManager.currentInfected.Contains(plr))
                            tagManager.AddInfectedPlayer(plr);
                    }
                    break;

                case GameModeType.Ghost:
                case GameModeType.Ambush:
                    if (GorillaGameManager.instance is GorillaAmbushManager ghostManager)
                    {
                        if (ghostManager.isCurrentlyTag)
                            ghostManager.ChangeCurrentIt(plr);
                        else if (ghostManager.currentInfected != null && !ghostManager.currentInfected.Contains(plr))
                            ghostManager.AddInfectedPlayer(plr);
                    }
                    break;

                case GameModeType.Paintbrawl:
                    if (GorillaGameManager.instance is GorillaPaintbrawlManager paintbrawlManager && paintbrawlManager.playerLives != null)
                        paintbrawlManager.playerLives[plr.ActorNumber] = 0;

                    break;
            }
        }

        public static void RemoveInfected(NetPlayer plr)
        {
            if (!NetworkSystem.Instance.InRoom || GorillaGameManager.instance == null || plr == null)
                return;

            switch (GorillaGameManager.instance.GameType())
            {
                case GameModeType.Infection:
                case GameModeType.InfectionCompetitive:
                case GameModeType.SuperInfect:
                case GameModeType.FreezeTag:
                case GameModeType.PropHunt:
                    if (GorillaGameManager.instance is GorillaTagManager tagManager)
                    {
                        switch (tagManager.isCurrentlyTag)
                        {
                            case true when tagManager.currentIt == plr:
                                tagManager.currentIt = null;
                                break;
                            case false when tagManager.currentInfected != null && tagManager.currentInfected.Contains(plr):
                                tagManager.currentInfected.Remove(plr);
                                break;
                        }
                    }
                    break;

                case GameModeType.Ghost:
                case GameModeType.Ambush:
                    if (GorillaGameManager.instance is GorillaAmbushManager ghostManager)
                    {
                        switch (ghostManager.isCurrentlyTag)
                        {
                            case true when ghostManager.currentIt == plr:
                                ghostManager.currentIt = null;
                                break;
                            case false when ghostManager.currentInfected != null && ghostManager.currentInfected.Contains(plr):
                                ghostManager.currentInfected.Remove(plr);
                                break;
                        }
                    }
                    break;

                case GameModeType.Paintbrawl:
                    if (GorillaGameManager.instance is GorillaPaintbrawlManager paintbrawlManager && paintbrawlManager.playerLives != null)
                        paintbrawlManager.playerLives[plr.ActorNumber] = 3;

                    break;
            }
        }

        public static void AddRock(NetPlayer plr)
        {
            if (!NetworkSystem.Instance.InRoom || GorillaGameManager.instance == null || plr == null)
                return;

            switch (GorillaGameManager.instance.GameType())
            {
                case GameModeType.Infection:
                case GameModeType.InfectionCompetitive:
                case GameModeType.SuperInfect:
                case GameModeType.FreezeTag:
                case GameModeType.PropHunt:
                    if (GorillaGameManager.instance is GorillaTagManager tagManager)
                        tagManager.ChangeCurrentIt(plr);

                    break;

                case GameModeType.Ghost:
                case GameModeType.Ambush:
                    if (GorillaGameManager.instance is GorillaAmbushManager ghostManager)
                        ghostManager.ChangeCurrentIt(plr);

                    break;

                case GameModeType.Paintbrawl:
                    if (GorillaGameManager.instance is GorillaPaintbrawlManager paintbrawlManager && paintbrawlManager.playerLives != null)
                        paintbrawlManager.playerLives[plr.ActorNumber] = 0;

                    break;
            }
        }

        public static void RemoveRock(NetPlayer plr)
        {
            if (!NetworkSystem.Instance.InRoom || GorillaGameManager.instance == null || plr == null)
                return;

            switch (GorillaGameManager.instance.GameType())
            {
                case GameModeType.Infection:
                case GameModeType.InfectionCompetitive:
                case GameModeType.SuperInfect:
                case GameModeType.FreezeTag:
                case GameModeType.PropHunt:
                    if (GorillaGameManager.instance is GorillaTagManager tagManager && tagManager.currentIt == plr)
                        tagManager.ChangeCurrentIt(null);

                    break;

                case GameModeType.Ghost:
                case GameModeType.Ambush:
                    if (GorillaGameManager.instance is GorillaAmbushManager ghostManager && ghostManager.currentIt == plr)
                        ghostManager.ChangeCurrentIt(null);

                    break;

                case GameModeType.Paintbrawl:
                    if (GorillaGameManager.instance is GorillaPaintbrawlManager paintbrawlManager && paintbrawlManager.playerLives != null)
                        paintbrawlManager.playerLives[plr.ActorNumber] = 3;

                    break;
            }
        }
    }
}
