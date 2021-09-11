// -----------------------------------------------------------------------
// <copyright file="SCPVoiceChatPatch.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Assets._Scripts.Dissonance;
using HarmonyLib;

namespace Mistaken.BetterSCP
{
    [HarmonyPatch(typeof(DissonanceUserSetup), "CallCmdAltIsActive")]
    internal static class SCPVoiceChatPatch
    {
        public static readonly List<RoleType> MimicedRoles = new List<RoleType>();

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        public static bool Prefix(DissonanceUserSetup __instance, bool value)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            CharacterClassManager ccm = ReferenceHub.GetHub(__instance.gameObject).characterClassManager;

            if (MimicedRoles.Contains(ccm.CurClass))
            {
                if (value && (__instance.NetworkspeakingFlags & SpeakingFlags.MimicAs939) == 0)
                    __instance.NetworkspeakingFlags |= SpeakingFlags.MimicAs939;
                else if (!value && (__instance.NetworkspeakingFlags & SpeakingFlags.MimicAs939) != 0)
                    __instance.NetworkspeakingFlags ^= SpeakingFlags.MimicAs939;
            }
            else if (ccm.IsAnyScp() && HasAccessToSCPAlt.Contains(ccm.UserId))
            {
                if (value && (__instance.NetworkspeakingFlags & SpeakingFlags.MimicAs939) == 0)
                    __instance.NetworkspeakingFlags |= SpeakingFlags.MimicAs939;
                else if (!value && (__instance.NetworkspeakingFlags & SpeakingFlags.MimicAs939) != 0)
                    __instance.NetworkspeakingFlags ^= SpeakingFlags.MimicAs939;
            }

            return true;
        }

        internal static readonly HashSet<string> HasAccessToSCPAlt = new HashSet<string>();
    }
}
