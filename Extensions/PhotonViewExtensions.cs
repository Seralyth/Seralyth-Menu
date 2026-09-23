/*
 * United Goyim College Fund  Extensions/PhotonViewExtensions.cs
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
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

namespace Seralyth.Extensions
{
    public static class PhotonViewExtensions
    {
        public static void RPC(this PhotonView view, string methodName, object target, params object[] parameters)
        {
            switch (target)
            {
                case RpcTarget rpcTarget:
                    view.RPC(methodName, rpcTarget, parameters);
                    break;
                case Player player:
                    view.RPC(methodName, player, parameters);
                    break;
            }
        }
        public static bool RPC(this PhotonView photonView, string methodName, RaiseEventOptions options, params object[] parameters)
        {
            if (photonView != null && parameters != null && !string.IsNullOrEmpty(methodName))
            {
                Hashtable rpcData = new Hashtable
                {
                    { 0, photonView.ViewID },
                    { 2, PhotonNetwork.ServerTimestamp },
                    { 3, methodName },
                    { 4, parameters }
                };

                if (photonView.Prefix > 0)
                    rpcData[1] = (short)photonView.Prefix;

                if (PhotonNetwork.PhotonServerSettings.RpcList.Contains(methodName))
                    rpcData[5] = (byte)PhotonNetwork.PhotonServerSettings.RpcList.IndexOf(methodName);

                if (options.Receivers == ReceiverGroup.All || (options.TargetActors != null && options.TargetActors.Contains(NetworkSystem.Instance.LocalPlayer.ActorNumber)))
                {
                    if (options.Receivers == ReceiverGroup.All)
                        options.Receivers = ReceiverGroup.Others;

                    if (options.TargetActors != null && options.TargetActors.Contains(NetworkSystem.Instance.LocalPlayer.ActorNumber))
                        options.TargetActors = options.TargetActors.Where(id => id != NetworkSystem.Instance.LocalPlayer.ActorNumber).ToArray();

                    PhotonNetwork.ExecuteRpc(rpcData, PhotonNetwork.LocalPlayer);
                }

                else
                {
                    PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent(Photon.Pun.PunEvent.RPC, rpcData, options, new SendOptions
                    {
                        Reliability = true,
                        DeliveryMode = DeliveryMode.ReliableUnsequenced,
                        Encrypt = false
                    });
                }
            }
            return false;
        }
    }
}
