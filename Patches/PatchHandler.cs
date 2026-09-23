/*
 * United Goyim College Fund  Patches/PatchHandler.cs
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
using Seralyth.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Seralyth.Patches
{
    public class PatchHandler
    {
        public static bool IsPatched { get; internal set; }
        public static int PatchErrors { get; internal set; }

        public static bool CriticalPatchFailed { get; internal set; }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
        public class SecurityPatch : Attribute { }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
        public class PatchOnAwake : Attribute { }

        private static readonly HashSet<MethodBase> ProtectedMethods = new HashSet<MethodBase>();
        private static readonly Dictionary<Type, HashSet<MethodBase>> PatchClassOriginals = new Dictionary<Type, HashSet<MethodBase>>();
        private static readonly object protectedLock = new object();

        public static void PatchAll(bool awake = false)
        {
            if (IsPatched && !awake) return;

            instance ??= new HarmonyLib.Harmony(PluginInfo.GUID);

            Type[] types;
            try
            {
                types = Assembly.GetExecutingAssembly().GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types.Where(t => t != null).ToArray();
            }

            foreach (var type in types
                         .Where(t => t != null && t.IsClass && t.GetCustomAttribute<HarmonyPatch>() != null && (t.GetCustomAttribute<PatchOnAwake>() != null) == awake))
            {
                try
                {
                    var originals = instance.CreateClassProcessor(type).Patch();

                    if (type.GetCustomAttribute<SecurityPatch>() != null && originals != null)
                    {
                        lock (protectedLock)
                        {
                            if (!PatchClassOriginals.TryGetValue(type, out var set))
                            {
                                set = new HashSet<MethodBase>();
                                PatchClassOriginals[type] = set;
                            }

                            foreach (var m in originals)
                            {
                                if (m == null) continue;
                                ProtectedMethods.Add(m);
                                set.Add(m);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    PatchErrors++;

                    if (type.GetCustomAttribute<SecurityPatch>() != null)
                        CriticalPatchFailed = true;

                    LogManager.LogError($"Failed to patch {type.FullName}: {ex}");
                }
            }

            LogManager.Log($"Patched with {PatchErrors} errors");

            if (!awake)
                IsPatched = true;
        }

        public static void UnpatchAll()
        {
            if (instance == null || !IsPatched) return;

            var securityTypes = GetTypes()
                .Where(t => t?.GetCustomAttribute<SecurityPatch>() != null)
                .ToArray();

            instance.UnpatchSelf();
            IsPatched = false;

            if (securityTypes.Length > 0)
            {
                var reinit = new HarmonyLib.Harmony(PluginInfo.GUID);
                foreach (var type in securityTypes)
                {
                    try
                    {
                        reinit.CreateClassProcessor(type).Patch();
                    }
                    catch (Exception ex)
                    {
                        LogManager.LogError($"Failed to restore security patch {type.FullName} after UnpatchAll: {ex}");
                        CriticalPatchFailed = true;
                    }
                }
                instance = reinit;
                IsPatched = true;
            }
            else
            {
                instance = null;
            }
        }

        internal static void ApplyPatch(Type targetClass, string methodName, MethodInfo prefix = null, MethodInfo postfix = null, Type[] parameterTypes = null)
        {
            if (targetClass == null)
                throw new ArgumentNullException(nameof(targetClass));

            if (string.IsNullOrWhiteSpace(methodName))
                throw new ArgumentException("Method name cannot be null or empty.", nameof(methodName));

            instance ??= new HarmonyLib.Harmony(PluginInfo.GUID);

            var original =
                (parameterTypes == null ?
                targetClass.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static) :
                targetClass.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, parameterTypes, null)) ?? throw new Exception($"Method '{methodName}' not found on {targetClass.FullName}");

            instance.Patch(original,
                prefix: prefix != null ? new HarmonyMethod(prefix) : null,
                postfix: postfix != null ? new HarmonyMethod(postfix) : null);
        }

        internal static void RemovePatch(Type targetClass, string methodName, Type[] parameterTypes = null)
        {
            if (instance == null)
                return;

            if (targetClass == null)
                throw new ArgumentNullException(nameof(targetClass));

            if (string.IsNullOrWhiteSpace(methodName))
                throw new ArgumentException("Method name cannot be null or empty.", nameof(methodName));

            var original =
                (parameterTypes == null ?
                targetClass.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static) :
                targetClass.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, parameterTypes, null)) ?? throw new Exception($"Method '{methodName}' not found on {targetClass.FullName}");

            lock (protectedLock)
            {
                if (ProtectedMethods.Contains(original))
                {
                    LogManager.LogError($"Refused to remove protected patch: {targetClass.FullName}.{methodName}");
                    return;
                }
            }

            instance.Unpatch(original, HarmonyPatchType.All, instance.Id);
        }

        private static Type[] GetTypes()
        {
            try
            {
                return Assembly.GetExecutingAssembly().GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null).ToArray();
            }
        }

        public static void PatchIntegrityCheck()
        {
            if (instance == null) return;

            List<MethodBase> protectedSnapshot;
            lock (protectedLock)
            {
                protectedSnapshot = ProtectedMethods.ToList();
            }

            var infoCache = new Dictionary<MethodBase, HarmonyLib.Patches>(protectedSnapshot.Count);
            var missingByType = new HashSet<Type>();

            foreach (var method in protectedSnapshot)
            {
                var info = HarmonyLib.Harmony.GetPatchInfo(method);
                infoCache[method] = info;

                if (info == null)
                {
                    CriticalPatchFailed = true;
                    continue;
                }

                bool owned = false;
                if (!ContainsOwner(info.Prefixes, instance.Id) &&
                    !ContainsOwner(info.Postfixes, instance.Id) &&
                    !ContainsOwner(info.Transpilers, instance.Id) &&
                    !ContainsOwner(info.Finalizers, instance.Id))
                {
                    RemoveForeignOwners(method, info);
                    owned = false;
                }
                else
                {
                    owned = true;
                }

                if (!owned)
                    CriticalPatchFailed = true;
            }

            var securityTypes = GetTypes().Where(t => t?.GetCustomAttribute<SecurityPatch>() != null);
            foreach (var type in securityTypes)
            {
                bool stillMissing = protectedSnapshot
                    .Where(m => ProtectedMethodBelongsToPatchClass(type, m))
                    .Any(m => !infoCache.TryGetValue(m, out var info) || info == null ||
                              !ContainsOwner(info.Prefixes, instance.Id) &&
                              !ContainsOwner(info.Postfixes, instance.Id) &&
                              !ContainsOwner(info.Transpilers, instance.Id) &&
                              !ContainsOwner(info.Finalizers, instance.Id));

                if (!stillMissing) continue;
                ReapplySecurityPatch(type);
            }
        }

        private static void ReapplySecurityPatch(Type type)
        {
            try
            {
                var reapplied = instance.CreateClassProcessor(type).Patch();
                if (reapplied != null)
                {
                    lock (protectedLock)
                    {
                        if (!PatchClassOriginals.TryGetValue(type, out var set))
                        {
                            set = new HashSet<MethodBase>();
                            PatchClassOriginals[type] = set;
                        }

                        foreach (var m in reapplied)
                        {
                            if (m == null) continue;
                            ProtectedMethods.Add(m);
                            set.Add(m);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogError($"Failed to reapply security patch on {type.FullName}: {ex}");
            }
        }

        private static bool ContainsOwner(IEnumerable<Patch> patches, string ownerId)
        {
            foreach (var p in patches)
                if (p.owner == ownerId) return true;
            return false;
        }

        private static void RemoveForeignOwners(MethodBase method, HarmonyLib.Patches info)
        {
            var foreignOwners = new HashSet<string>();
            CollectOwners(info.Prefixes, instance.Id, foreignOwners);
            CollectOwners(info.Postfixes, instance.Id, foreignOwners);
            CollectOwners(info.Transpilers, instance.Id, foreignOwners);
            CollectOwners(info.Finalizers, instance.Id, foreignOwners);

            foreach (var owner in foreignOwners)
            {
                try { instance.Unpatch(method, HarmonyPatchType.All, owner); }
                catch (Exception ex) { LogManager.LogError($"Failed to remove patch from {method.Name}: {ex}"); }
            }
        }

        private static void CollectOwners(IEnumerable<Patch> patches, string excludeId, HashSet<string> into)
        {
            foreach (var p in patches)
                if (!string.IsNullOrEmpty(p.owner) && p.owner != excludeId)
                    into.Add(p.owner);
        }

        private static bool ProtectedMethodBelongsToPatchClass(Type patchClass, MethodBase original)
        {
            lock (protectedLock)
            {
                return PatchClassOriginals.TryGetValue(patchClass, out var known) && known.Contains(original);
            }
        }

        internal static HarmonyLib.Harmony instance;
        public const string InstanceId = PluginInfo.GUID;
    }
}