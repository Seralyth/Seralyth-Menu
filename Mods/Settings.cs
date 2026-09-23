/*
 * United Goyim College Fund  Mods/Settings.cs
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

using GorillaExtensions;
using GorillaLocomotion;
using Photon.Pun;
using Photon.Realtime;
using Seralyth.Classes.Menu;
using Seralyth.Extensions;
using Seralyth.Managers;
using Seralyth.Menu;
using Seralyth.Patches.Menu;
using Seralyth.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Windows.Speech;
using UnityEngine.XR;
using static Seralyth.Menu.Main;
using static Seralyth.Utilities.AssetUtilities;
using static Seralyth.Utilities.RigUtilities;
using Console = Seralyth.Classes.Menu.Console;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Seralyth.Mods
{
    public static class Settings
    {
        public static void Search() // This took me like 4 hours
        {
            isSearching = !isSearching;

            pageNumber = 0;
            keyboardInput = "";

            if (isSearching)
                SpawnKeyboard();
            else
                DestroyKeyboard();
        }

        public static void SpawnKeyboard()
        {
            isKeyboardPc = isOnPC || toggleButtonActive && keyboardWithToggleButton;
            inTextInput = true;
            keyboardInput = "";

            shift = false;
            lockShift = false;

            if (isKeyboardPc)
                lastPressedKeys.Add(Key.Q);

            if (!isKeyboardPc)
            {
                if (VRKeyboard == null)
                {
                    VRKeyboard = LoadObject<GameObject>("VRKeyboard");
                    VRKeyboard.transform.position = GorillaTagger.Instance.bodyCollider.transform.position;
                    VRKeyboard.transform.rotation = GorillaTagger.Instance.bodyCollider.transform.rotation;

                    menuSpawnPosition = VRKeyboard.transform.Find("MenuSpawnPosition").gameObject;
                    VRKeyboard.transform.Find("Canvas").AddComponent<ColorChanger>().colors = textColors[1];

                    VRKeyboard.transform.localScale *= scaleWithPlayer ? GTPlayer.Instance.scale * menuScale : menuScale;
                    menuSpawnPosition.transform.localScale *= scaleWithPlayer ? GTPlayer.Instance.scale * menuScale : menuScale;

                    ColorChanger backgroundColorChanger = VRKeyboard.transform.Find("Background").gameObject.AddComponent<ColorChanger>();
                    backgroundColorChanger.colors = menuBackgroundColor;

                    foreach (GameObject key in VRKeyboard.transform.Find("Seperate").Children()
                        .Select(t => t.gameObject)
                        .Concat(new[] { VRKeyboard.transform.Find("Keys/default").gameObject }))
                    {
                        ColorChanger keyColorChanger = key.AddComponent<ColorChanger>();
                        keyColorChanger.colors = buttonColors[0];
                    }

                    if (shouldOutline)
                        OutlineObject(VRKeyboard.transform.Find("Background").gameObject, true);

                    var keys = new[] { "Numbers", "Letters", "Special", "Seperate" }
                        .Select(name => VRKeyboard.transform.Find(name))
                        .Where(t => t != null)
                        .SelectMany(t => t.Children())
                        .Select(t => t.gameObject);

                    foreach (GameObject v in keys)
                    {
                        v.AddComponent<KeyboardKey>().key = v.name;
                        v.layer = 2;

                        if (shouldOutline)
                            OutlineObject(v, true);
                    }
                }
            }

            if (lKeyReference == null)
            {
                lKeyReference = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                lKeyReference.transform.parent = GorillaTagger.Instance.leftHandTransform;
                lKeyReference.GetComponent<Renderer>().material.color = backgroundColor.GetColor(0);
                lKeyReference.transform.localPosition = pointerOffset;
                lKeyReference.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                lKeyCollider = lKeyReference.GetComponent<SphereCollider>();

                ColorChanger colorChanger = lKeyReference.AddComponent<ColorChanger>();
                colorChanger.colors = backgroundColor;
            }

            if (rKeyReference == null)
            {
                rKeyReference = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rKeyReference.transform.parent = GorillaTagger.Instance.rightHandTransform;
                rKeyReference.GetComponent<Renderer>().material.color = backgroundColor.GetColor(0);
                rKeyReference.transform.localPosition = pointerOffset;
                rKeyReference.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                rKeyCollider = rKeyReference.GetComponent<SphereCollider>();

                ColorChanger colorChanger = rKeyReference.AddComponent<ColorChanger>();
                colorChanger.colors = backgroundColor;
            }
        }

        public static void DestroyKeyboard()
        {
            inTextInput = false;
            isKeyboardPc = false;

            if (lKeyReference != null)
            {
                Object.Destroy(lKeyReference);
                lKeyReference = null;
            }

            if (rKeyReference != null)
            {
                Object.Destroy(rKeyReference);
                rKeyReference = null;
            }

            if (VRKeyboard != null)
            {
                Object.Destroy(VRKeyboard);
                VRKeyboard = null;
            }

            if (TPC != null && TPC.transform.parent.gameObject.name.Contains("CameraTablet") && isOnPC)
            {
                isOnPC = false;
                TPC.transform.position = TPC.transform.parent.position;
                TPC.transform.rotation = TPC.transform.parent.rotation;
            }
        }

        public static void GlobalReturn()
        {
            NotificationManager.ClearAllNotifications();
            Toggle(Buttons.buttons[Buttons.CurrentCategoryIndex][Buttons.GetCategory("Main")].buttonText, true);
            SoundManager.Play("Return");

            if (prompts.Count > 0)
                StopCurrentPrompt();
        }

        public static void StopCurrentPrompt() =>
            prompts.RemoveAt(0);

        public static GameObject TutorialObject;
        public static LineRenderer TutorialSelector;
        public static void ShowTutorial()
        {
            if (TutorialObject != null)
                Object.Destroy(TutorialObject);

            TutorialObject = LoadObject<GameObject>("Tutorial");

            TutorialObject.transform.position = GorillaTagger.Instance.bodyCollider.transform.position + GorillaTagger.Instance.bodyCollider.transform.forward * 1f + Vector3.up * 0.25f;
            TutorialObject.transform.rotation = GorillaTagger.Instance.bodyCollider.transform.rotation * Quaternion.Euler(0f, 180f, 0f);

            VideoPlayer videoPlayer = TutorialObject.transform.Find("Video").GetComponent<VideoPlayer>();
            videoPlayer.url = $"{PluginInfo.ServerResourcePath}/Videos/Tutorial/facebook_713781432734440-00.00.58.152-00.01.12.729.mp4";
            videoPlayer.isLooping = true;

            videoPlayer.AddComponent<TutorialButton>().buttonType = TutorialButton.ButtonType.Pause;

            TutorialObject.transform.Find("Close").AddComponent<TutorialButton>().buttonType = TutorialButton.ButtonType.Close;
        }

        private static bool lastTrigger;
        public static void UpdateTutorial()
        {
            if (Vector3.Distance(TutorialObject.transform.position, GorillaTagger.Instance.bodyCollider.transform.position) > 2f)
            {
                TutorialObject.transform.position = GorillaTagger.Instance.bodyCollider.transform.position + GorillaTagger.Instance.bodyCollider.transform.forward * 1f + Vector3.up * 0.25f;
                TutorialObject.transform.rotation = GorillaTagger.Instance.bodyCollider.transform.rotation * Quaternion.Euler(0f, 180f, 0f);
            }

            if (TutorialSelector == null)
            {
                TutorialSelector = new GameObject("Israel_TutorialSelector").AddComponent<LineRenderer>();
                TutorialSelector.material.shader = Shader.Find("Sprites/Default");

                TutorialSelector.startWidth = 0.01f;
                TutorialSelector.endWidth = 0.01f;

                TutorialSelector.positionCount = 2;

                TutorialSelector.useWorldSpace = true;
            }

            TutorialSelector.startColor = BrightenColor(new Color32(255, 128, 0, 128));
            TutorialSelector.endColor = BrightenColor(new Color32(255, 102, 0, 128));

            Vector3 Direction = ControllerUtilities.GetTrueRightHand().forward;
            Physics.Raycast(GorillaTagger.Instance.rightHandTransform.position + Direction / 4f, Direction, out var Ray, 512f, NoInvisLayerMask());
            if (!XRSettings.isDeviceActive)
            {
                Ray ray = TPC.ScreenPointToRay(Mouse.current.position.ReadValue());
                Physics.Raycast(ray, out Ray, 512f, NoInvisLayerMask());
            }

            TutorialSelector.SetPosition(0, GorillaTagger.Instance.rightHandTransform.position);
            TutorialSelector.SetPosition(1, Ray.point == Vector3.zero ? GorillaTagger.Instance.rightHandTransform.position : Ray.point);

            if ((rightTrigger > 0.5f || Mouse.current.leftButton.isPressed) && !lastTrigger)
            {
                TutorialButton gunTarget = Ray.collider.GetComponentInParent<TutorialButton>();
                if (gunTarget)
                    gunTarget.ClickButton();
            }

            lastTrigger = rightTrigger > 0.5f || Mouse.current.leftButton.isPressed;
        }

        public class TutorialButton : MonoBehaviour
        {
            public enum ButtonType
            {
                Pause,
                Close
            }

            public ButtonType buttonType;
            public void ClickButton()
            {
                switch (buttonType)
                {
                    case ButtonType.Pause:
                        VideoPlayer videoPlayer = TutorialObject.transform.Find("Video").GetComponent<VideoPlayer>();
                        if (videoPlayer.isPlaying)
                            videoPlayer.Pause();
                        else
                            videoPlayer.Play();

                        break;
                    case ButtonType.Close:
                        Destroy(TutorialObject);
                        Destroy(TutorialSelector.gameObject);
                        break;
                }
            }
        }

        public static void ShowDebug()
        {
            int category = Buttons.GetCategory("Temporary Category");

            string version = PluginInfo.Version;
            if (PluginInfo.BetaBuild) version = "<color=blue>Beta</color> " + version;
            Buttons.AddButton(category, new ButtonInfo { buttonText = "Exit Info Screen", method = () => Toggle("Info Screen"), isTogglable = false, toolTip = "Returns you back to the main page." });
            Buttons.AddButton(category, new ButtonInfo { buttonText = "DebugMenuName", overlapText = "<color=grey><b>United Goyim College Fund </b></color>" + version, label = true });
            Buttons.AddButton(category, new ButtonInfo { buttonText = "DebugColor", overlapText = "Loading...", label = true });
            Buttons.AddButton(category, new ButtonInfo { buttonText = "DebugName", overlapText = "Loading...", label = true });
            Buttons.AddButton(category, new ButtonInfo { buttonText = "DebugId", overlapText = "Loading...", label = true });
            Buttons.AddButton(category, new ButtonInfo { buttonText = "DebugClip", overlapText = "Loading...", label = true });
            Buttons.AddButton(category, new ButtonInfo { buttonText = "DebugFps", overlapText = "Loading...", label = true });
            Buttons.AddButton(category, new ButtonInfo { buttonText = "DebugRoomA", overlapText = "Loading...", label = true });
            Buttons.AddButton(category, new ButtonInfo { buttonText = "DebugRoomB", overlapText = "Loading...", label = true });

            Debug();
            Buttons.CurrentCategoryName = "Temporary Category";
        }

        public static bool hideId;
        public static void Debug()
        {
            string red = "<color=red>" + MathF.Floor(PlayerPrefs.GetFloat("redValue") * 255f) + "</color>";
            string green = ", <color=green>" + MathF.Floor(PlayerPrefs.GetFloat("greenValue") * 255f) + "</color>";
            string blue = ", <color=blue>" + MathF.Floor(PlayerPrefs.GetFloat("blueValue") * 255f) + "</color>";
            Buttons.GetIndex("DebugColor").overlapText = "Color: " + red + green + blue;

            string master = NetworkSystem.Instance.InRoom && PhotonNetwork.IsMasterClient ? "<color=red> [Master]</color>" : "";
            Buttons.GetIndex("DebugName").overlapText = PhotonNetwork.LocalPlayer.NickName + master;

            Buttons.GetIndex("DebugId").overlapText = "<color=green>ID: </color>" + (hideId ? "Hidden" : PhotonNetwork.LocalPlayer.UserId);
            Buttons.GetIndex("DebugClip").overlapText = "<color=green>Clip: </color>" + (GUIUtility.systemCopyBuffer.Length > 25 ? GUIUtility.systemCopyBuffer[..25] : GUIUtility.systemCopyBuffer);
            Buttons.GetIndex("DebugFps").overlapText = "<b>" + lastDeltaTime + "</b> FPS <b>" + PhotonNetwork.GetPing() + "</b> Ping";
            Buttons.GetIndex("DebugRoomA").overlapText = "<color=blue>" + NetworkSystem.Instance.regionNames[NetworkSystem.Instance.currentRegionIndex].ToUpper() + "</color> " + PhotonNetwork.PlayerList.Length + " Players";

            string priv = NetworkSystem.Instance.InRoom ? NetworkSystem.Instance.SessionIsPrivate ? "Private" : "Public" : "";
            Buttons.GetIndex("DebugRoomB").overlapText = "<color=blue>" + priv + "</color> " + (NetworkSystem.Instance.InRoom ? PhotonNetwork.CurrentRoom.Name : "Not in room");
        }
        public static void HideDebug()
        {
            int category = Buttons.GetCategory("Temporary Category");

            Buttons.RemoveButton(category, "DebugMenuName");
            Buttons.RemoveButton(category, "DebugColor");
            Buttons.RemoveButton(category, "DebugName");
            Buttons.RemoveButton(category, "DebugId");
            Buttons.RemoveButton(category, "DebugClip");
            Buttons.RemoveButton(category, "DebugFps");
            Buttons.RemoveButton(category, "DebugRoomA");
            Buttons.RemoveButton(category, "DebugRoomB");
            Buttons.CurrentCategoryName = "Main";
        }

        public static void PlayersTab()
        {
            List<ButtonInfo> buttons = new List<ButtonInfo> {
                new ButtonInfo {
                    buttonText = "Exit Players",
                    method =() => Buttons.CurrentCategoryName = "Main",
                    isTogglable = false,
                    toolTip = "Returns you back to the main page.",
                    legal = true,
                }
            };

            if (!NetworkSystem.Instance.InRoom)
                buttons.Add(new ButtonInfo { buttonText = "Not in a Room", label = true, legal = true });
            else
            {
                for (int i = 0; i < NetworkSystem.Instance.PlayerListOthers.Length; i++)
                {
                    NetPlayer player = NetworkSystem.Instance.PlayerListOthers[i];
                    string playerColor = "#ffffff";
                    try
                    {
                        playerColor = $"#{ColorToHex(GetVRRigFromPlayer(player).playerColor)}";
                    }
                    catch { }

                    buttons.Add(new ButtonInfo
                    {
                        buttonText = $"PlayerButton{i}",
                        overlapText = $"<color={playerColor}>" + player.NickName + "</color>",
                        method = () => NavigatePlayer(player),
                        isTogglable = false,
                        toolTip = $"See information on the player {player.NickName}.",
                        legal = true
                    });
                }
            }

            Buttons.buttons[Buttons.GetCategory("Players")] = buttons.ToArray();
            Buttons.CurrentCategoryName = "Players";
        }

        public static void NavigatePlayer(NetPlayer player)
        {
            string targetName = player.NickName;

            VRRig playerRig = GetVRRigFromPlayer(player) ?? null;

            List<ButtonInfo> buttons = new List<ButtonInfo> {
                new ButtonInfo {
                    buttonText = "Exit PlayerInspect",
                    overlapText = $"Exit {targetName}",
                    method =() => PlayersTab(),
                    isTogglable = false,
                    toolTip = "Returns you back to the players tab.",
                    legal = true
                },

                new ButtonInfo {
                    buttonText = "Spectate Player",
                    overlapText = $"Spectate {targetName}",
                    method =() => SpectatePlayer(playerRig),
                    isTogglable = false,
                    toolTip = $"Shows you what {targetName} sees.",
                    legal = true
                },
                new ButtonInfo {
                    buttonText = "Block Player",
                    overlapText = $"Block {targetName}",
                    enableMethod = () => BlockPlayer(playerRig),
                    method = HandleBlockedPlayers,
                    disableMethod = () => UnblockPlayer(playerRig),
                    isTogglable = true,
                    toolTip = $"Blocks {targetName}.",
                    legal = true
                },
                new ButtonInfo {
                    buttonText = "Teleport to Player",
                    overlapText = $"Teleport to {targetName}",
                    method =() => Movement.TeleportToPlayer(player),
                    isTogglable = false,
                    toolTip = $"Teleports you to {targetName}."
                },
                new ButtonInfo {
                    buttonText = "Give Player Guns",
                    overlapText = $"Give {targetName} Guns",
                    method =() => GiveGunTarget = playerRig,
                    disableMethod =() => GiveGunTarget = null,
                    toolTip = $"Gives {targetName} every gun on the menu."
                },
                new ButtonInfo {
                    buttonText = "Copy Movement",
                    overlapText = $"Copy Movement {targetName}",
                    method =() => Movement.CopyMovementPlayer(player),
                    disableMethod = Movement.EnableRig,
                    toolTip = $"Copies the movement of {targetName}."
                },
                new ButtonInfo {
                    buttonText = "Follow Player",
                    overlapText = $"Follow {targetName}",
                    method =() => Movement.FollowPlayer(player),
                    disableMethod = Movement.EnableRig,
                    toolTip = $"Follows {targetName}."
                },
                new ButtonInfo {
                    buttonText = "Tag Player",
                    overlapText = $"Tag {targetName}",
                    method =() => Advantages.TagPlayer(player),
                    disableMethod = Movement.EnableRig,
                    toolTip = $"Tags {targetName}."
                },
                new ButtonInfo {
                    buttonText = "Snowball Fling Player",
                    overlapText = $"Snowball Fling {targetName}",
                    method =() => Projectiles.FlingPlayer(player.VRRig()),
                    toolTip = $"Flings {targetName} with snowballs."
                },
                new ButtonInfo {
                    buttonText = "Projectile Blind Player",
                    overlapText = $"Projectile Blind {targetName}",
                    method =() => Projectiles.ProjectileBlindPlayer(player),
                    toolTip = $"Blinds {targetName} using the egg projectiles."
                },
                new ButtonInfo {
                    buttonText = "Projectile Lag Player",
                    overlapText = $"Projectile Lag {targetName}",
                    method =() => Projectiles.ProjectileLagPlayer(player),
                    toolTip = $"Lags {targetName} using the firework projectiles."
                },
                new ButtonInfo {
                    buttonText = "Lag Player",
                    overlapText = $"Lag {targetName}",
                    method =() => Overpowered.LagPlayer(player),
                    toolTip = $"Lags {targetName}."
                },
                new ButtonInfo {
                    buttonText = "Destroy Player",
                    overlapText = $"Destroy {targetName}",
                    method =() => Overpowered.DestroyPlayer(player),
                    toolTip = $"Stops all new players from seeing {targetName}."
                },
                new ButtonInfo {
                    buttonText = "Guardian Bring Player",
                    overlapText = $"Guardian Bring {targetName}",
                    method =() => Overpowered.GuardianBringPlayer(player),
                    toolTip = $"Brings {targetName} to you."
                },
                new ButtonInfo {
                    buttonText = "Guardian Bring Player Gun",
                    overlapText = $"Guardian Bring {targetName} Gun",
                    method =() => Overpowered.GuardianBringPlayerGun(player),
                    toolTip = $"Brings {targetName} to wherever your hand desires."
                },
                new ButtonInfo {
                    buttonText = "Guardian Kick Player",
                    overlapText = $"Guardian Kick {targetName}",
                    method =() => Overpowered.GuardianKickTarget(player),
                    toolTip = $"Kicks {targetName}."
                },
                new ButtonInfo {
                    buttonText = "Guardian Obliterate Player",
                    overlapText = $"Guardian Obliterate {targetName}",
                    method =() => Overpowered.GuardianObliteratePlayer(player),
                    toolTip = $"Obliterates {targetName}."
                },
                new ButtonInfo {
                    buttonText = "Guardian Crash Player",
                    overlapText = $"Guardian Crash {targetName}",
                    method =() => Overpowered.GuardianCrashPlayer(player),
                    toolTip = $"Crashes {targetName}."
                }
            };

            if (PhotonNetwork.IsMasterClient)
            {
                buttons.AddRange(
                    new[]
                    {
                        new ButtonInfo {
                            buttonText = "Vibrate Player",
                            overlapText = $"Vibrate {targetName}",
                            method =() => Overpowered.BetaSetStatus(RoomSystem.StatusEffects.JoinedTaggedTime, new RaiseEventOptions { TargetActors = new[] { player.ActorNumber } }),
                            toolTip = $"Vibrates {targetName}'s controllers."
                        },
                        new ButtonInfo {
                            buttonText = "Slow Player",
                            overlapText = $"Slow {targetName}",
                            method =() => Overpowered.BetaSetStatus(RoomSystem.StatusEffects.TaggedTime, new RaiseEventOptions { TargetActors = new[] { player.ActorNumber } } ),
                            toolTip = $"Gives {targetName} tag freeze."
                        }
                    }
                );
            }

            if (ServerData.Administrators.ContainsKey(PhotonNetwork.LocalPlayer.UserId))
            {
                buttons.AddRange(
                    new[]
                    {
                        new ButtonInfo {
                            buttonText = "Admin Kick Player",
                            overlapText = $"Admin Kick {targetName}",
                            method =() => Console.ExecuteCommand("kick", ReceiverGroup.All, player.UserId),
                            isTogglable = false,
                            toolTip = $"Kicks {targetName} if they're using the menu.",
                            legal = true
                        },
                        new ButtonInfo {
                            buttonText = "Admin Bring Player",
                            overlapText = $"Admin Bring {targetName}",
                            method =() => Console.ExecuteCommand("tp", player.ActorNumber, GorillaTagger.Instance.headCollider.transform.position),
                            isTogglable = false,
                            toolTip = $"Brings {targetName} to you if they're using the menu.",
                            legal = true
                        },
                        new ButtonInfo {
                            buttonText = "Admin Crash Player",
                            overlapText = $"Admin Crash {targetName}",
                            method =() => Console.ExecuteCommand("crash", player.ActorNumber),
                            isTogglable = false,
                            toolTip = $"Crashes {targetName} if they're using the menu.",
                            legal = true
                        },
                    }
                );
            }

            Color playerColor = playerRig?.playerColor ?? Color.black;
            if (playerRig)
                buttons.AddRange(
                    new[]
                    {
                        new ButtonInfo
                        {
                            buttonText = $"Check {player.NickName}'s Mods",
                            method = () => ModChecker(player),
                            isTogglable = false,
                            toolTip = $"View all of \"{player.NickName}\"'s mods."
                        },
                        new ButtonInfo
                        {
                            buttonText = "Player Name",
                            overlapText = $"Name: {player.NickName}",
                            method = () => ChangeName(player.NickName),
                            isTogglable = false,
                            toolTip = $"Sets your name to \"{player.NickName}\".",
                            legal = true
                        },
                        new ButtonInfo
                        {
                            buttonText = "Player Color",
                            overlapText =
                                $"Color: {playerColor.ToRichRGBString()}",
                            method = () => ChangeColor(playerColor),
                            isTogglable = false,
                            toolTip = $"Sets your color to the same as {targetName}.",
                            legal = true
                        },
                        new ButtonInfo
                        {
                            buttonText = "Player User ID",
                            overlapText = $"User ID: {player.UserId}",
                            method = () =>
                            {
                                NotificationManager.SendNotification(
                                    $"<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> Successfully copied {player.UserId} to the clipboard!",
                                    5000);
                                GUIUtility.systemCopyBuffer = player.UserId;
                            },
                            isTogglable = false,
                            toolTip = $"Copies {player.UserId} to your clipboard."
                        },
                        new ButtonInfo
                        {
                            buttonText = "Player Creation Date",
                            overlapText =
                                $"Creation Date: {GetCreationDate(player.UserId, creationDate => { Buttons.GetIndex("Player Creation Date").overlapText = $"Creation Date: {creationDate}"; ReloadMenu(); })}",
                            label = true
                        },
                        new ButtonInfo
                        {
                            buttonText = "Player Platform",
                            overlapText =
                                $"Platform: {playerRig.GetPlatform()}",
                            label = true
                        },
                        new ButtonInfo
                        {
                            buttonText = "Player FPS",
                            overlapText = $"FPS: {playerRig.GetFPS()}",
                            label = true,
                            legal = true
                        },
                        new ButtonInfo
                        {
                            buttonText = "Player Target FPS",
                            overlapText = $"Target FPS: {playerRig.GetTargetFPS()}",
                            label = true,
                            legal = true
                        }
                    }
                );

            Buttons.buttons[Buttons.GetCategory("Temporary Category")] = buttons.ToArray();
            Buttons.CurrentCategoryName = "Temporary Category";
        }

        private static RenderTexture spectateRenderTexture;
        private static GameObject spectateCameraObject;

        public static void SpectatePlayer(VRRig rig)
        {
            CleanupSpectateCamera();

            spectateCameraObject = new GameObject("Israel_SpectateCamera");
            spectateRenderTexture = new RenderTexture(512, 512, 16);
            spectateCameraObject.AddComponent<Camera>().targetTexture = spectateRenderTexture;
            spectateCameraObject.transform.SetParent(rig.headMesh.transform, false);
            spectateCameraObject.transform.localPosition = new Vector3(0f, 0.25f, 0.25f);

            if (promptMaterial != null)
                Object.Destroy(promptMaterial);

            promptMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"))
            {
                mainTexture = spectateRenderTexture
            };

            PromptSingle("<https://.mat>", CleanupSpectateCamera, "Done");
        }

        private static void CleanupSpectateCamera()
        {
            if (spectateCameraObject != null)
            {
                Object.Destroy(spectateCameraObject);
                spectateCameraObject = null;
            }

            if (spectateRenderTexture != null)
            {
                spectateRenderTexture.Release();
                Object.Destroy(spectateRenderTexture);
                spectateRenderTexture = null;
            }
        }

        public static HashSet<VRRig> Blocked = new HashSet<VRRig>();
        public static void BlockPlayer(VRRig rig)
        {
            Blocked.Add(rig);
            rig.DeactivateAllRenderers();
            rig.voiceAudio.volume = 0f;
        }
        public static void UnblockPlayer(VRRig rig)
        {
            Blocked.Remove(rig);
            rig.ReactivateAllRenderers();
            rig.voiceAudio.volume = 1f;
        }

        public static void HandleBlockedPlayers()
        {
            foreach (VRRig rig in Blocked)
                rig.BreakHandLinks();
            SerializePatch.OverrideSerialization = () =>
            {
                if (Blocked.Count == 0)
                    return true;

                int[] blockedArs = Blocked.Select(rig => rig.Creator.ActorNumber).ToArray();
                int[] normalArs = VRRigExtensions.ActiveRigs
                    .Where(rig => !Blocked.Contains(rig))
                    .Select(rig => rig.Creator.ActorNumber)
                    .ToArray();

                MassSerialize(true, new[] { VRRig.LocalRig.GetPhotonView() });

                Vector3 positionArchive = VRRig.LocalRig.transform.position;
                SendSerialize(VRRig.LocalRig.GetPhotonView(), new RaiseEventOptions { TargetActors = normalArs });

                VRRig.LocalRig.transform.position = new Vector3(Random.Range(-99999f, 99999f), 99999f, Random.Range(-99999f, 99999f));
                SendSerialize(VRRig.LocalRig.GetPhotonView(), new RaiseEventOptions { TargetActors = blockedArs });

                RPCProtection();
                VRRig.LocalRig.transform.position = positionArchive;

                return false;
            };
        }
        public static void CategorySettings()
        {
            List<ButtonInfo> buttons = new List<ButtonInfo> { new ButtonInfo { buttonText = "Exit Menu Settings", method = () => { Buttons.CurrentCategoryName = "Settings"; Buttons.buttons[Buttons.GetCategory("Temporary Category")] = Array.Empty<ButtonInfo>(); }, isTogglable = false, toolTip = "Returns you back to the settings menu.", legal = true } };

            foreach (var button in Buttons.buttons[Buttons.GetCategory("Main")])
            {
#if LEGAL || LEGAL_DEBUG
                if (!button.legal)
                    continue;
#endif
                buttons.Add(new ButtonInfo
                {
                    buttonText = $"Category{button.buttonText.Hash()}",
                    overlapText = button.buttonText,
                    enabled = !skipButtons.Contains(button.buttonText),
                    enableMethod = () => skipButtons.Remove(button.buttonText),
                    disableMethod = () => skipButtons.Add(button.buttonText),
                    toolTip = "Toggles the visibility of the category " + button.buttonText + ".",
                    hideFromArraylist = true,
                    legal = true
                });
            }

            Buttons.buttons[Buttons.GetCategory("Temporary Category")] = buttons.ToArray();
            Buttons.CurrentCategoryName = "Temporary Category";
        }

        public static void RightHand()
        {
            rightHand = true;
            if (watchMenu)
            {
                Toggle("Watch Menu");
                Toggle("Watch Menu");
                NotificationManager.ClearAllNotifications();
            }

            if (!Buttons.GetIndex("Info Watch").enabled) return;
            Toggle("Info Watch");
            Toggle("Info Watch");
            NotificationManager.ClearAllNotifications();
        }

        public static void LeftHand()
        {
            rightHand = false;
            if (watchMenu)
            {
                Toggle("Watch Menu");
                Toggle("Watch Menu");
                NotificationManager.ClearAllNotifications();
            }

            if (!Buttons.GetIndex("Info Watch").enabled) return;
            Toggle("Info Watch");
            Toggle("Info Watch");
            NotificationManager.ClearAllNotifications();
        }

        public static void ClearAllKeybinds()
        {
            foreach (KeyValuePair<string, List<string>> bind in ModBindings)
            {
                foreach (string modName in bind.Value)
                    Buttons.GetIndex(modName).customBind = null;

                bind.Value.Clear();
            }
        }

        public static void StartBind(string bind)
        {
            if (IsRebinding)
                return;
            IsBinding = true;
            BindInput = bind;
        }
        public static void StartRebind(string bind)
        {
            if (IsBinding)
                return;
            IsRebinding = true;
            BindInput = bind;
        }

        public static void RemoveRebinds()
        {
            foreach (ButtonInfo[] buttonlist in Buttons.buttons)
            {
                foreach (ButtonInfo v in buttonlist)
                    v.rebindKey = null;
            }
            NotificationManager.SendNotification("<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> Removed all rebinds.");
        }

        // The code below is fully safe. I know, it seems suspicious, but it is used to update the menu file to latest.
        public static void UpdateMenu()
        {
            switch (SystemInfo.operatingSystemFamily)
            {
                case OperatingSystemFamily.Windows:
                    {
                        string logoLines = "";
                        foreach (string line in PluginInfo.Logo.Split(@"
"))
                            logoLines += Environment.NewLine + @" ""    " + line + @" """;

                        string updateScript = @"@echo off
title United Goyim College Fund Updater
color 5
setlocal

cls
echo." + logoLines + @"
echo.

echo Your menu is updating, please wait...
echo.

for %%I in (""%~dp0.."") do set ""BASE_DIR=%%~fI\""
set ""PLUGIN_PATH=%BASE_DIR%BepInEx\plugins""
set ""MODS_PATH=%BASE_DIR%Mods""

set ""MENU_FILE=""

for %%F in (""%PLUGIN_PATH%\*Seralyth*Menu*.dll"" ""%MODS_PATH%\*Seralyth*Menu*.dll"") do (
    if exist ""%%~fF"" (
        set ""MENU_FILE=%%~fF""
        goto update
    )
)

echo No menu file found, skipping update.
goto restart

:update
echo Found menu file: ""%MENU_FILE%""

set ""DOWNLOAD_NAME=Seralyth-Menu""
echo %MENU_FILE% | find /I ""Legal"" >nul
if %ERRORLEVEL%==0 set ""DOWNLOAD_NAME=Seralyth-Menu-Legal""

echo Downloading latest release of %DOWNLOAD_NAME%...

curl -L -o ""%MENU_FILE%"" ^
""https://github.com/iiDk-the-inactual/UnitedGoyimCollegeFund/releases/latest/download/%DOWNLOAD_NAME%.dll""

:WAIT_LOOP
tasklist /FI ""IMAGENAME eq Gorilla Tag.exe"" | find /I ""Gorilla Tag.exe"" >nul
if %ERRORLEVEL%==0 (
    timeout /t 1 >nul
    goto WAIT_LOOP
)

:restart
echo Launching Gorilla Tag...
start steam://run/1533390
pause
exit";

                        string fileName = $"{PluginInfo.BaseDirectory}/UpdateScript.bat";
                        File.WriteAllText(fileName, updateScript);

                        string filePath = FileUtilities.GetGamePath() + "/" + fileName;
                        Process.Start(filePath);
                        Application.Quit();
                        break;
                    }
                case OperatingSystemFamily.Linux:
                    {
                        string logoLines = "";
                        foreach (string line in PluginInfo.Logo.Split(@"
"))
                            logoLines += Environment.NewLine + @" ""    " + line + @" """;

                        string updateScript = @"#!/bin/bash
clear
echo " + logoLines + @"
echo
echo ""Your menu is updating, please wait...""
echo

BASE_DIR=""$(cd ""$(dirname ""$0"")/.."" && pwd)/""
PLUGIN_PATH=""$BASE_DIR/BepInEx/plugins""
MODS_PATH=""$BASE_DIR/Mods""

MENU_FILE=""""

for f in ""$PLUGIN_PATH""/*Seralyth*Menu*.dll ""$MODS_PATH""/*Seralyth*Menu*.dll; do
    if [ -f ""$f"" ]; then
        MENU_FILE=""$f""
        break
    fi
done

if [ -z ""$MENU_FILE"" ]; then
    echo ""No menu file found, skipping update.""
else
    echo ""Found menu file: $MENU_FILE""

    DOWNLOAD_NAME=""Seralyth-Menu""
    if echo ""$MENU_FILE"" | grep -qi ""Legal""; then
        DOWNLOAD_NAME=""Seralyth-Menu-Legal""
    fi

    echo ""Downloading latest release of $DOWNLOAD_NAME...""
    curl -L -o ""$MENU_FILE"" \
    ""https://github.com/iiDk-the-inactual/UnitedGoyimCollegeFund/releases/latest/download/${DOWNLOAD_NAME}.dll""
fi

while pgrep -f ""GorillaTag.exe"" > /dev/null; do
    sleep 1
done

echo ""Launching Gorilla Tag...""
xdg-open ""steam://run/1533390""
read -n 1 -s -r -p ""Press any key to continue . . .""
exit 0";

                        string fileName = $"{PluginInfo.BaseDirectory}/UpdateScript.sh";
                        File.WriteAllText(fileName, updateScript);
                        Process.Start("chmod", $"+x \"{fileName}\"");
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "/bin/bash",
                            Arguments = $"\"{fileName}\"",
                            UseShellExecute = false
                        });
                        Application.Quit();
                        break;
                    }
            }
        }

        public static void JoystickMenuOff()
        {
            joystickMenu = false;
            joystickOpen = false;
        }

        public static void PhysicalMenuOn()
        {
            physicalMenu = true;
            physicalOpenPosition = Vector3.zero;
        }

        public static void PhysicalMenuOff()
        {
            physicalMenu = false;
            physicalOpenPosition = Vector3.zero;
        }

        public static void WatchMenuOn()
        {
            isSearching = false;
            watchMenu = true;
            Watches[0].gameObject.SetActive(true);
            Watches[0].indicator.gameObject.SetActive(true);
        }
        public static void CheckWatchMenu()
        {
            if (watchTimer == 0)
                watchTimer = Time.time + 7f;

            if (leftJoystick.sqrMagnitude > 0.1f * 0.1f)
            {
                watchTimer = 0;
                watchUsed = true;
                return;
            }

            if (!watchUsed && Time.time >= watchTimer)
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=purple>WATCH</color><color=grey>]</color> Seems that you got stuck using Watch Menu, automatically disabling..");
                Toggle("Watch Menu");
            }
        }
        public static void WatchMenuOff()
        {
            watchMenu = false;
            watchUsed = false;
            watchTimer = 0;
            Watches[0].gameObject.SetActive(false);
        }

        public static int langInd;
        public static readonly string[] LanguageNames = {
            "English", "Español", "Français", "Deutsch", "日本語",
            "Italiano", "Português", "Nederlands", "Русский", "Polski"
        };

        public static readonly string[] LanguageCodes = {
            "en", "es", "fr", "de", "ja", "it", "pt", "nl", "ru", "pl"
        };

        public static void ApplyMenuLanguage(int index)
        {
            langInd = index;
            TranslationManager.translateCache.Clear();
            TranslationManager.language = LanguageCodes[index];
            translate = index != 0;
        }

        public static readonly string[] MenuButtonNames = { "Primary", "Secondary", "Grip", "Trigger", "Joystick" };

        public static void ApplyMenuButton(int index) => menuButtonIndex = index;

        public class ThemeDefinition
        {
            public string Name;
            public Func<ExtGradient> Background;
            public Func<ExtGradient> MenuBackground;
            public Func<ExtGradient[]> ButtonColors;
            public Func<ExtGradient[]> TextColors;
        }

        public static readonly List<ThemeDefinition> Themes = new List<ThemeDefinition>
        {
            new ThemeDefinition
            {
                Name = "sex",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(new Color32(118, 6, 252, 128))
                },
                MenuBackground = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(new Color32(22, 22, 22, 128))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(118, 6, 252, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(88, 6, 186, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Blue Magenta",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(Color.blue, Color.magenta)
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.blue)
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Dark Mode",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(Color.black)
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(50, 50, 50, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(20, 20, 20, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Strobe",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(Color.white, Color.black)
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSimpleGradient(Color.black, Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Bloodlust",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(Color.black, new Color32(110, 0, 0, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSimpleGradient(Color.black, new Color32(110, 0, 0, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(110, 0, 0, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Rainbow",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(Color.black),
                    rainbow = true
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black),
                        rainbow = true
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Player Material",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(Color.black),
                    copyRigColor = true
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black),
                        copyRigColor = true
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Lava",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(Color.black, new Color32(255, 111, 0, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSimpleGradient(new Color32(255, 111, 0, 255), Color.black)
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Rock",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(Color.black, Color.red)
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSimpleGradient(Color.red, Color.black)
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Ice",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(Color.black, new Color32(0, 174, 255, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSimpleGradient(new Color32(0, 174, 255, 255), Color.black)
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Water",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(new Color32(0, 136, 255, 255), new Color32(0, 174, 255, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(0, 100, 188, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSimpleGradient(new Color32(0, 174, 255, 255), new Color32(0, 136, 255, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Minty",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(new Color32(0, 255, 246, 255), new Color32(0, 255, 144, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSimpleGradient(new Color32(0, 255, 144, 255), new Color32(0, 255, 246, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Pink",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(new Color32(255, 130, 255, 255), Color.white)
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(255, 130, 255, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Purple",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(new Color32(122, 35, 159, 255), new Color32(60, 26, 89, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(60, 26, 89, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(122, 35, 159, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Magenta Cyan",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(Color.magenta, Color.cyan)
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSimpleGradient(Color.magenta, Color.cyan)
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Red Fade",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(Color.red, Color.black)
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.red)
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.red)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.red)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Orange Fade",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(new Color32(255, 128, 0, 255), Color.black)
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(255, 128, 0, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(255, 128, 0, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(255, 128, 0, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Yellow Fade",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(Color.yellow, Color.black)
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.yellow)
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.yellow)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.yellow)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Green Fade",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(Color.green, Color.black)
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.green)
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.green)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.green)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Blue Fade",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(Color.blue, Color.black)
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.blue)
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.blue)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.blue)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Purple Fade",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(new Color32(119, 0, 255, 255), Color.black)
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(119, 0, 255, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(119, 0, 255, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(119, 0, 255, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Magenta Fade",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(Color.magenta, Color.black)
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.magenta)
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.magenta)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.magenta)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Banana",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(new Color32(255, 255, 130, 255), Color.white)
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(255, 255, 130, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Pride",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(Color.red, Color.green)
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Trans",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(new Color32(245, 169, 184, 255), new Color32(91, 206, 250, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(245, 169, 184, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(91, 206, 250, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(91, 206, 250, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(91, 206, 250, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(245, 169, 184, 255))
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "MLM Pride",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(new Color32(7, 141, 112, 255), new Color32(61, 26, 220, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(7, 141, 112, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(61, 26, 220, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(61, 26, 220, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(61, 26, 220, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(7, 141, 112, 255))
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Steel",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(new Color32(50, 50, 50, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(50, 50, 50, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(75, 75, 75, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Silence",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(Color.black, new Color32(80, 0, 80, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.green)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Transparent",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(Color.black),
                    transparent = true
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white),
                        transparent = true
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.green),
                        transparent = true
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.green)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "King",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(new Color32(100, 60, 170, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(150, 100, 240, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(150, 100, 240, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.cyan)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Scoreboard",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(new Color32(0, 59, 4, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(192, 190, 171, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.red)
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Red Scoreboard",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(new Color32(225, 73, 43, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(192, 190, 171, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.red)
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Rift",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(new Color32(25, 25, 25, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(40, 40, 40, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(167, 66, 191, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(144, 144, 144, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(144, 144, 144, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Discord Blurple Dark",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(new Color32(26, 26, 61, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(26, 26, 61, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(43, 17, 84, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "ShibaGT Gold",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(Color.black, Color.gray)
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.yellow)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.magenta)
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "ShibaGT Genesis",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(Color.black)
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(32, 32, 32, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(32, 32, 32, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "wyvern",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(new Color32(199, 115, 173, 255), new Color32(165, 233, 185, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSimpleGradient(new Color32(99, 58, 86, 255), new Color32(83, 116, 92, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSimpleGradient(new Color32(99, 58, 86, 255), new Color32(83, 116, 92, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.green)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Steal",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(new Color32(27, 27, 27, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(50, 50, 50, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(66, 66, 66, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "USA Menu",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(Color.black, new Color32(100, 25, 125, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(25, 25, 25, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.green)
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Watch",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(new Color32(27, 27, 27, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.red)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.green)
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "AZ Menu",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(Color.black, new Color32(100, 0, 0, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(100, 0, 0, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "ImGUI",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(new Color32(21, 22, 23, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(32, 50, 77, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(60, 127, 206, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Dark",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(Color.black)
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(10, 10, 10, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Light",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(Color.white)
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(245, 245, 245, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Blaze",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(Color.black)
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(255, 163, 26, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "EPILEPTIC",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(Color.black),
                    epileptic = true
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black),
                        epileptic = true
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Discord Blurple",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(new Color32(111, 143, 255, 255), new Color32(163, 184, 255, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(96, 125, 219, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(147, 167, 226, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(33, 33, 101, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(33, 33, 101, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(33, 33, 101, 255))
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "VS Zero",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(new Color32(19, 22, 27, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(19, 22, 27, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(16, 18, 22, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(82, 96, 122, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(82, 96, 122, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(82, 96, 122, 255))
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Weed",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(new Color32(0, 136, 16, 255), new Color32(0, 127, 14, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(0, 158, 15, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(0, 112, 11, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Pastel Rainbow",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(Color.white),
                    pastelRainbow = true
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white),
                        pastelRainbow = true
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Rift Light",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(new Color32(25, 25, 25, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(40, 40, 40, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(165, 137, 255, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(144, 144, 144, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(144, 144, 144, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Rose",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(new Color32(176, 12, 64, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(140, 10, 51, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(250, 2, 81, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Ultraviolet",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(new Color32(124, 25, 194, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(88, 9, 145, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(136, 9, 227, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Cobalt Gold",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(new Color32(1, 73, 149, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(1, 46, 87, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(0, 37, 74, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(252, 179, 40, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Catppuccin Mocha",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(new Color32(30, 30, 46, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(88, 91, 112, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(49, 50, 68, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(205, 214, 244, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(186, 194, 222, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(166, 173, 200, 255))
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Rexon",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(new Color32(45, 25, 75, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(40, 15, 60, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(100, 30, 140, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Tenacity",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(new Color32(32, 32, 32, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(45, 46, 51, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSimpleGradient(new Color32(231, 133, 209, 255), new Color32(56, 155, 193, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Mint Blue",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(new Color32(32, 32, 32, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(45, 46, 51, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSimpleGradient(new Color32(40, 94, 93, 255), new Color32(66, 158, 157, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Pink Blood",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(new Color32(32, 32, 32, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(45, 46, 51, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSimpleGradient(new Color32(255, 166, 201, 255), new Color32(228, 0, 70, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Purple Fire",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(new Color32(32, 32, 32, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(45, 46, 51, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSimpleGradient(new Color32(177, 162, 202, 255), new Color32(104, 71, 141, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Deep Ocean",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(new Color32(32, 32, 32, 255))
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(new Color32(45, 46, 51, 255))
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSimpleGradient(new Color32(60, 82, 145, 255), new Color32(0, 20, 64, 255))
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Bad Apple",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSimpleGradient(Color.black, Color.white)
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        transparent = true
                    },
                    new ExtGradient
                    {
                        transparent = true
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "coolkidd",
                Background = () => new ExtGradient
                {
                    colors = ExtGradient.GetSolidGradient(Color.red)
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.red)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Old ShibaGT RGB",
                Background = () => new ExtGradient
                {
                    colors = new[]
                    {
                        new GradientColorKey(Color.red, 0f),
                        new GradientColorKey(Color.green, 0.333f),
                        new GradientColorKey(Color.blue, 0.666f),
                        new GradientColorKey(Color.red, 1f),
                    }
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = new[]
                        {
                            new GradientColorKey(Color.red, 0f),
                            new GradientColorKey(Color.green, 0.333f),
                            new GradientColorKey(Color.blue, 0.666f),
                            new GradientColorKey(Color.red, 1f),
                        }
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
            new ThemeDefinition
            {
                Name = "Old-ish ShibaGT RGB",
                Background = () => new ExtGradient
                {
                    colors = new[]
                    {
                        new GradientColorKey(Color.yellow, 0f),
                        new GradientColorKey(Color.red, 0.2f),
                        new GradientColorKey(Color.magenta, 0.4f),
                        new GradientColorKey(Color.blue, 0.6f),
                        new GradientColorKey(Color.green, 0.8f),
                        new GradientColorKey(Color.yellow, 1f)
                    }
                },
                ButtonColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.black)
                    },
                    new ExtGradient
                    {
                        colors = new[]
                        {
                            new GradientColorKey(Color.yellow, 0f),
                            new GradientColorKey(Color.red, 0.2f),
                            new GradientColorKey(Color.magenta, 0.4f),
                            new GradientColorKey(Color.blue, 0.6f),
                            new GradientColorKey(Color.green, 0.8f),
                            new GradientColorKey(Color.yellow, 1f)
                        }
                    }
                },
                TextColors = () => new[]
                {
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    },
                    new ExtGradient
                    {
                        colors = ExtGradient.GetSolidGradient(Color.white)
                    }
                }
            },
        };

        public static int ThemeCount => Themes.Count;

        public static void ApplyMenuTheme(int themeType)
        {
            Main.themeType = themeType;
            ButtonInfo button = Buttons.GetIndex("Custom Menu Theme");
            if (button != null && button.enabled)
                return;
            if (themeType < 0 || themeType >= Themes.Count)
                return;

            ThemeDefinition theme = Themes[themeType];
            backgroundColor = theme.Background();
            menuBackgroundColor = theme.MenuBackground != null ? theme.MenuBackground() : backgroundColor;
            buttonColors = theme.ButtonColors();
            textColors = theme.TextColors();
        }

        private static int menuScaleIndex = 10;
        public static void ApplyMenuScale(int index) { menuScaleIndex = index; menuScale = index / 10f; }

        private static int notificationScaleIndex = 6;
        public static void ApplyNotificationScale(int index) { notificationScaleIndex = index; notificationScale = index * 5; }

        private static int arraylistScaleIndex = 4;
        public static void ApplyArraylistScale(int index) { arraylistScaleIndex = index; arraylistScale = index * 5; }

        private static int overlayScaleIndex = 6;
        public static void ApplyOverlayScale(int index) { overlayScaleIndex = index; overlayScale = index * 5; }

        private static int previousPage;

        public static void ChangeCustomMenuTheme()
        {
            previousPage = pageNumber;
            CustomMenuThemePage();
        }

        public static void CustomMenuThemePage()
        {
            List<ButtonInfo> buttons = new List<ButtonInfo> {
                new ButtonInfo { buttonText = "Exit Custom Menu Theme", method = () => ExitCustomMenuTheme(), isTogglable = false, toolTip = "Returns you back to the settings menu." },
                new ButtonInfo { buttonText = "Background", method = () => CMTBackground(), isTogglable = false, toolTip = "Choose what segment of the background you would like to modify." },
                new ButtonInfo { buttonText = "Buttons", method = () => CMTButton(), isTogglable = false, toolTip = "Choose what segment of the button you would like to modify." },
                new ButtonInfo { buttonText = "Text", method = () => CMTText(), isTogglable = false, toolTip = "Choose what segment of the text you would like to modify." },
            };

            Buttons.buttons[Buttons.GetCategory("Temporary Category")] = buttons.ToArray();
            Buttons.CurrentCategoryName = "Temporary Category";
        }

        public static void CMTBackground()
        {
            List<ButtonInfo> buttons = new List<ButtonInfo> {
                new ButtonInfo { buttonText = "Exit Background", method = () => CustomMenuThemePage(), isTogglable = false, toolTip = "Returns you back to the customize menu." },
                new ButtonInfo { buttonText = "First Color", method = () => CMTBackgroundFirst(), isTogglable = false, toolTip = "Change the color of the first color of the background." },
                new ButtonInfo { buttonText = "Second Color", method = () => CMTBackgroundSecond(), isTogglable = false, toolTip = "Change the color of the second color of the background." },
            };

            Buttons.buttons[Buttons.GetCategory("Temporary Category")] = buttons.ToArray();
            Buttons.CurrentCategoryName = "Temporary Category";
        }

        private static List<ButtonInfo> CreateColorButtons(Func<Color> getColor, Action<Color> setColor, string tooltipTarget)
        {
            ButtonInfo previewButton = new ButtonInfo
            {
                buttonText = "PreviewLabel",
                overlapText = "<color=#" + ColorToHex(getColor()) + ">Preview</color>",
                label = true
            };

            void RefreshPreview()
            {
                previewButton.overlapText = "<color=#" + ColorToHex(getColor()) + ">Preview</color>";
            }

            void ApplyChannel(int channel, int v)
            {
                if (!Buttons.GetIndex("Custom Menu Theme").enabled)
                    return;

                Color c = getColor();
                Color updated = channel switch
                {
                    0 => new Color(v / 10f, c.g, c.b),
                    1 => new Color(c.r, v / 10f, c.b),
                    _ => new Color(c.r, c.g, v / 10f),
                };
                setColor(updated);
                WriteCustomTheme();
            }

            ButtonInfo redButton = ButtonHelper.CreateNumeric(
                "Red", 0, 10,
                (int)Math.Round(getColor().r * 10f),
                v => ApplyChannel(0, v),
                v => v.ToString(),
                $"Change the red of {tooltipTarget}.",
                onCycle: _ => RefreshPreview());

            ButtonInfo greenButton = ButtonHelper.CreateNumeric(
                "Green", 0, 10,
                (int)Math.Round(getColor().g * 10f),
                v => ApplyChannel(1, v),
                v => v.ToString(),
                $"Change the green of {tooltipTarget}.",
                onCycle: _ => RefreshPreview());

            ButtonInfo blueButton = ButtonHelper.CreateNumeric(
                "Blue", 0, 10,
                (int)Math.Round(getColor().b * 10f),
                v => ApplyChannel(2, v),
                v => v.ToString(),
                $"Change the blue of {tooltipTarget}.",
                onCycle: _ => RefreshPreview());

            return new List<ButtonInfo> { redButton, greenButton, blueButton, previewButton };
        }
        public static void CMTBackgroundFirst()
        {
            List<ButtonInfo> buttons = new List<ButtonInfo> {
                new ButtonInfo { buttonText = "Exit First Color", method = () => CMTBackground(), isTogglable = false, toolTip = "Returns you back to the background menu." },
            };
            buttons.AddRange(CreateColorButtons(
                () => backgroundColor.GetColor(0),
                c => backgroundColor.SetColor(0, c),
                "the first color of the background"));

            Buttons.buttons[Buttons.GetCategory("Temporary Category")] = buttons.ToArray();
            Buttons.CurrentCategoryName = "Temporary Category";
        }

        public static void CMTBackgroundSecond()
        {
            List<ButtonInfo> buttons = new List<ButtonInfo> {
                new ButtonInfo { buttonText = "Exit Second Color", method = () => CMTBackground(), isTogglable = false, toolTip = "Returns you back to the background menu." },
            };
            buttons.AddRange(CreateColorButtons(
                () => backgroundColor.GetColor(1),
                c => backgroundColor.SetColor(1, c),
                "the second color of the background"));

            Buttons.buttons[Buttons.GetCategory("Temporary Category")] = buttons.ToArray();
            Buttons.CurrentCategoryName = "Temporary Category";
        }

        public static void CMTButtonEnabledFirst()
        {
            List<ButtonInfo> buttons = new List<ButtonInfo> {
                new ButtonInfo { buttonText = "Exit First Color", method = () => CMTButtonEnabled(), isTogglable = false, toolTip = "Returns you back to the enabled button menu." },
            };
            buttons.AddRange(CreateColorButtons(
                () => buttonColors[1].GetColor(0),
                c => buttonColors[1].SetColor(0, c),
                "the first color of the enabled button color"));

            Buttons.buttons[Buttons.GetCategory("Temporary Category")] = buttons.ToArray();
            Buttons.CurrentCategoryName = "Temporary Category";
        }

        public static void CMTButtonEnabledSecond()
        {
            List<ButtonInfo> buttons = new List<ButtonInfo> {
                new ButtonInfo { buttonText = "Exit Second Color", method = () => CMTButtonEnabled(), isTogglable = false, toolTip = "Returns you back to the enabled button menu." },
            };
            buttons.AddRange(CreateColorButtons(
                () => buttonColors[1].GetColor(1),
                c => buttonColors[1].SetColor(1, c),
                "the second color of the enabled button color"));

            Buttons.buttons[Buttons.GetCategory("Temporary Category")] = buttons.ToArray();
            Buttons.CurrentCategoryName = "Temporary Category";
        }

        public static void CMTButtonDisabledFirst()
        {
            List<ButtonInfo> buttons = new List<ButtonInfo> {
                new ButtonInfo { buttonText = "Exit First Color", method = () => CMTButtonDisabled(), isTogglable = false, toolTip = "Returns you back to the disabled button menu." },
            };
            buttons.AddRange(CreateColorButtons(
                () => buttonColors[0].GetColor(0),
                c => buttonColors[0].SetColor(0, c),
                "the first color of the disabled button color"));

            Buttons.buttons[Buttons.GetCategory("Temporary Category")] = buttons.ToArray();
            Buttons.CurrentCategoryName = "Temporary Category";
        }

        public static void CMTButtonDisabledSecond()
        {
            List<ButtonInfo> buttons = new List<ButtonInfo> {
                new ButtonInfo { buttonText = "Exit Second Color", method = CMTButtonDisabled, isTogglable = false, toolTip = "Returns you back to the disabled button menu." },
            };
            buttons.AddRange(CreateColorButtons(
                () => buttonColors[0].GetColor(1),
                c => buttonColors[0].SetColor(1, c),
                "the second color of the disabled button color"));

            Buttons.buttons[Buttons.GetCategory("Temporary Category")] = buttons.ToArray();
            Buttons.CurrentCategoryName = "Temporary Category";
        }

        public static void CMTTextTitle()
        {
            List<ButtonInfo> buttons = new List<ButtonInfo> {
                new ButtonInfo { buttonText = "Exit Title", method = CMTText, isTogglable = false, toolTip = "Returns you back to the text menu." },
            };
            buttons.AddRange(CreateColorButtons(
                () => textColors[0].GetColor(0),
                c => textColors[0].SetColors(c),
                "the title color"));

            Buttons.buttons[Buttons.GetCategory("Temporary Category")] = buttons.ToArray();
            Buttons.CurrentCategoryName = "Temporary Category";
        }

        public static void CMTTextEnabled()
        {
            List<ButtonInfo> buttons = new List<ButtonInfo> {
                new ButtonInfo { buttonText = "Exit Second Color", method = () => CMTText(), isTogglable = false, toolTip = "Returns you back to the text menu." },
            };
            buttons.AddRange(CreateColorButtons(
                () => textColors[2].GetColor(0),
                c => textColors[2].SetColors(c),
                "the enabled text color"));

            Buttons.buttons[Buttons.GetCategory("Temporary Category")] = buttons.ToArray();
            Buttons.CurrentCategoryName = "Temporary Category";
        }

        public static void CMTTextDisabled()
        {
            List<ButtonInfo> buttons = new List<ButtonInfo> {
                new ButtonInfo { buttonText = "Exit Second Color", method = () => CMTText(), isTogglable = false, toolTip = "Returns you back to the text menu." },
            };
            buttons.AddRange(CreateColorButtons(
                () => textColors[1].GetColor(0),
                c => textColors[1].SetColors(c),
                "the disabled text color"));

            Buttons.buttons[Buttons.GetCategory("Temporary Category")] = buttons.ToArray();
            Buttons.CurrentCategoryName = "Temporary Category";
        }

        public static void CMTButton()
        {
            List<ButtonInfo> buttons = new List<ButtonInfo> {
                new ButtonInfo { buttonText = "Exit Buttons", method = CustomMenuThemePage, isTogglable = false, toolTip = "Returns you back to the customize menu." },
                new ButtonInfo { buttonText = "Enabled", method = CMTButtonEnabled, isTogglable = false, toolTip = "Choose what type of button color to modify." },
                new ButtonInfo { buttonText = "Disabled", method = CMTButtonDisabled, isTogglable = false, toolTip = "Change the color of the second color of the background." },
            };

            Buttons.buttons[Buttons.GetCategory("Temporary Category")] = buttons.ToArray();
            Buttons.CurrentCategoryName = "Temporary Category";
        }

        public static void CMTButtonEnabled()
        {
            List<ButtonInfo> buttons = new List<ButtonInfo> {
                new ButtonInfo { buttonText = "Exit Enabled", method = CMTButton, isTogglable = false, toolTip = "Returns you back to the customize menu." },
                new ButtonInfo { buttonText = "First Color", method = CMTButtonEnabledFirst, isTogglable = false, toolTip = "Change the color of the first color of the enabled button color." },
                new ButtonInfo { buttonText = "Second Color", method = () => CMTButtonEnabledSecond(), isTogglable = false, toolTip = "Change the color of the second color of the enabled button color." },
            };

            Buttons.buttons[Buttons.GetCategory("Temporary Category")] = buttons.ToArray();
            Buttons.CurrentCategoryName = "Temporary Category";
        }

        public static void CMTButtonDisabled()
        {
            List<ButtonInfo> buttons = new List<ButtonInfo> {
                new ButtonInfo { buttonText = "Exit Enabled", method = () => CMTButton(), isTogglable = false, toolTip = "Returns you back to the customize menu." },
                new ButtonInfo { buttonText = "First Color", method = () => CMTButtonDisabledFirst(), isTogglable = false, toolTip = "Change the color of the first color of the disabled button color." },
                new ButtonInfo { buttonText = "Second Color", method = () => CMTButtonDisabledSecond(), isTogglable = false, toolTip = "Change the color of the second color of the disabled button color." },
            };

            Buttons.buttons[Buttons.GetCategory("Temporary Category")] = buttons.ToArray();
            Buttons.CurrentCategoryName = "Temporary Category";
        }

        public static void CMTText()
        {
            List<ButtonInfo> buttons = new List<ButtonInfo> {
                new ButtonInfo { buttonText = "Exit Text", method = CustomMenuThemePage, isTogglable = false, toolTip = "Returns you back to the customize menu." },
                new ButtonInfo { buttonText = "Title", method = CMTTextTitle, isTogglable = false, toolTip = "Change the color of the title." },
                new ButtonInfo { buttonText = "Enabled", method = CMTTextEnabled, isTogglable = false, toolTip = "Change the color of the enabled text." },
                new ButtonInfo { buttonText = "Disabled", method = CMTTextDisabled, isTogglable = false, toolTip = "Change the color of the disabled text." },
            };

            Buttons.buttons[Buttons.GetCategory("Temporary Category")] = buttons.ToArray();
            Buttons.CurrentCategoryName = "Temporary Category";
        }

        public static void ExitCustomMenuTheme()
        {
            pageNumber = previousPage;
            Buttons.CurrentCategoryName = "Menu Settings";
        }

        public static void ApplyTheme(Preferences.CustomThemeData theme)
        {
            backgroundColor.SetColor(0, theme.backgroundFirst.ToColor32());
            backgroundColor.SetColor(1, theme.backgroundSecond.ToColor32());

            buttonColors[0].SetColor(0, theme.buttonDisabledFirst.ToColor32());
            buttonColors[0].SetColor(1, theme.buttonDisabledSecond.ToColor32());
            buttonColors[1].SetColor(0, theme.buttonEnabledFirst.ToColor32());
            buttonColors[1].SetColor(1, theme.buttonEnabledSecond.ToColor32());

            textColors[0].SetColors(theme.textTitle.ToColor32());
            textColors[1].SetColors(theme.textDisabled.ToColor32());
            textColors[2].SetColors(theme.textEnabled.ToColor32());
        }

        public static void ReadCustomTheme()
        {
            var theme = Preferences.GetCustomTheme();
            if (theme != null)
                ApplyTheme(theme);
        }

        public static void ImportCustomTheme(Preferences.CustomThemeData theme)
        {
            ApplyTheme(theme);
            Preferences.SaveCustomTheme(theme);
        }

        public static Preferences.CustomThemeData ExportCustomTheme() => new Preferences.CustomThemeData
        {
            backgroundFirst = new Preferences.RgbColor(backgroundColor.GetColor(0)),
            backgroundSecond = new Preferences.RgbColor(backgroundColor.GetColor(1)),
            buttonDisabledFirst = new Preferences.RgbColor(buttonColors[0].GetColor(0)),
            buttonDisabledSecond = new Preferences.RgbColor(buttonColors[0].GetColor(1)),
            buttonEnabledFirst = new Preferences.RgbColor(buttonColors[1].GetColor(0)),
            buttonEnabledSecond = new Preferences.RgbColor(buttonColors[1].GetColor(1)),
            textTitle = new Preferences.RgbColor(textColors[0].GetColor(0)),
            textDisabled = new Preferences.RgbColor(textColors[1].GetColor(0)),
            textEnabled = new Preferences.RgbColor(textColors[2].GetColor(0)),
        };

        public static void WriteCustomTheme() =>
            Preferences.SaveCustomTheme(ExportCustomTheme());

        public static void CustomMenuTheme()
        {
            if (Preferences.GetCustomTheme() == null)
                WriteCustomTheme();

            ReadCustomTheme();
        }

        public static void FixTheme()
        {
            themeType--;
            Buttons.GetIndex("Change Menu Theme").cycleValue(true);
        }

        public static void CustomMenuBackground()
        {
            if (!File.Exists($"{PluginInfo.BaseDirectory}/CustomBackground.png"))
                LoadTextureFromURL($"{PluginInfo.ServerResourcePath}/Images/CustomBackground.png", "CustomBackground.png"); // Do not move outside of its path

            textureFileDirectory.Remove("CustomBackground.png");

            doCustomMenuBackground = true;
            customMenuBackgroundImage = LoadTextureFromFile("CustomBackground.png");
        }

        public static void FixMenuBackground()
        {
            customMenuBackgroundImage = null;
            doCustomMenuBackground = false;
        }

        public static void EnableWatermark()
        {
            bool enabled = Buttons.GetIndex("Custom Watermark").enabled;
            if (enabled)
            {
                if (!File.Exists($"{PluginInfo.BaseDirectory}/CustomWatermark.png"))
                    LoadTextureFromURL($"{PluginInfo.ServerResourcePath}/Images/CustomWatermark.png", "CustomWatermark.png"); // Do not move outside of its path

                textureFileDirectory.Remove("CustomWatermark.png");
                customWatermark = LoadTextureFromFile("CustomWatermark.png");
            }
            else
            {
                watermarkImage = new GameObject
                {
                    transform =
                    {
                        parent = canvasObj.transform
                    }
                }.AddComponent<Image>();

                if (watermarkMat == null)
                    watermarkMat = new Material(watermarkImage.material);

                watermarkImage.material = watermarkMat;
                watermarkImage.material.SetTexture("_MainTex", customWatermark ?? LoadTextureFromResource($"{PluginInfo.ClientResourcePath}.icon.png"));
            }
        }

        public static void CustomWatermark()
        {
            if (!File.Exists($"{PluginInfo.BaseDirectory}/CustomWatermark.png"))
                LoadTextureFromURL($"{PluginInfo.ServerResourcePath}/Images/CustomWatermark.png", "CustomWatermark.png"); // Do not move outside of its path

            textureFileDirectory.Remove("CustomWatermark.png");
            customWatermark = LoadTextureFromFile("CustomWatermark.png");
        }

        private static TMP_FontAsset chosenFont;
        public static void CustomFontType()
        {
            string filePath = $"{PluginInfo.BaseDirectory}/CustomFont.ttf";
            if (!File.Exists(filePath))
            {
                LogManager.Log("Downloading CustomFont.ttf");
                WebClient stream = new WebClient();
                stream.DownloadFile($"{PluginInfo.ServerResourcePath}/Fonts/LiberationSans.ttf", filePath);
            }

            chosenFont = TMP_FontAsset.CreateFontAsset(new Font($"{FileUtilities.GetGamePath()}/{filePath}"));
            PersistCustomFont();
        }

        public static void PersistCustomFont()
        {
            if (activeFont != chosenFont)
                activeFont = chosenFont;
        }

        public static void DisableCustomFont()
        {
            fontCycle--;
            Buttons.GetIndex("Change Font Type").cycleValue(true);
        }

        public static void ApplyPageType(int index) { pageButtonType = index; buttonOffset = index == 2 ? 2 : 0; }
        public static void ApplyPageSize(int index) => _pageSize = index;
        public static void ApplyCharacterDistance(int index) => characterDistance = index;
        public static void ApplyArrowType(int index) => arrowType = index;

        public static void ApplyFontType(int index)
        {
            fontCycle = index;
            switch (index)
            {
                case 0:
                    activeFont = AgencyFB;
                    return;
                case 1:
                    activeFont = FreeSans;
                    return;
                case 2:
                    activeFont = DejaVuSans;
                    return;
                case 3:
                    activeFont = Utopium;
                    return;
                case 4:
                    activeFont = ComicSans;
                    return;
                case 5:
                    activeFont = CascadiaMono;
                    return;
                case 6:
                    activeFont = Candara;
                    return;
                case 7:
                    activeFont = MSGothic;
                    return;
                case 8:
                    activeFont = Anton;
                    return;
                case 9:
                    activeFont = SimSun;
                    return;
                case 10:
                    activeFont = Minecraft;
                    return;
                case 11:
                    activeFont = Terminal;
                    return;
                case 12:
                    activeFont = OpenDyslexic;
                    return;
                case 13:
                    activeFont = Taiko;
                    return;
                case 14:
                    activeFont = LiberationSans;
                    return;
            }
        }

        public static float fontTime;
        public static void ChangeFontRapid()
        {
            if (Time.time > fontTime)
            {
                Buttons.GetIndex("Change Font Type").cycleValue(true);
                fontTime = Time.time + 0.4f;

                ReloadMenu();
            }
        }

        public static int fontStyleType = 2;
        public static void ApplyFontStyleType(int index)
        {
            fontStyleType = index;
            activeFontStyle = index switch
            {
                0 => FontStyles.Normal,
                1 => FontStyles.Bold,
                2 => FontStyles.Italic,
                3 => FontStyles.Bold | FontStyles.Italic,
                _ => FontStyles.Normal
            };
        }


        public static readonly string[] InputColorNames = {
            "Red", "Orange", "Yellow", "Green", "Blue", "Cyan",
            "Purple", "Pink", "White", "Grey", "Black", "Rose"
        };
        public static readonly string[] RealInputColors = {
            "red", "#ff8000", "yellow", "green", "blue", "#00FFFF",
            "purple", "#FF00FF", "white", "grey", "black", "#ff005d"
        };
        public static int inputTextColorInt = 3;
        public static void ApplyInputTextColor(int index) { inputTextColorInt = index; inputTextColor = RealInputColors[index]; }
        public static void ApplyPCUI(int index) => pcbg = index;
        public static void ApplyJoystickMenuPosition(int index) => joystickMenuPosition = index;
        public static void ApplyNotificationTime(int index) => notificationDecayTime = index * 1000;

        public static void ApplyNotificationSound(string soundName)
        {
            SoundManager.DefaultSounds["Notification"] = soundName;
            Buttons.GetIndex("Change Notification Sound").overlapText =
                $"Change Notification Sound <color=grey>[</color><color=green>{soundName}</color><color=grey>]</color>";
        }
        public static void ChangeNotificationSound(bool positive = true, bool fromMenu = false)
        {
            var notificationKeys = SoundManager.Sounds["Notifications"].Keys.ToArray();
            string current = SoundManager.DefaultSounds["Notification"];

            int index = Array.IndexOf(notificationKeys, current);
            if (index < 0) index = 0;
            index = ButtonHelper.Wrap(index, 0, notificationKeys.Length - 1, positive);

            string newSound = notificationKeys[index];
            ApplyNotificationSound(newSound);
            Buttons.GetIndex("Change Notification Sound").value = newSound;

            if (!fromMenu) return;

            var src = audioManager?.GetComponent<AudioSource>();
            src?.Stop();

            SoundManager.Play(SoundManager.DefaultSounds["Notification"]);
        }

        public static readonly string[] NarratorNames = {
            "Default", "Kimberly", "Brian", "Matthew", "Joey", "Justin", "Cristiano",
            "Giorgio", "Ewa", "TikTok", "Grandma", "Trickster", "Elf", "Ghostface",
            "Zombie", "Narrator", "Pirate", "Song", "TikTok Joey", "Gingerbread Man",
            "Chris", "Thanksgiving", "Santa", "Google US", "Google UK", "Dog",
            "Jerkface", "Robot", "Vlad", "Obama"/*, "Mommy ASMR"*/
        };
        public static void ApplyNarrationVoice(int index)
        {
            narratorIndex = index;
            narratorName = NarratorNames[index];

            if (krec != null && krec.IsRunning && Time.time > dRestartTime)
            {
                DictationRestart();
                dRestartTime = Time.time + 1f;
            }
        }


        public static void KickToSpecificRoom()
        {
            if (Time.time < timeMenuStarted + 5f)
                return;

            PromptText("What would you like the room code to be?", () => Overpowered.specificRoom = keyboardInput.ToUpper(), () => Toggle("Kick to Specific Room"), "Done", "Cancel");
        }
        public static readonly Vector3[] PointerPositions = {
            new Vector3(0f, -0.1f, 0f),
            new Vector3(0f, -0.1f, -0.15f),
            new Vector3(0f, 0.1f, -0.05f),
            new Vector3(0f, 0.0666f, 0.1f)
        };
        public static void ApplyPointerPosition(int index)
        {
            pointerIndex = index;
            pointerOffset = PointerPositions[index];
            try { reference.transform.localPosition = pointerOffset; } catch { }
        }


        public static readonly string[] GunVariationNames = {
            "Default", "Lightning", "Wavy", "Blocky", "Zigzag", "Spring", "Bouncy", "Audio", "Bezier", "Rope"
        };
        public static void ApplyGunVariation(int index) => gunVariation = index;

        public static readonly string[] GunDirectionNames = { "Default", "Legacy", "Laser", "Finger", "Face" };
        public static void ApplyGunDirection(int index) => GunDirection = index;

        private static int gunLineQualityIndex = 2;
        public static readonly string[] GunQualityNames = { "Potato", "Low", "Normal", "High", "Extreme" };
        public static readonly int[] GunQualityValues = { 10, 25, 50, 100, 250 };
        public static void ApplyGunLineQuality(int index) { gunLineQualityIndex = index; GunLineQuality = GunQualityValues[index]; }

        public static void FreezePlayerInMenu()
        {
            if (physicalMenu ? isMenuButtonHeld : menu != null)
            {
                if (closePosition == Vector3.zero)
                    closePosition = GorillaTagger.Instance.rigidbody.transform.position;
                else
                    GorillaTagger.Instance.rigidbody.transform.position = closePosition;
                GorillaTagger.Instance.rigidbody.linearVelocity = new Vector3(0f, 0f, 0f);
            }
            else
                closePosition = Vector3.zero;
        }

        public static bool currentmentalstate;
        public static void FreezeRigInMenu()
        {
            if (menu != null)
            {
                if (!currentmentalstate)
                {
                    currentmentalstate = true;
                    VRRig.LocalRig.enabled = false;
                }
            }
            else
            {
                if (currentmentalstate)
                {
                    currentmentalstate = false;
                    VRRig.LocalRig.enabled = true;
                }
            }
        }

        public static void DisorganizeMenu()
        {
            if (!disorganized)
            {
                disorganized = true;
                foreach (ButtonInfo[] buttonArray in Buttons.buttons)
                {
                    if (buttonArray.Length > 0)
                    {
                        for (int i = 0; i < buttonArray.Length; i++)
                            Buttons.buttons[Buttons.GetCategory("Main")] = Buttons.buttons[Buttons.GetCategory("Main")].Concat(new[] { buttonArray[i] }).ToArray();

                        Array.Clear(buttonArray, 0, buttonArray.Length);
                    }
                }
            }
        }

        public static void AnnoyingModeOff()
        {
            annoyingMode = false;
            themeType--;
            Buttons.GetIndex("Change Menu Theme").cycleValue(true);
        }

        public static void DisablePageButtons()
        {
            if (Buttons.GetIndex("Joystick Menu").enabled)
            {
                disablePageButtons = true;
            }
            else
            {
                Buttons.GetIndex("Disable Page Buttons").SetEnabled(false);
                ReloadMenu();
                NotificationManager.SendNotification("<color=grey>[</color><color=red>DISABLE</color><color=grey>]</color> Disable Page Buttons can only be used when using Joystick Menu.");
            }
        }

        public static void CustomMenuName()
        {
            if (Time.time > timeMenuStarted + 10f)
            {
                Prompt("Would you like to set a custom menu name right now?", () =>
                {
                    PromptSingleText("What would you like to set the menu name to?", () =>
                    {
                        File.WriteAllText($"{PluginInfo.BaseDirectory}/Israel_CustomMenuName.txt", keyboardInput);
                        Apply();
                        PromptSingle("You can always change this again by re-enabling the mod or changing it in the SeralythMenu folder! (located in the Gorilla Tag installation folder)");
                    });
                }, Apply);

                static void Apply()
                {
                    doCustomName = true;
                    if (!File.Exists($"{PluginInfo.BaseDirectory}/Israel_CustomMenuName.txt"))
                        File.WriteAllText($"{PluginInfo.BaseDirectory}/Israel_CustomMenuName.txt", "Your Text Here");
                    customMenuName = File.ReadAllText($"{PluginInfo.BaseDirectory}/Israel_CustomMenuName.txt");
                }
                Apply();
            }
        }

        private static bool lastFocused;
        public static void CheckFocus()
        {
            if (!Application.isFocused && lastFocused && Time.time > timeMenuStarted + 5f)
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not focused on Gorilla Tag. Voice transcription mods will not function. Please focus/click on the game.");

            lastFocused = Application.isFocused;
            if (Application.isFocused && lastFocused)
                DictationRestart();
        }

        private static KeywordRecognizer mainPhrases;
        private static KeywordRecognizer modPhrases;
        private static string[] keyWords = { "jarvis", "seralyth", "seralith", "sarolith", "siri", "google", "alexa", "dummy", "computer", "stinky", "silly", "stupid", "console", "go go gadget", "monika", "wikipedia", "gideon", "a i", "ai", "a.i", "chat gpt", "chatgpt", "grok", "grock", "groq", "garmin" };
        private static readonly string[] cancelKeywords = { "nevermind", "cancel", "never mind", "stop", "i hate you", "die" };
        public static void VoiceRecognitionOn()
        {
            if (!File.Exists($"{PluginInfo.BaseDirectory}/Israel_Keywords.txt"))
                File.WriteAllLines($"{PluginInfo.BaseDirectory}/Israel_Keywords.txt", keyWords);
            keyWords = File.ReadAllLines($"{PluginInfo.BaseDirectory}/Israel_Keywords.txt");
            mainPhrases = new KeywordRecognizer(keyWords);
            mainPhrases.OnPhraseRecognized += ModRecognition;
            mainPhrases.Start();
        }

        private static Coroutine timeoutCoroutine;
        public static void ModRecognition(PhraseRecognizedEventArgs args)
        {
            mainPhrases.Stop();

            if (!Buttons.GetIndex("Chain Voice Commands").enabled)
                timeoutCoroutine = CoroutineManager.instance.StartCoroutine(Timeout(string.Empty));

            List<string> rawbuttonnames = cancelKeywords.ToList();

            foreach (ButtonInfo[] buttonlist in Buttons.buttons)
            {
                foreach (ButtonInfo v in buttonlist)
                {
                    string buttonName = v.overlapText ?? v.buttonText;

                    if (buttonName.Contains(" <color"))
                        buttonName = buttonName.Split(" <color")[0];

                    rawbuttonnames.Add(buttonName);
                }
            }


            modPhrases = new KeywordRecognizer(rawbuttonnames.ToArray());
            modPhrases.OnPhraseRecognized += ExecuteVoiceCommand;
            modPhrases.Start();

            if (dynamicSounds)
                LoadSoundFromURL($"{PluginInfo.ServerResourcePath}/Audio/Menu/select.ogg", "Audio/Menu/select.ogg", clip => DictationPlay(clip, buttonClickVolume / 10f));

            NotificationManager.SendNotification("<color=grey>[</color><color=purple>VOICE</color><color=grey>]</color> Listening...", 3000);
        }

        public static void ExecuteVoiceCommand(PhraseRecognizedEventArgs args)
        {
            if (!Buttons.GetIndex("Chain Voice Commands").enabled)
            {
                modPhrases.Stop();
                mainPhrases.Start();
                CoroutineManager.instance.StopCoroutine(timeoutCoroutine);
            }

            if (cancelKeywords.Contains(args.text))
            {
                CancelModRecognition(args.text);
                return;
            }

            string modTarget = null;
            bool exactMatch = false;

            foreach (ButtonInfo[] buttonlist in Buttons.buttons)
            {
                if (exactMatch)
                    break;

                foreach (ButtonInfo v in buttonlist)
                {
                    if (exactMatch)
                        break;

                    string buttonName = v.overlapText ?? v.buttonText;

                    if (buttonName.Contains(" <color"))
                        buttonName = buttonName.Split(" <color")[0];

                    if (args.text.ToLower() == buttonName.ToLower())
                    {
                        modTarget = v.buttonText;
                        exactMatch = true;
                    }
                    else
                    {
                        if (args.text.Contains(buttonName.ToLower()))
                            modTarget = v.buttonText;
                    }
                }
            }

            if (modTarget != null)
            {
                ButtonInfo mod = Buttons.GetIndex(modTarget);
                NotificationManager.SendNotification("<color=grey>[</color><color=" + (mod.enabled ? "red" : "green") + ">VOICE</color><color=grey>]</color> " + (mod.enabled ? "Disabling " : "Enabling ") + (mod.overlapText ?? mod.buttonText) + "...", 3000);
                if (dynamicSounds)
                    LoadSoundFromURL($"{PluginInfo.ServerResourcePath}/Audio/Menu/confirm.ogg", "Audio/Menu/confirm.ogg", clip => DictationPlay(clip, buttonClickVolume / 10f));

#if LEGAL || LEGAL_DEBUG
                if (!mod.legal)
                    return;
#endif
                Toggle(modTarget, true, true);
            }
            else
            {
                NotificationManager.SendNotification("<color=grey>[</color><color=red>VOICE</color><color=grey>]</color> No command found (" + args.text + ").", 3000);
                if (dynamicSounds)
                    LoadSoundFromURL($"{PluginInfo.ServerResourcePath}/Audio/Menu/close.ogg", "Audio/Menu/close.ogg", clip => DictationPlay(clip, buttonClickVolume / 10f));
            }
        }

        public static IEnumerator Timeout(string text)
        {
            yield return new WaitForSeconds(10f);
            CancelModRecognition(text);
        }

        public static void CancelModRecognition(string text)
        {
            modPhrases.Stop();
            mainPhrases.Start();
            try
            {
                CoroutineManager.instance.StopCoroutine(timeoutCoroutine);
            }
            catch { }

            NotificationManager.SendNotification($"<color=grey>[</color><color=red>VOICE</color><color=grey>]</color> {(text == "i hate you" ? "I hate you too." : "Cancelling...")}", 3000);
            if (dynamicSounds)
                LoadSoundFromURL($"{PluginInfo.ServerResourcePath}/Audio/Menu/close.ogg", "Audio/Menu/close.ogg", clip => DictationPlay(clip, buttonClickVolume / 10f));
        }

        public static void VoiceRecognitionOff()
        {
            mainPhrases?.Dispose();
            mainPhrases?.Stop();
            modPhrases?.Dispose();
            modPhrases?.Stop();
            mainPhrases = null;
            modPhrases = null;
            PhraseRecognitionSystem.Shutdown();
        }

        public static DictationRecognizer drec;
        public static KeywordRecognizer krec;
        public static bool debugDictation;
        public static bool restartOnFocus;
        public static float dRestartTime;

        public static IEnumerator DictationOn()
        {
            ButtonInfo mod = Buttons.GetIndex("AI Assistant");

            if (Application.platform == RuntimePlatform.WindowsPlayer && Environment.OSVersion.Version.Major < 10)
                PromptSingle("Your version of Windows is too old for this mod to run.", () => mod.SetEnabled(false));
            else if (Application.platform != RuntimePlatform.WindowsPlayer)
                PromptSingle("You must be on Windows 10 or greater for this mod to run.", () => mod.SetEnabled(false));


            ButtonInfo vc = Buttons.GetIndex("Voice Commands");
            if (vc.enabled)
                Prompt("You currently have Voice Commands enabled. These mods may overlap eachother. Would you like to disable it?", () => vc.SetEnabled(false), () => mod.SetEnabled(false));
            else if (PhraseRecognitionSystem.Status != SpeechSystemStatus.Stopped)
                PromptSingle("You can not use AI Assistant while you have another voice-related mod on.", () => mod.SetEnabled(false), "Ok");

            if (!File.Exists($"{PluginInfo.BaseDirectory}/Israel_Keywords.txt"))
                File.WriteAllLines($"{PluginInfo.BaseDirectory}/Israel_Keywords.txt", keyWords);
            keyWords = File.ReadAllLines($"{PluginInfo.BaseDirectory}/Israel_Keywords.txt");

            while (PhraseRecognitionSystem.Status != SpeechSystemStatus.Stopped)
                yield return null;

            string[] kw = keyWords;
            if (narratorName == "Mommy ASMR")
                kw = kw.Concat(new[] { "mommy", "momma" }).ToArray();

            krec = new KeywordRecognizer(kw);

            krec.OnPhraseRecognized += (args) => CoroutineManager.instance.StartCoroutine(DictationRecognizer());
            krec.Start();
            yield break;
        }

        public static IEnumerator DictationRecognizer()
        {
            ButtonInfo mod = Buttons.GetIndex("AI Assistant");

            PhraseRecognitionSystem.Shutdown();
            while (PhraseRecognitionSystem.Status != SpeechSystemStatus.Stopped)
                yield return null;

            switch (narratorName)
            {
                case "Mommy ASMR":
                    LoadSoundFromURL($"{PluginInfo.ServerResourcePath}/Audio/TTS/yes_sweetheart.ogg", "Audio/TTS/yes_sweetheart.ogg", clip => DictationPlay(clip, buttonClickVolume / 10f));
                    NotificationManager.SendNotification("<color=grey>[</color><color=#ffb6c1>MOMMY</color><color=grey>]</color> Yes, sweetheart?", 3000);
                    break;
                default:
                    LoadSoundFromURL($"{PluginInfo.ServerResourcePath}/Audio/Menu/select.ogg", "Audio/Menu/select.ogg", clip => DictationPlay(clip, buttonClickVolume / 10f));
                    NotificationManager.SendNotification("<color=grey>[</color><color=purple>VOICE</color><color=grey>]</color> Listening...", 3000);
                    break;
            }

            if (debugDictation)
                LogManager.Log("Dictation listening");

            drec = new DictationRecognizer();
            drec.DictationResult += (text, confidence) =>
            {
                if (AIManager.generating)
                    return;

                if (debugDictation)
                    LogManager.Log($"Dictation result: {text}");
                if (cancelKeywords.Contains(text.ToLower()))
                {
                    if (dynamicSounds)
                        LoadSoundFromURL($"{PluginInfo.ServerResourcePath}/Audio/Menu/close.ogg", "Audio/Menu/close.ogg", clip => DictationPlay(clip, buttonClickVolume / 10f));

                    NotificationManager.SendNotification($"<color=grey>[</color><color=red>AI</color><color=grey>]</color> {(text.ToLower() == "i hate you" ? "I hate you too." : "Cancelling...")}", 3000);
                    CoroutineManager.instance.StartCoroutine(DictationRestart());
                    return;
                }

                switch (narratorName)
                {
                    case "Mommy ASMR":
                        NotificationManager.SendNotification($"<color=grey>[</color><color=#ffb6c1>MOMMY</color><color=grey>]</color> Let me get that for you..");
                        break;
                    default:
                        NotificationManager.SendNotification($"<color=grey>[</color><color=blue>AI</color><color=grey>]</color> Generating response..");
                        break;

                }


                CoroutineManager.instance.StartCoroutine(AIManager.AskAI(text));
                return;

            };

            drec.DictationComplete += (completionCause) =>
            {
                if (AIManager.generating)
                    return;
                if (debugDictation)
                    LogManager.Log($"completion cause: {completionCause}");
                if (completionCause.ToString() == "TimeoutExceeded")
                {
                    if (dynamicSounds)
                        LoadSoundFromURL($"{PluginInfo.ServerResourcePath}/Audio/Menu/close.ogg", "Audio/Menu/close.ogg", clip => DictationPlay(clip, buttonClickVolume / 10f));
                    NotificationManager.SendNotification($"<color=grey>[</color><color=red>AI</color><color=grey>]</color> Cancelling...", 3000);
                }
            };

            drec.DictationError += (error, hresult) =>
            {
                if (debugDictation)
                    LogManager.LogError($"Dictation error: {error}");
                if (error.Contains("Dictation support is not enabled on this device"))
                {
                    DictationOff();

                    NotificationManager.SendNotification($"<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> Online Speech Recognition is not enabled on this device. Either open the menu to enable it, or check your internet connection.", 3000);
                    Prompt("Online Speech Recognition is not enabled on your device. Would you like to open the Settings page to enable it?", () => { Process.Start("ms-settings:privacy-speech"); PromptSingle("Once you enable Online Speech Recognition, turn this mod back on!", () => mod.SetEnabled(false), "Ok"); }, () => PromptSingle("You will not be able to use this mod until you enable Online Speech Recognition.", () => mod.SetEnabled(false), "Ok"));
                }
            };

            drec.DictationHypothesis += (text) =>
            {
                if (AIManager.generating)
                    return;
                if (debugDictation)
                    LogManager.Log($"Hypothesis: {text}");

                NotificationManager.ClearAllNotifications();
                NotificationManager.SendNotification($"<color=grey>[</color><color=green>VOICE</color><color=grey>]</color> {text}");
            };

            drec?.Start();
            yield break;
        }

        public static IEnumerator DictationRestart()
        {
            DictationOff();
            while (PhraseRecognitionSystem.Status != SpeechSystemStatus.Stopped)
                yield return null;
            CoroutineManager.instance.StartCoroutine(DictationOn());
            yield break;
        }
        public static void DictationOff()
        {
            drec?.Dispose();
            drec?.Stop();
            drec = null;
            PhraseRecognitionSystem.Shutdown();
        }

        public static void DictationPlay(AudioClip clip, float volume)
        {
            bool enabled = Buttons.GetIndex("Global Dynamic Sounds").enabled;
            switch (enabled)
            {
                case true:
                    Sound.PlayAudio(clip);
                    break;
                case false:
                    Play2DAudio(clip, volume);
                    break;
            }
        }

        private static LineRenderer clickGuiLine;
        private static bool lastTriggerClick;

        private static EventSystem eventSystem;
        private static PointerEventData pointerData;
        private static readonly List<RaycastResult> uiResults = new List<RaycastResult>();
        private static GameObject currentUI;

        private static GameObject pressedUI;
        private static GameObject draggedUI;
        private static Vector2 lastPointerPos;
        private static Canvas canvas;

        private static bool isDragging;

        public static void ReloadOnCategoryChange() =>
            ReloadMenu();

        public static void EnableClickGUI()
        {
            clickGUI = true;
            ReloadMenu();

            Buttons.OnCategoryChanged += ReloadOnCategoryChange;
        }

        public static void DisableClickGUI()
        {
            clickGUI = false;
            Buttons.OnCategoryChanged -= ReloadOnCategoryChange;

            if (clickGuiLine != null)
            {
                Object.Destroy(clickGuiLine.gameObject);
                clickGuiLine = null;
            }
        }

        public static void InitializeClickGUI()
        {
            canvas = menu.transform.Find("Canvas").GetComponent<Canvas>();

            if (!XRSettings.isDeviceActive)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 1;

                canvas.gameObject.transform.Find("Main").AddComponent<UIDragWindow>();
            }

            Transform canvasTransform = canvas.gameObject.transform;
            canvasTransform.Find("Main").AddComponent<UIColorChanger>().colors = backgroundColor;

            ExtGradient sidebarColor = buttonColors[1].Clone();
            for (int i = 0; i < sidebarColor.colors.Length; i++)
            {
                GradientColorKey colorKey = sidebarColor.colors[i];
                sidebarColor.colors[i] = new GradientColorKey { time = colorKey.time, color = DarkenColor(colorKey.color, 0.35f) };
            }

            canvasTransform.Find("Main/Sidebar").AddComponent<UIColorChanger>().colors = sidebarColor;
            canvasTransform.Find("Main/Separator").AddComponent<UIColorChanger>().colors = buttonColors[1];

            canvasTransform.Find("Main/Sidebar/Watermark").localRotation = Quaternion.Euler(0f, 0f, rockWatermark ? Mathf.Sin(Time.time * 2f) * 10f : 0f);

            List<MaskableGraphic> toRecolor = new List<MaskableGraphic>();
            foreach (string partName in new[]
            {
                "Main/Sidebar/Title",
                "Main/Sidebar/Watermark",
                "Main/Sidebar/Settings",
                "Main/Sidebar/Players",
                "Main/Sidebar/Friends",
                "Main/Sidebar/Scroll View/Scrollbar Vertical/Sliding Area/Handle"
            })
                toRecolor.Add(canvasTransform.Find(partName).GetComponent<MaskableGraphic>());

            Transform sidebarTransform = canvasTransform.Find("Main/Sidebar");
            foreach (string buttonName in new[]
            {
                "Settings", "Players", "Friends"
            })
                sidebarTransform.Find(buttonName).GetComponent<Button>().onClick.AddListener(() =>
                {
                    Toggle(buttonName);
                    SoundManager.Play(SoundManager.DefaultSounds["Button"]);
                });

            var selection = canvasTransform.Find("Main/Sidebar/Scroll View/Viewport/Content/Home/Selection");
            selection.AddComponent<UIColorChanger>().colors = buttonColors[1];

            bool movedSelection = false;

            string[] ignoreButtons = {
                "Join Discord",
                "Settings",
                "Friends",
                "Players",
                "Favorite Mods",
                "Enabled Mods",
                "Room Mods",
                "Important Mods",
                "Safety Mods",
                "Movement Mods",
                "Advantage Mods",
                "Visual Mods",
                "Fun Mods",
                "Sound Mods",
                "Projectile Mods",
                "Master Mods",
                "Overpowered Mods",
                "Experimental Mods",
                "Detected Mods",
                "Achievements",
                "Credits"
            };

            GameObject otherBase = canvasTransform.Find("Main/Sidebar/Scroll View/Viewport/Content/Other").gameObject;
            foreach (ButtonInfo button in Buttons.buttons[Buttons.GetCategory("Main")])
            {
                if (!ignoreButtons.Contains(button.buttonText))
                {
                    GameObject otherButton = Object.Instantiate(otherBase, canvasTransform.Find("Main/Sidebar/Scroll View/Viewport/Content"), false);
                    otherButton.SetActive(true);
                    otherButton.name = button.buttonText;
                    otherButton.transform.Find("Title").GetComponent<TextMeshProUGUI>().SafeSetText(button.buttonText);
                }
            }

            foreach (GameObject tab in canvasTransform.Find("Main/Sidebar/Scroll View/Viewport/Content").Children())
            {
                if (!tab.activeSelf)
                    continue;

                toRecolor.Add(tab.transform.Find("Title").GetComponent<MaskableGraphic>());
                toRecolor.Add(tab.transform.Find("Image").GetComponent<MaskableGraphic>());

                tab.AddComponent<UIColorChanger>().colors = buttonColors[0];
                tab.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Toggle(Buttons.buttons[Buttons.GetCategory("Main")].Where(button => button.buttonText.StartsWith(tab.name)).FirstOrDefault() ?? Buttons.GetIndex("Exit Settings"));
                    SoundManager.Play(SoundManager.DefaultSounds["Button"]);
                });

                if (Buttons.CurrentCategoryName.StartsWith(tab.name == "Home" ? "Main" : tab.name))
                {
                    movedSelection = true;
                    selection.SetParent(tab.transform, false);
                }
                else
                {
                    tab.transform.Find("Title").GetComponent<RectTransform>().localPosition += Vector3.left * 10f;
                    tab.transform.Find("Image").GetComponent<RectTransform>().localPosition += Vector3.left * 10f;
                }
            }

            if (!movedSelection)
                selection.gameObject.SetActive(false);

            GameObject buttonTemplate = canvasTransform.Find("Main/Button").gameObject;
            void AddButton(Transform parent, ButtonInfo info)
            {
                static void UpdateButton(GameObject button, ButtonInfo info)
                {
                    Transform transform = button.transform;
                    string buttonText = info.overlapText ?? info.buttonText;

                    if (inputTextColor != "green")
                        buttonText = buttonText.Replace(" <color=grey>[</color><color=green>", $" <color=grey>[</color><color={inputTextColor}>");

                    buttonText = FixTMProTags(buttonText);
                    buttonText = FollowMenuSettings(buttonText);

                    transform.Find("Title").GetComponent<TextMeshProUGUI>().SafeSetText(buttonText);
                    transform.Find("Title").GetComponent<TextMeshProUGUI>().spriteAsset = ButtonSpriteSheet;

                    string toolTipText = info.toolTip;

                    if (inputTextColor != "green")
                        toolTipText = toolTipText.Replace("<color=green>", $"<color={inputTextColor}>");

                    toolTipText = FixTMProTags(toolTipText);
                    toolTipText = FollowMenuSettings(toolTipText);

                    transform.Find("ToolTip").GetComponent<TextMeshProUGUI>().SafeSetText(toolTipText);

                    transform.Find("Title").GetComponent<TextMeshProUGUI>().Chams();
                    transform.Find("ToolTip").GetComponent<TextMeshProUGUI>().Chams();

                    button.name = buttonText;

                    if (info.label)
                    {
                        RectTransform title = transform.Find("Title").gameObject.GetComponent<RectTransform>();
                        title.anchorMin = new Vector2(0.5f, 0.5f);
                        title.anchorMax = new Vector2(0.5f, 0.5f);

                        title.localPosition = new Vector3(0f, 0f, 0f);

                        transform.Find("ToolTip").gameObject.SetActive(false);
                        transform.Find("Toggle").gameObject.SetActive(false);
                        transform.Find("Increment").gameObject.SetActive(false);
                        transform.Find("Decrement").gameObject.SetActive(false);
                    }
                    else if (info.incremental)
                    {
                        transform.Find("Increment").gameObject.SetActive(true);
                        transform.Find("Decrement").gameObject.SetActive(true);

                        transform.Find("Increment").gameObject.GetOrAddComponent<UIColorChanger>().colors = buttonColors[0];
                        transform.Find("Decrement").gameObject.GetOrAddComponent<UIColorChanger>().colors = buttonColors[0];

                        transform.Find("Increment/Image").gameObject.GetOrAddComponent<UIColorChanger>().colors = textColors[1];
                        transform.Find("Decrement/Image").gameObject.GetOrAddComponent<UIColorChanger>().colors = textColors[1];
                    }
                    else
                    {
                        transform.Find("Toggle").gameObject.SetActive(true);
                        transform.Find("Toggle/Image").gameObject.SetActive(info.enabled);

                        transform.Find("Toggle").gameObject.GetOrAddComponent<UIColorChanger>().colors = info.enabled ? buttonColors[1] : buttonColors[0];
                        transform.Find("Toggle/Image").gameObject.GetOrAddComponent<UIColorChanger>().colors = info.enabled ? textColors[2] : textColors[1];
                    }

                    transform.Find("Title").AddComponent<UIColorChanger>().colors = textColors[1];
                    transform.Find("ToolTip").AddComponent<UIColorChanger>().colors = textColors[1];
                }

                GameObject button = Object.Instantiate(buttonTemplate, parent, false);
                button.SetActive(true);

                ExtGradient buttonBackgroundColor = backgroundColor.Clone();
                for (int i = 0; i < buttonBackgroundColor.colors.Length; i++)
                {
                    GradientColorKey colorKey = buttonBackgroundColor.colors[i];
                    buttonBackgroundColor.colors[i] = new GradientColorKey { time = colorKey.time, color = DarkenColor(colorKey.color, 0.75f) };
                }
                button.AddComponent<UIColorChanger>().colors = buttonBackgroundColor;

                UpdateButton(button, info);

                Transform transform = button.transform;
                if (info.incremental)
                {
                    transform.Find("Increment").GetComponent<Button>().onClick.AddListener(() =>
                    {
                        ToggleIncremental(info.buttonText, true);
                        SoundManager.Play(SoundManager.DefaultSounds["Button"]);
                        UpdateButton(button, info);
                    });
                    transform.Find("Decrement").GetComponent<Button>().onClick.AddListener(() =>
                    {
                        ToggleIncremental(info.buttonText, false);
                        SoundManager.Play(SoundManager.DefaultSounds["Button"]);
                        UpdateButton(button, info);
                    });
                }
                else
                {
                    transform.Find("Toggle").GetComponent<Button>().onClick.AddListener(() =>
                    {
                        Toggle(info);
                        SoundManager.Play(SoundManager.DefaultSounds["Button"]);
                        UpdateButton(button, info);
                    });
                }
            }

            if (CurrentPrompt != null)
            {
                canvasTransform.Find("Main/PromptTab").gameObject.SetActive(true);

                foreach (string partName in new[]
                    {
                        "Main/PromptTab/Title",
                        "Main/PromptTab/Accept/Text",
                        "Main/PromptTab/Decline/Text"
                    })
                    toRecolor.Add(canvasTransform.Find(partName).GetComponent<MaskableGraphic>());

                GameObject title = canvasTransform.Find("Main/PromptTab/Title").gameObject;
                title.GetComponent<TextMeshProUGUI>().SafeSetText(CurrentPrompt.Message);

                GameObject accept = canvasTransform.Find("Main/PromptTab/Accept").gameObject;
                accept.transform.Find("Text").GetComponent<TextMeshProUGUI>().SafeSetText(CurrentPrompt.AcceptText);
                accept.transform.Find("Text").GetComponent<TextMeshProUGUI>().Chams();
                accept.GetOrAddComponent<UIColorChanger>().colors = buttonColors[0];
                accept.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Toggle("Accept Prompt");
                    SoundManager.Play(SoundManager.DefaultSounds["Button"]);
                    ReloadMenu();
                });

                if (CurrentPrompt.DeclineText != null)
                {
                    GameObject decline = canvasTransform.Find("Main/PromptTab/Decline").gameObject;
                    decline.transform.Find("Text").GetComponent<TextMeshProUGUI>().SafeSetText(CurrentPrompt.DeclineText);
                    decline.GetOrAddComponent<UIColorChanger>().colors = buttonColors[0];
                    decline.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        Toggle("Decline Prompt");
                        SoundManager.Play(SoundManager.DefaultSounds["Button"]);
                        ReloadMenu();
                    });
                }
                else
                {
                    canvasTransform.Find("Main/PromptTab/Decline").gameObject.SetActive(false);

                    RectTransform rectTransform = accept.GetComponent<RectTransform>();
                    rectTransform.localPosition = new Vector3(title.GetComponent<RectTransform>().localPosition.x, rectTransform.localPosition.y, rectTransform.localPosition.z);
                    rectTransform.localScale = new Vector3(rectTransform.localScale.y * 2.05f, rectTransform.localScale.y, rectTransform.localScale.z);

                    accept.transform.Find("Text").GetComponent<RectTransform>().localScale = new Vector3(rectTransform.localScale.y / 2.05f, rectTransform.localScale.y, rectTransform.localScale.z);
                }
            }
            else if (Buttons.CurrentCategoryIndex == 0)
            {
                canvasTransform.Find("Main/HomeTab").gameObject.SetActive(true);
                canvasTransform.Find("Main/HomeTab/Title").GetComponent<TextMeshProUGUI>().SafeSetText($"Hey, {PhotonNetwork.NickName ?? "null"}!");

                if (Buttons.CurrentCategoryIndex == 0)
                {
                    foreach (string partName in new[]
                    {
                        "Main/HomeTab/Title",
                        "Main/HomeTab/EnabledTitle",
                        "Main/HomeTab/FavoritesTitle",
                        "Main/HomeTab/EnabledIcon",
                        "Main/HomeTab/FavoritesIcon",
                        "Main/HomeTab/Enabled/Viewport/Content/None",
                        "Main/HomeTab/Favorites/Viewport/Content/None",
                        "Main/HomeTab/Enabled/Scrollbar Vertical/Sliding Area/Handle",
                        "Main/HomeTab/Favorites/Scrollbar Vertical/Sliding Area/Handle"
                    })
                        toRecolor.Add(canvasTransform.Find(partName).GetComponent<MaskableGraphic>());
                }

                Transform enabledModsTransform = canvasTransform.Find("Main/HomeTab/Enabled/Viewport/Content");

                List<ButtonInfo> enabledMods = new List<ButtonInfo>();
                int categoryIndex = 0;
                foreach (ButtonInfo[] buttonList in Buttons.buttons)
                {
                    enabledMods.AddRange(buttonList.Where(v => v.enabled && (!hideSettings || !Buttons.categoryNames[categoryIndex].Contains("Settings")) && (!hideMacros || !Buttons.categoryNames[categoryIndex].Contains("Macro"))));
                    categoryIndex++;
                }
                enabledMods = enabledMods.OrderBy(v => v.overlapText ?? v.buttonText).ToList();

                if (enabledMods.Count > 0)
                {
                    canvasTransform.Find("Main/HomeTab/Enabled/Viewport/Content/None").gameObject.SetActive(false);
                    foreach (ButtonInfo info in enabledMods)
                        AddButton(enabledModsTransform, info);
                }

                Transform favoritedModsTransform = canvasTransform.Find("Main/HomeTab/Favorites/Viewport/Content");
                List<ButtonInfo> favoriteMods = StringsToInfos(favorites.ToArray()).ToList();

                if (favoriteMods.Count > 0)
                    favoriteMods.RemoveAt(0);

                if (favoriteMods.Count > 0)
                {
                    canvasTransform.Find("Main/HomeTab/Favorites/Viewport/Content/None").gameObject.SetActive(false);
                    foreach (ButtonInfo info in favoriteMods)
                        AddButton(favoritedModsTransform, info);
                }
            }
            else
            {
                canvasTransform.Find("Main/ModuleTab").gameObject.SetActive(true);

                foreach (string partName in new[]
                    {
                        "Main/ModuleTab/Search/SearchIcon",
                        "Main/ModuleTab/Search/Text Area/Placeholder",
                        "Main/ModuleTab/Search/Text Area/Text",
                        "Main/ModuleTab/Modules/Scrollbar Vertical/Sliding Area/Handle"
                    })
                    toRecolor.Add(canvasTransform.Find(partName).GetComponent<MaskableGraphic>());

                List<ButtonInfo> buttons = Buttons.buttons[Buttons.CurrentCategoryIndex].ToList();

                if (buttons.Count > 0 && ignoreButtons.Contains(Buttons.CurrentCategoryName))
                    buttons.RemoveAt(0);

                if (buttons.Count > 0)
                {
                    Transform modulesTransform = canvasTransform.Find("Main/ModuleTab/Modules/Viewport/Content");
                    foreach (ButtonInfo button in buttons)
                        AddButton(modulesTransform, button);
                }

                Transform searchBar = canvasTransform.Find("Main/ModuleTab/Search");
                TMP_InputField inputField = searchBar.GetComponent<TMP_InputField>();

                inputField.onSelect.AddListener(_ =>
                {
                    if (!isSearching)
                        Search();
                });

                inputField.onDeselect.AddListener(_ =>
                {
                    if (isSearching && keyboardInput.IsNullOrEmpty())
                        Search();
                });
            }

            for (int i = 0; i < toRecolor.Count; i++)
            {
                MaskableGraphic graphic = toRecolor[i];
                graphic.gameObject.AddComponent<UIColorChanger>().colors = textColors[i <= 1 ? 0 : 1];

                if (graphic is TMP_Text text)
                    text.Chams();
            }

            ExtGradient buttonBackgroundColor = backgroundColor.Clone();
            for (int i = 0; i < buttonBackgroundColor.colors.Length; i++)
            {
                GradientColorKey colorKey = buttonBackgroundColor.colors[i];
                buttonBackgroundColor.colors[i] = new GradientColorKey { time = colorKey.time, color = DarkenColor(colorKey.color, 0.75f) };
            }
            canvasTransform.Find("Main/ModuleTab/Search").AddComponent<UIColorChanger>().colors = buttonBackgroundColor;

            Canvas.ForceUpdateCanvases();
        }

        public static void UpdateSearch()
        {
            Transform searchBar = canvas.transform.Find("Main/ModuleTab/Search");
            TMP_InputField inputField = searchBar.GetComponent<TMP_InputField>();

            inputField.text = keyboardInput;
            foreach (GameObject button in canvas.transform.Find("Main/ModuleTab/Modules/Viewport/Content").Children())
                button.SetActive(keyboardInput.IsNullOrEmpty() || button.name.ClearTags().Replace(" ", "").ToLower().Contains(keyboardInput.Replace(" ", "").ToLower()));
        }

        public static void ClickGUI()
        {
            if (menu == null)
            {
                if (clickGuiLine != null)
                {
                    Object.Destroy(clickGuiLine.gameObject);
                    clickGuiLine = null;
                }
            }
            else
            {
                canvas.transform.Find("Main/Sidebar/Watermark").localRotation = Quaternion.Euler(0f, 0f, rockWatermark ? Mathf.Sin(Time.time * 2f) * 10f : 0f);

                if (isSearching && Buttons.CurrentCategoryIndex != 0)
                {
                    Transform searchBar = canvas.transform.Find("Main/ModuleTab/Search");
                    TMP_InputField inputField = searchBar.GetComponent<TMP_InputField>();

                    if (inputField.text != keyboardInput)
                        UpdateSearch();
                }

                if (!XRSettings.isDeviceActive)
                    return;

                if (clickGuiLine == null)
                {
                    clickGuiLine = new GameObject("Israel_ClickGUILine")
                        .GetOrAddComponent<LineRenderer>();

                    clickGuiLine.material = new Material(Shader.Find("GUI/Text Shader"));
                    clickGuiLine.startWidth = 0.025f * (scaleWithPlayer ? GTPlayer.Instance.scale : 1f);
                    clickGuiLine.endWidth = clickGuiLine.startWidth;
                    clickGuiLine.useWorldSpace = true;
                    clickGuiLine.positionCount = 2;

                    if (smoothLines)
                    {
                        clickGuiLine.numCapVertices = 10;
                        clickGuiLine.numCornerVertices = 5;
                    }
                }

                clickGuiLine.startColor = backgroundColor.GetCurrentColor();
                clickGuiLine.endColor = backgroundColor.GetCurrentColor(0.5f);

                var uiRaycaster = canvas.GetComponent<GraphicRaycaster>();
                eventSystem ??= EventSystem.current;

                pointerData ??= new PointerEventData(eventSystem);

                bool useLeft = rightHand || (bothHands && ControllerInputPoller.instance.rightControllerSecondaryButton);

                var (_, _, _, forward, _) = useLeft
                    ? ControllerUtilities.GetTrueLeftHand()
                    : ControllerUtilities.GetTrueRightHand();

                Vector3 startPos = useLeft
                    ? GorillaTagger.Instance.leftHandTransform.position
                    : GorillaTagger.Instance.rightHandTransform.position;

                Vector3 direction = forward.normalized;

                Vector3 screenPoint = Camera.main.WorldToScreenPoint(startPos + direction * 5f);
                pointerData.position = screenPoint;

                uiResults.Clear();
                uiRaycaster.Raycast(pointerData, uiResults);

                currentUI = uiResults.Count > 0 ? uiResults[0].gameObject : null;

                Vector3 endPos = currentUI != null
                    ? uiResults[0].worldPosition
                    : startPos + direction * 5f;

                clickGuiLine.SetPosition(0, startPos);
                clickGuiLine.SetPosition(1, endPos);

                bool trigger = useLeft ? leftTrigger > 0.5f : rightTrigger > 0.5f;
                Vector2 currentPos = pointerData.position;
                pointerData.delta = currentPos - lastPointerPos;
                lastPointerPos = currentPos;

                if (trigger && !lastTriggerClick && currentUI != null)
                {
                    GameObject targetUI = null;
                    foreach (var result in uiResults)
                    {
                        var button = result.gameObject.GetComponent<Button>();
                        var toggle = result.gameObject.GetComponent<Toggle>();
                        var slider = result.gameObject.GetComponent<Slider>();
                        var inputField = result.gameObject.GetComponent<TMP_InputField>();

                        if (button != null || toggle != null || slider != null || inputField != null)
                        {
                            targetUI = result.gameObject;
                            break;
                        }
                    }

                    pressedUI = targetUI ?? currentUI;
                    pointerData.pressPosition = currentPos;
                    pointerData.pointerPressRaycast = uiResults[0];

                    ExecuteEvents.Execute(pressedUI, pointerData, ExecuteEvents.pointerDownHandler);
                    pointerData.pointerPress = pressedUI;

                    isDragging = false;
                    draggedUI = ExecuteEvents.GetEventHandler<IDragHandler>(currentUI);
                    pointerData.pointerDrag = draggedUI ?? null;
                }

                switch (trigger)
                {
                    case true when draggedUI != null:
                        {
                            if (!isDragging)
                            {
                                if (Vector2.Distance(pointerData.pressPosition, currentPos) > 15f)
                                {
                                    isDragging = true;
                                    ExecuteEvents.Execute(draggedUI, pointerData, ExecuteEvents.beginDragHandler);

                                    if (pressedUI != null && pressedUI != draggedUI)
                                    {
                                        ExecuteEvents.Execute(pressedUI, pointerData, ExecuteEvents.pointerUpHandler);
                                        pointerData.pointerPress = null;
                                    }
                                }
                            }

                            if (isDragging)
                                ExecuteEvents.Execute(draggedUI, pointerData, ExecuteEvents.dragHandler);
                            break;
                        }
                    case false when lastTriggerClick:
                        {
                            if (pressedUI != null && !isDragging)
                            {
                                ExecuteEvents.Execute(pressedUI, pointerData, ExecuteEvents.pointerUpHandler);
                                ExecuteEvents.Execute(pressedUI, pointerData, ExecuteEvents.pointerClickHandler);
                            }
                            else if (pressedUI != null)
                                ExecuteEvents.Execute(pressedUI, pointerData, ExecuteEvents.pointerUpHandler);

                            if (isDragging && draggedUI != null)
                                ExecuteEvents.Execute(draggedUI, pointerData, ExecuteEvents.endDragHandler);

                            pressedUI = null;
                            draggedUI = null;
                            pointerData.pointerDrag = null;
                            pointerData.pointerPress = null;
                            isDragging = false;
                            break;
                        }
                }

                lastTriggerClick = trigger;
            }
        }

        public static GameObject selectObject;
        public static VRRig lastTarget;
        public static bool lastTriggerSelect;
        public static void PlayerSelect()
        {
            if (XRSettings.isDeviceActive)
            {
                bool leftHand = rightHand || (bothHands && ControllerInputPoller.instance.rightControllerSecondaryButton);

                var (_, _, _, forward, _) = leftHand ? ControllerUtilities.GetTrueLeftHand() : ControllerUtilities.GetTrueRightHand();
                bool canSelect = NetworkSystem.Instance.InRoom && menu != null && reference != null && Vector3.Distance(menu.transform.position, reference.transform.position) > 0.5f;

                if (canSelect)
                {
                    if (selectObject == null)
                        selectObject = new GameObject("Israel_PingLine");

                    Color targetColor = Buttons.GetIndex("Swap GUI Colors").enabled ? buttonColors[1].GetCurrentColor() : backgroundColor.GetCurrentColor();
                    Color lineColor = targetColor;
                    lineColor.a = 0.15f;

                    LineRenderer pingLine = selectObject.GetOrAddComponent<LineRenderer>();
                    pingLine.material.shader = Shader.Find("GUI/Text Shader");
                    pingLine.startColor = lineColor;
                    pingLine.endColor = lineColor;
                    pingLine.startWidth = 0.025f * (scaleWithPlayer ? GTPlayer.Instance.scale : 1f);
                    pingLine.endWidth = 0.025f * (scaleWithPlayer ? GTPlayer.Instance.scale : 1f);
                    pingLine.positionCount = 2;
                    pingLine.useWorldSpace = true;
                    if (smoothLines)
                    {
                        pingLine.numCapVertices = 10;
                        pingLine.numCornerVertices = 5;
                    }

                    Vector3 StartPosition = leftHand ? GorillaTagger.Instance.leftHandTransform.position : GorillaTagger.Instance.rightHandTransform.position;
                    Vector3 Direction = forward;

                    Physics.SphereCast(StartPosition + Direction / 4f * (scaleWithPlayer ? GTPlayer.Instance.scale : 1f), 0.15f, Direction, out var Ray, 512f, NoInvisLayerMask());
                    Vector3 EndPosition = Ray.point == Vector3.zero ? StartPosition + (Direction * 512f) : Ray.point;

                    pingLine.SetPosition(0, StartPosition);
                    pingLine.SetPosition(1, EndPosition);

                    VRRig rigTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (Ray.collider != null && rigTarget != null && !rigTarget.IsLocal())
                    {
                        if (lastTarget != null && lastTarget != rigTarget)
                        {
                            lastTarget.mainSkin.material.shader = Shader.Find("GorillaTag/UberShader");
                            if (lastTarget.mainSkin.sharedMaterial.name.Contains("gorilla_body"))
                                lastTarget.mainSkin.material.color = lastTarget.playerColor;

                            lastTarget = null;
                        }

                        if (lastTarget == null)
                        {
                            Visuals.FixRigMaterialESPColors(rigTarget);

                            rigTarget.mainSkin.material.shader = Shader.Find("GUI/Text Shader");
                            rigTarget.mainSkin.material.color = targetColor;

                            GorillaTagger.Instance.StartVibration(leftHand, GorillaTagger.Instance.tagHapticStrength / 2f, 0.05f);

                            lastTarget = rigTarget;
                        }
                        else
                            lastTarget.mainSkin.material.color = targetColor;

                        bool trigger = leftHand ? leftTrigger > 0.5f : rightTrigger > 0.5f;

                        if (trigger && !lastTriggerSelect)
                        {
                            VRRig.LocalRig.PlayHandTapLocal(50, leftHand, 0.4f);
                            GorillaTagger.Instance.StartVibration(leftHand, GorillaTagger.Instance.tagHapticStrength / 2f, GorillaTagger.Instance.tagHapticDuration / 2f);

                            NavigatePlayer(GetPlayerFromVRRig(rigTarget));
                            ReloadMenu();

                            NotificationManager.SendNotification($"<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> Selected player {GetPlayerFromVRRig(rigTarget).NickName}.");
                        }

                        lastTriggerSelect = trigger;
                    }
                    else
                    {
                        if (lastTarget != null)
                        {
                            lastTarget.mainSkin.material.shader = Shader.Find("GorillaTag/UberShader");
                            if (lastTarget.mainSkin.sharedMaterial.name.Contains("gorilla_body"))
                                lastTarget.mainSkin.material.color = lastTarget.playerColor;

                            lastTarget = null;
                        }
                    }
                }
                else
                {
                    if (selectObject != null)
                    {
                        Object.Destroy(selectObject);
                        selectObject = null;
                    }

                    if (lastTarget != null)
                    {
                        lastTarget.mainSkin.material.shader = Shader.Find("GorillaTag/UberShader");
                        if (lastTarget.mainSkin.sharedMaterial.name.Contains("gorilla_body"))
                            lastTarget.mainSkin.material.color = lastTarget.playerColor;

                        lastTarget = null;
                    }

                    lastTriggerSelect = false;
                }
            }
        }

        public static IEnumerator MenuIntroCoroutine()
        {
            if (Time.time < timeMenuStarted)
                yield return new WaitForSeconds(1f);

            float fps = 1f / Time.unscaledDeltaTime;
            yield return new WaitUntil(() => { fps = Mathf.Lerp(fps, 1f / Time.unscaledDeltaTime, 0.1f); return fps > 30f; });

            GameObject menuIntro = LoadObject<GameObject>("Intro");

            menuIntro.transform.position = GorillaTagger.Instance.bodyCollider.transform.position;
            menuIntro.transform.rotation = GorillaTagger.Instance.bodyCollider.transform.rotation;

            VideoPlayer videoPlayer = menuIntro.transform.Find("Video").GetComponent<VideoPlayer>();
            ParticleSystem particleSystem = menuIntro.transform.Find("Particles").GetComponent<ParticleSystem>();

            Color backgroundColor = Color.white;
            Fun.HueShift(Color.white);

            var main = particleSystem.main; // ????
            main.startColor = new ParticleSystem.MinMaxGradient(
                Main.backgroundColor.GetColor(0)
            );

            void EndImmediately()
            {
                Fun.HueShift(Color.clear);
                Object.Destroy(menuIntro);
            }

            float timeout = 0f;

            while (!videoPlayer.isPrepared)
            {
                timeout += Time.deltaTime;
                if (timeout > 5f)
                {
                    EndImmediately();
                    yield break;
                }
                yield return null;
            }

            bool videoEnded = false;
            videoPlayer.Play();
            videoPlayer.loopPointReached += (_) => videoEnded = true;

            yield return new WaitUntil(() => videoEnded);

            float fadeEnd = Time.time + 1f;
            Color transparentColor = backgroundColor;
            transparentColor.a = 0f;

            while (Time.time < fadeEnd)
            {
                float t = 1f - (fadeEnd - Time.time);
                Fun.HueShift(Color.Lerp(backgroundColor, transparentColor, t));
                videoPlayer.gameObject.GetComponent<Renderer>().material.color = Color.Lerp(Color.white, Color.clear, t);
                main.startColor = new ParticleSystem.MinMaxGradient(
                    Color.Lerp(main.startColor.color, Color.clear, t)
                );

                yield return null;
            }

            EndImmediately();
        }

        public static void MenuIntro() =>
            CoroutineManager.instance.StartCoroutine(MenuIntroCoroutine());

        public static void ResetVoiceCommandsKeywords()
        {
            if (!File.Exists($"{PluginInfo.BaseDirectory}/Israel_Keywords.txt"))
                File.WriteAllLines($"{PluginInfo.BaseDirectory}/Israel_Keywords.txt", keyWords);
        }

        public static void ResetSystemPrompt()
        {
            if (!File.Exists($"{PluginInfo.BaseDirectory}/Israel_SystemPrompt.txt"))
                File.WriteAllText($"{PluginInfo.BaseDirectory}/Israel_SystemPrompt.txt", AIManager.SystemPrompt);
        }

        public static string SavePreferencesToText()
        {
            string seperator = ";;";

            string enabledtext = "";
            foreach (ButtonInfo[] buttonlist in Buttons.buttons)
            {
                foreach (ButtonInfo v in buttonlist)
                {
                    if (!v.detected && v.enabled && v.buttonText != "Save Preferences")
                    {
                        if (enabledtext == "")
                            enabledtext += v.buttonText;
                        else
                            enabledtext += seperator + v.buttonText;
                    }
                }
            }

            string favoritetext = "";
            foreach (string fav in favorites)
            {
                if (favoritetext == "")
                    favoritetext += fav;
                else
                    favoritetext += seperator + fav;
            }

            string[] settings = {
                Movement.platformMode.ToString(),
                Movement.platformShape.ToString(),
                Movement.flySpeedCycle.ToString(),
                Movement.longarmCycle.ToString(),
                Movement.speedboostCycle.ToString(),
                Projectiles.ProjectileMode.ToString(),
                Movement.timerPowerIndex.ToString(),
                Projectiles.shootCycle.ToString(),
                pointerIndex.ToString(),
                Advantages.tagAuraIndex.ToString(),
                notificationDecayTime.ToString(),
                fontStyleType.ToString(),
                arrowType.ToString(),
                pcbg.ToString(),
                Important.reconnectDelay.ToString(),
                "0",//Safety.fpsSpoofValue.ToString(),
                SoundManager.DefaultSounds["Button"],
                buttonClickVolume.ToString(),
                Safety.antiReportRangeIndex.ToString(),
                Advantages.tagRangeIndex.ToString(),
                Sound.BindMode.ToString(),
                Movement.driveInt.ToString(),
                langInd.ToString(),
                inputTextColorInt.ToString(),
                Movement.pullPowerInt.ToString(),
                SoundManager.DefaultSounds["Notification"],
                Visuals.PerformanceModeStepIndex.ToString(),
                gunVariation.ToString(),
                GunDirection.ToString(),
                narratorIndex.ToString(),
                Movement.predInt.ToString(),
                gunLineQualityIndex.ToString(),
                Projectiles.projDebounceIndex.ToString(),
                Projectiles.red.ToString(),
                Projectiles.green.ToString(),
                Projectiles.blue.ToString(),
                Safety.rankIndex.ToString(),
                Projectiles.SnowballSize.ToString(),
                Overpowered.lagIndex.ToString(),
                Fun.blockDebounceIndex.ToString(),
                Fun.nameCycleIndex.ToString(),
                menuScaleIndex.ToString(),
                Sound.soundId.ToString(),
                Fun.targetQuestScore.ToString(),
                notificationScaleIndex.ToString(),
                overlayScaleIndex.ToString(),
                arraylistScaleIndex.ToString(),
                ((int)MathF.Ceiling(playTime)).ToString(),
                PhotonNetwork.LocalPlayer?.UserId ?? "null",
                _pageSize.ToString(),
                Projectiles.snowballMultiplicationFactor.ToString(),
                menuButtonIndex.ToString(),
                Safety.targetElo.ToString(),
                Safety.targetBadge.ToString(),
                Movement.playspaceAbuseIndex.ToString(),
                Movement.wallWalkStrengthIndex.ToString(),
                Fun.headSpinIndex.ToString(),
                Movement.macroPlaybackRangeIndex.ToString(),
                joystickMenuPosition.ToString(),
                Movement.multiplicationAmount.ToString(),
                Fun.targetFOV.ToString(),
                Projectiles.targetProjectileIndex.ToString(),
                Movement.fakeLagDelayIndex.ToString(),
                "0",//Projectiles.snowballIndex.ToString(),
                characterDistance.ToString(),
                Overpowered.lagTypeIndex.ToString(),
                Overpowered.masterVisualizationType.ToString(),
                Movement.targetHz.ToString(),
                "0",//Safety.pingSpoofValue.ToString(),
                Fun.soundboardVolumeIndex.ToString(),
                Fun.soundboardSpeedIndex.ToString(),
                SoundManager.DefaultSoundpack,
                Sound.disableLocalSoundboard.ToString(),
            };

            string settingstext = string.Join(seperator, settings);

            string bindingtext = "";
            foreach (KeyValuePair<string, List<string>> Bind in ModBindings)
            {
                if (bindingtext != "")
                    bindingtext += "~~";

                string toAppend = Bind.Key;
                foreach (string modName in Bind.Value)
                    toAppend += seperator + modName;

                bindingtext += toAppend;
            }

            string quickActionString = string.Join(seperator, quickActions);

            string rebindingtext = "";
            foreach (ButtonInfo[] buttonlist in Buttons.buttons)
            {
                foreach (ButtonInfo v in buttonlist)
                {
                    if (v.rebindKey != null)
                    {
                        if (rebindingtext == "")
                            rebindingtext += v.buttonText + ";" + v.rebindKey;
                        else
                            rebindingtext += seperator + v.buttonText + ";" + v.rebindKey;
                    }
                }
            }

            string skipButtonString = string.Join(seperator, skipButtons);

            string finaltext =
                enabledtext + "\n" +
                favoritetext + "\n" +
                settingstext + "\n" +
                pageButtonType + "\n" +
                themeType + "\n" +
                fontCycle + "\n" +
                bindingtext + "\n" +
                quickActionString + "\n" +
                rebindingtext + "\n" +
                skipButtonString;

            return finaltext;
        }


        [Obsolete("Replaced by Preferences.Save()")]
        public static void SavePreferences() =>
            Preferences.Save();

        public static int loadingPreferencesFrame;

        private static void Restore(string buttonName, object newValue)
        {
            ButtonInfo button = Buttons.GetIndex(buttonName);
            if (button == null) return;

            try
            {
                button.value = newValue;
                button.onValueChanged?.Invoke();
            }
            catch (Exception e)
            {
                LogManager.Log($"Failed to restore button '{buttonName}' to {newValue}. Just gonna leave it at the default ({e.Message})");
            }
        }

        private static void RestoreCycle(string buttonName, int newValue) => Restore(buttonName, newValue);
        private static void RestoreNamedCycle(string buttonName, string newValue) => Restore(buttonName, newValue);

        public static void LoadPreferencesFromText(string text)
        {
            loadingPreferencesFrame = Time.frameCount;

            Panic();
            string[] textData = text.Split("\n");

            string[] activebuttons = textData[0].Split(";;");
            for (int index = 0; index < activebuttons.Length; index++)
                Toggle(activebuttons[index]);

            string[] favoritesarray = textData[1].Split(";;");
            favorites.Clear();
            foreach (string favorite in favoritesarray)
                favorites.Add(favorite);

            try
            {
                string[] data = textData[2].Split(";;");
                Movement.platformMode = int.Parse(data[0]);
                RestoreCycle("Change Platform Type", Movement.platformMode);

                Movement.platformShape = int.Parse(data[1]);
                RestoreCycle("Change Platform Shape", Movement.platformShape);

                Movement.flySpeedCycle = int.Parse(data[2]);
                RestoreCycle("Change Fly Speed", Movement.flySpeedCycle);

                Movement.longarmCycle = int.Parse(data[3]);
                RestoreCycle("Change Arm Length", Movement.longarmCycle);

                Movement.speedboostCycle = int.Parse(data[4]);
                RestoreCycle("Change Speed Boost Amount", Movement.speedboostCycle);

                Projectiles.ProjectileMode = int.Parse(data[5]);
                RestoreCycle("Change Projectile", Projectiles.ProjectileMode);

                Movement.timerPowerIndex = int.Parse(data[6]);
                RestoreCycle("Change Timer Speed", Movement.timerPowerIndex);

                Projectiles.shootCycle = int.Parse(data[7]);
                RestoreCycle("Change Shoot Speed", Projectiles.shootCycle);

                pointerIndex = int.Parse(data[8]);
                RestoreCycle("Change Pointer Position", pointerIndex);

                Advantages.tagAuraIndex = int.Parse(data[9]);
                RestoreCycle("ctaRange", Advantages.tagAuraIndex);

                notificationDecayTime = int.Parse(data[10]);
                RestoreCycle("Change Notification Time", notificationDecayTime / 1000);

                fontStyleType = int.Parse(data[11]);
                RestoreCycle("Change Font Style Type", fontStyleType);

                arrowType = int.Parse(data[12]);
                RestoreCycle("Change Arrow Type", arrowType);

                pcbg = int.Parse(data[13]);
                RestoreCycle("Change PC Menu Background", pcbg);

                Important.reconnectDelay = int.Parse(data[14]);
                RestoreCycle("Change Reconnect Time", Important.reconnectDelay);

                //Safety.fpsSpoofValue = string.IsNullOrWhiteSpace(data[15]) ? 85 : int.Parse(data[15]);
                //RestoreCycle("Change FPS Spoof Value", Safety.fpsSpoofValue);

                SoundManager.DefaultSounds["Button"] = data[16];
                RestoreNamedCycle("Change Button Sound", data[16]);

                buttonClickVolume = int.Parse(data[17]);
                RestoreCycle("Change Button Volume", buttonClickVolume);

                Safety.antiReportRangeIndex = int.Parse(data[18]);
                RestoreCycle("Change Anti Report Distance", Safety.antiReportRangeIndex);

                Advantages.tagRangeIndex = int.Parse(data[19]);
                RestoreCycle("ctrRange", Advantages.tagRangeIndex);

                Sound.BindMode = int.Parse(data[20]);
                RestoreCycle("Sound Bindings", Sound.BindMode);

                Movement.driveInt = int.Parse(data[21]);
                RestoreCycle("cdSpeed", Movement.driveInt);

                langInd = int.Parse(data[22]);
                RestoreCycle("Change Menu Language", langInd);

                inputTextColorInt = int.Parse(data[23]);
                RestoreCycle("Change Input Text Color", inputTextColorInt);

                Movement.pullPowerInt = int.Parse(data[24]);
                RestoreCycle("Change Pull Mod Power", Movement.pullPowerInt);

                SoundManager.DefaultSounds["Notification"] = data[25];
                RestoreNamedCycle("Change Notification Sound", data[25]);

                Visuals.PerformanceModeStepIndex = int.Parse(data[26]);
                RestoreCycle("Change Performance Visuals Step", Visuals.PerformanceModeStepIndex);

                gunVariation = int.Parse(data[27]);
                RestoreCycle("Change Gun Variation", gunVariation);

                GunDirection = int.Parse(data[28]);
                RestoreCycle("Change Gun Direction", GunDirection);

                narratorIndex = int.Parse(data[29]);
                RestoreCycle("Change Narration Voice", narratorIndex);

                Movement.predInt = int.Parse(data[30]);
                RestoreCycle("Change Prediction Amount", Movement.predInt);

                gunLineQualityIndex = int.Parse(data[31]);
                RestoreCycle("Change Gun Line Quality", gunLineQualityIndex);

                Projectiles.red = int.Parse(data[33]);
                RestoreCycle("RedProj", Projectiles.red);

                Projectiles.green = int.Parse(data[34]);
                RestoreCycle("GreenProj", Projectiles.green);

                Projectiles.blue = int.Parse(data[35]);
                RestoreCycle("BlueProj", Projectiles.blue);

                Safety.rankIndex = int.Parse(data[36]);
                RestoreCycle("Change Ranked Tier", Safety.rankIndex);

                Projectiles.SnowballSize = int.Parse(data[37]);
                RestoreCycle("Change Snowball Size", Projectiles.SnowballSize);

                Overpowered.lagIndex = int.Parse(data[38]);
                RestoreCycle("Change Lag Power", Overpowered.lagIndex);

                Fun.blockDebounceIndex = int.Parse(data[39]);
                RestoreCycle("Change Block Delay", Fun.blockDebounceIndex);

                Fun.nameCycleIndex = int.Parse(data[40]);
                RestoreCycle("Change Cycle Delay", Fun.nameCycleIndex);

                menuScaleIndex = int.Parse(data[41]);
                RestoreCycle("Change Menu Scale", menuScaleIndex);

                Sound.soundId = int.Parse(data[42]);
                RestoreCycle("Custom Sound ID", Sound.soundId);

                Fun.targetQuestScore = int.Parse(data[43]);
                RestoreCycle("Change Custom Quest Score", Fun.targetQuestScore);

                notificationScaleIndex = int.Parse(data[44]);
                RestoreCycle("Change Notification Scale", notificationScaleIndex);

                overlayScaleIndex = int.Parse(data[45]);
                RestoreCycle("Change Overlay Scale", overlayScaleIndex);

                arraylistScaleIndex = int.Parse(data[46]);
                RestoreCycle("Change Arraylist Scale", arraylistScaleIndex);

                playTime = int.Parse(data[47]);

                Important.oldId = data[48];

                _pageSize = int.Parse(data[49]);
                RestoreCycle("Change Page Size", _pageSize);

                Projectiles.snowballMultiplicationFactor = int.Parse(data[50]);
                RestoreCycle("Change Snowball Multiplication Factor", Projectiles.snowballMultiplicationFactor);

                menuButtonIndex = int.Parse(data[51]);
                RestoreCycle("Change Menu Button", menuButtonIndex);

                Safety.targetElo = int.Parse(data[52]);
                RestoreCycle("Change ELO Value", Safety.targetElo);

                Safety.targetBadge = int.Parse(data[53]);
                RestoreCycle("Change Badge Tier", Safety.targetBadge);

                Movement.playspaceAbuseIndex = int.Parse(data[54]);
                RestoreCycle("Change Playspace Abuse Speed", Movement.playspaceAbuseIndex);

                Movement.wallWalkStrengthIndex = int.Parse(data[55]);
                RestoreCycle("Change Wall Walk Strength", Movement.wallWalkStrengthIndex);

                Fun.headSpinIndex = int.Parse(data[56]);
                RestoreCycle("Change Head Spin Speed", Fun.headSpinIndex);

                Movement.macroPlaybackRangeIndex = int.Parse(data[57]);
                RestoreCycle("Change Macro Playback Range", Movement.macroPlaybackRangeIndex);

                joystickMenuPosition = int.Parse(data[58]);
                RestoreCycle("Change Joystick Menu Position", joystickMenuPosition);

                Movement.multiplicationAmount = int.Parse(data[59]);
                RestoreCycle("Knockback Multiplication Amount", Movement.multiplicationAmount);

                Fun.targetFOV = int.Parse(data[60]);
                RestoreCycle("Change Target FOV", Fun.targetFOV);

                Projectiles.targetProjectileIndex = int.Parse(data[61]);
                RestoreCycle("Change Projectile Index", Projectiles.targetProjectileIndex);

                Movement.fakeLagDelayIndex = int.Parse(data[62]);
                RestoreCycle("Change Fake Lag Strength", Movement.fakeLagDelayIndex);

                //Projectiles.snowballIndex = int.Parse(data[63]);
                //Projectiles.ChangeGrowingProjectile();

                characterDistance = int.Parse(data[64]);
                RestoreCycle("Change Character Distance", characterDistance);

                Overpowered.lagTypeIndex = int.Parse(data[65]);
                RestoreCycle("Change Lag Type", Overpowered.lagTypeIndex);

                Overpowered.masterVisualizationType = int.Parse(data[66]);
                RestoreCycle("Master Visualization Type", Overpowered.masterVisualizationType);

                Movement.targetHz = int.Parse(data[67]);
                RestoreCycle("Change Tinnitus Hertz", Movement.targetHz);

                //Safety.pingSpoofValue = int.Parse(data[68]);
                //RestoreCycle("Change Ping Spoof Value", Safety.pingSpoofValue);

                Fun.soundboardVolumeIndex = float.Parse(data[69]);
                RestoreCycle("Change Soundboard Volume", (int)Fun.soundboardVolumeIndex);

                Fun.soundboardSpeedIndex = float.Parse(data[70]);
                RestoreCycle("Change Soundboard Speed", (int)Fun.soundboardSpeedIndex);

                ButtonInfo soundpack = Buttons.GetIndex("Change Menu Soundpack");
                RestoreNamedCycle("Change Menu Soundpack", data[71]);

                Sound.disableLocalSoundboard = bool.Parse(data[72]);
            }
            catch (Exception e) { LogManager.Log("Save file out of date: " + e); }


            pageButtonType = int.Parse(textData[3]);
            RestoreCycle("Change Page Type", pageButtonType);
            themeType = int.Parse(textData[4]);
            RestoreCycle("Change Menu Theme", themeType - 1);
            fontCycle = int.Parse(textData[5]);
            RestoreCycle("Change Font Type", fontCycle);

            try
            {
                foreach (string Bindings in textData[6].Split("~~"))
                {
                    if (Bindings.Contains(";;"))
                    {
                        string[] BindData = Bindings.Split(";;");
                        string BindName = BindData[0];

                        List<string> Binds = new List<string>();

                        for (int i = 1; i < BindData.Length; i++)
                        {
                            string ModName = BindData[i];
                            if (Buttons.GetIndex(ModName) != null)
                                Binds.Add(ModName);
                        }

                        ModBindings[BindName] = Binds;
                    }
                }
            }
            catch { }

            try
            {
                quickActions.Clear();
                foreach (string quickAction in textData[7].Split(";;"))
                {
                    ButtonInfo button = Buttons.GetIndex(quickAction);
                    if (button != null)
                        quickActions.Add(quickAction);
                }
            }
            catch { }

            try
            {
                foreach (string bind in textData[8].Split(";;"))
                {
                    string rebindText = bind.Split(";")[0];
                    string rebindKey = bind.Split(";")[1];
                    ButtonInfo button = Buttons.GetIndex(rebindText);
                    if (button != null)
                        button.rebindKey = rebindKey;
                }
            }
            catch { }

            try
            {
                skipButtons.Clear();
                foreach (string skipButton in textData[9].Split(";;"))
                {
                    ButtonInfo button = Buttons.GetIndex(skipButton);
                    if (button != null)
                        skipButtons.Add(skipButton);
                }
            }
            catch { }

            hasLoadedPreferences = true;
        }

        public static void LoadPreferences() => Preferences.Load();

        public static void Panic()
        {
            AnnoyingModeOff();
            foreach (ButtonInfo[] buttonlist in Buttons.buttons)
            {
                foreach (ButtonInfo v in buttonlist)
                {
                    if (v.enabled)
                        Toggle(v.buttonText);
                }
            }
        }

        public enum ControllerBinding
        {
            None,
            LeftTrigger,
            RightTrigger,
            LeftGrip,
            RightGrip,
            LeftPrimaryButton,
            RightPrimaryButton,
            LeftSecondaryButton,
            RightSecondaryButton,
            JoystickClick,
            LeftOverride
        }

        public static readonly Dictionary<ControllerBinding, Key> pcBindings = new Dictionary<ControllerBinding, Key>
        {
            { ControllerBinding.RightPrimaryButton, Key.E },
            { ControllerBinding.RightSecondaryButton, Key.R },
            { ControllerBinding.LeftPrimaryButton, Key.F },
            { ControllerBinding.LeftSecondaryButton, Key.G },
            { ControllerBinding.LeftGrip, Key.LeftBracket },
            { ControllerBinding.RightGrip, Key.RightBracket },
            { ControllerBinding.LeftTrigger, Key.Minus },
            { ControllerBinding.RightTrigger, Key.Equals },
            { ControllerBinding.JoystickClick, Key.Enter },
            { ControllerBinding.LeftOverride, Key.LeftAlt }
        };

        public static void LoadPCControls()
        {
            string fileName = $"{PluginInfo.BaseDirectory}/Israel_PCControls.txt";

            if (File.Exists(fileName))
            {
                string data = File.ReadAllText(fileName);
                string[] lines = data.Split('\n');
                pcBindings.Clear();

                foreach (string line in lines)
                {
                    string finalLine = line.Trim();

                    if (!finalLine.Contains(" - "))
                        continue;

                    string[] splitData = finalLine.Split(" - ");

                    if (Enum.TryParse(splitData[1], out ControllerBinding binding) && Enum.TryParse(splitData[0], out Key key))
                        pcBindings[binding] = key;
                }
            }
            else
            {
                var lines = new List<string>();

                foreach (var pair in pcBindings)
                    lines.Add($"{pair.Value} - {pair.Key}");

                File.WriteAllLines(fileName, lines);
            }
        }

        public static void ApplyReconnectTime(int index) => Important.reconnectDelay = index;

        public static void ApplyButtonSound(string soundName)
        {
            SoundManager.DefaultSounds["Button"] = soundName;
            Buttons.GetIndex("Change Button Sound").overlapText =
                $"Change Button Sound <color=grey>[</color><color=green>{soundName}</color><color=grey>]</color>";
        }
        public static void ChangeButtonSound(bool positive = true, bool fromMenu = false)
        {
            var buttonKeys = SoundManager.Sounds["Buttons"].Keys.ToArray();

            int index = Array.IndexOf(buttonKeys, SoundManager.DefaultSounds["Button"]);
            if (index < 0) index = 0;
            index = ButtonHelper.Wrap(index, 0, buttonKeys.Length - 1, positive);

            string newSound = buttonKeys[index];
            ApplyButtonSound(newSound);
            Buttons.GetIndex("Change Button Sound").value = newSound;

            if (!fromMenu) return;
            if (VRRig.LocalRig == null) return;
            if (VRRig.LocalRig.leftHandPlayer != null) VRRig.LocalRig.leftHandPlayer.Stop();
            if (VRRig.LocalRig.rightHandPlayer != null) VRRig.LocalRig.rightHandPlayer.Stop();

            SoundManager.Play(SoundManager.DefaultSounds["Button"]);
        }

        public static void ApplyButtonVolume(int index) => buttonClickVolume = index;
        public static void PreviewButtonVolume(bool positive)
        {
            if (VRRig.LocalRig == null) return;
            VRRig.LocalRig.leftHandPlayer.Stop();
            VRRig.LocalRig.rightHandPlayer.Stop();
            SoundManager.Play(SoundManager.DefaultSounds["Button"]);
        }
        public static void ApplyMenuSoundpack(string packName) => SoundManager.DefaultSoundpack = packName;
    }
}