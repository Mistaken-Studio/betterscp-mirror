// -----------------------------------------------------------------------
// <copyright file="SCPVoiceChatPatch.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Reflection.Emit;
using Assets._Scripts.Dissonance;
using Exiled.API.Features;
using HarmonyLib;
using NorthwoodLib.Pools;

#pragma warning disable SA1118 // Parameter should not span multiple lines

namespace Mistaken.BetterSCP
{
    /// <summary>
    /// Patch used to allow other scps to speak like SCP-939.
    /// </summary>
    [HarmonyPatch(typeof(Radio), nameof(Radio.UserCode_CmdSyncTransmissionStatus))]
    public static class SCPVoiceChatPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);
            Label elseIfLabel = generator.DefineLabel();
            Label elseLabel = generator.DefineLabel();
            Label continueLabel = generator.DefineLabel();

            LocalBuilder player = generator.DeclareLocal(typeof(Player));
            LocalBuilder config = generator.DeclareLocal(typeof(Config));

            int startIndex = newInstructions.Count - 1;

            newInstructions.InsertRange(
                startIndex,
                new CodeInstruction[]
                {
                    /*
                     *  var player = Player.Get(this._hub);
                     *  if (player == null)
                     *      return;
                     *  if (!player.IsScp)
                     *      return;
                     *  if (PluginHandler.Instance.Config.AllowedSCPVCRoles.Contains(player.Role))
                     *  {
                     *      Log.Debug("[Mimic] Granted: Class", PluginHandler.Instance.Config.VerbouseOutput);
                     *      this._dissonanceSetup.MimicAs939 = b;
                     *  }
                     *  else if (HasVCOverride(player))
                     *  {
                     *      Log.Debug("[Mimic] Granted: Override", PluginHandler.Instance.Config.VerbouseOutput);
                     *      this._dissonanceSetup.MimicAs939 = b;
                     *  }
                     *  else
                     *  {
                     *      Log.Debug("[Mimic] Denied", PluginHandler.Instance.Config.VerbouseOutput);
                     *      this._dissonanceSetup.MimicAs939 = false;
                     *  }
                     */

                    // var player = Player.Get(this._hub);
                    new CodeInstruction(OpCodes.Ldarg_0).MoveBlocksFrom(newInstructions[startIndex]), // [Radio]
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Radio), nameof(Radio._hub))), // [ReferenceHub]
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Player), nameof(Player.Get), new System.Type[] { typeof(ReferenceHub) })), // [Player]
                    new CodeInstruction(OpCodes.Stloc, player), // []

                    // if (player == null) return;
                    new CodeInstruction(OpCodes.Ldloc, player), // [Player]
                    new CodeInstruction(OpCodes.Brfalse_S, continueLabel), // []

                    // if (!player.IsScp) return;
                    new CodeInstruction(OpCodes.Ldloc, player), // [Player]
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Player), nameof(Player.IsScp))), // [bool]
                    new CodeInstruction(OpCodes.Brfalse_S, continueLabel), // []

                    new CodeInstruction(OpCodes.Ldloc, player), // [Player]
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Player), nameof(Player.Role))), // [RoleType]
                    new CodeInstruction(OpCodes.Conv_I1), // [sbyte]
                    new CodeInstruction(OpCodes.Ldc_I4_7), // [int, sbyte]
                    new CodeInstruction(OpCodes.Conv_I1), // [sbyte, sbyte]
                    new CodeInstruction(OpCodes.Beq_S, continueLabel), // []

                    // var config = PluginHandler.Instance.Config;
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(PluginHandler), nameof(PluginHandler.Instance))), // [PluginHandler]
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(PluginHandler), nameof(PluginHandler.Config))), // [Config]
                    new CodeInstruction(OpCodes.Stloc, config), // [Config]

                    // if (config.AllowedSCPVCRoles.Contains(player.Role))
                    new CodeInstruction(OpCodes.Ldloc, config), // [Config]
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Config), nameof(Config.AllowedSCPVCRoles))), // [List<RoleType>]

                    new CodeInstruction(OpCodes.Ldloc, player), // [Player, List<RoleType>]
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Player), nameof(Player.Role))), // [RoleType, List<RoleType>]

                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(List<RoleType>), nameof(List<RoleType>.Contains))), // [bool]
                    new CodeInstruction(OpCodes.Brfalse_S, elseIfLabel), // []

                    // Log.Debug("[Mimic] Granted: Class", PluginHandler.Instance.Config.VerbouseOutput);
                    new CodeInstruction(OpCodes.Ldstr, "[Mimic] Granted: Class"), // [string]
                    new CodeInstruction(OpCodes.Ldloc, config), // [Config, string]
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Config), nameof(Config.VerbouseOutput))), // [bool, string]
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Log), nameof(Log.Debug))), // []

                    // this._dissonanceSetup.MimicAs939 = b;
                    new CodeInstruction(OpCodes.Ldarg_0), // [Radio]
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Radio), nameof(Radio._dissonanceSetup))), // [DissonanceUserSetup]
                    new CodeInstruction(OpCodes.Ldarg_1), // [bool, DissonanceUserSetup]
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertySetter(typeof(DissonanceUserSetup), nameof(DissonanceUserSetup.MimicAs939))), // []
                    new CodeInstruction(OpCodes.Br_S, continueLabel), // []

                    // else if (HasVCOverride(player))
                    new CodeInstruction(OpCodes.Ldloc, player).WithLabels(elseIfLabel), // [Player]
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SCPVoiceChatPatch), nameof(SCPVoiceChatPatch.HasVCOverride))), // [bool]
                    new CodeInstruction(OpCodes.Brfalse_S, elseLabel), // []

                    // Log.Debug("[Mimic] Granted: Override", PluginHandler.Instance.Config.VerbouseOutput);
                    new CodeInstruction(OpCodes.Ldstr, "[Mimic] Granted: Override"), // [string]
                    new CodeInstruction(OpCodes.Ldloc, config), // [Config, string]
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Config), nameof(Config.VerbouseOutput))), // [bool, string]
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Log), nameof(Log.Debug))), // []

                    // this._dissonanceSetup.MimicAs939 = b;
                    new CodeInstruction(OpCodes.Ldarg_0), // [Radio]
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Radio), nameof(Radio._dissonanceSetup))), // [DissonanceUserSetup]
                    new CodeInstruction(OpCodes.Ldarg_1), // [bool, DissonanceUserSetup]
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertySetter(typeof(DissonanceUserSetup), nameof(DissonanceUserSetup.MimicAs939))), // []
                    new CodeInstruction(OpCodes.Br_S, continueLabel), // []

                    // else
                    // Log.Debug("[Mimic] Denied", PluginHandler.Instance.Config.VerbouseOutput);
                    new CodeInstruction(OpCodes.Ldstr, "[Mimic] Denied").WithLabels(elseLabel), // [string]
                    new CodeInstruction(OpCodes.Ldloc, config), // [Config, string]
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Config), nameof(Config.VerbouseOutput))), // [bool, string]
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Log), nameof(Log.Debug))), // []

                    // this._dissonanceSetup.MimicAs939 = false;
                    new CodeInstruction(OpCodes.Ldarg_0), // [Radio]
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Radio), nameof(Radio._dissonanceSetup))), // [DissonanceUserSetup]
                    new CodeInstruction(OpCodes.Ldc_I4_0), // [bool, DissonanceUserSetup]
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertySetter(typeof(DissonanceUserSetup), nameof(DissonanceUserSetup.MimicAs939))), // []

                    new CodeInstruction(OpCodes.Nop).WithLabels(continueLabel),
                });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
            yield break;
        }

        private static bool HasVCOverride(Player player)
            => player.TryGetSessionVariable("HUMAN_VC_ACCESS", out bool value) && value;
    }
}
