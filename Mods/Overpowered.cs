/*
 * United Goyim College Fund  Mods/Overpowered.cs
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

using ExitGames.Client.Photon;
using GorillaExtensions;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaLocomotion.Gameplay;
using GorillaNetworking;
using GorillaTagScripts;
using GorillaTagScripts.VirtualStumpCustomMaps;
using Ionic.Zlib;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice;
using Photon.Voice.PUN;
using Seralyth.Extensions;
using Seralyth.Managers;
using Seralyth.Menu;
using Seralyth.Patches.Menu;
using Seralyth.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;
using static Seralyth.Menu.Main;
using static Seralyth.Utilities.AssetUtilities;
using static Seralyth.Utilities.GameModeUtilities;
using static Seralyth.Utilities.RandomUtilities;
using static Seralyth.Utilities.RigUtilities;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using JoinType = GorillaNetworking.JoinType;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Seralyth.Mods
{
    public static class Overpowered
    {
        public static void VIMKickGun()
        {
            if (!VRRig.LocalRig.IsVIMSubscriber())
            {
                PromptSingle("You are not a VIM subscriber, so this mod will not function.");
                Buttons.GetIndex("VIM Kick Gun").SetEnabled(false);
                return;
            }
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        RoomControls.KickPlayer(gunTarget.GetPlayer().ActorNumber);
                    }
                }
            }
        }

        public static void VIMKickAll()
        {
            if (!VRRig.LocalRig.IsVIMSubscriber())
            {
                PromptSingle("You are not a VIM subscriber, so this mod will not function.");
                return;
            }
            NetworkSystem.Instance.PlayerListOthers.ForEach(p => RoomControls.KickPlayer(p.ActorNumber));
        }

        public static void VIMBlockGun()
        {
            if (!VRRig.LocalRig.IsVIMSubscriber())
            {
                PromptSingle("You are not a VIM subscriber, so this mod will not function.");
                Buttons.GetIndex("VIM Block Gun").SetEnabled(false);
                return;
            }
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        RoomControls.KickAndBlockPlayer(gunTarget.GetPlayer().ActorNumber);
                    }
                }
            }
        }

        public static void VIMBlockAll()
        {
            if (!VRRig.LocalRig.IsVIMSubscriber())
            {
                PromptSingle("You are not a VIM subscriber, so this mod will not function.");
                return;
            }
            NetworkSystem.Instance.PlayerListOthers.ForEach(p => RoomControls.KickAndBlockPlayer(p.ActorNumber));
        }

        public static void VIMMuteGun()
        {
            if (!VRRig.LocalRig.IsVIMSubscriber())
            {
                PromptSingle("You are not a VIM subscriber, so this mod will not function.");
                Buttons.GetIndex("VIM Mute Gun").SetEnabled(false);
                return;
            }
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null && !RoomControls.MutedPlayers.ContainsKey(lockTarget.GetPlayer().UserId))
                    RoomControls.MutePlayer(lockTarget.GetPlayer().ActorNumber);
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal() && !gunTarget.IsTagged())
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            gunLocked = true;
                            lockTarget = gunTarget;
                        }
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void VIMMuteAll()
        {
            if (!VRRig.LocalRig.IsVIMSubscriber())
            {
                PromptSingle("You are not a VIM subscriber, so this mod will not function.");
                return;
            }
            NetworkSystem.Instance.PlayerListOthers.ForEach(p => RoomControls.MutePlayer(p.ActorNumber));
        }
        public static void VIMUnmuteAll() =>
            NetworkSystem.Instance.PlayerListOthers.ForEach(p => RoomControls.UnmutePlayer(p.UserId));

        public static void SetGuardianTarget(NetPlayer target)
        {
            if (!NetworkSystem.Instance.IsMasterClient) { NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client."); return; }
            GorillaGuardianManager guardianManager = (GorillaGuardianManager)GorillaGameManager.instance;
            if (guardianManager.IsPlayerGuardian(target))
                return;

            foreach (TappableGuardianIdol tgi in GetAllType<TappableGuardianIdol>())
            {
                if (tgi.manager && tgi.manager.photonView && !tgi.isChangingPositions)
                {
                    GorillaGuardianZoneManager zoneManager = tgi.zoneManager;
                    if (zoneManager.IsZoneValid() && tgi.manager && zoneManager.CurrentGuardian == null)
                    {
                        zoneManager.SetGuardian(target);
                        return;
                    }
                }
            }
        }

        public static void GuardianSelf() =>
            SetGuardianTarget(PhotonNetwork.LocalPlayer);

        private static float guardianDelay;
        public static void GuardianGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > guardianDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        SetGuardianTarget(GetPlayerFromVRRig(gunTarget));
                        guardianDelay = Time.time + 0.1f;
                    }
                }
            }
        }

        public static void GuardianAll()
        {
            if (NetworkSystem.Instance.IsMasterClient)
            {
                int i = 0;
                foreach (var gorillaGuardianZoneManager in GorillaGuardianZoneManager.zoneManagers.Where(gorillaGuardianZoneManager => gorillaGuardianZoneManager.enabled && gorillaGuardianZoneManager.IsZoneValid()))
                {
                    gorillaGuardianZoneManager.SetGuardian(PhotonNetwork.PlayerList[i]);
                    i++;
                }
            }
            else NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void UnguardianSelf()
        {
            if (NetworkSystem.Instance.IsMasterClient)
            {
                foreach (var gorillaGuardianZoneManager in GorillaGuardianZoneManager.zoneManagers.Where(gorillaGuardianZoneManager => gorillaGuardianZoneManager.enabled && gorillaGuardianZoneManager.IsZoneValid()).Where(gorillaGuardianZoneManager => gorillaGuardianZoneManager.CurrentGuardian == NetworkSystem.Instance.LocalPlayer))
                    gorillaGuardianZoneManager.SetGuardian(null);
            }
            else NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void UnguardianGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > guardianDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        if (NetworkSystem.Instance.IsMasterClient)
                        {
                            foreach (var gorillaGuardianZoneManager in GorillaGuardianZoneManager.zoneManagers.Where(gorillaGuardianZoneManager => gorillaGuardianZoneManager.enabled && gorillaGuardianZoneManager.IsZoneValid()).Where(gorillaGuardianZoneManager => gorillaGuardianZoneManager.CurrentGuardian == GetPlayerFromVRRig(gunTarget)))
                                gorillaGuardianZoneManager.SetGuardian(null);
                        }
                        else NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                        guardianDelay = Time.time + 0.1f;
                    }
                }
            }
        }

        public static void UnguardianAll()
        {
            if (NetworkSystem.Instance.IsMasterClient)
            {
                foreach (var gorillaGuardianZoneManager in GorillaGuardianZoneManager.zoneManagers.Where(gorillaGuardianZoneManager => gorillaGuardianZoneManager.enabled && gorillaGuardianZoneManager.IsZoneValid()))
                    gorillaGuardianZoneManager.SetGuardian(null);
            }
            else NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void SetPlayerColors(Dictionary<int, int> colors) // ActorNumber : Team // 0 = Blue, 1 = Red, -1 = None
        {
            var filteredPlayers = NetworkSystem.Instance.AllNetPlayers
                .Where(p => colors.ContainsKey(p.ActorNumber));
            MonkeBallGame.Instance.photonView.RPC(
                "RequestSetGameStateRPC",
                RpcTarget.All,
                (int)MonkeBallGame.GameState.Playing,
                PhotonNetwork.Time + (MonkeBallGame.Instance.gameDuration - 1f),
                filteredPlayers.Select(p => p.ActorNumber).ToArray(),
                filteredPlayers.Select(p => colors[p.ActorNumber]).ToArray(),
                new int[MonkeBallGame.Instance.team.Count],
                MonkeBallGame.Instance.startingBalls
                    .Select(ball => BitPackUtils.PackHandPosRotForNetwork(ball.transform.position, ball.transform.rotation))
                    .ToArray(),
                MonkeBallGame.Instance.startingBalls
                    .Select(ball => BitPackUtils.PackWorldPosForNetwork(ball.gameBall.GetVelocity()))
                    .ToArray()
            );
        }

        private static float playerColorDelay;
        public static void SetColorSelf(int color) =>
            SetPlayerColors(new Dictionary<int, int> { { NetworkSystem.Instance.LocalPlayer.ActorNumber, color } });

        public static void SetColorGun(int color)
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > playerColorDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        playerColorDelay = Time.time + 0.1f;
                        if (PhotonNetwork.IsMasterClient)
                            SetPlayerColors(new Dictionary<int, int> { { GetPlayerFromVRRig(gunTarget).ActorNumber, color } });
                        else
                            NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                    }
                }
            }
        }

        public static void SetColorAll(int color)
        {
            if (PhotonNetwork.IsMasterClient)
                SetPlayerColors(NetworkSystem.Instance.AllNetPlayers.ToDictionary(p => p.ActorNumber, p => color));
            else
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void StrobeColorSelf()
        {
            if (Time.time > playerColorDelay)
            {
                playerColorDelay = Time.time + 0.1f;
                if (NetworkSystem.Instance.IsMasterClient)
                    SetColorSelf(Time.time % 0.2f > 0.1f ? 1 : 0);
                else
                    NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
            }
        }

        public static void StrobeColorGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > playerColorDelay)
                    {
                        playerColorDelay = Time.time + 0.1f;
                        if (NetworkSystem.Instance.IsMasterClient)
                            SetPlayerColors(new Dictionary<int, int> { { lockTarget.GetPlayer().ActorNumber, Time.time % 0.2f > 0.1f ? 1 : 0 } });
                        else
                            NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal() && !gunTarget.IsTagged())
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            gunLocked = true;
                            lockTarget = gunTarget;
                        }
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void StrobeColorAll()
        {
            if (Time.time > playerColorDelay)
            {
                playerColorDelay = Time.time + 0.1f;
                if (NetworkSystem.Instance.IsMasterClient)
                    SetColorAll(Time.time % 0.2f > 0.1f ? 1 : 0);
                else
                    NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
            }
        }

        private static readonly Dictionary<VRRig, int> materialState = new Dictionary<VRRig, int>();
        public static float materialDelay;
        public static void MaterialTarget(VRRig rig)
        {
            if (!NetworkSystem.Instance.IsMasterClient)
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                return;
            }

            NetPlayer player = rig.GetPlayer();
            materialState.TryGetValue(rig, out var state);

            state++;
            state %= 6;

            if (GorillaGameManager.instance.GameType() == GameModeType.Casual)
            {
                if (state < 4)
                    state = 4;
            }

            materialState[rig] = state;

            switch (state)
            {
                case 0:
                    AddInfected(player);
                    break;
                case 1:
                    RemoveInfected(player);
                    break;
                case 2:
                    AddRock(player);
                    break;
                case 3:
                    RemoveRock(player);
                    break;
                case 4:
                    SetPlayerColors(new Dictionary<int, int> { { player.ActorNumber, 0 } });
                    break;
                case 5:
                    SetPlayerColors(new Dictionary<int, int> { { player.ActorNumber, 1 } });
                    break;
            }
        }

        public static void MaterialGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (!NetworkSystem.Instance.IsMasterClient)
                        NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                    else
                    {
                        if (Time.time > materialDelay)
                        {
                            materialDelay = Time.time + 0.1f;
                            MaterialTarget(lockTarget);
                        }
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal() && !gunTarget.IsTagged())
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            gunLocked = true;
                            lockTarget = gunTarget;
                        }
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void MaterialAll()
        {
            if (Time.time > materialDelay)
            {
                materialDelay = Time.time + 0.1f;
                foreach (VRRig rig in VRRigExtensions.ActiveRigs)
                    MaterialTarget(rig);
            }
        }

        private static float guardianSpazDelay;
        private static bool guardianSpazToggle;
        public static void GuardianSpaz()
        {
            if (Time.time > guardianSpazDelay)
            {
                guardianSpazDelay = Time.time + 0.1f;
                guardianSpazToggle = !guardianSpazToggle;
                if (guardianSpazToggle)
                    GuardianAll();
                else
                    UnguardianAll();
            }
        }

        public static float alwaysGuardianDelay;
        public static void AlwaysGuardian()
        {
            if (NetworkSystem.Instance.InRoom)
            {
                if (GorillaGameManager.instance.GameType() != GameModeType.Guardian)
                    return;

                if (NetworkSystem.Instance.IsMasterClient)
                {
                    if (!VRRig.LocalRig.enabled)
                        VRRig.LocalRig.enabled = true;
                    GorillaGuardianManager guardianManager = (GorillaGuardianManager)GorillaGameManager.instance;
                    if (!guardianManager.IsPlayerGuardian(PhotonNetwork.LocalPlayer))
                        SetGuardianTarget(PhotonNetwork.LocalPlayer);
                }
                else
                {
                    GorillaGuardianManager guardianManager = (GorillaGuardianManager)GorillaGameManager.instance;
                    foreach (TappableGuardianIdol tgi in GetAllType<TappableGuardianIdol>())
                    {
                        if (tgi.manager && tgi.manager.photonView && !tgi.isChangingPositions)
                        {
                            GorillaGuardianZoneManager zoneManager = tgi.zoneManager;
                            if (!guardianManager.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer) && zoneManager.IsZoneValid() && tgi.manager)
                            {
                                VRRig.LocalRig.enabled = false;
                                VRRig.LocalRig.transform.position = tgi.transform.position;
                                VRRig.LocalRig.leftHand.rigTarget.transform.position = tgi.transform.position;
                                VRRig.LocalRig.rightHand.rigTarget.transform.position = tgi.transform.position;

                                if (Time.time > alwaysGuardianDelay)
                                {
                                    alwaysGuardianDelay = Time.time + (zoneManager._currentActivationTime >= zoneManager.requiredActivationTime - 1f ? 0f : 0.2f);
                                    tgi.OnTap(1);
                                    RPCProtection();
                                }
                            }
                        }
                        else
                            VRRig.LocalRig.enabled = true;
                    }
                }
            }
        }

        public static float guardianProtectorDelay;
        public static void GuardianProtector()
        {
            if (NetworkSystem.Instance.InRoom)
            {
                GorillaGuardianManager manager = (GorillaGuardianManager)GorillaGameManager.instance;

                if (!manager.IsPlayerGuardian(PhotonNetwork.LocalPlayer)) return;
                foreach (TappableGuardianIdol tgi in GetAllType<TappableGuardianIdol>())
                {
                    if (!tgi.manager || !tgi.manager.photonView) continue;
                    foreach (var rig in VRRigExtensions.ActiveRigs.Where(rig => !rig.isLocal && Vector3.Distance(rig.transform.position, tgi.transform.position) < 2f && Time.time > guardianProtectorDelay))
                    {
                        BetaSetVelocityPlayer(rig.GetPlayer(), (rig.transform.position - tgi.transform.position).normalized * 50f);
                        guardianProtectorDelay = Time.time + 0.1f;
                    }
                }
            }
        }

        public static void GuardianKickTarget(NetPlayer target)
        {
            if (Time.time > crashAllDelay)
            {
                crashAllDelay = Time.time + 0.1f;
                VRRig rig = GetVRRigFromPlayer(target);
                BetaSetVelocityPlayer(target, rig.transform.position.z < -28.5f ? (new Vector3(-47.82025f, 6.460508f, -29.04836f) - rig.transform.position).normalized * 50f : rig.transform.position.z < -23f ? new Vector3(-50f, 0f, 50f) : Vector3.left * 50f);
                RPCProtection();
            }
        }

        private static float crashAllDelay;
        public static void GuardianKickGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > crashAllDelay)
                    {
                        crashAllDelay = Time.time + 0.1f;
                        BetaSetVelocityPlayer(lockTarget.GetPlayer(), lockTarget.transform.position.z < -28.5f ? (new Vector3(-47.82025f, 6.460508f, -29.04836f) - lockTarget.transform.position).normalized * 50f : lockTarget.transform.position.z < -23f ? new Vector3(-50f, 0f, 50f) : Vector3.left * 50f);
                        RPCProtection();
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void GuardianKickAll()
        {
            if (rightTrigger > 0.5f && Time.time > crashAllDelay)
            {
                crashAllDelay = Time.time + 0.1f;
                foreach (var rig in VRRigExtensions.ActiveRigs.Where(rig => !rig.isLocal))
                {
                    BetaSetVelocityPlayer(rig.GetPlayer(), rig.transform.position.z < -28.5f ? (new Vector3(-47.82025f, 6.460508f, -29.04836f) - rig.transform.position).normalized * 50f : rig.transform.position.z < -23f ? new Vector3(-50f, 0f, 50f) : Vector3.left * 50f);
                    RPCProtection();
                }
            }
        }

        public static void GuardianCrashPlayer(NetPlayer target)
        {
            VRRig rig = GetVRRigFromPlayer(target);
            if (Time.time > crashAllDelay && rig.transform.position.x < -5)
            {
                crashAllDelay = Time.time + 0.1f;

                BetaSetVelocityPlayer(target, (rig.transform.position.y > 55f ? Vector3.right : Vector3.up) * 50f);
                RPCProtection();
            }
        }

        public static void GuardianCrashGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > crashAllDelay && lockTarget.transform.position.x < -5)
                    {
                        crashAllDelay = Time.time + 0.1f;
                        BetaSetVelocityPlayer(lockTarget.GetPlayer(), (lockTarget.transform.position.y > 55f ? Vector3.right : Vector3.up) * 50f);
                        RPCProtection();
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void GuardianCrashAll()
        {
            if (rightTrigger > 0.5f && Time.time > crashAllDelay)
            {
                crashAllDelay = Time.time + 0.1f;
                foreach (var rig in VRRigExtensions.ActiveRigs.Where(rig => !rig.isLocal && rig.transform.position.x < -5))
                {
                    BetaSetVelocityPlayer(rig.GetPlayer(), (rig.transform.position.y > 55f ? Vector3.right : Vector3.up) * 50f);
                    RPCProtection();
                }
            }
        }

        public static void DriverStatus(bool locked)
        {
            if (PhotonNetwork.IsMasterClient)
                CustomMapsTerminal.instance.mapTerminalNetworkObject.SendRPC("SetTerminalControlStatus_RPC", true, locked, PhotonNetwork.LocalPlayer.ActorNumber);
            else
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        private static float spazDriverDelay;
        public static void SpazDriver()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (Time.time > spazDriverDelay)
                {
                    spazDriverDelay = Time.time + 0.1f;
                    CustomMapsTerminal.instance.mapTerminalNetworkObject.SendRPC("SetTerminalControlStatus_RPC", true, true, PhotonNetwork.LocalPlayer.ActorNumber);
                    CustomMapsTerminal.instance.mapTerminalNetworkObject.SendRPC("SetTerminalControlStatus_RPC", true, false, PhotonNetwork.LocalPlayer.ActorNumber);
                }
            }
        }

        public static void DriverStatusGun(bool locked)
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (PhotonNetwork.IsMasterClient)
                        CustomMapsTerminal.instance.mapTerminalNetworkObject.SendRPC("SetTerminalControlStatus_RPC", true, locked, lockTarget.GetPlayer().ActorNumber);
                }

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void SpazDriverStatusGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        if (Time.time > spazDriverDelay)
                        {
                            spazDriverDelay = Time.time + 0.1f;
                            CustomMapsTerminal.instance.mapTerminalNetworkObject.SendRPC("SetTerminalControlStatus_RPC", true, true, lockTarget.GetPlayer().ActorNumber);
                            CustomMapsTerminal.instance.mapTerminalNetworkObject.SendRPC("SetTerminalControlStatus_RPC", true, false, lockTarget.GetPlayer().ActorNumber);
                        }
                    }
                }

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void BecomeDriver()
        {
            if (PhotonNetwork.IsMasterClient)
                CustomMapsTerminal.instance.mapTerminalNetworkObject.SendRPC("SetTerminalControlStatus_RPC", true, true, PhotonNetwork.LocalPlayer.ActorNumber);
            else
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                return;
            }

            CustomMapsTerminal.instance.mapTerminalNetworkObject.photonView.OwnerActorNr = PhotonNetwork.LocalPlayer.ActorNumber;
        }

        private static long? id;
        private static float setMapDelay;
        public static void VirtualStumpKickGun()
        {
            if (!NetworkSystem.Instance.InRoom)
            {
                id = null;
                return;
            }

            if (!PhotonNetwork.IsMasterClient)
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                Buttons.GetIndex("Virtual Stump Kick Gun").SetEnabled(false);
                return;
            }

            if (id == null && Time.time > setMapDelay)
            {
                setMapDelay = Time.time + 1f;

                if (CustomMapsTerminal.GetDriverID() != PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    NotificationManager.SendNotification("<color=grey>[</color><color=purple>VSTUMP</color><color=grey>]</color> Gaining control of the terminal, please wait...");
                    BecomeDriver();
                    return;
                }

                if (CustomMapManager.IsRemotePlayerInVirtualStump(NetworkSystem.Instance.LocalPlayer.UserId))
                {
                    id = CustomMaps.Manager.currentMapId == 4977315 ? 5024157 : 4977315;

                    CustomMapsTerminal.instance.mapTerminalNetworkObject.photonView.RPC("UpdateScreen_RPC", lockTarget.GetPhotonPlayer(), new object[]
                    {
                        6,
                        id,
                        CustomMapsTerminal.GetDriverID()
                    });
                    NotificationManager.SendNotification("<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> You now have access to use the gun.");
                }
                else
                    NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> Please temporarily enter the Virtual Stump.");
            }

            if (GetGunInput(false) && id != null)
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                        CustomMapsTerminal.instance.mapTerminalNetworkObject.photonView.RPC("SetRoomMap_RPC", lockTarget.GetPhotonPlayer(), id.Value);
                }
            }
        }

        public static void VirtualStumpKickAll()
        {
            if (!NetworkSystem.Instance.InRoom)
            {
                id = null;
                return;
            }

            if (!PhotonNetwork.IsMasterClient)
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                Toggle("Virtual Stump Kick All");
                return;
            }

            if (id == null && Time.time > setMapDelay)
            {
                setMapDelay = Time.time + 1f;

                if (CustomMapsTerminal.GetDriverID() != PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    NotificationManager.SendNotification("<color=grey>[</color><color=purple>INFO</color><color=grey>]</color> Gaining control of the terminal, please wait...");
                    BecomeDriver();
                    return;
                }

                if (CustomMapManager.IsRemotePlayerInVirtualStump(NetworkSystem.Instance.LocalPlayer.UserId))
                {
                    id = CustomMaps.Manager.currentMapId == 4977315 ? 5024157 : 4977315;

                    CustomMapsTerminal.instance.mapTerminalNetworkObject.photonView.RPC("UpdateScreen_RPC", RpcTarget.Others, new object[]
                    {
                        6,
                        id,
                        CustomMapsTerminal.GetDriverID()
                    });
                    NotificationManager.SendNotification("<color=grey>[</color><color=purple>INFO</color><color=grey>]</color> Kicking...");
                }
                else
                    NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> Please temporarily enter the Virtual Stump.");
            }

            CustomMapsTerminal.instance.mapTerminalNetworkObject.photonView.RPC("SetRoomMap_RPC", RpcTarget.Others, id.Value);

            NotificationManager.SendNotification("<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> Successfully kicked others.");
            Toggle("Virtual Stump Kick All");
        }

        public const int ItemCrashCount = 500;
        public static void GameEntityCrash(GameEntityManager manager, object target, Vector3? targetPosition = null)
        {
            if (manager == null)
                return;

            targetPosition ??= GorillaTagger.Instance.bodyCollider.transform.position;

            int[] objectIds = manager.itemPrefabFactory.Keys.ToArray();
            int[] ids = new int[ItemCrashCount];
            Vector3[] positions = new Vector3[ItemCrashCount];
            Quaternion[] rotations = new Quaternion[ItemCrashCount];

            for (int i = 0; i < ItemCrashCount; i++)
            {
                ids[i] = objectIds[Random.Range(0, objectIds.Length)];
                positions[i] = targetPosition.Value;
                rotations[i] = Quaternion.identity;
            }

            CreateItems(target, ids, positions, rotations, manager: manager);
        }

        public static int masterVisualizationType;
        public static readonly string[] VisualizationTypeNames = { "Sphere", "Cube", "Tracer" };
        public static void ApplyMasterVisualizationType(int index) => masterVisualizationType = index;

        public static void VisualizeMasterClient()
        {
            if (Visuals.DoPerformanceCheck())
                return;

            if (NetworkSystem.Instance.IsMasterClient)
                return;

            VRRig rig = NetworkSystem.Instance.MasterClient.VRRig();
            if (rig == null)
                return;

            long visualizeId = 2017928;
            switch (masterVisualizationType)
            {
                case 0:
                    Visuals.Visualize(PrimitiveType.Sphere, rig.transform.position, Quaternion.identity, new Vector3(0.15f, 0.15f, 0.15f), Color.blue, visualizeId, 0.1f);
                    break;
                case 1:
                    Visuals.Visualize(PrimitiveType.Cube, rig.transform.position, Quaternion.Euler(Time.time * 90, Time.time * 60, Time.time * 30), new Vector3(0.3f, 0.3f, 0.3f), Color.blue);
                    break;
                case 2:
                    LineRenderer line = Visuals.GetLineRender();

                    line.startColor = Color.blue;
                    line.endColor = Color.blue;
                    float width = 0.025f;
                    line.startWidth = width;
                    line.endWidth = width;
                    line.SetPosition(0, rig.transform.position);
                    line.SetPosition(1, GorillaTagger.Instance.rightHandTransform.position);
                    break;
            }
        }

        public static void MasterVirtualStumpCrashGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    GameEntityCrash(ManagerRegistry.CustomMaps.GameEntityManager, lockTarget.GetPhotonPlayer(), lockTarget.transform.position);

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void MasterVirtualStumpCrashAll() =>
            GameEntityCrash(ManagerRegistry.CustomMaps.GameEntityManager, RpcTarget.Others);

        public static void GhostReactorCrashGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    GameEntityCrash(ManagerRegistry.GhostReactor.GameEntityManager, lockTarget.GetPhotonPlayer(), lockTarget.transform.position);

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void GhostReactorCrashAll() =>
            GameEntityCrash(ManagerRegistry.GhostReactor.GameEntityManager, RpcTarget.Others);

        public static void SuperInfectionCrashGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    GameEntityCrash(ManagerRegistry.SuperInfection.GameEntityManager, lockTarget.GetPhotonPlayer(), lockTarget.transform.position);

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void SuperInfectionCrashAll() =>
            GameEntityCrash(ManagerRegistry.SuperInfection.GameEntityManager, RpcTarget.Others);

        public static void SuperInfectionBreakAudioGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    CreateItem(lockTarget.GetPlayer(), GadgetByName["WristJetGadgetPropellor"], lockTarget.transform.position, RandomQuaternion(), Vector3.zero, Vector3.zero, 0L, ManagerRegistry.SuperInfection.GameEntityManager);

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void SuperInfectionBreakAudioAll() =>
            CreateItem(RpcTarget.Others, GadgetByName["WristJetGadgetPropellor"], GorillaTagger.Instance.bodyCollider.transform.position, RandomQuaternion(), Vector3.zero, Vector3.zero, 0L, ManagerRegistry.SuperInfection.GameEntityManager);

        private static float reportDelay;
        public static void DelayBanGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal() && !gunLocked)
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;

                        if (VRRig.LocalRig.IsTagged())
                        {
                            NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must not be tagged.");
                            return;
                        }

                        if (!lockTarget.IsTagged())
                        {
                            NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> The target must be tagged.");
                            return;
                        }

                        if (PhotonNetwork.IsMasterClient)
                        {
                            NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must not be master client.");
                            return;
                        }

                        if (Time.time > reportDelay)
                        {
                            reportDelay = Time.time + 0.5f;
                            GorillaPlayerScoreboardLine.ReportPlayer(lockTarget.GetPlayer().UserId, GorillaPlayerLineButton.ButtonType.Cheating, lockTarget.GetPlayer().NickName);
                        }

                        SerializePatch.OverrideSerialization = () =>
                        {
                            lockTarget.GetPlayer();
                            MassSerialize(true, new[] { VRRig.LocalRig.GetPhotonView() });

                            Vector3 positionArchive = VRRig.LocalRig.transform.position;
                            SendSerialize(VRRig.LocalRig.GetPhotonView(), new RaiseEventOptions { TargetActors = PhotonNetwork.PlayerList.Where(plr => !(new[] { PhotonNetwork.MasterClient.ActorNumber, lockTarget.GetPlayer().ActorNumber }).Contains(plr.ActorNumber)).Select(plr => plr.ActorNumber).ToArray() });

                            VRRig.LocalRig.transform.position = new Vector3(99999f, 99999f, 99999f);
                            SendSerialize(VRRig.LocalRig.GetPhotonView(), new RaiseEventOptions { TargetActors = new[] { PhotonNetwork.MasterClient.ActorNumber } });

                            VRRig.LocalRig.transform.position = lockTarget.rightHandTransform.position;
                            SendSerialize(VRRig.LocalRig.GetPhotonView(), new RaiseEventOptions { TargetActors = new[] { lockTarget.GetPlayer().ActorNumber } });

                            RPCProtection();
                            VRRig.LocalRig.transform.position = positionArchive;

                            return false;
                        };
                    }
                }
            }
            else
            {
                if (gunLocked)
                {
                    gunLocked = false;
                    SerializePatch.OverrideSerialization = null;
                }
            }
        }

        public static void DelayBanAll()
        {
            SerializePatch.OverrideSerialization = () =>
            {
                if (VRRig.LocalRig.IsTagged())
                {
                    NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must not be tagged.");
                    return true;
                }

                if (PhotonNetwork.IsMasterClient)
                {
                    NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must not be master client.");
                    return true;
                }

                lockTarget.GetPlayer();
                MassSerialize(true, new[] { VRRig.LocalRig.GetPhotonView() });

                Vector3 positionArchive = VRRig.LocalRig.transform.position;
                SendSerialize(VRRig.LocalRig.GetPhotonView(), new RaiseEventOptions { TargetActors = PhotonNetwork.PlayerList.Where(player => !player.IsMasterClient && player.VRRig().IsTagged()).Select(player => player.ActorNumber).ToArray() });

                VRRig.LocalRig.transform.position = new Vector3(99999f, 99999f, 99999f);
                SendSerialize(VRRig.LocalRig.GetPhotonView(), new RaiseEventOptions { TargetActors = new[] { PhotonNetwork.MasterClient.ActorNumber } });

                foreach (NetPlayer player in NetworkSystem.Instance.PlayerListOthers)
                {
                    VRRig rig = GetVRRigFromPlayer(player);
                    if (!player.IsMasterClient && rig.IsTagged())
                    {
                        VRRig.LocalRig.transform.position = rig.rightHandTransform.position;
                        SendSerialize(VRRig.LocalRig.GetPhotonView(), new RaiseEventOptions { TargetActors = new[] { player.ActorNumber } });
                    }
                }

                RPCProtection();
                VRRig.LocalRig.transform.position = positionArchive;

                return false;
            };
        }

        public static void GuardianObliteratePlayer(NetPlayer target)
        {
            if (Time.time > crashAllDelay)
            {
                crashAllDelay = Time.time + 0.1f;
                VRRig rig = GetVRRigFromPlayer(target);
                BetaSetVelocityPlayer(target, (rig.transform.position.y > 55f ? Vector3.right : Vector3.up) * 50f);
                RPCProtection();
            }
        }


        public static void GiveFlyOnGrab()
        {
            foreach (VRRig rig in VRRigExtensions.ActiveRigs)
            {
                if (!rig.isLocal)
                {
                    if (rig.leftHandLink.grabbedPlayer == NetworkSystem.Instance.LocalPlayer || rig.rightHandLink.grabbedPlayer == NetworkSystem.Instance.LocalPlayer)
                    {
                        bool grabbedOnLeft = VRRig.LocalRig.leftHandLink.grabbedPlayer == rig.GetPlayer();
                        if (grabbedOnLeft ? rig.leftIndex.calcT > 0 : rig.rightIndex.calcT > 0)
                        {
                            GTPlayer.Instance.transform.position += rig.headMesh.transform.forward * (Time.deltaTime * Movement.FlySpeed);
                            GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
                        }
                    }
                }
            }
        }



        public static void ForceGrab(Vector3 targetTransform)
        {
            foreach (VRRig rig in VRRigExtensions.ActiveRigs)
                ForceGrab(rig, targetTransform);
        }

        public static bool ForceGrab(VRRig rig, Vector3 targetTransform, bool returnOnGrab = false, bool enableRigOnceDone = false)
        {
            if (rig == null || rig.IsLocal()) return false;

            bool isLeftHand = rig.IsLeftHandGrabbable();
            bool isRightHand = rig.IsRightHandGrabbable();

            if (!isLeftHand && !isRightHand)
            {
                VRRig.LocalRig.enabled = true;
                return false;
            }

            VRRig.LocalRig.enabled = false;
            VRRig.LocalRig.transform.position = rig.syncPos;

            var localLink = isLeftHand ? VRRig.LocalRig.leftHandLink : VRRig.LocalRig.rightHandLink;
            var remoteLink = isLeftHand ? rig.leftHandLink : rig.rightHandLink;

            if (remoteLink.grabbedPlayer != NetworkSystem.Instance.LocalPlayer)
            {
                if (grabDelay == 0)
                    grabDelay = remoteLink.rejectGrabsUntilTimestamp > Time.time ? remoteLink.rejectGrabsUntilTimestamp : Time.time + 1f;

                if (Time.time > grabDelay)
                {
                    localLink.TentacleTryCreateLink(remoteLink);
                    VRRig.LocalRig.transform.position = targetTransform;
                    NotificationManager.SendNotification("<color=grey>[</color><color=purple>MENU</color><color=grey>]</color> Tried to grab " + rig.GetPlayer().NickName + ".");
                    grabDelay = remoteLink.rejectGrabsUntilTimestamp > Time.time ? remoteLink.rejectGrabsUntilTimestamp : Time.time + 1f;
                }
            }
            else if (returnOnGrab)
            {
                if (enableRigOnceDone && !VRRig.LocalRig.enabled)
                    VRRig.LocalRig.enabled = true;
                return true;
            }

            return false;
        }


        public static void FlingOnGrab()
        {
            if (VRRig.LocalRig.IsBeingHeld())
            {
                Transform transform = VRRig.LocalRig.leftHandLink.IsLinkActive() ? VRRig.LocalRig.leftHandTransform : VRRig.LocalRig.rightHandTransform;
                VRRig rig = VRRig.LocalRig.leftHandLink.grabbedPlayer.VRRig() ?? VRRig.LocalRig.rightHandLink.grabbedPlayer.VRRig();
                Vector3 velocity = rig.transform.up * 3f;
                rig.GetNetView().SendRPC("DroppedByPlayer", rig.GetPlayer(), velocity);
            }
        }

        private static float propHuntSpazDelay;
        private static bool propHuntSpazMode;
        public static void SpazPropHuntObjects()
        {
            if (Time.time > propHuntSpazDelay)
            {
                propHuntSpazDelay = Time.time + 0.1f;
                propHuntSpazMode = !propHuntSpazMode;

                if (!NetworkSystem.Instance.IsMasterClient) { NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client."); return; }

                if (NetworkSystem.Instance.InRoom && GorillaGameManager.instance.GameType() == GameModeType.PropHunt)
                {
                    GorillaPropHuntGameManager hauntManager = (GorillaPropHuntGameManager)GorillaGameManager.instance;
                    hauntManager._ph_timeRoundStartedMillis = propHuntSpazMode ? 1 : 2;
                    hauntManager._ph_randomSeed = Random.Range(1, int.MaxValue);
                }
            }
        }

        public static void SpazPropHunt()
        {
            if (Time.time > propHuntSpazDelay)
            {
                propHuntSpazDelay = Time.time + 0.1f;
                propHuntSpazMode = !propHuntSpazMode;

                if (!NetworkSystem.Instance.IsMasterClient) { NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client."); return; }

                if (NetworkSystem.Instance.InRoom && GorillaGameManager.instance.GameType() == GameModeType.PropHunt)
                {
                    GorillaPropHuntGameManager hauntManager = (GorillaPropHuntGameManager)GorillaGameManager.instance;
                    hauntManager._ph_timeRoundStartedMillis = propHuntSpazMode ? 0 : 1;
                }
            }
        }

        public static void CreateItem(object target, int hash, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angVelocity, long sendData = 0L, GameEntityManager manager = null)
        {
            GameEntityManager gameEntityManager = manager ?? ManagerRegistry.GhostReactor.GameEntityManager;
            if (NetworkSystem.Instance.IsMasterClient)
            {
                if (gameEntityManager.m_RpcSpamChecks.m_callLimiters[(int)GameEntityManager.RPC.CreateItem].CanCallNow())
                {
                    int netId = gameEntityManager.CreateTypeNetId(hash);

                    if (target is NetPlayer netPlayer)
                        target = NetPlayerToPlayer(netPlayer);

                    object[] createData = {
                        new[] { netId },
                        new[] { hash },
                        new[] { BitPackUtils.PackWorldPosForNetwork(position) },
                        new[] { BitPackUtils.PackQuaternionForNetwork(rotation) },
                        new[] { sendData },
                        new[] { gameEntityManager.GetInvalidNetId() }
                    };

                    switch (target)
                    {
                        case RpcTarget rpcTarget:
                            gameEntityManager.photonView.RPC("CreateItemRPC", rpcTarget, createData);
                            break;
                        case Player player:
                            gameEntityManager.photonView.RPC("CreateItemRPC", player, createData);
                            break;
                    }

                    if ((velocity != Vector3.zero || angVelocity != Vector3.zero || Buttons.GetIndex("Entity Gravity").enabled) && gameEntityManager.m_RpcSpamChecks.m_callLimiters[(int)GameEntityManager.RPC.ThrowEntity].CanCallNow())
                    {

                        velocity = velocity.ClampSqrMagnitude(1600f);

                        object[] dropData = {
                            netId,
                            true,
                            position,
                            rotation,
                            velocity,
                            angVelocity,
                            PhotonNetwork.LocalPlayer,
                            PhotonNetwork.Time
                        };

                        switch (target)
                        {
                            case RpcTarget rpcTarget:
                                gameEntityManager.photonView.RPC("ThrowEntityRPC", rpcTarget, dropData);
                                break;
                            case Player player:
                                gameEntityManager.photonView.RPC("ThrowEntityRPC", player, dropData);
                                break;
                        }
                    }
                }

                RPCProtection();
            }
            else
            {
                float maxDistance = 12f;
                if (Vector3.Distance(ServerLeftHandPos, position) > maxDistance)
                    position = ServerLeftHandPos + (position - ServerLeftHandPos).normalized * maxDistance;

                GamePlayer gamePlayer = GamePlayer.GetGamePlayer(PhotonNetwork.LocalPlayer);
                if (gamePlayer.IsHoldingEntity(gameEntityManager, true) && gameEntityManager.m_RpcSpamChecks.m_callLimiters[(int)GameEntityManager.RPC.CreateItem].CanCallNow())
                {
                    VRRig.LocalRig.enabled = true;
                    if (ServerLeftHandPos.Distance(position) < maxDistance)
                        gameEntityManager.GetGameEntity(gamePlayer.GetGrabbedGameEntityId(GamePlayer.GetHandIndex(true))).RequestThrow(true, position, rotation, velocity, angVelocity, gameEntityManager);
                    else
                        return;
                }

                List<GameEntity> entities = gameEntityManager.entities.Where(e =>
                    e != null &&
                    e.typeId == hash &&
                    Vector3.Distance(ServerLeftHandPos, e.transform.position) < maxDistance &&
                    Vector3.Distance(GorillaTagger.Instance.bodyCollider.transform.position, e.transform.position) > 3f &&
                    gameEntityManager.ValidateGrab(e, PhotonNetwork.LocalPlayer.actorNumber, true)).ToList();

                if (entities.Count <= 0)
                    entities = gameEntityManager.entities.Where(e =>
                    e != null &&
                    e.typeId == hash &&
                    Vector3.Distance(ServerLeftHandPos, e.transform.position) < maxDistance &&
                    gameEntityManager.ValidateGrab(e, PhotonNetwork.LocalPlayer.actorNumber, true)).ToList();

                if (entities.Count <= 0)
                    entities = gameEntityManager.entities.Where(e =>
                    e != null &&
                    e.typeId == hash &&
                    gameEntityManager.ValidateGrab(e, PhotonNetwork.LocalPlayer.actorNumber, true)).ToList();

                if (entities.Count <= 0) // Desperate measures
                    entities = gameEntityManager.entities.Where(e =>
                    e != null &&
                    e.typeId == hash).ToList();

                if (entities.Count <= 0)
                    return;

                GameEntity entity = entities.OrderByDescending(entity => entity.transform.position.Distance(GorillaTagger.Instance.bodyCollider.transform.position)).FirstOrDefault();

                if (Vector3.Distance(entity.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > maxDistance)
                {
                    VRRig.LocalRig.enabled = false;
                    VRRig.LocalRig.transform.position = entity.transform.position - Vector3.one * 5f;

                    if (CritterCoroutine != null)
                        CoroutineManager.instance.StopCoroutine(CritterCoroutine);

                    CritterCoroutine = CoroutineManager.instance.StartCoroutine(RopeEnableRig());
                }

                if (Vector3.Distance(entity.transform.position, ServerPos) < maxDistance && gameEntityManager.m_RpcSpamChecks.m_callLimiters[(int)GameEntityManager.RPC.CreateItem].CanCallNow())
                {
                    entity.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                    entity.transform.rotation = RandomQuaternion();

                    entity.RequestGrab(true, Vector3.zero, Quaternion.identity, gameEntityManager);
                    RPCProtection();
                }
            }
        }

        public static void CreateItems(object target, int[] hashes, Vector3[] positions, Quaternion[] rotations, long[] sendData = null, GameEntityManager manager = null)
        {
            GameEntityManager gameEntityManager = manager ?? ManagerRegistry.GhostReactor.GameEntityManager;
            if (NetworkSystem.Instance.IsMasterClient)
            {
                if (gameEntityManager.m_RpcSpamChecks.m_callLimiters[(int)GameEntityManager.RPC.CreateItems].CanCallNow())
                {
                    if (target is NetPlayer netPlayer)
                        target = NetPlayerToPlayer(netPlayer);

                    sendData ??= Enumerable.Repeat(0L, hashes.Length).ToArray();

                    byte[] data = new byte[15360];
                    MemoryStream memoryStream = new MemoryStream(data);
                    BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
                    binaryWriter.Write(hashes.Length);

                    for (int i = 0; i < hashes.Length; i++)
                    {
                        binaryWriter.Write(manager.CreateTypeNetId(hashes[i]));
                        binaryWriter.Write(hashes[i]);
                        binaryWriter.Write(BitPackUtils.PackWorldPosForNetwork(positions[i]));
                        binaryWriter.Write(BitPackUtils.PackQuaternionForNetwork(rotations[i]));
                        binaryWriter.Write(sendData[i]);
                        binaryWriter.Write(gameEntityManager.GetInvalidNetId());
                    }

                    byte[] array = GZipStream.CompressBuffer(data);

                    object[] createData = {
                        (int)manager.zone,
                        array
                    };

                    switch (target)
                    {
                        case RpcTarget rpcTarget:
                            gameEntityManager.photonView.RPC("CreateItemsRPC", rpcTarget, createData);
                            break;
                        case Player player:
                            gameEntityManager.photonView.RPC("CreateItemsRPC", player, createData);
                            break;
                    }

                    RPCProtection();
                }


            }
            else
                CreateItem(target, hashes[0], positions[0], rotations[0], Vector3.zero, Vector3.zero, sendData.Length > 0 ? sendData[1] : 0L, manager);
        }

        public static Dictionary<string, int> ObjectByName { get => ManagerRegistry.GhostReactor.GameEntityManager.itemPrefabFactory.ToDictionary(prefab => prefab.Value.name, prefab => prefab.Key); }

        public static void SpamObjectGrip(int objectId)
        {
            if (rightGrab)
                CreateItem(RpcTarget.All, objectId, GorillaTagger.Instance.rightHandTransform.position, RandomQuaternion(), GorillaTagger.Instance.rightHandTransform.forward * ShootStrength, Vector3.zero);
        }

        public static void SpamEntityGrip()
        {
            int[] objectIds = ObjectByName.Select(x => x.Value).ToArray();
            SpamObjectGrip(objectIds[Random.Range(0, objectIds.Length)]);
        }

        public static void ToolSpamGrip()
        {
            int[] objectIds = ObjectByName.Where(x => x.Key.Contains("Tool")).Select(x => x.Value).ToArray();
            SpamObjectGrip(objectIds[Random.Range(0, objectIds.Length)]);
        }

        public static void ToolSpamGun()
        {
            int[] objectIds = ObjectByName.Where(x => x.Key.Contains("Tool")).Select(x => x.Value).ToArray();
            SpamObjectGun(objectIds[Random.Range(0, objectIds.Length)]);
        }

        public static void SpamObjectGun(int objectId)
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                    CreateItem(RpcTarget.All, objectId, NewPointer.transform.position, RandomQuaternion(), Vector3.zero, Vector3.zero);
            }
        }

        public static void SpamEntityGun()
        {
            int[] objectIds = ObjectByName.Select(x => x.Value).ToArray();
            SpamObjectGun(objectIds[Random.Range(0, objectIds.Length)]);
        }

        public static void RainEntities()
        {
            int[] objectIds = ObjectByName.Select(x => x.Value).ToArray();
            CreateItem(RpcTarget.All, objectIds[Random.Range(0, objectIds.Length)], VRRig.LocalRig.transform.position + new Vector3(Random.Range(-3f, 3f), 4f, Random.Range(-3f, 3f)), Quaternion.identity, Vector3.down, Vector3.zero);
        }

        public static void EntityAura()
        {
            int[] objectIds = ObjectByName.Select(x => x.Value).ToArray();
            CreateItem(RpcTarget.All, objectIds[Random.Range(0, objectIds.Length)], VRRig.LocalRig.transform.position + RandomVector3().normalized * 2f, Quaternion.identity, Vector3.down, Vector3.zero);
        }

        public static void EntityFountain()
        {
            int[] objectIds = ObjectByName.Select(x => x.Value).ToArray();
            CreateItem(RpcTarget.All, objectIds[Random.Range(0, objectIds.Length)], VRRig.LocalRig.transform.position + Vector3.up * 3f, Quaternion.identity, RandomVector3(15f), Vector3.zero);
        }

        public static Dictionary<string, bool[][]> Letters = new Dictionary<string, bool[][]> {
            { "A", new[]
            {
                new[] { false, true, true, true, false },
                new[] { true, false, false, false, true },
                new[] { true, true, true, true, true },
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
            } },
            { "B", new[]
            {
                new[] { true, true, true, true, false },
                new[] { true, false, false, false, true },
                new[] { true, true, true, true, false },
                new[] { true, false, false, false, true },
                new[] { true, true, true, true, true },
            } },
            { "C", new[]
            {
                new[] { false, true, true, true, true },
                new[] { true, false, false, false, false },
                new[] { true, false, false, false, false },
                new[] { true, false, false, false, false },
                new[] { false, true, true, true, true },
            } },
            { "D", new[]
            {
                new[] { true, true, true, true, false },
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
                new[] { true, true, true, true, false },
            } },
            { "E", new[]
            {
                new[] { true, true, true, true, true },
                new[] { true, false, false, false, false },
                new[] { true, true, true, false, false },
                new[] { true, false, false, false, false },
                new[] { true, true, true, true, true },
            } },
            { "F", new[]
            {
                new[] { true, true, true, true, true },
                new[] { true, false, false, false, false },
                new[] { true, true, true, false, false },
                new[] { true, false, false, false, false },
                new[] { true, false, false, false, false },
            } },
            { "G", new[]
            {
                new[] { true, true, true, true, true },
                new[] { true, false, false, false, false },
                new[] { true, false, false, true, true },
                new[] { true, false, false, false, true },
                new[] { true, true, true, true, true },
            } },
            { "H", new[]
            {
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
                new[] { true, true, true, true, true },
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
            } },
            { "I", new[]
            {
                new[] { true, true, true, true, true },
                new[] { false, false, true, false, false },
                new[] { false, false, true, false, false },
                new[] { false, false, true, false, false },
                new[] { true, true, true, true, true },
            } },
            { "J", new[]
            {
                new[] { false, false, false, false, true },
                new[] { false, false, false, false, true },
                new[] { false, false, false, false, true },
                new[] { true, false, false, false, true },
                new[] { false, true, true, true, false },
            } },
            { "K", new[]
            {
                new[] { true, false, false, false, true },
                new[] { true, false, false, true, false },
                new[] { true, true, true, false, false },
                new[] { true, false, false, true, false },
                new[] { true, false, false, false, true },
            } },
            { "L", new[]
            {
                new[] { true, false, false, false, false },
                new[] { true, false, false, false, false },
                new[] { true, false, false, false, false },
                new[] { true, false, false, false, false },
                new[] { true, true, true, true, true },
            } },
            { "M", new[]
            {
                new[] { true, true, false, true, true },
                new[] { true, false, true, false, true },
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
            } },
            { "N", new[]
            {
                new[] { true, false, false, false, true },
                new[] { true, true, false, false, true },
                new[] { true, false, true, false, true },
                new[] { true, false, false, true, true },
                new[] { true, false, false, false, true },
            } },
            { "O", new[]
            {
                new[] { false, true, true, true, false },
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
                new[] { false, true, true, true, false },
            } },
            { "P", new[]
            {
                new[] { true, true, true, true, false },
                new[] { true, false, false, false, true },
                new[] { true, true, true, true, false },
                new[] { true, false, false, false, false },
                new[] { true, false, false, false, false },
            } },
            { "Q", new[]
            {
                new[] { false, true, true, true, false },
                new[] { true, false, false, false, true },
                new[] { true, false, true, false, true },
                new[] { true, false, false, true, false },
                new[] { false, true, true, false, true },
            } },
            { "R", new[]
            {
                new[] { true, true, true, true, true },
                new[] { true, false, false, false, true },
                new[] { true, true, true, true, true },
                new[] { true, false, false, true, false },
                new[] { true, false, false, false, true },
            } },
            { "S", new[]
            {
                new[] { true, true, true, true, true },
                new[] { true, false, false, false, false },
                new[] { true, true, true, true, true },
                new[] { false, false, false, false, true },
                new[] { true, true, true, true, true },
            } },
            { "T", new[]
            {
                new[] { true, true, true, true, true },
                new[] { false, false, true, false, false },
                new[] { false, false, true, false, false },
                new[] { false, false, true, false, false },
                new[] { false, false, true, false, false },
            } },
            { "U", new[]
            {
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
                new[] { false, true, true, true, false },
            } },
            { "V", new[]
            {
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
                new[] { false, true, false, true, false },
                new[] { false, true, false, true, false },
                new[] { false, false, true, false, false },
            } },
            { "W", new[]
            {
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
                new[] { true, false, false, false, true },
                new[] { true, false, true, false, true },
                new[] { false, true, false, true, false },
            } },
            { "X", new[]
            {
                new[] { true, false, false, false, true },
                new[] { false, true, false, true, false },
                new[] { false, false, true, false, false },
                new[] { false, true, false, true, false },
                new[] { true, false, false, false, true },
            } },
            { "Y", new[]
            {
                new[] { true, false, false, false, true },
                new[] { false, true, false, true, false },
                new[] { false, false, true, false, false },
                new[] { false, false, true, false, false },
                new[] { false, false, true, false, false },
            } },
            { "Z", new[]
            {
                new[] { true, true, true, true, true },
                new[] { false, false, false, true, false },
                new[] { false, false, true, false, false },
                new[] { false, true, false, false, false },
                new[] { true, true, true, true, true },
            } },
            { ".", new[]
            {
                new[] { false, false, false, false, false },
                new[] { false, false, false, false, false },
                new[] { false, false, false, false, false },
                new[] { false, false, false, false, false },
                new[] { false, false, true, false, false },
            } },
            { "/", new[]
            {
                new[] { false, false, false, false, true },
                new[] { false, false, false, true, false },
                new[] { false, false, true, false, false },
                new[] { false, true, false, false, false },
                new[] { true, false, false, false, false },
            } },
            { " ", new[]
            {
                new[] { false, false, false, false, false },
                new[] { false, false, false, false, false },
                new[] { false, false, false, false, false },
                new[] { false, false, false, false, false },
                new[] { false, false, false, false, false },
            } }
        };

        public static string textToRender;
        public static float textDelay;
        public static int characterIndex;
        public static Vector3? basePosition;

        public static void GhostReactorTextGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    if (basePosition == null)
                        basePosition = NewPointer.transform.position + Vector3.up;

                    if (Time.time > textDelay)
                    {
                        textDelay = Time.time + 0.1f;
                        bool[][] characterData = Letters[textToRender[characterIndex].ToString()];

                        List<Vector3> position = new List<Vector3>();
                        for (int i = 0; i < characterData.Length; i++)
                        {
                            bool[] column = characterData[i];

                            for (int j = 0; j < column.Length; j++)
                            {
                                bool currentIndex = column[j];
                                Vector3 offset = new Vector3((j * 0.2f) + (characterIndex * 1.2f), i * -0.2f, 0f);

                                if (currentIndex)
                                    position.Add(basePosition.Value + offset);
                            }
                        }

                        CreateItems(RpcTarget.All, Enumerable.Repeat(ObjectByName["GhostReactorCollectibleFlower"], position.Count).ToArray(), position.ToArray(), Enumerable.Repeat(Quaternion.identity, position.Count).ToArray());
                        characterIndex++;
                    }
                }
                else
                {
                    characterIndex = 0;
                    basePosition = null;
                }
            }
        }

        public static void SuperInfectionTextGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    if (basePosition == null)
                        basePosition = NewPointer.transform.position + Vector3.up;

                    if (Time.time > textDelay)
                    {
                        textDelay = Time.time + 0.1f;
                        bool[][] characterData = Letters[textToRender[characterIndex].ToString()];

                        List<Vector3> position = new List<Vector3>();
                        for (int i = 0; i < characterData.Length; i++)
                        {
                            bool[] column = characterData[i];

                            for (int j = 0; j < column.Length; j++)
                            {
                                bool currentIndex = column[j];
                                Vector3 offset = new Vector3((j * 0.2f) + (characterIndex * 1.2f), i * -0.2f, 0f);

                                if (currentIndex)
                                    position.Add(basePosition.Value + offset);
                            }
                        }

                        CreateItems(RpcTarget.All, Enumerable.Repeat(GadgetByName["SIGadgetDashYoyo"], position.Count).ToArray(), position.ToArray(), Enumerable.Repeat(Quaternion.identity, position.Count).ToArray(), null, ManagerRegistry.SuperInfection.GameEntityManager);
                        characterIndex++;
                    }
                }
                else
                {
                    characterIndex = 0;
                    basePosition = null;
                }
            }
        }

        public static IEnumerator DrawSmallDelay(Vector3 position, int id, GameEntityManager manager)
        {
            GameObject Temporary = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Temporary.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            Temporary.transform.position = position;
            Object.Destroy(Temporary.GetComponent<Collider>());
            yield return new WaitForSeconds(0.5f);
            CreateItem(RpcTarget.All, id, Temporary.transform.position + new Vector3(0f, 0.1f, 0f), RandomQuaternion(), Vector3.zero, Vector3.zero, manager: manager);
            Object.Destroy(Temporary);
            RPCProtection();
        }

        public static void GhostReactorDrawGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                    CoroutineManager.instance.StartCoroutine(DrawSmallDelay(NewPointer.transform.position, ObjectByName["GhostReactorCollectibleCore"], ManagerRegistry.GhostReactor.GameEntityManager));
            }
        }

        public static void SuperInfectionDrawGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    int[] objectIds = GadgetByName.Select(element => element.Value).ToArray();
                    CoroutineManager.instance.StartCoroutine(DrawSmallDelay(NewPointer.transform.position, objectIds[Random.Range(0, objectIds.Length)], ManagerRegistry.SuperInfection.GameEntityManager));
                }
            }
        }

        private static float destroyDelay;
        public static void DestroyEntityGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    if (Time.time > destroyDelay)
                    {
                        GameEntity gameEntity = null;
                        float closestDist = float.MaxValue;

                        foreach (GameEntity entity in ManagerRegistry.GhostReactor.GameEntityManager.entities)
                        {
                            if (entity != null)
                            {
                                float distance = Vector3.Distance(NewPointer.transform.position, entity.transform.position);
                                if (distance < 0.75f && distance < closestDist)
                                {
                                    gameEntity = entity;
                                    closestDist = distance;
                                }
                            }
                        }

                        if (gameEntity != null)
                        {
                            destroyDelay = Time.time + 0.02f;
                            if (NetworkSystem.Instance.IsMasterClient)
                            {
                                ManagerRegistry.GhostReactor.GameEntityManager.photonView.RPC("DestroyItemRPC", RpcTarget.All, new[] { gameEntity.GetNetId() });
                                RPCProtection();
                            }
                            else
                            {
                                gameEntity.RequestGrab(true, Vector3.zero, Quaternion.identity);
                                gameEntity.RequestThrow(true, GorillaTagger.Instance.bodyCollider.transform.position - (Vector3.up * 14f), Quaternion.identity, Vector3.zero, Vector3.zero);
                            }
                        }
                    }
                }
            }
        }

        private static float resourceIncrementDelay;
        public static void InfiniteResources()
        {
            var player = SIPlayer.Get(NetworkSystem.Instance.LocalPlayer.ActorNumber);
            for (int i = 0; i < player.CurrentProgression.resourceArray.Length; i++)
                player.CurrentProgression.resourceArray[i] = int.MaxValue;

            if (Time.time > resourceIncrementDelay)
            {
                resourceIncrementDelay = Time.time + 1f;
                for (int i = 0; i < (int)SIResource.ResourceType.Count; i++)
                {
                    var resourceType = (SIResource.ResourceType)i;
                    ProgressionManager.Instance.IncrementSIResource(resourceType.ToString(), OnFailure: (error) => resourceIncrementDelay = Time.time + 10f);
                }
            }
        }

        public static void CompleteAllQuests()
        {
            for (int i = 0; i < SIProgression.Instance.activeQuestIds.Length; i++)
            {
                RotatingQuest quest = SIProgression.Instance.questSourceList.GetQuestById(SIProgression.Instance.activeQuestIds[i]);
                quest.SetProgress(quest.requiredOccurenceCount);
            }
        }

        public static void ClaimAllTerminals()
        {
            foreach (var terminal in ManagerRegistry.SuperInfection.ZoneSuperInfection.siTerminals)
                terminal?.PlayerHandScanned(NetworkSystem.Instance.LocalPlayer.ActorNumber);
        }

        public static void UnlockAllGadgets()
        {
            foreach (var page in SIProgression.Instance.unlockedTechTreeData)
            {
                for (int i = 0; i < page.Length; i++)
                    page[i] = true;
            }
        }

        public static void DebugBlasterAimbot()
        {
            List<NetPlayer> infected = InfectedList();
            List<VRRig> rigs = VRRigExtensions.ActiveRigs
                .Where(rig => !rig.isLocal)
                .Where(rig => !infected.Contains(rig.GetPlayer()))
                .ToList();

            Transform head = GorillaTagger.Instance.headCollider.transform;
            VRRig targetRig = rigs
                .Where(rig => rig != null)
                .Select(rig => new
                {
                    Rig = rig,
                    ToRig = (rig.transform.position - head.position).normalized,
                    Distance = Vector3.Distance(head.position, rig.transform.position)
                })
                .OrderBy(x => Vector3.Angle(head.forward, x.ToRig)) // only angle matters
                .ThenBy(x => x.Distance) // tiebreaker if multiple at same angle
                .Select(x => x.Rig)
                .FirstOrDefault();

            if (targetRig == null)
                return;

            Visuals.Visualize(PrimitiveType.Sphere, targetRig.headMesh.transform.position, Quaternion.identity, new Vector3(0.1f, 0.1f, 0.1f), Color.green, -91752, 0.1f);
        }

        public static int? spawnedNetId;
        public static int spawnedFrame;

        public static SIGadgetChargeBlaster GetBlaster()
        {
            ControllerInputPoller.instance.leftGrab = true;
            ControllerInputPoller.instance.leftControllerGripFloat = 1f;

            int hash = GadgetByName["MegaChargeBlasterGadget"];

            GameEntityManager gameEntityManager = ManagerRegistry.SuperInfection.GameEntityManager;
            GamePlayer gamePlayer = GamePlayer.GetGamePlayer(PhotonNetwork.LocalPlayer);
            if (gamePlayer.IsHoldingEntity(gameEntityManager, true))
            {
                GameEntity entity = gameEntityManager.GetGameEntity(gamePlayer.GetGrabbedGameEntityId(GamePlayer.GetHandIndex(true)));
                if (entity.gameObject.TryGetComponent<SIGadgetChargeBlaster>(out var blaster))
                    return blaster;
                else
                    entity.RequestThrow(true, entity.transform.position, entity.transform.rotation, Vector3.zero, Vector3.zero, gameEntityManager);
            }
            else
            {
                if (NetworkSystem.Instance.IsMasterClient)
                {
                    if (spawnedNetId != null && spawnedFrame > Time.frameCount - 10)
                    {
                        GameEntity entity = gameEntityManager.GetGameEntity(spawnedNetId.Value);
                        entity.RequestGrab(true, Vector3.zero, Quaternion.identity, gameEntityManager);
                        if (entity.gameObject.TryGetComponent<SIGadgetChargeBlaster>(out var blaster))
                            return blaster;
                    }

                    if (!gameEntityManager.m_RpcSpamChecks.m_callLimiters[(int)GameEntityManager.RPC.CreateItem].CanCallNow())
                        return null;


                    int netId = gameEntityManager.CreateTypeNetId(hash);

                    object[] createData = {
                        new[] { netId },
                        new[] { hash },
                        new[] { BitPackUtils.PackWorldPosForNetwork(GorillaTagger.Instance.leftHandTransform.position) },
                        new[] { BitPackUtils.PackQuaternionForNetwork(GorillaTagger.Instance.leftHandTransform.rotation) },
                        new[] { 0L },
                        new[] { gameEntityManager.GetInvalidNetId() }
                    };

                    gameEntityManager.photonView.RPC("CreateItemRPC", RpcTarget.All, createData);
                    spawnedNetId = netId;
                    spawnedFrame = Time.frameCount;

                    RPCProtection();
                }
                else
                {
                    Vector3 position = GorillaTagger.Instance.leftHandTransform.position;

                    float maxDistance = 12f;
                    if (Vector3.Distance(ServerLeftHandPos, position) > maxDistance)
                        position = ServerLeftHandPos + (position - ServerLeftHandPos).normalized * maxDistance;

                    List<GameEntity> entities = gameEntityManager.entities.Where(e =>
                        e != null &&
                        e.typeId == hash &&
                        Vector3.Distance(ServerLeftHandPos, e.transform.position) < maxDistance &&
                        Vector3.Distance(GorillaTagger.Instance.bodyCollider.transform.position, e.transform.position) > 3f &&
                        gameEntityManager.ValidateGrab(e, PhotonNetwork.LocalPlayer.actorNumber, true)).ToList();

                    if (entities.Count <= 0)
                        entities = gameEntityManager.entities.Where(e =>
                        e != null &&
                        e.typeId == hash &&
                        Vector3.Distance(ServerLeftHandPos, e.transform.position) < maxDistance &&
                        gameEntityManager.ValidateGrab(e, PhotonNetwork.LocalPlayer.actorNumber, true)).ToList();

                    if (entities.Count <= 0)
                        entities = gameEntityManager.entities.Where(e =>
                        e != null &&
                        e.typeId == hash &&
                        gameEntityManager.ValidateGrab(e, PhotonNetwork.LocalPlayer.actorNumber, true)).ToList();

                    if (entities.Count <= 0) // Desperate measures
                        entities = gameEntityManager.entities.Where(e =>
                        e != null &&
                        e.typeId == hash).ToList();

                    if (entities.Count <= 0)
                        return null;

                    GameEntity entity = entities.OrderByDescending(entity => entity.transform.position.Distance(GorillaTagger.Instance.bodyCollider.transform.position)).FirstOrDefault();

                    if (Vector3.Distance(entity.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > maxDistance)
                    {
                        VRRig.LocalRig.enabled = false;
                        VRRig.LocalRig.transform.position = entity.transform.position - Vector3.one * 5f;

                        if (CritterCoroutine != null)
                            CoroutineManager.instance.StopCoroutine(CritterCoroutine);

                        CritterCoroutine = CoroutineManager.instance.StartCoroutine(RopeEnableRig());
                    }

                    if (Vector3.Distance(entity.transform.position, ServerPos) < maxDistance && gameEntityManager.m_RpcSpamChecks.m_callLimiters[(int)GameEntityManager.RPC.CreateItem].CanCallNow())
                    {
                        entity.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                        entity.transform.rotation = RandomQuaternion();

                        entity.RequestGrab(true, Vector3.zero, Quaternion.identity, gameEntityManager);
                        RPCProtection();
                    }
                }
            }

            return null;
        }

        public static float blasterDelay;
        public static Coroutine BlasterCoroutine;

        public static void BetaFireBlaster(Vector3 position, Vector3 direction)
        {
            SIGadgetChargeBlaster blaster = GetBlaster();
            if (blaster == null)
                return;

            Quaternion rotation = Quaternion.LookRotation(direction);
            if (Time.time < blasterDelay)
                return;

            blasterDelay = Time.time + SIPlayer.LocalPlayer.clientToClientRPCLimiter.GetDelay();

            if ((position - GorillaTagger.Instance.leftHandTransform.position).magnitude > blaster.blaster.maxLagDistance)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = position - Vector3.one;

                if (BlasterCoroutine != null)
                    CoroutineManager.instance.StopCoroutine(BlasterCoroutine);

                BlasterCoroutine = CoroutineManager.instance.StartCoroutine(RopeEnableRig());
            }

            blaster.blaster.lastFired = 0f;
            blaster.FireProjectile(blaster.maxChargeDiff, blaster.blaster.NextFireId(), position, rotation);

            RPCProtection();
        }

        public static readonly Dictionary<NetPlayer, float> perPlayerDictionary = new Dictionary<NetPlayer, float>();
        public static void BetaFireBlaster(Vector3 position, Vector3 direction, NetPlayer target, bool ignoreDistnace = false)
        {
            perPlayerDictionary.TryGetValue(target, out float perPlayerDelay);

            Quaternion rotation = Quaternion.LookRotation(direction);
            if (Time.time < perPlayerDelay)
                return;

            SIGadgetChargeBlaster blaster = GetBlaster();
            if (blaster == null)
                return;

            perPlayerDictionary[target] = Time.time + SIPlayer.LocalPlayer.clientToClientRPCLimiter.GetDelay();

            if (!ignoreDistnace && (position - GorillaTagger.Instance.leftHandTransform.position).magnitude > blaster.blaster.maxLagDistance)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = position - Vector3.one;

                if (BlasterCoroutine != null)
                    CoroutineManager.instance.StopCoroutine(BlasterCoroutine);

                BlasterCoroutine = CoroutineManager.instance.StartCoroutine(RopeEnableRig());
            }

            blaster.blaster.lastFired = 0f;
            SuperInfectionManager.GetSIManagerForZone(blaster.blaster.gameEntity.manager.zone)?.photonView.RPC("SIClientToClientRPC", target.GetPlayer(), new object[]
            {
                (int)SuperInfectionManager.ClientToClientRPC.CallEntityRPCData,
                new object[]
                {
                    blaster.blaster.gameEntity.GetNetId(),
                    0,
                    new object[]
                    {
                        blaster.maxChargeDiff,
                        blaster.blaster.NextFireId(),
                        position,
                        rotation
                    }
                }
            });

            RPCProtection();
        }

        public static void BlasterLaserSpam()
        {
            if (rightGrab)
                BetaFireBlaster(GorillaTagger.Instance.rightHandTransform.position, GorillaTagger.Instance.rightHandTransform.forward);
        }

        public static void BlasterFlingGun(Vector3 direction)
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    BetaFireBlaster(lockTarget.transform.position, direction.normalized);

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void BlasterFlingAll(Vector3 direction)
        {
            if (GetBlaster() == null)
                SerializePatch.OverrideSerialization = null;
            else SerializePatch.OverrideSerialization ??= () =>
            {
                MassSerialize(true, new[] { VRRig.LocalRig.GetPhotonView() });

                Vector3 archivePos = VRRig.LocalRig.transform.position;

                foreach (NetPlayer Player in NetworkSystem.Instance.PlayerListOthers)
                {
                    VRRig targetRig = GetVRRigFromPlayer(Player);

                    VRRig.LocalRig.transform.position = targetRig.transform.position - Vector3.up;
                    SendSerialize(VRRig.LocalRig.GetPhotonView(), new RaiseEventOptions { TargetActors = new[] { Player.ActorNumber } });
                }

                RPCProtection();

                VRRig.LocalRig.transform.position = archivePos;

                return false;
            };

            foreach (NetPlayer Player in NetworkSystem.Instance.PlayerListOthers)
            {
                VRRig targetRig = GetVRRigFromPlayer(Player);
                BetaFireBlaster(targetRig.transform.position, direction, Player, true);
            }
        }

        public static void BlasterFlingTowardsGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    BetaFireBlaster(lockTarget.transform.position, GorillaTagger.Instance.bodyCollider.transform.position - lockTarget.transform.position);

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void BlasterFlingTowardsAll()
        {
            if (GetBlaster() == null)
                SerializePatch.OverrideSerialization = null;
            else SerializePatch.OverrideSerialization ??= () =>
            {
                MassSerialize(true, new[] { VRRig.LocalRig.GetPhotonView() });

                Vector3 archivePos = VRRig.LocalRig.transform.position;

                foreach (NetPlayer Player in NetworkSystem.Instance.PlayerListOthers)
                {
                    VRRig targetRig = GetVRRigFromPlayer(Player);

                    VRRig.LocalRig.transform.position = targetRig.transform.position - Vector3.up;
                    SendSerialize(VRRig.LocalRig.GetPhotonView(), new RaiseEventOptions { TargetActors = new[] { Player.ActorNumber } });
                }

                RPCProtection();

                VRRig.LocalRig.transform.position = archivePos;

                return false;
            };

            foreach (NetPlayer Player in NetworkSystem.Instance.PlayerListOthers)
            {
                VRRig targetRig = GetVRRigFromPlayer(Player);
                BetaFireBlaster(targetRig.transform.position, GorillaTagger.Instance.bodyCollider.transform.position - targetRig.transform.position, Player, true);
            }
        }

        public static void BlasterFlingAwayGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    BetaFireBlaster(lockTarget.transform.position, lockTarget.transform.position - GorillaTagger.Instance.bodyCollider.transform.position);

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void BlasterFlingAwayAll()
        {
            if (GetBlaster() == null)
                SerializePatch.OverrideSerialization = null;
            else SerializePatch.OverrideSerialization ??= () =>
            {
                MassSerialize(true, new[] { VRRig.LocalRig.GetPhotonView() });

                Vector3 archivePos = VRRig.LocalRig.transform.position;

                foreach (NetPlayer Player in NetworkSystem.Instance.PlayerListOthers)
                {
                    VRRig targetRig = GetVRRigFromPlayer(Player);

                    VRRig.LocalRig.transform.position = targetRig.transform.position - Vector3.up;
                    SendSerialize(VRRig.LocalRig.GetPhotonView(), new RaiseEventOptions { TargetActors = new[] { Player.ActorNumber } });
                }

                RPCProtection();

                VRRig.LocalRig.transform.position = archivePos;

                return false;
            };

            foreach (NetPlayer Player in NetworkSystem.Instance.PlayerListOthers)
            {
                VRRig targetRig = GetVRRigFromPlayer(Player);
                BetaFireBlaster(targetRig.transform.position, targetRig.transform.position - GorillaTagger.Instance.bodyCollider.transform.position, Player, true);
            }
        }

        public static void BlasterKickGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    BetaFireBlaster(lockTarget.transform.position, lockTarget.transform.position.z < -28.5f ? (new Vector3(-47.82025f, 6.460508f, -29.04836f) - lockTarget.transform.position).normalized : lockTarget.transform.position.z < -23f ? new Vector3(-50f, 0f, 50f) : Vector3.left);

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void BlasterKickAll()
        {
            if (GetBlaster() == null)
                SerializePatch.OverrideSerialization = null;
            else SerializePatch.OverrideSerialization ??= () =>
            {
                MassSerialize(true, new[] { VRRig.LocalRig.GetPhotonView() });

                Vector3 archivePos = VRRig.LocalRig.transform.position;

                foreach (NetPlayer Player in NetworkSystem.Instance.PlayerListOthers)
                {
                    VRRig targetRig = GetVRRigFromPlayer(Player);

                    VRRig.LocalRig.transform.position = targetRig.transform.position - Vector3.up;
                    SendSerialize(VRRig.LocalRig.GetPhotonView(), new RaiseEventOptions { TargetActors = new[] { Player.ActorNumber } });
                }

                RPCProtection();

                VRRig.LocalRig.transform.position = archivePos;

                return false;
            };

            foreach (NetPlayer Player in NetworkSystem.Instance.PlayerListOthers)
            {
                VRRig targetRig = GetVRRigFromPlayer(Player);
                BetaFireBlaster(targetRig.transform.position, targetRig.transform.position.z < -28.5f ? (new Vector3(-47.82025f, 6.460508f, -29.04836f) - targetRig.transform.position).normalized : targetRig.transform.position.z < -23f ? new Vector3(-50f, 0f, 50f) : Vector3.left, Player, true);
            }
        }

        public static void BlasterCrashGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    BetaFireBlaster(lockTarget.transform.position, lockTarget.transform.position.y > 55f ? Vector3.right : Vector3.up);

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void BlasterCrashAll()
        {
            if (GetBlaster() == null)
                SerializePatch.OverrideSerialization = null;
            else SerializePatch.OverrideSerialization ??= () =>
            {
                MassSerialize(true, new[] { VRRig.LocalRig.GetPhotonView() });

                Vector3 archivePos = VRRig.LocalRig.transform.position;

                foreach (NetPlayer Player in NetworkSystem.Instance.PlayerListOthers)
                {
                    VRRig targetRig = GetVRRigFromPlayer(Player);

                    VRRig.LocalRig.transform.position = targetRig.transform.position - Vector3.up;
                    SendSerialize(VRRig.LocalRig.GetPhotonView(), new RaiseEventOptions { TargetActors = new[] { Player.ActorNumber } });
                }

                RPCProtection();

                VRRig.LocalRig.transform.position = archivePos;

                return false;
            };

            foreach (NetPlayer Player in NetworkSystem.Instance.PlayerListOthers)
            {
                VRRig targetRig = GetVRRigFromPlayer(Player);
                BetaFireBlaster(targetRig.transform.position, targetRig.transform.position.y > 55f ? Vector3.right : Vector3.up, Player, true);
            }
        }

        public static void BlasterControlGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null && lockTarget.Distance(GorillaTagger.Instance.bodyCollider.transform.position) > 0.5f)
                    BetaFireBlaster(lockTarget.transform.position, GorillaTagger.Instance.bodyCollider.transform.position - lockTarget.transform.position);

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static Dictionary<string, int> GadgetByName
        {
            get =>
                ManagerRegistry.SuperInfection.GameEntityManager.itemPrefabFactory
                .ToDictionary(prefab => prefab.Value.name, prefab => prefab.Key);
        }

        public static void SpamGadgetGrip(int objectId)
        {
            if (rightGrab)
                CreateItem(RpcTarget.All, objectId, GorillaTagger.Instance.rightHandTransform.position, RandomQuaternion(), GorillaTagger.Instance.rightHandTransform.forward * ShootStrength, Vector3.zero, 0L, ManagerRegistry.SuperInfection.GameEntityManager);
        }

        public static void SpamGadgetGun(int objectId)
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                    CreateItem(RpcTarget.All, objectId, NewPointer.transform.position, RandomQuaternion(), Vector3.zero, Vector3.zero, 0L, ManagerRegistry.SuperInfection.GameEntityManager);
            }
        }

        public static void GadgetSpamGrip()
        {
            int[] objectIds = GadgetByName.Select(element => element.Value).ToArray();
            SpamGadgetGrip(objectIds[Random.Range(0, objectIds.Length)]);
        }

        public static void GadgetSpamGun()
        {
            int[] objectIds = GadgetByName.Select(element => element.Value).ToArray();
            SpamGadgetGun(objectIds[Random.Range(0, objectIds.Length)]);
        }

        public static void RainGadgets()
        {
            int[] objectIds = GadgetByName.Select(element => element.Value).ToArray();
            CreateItem(RpcTarget.All, objectIds[Random.Range(0, objectIds.Length)], VRRig.LocalRig.transform.position + new Vector3(Random.Range(-3f, 3f), 4f, Random.Range(-3f, 3f)), Quaternion.identity, Vector3.down, Vector3.zero, 0L, ManagerRegistry.SuperInfection.GameEntityManager);
        }

        public static void GadgetAura()
        {
            int[] objectIds = GadgetByName.Select(element => element.Value).ToArray();
            CreateItem(RpcTarget.All, objectIds[Random.Range(0, objectIds.Length)], VRRig.LocalRig.transform.position + RandomVector3().normalized * 2f, Quaternion.identity, Vector3.down, Vector3.zero, 0L, ManagerRegistry.SuperInfection.GameEntityManager);
        }

        public static void GadgetFountain()
        {
            int[] objectIds = GadgetByName.Select(element => element.Value).ToArray();
            CreateItem(RpcTarget.All, objectIds[Random.Range(0, objectIds.Length)], VRRig.LocalRig.transform.position + Vector3.up * 3f, Quaternion.identity, RandomVector3(15f), Vector3.zero, 0L, ManagerRegistry.SuperInfection.GameEntityManager);
        }

        public static void ResourceSpamGrip()
        {
            int[] objectIds = GadgetByName.Where(x => x.Key.Contains("Resource")).Select(x => x.Value).ToArray();
            SpamGadgetGrip(objectIds[Random.Range(0, objectIds.Length)]);
        }

        public static void ResourceSpamGun()
        {
            int[] objectIds = GadgetByName.Where(x => x.Key.Contains("Resource")).Select(x => x.Value).ToArray();
            SpamGadgetGun(objectIds[Random.Range(0, objectIds.Length)]);
        }

        public static void DestroyGadgetGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    if (Time.time > destroyDelay)
                    {
                        GameEntity gameEntity = null;
                        float closestDist = float.MaxValue;

                        foreach (GameEntity entity in ManagerRegistry.SuperInfection.GameEntityManager.entities)
                        {
                            if (entity != null)
                            {
                                float distance = Vector3.Distance(NewPointer.transform.position, entity.transform.position);
                                if (distance < 0.75f && distance < closestDist)
                                {
                                    gameEntity = entity;
                                    closestDist = distance;
                                }
                            }
                        }

                        if (gameEntity != null)
                        {
                            destroyDelay = Time.time + 0.02f;
                            if (NetworkSystem.Instance.IsMasterClient)
                            {
                                ManagerRegistry.SuperInfection.GameEntityManager.photonView.RPC("DestroyItemRPC", RpcTarget.All, new[] { gameEntity.GetNetId() });
                                RPCProtection();
                            }
                            else
                            {
                                gameEntity.RequestGrab(true, Vector3.zero, Quaternion.identity, ManagerRegistry.SuperInfection.GameEntityManager);
                                gameEntity.RequestThrow(true, GorillaTagger.Instance.bodyCollider.transform.position - (Vector3.up * 14f), Quaternion.identity, Vector3.zero, Vector3.zero, ManagerRegistry.SuperInfection.GameEntityManager);
                            }
                        }
                    }
                }
            }
        }

        public static HalloweenGhostChaser _lucy;
        public static HalloweenGhostChaser Lucy
        {
            get
            {
                _lucy ??= GetObject("Environment Objects/05Maze_PersistentObjects/2025_Halloween1_PersistentObjects/Halloween Ghosts/Lucy/Halloween Ghost/FloatingChaseSkeleton").GetComponent<HalloweenGhostChaser>();
                return _lucy;
            }
            set => _lucy = value;
        }

        public static LurkerGhost _lurker;
        public static LurkerGhost Lurker
        {
            get
            {
                _lurker ??= GetObject("Environment Objects/05Maze_PersistentObjects/2025_Halloween1_PersistentObjects/Halloween Ghosts/Lurker Ghost/GhostLurker_Prefab").GetComponent<LurkerGhost>();
                return _lurker;
            }
            set => _lurker = value;
        }

        public static void SpawnBlueLucy()
        {
            HalloweenGhostChaser hgc = Lucy;
            if (hgc.IsMine)
            {
                hgc.timeGongStarted = Time.time;
                hgc.currentState = HalloweenGhostChaser.ChaseState.Gong;
                hgc.isSummoned = false;
            }
            else NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void SpawnRedLucy()
        {
            HalloweenGhostChaser hgc = Lucy;
            if (hgc.IsMine)
            {
                hgc.timeGongStarted = Time.time;
                hgc.currentState = HalloweenGhostChaser.ChaseState.Gong;
                hgc.isSummoned = true;
            }
            else NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void DespawnLucy()
        {
            HalloweenGhostChaser hgc = Lucy;
            if (hgc.IsMine)
            {
                hgc.currentState = HalloweenGhostChaser.ChaseState.Dormant;
                hgc.isSummoned = false;
            }
            else NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void LucyChase(NetPlayer player)
        {
            HalloweenGhostChaser hgc = Lucy;
            if (hgc.IsMine)
            {
                hgc.currentState = HalloweenGhostChaser.ChaseState.Chasing;
                hgc.targetPlayer = player;
                hgc.followTarget = GorillaTagger.Instance.offlineVRRig.transform;
            }
            else
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void LucyChaseGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                        LucyChase(gunTarget.GetPlayer());
                }
            }
        }

        public static void LucyAttack(NetPlayer player)
        {
            HalloweenGhostChaser hgc = Lucy;
            if (hgc.IsMine)
            {
                if (Time.time > hgc.grabTime + hgc.grabDuration + 0.1f)
                {
                    if (hgc.targetPlayer != player)
                    {
                        hgc.currentState = HalloweenGhostChaser.ChaseState.Dormant;
                        SendSerialize(hgc.GetView);
                    }
                    hgc.currentState = HalloweenGhostChaser.ChaseState.Grabbing;
                    hgc.grabTime = Time.time;
                    hgc.targetPlayer = player;
                }
            }
            else
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void LucyAttackGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    LucyAttack(lockTarget.GetPlayer());

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void LucyHarassGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    HalloweenGhostChaser hgc = Lucy;
                    if (hgc.IsMine)
                    {
                        if (Time.time > lucyDelay)
                        {
                            hgc.currentState = hgc.currentState == HalloweenGhostChaser.ChaseState.Grabbing ? HalloweenGhostChaser.ChaseState.Chasing : HalloweenGhostChaser.ChaseState.Grabbing;
                            hgc.transform.position = lockTarget.transform.position + Vector3.up;
                            hgc.currentSpeed = 0f;
                            hgc.targetPlayer = lockTarget.GetPlayer();
                            hgc.followTarget = lockTarget.transform;
                            lucyDelay = Time.time + 0.1f;
                        }
                    }
                    else
                        NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                }

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void LucyAttackAll()
        {
            HalloweenGhostChaser hgc = Lucy;
            if (SerializePatch.OverrideSerialization != null)
            {
                SerializePatch.OverrideSerialization = () =>
                {
                    MassSerialize(true, new[] { hgc.GetView });
                    return false;
                };
            }

            if (hgc.IsMine)
            {
                if (Time.time > hgc.grabTime + hgc.grabDuration + 0.1f)
                {
                    foreach (NetPlayer player in NetworkSystem.Instance.PlayerListOthers)
                    {
                        hgc.currentState = HalloweenGhostChaser.ChaseState.Grabbing;
                        hgc.grabTime = Time.time;
                        hgc.targetPlayer = player;
                        SendSerialize(Lucy.GetView, new RaiseEventOptions { TargetActors = new[] { player.ActorNumber } });
                    }
                }
            }
            else
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static float lucyDelay;
        public static void SpazLucy()
        {
            HalloweenGhostChaser hgc = Lucy;
            if (hgc.IsMine)
            {
                if (Time.time > lucyDelay)
                {
                    hgc.timeGongStarted = hgc.timeGongStarted == 0f ? Time.time : 0f;
                    hgc.currentState = HalloweenGhostChaser.ChaseState.Gong;
                    hgc.isSummoned = true;
                    lucyDelay = Time.time + 0.1f;
                }
            }
            else NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void AnnoyingLucy()
        {
            HalloweenGhostChaser hgc = Lucy;
            if (hgc.IsMine)
            {
                if (Time.time > lucyDelay)
                {
                    hgc.timeGongStarted = Time.time;
                    hgc.grabTime = Time.time;
                    hgc.currentState = hgc.currentState == HalloweenGhostChaser.ChaseState.Gong ? HalloweenGhostChaser.ChaseState.Grabbing : HalloweenGhostChaser.ChaseState.Gong;
                    hgc.targetPlayer = GetRandomPlayer(true);
                    lucyDelay = Time.time + 0.1f;
                }
            }
            else NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void BecomeLucy()
        {
            if (!NetworkSystem.Instance.IsMasterClient)
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                return;
            }

            if (Lucy != null)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = GorillaTagger.Instance.bodyCollider.transform.position - Vector3.up * 99999f;

                Lucy.transform.position = GorillaTagger.Instance.bodyCollider.transform.position;
                Lucy.transform.rotation = GorillaTagger.Instance.headCollider.transform.rotation;

                Lucy.currentState = HalloweenGhostChaser.ChaseState.Chasing;
                Lucy.targetPlayer = null;
            }
        }

        public static void MoveLucyGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    if (Lucy.IsMine)
                        Lucy.transform.position = NewPointer.transform.position + Vector3.up;
                    else NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                }
            }
        }

        public static void FastLucy()
        {
            HalloweenGhostChaser hgc = Lucy;
            if (hgc.IsMine)
                hgc.currentSpeed = 10f;
            else NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void SlowLucy()
        {
            HalloweenGhostChaser hgc = Lucy;
            if (hgc.IsMine)
                hgc.currentSpeed = 1f;
            else NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void SpawnLurker()
        {
            if (Lurker.IsMine)
                Lurker.currentState = LurkerGhost.ghostState.patrol;
            else NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void MoveLurkerGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    if (Lurker.IsMine)
                        Lurker.transform.position = NewPointer.transform.position + Vector3.up;
                    else NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                }
            }
        }

        public static void DespawnLurker()
        {
            if (Lurker.IsMine)
            {
                Lurker.currentState = LurkerGhost.ghostState.patrol;
            }
            else NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void LurkerAttack(NetPlayer player)
        {
            if (Lurker.IsMine)
            {
                if (Lurker.targetPlayer != player)
                {
                    Lurker.ChangeState(LurkerGhost.ghostState.patrol);
                    SendSerialize(Lurker.GetView);
                }

                Lurker.currentState = LurkerGhost.ghostState.possess;
                Lurker.targetPlayer = player;
            }
            else NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void LurkerAttackGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    LurkerAttack(lockTarget.GetPlayer());

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void LurkerAttackAll()
        {
            if (SerializePatch.OverrideSerialization != null)
            {
                SerializePatch.OverrideSerialization = () =>
                {
                    MassSerialize(true, new[] { Lurker.GetView });
                    return false;
                };
            }

            if (Lurker.IsMine)
            {
                if (Lurker.currentState != LurkerGhost.ghostState.possess)
                {
                    foreach (NetPlayer player in NetworkSystem.Instance.PlayerListOthers)
                    {
                        Lurker.currentState = LurkerGhost.ghostState.possess;
                        Lurker.targetPlayer = player;
                        SendSerialize(Lucy.GetView, new RaiseEventOptions { TargetActors = new[] { player.ActorNumber } });
                    }
                }
            }
            else
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static float lurkerDelay;
        public static void SpazLurker()
        {
            if (Lurker.IsMine)
            {
                if (Time.time > lurkerDelay)
                {
                    Lurker.currentState = Lurker.currentState == LurkerGhost.ghostState.charge ? LurkerGhost.ghostState.seek : LurkerGhost.ghostState.charge;
                    Lurker.targetPlayer = GetRandomPlayer(true);
                    lurkerDelay = Time.time + 0.1f;
                }
            }
            else NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void BreakLurker()
        {
            if (Lurker.IsMine)
            {
                Lurker.currentState = Lurker.currentState == LurkerGhost.ghostState.charge ? LurkerGhost.ghostState.possess : LurkerGhost.ghostState.charge;
                Lurker.targetPlayer = GetRandomPlayer(true);

                SendSerialize(Lurker.GetView);
            }
            else NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void AnnoyingLurker()
        {
            if (Lurker.IsMine)
            {
                if (Time.time > lurkerDelay)
                {
                    Lurker.currentState = Lurker.currentState == LurkerGhost.ghostState.possess ? LurkerGhost.ghostState.charge : LurkerGhost.ghostState.possess;
                    Lurker.targetPlayer = GetRandomPlayer(true);
                    lurkerDelay = Time.time + 0.1f;
                }
            }
            else NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void BecomeLurker()
        {
            if (!NetworkSystem.Instance.IsMasterClient)
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                return;
            }

            if (Lurker != null)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = GorillaTagger.Instance.bodyCollider.transform.position - Vector3.up * 99999f;

                Lurker.transform.position = GorillaTagger.Instance.bodyCollider.transform.position;
                Lurker.transform.rotation = GorillaTagger.Instance.headCollider.transform.rotation;

                Lurker.currentState = LurkerGhost.ghostState.seek;
                SerializePatch.OverrideSerialization = () =>
                {
                    MassSerialize(true, new[] { VRRig.LocalRig.GetPhotonView() });

                    foreach (NetPlayer Player in NetworkSystem.Instance.PlayerListOthers)
                    {
                        Lurker.targetPlayer = Player;
                        SendSerialize(Lurker.GetView, new RaiseEventOptions { TargetActors = new[] { Player.ActorNumber } });
                    }

                    RPCProtection();

                    return false;
                };
            }
        }

        public static void BetaSetVelocityPlayer(NetPlayer victim, Vector3 velocity)
        {
            if (velocity.sqrMagnitude > 20f)
                velocity = Vector3.Normalize(velocity) * 20f;

            GorillaGuardianManager gman = (GorillaGuardianManager)GorillaGameManager.instance;
            if (gman.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
            {
                GetNetworkViewFromVRRig(GetVRRigFromPlayer(victim)).SendRPC("GrabbedByPlayer", victim, true, false, false);
                GetNetworkViewFromVRRig(GetVRRigFromPlayer(victim)).SendRPC("DroppedByPlayer", victim, velocity);
            }
            else
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must be guardian.");
        }

        public static void BetaSetVelocityTargetGroup(RpcTarget victim, Vector3 velocity)
        {
            if (velocity.sqrMagnitude > 20f)
                velocity = Vector3.Normalize(velocity) * 20f;

            GorillaGuardianManager gman = (GorillaGuardianManager)GorillaGameManager.instance;
            if (gman.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
            {
                switch (victim)
                {
                    case RpcTarget.All:
                        {
                            foreach (VRRig rig in VRRigExtensions.ActiveRigs)
                            {
                                GetNetworkViewFromVRRig(rig).SendRPC("GrabbedByPlayer", rig.GetPlayer(), true, false, false);
                                GetNetworkViewFromVRRig(rig).SendRPC("DroppedByPlayer", rig.GetPlayer(), velocity);
                            }
                            break;
                        }
                    case RpcTarget.Others:
                        {
                            foreach (var rig in VRRigExtensions.ActiveRigs.Where(rig => !rig.isLocal))
                            {
                                GetNetworkViewFromVRRig(rig).SendRPC("GrabbedByPlayer", rig.GetPlayer(), true, false, false);
                                GetNetworkViewFromVRRig(rig).SendRPC("DroppedByPlayer", rig.GetPlayer(), velocity);
                            }

                            break;
                        }
                    case RpcTarget.MasterClient:
                        {
                            GetNetworkViewFromVRRig(GetVRRigFromPlayer(NetworkSystem.Instance.MasterClient)).SendRPC("GrabbedByPlayer", RpcTarget.Others, true, false, false);
                            GetNetworkViewFromVRRig(GetVRRigFromPlayer(NetworkSystem.Instance.MasterClient)).SendRPC("DroppedByPlayer", RpcTarget.Others, velocity);
                            break;
                        }
                }
            }
            else
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must be guardian.");
        }

        private static float grabDelay = 0;
        public static void GuardianGrabGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > grabDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        GorillaGuardianManager gman = (GorillaGuardianManager)GorillaGameManager.instance;
                        if (gman.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
                        {
                            GetNetworkViewFromVRRig(gunTarget).SendRPC("GrabbedByPlayer", RpcTarget.Others, true, false, false);
                            RPCProtection();
                        }
                        else
                            NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must be guardian.");
                        grabDelay = Time.time + 0.1f;
                    }
                }
            }
        }

        public static void GuardianGrabAll()
        {
            if (rightGrab && Time.time > grabDelay)
            {
                grabDelay = Time.time + 0.1f;
                GorillaGuardianManager guardianManager = (GorillaGuardianManager)GorillaGameManager.instance;
                if (guardianManager.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
                {
                    foreach (var plr in VRRigExtensions.ActiveRigs.Where(plr => !plr.isLocal))
                    {
                        GetNetworkViewFromVRRig(plr).SendRPC("GrabbedByPlayer", RpcTarget.Others, true, false, false);
                        RPCProtection();
                    }
                }
                else
                    NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must be guardian.");
            }
        }

        private static float releaseDelay;
        public static void GuardianReleaseGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > releaseDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        GorillaGuardianManager gman = (GorillaGuardianManager)GorillaGameManager.instance;
                        if (gman.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
                        {
                            GetNetworkViewFromVRRig(gunTarget).SendRPC("DroppedByPlayer", RpcTarget.Others, new Vector3(0f, 0f, 0f));
                            RPCProtection();
                        }
                        else
                            NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must be guardian.");

                        releaseDelay = Time.time + 0.1f;
                    }
                }
            }
        }

        public static void GuardianReleaseAll()
        {
            if (rightTrigger > 0.5f && Time.time > releaseDelay)
            {
                releaseDelay = Time.time + 0.1f;
                GorillaGuardianManager guardianManager = (GorillaGuardianManager)GorillaGameManager.instance;
                if (guardianManager.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
                {
                    foreach (var plr in VRRigExtensions.ActiveRigs.Where(plr => !plr.isLocal))
                    {
                        GetNetworkViewFromVRRig(plr).SendRPC("DroppedByPlayer", RpcTarget.Others, new Vector3(0f, 0f, 0f));
                        RPCProtection();
                    }
                }
                else
                    NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must be guardian.");
            }
        }

        private static float flingDelay;
        public static void GuardianFlingGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > flingDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        BetaSetVelocityPlayer(GetPlayerFromVRRig(gunTarget), new Vector3(0f, 19.9f, 0f));
                        RPCProtection();
                        flingDelay = Time.time + 0.1f;
                    }
                }
            }
        }

        public static void GuardianFlingAll()
        {
            if (rightTrigger > 0.5f && Time.time > flingDelay)
            {
                flingDelay = Time.time + 0.1f;

                BetaSetVelocityTargetGroup(RpcTarget.Others, new Vector3(0f, 19.9f, 0f));
                RPCProtection();
            }
        }

        public static void GuardianSpazPlayerGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > flingDelay)
                    {
                        BetaSetVelocityPlayer(lockTarget.GetPlayer(), RandomVector3(50f));
                        RPCProtection();
                        flingDelay = Time.time + 0.1f;
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void GuardianSpazAllPlayers()
        {
            if (rightTrigger > 0.5f && Time.time > flingDelay)
            {
                flingDelay = Time.time + 0.1f;
                BetaSetVelocityTargetGroup(RpcTarget.Others, RandomVector3(50f));
                RPCProtection();
            }
        }

        public static void BlockCrashGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (!NetworkSystem.Instance.IsMasterClient) { NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client."); return; }
                    Fun.RequestCreatePiece(1934114066, new Vector3(-127.6248f, 16.99441f, -217.2094f), Quaternion.identity, 0, NetPlayerToPlayer(lockTarget.GetPlayer()), false, true);
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void BlockCrashAll()
        {
            if (rightTrigger > 0.5f)
            {
                if (!NetworkSystem.Instance.IsMasterClient) { NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client."); return; }
                Fun.RequestCreatePiece(1934114066, new Vector3(-127.6248f, 16.99441f, -217.2094f), Quaternion.identity, 0, RpcTarget.Others, false, true);
            }
        }


        private static float antiReportFlingDelay;
        public static void AntiReportFling()
        {
            if (Time.time > antiReportFlingDelay)
            {
                Safety.AntiReport((vrrig, position) =>
                {
                    antiReportFlingDelay = Time.time + 0.1f;
                    BetaSetVelocityPlayer(GetPlayerFromVRRig(vrrig), (vrrig.transform.position - position) * 50f);
                    NotificationManager.SendNotification("<color=grey>[</color><color=purple>ANTI-REPORT</color><color=grey>]</color> " + GetPlayerFromVRRig(vrrig).NickName + " attempted to report you, they have been flung.");
                });
            }
        }

        public static bool SpecialTimeRPC(PhotonView photonView, int timeOffset, string method, RaiseEventOptions options, params object[] parameters)
        {
            if (photonView != null && parameters != null && !string.IsNullOrEmpty(method))
            {
                Hashtable rpcData = new Hashtable
                {
                    { 0, photonView.ViewID },
                    { 2, PhotonNetwork.ServerTimestamp + timeOffset },
                    { 3, method },
                    { 4, parameters }
                };

                if (photonView.Prefix > 0)
                    rpcData[1] = (short)photonView.Prefix;

                if (PhotonNetwork.PhotonServerSettings.RpcList.Contains(method))
                    rpcData[5] = (byte)PhotonNetwork.PhotonServerSettings.RpcList.IndexOf(method);

                if (options.Receivers == ReceiverGroup.All || (options.TargetActors != null && options.TargetActors.Contains(NetworkSystem.Instance.LocalPlayer.ActorNumber)))
                {
                    if (options.Receivers == ReceiverGroup.All)
                        options.Receivers = ReceiverGroup.Others;

                    if (options.TargetActors != null && options.TargetActors.Contains(NetworkSystem.Instance.LocalPlayer.ActorNumber))
                        options.TargetActors = options.TargetActors.Where(id => id != NetworkSystem.Instance.LocalPlayer.ActorNumber).ToArray();

                    PhotonNetwork.ExecuteRpc(rpcData, PhotonNetwork.LocalPlayer);
                }

                else
                {
                    PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent(Photon.Pun.PunEvent.RPC, rpcData, options, new SendOptions
                    {
                        Reliability = true,
                        DeliveryMode = DeliveryMode.ReliableUnsequenced,
                        Encrypt = false
                    });
                }
            }
            return false;
        }

        public static void GuardianPhysicalFreezeGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > flingDelay)
                    {
                        BetaSetVelocityPlayer(lockTarget.GetPlayer(), Vector3.zero);
                        RPCProtection();
                        flingDelay = Time.time + 0.1f;
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void GuardianPhysicalFreezeAll()
        {
            if (rightTrigger > 0.5f && Time.time > flingDelay)
            {
                flingDelay = Time.time + 0.1f;
                BetaSetVelocityTargetGroup(RpcTarget.Others, Vector3.zero);
                RPCProtection();
            }
        }

        public static void GuardianBringPlayer(NetPlayer player)
        {
            if (Time.time > flingDelay)
            {
                BetaSetVelocityPlayer(player, (GorillaTagger.Instance.bodyCollider.transform.position - GetVRRigFromPlayer(player).transform.position).normalized * 20f);
                RPCProtection();
                flingDelay = Time.time + 0.1f;
            }
        }

        public static void GuardianBringPlayerGun(NetPlayer player)
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    if (Time.time > flingDelay)
                    {
                        BetaSetVelocityPlayer(player, Vector3.Normalize(NewPointer.transform.position - GetVRRigFromPlayer(player).transform.position) * 50f);
                        RPCProtection();
                        flingDelay = Time.time + 0.2f;
                    }
                }
            }
        }

        public static void GuardianBringGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > flingDelay)
                    {
                        BetaSetVelocityPlayer(lockTarget.GetPlayer(), (GorillaTagger.Instance.bodyCollider.transform.position - lockTarget.transform.position).normalized * 20f);
                        RPCProtection();
                        flingDelay = Time.time + 0.1f;
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void GuardianBringAll()
        {
            if (rightTrigger > 0.5f && Time.time > flingDelay)
            {
                flingDelay = Time.time + 0.2f;
                foreach (var plr in VRRigExtensions.ActiveRigs.Where(plr => !plr.isLocal))
                {
                    BetaSetVelocityPlayer(GetPlayerFromVRRig(plr), (GorillaTagger.Instance.bodyCollider.transform.position - plr.transform.position).normalized * 20f);
                    RPCProtection();
                }
            }
        }

        public static void GuardianBringAwayGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > flingDelay)
                    {
                        BetaSetVelocityPlayer(lockTarget.GetPlayer(), (lockTarget.transform.position - GorillaTagger.Instance.bodyCollider.transform.position).normalized * 20f);
                        RPCProtection();
                        flingDelay = Time.time + 0.1f;
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void GuardianBringAwayAll()
        {
            if (rightTrigger > 0.5f && Time.time > flingDelay)
            {
                flingDelay = Time.time + 0.2f;
                foreach (var plr in VRRigExtensions.ActiveRigs.Where(plr => !plr.isLocal))
                {
                    BetaSetVelocityPlayer(GetPlayerFromVRRig(plr), (plr.transform.position - GorillaTagger.Instance.bodyCollider.transform.position).normalized * 20f);
                    RPCProtection();
                }
            }
        }

        public static void GuardianOrbitAll()
        {
            float scale = 5f;
            if (rightTrigger > 0.5f && Time.time > flingDelay)
            {
                flingDelay = Time.time + 0.2f;
                int index = 0;

                VRRig[] rigs = VRRigExtensions.ActiveRigs.Where(rig => !rig.isLocal).ToArray();
                foreach (VRRig rig in rigs)
                {
                    float offset = 360f / rigs.Length * index;
                    Vector3 targetPosition = GorillaTagger.Instance.headCollider.transform.position + new Vector3(MathF.Cos(offset + Time.time) * scale, 2, MathF.Sin(offset + Time.time) * scale);

                    BetaSetVelocityPlayer(rig.GetPlayer(), (targetPosition - rig.transform.position) * 1f);
                    RPCProtection();
                    index++;
                }
            }
        }

        private static float thingdeb;
        public static void GuardianGiveFlyGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (Time.time > thingdeb)
                    {
                        if (lockTarget.rightThumb.calcT > 0.5f)
                        {
                            BetaSetVelocityPlayer(lockTarget.GetPlayer(), lockTarget.headMesh.transform.forward * Movement._flySpeed);
                            RPCProtection();
                        }
                        thingdeb = Time.time + 0.1f;
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void GuardianGiveFlyAll()
        {
            if (Time.time > thingdeb)
            {
                thingdeb = Time.time + 0.1f;
                foreach (var plr in VRRigExtensions.ActiveRigs.Where(plr => !plr.isLocal).Where(plr => plr.rightThumb.calcT > 0.5f))
                {
                    BetaSetVelocityPlayer(GetPlayerFromVRRig(plr), plr.headMesh.transform.forward * Movement._flySpeed);
                    RPCProtection();
                }
            }
        }

        public static void GuardianPunchMod()
        {
            if (Time.time > thingdeb)
            {
                foreach (VRRig rig in VRRigExtensions.ActiveRigs)
                {
                    bool leftHand = Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, rig.headMesh.transform.position) < 0.25f;
                    bool rightHand = Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, rig.headMesh.transform.position) < 0.25f;

                    if (!rig.isLocal && (leftHand || rightHand))
                    {
                        Vector3 vel = rightHand ? GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0) : GTPlayer.Instance.LeftHand.velocityTracker.GetAverageVelocity(true, 0);

                        BetaSetVelocityPlayer(rig.GetPlayer(), vel);
                        thingdeb = Time.time + 0.1f;

                        if (Buttons.GetIndex("Graphic Punch Mod").enabled)
                            Projectiles.SendProjectile(Projectiles.FindProjectile("Apple"), rig.headMesh.transform.position, Vector3.down * 600f, new Color32(100, 0, 0, 255));
                    }
                }
            }
        }

        public static void GuardianBoxing()
        {
            foreach (VRRig rig1 in VRRigExtensions.ActiveRigs)
            {
                if (Time.time < Projectiles.GetBoxingDelay(rig1))
                    continue;

                foreach (var targetDirection in from rig2 in VRRigExtensions.ActiveRigs where rig2 != rig1 where Vector3.Distance(rig2.leftHandTransform.position, rig1.headMesh.transform.position) < 0.25f || Vector3.Distance(rig2.rightHandTransform.position, rig1.headMesh.transform.position) < 0.25f select (rig1.headMesh.transform.position - rig2.headMesh.transform.position) * 20f)
                {
                    BetaSetVelocityPlayer(GetPlayerFromVRRig(rig1), targetDirection);
                    Projectiles.SetBoxingDelay(rig1);
                }
            }
        }

        public static void GuardianBringAllGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    if (Time.time > flingDelay)
                    {
                        foreach (var plr in VRRigExtensions.ActiveRigs.Where(plr => !plr.isLocal))
                            BetaSetVelocityPlayer(GetPlayerFromVRRig(plr), Vector3.Normalize(NewPointer.transform.position - plr.transform.position) * 50f);

                        RPCProtection();
                        flingDelay = Time.time + 0.2f;
                    }
                }
            }
        }

        public static void GuardianBringAwayAllGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    if (Time.time > flingDelay)
                    {
                        foreach (var plr in VRRigExtensions.ActiveRigs.Where(plr => !plr.isLocal))
                            BetaSetVelocityPlayer(GetPlayerFromVRRig(plr), Vector3.Normalize(plr.transform.position - NewPointer.transform.position) * 50f);

                        RPCProtection();
                        flingDelay = Time.time + 0.2f;
                    }
                }
            }
        }

        public static void GuardianAntiStump()
        {
            if (Time.time > flingDelay)
            {
                foreach (VRRig rig in VRRigExtensions.ActiveRigs)
                {
                    if (!rig.isLocal)
                    {
                        Vector3 stump = new Vector3(-66f, 12f, -79f);
                        if (Vector3.Distance(stump, rig.transform.position) < 3f)
                        {
                            BetaSetVelocityPlayer(rig.GetPlayer(), (rig.transform.position - stump).normalized * 20f);
                            flingDelay = Time.time + 0.2f;
                        }
                    }
                }
            }
        }

        private static float slamDel;
        private static bool flip;
        public static void GuardianEffectSpamHands()
        {
            if (rightGrab)
            {
                if (Time.time > slamDel)
                {
                    GorillaGuardianManager gman = (GorillaGuardianManager)GorillaGameManager.instance;
                    if (gman.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
                    {
                        GameMode.ActiveNetworkHandler.NetView.GetView.RPC(flip ? "ShowSlamEffect" : "ShowSlapEffects", RpcTarget.All, GorillaTagger.Instance.rightHandTransform.position, new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)));
                        RPCProtection();
                        flip = !flip;
                    }
                    else
                        NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must be guardian.");

                    slamDel = Time.time + 0.05f;
                }
            }
            if (leftGrab)
            {
                if (Time.time > slamDel)
                {
                    GorillaGuardianManager gman = (GorillaGuardianManager)GorillaGameManager.instance;
                    if (gman.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
                    {
                        GameMode.ActiveNetworkHandler.NetView.GetView.RPC(flip ? "ShowSlamEffect" : "ShowSlapEffects", RpcTarget.All, GorillaTagger.Instance.leftHandTransform.position, new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)));
                        RPCProtection();
                        flip = !flip;
                    }
                    else
                        NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must be guardian.");

                    slamDel = Time.time + 0.05f;
                }
            }
        }

        public static void GuardianEffectSpamGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    GorillaGuardianManager gman = (GorillaGuardianManager)GorillaGameManager.instance;
                    if (Time.time > slamDel)
                    {
                        if (gman.IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
                        {
                            GameMode.ActiveNetworkHandler.NetView.GetView.RPC(flip ? "ShowSlamEffect" : "ShowSlapEffects", RpcTarget.All, NewPointer.transform.position, new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)));
                            RPCProtection();
                            flip = !flip;
                        }
                        else
                            NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must be guardian.");

                        slamDel = Time.time + 0.05f;
                    }

                }
            }
        }

        //private static float freezeAllDelay;
        //public static bool muteOnFreeze;
        //public static bool acklowledgeFreeze;
        public static void FreezeServer(float delay = 1f, int eventCount = 11, RaiseEventOptions options = null)
        {
            return;
            /*
            if (!NetworkSystem.Instance.InRoom) return;

            if (!acklowledgeFreeze)
            {
                acklowledgeFreeze = true;
                PromptSingle("Using mods that rely on freezing the server could not work unless a player leaves or joins. Just so you know.");
            }
                
            options ??= new RaiseEventOptions
            {
                Flags = new WebFlags(byte.MaxValue),
                TargetActors = new[] { -1 }
            };

            if (muteOnFreeze)
            {
                for (int i = 0; i < 10; i++)
                    MuteTarget(options);
            }

            if (Time.time > freezeAllDelay)
            {
                for (int i = 0; i < eventCount; i++)
                    PhotonNetwork.NetworkingClient.OpRaiseEvent(200, new object[] { serverLink }, options, SendOptions.SendUnreliable);

                RPCProtection();
                freezeAllDelay = Time.time + delay;
            }
            */
        }

        private static float closeRoomDelay;
        public static void CloseRoom()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            if (Time.time < closeRoomDelay) return;

            closeRoomDelay = Time.time + 0.1f;

            for (int i = 0; i < 40; i++)
            {
                WebFlags flags = new WebFlags(byte.MaxValue);
                RaiseEventOptions options = new RaiseEventOptions
                {
                    Flags = flags,
                    Receivers = ReceiverGroup.All,
                    CachingOption = EventCaching.AddToRoomCacheGlobal
                };
                byte code = 51;
                PhotonNetwork.RaiseEvent(code, new object[] { serverLink }, options, SendOptions.SendUnreliable);
            }

            RPCProtection();
        }

        public static float zaWarudoNotificationDelay;

        public static Coroutine ZaWarudo_StartCoroutineVariable;
        public static Coroutine ZaWarudo_EndCoroutineVariable;

        public static AudioClip ZaWarudo_Start;
        public static AudioClip ZaWarudo_Stop;

        public static void ZaWarudo_enableMethod()
        {
            LoadSoundFromURL($"{PluginInfo.ServerResourcePath}/Audio/Mods/Overpowered/Timestop/start.ogg", "Audio/Mods/Overpowered/Timestop/start.ogg", clip => ZaWarudo_Start = clip);
            LoadSoundFromURL($"{PluginInfo.ServerResourcePath}/Audio/Mods/Overpowered/Timestop/end.ogg", "Audio/Mods/Overpowered/Timestop/end.ogg", clip => ZaWarudo_Stop = clip);
        }

        private static bool zaWarudoTrigger;
        public static void ZaWarudo()
        {
            if (!NetworkSystem.Instance.InRoom) return;

            if (rightTrigger > 0.5f)
            {
                if (Buttons.GetIndex("No Freeze Za Warudo").enabled)
                {
                    if (!Buttons.GetIndex("No Freeze Za Warudo").enabled)
                        SerializePatch.OverrideSerialization = () => false;

                    if (!zaWarudoTrigger)
                    {
                        if (ZaWarudo_StartCoroutineVariable != null)
                        {
                            CoroutineManager.instance.StopCoroutine(ZaWarudo_StartCoroutineVariable);
                            ZaWarudo_StartCoroutineVariable = null;
                        }

                        if (ZaWarudo_EndCoroutineVariable != null)
                        {
                            CoroutineManager.instance.StopCoroutine(ZaWarudo_StartCoroutineVariable);
                            ZaWarudo_EndCoroutineVariable = null;
                        }

                        ZaWarudo_StartCoroutineVariable = CoroutineManager.instance.StartCoroutine(ZaWarudo_StartCoroutine());
                    }

                    zaWarudoTrigger = true;

                    Movement.LowGravity();

                    if (!Buttons.GetIndex("No Freeze Za Warudo").enabled)
                        FreezeServer();
                }
            }
            else
            {
                if (zaWarudoTrigger)
                {
                    if (ZaWarudo_StartCoroutineVariable != null)
                    {
                        CoroutineManager.instance.StopCoroutine(ZaWarudo_StartCoroutineVariable);
                        ZaWarudo_StartCoroutineVariable = null;
                    }

                    if (ZaWarudo_EndCoroutineVariable != null)
                    {
                        CoroutineManager.instance.StopCoroutine(ZaWarudo_EndCoroutineVariable);
                        ZaWarudo_EndCoroutineVariable = null;
                    }

                    ZaWarudo_EndCoroutineVariable = CoroutineManager.instance.StartCoroutine(ZaWarudo_StopCoroutine());
                }

                SerializePatch.OverrideSerialization = null;
                zaWarudoTrigger = false;
            }
        }

        public static IEnumerator ZaWarudo_StartCoroutine()
        {
            Sound.PlayAudio(ZaWarudo_Start);
            yield return new WaitForSeconds(2.4f);

            float endWhiteFadeTime = Time.time;
            Vector3 originPoint = GorillaTagger.Instance.bodyCollider.transform.position;

            while (Time.time < endWhiteFadeTime + 0.2f)
            {
                float t = (Time.time - endWhiteFadeTime) / 0.2f;
                Fun.HueShift(Color.Lerp(Color.clear, Color.white, t));

                TeleportPlayer(originPoint + RandomVector3(t * 0.2f));

                yield return null;
            }

            float purpleFadeTime = Time.time;

            while (Time.time < purpleFadeTime + 2f)
            {
                float t = (Time.time - purpleFadeTime) / 2f;
                Fun.HueShift(Color.Lerp(Color.white, new Color32(120, 47, 196, 100), t));

                TeleportPlayer(originPoint + RandomVector3((1 - t) * 0.2f));

                yield return null;
            }

            TeleportPlayer(originPoint);

            Fun.HueShift(new Color32(120, 47, 196, 100));

            ZaWarudo_StartCoroutineVariable = null;
        }

        public static IEnumerator ZaWarudo_StopCoroutine()
        {
            Sound.PlayAudio(ZaWarudo_Stop);
            yield return new WaitForSeconds(0.5f);

            float purpleFadeTime = Time.time;

            while (Time.time < purpleFadeTime + 1f)
            {
                float t = Time.time - purpleFadeTime;
                Fun.HueShift(Color.Lerp(new Color32(120, 47, 196, 100), new Color32(120, 47, 196, 0), t));

                yield return null;
            }

            Fun.HueShift(Color.clear);

            ZaWarudo_EndCoroutineVariable = null;
        }

        public static int lagIndex = 2;
        public static int lagAmount;
        public static float lagDelay;
        public static readonly string[] LagPowerNames = { "Light", "Heavy", "Spike", "Stutter", "Freeze" };
        public static readonly int[] LagAmounts = { 40, 113, 425, 1000, 3800 };
        public static readonly float[] LagDelays = { 0.1f, 0.25f, 1f, 3f, 8f };
        public static void ApplyLagPower(int index)
        {
            lagIndex = index;
            lagAmount = LagAmounts[index];
            lagDelay = LagDelays[index];
        }

        public static int lagTypeIndex;
        public static readonly string[] LagTypeNames = { "Default", "Backup" };
        public static void ApplyLagType(int index) => lagTypeIndex = index;

        private static float lagDebounce;

        public static byte LagEvent
        {
            get
            {
                return lagTypeIndex switch
                {
                    0 => 3,
                    1 => 186,
                    _ => 3,
                };
            }
        }

        public static object LagData
        {
            get
            {
                return lagTypeIndex switch
                {
                    0 => default,
                    1 => new object[] { float.NaN },
                    _ => default,
                };
            }
        }

        public static void LagPlayer(object target)
        {
            if (!NetworkSystem.Instance.InRoom) return;
            if (Time.time < lagDebounce) return;

            if (target is VRRig rig) target = rig.GetPhotonPlayer();
            if (target is NetPlayer NetPlayer) target = NetPlayer.GetPlayer();

            lagDebounce = Time.time + lagDelay;

            byte eventIndex = LagEvent;
            object data = LagData;
            SendOptions sendOptions = SendOptions.SendUnreliable;
            RaiseEventOptions raiseEventOptions = RaiseEventOptions.Default;

            switch (target)
            {
                case RpcTarget rpcTarget:
                    raiseEventOptions.Receivers =
                        rpcTarget == RpcTarget.All ? ReceiverGroup.All :
                        rpcTarget == RpcTarget.MasterClient ? ReceiverGroup.MasterClient :
                        ReceiverGroup.Others;
                    break;

                case Player player:
                    raiseEventOptions.TargetActors = new[] { player.ActorNumber };
                    break;

                case int[] actorNumbers:
                    raiseEventOptions.TargetActors = actorNumbers;
                    break;
            }

            raiseEventOptions.CachingOption = EventCaching.DoNotCache;

            for (int i = 0; i < lagAmount; i++)
                PhotonNetwork.NetworkingClient.OpRaiseEvent(eventIndex, data, raiseEventOptions, sendOptions);

            RPCProtection();
        }

        public static bool GrabStatus
        {
            get => HandLinkPatch.enabled;
            set
            {
                HandLinkPatch.enabled = value;

                if (!value && !VRRig.LocalRig.enabled)
                    VRRig.LocalRig.enabled = true;
            }
        }

        public static void ForceGrabGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    ForceGrab(lockTarget, VRRig.LocalRig.transform.position);

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
                VRRig.LocalRig.enabled = true;
            }
        }

        public static void GrabFlingGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (!lockTarget.IsBeingHeld())
                    {
                        if (!lockTarget.IsNear())
                        {
                            VRRig.LocalRig.enabled = false;
                            VRRig.LocalRig.transform.position = lockTarget.transform.position;
                        }
                        else if (!VRRig.LocalRig.enabled)
                            VRRig.LocalRig.enabled = true;
                        VRRig.LocalRig.rightHandLink.TentacleTryCreateLink(lockTarget.rightHandLink);
                    }
                    else if (lockTarget.IsBeingHeld(VRRig.LocalRig))
                    {
                        if (lockTarget.rightHandLink.grabbedPlayer == VRRig.LocalRig.GetPlayer())
                        {
                            Vector3 velocity = lockTarget.transform.up * 3f;
                            lockTarget.GetNetView().SendRPC("DroppedByPlayer", lockTarget.GetPlayer(), velocity);
                        }

                    }
                }

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
                VRRig.LocalRig.enabled = true;
            }
        }

        public static void GrabFlingAll()
        {
            if (!VRRig.LocalRig.IsBeingHeld() && !lockTarget.IsNear())
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = lockTarget.transform.position;
                VRRig.LocalRig.rightHandLink.TentacleTryCreateLink(lockTarget.rightHandLink);
            }
            else if (VRRig.LocalRig.IsBeingHeld())
            {
                FlingOnGrab();
                if (!VRRig.LocalRig.enabled)
                    VRRig.LocalRig.enabled = true;
            }
        }

        public static void BringPlayerGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    ForceGrab(lockTarget, VRRig.LocalRig.transform.position - VRRig.LocalRig.transform.forward * 10f);

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void BringAllPlayers() =>
            ForceGrab(VRRig.LocalRig.transform.position - VRRig.LocalRig.transform.forward * 10f);

        public static void PushPlayerGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    ForceGrab(lockTarget, VRRig.LocalRig.transform.position = lockTarget.transform.position - lockTarget.transform.forward * 10f);

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
                VRRig.LocalRig.enabled = true;
            }
        }

        public static void PushAllPlayers()
        {
            foreach (VRRig rig in VRRigExtensions.ActiveRigs)
                ForceGrab(rig, rig.transform.position - rig.transform.forward * 10f);
        }

        public static void GrabCrashGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;
                float value = float.MaxValue / 4f;
                if (gunLocked && lockTarget != null)
                    ForceGrab(lockTarget, new Vector3(value, -value, value));

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
                VRRig.LocalRig.enabled = true;
            }
        }

        public static void GrabCrashAll()
        {
            float value = float.MaxValue / 4f;
            ForceGrab(new Vector3(value, -value, value));
        }

        public static void LagGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    LagPlayer(lockTarget.GetPlayer());

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void LagAll() =>
            LagPlayer(RpcTarget.Others);

        public static void LagAura()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            List<int> nearbyPlayers = new List<int>();

            foreach (VRRig vrrig in VRRigExtensions.ActiveRigs)
            {
                if (Vector3.Distance(vrrig.transform.position, VRRig.LocalRig.transform.position) < 4 && !vrrig.IsTagged())
                    nearbyPlayers.Add(GetPlayerFromVRRig(vrrig).ActorNumber);
                else if (nearbyPlayers.Contains(GetPlayerFromVRRig(vrrig).ActorNumber))
                    nearbyPlayers.Remove(GetPlayerFromVRRig(vrrig).ActorNumber);
            }

            if (nearbyPlayers.Count > 0)
                LagPlayer(nearbyPlayers);
        }

        public static void LagOnTouch()
        {
            if (!NetworkSystem.Instance.InRoom) return;

            List<int> touchedPlayers = new List<int>();

            foreach (VRRig rig in VRRigExtensions.ActiveRigs.Where(rig => !rig.IsLocal()))
            {
                if (rig.IsBeingTouched())
                    touchedPlayers.Add(rig.GetPlayer().ActorNumber);
            }

            if (touchedPlayers.Count > 0)
                LagPlayer(touchedPlayers);
        }

        public static void MuteTarget(object target)
        {
            RaiseEventOptions raiseOptions = new RaiseEventOptions();

            if (target is ReceiverGroup group)
                raiseOptions.Receivers = group;
            else if (target is int[] actors)
                raiseOptions.TargetActors = actors;
            else if (target is RaiseEventOptions options)
                raiseOptions = options;
            else
                return;

            SendOptions sendOptions = new SendOptions
            {
                Reliability = false,
                Channel = 0
            };


            Dictionary<byte, object> voiceData = new Dictionary<byte, object>
            {
                { 1, 255 },
                { 2, VoiceManager.Get().SamplingRate },
                { 3, 2 },
                { 4, 20000 },
                { 5, 30000 },
                { 10, null },
                { 11, (byte)0 },
                { 12, Codec.AudioOpus }
            };

            object[] eventData =
            {
                (byte)0,
                (byte)1,
                new object[] { voiceData }
            };

            PhotonVoiceNetwork.Instance.Client.OpRaiseEvent(
                202,
                eventData,
                raiseOptions,
                sendOptions
            );
        }

        public static void ServerMuteAll()
        {
            for (int i = 0; i < 2; i++)
                MuteTarget(ReceiverGroup.All);
        }

        public static void DeafenGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    for (int i = 0; i < 2; i++)
                        MuteTarget(new int[] { lockTarget.GetPlayer().ActorNumber });
                }


                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                {
                    gunLocked = false;
                    VRRig.LocalRig.enabled = true;
                }
            }
        }

        public static void DeafenAll()
        {
            for (int i = 0; i < 2; i++)
                MuteTarget(ReceiverGroup.Others);
        }

        public static void BarrelFlingGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    SendBarrelProjectile(lockTarget.transform.position + Vector3.down * 0.2f, Vector3.up * 15000, Random.rotation, new RaiseEventOptions { TargetActors = new[] { lockTarget.GetPlayer().ActorNumber } });

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                {
                    gunLocked = false;
                    VRRig.LocalRig.enabled = true;
                }
            }
        }
        public static void BarrelFlingAll()
        {
            foreach (var TargetRig in VRRigExtensions.ActiveRigs.Where(TargetRig => !TargetRig.IsTagged()))
                SendBarrelProjectile(lockTarget.transform.position + Vector3.down * 0.2f, Vector3.up * 15000, Random.rotation, new RaiseEventOptions { TargetActors = new[] { TargetRig.GetPlayer().ActorNumber } });
        }
        public static void BarrelObliterateGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    SendBarrelProjectile(lockTarget.transform.position + Vector3.down * 0.2f, lockTarget.bodyTransform.up * int.MaxValue, Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)), new RaiseEventOptions { TargetActors = new[] { lockTarget.GetPlayer().ActorNumber } });

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                {
                    gunLocked = false;
                    VRRig.LocalRig.enabled = true;
                }
            }
        }

        public static void BarrelObliterateAll()
        {
            foreach (var TargetRig in VRRigExtensions.ActiveRigs.Where(TargetRig => !TargetRig.IsTagged()))
                SendBarrelProjectile(lockTarget.transform.position + Vector3.down * 0.2f, lockTarget.bodyTransform.up * int.MaxValue, Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)), new RaiseEventOptions { TargetActors = new[] { TargetRig.GetPlayer().ActorNumber } });
        }
        public static void BarrelPunchMod()
        {
            foreach (VRRig rig in VRRigExtensions.ActiveRigs)
            {
                if (!rig.isLocal && (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, rig.headMesh.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, rig.headMesh.transform.position) < 0.25f))
                {
                    Vector3 targetDirection = rig.headMesh.transform.position - GorillaTagger.Instance.headCollider.transform.position;
                    SendBarrelProjectile(rig.transform.position + (GorillaTagger.Instance.headCollider.transform.position - rig.headMesh.transform.position).normalized * 0.1f, targetDirection.normalized * 50f, Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)), new RaiseEventOptions { TargetActors = new[] { NetPlayerToPlayer(rig.GetPlayer()).ActorNumber } });

                    if (Buttons.GetIndex("Graphic Punch Mod").enabled)
                        Projectiles.SendProjectile(Projectiles.FindProjectile("Apple"), rig.headMesh.transform.position, Vector3.down * 600f, new Color32(100, 0, 0, 255));
                }
            }
        }

        public static void BarrelCrashGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    SendBarrelProjectile(lockTarget.transform.position + Vector3.down * 0.2f, lockTarget.bodyTransform.up * 10000f, Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)), new RaiseEventOptions { TargetActors = new[] { lockTarget.GetPlayer().ActorNumber } });

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                {
                    gunLocked = false;
                    VRRig.LocalRig.enabled = true;
                }
            }
        }

        public static void BarrelCrashAll()
        {
            foreach (var TargetRig in VRRigExtensions.ActiveRigs.Where(TargetRig => !TargetRig.IsTagged()))
                SendBarrelProjectile(lockTarget.transform.position + Vector3.down * 0.2f, lockTarget.bodyTransform.up * 10000f, Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)), new RaiseEventOptions { TargetActors = new[] { TargetRig.GetPlayer().ActorNumber } });
        }

        public static void BarrelGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                if (GetGunInput(true))
                    SendBarrelProjectile(GunData.NewPointer.transform.position + Vector3.up, Vector3.zero, Quaternion.identity);
            }
        }

        public const int BarrelIndex = 618;

        public static void SendBarrelProjectile(Vector3 pos, Vector3 vel, Quaternion rot, RaiseEventOptions options = null, bool disableCooldown = false)
        {
            options ??= new RaiseEventOptions { Receivers = ReceiverGroup.Others };

            int index = BarrelIndex;

            if (Fun.DisableThrowableCoroutine != null)
                CoroutineManager.instance.StopCoroutine(Fun.DisableThrowableCoroutine);

            Fun.DisableThrowableCoroutine = CoroutineManager.instance.StartCoroutine(Fun.DisableThrowable(index));
            TransferrableObject transferrableObject = VRRig.LocalRig.myBodyDockPositions.allObjects[index];

            if (transferrableObject == null)
            {
                if (!CosmeticsOwned.Contains("Lucky Smash Barrel"))
                {
                    VRRig.LocalRig.transform.position = TryOnRoom.transform.position;
                    CosmeticsController.instance.PressWardrobeItemButton(CosmeticsController.instance.allCosmetics.FirstOrDefault(c => string.Equals(c.overrideDisplayName, "Lucky Smash Barrel", StringComparison.OrdinalIgnoreCase)), false, false);
                }
                VRRig.LocalRig.SetActiveTransferrableObjectIndex(1, index);
                transferrableObject = VRRig.LocalRig.myBodyDockPositions.allObjects[index];
            }

            if (transferrableObject == null)
            {
                LogManager.LogError("Lucky smash barrel not found, cannot send barrel projectile.");
                return;
            }

            DeployableObject barrel = transferrableObject.GetComponent<DeployableObject>();
            if (!disableCooldown && barrel.m_spamChecker.CanCallNow(Time.unscaledTime))
            {
                transferrableObject.currentState = TransferrableObject.PositionState.InRightHand;

                object[] data = {
                    barrel._deploySignal._signalID,
                    NetworkSystem.Instance.ServerTimestamp,
                    BitPackUtils.PackWorldPosForNetwork(pos),
                    BitPackUtils.PackQuaternionForNetwork(rot),
                    BitPackUtils.PackWorldPosForNetwork(vel)
                };
                PhotonNetwork.RaiseEvent(177, data, options, SendOptions.SendReliable);
                barrel._child.Deploy(barrel, pos, rot, vel);
                RPCProtection();
            }
        }

        public static void BarrelKickGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    Vector3 targetDirection = new Vector3(-71.33718f, 101.4977f, -93.09029f) - lockTarget.headMesh.transform.position;
                    SendBarrelProjectile(lockTarget.transform.position + (lockTarget.headMesh.transform.position - new Vector3(-71.33718f, 101.4977f, -93.09029f)).normalized * 0.1f, targetDirection.normalized * 50f, Quaternion.identity, new RaiseEventOptions { TargetActors = new[] { lockTarget.GetPlayer().ActorNumber } });
                }

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                {
                    gunLocked = false;
                    VRRig.LocalRig.enabled = true;
                }
            }
        }

        public static void BarrelKickAll()
        {
            foreach (VRRig TargetRig in VRRigExtensions.ActiveRigs)
            {
                if (TargetRig.IsTagged()) continue;

                Vector3 targetDirection = new Vector3(-71.33718f, 101.4977f, -93.09029f) - TargetRig.headMesh.transform.position;
                SendBarrelProjectile(TargetRig.transform.position + (TargetRig.headMesh.transform.position - new Vector3(-71.33718f, 101.4977f, -93.09029f)).normalized * 0.1f, targetDirection.normalized * 50f, Quaternion.identity, new RaiseEventOptions { TargetActors = new[] { TargetRig.GetPlayer().ActorNumber } });
            }
        }

        public static void BarrelFlingTowardsGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    SendBarrelProjectile(lockTarget.transform.position + (GorillaTagger.Instance.headCollider.transform.position - lockTarget.headMesh.transform.position).normalized * 0.1f, (GorillaTagger.Instance.bodyCollider.transform.position - lockTarget.transform.position).normalized * 5000f, Quaternion.identity, new RaiseEventOptions { TargetActors = new[] { NetPlayerToPlayer(lockTarget.GetPlayer()).ActorNumber } });

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                {
                    gunLocked = false;
                    VRRig.LocalRig.enabled = true;
                }
            }
        }

        public static void BarrelFlingTowardsAll()
        {
            foreach (var TargetRig in VRRigExtensions.ActiveRigs.Where(TargetRig => !TargetRig.IsTagged()))
                SendBarrelProjectile(TargetRig.transform.position + (GorillaTagger.Instance.headCollider.transform.position - TargetRig.headMesh.transform.position).normalized * 0.1f, (GorillaTagger.Instance.bodyCollider.transform.position - TargetRig.transform.position).normalized * 5000f, Quaternion.identity, new RaiseEventOptions { TargetActors = new[] { NetPlayerToPlayer(TargetRig.GetPlayer()).ActorNumber } }, true);
        }

        public static void CityKickGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    SendBarrelProjectile(lockTarget.transform.position + (lockTarget.transform.position - new Vector3(-71.14215f, 13.73829f, -95.17883f)).normalized * 0.1f, (new Vector3(-71.14215f, 13.73829f, -95.17883f) - lockTarget.transform.position).normalized * 5000f, Quaternion.identity, new RaiseEventOptions { TargetActors = new[] { NetPlayerToPlayer(lockTarget.GetPlayer()).ActorNumber } });

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                {
                    gunLocked = false;
                    VRRig.LocalRig.enabled = true;
                }
            }
        }

        public static void CityKickAll()
        {
            foreach (VRRig TargetRig in VRRigExtensions.ActiveRigs)
            {
                if (TargetRig.IsTagged()) continue;
                SendBarrelProjectile(TargetRig.transform.position + (TargetRig.transform.position - new Vector3(-71.14215f, 13.73829f, -95.17883f)).normalized * 0.1f, (new Vector3(-71.14215f, 13.73829f, -95.17883f) - TargetRig.transform.position).normalized * 5000f, Quaternion.identity, new RaiseEventOptions { TargetActors = new[] { TargetRig.GetPlayer().ActorNumber } });
            }
        }

        private static float notifyTime;
        public static bool IsModded(bool notify)
        {
            if (!NetworkSystem.Instance.InRoom) return false;
            bool modded = NetworkSystem.Instance.GameModeString.Contains("MODDED_");
            if (!modded && notify && Time.time > notifyTime)
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not in a modded gamemode. Use Utilla to create one, or join an already existing modded room.");
                notifyTime = Time.time + 1;
            }
            return modded;
        }

        public static float delay;

        public static void BetaNearbyFollowCommand(GorillaFriendCollider friendCollider, Player player)
        {
            PhotonNetworkController.Instance.FriendIDList.Add(player.UserId);

            object[] groupJoinSendData = new object[2];
            groupJoinSendData[0] = PhotonNetworkController.Instance.shuffler;
            groupJoinSendData[1] = PhotonNetworkController.Instance.keyStr;
            NetEventOptions netEventOptions = new NetEventOptions { TargetActors = new[] { player.ActorNumber } };

            if (friendCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId) && friendCollider.playerIDsCurrentlyTouching.Contains(player.UserId) && player != PhotonNetwork.LocalPlayer)
                RoomSystem.SendEvent(4, groupJoinSendData, netEventOptions, false);
            else if (!friendCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId))
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not in stump.");
        }

        public static IEnumerator StumpKickDelay(Action action, Action action2, float extraDelay = 0f, bool changeQueue = false)
        {
            PhotonNetworkController.Instance.FriendIDList.Clear();
            yield return new WaitForSeconds(extraDelay);

            bool joinedRoomPatchEnabled = JoinedRoomPatch.enabled;

            string queueArchive = GorillaComputer.instance.currentQueue;
            if (changeQueue)
                GorillaComputer.instance.currentQueue = RandomString();

            action?.Invoke();
            yield return new WaitForSeconds(0.3f);
            action2?.Invoke();
            yield return new WaitForSeconds(1f);

            if (changeQueue)
                GorillaComputer.instance.currentQueue = queueArchive;

            yield return new WaitForSeconds(30f);

            JoinedRoomPatch.enabled = joinedRoomPatchEnabled;
        }

        public static bool kickToPublic;
        public static bool rejoinOnKick;
        public static string specificRoom;
        public static void CreateKickRoom()
        {
            if (rejoinOnKick)
            {
                Important.BroadcastRoom(specificRoom ?? RandomString(), true, PhotonNetworkController.Instance.keyToFollow, PhotonNetworkController.Instance.shuffler);
                Important.Reconnect();

                return;
            }

            Important.CreateRoom(specificRoom ?? RandomString(), kickToPublic, 0, JoinType.JoinWithNearby);
        }

        private static float kickDelay;
        public static void StumpKickGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > kickDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        NetPlayer player = GetPlayerFromVRRig(gunTarget);
                        kickDelay = Time.time + 0.5f;

                        if (!GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(player.UserId))
                        {
                            NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> The player must be in stump.");
                            return;
                        }

                        if (!NetworkSystem.Instance.SessionIsPrivate)
                        {
                            NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must be in a private room.");
                            return;
                        }

                        CoroutineManager.instance.StartCoroutine(StumpKickDelay(() =>
                        {
                            PhotonNetworkController.Instance.shuffler = Random.Range(0, 99).ToString().PadLeft(2, '0') + Random.Range(0, 99999999).ToString().PadLeft(8, '0');
                            PhotonNetworkController.Instance.keyStr = Random.Range(0, 99999999).ToString().PadLeft(8, '0');

                            BetaNearbyFollowCommand(GorillaComputer.instance.friendJoinCollider, NetPlayerToPlayer(player));
                            RPCProtection();
                        }, () =>
                        {
                            CreateKickRoom();
                        }));
                    }
                }
            }
        }

        public static void StumpKickAll()
        {
            if (NetworkSystem.Instance.InRoom)
            {
                if (!NetworkSystem.Instance.SessionIsPrivate)
                {
                    NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must be in a private room.");
                    return;
                }

                CoroutineManager.instance.StartCoroutine(StumpKickDelay(() =>
                {
                    PhotonNetworkController.Instance.shuffler = Random.Range(0, 99).ToString().PadLeft(2, '0') + Random.Range(0, 99999999).ToString().PadLeft(8, '0');
                    PhotonNetworkController.Instance.keyStr = Random.Range(0, 99999999).ToString().PadLeft(8, '0');

                    foreach (VRRig rig in VRRigExtensions.ActiveRigs.Where(rig => !rig.IsLocal() && GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(rig.GetPlayer().UserId)))
                        BetaNearbyFollowCommand(GorillaComputer.instance.friendJoinCollider, NetPlayerToPlayer(rig.GetPlayer()));

                    RPCProtection();
                }, () =>
                {
                    CreateKickRoom();
                }));
            }
            else
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not in a room.");
        }

        private static float elevatorKickDelay;
        public static void ElevatorKickGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal() && Time.time > elevatorKickDelay)
                    {
                        elevatorKickDelay = Time.time + 0.5f;

                        if (PhotonNetwork.IsMasterClient)
                            SpecialTimeRPC(GRElevatorManager._instance.photonView, -750, "RemoteActivateTeleport", new RaiseEventOptions { TargetActors = new[] { gunTarget.GetPlayer().ActorNumber } }, (int)GRElevatorManager._instance.currentLocation, 3, GRElevatorManager.LowestActorNumberInElevator());
                        else
                            GRElevatorManager._instance.SendRPC("RemoteElevatorButtonPress", RpcTarget.MasterClient, new[] { 3, (int)GRElevatorManager._instance.currentLocation });

                        RPCProtection();
                    }
                }
            }
        }

        public static void ElevatorKickAll()
        {
            if (PhotonNetwork.IsMasterClient)
                SpecialTimeRPC(GRElevatorManager._instance.photonView, -750, "RemoteActivateTeleport", new RaiseEventOptions { Receivers = ReceiverGroup.Others }, (int)GRElevatorManager._instance.currentLocation, 2, GRElevatorManager.LowestActorNumberInElevator());
            else
                GRElevatorManager._instance.SendRPC("RemoteElevatorButtonPress", RpcTarget.MasterClient, new[] { 3, (int)GRElevatorManager._instance.currentLocation });
        }

        public static void ElevatorKickAura()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            List<VRRig> nearbyPlayers = new List<VRRig>();

            foreach (VRRig vrrig in VRRigExtensions.ActiveRigs)
            {
                if (Vector3.Distance(vrrig.transform.position, VRRig.LocalRig.transform.position) < 4 && !vrrig.IsLocal())
                    nearbyPlayers.Add(vrrig);
                else if (nearbyPlayers.Contains(vrrig))
                    nearbyPlayers.Remove(vrrig);
            }

            if (nearbyPlayers.Count > 0)
            {
                foreach (VRRig nearbyPlayer in nearbyPlayers)
                {
                    if (PhotonNetwork.IsMasterClient)
                        SpecialTimeRPC(GRElevatorManager._instance.photonView, -750, "RemoteActivateTeleport", new RaiseEventOptions { TargetActors = new[] { nearbyPlayer.GetPlayer().ActorNumber } }, (int)GRElevatorManager._instance.currentLocation, 3, GRElevatorManager.LowestActorNumberInElevator());
                    else
                        GRElevatorManager._instance.SendRPC("RemoteElevatorButtonPress", RpcTarget.MasterClient, new[] { 3, (int)GRElevatorManager._instance.currentLocation });

                    RPCProtection();
                }
            }
        }

        public static void ElevatorKickOnTouch()
        {
            if (!NetworkSystem.Instance.InRoom) return;

            List<VRRig> touchedPlayers = new List<VRRig>();

            foreach (VRRig rig in VRRigExtensions.ActiveRigs)
            {
                if (!rig.IsLocal())
                {
                    if (rig.IsBeingTouched())
                    {
                        touchedPlayers.Add(rig);
                    }
                }
            }

            if (touchedPlayers.Count > 0)
            {
                foreach (VRRig rig in touchedPlayers)
                {
                    if (PhotonNetwork.IsMasterClient)
                        SpecialTimeRPC(GRElevatorManager._instance.photonView, -750, "RemoteActivateTeleport", new RaiseEventOptions { TargetActors = new[] { rig.GetPlayer().ActorNumber } }, (int)GRElevatorManager._instance.currentLocation, 3, GRElevatorManager.LowestActorNumberInElevator());
                    else
                        GRElevatorManager._instance.SendRPC("RemoteElevatorButtonPress", RpcTarget.MasterClient, new[] { 3, (int)GRElevatorManager._instance.currentLocation });

                    RPCProtection();
                }
            }
        }

        public static void CreatePeerBase()
        {
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.TransportProtocol = ConnectionProtocol.Tcp;
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.peerBase = new TPeer()
            {
                DoFraming = true,
                photonPeer = PhotonNetwork.NetworkingClient.LoadBalancingPeer,
                usedTransportProtocol = ConnectionProtocol.Tcp
            };
        }

        public static void UnloadPeerBase()
        {
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.TransportProtocol = ConnectionProtocol.Udp;
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.peerBase = new EnetPeer
            {
                photonPeer = PhotonNetwork.NetworkingClient.LoadBalancingPeer,
                usedTransportProtocol = ConnectionProtocol.Udp
            };
            NetworkSystem.Instance.ReturnToSinglePlayer();
        }

        public static void BetaShuttleFollowCommand(Player player)
        {
            PhotonNetworkController.Instance.FriendIDList.Add(player.UserId);

            object[] groupJoinSendData = new object[2];
            groupJoinSendData[0] = PhotonNetworkController.Instance.shuffler;
            groupJoinSendData[1] = PhotonNetworkController.Instance.keyStr;
            NetEventOptions netEventOptions = new NetEventOptions { TargetActors = new[] { player.ActorNumber } };

            RoomSystem.SendEvent(11, groupJoinSendData, netEventOptions, false);
        }

        private static float greyZoneDelay;
        public static void ActivateGreyZoneGun(bool status)
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal() && Time.time > greyZoneDelay)
                    {
                        greyZoneDelay = Time.time + 0.1f;
                        ActivateGreyZone(status, gunTarget.GetPhotonPlayer());
                    }
                }
            }
        }

        private static Coroutine wipeOverride;
        public static IEnumerator ClearOverride()
        {
            yield return new WaitUntil(() => !NetworkSystem.Instance.InRoom);
            SerializePatch.OverrideSerialization = null;

            wipeOverride = null;
        }

        public static void ActivateGreyZone(bool status, Player target)
        {
            if (!NetworkSystem.Instance.IsMasterClient)
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                return;
            }

            SerializePatch.OverrideSerialization = () =>
            {
                MassSerialize(true, new[] { GreyZoneManager.Instance.photonView });
                SerializePatch.OverrideSerialization = null;
                return false;
            };

            wipeOverride ??= CoroutineManager.instance.StartCoroutine(ClearOverride());

            GreyZoneManager.Instance.greyZoneActive = status;
            GreyZoneManager.Instance.photonConnectedDuringActivation = NetworkSystem.Instance.InRoom;
            GreyZoneManager.Instance.greyZoneActivationTime = (GreyZoneManager.Instance.photonConnectedDuringActivation ? PhotonNetwork.Time : ((double)Time.time));

            SendSerialize(GreyZoneManager.Instance.photonView, new RaiseEventOptions { TargetActors = new[] { target.ActorNumber } });
        }

        public static void ActivateGreyZone(bool status)
        {
            if (NetworkSystem.Instance.InRoom)
            {
                if (!NetworkSystem.Instance.IsMasterClient)
                {
                    NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                    return;
                }

                if (status)
                {

                    GreyZoneManager.Instance.ActivateGreyZoneAuthority();
                }

                else if (!status)
                    GreyZoneManager.Instance.DeactivateGreyZoneAuthority();
            }
        }

        public static float spazGreyDelay;
        public static bool greyState;
        public static void SpazGreyZoneGun()
        {
            if (Time.time > spazGreyDelay)
            {
                greyState = !greyState;
                spazGreyDelay = Time.time + 0.1f;
            }

            ActivateGreyZoneGun(greyState);
        }

        public static void SpazGreyZone()
        {
            if (Time.time > spazGreyDelay)
            {
                greyState = !greyState;
                ActivateGreyZone(greyState);
                spazGreyDelay = Time.time + 0.1f;
            }
        }

        public static void KickAllInParty()
        {
            if (FriendshipGroupDetection.Instance.IsInParty)
            {
                partyLastCode = PhotonNetwork.CurrentRoom.Name;
                waitForPlayerJoin = false;
                PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(Important.RandomRoomName(), JoinType.ForceJoinWithParty);
                partyTime = Time.time + 0.25f;
                partyKickReconnecting = false;
                amountPartying = FriendshipGroupDetection.Instance.myPartyMemberIDs.Count - 1;
                NotificationManager.SendNotification("<color=grey>[</color><color=purple>PARTY</color><color=grey>]</color> Kicking " + amountPartying + " party members, please be patient..");
            }
            else
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not in a party.");
        }

        public static void BanAllInParty()
        {
            if (FriendshipGroupDetection.Instance.IsInParty)
            {
                partyLastCode = PhotonNetwork.CurrentRoom.Name;
                waitForPlayerJoin = true;
                PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(GorillaComputer.instance.anywhereTwoWeek[Random.Range(0, GorillaComputer.instance.anywhereTwoWeek.Length)], JoinType.ForceJoinWithParty);
                partyTime = Time.time + 0.25f;
                partyKickReconnecting = false;
                amountPartying = FriendshipGroupDetection.Instance.myPartyMemberIDs.Count - 1;
                NotificationManager.SendNotification("<color=grey>[</color><color=purple>PARTY</color><color=grey>]</color> Banning " + amountPartying + " party members, please be patient..");
            }
            else
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not in a party.");
        }

        public static Coroutine partyKickDelayCoroutine;
        public static IEnumerator PartyKickDelay(bool ban)
        {
            yield return new WaitForSeconds(0.25f);

            if (ban)
                BanAllInParty();
            else
                KickAllInParty();

            Coroutine thisCoroutine = partyKickDelayCoroutine;
            partyKickDelayCoroutine = null;

            CoroutineManager.instance.StopCoroutine(thisCoroutine);
        }

        public static bool previousInParty;
        public static void AutoPartyKick()
        {
            if (FriendshipGroupDetection.Instance.IsInParty && !previousInParty)
                partyKickDelayCoroutine ??= CoroutineManager.instance.StartCoroutine(PartyKickDelay(false));

            previousInParty = FriendshipGroupDetection.Instance.IsInParty;
        }

        public static void AutoPartyBan()
        {
            if (FriendshipGroupDetection.Instance.IsInParty && !previousInParty)
                partyKickDelayCoroutine ??= CoroutineManager.instance.StartCoroutine(PartyKickDelay(true));

            previousInParty = FriendshipGroupDetection.Instance.IsInParty;
        }

        private static float breakDelay;
        public static void PartyBreakNetworkTriggers()
        {
            if (FriendshipGroupDetection.Instance.IsInParty && Time.time > breakDelay)
            {
                breakDelay = Time.time + 1f;
                FriendshipGroupDetection.Instance.photonView.RPC("PartyMemberIsAboutToGroupJoin", RpcTarget.All, Array.Empty<object>());
            }
        }

        public static bool legacyKickFreeze;

        /// <summary>
        /// Indicates whether event optimization is enabled. When enabled, it reduces network load by limiting certain RPC calls and adjusting serialization rates.
        /// </summary>
        private static bool _optimizeEvents;
        public static bool OptimizeEvents
        {
            get => _optimizeEvents;
            set
            {
                if (_optimizeEvents != value)
                {
                    _optimizeEvents = value;
                    if (_optimizeEvents)
                    {
                        if (legacyKickFreeze)
                            SerializePatch.OverrideSerialization = () => false;
                        else
                        {
                            PhotonNetwork.SerializationRate = 2;
                            RPCFilter.FilteredRPCs["OnHandTapRPC"] = () => false;
                            RPCFilter.FilteredRPCs["RPC_UpdateCosmeticsWithTryonPacked"] = () => false;

                            SerializePatch.OverrideSerialization = () =>
                            {
                                VRRig.LocalRig.GetPhotonView().Serialize();
                                return true;
                            };
                        }
                    }
                    else
                    {
                        if (SerializePatch.OverrideSerialization != null)
                            SerializePatch.OverrideSerialization = null;

                        PhotonNetwork.SerializationRate = 10;
                        RPCFilter.FilteredRPCs.Remove("OnHandTapRPC");
                        RPCFilter.FilteredRPCs.Remove("RPC_UpdateCosmeticsWithTryonPacked");
                    }
                }
            }
        }

        public static void PartyKickGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    OptimizeEvents = true;

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        if (lockTarget == null && FriendshipGroupDetection.Instance.IsInMyGroup(gunTarget.GetPlayer().UserId))
                        {
                            for (int i = 0; i < 3970; i++)
                                FriendshipGroupDetection.Instance.photonView.RPC("RequestPartyGameMode", gunTarget.GetPhotonPlayer(),
                                    new object[] { GameMode.gameModeKeyByName.Keys.ToArray()[Random.Range(0, GameMode.gameModeKeyByName.Keys.Count)] });

                            RPCProtection();
                        }

                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                OptimizeEvents = false;
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void PartyKickAll()
        {
            SerializePatch.OverrideSerialization = () => false;

            if (Time.time > kickDelay)
            {
                kickDelay = Time.time + 10f;
                for (int i = 0; i < 3950; i++)
                    FriendshipGroupDetection.Instance.photonView.RPC
                    (
                        "RequestPartyGameMode",
                        new RaiseEventOptions
                        {
                            TargetActors =
                            NetworkSystem.Instance.PlayerListOthers
                                .Where(plr => FriendshipGroupDetection.Instance.IsInMyGroup(plr.UserId))
                                .Select(plr => plr.ActorNumber).ToArray()
                        },
                        new object[]
                        {
                            GameMode.gameModeKeyByName.Keys.ToArray()[Random.Range(0, GameMode.gameModeKeyByName.Keys.Count)]
                        }
                    );

                RPCProtection();
            }
        }

        public static void PartyKickAura()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            List<VRRig> nearbyPlayers = new List<VRRig>();

            foreach (VRRig vrrig in VRRigExtensions.ActiveRigs)
            {
                if (Vector3.Distance(vrrig.transform.position, VRRig.LocalRig.transform.position) < 4 && !vrrig.IsLocal())
                    nearbyPlayers.Add(vrrig);
                else if (nearbyPlayers.Contains(vrrig))
                    nearbyPlayers.Remove(vrrig);
            }

            if (nearbyPlayers.Count > 0)
            {
                SerializePatch.OverrideSerialization = () => false;
                foreach (VRRig nearbyPlayer in nearbyPlayers)
                {
                    for (int i = 0; i < 3950; i++)
                        FriendshipGroupDetection.Instance.photonView.RPC("RequestPartyGameMode", nearbyPlayer.GetPhotonPlayer(),
                            new object[] { GameMode.gameModeKeyByName.Keys.ToArray()[Random.Range(0, GameMode.gameModeKeyByName.Keys.Count)] });

                    RPCProtection();
                }
            }
            else
                OptimizeEvents = false;
        }

        public static void PartyKickOnTouch()
        {
            if (!NetworkSystem.Instance.InRoom) return;

            List<VRRig> touchedPlayers = new List<VRRig>();

            foreach (VRRig rig in VRRigExtensions.ActiveRigs)
            {
                if (!rig.IsLocal())
                {
                    if (rig.IsBeingTouched())
                    {
                        touchedPlayers.Add(rig);
                    }
                }
            }

            if (touchedPlayers.Count > 0)
            {
                SerializePatch.OverrideSerialization = () => false;
                foreach (VRRig rig in touchedPlayers)
                {
                    for (int i = 0; i < 3950; i++)
                        FriendshipGroupDetection.Instance.photonView.RPC("RequestPartyGameMode", rig.GetPhotonPlayer(),
                            new object[] { GameMode.gameModeKeyByName.Keys.ToArray()[Random.Range(0, GameMode.gameModeKeyByName.Keys.Count)] });

                    RPCProtection();
                }
            }
            else
                OptimizeEvents = false;
        }

        private static float antiReportLagDelay;
        public static void AntiReportLag()
        {
            if (Time.time > antiReportLagDelay)
            {
                List<int> actors = new List<int>();

                Safety.AntiReport((vrrig, position) =>
                {
                    antiReportLagDelay = Time.time + 0.1f;
                    actors.Add(GetPlayerFromVRRig(vrrig).ActorNumber);
                    NotificationManager.SendNotification("<color=grey>[</color><color=purple>ANTI-REPORT</color><color=grey>]</color> " + GetPlayerFromVRRig(vrrig).NickName + " attempted to report you, they are being lagged.");
                });

                if (actors.Count > 0)
                    LagPlayer(actors);
            }
        }

        public static float setMasterDelay;
        public static void SetMasterClient(bool skip = false)
        {
            if (NetworkSystem.Instance.IsMasterClient)
                return;
            if (PhotonNetwork.PlayerList.Length > 5)
            {
                if (!skip)
                    NotificationManager.SendNotification($"<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> {PhotonNetwork.PlayerList.Length - 5} people must leave for this mod to work.");
                return;
            }
            if (Time.time > setMasterDelay)
            {
                PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
                setMasterDelay = Time.time + 5f;
            }
        }

        public static void SetRoomStatus(bool status)
        {
            Dictionary<byte, object> dictionary = new Dictionary<byte, object>
            {
                { OperationCode.GetProperties, new Hashtable { { GamePropertyKey.IsOpen, status }, { GamePropertyKey.IsVisible, status }, { GamePropertyKey.MaxPlayers, status ? 0 : PhotonNetworkController.Instance.currentJoinTrigger.GetRoomSize(SubscriptionManager.IsLocalSubscribed()) } } },
                { OperationCode.AuthenticateOnce, null }
            };

            PhotonNetwork.CurrentRoom.LoadBalancingClient.LoadBalancingPeer.SendOperation(
                OperationCode.SetProperties,
                dictionary,
                SendOptions.SendReliable
            );
            GorillaScoreboardTotalUpdater.instance.UpdateActiveScoreboards();
        }

        public static void DestroyGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > destroyDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        DestroyPlayer(NetPlayerToPlayer(GetPlayerFromVRRig(gunTarget)));
                        destroyDelay = Time.time + 0.5f;
                    }
                }
            }
        }

        public static void DestroyAll()
        {
            foreach (Player player in PhotonNetwork.PlayerListOthers)
                DestroyPlayer(player);
        }

        public static void DestroyAura()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            List<VRRig> nearbyPlayers = new List<VRRig>();

            foreach (VRRig vrrig in VRRigExtensions.ActiveRigs)
            {
                if (Vector3.Distance(vrrig.transform.position, VRRig.LocalRig.transform.position) < 4 && !vrrig.IsLocal())
                    nearbyPlayers.Add(vrrig);
                else if (nearbyPlayers.Contains(vrrig))
                    nearbyPlayers.Remove(vrrig);
            }

            if (nearbyPlayers.Count > 0)
            {
                foreach (VRRig nearbyPlayer in nearbyPlayers)
                {
                    DestroyPlayer(NetPlayerToPlayer(GetPlayerFromVRRig(nearbyPlayer)));
                }
            }
        }

        public static void DestroyOnTouch()
        {
            if (!NetworkSystem.Instance.InRoom) return;

            List<VRRig> touchedPlayers = new List<VRRig>();

            foreach (VRRig rig in VRRigExtensions.ActiveRigs)
            {
                if (!rig.IsLocal())
                {
                    if (rig.IsBeingTouched())
                    {
                        touchedPlayers.Add(rig);
                    }
                }
            }

            if (touchedPlayers.Count > 0)
            {
                foreach (VRRig rig in touchedPlayers)
                {
                    DestroyPlayer(NetPlayerToPlayer(rig.GetPlayer()));
                }
            }
        }

        public static void DestroyPlayer(NetPlayer player) =>
            PhotonNetwork.OpRemoveCompleteCacheOfPlayer(player.ActorNumber);

        public static void ChangeLavaState(InfectionLavaController.RisingLavaState state)
        {
            InfectionLavaController controller = InfectionLavaController.ActiveControllers.FirstOrDefault();

            if (controller != null)
            {
                controller.JumpToState(state);
                controller.reliableState.stateStartTime = NetworkSystem.Instance.InRoom ? NetworkSystem.Instance.SimTime : Time.timeAsDouble;
            }
        }

        public static void TargetSpam()
        {
            if (NetworkSystem.Instance.IsMasterClient)
            {
                foreach (HitTargetNetworkState hitTargetNetworkState in GetAllType<HitTargetNetworkState>())
                {
                    hitTargetNetworkState.hitCooldownTime = 0;
                    hitTargetNetworkState.TargetHit(Vector3.zero, Vector3.zero);
                }
            }
            else NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void InfectionToTag()
        {
            if (!NetworkSystem.Instance.IsMasterClient)
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
            else
            {
                GorillaTagManager gorillaTagManager = (GorillaTagManager)GorillaGameManager.instance;
                gorillaTagManager.infectedModeThreshold = PhotonNetwork.CurrentRoom.MaxPlayers + 1;
            }
        }

        public static void TagToInfection()
        {
            if (!NetworkSystem.Instance.IsMasterClient)
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
            else
            {
                GorillaTagManager gorillaTagManager = (GorillaTagManager)GorillaGameManager.instance;
                gorillaTagManager.infectedModeThreshold = 1;
            }
        }

        public static void FixThreshold()
        {
            GorillaTagManager gorillaTagManager = (GorillaTagManager)GorillaGameManager.instance;
            gorillaTagManager.infectedModeThreshold = 4;
        }

        private static float rockDebounce;
        public static void RockSelf()
        {
            if (PhotonNetwork.IsMasterClient)
                AddRock(NetworkSystem.Instance.LocalPlayer);
            else
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
        }

        public static void RockGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal() && Time.time > rockDebounce)
                    {
                        rockDebounce = Time.time + 0.1f;
                        if (PhotonNetwork.IsMasterClient)
                            AddRock(GetPlayerFromVRRig(gunTarget));
                        else
                            NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                    }
                }
            }
        }

        public static void RockAura()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            List<VRRig> nearbyPlayers = new List<VRRig>();

            foreach (VRRig vrrig in VRRigExtensions.ActiveRigs)
            {
                if (Vector3.Distance(vrrig.transform.position, VRRig.LocalRig.transform.position) < 4 && !vrrig.IsLocal())
                    nearbyPlayers.Add(vrrig);
                else if (nearbyPlayers.Contains(vrrig))
                    nearbyPlayers.Remove(vrrig);
            }

            if (nearbyPlayers.Count > 0)
            {
                foreach (VRRig nearbyPlayer in nearbyPlayers)
                {
                    if (Time.time > rockDebounce)
                    {
                        rockDebounce = Time.time + 0.1f;
                        if (PhotonNetwork.IsMasterClient)
                            AddRock(GetPlayerFromVRRig(nearbyPlayer));
                        else
                            NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                    }
                }
            }
        }

        public static void RockOnTouch()
        {
            if (!NetworkSystem.Instance.InRoom) return;

            List<VRRig> touchedPlayers = new List<VRRig>();

            foreach (VRRig rig in VRRigExtensions.ActiveRigs)
            {
                if (!rig.IsLocal())
                {
                    if (rig.IsBeingTouched())
                    {
                        touchedPlayers.Add(rig);
                    }
                }
            }

            if (touchedPlayers.Count > 0)
            {
                foreach (VRRig rig in touchedPlayers)
                {
                    if (Time.time > rockDebounce)
                    {
                        rockDebounce = Time.time + 0.1f;
                        if (PhotonNetwork.IsMasterClient)
                            AddRock(rig.GetPlayer());
                        else
                            NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                    }
                }
            }
        }

        public static void RockAll()
        {
            if (Time.time > rockDebounce)
            {
                rockDebounce = Time.time + 0.1f;
                if (PhotonNetwork.IsMasterClient)
                    AddRock(GetRandomPlayer(true));
                else
                    NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
            }
        }

        public static void BetaSetStatus(RoomSystem.StatusEffects state, RaiseEventOptions reo)
        {
            if (!NetworkSystem.Instance.IsMasterClient)
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
            else
            {
                object[] statusSendData = new object[1];
                statusSendData[0] = (int)state;
                object[] sendEventData = new object[3];
                sendEventData[0] = NetworkSystem.Instance.ServerTimestamp;
                sendEventData[1] = (byte)2;
                sendEventData[2] = statusSendData;
                PhotonNetwork.RaiseEvent((byte)Constants.Network.ROOM_SYSTEM, sendEventData, reo, SendOptions.SendUnreliable);
            }
        }

        public static void SlowSelf()
        {
            NetPlayer player = PhotonNetwork.LocalPlayer;
            BetaSetStatus(RoomSystem.StatusEffects.TaggedTime, new RaiseEventOptions { TargetActors = new[] { player.ActorNumber } });
            RPCProtection();
        }

        private static float slowDelay;
        public static void SlowGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > slowDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        NetPlayer player = GetPlayerFromVRRig(gunTarget);
                        BetaSetStatus(RoomSystem.StatusEffects.TaggedTime, new RaiseEventOptions { TargetActors = new[] { player.ActorNumber } });
                        RPCProtection();
                        slowDelay = Time.time + 1f;
                    }
                }
            }
        }

        public static void SlowAura()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            List<VRRig> nearbyPlayers = new List<VRRig>();

            foreach (VRRig vrrig in VRRigExtensions.ActiveRigs)
            {
                if (Vector3.Distance(vrrig.transform.position, VRRig.LocalRig.transform.position) < 4 && !vrrig.IsLocal())
                    nearbyPlayers.Add(vrrig);
                else if (nearbyPlayers.Contains(vrrig))
                    nearbyPlayers.Remove(vrrig);
            }

            if (nearbyPlayers.Count > 0)
            {
                foreach (VRRig nearbyPlayer in nearbyPlayers)
                {
                    NetPlayer player = GetPlayerFromVRRig(nearbyPlayer);
                    BetaSetStatus(RoomSystem.StatusEffects.TaggedTime, new RaiseEventOptions { TargetActors = new[] { player.ActorNumber } });
                    RPCProtection();
                }
            }
        }

        public static void SlowOnTouch()
        {
            if (!NetworkSystem.Instance.InRoom) return;

            List<VRRig> touchedPlayers = new List<VRRig>();

            foreach (VRRig rig in VRRigExtensions.ActiveRigs)
            {
                if (!rig.IsLocal())
                {
                    if (rig.IsBeingTouched())
                    {
                        touchedPlayers.Add(rig);
                    }
                }
            }

            if (touchedPlayers.Count > 0)
            {
                foreach (VRRig rig in touchedPlayers)
                {
                    NetPlayer player = rig.GetPlayer();
                    BetaSetStatus(RoomSystem.StatusEffects.TaggedTime, new RaiseEventOptions { TargetActors = new[] { player.ActorNumber } });
                    RPCProtection();
                }
            }
        }

        public static void SlowAll()
        {
            if (Time.time > slowDelay)
            {
                BetaSetStatus(RoomSystem.StatusEffects.TaggedTime, new RaiseEventOptions { Receivers = ReceiverGroup.Others });
                RPCProtection();
                slowDelay = Time.time + 1f;
            }
        }

        public static void VibrateSelf()
        {
            NetPlayer owner = PhotonNetwork.LocalPlayer;
            BetaSetStatus(RoomSystem.StatusEffects.JoinedTaggedTime, new RaiseEventOptions { TargetActors = new[] { owner.ActorNumber } });
            RPCProtection();
        }

        private static float vibrateDelay;
        public static void VibrateGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > vibrateDelay)
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        NetPlayer owner = GetPlayerFromVRRig(gunTarget);
                        BetaSetStatus(RoomSystem.StatusEffects.JoinedTaggedTime, new RaiseEventOptions { TargetActors = new[] { owner.ActorNumber } });
                        RPCProtection();
                        vibrateDelay = Time.time + 0.5f;
                    }
                }
            }
        }

        public static void VibrateAura()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            List<VRRig> nearbyPlayers = new List<VRRig>();

            foreach (VRRig vrrig in VRRigExtensions.ActiveRigs)
            {
                if (Vector3.Distance(vrrig.transform.position, VRRig.LocalRig.transform.position) < 4 && !vrrig.IsLocal())
                    nearbyPlayers.Add(vrrig);
                else if (nearbyPlayers.Contains(vrrig))
                    nearbyPlayers.Remove(vrrig);
            }

            if (nearbyPlayers.Count > 0)
            {
                foreach (VRRig nearbyPlayer in nearbyPlayers)
                {
                    NetPlayer owner = GetPlayerFromVRRig(nearbyPlayer);
                    BetaSetStatus(RoomSystem.StatusEffects.JoinedTaggedTime, new RaiseEventOptions { TargetActors = new[] { owner.ActorNumber } });
                    RPCProtection();
                }
            }
        }

        public static void VibrateOnTouch()
        {
            if (!NetworkSystem.Instance.InRoom) return;

            List<VRRig> touchedPlayers = new List<VRRig>();

            foreach (VRRig rig in VRRigExtensions.ActiveRigs)
            {
                if (!rig.IsLocal())
                {
                    if (rig.IsBeingTouched())
                    {
                        touchedPlayers.Add(rig);
                    }
                }
            }

            if (touchedPlayers.Count > 0)
            {
                foreach (VRRig rig in touchedPlayers)
                {
                    NetPlayer owner = rig.GetPlayer();
                    BetaSetStatus(RoomSystem.StatusEffects.JoinedTaggedTime, new RaiseEventOptions { TargetActors = new[] { owner.ActorNumber } });
                    RPCProtection();
                }
            }
        }

        public static void VibrateAll()
        {
            if (Time.time > vibrateDelay)
            {
                BetaSetStatus(RoomSystem.StatusEffects.JoinedTaggedTime, new RaiseEventOptions { Receivers = ReceiverGroup.Others });
                RPCProtection();
                vibrateDelay = Time.time + 0.5f;
            }
        }

        public static void GliderBlindGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }

                if (gunLocked)
                {
                    foreach (GliderHoldable glider in GetAllType<GliderHoldable>())
                    {
                        if (glider.GetView.Owner == PhotonNetwork.LocalPlayer)
                        {
                            glider.gameObject.transform.position = lockTarget.headMesh.transform.position;
                            glider.gameObject.transform.rotation = Quaternion.Euler(new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)));
                        }
                        else
                            glider.OnHover(null, null);
                    }
                }
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void GliderBlindAll()
        {
            GliderHoldable[] those = GetAllType<GliderHoldable>();
            int index = 0;
            foreach (var vrrig in VRRigExtensions.ActiveRigs.Where(vrrig => !vrrig.isLocal))
            {
                try
                {
                    GliderHoldable glider = those[index];
                    if (glider.GetView.Owner == PhotonNetwork.LocalPlayer)
                    {
                        glider.gameObject.transform.position = vrrig.headMesh.transform.position;
                        glider.gameObject.transform.rotation = Quaternion.Euler(new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)));
                    }
                    else
                        glider.OnHover(null, null);
                }
                catch { }
                index++;
            }
        }

        public static void BreakAudioGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", lockTarget.GetPlayer(), 111, false, 999999f);
                    RPCProtection();
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        gunLocked = true;
                        lockTarget = gunTarget;
                    }
                }
            }
            else
            {
                if (gunLocked)
                {
                    gunLocked = false;
                    VRRig.LocalRig.enabled = true;
                }
            }
        }

        public static void BreakAudioAll()
        {
            if (rightTrigger > 0.5f)
            {
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.Others, 111, false, 999999f);
            }
        }

        public static Coroutine RopeCoroutine;
        public static IEnumerator RopeEnableRig()
        {
            yield return new WaitForSeconds(0.3f);
            VRRig.LocalRig.enabled = true;
        }

        public static void BetaSetRopeVelocity(int RopeId, Vector3 Velocity)
        {
            Velocity = Velocity.ClampMagnitudeSafe(15f);

            if (RopeSwingManager.instance.ropes.TryGetValue(RopeId, out GorillaRopeSwing Rope))
            {
                var ClosestNode = Rope.nodes
                    .Skip(1)
                    .Select((v, i) => new
                    {
                        index = i,
                        transform = v,
                        distance = Vector3.Distance(GorillaTagger.Instance.bodyCollider.transform.position, v.transform.position)
                    })
                    .OrderBy(x => x.distance)
                    .First();

                if (ClosestNode.distance > 5f)
                {
                    if (RopeCoroutine != null)
                        CoroutineManager.instance.StopCoroutine(RopeCoroutine);

                    RopeCoroutine = CoroutineManager.instance.StartCoroutine(RopeEnableRig());

                    VRRig.LocalRig.enabled = false;
                    VRRig.LocalRig.transform.position = ClosestNode.transform.position;
                }

                if (Vector3.Distance(ServerPos, ClosestNode.transform.position) < 5f)
                    RopeSwingManager.instance.SendSetVelocity_RPC(RopeId, ClosestNode.index, Velocity, true);
                else
                    RopeDelay = 0f;

                RPCProtection();
            }
        }

        public static void BetaSetRopeVelocity(GorillaRopeSwing Rope, Vector3 Velocity) =>
            BetaSetRopeVelocity(RopeSwingManager.instance.ropes.FirstOrDefault(x => x.Value == Rope).Key, Velocity);

        private static float randomRopeDelay;
        private static GorillaRopeSwing randomRope;
        public static GorillaRopeSwing GetRandomRope()
        {
            if (Time.time > randomRopeDelay)
            {
                randomRopeDelay = Time.time + 0.5f;
                randomRope = RopeSwingManager.instance.ropes.Values.OrderBy(_ => Random.value).FirstOrDefault();
            }
            return randomRope;
        }

        private static float RopeDelay;
        public static void JoystickRopeControl()
        {
            Vector2 r = rightJoystick;
            Vector2 l = leftJoystick;

            Vector2 stick = (l.sqrMagnitude > r.sqrMagnitude) ? l : r;

            if (stick.sqrMagnitude > 0.0025f && Time.time > RopeDelay)
            {
                RopeDelay = Time.time + 0.125f;

                GorillaRopeSwing rope = GetRandomType<GorillaRopeSwing>(0.25f);
                BetaSetRopeVelocity(rope, new Vector3(stick.x * 100f, stick.y * 100f, 0f));
            }
        }

        public static void SpazRopeGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true))
                {
                    GorillaRopeSwing gunTarget = Ray.collider.GetComponentInParent<GorillaRopeSwing>();
                    if (gunTarget && Time.time > RopeDelay)
                    {
                        RopeDelay = Time.time + 0.25f;
                        BetaSetRopeVelocity(gunTarget, RandomVector3(100f));
                    }
                }
            }
        }

        public static void SpazAllRopes()
        {
            if (rightTrigger > 0.5f && Time.time > RopeDelay)
            {
                RopeDelay = Time.time + 0.125f;

                GorillaRopeSwing rope = GetRandomRope();
                BetaSetRopeVelocity(rope, RandomVector3(100f));
            }
        }

        public static void SpazGrabbedRopes()
        {
            if (Time.time > RopeDelay)
            {
                RopeDelay = Time.time + 0.125f;
                VRRig randomRig = VRRigExtensions.ActiveRigs
                    .Where(rig => rig.currentRopeSwing != null)
                    .OrderBy(_ => Random.value)
                    .FirstOrDefault();

                if (randomRig != null)
                    BetaSetRopeVelocity(randomRig.currentRopeSwing, RandomVector3(100f));
            }
        }

        public static void FlingRopeGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true))
                {
                    GorillaRopeSwing gunTarget = Ray.collider.GetComponentInParent<GorillaRopeSwing>();

                    if (gunTarget && Time.time > RopeDelay)
                    {
                        RopeDelay = Time.time + 0.125f;
                        BetaSetRopeVelocity(gunTarget, RandomVector3(100f));
                    }
                }
            }
        }

        public static void FlingAllRopesGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true) && Time.time > RopeDelay)
                {
                    RopeDelay = Time.time + 0.125f;

                    GorillaRopeSwing rope = GetRandomRope();
                    BetaSetRopeVelocity(rope, (NewPointer.transform.position - rope.transform.position).normalized * 100f);
                }
            }
        }

        public static void EffectSpam(CrittersManager.CritterEvent critterEvent)
        {
            if (rightGrab)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    CrittersPawn[] critters = CrittersManager.instance.crittersPawns.ToArray();
                    if (critters.Length > 0)
                    {
                        CrittersPawn critter = critters[0];
                        critter.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                        int actorId = critter.actorId;
                        CrittersManager.instance.TriggerEvent(critterEvent, actorId, critter.transform.position, Quaternion.LookRotation(critter.transform.up));
                    }
                }
                else
                {
                    CrittersActor.CrittersActorType type = CrittersActor.CrittersActorType.StickyTrap;
                    Vector3 velocity = Vector3.down * 20f;
                    switch (critterEvent)
                    {
                        case CrittersManager.CritterEvent.StunExplosion:
                            type = CrittersActor.CrittersActorType.StunBomb;
                            break;
                        case CrittersManager.CritterEvent.StickyDeployed:
                        case CrittersManager.CritterEvent.StickyTriggered:
                            type = CrittersActor.CrittersActorType.StickyTrap;
                            break;
                        case CrittersManager.CritterEvent.NoiseMakerTriggered:
                            type = CrittersActor.CrittersActorType.NoiseMaker;
                            break;
                    }

                    CrittersGrabber localGrabber = GetAllType<CrittersGrabber>().Where(grabber => grabber.rigPlayerId == PhotonNetwork.LocalPlayer.ActorNumber && grabber.isLeft).FirstOrDefault();
                    List<CrittersActor> critters = GetAllType<CrittersActor>().Where(critter => critter != null && critter.crittersActorType == type && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 3f).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                    if (critters.Count <= 0)
                        critters = GetAllType<CrittersActor>().Where(critter => critter != null && critter.crittersActorType == type && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                    if (critters.Count <= 0)
                        critters = GetAllType<CrittersActor>().Where(critter => critter != null && critter.crittersActorType == type).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                    CrittersActor critter = critters[Random.Range(0, critters.Count)];

                    if (Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 25f)
                    {
                        VRRig.LocalRig.enabled = false;
                        VRRig.LocalRig.transform.position = critter.transform.position - Vector3.one * 5f;

                        if (CritterCoroutine != null)
                            CoroutineManager.instance.StopCoroutine(CritterCoroutine);

                        CritterCoroutine = CoroutineManager.instance.StartCoroutine(RopeEnableRig());
                    }

                    if (Vector3.Distance(critter.transform.position, ServerPos) < 25f && Time.time > critterGrabDelay)
                    {
                        critterGrabDelay = Time.time + 0.1f;

                        critter.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                        critter.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;

                        if (critter)
                            critter.SetImpulseVelocity(velocity, Vector3.zero);

                        if (localGrabber != null)
                            CrittersManager.instance.SendRPC("RemoteCrittersActorGrabbedby",
                                CrittersManager.instance.guard.currentOwner, critter.actorId, localGrabber.actorId,
                                Quaternion.identity, Vector3.zero, false);
                        CrittersManager.instance.SendRPC("RemoteCritterActorReleased", CrittersManager.instance.guard.currentOwner, critter.actorId, false, critter.transform.rotation, critter.transform.position, velocity, Vector3.zero);
                    }
                }
            }
        }

        public static void EffectGun(CrittersManager.CritterEvent critterEvent)
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        CrittersPawn[] critters = CrittersManager.instance.crittersPawns.ToArray();
                        if (critters.Length > 0)
                        {
                            CrittersPawn critter = critters[0];
                            critter.transform.position = NewPointer.transform.position;
                            int actorId = critter.actorId;
                            CrittersManager.instance.TriggerEvent(critterEvent, actorId, critter.transform.position, Quaternion.LookRotation(critter.transform.up));
                        }
                    }
                    else
                    {
                        CrittersActor.CrittersActorType type = CrittersActor.CrittersActorType.StickyTrap;
                        Vector3 velocity = -Ray.normal * 50f;
                        switch (critterEvent)
                        {
                            case CrittersManager.CritterEvent.StunExplosion:
                                type = CrittersActor.CrittersActorType.StunBomb;
                                break;
                            case CrittersManager.CritterEvent.StickyDeployed:
                            case CrittersManager.CritterEvent.StickyTriggered:
                                type = CrittersActor.CrittersActorType.StickyTrap;
                                break;
                            case CrittersManager.CritterEvent.NoiseMakerTriggered:
                                type = CrittersActor.CrittersActorType.NoiseMaker;
                                break;
                        }

                        CrittersGrabber localGrabber = GetAllType<CrittersGrabber>().Where(grabber => grabber.rigPlayerId == PhotonNetwork.LocalPlayer.ActorNumber && grabber.isLeft).FirstOrDefault();
                        List<CrittersActor> critters = GetAllType<CrittersActor>().Where(critter => critter != null && critter.crittersActorType == type && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 3f).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                        if (critters.Count <= 0)
                            critters = GetAllType<CrittersActor>().Where(critter => critter != null && critter.crittersActorType == type && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                        if (critters.Count <= 0)
                            critters = GetAllType<CrittersActor>().Where(critter => critter != null && critter.crittersActorType == type).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                        CrittersActor critter = critters[Random.Range(0, critters.Count)];

                        if (Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 25f)
                        {
                            VRRig.LocalRig.enabled = false;
                            VRRig.LocalRig.transform.position = critter.transform.position - Vector3.one * 5f;

                            if (CritterCoroutine != null)
                                CoroutineManager.instance.StopCoroutine(CritterCoroutine);

                            CritterCoroutine = CoroutineManager.instance.StartCoroutine(RopeEnableRig());
                        }

                        if (Vector3.Distance(critter.transform.position, ServerPos) < 25f && Time.time > critterGrabDelay)
                        {
                            critterGrabDelay = Time.time + 0.05f;

                            critter.transform.position = NewPointer.transform.position + Ray.normal;
                            critter.transform.rotation = RandomQuaternion();

                            if (critter)
                                critter.SetImpulseVelocity(velocity, Vector3.zero);

                            if (localGrabber != null)
                                CrittersManager.instance.SendRPC("RemoteCrittersActorGrabbedby",
                                    CrittersManager.instance.guard.currentOwner, critter.actorId, localGrabber.actorId,
                                    Quaternion.identity, Vector3.zero, false);
                            CrittersManager.instance.SendRPC("RemoteCritterActorReleased", CrittersManager.instance.guard.currentOwner, critter.actorId, false, critter.transform.rotation, critter.transform.position, velocity, Vector3.zero);
                        }
                    }
                }
            }
        }

        public static void CritterSpam()
        {
            if (rightGrab)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    List<CrittersPawn> critters = CrittersManager.instance.crittersPawns.Where(critter => critter != null).ToList();

                    CrittersPawn targetCritter = critters[Random.Range(0, critters.Count)];
                    targetCritter.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                    targetCritter.transform.rotation = RandomQuaternion();
                }
                else
                {
                    CrittersGrabber localGrabber = GetAllType<CrittersGrabber>().Where(grabber => grabber.rigPlayerId == PhotonNetwork.LocalPlayer.ActorNumber && grabber.isLeft).FirstOrDefault();
                    List<CrittersPawn> critters = CrittersManager.instance.crittersPawns.Where(critter => critter != null && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 3f).ToList();

                    if (critters.Count <= 0)
                        critters = CrittersManager.instance.crittersPawns.Where(critter => critter != null && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f).ToList();

                    if (critters.Count <= 0)
                        critters = CrittersManager.instance.crittersPawns.Where(critter => critter != null).ToList();

                    if (critters.Count <= 0)
                        return;

                    CrittersPawn critter = critters[Random.Range(0, critters.Count)];

                    if (Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 25f)
                    {
                        VRRig.LocalRig.enabled = false;
                        VRRig.LocalRig.transform.position = critter.transform.position - Vector3.one * 5f;

                        if (CritterCoroutine != null)
                            CoroutineManager.instance.StopCoroutine(CritterCoroutine);

                        CritterCoroutine = CoroutineManager.instance.StartCoroutine(RopeEnableRig());
                    }

                    if (Vector3.Distance(critter.transform.position, ServerPos) < 25f && critter.currentState != CrittersPawn.CreatureState.Grabbed && Time.time > critterGrabDelay)
                    {
                        critterGrabDelay = Time.time + 0.05f;

                        critter.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                        critter.transform.rotation = RandomQuaternion();

                        if (localGrabber != null)
                            CrittersManager.instance.SendRPC("RemoteCrittersActorGrabbedby",
                                CrittersManager.instance.guard.currentOwner, critter.actorId, localGrabber.actorId,
                                Quaternion.identity, Vector3.zero, false);
                        CrittersManager.instance.SendRPC("RemoteCritterActorReleased", CrittersManager.instance.guard.currentOwner, critter.actorId, false, critter.transform.rotation, critter.transform.position, Vector3.zero, Vector3.zero);
                    }
                }
            }
        }

        public static void CritterMinigun()
        {
            if (rightGrab)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    List<CrittersPawn> critters = CrittersManager.instance.crittersPawns.Where(critter => critter != null).ToList();

                    CrittersPawn targetCritter = critters[Random.Range(0, critters.Count)];
                    targetCritter.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                    targetCritter.transform.rotation = RandomQuaternion();

                    if (targetCritter.usesRB)
                        targetCritter.SetImpulseVelocity(GetGunDirection(GorillaTagger.Instance.rightHandTransform) * ShootStrength, RandomVector3(100f));
                }
                else
                {
                    CrittersGrabber localGrabber = GetAllType<CrittersGrabber>().Where(grabber => grabber.rigPlayerId == PhotonNetwork.LocalPlayer.ActorNumber && grabber.isLeft).FirstOrDefault();
                    List<CrittersPawn> critters = CrittersManager.instance.crittersPawns.Where(critter => critter != null && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 3f).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                    if (critters.Count <= 0)
                        critters = CrittersManager.instance.crittersPawns.Where(critter => critter != null && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                    if (critters.Count <= 0)
                        critters = CrittersManager.instance.crittersPawns.Where(critter => critter != null).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                    CrittersPawn critter = critters[Random.Range(0, critters.Count)];

                    if (Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 25f)
                    {
                        VRRig.LocalRig.enabled = false;
                        VRRig.LocalRig.transform.position = critter.transform.position - Vector3.one * 5f;

                        if (CritterCoroutine != null)
                            CoroutineManager.instance.StopCoroutine(CritterCoroutine);

                        CritterCoroutine = CoroutineManager.instance.StartCoroutine(RopeEnableRig());
                    }

                    if (Vector3.Distance(critter.transform.position, ServerPos) < 25f && Time.time > critterGrabDelay)
                    {
                        critterGrabDelay = Time.time + 0.05f;

                        critter.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                        critter.transform.rotation = RandomQuaternion();

                        if (localGrabber != null)
                            CrittersManager.instance.SendRPC("RemoteCrittersActorGrabbedby",
                                CrittersManager.instance.guard.currentOwner, critter.actorId, localGrabber.actorId,
                                Quaternion.identity, Vector3.zero, false);
                        CrittersManager.instance.SendRPC("RemoteCritterActorReleased", CrittersManager.instance.guard.currentOwner, critter.actorId, false, critter.transform.rotation, critter.transform.position, GetGunDirection(GorillaTagger.Instance.rightHandTransform) * ShootStrength, Vector3.zero);
                    }
                }
            }
        }

        private static Coroutine CritterCoroutine;
        private static float critterGrabDelay;
        public static void CritterGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        List<CrittersPawn> critters = CrittersManager.instance.crittersPawns.Where(critter => critter != null).ToList();

                        CrittersPawn targetCritter = critters[Random.Range(0, critters.Count)];
                        targetCritter.transform.position = NewPointer.transform.position;
                        targetCritter.transform.rotation = RandomQuaternion();
                    }
                    else
                    {
                        CrittersGrabber localGrabber = GetAllType<CrittersGrabber>().Where(grabber => grabber.rigPlayerId == PhotonNetwork.LocalPlayer.ActorNumber && grabber.isLeft).FirstOrDefault();
                        List<CrittersPawn> critters = CrittersManager.instance.crittersPawns.Where(critter => critter != null && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 3f).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                        if (critters.Count <= 0)
                            critters = CrittersManager.instance.crittersPawns.Where(critter => critter != null && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                        if (critters.Count <= 0)
                            critters = CrittersManager.instance.crittersPawns.Where(critter => critter != null).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                        CrittersPawn critter = critters[Random.Range(0, critters.Count)];

                        if (Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 25f)
                        {
                            VRRig.LocalRig.enabled = false;
                            VRRig.LocalRig.transform.position = critter.transform.position - Vector3.one * 5f;

                            if (CritterCoroutine != null)
                                CoroutineManager.instance.StopCoroutine(CritterCoroutine);

                            CritterCoroutine = CoroutineManager.instance.StartCoroutine(RopeEnableRig());
                        }

                        if (Vector3.Distance(critter.transform.position, ServerPos) < 25f && Time.time > critterGrabDelay)
                        {
                            critterGrabDelay = Time.time + 0.05f;

                            critter.transform.position = NewPointer.transform.position + Vector3.up;
                            critter.transform.rotation = RandomQuaternion();

                            if (localGrabber != null)
                                CrittersManager.instance.SendRPC("RemoteCrittersActorGrabbedby",
                                    CrittersManager.instance.guard.currentOwner, critter.actorId, localGrabber.actorId,
                                    Quaternion.identity, Vector3.zero, false);
                            CrittersManager.instance.SendRPC("RemoteCritterActorReleased", CrittersManager.instance.guard.currentOwner, critter.actorId, false, critter.transform.rotation, critter.transform.position, Vector3.zero, Vector3.zero);
                        }
                    }
                }
            }
        }

        public static void ObjectSpam(CrittersActor.CrittersActorType type)
        {
            if (rightGrab)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    CrittersActor Object = CrittersManager.instance.SpawnActor(type);
                    Object.MoveActor(GorillaTagger.Instance.rightHandTransform.position, GorillaTagger.Instance.rightHandTransform.rotation);

                    if (Object.usesRB)
                        Object.SetImpulseVelocity(GetGunDirection(GorillaTagger.Instance.rightHandTransform) * ShootStrength, RandomVector3(100f));
                }
                else
                {
                    Vector3 velocity = GetGunDirection(GorillaTagger.Instance.rightHandTransform) * ShootStrength;
                    switch (type)
                    {
                        case CrittersActor.CrittersActorType.LoudNoise:
                            type = CrittersActor.CrittersActorType.NoiseMaker;
                            velocity = Vector3.down * 50f;
                            break;
                        case CrittersActor.CrittersActorType.StickyGoo:
                            type = CrittersActor.CrittersActorType.StickyTrap;
                            velocity = Vector3.down * 50f;
                            break;
                    }

                    CrittersGrabber localGrabber = GetAllType<CrittersGrabber>().Where(grabber => grabber.rigPlayerId == PhotonNetwork.LocalPlayer.ActorNumber && grabber.isLeft).FirstOrDefault();
                    List<CrittersActor> critters = GetAllType<CrittersActor>().Where(critter => critter != null && critter.crittersActorType == type && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 3f).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                    if (critters.Count <= 0)
                        critters = GetAllType<CrittersActor>().Where(critter => critter != null && critter.crittersActorType == type && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                    if (critters.Count <= 0)
                        critters = GetAllType<CrittersActor>().Where(critter => critter != null && critter.crittersActorType == type).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                    CrittersActor critter = critters[Random.Range(0, critters.Count)];

                    if (Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 25f)
                    {
                        VRRig.LocalRig.enabled = false;
                        VRRig.LocalRig.transform.position = critter.transform.position - Vector3.one * 5f;

                        if (CritterCoroutine != null)
                            CoroutineManager.instance.StopCoroutine(CritterCoroutine);

                        CritterCoroutine = CoroutineManager.instance.StartCoroutine(RopeEnableRig());
                    }

                    if (Vector3.Distance(critter.transform.position, ServerPos) < 25f && Time.time > critterGrabDelay)
                    {
                        critterGrabDelay = Time.time + 0.05f;

                        critter.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                        critter.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;

                        if (critter)
                            critter.SetImpulseVelocity(velocity, Vector3.zero);

                        if (localGrabber != null)
                            CrittersManager.instance.SendRPC("RemoteCrittersActorGrabbedby",
                                CrittersManager.instance.guard.currentOwner, critter.actorId, localGrabber.actorId,
                                Quaternion.identity, Vector3.zero, false);
                        CrittersManager.instance.SendRPC("RemoteCritterActorReleased", CrittersManager.instance.guard.currentOwner, critter.actorId, false, critter.transform.rotation, critter.transform.position, velocity, Vector3.zero);
                    }
                }
            }
        }

        public static void ObjectGun(CrittersActor.CrittersActorType type)
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        CrittersActor Object = CrittersManager.instance.SpawnActor(type);
                        Object.MoveActor(NewPointer.transform.position + Vector3.up, RandomQuaternion());
                    }
                    else
                    {
                        Vector3 velocity = Vector3.zero;
                        Vector3 position = NewPointer.transform.position + Vector3.up;

                        switch (type)
                        {
                            case CrittersActor.CrittersActorType.LoudNoise:
                                type = CrittersActor.CrittersActorType.NoiseMaker;
                                velocity = Ray.normal * -20f;
                                position = NewPointer.transform.position + Ray.normal;
                                break;
                            case CrittersActor.CrittersActorType.StickyGoo:
                                type = CrittersActor.CrittersActorType.StickyTrap;
                                velocity = Ray.normal * -20f;
                                position = NewPointer.transform.position + Ray.normal;
                                break;
                        }

                        CrittersGrabber localGrabber = GetAllType<CrittersGrabber>().Where(grabber => grabber.rigPlayerId == PhotonNetwork.LocalPlayer.ActorNumber && grabber.isLeft).FirstOrDefault();
                        List<CrittersActor> critters = GetAllType<CrittersActor>().Where(critter => critter != null && critter.crittersActorType == type && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 3f).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                        if (critters.Count <= 0)
                            critters = GetAllType<CrittersActor>().Where(critter => critter != null && critter.crittersActorType == type && Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) < 25f).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                        if (critters.Count <= 0)
                            critters = GetAllType<CrittersActor>().Where(critter => critter != null && critter.crittersActorType == type).OrderByDescending(critter => Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position)).ToList();

                        CrittersActor critter = critters[Random.Range(0, critters.Count)];

                        if (Vector3.Distance(critter.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 25f)
                        {
                            VRRig.LocalRig.enabled = false;
                            VRRig.LocalRig.transform.position = critter.transform.position - Vector3.one * 5f;

                            if (CritterCoroutine != null)
                                CoroutineManager.instance.StopCoroutine(CritterCoroutine);

                            CritterCoroutine = CoroutineManager.instance.StartCoroutine(RopeEnableRig());
                        }

                        if (Vector3.Distance(critter.transform.position, ServerPos) < 25f && Time.time > critterGrabDelay)
                        {
                            critterGrabDelay = Time.time + 0.1f;

                            critter.transform.position = position;
                            critter.transform.rotation = RandomQuaternion();

                            if (critter)
                                critter.SetImpulseVelocity(velocity, Vector3.zero);

                            if (localGrabber != null)
                                CrittersManager.instance.SendRPC("RemoteCrittersActorGrabbedby",
                                    CrittersManager.instance.guard.currentOwner, critter.actorId, localGrabber.actorId,
                                    Quaternion.identity, Vector3.zero, false);
                            CrittersManager.instance.SendRPC("RemoteCritterActorReleased", CrittersManager.instance.guard.currentOwner, critter.actorId, false, critter.transform.rotation, critter.transform.position, velocity, Vector3.zero);
                        }
                    }
                }
            }
        }
    }
}
