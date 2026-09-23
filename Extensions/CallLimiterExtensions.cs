/*
 * United Goyim College Fund  Extensions/CallLimiterExtensions.cs
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
using Photon.Pun;
using Seralyth.Managers;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Seralyth.Extensions
{
    public static class CallLimiterExtensions
    {
        private static readonly ConditionalWeakTable<CallLimiter, CallLimiter> shadows = new ConditionalWeakTable<CallLimiter, CallLimiter>();

        public static CallLimiter Clone(this CallLimiter limiter)
        {
            return new CallLimiter
            {
                callHistoryLength = limiter.callHistoryLength,
                timeCooldown = limiter.timeCooldown,
                maxLatency = limiter.maxLatency,
                oldTimeIndex = limiter.oldTimeIndex,
                blockCall = limiter.blockCall,
                blockStartTime = limiter.blockStartTime,
                callTimeHistory = (float[])limiter.callTimeHistory.Clone()
            };
        }

        public static float GetDelay(this CallLimiter limiter, bool usesNetworkTime = false)
        {
            if (limiter?.callTimeHistory == null || limiter.callHistoryLength <= 0)
                return 0f;

            float next = limiter.callTimeHistory[limiter.oldTimeIndex];
            if (next == float.MinValue)
                return 0f;

            float currentTime = usesNetworkTime && NetworkSystem.Instance.IsOnline
                ? NetworkSystem.Instance.ServerTimestamp / 1000.0f
                : Time.time;

            return Mathf.Max(0f, next - currentTime);
        }

        public static bool CanCallNow(this CallLimiter limiter, double? time = null, bool usesNetworkTime = false)
        {
            if (limiter == null)
            {
                LogManager.LogWarning("CallLimiter is null");
                return false;
            }

            CallLimiter shadow = shadows.GetValue(limiter, rl => rl.Clone());

            if (usesNetworkTime && NetworkSystem.Instance.IsOnline)
                return shadow.CheckCallServerTime(time ?? (PhotonNetwork.CurrentTime));
            return shadow.CheckCallTime((float)(time ?? Time.time));
        }

        public static void Remove(this CallLimiter limiter)
        {
            if (limiter == null) return;
            shadows.Remove(limiter);
        }
    }
}