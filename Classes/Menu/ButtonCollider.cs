/*
 * United Goyim College Fund  Classes/Menu/ButtonCollider.cs
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

using Seralyth.Managers;
using UnityEngine;
using static Seralyth.Menu.Main;

namespace Seralyth.Classes.Menu
{
    public class ButtonCollider : MonoBehaviour
    {
        public string relatedText;

        public bool incremental;
        public bool positive;

        public void OnTriggerEnter(Collider collider)
        {
            if (!(Time.time > buttonCooldown) ||
                (collider != buttonCollider && collider != lKeyCollider && collider != rKeyCollider) || joystickMenu ||
                menu == null) return;
            buttonCooldown = Time.time + 0.2f;
            if (relatedText != "Global Return") // HARDCODED GLOBAL RETURN CHECK (im gonna forget)
                SoundManager.Play(SoundManager.DefaultSounds["Button"], buttonText: relatedText);

            if (annoyingMode)
            {
                if (Random.Range(1, 5) == 2)
                {
                    NotificationManager.SendNotification("Error");
                    return;
                }
            }

            if (incremental)
                ToggleIncremental(relatedText, positive);
            else
                Toggle(relatedText, true);
        }
    }
}
