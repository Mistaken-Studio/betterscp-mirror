// -----------------------------------------------------------------------
// <copyright file="SCPVoiceChatPatch.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.API.Features;
using HarmonyLib;

namespace Mistaken.BetterSCP
{
    /// <summary>
    /// Patch used to allow other scps to speak like SCP-939.
    /// </summary>
    [HarmonyPatch(typeof(Radio), nameof(Radio.UserCode_CmdSyncTransmissionStatus))]
    public static class SCPVoiceChatPatch
    {
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        internal static bool Prefix(Radio __instance, bool b)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            var player = Player.Get(__instance.gameObject);
            if (!player.IsScp)
                return true;
            if (PluginHandler.Instance.Config.AllowedSCPVCRoles.Contains(player.Role))
            {
                Log.Debug("[Mimic] Granted: Class", PluginHandler.Instance.Config.VerbouseOutput);
                __instance._dissonanceSetup.MimicAs939 = b;
            }
            else if (player.TryGetSessionVariable("HUMAN_VC_ACCESS", out bool value) && value)
            {
                Log.Debug("[Mimic] Granted: Override", PluginHandler.Instance.Config.VerbouseOutput);
                __instance._dissonanceSetup.MimicAs939 = b;
            }
            else
            {
                Log.Debug("[Mimic] Denied", PluginHandler.Instance.Config.VerbouseOutput);
                __instance._dissonanceSetup.MimicAs939 = false;
            }

            return true;
        }
    }
}
