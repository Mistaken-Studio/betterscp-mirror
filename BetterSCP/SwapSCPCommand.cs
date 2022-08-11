// -----------------------------------------------------------------------
// <copyright file="SwapSCPCommand.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Mistaken.API;
using Mistaken.API.Commands;
using Mistaken.API.Diagnostics;
using Mistaken.API.Extensions;

#pragma warning disable SA1005 // Single line comments should begin with a space.

namespace Mistaken.BetterSCP
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    internal class SwapSCPCommand : IBetterCommand
    {
        public override string Description => "Pozwala zmienić SCP";

        public override string Command => "swapscp";

        public override string[] Aliases => new string[] { "swap" };

        public string GetUsage()
        {
            return "swapscp [SCP]";
        }

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            var player = sender.GetPlayer();
            if (args.Length == 0)
                return new string[] { this.GetUsage() };
            if (player.Role.Team != Team.SCP)
                return new string[] { "Nie możesz zmienić SCP nie będąc SCP" };
            if (player.Role.Type == RoleType.Scp0492)
                return new string[] { "Nie możesz zmienić SCP jako SCP 049-2" };

            if (Round.ElapsedTime.TotalSeconds > 45)
                return new string[] { "Za późno, możesz zmienić SCP tylko przez pierwsze 30 sekund rundy" };

            if (this.roleRequests.Any(i => i.Key == player))
                return new string[] { "Już wysłałeś prośbę aby zamienić SCP" };

            if (AlreadyChanged.Contains(player))
                return new string[] { "Możesz zmienić SCP tylko raz na rundę" };

            var scp = args[0];
            scp = scp.ToLower().Replace("scp", string.Empty).Replace("-", string.Empty);
            var role = RoleType.Scp0492;

            switch (scp)
            {
                case "173":
                    role = RoleType.Scp173;
                    break;
                case "106":
                    role = RoleType.Scp106;
                    break;
                case "93953":
                case "939":
                    role = RoleType.Scp93953;
                    break;
                case "93989":
                    role = RoleType.Scp93989;
                    break;
                case "049":
                    role = RoleType.Scp049;
                    break;
                case "079":
                    if (RealPlayers.List.Any(p => p.Role.Team == Team.SCP && p.Id != player.Id))
                        role = RoleType.Scp079;
                    else
                        return new string[] { "Jesteś jedynym SCP, nie możesz się zamienić w SCP 079" };
                    break;
                case "096":
                    role = RoleType.Scp096;
                    break;

                case "yes":
                case "no":
                    if (this.roleRequests.Any(i => i.Value.Key == player))
                    {
                        var data = this.roleRequests.First(i => i.Value.Key == player);
                        var requester = data.Key;
                        if (args[0].ToLower() == "yes")
                        {
                            player.Role.Type = requester.Role.Type;
                            requester.Role.Type = data.Value.Value;
                            AlreadyChanged.Add(requester);
                            this.roleRequests.Remove(data);
                            return new string[] { "Ok" };
                        }
                        else if (args[0].ToLower() == "no")
                        {
                            requester.Broadcast("Swap SCP", 5, $"{player.Nickname} nie chce zamienić się SCP");
                            this.roleRequests.Remove(data);
                            return new string[] { "Ok" };
                        }
                        else
                            return new string[] { ".scpswap yes/no", $"'yes' aby zmienić SCP na {requester.Role.Type}", $"'no' aby zostać jako {player.Role.Type}" };
                    }

                    break;
                default:
                    return new string[] { "Nieznany SCP", this.GetUsage() };
            }

            if (player.Role.Type == role)
            {
                success = false;
                return new string[] { "Już jesteś tym SCP" };
            }

            if (RealPlayers.List.Any(p => p.Role.Type == role))
            {
                Player requester = null;

                if (this.roleRequests.Any(i => i.Value.Key == player))
                {
                    var request = this.roleRequests.First(i => i.Value.Key == player);
                    requester = request.Key;
                }

                var target = RealPlayers.List.First(p => p.Role.Type == role);

                if (requester == target)
                {
                    var playerRole = player.Role.Type;
                    player.Role.Type = requester.Role.Type;
                    requester.Role.Type = playerRole;
                    AlreadyChanged.Add(requester);
                    this.roleRequests.Remove(this.roleRequests.First(i => i.Value.Key == player));

                    return new string[] { "Zamieniono SCP" };
                }

                var data = new KeyValuePair<Player, KeyValuePair<Player, RoleType>>(player, new KeyValuePair<Player, RoleType>(target, role));
                this.roleRequests.Add(data);
                target.Broadcast("Swap SCP", 15, $"<size=50%>{player.Nickname} chce się z tobą zamienić SCP, jeżeli się zgodzisz to zostaniesz <b>{player.Role}</b>\nWpisz \".swapscp yes\" lub \".swapscp no\" w konsoli(~) aby się zamienić lub aby tego nie robić</size>");
                Module.CallSafeDelayed(
                    15,
                    () =>
                    {
                        if (this.roleRequests.Contains(data))
                        {
                            player.Broadcast("Swap SCP", 5, "Czas minął");
                            this.roleRequests.Remove(data);
                        }
                    },
                    "SawpSCP");
                return new string[] { "Prośba zamiany wysłana" };
            }
            else
                return new string[] { "Możesz zamienić się w SCP, tylko gdy taki SCP jest w aktualnej rundzie" };
        }

        internal static readonly List<Player> AlreadyChanged = new List<Player>();

        private readonly List<KeyValuePair<Player, KeyValuePair<Player, RoleType>>> roleRequests = new List<KeyValuePair<Player, KeyValuePair<Player, RoleType>>>();
    }
}
