/*
 * United Goyim College Fund  Managers/LogManager.cs
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

using System;

namespace Seralyth.Managers
{
    public enum Level
    {
        Info,
        Warning,
        Error,
        Debug
    }

    public static class LogManager
    {
        private static Action<Level, string> _sink;

        /// <summary>Call once from the loader entrypoint (BepInEx or MelonLoader).</summary>
        public static void SetLogger(Action<Level, string> sink) => _sink = sink;

        private static void Write(Level level, object log)
        {
            string msg = log?.ToString() ?? string.Empty;

            // Fallback
            if (_sink is null)
            {
                UnityEngine.Debug.Log($"[{level}] {msg}");
                return;
            }

            _sink(level, msg);
        }

        public static void Log(object log) => Write(Level.Info, log);

        public static void Log(object log, object[] args) =>
            Write(Level.Info, string.Format(log?.ToString() ?? "", args));

        public static void LogError(object log) => Write(Level.Error, log);

        public static void LogError(object log, object[] args) =>
            Write(Level.Error, string.Format(log?.ToString() ?? "", args));

        public static void LogWarning(object log) => Write(Level.Warning, log);

        public static void LogWarning(object log, object[] args) =>
            Write(Level.Warning, string.Format(log?.ToString() ?? "", args));

        public static void LogDebug(object log) => Write(Level.Debug, log);

        public static void LogDebug(object log, object[] args) =>
            Write(Level.Debug, string.Format(log?.ToString() ?? "", args));
    }
}
