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
    /// <summary>
    /// Patch used to allow other scps to speak like SCP-939.
    /// </summary>
    [HarmonyPatch(typeof(Radio), nameof(Radio.UserCode_CmdSyncTransmissionStatus))]
    public static class SCPVoiceChatPatch
    {
        /// <summary>
        /// List of scp classes that can use human voice chat.
        /// </summary>
        public static readonly HashSet<RoleType> MimicedRoles = new HashSet<RoleType>();

        internal static readonly HashSet<string> HasAccessToSCPAlt = new HashSet<string>();

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        internal static bool Prefix(Radio __instance, bool b)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            CharacterClassManager ccm = __instance._hub.characterClassManager;
            if (MimicedRoles.Contains(ccm.CurClass))
            {
                Log.Debug("[Mimic] Granted: Class", PluginHandler.Instance.Config.VerbouseOutput);
                __instance._dissonanceSetup.MimicAs939 = b;
            }
            else if (ccm.IsAnyScp() && HasAccessToSCPAlt.Contains(ccm.UserId))
            {
                Log.Debug("[Mimic] Granted: Override", PluginHandler.Instance.Config.VerbouseOutput);
                __instance._dissonanceSetup.MimicAs939 = b;
            }
            else
            {
                Log.Debug("[Mimic] Denied", PluginHandler.Instance.Config.VerbouseOutput);
            }

            return true;
        }
    }
}
