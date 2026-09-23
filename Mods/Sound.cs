/*
 * United Goyim College Fund  Mods/Sound.cs
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
using GorillaLocomotion;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using Seralyth.Classes.Menu;
using Seralyth.Extensions;
using Seralyth.Managers;
using Seralyth.Menu;
using Seralyth.Patches.Menu;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using static Seralyth.Menu.Main;
using static Seralyth.Utilities.AssetUtilities;
using static Seralyth.Utilities.FileUtilities;
using Random = UnityEngine.Random;

namespace Seralyth.Mods
{
    public static class Sound
    {
        public static bool LegacySoundboard = false;
        public static bool LoopAudio = false;
        public static bool OverlapAudio = false;
        public static int BindMode;
        public static string Subdirectory = "";
        public static readonly Dictionary<string, ButtonInfo[]> CachedButtons = new Dictionary<string, ButtonInfo[]>();

        public static void LoadSoundboard(bool openCategory = true)
        {
            string key = Subdirectory ?? "";

            if (CachedButtons.TryGetValue(key, out ButtonInfo[] buttons))
            {
                Buttons.buttons[Buttons.GetCategory("Soundboard")] = buttons;

                if (openCategory)
                    Buttons.CurrentCategoryName = "Soundboard";

                return;
            }

            string path = Path.Combine(PluginInfo.BaseDirectory, "Sounds", Subdirectory.TrimStart('/'));

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            List<ButtonInfo> soundButtons = new List<ButtonInfo>();

            if (Subdirectory != "")
            {
                soundButtons.Add(new ButtonInfo
                {
                    buttonText = "Exit Subdirectory",
                    overlapText = "Exit " + Subdirectory.Split("/")[^1],
                    method = () =>
                    {
                        Subdirectory = RemoveLastDirectory(Subdirectory);
                        LoadSoundboard();
                    },
                    isTogglable = false,
                    toolTip = "Returns you back to the last folder."
                });
            }
            else
            {
                soundButtons.Add(new ButtonInfo
                {
                    buttonText = "Exit Soundboard",
                    method = () => Buttons.CurrentCategoryName = "Sound Mods",
                    isTogglable = false,
                    toolTip = "Returns you back to the sound mods."
                });
            }

            string[] folders = Directory.GetDirectories(path);
            string[] files = Directory.GetFiles(path);

            soundButtons.AddRange(
                from folder in folders
                let relativePath = Path.GetRelativePath(path, folder)
                select new ButtonInfo
                {
                    buttonText = "SoundboardFolder" + relativePath.Hash(),
                    overlapText = $"<sprite name=\"Folder\">  {relativePath}  ",
                    method = () => OpenFolder(relativePath),
                    isTogglable = false,
                    toolTip = "Opens the " + relativePath + " folder."
                });

            if (!RecorderPatch.enabled)
                NotificationManager.SendNotification($"<color=grey>[</color><color=red>WARNING</color><color=grey>]</color> You are using the legacy microphone system. Modern soundboard features will not be implemented.");

            foreach (string file in files)
            {
                string fileName = Path.GetRelativePath(path, file);
                string soundName = RemoveFileExtension(fileName).Replace("_", " ");
                string soundPath = Path.GetRelativePath(PluginInfo.BaseDirectory, file).Replace("\\", "/");

                string soundKey = "SoundboardSound" + soundName.Hash();


                if (!RecorderPatch.enabled || LegacySoundboard)
                {
                    if (BindMode > 0)
                    {
                        soundButtons.Add(new ButtonInfo
                        {
                            buttonText = soundKey,
                            overlapText = soundName,
                            method = () => PrepareBindAudio(soundPath),
                            disableMethod = StopAllSounds,
                            toolTip = $"Plays {soundName} through your microphone."
                        });
                    }
                    else
                    {
                        if (LoopAudio)
                        {
                            soundButtons.Add(new ButtonInfo
                            {
                                buttonText = soundKey,
                                overlapText = soundName,
                                enableMethod = () => PlayAudio(soundPath),
                                disableMethod = StopAllSounds,
                                toolTip = $"Plays {soundName} through your microphone."
                            });
                        }
                        else
                        {
                            soundButtons.Add(new ButtonInfo
                            {
                                buttonText = soundKey,
                                overlapText = soundName,
                                method = () => PlayAudio(soundPath),
                                isTogglable = false,
                                toolTip = $"Plays {soundName} through your microphone."
                            });
                        }
                    }
                }
                else if (RecorderPatch.enabled)
                {
                    ButtonInfo buttonInfo = null;
                    buttonInfo = new ButtonInfo
                    {
                        buttonText = soundKey,
                        overlapText = soundName,
                        toolTip = $"Allows you to view {soundName}'s properties.",
                        method = () => LoadSoundProperties(soundName, soundPath, soundName.Hash()),
                        isTogglable = false
                    };

                    soundButtons.Add(buttonInfo);
                }
            }

            soundButtons.Add(new ButtonInfo
            {
                buttonText = "Stop All Sounds",
                method = StopAllSounds,
                isTogglable = false,
                toolTip = "Stops all currently playing sounds."
            });

            soundButtons.Add(new ButtonInfo
            {
                buttonText = "Open Sound Folder",
                aliases = new[] { "Open Soundboard Folder" },
                method = OpenSoundFolder,
                isTogglable = false,
                toolTip = "Opens a folder containing all of your sounds."
            });

            soundButtons.Add(new ButtonInfo
            {
                buttonText = "Reload Sounds",
                method = () =>
                {
                    CachedButtons.Clear();
                    LoadSoundboard();
                },
                isTogglable = false,
                toolTip = "Reloads all of your sounds."
            });

            soundButtons.Add(new ButtonInfo
            {
                buttonText = "Get More Sounds",
                method = LoadSoundLibrary,
                isTogglable = false,
                toolTip = "Opens a public audio library, where you can download your own sounds."
            });

            CachedButtons[key] = soundButtons.ToArray();
            Buttons.buttons[Buttons.GetCategory("Soundboard")] = CachedButtons[key];

            if (openCategory)
                Buttons.CurrentCategoryName = "Soundboard";
        }

        public static void OpenFolder(string folder)
        {
            if (string.IsNullOrEmpty(Subdirectory))
                Subdirectory = "/" + folder;
            else
                Subdirectory = Subdirectory.TrimEnd('/') + "/" + folder;

            LoadSoundboard();
        }

        public static void LoadSoundLibrary()
        {
            string library = GetHttp($"{PluginInfo.ServerResourcePath}/Audio/Mods/Fun/Soundboard/SoundLibrary.txt");
            string[] audios = AlphabetizeNoSkip(library.Split("\n"));
            List<ButtonInfo> soundbuttons = new List<ButtonInfo> { new ButtonInfo { buttonText = "Exit Sound Library", method = () => LoadSoundboard(), isTogglable = false, toolTip = "Returns you back to the soundboard." } };
            int index = 0;
            foreach (string audio in audios)
            {
                if (audio.Length > 2)
                {
                    index++;
                    string[] Data = audio.Split(";");
                    soundbuttons.Add(new ButtonInfo { buttonText = "SoundboardDownload" + index, overlapText = Data[0], method = () => DownloadSound(Data[0], $"{PluginInfo.ServerResourcePath}/Audio/Mods/Fun/Soundboard/Sounds/{Data[1]}"), isTogglable = false, toolTip = "Downloads " + Data[0] + " to your sound library." });
                }
            }
            Buttons.buttons[Buttons.GetCategory("Sound Library")] = soundbuttons.ToArray();
            Buttons.CurrentCategoryName = "Sound Library";
        }

        public static void LoadSoundProperties(string soundName, string soundPath, string hash)
        {
            ButtonInfo playButton = null;
            ButtonInfo pauseButton = null;
            ButtonInfo durationButton = null;
            ButtonInfo volumeButton = null;
            ButtonInfo speedButton = null;

            float skipAmount = 1f;
            ButtonInfo skipAmountButton = null;
            ButtonInfo skipButton = null;

            bool isActive = activeSounds.ContainsKey(hash);
            string hashedName = soundName.Hash();

            void Play()
            {
                if (OverlapAudio)
                {
                    PlayAudio(soundPath);
                    playButton.SetEnabled(false);
                    return;
                }

                if (!activeSounds.ContainsKey(hash))
                {
                    playButton.overlapText = "Stop";
                    PlaySoundboardSound(soundPath, hash, LoopAudio, BindMode > 0);

                    System.Collections.IEnumerator Reload()
                    {
                        while (!activeSounds.ContainsKey(hash))
                            yield return null;

                        LoadSoundProperties(soundName, soundPath, hash);
                        ReloadMenu();
                    }

                    CoroutineManager.instance.StartCoroutine(Reload());
                }
            }

            void Stop()
            {
                if (activeSounds.ContainsKey(hash))
                {
                    playButton.overlapText = "Play";
                    StopSoundboardSound(hash);
                    LoadSoundProperties(soundName, soundPath, hash);
                    ReloadMenu();
                }
            }

            string FormatDuration(double totalSeconds)
            {
                if (totalSeconds < 0) totalSeconds = 0;

                int days = (int)(totalSeconds / 86400);
                int hours = (int)((totalSeconds % 86400) / 3600);
                int minutes = (int)((totalSeconds % 3600) / 60);
                int seconds = (int)(totalSeconds % 60);

                List<string> parts = new List<string>();

                if (days > 0) parts.Add($"{days} day{(days == 1 ? "" : "s")}");
                if (hours > 0) parts.Add($"{hours} hour{(hours == 1 ? "" : "s")}");
                if (minutes > 0) parts.Add($"{minutes} minute{(minutes == 1 ? "" : "s")}");
                if (seconds > 0 || parts.Count == 0) parts.Add($"{seconds} second{(seconds == 1 ? "" : "s")}");

                return string.Join(" ", parts);
            }

            playButton = new ButtonInfo
            {
                buttonText = $"Play or Pause SoundboardSound {hashedName}",
                toolTip = $"Plays or pauses the sound {soundName}.",
                overlapText = isActive ? "Stop" : "Play",
                enableMethod = Play,
                disableMethod = Stop,
                enabled = isActive
            };

            durationButton = new ButtonInfo
            {
                label = true,
                buttonText = $"SoundboardSound {hashedName}'s Duration",
                overlapText = $"Duration: Loading..",
                method = () =>
                {
                    try
                    {
                        if (activeSounds.TryGetValue(hash, out var s) && s.Clip != null)
                            durationButton.overlapText = $"Duration: {FormatDuration(s.Clip.CurrentTime)}";
                        else
                            durationButton.overlapText = "Duration: N/A";
                    }
                    catch { }
                }
            };

            var list = new List<ButtonInfo>
            {
                new ButtonInfo
                {
                    buttonText = $"Exit {soundName}'s Properties",
                    method = () => LoadSoundboard(),
                    isTogglable = false,
                    toolTip = "Returns you back to the soundboard."
                },
                playButton
            };

            if (activeSounds.TryGetValue(hash, out var sound) && sound.Clip != null)
            {
                pauseButton = new ButtonInfo
                {
                    buttonText = $"Pause SoundboardSound {hashedName}",
                    overlapText = "Pause",
                    enableMethod = () =>
                    {
                        sound.Clip.Pause();
                        pauseButton.overlapText = "Resume";
                    },
                    disableMethod = () =>
                    {
                        sound.Clip.Resume();
                        pauseButton.overlapText = "Pause";
                    },
                    toolTip = $"Pauses or resumes the sound."
                };
                list.Add(pauseButton);
                list.Add(new ButtonInfo
                {
                    buttonText = $"Loop SoundboardSound {hashedName}",
                    overlapText = "Loop",
                    enableMethod = () => sound.Clip.Looping = true,
                    disableMethod = () => sound.Clip.Looping = false,
                    toolTip = "Makes the song loop when it ends."
                });

                skipButton = new ButtonInfo
                {
                    buttonText = $"Skip SoundboardSound {hashedName}",
                    overlapText = $"Skip <color=grey>[</color><color=green>{skipAmount:0.0}s</color><color=grey>]</color>",
                    enableMethod = () => sound.Clip.CurrentTime = Mathf.Clamp(sound.Clip.CurrentTime + skipAmount, 0, sound.Clip.Length),
                    disableMethod = () => sound.Clip.CurrentTime = Mathf.Clamp(sound.Clip.CurrentTime - skipAmount, 0, sound.Clip.Length),
                    incremental = true,
                    isTogglable = false,
                    toolTip = "Skips forward or backward in the sound by the skip amount."
                };

                list.Add(skipButton);
                list.Add(durationButton);
                list.Add(new ButtonInfo
                {
                    label = true,
                    buttonText = $"SoundboardSound {hashedName}'s Length",
                    overlapText = $"Length: {FormatDuration(sound.Clip.Length)}"
                });

                volumeButton = ButtonHelper.CreateNumeric(
                    $"Change SoundboardSound {hashedName}'s Volume",
                    min: 0,
                    max: 200,
                    defaultValue: Mathf.RoundToInt(Mathf.Clamp(sound.Clip.Volume, 0, 10) / 0.05f),
                    apply: v => sound.Clip.Volume = v * 0.05f,
                    display: v => (v * 0.05f).ToString("F2"),
                    toolTip: "Changes the volume of the sound. Higher volumes will make the sound louder, while lower volumes will make it quieter.",
                    overlapText: "Change Volume");

                speedButton = ButtonHelper.CreateNumeric(
                    $"Change SoundboardSound {hashedName}'s Speed",
                    min: 0,
                    max: 100,
                    defaultValue: Mathf.RoundToInt(Mathf.Clamp(sound.Clip.Speed, 0, 5) / 0.05f),
                    apply: v => sound.Clip.Speed = v * 0.05f,
                    display: v => (v * 0.05f).ToString("F2"),
                    toolTip: "Changes the speed of the sound. Higher speeds will make the pitch higher, while lower speeds will make it lower.",
                    overlapText: "Change Speed");

                skipAmountButton = ButtonHelper.CreateNumeric(
                    $"Change Skip Amount SoundboardSound {hashedName}",
                    min: 0,
                    max: 60,
                    defaultValue: Mathf.RoundToInt(skipAmount / 0.5f),
                    apply: v =>
                    {
                        skipAmount = v * 0.5f;
                        skipButton.overlapText = $"Skip Amount <color=grey>[</color><color=green>{skipAmount:0.0}s</color><color=grey>]</color>";
                    },
                    display: v => $"{v * 0.5f:0.0}s",
                    persist: false,
                    toolTip: "Changes how much time is skipped when you press the skip button.",
                    overlapText: "Skip Amount");

                list.Add(skipAmountButton);
                list.Add(volumeButton);
                list.Add(speedButton);
            }

            Buttons.buttons[Buttons.GetCategory("Sound Properties")] = list.ToArray();
            Buttons.CurrentCategoryName = "Sound Properties";
        }

        public static void DownloadSound(string name, string url)
        {
            if (name.Contains(".."))
                name = name.Replace("..", "");

            if (name.Contains(":"))
                return;

            string filename = Path.Combine("Sounds", Subdirectory.TrimStart('/'), $"{name}.{GetFileExtension(url)}");
            if (File.Exists($"{PluginInfo.BaseDirectory}/{filename}"))
                File.Delete($"{PluginInfo.BaseDirectory}/{filename}");

            audioFilePool.Remove(name);

            LoadSoundFromURL(url, filename, clip =>
            {
                if (clip.length < 20f)
                    Play2DAudio(clip);
            });

            CachedButtons.Remove(Subdirectory ?? "");

            NotificationManager.SendNotification("<color=grey>[</color><color=green>SUCCESS</color><color=grey>]</color> Successfully downloaded " + name + " to the soundboard.");
        }

        public static bool AudioIsPlaying;
        public static float RecoverTime = -1f;

        private static GameObject soundboardAudioManager;

        public static bool disableLocalSoundboard;
        public static void PlayAudio(AudioClip sound, bool disableMicrophone = false)
        {
            if (sound == null)
            {
                LogManager.LogError("AudioClip is null");
                return;
            }
            if (!NetworkSystem.Instance.InRoom)
            {
                if (soundboardAudioManager == null)
                {
                    soundboardAudioManager = new GameObject("2DAudioMgr");
                    AudioSource temp = soundboardAudioManager.AddComponent<AudioSource>();
                    temp.spatialBlend = 0f;
                }

                AudioSource ausrc = soundboardAudioManager.GetComponent<AudioSource>();
                ausrc.volume = 1f;
                ausrc.clip = sound;
                ausrc.loop = false;
                ausrc.Play();

                AudioIsPlaying = true;
                RecoverTime = Time.time + sound.length;

                return;
            }

            if (!RecorderPatch.enabled)
            {
                NetworkSystem.Instance.VoiceConnection.PrimaryRecorder.SourceType = Recorder.InputSourceType.AudioClip;
                NetworkSystem.Instance.VoiceConnection.PrimaryRecorder.AudioClip = sound;
                NetworkSystem.Instance.VoiceConnection.PrimaryRecorder.RestartRecording(true);
            }
            else if (RecorderPatch.enabled)
            {
                if (LegacySoundboard && VoiceManager.Get().AudioClips.Count == 1)
                    VoiceManager.Get().StopAudioClips();
                VoiceManager.Get().AudioClip(sound, disableMicrophone);
            }

            if (!LoopAudio)
            {
                AudioIsPlaying = true;
                RecoverTime = Time.time + sound.length + 0.4f;
            }
        }

        private class SoundData
        {
            public VoiceManager.Clip Clip;
            public AudioClip AudioClip;
        }

        private static readonly Dictionary<string, SoundData> activeSounds = new Dictionary<string, SoundData>();

        public static void PlaySoundboardSound(object file, string hash, bool loopAudio, bool bind)
        {
            bool[] bindings = {
                rightPrimary,
                rightSecondary,
                leftPrimary,
                leftSecondary,
                leftGrab,
                rightGrab,
                leftTrigger > 0.5f,
                rightTrigger > 0.5f,
                leftJoystickClick,
                rightJoystickClick
            };

            bool shouldPlay = true;
            if (bind && BindMode > 0)
            {
                bool bindPressed = bindings[BindMode - 1];
                shouldPlay = bindPressed && !lastBindPressed;
                lastBindPressed = bindPressed;
            }

            if (!shouldPlay)
                return;

            void Play(AudioClip clip)
            {
                if (clip == null)
                    return;

                if (!activeSounds.ContainsKey(hash))
                {
                    if (RecorderPatch.enabled && NetworkSystem.Instance.InRoom)
                    {
                        var id = VoiceManager.Get().AudioClip(clip, false);
                        activeSounds[hash] = new SoundData { Clip = id, AudioClip = clip };
                    }
                }

                var clips = VoiceManager.Get().AudioClips;
                var keys = activeSounds.Keys.ToList();
            }

            if (file is string filePath)
                LoadSoundFromFile(filePath, Play);
            else if (file is AudioClip audioClip)
                Play(audioClip);
        }
        public static void StopSoundboardSound(string hash)
        {
            if (activeSounds.Any())
            {
                if (activeSounds.ContainsKey(hash))
                {
                    if (RecorderPatch.enabled)
                        VoiceManager.Get().StopAudioClip(activeSounds[hash].Clip);
                    activeSounds.Remove(hash);
                }
            }
        }


        public static void PlayAudio(string file)
        {
            if (NetworkSystem.Instance.InRoom)
            {
                LoadSoundFromFile(file, clip =>
                {
                    PlayAudio(clip);
                });
            }
        }

        public static void StopAllSounds() // used to be FixMicrophone
        {
            if (soundboardAudioManager != null)
            {
                soundboardAudioManager.GetComponent<AudioSource>().Stop();
                AudioIsPlaying = false;
                RecoverTime = -1f;
            }

            foreach (ButtonInfo[] buttonArray in CachedButtons.Values)
            {
                foreach (ButtonInfo button in buttonArray)
                {
                    if (button != null && button.enabled)
                        button.SetEnabled(false);
                }
            }

            if (NetworkSystem.Instance.InRoom)
            {
                if (RecorderPatch.enabled)
                {
                    if (activeSounds != null)
                    {
                        var keys = new HashSet<string>(activeSounds.Keys);

                        foreach (var row in Buttons.buttons)
                        {
                            foreach (var button in row)
                            {
                                if (keys.Contains(button.buttonText))
                                    button.SetEnabled(false);
                            }
                        }

                        activeSounds.Clear();
                    }
                    VoiceManager.Get().StopAudioClips();

                    NetworkSystem.Instance.VoiceConnection.PrimaryRecorder.DebugEchoMode = false;
                }
                else
                {
                    NetworkSystem.Instance.VoiceConnection.PrimaryRecorder.SourceType = Recorder.InputSourceType.Microphone;
                    NetworkSystem.Instance.VoiceConnection.PrimaryRecorder.AudioClip = null;
                    NetworkSystem.Instance.VoiceConnection.PrimaryRecorder.RestartRecording(true);
                    NetworkSystem.Instance.VoiceConnection.PrimaryRecorder.DebugEchoMode = false;
                }
            }

            AudioIsPlaying = false;
            RecoverTime = -1f;
        }

        public static void FixMicrophone()
        {
            if (!NetworkSystem.Instance.InRoom)
                return;
            if (RecorderPatch.enabled)
            {
                if (activeSounds != null)
                {
                    var keys = new HashSet<string>(activeSounds.Keys);

                    foreach (var row in Buttons.buttons)
                    {
                        foreach (var button in row)
                        {
                            if (keys.Contains(button.buttonText))
                                button.SetEnabled(false);
                        }
                    }

                    activeSounds.Clear();
                }
                VoiceManager.Get().StopAudioClips();
            }
            else
            {

            }
            NetworkSystem.Instance.VoiceConnection.PrimaryRecorder.SourceType = Recorder.InputSourceType.Microphone;
            NetworkSystem.Instance.VoiceConnection.PrimaryRecorder.AudioClip = null;
            NetworkSystem.Instance.VoiceConnection.PrimaryRecorder.RestartRecording(true);
            NetworkSystem.Instance.VoiceConnection.PrimaryRecorder.DebugEchoMode = false;
        }

        private static bool lastBindPressed;
        public static void PrepareBindAudio(string file)
        {
            bool[] bindings = {
                rightPrimary,
                rightSecondary,
                leftPrimary,
                leftSecondary,
                leftGrab,
                rightGrab,
                leftTrigger > 0.5f,
                rightTrigger > 0.5f,
                leftJoystickClick,
                rightJoystickClick
            };

            bool bindPressed = bindings[BindMode - 1];
            if (bindPressed && !lastBindPressed)
            {
                if (NetworkSystem.Instance.VoiceConnection.PrimaryRecorder.SourceType == Recorder.InputSourceType.AudioClip)
                    FixMicrophone();
                else
                    PlayAudio(file);
            }
            lastBindPressed = bindPressed;
        }

        public static void OpenSoundFolder()
        {
            string filePath = GetGamePath() + $"/{PluginInfo.BaseDirectory}/Sounds";
            Process.Start(filePath);
        }

        public static readonly string[] BindModeNames = {
            "None", "A", "B", "X", "Y", "Left Grip", "Right Grip",
            "Left Trigger", "Right Trigger", "Left Joystick", "Right Joystick"
        };
        public static void ApplySoundBindings(int index) => BindMode = index;

        public static float sendEffectDelay;
        public static void BetaPlayTag(int id, float volume)
        {
            if (!NetworkSystem.Instance.IsMasterClient)
                NotificationManager.SendNotification("<color=grey>[</color><color=red>ERROR</color><color=grey>]</color> You are not master client.");
            else
            {
                if (Time.time > sendEffectDelay)
                {
                    object[] soundSendData = { id, volume, false };
                    object[] sendEventData = { PhotonNetwork.ServerTimestamp, (byte)Constants.Network.ROOM_SYSTEM, soundSendData };

                    try
                    {
                        PhotonNetwork.RaiseEvent(3, sendEventData, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendUnreliable);
                    }
                    catch { }
                    RPCProtection();

                    sendEffectDelay = Time.time + 0.2f;
                }
            }
        }

        private static float soundSpamDelay;
        public static void SoundSpam(int soundId, bool constant = false)
        {
            if (rightGrab || constant)
            {
                if (Time.time > soundSpamDelay)
                    soundSpamDelay = Time.time + 0.1f;
                else
                    return;

                if (NetworkSystem.Instance.InRoom)
                {
                    GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, soundId, false, 999999f);
                    RPCProtection();
                }
                else
                    VRRig.LocalRig.PlayHandTapLocal(soundId, false, 999999f);
            }
        }

        public static void JmancurlySoundSpam() =>
            SoundSpam(Random.Range(336, 338));

        public static void RandomSoundSpam() =>
            SoundSpam(Random.Range(0, GTPlayer.Instance.materialData.Count));

        public static void CrystalSoundSpam()
        {
            int[] sounds = {
                Random.Range(40,54),
                Random.Range(214,221)
            };
            SoundSpam(sounds[Random.Range(0, 1)]);
        }

        private static bool squeakToggle;
        public static void SqueakSoundSpam()
        {
            if (Time.time > soundSpamDelay)
                squeakToggle = !squeakToggle;

            SoundSpam(squeakToggle ? 75 : 76);
        }

        private static bool sirenToggle;
        public static void SirenSoundSpam()
        {
            if (Time.time > soundSpamDelay)
                sirenToggle = !sirenToggle;

            SoundSpam(sirenToggle ? 48 : 50);
        }

        public static int soundId;

        public static void CustomSoundSpam() => SoundSpam(soundId);

        public static void BetaSoundSpam(int id)
        {
            if (rightGrab)
                BetaPlayTag(id, 999999f);
        }
    }
}
