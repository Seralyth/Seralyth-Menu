/*
 * United Goyim College Fund  Classes/Mods/PortalTrigger.cs
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

using GorillaLocomotion;
using Seralyth.Managers;
using Seralyth.Mods;
using System;
using System.Linq;
using UnityEngine;

namespace Seralyth.Classes.Mods
{
    public class PortalTrigger : MonoBehaviour
    {
        private static readonly Type[] allowedTypes = { typeof(ThrowableBug), typeof(SlingshotProjectile) };

        static bool HasAllowedComponent(Collider col) =>
            allowedTypes.Any(t => col.GetComponent(t) != null);

        public GameObject destination;
        public void OnTriggerEnter(Collider other)
        {
            if (other == GTPlayer.Instance.bodyCollider || other == GTPlayer.Instance.headCollider)
                CoroutineManager.instance.StartCoroutine(Movement.TeleportPortal(destination));
            else if (HasAllowedComponent(other))
                CoroutineManager.instance.StartCoroutine(Movement.TeleportObject(other.gameObject, destination));
        }
    }
}
