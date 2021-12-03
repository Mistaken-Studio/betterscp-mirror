// -----------------------------------------------------------------------
// <copyright file="GlobalHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Mistaken.API;
using Mistaken.API.Components;
using Mistaken.API.Diagnostics;
using Mistaken.API.Extensions;
using Mistaken.API.GUI;
using Mistaken.RoundLogger;
using PlayableScps;
using UnityEngine;

namespace Mistaken.BetterSCP
{
    /// <inheritdoc/>
    public class GlobalHandler : Module
    {
        /// <summary>
        /// Kills <paramref name="player"/> and selects random spectator to replace him.
        /// This is used to not lose SCPs when player leaves server and there are spectators.
        /// </summary>
        /// <param name="player">Player to change.</param>
        public static void RespawnSCP(Player player)
        {
            RLogger.Log("SCP RESPAWN", "SCP", "Respawning SCP");

            var spectators = RealPlayers.Get(Team.RIP).Where(x => !x.IsOverwatchEnabled).ToArray();

            if (spectators.Length == 0)
            {
                player.IsGodModeEnabled = false;
                MapPlus.Broadcast("RESPAWN", 10, $"SCP player Change, ({player.Id}) {player.Nickname} -> Nobody", Broadcast.BroadcastFlags.AdminChat);
                player.Kill("Wall");
            }
            else
            {
                var randomPlayer = spectators[UnityEngine.Random.Range(0, spectators.Length)];

                var position = player.Position + (Vector3.up * 0.5f);
                var hp = player.Health;
                var ahp = player.ArtificialHealth;
                var lvl = player.Level;
                var energy = player.Energy;
                var experience = player.Experience;
                Camera079 camera = player.Camera;

                bool scp079 = player.Role == RoleType.Scp079;
                randomPlayer.SetRole(player.Role, SpawnReason.ForceClass, false);
                Module.CallSafeDelayed(
                    .2f,
                    () =>
                    {
                        if (scp079)
                        {
                            randomPlayer.Level = lvl;
                            randomPlayer.Energy = energy;
                            randomPlayer.Experience = experience;
                            if (player.Camera != null)
                                randomPlayer.Camera = player.Camera;
                        }
                        else
                        {
                            randomPlayer.Health = hp;
                            randomPlayer.ArtificialHealth = ahp;
                        }
                    },
                    "GlobalHandler.LateSync");

                Module.CallSafeDelayed(.5f, () => randomPlayer.Position = position, "GlobalHandler.LateTeleport");

                player.SetRole(RoleType.Spectator, SpawnReason.None);
                randomPlayer.Broadcast(10, $"Player {player.GetDisplayName()} left game so you were moved to replace him");
                MapPlus.Broadcast("RESPAWN", 10, $"SCP player Change, ({player.Id}) {player.Nickname} -> ({randomPlayer.Id}) {randomPlayer.Nickname}", Broadcast.BroadcastFlags.AdminChat);
            }
        }

        /// <inheritdoc cref="Module.Module(Exiled.API.Interfaces.IPlugin{Exiled.API.Interfaces.IConfig})"/>
        public GlobalHandler(PluginHandler p)
            : base(p)
        {
        }

        /// <inheritdoc/>
        public override string Name => nameof(GlobalHandler);

        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Verified += this.Player_Verified;
            Exiled.Events.Handlers.Player.Destroying += this.Player_Destroying;
            Exiled.Events.Handlers.Scp106.Containing += this.Scp106_Containing;
            Exiled.Events.Handlers.Server.RestartingRound += this.Server_RestartingRound;
        }

        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Verified -= this.Player_Verified;
            Exiled.Events.Handlers.Player.Destroying -= this.Player_Destroying;
            Exiled.Events.Handlers.Scp106.Containing -= this.Scp106_Containing;
            Exiled.Events.Handlers.Server.RestartingRound -= this.Server_RestartingRound;
        }

        private static readonly Dictionary<string, DateTime> LastSeeTime = new Dictionary<string, DateTime>();
        private static readonly Func<Player, Action<Player>> OnEnterVision = (player) => (scp) =>
        {
            Exiled.API.Features.Log.Debug($"[Panic] Begin {player.Nickname}", PluginHandler.Instance.Config.VerbouseOutput);
            if (!scp.IsScp || scp.Role == RoleType.Scp079)
                return;
            Exiled.API.Features.Log.Debug($"[Panic] Post SCP {player.Nickname}", PluginHandler.Instance.Config.VerbouseOutput);
            if (!player.IsHuman)
                return;
            Exiled.API.Features.Log.Debug($"[Panic] Post Human {player.Nickname}", PluginHandler.Instance.Config.VerbouseOutput);
            if (player.GetEffectActive<CustomPlayerEffects.Flashed>())
                return;
            Exiled.API.Features.Log.Debug($"[Panic] Post flash {player.Nickname}", PluginHandler.Instance.Config.VerbouseOutput);

            Vector3 realModelPosition = scp.Position;
            if (
                VisionInformation.GetVisionInformation(
                    player.ReferenceHub,
                    realModelPosition,
                    -2f,
                    40f,
                    false,
                    false,
                    scp.ReferenceHub.localCurrentRoomEffects,
                    0)
                .IsLooking
                && (
                    !Physics.Linecast(realModelPosition + new Vector3(0f, 1.5f, 0f), player.CameraTransform.position, VisionInformation.VisionLayerMask)
                    || !Physics.Linecast(realModelPosition + new Vector3(0f, -1f, 0f), player.CameraTransform.position, VisionInformation.VisionLayerMask)))
            {
                if (LastSeeTime.TryGetValue(player.UserId, out DateTime lastSeeTime) && (DateTime.Now - lastSeeTime).TotalSeconds < 60)
                {
                    LastSeeTime[player.UserId] = DateTime.Now;
                    Exiled.API.Features.Log.Debug($"[Panic] Panic cooldown active for {player.Nickname}", PluginHandler.Instance.Config.VerbouseOutput);
                    return;
                }

                player.EnableEffect<CustomPlayerEffects.Invigorated>(5, true);

                player.EnableEffect<CustomPlayerEffects.MovementBoost>(5, true);
                player.ChangeEffectIntensity<CustomPlayerEffects.MovementBoost>(20);

                /*if (!player.GetEffectActive<CustomPlayerEffects.Panic>())
                    player.EnableEffect<CustomPlayerEffects.Panic>(15, true);*/
                player.SetGUI("panic", PseudoGUIPosition.MIDDLE, "Zaczynasz <color=yellow>panikować</color>", 3);
                LastSeeTime[player.UserId] = DateTime.Now;
                Exiled.API.Features.Log.Debug($"[Panic] Activated {player.Nickname}", PluginHandler.Instance.Config.VerbouseOutput);
            }

            Exiled.API.Features.Log.Debug($"[Panic] End {player.Nickname}", PluginHandler.Instance.Config.VerbouseOutput);
        };

        private static bool change106 = true;

        private void Scp106_Containing(Exiled.Events.EventArgs.ContainingEventArgs ev)
        {
            change106 = false;
        }

        private void Player_Destroying(Exiled.Events.EventArgs.DestroyingEventArgs ev)
        {
            if (!Round.IsStarted)
                return;

            if (!ev.Player.IsScp || ev.Player.Role == RoleType.Scp0492)
                return;

            if (!change106 && ev.Player.Role == RoleType.Scp106)
                return;

            RespawnSCP(ev.Player);
        }

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            // Panic
            InRange.Spawn(ev.Player.CameraTransform, Vector3.forward * 10f, new Vector3(10, 5, 20), OnEnterVision(ev.Player));
        }

        private void Server_RestartingRound()
        {
            change106 = true;
        }
    }
}
