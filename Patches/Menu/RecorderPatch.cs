/*
 * United Goyim College Fund  Patches/Menu/RecorderPatch.cs
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

using HarmonyLib;
using Photon.Voice;
using Photon.Voice.Unity;
using Seralyth.Managers;

namespace Seralyth.Patches.Menu
{
    [HarmonyPatch(typeof(Recorder))]
    public class RecorderPatch
    {
        public static bool enabled = true;
        [HarmonyPatch(nameof(Recorder.SourceType), MethodType.Getter)]
        public static bool Prefix(ref Recorder.InputSourceType __result)
        {
            if (enabled)
            {
                __result = Recorder.InputSourceType.Factory;
                return false;
            }
            return true;

        }

        [HarmonyPatch(nameof(Recorder.InputFactory), MethodType.Getter)]
        public static bool Prefix(ref System.Func<IAudioDesc> __result)
        {
            if (enabled)
            {
                __result = () => VoiceManager.Get();
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(Recorder.CreateLocalVoiceAudioAndSource))]
        public static bool Prefix(Recorder __instance)
        {
            if (enabled)
            {
                __instance.SourceType = Recorder.InputSourceType.Factory;
                __instance.InputFactory = () => VoiceManager.Get();
            }
            return true;
        }
    }
}
