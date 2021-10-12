// -----------------------------------------------------------------------
// <copyright file="SCPGUIHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using MEC;
using Mirror;
using Mistaken.API;
using Mistaken.API.Diagnostics;
using Mistaken.API.GUI;
using Respawning;
using Respawning.NamingRules;
using UnityEngine;

namespace Mistaken.BetterSCP
{
    /// <inheritdoc/>
    public class SCPGUIHandler : Module
    {
        /// <summary>
        /// Messages shown after changing to role.
        /// </summary>
        public static readonly Dictionary<RoleType, string> SCPMessages = new Dictionary<RoleType, string>();

        /// <inheritdoc cref="Module.Module(Exiled.API.Interfaces.IPlugin{Exiled.API.Interfaces.IConfig})"/>
        public SCPGUIHandler(PluginHandler p)
            : base(p)
        {
        }

        /// <inheritdoc/>
        public override string Name => nameof(SCPGUIHandler);

        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Hurting += this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => this.Player_Hurting(ev));
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => this.Player_ChangingRole(ev));
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Handle(() => this.Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => this.Server_RoundStarted(), "RoundStarted");
            Exiled.Events.Handlers.Scp079.GainingLevel += this.Handle<Exiled.Events.EventArgs.GainingLevelEventArgs>((ev) => this.Scp079_GainingLevel(ev));
            Exiled.Events.Handlers.Scp049.FinishingRecall += this.Handle<Exiled.Events.EventArgs.FinishingRecallEventArgs>((ev) => this.Scp049_FinishingRecall(ev));
            Exiled.Events.Handlers.Server.RespawningTeam += this.Handle<Exiled.Events.EventArgs.RespawningTeamEventArgs>((ev) => this.Server_RespawningTeam(ev));
            Exiled.Events.Handlers.Player.Dying += this.Handle<Exiled.Events.EventArgs.DyingEventArgs>((ev) => this.Player_Dying(ev));

            UnitNamingManager.RolesWithEnforcedDefaultName[RoleType.Scp049] = SpawnableTeamType.NineTailedFox;
            UnitNamingManager.RolesWithEnforcedDefaultName[RoleType.Scp0492] = SpawnableTeamType.NineTailedFox;
            UnitNamingManager.RolesWithEnforcedDefaultName[RoleType.Scp079] = SpawnableTeamType.NineTailedFox;
            UnitNamingManager.RolesWithEnforcedDefaultName[RoleType.Scp096] = SpawnableTeamType.NineTailedFox;
            UnitNamingManager.RolesWithEnforcedDefaultName[RoleType.Scp106] = SpawnableTeamType.NineTailedFox;
            UnitNamingManager.RolesWithEnforcedDefaultName[RoleType.Scp173] = SpawnableTeamType.NineTailedFox;
            UnitNamingManager.RolesWithEnforcedDefaultName[RoleType.Scp93953] = SpawnableTeamType.NineTailedFox;
            UnitNamingManager.RolesWithEnforcedDefaultName[RoleType.Scp93989] = SpawnableTeamType.NineTailedFox;
        }

        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Hurting -= this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => this.Player_Hurting(ev));
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => this.Player_ChangingRole(ev));
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Handle(() => this.Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => this.Server_RoundStarted(), "RoundStarted");
            Exiled.Events.Handlers.Scp079.GainingLevel -= this.Handle<Exiled.Events.EventArgs.GainingLevelEventArgs>((ev) => this.Scp079_GainingLevel(ev));
            Exiled.Events.Handlers.Scp049.FinishingRecall -= this.Handle<Exiled.Events.EventArgs.FinishingRecallEventArgs>((ev) => this.Scp049_FinishingRecall(ev));
            Exiled.Events.Handlers.Server.RespawningTeam -= this.Handle<Exiled.Events.EventArgs.RespawningTeamEventArgs>((ev) => this.Server_RespawningTeam(ev));
            Exiled.Events.Handlers.Player.Dying -= this.Handle<Exiled.Events.EventArgs.DyingEventArgs>((ev) => this.Player_Dying(ev));

            UnitNamingManager.RolesWithEnforcedDefaultName.Remove(RoleType.Scp049);
            UnitNamingManager.RolesWithEnforcedDefaultName.Remove(RoleType.Scp0492);
            UnitNamingManager.RolesWithEnforcedDefaultName.Remove(RoleType.Scp079);
            UnitNamingManager.RolesWithEnforcedDefaultName.Remove(RoleType.Scp096);
            UnitNamingManager.RolesWithEnforcedDefaultName.Remove(RoleType.Scp106);
            UnitNamingManager.RolesWithEnforcedDefaultName.Remove(RoleType.Scp173);
            UnitNamingManager.RolesWithEnforcedDefaultName.Remove(RoleType.Scp93953);
            UnitNamingManager.RolesWithEnforcedDefaultName.Remove(RoleType.Scp93989);
        }

        internal void ClearUnitNames(Player player)
        {
            if (player.Connection == null)
                return;

            MirrorExtensions.SendFakeSyncObject(player, RespawnManager.Singleton.NamingManager.netIdentity, typeof(UnitNamingManager), writer =>
            {
                writer.WriteUInt64(1ul);
                writer.WriteUInt32(1);
                writer.WriteByte((byte)SyncList<byte>.Operation.OP_CLEAR);
            });
        }

        internal void SendFakeUnitName(Player player, string name)
        {
            if (player.Connection == null)
                return;

            MirrorExtensions.SendFakeSyncObject(player, RespawnManager.Singleton.NamingManager.netIdentity, typeof(UnitNamingManager), writer =>
            {
                writer.WriteUInt64(1ul);
                writer.WriteUInt32(1);
                writer.WriteByte((byte)SyncList<byte>.Operation.OP_ADD);
                writer.WriteByte((byte)SpawnableTeamType.NineTailedFox);
                writer.WriteString(name);
            });
        }

        internal void ResyncUnitName(Player player)
        {
            if (player.Connection == null)
                return;

            MirrorExtensions.SendFakeSyncObject(player, RespawnManager.Singleton.NamingManager.netIdentity, typeof(UnitNamingManager), writer =>
            {
                writer.WriteUInt64(1ul);
                writer.WriteUInt32((uint)RespawnManager.Singleton.NamingManager.AllUnitNames.Count + 1);
                writer.WriteByte((byte)SyncList<byte>.Operation.OP_CLEAR);
                foreach (var item in RespawnManager.Singleton.NamingManager.AllUnitNames)
                {
                    writer.WriteByte((byte)SyncList<byte>.Operation.OP_ADD);
                    writer.WriteByte(item.SpawnableTeam);
                    writer.WriteString(item.UnitName);
                }
            });
            player.SendFakeSyncVar(Server.Host.ReferenceHub.networkIdentity, typeof(CharacterClassManager), nameof(CharacterClassManager.NetworkCurClass), (sbyte)RoleType.NtfCaptain);
        }

        private static readonly Dictionary<Player, DateTime> SpawnTimes = new Dictionary<Player, DateTime>();
        private bool blockUpdate = false;
        private Dictionary<Player, string> cache = new Dictionary<Player, string>();

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            if (ev.Target.IsScp)
                this.ResyncUnitName(ev.Target);
        }

        private void Server_RespawningTeam(Exiled.Events.EventArgs.RespawningTeamEventArgs ev)
        {
            this.blockUpdate = true;
            this.CallDelayed(2, () => this.SyncSCP(true));
        }

        private void Scp049_FinishingRecall(Exiled.Events.EventArgs.FinishingRecallEventArgs ev)
        {
            this.blockUpdate = true;
            this.CallDelayed(1, () => this.SyncSCP(true));
        }

        private void Server_RoundStarted()
        {
            this.blockUpdate = true;
            this.CallDelayed(2, () => this.SyncSCP(true));
            this.CallDelayed(
                5,
                () =>
                {
                    foreach (var item in SwapSCPCommand.SwapCooldown.ToArray())
                    {
                        if (RealPlayers.List.Any(p => p.UserId == item.Key))
                        {
                            var player = RealPlayers.List.First(p => p.UserId == item.Key);
                            if (player.Team == Team.SCP)
                            {
                                if (item.Value == 1)
                                    SwapSCPCommand.SwapCooldown.Remove(player.UserId);
                                else
                                    SwapSCPCommand.SwapCooldown[player.UserId]--;
                            }
                        }
                    }
                },
                "RoundStart");
        }

        private void Server_WaitingForPlayers()
        {
            SwapSCPCommand.AlreadyChanged.Clear();
            this.cache.Clear();
            this.blockUpdate = false;
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.NewRole.GetSide() == Exiled.API.Enums.Side.Scp)
                this.RunCoroutine(this.UpdateSCPs(ev.Player), "UpdateSCPs");
            SpawnTimes[ev.Player] = DateTime.Now;

            if (Round.ElapsedTime.TotalSeconds < 2.5f)
                return;
            this.CallDelayed(
                .2f,
                () =>
                {
                    if (ev.NewRole.GetSide() != Side.Scp && ev.Player.Role.GetSide() == Side.Scp)
                    {
                        this.SyncSCP(true);

                        // ClearUnitNames(ev.Player);
                        this.ResyncUnitName(ev.Player);
                    }
                    else if (ev.NewRole.GetSide() == Side.Scp)
                    {
                        this.CallDelayed(
                            1,
                            () =>
                            {
                                foreach (var item in RealPlayers.List.Where(p => p != ev.Player && p.Connection != null))
                                    item.SendFakeSyncVar(ev.Player.Connection.identity, typeof(CharacterClassManager), nameof(CharacterClassManager.NetworkCurSpawnableTeamType), 0);
                            },
                            "LateForceNoBaseGameHierarchy");
                        this.SyncSCP(true);
                    }
                });
        }

        private string GetColorByHP(Player player)
        {
            if (player.MaxHealth == 0)
                return "blue";
            int health = (int)Math.Round((player.Health / player.MaxHealth) * 100);
            if (health > 80)
                return "green";
            if (health > 60)
                return "#44693a";
            if (health > 40)
                return "yellow";
            if (health > 20)
                return "orange";
            return "red";
        }

        private string GetColorByLevel(Player player)
        {
            switch (player.Level)
            {
                case 0:
                    return "red";
                case 1:
                    return "orange";
                case 2:
                    return "yellow";
                case 3:
                    return "#44693a";
                case 4:
                    return "green";
                default:
                    return "blue";
            }
        }

        private string GetColorByAmount(int number)
        {
            if (number > 3)
                return "green";
            if (number > 0)
                return "orange";
            return "red";
        }

        private void SyncSCP(bool forceUpdate = false)
        {
            var players = RealPlayers.Get(Team.SCP);
            List<string> units = new List<string>();
            int zombie = 0;
            bool scp049 = false;
            bool changed = forceUpdate;
            string unit;
            foreach (var player in players.OrderByDescending(x => x.Role))
            {
                switch (player.Role)
                {
                    case RoleType.Scp0492:
                        zombie++;
                        break;
                    case RoleType.Scp049:
                        scp049 = true;
                        unit = $"<color={this.GetColorByHP(player)}>SCP-049</color>";
                        units.Add(unit);
                        if (this.cache.ContainsKey(player) && this.cache[player].Equals(unit))
                            continue;
                        this.cache[player] = unit;
                        changed = true;
                        break;
                    case RoleType.Scp079:
                        unit = $"<color={this.GetColorByLevel(player)}>SCP-079</color>";
                        units.Add(unit);
                        if (this.cache.ContainsKey(player) && this.cache[player].Equals(unit))
                            continue;
                        this.cache[player] = unit;
                        changed = true;
                        break;
                    case RoleType.Scp096:
                        unit = $"<color={this.GetColorByHP(player)}>SCP-096</color>";
                        units.Add(unit);
                        if (this.cache.ContainsKey(player) && this.cache[player].Equals(unit))
                            continue;
                        this.cache[player] = unit;
                        changed = true;
                        break;
                    case RoleType.Scp106:
                        unit = $"<color={this.GetColorByHP(player)}>SCP-106</color>";
                        units.Add(unit);
                        if (this.cache.ContainsKey(player) && this.cache[player].Equals(unit))
                            continue;
                        this.cache[player] = unit;
                        changed = true;
                        break;
                    case RoleType.Scp173:
                        unit = $"<color={this.GetColorByHP(player)}>SCP-173</color>";
                        units.Add(unit);
                        if (this.cache.ContainsKey(player) && this.cache[player].Equals(unit))
                            continue;
                        this.cache[player] = unit;
                        changed = true;
                        break;
                    case RoleType.Scp93953:
                        unit = $"<color={this.GetColorByHP(player)}>SCP-939-53</color>";
                        units.Add(unit);
                        if (this.cache.ContainsKey(player) && this.cache[player].Equals(unit))
                            continue;
                        this.cache[player] = unit;
                        changed = true;
                        break;
                    case RoleType.Scp93989:
                        unit = $"<color={this.GetColorByHP(player)}>SCP-939-89</color>";
                        units.Add(unit);
                        if (this.cache.ContainsKey(player) && this.cache[player].Equals(unit))
                            continue;
                        this.cache[player] = unit;
                        changed = true;
                        break;
                }
            }

            if (scp049 || zombie > 0)
                units.Add($"<color={this.GetColorByAmount(zombie)}>SCP-049-02 | {zombie}</color>");
            foreach (var player in players)
            {
                this.ClearUnitNames(player);
                foreach (var item in units)
                    this.SendFakeUnitName(player, item);

                player.SendFakeSyncVar(Server.Host.ReferenceHub.networkIdentity, typeof(CharacterClassManager), nameof(CharacterClassManager.NetworkCurClass), (sbyte)RoleType.NtfCaptain);
            }

            this.blockUpdate = false;
        }

        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            if (ev.Target.Team == Team.SCP && !this.blockUpdate)
            {
                this.blockUpdate = true;
                this.CallDelayed(1, () =>
                {
                    this.SyncSCP(false);
                });
            }
        }

        private void Scp079_GainingLevel(Exiled.Events.EventArgs.GainingLevelEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            if (!this.blockUpdate)
            {
                this.blockUpdate = true;
                this.SyncSCP(false);
            }
        }

        private TimeSpan TimeSinceChangedRole(Player player) =>
            SpawnTimes.ContainsKey(player) ? DateTime.Now - SpawnTimes[player] : default;

        private IEnumerator<float> UpdateSCPs(Player p)
        {
            yield return Timing.WaitForSeconds(1);
            PseudoGUIHandler.Ignore(p);
            for (int i = 0; i < 30; i++)
            {
                this.GetSCPS(p);
                yield return Timing.WaitForSeconds(1);
            }

            PseudoGUIHandler.StopIgnore(p);
        }

        private void GetSCPS(Player p)
        {
            DateTime start = DateTime.Now;
            List<string> message = NorthwoodLib.Pools.ListPool<string>.Shared.Rent();
            message.Add("<br><br><br>");
            if (p.Role != RoleType.Scp0492 && 30 - Round.ElapsedTime.TotalSeconds > 0)
                message.Add(string.Format(PluginHandler.Instance.Translation.Info_SCP_Swap, Mathf.RoundToInt(30 - (float)Round.ElapsedTime.TotalSeconds)));

            if (RealPlayers.Get(Team.SCP).Count() > 1)
                message.Add(PluginHandler.Instance.Translation.Info_SCP_List);

            foreach (var player in RealPlayers.List.Where(player => player.Team == Team.SCP && player.Role != RoleType.Scp0492 && p.Id != player.Id))
                message.Add(string.Format(PluginHandler.Instance.Translation.Info_SCP_List_Element, player?.Nickname, player?.Role.ToString().ToUpper()));

            string fullmsg = string.Join("<br>", message);
            if (this.TimeSinceChangedRole(p).TotalSeconds < 30 && SCPMessages.TryGetValue(p.Role, out string roleMessage))
                fullmsg = $"<size=40><voffset=20em>{roleMessage}<br><br><br><size=90%>{fullmsg}</size><br><br><br><br><br><br><br><br><br><br></voffset></size>";

            p.ShowHint(fullmsg, 2);

            NorthwoodLib.Pools.ListPool<string>.Shared.Return(message);
            MasterHandler.LogTime("InfoMessageManager", "GetSCPS", start, DateTime.Now);
        }
    }
}
