// -----------------------------------------------------------------------
// <copyright file="SCPVoiceChatPatch.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Exiled.API.Features;
using HarmonyLib;
using Mirror;
using Mistaken.API.Extensions;
using PlayableScps.Messages;

namespace Mistaken.BetterSCP
{
    [HarmonyPatch(typeof(PlayableScps.Scp939), nameof(PlayableScps.Scp939.ServerReceivedVoiceMsg))]
    internal static class SCPVoiceChatPatch
    {
        public static readonly HashSet<RoleType> MimicedRoles = new HashSet<RoleType>();

        public static bool Prefix(NetworkConnection conn, Scp939VoiceMessage msg)
        {
            if (!ReferenceHub.TryGetHubNetID(conn.identity.netId, out ReferenceHub hub))
                return false;

            CharacterClassManager ccm = hub.characterClassManager;

            if (MimicedRoles.Contains(ccm.CurClass))
            {
                Log.Debug("[Mimic] Granted: Class");
                hub.dissonanceUserSetup.MimicAs939 = msg.IsMimicking;
            }
            else if (ccm.IsAnyScp() && (HasAccessToSCPAlt.Contains(ccm.UserId) || ccm.UserId.IsDevUserId()))
            {
                Log.Debug("[Mimic] Granted: Override");
                hub.dissonanceUserSetup.MimicAs939 = msg.IsMimicking;
            }
            else
            {
                Log.Debug("[Mimic] Denied");
            }

            return true;
        }

        internal static readonly HashSet<string> HasAccessToSCPAlt = new HashSet<string>();
    }
}
