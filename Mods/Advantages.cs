/*
 * United Goyim College Fund  Mods/Advantages.cs
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
using GorillaGameModes;
using GorillaLocomotion;
using Photon.Pun;
using Photon.Realtime;
using Seralyth.Extensions;
using Seralyth.Managers;
using Seralyth.Menu;
using Seralyth.Patches.Menu;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Seralyth.Menu.Main;
using static Seralyth.Utilities.GameModeUtilities;
using static Seralyth.Utilities.RandomUtilities;
using static Seralyth.Utilities.RigUtilities;

namespace Seralyth.Mods
{
    public static class Advantages
    {
        public static bool instantTag = true;

        public static void TagSelf()
        {
            static void TurnOff()
            {
                Buttons.GetIndex("Tag Self").SetEnabled(false);
                ReloadMenu();
            }
            if (PhotonNetwork.IsMasterClient)
            {
                AddInfected(PhotonNetwork.LocalPlayer);
                NotificationManager.SendNotification("<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> You have been tagged.");
                TurnOff();
            }
            else
            {
                if (InfectedList().Contains(PhotonNetwork.LocalPlayer))
                {
                    NotificationManager.SendNotification("<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> You have been tagged.");
                    VRRig.LocalRig.enabled = true;
                    TurnOff();

                    if (instantTag)
                        SerializePatch.OverrideSerialization = null;
                }
                else
                {
                    VRRig rig = VRRigExtensions.ActiveRigs
                        .Where(r => !r.IsLocal() && r.IsTagged())
                        .OrderBy(r => r.Distance(VRRig.LocalRig)
                                    + r.LatestVelocity().magnitude)
                        .FirstOrDefault();

                    if (instantTag)
                    {
                        SerializePatch.OverrideSerialization = () =>
                        {
                            if (VRRig.LocalRig.IsTagged())
                                return true;

                            MassSerialize(true, new[] { VRRig.LocalRig.GetPhotonView() });

                            Vector3 positionArchive = VRRig.LocalRig.transform.position;
                            VRRig.LocalRig.transform.position = rig.rightHandTransform.transform.position;
                            SendSerialize(VRRig.LocalRig.GetPhotonView(), new RaiseEventOptions { TargetActors = new[] { PhotonNetwork.MasterClient.ActorNumber, rig.GetPlayer().ActorNumber } });

                            RPCProtection();
                            VRRig.LocalRig.transform.position = positionArchive;

                            return false;
                        };
                    }
                    else
                    {
                        if (!rig.IsTagged()) return;
                        VRRig.LocalRig.enabled = false;
                        if (rig != null) VRRig.LocalRig.transform.position = rig.rightHandTransform.position;

                        if (!Buttons.GetIndex("Obnoxious Tag").enabled) return;
                        Quaternion rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
                        VRRig.LocalRig.transform.rotation = rotation;

                        VRRig.LocalRig.head.rigTarget.transform.rotation = RandomQuaternion();
                        VRRig.LocalRig.leftHand.rigTarget.transform.position = VRRig.LocalRig.transform.position + RandomVector3();
                        VRRig.LocalRig.rightHand.rigTarget.transform.position = VRRig.LocalRig.transform.position + RandomVector3();

                        VRRig.LocalRig.leftHand.rigTarget.transform.rotation = RandomQuaternion();
                        VRRig.LocalRig.rightHand.rigTarget.transform.rotation = RandomQuaternion();
                    }
                }
            }
        }

        public static void UntagSelf()
        {
            if (NetworkSystem.Instance.IsMasterClient)
                RemoveInfected(PhotonNetwork.LocalPlayer);
            else if (!NetworkSystem.Instance.IsMasterClient)
            {
                Important.Reconnect();
                NoTagOnJoin();
            }

            GTPlayer.Instance.disableMovement = false;
        }

        public static void AntiTag()
        {
            if (NetworkSystem.Instance.InRoom)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    if (!ReportTagPatch.invinciblePlayers.Contains(NetworkSystem.Instance.LocalPlayer))
                        ReportTagPatch.invinciblePlayers.Add(NetworkSystem.Instance.LocalPlayer);
                }
                else
                {
                    if (VRRig.LocalRig.IsTagged())
                        UntagSelf();
                }
            }
            else
            {
                TagOnJoin();
                ReportTagPatch.invinciblePlayers.Clear();
            }
        }

        public static void DisableAntiTag()
        {
            if (NetworkSystem.Instance.InRoom)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    if (ReportTagPatch.invinciblePlayers.Contains(NetworkSystem.Instance.LocalPlayer))
                        ReportTagPatch.invinciblePlayers.Remove(NetworkSystem.Instance.LocalPlayer);
                }
                NoTagOnJoin();
            }
        }

        public static void UntagAll()
        {
            if (!NetworkSystem.Instance.IsMasterClient)
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
            else
            {
                foreach (Player v in PhotonNetwork.PlayerList)
                    RemoveInfected(v);
            }
        }

        public static float spamTagDelay;
        public static void SpamTagSelf()
        {
            if (!NetworkSystem.Instance.IsMasterClient)
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
            else
            {
                if (!(Time.time > spamTagDelay)) return;
                spamTagDelay = Time.time + 0.1f;
                if (InfectedList().Contains(PhotonNetwork.LocalPlayer))
                    RemoveInfected(PhotonNetwork.LocalPlayer);
                else
                    AddInfected(PhotonNetwork.LocalPlayer);
            }
        }

        public static void SpamTagGun()
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
                        if (Time.time > spamTagDelay)
                        {
                            spamTagDelay = Time.time + 0.1f;
                            if (InfectedList().Contains(lockTarget.GetPlayer()))
                                RemoveInfected(lockTarget.GetPlayer());
                            else
                                AddInfected(lockTarget.GetPlayer());
                        }
                    }
                }

                if (!GetGunInput(true)) return;
                VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                if (!gunTarget || gunTarget.IsLocal()) return;
                if (!PhotonNetwork.IsMasterClient) return;
                gunLocked = true;
                lockTarget = gunTarget;
            }
            else
            {
                if (gunLocked)
                    gunLocked = false;
            }
        }

        public static void SpamTagAll()
        {
            if (!NetworkSystem.Instance.IsMasterClient)
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
            else
            {
                if (!(Time.time > spamTagDelay)) return;
                spamTagDelay = Time.time + 0.1f;
                foreach (Player v in PhotonNetwork.PlayerList)
                {
                    if (InfectedList().Contains(v))
                        AddInfected(v);
                    else
                        RemoveInfected(v);
                }
            }
        }

        public static void TagLagGun()
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
                        if (PhotonNetwork.IsMasterClient)
                        {
                            if (lockTarget != null)
                                ReportTagPatch.blacklistedPlayers.Remove(lockTarget.GetPlayer());

                            gunLocked = true;
                            lockTarget = gunTarget;

                            ReportTagPatch.blacklistedPlayers.Add(GetPlayerFromVRRig(gunTarget));

                        }
                        else
                            NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                    }
                }
            }
            else
            {
                if (gunLocked)
                {
                    gunLocked = false;
                    ReportTagPatch.blacklistedPlayers.Remove(lockTarget.GetPlayer());
                }
            }
        }

        public static void GiveTagLagGun()
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
                        if (PhotonNetwork.IsMasterClient)
                        {
                            if (lockTarget != null)
                                ReportTagPatch.invinciblePlayers.Remove(lockTarget.GetPlayer());

                            gunLocked = true;
                            lockTarget = gunTarget;

                            ReportTagPatch.invinciblePlayers.Add(GetPlayerFromVRRig(gunTarget));

                        }
                        else
                            NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                    }
                }
            }
            else
            {
                if (gunLocked)
                {
                    gunLocked = false;
                    ReportTagPatch.invinciblePlayers.Remove(lockTarget.GetPlayer());
                }
            }
        }

        public static void SetTagCooldown(float value)
        {
            if (!NetworkSystem.Instance.InRoom)
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not in a room.");
                return;
            }
            if (!NetworkSystem.Instance.IsMasterClient)
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                return;
            }
            GorillaTagManager tagman = (GorillaTagManager)GorillaGameManager.instance;
            tagman.tagCoolDown = value;
        }

        public static float tagAuraDistance = 1.666f;
        public static int tagAuraIndex = 1;
        public static readonly string[] TagAuraRangeNames = { "Short", "Normal", "Far", "Maximum" };
        public static readonly float[] TagAuraRangeDistances = { 0.777f, 1.666f, 3f, 5.5f };
        public static void ApplyTagAuraRange(int index) { tagAuraIndex = index; tagAuraDistance = TagAuraRangeDistances[index]; }

        public static int tagRangeIndex;
        private static float tagReachDistance = 0.3f;
        public static readonly string[] TagReachDistanceNames = { "Unnoticable", "Normal", "Far", "Maximum" };
        public static readonly float[] TagReachDistances = { 0.3f, 0.5f, 1f, 3f };
        public static void ApplyTagReachDistance(int index) { tagRangeIndex = index; tagReachDistance = TagReachDistances[index]; }

        public static void TagAura()
        {
            Color color = Color.red;
            foreach (var vrrig in VRRigExtensions.ActiveRigs.Where(vrrig => VRRig.LocalRig.IsTagged() && !vrrig.IsTagged() && !GTPlayer.Instance.disableMovement && vrrig.Distance(VRRig.LocalRig) < tagAuraDistance))
            {
                color = Color.green;
                ReportTag(vrrig);
            }
            if (Buttons.GetIndex("Visualize Tag Aura").enabled)
                Visuals.Visualize(PrimitiveType.Cylinder, VRRig.LocalRig.bodyTransform.position, Quaternion.identity, new Vector3(tagAuraDistance, 0.01f, tagAuraDistance), Buttons.GetIndex("Prettier Visualize").enabled ? color : backgroundColor.GetCurrentColor(), -20170121181, 0.1f);
        }

        public static void GripTagAura()
        {
            if (rightGrab)
                TagAura();
        }

        public static void TagAuraPlayer(VRRig giving)
        {
            foreach (var vrrig in from vrrig in VRRigExtensions.ActiveRigs let distance = Vector3.Distance(vrrig.headMesh.transform.position, giving.transform.position) where giving.IsTagged() && !vrrig.IsTagged() && !GTPlayer.Instance.disableMovement && distance < tagAuraDistance && !VRRig.LocalRig.IsLocal() && VRRig.LocalRig.IsTagged() select vrrig)
                TagPlayer(GetPlayerFromVRRig(vrrig));
        }

        public static void TagAuraGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                    TagAuraPlayer(lockTarget);

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

        public static void TagAuraAll()
        {
            foreach (VRRig vrrig in VRRigExtensions.ActiveRigs)
                TagAuraPlayer(vrrig);
        }

        public static void TagReach()
        {
            if (!VRRig.LocalRig.IsTagged()) return;
            GorillaTagger.Instance.maxTagDistance = float.MaxValue;

            GorillaTagger.Instance.tagRadiusOverride = tagReachDistance;
            GorillaTagger.Instance.tagRadiusOverrideFrame = Time.frameCount + 16;

            if (Buttons.GetIndex("Visualize Tag Reach").enabled)
            {
                Visuals.Visualize(PrimitiveType.Sphere, GorillaTagger.Instance.leftHandTransform.position, Quaternion.identity, new Vector3(tagReachDistance, 1, tagReachDistance), backgroundColor.GetCurrentColor(), -149286, 0.1f);
                Visuals.Visualize(PrimitiveType.Sphere, GorillaTagger.Instance.rightHandTransform.position, Quaternion.identity, new Vector3(tagReachDistance, 1, tagReachDistance), backgroundColor.GetCurrentColor(), -149285, 0.1f);
            }
        }

        public static bool ValidateTag(VRRig Rig) =>
            Vector3.Distance(ServerSyncPos, Rig.transform.position) < 6f;

        public static void TagGun()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            if (instantTag)
            {
                InstantTagGun();
                return;
            }

            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (gunLocked && lockTarget != null)
                {
                    if (!lockTarget.IsTagged())
                    {
                        VRRig.LocalRig.enabled = false;

                        if (!Buttons.GetIndex("Obnoxious Tag").enabled)
                            VRRig.LocalRig.transform.position = lockTarget.transform.position - new Vector3(0f, 3f, 0f);
                        else
                        {
                            Vector3 position = lockTarget.transform.position + RandomVector3();

                            VRRig.LocalRig.transform.position = position;

                            VRRig.LocalRig.head.rigTarget.transform.rotation = RandomQuaternion();
                            VRRig.LocalRig.leftHand.rigTarget.transform.position = lockTarget.transform.position + RandomVector3();
                            VRRig.LocalRig.rightHand.rigTarget.transform.position = lockTarget.transform.position + RandomVector3();

                            VRRig.LocalRig.leftHand.rigTarget.transform.rotation = RandomQuaternion();
                            VRRig.LocalRig.rightHand.rigTarget.transform.rotation = RandomQuaternion();

                            VRRig.LocalRig.leftIndex.calcT = 0f;
                            VRRig.LocalRig.leftMiddle.calcT = 0f;
                            VRRig.LocalRig.leftThumb.calcT = 0f;

                            VRRig.LocalRig.leftIndex.LerpFinger(1f, false);
                            VRRig.LocalRig.leftMiddle.LerpFinger(1f, false);
                            VRRig.LocalRig.leftThumb.LerpFinger(1f, false);

                            VRRig.LocalRig.rightIndex.calcT = 0f;
                            VRRig.LocalRig.rightMiddle.calcT = 0f;
                            VRRig.LocalRig.rightThumb.calcT = 0f;

                            VRRig.LocalRig.rightIndex.LerpFinger(1f, false);
                            VRRig.LocalRig.rightMiddle.LerpFinger(1f, false);
                            VRRig.LocalRig.rightThumb.LerpFinger(1f, false);
                        }

                        if (ValidateTag(lockTarget))
                            ReportTag(lockTarget);
                    }
                    else
                    {
                        gunLocked = false;
                        VRRig.LocalRig.enabled = true;
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
                    {
                        if (PhotonNetwork.IsMasterClient)
                            AddInfected(GetPlayerFromVRRig(gunTarget));
                        else
                        {
                            if (!VRRig.LocalRig.IsTagged()) return;
                            gunLocked = true;
                            lockTarget = gunTarget;
                        }
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

        private static float reportTagDelay;
        public static void ReportTag(VRRig rig)
        {
            if (Time.time > reportTagDelay)
            {
                reportTagDelay = Time.time + 0.1f;
                GameMode.ReportTag(rig.GetPlayer());
            }
        }

        public static void TagPlayer(NetPlayer player)
        {
            if (!NetworkSystem.Instance.InRoom) return;
            static void TurnOff()
            {
                Buttons.GetIndex("Tag Player").SetEnabled(false);
                ReloadMenu();
            }
            if (PhotonNetwork.IsMasterClient)
            {
                AddInfected(player);
                return;
            }

            if (!VRRig.LocalRig.IsTagged())
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must be tagged.");
                TurnOff();
                return;
            }

            if (instantTag)
            {
                InstantTagPlayer(player);
                TurnOff();
                return;
            }

            VRRig targetRig = GetVRRigFromPlayer(player);
            if (!targetRig.IsTagged())
            {
                VRRig.LocalRig.enabled = false;

                if (!Buttons.GetIndex("Obnoxious Tag").enabled)
                    VRRig.LocalRig.transform.position = targetRig.transform.position - new Vector3(0f, 3f, 0f);
                else
                {
                    Vector3 position = targetRig.transform.position + RandomVector3();

                    VRRig.LocalRig.transform.position = position;

                    VRRig.LocalRig.head.rigTarget.transform.rotation = RandomQuaternion();
                    VRRig.LocalRig.leftHand.rigTarget.transform.position = lockTarget.transform.position + RandomVector3();
                    VRRig.LocalRig.rightHand.rigTarget.transform.position = lockTarget.transform.position + RandomVector3();

                    VRRig.LocalRig.leftHand.rigTarget.transform.rotation = RandomQuaternion();
                    VRRig.LocalRig.rightHand.rigTarget.transform.rotation = RandomQuaternion();

                    VRRig.LocalRig.leftIndex.calcT = 0f;
                    VRRig.LocalRig.leftMiddle.calcT = 0f;
                    VRRig.LocalRig.leftThumb.calcT = 0f;

                    VRRig.LocalRig.leftIndex.LerpFinger(1f, false);
                    VRRig.LocalRig.leftMiddle.LerpFinger(1f, false);
                    VRRig.LocalRig.leftThumb.LerpFinger(1f, false);

                    VRRig.LocalRig.rightIndex.calcT = 0f;
                    VRRig.LocalRig.rightMiddle.calcT = 0f;
                    VRRig.LocalRig.rightThumb.calcT = 0f;

                    VRRig.LocalRig.rightIndex.LerpFinger(1f, false);
                    VRRig.LocalRig.rightMiddle.LerpFinger(1f, false);
                    VRRig.LocalRig.rightThumb.LerpFinger(1f, false);
                }

                if (ValidateTag(targetRig))
                    ReportTag(targetRig);
            }
            else
                TurnOff();
        }

        public static void UntagGun()
        {

            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal() && gunTarget.IsTagged())
                    {
                        if (PhotonNetwork.IsMasterClient)
                            RemoveInfected(GetPlayerFromVRRig(gunTarget));
                        else
                            NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                    }
                }
            }
        }

        public static void FlickTagGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                GameObject NewPointer = GunData.NewPointer;

                if (GetGunInput(true))
                {
                    GTPlayer.Instance.GetControllerTransform(false).position = NewPointer.transform.position;

                    if (Vector3.Distance(GTPlayer.Instance.GetControllerTransform(false).position, GorillaTagger.Instance.bodyCollider.transform.position) > 4f)
                        GTPlayer.Instance.GetControllerTransform(false).position = GorillaTagger.Instance.bodyCollider.transform.position + (GTPlayer.Instance.GetControllerTransform(false).position - GorillaTagger.Instance.bodyCollider.transform.position) * 4f;
                }
            }
        }

        public static void TagAll()
        {
            if (!NetworkSystem.Instance.InRoom) return;

            static void TurnOff()
            {
                Buttons.GetIndex("Tag All").SetEnabled(false);
                ReloadMenu();
            }

            if (GorillaGameManager.instance.GameType() == GameModeType.HuntDown)
            {
                HuntTagAll();
                return;
            }

            if (NetworkSystem.Instance.IsMasterClient)
            {
                foreach (Player v in PhotonNetwork.PlayerList)
                    AddInfected(v);

                TurnOff();
                NotificationManager.SendNotification("<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> Everyone is tagged!");
            }
            else
            {
                if (instantTag)
                {
                    InstantTagAll();
                    NotificationManager.SendNotification("<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> Everyone is tagged!");
                    TurnOff();
                    return;
                }

                if (!VRRig.LocalRig.IsTagged())
                {
                    NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must be tagged.");
                    TurnOff();
                }
                else
                {
                    bool isInfectedPlayers = VRRigExtensions.ActiveRigs.Any(vrrig => !vrrig.IsTagged());
                    if (isInfectedPlayers)
                    {
                        foreach (var vrrig in VRRigExtensions.ActiveRigs.Where(vrrig => !vrrig.IsTagged()))
                        {
                            VRRig.LocalRig.enabled = false;

                            if (!Buttons.GetIndex("Obnoxious Tag").enabled)
                                VRRig.LocalRig.transform.position = vrrig.transform.position - new Vector3(0f, 3f, 0f);
                            else
                            {
                                Vector3 position = vrrig.transform.position + RandomVector3();

                                VRRig.LocalRig.transform.position = position;
                                VRRig.LocalRig.transform.rotation = RandomQuaternion();

                                VRRig.LocalRig.head.rigTarget.transform.rotation = RandomQuaternion();
                                VRRig.LocalRig.leftHand.rigTarget.transform.position = vrrig.transform.position + RandomVector3();
                                VRRig.LocalRig.rightHand.rigTarget.transform.position = vrrig.transform.position + RandomVector3();

                                VRRig.LocalRig.leftHand.rigTarget.transform.rotation = RandomQuaternion();
                                VRRig.LocalRig.rightHand.rigTarget.transform.rotation = RandomQuaternion();

                                VRRig.LocalRig.leftIndex.calcT = 0f;
                                VRRig.LocalRig.leftMiddle.calcT = 0f;
                                VRRig.LocalRig.leftThumb.calcT = 0f;

                                VRRig.LocalRig.leftIndex.LerpFinger(1f, false);
                                VRRig.LocalRig.leftMiddle.LerpFinger(1f, false);
                                VRRig.LocalRig.leftThumb.LerpFinger(1f, false);

                                VRRig.LocalRig.rightIndex.calcT = 0f;
                                VRRig.LocalRig.rightMiddle.calcT = 0f;
                                VRRig.LocalRig.rightThumb.calcT = 0f;

                                VRRig.LocalRig.rightIndex.LerpFinger(1f, false);
                                VRRig.LocalRig.rightMiddle.LerpFinger(1f, false);
                                VRRig.LocalRig.rightThumb.LerpFinger(1f, false);
                            }

                            if (ValidateTag(vrrig))
                                ReportTag(vrrig);
                        }
                    }
                    else
                    {
                        NotificationManager.SendNotification("<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> Everyone is tagged!");
                        VRRig.LocalRig.enabled = true;
                        TurnOff();
                    }
                }
            }
        }

        public static void InstantTagPlayer(NetPlayer Target)
        {
            if (!VRRig.LocalRig.IsTagged() || Target.VRRig().IsTagged())
                return;

            Vector3 archiveRigPosition = VRRig.LocalRig.transform.position;
            VRRig.LocalRig.transform.position = GetVRRigFromPlayer(Target).transform.position;

            SendSerialize(VRRig.LocalRig.GetPhotonView(), new RaiseEventOptions { TargetActors = new[] { PhotonNetwork.MasterClient.ActorNumber } });
            GameMode.ReportTag(Target);

            VRRig.LocalRig.transform.position = archiveRigPosition;

            RPCProtection();
        }

        private static float tagGunDelay;
        public static void InstantTagGun()
        {
            if (GetGunInput(false))
            {
                var GunData = RenderGun();
                RaycastHit Ray = GunData.Ray;

                if (GetGunInput(true) && Time.time > tagGunDelay)
                {
                    try
                    {
                        VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                        if (gunTarget && !gunTarget.IsLocal())
                        {
                            tagGunDelay = Time.time + 0.2f;
                            InstantTagPlayer(NetPlayerToPlayer(GetPlayerFromVRRig(gunTarget)));
                        }
                    }
                    catch { }
                }
            }
        }

        public static void InstantTagAll()
        {
            if (!VRRig.LocalRig.IsTagged())
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You must be tagged.");
                return;
            }

            Vector3 archiveRigPosition = VRRig.LocalRig.transform.position;

            foreach (var vrrig in VRRigExtensions.ActiveRigs.Where(vrrig => !vrrig.IsTagged()))
            {
                VRRig.LocalRig.transform.position = vrrig.transform.position;
                SendSerialize(VRRig.LocalRig.GetPhotonView(), new RaiseEventOptions { TargetActors = new[] { PhotonNetwork.MasterClient.ActorNumber } });
                GameMode.ReportTag(GetPlayerFromVRRig(vrrig));
            }

            VRRig.LocalRig.transform.position = archiveRigPosition;

            SendSerialize(VRRig.LocalRig.GetPhotonView(), new RaiseEventOptions { TargetActors = new[] { PhotonNetwork.MasterClient.ActorNumber } });
            RPCProtection();
        }

        public static void HuntTagAll()
        {
            GorillaHuntManager huntComputer = (GorillaHuntManager)GorillaGameManager.instance;
            NetPlayer target = huntComputer.GetTargetOf(PhotonNetwork.LocalPlayer);
            if (!GTPlayer.Instance.disableMovement)
            {
                VRRig vrrig = GetVRRigFromPlayer(target);
                VRRig.LocalRig.enabled = false;

                if (!Buttons.GetIndex("Obnoxious Tag").enabled)
                    VRRig.LocalRig.transform.position = vrrig.transform.position - new Vector3(0f, 3f, 0f);
                else
                {
                    Vector3 position = vrrig.transform.position + RandomVector3();

                    VRRig.LocalRig.transform.position = position;

                    VRRig.LocalRig.head.rigTarget.transform.rotation = RandomQuaternion();
                    VRRig.LocalRig.leftHand.rigTarget.transform.position = vrrig.transform.position + RandomVector3();
                    VRRig.LocalRig.rightHand.rigTarget.transform.position = vrrig.transform.position + RandomVector3();

                    VRRig.LocalRig.leftHand.rigTarget.transform.rotation = RandomQuaternion();
                    VRRig.LocalRig.rightHand.rigTarget.transform.rotation = RandomQuaternion();

                    VRRig.LocalRig.leftIndex.calcT = 0f;
                    VRRig.LocalRig.leftMiddle.calcT = 0f;
                    VRRig.LocalRig.leftThumb.calcT = 0f;

                    VRRig.LocalRig.leftIndex.LerpFinger(1f, false);
                    VRRig.LocalRig.leftMiddle.LerpFinger(1f, false);
                    VRRig.LocalRig.leftThumb.LerpFinger(1f, false);

                    VRRig.LocalRig.rightIndex.calcT = 0f;
                    VRRig.LocalRig.rightMiddle.calcT = 0f;
                    VRRig.LocalRig.rightThumb.calcT = 0f;

                    VRRig.LocalRig.rightIndex.LerpFinger(1f, false);
                    VRRig.LocalRig.rightMiddle.LerpFinger(1f, false);
                    VRRig.LocalRig.rightThumb.LerpFinger(1f, false);
                }

                if (ValidateTag(vrrig))
                    ReportTag(vrrig);
            }
            else
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> Everyone is tagged!");
                VRRig.LocalRig.enabled = true;
                Buttons.GetIndex("Tag All").enabled = false;
                ReloadMenu();
            }
        }

        public static void TagBot()
        {
            if (NetworkSystem.Instance.InRoom)
            {
                if (!VRRig.LocalRig.IsTagged())
                {
                    if (InfectedList().Count > 0)
                        TagSelf();
                }
                else
                {
                    if (InfectedList().Count != PhotonNetwork.PlayerList.Length)
                        TagAll();
                }
            }
            else
                VRRig.LocalRig.enabled = true;
        }

        public static void NoTagOnJoin()
        {
            PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(PlayerConfig.Player_HasDoneTutorial, out object obj);
            if (obj is bool b && b)
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
                {
                    { PlayerConfig.Player_HasDoneTutorial, false }
                });
            }
        }

        public static void TagOnJoin() =>
            NetworkSystem.Instance.SetMyTutorialComplete();

        public static void ReportAntiTag()
        {
            SerializePatch.OverrideSerialization = () =>
            {
                if (VRRig.LocalRig.IsTagged())
                    return true;

                MassSerialize(true, new[] { VRRig.LocalRig.GetPhotonView() });

                Vector3 positionArchive = VRRig.LocalRig.transform.position;
                SendSerialize(VRRig.LocalRig.GetPhotonView(), new RaiseEventOptions { TargetActors = PhotonNetwork.PlayerList.Where(plr => plr.ActorNumber != PhotonNetwork.MasterClient.ActorNumber).Select(plr => plr.ActorNumber).ToArray() });

                VRRig.LocalRig.transform.position = new Vector3(99999f, 99999f, 99999f);
                SendSerialize(VRRig.LocalRig.GetPhotonView(), new RaiseEventOptions { TargetActors = new[] { PhotonNetwork.MasterClient.ActorNumber } });

                RPCProtection();
                VRRig.LocalRig.transform.position = positionArchive;

                return false;
            };
        }

        public static void PaintbrawlStartGame()
        {
            if (!NetworkSystem.Instance.IsMasterClient)
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
            else
            {
                GorillaPaintbrawlManager brawlManager = (GorillaPaintbrawlManager)GorillaGameManager.instance;
                brawlManager.StartBattle();
            }
        }

        public static void PaintbrawlEndGame()
        {
            if (!NetworkSystem.Instance.IsMasterClient)
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
            else
            {
                GorillaPaintbrawlManager brawlManager = (GorillaPaintbrawlManager)GorillaGameManager.instance;
                brawlManager.BattleEnd();
            }
        }

        public static void PaintbrawlRestartGame()
        {
            if (!NetworkSystem.Instance.IsMasterClient)
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
            else
            {
                GorillaPaintbrawlManager brawlManager = (GorillaPaintbrawlManager)GorillaGameManager.instance;
                brawlManager.BattleEnd();
                brawlManager.StartBattle();
            }
        }

        public static float paintbrawlSpamDelay;
        public static void PaintbrawlBalloonSpamSelf()
        {
            if (Time.time < paintbrawlSpamDelay)
                return;

            paintbrawlSpamDelay = Time.time + 0.1f;

            if (!NetworkSystem.Instance.IsMasterClient)
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
            else
            {
                GorillaPaintbrawlManager brawlManager = (GorillaPaintbrawlManager)GorillaGameManager.instance;
                brawlManager.playerLives[PhotonNetwork.LocalPlayer.ActorNumber] = Random.Range(0, 4);
            }
        }

        public static void PaintbrawlBalloonSpamGun()
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
                        if (Time.time > paintbrawlSpamDelay)
                        {
                            paintbrawlSpamDelay = Time.time + 0.1f;

                            if (!NetworkSystem.Instance.IsMasterClient)
                                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                            else
                            {
                                GorillaPaintbrawlManager brawlManager = (GorillaPaintbrawlManager)GorillaGameManager.instance;
                                brawlManager.playerLives[PhotonNetwork.LocalPlayer.ActorNumber] = Random.Range(0, 4);
                            }
                        }
                    }
                }
                if (GetGunInput(true))
                {
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.IsLocal())
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

        public static void PaintbrawlBalloonSpam()
        {
            if (Time.time < paintbrawlSpamDelay)
                return;

            paintbrawlSpamDelay = Time.time + 0.1f;

            if (!NetworkSystem.Instance.IsMasterClient)
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
            else
            {
                GorillaPaintbrawlManager brawlManager = (GorillaPaintbrawlManager)GorillaGameManager.instance;
                foreach (Player player in PhotonNetwork.PlayerList)
                    brawlManager.playerLives[player.ActorNumber] = Random.Range(0, 4);
            }
        }

        public static void PaintbrawlKillGun()
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
                        NetPlayer owner = GetPlayerFromVRRig(gunTarget);
                        if (!NetworkSystem.Instance.IsMasterClient)
                            NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                        else
                        {
                            GorillaPaintbrawlManager brawlManager = (GorillaPaintbrawlManager)GorillaGameManager.instance;
                            brawlManager.playerLives[owner.ActorNumber] = 0;
                        }
                    }
                }
            }
        }

        public static int paintbrawlKillIndex;
        public static readonly Dictionary<int, float> paintbrawlKillDelays = new Dictionary<int, float>();
        public static void PaintbrawlKillPlayer(NetPlayer Target)
        {
            if (!NetworkSystem.Instance.IsMasterClient)
            {
                if (paintbrawlKillDelays.TryGetValue(Target.ActorNumber, out float lastTime))
                {
                    if (Time.time > lastTime)
                        return;
                }

                paintbrawlKillDelays[Target.ActorNumber] = Time.time + 3.1f;

                VRRig rig = GetVRRigFromPlayer(Target);
                GameMode.ActiveNetworkHandler.SendRPC("RPC_ReportSlingshotHit", false, NetPlayerToPlayer(Target), rig.transform.position, paintbrawlKillIndex);
                RPCProtection();

                paintbrawlKillIndex++;
            }
            else
            {
                GorillaPaintbrawlManager brawlManager = (GorillaPaintbrawlManager)GorillaGameManager.instance;
                brawlManager.playerLives[Target.ActorNumber] = 0;
            }
        }

        public static void PaintbrawlKillSelf()
        {
            if (!NetworkSystem.Instance.IsMasterClient)
                PaintbrawlKillPlayer(NetworkSystem.Instance.LocalPlayer);
            else
            {
                GorillaPaintbrawlManager brawlManager = (GorillaPaintbrawlManager)GorillaGameManager.instance;
                brawlManager.playerLives[PhotonNetwork.LocalPlayer.ActorNumber] = 0;
            }
        }

        public static void PaintbrawlKillAll()
        {
            if (!NetworkSystem.Instance.IsMasterClient)
                PaintbrawlKillPlayer(GetRandomPlayer(false));
            else
            {
                GorillaPaintbrawlManager brawlManager = (GorillaPaintbrawlManager)GorillaGameManager.instance;
                foreach (Player player in PhotonNetwork.PlayerList)
                    brawlManager.playerLives[player.ActorNumber] = 0;
            }
        }

        public static void PaintbrawlReviveGun()
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
                        NetPlayer owner = GetPlayerFromVRRig(gunTarget);
                        if (!NetworkSystem.Instance.IsMasterClient)
                            NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
                        else
                        {
                            GorillaPaintbrawlManager brawlManager = (GorillaPaintbrawlManager)GorillaGameManager.instance;
                            brawlManager.playerLives[owner.ActorNumber] = 4;
                        }
                    }
                }
            }
        }

        public static void PaintbrawlReviveSelf()
        {
            if (!NetworkSystem.Instance.IsMasterClient)
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
            else
            {
                GorillaPaintbrawlManager brawlManager = (GorillaPaintbrawlManager)GorillaGameManager.instance;
                brawlManager.playerLives[PhotonNetwork.LocalPlayer.ActorNumber] = 4;
            }
        }

        public static void PaintbrawlReviveAll()
        {
            if (!NetworkSystem.Instance.IsMasterClient)
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
            else
            {
                GorillaPaintbrawlManager brawlManager = (GorillaPaintbrawlManager)GorillaGameManager.instance;
                foreach (Player player in PhotonNetwork.PlayerList)
                    brawlManager.playerLives[player.ActorNumber] = 4;
            }
        }

        // Credits to Malachi for the idea
        public static void PaintbrawlNoDelay()
        {
            if (!NetworkSystem.Instance.IsMasterClient)
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
            else
            {
                GorillaPaintbrawlManager brawlManager = (GorillaPaintbrawlManager)GorillaGameManager.instance;
                brawlManager.hitCooldown = 0f;
                brawlManager.tagCoolDown = 0f;
                brawlManager.stunGracePeriod = 0f;
            }
        }

        public static void DisablePaintbrawlNoDelay()
        {
            GorillaPaintbrawlManager brawlManager = (GorillaPaintbrawlManager)GorillaGameManager.instance;
            brawlManager.hitCooldown = 3f;
            brawlManager.tagCoolDown = 5f;
            brawlManager.stunGracePeriod = 2f;
        }

        public static void PaintbrawlGodMode()
        {
            if (!NetworkSystem.Instance.IsMasterClient)
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
            else
            {
                GorillaPaintbrawlManager brawlManager = (GorillaPaintbrawlManager)GorillaGameManager.instance;
                brawlManager.playerLives[PhotonNetwork.LocalPlayer.ActorNumber] = 4;
                GTPlayer.Instance.disableMovement = false;
            }
        }
    }
}
