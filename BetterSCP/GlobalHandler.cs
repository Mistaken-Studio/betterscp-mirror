// -----------------------------------------------------------------------
// <copyright file="GlobalHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Mistaken.API.Components;
using Mistaken.API.Diagnostics;
using Mistaken.API.Extensions;
using Mistaken.API.GUI;
using PlayableScps;
using UnityEngine;

namespace Mistaken.BetterSCP
{
    /// <inheritdoc/>
    public class GlobalHandler : Module
    {
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
            Exiled.Events.Handlers.Player.Verified += this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => this.Player_Verified(ev));
        }

        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Verified -= this.Handle<Exiled.Events.EventArgs.VerifiedEventArgs>((ev) => this.Player_Verified(ev));
        }

        private static readonly Dictionary<string, DateTime> LastSeeTime = new Dictionary<string, DateTime>();
        private static readonly Func<Player, Action<Player>> OnEnterVision = (player) => (scp) =>
        {
            if (!scp.IsScp || scp.Role == RoleType.Scp079)
                return;
            if (!player.IsHuman)
                return;
            if (player.GetEffectActive<CustomPlayerEffects.Flashed>())
                return;
            var scpPosition = scp.Position;
            if (Vector3.Dot((player.Position - scpPosition).normalized, scp.ReferenceHub.PlayerCameraReference.forward) >= 0.1f)
            {
                VisionInformation visionInformation = VisionInformation.GetVisionInformation(player.ReferenceHub, scpPosition, -0.1f, 30f, true, true, scp.ReferenceHub.localCurrentRoomEffects);
                if (visionInformation.IsLooking)
                {
                    if (LastSeeTime.TryGetValue(player.UserId, out DateTime lastSeeTime) && (DateTime.Now - lastSeeTime).TotalSeconds < 60)
                    {
                        LastSeeTime[player.UserId] = DateTime.Now;
                        Exiled.API.Features.Log.Debug($"[Panic] Panic cooldown active for {player.Nickname}", PluginHandler.Instance.Config.VerbouseOutput);
                        return;
                    }

                    player.EnableEffect<CustomPlayerEffects.Invigorated>(5, true);
                    if (!player.GetEffectActive<CustomPlayerEffects.Panic>())
                        player.EnableEffect<CustomPlayerEffects.Panic>(15, true);
                    player.SetGUI("panic", PseudoGUIPosition.MIDDLE, "Zaczynasz <color=yellow>panikować</color>", 3);
                    LastSeeTime[player.UserId] = DateTime.Now;
                }
                else
                    Exiled.API.Features.Log.Debug($"[Panic] Not looking: {player.Nickname}", PluginHandler.Instance.Config.VerbouseOutput);
            }
            else
            {
                Exiled.API.Features.Log.Debug($"[Panic] Not looking2: {player.Nickname}", PluginHandler.Instance.Config.VerbouseOutput);
            }
        };

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            // Panic
            InRange.Spawn(ev.Player.CameraTransform, Vector3.forward * 10f, new Vector3(10, 5, 20), OnEnterVision(ev.Player));
        }
    }
}
