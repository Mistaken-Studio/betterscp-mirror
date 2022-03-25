// -----------------------------------------------------------------------
// <copyright file="GlobalHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Exiled.API.Features;
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
            Exiled.Events.Handlers.Player.Verified += this.Player_Verified;
            Exiled.Events.Handlers.Player.VoiceChatting += this.Player_VoiceChatting;
        }

        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Verified -= this.Player_Verified;
            Exiled.Events.Handlers.Player.VoiceChatting -= this.Player_VoiceChatting;
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

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            // Panic
            // InRange.Spawn(ev.Player.CameraTransform, Vector3.forward * 10f, new Vector3(10, 5, 20), OnEnterVision(ev.Player));
        }

        private void Player_VoiceChatting(Exiled.Events.EventArgs.VoiceChattingEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (!ev.Player.IsScp)
                return;
            if (PluginHandler.Instance.Config.AllowedSCPVCRoles.Contains(ev.Player.Role))
            {
                this.Log.Debug("[Mimic] Granted: Class", PluginHandler.Instance.Config.VerbouseOutput);
                ev.DissonanceUserSetup.MimicAs939 = ev.IsVoiceChatting;
            }
            else if (ev.Player.TryGetSessionVariable("HUMAN_VC_ACCESS", out bool value) && value)
            {
                this.Log.Debug("[Mimic] Granted: Override", PluginHandler.Instance.Config.VerbouseOutput);
                ev.DissonanceUserSetup.MimicAs939 = ev.IsVoiceChatting;
            }
            else
            {
                this.Log.Debug("[Mimic] Denied", PluginHandler.Instance.Config.VerbouseOutput);
                ev.DissonanceUserSetup.MimicAs939 = false;
            }
        }
    }
}
