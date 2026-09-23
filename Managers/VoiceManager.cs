/*
 * United Goyim College Fund  Managers/VoiceManager.cs
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

// Written with love by kingofnetflix </3
// For anyone else snooping in this class hoping to use it, you need to make sure that your recorder source type is a Factory and that the Factory is a new instance of this class.
// You may use VoiceManager.Get()
using Photon.Voice;
using Seralyth.Mods;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Seralyth.Managers
{
    public class VoiceManager : IAudioReader<float>
    {
        private int samplingRate = 48000;
        private int outputRate = 48000;
        private float gain = 1f;
        private float clipVolume = 1f;
        private float pitch = 1f;
        private float clipSpeed = 1f;

        private readonly int loopLength;
        private string currentDevice;
        public AudioClip microphoneClip;
        private int lastSamplePosition;
        private float step;

        private string error;

        private float[] rawMicrophoneData;
        private float[] microphoneBuffer;
        private float resamplePointer;

        private readonly object audioClipsLock = new object();

        public sealed class Clip
        {
            public Guid Id { get; set; }
            public AudioClip Source { get; set; }
            public float[] Samples;
            public int Channels;
            private float _samplePosition;
            public float Step { get; set; }
            public bool MuteMicrophone { get; set; }
            public float Volume { get; set; } = 1f;
            public float Speed { get; set; } = 1f;
            public bool IsPaused { get; set; }
            public bool Looping { get; set; }
            public float Length => (Samples != null && Channels > 0)
                ? (float)(Samples.Length / Channels) / Instance.OutputRate
                : 0f;
            public float CurrentTime
            {
                get => (Samples != null && Channels > 0)
                    ? (_samplePosition / (Samples.Length / Channels)) * Length
                    : 0f;
                set => Seek(value);
            }
            public float InternalPosition
            {
                get => _samplePosition;
                set => _samplePosition = value;
            }

            public void Pause() => IsPaused = true;

            public void Resume() => IsPaused = false;

            public void Seek(float seconds)
            {
                if (Samples == null || Channels <= 0) return;
                float targetSample = seconds * VoiceManager.Instance.OutputRate;
                int maxFrames = Samples.Length / Channels;
                _samplePosition = Mathf.Clamp(targetSample, 0, maxFrames);
            }

        }

        private readonly List<Clip> audioClips = new List<Clip>();

        private bool muteMicrophone;

        public VoiceManager(int loopLength = 1, string device = null)
        {
            this.loopLength = Mathf.Max(1, loopLength);
            Instance ??= this;
            StartRecording(device);
        }

        /// <summary>
        /// A read-only list of AudioClips currently playing.
        /// </summary>
        public IReadOnlyList<Clip> AudioClips
        {
            get
            {
                lock (audioClipsLock)
                {
                    return audioClips.ToArray();
                }
            }
        }

        /// <summary>
        /// Gets or sets the microphone's recording status. This does not stop the pushed AudioClip from playing.
        /// </summary>
        public bool MuteMicrophone
        {
            get { return muteMicrophone; }
            set { muteMicrophone = value; }
        }

        /// <summary>
        /// Gets or sets the microphone sampling rate. Setting a value restarts the microphone.
        /// </summary>
        public int SamplingRate
        {
            get { return samplingRate; }
            set
            {
                samplingRate = Mathf.Max(8000, value);
                RestartMicrophone();
            }
        }

        /// <summary>
        /// Gets or sets the output rate used for AudioClip samples.
        /// </summary>
        public int OutputRate
        {
            get { return outputRate; }
            set
            {
                outputRate = Mathf.Max(8000, value);
                RestartMicrophone();
            }
        }

        /// <summary>
        /// Gets or sets the microphone gain multiplier.
        /// </summary>
        public float Gain
        {
            get { return gain; }
            set { gain = Mathf.Max(0f, value); }
        }

        /// <summary>
        /// Gets or sets the default AudioClip gain multiplier for the Instance.
        /// </summary>
        public float ClipVolume
        {
            get { return clipVolume; }
            set { clipVolume = Mathf.Max(0f, value); }
        }

        /// <summary>
        /// Gets or sets the pitch. Lowest possible value can be 0.1f.
        /// </summary>
        public float Pitch
        {
            get => pitch;
            set => pitch = Mathf.Max(0.1f, value);
        }

        /// <summary>
        /// Gets or sets the default clip speed. Lowest possible value can be 0.1f.
        /// </summary>
        public float ClipSpeed
        {
            get => clipSpeed;
            set => clipSpeed = Mathf.Max(0.1f, value);
        }

        /// <summary>
        /// A list of post processors that can be used to edit the buffer after all the audio data is compiled.
        /// </summary>
        public readonly Dictionary<string, Action<float[]>> PostProcessors = new Dictionary<string, Action<float[]>>();

        /// <summary>
        /// Gets or sets the decision on if the post processing should affect the applied Audio Clip or not.
        /// </summary>
        public bool PostProcessClip { get; set; }

        public int Channels => 2;
        public string Error => error;
        public string CurrentDevice => currentDevice;

        public static VoiceManager Instance { get; private set; }

        /// <summary>
        /// Returns a valid VoiceManager instance. If the Instance variable is null, it will create a new VoiceManager.
        /// </summary>
        /// <param name="loopLength">Length (in seconds) of the looping mic buffer.</param>
        /// <param name="device">The microphone device to be used in recording.</param>
        public static VoiceManager Get(int loopLength = 1, string device = null)
        {
            return Instance ??= new VoiceManager(loopLength, device);
        }

        /// <summary>
        /// Starts the microphone recording.
        /// </summary>
        /// <param name="device">Microphone device name to be used. If empty, the default microphone is selected.</param>
        public bool StartRecording(string device = null)
        {
            error = null;

            if (Microphone.devices == null || Microphone.devices.Length == 0)
            {
                error = "No microphone devices found";
                LogManager.LogWarning(error);
                return false;
            }

            if (string.IsNullOrEmpty(device))
                currentDevice = Microphone.devices[0];
            else
            {
                bool found = false;
                for (int i = 0; i < Microphone.devices.Length; i++)
                {
                    if (Microphone.devices[i] == device)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    error = $"Microphone device '{device}' not found";
                    LogManager.LogError(error);
                    return false;
                }

                currentDevice = device;
            }

            if (Microphone.IsRecording(currentDevice))
                Microphone.End(currentDevice);

            microphoneClip = Microphone.Start(currentDevice, true, loopLength, samplingRate);

            if (microphoneClip == null)
            {
                error = $"Failed to start microphone '{currentDevice}'";
                LogManager.LogError(error);
                return false;
            }

            lastSamplePosition = 0;
            step = (samplingRate / (float)OutputRate);
            resamplePointer = 0f;
            return true;
        }

        /// <summary>
        /// Stops the microphone recording.
        /// </summary>
        public bool StopRecording()
        {
            if (!string.IsNullOrEmpty(currentDevice) && Microphone.IsRecording(currentDevice))
                Microphone.End(currentDevice);

            microphoneClip = null;
            lastSamplePosition = 0;
            resamplePointer = 0f;
            return true;
        }

        /// <summary>
        /// Switches the microphone device and restarts recording.
        /// </summary>
        public bool SwitchMicrophone(string device)
            => StopRecording() && StartRecording(device);

        /// <summary>
        /// Restarts the microphone using the current device, or the default if none is set.
        /// </summary>
        public bool RestartMicrophone()
            => StopRecording() && StartRecording(currentDevice);

        /// <summary>
        /// Pushes an AudioClip into the output stream.
        /// </summary>
        public Clip AudioClip(AudioClip audioClip, bool disableMicrophone = false)
        {
            if (audioClip == null)
                return null;

            Guid id = Guid.NewGuid();
            int channels = Mathf.Max(1, audioClip.channels);
            float[] raw = new float[audioClip.samples * channels];
            audioClip.GetData(raw, 0);

            try
            {
                if (audioClip.frequency != OutputRate)
                    raw = Resample(raw, audioClip.frequency, OutputRate, channels);

                Clip clipState = new Clip
                {
                    Id = id,
                    Source = audioClip,
                    Samples = raw,
                    Channels = channels,
                    Step = 1f,
                    MuteMicrophone = disableMicrophone,
                    Volume = clipVolume,
                    Speed = clipSpeed,
                    Looping = Sound.LoopAudio
                };

                lock (audioClipsLock)
                    audioClips.Add(clipState);

                return clipState;
            }
            catch (Exception e)
            {
                LogManager.LogError($"Failed to insert audio clip: {e}");
                return null;
            }
        }

        public Clip GetAudioClip(Guid id)
        {
            lock (audioClipsLock)
            {
                int index = audioClips.FindIndex(c => c.Id == id);
                if (index == -1) return null;
                return audioClips[index];
            }
        }

        /// <summary>
        /// Resamples a raw float array to the target sample rate.
        /// </summary>
        public static float[] Resample(float[] source, int sourceRate, int targetRate, int channels)
        {
            if (source == null || source.Length == 0 || sourceRate <= 0 || sourceRate == targetRate)
                return source;

            int sourceSamples = Mathf.Max(1, source.Length / channels);
            float lengthInSeconds = (float)sourceSamples / sourceRate;
            int targetSamples = Mathf.Max(1, Mathf.RoundToInt(lengthInSeconds * targetRate));

            float[] target = new float[targetSamples * channels];

            if (sourceSamples == 1 || targetSamples == 1)
            {
                for (int c = 0; c < channels && c < target.Length; c++)
                    target[c] = source[Mathf.Clamp(c, 0, source.Length - 1)];
            }
            else
            {
                float ratio = (sourceSamples - 1f) / (targetSamples - 1f);

                for (int i = 0; i < targetSamples; i++)
                {
                    float p = i * ratio;
                    int a = Mathf.Clamp((int)p, 0, sourceSamples - 1);
                    int b = Mathf.Clamp(a + 1, 0, sourceSamples - 1);
                    float t = p - a;

                    for (int c = 0; c < channels; c++)
                    {
                        int o = i * channels + c;
                        int ia = Mathf.Clamp(a * channels + c, 0, source.Length - 1);
                        int ib = Mathf.Clamp(b * channels + c, 0, source.Length - 1);
                        target[o] = Mathf.Lerp(source[ia], source[ib], t);
                    }
                }
            }

            return target;
        }

        /// <summary>
        /// Stops the specified AudioClip from playing.
        /// </summary>
        public bool StopAudioClip(Clip clip)
        {
            if (clip == null) return false;
            lock (audioClipsLock)
                return audioClips.Remove(clip);
        }

        /// <summary>
        /// Stops all currently playing audio clips.
        /// </summary>
        public void StopAudioClips()
        {
            lock (audioClipsLock)
                audioClips.Clear();
        }

        /// <summary>
        /// Used to pull the next chunk of audio samples.
        /// Automatically called by Photon.
        /// </summary>
        public bool Read(float[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
                return false;

            if (microphoneClip != null && !string.IsNullOrEmpty(currentDevice))
            {
                int outFrames = buffer.Length / Channels;
                int micChannels = Mathf.Max(1, microphoneClip.channels);
                int micFrames = microphoneClip.samples;
                int micSampleCount = micFrames * micChannels;

                if (rawMicrophoneData == null || rawMicrophoneData.Length != micSampleCount)
                    rawMicrophoneData = new float[micSampleCount];

                if (microphoneBuffer == null || microphoneBuffer.Length != buffer.Length)
                    microphoneBuffer = new float[buffer.Length];

                int curFrame = Microphone.GetPosition(currentDevice);
                int lastFrame = lastSamplePosition;

                int available = curFrame < lastFrame
                    ? (micFrames - lastFrame) + curFrame
                    : (curFrame - lastFrame);

                float micHz = microphoneClip.frequency;
                float sourceStep = (micHz / (float)outputRate) * pitch;

                int needed = Mathf.CeilToInt(outFrames * sourceStep) + 2;
                if (available < needed)
                    return false;

                microphoneClip.GetData(rawMicrophoneData, 0);

                bool muteMicForClip = false;
                lock (audioClipsLock)
                {
                    for (int i = 0; i < audioClips.Count; i++)
                    {
                        if (!audioClips[i].IsPaused && audioClips[i].MuteMicrophone)
                        {
                            muteMicForClip = true;
                            break;
                        }
                    }
                }

                float sourcePosition = lastFrame + resamplePointer;

                for (int i = 0; i < buffer.Length; i += Channels)
                {
                    float left = 0f;
                    float right = 0f;

                    int aFrame = ((int)sourcePosition) % micFrames;
                    int bFrame = (aFrame + 1) % micFrames;
                    float frac = sourcePosition - Mathf.Floor(sourcePosition);

                    if (!muteMicrophone && !muteMicForClip)
                    {
                        if (micChannels == 1)
                        {
                            float a = rawMicrophoneData[aFrame];
                            float b = rawMicrophoneData[bFrame];
                            left = right = Mathf.Lerp(a, b, frac) * gain;
                        }
                        else
                        {
                            int a = aFrame * micChannels;
                            int b = bFrame * micChannels;

                            float aL = rawMicrophoneData[Mathf.Clamp(a + 0, 0, rawMicrophoneData.Length - 1)];
                            float aR = rawMicrophoneData[Mathf.Clamp(a + 1, 0, rawMicrophoneData.Length - 1)];
                            float bL = rawMicrophoneData[Mathf.Clamp(b + 0, 0, rawMicrophoneData.Length - 1)];
                            float bR = rawMicrophoneData[Mathf.Clamp(b + 1, 0, rawMicrophoneData.Length - 1)];

                            left = Mathf.Lerp(aL, bL, frac) * gain;
                            right = Mathf.Lerp(aR, bR, frac) * gain;
                        }
                    }

                    microphoneBuffer[i] = left;
                    if (Channels > 1 && i + 1 < buffer.Length)
                        microphoneBuffer[i + 1] = right;

                    sourcePosition += sourceStep;
                }
                int usedFrames = Mathf.FloorToInt(sourcePosition) - lastFrame;
                lastSamplePosition = (lastFrame + usedFrames) % micFrames;
                resamplePointer = sourcePosition - Mathf.Floor(sourcePosition);
            }

            if (!PostProcessClip)
            {
                foreach (var postProcess in PostProcessors.Values)
                    postProcess?.Invoke(microphoneBuffer);
            }

            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = microphoneBuffer[i];

            lock (audioClipsLock)
            {
                for (int c = audioClips.Count - 1; c >= 0; c--)
                {
                    var clip = audioClips[c];
                    if (clip.IsPaused) continue;

                    bool clipFinished = false;

                    for (int i = 0; i < buffer.Length; i += Channels)
                    {
                        int index = (int)clip.InternalPosition;
                        int maxFrames = clip.Samples.Length / clip.Channels;

                        if (index >= maxFrames)
                        {
                            if (clip.Looping)
                            {
                                clip.InternalPosition = 0f;
                                continue;
                            }

                            clipFinished = true;
                            break;
                        }

                        int nextIndex = index + 1;
                        float left = 0f;
                        float right = 0f;

                        if (nextIndex >= maxFrames)
                        {
                            if (clip.Looping)
                            {
                                clip.InternalPosition = 0f;

                                if (clip.Channels == 1)
                                {
                                    left = right = clip.Samples[index] * clip.Volume;
                                }
                                else
                                {
                                    int baseIdx = index * clip.Channels;
                                    left = clip.Samples[baseIdx] * clip.Volume;
                                    right = clip.Samples[baseIdx + 1] * clip.Volume;
                                }

                                continue;
                            }

                            if (clip.Channels == 1)
                            {
                                left = right = clip.Samples[index] * clip.Volume;
                            }
                            else
                            {
                                int baseIdx = index * clip.Channels;
                                left = clip.Samples[baseIdx] * clip.Volume;
                                right = clip.Samples[baseIdx + 1] * clip.Volume;
                            }

                            clipFinished = true;
                        }
                        else
                        {
                            float frac = clip.InternalPosition - index;

                            if (clip.Channels == 1)
                            {
                                left = right = Mathf.Lerp(
                                    clip.Samples[index],
                                    clip.Samples[nextIndex],
                                    frac
                                ) * clip.Volume;
                            }
                            else
                            {
                                int baseIdx1 = index * clip.Channels;
                                int baseIdx2 = nextIndex * clip.Channels;

                                float l1 = clip.Samples[baseIdx1];
                                float r1 = clip.Samples[baseIdx1 + 1];
                                float l2 = clip.Samples[baseIdx2];
                                float r2 = clip.Samples[baseIdx2 + 1];

                                left = Mathf.Lerp(l1, l2, frac) * clip.Volume;
                                right = Mathf.Lerp(r1, r2, frac) * clip.Volume;
                            }

                            clip.InternalPosition += Mathf.Max(0.0001f, clip.Step * clip.Speed);
                        }

                        buffer[i] += left;
                        if (Channels > 1 && i + 1 < buffer.Length)
                            buffer[i + 1] += right;

                        if (clipFinished) break;
                    }

                    if (clipFinished && !clip.Looping)
                        audioClips.RemoveAt(c);
                }
            }

            if (PostProcessClip)
            {
                foreach (var postProcess in PostProcessors.Values)
                    postProcess?.Invoke(buffer);
            }

            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = Mathf.Clamp(buffer[i], -1f, 1f);

            return true;
        }

        public void Dispose()
        {
            StopRecording();
            StopAudioClips();

            if (ReferenceEquals(Instance, this))
                Instance = null;
        }
    }
}