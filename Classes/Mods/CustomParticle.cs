/*
 * United Goyim College Fund  Classes/Mods/CustomParticle.cs
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
using UnityEngine;
using static Seralyth.Menu.Main;
using static Seralyth.Utilities.RandomUtilities;

namespace Seralyth.Classes.Mods
{
    public class CustomParticle : MonoBehaviour
    {
        public float spawnTime;
        public float startScale;

        public Renderer renderer;
        public Vector3 velocity;

        public void Awake()
        {
            spawnTime = Time.time;

            startScale = transform.localScale.x;

            renderer = gameObject.GetComponent<Renderer>() ?? null;
            velocity = RandomVector3(scaleWithPlayer ? GTPlayer.Instance.scale : 1f);

            Update();
        }

        public void Update()
        {
            if (renderer != null)
                renderer.material.color = buttonColors[1].GetCurrentColor();

            if (Time.time > spawnTime + 1f)
            {
                Destroy(gameObject);
                return;
            }

            transform.position += velocity * Time.unscaledDeltaTime;
            transform.localScale = Vector3.one * Mathf.Lerp(startScale, 0f, Time.time - spawnTime);
        }
    }
}
