﻿// -----------------------------------------------------------------------
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

            Vector3 realModelPosition = player.Position;
            if (
                VisionInformation.GetVisionInformation(
                    player.ReferenceHub,
                    realModelPosition,
                    -2f,
                    40f,
                    false,
                    false,
                    player.ReferenceHub.localCurrentRoomEffects,
                    0)
                .IsLooking
                && (
                    !Physics.Linecast(realModelPosition + new Vector3(0f, 1.5f, 0f), player.CameraTransform.position, VisionInformation.VisionLayerMask)
                    || !Physics.Linecast(realModelPosition + new Vector3(0f, -1f, 0f), player.CameraTransform.position, VisionInformation.VisionLayerMask)
                )
            )
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

            Exiled.API.Features.Log.Debug($"[Panic] End {player.Nickname}", PluginHandler.Instance.Config.VerbouseOutput);
        };

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            // Panic
            InRange.Spawn(ev.Player.CameraTransform, Vector3.forward * 10f, new Vector3(10, 5, 20), OnEnterVision(ev.Player));
        }
    }
}
